using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{
    public enum eTypeData
    {
        Directions = 1,
        Brand = 2
    }

    public class WaresWarehouse
    {
        public int CodeWarehouse { get; set; }
        public eTypeData TypeData { get; set; }
        public int Data { get; set; }
    }
}
