using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
//using ModernExpo.SelfCheckout.Entities.Pos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ECRCommXLib;
using System.Threading;
using Newtonsoft.Json;
using System.Linq;
using Front.Equipments.Ingenico;
using ModelMID;

namespace Front.Equipments
{
    public class IngenicoH : BankTerminal
    {

        Ingenico.Ingenico EquipmentIngenico = null;
        //public IngenicoH(string pSerialPortName, int pBaudRate = 9600, Action<string, string> pLogger = null) : base(pSerialPortName, pBaudRate, pLogger) { }
        public IngenicoH(IConfiguration pConfiguration, Action<string, string> pLogger = null, Action<IPosStatus> pActionStatus = null) : base(pConfiguration, pLogger, eModelEquipment.Ingenico)
        {
            ILoggerFactory loggerFactory = new LoggerFactory().AddConsole((_, __) => true);
            ILogger<Ingenico.Ingenico> logger = loggerFactory.CreateLogger<Ingenico.Ingenico>();
            EquipmentIngenico = new Ingenico.Ingenico(pConfiguration, logger);

            EquipmentIngenico.OnStatus += pActionStatus;
        }

        public override PaymentResultModel Purchase(decimal pAmount)
        {
            return EquipmentIngenico.Purchase(Convert.ToDouble(pAmount)).Result;
        }

        public override PaymentResultModel Refund(decimal pAmount, string pRRN)
        {
            return EquipmentIngenico.Refund(Convert.ToDouble(pAmount), pRRN).Result;
        }

        public override BatchTotals PrintZ()
        {
            var r = EquipmentIngenico.GetBatchTotals();
            return r.Result;
        }

        public override BatchTotals PrintX()
        {
            var r = EquipmentIngenico.GetBatchTotals();
            return r.Result;
        }

        public override eStateEquipment TestDevice()
        {
            EquipmentIngenico.TestDeviceSync();
            return eStateEquipment.Ok;
        }
    }
}

namespace Front.Equipments.Ingenico
{

    public enum ReturnCodes
    {
        ECRC_OK,
        ECRC_FAIL,
        ECRC_FAILEDTOOPENPORT,
        ECRC_FAILEDTOENUMETAREPORTS,
        ECRC_FAILEDTOCONNECT,
        ECRC_NOTOPENED,
        ECRC_NOTINITED,
        ECRC_BUSY,
        ECRC_NODATA,
        ECRC_MESSAGECORRUPTED,
        ECRC_TOOSMALLBUFFER,
        ECRC_DISCONNECTED,
        ECRC_RECVTIMEOUT,
        ECRC_SENDMESSAGE,
        ECRC_CONNECTION,
        ECRC_READ_FILE,
        ECRC_UNKNOWNERROR,
        ECRC_CHECKTERM_TO,
    }

    public enum DeviceConnectionStatus
    {
        NotConnected = 1,
        InitializationError = 2,
        Enabled = 3,
        Disabled = 4,
    }

    /*public enum ePosStatus
    {
        StatusCodeIsNotAvailable,
        CardWasRead,
        UsedAChipCard,
        AuthorizationInProgress,
        WaitingForCashierAction,
        PrintingReceipt,
        PinEntryIsNeeded,
        CardWasRemoved,
        EMVMultiAids,
        WaitingForCard,
        InProgress,
        CorrectTransaction,
        PinInputWaitKey,
        PinInputBackspacePressed,
        PinInputKeyPressed,
        ErrorOpeningCOMPort,
        NeedToOpenCOMPort,
        ErrorConnectingWithTerminal,
        TerminalReturnedAnError,
        SuccessfullyFulfilled,
        Error,
        AuthorizationRejectedNoPayment,
        WrongPIN,
        NotEnoughMoney,
        GeneralError,
        TransactionCanceledByUser,
        EMVDecline,
        TransactionLogIsFullNeedCloseBatch,
        NoConnectionWithHost,
        NoPaperInPrinter,
        ErrorCryptoKeys,
        CardReaderIsNotConnected,
        TransactionIsAlreadyComplete,
        ApprovedAndCompleted,
        TheCardIsInOrderThereIsNoReasonToRefuse,
        AuthorizationDenied,
        UnregisteredTradingPoint,
        AuthorizationRejectedWithdrawTheCardAtTheBanksRequest,
        CommonErrorNeedToRepeat,
        InvalidTransactionNetworkErrorNeedToRepeat,
        IncorrectAmountEntered,
        InvalidCardNumber,
        BankNodeIsNotFoundOnTheNetwork,
        CanceledByTheClient,
        ActionsNotCompletedDidNotMatchData,
        NoResponseFileIsTemporarilyUnavailable,
        WrongFormatNeedToRepeat,
        TheIssuerIsNotFoundInThePaymentSystem,
        PartiallyCompleted,
        TheValidityPeriodOfTheCardHasExpiredTheCardHasBeenWithdrawnAtTheBanksRequest,
        ForbiddenCardRemove,
        WithdrawnByTheIssuerRemovedFromTheCardAndContactedByTheAcquirer,
        ThereAreNoAttemptsToEnterThePINRemoveTheCard,
        NoClientsCreditAccount,
        CardIslostRemoved,
        CardIsStolenRemoved,
        NoSettlementSpecifiedClienAccount,
        ThereIsNoCumulativeAccountOfTheClient,
        TheExpirationDateOfTheCardExpires,
        ThisTransactionTypeIsNotProvidedForTheGivenCard,
        ThisTypeOfTransactionIsNotProvidedForPOSTerminal,
        TheAamountOfAuthorizationExceededTheExpenseLimitOnTheCard,
        IncorrectServiceCodeForbiddenCardCanNotBeSeized,
        TheAmountOfTheCancellationAuthorizationIsDifferentFromTheAmountOfTheOriginalAuthorization,
        TheExpenseLimitExpiredOnTheAccount,
        TheCardIsVoidCanNotBeSeized,
        CardIsWithdrawnFromATM,
        ItIsTooLateToReceiveAnAnswerFromTheNetworkItIsNecessaryToRepeat,
        TheNumberOfIncorrectlyEnteredPINsExceededTheAmountDischarged,
        ActionsAreNotCompletedIncompleteDataItIsNecessaryToRollbackOrRepeat,
        NoAccount,
        AlreadyCanceledWhenTurnedOn,
        GeneralNetworkErrorIncorrectData,
        RemoteNetworkErrorOrPINEncryption,
        TimeoutWhenConnectedWithTheIssuersNodeOrWrongCVVOrCacheIsNotApprovedTheCashbackSumLimitIsExceeded,
        ThePINVerificationTransactionIsUnsuccessfulNetworkError,
        PINCanNotBeCheckedNetworkError,
        PINEncryptionErrorNetworkError,
        IdentificationErrorIsANetworkError,
        NoConnectionWithTheBankByTheIssuerNetworkError,
        UnsuccessfulRequestRoutingIsNotPossibleNetworkError,
        TheTransactionCanNotBeCompletedTheIssuerDeclineAuthorizationDueToAViolationOfTheRules,
        DuplicationOfTransmissionNetworkError,
        GeneralSystemMalfunction,
        UnableToSendEncryptedMessage,
    }*/

    public interface IPosStatus {}

    public class PosStatus : IPosStatus
    {
        public ePosStatus Status { get; set; }

        public byte MsgCode { get; set; }

        public string MsgDescription { get; set; }
    }

    public class BatchTotals
    {
        public uint DebitCount { get; set; }

        public uint DebitSum { get; set; }

        public uint CreditCount { get; set; }

        public uint CreditSum { get; set; }

        public uint CencelledCount { get; set; }

        public uint CencelledSum { get; set; }
    }

    public class PaymentResultModel
    {
        public bool IsSuccess { get; set; }

        public string TransactionId { get; set; }

        public string TransactionCode { get; set; }

