using Hangfire.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UIApp.Models;

namespace UIApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _waresQuantity;
        public string WaresQuantity
        {
            private set
            {
                _waresQuantity = value;
                OnPropertyChanged(nameof(WaresQuantity));
            }
            get
            {
                return _waresQuantity;
            }
        }

        private string _moneySum;
        public string MoneySum
        {
            private set
            {
                _moneySum = value;
                OnPropertyChanged(nameof(MoneySum));
            }
            get
            {
                return _moneySum;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            _waresQuantity = "100500";
            _moneySum = "200500 грн";

            List<Ware> Wares = new List<Ware>();

            Wares.Add(new Ware() { Name = "горох", Quantity = 1, Discount = 0.3m, Price = 1.9m, Weight = 200m, Sum = 500.0m });
            Wares.Add(new Ware() { Name = "горох1", Quantity = 1, Discount = 0.3m, Price = 1.9m, Weight = 200m, Sum = 500.0m });

            InitializeComponent();
            
            WaresList.ItemsSource = Wares;
        }
    }
}
