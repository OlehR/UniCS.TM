using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class ParamMoveReceipt:IdReceipt
    {
        public int NewIdWorkplace { get; set; }
        public int NewCodePeriod { get; set; }
        public int NewCodeReceipt { get; set; }
        public ParamMoveReceipt(IdReceipt parIdReceipt) : base(parIdReceipt)
        {
        }
    }
}