        public string TransactionStatus { get; set; }

        public string CardPan { get; set; }

        public string AuthCode { get; set; }

        public string TerminalId { get; set; }

        public DateTime OperationDateTime { get; set; }

        public Decimal PosPaid { get; set; }

        public Decimal PosAddAmount { get; set; }

        public long InvoiceNumber { get; set; }

        public string CardHolder { get; set; }

        public string IssuerName { get; set; }

        public string Bank { get; set; }

        public PaymentResultModel() => this.IsSuccess = false;
    }

    /*public class BPOSClient
    {
        private const string BPOSLib = "libBPOSLib.so";
        private const string CommLibX = "libCommLibX.so";
        private const string ECRLibX = "libECRLibX.so";

        [DllImport("testDll.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern IntPtr fntestDll([MarshalAs(UnmanagedType.LPStr)] string i);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int CommOpen(string sPort, int iBaudRate);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int CommOpenTCP(string sIP, string sPort);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int CommOpenAuto(int iBaudRate);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int CommClose();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int CheckConnection(byte MerchIdx);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int Purchase(int iAmount, int iAddAmount, byte bMerchIdx);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int Refund(int iAmount, int iAddAmount, byte bMerchIdx, string RRN);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int Void(int InvoiceNum, byte MerchIdx);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int Balance(byte MerchIdx, string CurrCode, byte Accnumber);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int Deposit(byte MerchIdx, int Amount, string CurrCode, byte Accnumber);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int CashAdvance(
          byte MerchIdx,
          int Amount,
          string CurrCode,
          byte AccNumber);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int Completion(byte MerchIdx, int Amount, string RRN, int InvoiceNum);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int ReadCard();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int ReadBankCard();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int PurchaseService(byte MerchIdx, int Amount, string ServiceParams);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int IdentifyCard(byte MerchIdx, string CurrCode, byte AccNumber);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int POSGetInfo();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int POSExTransaction();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int Settlement(byte MerchIdx);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int PrintBatchTotals(byte MerchIdx);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int PrintLastSettleCopy(byte MerchIdx);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int PrintBatchJournal(byte MerchIdx);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int GetBatchTotals(byte MerchIdx);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int GetTxnDataByInv(int InvoiceNum, byte MerchIdx);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int GetTxnNum();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int GetTxnDataByOrder(int OrderNum);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int ReqCurrReceipt();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int ReqReceiptByInv(int InvoiceNum, byte MerchIdx);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int Confirm();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int SelectApp(string AppName, int AppIdx);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int CloseApp();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int StartScenario(int ScenarioID, string ScenarioData);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int SetExtraPrintData(string ExtraPrintData);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int SetExtraXmlData(string ExtraXmlData);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int SetErrorLang(byte ErrorLanguage);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int SendFile(string FullPath, byte ECRDataType, byte ECRCommand);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int CorrectTransaction(int Amount, int AddAmount);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int useLogging(byte Logginglevel, string FilePath);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int UseMac(byte macType, string bsKey);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern byte get_LastResult();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern byte get_LastErrorCode();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_LastErrorDescription();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int Cancel();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int Ping();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int CheckTerminal();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern byte get_LastStatMsgCode();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_LastStatMsgDescription();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_ResponseCode();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_Receipt();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_LibraryVersion();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int ReqDataFile();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_DataFile();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_PAN();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_PanHash();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_SlipPrinted();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_DateTime();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_TerminalID();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_MerchantID();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_AuthCode();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_Amount();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_AddAmount();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern byte get_TxnType();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern byte get_EntryMode();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_emvAID();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_ExpDate();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_CardHolder();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_IssueName();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_InvoiceNum();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_CompletionInvoiceNum();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_RRN();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern byte get_SignVerif();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_Track3();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_AddData();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_CryptedData();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_ExtraCardData();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_TerminalInfo();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_DiscountName();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_DiscountAttribute();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_ECRDataTM();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern byte get_TrnStatus();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_Currency();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_TrnBatchNum();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_RNK();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_CurrencyCode();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_FlagAcquirer();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_TotalsDebitAmt();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_TotalsDebitNum();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_TotalsCreditAmt();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_TotalsCreditNum();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_TotalsCancelledAmt();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_TotalsCancelledNum();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int get_TxnNum();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern string get_ScenarioData();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern byte get_Key();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern byte get_TermStatus();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int EnterControlMode();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int ExitControlMode();

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int SetControlMode(bool isCtrlMode);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int ReadKey(byte TimeOut);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int DisplayText(byte Beep);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int SetLine(byte Row, byte Col, string Text, byte Invert);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int SetScreen(int ScreenNumber);

        [DllImport("libBPOSLib.so", CharSet = CharSet.Unicode)]
        public static extern int ExchangeStatuses(byte bECRStatus);
    }
    */
    
    public class PosDeviceLog : DeviceLog
    {
        public PosDeviceLog() => this.DeviceType = DeviceType.PosTerminal;
    }
    
    public interface IPosResponse
    {
    }
    
    public class PayPosResponse : IPosResponse
    {
        public PaymentResultModel Response { get; set; }

        public List<string> Errors { get; set; }

        public PayPosResponse() => this.Errors = new List<string>();
    }

    public interface IPosTerminal : IBaseDevice, IDisposable
    {
        Task<PaymentResultModel> Purchase(double amount);

        Task<PaymentResultModel> Refund(double amount, string bsRRN);

        void Settlement();

        void PrintLastSettleCopy();

        void PrintBatchTotals();

        Task<BatchTotals> GetBatchTotals();

        void PrintBatchJournal();

        Action<IPosResponse> OnResponse { get; set; }

        Action<IPosStatus> OnStatus { get; set; }

        void Cancel();

        List<string> GetLastReceipt();
    }
    
    public interface IBaseDevice : IDisposable
    {
        Action<DeviceLog> OnDeviceWarning { get; set; }

        bool IsReady { get; }

        DeviceConnectionStatus Init();

        Task<DeviceConnectionStatus> GetDeviceStatus();

        Task<DeviceConnectionStatus> TestDevice();

        Task<string> GetInfo();
    }
    
    public class Ingenico : IPosTerminal, IBaseDevice, IDisposable
    {
        private const string KeyPrefix = "Devices:Ingenico:";
        private static BPOS1LibClass _bpos1LibClass;
        private static bool _isCancelRequested;
        private byte port;
        private int boundRate;
        private byte merchantId;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Ingenico> _logger;
        private readonly Encoding _encoding1251 = Encoding.GetEncoding(1251);
        private readonly Encoding _encoding1252 = Encoding.GetEncoding(1252);

        public Action<IPosResponse> OnResponse { get; set; }

        public Action<IPosStatus> OnStatus { get; set; }

        public Action<DeviceLog> OnDeviceWarning { get; set; }

        public bool IsReady => true;

        public Ingenico(IConfiguration configuration, ILogger<Ingenico> logger)
        {
            this._configuration = configuration;
            this._logger = logger;
        }

        public async Task<PaymentResultModel> Refund(double amount, string bsRRN)
        {
            Ingenico ingenico = this;
            try
            {
                if (!ingenico.StartBPOS())
                    return new PaymentResultModel()
                    {
                        IsSuccess = false
                    };
                _isCancelRequested = false;
                _bpos1LibClass.Refund(Convert.ToUInt32(amount * 100.0), 0U, ingenico.merchantId, bsRRN);
                PaymentResultModel paymentResultModel = ingenico.WaitPosRespone();
                if (paymentResultModel.IsSuccess)
                {
                    _bpos1LibClass.Confirm();
                    ingenico.WaitResponse();
                    // ISSUE: reference to a compiler-generated method
                   // CommonUtils.ExecuteSync(new Action(ingenico.\u003CRefund\u003Eb__23_0));
                }
                StopBPOS();
                return paymentResultModel;
            }
            catch (Exception ex)
            {
                return new PaymentResultModel()
                {
                    IsSuccess = false
                };
            }
            finally
            {
                _bpos1LibClass = (BPOS1LibClass)null;
            }
        }

