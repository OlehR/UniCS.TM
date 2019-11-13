using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class Wares
    {
        /// <summary>
        /// Код товару
        /// </summary>
        public int CodeWares { get; set; }
        
        /// <summary>
        /// Код Групи
        /// </summary>
        public int CodeGroup { get; set; }
        /// <summary>
        /// Назва товару
        /// </summary>
        public string NameWares { get; set; }
        /// <summary>
        /// Назва для чека.
        /// </summary>
        public string NameWaresUpper { get { return NameWares.ToUpper(); } }
        public string NameWaresReceipt { get; set; }
        public int Articl { get; set; }
        public int CodeBrand { get; set; }
        /// <summary>
        /// % Ставки ПДВ (0 -0 ,20 -20%)
        /// </summary>
        public decimal PercentVat { get; set; }
        /// <summary>
        /// Код одиниці виміру позамовчуванню
        /// </summary>
        /// 
        public int TypeVat { get; set; }
        
        
        /// <summary>
        /// Текуча одиниця виміру
        /// </summary>
        public int CodeUnit { get; set; }
        
        public string Description { get; set; }
        /// <summary>
        /// 0-звичайний,1-алкоголь,2-тютюн
        /// </summary>
        public int TypeWares { get; set; }
        public decimal WeightBrutto { get; set; }



    }
}
