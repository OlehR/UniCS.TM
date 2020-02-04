using System;
using System.Collections.Generic;
using System.Text;
using ModernIntegration.Enums;
using ModernIntegration.ViewModels;

namespace ModernIntegration.Models
{
    public class ReceiptItem
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
        /// Gets or sets the product identifier.
        /// </summary>
        /// <value>
        /// The product identifier.
        /// </value>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        /// <value>
        /// The name of the product.
        /// </value>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the product weight.
        /// </summary>
        /// <value>
        /// The product weight.
        /// </value>
        public int ProductWeight { get; set; }

        /// <summary>
        /// Gets or sets the product calculated weight.
        /// </summary>
        /// <value>
        /// The product calculated weight.
        /// </value>
        public int ProductCalculatedWeight { get; set; }

        /// <summary>
        /// Gets or sets the type of the product weight.
        /// </summary>
        /// <value>
        /// The type of the product weight.
        /// </value>
        public ProductWeightType ProductWeightType { get; set; }

        /// <summary>
        /// Gets or sets the product barcode.
        /// </summary>
        /// <value>
        /// The product barcode.
        /// </value>
        public string ProductBarcode { get; set; }

        /// <summary>
        /// Gets or sets the product quantity.
        /// </summary>
        /// <value>
        /// The product quantity.
        /// </value>
        public decimal ProductQuantity { get; set; }

        /// <summary>
        /// Gets or sets the product price.
        /// </summary>
        /// <value>
        /// The product price.
        /// </value>
        public decimal ProductPrice { get; set; }

        /// <summary>
        /// Gets or sets the full price.
        /// </summary>
        /// <value>
        /// The full price.
        /// </value>
        public decimal FullPrice { get; set; }

        /// <summary>
        /// Gets or sets the discount.
        /// </summary>
        /// <value>
        /// The discount.
        /// </value>
        public decimal Discount { get; set; }

        public decimal RefundedQuantity { get; set; }

        /// <summary>
        /// Gets or sets the total price.
        /// </summary>
        /// <value>
        /// The total price.
        /// </value>
        public decimal TotalPrice { get; set; }
        public string TaxGroup { get; set; }
    }
}
