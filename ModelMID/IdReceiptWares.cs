using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class IdReceiptWares:IdReceipt
    {
        public int CodeWares { get; set; }
        public int CodeUnit { get; set; }
        public int Order { get; set; }
    }
}
