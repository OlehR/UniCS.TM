using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{
    public class ReceiptWaresPromotionNoPrice : IdReceiptWares
    {
        public ReceiptWaresPromotionNoPrice():base(){}
        public ReceiptWaresPromotionNoPrice(IdReceiptWares pidRW) : base(pidRW) {}
        public long CodePS {  get; set; }
        public eTypeDiscount TypeDiscount { get; set; }
        public decimal Data { get; set; }
        public decimal DataEx { get; set; }
    }
}
