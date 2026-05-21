using ECRCommXLib;
using Equipments.Equipments.Glory;
using Front.Equipments.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelMID;
using ModelMID.DB;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;
using Utils;


namespace Front.Equipments
{
    public class GloryCash : CashMachine, IDisposable
    {
        private string SessionID = string.Empty;
        private string Url = string.Empty;
        private string UserLogin = string.Empty;
        private string UserPassword = string.Empty;

        public GloryCash(Equipment pEquipment, IConfiguration pConfiguration, ILoggerFactory pLoggerFactory = null,
            Action<StatusEquipment> pActionStatus = null) :
            base(pEquipment, pConfiguration, eModelEquipment.Ingenico, pLoggerFactory, pActionStatus)
        {
            State = eStateEquipment.Init;
            Url = Configuration[$"{KeyPrefix}Url"];
            UserLogin = Configuration[$"{KeyPrefix}UserLogin"];
            UserPassword = Configuration[$"{KeyPrefix}UserPassword"];
            Init();


            ;
        }

        public async void Init()
        {
            // логін для отримання SessionID
            string SOAPAction = eNameSOAPAction.OpenOperation.ToString();
            string pData = GloryXMLData.XMLLogin(UserLogin, UserPassword);
            FileLogger.WriteLogMessage($"Request OpenOperation: {SOAPAction} {Environment.NewLine} {pData}");
            string XMLRespons = await GloryNetworkUtilities.HTTPRequestAsync(Url, SOAPAction, pData);
            FileLogger.WriteLogMessage($"Respons: {Environment.NewLine} {XMLRespons}");
            var msg = SoapParser.Parse(XMLRespons);
            if (msg is OpenResponse open)
            {
                SessionID = open.SessionID;
                // Атрибут result зчитується базовим BrueboxResponse через XmlAnyAttribute
                FileLogger.WriteLogMessage($"Result={open.Result}, Id={open.Id}, SeqNo={open.SeqNo}, User={open.User}, SessionID={open.SessionID}");
                State = eStateEquipment.On;
                //var aa = InventoryAsync().Result;
            }
            else State = eStateEquipment.Error;




        }
        public void startListening()
        {
            GloryNetworkUtilities.GloryStartListening(IP, IpPort, OnStatus);
        }
        public override Payment Purchase(decimal pAmount, int IdWorkPlace = 0) => AsyncHelper.RunSync(async() => await PurchaseAsync(pAmount,  IdWorkPlace));
        

        public async Task<Payment> PurchaseAsync(decimal pAmount,  int IdWorkPlace = 0)
        {
            try
            {

                //початок прослуховування глорі
                GloryNetworkUtilities.IsListening = true;
                new Thread(new ThreadStart(this.startListening))
                {
                    IsBackground = true
                }.Start();

                string SOAPAction = eNameSOAPAction.ChangeOperation.ToString();
                string pData = GloryXMLData.XMLChangeOperation(SessionID, pAmount);
                FileLogger.WriteLogMessage($"Request OpenOperation: {SOAPAction} {Environment.NewLine} {pData}");
                string XMLRespons =
                    await GloryNetworkUtilities.HTTPRequestAsync(Url, SOAPAction, pData,
                        200); // збільшений тайм-аут для оплати
                FileLogger.WriteLogMessage($"Respons pay: {Environment.NewLine} {XMLRespons}");
                var msg = SoapParser.Parse(XMLRespons);

                if (msg is ChangeResponse resp)
                {
                    //Console.WriteLine($"Result={resp.Result}, Amount={resp.Amount}, ManualDeposit={resp.ManualDeposit}");
                    //Console.WriteLine($"Status.Code={resp.Status?.Code}");
                    //foreach (var d in resp.Status?.Devices ?? new List<DevStatus>())
                    //    Console.WriteLine($"Dev {d.DevId}: val={d.Value}, st={d.Status}");

                    //foreach (var cash in resp.Cash ?? new List<CashBlock>())
                    //{
                    //    Console.WriteLine($"Cash type={cash.Type}");
                    //    foreach (var den in cash.Denominations ?? new List<Denomination>())
                    //        Console.WriteLine($"  {den.CurrencyCode} {den.FaceValue}: piece={den.Piece}, status={den.Status}, devid={den.DeviceId}");
                    //}
                    return new Payment()
                    {
                        IsSuccess = true,
                        //TransactionId = string.Format("{0}", (object)BPOS.TrnBatchNum), //приходить лише в онлайн лістенері
                        DateCreate = DateTime.Now,
                        TransactionStatus = GetErrorText(resp.Result),
                        SumPay = Math.Round(Convert.ToDecimal(resp.Amount) / 100M, 2, MidpointRounding.AwayFromZero)
                    };
                }

                
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"Error pay: {Environment.NewLine} {ex}");
            }
            finally {
                GloryNetworkUtilities.IsListening = false;
            }
            return new Payment() { IsSuccess = false };
        }

