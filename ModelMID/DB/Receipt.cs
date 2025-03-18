using ModelMID.DB;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utils;


namespace ModelMID
{
    /// <summary>
    /// Зберігає інформацію про чек
    /// </summary>
    public class Receipt : IdReceipt
    {
        public long Id;
        private DateTime _DateReceipt;
        /// <summary>
        /// Дата Чека
        /// </summary>
        public DateTime DateReceipt
        {
            get { return (_DateReceipt != default(DateTime) ? _DateReceipt : (_DateReceipt.Date == DTPeriod ? _DateReceipt : DTPeriod)); }
            set { _DateReceipt = value.AddTicks(-(value.Ticks % TimeSpan.TicksPerSecond)); ; }
        }

        public eTypeReceipt TypeReceipt { get; set; }
        /// <summary>
        /// переклади eTypeReceipt для відображення
        /// </summary>
        public string TranslationTypeReceipt { get { return TypeReceipt.GetDescription(); } }

        long _CodeClient;
        public long CodeClient { get { return Client?.CodeClient ?? _CodeClient; } set { _CodeClient = value; } }
        public Client Client { get; set; }
        public int CodePattern { get; set; }
        public long NumberCashier { get; set; }
        public eTypeWorkplace TypeWorkplace { get; set; }
        /// <summary>
        /// Назва касира необхідно для друку на чеку.
        /// </summary>
        public string NameCashier { get; set; }

        /// <summary>
        /// 0- готується,1- оплачено,2- фіскалізовано,3 - Send
        /// </summary>
        public eStateReceipt StateReceipt { get; set; }
        /// <summary>
        /// переклади станів для відображення
        /// </summary>
        public string TranslationStateReceipt { get { return StateReceipt.GetDescription(); } }

        /// <summary>
        /// Номер чека в фіскальному реєстраторі
        /// </summary>
        public string NumberReceipt { get; set; }

        /// <summary>
        /// Номер замовлення якщо чек на основі замовлення.
        /// </summary>
        public string NumberOrder { get; set; }

        /// <summary>
        /// Сума, яка фіскалізувалась на РРО
        /// </summary>
        public decimal SumFiscal { get; set; }

        // public int CodeWarehouse { get; set; }

        public decimal SumReceipt { get; set; }
        //public string StSumReceipt="0.000"; //TMP test
        public decimal VatReceipt { get; set; }
        public decimal PercentDiscount { get; set; }
        public decimal SumDiscount { get; set; }
        /// <summary>
        /// Сума округлення фіскалки
        /// </summary>
        public decimal SumRest { get; set; }
        public decimal SumTotal { get { return SumReceipt - SumDiscount - SumBonus - SumWallet; } }
        IEnumerable<string> _BankName;
        public IEnumerable<string> BankName
        {
            get { return Payment != null && Payment.Any(el => el.TypePay == eTypePay.Card) ? Payment.Select(el=>el.CodeBank.ToString()) : new List<string>(); }
            set{ _BankName = value;}
        }
      

        decimal _SumCash;
        /// <summary>
        /// Оплачено Готівкою
        /// </summary>
        public decimal SumCash
        {
            get { return (Payment != null && Payment.Any(el => el.TypePay == eTypePay.Cash) ? Payment.Where(el => el.TypePay == eTypePay.Cash).Sum(el => el.SumPay) : _SumCash); }
            set { _SumCash = value; }
        }

        decimal _SumWallet = 0;
        /// <summary>
        /// Списані/нараховані гроші скарбничка
        /// </summary>
        public decimal SumWallet { get { return Payment?.Any() == true ? Payment.Where(el => el.TypePay == eTypePay.Wallet)?.Sum(el => el.SumPay) ?? 0 : _SumWallet; } set { _SumWallet = value; } }

        decimal _SumCreditCard;
        /// <summary>
        /// Сума оплачена Кредиткою
        /// </summary>
        public decimal SumCreditCard
        {
            get { return (Payment != null && Payment.Any(el => el.TypePay == eTypePay.Card) ? Payment.Where(el => el.TypePay == eTypePay.Card).Sum(el => el.SumPay) : _SumCreditCard); }
            set { _SumCreditCard = value; }
        }

