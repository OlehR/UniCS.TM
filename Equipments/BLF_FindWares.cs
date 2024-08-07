using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelMID;
using SharedLib;
using Utils;

namespace Front.Equipments
{
    public partial class BLF
    {
        string LastStr = null;
        public IEnumerable<GW>? GetDataFindWares(int pCodeFastGroup,string pWaresName,IdReceipt pIdR ,ref int pOffSet,ref int pMaxPage,ref int pLimit)
        {
            IEnumerable<GW>? WG = null;
            if (pCodeFastGroup == 0 && pWaresName.Length == 0)
            {
                var a = Bl.db.GetFastGroup(Global.CodeWarehouse);
                var aa=a?.Select(r => new GW(r))?.ToList();
                pMaxPage = aa.Count() / pLimit;
                for (int i = 0; i < pLimit * pOffSet; i++)
                    aa.RemoveAt(0);
                WG = aa;
            }
            else
            {
                if (pWaresName != LastStr)
                {
                    LastStr = pWaresName;
                    pOffSet = 0;
                }
                WG = Bl.GetProductsByName(pIdR, (pWaresName.Length > 1 ? pWaresName : ""), pOffSet * pLimit, pLimit, pCodeFastGroup)?.Select(r => new GW(r));
                if (WG != null)
                    pMaxPage = WG.First().TotalRows / pLimit;
                else
                    pMaxPage = 0;
            }
            return WG;
        }
        
        public decimal GetQuantity(string pS,int pCodeUnit)
        {
            decimal Quantity = 0m;
            if (pS.IndexOf('*') > 0 || pS.IndexOf('=') > 0)
            {
                var s = pS.Split(pS.IndexOf('*') > 0 ? '*' : '=');
                Quantity = s[0].ToDecimal() * (pCodeUnit == Global.WeightCodeUnit ? 1000 : 1);
                if (pCodeUnit != Global.WeightCodeUnit)
                {
                    Quantity = Math.Round(Quantity, 0);
                    if (Quantity == 0) Quantity=1m;
                }
            }
            return pCodeUnit != Global.WeightCodeUnit && Quantity == 0 ?1:Quantity;
        }
    }
}
