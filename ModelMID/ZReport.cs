using System.Collections;
using System.Collections.Generic;

namespace ModelMID
{

    internal class ZReport : IdReceipt
    {
        /// <summary>
        /// Оборот Готівка
        /// </summary>
        public decimal Cash { get; set; }
        /// <summary>
        /// Сума з Термінала.
        /// </summary>
        public decimal Bank { get; set; }
        /// <summary>
        /// Оборот Картка
        /// </summary>
        public decimal Card { get; set; }
        /// <summary>
        /// Обіг Загальний
        /// </summary>
        public decimal Total { get; set; }
        /// <summary>
        /// Оборот Повернення Готівка
        /// </summary>
        public decimal RefaundCash { get; set; }
        /// <summary>
        /// Сума повернення з Термінала.
        /// </summary>
        public decimal RefaundBank { get; set; }
        /// <summary>
        /// Оборот повернення Картка
        /// </summary>
        public decimal RefaundCard { get; set; }
        /// <summary>
        /// Всього поверненя
        /// </summary>
        public decimal RefaundTotal { get; set; }
        /// <summary>
        /// Ставка 20%
        /// </summary>
        public decimal Tax20 { get; set; }
        /// <summary>
        /// Ставка 7%
        /// </summary>
        public decimal Tax7 { get; set; }
        /// <summary>
        /// Ставка 0%
        /// </summary>
        public decimal Tax0 { get; set; }
        /// <summary>
        /// Без ПДВ
        /// </summary>
        public decimal Tax { get; set; }
        /// <summary>
        /// Продаж Акцизу
        /// </summary>
        public decimal TaxExcise { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal TotalTax { get; set; }
        /// <summary>
        /// Готівка в касі.
        /// </summary>
        public decimal InCash { get; set; }
        /// <summary>
        /// Текст звіта з фіскалки.
        /// </summary>
        public string Z { get; set; }
        /// <summary>
        /// Текст звіта з ПОС термінала.
        /// </summary>
        public string ZPos { get; set; }
        public decimal CashReal { get; set; }
        public IEnumerable<BillCoin> BillCoins { get; set; }

    }
    public class BillCoin
    {
        public decimal Denomination { get; set;}
        public decimal Quantity { get; set;}
    }
}
