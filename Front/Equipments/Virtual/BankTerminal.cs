using Microsoft.Extensions.Configuration;
//using ModernExpo.SelfCheckout.Entities.Pos;
using Front.Equipments.Ingenico;
using System;
using System.Collections.Generic;
using System.Text;
using ModelMID;
using Front.Equipments.Implementation;
using Front.Equipments.Virtual;
using Microsoft.Extensions.Logging;

namespace Front.Equipments
{
    public class BankTerminal:Equipment
    {
       // public BankTerminal(string pSerialPortName, int pBaudRate = 9600, Action<string, string> pLogger = null) : base(pSerialPortName, pBaudRate) { }

        public BankTerminal(Equipment pEquipment, IConfiguration pConfiguration, eModelEquipment pModelEquipment  = eModelEquipment.NotDefine, ILoggerFactory pLoggerFactory = null) : base(pEquipment, pConfiguration,pModelEquipment, pLoggerFactory) { }
        
        virtual public BatchTotals PrintZ() {throw new NotImplementedException();}
        virtual public BatchTotals PrintX(){ throw new NotImplementedException();}
        public virtual Payment Purchase(decimal pAmount,decimal pCash=0) { throw new NotImplementedException(); }
        public virtual Payment Refund(decimal pAmount, string pRRN) { throw new NotImplementedException(); }

        public virtual IEnumerable<string> GetLastReceipt() { throw new NotImplementedException(); }

        public virtual void Cancel() { throw new NotImplementedException(); }

        protected void SetStatus(eStatusPos pStatus)
        {
            ActionStatus?.Invoke(new PosStatus() { Status = pStatus, ModelEquipment = Model, State = (int)pStatus, TextState = pStatus.ToString() });
        }
    }
}
