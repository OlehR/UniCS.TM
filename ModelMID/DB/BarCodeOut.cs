using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{
    public class BarCodeOut
    {
        public int CodeWares { get; set; }
        public string BarCode { get; set; }
        public decimal Weight { get; set; }
        public DateTime Date { get; set; }
        public string Url { get; set; }
        public string Data { get; set; }
        public decimal WeightUrl { get; set; }
        public DateTime DateUrl { get; set; }
        public string Error { get; set; }        
        public string UrlPicture { get; set; }

    }

}
