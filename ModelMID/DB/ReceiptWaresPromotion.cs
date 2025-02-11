using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    // - ділерська категорія - 2 ділерська категорія+знижка,3 -фіксація ціни,4-Обмеження по нижньому індикативу, 5-Обмеження по верхньому індикативу, 9 -акція

    public class WaresReceiptPromotion : IdReceiptWares
    {

        /// <summary>
        /// ціна за базову одиницю.
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Кількість товару
        /// </summary>
        public decimal Quantity { get; set; }

        public eTypeDiscount TypeDiscount { get; set; } = eTypeDiscount.PriceDiscount;

        public decimal _Sum = 0;
        public decimal Sum
        {
            get { return Global.RoundDown(_Sum>0?_Sum:Quantity * Price); }
            set { Price = (Quantity > 0 ? value / Quantity : 0); }
        }

        /// <summary>
        /// Приоритет спрацьованої акції
        /// </summary>
        public int Priority { get; set; }
        public Int64 CodePS { get; set; }
        public string NamePS { get; set; }
        public int NumberGroup { get; set; }
        
        public string BarCode2Category { get; set; }

        public eTypeWares TypeWares { get; set; }

        public decimal Coefficient { get; set; }

        public WaresReceiptPromotion() : base()
        {
        }
        public WaresReceiptPromotion(IdReceiptWares parIdReceiptWares) :base(parIdReceiptWares)
            {
            }
        public WaresReceiptPromotion(IdReceipt parIdReceipt) : base(parIdReceipt)
        {
        }

    }
}
