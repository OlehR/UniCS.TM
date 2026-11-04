using Microsoft.Extensions.Configuration;
//using ModernExpo.SelfCheckout.Entities.Pos;
//using Front.Equipments.Ingenico;
using System;
using System.Collections.Generic;
using ModelMID;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using Utils;

namespace Front.Equipments
{
    public class BankTerminal : Equipment
    {       

        public Action<StatusEquipment> OnStatus { get; set; }
        protected static bool CancelRequested;

        protected byte MerchantId;
        List<Merchants> Merchants = [];
        public BankTerminal() { }
        public BankTerminal(Equipment pEquipment, IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefine, ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) : 
            base(pEquipment, pConfiguration, pModelEquipment, pLoggerFactory) 
        {
            pConfiguration.GetSection($"{KeyPrefix}MerchantIds").Bind(Merchants);
            MerchantId = Convert.ToByte(Configuration[$"{KeyPrefix}MerchanId"]);
            OnStatus += pActionStatus;
        }

        public eBank CodeBank { get; set; } = eBank.NotDefine;
        public string TranslateCodeBank { get => CodeBank.GetDescription(); }

        virtual public BatchTotals PrintZ(int IdWorkPlace = 0) { throw new NotImplementedException(); }
        virtual public BatchTotals PrintX(int IdWorkPlace = 0) { throw new NotImplementedException(); }
        public virtual Payment Purchase(decimal pAmount, decimal pCash = 0, int IdWorkPlace = 0, bool pIsCashBack = false) { throw new NotImplementedException(); }
        public virtual Payment Refund(decimal pAmount, string pRRN, int IdWorkPlace = 0, bool pIsCashBack = false) { throw new NotImplementedException(); }

        public virtual IEnumerable<string> GetLastReceipt() { throw new NotImplementedException(); }

        public virtual void Cancel() { CancelRequested = true; }     

        protected void SetStatus(eStatusPos pStatus)
        {
            ActionStatus?.Invoke(new PosStatus() { Status = pStatus, ModelEquipment = Model, State = (int)pStatus, TextState = pStatus.ToString() });
        }

        public byte GetMechantIdByIdWorkPlace(int pIdWorkPlace, bool pIsCashBack = false)
        {
            if(Merchants!=null)
            {
                if(pIsCashBack)
                {
                    var r = Merchants.Where(el => el.IdWorkplace == -pIdWorkPlace).FirstOrDefault();
                    if (r != null)
                        return r.MerchantId;
                }
                var res=Merchants.Where(el=> el.IdWorkplace == pIdWorkPlace).FirstOrDefault();
                if (res != null)
                    return res.MerchantId;
            }
            return MerchantId;
        }

