using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{    
    public class WeightReceipt
    {
        public int TypeSource { get; set; }
        public long CodeWares { get; set; }
        public decimal Weight { get; set; }
        public DateTime Date { get; set; }
        public int IdWorkplace { get; set; }
        public int CodeReceipt { get; set; }
        public decimal Quantity { get; set; }
    }
}
