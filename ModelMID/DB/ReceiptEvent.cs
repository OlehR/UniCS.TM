using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{
    public class ReceiptEvent:IdReceiptWares
    {
        public ReceiptEvent() { CreatedAt = DateTime.Now; }

        public ReceiptEvent(IdReceiptWares pRW) : base(pRW)  { CreatedAt = DateTime.Now; }
        public ReceiptEvent(IdReceipt idReceipt, int parCodeWares = 0, int parCodeUnit = 0, int parOrder = 0) : 
            base(idReceipt, parCodeWares, parCodeUnit, parOrder) 
        { CreatedAt = DateTime.Now; }

        /// <summary>
        /// Gets or sets the receipt product name.
        /// </summary>
        /// <value>The receipt product name.</value>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        /// <value>The type of the event.</value>
        public eReceiptEventType EventType { get; set; }

        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the product weight.
        /// </summary>
        /// <value>The product weight.</value>
        public int ProductWeight { get; set; }

        /// <summary>
        /// Gets or sets the product confirmed weight.
        /// </summary>
        /// <value>The product confirmed weight.</value>
        public int ProductConfirmedWeight { get; set; }
        
        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        /// <value>The user name.</value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        /// <value>The created at.</value>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the resolved at.
        /// </summary>
        /// <value>The resolved at.</value>
        public DateTime ResolvedAt { get; set; }

        /// <summary>
        /// Gets or sets the refund amount.
        /// </summary>
        /// <value>The refund amount.</value>
        public decimal RefundAmount { get; set; }

        /// <summary>
        /// Gets or sets the fiscal number.
        /// </summary>
        /// <value>The fiscal number.</value>
        public string FiscalNumber { get; set; }

        public decimal SumFiscal { get; set; }

        public eTypePayment PaymentType { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
