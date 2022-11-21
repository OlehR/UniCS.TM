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
using Front.Equipments.Implementation;
using Front.Equipments.Virtual;
using Utils;
using Front.Equipments.Utils;

namespace Front.Equipments
{
    public class IngenicoH : BankTerminal
    {

        Ingenico.Ingenico EquipmentIngenico = null;
        //public IngenicoH(string pSerialPortName, int pBaudRate = 9600, Action<string, string> pLogger = null) : base(pSerialPortName, pBaudRate, pLogger) { }
        public IngenicoH(Equipment pEquipment, IConfiguration pConfiguration, ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) : base(pEquipment,pConfiguration, eModelEquipment.Ingenico, pLoggerFactory)
        {
            try
            {
                State = eStateEquipment.Init;
                //ILoggerFactory loggerFactory = new LoggerFactory().AddConsole((_, __) => true);
                ILogger<Ingenico.Ingenico> logger = LoggerFactory?.CreateLogger<Ingenico.Ingenico>();
                EquipmentIngenico = new Ingenico.Ingenico(pConfiguration, logger);

                EquipmentIngenico.OnStatus += pActionStatus;
                State = eStateEquipment.On;
            }
            catch(Exception e)
            {
                State = eStateEquipment.Error;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);                
            }
        }

        public override Payment Purchase(decimal pAmount, decimal pCash = 0)
        {           
            return PaymentResultModelToPayment(EquipmentIngenico.Purchase(Convert.ToDouble(pAmount), Convert.ToDouble(pCash)).Result);
        }

        public override Payment Refund(decimal pAmount, string pRRN)
        {
            return PaymentResultModelToPayment(EquipmentIngenico.Refund(Convert.ToDouble(pAmount), pRRN).Result);
        }

        public override BatchTotals PrintZ()
        {
            var r = EquipmentIngenico.GetXZ(false);
            return r.Result;
        }

        public override BatchTotals PrintX()
        {
            var r = EquipmentIngenico.GetXZ(true);
            return r.Result;
        }

        public override StatusEquipment TestDevice()
        {           
            var r = EquipmentIngenico.TestDeviceSync();
            State = r == Ingenico.DeviceConnectionStatus.Enabled ? eStateEquipment.On:eStateEquipment.Error;
            return new StatusEquipment(eModelEquipment.Ingenico, State,r.ToString());
        }

        public override string GetDeviceInfo()
        {
            return EquipmentIngenico.GetInfoSync();
        }

        public override void Cancel() 
        { 
            EquipmentIngenico.Cancel();
        }

        Payment PaymentResultModelToPayment(PaymentResultModel pRP, ModelMID.eTypePay pTypePay = ModelMID.eTypePay.Card)
        {
            return new Payment()
            {
                IsSuccess = pRP.IsSuccess,
                TypePay = pTypePay,
                SumPay = pRP.PosPaid,
                NumberReceipt = pRP.InvoiceNumber, //parRP.TransactionId,
                NumberCard = pRP.CardPan,
                CodeAuthorization = pRP.TransactionCode, //RRN
                NumberTerminal = pRP.TerminalId,
                NumberSlip = pRP.AuthCode, //код авторизації
                PosPaid = pRP.PosPaid,
                PosAddAmount = pRP.PosAddAmount,
                DateCreate = pRP.OperationDateTime,
                CardHolder = pRP.CardHolder,
                IssuerName = pRP.IssuerName,
                Bank = pRP.Bank,
                Receipt = EquipmentIngenico.GetLastReceipt()
            };
        }

        public override IEnumerable<string> GetLastReceipt() { return EquipmentIngenico.GetLastReceipt(); }
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

   
    public class BatchTotals
    {
        public uint DebitCount { get; set; }

        public uint DebitSum { get; set; }

        public uint CreditCount { get; set; }

        public uint CreditSum { get; set; }

        public uint CencelledCount { get; set; }

