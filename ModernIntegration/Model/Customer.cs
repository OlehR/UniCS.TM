using System;
using System.Runtime.InteropServices.ComTypes;

namespace ModernExpo.SelfCheckout.Entities.Models
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
        /// Gets or sets the loyalty points.
        /// </summary>
        /// <value>
        /// The loyalty points.
        /// </value>
        public double LoyaltyPoints { get; set; }
        /// <summary>
        /// Gets or sets the loyalty points total.
        /// </summary>
        /// <value>
        /// The loyalty points total.
        /// </value>
        public double LoyaltyPointsTotal { get; set; }
    }
}