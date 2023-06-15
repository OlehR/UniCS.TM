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
        public VirtualRRO(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) :
                       base(pEquipment, pConfiguration, eModelEquipment.VirtualRRO, pLoggerFactory, pActionStatus)
        {
            State = eStateEquipment.On;
        }
       
        public override LogRRO PrintCopyReceipt(int parNCopy = 1)
        {
            throw new NotImplementedException();
        }        

        override public   LogRRO PrintZ(IdReceipt pIdR)
        {
            return new LogRRO(pIdR) { TypeOperation = eTypeOperation.ZReport, FiscalNumber = "V0001111" };
        }

        override public   LogRRO PrintX(IdReceipt pIdR)
        {
            return new LogRRO(pIdR) { TypeOperation = eTypeOperation.XReport, FiscalNumber = "V0001111" };
        }


        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        override public   LogRRO MoveMoney(decimal pSum, IdReceipt pIdR = null)
        {
            return new LogRRO(pIdR) { TypeOperation = eTypeOperation.MoneyIn, FiscalNumber = "V0001111", SUM = 1230m };
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        override public   LogRRO PrintReceipt(Receipt pR)
        {
            return new LogRRO(pR) {TypeOperation= eTypeOperation.Sale,FiscalNumber="V0001111",SUM=pR.SumReceipt,CodeError=0,Error="Проблема з лентою" }; 
        }

        override public bool PutToDisplay(string ptext, int pLine)
        {
            return true;
        }

        override public bool PeriodZReport(DateTime pBegin, DateTime pEnd, bool IsFull = true)
        {
            return true;
        }
        public override StatusEquipment TestDevice()
        {
            return new StatusEquipment(Model, State, "Ok");
        }
        
    }
}