        public uint CencelledSum { get; set; }
        /// <summary>
        /// Чек построчно.
        /// </summary>
        public IEnumerable<string> Receipt { get; set; }
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

        Action<StatusEquipment> OnStatus { get; set; }

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
    
    public class Ingenico :  IBaseDevice, IDisposable //IPosTerminal,
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

        public Action<StatusEquipment> OnStatus { get; set; }

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
           
            if (_logger != null)
                LoggerExtensions.LogDebug((ILogger)_logger, "[Ingenico] Get Device Connection Status");
            DeviceConnectionStatus connectionStatus2;
            if (_bpos1LibClass.LastResult == (byte)2)
            {
                int lastResult = (int)_bpos1LibClass.LastResult;
                int lastErrorCode = (int)_bpos1LibClass.LastErrorCode;
                byte lastStatMsgCode = _bpos1LibClass.LastStatMsgCode;

                if (_logger != null)
                {
                    LoggerExtensions.LogTrace((ILogger)_logger, $"[Ingenico] LastStatMsgCode = {lastStatMsgCode}");
                    LoggerExtensions.LogTrace((ILogger)_logger, "[Ingenico] Description = " + _bpos1LibClass.LastStatMsgDescription);
                    LoggerExtensions.LogTrace((ILogger)_logger, "[Ingenico] LastErrorDescription = " + _bpos1LibClass.LastErrorDescription);
                }
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
                    PosDeviceLog posDeviceLog = new PosDeviceLog()
                    {
                        Category = TerminalLogCategory.Warning,
                        Message = "Cannot get information during error - " + ex.Message
                    };
                    onDeviceWarning((DeviceLog)posDeviceLog);
                }
                return (string)null;
            }
        }

        public Task<string> GetInfo() => Task.Run<string>((Func<string>)(() => this.GetInfoSync()));

        public PaymentResultModel WaitPosRespone()
        {            
            if (_logger != null)
                LoggerExtensions.LogDebug((ILogger)_logger, "[Ingenico] WaitPosRespone 1");
            int num = 0;
            while (_bpos1LibClass.LastResult == (byte)2 && num < 120)
            {             
                LoggerExtensions.LogDebug((ILogger)_logger, "[Ingenico] WaitPosRespone *");
                if (_isCancelRequested)
                {
                    _isCancelRequested = false;
                    _bpos1LibClass.Cancel();
                    Thread.Sleep(1000);
                    Action<StatusEquipment> onStatus = this.OnStatus;
                    if (onStatus != null)  onStatus((StatusEquipment)new PosStatus() { Status = eStatusPos.TransactionCanceledByUser });
                    return new PaymentResultModel() { IsSuccess = false };
                }
                Thread.Sleep(1000);
                ++num;
                ILogger<Ingenico> logger3 = this._logger;
                if (logger3 != null)
                    LoggerExtensions.LogDebug((ILogger)logger3, string.Format("[Ingenico] LastStatMsgCode = {0}\n", (object)_bpos1LibClass.LastStatMsgCode) + "[Ingenico] Description = " + this.GetString(_bpos1LibClass.LastStatMsgDescription) + "\n[Ingenico] LastErrorDescription = " + this.GetString(_bpos1LibClass.LastErrorDescription) + "\n" + string.Format("[Ingenico] LastResult = {0}\n", (object)_bpos1LibClass.LastResult) + string.Format("[Ingenico] LastErrorCode = {0}", (object)_bpos1LibClass.LastErrorCode), Array.Empty<object>());
                if (_bpos1LibClass == null)
                {
                    Action<StatusEquipment> onStatus = this.OnStatus;
                    if (onStatus != null)
                        onStatus((StatusEquipment)new PosStatus()
                        {
                            Status = eStatusPos.TransactionCanceledByUser
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
                Action<StatusEquipment> onStatus = this.OnStatus;
                if (onStatus != null)
                    onStatus((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.SuccessfullyFulfilled
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
                        Action<StatusEquipment> onStatus1 = this.OnStatus;
                        if (onStatus1 != null)
                            onStatus1((StatusEquipment)new PosStatus()
                            {
                                Status = eStatusPos.ErrorOpeningCOMPort
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
                        Action<StatusEquipment> onStatus2 = this.OnStatus;
                        if (onStatus2 != null)
                            onStatus2((StatusEquipment)new PosStatus()
                            {
                                Status = eStatusPos.NeedToOpenCOMPort
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
                        Action<StatusEquipment> onStatus3 = this.OnStatus;
                        if (onStatus3 != null)
                            onStatus3((StatusEquipment)new PosStatus()
                            {
                                Status = eStatusPos.ErrorConnectingWithTerminal
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
                    Action<StatusEquipment> onStatus1 = this.OnStatus;
                    if (onStatus1 == null)
                        break;
                    onStatus1((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.ApprovedAndCompleted
                    });
                    break;
                case 1:
                    Action<StatusEquipment> onStatus2 = this.OnStatus;
                    if (onStatus2 == null)
                        break;
                    onStatus2((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.AuthorizationDenied
                    });
                    break;
                case 2:
                    Action<StatusEquipment> onStatus3 = this.OnStatus;
                    if (onStatus3 == null)
                        break;
                    onStatus3((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.AuthorizationDenied
                    });
                    break;
                case 3:
                    Action<StatusEquipment> onStatus4 = this.OnStatus;
                    if (onStatus4 == null)
                        break;
                    onStatus4((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.UnregisteredTradingPoint
                    });
                    break;
                case 4:
                    Action<StatusEquipment> onStatus5 = this.OnStatus;
                    if (onStatus5 == null)
                        break;
                    onStatus5((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.AuthorizationRejectedWithdrawTheCardAtTheBanksRequest
                    });
                    break;
                case 5:
                    Action<StatusEquipment> onStatus6 = this.OnStatus;
                    if (onStatus6 == null)
                        break;
                    onStatus6((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.AuthorizationRejectedNoPayment
                    });
                    break;
                case 6:
                    Action<StatusEquipment> onStatus7 = this.OnStatus;
                    if (onStatus7 != null)
                        onStatus7((StatusEquipment)new PosStatus()
                        {
                            Status = eStatusPos.CommonErrorNeedToRepeat
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
                    Action<StatusEquipment> onStatus8 = this.OnStatus;
                    if (onStatus8 == null)
                        break;
                    onStatus8((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.AuthorizationRejectedWithdrawTheCardAtTheBanksRequest
                    });
                    break;
                case 12:
                    Action<StatusEquipment> onStatus9 = this.OnStatus;
                    if (onStatus9 != null)
                        onStatus9((StatusEquipment)new PosStatus()
                        {
                            Status = eStatusPos.InvalidTransactionNetworkErrorNeedToRepeat
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
                    Action<StatusEquipment> onStatus10 = this.OnStatus;
                    if (onStatus10 == null)
                        break;
                    onStatus10((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.IncorrectAmountEntered
                    });
                    break;
                case 14:
                    Action<StatusEquipment> onStatus11 = this.OnStatus;
                    if (onStatus11 == null)
                        break;
                    onStatus11((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.InvalidCardNumber
                    });
                    break;
                case 15:
                    Action<StatusEquipment> onStatus12 = this.OnStatus;
                    if (onStatus12 == null)
                        break;
                    onStatus12((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.BankNodeIsNotFoundOnTheNetwork
                    });
                    break;
                case 17:
                    Action<StatusEquipment> onStatus13 = this.OnStatus;
                    if (onStatus13 == null)
                        break;
                    onStatus13((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.CanceledByTheClient
                    });
                    break;
                case 21:
                    Action<StatusEquipment> onStatus14 = this.OnStatus;
                    if (onStatus14 == null)
                        break;
                    onStatus14((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.ActionsNotCompletedDidNotMatchData
                    });
                    break;
                case 28:
                    Action<StatusEquipment> onStatus15 = this.OnStatus;
                    if (onStatus15 != null)
                        onStatus15((StatusEquipment)new PosStatus()
                        {
                            Status = eStatusPos.NoResponseFileIsTemporarilyUnavailable
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
                    Action<StatusEquipment> onStatus16 = this.OnStatus;
                    if (onStatus16 != null)
                        onStatus16((StatusEquipment)new PosStatus()
                        {
                            Status = eStatusPos.WrongFormatNeedToRepeat
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
                    Action<StatusEquipment> onStatus17 = this.OnStatus;
                    if (onStatus17 == null)
                        break;
                    onStatus17((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.TheIssuerIsNotFoundInThePaymentSystem
                    });
                    break;
                case 32:
                    Action<StatusEquipment> onStatus18 = this.OnStatus;
                    if (onStatus18 == null)
                        break;
                    onStatus18((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.PartiallyCompleted
                    });
                    break;
                case 33:
                    Action<StatusEquipment> onStatus19 = this.OnStatus;
                    if (onStatus19 == null)
                        break;
                    onStatus19((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.TheValidityPeriodOfTheCardHasExpiredTheCardHasBeenWithdrawnAtTheBanksRequest
                    });
                    break;
                case 36:
                    Action<StatusEquipment> onStatus20 = this.OnStatus;
                    if (onStatus20 == null)
                        break;
                    onStatus20((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.ForbiddenCardRemove
                    });
                    break;
                case 37:
                    Action<StatusEquipment> onStatus21 = this.OnStatus;
                    if (onStatus21 == null)
                        break;
                    onStatus21((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.WithdrawnByTheIssuerRemovedFromTheCardAndContactedByTheAcquirer
                    });
                    break;
                case 38:
                    Action<StatusEquipment> onStatus22 = this.OnStatus;
                    if (onStatus22 == null)
                        break;
                    onStatus22((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.ThereAreNoAttemptsToEnterThePINRemoveTheCard
                    });
                    break;
                case 39:
                    Action<StatusEquipment> onStatus23 = this.OnStatus;
                    if (onStatus23 == null)
                        break;
                    onStatus23((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.NoClientsCreditAccount
                    });
                    break;
                case 41:
                    Action<StatusEquipment> onStatus24 = this.OnStatus;
                    if (onStatus24 == null)
                        break;
                    onStatus24((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.CardIslostRemoved
                    });
                    break;
                case 43:
                    Action<StatusEquipment> onStatus25 = this.OnStatus;
                    if (onStatus25 == null)
                        break;
                    onStatus25((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.CardIsStolenRemoved
                    });
                    break;
                case 51:
                    Action<StatusEquipment> onStatus26 = this.OnStatus;
                    if (onStatus26 == null)
                        break;
                    onStatus26((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.NotEnoughMoney
                    });
                    break;
                case 52:
                    Action<StatusEquipment> onStatus27 = this.OnStatus;
                    if (onStatus27 == null)
                        break;
                    onStatus27((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.NoSettlementSpecifiedClienAccount
                    });
                    break;
                case 53:
                    Action<StatusEquipment> onStatus28 = this.OnStatus;
                    if (onStatus28 == null)
                        break;
                    onStatus28((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.ThereIsNoCumulativeAccountOfTheClient
                    });
                    break;
                case 54:
                    Action<StatusEquipment> onStatus29 = this.OnStatus;
                    if (onStatus29 == null)
                        break;
                    onStatus29((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.TheExpirationDateOfTheCardExpires
                    });
                    break;
                case 55:
                    Action<StatusEquipment> onStatus30 = this.OnStatus;
                    if (onStatus30 == null)
                        break;
                    onStatus30((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.WrongPIN
                    });
                    break;
                case 57:
                    Action<StatusEquipment> onStatus31 = this.OnStatus;
                    if (onStatus31 == null)
                        break;
                    onStatus31((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.ThisTransactionTypeIsNotProvidedForTheGivenCard
                    });
                    break;
                case 58:
                    Action<StatusEquipment> onStatus32 = this.OnStatus;
                    if (onStatus32 == null)
                        break;
                    onStatus32((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.ThisTypeOfTransactionIsNotProvidedForPOSTerminal
                    });
                    break;
                case 61:
                    Action<StatusEquipment> onStatus33 = this.OnStatus;
                    if (onStatus33 == null)
                        break;
                    onStatus33((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.TheAamountOfAuthorizationExceededTheExpenseLimitOnTheCard
                    });
                    break;
                case 62:
                    Action<StatusEquipment> onStatus34 = this.OnStatus;
                    if (onStatus34 == null)
                        break;
                    onStatus34((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.IncorrectServiceCodeForbiddenCardCanNotBeSeized
                    });
                    break;
                case 64:
                    Action<StatusEquipment> onStatus35 = this.OnStatus;
                    if (onStatus35 == null)
                        break;
                    onStatus35((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.TheAmountOfTheCancellationAuthorizationIsDifferentFromTheAmountOfTheOriginalAuthorization
                    });
                    break;
                case 65:
                    Action<StatusEquipment> onStatus36 = this.OnStatus;
                    if (onStatus36 == null)
                        break;
                    onStatus36((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.TheExpenseLimitExpiredOnTheAccount
                    });
                    break;
                case 66:
                    Action<StatusEquipment> onStatus37 = this.OnStatus;
                    if (onStatus37 == null)
                        break;
                    onStatus37((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.TheCardIsVoidCanNotBeSeized
                    });
                    break;
                case 67:
                    Action<StatusEquipment> onStatus38 = this.OnStatus;
                    if (onStatus38 == null)
                        break;
                    onStatus38((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.CardIsWithdrawnFromATM
                    });
                    break;
                case 68:
                    Action<StatusEquipment> onStatus39 = this.OnStatus;
                    if (onStatus39 == null)
                        break;
                    onStatus39((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.ItIsTooLateToReceiveAnAnswerFromTheNetworkItIsNecessaryToRepeat
                    });
                    break;
                case 75:
                    Action<StatusEquipment> onStatus40 = this.OnStatus;
                    if (onStatus40 == null)
                        break;
                    onStatus40((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.TheNumberOfIncorrectlyEnteredPINsExceededTheAmountDischarged
                    });
                    break;
                case 76:
                    Action<StatusEquipment> onStatus41 = this.OnStatus;
                    if (onStatus41 == null)
                        break;
                    onStatus41((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.TheNumberOfIncorrectlyEnteredPINsExceededTheAmountDischarged
                    });
                    break;
                case 77:
                    Action<StatusEquipment> onStatus42 = this.OnStatus;
                    if (onStatus42 == null)
                        break;
                    onStatus42((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.ActionsAreNotCompletedIncompleteDataItIsNecessaryToRollbackOrRepeat
                    });
                    break;
                case 78:
                    Action<StatusEquipment> onStatus43 = this.OnStatus;
                    if (onStatus43 == null)
                        break;
                    onStatus43((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.NoAccount
                    });
                    break;
                case 79:
                    Action<StatusEquipment> onStatus44 = this.OnStatus;
                    if (onStatus44 == null)
                        break;
                    onStatus44((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.AlreadyCanceledWhenTurnedOn
                    });
                    break;
                case 80:
                    Action<StatusEquipment> onStatus45 = this.OnStatus;
                    if (onStatus45 != null)
                        onStatus45((StatusEquipment)new PosStatus()
                        {
                            Status = eStatusPos.GeneralNetworkErrorIncorrectData
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
                    Action<StatusEquipment> onStatus46 = this.OnStatus;
                    if (onStatus46 == null)
                        break;
                    onStatus46((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.RemoteNetworkErrorOrPINEncryption
                    });
                    break;
                case 82:
                    Action<StatusEquipment> onStatus47 = this.OnStatus;
                    if (onStatus47 == null)
                        break;
                    onStatus47((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.TimeoutWhenConnectedWithTheIssuersNodeOrWrongCVVOrCacheIsNotApprovedTheCashbackSumLimitIsExceeded
                    });
                    break;
                case 83:
                    Action<StatusEquipment> onStatus48 = this.OnStatus;
                    if (onStatus48 != null)
                        onStatus48((StatusEquipment)new PosStatus()
                        {
                            Status = eStatusPos.ThePINVerificationTransactionIsUnsuccessfulNetworkError
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
                    Action<StatusEquipment> onStatus49 = this.OnStatus;
                    if (onStatus49 == null)
                        break;
                    onStatus49((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.TheCardIsInOrderThereIsNoReasonToRefuse
                    });
                    break;
                case 86:
                    Action<StatusEquipment> onStatus50 = this.OnStatus;
                    if (onStatus50 != null)
                        onStatus50((StatusEquipment)new PosStatus()
                        {
                            Status = eStatusPos.PINCanNotBeCheckedNetworkError
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
                    Action<StatusEquipment> onStatus51 = this.OnStatus;
                    if (onStatus51 != null)
                        onStatus51((StatusEquipment)new PosStatus()
                        {
                            Status = eStatusPos.PINEncryptionErrorNetworkError
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
                    Action<StatusEquipment> onStatus52 = this.OnStatus;
                    if (onStatus52 != null)
                        onStatus52((StatusEquipment)new PosStatus()
                        {
                            Status = eStatusPos.IdentificationErrorIsANetworkError
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
                    Action<StatusEquipment> onStatus53 = this.OnStatus;
                    if (onStatus53 != null)
                        onStatus53((StatusEquipment)new PosStatus()
                        {
                            Status = eStatusPos.NoConnectionWithTheBankByTheIssuerNetworkError
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
                    Action<StatusEquipment> onStatus54 = this.OnStatus;
                    if (onStatus54 != null)
                        onStatus54((StatusEquipment)new PosStatus()
                        {
                            Status = eStatusPos.UnsuccessfulRequestRoutingIsNotPossibleNetworkError
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
                    Action<StatusEquipment> onStatus55 = this.OnStatus;
                    if (onStatus55 == null)
                        break;
                    onStatus55((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.TheTransactionCanNotBeCompletedTheIssuerDeclineAuthorizationDueToAViolationOfTheRules
                    });
                    break;
                case 94:
                    Action<StatusEquipment> onStatus56 = this.OnStatus;
                    if (onStatus56 != null)
                        onStatus56((StatusEquipment)new PosStatus()
                        {
                            Status = eStatusPos.DuplicationOfTransmissionNetworkError
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
                    Action<StatusEquipment> onStatus57 = this.OnStatus;
                    if (onStatus57 == null)
                        break;
                    onStatus57((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.GeneralSystemMalfunction
                    });
                    break;
                case 119:
                    Action<StatusEquipment> onStatus58 = this.OnStatus;
                    if (onStatus58 == null)
                        break;
                    onStatus58((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.UnableToSendEncryptedMessage
                    });
                    break;
                case 1000:
                    Action<StatusEquipment> onStatus59 = this.OnStatus;
                    if (onStatus59 != null)
                        onStatus59((StatusEquipment)new PosStatus()
                        {
                            Status = eStatusPos.GeneralError
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
                    Action<StatusEquipment> onStatus60 = this.OnStatus;
                    if (onStatus60 == null)
                        break;
                    onStatus60((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.TransactionCanceledByUser
                    });
                    break;
                case 1002:
                    Action<StatusEquipment> onStatus61 = this.OnStatus;
                    if (onStatus61 == null)
                        break;
                    onStatus61((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.EMVDecline
                    });
                    break;
                case 1003:
                    Action<StatusEquipment> onStatus62 = this.OnStatus;
                    if (onStatus62 == null)
                        break;
                    onStatus62((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.TransactionLogIsFullNeedCloseBatch
                    });
                    break;
                case 1004:
                    Action<StatusEquipment> onStatus63 = this.OnStatus;
                    if (onStatus63 != null)
                        onStatus63((StatusEquipment)new PosStatus()
                        {
                            Status = eStatusPos.NoConnectionWithHost
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
                    Action<StatusEquipment> onStatus64 = this.OnStatus;
                    if (onStatus64 != null)
                        onStatus64((StatusEquipment)new PosStatus()
                        {
                            Status = eStatusPos.NoPaperInPrinter
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
                    Action<StatusEquipment> onStatus65 = this.OnStatus;
                    if (onStatus65 == null)
                        break;
                    onStatus65((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.ErrorCryptoKeys
                    });
                    break;
                case 1007:
                    Action<StatusEquipment> onStatus66 = this.OnStatus;
                    if (onStatus66 == null)
                        break;
                    onStatus66((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.CardReaderIsNotConnected
                    });
                    break;
                case 1008:
                    Action<StatusEquipment> onStatus67 = this.OnStatus;
                    if (onStatus67 == null)
                        break;
                    onStatus67((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.TransactionIsAlreadyComplete
                    });
                    break;
            }
        }

        private void InvokeLastStatusMsg(byte LastStatMsgCode)
        {
            switch (LastStatMsgCode)
            {
                case 1:
                    Action<StatusEquipment> onStatus1 = this.OnStatus;
                    if (onStatus1 == null)
                        break;
                    onStatus1((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.CardWasRead
                    });
                    break;
                case 2:
                    Action<StatusEquipment> onStatus2 = this.OnStatus;
                    if (onStatus2 == null)
                        break;
                    onStatus2((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.UsedAChipCard
                    });
                    break;
                case 3:
                    Action<StatusEquipment> onStatus3 = this.OnStatus;
                    if (onStatus3 == null)
                        break;
                    onStatus3((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.AuthorizationInProgress
                    });
                    break;
                case 4:
                    Action<StatusEquipment> onStatus4 = this.OnStatus;
                    if (onStatus4 == null)
                        break;
                    onStatus4((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.WaitingForCashierAction
                    });
                    break;
                case 5:
                    Action<StatusEquipment> onStatus5 = this.OnStatus;
                    if (onStatus5 == null)
                        break;
                    onStatus5((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.PrintingReceipt
                    });
                    break;
                case 6:
                    Action<StatusEquipment> onStatus6 = this.OnStatus;
                    if (onStatus6 == null)
                        break;
                    onStatus6((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.PinEntryIsNeeded
                    });
                    break;
                case 7:
                    Action<StatusEquipment> onStatus7 = this.OnStatus;
                    if (onStatus7 == null)
                        break;
                    onStatus7((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.CardWasRemoved
                    });
                    break;
                case 8:
                    Action<StatusEquipment> onStatus8 = this.OnStatus;
                    if (onStatus8 == null)
                        break;
                    onStatus8((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.EMVMultiAids
                    });
                    break;
                case 9:
                    Action<StatusEquipment> onStatus9 = this.OnStatus;
                    if (onStatus9 == null)
                        break;
                    onStatus9((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.WaitingForCard
                    });
                    break;
                case 10:
                    Action<StatusEquipment> onStatus10 = this.OnStatus;
                    if (onStatus10 == null)
                        break;
                    onStatus10((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.InProgress
                    });
                    break;
                case 11:
                    Action<StatusEquipment> onStatus11 = this.OnStatus;
                    if (onStatus11 == null)
                        break;
                    onStatus11((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.CorrectTransaction
                    });
                    break;
                case 12:
                    Action<StatusEquipment> onStatus12 = this.OnStatus;
                    if (onStatus12 == null)
                        break;
                    onStatus12((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.PinInputWaitKey
                    });
                    break;
                case 13:
                    Action<StatusEquipment> onStatus13 = this.OnStatus;
                    if (onStatus13 == null)
                        break;
                    onStatus13((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.PinInputBackspacePressed
                    });
                    break;
                case 14:
                    Action<StatusEquipment> onStatus14 = this.OnStatus;
                    if (onStatus14 == null)
                        break;
                    onStatus14((StatusEquipment)new PosStatus()
                    {
                        Status = eStatusPos.PinInputKeyPressed
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


        public async Task<BatchTotals> GetXZ(bool IsX=true)
        {
            if (!this.StartBPOS())
                return (BatchTotals)null;
            if(IsX)
                _bpos1LibClass.PrintBatchTotals(this.merchantId);
            else
            _bpos1LibClass.Settlement(this.merchantId);
            this.WaitResponse();            
            BatchTotals batchTotals = new BatchTotals()
            {
                DebitCount = _bpos1LibClass.TotalsDebitNum,
                DebitSum = _bpos1LibClass.TotalsDebitAmt,
                CreditCount = _bpos1LibClass.TotalsCreditNum,
                CreditSum = _bpos1LibClass.TotalsCreditAmt,
                CencelledCount = _bpos1LibClass.TotalsCancelledNum,
                CencelledSum = _bpos1LibClass.TotalsCancelledAmt,      
            };
            batchTotals.Receipt = GetLastReceipt(false);
            StopBPOS();
            return batchTotals;
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
            BatchTotals batchTotals = new BatchTotals() {
                DebitCount = _bpos1LibClass.TotalsDebitNum,
                DebitSum = _bpos1LibClass.TotalsDebitAmt,
                CreditCount = _bpos1LibClass.TotalsCreditNum,
                CreditSum = _bpos1LibClass.TotalsCreditAmt,
                CencelledCount = _bpos1LibClass.TotalsCancelledNum,
                CencelledSum = _bpos1LibClass.TotalsCancelledAmt,
                Receipt = GetLastReceipt(false)
            };
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

        public Task<PaymentResultModel> Purchase(double amount,double pCash)
        {
            try
            {
                if (!this.StartBPOS())
                    return Task.FromResult<PaymentResultModel>(new PaymentResultModel() { IsSuccess = false });
                _isCancelRequested = false;
                _bpos1LibClass.Purchase(Convert.ToUInt32(amount * 100.0), 0U, this.merchantId);

                if (OnStatus != null)
                    OnStatus((StatusEquipment)new PosStatus() {  Status = eStatusPos.WaitingForCard });
                PaymentResultModel result = this.WaitPosRespone();                
                if (_logger != null)
                    LoggerExtensions.LogDebug((ILogger)_logger, "[Ingenico] Purches " + JsonConvert.SerializeObject((object)result), Array.Empty<object>());
                if (result.IsSuccess)
                {
                    _bpos1LibClass.Confirm();
                    WaitResponse();
                }
                
                if (OnResponse != null) OnResponse((IPosResponse)new PayPosResponse() { Response = result });                
                StopBPOS();
                return Task.FromResult<PaymentResultModel>(result);
            }
            catch (Exception ex)
            {
                LoggerExtensions.LogError((ILogger)this._logger, ex, "[Ingenico] " + ex.Message);
                return Task.FromResult<PaymentResultModel>(new PaymentResultModel(){ IsSuccess = false });
            }
            finally
            {
                _bpos1LibClass = (BPOS1LibClass)null;
            }
        }

        public void Cancel() => _isCancelRequested = true;

        public List<string> GetLastReceipt(bool IsStart = true)
        {
            if (IsStart)
                if (!this.StartBPOS())
                    return (List<string>)null;
            _bpos1LibClass.ReqCurrReceipt();
            this.WaitResponse();
            string receipt = _bpos1LibClass.Receipt;
            if (IsStart)
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

