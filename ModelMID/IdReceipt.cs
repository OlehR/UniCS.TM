using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class IdReceipt
    {
        public Guid ReceiptId
        {
            get
            {
                var strWorkplace = new String('0', 8) + IdWorkplace.ToString();
                strWorkplace = strWorkplace.Substring(strWorkplace.Length - 8);
                var strPeriod = CodePeriod.ToString().Substring(0, 4) + "-" + (CodePeriod.ToString().Substring(4, 4) + new String('0', 4)).Substring(0, 4);

                var strCodeReceipt = new String('0', 12) + CodeReceipt.ToString();
                var strGuid = strWorkplace + "-FFFF-" + strPeriod  +"-"+ strCodeReceipt.Substring(strCodeReceipt.Length - 12);

                return Guid.Parse(strGuid);
            }
        }

        public int IdWorkplace { get; set; }
        public int CodePeriod { get; set; }
        public int CodeReceipt { get; set; }

        public IdReceipt()
        {
            IdWorkplace = 0;
            CodePeriod = 0;
            CodeReceipt = 0;

        }


        public IdReceipt(IdReceipt parIdReceipt)
        {
            SetIdReceipt(parIdReceipt);
        }

        public IdReceipt(Guid parReceiptId)
        {
            var strReceiptId = parReceiptId.ToString();
            IdWorkplace = Convert.ToInt32(strReceiptId.Substring(0, 8));
            CodePeriod = Convert.ToInt32(strReceiptId.Substring(14, 4)) * 10000 + Convert.ToInt32(strReceiptId.Substring(19, 4));
            CodeReceipt = Convert.ToInt32(strReceiptId.Substring(24, 12));
        }

        public void SetIdReceipt(IdReceipt idReceipt)
        {
            IdWorkplace = idReceipt.IdWorkplace;
            CodePeriod = idReceipt.CodePeriod;
            CodeReceipt = idReceipt.CodeReceipt;
        }
    }
}