        public DeviceConnectionStatus Init()
        {
            try
            {
                Action<DeviceLog> onDeviceWarning1 = this.OnDeviceWarning;
                if (onDeviceWarning1 != null)
                {
                    PosDeviceLog posDeviceLog = new PosDeviceLog();
                    posDeviceLog.Category = TerminalLogCategory.All;
                    posDeviceLog.Message = "[Ingenico] - Start Initialization";
                    onDeviceWarning1((DeviceLog)posDeviceLog);
                }
                if (!this.StartBPOS())
                {
                    Action<DeviceLog> onDeviceWarning2 = this.OnDeviceWarning;
                    if (onDeviceWarning2 != null)
                    {
                        PosDeviceLog posDeviceLog = new PosDeviceLog();
                        posDeviceLog.Category = TerminalLogCategory.Critical;
                        posDeviceLog.Message = "Device not connected";
                        onDeviceWarning2((DeviceLog)posDeviceLog);
                    }
                    StopBPOS();
                    return DeviceConnectionStatus.NotConnected;
                }
                byte? lastErrorCode = _bpos1LibClass?.LastErrorCode;
                int? nullable = lastErrorCode.HasValue ? new int?((int)lastErrorCode.GetValueOrDefault()) : new int?();
                int num = 0;
                bool flag = nullable.GetValueOrDefault() == num & nullable.HasValue;
                StopBPOS();
                Action<DeviceLog> onDeviceWarning3 = this.OnDeviceWarning;
                if (onDeviceWarning3 != null)
                {
                    PosDeviceLog posDeviceLog = new PosDeviceLog();
                    posDeviceLog.Category = TerminalLogCategory.All;
                    posDeviceLog.Message = string.Format("[Ingenico] - Initilization result {0}", (object)flag);
                    onDeviceWarning3((DeviceLog)posDeviceLog);
                }
                return DeviceConnectionStatus.Enabled;
            }
            catch (Exception ex)
            {
                ILogger<Ingenico> logger = this._logger;
                if (logger != null)
                    LoggerExtensions.LogError((ILogger)logger, ex, ex.Message, Array.Empty<object>());
                Action<DeviceLog> onDeviceWarning = this.OnDeviceWarning;
                if (onDeviceWarning != null)
                {
                    PosDeviceLog posDeviceLog = new PosDeviceLog();
                    posDeviceLog.Category = TerminalLogCategory.Critical;
                    posDeviceLog.Message = "Device not connected";
                    onDeviceWarning((DeviceLog)posDeviceLog);
                }
                return DeviceConnectionStatus.NotConnected;
            }
        }

        public DeviceConnectionStatus GetDeviceStatusSync() => this.TestDeviceSync();

        public Task<DeviceConnectionStatus> GetDeviceStatus() => Task.Run<DeviceConnectionStatus>((Func<DeviceConnectionStatus>)(() => this.GetDeviceStatusSync()));

        public DeviceConnectionStatus TestDeviceSync()
        {
            DeviceConnectionStatus connectionStatus1 = DeviceConnectionStatus.NotConnected;
            if (!this.StartBPOS())
            {
                Action<DeviceLog> onDeviceWarning = this.OnDeviceWarning;
                if (onDeviceWarning != null)
                {
                    PosDeviceLog posDeviceLog = new PosDeviceLog();
                    posDeviceLog.Category = TerminalLogCategory.Critical;
                    posDeviceLog.Message = "Device not connected";
                    onDeviceWarning((DeviceLog)posDeviceLog);
                }
                return connectionStatus1;
            }
            _bpos1LibClass.Ping();
            ILogger<Ingenico> logger1 = this._logger;
            if (logger1 != null)
                LoggerExtensions.LogDebug((ILogger)logger1, "[Ingenico] Get Device Connection Status", Array.Empty<object>());
            DeviceConnectionStatus connectionStatus2;
            if (_bpos1LibClass.LastResult == (byte)2)
            {
                int lastResult = (int)_bpos1LibClass.LastResult;
                int lastErrorCode = (int)_bpos1LibClass.LastErrorCode;
                byte lastStatMsgCode = _bpos1LibClass.LastStatMsgCode;
                ILogger<Ingenico> logger2 = this._logger;
                if (logger2 != null)
                    LoggerExtensions.LogTrace((ILogger)logger2, string.Format("[Ingenico] LastStatMsgCode = {0}", (object)lastStatMsgCode), Array.Empty<object>());
                ILogger<Ingenico> logger3 = this._logger;
                if (logger3 != null)
                    LoggerExtensions.LogTrace((ILogger)logger3, "[Ingenico] Description = " + _bpos1LibClass.LastStatMsgDescription, Array.Empty<object>());
                ILogger<Ingenico> logger4 = this._logger;
                if (logger4 != null)
                    LoggerExtensions.LogTrace((ILogger)logger4, "[Ingenico] LastErrorDescription = " + _bpos1LibClass.LastErrorDescription, Array.Empty<object>());
                connectionStatus2 = DeviceConnectionStatus.Enabled;
            }
            else
            {
                Action<DeviceLog> onDeviceWarning = this.OnDeviceWarning;
                if (onDeviceWarning != null)
                {
                    PosDeviceLog posDeviceLog = new PosDeviceLog();
                    posDeviceLog.Category = TerminalLogCategory.Critical;
                    posDeviceLog.Message = string.Format("Test failure with code {0}", (object)_bpos1LibClass.LastResult);
                    onDeviceWarning((DeviceLog)posDeviceLog);
                }
                connectionStatus2 = DeviceConnectionStatus.InitializationError;
            }
            StopBPOS();
            return connectionStatus2;
        }

        public Task<DeviceConnectionStatus> TestDevice() => Task.Run<DeviceConnectionStatus>((Func<DeviceConnectionStatus>)(() => this.TestDeviceSync()));

        public string GetInfoSync()
        {
            try
            {
                if (!this.StartBPOS())
                    return string.Empty;
                _bpos1LibClass.POSGetInfo();
                Thread.Sleep(1000);
                string terminalInfo = _bpos1LibClass.TerminalInfo;
                StopBPOS();
                string str = "Model: Ingenico\n" + string.Format("COM port: COM{0}\n", (object)this.port) + string.Format("Baud rate: {0}\n", (object)this.boundRate);
                if (!string.IsNullOrEmpty(terminalInfo))
                    str = str + "Software version: " + new string(terminalInfo.TakeWhile<char>((Func<char, bool>)(x => x != ' ')).ToArray<char>()) + "\nTerminal profile ID: " + new string(terminalInfo.Substring(terminalInfo.IndexOf(" ", StringComparison.Ordinal)).Take<char>(8).ToArray<char>());
                return str;
            }
            catch (Exception ex)
            {
                LoggerExtensions.LogError((ILogger)this._logger, ex, ex.Message, Array.Empty<object>());
                Action<DeviceLog> onDeviceWarning = this.OnDeviceWarning;
                if (onDeviceWarning != null)
                {
                    PosDeviceLog posDeviceLog = new PosDeviceLog();
                    posDeviceLog.Category = TerminalLogCategory.Warning;
                    posDeviceLog.Message = "Cannot get information during error - " + ex.Message;
                    onDeviceWarning((DeviceLog)posDeviceLog);
                }
                return (string)null;
            }
        }

        public Task<string> GetInfo() => Task.Run<string>((Func<string>)(() => this.GetInfoSync()));

