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

        public decimal Sum
        {
            get { return Quantity * Price; }
            set { Price = (Quantity > 0 ? value / Quantity : 0); }
        }

        public Int64 CodePS { get; set; }
        public int NumberGroup { get; set; }
        public WaresReceiptPromotion(IdReceiptWares parIdReceiptWares) :base(parIdReceiptWares)
            {
            }
        public WaresReceiptPromotion(IdReceipt parIdReceipt) : base(parIdReceipt)
        {
        }

    }
}
