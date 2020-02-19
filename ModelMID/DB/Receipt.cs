using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    /// <summary>
    /// Зберігає інформацію про чек
    /// </summary>
    public class Receipt : IdReceipt
    {
        /// <summary>
        /// Дата Чека
        /// </summary>
        public DateTime DateReceipt { get; set; }

        public eTypeReceipt TypeReceipt { get; set; }
        public Guid TerminalId { get; set; }
        public int CodeClient { get; set; }
        public int CodePattern { get; set; }
        public int NumberCashier { get; set; }
        public string NumberReceipt1C { get {
              
                var d = Convert.ToInt32(Math.Floor((DateReceipt - new DateTime(2019, 01, 01)).TotalDays)).ToString("D4");
                return Global.PrefixWarehouse + Global.GetNumberCashDeskByIdWorkplace(IdWorkplace) + d + CodeReceipt.ToString("D4"); 
            } }///Придуати номер каси.


        /// <summary>
        /// 0- готується,1- оплачено,2- фіскалізовано,3 - Send
        /// </summary>
        public eStateReceipt StateReceipt { get; set; }

        /// <summary>
        /// Номер чека в фіскальному реєстраторі
        /// </summary>
        public string NumberReceipt { get; set; }
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
        /// <summary>
        /// Сума оплачена Кредиткою
        /// </summary>
        public decimal SumCreditCard { get; set; }
        /// <summary>
        /// Сума використаних бонусних грн.
        /// </summary>
        public decimal SumBonus { get; set; }
        public Int64 CodeCreditCard { get; set; }
        public Int64 NumberSlip { get; set; }

        public DateTime DateCreate { get; set; }
        public int UserCreate { get; set; }

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
                var d = Convert.ToInt32(Math.Floor((RefundId.DTPeriod  - new DateTime(2019, 01, 01)).TotalDays)).ToString("D4");
                return Global.PrefixWarehouse + Global.GetNumberCashDeskByIdWorkplace(IdWorkplaceRefund) + d + CodeReceiptRefund.ToString("D4");
            }
        }
        public int IdWorkplaceRefund { get { return RefundId == null ? 0 : RefundId.IdWorkplace; } set { if (RefundId == null) RefundId = new IdReceipt(); RefundId.IdWorkplace = value;  } }
        public int CodePeriodRefund { get { return RefundId == null ? 0 : RefundId.CodePeriod; } set { if (RefundId == null) RefundId = new IdReceipt(); RefundId.CodePeriod = value; } }
        public int CodeReceiptRefund { get { return RefundId == null ? 0 : RefundId.CodeReceipt; } set { if (RefundId == null) RefundId = new IdReceipt(); RefundId.CodeReceipt = value; } }
        public IEnumerable<ReceiptWares> Wares { get; set; }
        public IEnumerable<Payment> Payment { get; set; }
        public Receipt()
        {
            Clear();
        }
        public Receipt(IdReceipt parId)
        {
            IdWorkplace = parId.IdWorkplace;
            CodePeriod  = parId.CodePeriod;
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
            CodeCreditCard = 0;
            SumBonus = 0;
            NumberSlip = 0;

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
        public IdReceipt GetIdReceipt()
        {
            return (IdReceipt)this; //new IdReceipt() { CodePeriod=this.}
        }


    }
}
