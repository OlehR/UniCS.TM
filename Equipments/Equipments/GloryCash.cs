using ECRCommXLib;
using Equipments.Equipments.Glory;
using Front.Equipments.Implementation.ModelVchasno;
using Front.Equipments.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelMID;
using ModelMID.DB;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using System.Xml.Serialization;
using Utils;
using static System.Windows.Forms.AxHost;


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
        public override Payment Purchase(decimal pAmount, int IdWorkPlace = 0) => AsyncHelper.RunSync(async () => await PurchaseAsync(pAmount, IdWorkPlace));


        public async Task<Payment> PurchaseAsync(decimal pAmount, int IdWorkPlace = 0)
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
                    if (resp.Result == (int)eResultCode.Success || resp.Result == (int)eResultCode.DesignationDenominationShortage) //якогось хрена коли є решта і він має чим її віддати всеодно вертає помилку
                    {
                        return new Payment()
                        {
                            IsSuccess = true,
                            //TransactionId = string.Format("{0}", (object)BPOS.TrnBatchNum), //приходить лише в онлайн лістенері
                            DateCreate = DateTime.Now,
                            TransactionStatus = GetErrorText(resp.Result),
                            SumPay = Math.Round(Convert.ToDecimal(resp.Amount) / 100M, 2, MidpointRounding.AwayFromZero)
                        };

                    }
                    else
                    {
                        //State = eStateEquipment.Error;
                        OnStatus?.Invoke(new CashMachineStatus() { Status = eStatusChangeEvent.CanceledByUser, TextError = GetErrorText(resp.Result) });
                        return new Payment()
                        {
                            IsSuccess = false,
                            DateCreate = DateTime.Now,
                            TransactionStatus = GetErrorText(resp.Result)
                        };
                    }

                }


            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"Error pay: {Environment.NewLine} {ex}");
            }
            finally
            {
                GloryNetworkUtilities.IsListening = false;
            }
            return new Payment() { IsSuccess = false };
        }

        public override void Cancel() => AsyncHelper.RunSync(async () => await CancelAsync());
        private async Task CancelAsync()
        {
            string SOAPAction = eNameSOAPAction.ChangeCancelOperation.ToString();
            string XMLText = GloryXMLData.XMLChangeCancelOperation(SessionID);
            FileLogger.WriteLogMessage($"Request {SOAPAction}:  {Environment.NewLine} {XMLText}");
            string XMLRespons = await GloryNetworkUtilities.HTTPRequestAsync(Url, SOAPAction, XMLText);
            FileLogger.WriteLogMessage($"ResponsGloru: {Environment.NewLine} {XMLRespons}");
            var msg = SoapParser.Parse(XMLRespons);
            if (msg is ChangeCancelResponse resp)
            {
                //Console.WriteLine($"Result={resp.Result}, Id={resp.Id}, SeqNo={resp.SeqNo}, User={resp.User}");
                if (resp.Result != (int)eResultCode.Success)
                {
                    State = eStateEquipment.Error;
                    //MessageBox.Show(GetErrorText(resp.Result));
                }
                else
                    State = eStateEquipment.On;
            }
        }

        public override Payment Refund(decimal pAmount, int IdWorkPlace = 0) => AsyncHelper.RunSync(async () => await CashoutAsync(pAmount)).ResultCode == eResultCode.Success ? new Payment() { IsSuccess = true } : new Payment() { IsSuccess = false };
        public override CashMachineStatus Cashout(decimal pAmount) => AsyncHelper.RunSync(async () => await CashoutAsync(pAmount));



        public async Task<CashMachineStatus> CashoutAsync(decimal pAmount)
        {
            string SOAPAction = eNameSOAPAction.CashoutOperation.ToString();
            string pData = GloryXMLData.XMLCashoutOperation(SessionID, Decimal.ToInt32(pAmount));
            FileLogger.WriteLogMessage($"Request OpenOperation: {SOAPAction} {Environment.NewLine} {pData}");
            string XMLRespons =
                await GloryNetworkUtilities.HTTPRequestAsync(Url, SOAPAction, pData,
                    200); // збільшений тайм-аут для оплати
            FileLogger.WriteLogMessage($"Respons: {Environment.NewLine} {XMLRespons}");

            if (SoapParser.Parse(XMLRespons) is not CashoutResponse res)
                return new();

            return new CashMachineStatus
            {
                ResultCode = Enum.IsDefined(typeof(eResultCode), res.Result)
                    ? (eResultCode)res.Result
                    : eResultCode.UnknownError,
                Cash = FromCashBlocks(res?.Cash, singleBlock: true)
            };

        }

        public void Dispose()
        {

        }

        public string GetErrorText(int? pCodeError = 9999)
        {
            return (Enum.IsDefined(typeof(eResultCode), pCodeError)
                    ? (eResultCode)pCodeError
                    : eResultCode.UnknownError).GetDescription();
        }


        override public string GetDeviceInfo()
        {
            var getInfo = AsyncHelper.RunSync(async () => await GetStatusAsync());

            return $"SessionID: {SessionID}{Environment.NewLine}" +
                   $"Стан кеш-машини: {getInfo.Status.GetDescription()}, Відповідь на запит: {getInfo.ResultCode.GetDescription()}, ";
        }
        public override CashMachineStatus GetStatus()
        {
            return AsyncHelper.RunSync(async () => await GetStatusAsync());
        }
        public async Task<CashMachineStatus> GetStatusAsync()
        {
            var soapAction = eNameSOAPAction.GetStatus.ToString();
            var requestData = GloryXMLData.XMLGetStatus(SessionID);

            FileLogger.WriteLogMessage($"Request XMLGetStatus: {soapAction}{Environment.NewLine}{requestData}");

            var xmlResponse = await GloryNetworkUtilities.HTTPRequestAsync(Url, soapAction, requestData);

            FileLogger.WriteLogMessage($"Response:{Environment.NewLine}{xmlResponse}");

            if (SoapParser.Parse(xmlResponse) is not StatusResponse res)
                return new();

            return new CashMachineStatus
            {
                ResultCode = Enum.IsDefined(typeof(eResultCode), res.Result)
                    ? (eResultCode)res.Result
                    : eResultCode.UnknownError,

                Status = Enum.IsDefined(typeof(eStatusChangeEvent), res.Status.Code)
                    ? (eStatusChangeEvent)res.Status.Code
                    : eStatusChangeEvent.Initializing
            };
        }
        public override StatusEquipment TestDevice()
        {
            var getInfo = AsyncHelper.RunSync(async () => await GetStatusAsync());
            if (getInfo.Status == eStatusChangeEvent.Idle)
            {
                State = eStateEquipment.On;
            }
            else
            {
                State = eStateEquipment.Error;
            }

            return new StatusEquipment(eModelEquipment.GloryCash, State, getInfo.Status.GetDescription());
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
            => FromCashBlocks(response?.Cash);
        /// <summary>
        /// Конвертує EndReplenishmentFromEntranceResponse у список CashInventory
        /// </summary>
        public static List<CashInventory> FromEndReplenishmentResponse(EndReplenishmentFromEntranceResponse response)
            => FromCashBlocks(response?.Cash, singleBlock: true);

        private static List<CashInventory> FromCashBlocks(List<CashBlock> cash, bool singleBlock = false)
        {
            if (cash == null || cash.Count == 0)
                return new List<CashInventory>();

            if (singleBlock)
            {
                // Тільки All — один масив
                return cash[0].Denominations?
                    .Select(den => new CashInventory
                    {
                        FaceValue = den.FaceValue,
                        Quantity = den.Piece,
                        TypeMoney = den.DeviceId == 1 ? eTypeMoney.Banknote : eTypeMoney.Coin,
                        MoneyStoragePlace = eMoneyStoragePlace.All
                    })
                    .ToList()
                    ?? new List<CashInventory>();
            }

            if (cash.Count < 2)
                return new List<CashInventory>();

            var result = new List<CashInventory>();

            var allBlock = cash[0];
            var drumBlock = cash[1];

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
        public override CashMachineStatus StartReplenishment()
        {
            return AsyncHelper.RunSync(async () => await StartReplenishmentAsync());
        }
        public override CashMachineStatus EndReplenishment()
        {
            return AsyncHelper.RunSync(async () => await EndReplenishmentAsync());
        }
        public async Task<CashMachineStatus> StartReplenishmentAsync()
        {
            var soapAction = eNameSOAPAction.StartReplenishmentFromEntranceOperation.ToString();
            var requestData = GloryXMLData.XMLStartReplenishmentFromEntrance(SessionID);

            FileLogger.WriteLogMessage($"Request XMLGetStatus: {soapAction}{Environment.NewLine}{requestData}");

            var xmlResponse = await GloryNetworkUtilities.HTTPRequestAsync(Url, soapAction, requestData);

            FileLogger.WriteLogMessage($"Response:{Environment.NewLine}{xmlResponse}");
            var msg = SoapParser.Parse(xmlResponse);
            //if (msg is StartReplenishmentFromEntranceResponse resp)
            //{
            //    Console.WriteLine($"Result={resp.Result}, Id={resp.Id}, SeqNo={resp.SeqNo}, User='{resp.User}'");
            //    if (resp.Result != (int)eResultCode.Success)
            //    {
            //        MessageBox.Show(GetErrorText(resp.Result));

            //    }
            //}
            if (SoapParser.Parse(xmlResponse) is not StartReplenishmentFromEntranceResponse res)
                return new();

            return new CashMachineStatus
            {
                ResultCode = Enum.IsDefined(typeof(eResultCode), res.Result)
                    ? (eResultCode)res.Result
                    : eResultCode.UnknownError,
            };
        }
        public async Task<CashMachineStatus> EndReplenishmentAsync()
        {
            var soapAction = eNameSOAPAction.EndReplenishmentFromEntranceOperation.ToString();
            var requestData = GloryXMLData.XMLEndReplenishmentFromEntranceOperation(SessionID);

            FileLogger.WriteLogMessage($"Request XMLGetStatus: {soapAction}{Environment.NewLine}{requestData}");

            var xmlResponse = await GloryNetworkUtilities.HTTPRequestAsync(Url, soapAction, requestData);

            FileLogger.WriteLogMessage($"Response:{Environment.NewLine}{xmlResponse}");
            var msg = SoapParser.Parse(xmlResponse);
            //if (msg is EndReplenishmentFromEntranceResponse resp)
            //{
            //    Console.WriteLine($"Result={resp.Result}, Id={resp.Id}, SeqNo={resp.SeqNo}, User='{resp.User}', ManualDeposit={resp.ManualDeposit}");
            //    foreach (var cash in resp.Cash ?? new List<CashBlock>())
            //    {
            //        Console.WriteLine($"Cash type={cash.Type}");
            //        foreach (var d in cash.Denominations ?? new List<Denomination>())
            //            Console.WriteLine($"  {d.CurrencyCode} {d.FaceValue}: piece={d.Piece}, status={d.Status}, devid={d.DeviceId}");
            //    }
            //    if (resp.Result != (int)eResultCode.Success)
            //    {
            //        MessageBox.Show(GetErrorText(resp.Result));
            //    }
            //}


            if (SoapParser.Parse(xmlResponse) is not EndReplenishmentFromEntranceResponse res)
                return new();

            return new CashMachineStatus
            {
                ResultCode = Enum.IsDefined(typeof(eResultCode), res.Result)
                    ? (eResultCode)res.Result
                    : eResultCode.UnknownError,
                Cash = FromEndReplenishmentResponse(res)
            };
        }

        public override bool UnLockUnit(eTypeUnit pTypeUnit) => AsyncHelper.RunSync(async () => await UnLockUnitAsync(pTypeUnit));
        public override bool LockUnit(eTypeUnit pTypeUnit) => AsyncHelper.RunSync(async () => await LockUnitAsync(pTypeUnit));

        public async Task<bool> UnLockUnitAsync(eTypeUnit pTypeUnit)
        {
            string SOAPAction = eNameSOAPAction.UnLockUnitOperation.ToString();
            string XMLText = GloryXMLData.XMLUnLockUnitOperation(SessionID, pTypeUnit);
            FileLogger.WriteLogMessage($"Request {SOAPAction}:  {Environment.NewLine} {XMLText}");
            string XMLRespons = await GloryNetworkUtilities.HTTPRequestAsync(Url, SOAPAction, XMLText);
            FileLogger.WriteLogMessage($"ResponsGloru: {Environment.NewLine} {XMLRespons}");


            if (SoapParser.Parse(XMLRespons) is not StartReplenishmentFromEntranceResponse res)
                return false;

            return Enum.IsDefined(typeof(eResultCode), res.Result);

        }

        public async Task<bool> LockUnitAsync(eTypeUnit pTypeUnit)
        {
            string SOAPAction = eNameSOAPAction.LockUnitOperation.ToString();
            string XMLText = GloryXMLData.XMLLockUnitOperation(SessionID, pTypeUnit);
            FileLogger.WriteLogMessage($"Request {SOAPAction}:  {Environment.NewLine} {XMLText}");
            string XMLRespons = await GloryNetworkUtilities.HTTPRequestAsync(Url, SOAPAction, XMLText);
            FileLogger.WriteLogMessage($"ResponsGloru: {Environment.NewLine} {XMLRespons}");


            if (SoapParser.Parse(XMLRespons) is not StartReplenishmentFromEntranceResponse res)
                return false;

            return Enum.IsDefined(typeof(eResultCode), res.Result);

        }

        public override CashMachineStatus Collect(List<CashInventory> pCashInventories, eTypeCollectMoney pTypeCollectMoney)
            => AsyncHelper.RunSync(async () => await CollectAsync(pCashInventories, pTypeCollectMoney));
        public async Task<CashMachineStatus> CollectAsync(List<CashInventory> pCashInventories, eTypeCollectMoney pTypeCollectMoney)
        {
            string SOAPAction = eNameSOAPAction.CollectOperation.ToString();
            string XMLText = GloryXMLData.XMLCollectOperation(SessionID, pCashInventories, pTypeCollectMoney);
            FileLogger.WriteLogMessage($"Request {SOAPAction}:  {Environment.NewLine} {XMLText}");
            string XMLRespons = await GloryNetworkUtilities.HTTPRequestAsync(Url, SOAPAction, XMLText);
            FileLogger.WriteLogMessage($"ResponsGloru: {Environment.NewLine} {XMLRespons}");
            //var msg = SoapParser.Parse(XMLRespons);
            //if (msg is CollectResponse resp)
            //{
            //    var a = resp.SeqNo;
            //}
            if (SoapParser.Parse(XMLRespons) is not CollectResponse res)
                return new();

            return new CashMachineStatus
            {
                ResultCode = Enum.IsDefined(typeof(eResultCode), res.Result)
                    ? (eResultCode)res.Result
                    : eResultCode.UnknownError,
            };
        }
    }
}