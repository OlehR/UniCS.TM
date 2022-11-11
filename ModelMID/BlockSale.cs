using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class BlockSale
    {
        public eTypeWares TypeWares {get;set;}
        public string TimeStart { get; set; }
        public string TimeEnd { get;set;}
        public bool IsBlock(eTypeWares pTypeWares)
        {
            long curTime = long.Parse(DateTime.Now.ToString("HHmmss"));
            long Start = long.Parse(TimeStart.Replace(":", ""));
            long End = long.Parse(TimeEnd.Replace(":", ""));
            return TypeWares == pTypeWares && !(curTime>=Start && curTime<=End);
        }
    }
}