        /// <summary>
        /// Статус для екрана покупця за кодом відповіді хоста/термінала (спільний для Ingenico (BPOS) і JSON-протоколу ПриватБанку)
        /// </summary>
        protected static eStatusPos? GetStatusByResponseCode(uint pResponseCode) => pResponseCode switch
        {
            0 => eStatusPos.ApprovedAndCompleted,
            1 or 2 => eStatusPos.AuthorizationDenied,
            3 => eStatusPos.UnregisteredTradingPoint,
            4 or 7 => eStatusPos.AuthorizationRejectedWithdrawTheCardAtTheBanksRequest,
            5 => eStatusPos.AuthorizationRejectedNoPayment,
            6 => eStatusPos.CommonErrorNeedToRepeat,
            12 => eStatusPos.InvalidTransactionNetworkErrorNeedToRepeat,
            13 => eStatusPos.IncorrectAmountEntered,
            14 => eStatusPos.InvalidCardNumber,
            15 => eStatusPos.BankNodeIsNotFoundOnTheNetwork,
            17 => eStatusPos.CanceledByTheClient,
            21 => eStatusPos.ActionsNotCompletedDidNotMatchData,
            28 => eStatusPos.NoResponseFileIsTemporarilyUnavailable,
            30 => eStatusPos.WrongFormatNeedToRepeat,
            31 => eStatusPos.TheIssuerIsNotFoundInThePaymentSystem,
            32 => eStatusPos.PartiallyCompleted,
            33 => eStatusPos.TheValidityPeriodOfTheCardHasExpiredTheCardHasBeenWithdrawnAtTheBanksRequest,
            36 => eStatusPos.ForbiddenCardRemove,
            37 => eStatusPos.WithdrawnByTheIssuerRemovedFromTheCardAndContactedByTheAcquirer,
            38 => eStatusPos.ThereAreNoAttemptsToEnterThePINRemoveTheCard,
            39 => eStatusPos.NoClientsCreditAccount,
            41 => eStatusPos.CardIslostRemoved,
            43 => eStatusPos.CardIsStolenRemoved,
            51 => eStatusPos.NotEnoughMoney,
            52 => eStatusPos.NoSettlementSpecifiedClienAccount,
            53 => eStatusPos.ThereIsNoCumulativeAccountOfTheClient,
            54 => eStatusPos.TheExpirationDateOfTheCardExpires,
            55 => eStatusPos.WrongPIN,
            57 => eStatusPos.ThisTransactionTypeIsNotProvidedForTheGivenCard,
            58 => eStatusPos.ThisTypeOfTransactionIsNotProvidedForPOSTerminal,
            61 => eStatusPos.TheAamountOfAuthorizationExceededTheExpenseLimitOnTheCard,
            62 => eStatusPos.IncorrectServiceCodeForbiddenCardCanNotBeSeized,
            64 => eStatusPos.TheAmountOfTheCancellationAuthorizationIsDifferentFromTheAmountOfTheOriginalAuthorization,
            65 => eStatusPos.TheExpenseLimitExpiredOnTheAccount,
            66 => eStatusPos.TheCardIsVoidCanNotBeSeized,
            67 => eStatusPos.CardIsWithdrawnFromATM,
            68 => eStatusPos.ItIsTooLateToReceiveAnAnswerFromTheNetworkItIsNecessaryToRepeat,
            75 or 76 => eStatusPos.TheNumberOfIncorrectlyEnteredPINsExceededTheAmountDischarged,
            77 => eStatusPos.ActionsAreNotCompletedIncompleteDataItIsNecessaryToRollbackOrRepeat,
            78 => eStatusPos.NoAccount,
            79 => eStatusPos.AlreadyCanceledWhenTurnedOn,
            80 => eStatusPos.GeneralNetworkErrorIncorrectData,
            81 => eStatusPos.RemoteNetworkErrorOrPINEncryption,
            82 => eStatusPos.TimeoutWhenConnectedWithTheIssuersNodeOrWrongCVVOrCacheIsNotApprovedTheCashbackSumLimitIsExceeded,
            83 => eStatusPos.ThePINVerificationTransactionIsUnsuccessfulNetworkError,
            85 => eStatusPos.TheCardIsInOrderThereIsNoReasonToRefuse,
            86 => eStatusPos.PINCanNotBeCheckedNetworkError,
            88 => eStatusPos.PINEncryptionErrorNetworkError,
            89 => eStatusPos.IdentificationErrorIsANetworkError,
            91 => eStatusPos.NoConnectionWithTheBankByTheIssuerNetworkError,
            92 => eStatusPos.UnsuccessfulRequestRoutingIsNotPossibleNetworkError,
            93 => eStatusPos.TheTransactionCanNotBeCompletedTheIssuerDeclineAuthorizationDueToAViolationOfTheRules,
            94 => eStatusPos.DuplicationOfTransmissionNetworkError,
            96 => eStatusPos.GeneralSystemMalfunction,
            119 => eStatusPos.UnableToSendEncryptedMessage,
            1000 => eStatusPos.GeneralError,
            1001 => eStatusPos.TransactionCanceledByUser,
            1002 => eStatusPos.EMVDecline,
            1003 => eStatusPos.TransactionLogIsFullNeedCloseBatch,
            1004 => eStatusPos.NoConnectionWithHost,
            1005 => eStatusPos.NoPaperInPrinter,
            1006 => eStatusPos.ErrorCryptoKeys,
            1007 => eStatusPos.CardReaderIsNotConnected,
            1008 => eStatusPos.TransactionIsAlreadyComplete,
            _ => null
        };

        protected void InvokeLastStatusMsg(byte LastStatMsgCode)
        {
            if (LastStatMsgCode == 0) return;
            eStatusPos StatusPos = eStatusPos.StatusCodeIsNotAvailable;
            if (Enum.IsDefined(typeof(eStatusPos),(int) LastStatMsgCode))
            {
                StatusPos = (eStatusPos)LastStatMsgCode;
                OnStatus?.Invoke(new PosStatus() { Status = StatusPos });
            }            
        }
    }

    public class Merchants
    {
        public int IdWorkplace { get; set; }
        public byte MerchantId { get; set; }
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
}