        public override void Cancel()
        {
            base.Cancel();
        }

        public override Payment Refund(decimal pAmount, int IdWorkPlace = 0) => AsyncHelper.RunSync(async () => await RefundAsync(pAmount, IdWorkPlace));


        public async Task<Payment> RefundAsync(decimal pAmount, int IdWorkPlace = 0)
        {
            string SOAPAction = eNameSOAPAction.CashoutOperation.ToString();
            string pData = GloryXMLData.XMLCashoutOperation(SessionID, Decimal.ToInt32(pAmount));
            FileLogger.WriteLogMessage($"Request OpenOperation: {SOAPAction} {Environment.NewLine} {pData}");
            string XMLRespons =
                await GloryNetworkUtilities.HTTPRequestAsync(Url, SOAPAction, pData,
                    200); // збільшений тайм-аут для оплати
            FileLogger.WriteLogMessage($"Respons: {Environment.NewLine} {XMLRespons}");
            var msg = SoapParser.Parse(XMLRespons);
            if (msg is CashoutResponse resp)
            {
                //Console.WriteLine($"Result={resp.Result}, Id={resp.Id}, SeqNo={resp.SeqNo}, User={resp.User}");
                //foreach (var c in resp.Cash ?? new List<CashBlock>())
                //    Console.WriteLine($"Cash type={c.Type}, denoms={c.Denominations?.Count ?? 0}");
                if (resp.Result == (int)eResultCode.Success)
                {
                    return new Payment() { IsSuccess = true };
                }
            }

            return new Payment() { IsSuccess = false };
        }

        public void Dispose()
        {

        }

        public string GetErrorText(int? pCodeError = 9999)
        {
            return Enum.GetName(typeof(eResultCode), pCodeError) ?? $"Unknown({pCodeError})";
        }

        override public StatusEquipment TestDevice()
        {
            return new StatusEquipment(eModelEquipment.GloryCash, State);
        }

        override public string GetDeviceInfo()
        {
            var getInfo = AsyncHelper.RunSync(async () => await GetStatus());

            var result = Enum.IsDefined(typeof(eResultCode), getInfo.Result)
                ? (eResultCode)getInfo.Result
                : eResultCode.UnknownError;

            var statusCode = Enum.IsDefined(typeof(eStatusChangeEvent), getInfo.Status.Code)
                ? (eStatusChangeEvent)getInfo.Status.Code
                : eStatusChangeEvent.Initializing;

            var devices = string.Join(Environment.NewLine, getInfo.Status.Devices
                .Select(d => $"  DevId={d.DevId}, Val={d.Value}, St={d.Status}"));

            return $"SessionID: {SessionID}{Environment.NewLine}" +
                   $"Result={result.GetDescription()}, Code={statusCode.GetDescription()}, " +
                   $"Id={getInfo.Id}, SeqNo={getInfo.SeqNo}, User={getInfo.User}" +
                   $"{Environment.NewLine}{devices}";
        }
        public async Task<StatusResponse> GetStatus()
        {
            string SOAPAction = eNameSOAPAction.GetStatus.ToString();
            string pData = GloryXMLData.XMLGetStatus(SessionID);
            FileLogger.WriteLogMessage($"Request XMLGetStatus: {SOAPAction} {Environment.NewLine} {pData}");
            string XMLRespons =
                await GloryNetworkUtilities.HTTPRequestAsync(Url, SOAPAction, pData);
            FileLogger.WriteLogMessage($"Respons: {Environment.NewLine} {XMLRespons}");
            var msg = SoapParser.Parse(XMLRespons);
            string res = "";
            if (msg is StatusResponse s)
            {
                return s;
            }
            else
                return new ();
        }

