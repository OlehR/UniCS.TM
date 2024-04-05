using ModelMID;
using System.Globalization;
using System.ComponentModel;

namespace Equipments.Model
{
    public class Price: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    
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
        public bool IsConfirmAge { get; set; } = false;        
    }

}
