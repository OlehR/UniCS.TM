﻿using Microsoft.Extensions.Configuration;
//using ModernExpo.SelfCheckout.Entities.Pos;
//using Front.Equipments.Ingenico;
using System;
using System.Collections.Generic;
using System.Text;
using ModelMID;
using Front.Equipments.Implementation;
using Front.Equipments.Virtual;
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
            pConfiguration.GetSection($"{KeyPrefix}Merchants").Bind(Merchants);
        }

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
}