        public PaymentResultModel WaitPosRespone()
        {
            ILogger<Ingenico> logger1 = this._logger;
            if (logger1 != null)
                LoggerExtensions.LogDebug((ILogger)logger1, "[Ingenico] WaitPosRespone 1", Array.Empty<object>());
            int num = 0;
            while (_bpos1LibClass.LastResult == (byte)2 && num < 120)
            {
                ILogger<Ingenico> logger2 = this._logger;
                if (logger2 != null)
                    LoggerExtensions.LogDebug((ILogger)logger2, "[Ingenico] WaitPosRespone *", Array.Empty<object>());
                if (_isCancelRequested)
                {
                    _isCancelRequested = false;
                    _bpos1LibClass.Cancel();
                    Thread.Sleep(1000);
                    Action<IPosStatus> onStatus = this.OnStatus;
                    if (onStatus != null)
                        onStatus((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.TransactionCanceledByUser
                        });
                    return new PaymentResultModel()
                    {
                        IsSuccess = false
                    };
                }
                Thread.Sleep(1000);
                ++num;
                ILogger<Ingenico> logger3 = this._logger;
                if (logger3 != null)
                    LoggerExtensions.LogDebug((ILogger)logger3, string.Format("[Ingenico] LastStatMsgCode = {0}\n", (object)_bpos1LibClass.LastStatMsgCode) + "[Ingenico] Description = " + this.GetString(_bpos1LibClass.LastStatMsgDescription) + "\n[Ingenico] LastErrorDescription = " + this.GetString(_bpos1LibClass.LastErrorDescription) + "\n" + string.Format("[Ingenico] LastResult = {0}\n", (object)_bpos1LibClass.LastResult) + string.Format("[Ingenico] LastErrorCode = {0}", (object)_bpos1LibClass.LastErrorCode), Array.Empty<object>());
                if (_bpos1LibClass == null)
                {
                    Action<IPosStatus> onStatus = this.OnStatus;
                    if (onStatus != null)
                        onStatus((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.TransactionCanceledByUser
                        });
                    return new PaymentResultModel()
                    {
                        IsSuccess = false
                    };
                }
                this.InvokeLastStatusMsg(_bpos1LibClass.LastStatMsgCode);
            }
            ILogger<Ingenico> logger4 = this._logger;
            if (logger4 != null)
                LoggerExtensions.LogDebug((ILogger)logger4, "[Ingenico] WaitPosRespone 2", Array.Empty<object>());
            ILogger<Ingenico> logger5 = this._logger;
            if (logger5 != null)
                LoggerExtensions.LogDebug((ILogger)logger5, string.Format("[Ingenico] LastStatMsgCode = {0}\n", (object)_bpos1LibClass.LastStatMsgCode) + "[Ingenico] Description = " + this.GetString(_bpos1LibClass.LastStatMsgDescription) + "\n[Ingenico] LastErrorDescription = " + this.GetString(_bpos1LibClass.LastErrorDescription) + "\n" + string.Format("[Ingenico] LastResult = {0}\n", (object)_bpos1LibClass.LastResult) + string.Format("[Ingenico] LastErrorCode = {0}", (object)_bpos1LibClass.LastErrorCode), Array.Empty<object>());
            if (_bpos1LibClass.LastResult == (byte)0 && _bpos1LibClass.LastErrorCode == (byte)0)
            {
                ILogger<Ingenico> logger6 = this._logger;
                if (logger6 != null)
                    LoggerExtensions.LogDebug((ILogger)logger6, "[Ingenico] WaitPosRespone 3", Array.Empty<object>());
                Action<IPosStatus> onStatus = this.OnStatus;
                if (onStatus != null)
                    onStatus((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.SuccessfullyFulfilled
                    });
                return new PaymentResultModel()
                {
                    IsSuccess = true,
                    TransactionId = string.Format("{0}", (object)_bpos1LibClass.TrnBatchNum),
                    CardPan = _bpos1LibClass.PAN,
                    OperationDateTime = string.IsNullOrWhiteSpace(_bpos1LibClass.DateTime) ? DateTime.Now : DateTime.ParseExact(_bpos1LibClass.DateTime, "yyMMddHHmmss", (IFormatProvider)null),
                    TransactionCode = _bpos1LibClass.RRN,
                    TransactionStatus = this.GetString(_bpos1LibClass.LastErrorDescription),
                    AuthCode = this.GetString(_bpos1LibClass.AuthCode),
                    TerminalId = this.GetString(_bpos1LibClass.TerminalID),
                    PosPaid = Math.Round(Convert.ToDecimal(_bpos1LibClass.Amount) / 100M, 2, MidpointRounding.AwayFromZero),
                    PosAddAmount = Math.Round(Convert.ToDecimal(_bpos1LibClass.AddAmount) / 100M, 2, MidpointRounding.AwayFromZero),
                    InvoiceNumber = (long)_bpos1LibClass.InvoiceNum,
                    CardHolder = this.GetString(_bpos1LibClass.CardHolder),
                    IssuerName = this.GetString(_bpos1LibClass.IssuerName),
                    Bank = this.GetString(_bpos1LibClass.ECRDataTM)
                };
            }
            ILogger<Ingenico> logger7 = this._logger;
            if (logger7 != null)
                LoggerExtensions.LogDebug((ILogger)logger7, "[Ingenico] WaitPosRespone 4", Array.Empty<object>());
            if (_bpos1LibClass.LastResult == (byte)1)
            {
                ILogger<Ingenico> logger8 = this._logger;
                if (logger8 != null)
                    LoggerExtensions.LogDebug((ILogger)logger8, "[Ingenico] WaitPosRespone 5", Array.Empty<object>());
                switch (_bpos1LibClass.LastErrorCode)
                {
                    case 1:
                        Action<IPosStatus> onStatus1 = this.OnStatus;
                        if (onStatus1 != null)
                            onStatus1((IPosStatus)new PosStatus()
                            {
                                Status = ePosStatus.ErrorOpeningCOMPort
                            });
                        Action<DeviceLog> onDeviceWarning1 = this.OnDeviceWarning;
                        if (onDeviceWarning1 != null)
                        {
                            PosDeviceLog posDeviceLog = new PosDeviceLog();
                            posDeviceLog.Category = TerminalLogCategory.Warning;
                            posDeviceLog.Message = "[Ingenico] Error open connection";
                            onDeviceWarning1((DeviceLog)posDeviceLog);
                            break;
                        }
                        break;
                    case 2:
                        Action<IPosStatus> onStatus2 = this.OnStatus;
                        if (onStatus2 != null)
                            onStatus2((IPosStatus)new PosStatus()
                            {
                                Status = ePosStatus.NeedToOpenCOMPort
                            });
                        Action<DeviceLog> onDeviceWarning2 = this.OnDeviceWarning;
                        if (onDeviceWarning2 != null)
                        {
                            PosDeviceLog posDeviceLog = new PosDeviceLog();
                            posDeviceLog.Category = TerminalLogCategory.Warning;
                            posDeviceLog.Message = "[Ingenico] Error open connection";
                            onDeviceWarning2((DeviceLog)posDeviceLog);
                            break;
                        }
                        break;
                    case 3:
                        Action<IPosStatus> onStatus3 = this.OnStatus;
                        if (onStatus3 != null)
                            onStatus3((IPosStatus)new PosStatus()
                            {
                                Status = ePosStatus.ErrorConnectingWithTerminal
                            });
                        Action<DeviceLog> onDeviceWarning3 = this.OnDeviceWarning;
                        if (onDeviceWarning3 != null)
                        {
                            PosDeviceLog posDeviceLog = new PosDeviceLog();
                            posDeviceLog.Category = TerminalLogCategory.Warning;
                            posDeviceLog.Message = "[Ingenico] Error open connection";
                            onDeviceWarning3((DeviceLog)posDeviceLog);
                            break;
                        }
                        break;
                    case 4:
                        this.InvokeResponseCode(_bpos1LibClass);
                        break;
                }
            }
            ILogger<Ingenico> logger9 = this._logger;
            if (logger9 != null)
                LoggerExtensions.LogDebug((ILogger)logger9, "[Ingenico] WaitPosRespone 6", Array.Empty<object>());
            return new PaymentResultModel() { IsSuccess = false };
        }

