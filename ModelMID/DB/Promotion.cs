using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class PromotionSale
    {
        public Int64 CodePS { get; set; }
        string _NamePS;
        public string NamePS { get { return string.IsNullOrEmpty(_NamePS) ? CodePS.ToString() : _NamePS.Trim(); } set { _NamePS = value; } }
        public int CodePattern { get; set; }
        public eStatePromotionSale State { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }
        /// <summary>
        /// Тип акції(1-На товари,2-на весь набір, 3-на товари в наборі)
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 0-товари, 1-товари, 2-бренди, 3-групи товарів, 4-альтернативні товарні ієрархії, 5-альтернативні групові ієрархії, 6-категорії товарів,7- конвертовані властивості товарів) (select t.*, t.rowid from C.DATA_NAME t where t.data_level=53)
        /// </summary>
        public int TypeData { get; set; }
        public int Priority { get; set; }
        public decimal SumOrder { get; set; }
        public eTypeWorkCoupon TypeWorkCoupon { get; set; }
        public string BarCodeCoupon { get; set; }
        public DateTime DateCreate { get; set; }
        public int UserCreate { get; set; }
        public bool IsOneTime { get; set; }
    }
    public class PromotionSaleData
    {
        public Int64 CodePS { get; set; }
        public int NumberGroup { get; set; }
        /// <summary>
        /// Код товару, 0 - на всі товари згідно фільтрів. (тип знижки 1 заборонено)
        /// </summary>
        public int CodeWares { get; set; }

        public bool UseIndicative { get; set; }
        /// <summary>
        /// Тип знижки (11-ціна,12-знижка,13-%знижки, 14- заміна ДК,41-Подарок) і тд (select t.*, t.rowid from C.DATA_NAME t where t.data_level=50)
        /// </summary>
        public eTypeDiscount TypeDiscount { get; set; }
        /// <summary>
        /// 0- на кожну позицію, 1-на кожну n- позицію, до n позиції,після n-кількості (n-data_ADDITIONAL_CONDITION )  (select t.*, t.rowid from C.DATA_NAME t where t.data_level=51)
        /// </summary>
        public int AdditionalCondition { get; set; }
        /// <summary>
        /// власне ціна, знижка , ...
        /// </summary>
        public decimal Data { get; set; }//власне ціна, знижка , ...
        /// <summary>
        /// кількість для ADDITIONAL_CONDITION
        /// </summary>
        public decimal DataAdditionalCondition { get; set; }
        public DateTime DateCreate { get; set; }
        public int UserCreate { get; set; }
        public string DataText { get; set; }
    }

    public class PromotionSaleFilter
    {
        public Int64 CodePS { get; set; }
        public int CodeGroupFilter { get; set; }
        /// <summary>
        /// Тип (10- товари,11- по товару, 12-Товари від кількості, 15-по групі, 20-Час,30-клієнт,4-Номер чека, 50-склади, 60-Форма оплати,70- Купони ) ( select t.*, t.rowid from C.DATA_NAME t where t.data_level=53)
        /// </summary>
        public int TypeGroupFilter { get; set; }
        /// <summary>
        /// Правило групи ( 1- логіче &  , -1 - Заперечення)
        /// </summary>
        public eRuleGroup RuleGroupFilter { get; set; }

        //int CodeProporty { get; set; }
        public int CodeChoice { get; set; }
        public decimal CodeData { get; set; }
        public decimal? CodeDataEnd { get; set; }

        public DateTime DateCreate { get; set; }
        public int UserCreate { get; set; }
    }

    
   public class PromotionSaleGift
    {
        public Int64 CodePS { get; set; }
        public int NumberGroup { get; set; }
        /// <summary>
        /// Код товару
        /// </summary>
        public int CodeWares { get; set; }
        /// <summary>
        /// Тип знижки (11-ціна,12-знижка,13-%знижки, 14- заміна ДК,41-Подарок) і тд (select t.*, t.rowid from C.DATA_NAME t where t.data_level=50)
        /// </summary>
        public eTypeDiscount TypeDiscount { get; set; }
        public decimal Data { get; set; }//власне ціна, знижка , ...
        public decimal Quantity { get; set; }

        public DateTime DateCreate { get; set; }
        public int UserCreate { get; set; }
    }
    


    public class PromotionSaleDealer
    {
        public Int64 CodePS { get; set; }
        public int CodeWares { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }
        public int CodeDealer { get; set; }        
        public int Priority { get; set; }
        public DateTime DateCreate { get; set; }
        public int UserCreate { get; set; }
        /// <summary>
        /// Максимальна кількість для якої працює знижку.
        /// </summary>
        public decimal MaxQuantity { get; set; }
    }


    public class PromotionSaleGroupWares
    {
        public int CodeGroupWaresPS { get; set; }
        public int CodeGroupWares { get; set; }
    }

    public class PromotionSale2Category
    {
        public Int64 CodePS { get; set; }
        public int CodeWares { get; set; }
    }

    public class ParameterPromotion:IdReceipt
    {
        public ParameterPromotion(IdReceipt pIR):base(pIR){ }
        public ParameterPromotion() : base() { }
        public int CodeWarehouse { get; set; }
        public int TypeCard { get; set; }
        public int Time { get; set; }
        public DateTime BirthDay { get; set; }
        public int CodeWares { get; set; }
        public int CodeDealer { get; set; }
        public int CodeClient { get; set; }
        /// <summary>
        /// Кількість товару
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Цінова акція
        /// </summary>
        public bool IsPricePromotion { get; set; } = true;
    }

    public class PricePromotion
    {
        public Int64 CodePs { get; set; }
        public int Priority { get; set; }
        public eTypeDiscount TypeDiscount { get; set; }
        public decimal Data { get; set; }        
        public decimal Price { get; set; }
        public int IsIgnoreMinPrice { get; set; }
        public decimal MaxQuantity { get; set; }
        public bool IsOneTime { get; set; }
        public string DataText { get; set; }
        public decimal CalcPrice(decimal parPrice,bool IsUsePrice=true)
        {
            //decimal curPrice = 0;
            switch (TypeDiscount)
            {
                case eTypeDiscount.Price:
                case eTypeDiscount.ReplacePriceDealer:
                    return ( (Data > 0 && (Data < parPrice || !IsUsePrice)) ? Data : parPrice);
                case eTypeDiscount.PriceDiscount:
                    return parPrice - Data;
                case eTypeDiscount.PercentDiscount:
                    return parPrice * (100m - Data) / 100m;
                default:
                    return parPrice;
            }
        }            
    }

    public class PromotionWaresKit
    {
        public Int64 CodePS { get; set; }

        public int NumberGroup { get; set; }

        public int CodeWares { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get { return (TypeDiscount == eTypeDiscount.Price? DataDiscount:0); }  }
        
       
        public eTypeDiscount TypeDiscount { get; set; }

        /// <summary>
        /// Власне знижка ціна тощо.
        /// </summary>
        public decimal DataDiscount { get; set; }
    }
    //psd.CODE_PS,psd.DATA,psfw.code_data as Code_wares,psd.Number_group

}
