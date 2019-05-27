using System;
using System.Collections.Generic;
using System.Text;
using ModernExpo.SelfCheckout.Entities.Enums;

namespace ModernExpo.SelfCheckout.Entities.Session
{
    /// <summary>
    /// SessionEvents
    /// </summary>
    public class SessionProductEvent
    {
        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        /// <value>
        /// The product identifier.
        /// </value>
        public Guid? ProductId { get; set; }

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        public ReceiptEventType EventType { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        /// <value>
        /// The full name of the user.
        /// </value>
        public string UserFullName { get; set; }

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
        public DateTime? ResolvedAt { get; set; }
    }
}
