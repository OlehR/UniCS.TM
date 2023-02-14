using Front.Equipments.Implementation;
using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelMID;
using ModelMID.DB;
using ModernExpo.SelfCheckout.Utils;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        public int IdWorkplacePay;

        string DefaultTax=null;
        SortedList<int, string> Tax = new SortedList<int, string>();
        /// <summary>
        /// Чи відкрита зміна.
        /// </summary>
        public bool IsOpenWorkDay { get; set; } = false;

        protected void SetStatus(eStatusRRO pStatus, string pMsg = null, int? pMsgCode = null)
        {
            ActionStatus?.Invoke(new RroStatus() { Status = pStatus, ModelEquipment = Model, State = pMsgCode ?? (int)pStatus, TextState = pMsg ?? pStatus.ToString() });
        }

        public Rro(Equipment pEquipment, IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefine, ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) : base(pEquipment, pConfiguration, pModelEquipment, pLoggerFactory)
        {
            ActionStatus = pActionStatus;
            try
            {
                IdWorkplacePay = Convert.ToInt32(Configuration[$"{KeyPrefix}IdWorkplacePay"]);
            }
            catch (Exception ex) { IdWorkplacePay = Global.IdWorkPlace; }
            if (IdWorkplacePay == 0)
                IdWorkplacePay = Global.IdWorkPlace;

            DefaultTax = Configuration[$"{KeyPrefix}DefaultTax"];
            var LTax = new List<TAX>();
            Configuration.GetSection($"{KeyPrefix}Tax").Bind(LTax);
            if (LTax.Count() == 0)
                Configuration.GetSection("MID:VAT").Bind(LTax);
            foreach (var el in LTax)
                    if (!Tax.ContainsKey(el.Code))
                        Tax.TryAdd(el.Code, el.CodeEKKA);                    
        }

        public virtual void SetOperatorName(string pOperatorName)
        {
            OperatorName = pOperatorName;
        }

        public virtual bool OpenWorkDay()
        {
            throw new NotImplementedException();
        }

        public virtual LogRRO PrintCopyReceipt(int parNCopy = 1)
        {
            throw new NotImplementedException();
        }

        public virtual LogRRO PrintZ(IdReceipt pIdR)
        {
            return null;//throw new NotImplementedException();
        }

        public virtual LogRRO PrintX(IdReceipt pIdR)
        {
            return null;//throw new NotImplementedException();
        }


        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        virtual public LogRRO MoveMoney(decimal pSum, IdReceipt pIdR = null)
        {
            return null;//throw new NotImplementedException();
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        virtual public LogRRO PrintReceipt(Receipt pR)
        {
            return null; //throw new NotImplementedException();
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        virtual public LogRRO PrintNoFiscalReceipt(IEnumerable<string> pR)
        {
            return null; //throw new NotImplementedException();
        }

        virtual public bool PutToDisplay(string ptext)
        {
            throw new NotImplementedException();
        }

        virtual public bool PeriodZReport(DateTime pBegin, DateTime pEnd, bool IsFull = true)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Програмування артикулів для фіскального апарата
        /// </summary>
        /// <param name="pRW"></param>
        /// <returns></returns>
        virtual public bool ProgramingArticle(ReceiptWares pRW)
        {
            return true;
        }


        public virtual string GetTextLastReceipt()
        {
            return null;
        }

        public virtual decimal SumReceiptFiscal(Receipt pR)
        {
            decimal sum = 0;
            if (pR != null && pR.Wares != null && pR.Wares.Any())            
                 sum = pR.IdWorkplacePay == 0  ? pR.Wares.Sum(r => (r.SumTotal)): pR.Wares.Where(r=>r.IdWorkplacePay== pR.IdWorkplacePay).Sum(r => (r.SumTotal));
             //decimal sum = pR.Wares.Sum(el => Math.Round(el.Price * el.Quantity, 2) - Math.Round(el.SumDiscount, 2)); //pR.SumTotal;
            return sum; //throw new NotImplementedException();
        }

        public virtual decimal SumCashReceiptFiscal(Receipt pR)
        {
            decimal sum = SumReceiptFiscal(pR);
            return Math.Round(sum, 1);
        }
        /// <summary>
        /// Зупиняє останню довготривалу операцію. Наприклад Отримання текста чеку на фізичних фіскалках. 
        /// </summary>
        virtual public void Stop() { }

       /// <summary>
       /// Отримуємо Суму з текста чека.
       /// </summary>
       /// <returns></returns>
        public virtual decimal GetSumFromTextReceipt(string pTextReceipt) { return 0; }

        public void ReCalc(Receipt pR)
        {
            var SumWallet = pR.Payment?.Where(r => r.TypePay == eTypePay.Wallet).Sum(r=>r.SumPay) ?? 0;
            if(SumWallet>0)
            {
                var OrdinaryWares = pR.Wares.Where(el => el.TypeWares == eTypeWares.Ordinary);
                decimal Sum = OrdinaryWares.Sum(el=>el.SumTotal);
                foreach(var el in pR.Wares)
                    el.SumWallet=Math.Round( el.SumTotal*  SumWallet/ Sum,2);
                decimal SumW = OrdinaryWares.Sum(el => el.SumWallet);
                if (SumW != SumWallet)
                {
                    var Wr = OrdinaryWares.First();
                    Wr.SumWallet += (SumWallet - SumW);
                }
                BL Bl = BL.GetBL;
                foreach(var el in OrdinaryWares)
                    Bl.db.ReplaceWaresReceipt(el);

            }else 
            if(SumWallet < 0)
            {
                pR.Wares.Add(new ReceiptWares(pR) 
                    { CodeWares = Global.CodeWaresWallet, Quantity = 1, CodeUnit=19,CodeDefaultUnit=19 ,Sum = -SumWallet, NameWares = "Скарбничка",TypeVat=0,PercentVat=20 });
            }

        }

        public string TaxGroup(ReceiptWares pRW) 
        {
            int Key = (int)pRW.TypeWares * 10 + pRW.TypeVat;
            if (Tax.ContainsKey(Key))            
                return Tax[Key];
            return DefaultTax;
        }
            
        

    }
}
