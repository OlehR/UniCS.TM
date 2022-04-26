using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Equipments.Implementation
{
    public class VirtualRRO:Rro
    {
        public VirtualRRO(IConfiguration pConfiguration, Action<string, string> pLogger = null, Action<StatusEquipment> pActionStatus = null) :
                       base(pConfiguration, eModelEquipment.VirtualRRO, pLogger, pActionStatus){ }
       
        public override LogRRO PrintCopyReceipt(int parNCopy = 1)
        {
            throw new NotImplementedException();
        }        

        override public async Task<LogRRO> PrintZAsync(IdReceipt pIdR)
        {
            return null;
        }

        override public async Task<LogRRO> PrintXAsync(IdReceipt pIdR)
        {
            return null;
        }


        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        override public async Task<LogRRO> MoveMoneyAsync(decimal pSum, IdReceipt pIdR = null)
        {
            return null;
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        override public async Task<LogRRO> PrintReceiptAsync(Receipt pR)
        {
            return null; 
        }
              

        override public bool PutToDisplay(string ptext)
        {
            return true;
        }

        override public bool PeriodZReport(DateTime pBegin, DateTime pEnd, bool IsFull = true)
        {
            return true;
        }
    }
}
