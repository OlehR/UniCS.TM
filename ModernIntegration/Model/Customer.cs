using System;
using System.Runtime.InteropServices.ComTypes;

namespace ModernIntegration.Models
{
    /// <summary>
    /// Summary description for StoreCustomer
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; set; }
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string CardNumber { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string FullName { get; set; }
        /// <summary>
        /// Gets or sets the discount.
        /// </summary>
        /// <value>
        /// The discount.
        /// </value>
        public double DiscountPercent { get; set; }
        /// <summary>
        /// Gets or sets the bonuses.
        /// </summary>
        /// <value>
        /// The bonuses.
        /// </value>
        public decimal Bonuses { get; set; }
        /// <summary>
        /// Gets or sets the wallet balance.
        /// </summary>
        /// <value>
        /// The wallet balance.
        /// </value>
        public decimal Wallet { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>The phone number.</value>
        public string PhoneNumber { get; set; }
    }
}