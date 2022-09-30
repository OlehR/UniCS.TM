using Front.Equipments.Implementation;
using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
// using System.Data.SQLite;
//using DatabaseLib;
namespace Front.Equipments
{
    /// <summary>
    /// Клас для роботи з касовим апаратом
    /// </summary>
    public class Rro : Equipment
    {
        protected string OperatorName;
        protected string OperatorPass = "0000";
        protected int CodeError = -1;
        protected string StrError;
        /// <summary>
        /// Чи відкрита зміна.
        /// </summary>
        public bool IsOpenWorkDay { get; set; } = false;

        protected void SetStatus(eStatusRRO pStatus,string pMsg=null,int? pMsgCode = null)
        {
            ActionStatus?.Invoke(new RroStatus() { Status = pStatus, ModelEquipment = Model, State = pMsgCode??(int)pStatus, TextState = pMsg??pStatus.ToString() });
        }

        public Rro(Equipment pEquipment, IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefine, ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) : base(pEquipment, pConfiguration,pModelEquipment, pLoggerFactory) 
        {
            ActionStatus =  pActionStatus;
        }
        
        public virtual void SetOperatorName(string pOperatorName)
        {
            OperatorName = pOperatorName;
        }

        public virtual bool OpenWorkDay()
        {
            throw new NotImplementedException();
        }

        public virtual LogRRO PrintCopyReceipt(int parNCopy=1)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<LogRRO> PrintZAsync(IdReceipt pIdR)
        {
            return null;//throw new NotImplementedException();
        }

        public virtual async Task<LogRRO>  PrintXAsync(IdReceipt pIdR)
        {
            return null;//throw new NotImplementedException();
        }

        
        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        virtual public async Task<LogRRO> MoveMoneyAsync(decimal pSum, IdReceipt pIdR=null)
        {
            return null;//throw new NotImplementedException();
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        virtual public async Task<LogRRO> PrintReceiptAsync(Receipt pR)
        {
            return null; //throw new NotImplementedException();
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        virtual public async Task<LogRRO> PrintNoFiscalReceiptAsync(IEnumerable<string> pR)
        {
            return null; //throw new NotImplementedException();
        }

        virtual public bool PutToDisplay(string ptext )
        {
            throw new NotImplementedException();
        }
        
        virtual public bool PeriodZReport(DateTime pBegin, DateTime pEnd,bool IsFull=true)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Програмування артикулів для фіскального апарата
        /// </summary>
        /// <param name="pRW"></param>
        /// <returns></returns>
        virtual public async Task<bool> ProgramingArticleAsync(IEnumerable<ReceiptWares> pRW)
        {
            return true;
        }


        public virtual async Task<string> GetTextLastReceipt()
        {
            return null;
        }



    }
}
