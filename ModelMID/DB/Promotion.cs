using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class PromotionSale
    {
        int CodePS { get; set; }
        string NamePS { get; set; }
        int CodePattern { get; set; }
        StatePromotionSale State { get; set; }
        DateTime DateBegin { get; set; }
        DateTime DateEnd { get; set; }
        /// <summary>
        /// Тип акції(1-На товари,2-на весь набір, 3-на товари в наборі)
        /// </summary>
        int Type  { get; set; } 
        /// <summary>
        /// 0-товари, 1-товари, 2-бренди, 3-групи товарів, 4-альтернативні товарні ієрархії, 5-альтернативні групові ієрархії, 6-категорії товарів,7- конвертовані властивості товарів) (select t.*, t.rowid from C.DATA_NAME t where t.data_level=53)
        /// </summary>
        int TypeData { get; set; } 
        int Priority { get; set; }
        decimal SumOrder { get; set; }
        TypeWorkCoupon TypeWorkCoupon { get; set; }
        string BarCodeCoupon { get; set; }
        DateTime DateCreate { get; set; }
        int UserCreate { get; set; }
    }
    public class PromotionSaleData
    {
        int CodePS { get; set; }
        int NumberGroup { get; set; }
        /// <summary>
        /// Код товару, 0 - на всі товари згідно фільтрів. (тип знижки 1 заборонено)
        /// </summary>
        int CodeWares { get; set; }

        bool UseIndicative { get; set; }
        /// <summary>
        /// Тип знижки (1-ціна,2-знижка,3-%знижки, 4- заміна ДК) і тд (select t.*, t.rowid from C.DATA_NAME t where t.data_level=50)
        /// </summary>
        int TypeDiscount { get; set; }
        /// <summary>
        /// 0- на кожну позицію, 1-на кожну n- позицію, до n позиції,після n-кількості (n-data_ADDITIONAL_CONDITION )  (select t.*, t.rowid from C.DATA_NAME t where t.data_level=51)
        /// </summary>
        int AdditionalCondition { get; set; }
        /// <summary>
        /// власне ціна, знижка , ...
        /// </summary>
        decimal Data { get; set; }//власне ціна, знижка , ...
        /// <summary>
        /// кількість для ADDITIONAL_CONDITION
        /// </summary>
        decimal DataAdditionalCondition { get; set; }
        DateTime DateCreate { get; set; }
        int UserCreate { get; set; }
    }

    public class PromotionSaleFilter
    {
        int CodePS { get; set; }
        int CodeGroupFilter { get; set; }
        /// <summary>
        /// Тип (10- товари 20-Час,30-клієнт,4-Номер чека, 50-склади, 60-Форма оплати, ) ( select t.*, t.rowid from C.DATA_NAME t where t.data_level=53)
        /// </summary>
        int TypeGroupFilter { get; set; }
        /// <summary>
        /// Правило групи ( 1- логіче &  , -1 - Заперечення)
        /// </summary>
        RuleGroup RuleGroupFilter { get; set; }

        //int CodeProporty { get; set; }
        int? CodeChoice { get; set; }
        decimal Data { get; set; }
        decimal? DataEnd { get; set; }
        
        DateTime DateCreate { get; set; }
        int UserCreate { get; set; }
    }

    /*
   public class PromotionSaleGiff
    {
        int CodePS { get; set; }
        int CodeGroup { get; set; }
        /// <summary>
        /// Тип (10- товари 20-Час,30-клієнт,4-Номер чека, 50-склади, 60-Форма оплати, ) ( select t.*, t.rowid from C.DATA_NAME t where t.data_level=53)
        /// </summary>
        int TypeGroupFilter { get; set; }
        /// <summary>
        /// Правило групи ( 1- логіче &  , -1 - Заперечення)
        /// </summary>
        RuleGroup RuleGroupFilter { get; set; }

        DateTime DateCreate { get; set; }
        int UserCreate { get; set; }
    }
    */


    public class PromotionSaleDealer
    {
        public Int64 CodePS { get; set; }
        public int CodeWares { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }
        public int CodeDealer { get; set; }
        public DateTime DateCreate { get; set; }
        public int UserCreate { get; set; }
     
    }




}
