using ECRCommXLib;
using Front.Equipments.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelMID;
using ModelMID.DB;
using Newtonsoft.Json;
using System;
using System.Text;
using Utils;


namespace Front.Equipments
{
    public class BT_Ingenico : BankTerminal, IDisposable
    {
        public Action<IPosResponse> OnResponse { get; set; }
        public Action<DeviceLog> OnDeviceWarning { get; set; }

        private static BPOS1LibClass BPOS;

        private byte Port;

        private readonly ILogger<BT_Ingenico> Logger;
        private readonly Encoding Encoding1251 = Encoding.GetEncoding(1251);
        private readonly Encoding Encoding1252 = Encoding.GetEncoding(1252);


        public BT_Ingenico(Equipment pEquipment, IConfiguration pConfiguration, ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) :
                                base(pEquipment, pConfiguration, eModelEquipment.Ingenico, pLoggerFactory, pActionStatus)
        {
            try
            {
                State = eStateEquipment.Init;
                Logger = LoggerFactory?.CreateLogger<BT_Ingenico>();

                byte.TryParse(SerialPort, out Port);

                SetCodeBank();
            }
            catch (Exception e)
            {
                State = eStateEquipment.Error;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
        }

        void SetCodeBank()
        {
            CodeBank = GetBank();
            if (CodeBank == eBank.NotDefine)
                State = eStateEquipment.Error;
            else
                State = eStateEquipment.On;
        }

        public string GetTerminalID { get { return BPOS.TerminalID; } }

        public override Payment Refund(decimal pAmount, string pRRN, int IdWorkPlace = 0, bool pIsCashBack = false)
        {
            return Refund(Convert.ToDouble(pAmount), pRRN, IdWorkPlace, pIsCashBack).Result;
        }


        public async Task<Payment> Refund(double amount, string bsRRN, int pIdWorkPlace = 0, bool pIsCashBack = false)
        {
            try
            {
                if (!StartBPOS())
                    return new Payment()
                    {
                        IsSuccess = false
                    };
                CancelRequested = false;
                BPOS.Refund(Convert.ToUInt32(amount * 100.0), 0U, GetMechantIdByIdWorkPlace(pIdWorkPlace), bsRRN);
                OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.WaitingForCard });
                Payment result = WaitPosRespone(120, pIsCashBack , Convert.ToUInt32(amount * 100.0));
                if (result.IsSuccess)
                {
                    BPOS.Confirm();
                    Payment Res = WaitPosRespone(20);
                    if (!Res.IsSuccess)
                        result = Res;
                }
                StopBPOS();
                return result;
            }
            catch (Exception ex)
            {
                return new Payment() { IsSuccess = false };
            }
            finally
            {
                BPOS = (BPOS1LibClass)null;
            }
        }

