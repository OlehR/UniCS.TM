using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public enum eStatusPos
    {
        StatusCodeIsNotAvailable = 0,
        CardWasRead = 1,
        UsedAChipCard = 2,
        AuthorizationInProgress = 3,
        WaitingForCashierAction = 4,
        PrintingReceipt = 5,
        PinEntryIsNeeded = 6,
        CardWasRemoved = 7,
        EMVMultiAids = 8,
        WaitingForCard = 9,
        InProgress = 10,
        CorrectTransaction = 11,
        PinInputWaitKey = 12,
        PinInputBackspacePressed = 13,
        PinInputKeyPressed = 14,
        ErrorOpeningCOMPort = 15,
        NeedToOpenCOMPort = 16,
        ErrorConnectingWithTerminal = 17,
        TerminalReturnedAnError = 18,
        SuccessfullyFulfilled = 19,
        Error = 20,
        AuthorizationRejectedNoPayment = 21,
        WrongPIN = 22,
        NotEnoughMoney = 23,
        GeneralError = 24,
        TransactionCanceledByUser = 25,
        EMVDecline = 26,
        TransactionLogIsFullNeedCloseBatch = 27,
        NoConnectionWithHost = 28,
        NoPaperInPrinter = 29,
        ErrorCryptoKeys = 30,
        CardReaderIsNotConnected = 31,
        TransactionIsAlreadyComplete = 32,
        ApprovedAndCompleted = 33,
        TheCardIsInOrderThereIsNoReasonToRefuse = 34,
        AuthorizationDenied = 35,
        UnregisteredTradingPoint = 36,
        AuthorizationRejectedWithdrawTheCardAtTheBanksRequest = 37,
        CommonErrorNeedToRepeat = 38,
        InvalidTransactionNetworkErrorNeedToRepeat = 39,
        IncorrectAmountEntered = 40,
        InvalidCardNumber = 41,
        BankNodeIsNotFoundOnTheNetwork = 42,
        CanceledByTheClient = 43,
        ActionsNotCompletedDidNotMatchData = 44,
        NoResponseFileIsTemporarilyUnavailable = 45,
        WrongFormatNeedToRepeat = 46,
        TheIssuerIsNotFoundInThePaymentSystem = 47,
        PartiallyCompleted = 48,
        TheValidityPeriodOfTheCardHasExpiredTheCardHasBeenWithdrawnAtTheBanksRequest = 49,
        ForbiddenCardRemove = 50,
        WithdrawnByTheIssuerRemovedFromTheCardAndContactedByTheAcquirer = 51,
        ThereAreNoAttemptsToEnterThePINRemoveTheCard = 52,
        NoClientsCreditAccount = 53,
        CardIslostRemoved = 54,
        CardIsStolenRemoved = 55,
        NoSettlementSpecifiedClienAccount = 56,
        ThereIsNoCumulativeAccountOfTheClient = 57,
        TheExpirationDateOfTheCardExpires = 58,
        ThisTransactionTypeIsNotProvidedForTheGivenCard = 59,
        ThisTypeOfTransactionIsNotProvidedForPOSTerminal = 60,
        TheAamountOfAuthorizationExceededTheExpenseLimitOnTheCard = 61,
        IncorrectServiceCodeForbiddenCardCanNotBeSeized = 62,
        TheAmountOfTheCancellationAuthorizationIsDifferentFromTheAmountOfTheOriginalAuthorization = 63,
        TheExpenseLimitExpiredOnTheAccount = 64,
        TheCardIsVoidCanNotBeSeized = 65,
        CardIsWithdrawnFromATM = 66,
        ItIsTooLateToReceiveAnAnswerFromTheNetworkItIsNecessaryToRepeat = 67,
        TheNumberOfIncorrectlyEnteredPINsExceededTheAmountDischarged = 68,
        ActionsAreNotCompletedIncompleteDataItIsNecessaryToRollbackOrRepeat = 69,
        NoAccount = 70,
        AlreadyCanceledWhenTurnedOn = 71,
        GeneralNetworkErrorIncorrectData = 72,
        RemoteNetworkErrorOrPINEncryption = 73,
        TimeoutWhenConnectedWithTheIssuersNodeOrWrongCVVOrCacheIsNotApprovedTheCashbackSumLimitIsExceeded = 74,
        ThePINVerificationTransactionIsUnsuccessfulNetworkError = 75,
        PINCanNotBeCheckedNetworkError = 76,
        PINEncryptionErrorNetworkError = 77,
        IdentificationErrorIsANetworkError = 78,
        NoConnectionWithTheBankByTheIssuerNetworkError = 79,
        UnsuccessfulRequestRoutingIsNotPossibleNetworkError = 80,
        TheTransactionCanNotBeCompletedTheIssuerDeclineAuthorizationDueToAViolationOfTheRules = 81,
        DuplicationOfTransmissionNetworkError = 82,
        GeneralSystemMalfunction = 83,
        UnableToSendEncryptedMessage = 84
    }

    public enum eStatusRRO
    {
        NotDefine = 0,

        /// <summary>
        /// Очікування команди
        /// </summary>
        Wait,
        /// <summary>
        /// Очікуєм відповідь від РРО
        /// </summary>
        WaitAnswer,
        /// <summary>
        /// Обробка результату
        /// </summary>
        ParseResult,
        /// <summary>
        /// Успішне виконаання
        /// </summary>
        OK,
        /// <summary>
        /// Виникла помилка під час операції
        /// </summary>

        StartPrintReceipt,
        AddWares,
        AddPay,
        ClosePort,
        TryOpenPort,
        OpenPort,
        Error
    }

    public enum ePosTypeError
    {
        NotDefine = 0,
        NoError,
        /// <summary>
        /// Pin,Limit?, можна пробувати повторно запустити транзакції
        /// </summary>
        Card,
        /// <summary>
        /// Повторна спроба не можлива без усуня проблеми.
        /// </summary>
        PosTerminal,
        /// <summary>
        /// Відсутній зв'язок з банком і тд.,
        /// Можна спробувати повторно запустити оплату.
        /// </summary>
        BankConect,
        /// <summary>
        /// Треба зменшити кількість таких статусів.
        /// </summary>
        Other
    }

    public enum eStateEquipment { On = 0, Init, Off, Error, Process }

    /// <summary>
    /// Типи обладнання (ваги, касові апарати)
    /// </summary>
    public enum eTypeEquipment
    {
        NotDefined,
        Scaner,
        Scale,
        ControlScale,
        BankTerminal,
        Signal,
        RRO
    }

    /// <summary>
    /// Підтримуване обладнання (Magellan,Exelio,Ingenico,...)
    /// </summary>
    public enum eModelEquipment
    {
        NotDefined,
        MagellanScaner,
        MagellanScale,
        ScaleModern,
        SignalFlagModern,
        Ingenico,
        VirtualBankPOS,
        ExellioFP,
        //Exellio,
        pRRO_SG,
        Maria,
        pRRo_WebCheck,
        VirtualRRO,
        VirtualScale,
        VirtualScaner,
        /// <summary>
        /// Контрольна вага на основі основної.
        /// </summary>
        VirtualControlScale

    }


    //public enum eDirectMoveCash { In=1,Out=0}

    public enum eTypeOperarionRRO
    { MoveCash, PrintReceipt }

    static class ModelMethods
    {
        /*public static ePosStatus GetPosStatusFromStatus(this Front.Equipments.Ingenico.ePosStatus pModel)
        {
            return (ePosStatus)(int)pModel;
        }*/

        public static eTypeEquipment GetTypeEquipment(this eModelEquipment pModel)
        {
            switch (pModel)
            {
                case eModelEquipment.MagellanScaner:
                case eModelEquipment.VirtualScaner:
                    return eTypeEquipment.Scaner;
                case eModelEquipment.MagellanScale:
                case eModelEquipment.VirtualScale:
                    return eTypeEquipment.Scale;
                case eModelEquipment.ScaleModern:
                case eModelEquipment.VirtualControlScale:
                    return eTypeEquipment.ControlScale;
                case eModelEquipment.SignalFlagModern:
                    return eTypeEquipment.Signal;
                case eModelEquipment.Ingenico:
                case eModelEquipment.VirtualBankPOS:
                    return eTypeEquipment.BankTerminal;
                case eModelEquipment.ExellioFP:
                //case eModelEquipment.Exellio:
                case eModelEquipment.pRRO_SG:
                case eModelEquipment.pRRo_WebCheck:
                case eModelEquipment.Maria:
                case eModelEquipment.VirtualRRO:

                    return eTypeEquipment.RRO;
                default:
                    return eTypeEquipment.NotDefined;
            }
        }

        /// <summary>
        /// треба обробити всі статуси.
        /// </summary>
        /// <param name="pStatus"></param>
        /// <returns></returns>
        public static ePosTypeError GetPosTypeError(this eStatusPos pStatus)
        {
            switch (pStatus)
            {
                case eStatusPos.StatusCodeIsNotAvailable:
                    return ePosTypeError.NotDefine;
                case eStatusPos.CardWasRead:
                case eStatusPos.UsedAChipCard:
                case eStatusPos.AuthorizationInProgress:
                case eStatusPos.WaitingForCashierAction:
                case eStatusPos.PrintingReceipt:
                case eStatusPos.PinEntryIsNeeded:
                case eStatusPos.CardWasRemoved:
                case eStatusPos.WaitingForCard:
                case eStatusPos.InProgress:
                case eStatusPos.CorrectTransaction:
                case eStatusPos.PinInputWaitKey:
                case eStatusPos.PinInputBackspacePressed:
                case eStatusPos.PinInputKeyPressed:
                    return ePosTypeError.NoError;

                case eStatusPos.WrongPIN:
                case eStatusPos.NotEnoughMoney:
                case eStatusPos.TransactionCanceledByUser:
                case eStatusPos.NoClientsCreditAccount:
                case eStatusPos.CardIslostRemoved:
                case eStatusPos.CardIsStolenRemoved:
                case eStatusPos.IncorrectAmountEntered:
                case eStatusPos.InvalidCardNumber:
                    return ePosTypeError.Card;


                case eStatusPos.ErrorConnectingWithTerminal:
                case eStatusPos.TerminalReturnedAnError:
                case eStatusPos.ErrorOpeningCOMPort:
                case eStatusPos.NeedToOpenCOMPort:
                case eStatusPos.Error:
                case eStatusPos.GeneralError:
                case eStatusPos.NoPaperInPrinter:
                    return ePosTypeError.PosTerminal;
                default:
                    return ePosTypeError.Other;
                    /*
                                        EMVMultiAids = 8,

                            SuccessfullyFulfilled = 19,

                            AuthorizationRejectedNoPayment = 21,

                             = 25,
                            EMVDecline = 26,
                            TransactionLogIsFullNeedCloseBatch = 27,
                            NoConnectionWithHost = 28,
                             = 29,
                            ErrorCryptoKeys = 30,
                            CardReaderIsNotConnected = 31,
                            TransactionIsAlreadyComplete = 32,
                            ApprovedAndCompleted = 33,
                            TheCardIsInOrderThereIsNoReasonToRefuse = 34,
                            AuthorizationDenied = 35,
                            UnregisteredTradingPoint = 36,
                            AuthorizationRejectedWithdrawTheCardAtTheBanksRequest = 37,
                            CommonErrorNeedToRepeat = 38,
                            InvalidTransactionNetworkErrorNeedToRepeat = 39,
                             = 40,
                             = 41,
                            BankNodeIsNotFoundOnTheNetwork = 42,
                            CanceledByTheClient = 43,
                            ActionsNotCompletedDidNotMatchData = 44,
                            NoResponseFileIsTemporarilyUnavailable = 45,
                            WrongFormatNeedToRepeat = 46,
                            TheIssuerIsNotFoundInThePaymentSystem = 47,
                            PartiallyCompleted = 48,
                            TheValidityPeriodOfTheCardHasExpiredTheCardHasBeenWithdrawnAtTheBanksRequest = 49,
                            ForbiddenCardRemove = 50,
                            WithdrawnByTheIssuerRemovedFromTheCardAndContactedByTheAcquirer = 51,
                            ThereAreNoAttemptsToEnterThePINRemoveTheCard = 52,
                             = 53,
                             = 54,
                             = 55,
                            NoSettlementSpecifiedClienAccount = 56,
                            ThereIsNoCumulativeAccountOfTheClient = 57,
                            TheExpirationDateOfTheCardExpires = 58,
                            ThisTransactionTypeIsNotProvidedForTheGivenCard = 59,
                            ThisTypeOfTransactionIsNotProvidedForPOSTerminal = 60,
                            TheAamountOfAuthorizationExceededTheExpenseLimitOnTheCard = 61,
                            IncorrectServiceCodeForbiddenCardCanNotBeSeized = 62,
                            TheAmountOfTheCancellationAuthorizationIsDifferentFromTheAmountOfTheOriginalAuthorization = 63,
                            TheExpenseLimitExpiredOnTheAccount = 64,
                            TheCardIsVoidCanNotBeSeized = 65,
                            CardIsWithdrawnFromATM = 66,
                            ItIsTooLateToReceiveAnAnswerFromTheNetworkItIsNecessaryToRepeat = 67,
                            TheNumberOfIncorrectlyEnteredPINsExceededTheAmountDischarged = 68,
                            ActionsAreNotCompletedIncompleteDataItIsNecessaryToRollbackOrRepeat = 69,
                            NoAccount = 70,
                            AlreadyCanceledWhenTurnedOn = 71,
                            GeneralNetworkErrorIncorrectData = 72,
                            RemoteNetworkErrorOrPINEncryption = 73,
                            TimeoutWhenConnectedWithTheIssuersNodeOrWrongCVVOrCacheIsNotApprovedTheCashbackSumLimitIsExceeded = 74,
                            ThePINVerificationTransactionIsUnsuccessfulNetworkError = 75,
                            PINCanNotBeCheckedNetworkError = 76,
                            PINEncryptionErrorNetworkError = 77,
                            IdentificationErrorIsANetworkError = 78,
                            NoConnectionWithTheBankByTheIssuerNetworkError = 79,
                            UnsuccessfulRequestRoutingIsNotPossibleNetworkError = 80,
                            TheTransactionCanNotBeCompletedTheIssuerDeclineAuthorizationDueToAViolationOfTheRules = 81,
                            DuplicationOfTransmissionNetworkError = 82,
                            GeneralSystemMalfunction = 83,
                            UnableToSendEncryptedMessage = 84


                    */


            }

        }



        /// <summary>
        /// треба обробити всі статуси.
        /// </summary>
        /// <param name="pStatus"></param>
        /// <returns></returns>
        public static eStateEquipment GetStateEquipment(this eStatusPos pStatus)
        {
            switch (pStatus)
            {
                case eStatusPos.StatusCodeIsNotAvailable:
                    return eStateEquipment.Process;
                case eStatusPos.CardWasRead:
                case eStatusPos.UsedAChipCard:
                case eStatusPos.AuthorizationInProgress:
                case eStatusPos.WaitingForCashierAction:
                case eStatusPos.PrintingReceipt:
                case eStatusPos.PinEntryIsNeeded:
                case eStatusPos.CardWasRemoved:
                case eStatusPos.WaitingForCard:
                case eStatusPos.InProgress:
                case eStatusPos.CorrectTransaction:
                case eStatusPos.PinInputWaitKey:
                case eStatusPos.PinInputBackspacePressed:
                case eStatusPos.PinInputKeyPressed:
                    return eStateEquipment.Process;

                case eStatusPos.WrongPIN:
                case eStatusPos.NotEnoughMoney:
                case eStatusPos.TransactionCanceledByUser:
                case eStatusPos.NoClientsCreditAccount:
                case eStatusPos.CardIslostRemoved:
                case eStatusPos.CardIsStolenRemoved:
                case eStatusPos.IncorrectAmountEntered:
                case eStatusPos.InvalidCardNumber:
                    return eStateEquipment.Process;


                case eStatusPos.ErrorConnectingWithTerminal:
                case eStatusPos.TerminalReturnedAnError:
                case eStatusPos.ErrorOpeningCOMPort:
                case eStatusPos.NeedToOpenCOMPort:
                case eStatusPos.Error:
                case eStatusPos.GeneralError:
                case eStatusPos.NoPaperInPrinter:
                    return eStateEquipment.Error;
                default:
                    return eStateEquipment.On;
            }

        }
    }
}
   
