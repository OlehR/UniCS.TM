using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{   
    public class ReceiptWaresLink:IdReceiptWares
    {        
        public Int64 CodeWaresTo { get; set; }        
        public decimal Sort { get; set; }
        public int  Quantity { get; set; }
        public string NameWares { get; set; }
    }
}
