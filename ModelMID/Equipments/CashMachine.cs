using Microsoft.Extensions.Configuration;
//using ModernExpo.SelfCheckout.Entities.Pos;
//using Front.Equipments.Ingenico;
using System;
using System.Collections.Generic;
using ModelMID;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModelMID.DB;
using Utils;

namespace Front.Equipments
{
    public class CashMachine : Equipment
    {       
        public Action<StatusEquipment> OnStatus { get; set; }
        protected static bool CancelRequested;

        public CashMachine(Equipment pEquipment, IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefine, ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) : 
            base(pEquipment, pConfiguration, pModelEquipment, pLoggerFactory) 
        {
            OnStatus += pActionStatus;
        }
       

        public  virtual Payment Purchase(decimal pAmount, int IdWorkPlace = 0) { throw new NotImplementedException(); }//StartCashin
        public virtual Payment Refund(decimal pAmount,  int IdWorkPlace = 0) { throw new NotImplementedException(); } //Cashout
        public virtual CashMachineStatus Cashout(decimal pAmount) { throw new NotImplementedException(); }
        public virtual List<CashInventory> Inventory() {  throw new NotImplementedException(); }
        public virtual Task<List<CashInventory>> InventoryAsync()
        {
            return Task.FromResult(Inventory());
        }
        public virtual CashMachineStatus GetStatus() { throw new NotImplementedException(); }
        public virtual bool UnLockUnit(eTypeUnit pTypeUnit) { throw new NotImplementedException(); }
        public virtual bool LockUnit(eTypeUnit pTypeUnit) { throw new NotImplementedException(); }
        public virtual CashMachineStatus StartReplenishment() {  throw new NotImplementedException(); }
        public virtual CashMachineStatus EndReplenishment() { throw new NotImplementedException(); }
        public virtual CashMachineStatus Collect(List<CashInventory> pCashInventories, eTypeCollectMoney pTypeCollectMoney) {  throw new NotImplementedException(); }

        public virtual void Cancel() { CancelRequested = true; }     

        protected void SetStatus(eStatusChangeEvent pStatus)
        {
            ActionStatus?.Invoke(new CashMachineStatus() { Status = pStatus, ModelEquipment = Model, State = (int)pStatus, TextState = pStatus.ToString() });
        }
       
    }
    
}