        private void InvokeResponseCode(BPOS1LibClass bpos1LibClass)
        {
            switch (bpos1LibClass.ResponseCode)
            {
                case 0:
                    Action<IPosStatus> onStatus1 = this.OnStatus;
                    if (onStatus1 == null)
                        break;
                    onStatus1((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.ApprovedAndCompleted
                    });
                    break;
                case 1:
                    Action<IPosStatus> onStatus2 = this.OnStatus;
                    if (onStatus2 == null)
                        break;
                    onStatus2((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.AuthorizationDenied
                    });
                    break;
                case 2:
                    Action<IPosStatus> onStatus3 = this.OnStatus;
                    if (onStatus3 == null)
                        break;
                    onStatus3((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.AuthorizationDenied
                    });
                    break;
                case 3:
                    Action<IPosStatus> onStatus4 = this.OnStatus;
                    if (onStatus4 == null)
                        break;
                    onStatus4((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.UnregisteredTradingPoint
                    });
                    break;
                case 4:
                    Action<IPosStatus> onStatus5 = this.OnStatus;
                    if (onStatus5 == null)
                        break;
                    onStatus5((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.AuthorizationRejectedWithdrawTheCardAtTheBanksRequest
                    });
                    break;
                case 5:
                    Action<IPosStatus> onStatus6 = this.OnStatus;
                    if (onStatus6 == null)
                        break;
                    onStatus6((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.AuthorizationRejectedNoPayment
                    });
                    break;
                case 6:
                    Action<IPosStatus> onStatus7 = this.OnStatus;
                    if (onStatus7 != null)
                        onStatus7((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.CommonErrorNeedToRepeat
                        });
                    Action<DeviceLog> onDeviceWarning1 = this.OnDeviceWarning;
                    if (onDeviceWarning1 == null)
                        break;
                    PosDeviceLog posDeviceLog1 = new PosDeviceLog();
                    posDeviceLog1.Category = TerminalLogCategory.Warning;
                    posDeviceLog1.Message = "[Ingenico] Common error need repeat";
                    onDeviceWarning1((DeviceLog)posDeviceLog1);
                    break;
                case 7:
                    Action<IPosStatus> onStatus8 = this.OnStatus;
                    if (onStatus8 == null)
                        break;
                    onStatus8((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.AuthorizationRejectedWithdrawTheCardAtTheBanksRequest
                    });
                    break;
                case 12:
                    Action<IPosStatus> onStatus9 = this.OnStatus;
                    if (onStatus9 != null)
                        onStatus9((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.InvalidTransactionNetworkErrorNeedToRepeat
                        });
                    Action<DeviceLog> onDeviceWarning2 = this.OnDeviceWarning;
                    if (onDeviceWarning2 == null)
                        break;
                    PosDeviceLog posDeviceLog2 = new PosDeviceLog();
                    posDeviceLog2.Category = TerminalLogCategory.Warning;
                    posDeviceLog2.Message = "[Ingenico] Common error need repeat";
                    onDeviceWarning2((DeviceLog)posDeviceLog2);
                    break;
                case 13:
                    Action<IPosStatus> onStatus10 = this.OnStatus;
                    if (onStatus10 == null)
                        break;
                    onStatus10((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.IncorrectAmountEntered
                    });
                    break;
                case 14:
                    Action<IPosStatus> onStatus11 = this.OnStatus;
                    if (onStatus11 == null)
                        break;
                    onStatus11((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.InvalidCardNumber
                    });
                    break;
                case 15:
                    Action<IPosStatus> onStatus12 = this.OnStatus;
                    if (onStatus12 == null)
                        break;
                    onStatus12((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.BankNodeIsNotFoundOnTheNetwork
                    });
                    break;
                case 17:
                    Action<IPosStatus> onStatus13 = this.OnStatus;
                    if (onStatus13 == null)
                        break;
                    onStatus13((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.CanceledByTheClient
                    });
                    break;
                case 21:
                    Action<IPosStatus> onStatus14 = this.OnStatus;
                    if (onStatus14 == null)
                        break;
                    onStatus14((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.ActionsNotCompletedDidNotMatchData
                    });
                    break;
                case 28:
                    Action<IPosStatus> onStatus15 = this.OnStatus;
                    if (onStatus15 != null)
                        onStatus15((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.NoResponseFileIsTemporarilyUnavailable
                        });
                    Action<DeviceLog> onDeviceWarning3 = this.OnDeviceWarning;
                    if (onDeviceWarning3 == null)
                        break;
                    PosDeviceLog posDeviceLog3 = new PosDeviceLog();
                    posDeviceLog3.Category = TerminalLogCategory.Warning;
                    posDeviceLog3.Message = "[Ingenico] Common error need to repeat";
                    onDeviceWarning3((DeviceLog)posDeviceLog3);
                    break;
                case 30:
                    Action<IPosStatus> onStatus16 = this.OnStatus;
                    if (onStatus16 != null)
                        onStatus16((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.WrongFormatNeedToRepeat
                        });
                    Action<DeviceLog> onDeviceWarning4 = this.OnDeviceWarning;
                    if (onDeviceWarning4 == null)
                        break;
                    PosDeviceLog posDeviceLog4 = new PosDeviceLog();
                    posDeviceLog4.Category = TerminalLogCategory.Warning;
                    posDeviceLog4.Message = "[Ingenico] Worng format need to repeat";
                    onDeviceWarning4((DeviceLog)posDeviceLog4);
                    break;
                case 31:
                    Action<IPosStatus> onStatus17 = this.OnStatus;
                    if (onStatus17 == null)
                        break;
                    onStatus17((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.TheIssuerIsNotFoundInThePaymentSystem
                    });
                    break;
                case 32:
                    Action<IPosStatus> onStatus18 = this.OnStatus;
                    if (onStatus18 == null)
                        break;
                    onStatus18((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.PartiallyCompleted
                    });
                    break;
                case 33:
                    Action<IPosStatus> onStatus19 = this.OnStatus;
                    if (onStatus19 == null)
                        break;
                    onStatus19((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.TheValidityPeriodOfTheCardHasExpiredTheCardHasBeenWithdrawnAtTheBanksRequest
                    });
                    break;
                case 36:
                    Action<IPosStatus> onStatus20 = this.OnStatus;
                    if (onStatus20 == null)
                        break;
                    onStatus20((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.ForbiddenCardRemove
                    });
                    break;
                case 37:
                    Action<IPosStatus> onStatus21 = this.OnStatus;
                    if (onStatus21 == null)
                        break;
                    onStatus21((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.WithdrawnByTheIssuerRemovedFromTheCardAndContactedByTheAcquirer
                    });
                    break;
                case 38:
                    Action<IPosStatus> onStatus22 = this.OnStatus;
                    if (onStatus22 == null)
                        break;
                    onStatus22((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.ThereAreNoAttemptsToEnterThePINRemoveTheCard
                    });
                    break;
                case 39:
                    Action<IPosStatus> onStatus23 = this.OnStatus;
                    if (onStatus23 == null)
                        break;
                    onStatus23((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.NoClientsCreditAccount
                    });
                    break;
                case 41:
                    Action<IPosStatus> onStatus24 = this.OnStatus;
                    if (onStatus24 == null)
                        break;
                    onStatus24((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.CardIslostRemoved
                    });
                    break;
                case 43:
                    Action<IPosStatus> onStatus25 = this.OnStatus;
                    if (onStatus25 == null)
                        break;
                    onStatus25((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.CardIsStolenRemoved
                    });
                    break;
                case 51:
                    Action<IPosStatus> onStatus26 = this.OnStatus;
                    if (onStatus26 == null)
                        break;
                    onStatus26((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.NotEnoughMoney
                    });
                    break;
                case 52:
                    Action<IPosStatus> onStatus27 = this.OnStatus;
                    if (onStatus27 == null)
                        break;
                    onStatus27((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.NoSettlementSpecifiedClienAccount
                    });
                    break;
                case 53:
                    Action<IPosStatus> onStatus28 = this.OnStatus;
                    if (onStatus28 == null)
                        break;
                    onStatus28((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.ThereIsNoCumulativeAccountOfTheClient
                    });
                    break;
                case 54:
                    Action<IPosStatus> onStatus29 = this.OnStatus;
                    if (onStatus29 == null)
                        break;
                    onStatus29((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.TheExpirationDateOfTheCardExpires
                    });
                    break;
                case 55:
                    Action<IPosStatus> onStatus30 = this.OnStatus;
                    if (onStatus30 == null)
                        break;
                    onStatus30((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.WrongPIN
                    });
                    break;
                case 57:
                    Action<IPosStatus> onStatus31 = this.OnStatus;
                    if (onStatus31 == null)
                        break;
                    onStatus31((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.ThisTransactionTypeIsNotProvidedForTheGivenCard
                    });
                    break;
                case 58:
                    Action<IPosStatus> onStatus32 = this.OnStatus;
                    if (onStatus32 == null)
                        break;
                    onStatus32((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.ThisTypeOfTransactionIsNotProvidedForPOSTerminal
                    });
                    break;
                case 61:
                    Action<IPosStatus> onStatus33 = this.OnStatus;
                    if (onStatus33 == null)
                        break;
                    onStatus33((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.TheAamountOfAuthorizationExceededTheExpenseLimitOnTheCard
                    });
                    break;
                case 62:
                    Action<IPosStatus> onStatus34 = this.OnStatus;
                    if (onStatus34 == null)
                        break;
                    onStatus34((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.IncorrectServiceCodeForbiddenCardCanNotBeSeized
                    });
                    break;
                case 64:
                    Action<IPosStatus> onStatus35 = this.OnStatus;
                    if (onStatus35 == null)
                        break;
                    onStatus35((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.TheAmountOfTheCancellationAuthorizationIsDifferentFromTheAmountOfTheOriginalAuthorization
                    });
                    break;
                case 65:
                    Action<IPosStatus> onStatus36 = this.OnStatus;
                    if (onStatus36 == null)
                        break;
                    onStatus36((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.TheExpenseLimitExpiredOnTheAccount
                    });
                    break;
                case 66:
                    Action<IPosStatus> onStatus37 = this.OnStatus;
                    if (onStatus37 == null)
                        break;
                    onStatus37((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.TheCardIsVoidCanNotBeSeized
                    });
                    break;
                case 67:
                    Action<IPosStatus> onStatus38 = this.OnStatus;
                    if (onStatus38 == null)
                        break;
                    onStatus38((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.CardIsWithdrawnFromATM
                    });
                    break;
                case 68:
                    Action<IPosStatus> onStatus39 = this.OnStatus;
                    if (onStatus39 == null)
                        break;
                    onStatus39((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.ItIsTooLateToReceiveAnAnswerFromTheNetworkItIsNecessaryToRepeat
                    });
                    break;
                case 75:
                    Action<IPosStatus> onStatus40 = this.OnStatus;
                    if (onStatus40 == null)
                        break;
                    onStatus40((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.TheNumberOfIncorrectlyEnteredPINsExceededTheAmountDischarged
                    });
                    break;
                case 76:
                    Action<IPosStatus> onStatus41 = this.OnStatus;
                    if (onStatus41 == null)
                        break;
                    onStatus41((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.TheNumberOfIncorrectlyEnteredPINsExceededTheAmountDischarged
                    });
                    break;
                case 77:
                    Action<IPosStatus> onStatus42 = this.OnStatus;
                    if (onStatus42 == null)
                        break;
                    onStatus42((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.ActionsAreNotCompletedIncompleteDataItIsNecessaryToRollbackOrRepeat
                    });
                    break;
                case 78:
                    Action<IPosStatus> onStatus43 = this.OnStatus;
                    if (onStatus43 == null)
                        break;
                    onStatus43((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.NoAccount
                    });
                    break;
                case 79:
                    Action<IPosStatus> onStatus44 = this.OnStatus;
                    if (onStatus44 == null)
                        break;
                    onStatus44((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.AlreadyCanceledWhenTurnedOn
                    });
                    break;
                case 80:
                    Action<IPosStatus> onStatus45 = this.OnStatus;
                    if (onStatus45 != null)
                        onStatus45((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.GeneralNetworkErrorIncorrectData
                        });
                    Action<DeviceLog> onDeviceWarning5 = this.OnDeviceWarning;
                    if (onDeviceWarning5 == null)
                        break;
                    PosDeviceLog posDeviceLog5 = new PosDeviceLog();
                    posDeviceLog5.Category = TerminalLogCategory.Warning;
                    posDeviceLog5.Message = "[Ingenico] General network error";
                    onDeviceWarning5((DeviceLog)posDeviceLog5);
                    break;
                case 81:
                    Action<IPosStatus> onStatus46 = this.OnStatus;
                    if (onStatus46 == null)
                        break;
                    onStatus46((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.RemoteNetworkErrorOrPINEncryption
                    });
                    break;
                case 82:
                    Action<IPosStatus> onStatus47 = this.OnStatus;
                    if (onStatus47 == null)
                        break;
                    onStatus47((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.TimeoutWhenConnectedWithTheIssuersNodeOrWrongCVVOrCacheIsNotApprovedTheCashbackSumLimitIsExceeded
                    });
                    break;
                case 83:
                    Action<IPosStatus> onStatus48 = this.OnStatus;
                    if (onStatus48 != null)
                        onStatus48((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.ThePINVerificationTransactionIsUnsuccessfulNetworkError
                        });
                    Action<DeviceLog> onDeviceWarning6 = this.OnDeviceWarning;
                    if (onDeviceWarning6 == null)
                        break;
                    PosDeviceLog posDeviceLog6 = new PosDeviceLog();
                    posDeviceLog6.Category = TerminalLogCategory.Warning;
                    posDeviceLog6.Message = "[Ingenico] General network error";
                    onDeviceWarning6((DeviceLog)posDeviceLog6);
                    break;
                case 85:
                    Action<IPosStatus> onStatus49 = this.OnStatus;
                    if (onStatus49 == null)
                        break;
                    onStatus49((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.TheCardIsInOrderThereIsNoReasonToRefuse
                    });
                    break;
                case 86:
                    Action<IPosStatus> onStatus50 = this.OnStatus;
                    if (onStatus50 != null)
                        onStatus50((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.PINCanNotBeCheckedNetworkError
                        });
                    Action<DeviceLog> onDeviceWarning7 = this.OnDeviceWarning;
                    if (onDeviceWarning7 == null)
                        break;
                    PosDeviceLog posDeviceLog7 = new PosDeviceLog();
                    posDeviceLog7.Category = TerminalLogCategory.Warning;
                    posDeviceLog7.Message = "[Ingenico] General network error";
                    onDeviceWarning7((DeviceLog)posDeviceLog7);
                    break;
                case 88:
                    Action<IPosStatus> onStatus51 = this.OnStatus;
                    if (onStatus51 != null)
                        onStatus51((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.PINEncryptionErrorNetworkError
                        });
                    Action<DeviceLog> onDeviceWarning8 = this.OnDeviceWarning;
                    if (onDeviceWarning8 == null)
                        break;
                    PosDeviceLog posDeviceLog8 = new PosDeviceLog();
                    posDeviceLog8.Category = TerminalLogCategory.Warning;
                    posDeviceLog8.Message = "[Ingenico] General network error";
                    onDeviceWarning8((DeviceLog)posDeviceLog8);
                    break;
                case 89:
                    Action<IPosStatus> onStatus52 = this.OnStatus;
                    if (onStatus52 != null)
                        onStatus52((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.IdentificationErrorIsANetworkError
                        });
                    Action<DeviceLog> onDeviceWarning9 = this.OnDeviceWarning;
                    if (onDeviceWarning9 == null)
                        break;
                    PosDeviceLog posDeviceLog9 = new PosDeviceLog();
                    posDeviceLog9.Category = TerminalLogCategory.Warning;
                    posDeviceLog9.Message = "[Ingenico] General network error";
                    onDeviceWarning9((DeviceLog)posDeviceLog9);
                    break;
                case 91:
                    Action<IPosStatus> onStatus53 = this.OnStatus;
                    if (onStatus53 != null)
                        onStatus53((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.NoConnectionWithTheBankByTheIssuerNetworkError
                        });
                    Action<DeviceLog> onDeviceWarning10 = this.OnDeviceWarning;
                    if (onDeviceWarning10 == null)
                        break;
                    PosDeviceLog posDeviceLog10 = new PosDeviceLog();
                    posDeviceLog10.Category = TerminalLogCategory.Warning;
                    posDeviceLog10.Message = "[Ingenico] General network error";
                    onDeviceWarning10((DeviceLog)posDeviceLog10);
                    break;
                case 92:
                    Action<IPosStatus> onStatus54 = this.OnStatus;
                    if (onStatus54 != null)
                        onStatus54((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.UnsuccessfulRequestRoutingIsNotPossibleNetworkError
                        });
                    Action<DeviceLog> onDeviceWarning11 = this.OnDeviceWarning;
                    if (onDeviceWarning11 == null)
                        break;
                    PosDeviceLog posDeviceLog11 = new PosDeviceLog();
                    posDeviceLog11.Category = TerminalLogCategory.Warning;
                    posDeviceLog11.Message = "[Ingenico] General network error";
                    onDeviceWarning11((DeviceLog)posDeviceLog11);
                    break;
                case 93:
                    Action<IPosStatus> onStatus55 = this.OnStatus;
                    if (onStatus55 == null)
                        break;
                    onStatus55((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.TheTransactionCanNotBeCompletedTheIssuerDeclineAuthorizationDueToAViolationOfTheRules
                    });
                    break;
                case 94:
                    Action<IPosStatus> onStatus56 = this.OnStatus;
                    if (onStatus56 != null)
                        onStatus56((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.DuplicationOfTransmissionNetworkError
                        });
                    Action<DeviceLog> onDeviceWarning12 = this.OnDeviceWarning;
                    if (onDeviceWarning12 == null)
                        break;
                    PosDeviceLog posDeviceLog12 = new PosDeviceLog();
                    posDeviceLog12.Category = TerminalLogCategory.Warning;
                    posDeviceLog12.Message = "[Ingenico] General network error";
                    onDeviceWarning12((DeviceLog)posDeviceLog12);
                    break;
                case 96:
                    Action<IPosStatus> onStatus57 = this.OnStatus;
                    if (onStatus57 == null)
                        break;
                    onStatus57((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.GeneralSystemMalfunction
                    });
                    break;
                case 119:
                    Action<IPosStatus> onStatus58 = this.OnStatus;
                    if (onStatus58 == null)
                        break;
                    onStatus58((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.UnableToSendEncryptedMessage
                    });
                    break;
                case 1000:
                    Action<IPosStatus> onStatus59 = this.OnStatus;
                    if (onStatus59 != null)
                        onStatus59((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.GeneralError
                        });
                    Action<DeviceLog> onDeviceWarning13 = this.OnDeviceWarning;
                    if (onDeviceWarning13 == null)
                        break;
                    PosDeviceLog posDeviceLog13 = new PosDeviceLog();
                    posDeviceLog13.Category = TerminalLogCategory.Warning;
                    posDeviceLog13.Message = "[Ingenico] General error";
                    onDeviceWarning13((DeviceLog)posDeviceLog13);
                    break;
                case 1001:
                    Action<IPosStatus> onStatus60 = this.OnStatus;
                    if (onStatus60 == null)
                        break;
                    onStatus60((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.TransactionCanceledByUser
                    });
                    break;
                case 1002:
                    Action<IPosStatus> onStatus61 = this.OnStatus;
                    if (onStatus61 == null)
                        break;
                    onStatus61((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.EMVDecline
                    });
                    break;
                case 1003:
                    Action<IPosStatus> onStatus62 = this.OnStatus;
                    if (onStatus62 == null)
                        break;
                    onStatus62((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.TransactionLogIsFullNeedCloseBatch
                    });
                    break;
                case 1004:
                    Action<IPosStatus> onStatus63 = this.OnStatus;
                    if (onStatus63 != null)
                        onStatus63((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.NoConnectionWithHost
                        });
                    Action<DeviceLog> onDeviceWarning14 = this.OnDeviceWarning;
                    if (onDeviceWarning14 == null)
                        break;
                    PosDeviceLog posDeviceLog14 = new PosDeviceLog();
                    posDeviceLog14.Category = TerminalLogCategory.Warning;
                    posDeviceLog14.Message = "[Ingenico] General network error";
                    onDeviceWarning14((DeviceLog)posDeviceLog14);
                    break;
                case 1005:
                    Action<IPosStatus> onStatus64 = this.OnStatus;
                    if (onStatus64 != null)
                        onStatus64((IPosStatus)new PosStatus()
                        {
                            Status = ePosStatus.NoPaperInPrinter
                        });
                    Action<DeviceLog> onDeviceWarning15 = this.OnDeviceWarning;
                    if (onDeviceWarning15 == null)
                        break;
                    PosDeviceLog posDeviceLog15 = new PosDeviceLog();
                    posDeviceLog15.Category = TerminalLogCategory.Warning;
                    posDeviceLog15.Message = "[Ingenico] Paper was ended";
                    onDeviceWarning15((DeviceLog)posDeviceLog15);
                    break;
                case 1006:
                    Action<IPosStatus> onStatus65 = this.OnStatus;
                    if (onStatus65 == null)
                        break;
                    onStatus65((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.ErrorCryptoKeys
                    });
                    break;
                case 1007:
                    Action<IPosStatus> onStatus66 = this.OnStatus;
                    if (onStatus66 == null)
                        break;
                    onStatus66((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.CardReaderIsNotConnected
                    });
                    break;
                case 1008:
                    Action<IPosStatus> onStatus67 = this.OnStatus;
                    if (onStatus67 == null)
                        break;
                    onStatus67((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.TransactionIsAlreadyComplete
                    });
                    break;
            }
        }

