using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Front.Equipments
{
    public enum DeviceConnectionStatus
    {
        NotConnected = 1,
        InitializationError,
        Enabled,
        Disabled
    }

    public enum eStatusPos
    {
        [Description("Код статусу: недоступний")]
        StatusCodeIsNotAvailable = 0,
        [Description("Считування картки")]
        CardWasRead = 1,
        [Description("Використана карта з чіпом")]
        UsedAChipCard = 2,
        [Description("Виконується авторизація")]
        AuthorizationInProgress = 3,
        [Description("Очікування дії касира")]
        WaitingForCashierAction = 4,
        [Description("Друк чека")]
        PrintingReceipt = 5,
        [Description("Потрібен PIN-код")]
        PinEntryIsNeeded = 6,
        [Description("Картка була видалена")]
        CardWasRemoved = 7,
        [Description("EMVMultiAids")]
        EMVMultiAids = 8,
        [Description("Очікування карти")]
        WaitingForCard = 9,
        [Description("В процесі")]
        InProgress = 10,
        [Description("Правильна трансакція")]
        CorrectTransaction = 11,
        [Description("Очікування введення PIN-коду")]
        PinInputWaitKey = 12,
        [Description("Pin Input Backspace Натиснуто")]
        PinInputBackspacePressed = 13,
        [Description("Натиснута клавіша введення PIN-коду")]
        PinInputKeyPressed = 14,
        [Description("Помилка відкриття COM-порту")]
        ErrorOpeningCOMPort = 15,
        [Description("Необхідно відкрити COM-порт")]
        NeedToOpenCOMPort = 16,
        [Description("Помилка підключення до терміналу")]
        ErrorConnectingWithTerminal = 17,
        [Description("Термінал повернув помилку")]
        TerminalReturnedAnError = 18,
        [Description("Успішно виконано")]
        SuccessfullyFulfilled = 19,
        [Description("Помилка")]
        Error = 20,
        [Description("Авторизацію відхилено. Немає платежу")]
        AuthorizationRejectedNoPayment = 21,
        [Description("Неправильний PIN-код")]
        WrongPIN = 22,
        [Description("Недостатньо грошей")]
        NotEnoughMoney = 23,
        [Description("Загальна помилка")]
        GeneralError = 24,
        [Description("Трансакцію скасовано користувачем")]
        TransactionCanceledByUser = 25,
        [Description("Зниження EMV")]
        EMVDecline = 26,
        [Description("Журнал транзакцій повний, потрібно закрити пакет")]
        TransactionLogIsFullNeedCloseBatch = 27,
        [Description("Немає з'єднання з хостом")]
        NoConnectionWithHost = 28,
        [Description("У принтері немає паперу")]
        NoPaperInPrinter = 29,
        [Description("Помилка кодування ключа")]
        ErrorCryptoKeys = 30,
        [Description("Не підключено пристрій для читання карток")]
        CardReaderIsNotConnected = 31,
        [Description("Трансакція вже завершена")]
        TransactionIsAlreadyComplete = 32,
        [Description("Затверджено та завершено")]
        ApprovedAndCompleted = 33,
        [Description("Картка в порядку, немає підстав для відмови")]
        TheCardIsInOrderThereIsNoReasonToRefuse = 34,
        [Description("Авторизацію відхилено")]
        AuthorizationDenied = 35,
        [Description("Незареєстрована торгова точка")]
        UnregisteredTradingPoint = 36,
        [Description("Авторизацію відхилено. Зніміть картку на вимогу банку")]
        AuthorizationRejectedWithdrawTheCardAtTheBanksRequest = 37,
        [Description("Поширена помилка, потрібно повторити")]
        CommonErrorNeedToRepeat = 38,
        [Description("Недісна трансакція через помилку мережі, потрібно повторити")]
        InvalidTransactionNetworkErrorNeedToRepeat = 39,
        [Description("Введено неправильну суму")]
        IncorrectAmountEntered = 40,
        [Description("Недійсний номер картки")]
        InvalidCardNumber = 41,
        [Description("Банківські вузли не знайдено в мережі")]
        BankNodeIsNotFoundOnTheNetwork = 42,
        [Description("Скасовано Клієнтом")]
        CanceledByTheClient = 43,
        [Description("Невиконані дії не відповідають даним")]
        ActionsNotCompletedDidNotMatchData = 44,
        [Description("Немає файлів відповідей, які тимчасово недоступні")]
        NoResponseFileIsTemporarilyUnavailable = 45,
        [Description("Неправильний формат, потрібно повторити")]
        WrongFormatNeedToRepeat = 46,
        [Description("Емітент не знайдено в платіжній системі")]
        TheIssuerIsNotFoundInThePaymentSystem = 47,
        [Description("Частково завершено")]
        PartiallyCompleted = 48,
        [Description("Закінчився термін дії картки. Картка була вилучена на вимогу банку")]
        TheValidityPeriodOfTheCardHasExpiredTheCardHasBeenWithdrawnAtTheBanksRequest = 49,
        [Description("Заборонене видалення картки")]
        ForbiddenCardRemove = 50,
        [Description("Знято емітентом, видалено з картки та зв’язано з еквайром")]
        WithdrawnByTheIssuerRemovedFromTheCardAndContactedByTheAcquirer = 51,
        [Description("Немає спроб ввести PIN-код. Витягніть картку")]
        ThereAreNoAttemptsToEnterThePINRemoveTheCard = 52,
        [Description("Немає кредитного рахунку клієнта")]
        NoClientsCreditAccount = 53,
        [Description("Картка втрачена. Вилучено")]
        CardIslostRemoved = 54,
        [Description("Викрадена картка видалена")]
        CardIsStolenRemoved = 55,
        [Description("Рахунок клієнта без розрахунків")]
        NoSettlementSpecifiedClienAccount = 56,
        [Description("Немає сукупного рахунку клієнта")]
        ThereIsNoCumulativeAccountOfTheClient = 57,
        [Description("Закінчився термін дії картки")]
        TheExpirationDateOfTheCardExpires = 58,
        [Description("Цей тип транзакції не передбачений для даної картки")]
        ThisTransactionTypeIsNotProvidedForTheGivenCard = 59,
        [Description("Цей тип транзакції не передбачений для POS-терміналу")]
        ThisTypeOfTransactionIsNotProvidedForPOSTerminal = 60,
        [Description("Сума авторизації перевищила ліміт витрат на картці")]
        TheAamountOfAuthorizationExceededTheExpenseLimitOnTheCard = 61,
        [Description("Неправильний сервісний код. Заборонена картка не може бути конфіскована")]
        IncorrectServiceCodeForbiddenCardCanNotBeSeized = 62,
        [Description("Сума авторизації на скасування відрізняється від суми оригінальної авторизації")]
        TheAmountOfTheCancellationAuthorizationIsDifferentFromTheAmountOfTheOriginalAuthorization = 63,
        [Description("На рахунку закінчився ліміт витрат")]
        TheExpenseLimitExpiredOnTheAccount = 64,
        [Description("Картка недійсна, не може бути конфіскована")]
        TheCardIsVoidCanNotBeSeized = 65,
        [Description("Картка вилучена з банкомату")]
        CardIsWithdrawnFromATM = 66,
        [Description("Відповідь з мережі надходить пізно. Необхідно повторити")]
        ItIsTooLateToReceiveAnAnswerFromTheNetworkItIsNecessaryToRepeat = 67,
        [Description("Кількість неправильно введених PIN-кодів перевищила списану суму")]
        TheNumberOfIncorrectlyEnteredPINsExceededTheAmountDischarged = 68,
        [Description("Дії не виконані. Неповні дані. Необхідно відкотити або повторити")]
        ActionsAreNotCompletedIncompleteDataItIsNecessaryToRollbackOrRepeat = 69,
        [Description("Немає облікового запису")]
        NoAccount = 70,
        [Description("Уже скасовано після ввімкнення")]
        AlreadyCanceledWhenTurnedOn = 71,
        [Description("Загальна помилка мережі. Неправильні дані")]
        GeneralNetworkErrorIncorrectData = 72,
        [Description("Помилка віддаленої мережі або PIN-шифрування")]
        RemoteNetworkErrorOrPINEncryption = 73,
        [Description("Тайм-аут під час з’єднання з вузлом емітента або неправильний CVVO або кеш не затверджено Обмеження суми повернення коштів перевищено")]
        TimeoutWhenConnectedWithTheIssuersNodeOrWrongCVVOrCacheIsNotApprovedTheCashbackSumLimitIsExceeded = 74,
        [Description("Транзакція перевірки PIN-коду не вдалася. Помилка мережі")]
        ThePINVerificationTransactionIsUnsuccessfulNetworkError = 75,
        [Description("PIN-код неможливо перевірити. Помилка мережі")]
        PINCanNotBeCheckedNetworkError = 76,
        [Description("Помилка шифрування PIN-коду. Помилка мережі")]
        PINEncryptionErrorNetworkError = 77,
        [Description("Помилка ідентифікації. Помилка мережі")]
        IdentificationErrorIsANetworkError = 78,
        [Description("Немає з’єднання з банком через емітент. Помилка мережі")]
        NoConnectionWithTheBankByTheIssuerNetworkError = 79,
        [Description("Невдала маршрутизація запиту неможлива. Помилка мережі")]
        UnsuccessfulRequestRoutingIsNotPossibleNetworkError = 80,
        [Description("Трансакцію неможливо завершити. Емітент відхилив авторизацію через порушення правил")]
        TheTransactionCanNotBeCompletedTheIssuerDeclineAuthorizationDueToAViolationOfTheRules = 81,
        [Description("Дублювання передачі. Помилка мережі")]
        DuplicationOfTransmissionNetworkError = 82,
        [Description("Загальна несправність системи")]
        GeneralSystemMalfunction = 83,
        [Description("Неможливо надіслати зашифроване повідомлення")]
        UnableToSendEncryptedMessage = 84
    }

    public enum eStatusRRO
    {
        [Description("NotDefine")]
        NotDefine = 0,
        [Description("Зачекайте")]
        /// <summary>
        /// Очікування команди
        /// </summary>
        Wait,
        [Description("Очікування відповіді")]
        /// <summary>
        /// Очікуєм відповідь від РРО
        /// </summary>
        WaitAnswer,
        [Description("Перетворення результату")]
        /// <summary>
        /// Обробка результату
        /// </summary>
        ParseResult,
        [Description("Все добре")]
        /// <summary>
        /// Успішне виконаання
        /// </summary>
        OK,
        [Description("Початок друку чеку")]
        /// <summary>
        /// Виникла помилка під час операції
        /// </summary>

        StartPrintReceipt,
        [Description("Додати товари")]
        AddWares,
        [Description("Додати оплату")]
        AddPay,
        [Description("Закрити порт")]
        ClosePort,
        [Description("Спроба відкрити порт")]
        TryOpenPort,
        [Description("Відкрити порт")]
        OpenPort,
        [Description("Критична Помилка(Необхідо звернутись в сервісний центр)")]
        Error,
        [Description("Помилка")]
        CriticalError,
        [Description("Ініціалізація")]
        Init,
        [Description("Попередження")]
        Warning
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

    public enum eStateEquipment { On = 0, Init=1, Off=2, Process=3, Error=4 }

    /// <summary>
    /// Типи обладнання (ваги, касові апарати)
    /// </summary>
    public enum eTypeEquipment
    {
        [Description("Невизначено")]
        NotDefine,
        [Description("Сканер")]
        Scaner,
        [Description("Вага")]
        Scale,
        [Description("Контрольна вага")]
        ControlScale,
        [Description("Банківський термінал")]
        BankTerminal,
        [Description("Кольоровий покажчик")]
        Signal,
        [Description("Фіскальний апарат")]
        RRO
    }

    /// <summary>
    /// Підтримуване обладнання (Magellan,Exelio,Ingenico,...)
    /// </summary>
    public enum eModelEquipment
    {
        [Description("Невизначено")]
        NotDefine,
        [Description("Магелан-сканер")]
        MagellanScaner,
        [Description("Магелан-вага")]
        MagellanScale,
        [Description("Контррльна вага")]
        ScaleModern,
        [Description("Сигнальний стяг")]
        SignalFlagModern,
        [Description("POS-термінал Ingenico")]
        Ingenico,
        [Description("Віртуальний POS-термінал")]
        VirtualBankPOS,
        [Description("ФР Exellio")]
        ExellioFP,
        [Description("Програмний ФР pRRO_SG")]
        //Exellio,
        pRRO_SG,
        [Description("ФР Марія")]
        Maria,
        [Description("ФР FP700")]
        FP700,
        [Description("ФР WebCheck")]
        pRRo_WebCheck,
        [Description("Віртуальний ФР")]
        VirtualRRO,
        [Description("Віртуальна вага")]
        VirtualScale,
        [Description("Віртуальний сканер")]
        VirtualScaner,
        [Description("Віртуальна контрольна вага")]
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
                case eModelEquipment.FP700:

                    return eTypeEquipment.RRO;
                default:
                    return eTypeEquipment.NotDefine;
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
   
