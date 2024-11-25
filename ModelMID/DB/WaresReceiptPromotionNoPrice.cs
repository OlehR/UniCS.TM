using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{
    public class WaresReceiptPromotionNoPrice : IdReceiptWares
    {
        public WaresReceiptPromotionNoPrice():base(){}
        public WaresReceiptPromotionNoPrice(IdReceiptWares pidRW) : base(pidRW) {}
        public long CodePS {  get; set; }
        public eTypeDiscount TypeDiscount { get; set; }
        public decimal Data { get; set; }
    }
}
