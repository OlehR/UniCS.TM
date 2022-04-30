using Microsoft.Extensions.Configuration;
//using ModernExpo.SelfCheckout.Entities.Pos;
using Front.Equipments.Ingenico;
using System;
using System.Collections.Generic;
using System.Text;
using ModelMID;
using Front.Equipments.Implementation;
using Front.Equipments.Virtual;

namespace Front.Equipments
{
    public class BankTerminal:Equipment
    {
       // public BankTerminal(string pSerialPortName, int pBaudRate = 9600, Action<string, string> pLogger = null) : base(pSerialPortName, pBaudRate) { }

        public BankTerminal(Equipment pEquipment, IConfiguration pConfiguration, eModelEquipment pModelEquipment  = eModelEquipment.NotDefined, Action<string, string> pLogger = null) : base(pEquipment, pConfiguration,pModelEquipment, pLogger) { }
        
        virtual public BatchTotals PrintZ() {throw new NotImplementedException();}
        virtual public BatchTotals PrintX(){ throw new NotImplementedException();}
        public virtual Payment Purchase(decimal pAmount) { throw new NotImplementedException(); }
        public virtual Payment Refund(decimal pAmount, string pRRN) { throw new NotImplementedException(); }

        protected void SetStatus(eStatusPos pStatus)
        {
            ActionStatus?.Invoke(new PosStatus() { Status = pStatus, ModelEquipment = Model, State = (int)pStatus, TextState = pStatus.ToString() });
        }
    }
}
