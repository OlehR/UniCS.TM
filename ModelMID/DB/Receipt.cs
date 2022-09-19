using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;


namespace ModelMID
{
    /// <summary>
    /// Зберігає інформацію про чек
    /// </summary>
    public class Receipt : IdReceipt
    {
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
        public Guid TerminalId { get; set; }
        int _CodeClient;
        public int CodeClient { get { return Client?.CodeClient ?? _CodeClient; } set { _CodeClient = value; } }
        public Client Client { get; set; }
        public int CodePattern { get; set; }
        public ulong NumberCashier { get; set; }
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
        /// Сума, яка фіскалізувалась на РРО
        /// </summary>
        public decimal SumFiscal { get; set; }

        // public int CodeWarehouse { get; set; }

        public decimal SumReceipt { get; set; }
        //public string StSumReceipt="0.000"; //TMP test
        public decimal VatReceipt { get; set; }
        public decimal PercentDiscount { get; set; }
        public decimal SumDiscount { get; set; }
        public decimal SumRest { get; set; }

        /// <summary>
        /// Оплачено Готівкою
        /// </summary>
        public decimal SumCash { get; set; }

        decimal _SumCreditCard;
        /// <summary>
        /// Сума оплачена Кредиткою
        /// </summary>
        public decimal SumCreditCard
        {
            get { return (Payment != null && Payment.Count() > 0 ? Payment.Sum(el => el.SumPay) : _SumCreditCard); }
            set { _SumCreditCard = value; }
        }
        /// <summary>
        /// Сума використаних бонусних грн.
        /// </summary>
        public decimal SumBonus { get; set; }

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
                if (RefundId == null)
                    return null;
                var d = Convert.ToInt32(Math.Floor((RefundId.DTPeriod - new DateTime(2019, 01, 01)).TotalDays)).ToString("D4");
                return PrefixWarehouse + Global.GetNumberCashDeskByIdWorkplace(IdWorkplaceRefund) + d + CodeReceiptRefund.ToString("D4");
            }
        }
        public int IdWorkplaceRefund { get { return RefundId == null ? 0 : RefundId.IdWorkplace; } set { if (RefundId == null) RefundId = new IdReceipt(); RefundId.IdWorkplace = value; } }
        public int CodePeriodRefund { get { return RefundId == null ? 0 : RefundId.CodePeriod; } set { if (RefundId == null) RefundId = new IdReceipt(); RefundId.CodePeriod = value; } }
        public int CodeReceiptRefund { get { return RefundId == null ? 0 : RefundId.CodeReceipt; } set { if (RefundId == null) RefundId = new IdReceipt(); RefundId.CodeReceipt = value; } }
        public IEnumerable<ReceiptWares> Wares { get; set; }
        public IEnumerable<Payment> Payment { get; set; }
        public IEnumerable<ReceiptEvent> ReceiptEvent { get; set; }

        //public bool _IsLockChange = false;
        public bool IsLockChange { get { return /*_IsLockChange ||*/ StateReceipt != eStateReceipt.Prepare || SumBonus > 0m; } }

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
        /// <summary>
        ///  Чи є товар, який потребує підтвердження віку. (0 не потребує підтверження віку)
        /// </summary>
        public decimal AgeRestrict
        {
            get
            {
                decimal Res = 0;
                if (Wares != null && Wares.Count() > 0)
                {
                    Res = Wares.Max(e => e.LimitAge);
                }
                return Res;
            }
        }
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
                }
                return Res;

            }
        }
    }
}