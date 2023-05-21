using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ModelMID
{
    public enum eTypeWorkplace
    {
        NotDefine = 0,
        SelfServicCheckout =3,
        CashRegister =1,

    }

    public enum ePeriod
    {
        Year,
        Month,
        Day
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
        FiscalInfo =7
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
        /// Заміна ДК (Не використовується)
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
        NotDefine = 0,
        PrivatBank = 3,
        Oschadbank = 11
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
        /// Пиво (Ще не використавується)
        /// </summary>
        Beer = 4
    }

    public enum eDBStatus
    {
        ErrorUpdateDB = -2,
        Error = -1,
        NotDefine = 0,
        Ok = 1
    }
}