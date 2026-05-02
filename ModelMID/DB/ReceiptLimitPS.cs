using System;
using System.Collections.Generic;
using Model;

namespace ModelMID.DB
{
    
    public class ReceiptLimitPS : IdReceipt
    {
        public ReceiptLimitPS() { }
        public ReceiptLimitPS(IdReceipt pRW) : base(pRW) { }
        public Int64 CodePS { get; set; }
        public Int64 CodeClient { get; set; }
        public Int64 CodeWares { get; set; }
        public decimal Data { get; set; }
    }
}
