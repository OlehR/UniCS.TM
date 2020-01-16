using System;
using ModernIntegration.Enums;

namespace ModernIntegration.Models
{
    public class ReceiptPayment
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
        /// Gets or sets the type of the payment.
        /// </summary>
        /// <value>
        /// The type of the payment.
        /// </value>
        public PaymentType PaymentType { get; set; }

        /// <summary>
        /// Gets or sets the pay in.
        /// </summary>
        /// <value>
        /// The pay in.
        /// </value>
        public decimal PayIn { get; set; }

        /// <summary>
        /// Gets or sets the pay out.
        /// </summary>
        /// <value>
        /// The pay out.
        /// </value>
        public decimal? PayOut { get; set; }

        /// <summary>
        /// Gets or sets the card pan.
        /// </summary>
        /// <value>
        /// The card pan.
        /// </value>
        public string CardPan { get; set; }

        /// <summary>
        /// Gets or sets the is pay out success.
        /// </summary>
        /// <value>
        /// The is pay out success.
        /// </value>
        public bool? IsPayOutSuccess { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the transaction code.
        /// </summary>
        /// <value>
        /// The transaction code.
        /// </value>
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the transaction status.
        /// </summary>
        /// <value>
        /// The transaction status.
        /// </value>
        public string TransactionStatus { get; set; }


        /// <summary>
        /// код авторизації
        /// </summary>
        public string PosAuthCode  { get; set; }

        /// <summary>
        /// термінал
        /// </summary>
        public string PosTerminalId { get; set; }

        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        /// <value>
        /// The created at.
        /// </value>
        public DateTime CreatedAt { get; set; }
    }
}
