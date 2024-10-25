using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{
    public class ReceiptOneTimePromotion : IdReceipt
    {
        public ReceiptOneTimePromotion() { }
        public ReceiptOneTimePromotion(IdReceipt pRW) : base(pRW) { }
        public int CodeClient { get; set; }
        public Int64 CodePS { get; set; }
    }
}