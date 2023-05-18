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
        public int CodeWaresWallet { get; set; }
        /// <summary>
        /// Код пакету який пропунує додати в чек
        /// </summary>
        public int CodePackagesBag { get; set; } = 193122;
    }
}
