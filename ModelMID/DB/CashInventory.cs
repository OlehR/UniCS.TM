using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelMID.DB
{
    public class CashInventory
    {
        /// <summary>
        /// Номінал валюти в копійках
        /// </summary>
        public int FaceValue { get; set; }
        /// <summary>
        /// Кількість купюр\монет
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// Тип грошей готіка\копійки
        /// </summary>
        public eTypeMoney TypeMoney { get; set; }
        /// <summary>
        /// Місце зберігання грошей в кеш-машині
        /// </summary>
        public eMoneyStoragePlace MoneyStoragePlace { get; set; }
        public DateTime DateInventory { get; set; }

        public CashInventory() { DateInventory = DateTime.Now; }

    }
    public enum eTypeMoney
    {
        Banknote = 0,
        Coin = 1
    }
    public enum eMoneyStoragePlace
    {
        All = 0,
        Safe = 1,
        Drum = 2
    }
}
