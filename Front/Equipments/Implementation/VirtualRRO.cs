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
        public VirtualRRO(Equipment pEquipment, IConfiguration pConfiguration, Action<string, string> pLogger = null, Action<StatusEquipment> pActionStatus = null) :
                       base(pEquipment, pConfiguration, eModelEquipment.VirtualRRO, pLogger, pActionStatus)
        {
            State = eStateEquipment.On;
        }
       
        public override LogRRO PrintCopyReceipt(int parNCopy = 1)
        {
            throw new NotImplementedException();
        }        

        override public async Task<LogRRO> PrintZAsync(IdReceipt pIdR)
        {
            return new LogRRO(pIdR) { TypeOperation = eTypeOperation.ZReport, FiscalNumber = "V0001111" };
        }

        override public async Task<LogRRO> PrintXAsync(IdReceipt pIdR)
        {
            return new LogRRO(pIdR) { TypeOperation = eTypeOperation.XReport, FiscalNumber = "V0001111" };
        }


        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        override public async Task<LogRRO> MoveMoneyAsync(decimal pSum, IdReceipt pIdR = null)
        {
            return new LogRRO(pIdR) { TypeOperation = eTypeOperation.MoneyIn, FiscalNumber = "V0001111", SUM = 1230m };
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        override public async Task<LogRRO> PrintReceiptAsync(Receipt pR)
        {
            return new LogRRO(pR) {TypeOperation= eTypeOperation.Sale,FiscalNumber="V0001111",SUM=pR.SumReceipt }; 
        }
              

        override public bool PutToDisplay(string ptext)
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
        public override string GetDeviceInfo()
        {
            return $"pModelEquipment={Model} State={State} Port={SerialPort} BaudRate={BaudRate}{Environment.NewLine}";
        }
    }
}
