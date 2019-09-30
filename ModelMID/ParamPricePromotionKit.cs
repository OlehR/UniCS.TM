using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class ParamPricePromotionKit:IdReceipt
    {
        public int CodeWarehouse { get; set; }
        public ParamPricePromotionKit(IdReceipt parIdReceipt, int parCodeWarehouse) : base(parIdReceipt)
        {
            CodeWarehouse = parCodeWarehouse;
        }
    }
}
