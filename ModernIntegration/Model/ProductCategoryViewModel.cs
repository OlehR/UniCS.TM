using Microsoft.AspNetCore.Http;
using ModernIntegration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ModernIntegration.ViewModels
{
    public class ProductCategoryViewModel
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; set; }
        /// <summary>
        /// Gets or sets the parent identifier.
        /// </summary>
        /// <value>
        /// The parent identifier.
        /// </value>
        public Guid? ParentId { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the custom identifier.
        /// </summary>
        /// <value>
        /// The custom identifier.
        /// </value>
        public string CustomId { get; set; }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the image path.
        /// </summary>
        /// <value>
        /// The image path.
        /// </value>
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the form image.
        /// </summary>
        /// <value>
        /// The form image.
        /// </value>
        public IFormFile FormImage { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>
        /// The tags.
        /// </value>
        public ProductCategoryTagsModel Tags { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ProductCategoryViewModel"/> class.
        /// </summary>
        public ProductCategoryViewModel() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductCategoryViewModel"/> class.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="tags">The tags.</param>
        public ProductCategoryViewModel(ProductCategory category)
        {
            Id = category.Id;
            CustomId = category.CustomId;
            Name = category.Name;
            ParentId = category.ParentId;
            Description = category.Description;
            Image = category.Image;
            Tags = category.Tags;
        }

        /// <summary>
        /// To the product.
        /// </summary>
        /// <returns></returns>
        public ProductCategory ToProduct()
        {           
            var productCategory = new ProductCategory
            {
                Id = Id,
                CustomId = CustomId,
                Name = Name,
                ParentId = ParentId,
                Description = Description,
                Image = Image,
                Tags = Tags
            };
            return productCategory;
        }
    }
}
