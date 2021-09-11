using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{
    public class ReceiptEvent:IdReceiptWares
    {
        public ReceiptEvent() { }

        public ReceiptEvent(IdReceiptWares pRW) : base(pRW) { }
        public ReceiptEvent(IdReceipt idReceipt, int parCodeWares = 0, int parCodeUnit = 0, int parOrder = 0) : base(idReceipt, parCodeWares, parCodeUnit, parOrder) { }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>

        public Guid Id { get; set; }

        public string IdGUID { get { return Id.ToString(); }set { Id = Guid.Parse(value); } }
        /// <summary>
        /// Gets or sets the mobile device identifier.
        /// </summary>
        /// <value>The mobile device identifier.</value>
        public Guid? MobileDeviceId { get; set; }
        
        public string MobileDeviceIdGUID { get { return MobileDeviceId?.ToString(); } set { MobileDeviceId = Guid.Parse(value); } }

        
        /// <summary>
        /// Gets or sets the receipt identifier.
        /// </summary>
        /// <value>The receipt identifier.</value>
        //public Guid ReceiptId { get; set; }

        /// <summary>
        /// Gets or sets the receipt item identifier.
        /// </summary>
        /// <value>The receipt item identifier.</value>
        public Guid? ReceiptItemId { get { return WaresId; } set { if(value!=null) WaresId = value.Value; } }

        /// <summary>
        /// Gets or sets the receipt product name.
        /// </summary>
        /// <value>The receipt product name.</value>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        /// <value>The type of the event.</value>
        public ReceiptEventType EventType { get; set; }

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
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public Guid? UserId { get; set; }

        public string UserIdGUID { get { return UserId?.ToString(); } set { UserId = Guid.Parse(value); } }

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
