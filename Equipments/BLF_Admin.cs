using ModelMID;
using ModelMID.DB;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace Front.Equipments
{    public partial class BLF
    {
        public Access Access = Access.GetAccess();
        public string OpenShift(User pU)
        {            
            MW.AdminSSC = pU;
            if (Global.TypeWorkplace == eTypeWorkplace.CashRegister)
                Access.СurUser = pU;
            MW.DTAdminSSC = DateTime.Now;
            Bl.db.SetConfig<DateTime>("DateAdminSSC", DateTime.Now);
            Bl.db.SetConfig<string>("CodeAdminSSC", pU.BarCode);
            Bl.StartWork(Global.IdWorkPlace, pU.BarCode);
            
            if (MW.State == eStateMainWindows.WaitAdmin)
                SetStateView(eStateMainWindows.StartWindow);
            if (!AsyncHelper.RunSync(() => MW.Bl.OpenCloseShiftAsync()))
                return "Не вдалось відкрити зміну в 1С";
            return null;
        }

        public string CloseShift()
        {
            if (!MW.IsOpenReceipt)
            {
                bool res = AsyncHelper.RunSync(() => MW.Bl.OpenCloseShiftAsync(false));
                if (!res)
                    return "Не вдалось закрити зміну в 1С";
                MW.AdminSSC = null;
                MW.Bl.db.SetConfig<string>("CodeAdminSSC", string.Empty);
                MW.Bl.StoptWork(Global.IdWorkPlace);
                return null;
            }
            else
            {
                MW.SetStateView(eStateMainWindows.StartWindow);               
                return "Існує відкритий чек! Для закриття зміни потрібно закрити всі чеки!";
            }
        }

    }
}
