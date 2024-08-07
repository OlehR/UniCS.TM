using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class IdReceiptWares : IdReceipt
    {        
        public int CodeWares { get; set; }
        public int CodeUnit { get; set; }
        public int Order { get; set; }
        public Receipt Parent { get; set; }       
        public IdReceiptWares() { }
        
        public IdReceiptWares(IdReceiptWares pIdReceiptWares) : base(pIdReceiptWares)
        {
            CodeWares = pIdReceiptWares.CodeWares;
            CodeUnit = pIdReceiptWares.CodeUnit;
            Order = pIdReceiptWares.Order;
        }

        public IdReceiptWares(IdReceipt idReceipt, int parCodeWares = 0, int parCodeUnit = 0, int parOrder = 0) : base(idReceipt)
        {
            CodeWares = parCodeWares;
            CodeUnit = parCodeUnit;
            Order = parOrder;
            Parent = idReceipt as Receipt;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is IdReceiptWares)
                {
                var o = (IdReceiptWares)obj;
                return CodeWares == o.CodeWares && base.Equals(o);
            }
            return false;
        }
        public void SetIdReceiptWares(IdReceiptWares idReceiptWares)
        {
            SetIdReceipt((IdReceipt)idReceiptWares);
            CodeWares = idReceiptWares.CodeWares;
            CodeUnit = idReceiptWares.CodeUnit;
            Order = idReceiptWares.Order;

        }
        public override int GetHashCode()
        {
            return IdWorkplace * 10000+ /*CodePeriod * 1000 +*/ CodeReceipt +10000* CodeWares;
            //2147 483 648
        }
    }
}