        decimal _SumBonus = 0m;
        /// <summary>
        /// Сума використаних бонусних грн.
        /// </summary>
        public decimal SumBonus { get { return _SumBonus > 0 && _Wares == null ? _SumBonus : _Wares?.Sum(el => el.SumBonus) ?? 0m; } set { _SumBonus = value; } }

        public string CodeCreditCard { get; set; }
        public string NumberSlip { get; set; }
        public long NumberReceiptPOS { get; set; }

        public DateTime DateCreate { get; set; }
        public long UserCreate { get; set; }

        public decimal AdditionN1 { get; set; }
        public decimal AdditionN2 { get; set; }
        public decimal AdditionN3 { get; set; }
        public string AdditionC1 { get; set; }
        public DateTime AdditionD1 { get; set; }

        public IdReceipt RefundId { get; set; }

        public string RefundNumberReceipt1C
        {
            get
            {
                //if (RefundId == null) return null;
                //var d = Convert.ToInt32(Math.Floor((RefundId.DTPeriod - new DateTime(2019, 01, 01)).TotalDays)).ToString("D4");
                return RefundId?.NumberReceipt1C; // Prefix + d + CodeReceiptRefund.ToString("D4");//PrefixWarehouse + GlobaQl.GetNumberCashDeskByIdWorkplace(IdWorkplaceRefund)
            }
        }
        public int IdWorkplaceRefund { get { return RefundId == null ? 0 : RefundId.IdWorkplace; } set { if (RefundId == null) RefundId = new IdReceipt(); RefundId.IdWorkplace = value; } }
        public int CodePeriodRefund { get { return RefundId == null ? 0 : RefundId.CodePeriod; } set { if (RefundId == null) RefundId = new IdReceipt(); RefundId.CodePeriod = value; } }
        public int CodeReceiptRefund { get { return RefundId == null ? 0 : RefundId.CodeReceipt; } set { if (RefundId == null) RefundId = new IdReceipt(); RefundId.CodeReceipt = value; } }
        public IEnumerable<ReceiptWares> _Wares;
        public IEnumerable<ReceiptWares> Wares
        {
            get
            {
                IEnumerable<ReceiptWares> res = null;
                if (IdWorkplacePay == 0 || _Wares == null)
                    res = _Wares;
                else
                    res = _Wares.Where(el => el.IdWorkplacePay == IdWorkplacePay && el.Quantity != 0m);
                if (IdWorkplacePay == IdWorkplace || IdWorkplacePay==0)
                {
                    decimal SumWallet = Math.Round(Payment?.Where(r => r.TypePay == eTypePay.Wallet).Sum(r => r.SumPay) ?? 0, 2);
                    if (SumWallet < 0)
                    {
                        var r = res==null?new List<ReceiptWares>():res.ToList();
                        r.Add(new ReceiptWares(this)
                        { CodeWares = Global.Settings.CodeWaresWallet, IdWorkplacePay=IdWorkplace, Quantity = 1, CodeUnit = 19, CodeDefaultUnit = 19, Sum = -SumWallet, PriceDealer = -SumWallet, NameWares = "Скарбничка", TypeVat = 2, PercentVat = 20 });
                        res = r;
                    }
                }
                return res;
            }
            set { _Wares = value?.Where(el => el.CodeWares != Global.Settings?.CodeWaresWallet).ToList(); }
        }

        public IEnumerable<Payment> _Payment;
        public IEnumerable<Payment> Payment { get { return IdWorkplacePay == 0 || _Payment == null ? _Payment : _Payment.Where(el => el.IdWorkplacePay == IdWorkplacePay); } set { _Payment = value; } }
       
        public IEnumerable<ReceiptEvent> ReceiptEvent { get; set; }
        public IEnumerable<OneTime> OneTime { get; set; }
        public IEnumerable<ReceiptWaresPromotionNoPrice> ReceiptWaresPromotionNoPrice { get; set; }

        //public bool _IsLockChange = false;
        public bool IsLockChange { get { return /*_IsLockChange ||*/ StateReceipt != eStateReceipt.Prepare || SumBonus > 0m; } }


