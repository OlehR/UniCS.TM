using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using ModelMID.DB;

namespace Equipments.Equipments.Glory
{
    internal static class TypeRegistry
    {
        // (ns, localName) -> Type
        public static readonly ConcurrentDictionary<(string ns, string name), Type> Map = new();

        static TypeRegistry()
        {
            // Автоматично знайти всі типи, що реалізують IBrueboxMessage і мають XmlRoot
            var asm = Assembly.GetExecutingAssembly();
            var types = asm.GetTypes()
                .Where(t => typeof(IBrueboxMessage).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            foreach (var t in types)
            {
                var root = t.GetCustomAttribute<XmlRootAttribute>();
                if (root == null) continue;

                var name = string.IsNullOrWhiteSpace(root.ElementName) ? t.Name : root.ElementName;
                var ns = root.Namespace ?? string.Empty;
                Map[(ns, name)] = t;
            }
        }
    }

    internal static class SerializerCache
    {
        private static readonly ConcurrentDictionary<(Type type, string rootName, string ns), XmlSerializer> Cache
            = new();

        public static XmlSerializer GetOrAdd(Type type, string rootName, string ns)
        {
            return Cache.GetOrAdd((type, rootName, ns), key =>
            {
                var (t, rn, nspace) = key;
                // ВАЖЛИВО: Використовуємо XmlRootAttribute для відповідності реальному payload елементу
                var root = new XmlRootAttribute(rn) { Namespace = nspace };
                return new XmlSerializer(t, root);
            });
        }
    }



    public static class SoapParser
    {
        private static readonly XNamespace SOAP11 = Ns.Soap11;
        private static readonly XNamespace SOAP12 = Ns.Soap12;

        /// <summary>
        /// Парсить будь-який SOAP Envelope і повертає відповідний IBrueboxMessage.
        /// Якщо тип невідомий — поверне UnknownResponse з RawXml.
        /// </summary>
        public static IBrueboxMessage Parse(string soapXml)
        {
            if (string.IsNullOrWhiteSpace(soapXml))
                throw new ArgumentException("XML is empty.", nameof(soapXml));

            var doc = XDocument.Parse(soapXml, LoadOptions.PreserveWhitespace);
            var envelope = doc.Root ?? throw new InvalidOperationException("No SOAP Envelope root.");

            // Підтримка SOAP 1.1 і SOAP 1.2
            var body = envelope.Element(SOAP11 + "Body") ?? envelope.Element(SOAP12 + "Body");
            if (body == null)
                throw new InvalidOperationException("SOAP Body not found.");

            // Беремо перший елемент у Body (ігноруємо текст/коментарі)
            var payload = body.Elements().FirstOrDefault();
            if (payload == null)
                throw new InvalidOperationException("Empty SOAP Body.");

            var local = payload.Name.LocalName;
            var ns = payload.Name.NamespaceName ?? string.Empty;

            // Пошук типу у реєстрі
            if (!TypeRegistry.Map.TryGetValue((ns, local), out var targetType))
            {
                // fallback: UnknownResponse
                return new UnknownResponse
                {
                    RawXml = payload.ToString(SaveOptions.DisableFormatting),
                    LocalName = local,
                    NamespaceUri = ns
                };
            }

            // Десеріалізація з точним коренем (name + ns)
            var serializer = SerializerCache.GetOrAdd(targetType, local, ns);

            using var reader = payload.CreateReader();
            var obj = (IBrueboxMessage)serializer.Deserialize(reader);
            obj.RawXml = payload.ToString(SaveOptions.DisableFormatting);
            return obj;
        }
    }


    public static class Ns
    {
        public const string Soap11 = "http://schemas.xmlsoap.org/soap/envelope/";
        public const string Soap12 = "http://www.w3.org/2003/05/soap-envelope";
        public const string Bruebox = "http://www.glory.co.jp/bruebox.xsd";
    }
    /// <summary>
    /// Базовий інтерфейс для всіх повідомлень (відповідей).
    /// </summary>
    public interface IBrueboxMessage
    {
        /// <summary>Сирий XML payload (для логування/діагностики).</summary>
        string RawXml { get; set; }