        public override List<CashInventory> Inventory()
        {
            return InventoryAsync().Result;
        }
        public override async Task<List<CashInventory>> InventoryAsync()
        {
            //отримання актуальних купюр
            string SOAPAction = eNameSOAPAction.InventoryOperation.ToString();
            string pData = GloryXMLData.XMLInventoryOperation(SessionID);
            FileLogger.WriteLogMessage($"Request {SOAPAction}:  {Environment.NewLine} {pData}");
            string XMLRespons = await GloryNetworkUtilities.HTTPRequestAsync(Url, SOAPAction, pData);
            FileLogger.WriteLogMessage($"ResponsGloru: {Environment.NewLine} {XMLRespons}");
            InventoryResponse Inventory = (InventoryResponse)SoapParser.Parse(XMLRespons);
            return FromInventoryResponse(Inventory);
        }
        /// <summary>
        /// Конвертує InventoryResponse у список CashInventory
        /// </summary>
        public static List<CashInventory> FromInventoryResponse(InventoryResponse response)
        {
            if (response?.Cash == null || response.Cash.Count < 2)
                return new List<CashInventory>();

            var result = new List<CashInventory>();

            var allBlock = response.Cash[0];
            var drumBlock = response.Cash[1];

            // All
            foreach (var den in allBlock.Denominations ?? Enumerable.Empty<Denomination>())
            {
                result.Add(new CashInventory
                {
                    FaceValue = den.FaceValue,
                    Quantity = den.Piece,
                    TypeMoney = den.DeviceId == 1 ? eTypeMoney.Banknote : eTypeMoney.Coin,
                    MoneyStoragePlace = eMoneyStoragePlace.All
                });
            }

            // Drum
            foreach (var den in drumBlock.Denominations ?? Enumerable.Empty<Denomination>())
            {
                result.Add(new CashInventory
                {
                    FaceValue = den.FaceValue,
                    Quantity = den.Piece,
                    TypeMoney = den.DeviceId == 1 ? eTypeMoney.Banknote : eTypeMoney.Coin,
                    MoneyStoragePlace = eMoneyStoragePlace.Drum
                });
            }

            // Safe = All - Drum
            var allLookup = allBlock.Denominations?
                .ToDictionary(d => (d.FaceValue, d.DeviceId), d => d.Piece)
                ?? new Dictionary<(int, int), int>();

            var drumLookup = drumBlock.Denominations?
                .ToDictionary(d => (d.FaceValue, d.DeviceId), d => d.Piece)
                ?? new Dictionary<(int, int), int>();

            foreach (var key in allLookup.Keys)
            {
                var allQty = allLookup[key];
                var drumQty = drumLookup.TryGetValue(key, out var d) ? d : 0;

                result.Add(new CashInventory
                {
                    FaceValue = key.FaceValue,
                    Quantity = allQty - drumQty,
                    TypeMoney = key.DeviceId == 1 ? eTypeMoney.Banknote : eTypeMoney.Coin,
                    MoneyStoragePlace = eMoneyStoragePlace.Safe
                });
            }

            return result;
        }
    }
}