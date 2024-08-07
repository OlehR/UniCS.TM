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
        bool _IsEnable;
        public bool IsEnable { get { return _IsEnable; } set{_IsEnable=value; OnPropertyChanged(nameof(IsEnable)); } }
        public bool IsConfirmAge { get; set; } = false;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
