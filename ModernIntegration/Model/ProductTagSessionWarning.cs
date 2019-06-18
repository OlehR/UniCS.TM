using System;
using System.Collections.Generic;
using System.Text;
using ModernIntegration.Session;
using ModernIntegration.ViewModels;

namespace MModernIntegration.Session
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ModernExpo.SelfCheckout.Entities.Session.ISessionWarning" />
    public class ProductTagSessionWarning : ISessionWarning
    {
        /// <summary>
        /// Gets or sets the product.
        /// </summary>
        /// <value>
        /// The product.
        /// </value>
        public ProductViewModel Product { get; set; }

        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        /// <value>
        /// The name of the tag.
        /// </value>
        public List<string> TagNames { get; set; }
    }
}
