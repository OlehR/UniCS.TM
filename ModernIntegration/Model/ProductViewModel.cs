using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ModernIntegration.Enums;
using ModernIntegration.Model;
using ModernIntegration.Models;

namespace ModernIntegration.ViewModels
{
    /// <summary>
    /// Summary description for ProductViewModel
    /// </summary>
    public class ProductViewModel
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; set; }
        /// <summary>
        /// Gets or sets the Barcode.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [Required(ErrorMessage = "Barcode is required")]
        public string Barcode { get; set; }
        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        [Required(ErrorMessage = "Code is required")]
        public int Code { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the additional description.
        /// </summary>
        /// <value>
        /// The additional description.
        /// </value>
        public string AdditionalDescription { get; set; }
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
        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the weight.
        /// </summary>
        /// <value>
        /// The weight.
        /// </value>
        [Required(ErrorMessage = "Weight is required")]
        public double Weight { get; set; }

        /// <summary>
        /// Gets or sets the additional weights.
        /// </summary>
        /// <value>
        /// The additional weights.
        /// </value>
        public List<WeightInfo> AdditionalWeights { get; set; }

        /// <summary>
        /// Gets or sets the delta weight.
        /// </summary>
        /// <value>
        /// The delta weight.
        /// </value>
        [Required(ErrorMessage = "Delta weight is required")]
        public double DeltaWeight { get; set; }


        /// <summary>
        /// Gets the total delta.
        /// </summary>
        /// <value>
        /// The total delta.
        /// </value>
        public double TotalDelta => DeltaWeight * (double)Quantity;

        public double DeltaWeightKg
        {
            get
            {
                var temp = (DeltaWeight / 1000).ToString().Split(',', '.');

                if (temp.Length == 1 || temp.Length == 0)
                    return DeltaWeight / 1000;

                if (temp[1].Length > 3)
                    return DeltaWeight;

                return DeltaWeight / 1000;
            }
        }

        /// <summary>
        /// Gets or sets the type of the product weight.
        /// </summary>
        /// <value>
        /// The type of the product weight.
        /// </value>
        [Required(ErrorMessage = "Product weight type is required")]
        public ProductWeightType ProductWeightType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is age restricted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is age restricted; otherwise, <c>false</c>.
        /// </value>
        public bool IsAgeRestrictedConfirmed { get; set; }
        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        /// <value>
        /// The quantity.
        /// </value>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal FullPrice
        {
            get
            {
                if (ProductWeightType == ProductWeightType.ByWeight)
                    return Price * ((decimal)Weight / 1000M);
                return Price * Quantity;
            }
        }

        /// <summary>
        /// Gets or sets the discount value.
        /// </summary>
        /// <value>
        /// The discount value.
        /// </value>
        public decimal DiscountValue { get; set; }
        /// <summary>
        /// Gets or sets the name of the discount.
        /// </summary>
        /// <value>
        /// The name of the discount.
        /// </value>
        public string DiscountName { get; set; }

        /// <summary>
        /// Gets or sets the type of the warning.
        /// </summary>
        /// <value>
        /// The type of the warning.
        /// </value>
        public WarningType? WarningType { get; set; }

        /// <summary>
        /// Gets or sets the calculated weight.
        /// </summary>
        /// <value>
        /// The calculated weight.
        /// </value>
        public double CalculatedWeight { get; set; }

        /// <summary>
        /// Gets the total weight.
        /// </summary>
        /// <value>
        /// The total weight.
        /// </value>
        public double TotalWeight
        {
            get
            {
                if (ProductWeightType == ProductWeightType.ByWeight)
                    return Weight;
                return Weight * (double)Quantity;
            }
        }

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
        /// The category of the product weight.
        /// </value>
        [Required(ErrorMessage = "Category weight is required")]
        public int WeightCategory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is product on processing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is product on processing; otherwise, <c>false</c>.
        /// </value>
        public bool IsProductOnProcessing { get; set; }

        public Guid CategoryId { get; set; }

        public decimal RefundedQuantity { get; set; }
        
        /// <summary>
        /// Gets or sets the UKTZED code.
        /// </summary>
        /// <value>
        /// The UKTZED code.
        /// </value>
        public string Uktzed { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating printing UKTZED code or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if printing the  UKTZED code; otherwise, <c>false</c>.
        /// </value>
        public bool IsUktzedNeedToPrint { get; set; }


        public List<string> Excises { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductViewModel"/> class.
        /// </summary>
        public ProductViewModel()
        {
            CalculatedWeight = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductViewModel"/> class.
        /// </summary>
        /// <param name="product">The product.</param>
        public ProductViewModel(Product product)
        {
            Id = product.Id;
            Barcode = product.Barcode;
            Code = product.Code;
            Name = product.Name;
            Image = product.Image;
            Price = product.Price;
            Weight = product.Weight;
            Quantity = 1;
            DeltaWeight = product.DeltaWeight;
            ProductWeightType = product.ProductWeightType;
            IsAgeRestrictedConfirmed = product.IsAgeRestrictedConfirmed;
            CalculatedWeight = -1;
            Tags = product.Tags;
            HasSecurityMark = product.HasSecurityMark;
            TotalRows = product.TotalRows;
            WeightCategory = product.WeightCategory;
            CategoryId = product.CategoryId;
            Uktzed = product.Uktzed;
            IsUktzedNeedToPrint = product.IsUktzedNeedToPrint;
            Excises = product.Excises;
        }

        /// <summary>
        /// To the product.
        /// </summary>
        /// <returns></returns>
        public virtual Product ToProduct()
        {
            return new Product()
            {
                Id = Id,
                Barcode = Barcode,
                Code = Code,
                Name = Name,
                Image = Image,
                Price = Price,
                Weight = Weight,
                DeltaWeight = DeltaWeight,
                ProductWeightType = ProductWeightType,
                IsAgeRestrictedConfirmed = IsAgeRestrictedConfirmed,
                HasSecurityMark = HasSecurityMark,
                WeightCategory = WeightCategory,
                CategoryId = CategoryId,
                Uktzed = Uktzed,
                IsUktzedNeedToPrint = IsUktzedNeedToPrint,
                 Excises = Excises
            };
        }

        public ReceiptItem ToReceiptItem()
        {
            var item = new ReceiptItem()
            {
                Discount = DiscountValue,
                FullPrice = FullPrice,
                Id = Guid.NewGuid(),
                ProductBarcode = Barcode,
                ProductId = Id,
                ProductName = Name,
                ProductPrice = Price,
                ProductWeight = (int) Math.Round(Weight, MidpointRounding.AwayFromZero),
                ProductCalculatedWeight = (int) Math.Round(CalculatedWeight, MidpointRounding.AwayFromZero),
                ProductQuantity = ProductWeightType == ProductWeightType.ByWeight ? (decimal)Weight/1000M : Quantity,
                ProductWeightType = ProductWeightType,
                TaxGroup = TaxGroup,
                RefundedQuantity= RefundedQuantity,
                Uktzed = Uktzed,
                IsUktzedNeedToPrint = IsUktzedNeedToPrint,
                Excises= Excises
            };

            item.TotalPrice = item.FullPrice - item.Discount;

            return item;
        }
        public string TaxGroup { get; set; }
    }
}