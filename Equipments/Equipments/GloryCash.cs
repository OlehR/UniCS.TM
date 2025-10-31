using ECRCommXLib;
using Equipments.Equipments.Glory;
using Front.Equipments.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelMID;
using ModelMID.DB;
using Newtonsoft.Json;
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
        public bool IsListening = false;

        public GloryCash(Equipment pEquipment, IConfiguration pConfiguration, ILoggerFactory pLoggerFactory = null,
            Action<StatusEquipment> pActionStatus = null) :
            base(pEquipment, pConfiguration, eModelEquipment.Ingenico, pLoggerFactory, pActionStatus)
        {
            State = eStateEquipment.Init;
            Url = Configuration[$"{KeyPrefix}Url"];
            UserLogin = Configuration[$"{KeyPrefix}UserLogin"];
            UserPassword = Configuration[$"{KeyPrefix}UserPassword"];
            IsListening = true;
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
                Console.WriteLine(
                    $"Result={open.Result}, Id={open.Id}, SeqNo={open.SeqNo}, User={open.User}, SessionID={open.SessionID}");
                State = eStateEquipment.On;
            }
            else State = eStateEquipment.Error;


            //початок прослуховування глорі
            GloryNetworkUtilities.GloryStartListening(IsListening, IP, IpPort);

        }

        public override Payment Purchase(decimal pAmount, decimal pCash = 0, int IdWorkPlace = 0)
        {
            return PurchaseAsync(pAmount, pCash, IdWorkPlace).Result;
        }

        public async Task<Payment> PurchaseAsync(decimal pAmount, decimal pCash = 0, int IdWorkPlace = 0)
        {
            string SOAPAction = eNameSOAPAction.ChangeOperation.ToString();
            string pData = GloryXMLData.XMLChangeOperation(SessionID, pAmount);
            FileLogger.WriteLogMessage($"Request OpenOperation: {SOAPAction} {Environment.NewLine} {pData}");
            string XMLRespons =
                await GloryNetworkUtilities.HTTPRequestAsync(Url, SOAPAction, pData,
                    200); // збільшений тайм-аут для оплати
            FileLogger.WriteLogMessage($"Respons: {Environment.NewLine} {XMLRespons}");
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

            return new Payment() { IsSuccess = false };
        }

        public override void Cancel()
        {
            base.Cancel();
        }

        public override Payment Refund(decimal pAmount, int IdWorkPlace = 0)
        {

            return RefundAsync(pAmount, IdWorkPlace).Result;
        }

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
            return $"SessionID: {SessionID}";
        }

    }
}