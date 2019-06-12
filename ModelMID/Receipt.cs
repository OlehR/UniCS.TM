using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    /// <summary>
    /// Зберігає інформацію про чек
    /// </summary>
    public class Receipt
    {
        /// <summary>
        /// Код Чека
        /// </summary>
        public int CodeReceipt;
        /// <summary>
        /// Дата Чека
        /// </summary>
        public DateTime DateReceipt;
        /// <summary>
        /// Код періода
        /// </summary>
        public int CodePeriod;


        public decimal SumReceipt;
        //public string StSumReceipt="0.000"; //TMP test
        public decimal VatReceipt;
        public decimal SumRest;
        /// <summary>
        /// Оплачено Готівкою
        /// </summary>
        public decimal SumCash;
        /// <summary>
        /// Сума оплачена Кредиткою
        /// </summary>
        public decimal SumCreditCard;
        /// <summary>
        /// Сума використаних бонусних грн.
        /// </summary>
        public decimal SumBonus;
        public Int64 CodeCreditCard;
        public Int64 NumberSlip;
        /// <summary>
        /// Послідній рядочов в чеку
        /// </summary>
        public int Sort;
        /// <summary>
        /// Шаблони чеків. ????
        /// </summary>
        public int CodePattern;
        public Receipt()
        {
            Clear();
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


    }
}
