using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public enum ePeriod
    {
        Year,
        Month,
        Day
    }
    public enum eTypeAccess
    {
        Question = -3, //Виводити вікно для введення логіна які розширять права на цю операцію. Якщо не введено не дозволяти цю операцію. 
        NoErrorAccess = -2, //не виконувати дію Видавати повідомленя про відсутність прав. Не виводити вікно для введення логіна які розширять права на цю операцію			
        No = -1, // не виконувати дію не видавати жодних повідомлень.
        Yes = 0 // Є права на зазначену операцію.		
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

    public enum eExchangeStatus
    {
        Green = 0,
        LightGreen = 1,
        Yellow = 2,
        Orange = 3,
        Red = 4
    }
}
