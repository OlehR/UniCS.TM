using ModernIntegration.Models;
using System;

namespace ModernIntegration.ViewModels
{
    /// <summary>
    /// Summary description for CustomerViewModel
    /// </summary>
    public class CustomerViewModel
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
        public string CustomerId { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
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
        public decimal Bonuses { get; set; }
        /// <summary>
        /// Gets or sets the loyalty points total.
        /// </summary>
        /// <value>
        /// The loyalty points total.
        /// </value>
        public decimal Wallet { get; set; }
        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>The phone number.</value>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerViewModel"/> class.
        /// </summary>
        /// <param name="customer">The customer.</param>
        public CustomerViewModel(Customer customer)
        {
            Id = customer.Id;
            CustomerId = customer.CardNumber;
            Name = customer.FullName;
            DiscountPercent = customer.DiscountPercent;
            Bonuses = customer.Bonuses;
            Wallet = customer.Wallet;
            PhoneNumber = customer.PhoneNumber;
        }

        public CustomerViewModel()
        {

        }
        /// <summary>
        /// To the user.
        /// </summary>
        /// <returns></returns>
        public Customer ToCustomer()
        {
            return new Customer
            {
                CardNumber = CustomerId,
                FullName = Name,
                DiscountPercent = DiscountPercent,
                Bonuses = Bonuses,
                Wallet = Wallet,
                PhoneNumber = PhoneNumber
            };
        }

        public CustomerViewModel Clone()
        {
            return new CustomerViewModel
            {
                Id = Id,
                CustomerId = CustomerId,
                DiscountPercent = DiscountPercent,
                Bonuses = Bonuses,
                Wallet = Wallet,
                Name = Name,
                PhoneNumber = PhoneNumber
            };
        }
    }
}
