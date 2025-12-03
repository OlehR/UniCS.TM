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
    public class BankTerminal : Equipment
    {       

        public Action<StatusEquipment> OnStatus { get; set; }
        protected static bool CancelRequested;

        protected byte MerchantId;
        List<Merchants> Merchants = [];
        public BankTerminal(Equipment pEquipment, IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefine, ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) : 
            base(pEquipment, pConfiguration, pModelEquipment, pLoggerFactory) 
        {
            pConfiguration.GetSection($"{KeyPrefix}MerchantIds").Bind(Merchants);
            MerchantId = Convert.ToByte(Configuration[$"{KeyPrefix}MerchanId"]);
            OnStatus += pActionStatus;
        }

        public eBank CodeBank { get; set; } = eBank.NotDefine;
        public string TranslateCodeBank { get => CodeBank.GetDescription(); }

        virtual public BatchTotals PrintZ(int IdWorkPlace = 0) { throw new NotImplementedException(); }
        virtual public BatchTotals PrintX(int IdWorkPlace = 0) { throw new NotImplementedException(); }
        public virtual Payment Purchase(decimal pAmount, decimal pCash = 0, int IdWorkPlace = 0, bool pIsCashBack = false) { throw new NotImplementedException(); }
        public virtual Payment Refund(decimal pAmount, string pRRN, int IdWorkPlace = 0, bool pIsCashBack = false) { throw new NotImplementedException(); }

        public virtual IEnumerable<string> GetLastReceipt() { throw new NotImplementedException(); }

        public virtual void Cancel() { CancelRequested = true; }     

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

        protected void InvokeLastStatusMsg(byte LastStatMsgCode)
        {
            if (LastStatMsgCode == 0) return;
            eStatusPos StatusPos = eStatusPos.StatusCodeIsNotAvailable;
            if (Enum.IsDefined(typeof(eStatusPos),(int) LastStatMsgCode))
            {
                StatusPos = (eStatusPos)LastStatMsgCode;
                OnStatus?.Invoke(new PosStatus() { Status = StatusPos });
            }            
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
