using System;
using System.Collections.Generic;
using System.Text;
using ModernExpo.SelfCheckout.Entities.ViewModels;

namespace ModernExpo.SelfCheckout.Entities.Models
{
    public class ProductCategory
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
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Language { get; set; }
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

        public bool HasChildren { get; set; }

        public bool HasProducts { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>
        /// The tags.
        /// </value>
        public ProductCategoryTagsModel Tags { get; set; }
    }
}
