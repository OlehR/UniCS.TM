using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{   
    public class WaresLink
    {
        public long CodeWares { get; set; }
        public Int64 CodeWaresTo { get; set; }
        public int CodeWarehouse { get; set; }
        public bool IsDefault { get; set; }
        public int Sort { get; set; }
    }
}
