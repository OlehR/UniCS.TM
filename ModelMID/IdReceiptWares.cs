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
        public Guid WaresId
        {
            get
            {
                return Guid.Parse(Order.ToString("D8") + "-abcd-" + CodeUnit.ToString("D8").Substring(0, 4) + "-" + CodeUnit.ToString("D8").Substring(4, 4) + "-" + CodeWares.ToString("D12"));
            }
            set
            {
                SetIdRW(value);
                /*int Code;
                CodeWares = (int.TryParse(value.ToString().Substring(24), out Code) ? Code : 0);
                CodeUnit  = (int.TryParse(value.ToString().Substring(19, 4), out Code) ? Code : 0);*/
            }
        }
        public IdReceiptWares() { }

        public IdReceiptWares(IdReceipt idReceipt, Guid parWaresId) : base(idReceipt)
        {
            SetIdRW(parWaresId);
            /* var strWaresId = parWaresId.ToString();
             Order = Convert.ToInt32(strWaresId.Substring(0, 8));
             CodeUnit = Convert.ToInt32(strWaresId.Substring(14, 4)) * 10000 + Convert.ToInt32(strWaresId.Substring(19, 4));
             CodeWares = Convert.ToInt32(strWaresId.Substring(24, 12));*/
        }

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

        void SetIdRW(Guid parWaresId)
        {
            int Code;
            var strWaresId = parWaresId.ToString();
            Order = (int.TryParse(strWaresId.Substring(0, 8), out Code) ? Code : 0);
            CodeUnit = (int.TryParse(strWaresId.Substring(14, 9).Replace("-", ""), out Code) ? Code : 0);
            CodeWares = (int.TryParse(strWaresId.Substring(24), out Code) ? Code : 0);

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