        private void InvokeLastStatusMsg(byte LastStatMsgCode)
        {
            switch (LastStatMsgCode)
            {
                case 1:
                    Action<IPosStatus> onStatus1 = this.OnStatus;
                    if (onStatus1 == null)
                        break;
                    onStatus1((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.CardWasRead
                    });
                    break;
                case 2:
                    Action<IPosStatus> onStatus2 = this.OnStatus;
                    if (onStatus2 == null)
                        break;
                    onStatus2((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.UsedAChipCard
                    });
                    break;
                case 3:
                    Action<IPosStatus> onStatus3 = this.OnStatus;
                    if (onStatus3 == null)
                        break;
                    onStatus3((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.AuthorizationInProgress
                    });
                    break;
                case 4:
                    Action<IPosStatus> onStatus4 = this.OnStatus;
                    if (onStatus4 == null)
                        break;
                    onStatus4((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.WaitingForCashierAction
                    });
                    break;
                case 5:
                    Action<IPosStatus> onStatus5 = this.OnStatus;
                    if (onStatus5 == null)
                        break;
                    onStatus5((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.PrintingReceipt
                    });
                    break;
                case 6:
                    Action<IPosStatus> onStatus6 = this.OnStatus;
                    if (onStatus6 == null)
                        break;
                    onStatus6((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.PinEntryIsNeeded
                    });
                    break;
                case 7:
                    Action<IPosStatus> onStatus7 = this.OnStatus;
                    if (onStatus7 == null)
                        break;
                    onStatus7((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.CardWasRemoved
                    });
                    break;
                case 8:
                    Action<IPosStatus> onStatus8 = this.OnStatus;
                    if (onStatus8 == null)
                        break;
                    onStatus8((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.EMVMultiAids
                    });
                    break;
                case 9:
                    Action<IPosStatus> onStatus9 = this.OnStatus;
                    if (onStatus9 == null)
                        break;
                    onStatus9((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.WaitingForCard
                    });
                    break;
                case 10:
                    Action<IPosStatus> onStatus10 = this.OnStatus;
                    if (onStatus10 == null)
                        break;
                    onStatus10((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.InProgress
                    });
                    break;
                case 11:
                    Action<IPosStatus> onStatus11 = this.OnStatus;
                    if (onStatus11 == null)
                        break;
                    onStatus11((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.CorrectTransaction
                    });
                    break;
                case 12:
                    Action<IPosStatus> onStatus12 = this.OnStatus;
                    if (onStatus12 == null)
                        break;
                    onStatus12((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.PinInputWaitKey
                    });
                    break;
                case 13:
                    Action<IPosStatus> onStatus13 = this.OnStatus;
                    if (onStatus13 == null)
                        break;
                    onStatus13((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.PinInputBackspacePressed
                    });
                    break;
                case 14:
                    Action<IPosStatus> onStatus14 = this.OnStatus;
                    if (onStatus14 == null)
                        break;
                    onStatus14((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.PinInputKeyPressed
                    });
                    break;
            }
        }

