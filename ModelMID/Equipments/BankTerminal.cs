using Microsoft.Extensions.Configuration;
//using ModernExpo.SelfCheckout.Entities.Pos;
//using Front.Equipments.Ingenico;
using System;
using System.Collections.Generic;
using ModelMID;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Front.Equipments
{
    public class BankTerminal : Equipment
    {
        
        protected byte MerchantId;
        List<Merchants> Merchants = new List<Merchants>();
        public BankTerminal(Equipment pEquipment, IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefine, ILoggerFactory pLoggerFactory = null) : 
            base(pEquipment, pConfiguration, pModelEquipment, pLoggerFactory) 
        {
            pConfiguration.GetSection($"{KeyPrefix}MerchantIds").Bind(Merchants);
        }

        public eBank CodeBank { get; set; } = eBank.NotDefine;

        virtual public BatchTotals PrintZ(int IdWorkPlace = 0) { throw new NotImplementedException(); }
        virtual public BatchTotals PrintX(int IdWorkPlace = 0) { throw new NotImplementedException(); }
        public virtual Payment Purchase(decimal pAmount, decimal pCash = 0, int IdWorkPlace = 0) { throw new NotImplementedException(); }
        public virtual Payment Refund(decimal pAmount, string pRRN, int IdWorkPlace = 0) { throw new NotImplementedException(); }

        public virtual IEnumerable<string> GetLastReceipt() { throw new NotImplementedException(); }

        public virtual void Cancel() { throw new NotImplementedException(); }

        protected void SetStatus(eStatusPos pStatus)
        {
            ActionStatus?.Invoke(new PosStatus() { Status = pStatus, ModelEquipment = Model, State = (int)pStatus, TextState = pStatus.ToString() });
        }

        public byte GetMechantIdByIdWorkPlace(int pIdWorkPlace)
        {
            if(Merchants!=null)
            {
                var res=Merchants.Where(el=> el.IdWorkplace== pIdWorkPlace).FirstOrDefault();
                if (res != null)
                    return res.MerchantId;
            }
            return this.MerchantId;
        }
    }

    public class Merchants
    {
        public int IdWorkplace { get; set; }
        public byte MerchantId { get; set; }
    }
    public class BatchTotals
    {
        public uint DebitCount { get; set; }

        public uint DebitSum { get; set; }

        public uint CreditCount { get; set; }

        public uint CreditSum { get; set; }

        public uint CencelledCount { get; set; }

        public uint CencelledSum { get; set; }
        /// <summary>
        /// Чек построчно.
        /// </summary>
        public IEnumerable<string> Receipt { get; set; }
    }
}
