using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class Payment:IdReceipt,ICloneable
    {        
        public bool IsSuccess { get; set; }
        public eTypePay TypePay { get; set; } = eTypePay.Card;
        public decimal SumPay  { get; set;}
        /// <summary>
        /// Здача(для готівкових операцій)
        /// </summary>
        public decimal Rest { get; set;}
        public decimal SumExt { get; set; }
        public string NumberTerminal { get; set; }
        /// <summary>
        /// Номер чека
        /// </summary>
        public long NumberReceipt { get; set; }
        /// <summary>
        /// RRN
        /// </summary>
        public string CodeAuthorization { get; set; }
        public string NumberSlip { get; set; }
        public string NumberCard { get; set; }
        /// <summary>
        /// Сума знятих коштів(сума - Сума бонусів (Приват) )
        /// </summary>
        public decimal PosPaid { get; set; }
        /// <summary>
        /// Сума бонусів (Приват)
        /// </summary>
        public decimal PosAddAmount { get; set; }
        /// <summary>
        /// Власник картки
        /// </summary>
        public string CardHolder { get; set; } //НОВЕ! 
        /// <summary>
        /// Платіжна система
        /// </summary>
        public string IssuerName { get; set; } //НОВЕ!
        /// <summary>
        /// Банк еквайр
        /// </summary>
        public string Bank { get; set; } //НОВЕ!
        /// <summary>
        /// Ймовірно транзакція в межах відкритої зміни
        /// </summary>
        public string  TransactionId { get; set; } //НОВЕ!
        
        public DateTime DateCreate { get; set; }

        public string TransactionStatus { get; set; }

        //public string Error { get; set; } = null;
        public IEnumerable<string> Receipt { get; set; }

        public Payment(Guid parReceipt) : base(parReceipt) { }
        public Payment(IdReceipt parIdReceipt) : base(parIdReceipt) { }
        public Payment() { }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