        public void Settlement()
        {
            if (!this.StartBPOS())
                return;
            _bpos1LibClass.Settlement(this.merchantId);
            this.WaitResponse();
            StopBPOS();
        }

        public void PrintBatchTotals()
        {
            if (!this.StartBPOS())
                return;
            _bpos1LibClass.PrintBatchTotals(this.merchantId);
            this.WaitResponse();
            StopBPOS();
        }

        private void WaitResponse()
        {
            if (_bpos1LibClass == null)
                return;
            for (int index = 0; _bpos1LibClass.LastResult == (byte)2 && index < 120; ++index)
                Thread.Sleep(1000);
        }

        public async Task<BatchTotals> GetBatchTotals()
        {
            if (!this.StartBPOS())
                return (BatchTotals)null;
            _bpos1LibClass.GetBatchTotals(this.merchantId);
            this.WaitResponse();
            BatchTotals batchTotals = new BatchTotals();
            batchTotals.DebitCount = _bpos1LibClass.TotalsDebitNum;
            batchTotals.DebitSum = _bpos1LibClass.TotalsDebitAmt;
            batchTotals.CreditCount = _bpos1LibClass.TotalsCreditNum;
            batchTotals.CreditSum = _bpos1LibClass.TotalsCreditAmt;
            batchTotals.CencelledCount = _bpos1LibClass.TotalsCancelledNum;
            batchTotals.CencelledSum = _bpos1LibClass.TotalsCancelledAmt;
            StopBPOS();
            return batchTotals;
        }

