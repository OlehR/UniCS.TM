using System;
using System.Collections.Generic;
using ModernIntegration.Enums;
using Newtonsoft.Json;

namespace ModernIntegration.Models
{
    /// <summary>
    /// Summary description for Product
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; set; }
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        public Guid CategoryId { get; set; }
        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        /// <value>
        /// The category name.
        /// </value>
        public string CategoryName { get; set; }
        /// <summary>
        /// Gets or sets the category description.
        /// </summary>
        /// <value>
        /// The category name.
        /// </value>
        public string CategoryDescription { get; set; }
        /// <summary>
        /// Gets or sets the Barcode.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Barcode { get; set; }
		/// <summary>
		/// Gets or sets the code.
		/// </summary>
		/// <value>
		/// The code.
		/// </value>
		public int Code { get; set; }
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		public string Name { get; set; }
        /// <summary>
		/// Gets or sets the image path.
		/// </summary>
		/// <value>
		/// The image path.
		/// </value>
		public string Image { get; set; }
        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>
        /// The price.
        /// </value>
        public decimal Price { get; set; }
        /// <summary>
        /// Gets or sets the weight.
        /// </summary>
        /// <value>
        /// The weight.
        /// </value>
        public double Weight { get; set; }
        /// <summary>
        /// Gets or sets the delta weight.
        /// </summary>
        /// <value>
        /// The delta weight.
        /// </value>
        public double DeltaWeight { get; set; }
		/// <summary>
		/// Gets or sets the type of the product weight.
		/// </summary>
		/// <value>
		/// The type of the product weight.
		/// </value>
		public ProductWeightType ProductWeightType { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is age restricted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is age restricted; otherwise, <c>false</c>.
        /// </value>
        public bool IsAgeRestrictedConfirmed { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>
        /// The tags.
        /// </value>
        public List<Tag> Tags { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has security mark.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has security mark; otherwise, <c>false</c>.
        /// </value>
        public bool HasSecurityMark { get; set; }

        /// <summary>
        /// Gets or sets the search count.
        /// </summary>
        /// <value>
        /// The search count.
        /// </value>
        public int SearchCount { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Gets or sets the total number of rows in DB table.
        /// </summary>
        /// <value>
        /// The total number.
        /// </value>
        public int TotalRows { get; set; }

        /// <summary>
        /// Gets or sets the category of the product weight.
        /// </summary>
        /// <value>
        /// The category id.
        /// </value>
        public int WeightCategory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Product"/> class.
        /// </summary>
        public Product()
        {
            IsAgeRestrictedConfirmed = true;
        }
    }
}