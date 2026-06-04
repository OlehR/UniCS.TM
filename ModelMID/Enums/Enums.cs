using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ModelMID
{
    public enum eTypeWorkplace
    {
        [Description("Не визначено")]
        NotDefine = 0,
        [Description("Каса самообслуговування")]
        SelfServicCheckout =3,
        [Description("Звичайна каса")]
        CashRegister =1,
        [Description("Вибір типу каси")]
        Both = 2

    }

    public enum ePeriod
    {
        Year=1,
        Month=2,
        Day=3
    }    

    public enum eTypePay
    {
        None = 0,
        Card = 1,
        Cash = 2,
        Both = 3,
        /// <summary>
        /// Видача готівки
        /// </summary>
        IssueOfCash = 4,
        /// <summary>
        /// Скарбничка(Здача)
        /// </summary>
        Wallet= 5,
        /// <summary>
        /// Розрахунок бонусами.
        /// </summary>
        Bonus = 6,
        /// <summary>
        /// Інформація з фіскального апарата.(Сума + Завкруглення)
        /// </summary>
        FiscalInfo =7,
        /// <summary>
        /// Готівкова оплата через кеш-машину
        /// </summary>
        CashMachine = 8,
        /// <summary>
        /// післяплата
        /// </summary>
        Postpaid = 9,
    }

    public enum eTypeBonus
    {
        NonBonus = 0, // 
        Bonus = 1,
        BonusWithOutRest = 2, // Використовувати бонус з врахуванням здачі( якщо бонуса не вистачає - берем таким чином щоб здача була кругла)
        BonusToRest = 3,
        BonusFromRest = 4
    }

    /// <summary>
    /// Інформація про те що знайшли в універсальному вікні пошуку
    /// 0 - все,1 - товари,2-клієнти,3-купони та акціїї
    /// </summary>	
    public enum eTypeFind
    {
        All = 0,
        Wares,
        Client,
        Action
    }

    public struct eRezultFind
    {
        public eTypeFind TypeFind;
        public int Count;
    }

    public enum eTypePayment
    {
        Cash,
        Bonus,
        CreditCard,
        MoneyBox

    }

    public enum eTypeCommit
    {
        Auto,
        Manual
    }

    public enum eStatePromotionSale
    {
        Completed = 1,
        Prepare = 0,
        Prepared = 1,
        Ready = 9
    }

    public enum eTypeWorkCoupon
    {
        WithOutCoupon = 0, // без купона, 
        All = 1,  //на всі товари, 
        Coupon = 2 //тільки на товар зчитаний перед купоном.
    }

    public enum eRuleGroup
    {
        Not = -1,
        Or = 0,
        And = 1
    }


    public enum eTypePromotionFilter
    {
        NotDefine = 0,
        /// <summary>
        /// Товари
        /// </summary>
        Wares = 11,
        /// <summary>
        /// Бренди(ТМ)
        /// </summary>
        TM = 14,
        /// <summary>
        /// Групи товарів
        /// </summary>
        GroupWares = 15,
        /// <summary>
        /// Властивості товарів
        /// </summary>
        ProportyWares = 19,
        /// <summary>
        /// Період
        /// </summary>
        Period = 21,
        /// <summary>
        /// Час
        /// </summary>
        Time = 22,
        /// <summary>
        /// Період відносно Дня народження
        /// </summary>
        Birthday = 23,
        /// <summary>
        /// Період відносно Активації карточки
        /// </summary>
        СardАctivation = 24,
        /// <summary>
        /// День в місяці
        /// </summary>
        DayInMonth = 25,
        /// <summary>
        /// День в Тижні
        /// </summary>
        DayInWeek = 26,
        /// <summary>
        /// День в Тижні з годинами
        /// </summary>
        DayInWeekWithTime = 27,
        /// <summary>
        /// По переліку клієнтів
        /// </summary>
        Client = 31,
        /// <summary>
        /// По типу клієнта.
        /// </summary>
        TypeClient = 32,
        /// <summary>
        /// По властивості клієнта.
        /// </summary>
        ProportyClient = 39,
        /// <summary>
        /// Для Чека №
        /// </summary>
        NumberReceipt = 41,
        /// <summary>
        /// Для кожного N чека.
        /// </summary>
        ForEachReceipt = 42,
        /// <summary>
        /// Склад
        /// </summary>
        Warehouse = 51,
        /// <summary>
        /// По властивості Склада
        /// </summary>
        ProportyWarehouse = 59,
        /// <summary>
        /// Готівка
        /// </summary>
        Cash = 61,
        /// <summary>
        /// Карткою
        /// </summary>
        Card = 62,
        /// <summary>
        /// Бонуси
        /// </summary>
        Bonus = 63,
        /// <summary>
        ///Купон 
        /// </summary>
        Coupon = 71

    }
    public enum eTypePrice
    {
        NotDefine = 0,
        PriceDealer = 1,
        PDDiscont = 2,
        PDDiscontMin = 3,
        Indicative = 4,
        PDDiscontIndicative = 5,
        Promotion = 9,
        PromotionIndicative = 10
    }

    public enum eTypeDiscount
    {
        /// <summary>
        /// "Бали" для розрахунку інших акцій.
        /// </summary>
        ForCountOtherPromotion=-9,
        /// <summary>
        /// Текст Касиру.
        /// </summary>
        TextСashier = -1,
        /// <summary>
        /// Друк тексту на чеку.
        /// </summary>
        TextReceipt =-2,
        /// <summary>
        /// Друк QR на чеку
        /// </summary>
        PrintQRReceipt = -3,
        /// <summary>
        /// Текст на екрані клієнта.
        /// </summary>
        TextClientScreen =-4,


        NotDefine = 0,
        /// <summary>
        /// Фіксована ціна
        /// </summary>
        Price = 11,
        /// <summary>
        /// Знижка в ГРН
        /// </summary>
        PriceDiscount = 12,
        /// <summary>
        /// Знижка в відсотках
        /// </summary>
        PercentDiscount = 13,
        /// <summary>
        /// Заміна ДК
        /// </summary>
        ReplacePriceDealer = 14,
        /// <summary>
        /// Набір
        /// </summary>
        Set = 21,
        /// <summary>
        /// Подарок
        /// </summary>
        Gift = 41
    }

    public enum eStateReceipt
    {
        [Description("Скасовано")]
        /// <summary>
        /// Скасовано
        /// </summary>
        Canceled = -1,
        [Description("Готується")]
        /// <summary>
        /// Готується
        /// </summary>
        Prepare = 0,
        [Description("Підготовка до оплати")]
        /// <summary>
        /// Початок оплати (Для блокування дій з чеком)
        /// </summary>
        StartPay = 1,
        [Description("Часткова оплата")]
        /// <summary>
        /// Часткова оплата
        /// </summary>
        PartialPay = 2,
        [Description("Оплачено")]
        /// <summary>
        /// Оплачено
        /// </summary>
        Pay = 3,
        [Description("Початок Фіскалізації")]
        /// <summary>
        /// Початок Фіскалізації (Для блокування дій з чеком)
        /// </summary>
        StartPrint = 6,
        [Description("Часткова Фіскалізації")]
        /// <summary>
        /// Часткова Фіскалізації (Для блокування дій з чеком)
        /// </summary>
        PartialPrint = 7,
        [Description("Фіскалізацізовано")]
        /// <summary>
        /// Надруковано
        /// </summary>
        Print = 8,
        [Description("Відправлено в 1С")]
        /// <summary>
        /// Відправлено в 1С
        /// </summary>
        Send = 9
    }

    public enum eTypeReceipt
    {
        [Description("Повернення")]
        Refund = -1,
        [Description("Продаж")]
        Sale = 1
    }

    public enum eMethodExecutionLoggingType
    {
        MoreThenMillis = 0,
        Always,
        WhenErrored,
    }
    /// <summary>
    /// Стан обміну()
    /// </summary>
    public enum eExchangeStatus
    {
        Green = 0,
        LightGreen = 1,
        Yellow = 2,
        Orange = 3,
        Red = 4
    }

    public enum eReceiptEventType
    {

        TimeScanReceipt = -11,
        OwnBag = -10,
        ErrorQR = -9,
        PackagesBag = -8,
        AnswerQR = -2,
        AskQR = -1,
        IncorrectWeight = 1,
        AgeRestrictedProduct = 2,
        ManualChangeGiving = 3,
        CustomerCardAccepted = 4,
        ProductWithSecurityMark = 5,
        ReceiptWithRefund = 6,
        Paid = 7,
        GarbageOnWeight = 8,
        Other = 9
    }

    public enum TypeSaveWeight
    {
        Add = -1,
        Main = 0,
        Client = 1
    }

    public enum eStateScale
    {
        NotDefine = 0,
        /// <summary>
        /// Товар необхідно поставити на платформу.
        /// </summary>
        WaitGoods,
        /// <summary>
        /// Почала стабілізовуватись
        /// </summary>
        StartStabilized,
        /// <summary>
        /// Вага стабільна.
        /// </summary>
        Stabilized,
        /// <summary>
        /// вага не стабільна (Середне значення поза межами похибки)
        /// </summary>
        NotStabilized,
        /// <summary>
        /// Невірна вага(відносно стабільна)
        /// </summary>
        BadWeight,
        /// <summary>
        /// Очікуємо очистку ваги.
        /// </summary>
        WaitClear
    }

    public enum eBank
    {
        [Description("Термінал")]
        Terminal = -1,
        [Description("НеВизначено")]
        NotDefine = 0,
        [Description("Приват")]
        PrivatBank = 3,
        [Description("Ощад")]
        OschadBank = 11,
        [Description("MTB")]
        MTB = 13
    }
   
    /// <summary>
    /// Кастомні вікна
    /// </summary>
    public enum eTypeWindows
    {
        LimitSales
    }

    /// <summary>
    /// Типи користувачів системи
    /// </summary>
    public enum eTypeUser
    {
        /// <summary>
        /// Клієнт КСО
        /// </summary>
        Client = -100,
        /// <summary>
        /// Працівник без Прав.
        /// </summary>
        Employee = -20,
        /// <summary>
        /// Працівник з доступом до ТЗД
        /// </summary>
        EmployeeDCT = -15,
        /// <summary>
        /// Касир
        /// </summary>
        Сashier = -14,
        /// <summary>
        /// Адміністратор self-service checkout 
        /// </summary>
        AdminSSC = -13,
        /// <summary>
        /// Охоронець
        /// </summary>
        Guardian = -12,
        /// <summary>
        /// Адміністратор ТЗ
        /// </summary>
        AdminShop =-11, 
        /// <summary>
        /// Адміністратор Дозвіл на всі операції
        /// </summary>
        Admin = 0
    }

    /*  public enum eСonfirmationActions
      {
          NoDefinition = 0,
          DelWares = 1,
          DelReciept = 2,
          ChoicePrice = 3,
          ConfirmAge = 4,
          ExciseStamp = 5,
          FixWeight = 6,
          AddNewWeight=7,
          Cr
          //Неспівпадіння ваги і невідповідність.
      }*/

    public enum eTypeAccess
    {
        NoDefine = 0,
        DelWares = 1,
        DelReciept = 2,
        ChoicePrice = 3,
        ConfirmAge = 4,
        ExciseStamp = 5,
        /// <summary>
        /// Дозвіл на фіксування ваги
        /// </summary>
        FixWeight = 6,
        /// <summary>
        /// Дозвіл на Добавлення ваги в Базу
        /// </summary>
        AddNewWeight = 7,
        /// <summary>
        /// Права на створення чека повернення
        /// </summary>
        ReturnReceipt = 8,
        AdminPanel = 9,
        /// <summary>
        /// Дозвіл списання Бонусів.
        /// </summary>
        UseBonus =10,

        LockSale = -1,
        StartFullUpdate = -2,
        ErrorFullUpdate = -3,
        ErrorEquipment = -4,
        ErrorDB =-5
    }

    public enum eTypeAccessAnsver
    {
        Question = -3, //Виводити вікно для введення логіна які розширять права на цю операцію. Якщо не введено не дозволяти цю операцію. 
        NoErrorAccess = -2, //не виконувати дію Видавати повідомленя про відсутність прав. Не виводити вікно для введення логіна які розширять права на цю операцію			
        No = -1, // не виконувати дію не видавати жодних повідомлень.
        Yes = 0 // Є права на зазначену операцію.		
    }

    public enum eTypeWares
    {
        /// <summary>
        /// Звичайний товар
        /// </summary>
        Ordinary=0,
        /// <summary>
        /// Алкоголь
        /// </summary>
        Alcohol=1,
        /// <summary>
        /// Тютюн
        /// </summary>
        Tobacco=2,
        /// <summary>
        /// Тютюн з включеним в МРЦ акцизом
        /// </summary>
        TobaccoNoExcise = 3,
        /// <summary>
        /// Пиво
        /// </summary>
        Beer = 4,
        /// <summary>
        /// Слабоалкогольні напої без акцизної марки
        /// </summary>
        LowAlcohol = 5
    }

    public enum eDBStatus
    {
        ErrorUpdateDB = -2,
        Error = -1,
        NotDefine = 0,
        Ok = 1
    }

    public enum eReturnClient
    {
        [Description("Помилка")]
        Error = -9999999,
        [Description("Не вдале з'єднання з 1С")]
        ErrorConnect = -99,
        [Description("Помилка збереження в 1С спробуйте пізніше")]
        ErrorWriteDB = -3,
        [Description("Дана картка вже видана. Операція реєстрації не виконана!")]
        ErrorCardIsAlreadyPresent = -2,
        [Description("По даній картці вже є обороти в системі. Операція реєстрації не виконана!")]   
        ErrorCardIsUse = -1,
        [Description("Картка не активована. Зверніться у відділ Реклами")]
        ErrorInputData = -0,
        [Description("Карточка успішно зареєстрована")]
        Ok = 1
    }

    public enum eTypeMessage
    {
        Error,
        Warning,
        Information,
        Question
    }

    public enum eNameSOAPAction
    {
        OpenOperation,
        AdjustTimeOperation,
        GetStatus,
        OccupyOperation,
        CashoutOperation,
        CashinCancelOperation,
        ChangeCancelOperation,
        [Description("Видати решту або оплатити товар")]
        ChangeOperation,
        CloseExitCoverOperation,
        ResetOperation,
        StartCashinOperation,
        EndCashinOperation,
        RefreshSalesTotalOperation,
        AutoRebootChangeOperation,
        CollectOperation,
        CounterClearOperation,
        DisableDenomOperation,
        EnableDenomOperation,
        EndReplenishmentFromCassetteOperation,
        EndReplenishmentFromEntranceOperation,
        EventNotificationStatusOperation,
        EventOfflineRecoveryOperation,
        GetLastResponseOperation,
        GetSettingFileOperation,
        LanguageChangeOperation,
        LockUnitOperation,
        LoginUserOperation,
        LogoutUserOperation,
        OpenExitCoverOperation,
        PowerControlOperation,
        RegisterEventOperation,
        ReplenishmentFromEntranceCancelOperation,
        [Description("Return remaining coins in the hopper to the exit slot")]
        ReturnCashOperation,

        RollbackOperation,
        RomVersionOperation,
        SetExchangeRateOperation,
        SetRestrictionOperation,
        StartDownloadOperation,
        StartLogreadOperation,
        StartReplenishmentFromCassetteOperation,
        StartReplenishmentFromEntranceOperation,
        StartSealingOperation,
        UnLockUnitOperation,
        UnRegisterEventOperation,
        UpdateCheckOperation,
        UpdateDeviceCassetteSettingOperation,
        UpdateManualDepositTotalOperation,
        UpdateSettingFileOperation,
        UserSettingOperation,



        InventoryOperation,

        ReleaseOperation,
        CloseOperation,
    }

    public enum eResultCode
    {
        [Description("Успішно")]
        Success = 0,

        [Description("Скасовано")]
        Cancel = 1,

        [Description("Скинуто")]
        Reset = 2,

        [Description("Зайнято іншим")]
        OccupiedByOther = 3,

        [Description("Захоплення недоступне")]
        OccupationNotAvailable = 4,

        [Description("Не зайнято")]
        NotOccupied = 5,

        [Description("Нестача номіналів для видачі")]
        DesignationDenominationShortage = 6,

        [Description("Нестача решти при скасуванні")]
        CancelChangeShortage = 9,

        [Description("Нестача решти")]
        ChangeShortage = 10,

        [Description("Помилка монопольного доступу")]
        ExclusiveError = 11,

        [Description("Невідповідність виданої решти")]
        DispensedChangeInconsistency = 12,

        [Description("Збій автовідновлення")]
        AutoRecoveryFailure = 13,

        [Description("Помилка автентифікації користувача")]
        UserAuthenticationFailure = 15,

        [Description("Перевищено кількість сесій")]
        NumberOfSessionOver = 16,

        [Description("Зайнято поточним пристроєм")]
        OccupiedByItself = 17,

        [Description("Сесія недоступна")]
        SessionNotAvailable = 20,

        [Description("Недійсна сесія")]
        InvalidSession = 21,

        [Description("Час сесії вичерпано")]
        SessionTimeout = 22,

        [Description("Розбіжність при ручному внесенні")]
        ManualDepositDisagreement = 26,

        [Description("Помилка перевірки/поповнення касети")]
        VerifyCollect_ReplenishFailed = 32,

        [Description("Неприпустимий номінал касети IF")]
        IFCassetteIllegalDenomination = 33,

        [Description("Нестача місця у стекері")]
        ShortageOfCapacityOfStacker = 34,

        [Description("Помилка зв'язку з CI-сервером")]
        CI_ServerCommunicationError = 35,

        [Description("Перевищено кількість реєстрацій")]
        NumberOfRegistrationOver = 36,

        [Description("Недійсний номер касети")]
        InvalidCassetteNumber = 40,

        [Description("Некоректна касета")]
        ImproperCassette = 41,

        [Description("Помилка курсу обміну")]
        ExchangeRateError = 43,

        [Description("Підраховано категорію 2")]
        CountedCategory2 = 44,

        [Description("Дублікат транзакції")]
        DuplicateTransaction = 96,

        [Description("Помилка параметру/типу")]
        ParameterError_TypeError = 98,

        [Description("Внутрішня помилка програми")]
        ProgramInnerError = 99,

        [Description("Помилка пристрою")]
        DeviceError = 100,

        [Description("Невідома помилка")]
        UnknownError = 9999
    }

    public enum eStatusChangeEvent
    {
        [Description("Ініціалізація")]
        Initializing = 0,

        [Description("Очікування")]
        Idle = 1,

        [Description("Початок операції розміну")]
        AtStartingChange = 2,

        [Description("Очікування внесення готівки")]
        WaitingInsertionOfCash = 3,

        [Description("Підрахунок")]
        Counting = 4,

        [Description("Видача")]
        Dispensing = 5,

        [Description("Очікування вилучення решти")] //"Очікування вилучення відхилених банкнот на вході" ПЕРЕВІРИТИ на живій кеш машині - емулятор вертає коли дає решту
        WaitingRemovalOfCashInReject = 6,

        [Description("Очікування вилучення відхилених банкнот на виході")]
        WaitingRemovalOfCashOutReject = 7,

        [Description("Скидання до початкового стану")]
        Resetting = 8,

        [Description("Скасування операції розміну")]
        CancelingOfChangeOperation = 9,

        [Description("Розрахунок суми решти")]
        CalculatingChangeAmount = 10,

        [Description("Скасування внесення")]
        CancelingDeposit = 11,

        [Description("Інкасація")]
        Collecting = 12,

        [Description("Помилка")]
        Error = 13,

        [Description("Завантаження прошивки")]
        UploadFirmware = 14,

        [Description("Читання журналу")]
        ReadingLog = 15,

        [Description("Очікування поповнення")]
        WaitingReplenishment = 16,

        [Description("Підрахунок при поповненні")]
        CountingReplenishment = 17,

        [Description("Розблокування")]
        Unlocking = 18,

        [Description("Очікування інвентаризації")]
        WaitingInventory = 19,

        [Description("Фіксована сума внесення")]
        FixedDepositAmount = 20,

        [Description("Фіксована сума видачі")]
        FixedDispenseAmount = 21,

        [Description("Очікування видачі")]
        WaitingDispense = 22,

        [Description("Очікування скасування розміну")]
        WaitingChangeCancel = 23,

        [Description("Підраховано банкноту категорії 2")]
        CountedCategory2Note = 24,

        [Description("Очікування завершення внесення")]
        WaitingDepositEnd = 25,

        [Description("Очікування вилучення COFT")]
        WaitingRemovalOfCOFT = 26,

        [Description("Запечатування")]
        Sealing = 27,

        [Description("Очікування відновлення після помилки")]
        WaitingForErrorRecovery = 30,

        [Description("Програма зайнята")]
        ProgramBusy = 40,

        [Description("Очікування оновлення")]
        WaitingForUpdate = 41,

        [Description("Скасовано користувачем")]
        CanceledByUser =100,
    }

    public enum eTypeUnit
    {
        RBW_100 = 1,
        RCW_100 = 2,
        RBW_200 = 3 //не наш варіант але може бути в майбутньому 
    }
    public enum eTypeAuditReceipt
    {
        [Description("Не визначено")]
        NotDefine = 0,
        [Description("Видалення чека")]
        DelReceipt = 1,
        [Description("Видалення товару")]
        DelWares = 2,
        [Description("Зменшення кількості")]
        DecreaseQuantity = 3,
        [Description("Збільшення кількості")]
        IncreaseQuantity = 4
    }
}