        public void PrintBatchJournal()
        {
            if (!this.StartBPOS())
                return;
            _bpos1LibClass.PrintBatchJournal(this.merchantId);
            StopBPOS();
        }

        public void PrintLastSettleCopy()
        {
            if (!this.StartBPOS())
                return;
            _bpos1LibClass.PrintLastSettleCopy(this.merchantId);
            StopBPOS();
        }

        public Task<PaymentResultModel> Purchase(double amount)
        {
            try
            {
                if (!this.StartBPOS())
                    return Task.FromResult<PaymentResultModel>(new PaymentResultModel()
                    {
                        IsSuccess = false
                    });
                _isCancelRequested = false;
                _bpos1LibClass.Purchase(Convert.ToUInt32(amount * 100.0), 0U, this.merchantId);
                Action<IPosStatus> onStatus = this.OnStatus;
                if (onStatus != null)
                    onStatus((IPosStatus)new PosStatus()
                    {
                        Status = ePosStatus.WaitingForCard
                    });
                PaymentResultModel result = this.WaitPosRespone();
                ILogger<Ingenico> logger = this._logger;
                if (logger != null)
                    LoggerExtensions.LogDebug((ILogger)logger, "[Ingenico] Purches " + JsonConvert.SerializeObject((object)result), Array.Empty<object>());
                if (result.IsSuccess)
                {
                    _bpos1LibClass.Confirm();
                    this.WaitResponse();
                }
                Action<IPosResponse> onResponse = this.OnResponse;
                if (onResponse != null)
                    onResponse((IPosResponse)new PayPosResponse()
                    {
                        Response = result
                    });
                StopBPOS();
                return Task.FromResult<PaymentResultModel>(result);
            }
            catch (Exception ex)
            {
                LoggerExtensions.LogError((ILogger)this._logger, ex, "[Ingenico] " + ex.Message, Array.Empty<object>());
                return Task.FromResult<PaymentResultModel>(new PaymentResultModel()
                {
                    IsSuccess = false
                });
            }
            finally
            {
                _bpos1LibClass = (BPOS1LibClass)null;
            }
        }

        public void Cancel() => _isCancelRequested = true;

        public List<string> GetLastReceipt()
        {
            if (!this.StartBPOS())
                return (List<string>)null;
            _bpos1LibClass.ReqCurrReceipt();
            this.WaitResponse();
            string receipt = _bpos1LibClass.Receipt;
            StopBPOS();
            return this.ParseReceipt(receipt);
        }

        private string GetString(string input) => string.IsNullOrWhiteSpace(input) || !ConfigurationBinder.GetValue<bool>(this._configuration, "Devices:Ingenico:Use1252Encoder", false) ? input : Encoding.UTF8.GetString(Encoding.Convert(this._encoding1251, Encoding.UTF8, this._encoding1252.GetBytes(input)));

        private List<string> ParseReceipt(string res)
        {
            if (string.IsNullOrEmpty(res))
                return (List<string>)null;
            res = this.GetString(res);
            List<string> list = ((IEnumerable<string>)res.Replace("\t", string.Empty).Replace("\r", string.Empty).Split('\n')).ToList<string>();
            for (int index = 0; index < list.Count; ++index)
            {
                if (string.IsNullOrEmpty(list[index]))
                    list.RemoveAt(index);
            }
            list.RemoveAt(0);
            return list;
        }

        private bool StartBPOS()
        {
            this.port = Convert.ToByte(this._configuration["Devices:Ingenico:Port"]);
            this.boundRate = Convert.ToInt32(this._configuration["Devices:Ingenico:BaudRate"]);
            this.merchantId = Convert.ToByte(this._configuration["Devices:Ingenico:MerchanId"]);
            int num = 120;
            while (_bpos1LibClass != null)
            {
                Thread.Sleep(100);
                if (num == 0)
                    return false;
                --num;
            }
            _bpos1LibClass = new BPOS1LibClass();
            _bpos1LibClass.SetErrorLang((byte)1);
            _bpos1LibClass.useLogging((byte)1, "ingenico.log");
            _bpos1LibClass.CommOpen(this.port, this.boundRate);
            return true;
        }

        private static void StopBPOS()
        {
            _bpos1LibClass?.CommClose();
            Task.Delay(100);
            _bpos1LibClass = (BPOS1LibClass)null;
        }

        public void Dispose()
        {
            _bpos1LibClass?.CommClose();
            _bpos1LibClass = (BPOS1LibClass)null;
        }
    }
}

