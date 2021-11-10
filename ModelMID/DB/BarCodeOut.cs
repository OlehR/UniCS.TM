using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{
    public class BarCodeOut
    {
        public int CodeWares { get; set; }
        public string NameWares { get; set; }
        public string BarCode { get; set; }
        public decimal Weight { get; set; }
        public DateTime Date { get; set; }
        public string Url { get; set; }
        public string Data { get; set; }
        public decimal WeightUrl { get; set; }
        public DateTime DateUrl { get; set; }
        public string Error { get; set; }        
        public string UrlPicture { get; set; }
        /// <summary>
        /// Чи актуальна інформація
        /// </summary>
        public bool IsActual { get; set; }
        /// <summary>
        /// Чи верифікований товар.
        /// </summary>
        public bool IsVerification { get; set; }
        public string Site { get; set; }
        public string Unit1 { get; set; }
        public string Unit2 { get; set; }
        public string Unit3 { get; set; }
        public string Unit4 { get; set; }
        public string Unit5 { get; set; }
        public string Name { get; set; }
        public string NameShort { get; set; }
        public string Other { get; set; }
        public string UKTZED { get; set; }
        public int VAT { get; set; }
        /// <summary>
        /// термін придатності в днях
        /// </summary>
        public decimal ExpirationDay { get; set; }

        public decimal UnitSale { get; set; }
        public string  UnitSaleName { get; set; }
        public string PaletteLayer { get; set; }
        public string Palette { get; set; }
 
    }

}
