﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{   
    public class WaresReceiptLink:IdReceiptWares
    {        
        public int CodeWaresTo { get; set; }
        public int CodeWarehouse { get; set; }
        public decimal Sort { get; set; }
    }
}
