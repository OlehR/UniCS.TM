using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelMID.DB
{
    public class FiscalArticle
    {
        public int IdWorkplacePay { get; set; }
        /// <summary>
        /// Код товару
        /// </summary>
        public int CodeWares { get; set; }
        /// <summary>
        /// Назва товару
        /// </summary>
        public string NameWares { get; set; }

        public int PLU { get; set; }
        public decimal Price { get; set; }        
    }
}
