using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelMID
{
    public class Settings
    {
        public Settings() { }
        /// <summary>
        /// Код складу
        /// </summary>
        public int CodeWarehouse { get; set; }
        /// <summary>
        /// 55-Вопак,56-Spar
        /// </summary>
        public int CodeTM { get; set; }
        /// <summary>
        /// Мінімальна сума видачі готівки через касу
        /// </summary>
        public decimal SumDisbursementMin { get; set; }
        /// <summary>
        /// Максимальна сума видачі готівки через касу
        /// </summary>
        public decimal SumDisbursementMax { get; set; }
        /// <summary>
        /// Список пакетів
        /// </summary>
        public IEnumerable<decimal> Bags { get; set; }
        /// <summary>
        /// максимальна вага власної сумки.
        /// </summary>
        public decimal MaxWeightBag { get; set; }
        /// <summary>
        /// Код товару скарбнички
        /// </summary>
        public int CodeWaresWallet { get; set; } = 163516;
        /// <summary>
        /// Код пакету який пропунує додати в чек
        /// </summary>
        public int CodePackagesBag { get; set; } = 193122;
        /// <summary>
        /// Текст в кінці чека
        /// </summary>
        public IEnumerable<string> Footer { get; set; }
        /// <summary>
        /// Склад Підприємець(Компанія)
        /// </summary>
        public int CodeWarehouseLink { get; set; }
        /// <summary>
        /// Робоче місце(каса) Підприємець(Компанія)
        /// </summary>
        public int IdWorkPlaceLink { get; set;}
        /// <summary>
        /// посилання на основний сервер.(куда скидати повідомлення)
        /// </summary>
        public int IdWorkPlaceMain { get; set; }
        /// <summary>
        /// Чи здійснювати контроль Акцизної марки на сервері.
        /// </summary>
        public bool IsCheckExciseStamp { get; set; }
        /// <summary>
        /// Чи відправляти чеки в 1С.
        /// </summary>
        public bool IsSend1C { get; set; } = true;
        /// <summary>
        /// Чи можна розраховуватись бонусамим.
        /// </summary>
        public bool IsPayBonus { get;set; } =true;
        /// <summary>
        /// Код в зовнішній системі
        /// </summary>
        public int CodeWarehouseExSystem { get; set; }
        /// <summary>
        /// Чи використовувати карточки SPAR Україна
        /// </summary>
        public bool IsUseCardSparUkraine { get; set; } = false;

        /// <summary>
        /// Api
        /// </summary>
        public string Api { get; set; }
        /// <summary>
        /// максимальна сума чека.
        /// </summary>
        public decimal MaxSumReceipt { get; set; }

        public IEnumerable<CustomerBarCode> CustomerBarCode { get; set; }

    }
}
