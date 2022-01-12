using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ModelMID
{
    public class IdReceipt
    {
        protected string PrefixWarehouse { get { switch (Global.CodeWarehouse) { case 9: return "K"; case 15: return "B"; default: return "X"; } } }

        public Guid ReceiptId
        {
            get
            {                             
                var strPeriod = CodePeriod.ToString("D8").Substring(0, 4) + "-" + CodePeriod.ToString("D8").Substring(4, 4);                              
                var strGuid = IdWorkplace.ToString("D8") + "-FFFF-" + strPeriod + "-" + CodeReceipt.ToString("D12"); 
                return Guid.Parse(strGuid);
            }
            set
            {
                SetGuid(value);
                /*var IdStr = value.ToString();
                IdWorkplace = Convert.ToInt32(IdStr.Substring(0, 8));
                CodePeriod = Convert.ToInt32(IdStr.Substring(14, 9).Replace("-",""));
                CodeReceipt = Convert.ToInt32(IdStr.Substring(24, 12));*/
            }

        }

        public int IdWorkplace { get; set; }
        public int CodePeriod { get; set; }
        public int CodeReceipt { get; set; }

        public DateTime DTPeriod { get 
            {
                DateTime res;
                if (DateTime.TryParseExact(CodePeriod.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out res))
                    return res;
                return DateTime.Now.Date;            
            } 
        }
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
            SetGuid(parReceiptId);
            /*var strReceiptId = parReceiptId.ToString();
            IdWorkplace = Convert.ToInt32(strReceiptId.Substring(0, 8));
            CodePeriod = Convert.ToInt32(strReceiptId.Substring(14, 4)) * 10000 + Convert.ToInt32(strReceiptId.Substring(19, 4));
            CodeReceipt = Convert.ToInt32(strReceiptId.Substring(24, 12));
        */
        }

        public void  SetGuid(Guid parReceiptId)
        {
            var strReceiptId = parReceiptId.ToString();
            int v;
            IdWorkplace = int.TryParse(strReceiptId.Substring(0, 8), out v) ? v : 0;
            //IdWorkplace = Convert.ToInt32(strReceiptId.Substring(0, 8));

            CodePeriod = int.TryParse(strReceiptId.Substring(14, 9).Replace("-", ""), out v) ? v:0;
            //CodePeriod = Convert.ToInt32(strReceiptId.Substring(14, 4)) * 10000 + Convert.ToInt32(strReceiptId.Substring(19, 4));

            CodeReceipt = int.TryParse(strReceiptId.Substring(24, 12), out v) ? v : 0;
            //CodeReceipt = Convert.ToInt32(strReceiptId.Substring(24, 12));
        }
        public void SetIdReceipt(IdReceipt idReceipt)
        {
            if (idReceipt == null)
                return;
            IdWorkplace = idReceipt.IdWorkplace;
            CodePeriod = idReceipt.CodePeriod;
            CodeReceipt = idReceipt.CodeReceipt;
        }
        public override bool Equals(object obj)
        {
            if (obj is IdReceipt)
            {
                var o = (IdReceipt)obj;
                return IdWorkplace == o.IdWorkplace && CodePeriod == o.CodePeriod && CodeReceipt == o.CodeReceipt;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return IdWorkplace * 100000 + CodePeriod * 1000 + CodeReceipt;
        }

        public string NumberReceipt1C
        {
            get
            {
                var d = Convert.ToInt32(Math.Floor((DTPeriod - new DateTime(2019, 01, 01)).TotalDays)).ToString("D4");
                return PrefixWarehouse + Global.GetNumberCashDeskByIdWorkplace(IdWorkplace) + d + CodeReceipt.ToString("D4");
            }
        }

    }
}
