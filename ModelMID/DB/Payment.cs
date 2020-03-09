using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class Payment:IdReceipt
    {
        public eTypePay TypePay { get; set;}
        public decimal SumPay  { get; set;}
        public decimal SumExt { get; set; }
        public string NumberTerminal { get; set; }
        public string NumberReceipt { get; set; }
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
        public DateTime DateCreate { get; set; }
        public Payment(Guid parReceipt) : base(parReceipt) { }
        public Payment(IdReceipt parIdReceipt) : base(parIdReceipt) { }
        public Payment() { }
    }
}
