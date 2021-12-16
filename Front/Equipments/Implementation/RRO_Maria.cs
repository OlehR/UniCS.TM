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
    public class RRO_Maria:Rro
    {
        public RRO_Maria(IConfiguration pConfiguration, Action<string, string> pLogger = null, Action<eStatusRRO> pActionStatus = null) : base(pConfiguration,pLogger, pActionStatus)
        {
           
        }

        public override void SetOperatorName(string pOperatorName)
        {
            OperatorName = pOperatorName;
        }

        public override LogRRO PrintCopyReceipt(int parNCopy = 1)
        {
            throw new NotImplementedException();
        }


        
        override public async Task<LogRRO> PrintZAsync(IdReceipt pIdR)
        {
            return null;//throw new NotImplementedException();
        }

        override public async Task<LogRRO> PrintXAsync(IdReceipt pIdR)
        {
            return null;//throw new NotImplementedException();
        }

        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        override public LogRRO MoveMoney(decimal pSum)
        {
            return null;//throw new NotImplementedException();
        }

        override public async Task<LogRRO> MoveMoneyAsync(decimal pSum, IdReceipt pIdR)
        {
            return null;//throw new NotImplementedException();
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

    }
}
