using ModelMID;
using System.Windows.Media;
using System.Globalization;

namespace Front.Models
{
    public class Price
    {
        public Price(decimal pPrice, bool pIsEnable, eTypeWares pTypeWares) //, bool pIsEnable = false
        {
            price = pPrice;
            IsEnable = pIsEnable;
            TypeWares = pTypeWares;               
        }
        public eTypeWares TypeWares { get; set; }
        public decimal price { get; set; }
        public string StrPrice { get { return $"{price.ToString("n2", CultureInfo.InvariantCulture)} ₴"; } }
        public bool IsEnable { get; set; }
        public Brush BackGroundColor
        {
            get
            {
                return new SolidColorBrush(IsEnable ? Color.FromArgb(20, 100, 100, 100) : Color.FromArgb(50, 100, 0, 0));
            }
        }
    }

}
