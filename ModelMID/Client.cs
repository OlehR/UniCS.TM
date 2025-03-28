﻿using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utils;

namespace ModelMID
{
    /// <summary>
    /// 0-Активна,1-Заблокована,2 - видалена.
    /// </summary>
    public enum eStatusCard
    {
        [Description("Активна")]
        Active =0,
        [Description("Заблокована")]
        Block =1,
        [Description("Видалена")]
        Delete =2
    }
    /// <summary>
    /// Зберігає інформацію про клієнта
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Код клієнта
        /// </summary>
        public long CodeClient { get; set; }
        /// <summary>
        ///  Назва клієнта
        /// </summary>
        public string NameClient { get; set; }
        /// <summary>
        /// Тип знижки
        /// </summary>
        public int TypeDiscount { get; set; }

        public string NameDiscount { get; set; }

        /// <summary>
        /// Штрихкод карточки
        /// </summary>
        public string BarCode { get; set; }
        public string MainPhone { get; set; }

        public string PhoneAdd { get; set; }

        /// <summary>
        /// Відсоток знижки / надбавки
        /// </summary>
        public decimal PersentDiscount { get; set; }
        /// <summary>
        /// Код дилерської категорії
        /// </summary>
        public int CodeDealer { get; set; }
        /// <summary>
        /// Сума накопичених бонусів
        /// </summary>
        public decimal SumBonus { get; set; }
        /// <summary>
        /// Відсоток перегахування.
        /// </summary>
        public decimal PercentBonus { get; set; }
        /// <summary>
        /// Відсоток перегахування.
        /// </summary>
        public int PercentBonusIntegers { get { return (int)(PercentBonus * 100); } }
        /// <summary>
        /// Сума накопичених бонусів в грошовому еквіваленті
        /// </summary>
        public decimal SumMoneyBonus { get; set; }
        /// <summary>
        /// Чи можна списувати бонуси за рахунок здачі
        /// </summary>
        public bool IsUseBonusToRest { get; set; }
        /// <summary>
        /// Здача Кошильок
        /// </summary>
        public decimal Wallet { get; set; }
        /// <summary>
        /// Чи можна нараховувати бонуси з здачі
        /// </summary>
        public bool IsUseBonusFromRest { get; set; }
        /// <summary>
        /// 0-Активна,1-Заблокована,2 - видалена.
        /// </summary>
        public eStatusCard StatusCard { get; set; }
       
        public string TranslatedStatusCard { get { return StatusCard.GetDescription(); } }
        /// <summary>
        /// Код карточки який видно на дисконтній карточці.
        /// </summary>
        public Int64 ViewCode { get; set; }

        public DateTime BirthDay { get; set; }

        public IEnumerable<Int64> OneTimePromotion { get; set; }
        /// <summary>
        /// Якщо пройшов результат з сервера
        /// </summary>
        public bool IsCheckOnline { get; set; }
        /// <summary>
        /// Чи карточка з коштами(сертифікати та інше)
        /// </summary>
        public bool IsMoneyCard { get; set; }
        public bool IsСertificate { get; set; }

        public IEnumerable<ReceiptGift> ReceiptGift { get; set; }
        public Client() { }
        public Client(int parCodeClient) => CodeClient=parCodeClient; 
    }
}
