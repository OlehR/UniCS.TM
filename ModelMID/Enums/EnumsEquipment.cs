using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public enum ePosStatus
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
}
