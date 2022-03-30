using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public enum eTypeWorkplace
    {
        SelfServicCheckout,
        СashRegister
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
        Both = 3

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
        /// <summary>
        /// Скасовано
        /// </summary>
        Canceled = -1,

        /// <summary>
        /// Готується
        /// </summary>
        Prepare = 0,
        /// <summary>
        /// Оплачено
        /// </summary>
        Pay = 1,
        /// <summary>
        /// Надруковано
        /// </summary>
        Print = 2,
        /// <summary>
        /// Відправлено в 1С
        /// </summary>
        Send = 3
    }

    public enum eTypeReceipt
    {
        Refund = -1,
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

    public enum ReceiptEventType
    {
        ErrorQR = -9,
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

    public enum eTypePOS
    {
        NotDefine = 0,
        PrivatBank = 3,
        Oschadbank = 11
    }
    /// <summary>
    /// Тип робочого місця (КСО,Звичайна каса)
    /// </summary>
    public enum eTypeWorkPlace
    {
        /// <summary>
        /// Каса самообслуги
        /// </summary>
        SelfServiceCheckout,
        /// <summary>
        /// Касове місце (Можливо є краща назва)
        /// </summary>
        CashPlace
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
        Client = 1,
        /// <summary>
        /// Касир
        /// </summary>
        Сashier = 2,
        /// <summary>
        /// Адміністратор self-service checkout 
        /// </summary>
        AdminSSC = 3,       
        /// <summary>
        /// Охоронець
        /// </summary>
        Guardian = 4,
        /// <summary>
        /// Адміністратор Дозвіл на всі операції
        /// </summary>
        Admin = 9
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
        NoDefinition = 0,
        DelWares = 1,
        DelReciept = 2,
        ChoicePrice = 3,
        ConfirmAge = 4,
        ExciseStamp = 5,
        FixWeight = 6,
        AddNewWeight = 7,
        ReturnReceipt = 8,
    }

    public enum eTypeAccessAnsver
    {
        Question = -3, //Виводити вікно для введення логіна які розширять права на цю операцію. Якщо не введено не дозволяти цю операцію. 
        NoErrorAccess = -2, //не виконувати дію Видавати повідомленя про відсутність прав. Не виводити вікно для введення логіна які розширять права на цю операцію			
        No = -1, // не виконувати дію не видавати жодних повідомлень.
        Yes = 0 // Є права на зазначену операцію.		
    }


}