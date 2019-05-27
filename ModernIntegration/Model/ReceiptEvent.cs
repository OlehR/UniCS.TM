using System;
using System.Collections.Generic;
using System.Text;
using ModernExpo.SelfCheckout.Entities.Enums;
using ModernExpo.SelfCheckout.Entities.Session;
using ModernExpo.SelfCheckout.Entities.ViewModels;

namespace ModernExpo.SelfCheckout.Entities.Models
{
    /// <summary>
    /// ReceiptEvent
    /// </summary>
    public class ReceiptEvent
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the receipt identifier.
        /// </summary>
        /// <value>
        /// The receipt identifier.
        /// </value>
        public Guid ReceiptId { get; set; }

        /// <summary>
        /// Gets or sets the receipt item identifier.
        /// </summary>
        /// <value>
        /// The receipt item identifier.
        /// </value>
        public Guid? ReceiptItemId { get; set; }
        
        /// <summary>
        /// Gets or sets the receipt product name.
        /// </summary>
        /// <value>
        /// The receipt product name.
        /// </value>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        public ReceiptEventType EventType { get; set; }

        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        /// <value>
        /// The name of the event.
        /// </value>
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public Guid? UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        /// <value>
        /// The user name.
        /// </value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        /// <value>
        /// The created at.
        /// </value>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the resolved at.
        /// </summary>
        /// <value>
        /// The resolved at.
        /// </value>
        public DateTime ResolvedAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiptEvent"/> class.
        /// </summary>
        public ReceiptEvent()
        {
            EventName = EventType.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiptEvent" /> class.
        /// </summary>
        /// <param name="eventElement">The event element.</param>
        /// <param name="receiptId">The receipt identifier.</param>
        /// <param name="receiptItemId">The receipt item identifier.</param>
        public ReceiptEvent(SessionProductEvent eventElement, Guid receiptId, Guid? receiptItemId)
        {
            Id = Guid.NewGuid();
            ReceiptId = receiptId;
            ReceiptItemId = receiptItemId;
            EventType = eventElement.EventType;
            EventName = eventElement.EventType.ToString();
            UserId = eventElement.UserId;
            CreatedAt = eventElement.CreatedAt;
            ResolvedAt = eventElement.ResolvedAt.Value;
        }
    }
}
