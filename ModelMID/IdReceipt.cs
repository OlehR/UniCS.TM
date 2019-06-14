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
                var strWorkplace = new String('0', 12) + IdWorkplace.ToString();
                strWorkplace =  strWorkplace.Substring(strWorkplace.Length - 12);
                var strPeriod = CodePeriod.ToString().Substring(0,4)+"-"+ (CodePeriod.ToString().Substring(4,4)+ new String('0', 4)).Substring(0,4);

                var strGuid = new String('0', 12) + CodeReceipt.ToString();
                strGuid = strWorkplace+ "-FFFF-"+strPeriod + GlobalVar.WaresGuid + strGuid.Substring(strGuid.Length - 12);

                return Guid.Parse(strGuid);
            }
        }

        public int IdWorkplace { get; set; }
        public int CodePeriod { get; set; }
        public int CodeReceipt { get; set; }

            
    }
}