        //public string FiscalQR { get { return Fiscal?.QR ; } }

        /// <summary>
        /// Фіскальний номер апарата
        /// </summary>
        //public string FiscalId { get { return Fiscal?.Id; } }


        //public IEnumerable<TaxResult> _Taxes;
        /// <summary>
        /// Податки отриманні з фіскалки після фіскалізації
        /// </summary>
        //public IEnumerable<TaxResult> Taxes { get { return Fiscal?.Taxes; } }

        /// <summary>
        /// Чи є підтвердження обмеження віку.
        /// </summary>
        public bool IsConfirmAgeRestrict
        {
            get
            {
                bool Res = false;
                if (ReceiptEvent != null)
                {
                    var res = ReceiptEvent.Where(e => e.EventType == eReceiptEventType.AgeRestrictedProduct);
                    Res = res.Any();
                }
                return Res;
            }
        }

        // public string FiscalsJSON { get { return Fiscals.ToJSON(); } }
        public SortedList<int, Fiscal> Fiscals = new SortedList<int, Fiscal>();

        public int CountWeightGoods { get
            {
                int Res = 0;
                if (Wares != null && Wares.Any(el => el.IsWeight))
                {
                    Res = Wares.Where(x => x.IsWeight).Sum(el => el.HistoryQuantity.Count() > 0 ? el.HistoryQuantity.Count() : 1);
                }
                return Res;
            } }

        public bool IsPakagesAded
        {
            get
            {
                bool Res = false;
                if (ReceiptEvent != null)
                {
                    var res = ReceiptEvent.Where(e => e.EventType == eReceiptEventType.PackagesBag);
                    Res = res.Any();
                }
                return Res;
            }
        }
        public Fiscal Fiscal { get { if (Fiscals.ContainsKey(IdWorkplacePay)) return Fiscals[IdWorkplacePay]; return null; } }
        /// <summary>
        ///  Чи є товар, який потребує підтвердження віку. (0 не потребує підтверження віку)
        /// </summary>
        public decimal AgeRestrict { get { return _Wares?.Any() == true ? _Wares?.Max(e => e.LimitAge) ?? 0 : 0; } }

        public bool IsOnlyOrdinary { get { return _Wares?.Any(e => e.TypeWares != eTypeWares.Ordinary) == false; } }

        public Receipt()
        {
            Clear();
        }
        public Receipt(IdReceipt parId)
        {
            IdWorkplace = parId.IdWorkplace;
            CodePeriod = parId.CodePeriod;
            CodeReceipt = parId.CodeReceipt;
        }

        public void Clear()
        {
            CodeReceipt = 0;  // Код Чека
            DateReceipt = new DateTime(1, 1, 1);  // Дата Чека
            CodePeriod = 0;  // Код періода
            //Sort = 0;            // Послідній рядочов в чеку
            CodePattern = 0;
            SumReceipt = 0;
            VatReceipt = 0;
            SumCash = 0;
            SumCreditCard = 0;
            CodeCreditCard = null;
            SumBonus = 0;
            NumberSlip = null;
        }

        public void SetIdReceipt(IdReceipt idReceipt)
        {
            base.SetIdReceipt(idReceipt);
            if (Wares != null)
                foreach (var el in Wares)
                    el.SetIdReceipt(idReceipt);
            if (Payment != null)
                foreach (var el in Payment)
                    el.SetIdReceipt(idReceipt);
            if (ReceiptEvent != null)
                foreach (var el in ReceiptEvent)
                    el.SetIdReceipt(idReceipt);
        }

        public void SetReceipt(int parCodeReceipt, DateTime parDateReceipt = new DateTime())
        {
            if (parDateReceipt == new DateTime())
                parDateReceipt = DateTime.Today;

            Clear();
            CodeReceipt = parCodeReceipt;
            //Global.Receipts[0] = parCodeReceipt; //tmp щоб зберегти глобально номер чека.???
            DateReceipt = parDateReceipt;
            //CodePeriod = Global.GetCodePeriod(parDateReceipt);
            //Sort = 0;
            //CodePattern = Global.DefaultCodePatternReceipt;
        }