        public eDeviceConnectionStatus Init()
        {
            try
            {
                OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.All, Message = "[Ingenico] - Start Initialization" });
                if (!this.StartBPOS())
                {
                    OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Critical, Message = "Device not connected" });
                    StopBPOS();
                    return eDeviceConnectionStatus.NotConnected;
                }
                byte? lastErrorCode = BPOS?.LastErrorCode;
                int? nullable = lastErrorCode.HasValue ? new int?((int)lastErrorCode.GetValueOrDefault()) : new int?();
                int num = 0;
                bool flag = nullable.GetValueOrDefault() == num & nullable.HasValue;
                StopBPOS();
                OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.All, Message = $"[Ingenico] - Initilization result {flag}" });
                return eDeviceConnectionStatus.Enabled;
            }
            catch (Exception ex)
            {
                ILogger<BT_Ingenico> logger = this.Logger;
                if (logger != null)
                    LoggerExtensions.LogError((ILogger)logger, ex, ex.Message, Array.Empty<object>());
                OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Critical, Message = "Device not connected" });
                return eDeviceConnectionStatus.NotConnected;
            }
        }

        public eDeviceConnectionStatus GetDeviceStatusSync() => this.TestDeviceSync();

        public Task<eDeviceConnectionStatus> GetDeviceStatus() => Task.Run<eDeviceConnectionStatus>((Func<eDeviceConnectionStatus>)(() => this.GetDeviceStatusSync()));

        public override StatusEquipment TestDevice()
        {
            var r = TestDeviceSync();
            State = r == eDeviceConnectionStatus.Enabled ? eStateEquipment.On : eStateEquipment.Error;
            if (State == eStateEquipment.On)
                SetCodeBank();
            return new StatusEquipment(eModelEquipment.Ingenico, State, r.ToString());
        }

        public eDeviceConnectionStatus TestDeviceSync()
        {
            eDeviceConnectionStatus connectionStatus = eDeviceConnectionStatus.NotConnected;
            if (!this.StartBPOS())
            {
                OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Critical, Message = "Device not connected" });
                return connectionStatus;
            }
            BPOS.Ping();

            if (BPOS.LastResult == (byte)2)
            {
                int lastResult = (int)BPOS.LastResult;
                int lastErrorCode = (int)BPOS.LastErrorCode;
                byte lastStatMsgCode = BPOS.LastStatMsgCode;

                if (Logger != null)
                {
                    LoggerExtensions.LogTrace((ILogger)Logger, $"[Ingenico] LastStatMsgCode = {lastStatMsgCode}");
                    LoggerExtensions.LogTrace((ILogger)Logger, "[Ingenico] Description = " + BPOS.LastStatMsgDescription);
                    LoggerExtensions.LogTrace((ILogger)Logger, "[Ingenico] LastErrorDescription = " + BPOS.LastErrorDescription);
                }
                connectionStatus = eDeviceConnectionStatus.Enabled;
            }
            else
            {
                OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Critical, Message = $"Test failure with code {BPOS.LastResult}" });
                connectionStatus = eDeviceConnectionStatus.InitializationError;
            }
            StopBPOS();
            return connectionStatus;
        }

        public Task<eDeviceConnectionStatus> TestDeviceAsync() => Task.Run<eDeviceConnectionStatus>((Func<eDeviceConnectionStatus>)(() => this.TestDeviceSync()));

        eBank GetBank()
        {
            if (StartBPOS())
            {
                BPOS.POSGetInfo();
                Thread.Sleep(1000);
                string terminalInfo = BPOS.TerminalInfo;
                StopBPOS();
                if (string.IsNullOrEmpty(terminalInfo))
                    return eBank.NotDefine;
                var a = terminalInfo.Split('/');

                if (a.Length > 3)
                    return eBank.PrivatBank;
                else
                    return terminalInfo.IndexOf("/MTB") > 0 ? eBank.MTB : eBank.OschadBank; //OSCHADBANK               
            }
            return eBank.NotDefine;
        }

        public override string GetDeviceInfo()
        {
            try
            {
                if (!StartBPOS())
                    return string.Empty;
                BPOS.POSGetInfo();
                Thread.Sleep(1000);
                string terminalInfo = BPOS.TerminalInfo;
                string str = @$"Model: Ingenico {Environment.NewLine}TerminalId: {GetTerminalID}{Environment.NewLine}Bank: {CodeBank}{Environment.NewLine}{base.GetDeviceInfo()}{Environment.NewLine}";

                StopBPOS();

                if (!string.IsNullOrEmpty(terminalInfo))
                {
                    str = str + "Software version: " + new string(terminalInfo.TakeWhile<char>((Func<char, bool>)(x => x != ' ')).ToArray<char>()) +
                        $"{Environment.NewLine}Terminal profile ID: " + new string(terminalInfo.Substring(terminalInfo.IndexOf(" ", StringComparison.Ordinal)).Take<char>(8).ToArray<char>());
                    var a = terminalInfo.Split('/');
                    if (a.Length > 2)
                        str += $"{Environment.NewLine}TerminalId: {a[1]}{Environment.NewLine}";
                    str += $"{Environment.NewLine}TerminalInfo: {terminalInfo}";
                    str += $"{Environment.NewLine}MerchantId=>{MerchantId}";
                }
                return str;
            }
            catch (Exception ex)
            {
                LoggerExtensions.LogError(Logger, ex, ex.Message, Array.Empty<object>());
                OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "Cannot get information during error - " + ex.Message });
                return (string)null;
            }
        }

        //public Task<string> GetInfoAsync() => Task.Run<string>((Func<string>)(() => this.GetInfoSync()));
        public Payment WaitPosRespone(int pTime = 120, bool pIsCashBack=false, uint pSum = 0)
        {
            bool Is11=false;
            pTime *= 2;
            while (BPOS.LastResult == (byte)2 && pTime > 0)
            {
                LoggerExtensions.LogDebug((ILogger)Logger, "[Ingenico] WaitPosRespone *");
                if (CancelRequested)
                {
                    CancelRequested = false;
                    BPOS.Cancel();
                    Thread.Sleep(1000);
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TransactionCanceledByUser });
                    return new Payment() { IsSuccess = false, IsCashBack= pIsCashBack };
                }
                Thread.Sleep(500);
                pTime--;
                if (Logger != null)
                    LoggerExtensions.LogDebug((ILogger)Logger, string.Format("[Ingenico] LastStatMsgCode = {0}\n", (object)BPOS.LastStatMsgCode) + "[Ingenico] Description = " + this.GetString(BPOS.LastStatMsgDescription) + "\n[Ingenico] LastErrorDescription = " + this.GetString(BPOS.LastErrorDescription) + "\n" + string.Format("[Ingenico] LastResult = {0}\n", (object)BPOS.LastResult) + string.Format("[Ingenico] LastErrorCode = {0}", (object)BPOS.LastErrorCode), Array.Empty<object>());
                if (BPOS == null)
                {
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TransactionCanceledByUser });
                    return new Payment() { IsSuccess = false, IsCashBack = pIsCashBack };
                }

                //List<string[]> xx = new();//!!!!TMP
                //xx.Add(["44838200"]);
                //Global.Settings.CashBackCard = xx;


                InvokeLastStatusMsg(BPOS.LastStatMsgCode);
                if (!Is11 && BPOS.LastStatMsgCode == 11)
                {
                    Is11 = true;
                    if (Global.Settings.CashBackCard?.Count() > 0)
                    {
                        bool IsCashBackCard = Global.Settings.CashBackCard.Where(el => BPOS.PAN.StartsWith(el[0]))?.Any() == true;
                        if(pIsCashBack && string.IsNullOrEmpty(BPOS.PAN) && (BPOS.PAN.Length!=16 ||   BPOS.PAN[8]<'0'|| BPOS.PAN[8] > '9'))
                        {
                            BPOS.Cancel();
                            Thread.Sleep(1000);
                            OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TerminalNotReadyCashBack });
                            FileLogger.WriteLogMessage("BT_Ingenico", "WaitPosRespone", $"pIsCashBack=>{pIsCashBack} IsCashBackCard=>{IsCashBackCard} {BPOS.PAN}");
                            return new Payment() { IsSuccess = false, IsCashBack = pIsCashBack };
                        }
                        if (pIsCashBack != IsCashBackCard )
                        {
                            BPOS.Cancel();
                            Thread.Sleep(1000);
                            OnStatus?.Invoke(new PosStatus() { Status = pIsCashBack ? eStatusPos.NoCashBackCard:eStatusPos.CashBackCardUseProhibited });
                            FileLogger.WriteLogMessage("BT_Ingenico", "WaitPosRespone", $"pIsCashBack=>{pIsCashBack} IsCashBackCard=>{IsCashBackCard} {BPOS.PAN}");
                            return new Payment() { IsSuccess = false, IsCashBack = pIsCashBack };
                        }
                    }
                    BPOS.CorrectTransaction(pSum, 0);
                    pTime = 240;
                }
            }

            if (Logger != null)
            {
                LoggerExtensions.LogDebug((ILogger)Logger, string.Format("[Ingenico] LastStatMsgCode = {0}\n", (object)BPOS.LastStatMsgCode) + "[Ingenico] Description = " + this.GetString(BPOS.LastStatMsgDescription) + "\n[Ingenico] LastErrorDescription = " + this.GetString(BPOS.LastErrorDescription) + "\n" + string.Format("[Ingenico] LastResult = {0}\n", (object)BPOS.LastResult) + string.Format("[Ingenico] LastErrorCode = {0}", (object)BPOS.LastErrorCode), Array.Empty<object>());
            }
            if (BPOS.LastResult == (byte)0 && BPOS.LastErrorCode == (byte)0)
            {
                OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.SuccessfullyFulfilled });
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"RRN=>{BPOS.RRN} TerminalID=>{BPOS.TerminalID} InvoiceNum={BPOS.InvoiceNum} Amount=>{BPOS.Amount} AddAmount=>{BPOS.AddAmount} ", eTypeLog.Full);
                return new Payment()
                {
                    IsSuccess = true,
                    TransactionId = string.Format("{0}", (object)BPOS.TrnBatchNum),
                    NumberCard = BPOS.PAN,
                    DateCreate = string.IsNullOrWhiteSpace(BPOS.DateTime) ? DateTime.Now : DateTime.ParseExact(BPOS.DateTime, "yyMMddHHmmss", (IFormatProvider)null),
                    CodeAuthorization = BPOS.RRN,
                    TransactionStatus = this.GetString(BPOS.LastErrorDescription),
                    NumberSlip = this.GetString(BPOS.AuthCode),
                    NumberTerminal = this.GetString(BPOS.TerminalID),
                    SumPay = Math.Round(Convert.ToDecimal(BPOS.Amount) / 100M, 2, MidpointRounding.AwayFromZero),
                    PosAddAmount = Math.Round(Convert.ToDecimal(BPOS.AddAmount) / 100M, 2, MidpointRounding.AwayFromZero),
                    NumberReceipt = (long)BPOS.InvoiceNum,
                    CardHolder = this.GetString(BPOS.CardHolder),
                    IssuerName = this.GetString(BPOS.IssuerName),
                    Bank = this.GetString(BPOS.ECRDataTM),
                    CodeBank = this.CodeBank,
                    MerchantID = BPOS.MerchantID,                    
                    IsCashBack = pIsCashBack
                    //Receipt= this.ParseReceipt(BPOS.Receipt)
                };
            }

            if (BPOS.LastResult == (byte)1)
            {
                switch (BPOS.LastErrorCode)
                {
                    case 1:
                        OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.ErrorOpeningCOMPort });
                        OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] Error open connection" });
                        //State = eStateEquipment.Error;
                        break;
                    case 2:
                        OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.NeedToOpenCOMPort });
                        OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] Error open connection" });
                        //State = eStateEquipment.Error;
                        break;
                    case 3:
                        OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.ErrorConnectingWithTerminal });
                        OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] Error open connection" });
                        //State = eStateEquipment.Error;
                        break;
                    case 4:
                        this.InvokeResponseCode(BPOS.ResponseCode);
                        break;
                }
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"LastResult=>{BPOS.LastResult} LastErrorCode={BPOS.LastErrorCode} ResponseCode=>{BPOS.ResponseCode}", eTypeLog.Error);
                if (BPOS.LastErrorCode > 0 && BPOS.LastErrorCode < 4)
                {
                    //OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.ErrorCommunication });
                    StopBPOS();
                    Thread.Sleep(1000);
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TestDevice });
                    var r = TestDeviceSync();
                    if (r != eDeviceConnectionStatus.Enabled)
                        State = eStateEquipment.Error;
                }
            }
            return new Payment() { IsSuccess = false };
        }

        private void InvokeResponseCode(uint pResponseCode)
        {
            switch (pResponseCode)
            {
                case 0:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.ApprovedAndCompleted });
                    break;
                case 1:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.AuthorizationDenied });
                    break;
                case 2:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.AuthorizationDenied });
                    break;
                case 3:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.UnregisteredTradingPoint });
                    break;
                case 4:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.AuthorizationRejectedWithdrawTheCardAtTheBanksRequest });
                    break;
                case 5:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.AuthorizationRejectedNoPayment });
                    break;
                case 6:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.CommonErrorNeedToRepeat });
                    OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] Common error need repeat" });
                    break;
                case 7:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.AuthorizationRejectedWithdrawTheCardAtTheBanksRequest });
                    break;
                case 12:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.InvalidTransactionNetworkErrorNeedToRepeat });
                    OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] Common error need repeat" });
                    break;
                case 13:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.IncorrectAmountEntered });
                    break;
                case 14:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.InvalidCardNumber });
                    break;
                case 15:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.BankNodeIsNotFoundOnTheNetwork });
                    break;
                case 17:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.CanceledByTheClient });
                    break;
                case 21:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.ActionsNotCompletedDidNotMatchData });
                    break;
                case 28:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.NoResponseFileIsTemporarilyUnavailable });
                    OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] Common error need repeat" });
                    break;
                case 30:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.WrongFormatNeedToRepeat });
                    OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] Worng format need to repeat" });
                    break;
                case 31:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TheIssuerIsNotFoundInThePaymentSystem });
                    break;
                case 32:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.PartiallyCompleted });
                    break;
                case 33:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TheValidityPeriodOfTheCardHasExpiredTheCardHasBeenWithdrawnAtTheBanksRequest });
                    break;
                case 36:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.ForbiddenCardRemove });
                    break;
                case 37:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.WithdrawnByTheIssuerRemovedFromTheCardAndContactedByTheAcquirer });
                    break;
                case 38:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.ThereAreNoAttemptsToEnterThePINRemoveTheCard });
                    break;
                case 39:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.NoClientsCreditAccount });
                    break;
                case 41:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.CardIslostRemoved });
                    break;
                case 43:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.CardIsStolenRemoved });
                    break;
                case 51:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.NotEnoughMoney });
                    break;
                case 52:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.NoSettlementSpecifiedClienAccount });
                    break;
                case 53:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.ThereIsNoCumulativeAccountOfTheClient });
                    break;
                case 54:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TheExpirationDateOfTheCardExpires });
                    break;
                case 55:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.WrongPIN });
                    break;
                case 57:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.ThisTransactionTypeIsNotProvidedForTheGivenCard });
                    break;
                case 58:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.ThisTypeOfTransactionIsNotProvidedForPOSTerminal });
                    break;
                case 61:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TheAamountOfAuthorizationExceededTheExpenseLimitOnTheCard });
                    break;
                case 62:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.IncorrectServiceCodeForbiddenCardCanNotBeSeized });
                    break;
                case 64:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TheAmountOfTheCancellationAuthorizationIsDifferentFromTheAmountOfTheOriginalAuthorization });
                    break;
                case 65:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TheExpenseLimitExpiredOnTheAccount });
                    break;
                case 66:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TheCardIsVoidCanNotBeSeized });
                    break;
                case 67:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.CardIsWithdrawnFromATM });
                    break;
                case 68:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.ItIsTooLateToReceiveAnAnswerFromTheNetworkItIsNecessaryToRepeat });
                    break;
                case 75:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TheNumberOfIncorrectlyEnteredPINsExceededTheAmountDischarged });
                    break;
                case 76:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TheNumberOfIncorrectlyEnteredPINsExceededTheAmountDischarged });
                    break;
                case 77:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.ActionsAreNotCompletedIncompleteDataItIsNecessaryToRollbackOrRepeat });
                    break;
                case 78:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.NoAccount });
                    break;
                case 79:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.AlreadyCanceledWhenTurnedOn });
                    break;
                case 80:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.GeneralNetworkErrorIncorrectData });
                    OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] General network error" });
                    break;
                case 81:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.RemoteNetworkErrorOrPINEncryption });
                    break;
                case 82:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TimeoutWhenConnectedWithTheIssuersNodeOrWrongCVVOrCacheIsNotApprovedTheCashbackSumLimitIsExceeded });
                    break;
                case 83:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.ThePINVerificationTransactionIsUnsuccessfulNetworkError });
                    OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] General network error" });
                    break;
                case 85:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TheCardIsInOrderThereIsNoReasonToRefuse });
                    break;
                case 86:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.PINCanNotBeCheckedNetworkError });
                    OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] General network error" });
                    break;
                case 88:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.PINEncryptionErrorNetworkError });
                    OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] General network error" });
                    break;
                case 89:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.IdentificationErrorIsANetworkError });
                    OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] General network error" });
                    break;
                case 91:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.NoConnectionWithTheBankByTheIssuerNetworkError });
                    OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] General network error" });
                    break;
                case 92:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.UnsuccessfulRequestRoutingIsNotPossibleNetworkError });
                    OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] General network error" });
                    break;
                case 93:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TheTransactionCanNotBeCompletedTheIssuerDeclineAuthorizationDueToAViolationOfTheRules });
                    break;
                case 94:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.DuplicationOfTransmissionNetworkError });
                    OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] General network error" });
                    break;
                case 96:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.GeneralSystemMalfunction });
                    break;
                case 119:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.UnableToSendEncryptedMessage });
                    break;
                case 1000:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.GeneralError });
                    OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] General error" });
                    break;
                case 1001:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TransactionCanceledByUser });
                    break;
                case 1002:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.EMVDecline });
                    break;
                case 1003:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TransactionLogIsFullNeedCloseBatch });
                    break;
                case 1004:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.NoConnectionWithHost });
                    OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] General network error" });
                    break;
                case 1005:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.NoPaperInPrinter });
                    OnDeviceWarning?.Invoke(new PosDeviceLog() { Category = TerminalLogCategory.Warning, Message = "[Ingenico] Paper was ended" });
                    break;
                case 1006:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.ErrorCryptoKeys });
                    break;
                case 1007:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.CardReaderIsNotConnected });
                    break;
                case 1008:
                    OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.TransactionIsAlreadyComplete });
                    break;
            }
        }



        public void Settlement(byte pMerchantId = 0)
        {
            if (!this.StartBPOS())
                return;
            BPOS.Settlement(pMerchantId);
            this.WaitResponse();
            StopBPOS();
        }

        public override BatchTotals PrintZ(int pIdWorkPlace = 0)
        {
            var r = GetXZ(false, pIdWorkPlace);
            return r.Result;
        }

        public override BatchTotals PrintX(int pIdWorkPlace = 0)
        {
            var r = GetXZ(true, pIdWorkPlace);
            return r.Result;
        }

        public async Task<BatchTotals> GetXZ(bool IsX = true, int pIdWorkPlace = 0)
        {
            byte MerchantId = GetMechantIdByIdWorkPlace(pIdWorkPlace);
            if (!this.StartBPOS())
                return (BatchTotals)null;
            if (IsX)
                BPOS.PrintBatchTotals(MerchantId);
            else
                BPOS.Settlement(MerchantId);
            this.WaitResponse();
            BatchTotals batchTotals = new BatchTotals()
            {
                DebitCount = BPOS.TotalsDebitNum,
                DebitSum = BPOS.TotalsDebitAmt,
                CreditCount = BPOS.TotalsCreditNum,
                CreditSum = BPOS.TotalsCreditAmt,
                CencelledCount = BPOS.TotalsCancelledNum,
                CencelledSum = BPOS.TotalsCancelledAmt,
            };
            batchTotals.Receipt = GetLastReceipt(false);
            StopBPOS();
            return batchTotals;
        }

        public void PrintBatchTotals(byte pMerchantId = 0)
        {
            if (!this.StartBPOS())
                return;
            BPOS.PrintBatchTotals(pMerchantId);
            this.WaitResponse();
            StopBPOS();
        }

        private bool WaitResponse(int pTime = 90)
        {
            if (BPOS == null)
                return false;
            for (int index = 0; BPOS.LastResult == (byte)2 && index < pTime; ++index)
                Thread.Sleep(1000);
            return BPOS.LastResult == 0;
        }

        public async Task<BatchTotals> GetBatchTotals(byte pMerchantId = 0)
        {
            if (!this.StartBPOS())
                return (BatchTotals)null;
            BPOS.GetBatchTotals(pMerchantId);
            this.WaitResponse();
            BatchTotals batchTotals = new BatchTotals()
            {
                DebitCount = BPOS.TotalsDebitNum,
                DebitSum = BPOS.TotalsDebitAmt,
                CreditCount = BPOS.TotalsCreditNum,
                CreditSum = BPOS.TotalsCreditAmt,
                CencelledCount = BPOS.TotalsCancelledNum,
                CencelledSum = BPOS.TotalsCancelledAmt,
                Receipt = GetLastReceipt(false)
            };
            StopBPOS();
            return batchTotals;
        }

        public void PrintBatchJournal(byte pMerchantId = 0)
        {
            if (!this.StartBPOS())
                return;
            BPOS.PrintBatchJournal(pMerchantId);
            StopBPOS();
        }

        public void PrintLastSettleCopy(byte pMerchantId = 0)
        {
            if (!this.StartBPOS())
                return;
            BPOS.PrintLastSettleCopy(pMerchantId);
            StopBPOS();
        }

        public override Payment Purchase(decimal pAmount, decimal pCash = 0, int IdWorkPlace = 0, bool pIsCashBack = false)
        {
            return PurchaseAsync(pAmount, pCash, IdWorkPlace, pIsCashBack).Result;
        }

        public Task<Payment> PurchaseAsync(decimal amount, decimal pCash, int pIdWorkPlace = 0,bool pIsCashBack=false)
        {
            uint Sum = (uint)(amount * 100m), SumCash = (uint)(pCash * 100m);
            try
            {
                byte MechantId = GetMechantIdByIdWorkPlace(pIdWorkPlace);
                if (!this.StartBPOS())
                    return Task.FromResult<Payment>(new Payment() { IsSuccess = false });
                CancelRequested = false;
                if (SumCash > 0)
                {
                    string ScenarioData = CodeBank == eBank.PrivatBank ?
                        $"<ActionScenarioRequest><Action>CashBack</Action><Amount>{Sum}</Amount><AddAmount>{SumCash}</AddAmount><MerchIdx>{MechantId}</MerchIdx></ActionScenarioRequest>" :
                        $"<ActionScenarioRequest><Action>CashBack</Action><Amount>{Sum}</Amount><CashAmount>{SumCash}</CashAmount><MerchantId>{MechantId}</MerchantId></ActionScenarioRequest>";
                    BPOS.StartScenario(CodeBank == eBank.PrivatBank ? 2u : 6u, ScenarioData);
                }
                else
                    BPOS.Purchase(Sum, 0, MechantId);

                OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.WaitingForCard });
                Payment result = WaitPosRespone(120, pIsCashBack, Sum);
                if (Logger != null)
                    LoggerExtensions.LogDebug(Logger, "[Ingenico] Purches " + JsonConvert.SerializeObject(result));
                if (result.IsSuccess)
                {
                    BPOS.Confirm();
                    Payment Res = WaitPosRespone(20);
                    if (!Res.IsSuccess)
                        result = Res;
                }

                if (OnResponse != null) OnResponse((IPosResponse)new PayPosResponse() { Response = result });

                //if (result.IsSuccess)
                //   result.Receipt = GetLastReceipt(false);

                StopBPOS();
                return Task.FromResult<Payment>(result);
            }
            catch (Exception ex)
            {
                LoggerExtensions.LogError((ILogger)this.Logger, ex, "[Ingenico] " + ex.Message);
                return Task.FromResult<Payment>(new Payment() { IsSuccess = false });
            }
            finally
            {
                BPOS = (BPOS1LibClass)null;
            }
        }

        public override IEnumerable<string> GetLastReceipt() { return GetLastReceipt(); }

        public List<string> GetLastReceipt(bool IsStart = true)
        {
            if (IsStart)
                if (!this.StartBPOS())
                    return (List<string>)null;
            BPOS.ReqCurrReceipt();
            this.WaitResponse();
            string receipt = BPOS.Receipt;
            if (IsStart)
                StopBPOS();
            return this.ParseReceipt(receipt);
        }

        private string GetString(string input) => string.IsNullOrWhiteSpace(input) || !ConfigurationBinder.GetValue<bool>(this.Configuration, $"{KeyPrefix}Use1252Encoder", false) ? input : Encoding.UTF8.GetString(Encoding.Convert(this.Encoding1251, Encoding.UTF8, this.Encoding1252.GetBytes(input)));

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
            int num = 120;
            while (BPOS != null)
            {
                Thread.Sleep(100);
                if (num == 0)
                    return false;
                --num;
            }
            BPOS = new BPOS1LibClass();
            BPOS.SetErrorLang((byte)1);
            BPOS.useLogging((byte)1, "ingenico.log");
            if (string.IsNullOrEmpty(IP) || IpPort == 0)
                BPOS.CommOpen(Port, BaudRate);
            else
                BPOS.CommOpenTCP(IP, IpPort.ToString());
            return true;
        }

        private static void StopBPOS()
        {
            BPOS?.CommClose();
            Task.Delay(100);
            BPOS = null;
        }

        public void Dispose()
        {
            BPOS?.CommClose();
            BPOS = null;
        }
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
        public Payment Response { get; set; }

        public List<string> Errors { get; set; }

        public PayPosResponse() => this.Errors = new List<string>();
    }

}