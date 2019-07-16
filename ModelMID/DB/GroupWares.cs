using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class GroupWares
    {
        /// <summary>
        /// Код групи товарів
        /// </summary>
        public int CodeGroupWares { get; set; }
        
        /// <summary>
        /// Код батька Групи(0- перший рівень)
        /// </summary>
        public int CodeParentGroupWares { get; set; }
        /// <summary>
        /// Назва групи товару
        /// </summary>
        public string Name { get; set; }        

    }
}