        /*public IdReceipt GetIdReceipt()
        {
            return (IdReceipt)this; //new IdReceipt() { CodePeriod=this.}
        }*/
        public ReceiptWares GetLastWares
        {
            get
            {
                var e = Wares?.Where(el => el.IsLast);
                if (e != null && e.Count() == 1)
                    return e.First();
                return null;
            }
        }

        public bool IsNeedExciseStamp { get { return GetLastWares?.IsNeedExciseStamp == true; } }
        /// <summary>
        /// Вага власної сумки.
        /// </summary>
        public double OwnBag { get { return ReceiptEvent?.Sum(r => Convert.ToDouble(r.ProductConfirmedWeight)) ?? 0d; } }

        public WorkplacePay WorkplacePay { get { return WorkplacePays?.Where(el => el.IdWorkplacePay == IdWorkplacePay)?.FirstOrDefault(); } }

        public WorkplacePay[] WorkplacePays { get; set; }

        public List<string> ReceiptComments
        {
            get
            {
                List<string> Res = new List<string>() { NumberReceipt1C };
                if (Client != null)
                {
                    if (!string.IsNullOrEmpty(Client.NameClient))
                        Res.Add(Client.NameClient);
                    if (Client.SumBonus > 0)
                        Res.Add($"Бонуси:{Client.SumBonus}");
                    if (Client.Wallet > 0)
                        Res.Add($"Скарбничка:{Client.Wallet}");
                    if (SumBonus > 0)
                        Res.Add($"Списані бонусні грн:{SumBonus}");
                }
                return Res;
            }
        }

        public IEnumerable<string> Footer { get { return Global.GetWorkPlaceByIdWorkplace(IdWorkplacePay > 0 ? IdWorkplacePay : IdWorkplace)?.Settings?.Footer; } }

        public bool IsQR()
        {
            return Wares?.Where(r => !string.IsNullOrEmpty(r.QR)).Any() ?? false;
        }

        public IEnumerable<ReceiptWares> GetParserWaresReceipt(bool pIsPrice = true, bool pIsExcise = true, bool pIsWeight = false)
        {
            if ((!pIsPrice && !pIsExcise) || Wares == null)
                return Wares;
            IEnumerable<ReceiptWares> Res = Wares;
            if (pIsPrice)
            {
                var res = new List<ReceiptWares>();
                foreach (var el in Res)
                    res.AddRange(el.ParseByPrice());
                Res = res;
            }

            if (pIsExcise)
            {
                var res = new List<ReceiptWares>();
                foreach (var el in Res)
                    res.AddRange(el.ParseByExcise());
                Res = res;
            }

            if (pIsWeight)
            {
                var res = new List<ReceiptWares>();
                foreach (var el in Res)
                    res.AddRange(el.ParseByWeight());
                Res = res;
            }

            //FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"ReceiptWares=>{Res}");
            return Res;
        }
        public bool IsUseBonus { get { return Wares?.Where(el => el.TypeWares != eTypeWares.Ordinary).Any() == true; } }
        public decimal MaxSumWallet { get { return Math.Round(Wares?.Where(el => el.TypeWares == eTypeWares.Ordinary).Sum(el => el.SumTotal * 0.25m) ?? 0, 2); } }

        public int[] IdWorkplacePays { get { return _Wares?.Select(el => el.IdWorkplacePay).Distinct().OrderBy(el => el).ToArray() ?? Array.Empty<int>(); } }

