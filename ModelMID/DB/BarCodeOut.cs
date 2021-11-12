using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{
    public class Unit
    {
        public string BarCode { get; set; }
        /// <summary>
        /// Назва
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Кількість
        /// </summary>
        public decimal Quantity { get; set; } = 1;

        /// <summary>
        /// Кількість коробок для шару і Палетти
        /// </summary>
        public decimal QuantityBox { get; set; } = 1;
        /// <summary>
        /// Кількість шарів у Палетти
        /// </summary>
        public decimal QuantityLayer { get; set; } = 1;
        /// <summary>
        /// Висота
        /// </summary>
        public decimal Height { get; set; }
        /// <summary>
        /// Ширина
        /// </summary>
        public decimal Width { get; set; }
        /// <summary>
        /// Глибина
        /// </summary>
        public decimal Depth { get; set; }
        /// <summary>
        /// Вага брутто
        /// </summary>
        public decimal GrossWeight { get; set; }
    }
    public class BarCodeOut
    {
        public int CodeWares { get; set; }
        public string NameWares { get; set; }
        public string BarCode { get; set; }
        public decimal Weight { get; set; }
        public DateTime Date { get; set; }
        public string Url { get; set; }
        [JsonIgnore]
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
        /// <summary>
        /// IEnumerable<Unit> конвертований в JSON
        /// </summary>
        public string Unit { get; set; }        
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
