using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
// using System.Data.SQLite;
//using DatabaseLib;
namespace Front.Equipments
{
    /// <summary>
    /// Клас для роботи з касовим апаратом
    /// </summary>
    public class Rro : Equipment
    {
        /// <summary>
        /// Серійний номер фіскалки.
        /// </summary>
        public string SerialNumber = null;
        protected string OperatorName;
        protected string OperatorPass = "0000";
        protected int CodeError = -1;
        protected string StrError;
        public int IdWorkplacePay;
        protected volatile bool IsStop = false;

        public DateTime LockDT { get; set; }
        public eTypeOperation TypeOperation { get; set; } = eTypeOperation.NotDefine;

        string DefaultTax = null;
        SortedList<int, string> Tax = new SortedList<int, string>();

        public eTypePay TypePay { get; set; }
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
                IdWorkplacePay = Configuration.GetValue<int>($"{KeyPrefix}IdWorkplacePay");
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
                    Tax.Add(el.Code, el.CodeEKKA);
            TypePay = Configuration.GetValue<eTypePay>($"{KeyPrefix}TypePay", eTypePay.None);
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

        virtual public bool OpenMoneyBox(int pTime = 15) { return false; }

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

        virtual public bool PutToDisplay(string ptext, int pLine = 1)
        {
            throw new NotImplementedException();
        }

        virtual public bool PeriodZReport(IdReceipt pIdR, DateTime pBegin, DateTime pEnd, bool IsFull = true)
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


        public virtual string GetTextLastReceipt(bool IsZReport = false)
        {
            return null;
        }

        public virtual decimal SumReceiptFiscal(Receipt pR)
        {
            decimal sum = 0;
            if (pR != null && pR.Wares != null && pR.Wares.Any())
                sum = pR.IdWorkplacePay == 0 ? pR.Wares.Sum(r => (r.SumTotal)) : pR.Wares.Where(r => r.IdWorkplacePay == pR.IdWorkplacePay).Sum(r => (r.SumTotal));
            //decimal sum = pR.Wares.Sum(el => Math.Round(el.Price * el.Quantity, 2) - Math.Round(el.SumDiscount, 2)); //pR.SumTotal;
            return sum; //throw new NotImplementedException();
        }

        public virtual decimal SumCashReceiptFiscal(Receipt pR)
        {
            decimal sum = SumReceiptFiscal(pR);
            return GetSumRoundCash(sum);
        }
        public static decimal GetSumRoundCash(decimal sum)
        {
            decimal R = 0;            
            sum = Math.Round(sum, 2);
            int C = (int)sum;
            decimal m = sum - C;
            if (m >= 0 && m <= 0.24m) R = 0;
            else
                if (m > 0.24m && m <= 0.74m) R = 0.5m;
            else
                if (m > 0.74m && m < 1m) R = 1m;
            decimal Res = ((decimal)C) + R;
            return Res < 0.5m ? 0.5M : Res;
        }
        /// <summary>
        /// Зупиняє останню довготривалу операцію. Наприклад Отримання текста чеку на фізичних фіскалках. 
        /// </summary>
        virtual public void Stop() { IsStop = true; }

        /// <summary>
        /// Отримуємо Суму з текста чека.
        /// </summary>
        /// <returns></returns>
        public virtual decimal GetSumFromTextReceipt(string pTextReceipt) { return 0; }

        public virtual void GetFiscalInfo(Receipt pR, object pRes) { }

        public string TaxGroup(ReceiptWares pRW)
        {
            int Key;
            if (pRW.TypeWares == eTypeWares.LowAlcohol)
                Key = (int)eTypeWares.Alcohol * 10 + pRW.TypeVat;
            else
                Key = (int)pRW.TypeWares * 10 + pRW.TypeVat;
            if (Tax.ContainsKey(Key))
                return Tax[Key];
            return DefaultTax??"-99";
        }

        /// <summary>
        /// Сума гоівки в касі
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        //public virtual decimal GetSumInCash() { throw new NotImplementedException(); }
        /// <summary>
        /// Сума гоівки в касі віртуальне РРО
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual decimal GetSumInCash(IdReceipt pIdR) { throw new NotImplementedException(); }
        /// <summary>


        /// <summary>
        /// Видача готівки з картки
        /// </summary>
        /// <param name="pP"></param>
        /// <returns></returns>
        public virtual LogRRO IssueOfCash(Receipt pR) { throw new NotImplementedException(); }
        /// <summary>
        /// Звіт X,Z з тексту чи JSON
        /// </summary>
        /// <param name="pTXT"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public ReportXZ GetReport(string pTXT) { throw new NotImplementedException(); }
    }

    public class SumReport
    {
        public eTypePay TypePay { get; set; }
        public eTypeReceipt TypeReceipt { get; set; }
        public decimal Sum { get; set; }
        /// <summary>
        /// Сума завкруглення в верхню сторону.
        /// </summary>
        public decimal SumUpRound { get; set; }
        /// <summary>
        /// Сума завкруглення в нижню сторону.
        /// </summary>
        public decimal SumDownRound { get; set; }
    }

    public class TaxReport
    {
        public int TypeVAT { get; set; }
        public string Name { get; set; }
        public string TaxLit { get; set; }
        public eTypeReceipt TypeReceipt { get; set; }
        public decimal BaseSum { get; set; }
        public decimal TaxSum { get; set; }
        public decimal ExTaxSum { get; set; }
    }

    public class ReportXZ
    {
        /// <summary>
        /// Чи звіт є Z звітом.
        /// </summary>
        public bool IsZ { get; set; }
        DateTime Date { get; set; }
        public IEnumerable<SumReport> Pay { get; set; }
        public IEnumerable<TaxReport> Tax { get; set; }
        public decimal SumCashRefund { get; set; }
        public decimal SumCashSale { get; set; }
        public decimal SumCardRefund { get; set; }
        public decimal SumCardSale { get; set; }
        /// <summary>
        /// Сума видачі готівки.
        /// </summary>
        public decimal SumIssueOfCash { get; set; }
        /// <summary>
        /// Чеків продаж
        /// </summary>
        public int CountSale { get; set; }
        /// <summary>
        /// Чеків повернення
        /// </summary>
        public int CountRefund { get; set; }
        /// <summary>
        /// Чеків видачі готівки
        /// </summary>
        public int CountIssueOfCash { get; set; }
        /// <summary>
        /// Службове внесення
        /// </summary>
        public decimal MoveMoneyIn { get; set; }
        /// <summary>
        /// Службове Вилучення
        /// </summary>
        public decimal MoveMoneyOut { get; set; }
    }
}