        public IEnumerable<LogRRO> LogRROs;
        //public string FiscalReceipt { get { return LogRROs?.Where(el => el.IdWorkplacePay == IdWorkplacePay && el.TypeOperation == eTypeOperation.Sale)?.FirstOrDefault()?.FiscalNumber ?? NumberReceipt; } }
        public bool ReCalcWallet()
        {
            SumWallet = Payment?.Where(r => r.TypePay == eTypePay.Wallet).Sum(r => r.SumPay) ?? 0;
            if (SumWallet > 0)
            {
                var OrdinaryWares = Wares.Where(el => el.TypeWares == eTypeWares.Ordinary);
                decimal Sum = OrdinaryWares.Sum(el => el.SumTotal);
                foreach (var el in OrdinaryWares)
                    el.SumWallet = Math.Round(el.SumTotal * SumWallet / Sum, 2);
                decimal SumW = OrdinaryWares.Sum(el => el.SumWallet);
                if (SumW != SumWallet)
                {
                    var Wr = OrdinaryWares.FirstOrDefault(el => el.SumWallet >= SumW - SumWallet) ?? OrdinaryWares.First();
                    Wr.SumWallet += (SumWallet - SumW);
                }
            }
            else
            if (SumWallet < 0)
            {
            }
            return SumWallet > 0;
        }

        public bool ReCalcBonus()
        {
            var SumBonusPay = Payment?.Where(r => r.TypePay == eTypePay.Bonus).Sum(r => r.SumExt) ?? 0;

            if (SumBonusPay > 0)
            {
                var NotOrdinaryWares = Wares.Where(el => el.TypeWares != eTypeWares.Ordinary);
                if (!NotOrdinaryWares.Any())
                {
                    decimal SumTotal = 0m;
                    foreach (var el in Wares)
                    {
                        el.SumBonus = 0;
                        el.SumBonus = el.SumTotal - 0.01m;
                        SumTotal += el.SumTotal;
                        SumBonus += el.SumBonus;
                    }
                    if (SumTotal < 0.06m)
                    {
                        var Wr = Wares.First();
                        decimal delta = (0.06m - SumTotal);
                        Wr.SumBonus -= delta;
                        SumBonus -= delta;
                    }
                }
            }
            return SumBonus > 0;
        }

        [JsonIgnore]
        public bool IsPrintIssueOfCash { get { return (Payment != null && Payment.Any(el => el.TypePay == eTypePay.IssueOfCash)) && (LogRROs == null || !LogRROs.Any(el => el.TypeOperation == eTypeOperation.IssueOfCash)); } }
        [JsonIgnore]
        public Payment IssueOfCash { get { return Payment?.Where(el => el.TypePay == eTypePay.IssueOfCash).FirstOrDefault(); } }

        public eTypePay TypePay { get { return Payment != null && Payment.Any(el => el.TypePay == eTypePay.Card || el.TypePay == eTypePay.Cash) ? Payment.FirstOrDefault(el => el.TypePay == eTypePay.Card || el.TypePay == eTypePay.Cash).TypePay : eTypePay.None; } }

        public bool IsManyPayments { get { return IdWorkplacePays?.Length > 1; } }

        public bool IsWaresLink { get { return GetLastWares?.IsWaresLink == true; } }

        public bool IsTobacco { get { return Wares?.Any(el => el.TypeWares == eTypeWares.Tobacco || el.TypeWares == eTypeWares.TobaccoNoExcise) == true; } }
        public bool IsAlcohol { get { return Wares?.Any(el => el.TypeWares == eTypeWares.Alcohol || el.TypeWares == eTypeWares.LowAlcohol || el.TypeWares == eTypeWares.Beer) == true; } }
    }
    public class WorkplacePay
    {
        public int IdWorkplacePay { get; set; }
        public decimal SumCash { get; set; }
        public decimal Sum { get; set; }
    }

    public class TaxResult
    {
        public int IdWorkplacePay { get; set; }
        public string Name { get; set; }
        public decimal Sum { get; set; }
    }
    public class Fiscal
    {
        public int IdWorkplacePay { get; set; }
        /// <summary>
        /// Номер фіскалки
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Фіскальний номер чека
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// Заголовок чека (Компанія адреса)
        /// </summary>
        public string Head { get; set; }
        /// <summary>
        /// Фіскалізована сума
        /// </summary>
        public decimal Sum { get; set; }
        /// <summary>
        /// Здача
        /// </summary>
        public decimal SumRest { get; set; }
        /// <summary>
        /// QR код для чека
        /// </summary>
        public string QR { get; set; }
        /// <summary>
        /// Час чека
        /// </summary>
        public DateTime DT { get; set; }
        public IEnumerable<TaxResult> Taxes { get; set; }       
    }
}