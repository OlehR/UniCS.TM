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
        /// Код Чека
        /// </summary>
        //public int CodeReceipt;

        /// <summary>
        /// Код періода
        /// </summary>
        //public int CodePeriod;
        /// <summary>
        /// Дата Чека
        /// </summary>
        public DateTime DateReceipt { get; set; }

        public Guid TerminalId { get; set; }
        public int CodeClient { get; set; }
        public int CodePattern { get; set; }
        public int NumberCashier { get; set; }
        /// <summary>
        /// Номер чека в фіскальному реєстраторі
        /// </summary>
        public string NumberReceipt { get; set; }
        public int CodeWarehouse { get; set; }

        public decimal SumReceipt { get; set; }
        //public string StSumReceipt="0.000"; //TMP test
        public decimal VatReceipt { get; set; }
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
        /// <summary>
        /// Послідній рядочов в чеку
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// Шаблони чеків. ????
        /// </summary>
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
            Sort = 0;            // Послідній рядочов в чеку
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
            Sort = 0;
            //CodePattern = Global.DefaultCodePatternReceipt;
        }
        public IdReceipt GetIdReceipt()
        {
            return (IdReceipt)this; //new IdReceipt() { CodePeriod=this.}
        }


    }
}
