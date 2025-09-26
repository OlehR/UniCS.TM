using Microsoft.Extensions.Configuration;
//using ModernExpo.SelfCheckout.Entities.Pos;
//using Front.Equipments.Ingenico;
using System;
using System.Collections.Generic;
using ModelMID;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using Utils;

namespace Front.Equipments
{
    public class CashMachine : Equipment
    {       
        public Action<StatusEquipment> OnStatus { get; set; }
        protected static bool CancelRequested;

        protected byte MerchantId;
        List<Merchants> Merchants = [];
        public CashMachine(Equipment pEquipment, IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefine, ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) : 
            base(pEquipment, pConfiguration, pModelEquipment, pLoggerFactory) 
        {
            pConfiguration.GetSection($"{KeyPrefix}MerchantIds").Bind(Merchants);
            MerchantId = Convert.ToByte(Configuration[$"{KeyPrefix}MerchanId"]);
            OnStatus += pActionStatus;
        }
       
        virtual public BatchTotals PrintZ(int IdWorkPlace = 0) { throw new NotImplementedException(); }
        virtual public BatchTotals PrintX(int IdWorkPlace = 0) { throw new NotImplementedException(); }
        public virtual Payment Purchase(decimal pAmount, decimal pCash = 0, int IdWorkPlace = 0) { throw new NotImplementedException(); }
        public virtual Payment Refund(decimal pAmount, string pRRN, int IdWorkPlace = 0) { throw new NotImplementedException(); }

        public virtual IEnumerable<string> GetLastReceipt() { throw new NotImplementedException(); }

        public virtual void Cancel() { CancelRequested = true; }     

        protected void SetStatus(eStatusPos pStatus)
        {
            ActionStatus?.Invoke(new PosStatus() { Status = pStatus, ModelEquipment = Model, State = (int)pStatus, TextState = pStatus.ToString() });
        }
       
    }
    
}
