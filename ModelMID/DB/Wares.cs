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

        public int CodeGroupUp { get; set; }        
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
        public eTypeWares TypeWares { get; set; }
        public decimal WeightBrutto { get; set; }
        public decimal WeightFact { get; set; }
        public decimal WeightDelta { get; set; }
        public string CodeUKTZED { get; set; }
        /// <summary>
        /// Вікові обмеження (Піротехніка)
        /// </summary>
        public decimal LimitAge { get; set; }
        /// <summary>
        /// PLU - кавомашини
        /// </summary>
        public  int PLU { get; set; }
        /// <summary>
        /// Напрямок
        /// </summary>
        public int CodeDirection { get; set; }
        /// <summary>
        /// Торгова марка (в 1С - Бренд) 
        /// </summary>
        public int CodeTM { get; set; }
        /// <summary>
        /// Код місця виготовлення (1 - Піцца, 2 - ХотДог)
        /// </summary>
        public int ProductionLocation { get; set; }
    }
}
