//using Hangfire.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Front.Models;
using ModelMID;
using SharedLib;

namespace Front
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

   //     [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            WaresQuantity = "100500";
            MoneySum = "200500 грн";
            
            /*
            List<Ware> Wares = new List<Ware>();

            Wares.Add(new Ware() { Name = "горох", Quantity = 1, Discount = 0.3m, Price = 1.9m, Weight = 200m, Sum = 500.0m });
            Wares.Add(new Ware() { Name = "горох1", Quantity = 1, Discount = 0.3m, Price = 1.9m, Weight = 200m, Sum = 500.0m });
            Wares.Add(new Ware() { Name = "Сіль ", Quantity = 1, Discount = 0.3m, Price = 1.9m, Weight = 200m, Sum = 500.0m });
            */

            InitializeComponent();
            
            var c = new Config("appsettings.json");// Конфігурація Програми(Шляхів до БД тощо)
          
            var r = GetData();

            WaresList.ItemsSource = r;// Wares;
        }

        private  IEnumerable<ReceiptWares> GetData()
        {
            var Bl = new BL();
            //_ = await Bl.SyncDataAsync(true);
            var TerminalId = Guid.Parse("1bb89aa9-dbdf-4eb0-b7a2-094665c3fdd0");
            var ReciptId = Bl.GetNewIdReceipt(TerminalId);
            Bl.AddWaresBarCode(ReciptId, "4823086109988", 10);
            Bl.AddWaresBarCode(ReciptId, "7622300813437", 1);
            Bl.AddWaresBarCode(ReciptId, "2201652300229", 3);
            Bl.AddWaresBarCode(ReciptId, "1110011760018", 11);
            return Bl.GetWaresReceipt(ReciptId);
        }
        private async System.Threading.Tasks.Task LoadDataAsync() {
            var Bl = new BL();
            _ = await Bl.SyncDataAsync(true);}
        }

    }