        /// <summary>Результат операції, якщо атрибут присутній (n:result, result тощо).</summary>
        int? Result { get; }
    }

    /// <summary>
    /// Базовий клас із універсальним збиранням атрибутів (у т.ч. n:result).
    /// </summary>
    public abstract class BrueboxResponse : IBrueboxMessage
    {
        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes { get; set; }

        [XmlIgnore]
        public string RawXml { get; set; }

        [XmlIgnore]
        public int? Result
        {
            get
            {
                if (AnyAttributes == null) return null;
                var attr = AnyAttributes.FirstOrDefault(a => a.LocalName == "result");
                if (attr == null) return null;
                if (int.TryParse(attr.Value, out var val)) return val;
                return null;
            }
        }
    }

    // === ПРИКЛАДИ ВІДПОВІДЕЙ ===
    // Додайте власні типи за аналогією (головне — [XmlRoot] з правильними ім'ям і Namespace)

    [XmlRoot("CloseResponse", Namespace = Ns.Bruebox)]
    public class CloseResponse : BrueboxResponse
    {
        [XmlElement("Id")] public int Id { get; set; }
        [XmlElement("SeqNo")] public int SeqNo { get; set; }
        [XmlElement("User")] public string User { get; set; }
    }

    [XmlRoot("OccupyResponse", Namespace = Ns.Bruebox)]
    public class OccupyResponse : BrueboxResponse
    {
        [XmlElement("Id")] public int Id { get; set; }
        [XmlElement("SeqNo")] public int SeqNo { get; set; }
        [XmlElement("User")] public string User { get; set; }
    }

    [XmlRoot("OpenResponse", Namespace = Ns.Bruebox)]
    public class OpenResponse : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }

        [XmlElement("SessionID")]
        public string SessionID { get; set; }
    }
    [XmlRoot("AdjustTimeResponse", Namespace = Ns.Bruebox)]
    public class AdjustTimeResponse : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }
    }

    [XmlRoot("StatusResponse", Namespace = Ns.Bruebox)]
    public class StatusResponse : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }

        // ВАЖЛИВО: <Status> у порожньому просторі назв
        [XmlElement("Status", Namespace = "")]
        public StatusBlock Status { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class StatusBlock
    {
        // <n:Code> у просторі Ns.Bruebox
        [XmlElement("Code", Namespace = Ns.Bruebox)]
        public int Code { get; set; }

        // <DevStatus> елементи у порожньому просторі назв
        [XmlElement("DevStatus", Namespace = "")]
        public List<DevStatus> Devices { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class DevStatus
    {
        // Атрибути в ns Bruebox
        [XmlAttribute("devid", Namespace = Ns.Bruebox)]
        public int DevId { get; set; }

        [XmlAttribute("val", Namespace = Ns.Bruebox)]
        public int Value { get; set; }

        [XmlAttribute("st", Namespace = Ns.Bruebox)]
        public int Status { get; set; }
    }
    // End StatusResponse

    [XmlRoot("InventoryResponse", Namespace = Ns.Bruebox)]
    public class InventoryResponse : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }

        // <Cash ...> елементи у порожньому просторі назв
        [XmlElement("Cash", Namespace = "")]
        public List<CashBlock> Cash { get; set; }

        // <CashUnits ...> елементи у порожньому просторі назв
        [XmlElement("CashUnits", Namespace = "")]
        public List<CashUnitsBlock> CashUnits { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class CashBlock
    {
        // n:type
        [XmlAttribute("type", Namespace = Ns.Bruebox)]
        public int Type { get; set; }

        // <Denomination ...> у порожньому просторі назв
        [XmlElement("Denomination", Namespace = "")]
        public List<Denomination> Denominations { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class CashUnitsBlock
    {
        // n:devid
        [XmlAttribute("devid", Namespace = Ns.Bruebox)]
        public int DeviceId { get; set; }

        // <CashUnit ...> у порожньому просторі назв
        [XmlElement("CashUnit", Namespace = "")]
        public List<CashUnit> Units { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class CashUnit
    {
        // n:unitno, n:st, n:nf, n:ne, n:max
        [XmlAttribute("unitno", Namespace = Ns.Bruebox)]
        public int UnitNo { get; set; }

        [XmlAttribute("st", Namespace = Ns.Bruebox)]
        public int State { get; set; }

        [XmlAttribute("nf", Namespace = Ns.Bruebox)]
        public int Nf { get; set; }

        [XmlAttribute("ne", Namespace = Ns.Bruebox)]
        public int Ne { get; set; }

        [XmlAttribute("max", Namespace = Ns.Bruebox)]
        public int Max { get; set; }

        // Може бути відсутній або містити кілька Denomination
        [XmlElement("Denomination", Namespace = "")]
        public List<Denomination> Denominations { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class Denomination
    {
        // Атрибути у просторі bruebox: n:cc, n:fv, n:rev, n:devid
        [XmlAttribute("cc", Namespace = Ns.Bruebox)]
        public string CurrencyCode { get; set; }

        [XmlAttribute("fv", Namespace = Ns.Bruebox)]
        public int FaceValue { get; set; }

        [XmlAttribute("rev", Namespace = Ns.Bruebox)]
        public int Rev { get; set; }

        [XmlAttribute("devid", Namespace = Ns.Bruebox)]
        public int DeviceId { get; set; }

        // Дочірні елементи у просторі bruebox
        [XmlElement("Piece", Namespace = Ns.Bruebox)]
        public int Piece { get; set; }

        [XmlElement("Status", Namespace = Ns.Bruebox)]
        public int Status { get; set; }
    }
    // End InventoryResponse

    [XmlRoot("StartCashinResponse", Namespace = Ns.Bruebox)]
    public class StartCashinResponse : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }
    }

    [XmlRoot("CashinCancelResponse", Namespace = Ns.Bruebox)]
    public class CashinCancelResponse : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }

        [XmlElement("ManualDeposit")]
        public int ManualDeposit { get; set; }

        // <Cash n:type="..."> у порожньому просторі назв
        [XmlElement("Cash", Namespace = "")]
        public List<CashEntry> Cash { get; set; }
    }

    [XmlRoot("RefreshSalesTotalResponse", Namespace = Ns.Bruebox)]
    public class RefreshSalesTotalResponse : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }
    }

    [XmlRoot("EndCashinResponse", Namespace = Ns.Bruebox)]
    public class EndCashinResponse : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }

        [XmlElement("ManualDeposit")]
        public int ManualDeposit { get; set; }

        // <Cash n:type="..."> елементи у порожньому просторі назв
        [XmlElement("Cash", Namespace = "")]
        public List<CashBlock> Cash { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class CashEntry
    {
        // атрибут n:type у просторі bruebox
        [XmlAttribute("type", Namespace = Ns.Bruebox)]
        public int Type { get; set; }
    }


    [XmlRoot("ChangeResponse", Namespace = Ns.Bruebox)]
    public class ChangeResponse : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }

        [XmlElement("Amount")]
        public int Amount { get; set; }

        [XmlElement("ManualDeposit")]
        public int ManualDeposit { get; set; }

        // <Status> без простору назв
        [XmlElement("Status", Namespace = "")]
        public StatusBlock Status { get; set; }

        // <Cash n:type="..."> без простору назв
        [XmlElement("Cash", Namespace = "")]
        public List<CashBlock> Cash { get; set; }
    }

    [XmlRoot("ChangeCancelResponse", Namespace = Ns.Bruebox)]
    public class ChangeCancelResponse : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }
    }
    [XmlRoot("ReleaseResponse", Namespace = Ns.Bruebox)]
    public class ReleaseResponse : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }
    }

    [XmlRoot("CashoutResponse", Namespace = Ns.Bruebox)]
    public class CashoutResponse : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }

        // <Cash n:type="..."> елементи у порожньому просторі назв (може бути 0..n)
        [XmlElement("Cash", Namespace = "")]
        public List<CashBlock> Cash { get; set; }
    }
    [XmlRoot("StartReplenishmentFromEntranceResponse", Namespace = Ns.Bruebox)]
    public class StartReplenishmentFromEntranceResponse : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }
    }

    [XmlRoot("EndReplenishmentFromEntranceResponse", Namespace = Ns.Bruebox)]
    public class EndReplenishmentFromEntranceResponse : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }

        [XmlElement("ManualDeposit")]
        public int ManualDeposit { get; set; }

        // <Cash n:type="..."> елементи у порожньому просторі назв
        [XmlElement("Cash", Namespace = "")]
        public List<CashBlock> Cash { get; set; }
    }
    [XmlRoot("UnLockUnitResponse", Namespace = Ns.Bruebox)]
    public class UnLockUnitResponse : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }
    }
    [XmlRoot("LockUnitResponse", Namespace = Ns.Bruebox)]
    public class LockUnitResponse : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }
    }
    /// <summary>
    /// Якщо тип не відомий — отримаємо UnknownResponse і зможемо обробити як сирий XML.
    /// </summary>
    [XmlRoot("UnknownResponse")]
    public class UnknownResponse : BrueboxResponse
    {
        [XmlIgnore] public string LocalName { get; set; }
        [XmlIgnore] public string NamespaceUri { get; set; }
    }




    // Кореневий контейнер події AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
    [XmlRoot("BbxEventRequest", Namespace = "")]
    public class BbxEventRequest
    {
        [XmlElement("ChangeResponse")]
        public ChangeResponseEvent ChangeResponse { get; set; }
        // Якщо з'являтимуться інші типи подій, можна додати інші властивості тут,
        // або зробити універсальне поле з [XmlAnyElement] для фолбеку.
        [XmlElement("AdjustTimeResponse")]
        public AdjustTimeResponseEvent AdjustTimeResponse { get; set; }
        [XmlElement("HeartBeatEvent")]
        public HeartBeatEvent HeartBeatEvent { get; set; }
        [XmlElement("StatusResponse")]
        public StatusResponseEvent StatusResponse { get; set; }
        [XmlElement("InventoryResponse")]
        public InventoryResponseEvent InventoryResponse { get; set; }
        [XmlElement("OccupyResponse")]
        public OccupyResponseEvent OccupyResponse { get; set; }
        [XmlElement("GlyCashierEvent")]
        public GlyCashierEvent GlyCashierEvent { get; set; }
        [XmlElement("StatusChangeEvent")]
        public StatusChangeEvent StatusChangeEvent { get; set; }
        [XmlElement("EndCashinResponse")]
        public EndCashinResponseEvent EndCashinResponse { get; set; }
        [XmlElement("ReleaseResponse")]
        public ReleaseResponseEvent ReleaseResponse { get; set; }
        [XmlElement("StartCashinResponse")]
        public StartCashinResponseEvent StartCashinResponse { get; set; }
        [XmlElement("RefreshSalesTotalResponse")]
        public RefreshSalesTotalResponseEvent RefreshSalesTotalResponse { get; set; }
        [XmlElement("UnLockUnitResponse")]
        public UnLockUnitResponseEvent UnLockUnitResponse { get; set; }
        [XmlElement("LockUnitResponse")]
        public LockUnitResponseEvent LockUnitResponse { get; set; }

    }


    [XmlRoot("LockUnitResponse", Namespace = "")]
    public class LockUnitResponseEvent : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }
    }
    [XmlRoot("UnLockUnitResponse", Namespace = "")]
    public class UnLockUnitResponseEvent : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }
    }
    [XmlRoot("RefreshSalesTotalResponse", Namespace = "")]
    public class RefreshSalesTotalResponseEvent : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }
    }
    [XmlRoot("StartCashinResponse", Namespace = "")]
    public class StartCashinResponseEvent : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }
    }
    [XmlRoot("ReleaseResponse", Namespace = "")]
    public class ReleaseResponseEvent : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }
    }
    [XmlRoot("EndCashinResponse", Namespace = "")]
    public class EndCashinResponseEvent : BrueboxResponse
    {
        [XmlElement("Id")] public int Id { get; set; }
        [XmlElement("SeqNo")] public int SeqNo { get; set; }
        [XmlElement("User")] public string User { get; set; }
        [XmlElement("TransactionId")] public string TransactionId { get; set; }
        [XmlElement("ManualDeposit")] public int ManualDeposit { get; set; }

        // <Cash type="..."> (0..n), може бути порожнім (self-closing)
        [XmlElement("Cash")]
        public List<CashBlockEvent> Cash { get; set; }
    }
    [XmlRoot("StatusChangeEvent", Namespace = "")]
    public class StatusChangeEvent : BrueboxResponse
    {
        [XmlElement("Status")]
        public int Status { get; set; }

        [XmlElement("Amount")]
        public int Amount { get; set; }

        [XmlElement("Error")]
        public int Error { get; set; }

        // Можуть бути порожні в XML → краще як string
        [XmlElement("RecoveryURL")]
        public string RecoveryURL { get; set; }

        [XmlElement("User")]
        public string User { get; set; }

        [XmlElement("SeqNo")]
        public string SeqNo { get; set; }
    }
    [XmlRoot("GlyCashierEvent", Namespace = "")]
    public class GlyCashierEvent : BrueboxResponse
    {
        [XmlAttribute("devid")]
        public int DeviceId { get; set; }

        [XmlAttribute("user")]
        public string User { get; set; }

        [XmlElement("eventDepositCountMonitor")]
        public EventDepositCountMonitor DepositCountMonitor { get; set; }
        [XmlElement("eventEmpty")]
        public EventEmpty EventEmpty { get; set; }
        [XmlElement("eventStatusChange")]
        public EventStatusChange EventStatusChange { get; set; }
        [XmlElement("eventExist")]
        public EventExist EventExist { get; set; }
        [XmlElement("eventDepositCountChange")]
        public EventDepositCountChange EventDepositCountChange { get; set; }
        [XmlElement("eventWaitForOpening")]
        public EventWaitForOpening EventWaitForOpening { get; set; }
    }



    [XmlType(AnonymousType = true)]
    public class EventWaitForOpening
    {
        [XmlElement("DevicePositionId")]
        public int DevicePositionId { get; set; }
    }
    [XmlType(AnonymousType = true)]
    public class EventDepositCountChange
    {
        [XmlElement("Denomination")]
        public List<DenominationEvent> Denominations { get; set; }
    }
    [XmlType(AnonymousType = true)]
    public class EventExist
    {
        [XmlElement("DevicePositionId")]
        public int DevicePositionId { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class EventStatusChange
    {
        [XmlElement("DeviceStatusID")]
        public int DeviceStatusID { get; set; }
    }
    [XmlType(AnonymousType = true)]
    public class EventEmpty
    {
        [XmlElement("DevicePositionId")]
        public int DevicePositionId { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class EventDepositCountMonitor
    {
        [XmlElement("Denomination")]
        public List<DenominationEvent> Denominations { get; set; }
    }


    [XmlRoot("OccupyResponse", Namespace = "")]
    public class OccupyResponseEvent : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }
    }
    [XmlRoot("InventoryResponse", Namespace = "")]
    public class InventoryResponseEvent : BrueboxResponse
    {
        [XmlElement("Id")] public int Id { get; set; }
        [XmlElement("SeqNo")] public int SeqNo { get; set; }
        [XmlElement("User")] public string User { get; set; }

        // <Cash type="...">
        [XmlElement("Cash")]
        public List<CashBlockEvent> Cash { get; set; }

        // <CashUnits devid="...">
        [XmlElement("CashUnits")]
        public List<CashUnitsBlockEvent> CashUnits { get; set; }
    }

    // --- Використовуй уже наявний CashBlockEvent / DenominationEvent з попередніх подій ---
    // Наводжу ще раз, якщо їх ще немає у проєкті:


    [XmlType(AnonymousType = true)]
    public class CashUnitsBlockEvent
    {
        [XmlAttribute("devid")]
        public int DeviceId { get; set; }

        [XmlElement("CashUnit")]
        public List<CashUnitEvent> Units { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class CashUnitEvent
    {
        [XmlAttribute("unitno")] public int UnitNo { get; set; }
        [XmlAttribute("st")] public int State { get; set; }
        [XmlAttribute("nf")] public int Nf { get; set; }
        [XmlAttribute("ne")] public int Ne { get; set; }
        [XmlAttribute("max")] public int Max { get; set; }

        [XmlElement("Denomination")]
        public List<DenominationEvent> Denominations { get; set; }
    }

    [XmlRoot("StatusResponse", Namespace = "")]
    public class StatusResponseEvent : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }

        // <Status> без простору назв
        [XmlElement("Status")]
        public StatusBlockEvent Status { get; set; }
    }

    [XmlRoot("HeartBeatEvent", Namespace = "")]
    public class HeartBeatEvent : BrueboxResponse
    {
        [XmlElement("SerialNo")]
        public string SerialNo { get; set; }
    }

    [XmlRoot("AdjustTimeResponse", Namespace = "")]
    public class AdjustTimeResponseEvent : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }
    }
    // ВАРІАНТ ChangeResponse БЕЗ простору назв (подія)
    [XmlRoot("ChangeResponse", Namespace = "")]
    public class ChangeResponseEvent : BrueboxResponse
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("SeqNo")]
        public int SeqNo { get; set; }

        [XmlElement("User")]
        public string User { get; set; }

        [XmlElement("TransactionId")]
        public string TransactionId { get; set; }

        [XmlElement("Amount")]
        public int Amount { get; set; }

        [XmlElement("ManualDeposit")]
        public int ManualDeposit { get; set; }

        // <Status> без простору назв
        [XmlElement("Status", Namespace = "")]
        public StatusBlockEvent Status { get; set; }

        // <Cash type="..."> без простору назв
        [XmlElement("Cash", Namespace = "")]
        public List<CashBlockEvent> Cash { get; set; }
    }

    // -------- Допоміжні типи для без-NS варіанта --------

    [XmlType(AnonymousType = true)]
    public class StatusBlockEvent
    {
        [XmlElement("Code")]
        public int Code { get; set; }

        // <DevStatus devid="..." val="..." st="..."/> без NS
        [XmlElement("DevStatus")]
        public List<DevStatusEvent> Devices { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class DevStatusEvent
    {
        [XmlAttribute("devid")]
        public int DevId { get; set; }

        [XmlAttribute("val")]
        public int Value { get; set; }

        [XmlAttribute("st")]
        public int Status { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class CashBlockEvent
    {
        [XmlAttribute("type")]
        public int Type { get; set; }

        [XmlElement("Denomination")]
        public List<DenominationEvent> Denominations { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class DenominationEvent
    {
        [XmlAttribute("cc")]
        public string CurrencyCode { get; set; }

        [XmlAttribute("fv")]
        public int FaceValue { get; set; }

        [XmlAttribute("rev")]
        public int Rev { get; set; }

        [XmlAttribute("devid")]
        public int DeviceId { get; set; }

        [XmlElement("Piece")]
        public int Piece { get; set; }

        [XmlElement("Status")]
        public int Status { get; set; }
    }

}
