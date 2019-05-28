using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModernExpo.SelfCheckout.Entities.ViewModels;

namespace ModernExpo.SelfCheckout.Entities.Session
{
    public class SessionProducts
    {
        /// <summary>
        /// Gets or sets the products.
        /// </summary>
        /// <value>
        /// The products.
        /// </value>
        public List<ProductViewModel> Products { get; set; }

        /// <summary>
        /// Gets or sets the total amount.
        /// </summary>
        /// <value>
        /// The total amount.
        /// </value>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the total quantity.
        /// </summary>
        /// <value>
        /// The total quantity.
        /// </value>
        public decimal TotalQuantity { get; set; }

        /// <summary>
        /// Gets or sets the total discount.
        /// </summary>
        /// <value>
        /// The total discount.
        /// </value>
        public decimal TotalDiscount { get; set; }

        /// <summary>
        /// Gets or sets the total weight.
        /// </summary>
        /// <value>
        /// The total weight.
        /// </value>
        public double TotalWeight { get; set; }

        public bool IsQuantityManualChanged { get; set; }

        public SessionProducts() { }

        public SessionProducts(List<ProductViewModel> products, bool isQuantityManualChanged = false)
        {
            Products = new List<ProductViewModel>(products);
            Products.ForEach(f => f.CalculatedWeight = Math.Round(f.CalculatedWeight));
            IsQuantityManualChanged = isQuantityManualChanged;
            TotalAmount = Math.Round(products.Sum(s => s.FullPrice), 2, MidpointRounding.AwayFromZero);
            TotalQuantity = products.Sum(s => s.Quantity);
            TotalDiscount = Math.Round(products.Sum(s => s.DiscountValue), 2, MidpointRounding.AwayFromZero);
            TotalAmount = Math.Round(TotalAmount - TotalDiscount, 2, MidpointRounding.AwayFromZero);
            TotalWeight = products.Where(w => w.CalculatedWeight > 0).Sum(s => s.CalculatedWeight) / 1000;
        }
    }
}
