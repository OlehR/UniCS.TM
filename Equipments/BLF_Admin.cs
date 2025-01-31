using ModelMID;
using ModelMID.DB;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Equipments
{    public partial class BLF
    {
        public Access Access = Access.GetAccess();
        public void OpenShift(User pU)
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
        }
    }
}
