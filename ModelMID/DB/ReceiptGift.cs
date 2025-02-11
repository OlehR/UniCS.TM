using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{
    public class ReceiptGift : IdReceipt
    {
        public ReceiptGift() { }
        public ReceiptGift(IdReceipt pRW) : base(pRW) { }        
        public Int64 CodePS { get; set; }
        public int NumberGroup { get; set; }
        public decimal Quantity { get; set; }
    }
}