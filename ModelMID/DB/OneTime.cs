using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{
    
    public class OneTime : IdReceipt
    {
        public OneTime() { }
        public OneTime(IdReceipt pRW) : base(pRW) { }
        public Int64 CodePS { get; set; }
        public eStateExciseStamp State { get; set; } = eStateExciseStamp.Try;
        public eTypeCode TypeData { get; set; }
        public Int64 CodeData { get; set; }
        //public int ExData { get; set; }
    }
}
