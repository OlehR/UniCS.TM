using System;
using System.Collections.Generic;
using System.Text;
using ModernExpo.SelfCheckout.Entities.Models;
using Newtonsoft.Json;

namespace ModernExpo.SelfCheckout.Entities.ViewModels
{
    public class ProductCategoryTagsModel
    {
        public List<Tag> OwnTags { get; set; }

        public List<Tag> InheritTags { get; set; }

        public List<Tag> AllTags { get; set; }

        public ProductCategoryTagsModel(ProductCategoryTagsDbModel dbModel)
        {
            OwnTags = !string.IsNullOrWhiteSpace(dbModel.OwnTags) ? JsonConvert.DeserializeObject<List<Tag>>(dbModel.OwnTags) : new List<Tag>();
            InheritTags = !string.IsNullOrWhiteSpace(dbModel.InheritTags) ? JsonConvert.DeserializeObject<List<Tag>>(dbModel.InheritTags) : new List<Tag>();
            AllTags = new List<Tag>(InheritTags);
            AllTags.AddRange(OwnTags);
        }

        public ProductCategoryTagsModel()
        {
        }
    }
}
