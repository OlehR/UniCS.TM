using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Models
{
    public class Ware
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Weight { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal Sum { get; set; }
    }
}
