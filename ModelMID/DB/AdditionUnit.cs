using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class AdditionUnit
    {
        public long CodeWares { get; set; }
        public int CodeUnit { get; set; }
        public decimal Coefficient { get; set; }
        public decimal Weight { get; set; }
        public string DefaultUnit { get; set; }
        public bool IsDefaultUnit { get { return DefaultUnit.Equals("Y") || DefaultUnit.Equals("1"); } set { DefaultUnit = (value ?  "Y" :  "N"); } }
        public decimal WeightNet { get; set; }

    }
}
