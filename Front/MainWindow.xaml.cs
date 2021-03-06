﻿//using Hangfire.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using Front.Equipments;
using Front.Models;
using Microsoft.Extensions.Configuration;
using ModelMID;
using SharedLib;

namespace Front
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        
        public event PropertyChangedEventHandler PropertyChanged;

        public string WaresQuantity { get; set; }

        public string MoneySum { get; set; }
        public bool Volume { get; set; }
        public string Weight { get; set; }
        public string WeightControl { get; set; }
        public BL Bl;
        EquipmentFront EF;

        public ObservableCollection<ReceiptWares> ListWares { get; set; }

        public MainWindow()
        {
            var c = new Config("appsettings.json");// Конфігурація Програми(Шляхів до БД тощо)
            

            Bl = new BL(true);
            EF = new EquipmentFront(Bl);
            //ad =  new Admin();
            Global.OnReceiptCalculationComplete += (wareses, guid) =>
            {
                try
                {
                    ListWares = new ObservableCollection<ReceiptWares>(wareses);

                    Dispatcher.BeginInvoke(
                        new ThreadStart(() => { WaresList.ItemsSource = ListWares; Recalc(); }));

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error OnReceiptCalculationComplete" + ex.Message);
                }
                Debug.WriteLine("\n==========================Start===================================");
                foreach (var receiptWarese in wareses)
                {
                    Debug.WriteLine($"Promotion=>{receiptWarese.GetStrWaresReceiptPromotion.Trim()} \n{receiptWarese.NameWares} - {receiptWarese.Price} Quantity=> {receiptWarese.Quantity} SumDiscount=>{receiptWarese.SumDiscount}");
                }

                Debug.WriteLine("===========================End==========================================\n");
            };

            Global.OnSyncInfoCollected += (SyncInfo) =>
            {
                Console.WriteLine($"OnSyncInfoCollected Status=>{SyncInfo.Status} StatusDescription=>{SyncInfo.StatusDescription}");
            };

            Global.OnStatusChanged += (Status) => { };

            Global.OnChangedStatusScale += (Status) => { };

            Global.OnClientChanged += (client, guid) =>
            {
                Debug.WriteLine($"Client.Wallet=> {client.Wallet} SumBonus=>{client.SumBonus} ");
            };


            WaresQuantity = "0";
            MoneySum = "0";
            Volume = true;

            InitializeComponent();

            MainWindowCS Processing = new MainWindowCS();

            ListWares = new ObservableCollection<ReceiptWares>(StartData());

            //ua.Tag = new CultureInfo("ua");
            en.Tag = new CultureInfo("en");
            hu.Tag = new CultureInfo("hu");
            //pln.Tag = new CultureInfo("pln");

            WaresList.ItemsSource = ListWares;// Wares;

            CultureInfo currLang = App.Language;
            Recalc();

        }

        private void _Delete(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.DataContext is ReceiptWares)
            {
                var el = btn.DataContext as ReceiptWares;
                //ListWares.Remove((ReceiptWares)btn.DataContext);
                Bl.ChangeQuantity(el, 0);
            }

        }

        private void _Minus(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.DataContext is ReceiptWares)
            {
                ReceiptWares temp = btn.DataContext as ReceiptWares;
                temp.Quantity--;
                Bl.ChangeQuantity(temp, temp.Quantity);
            }
        }

        private void _Plus(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.DataContext is ReceiptWares)
            {
                ReceiptWares temp = btn.DataContext as ReceiptWares;
                temp.Quantity++;
                Bl.ChangeQuantity(temp, temp.Quantity);
            }

        }

        private void _VolumeButton(object sender, RoutedEventArgs e)
        {
            Volume = !Volume;
        }

        private void Recalc()
        {
            var Sum = ListWares.Sum(r => r.Sum);
            MoneySum = Sum.ToString();
            WaresQuantity = ListWares.Count().ToString();
        }

        private void _ChangeLanguage(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            try
            {
                if (btn != null)
                {
                    if (btn.Tag is CultureInfo lang)
                    {
                        App.Language = lang;
                    }
                }
            }
            catch { }

            switch (btn.Name)
            {
                case "ua":
                    ua.Style = (Style)ua.FindResource("yelowButton");
                    en.Style = (Style)en.FindResource("Default");
                    hu.Style = (Style)hu.FindResource("Default");
                    pln.Style = (Style)pln.FindResource("Default");
                    break;
                case "en":
                    en.Style = (Style)en.FindResource("yelowButton");
                    ua.Style = (Style)ua.FindResource("Default");
                    hu.Style = (Style)hu.FindResource("Default");
                    pln.Style = (Style)pln.FindResource("Default");
                    break;
                case "hu":
                    hu.Style = (Style)hu.FindResource("yelowButton");
                    ua.Style = (Style)ua.FindResource("Default");
                    en.Style = (Style)en.FindResource("Default");
                    pln.Style = (Style)pln.FindResource("Default");
                    break;
                case "pln":
                    pln.Style = (Style)pln.FindResource("yelowButton");
                    ua.Style = (Style)ua.FindResource("Default");
                    en.Style = (Style)en.FindResource("Default");
                    hu.Style = (Style)hu.FindResource("Default");
                    break;
            }
        }

        private void _Back(object sender, RoutedEventArgs e)
        {
            var rand = new Random();

            string sql = @"select CODE_WARES from (select CODE_WARES,row_number() over (order by code_wares)  as nn from price p where p.code_DEALER=2)
                        where nn=  cast(abs(random()/9223372036854775808.0)*1000 as int)";
            var CodeWares = Bl.db.db.ExecuteScalar<int>(sql);
            if (CodeWares > 0)
                Bl.AddWaresCode( CodeWares, 0, Math.Round(1M + 5M * rand.Next() / (decimal)int.MaxValue));

        }

        private void _Search(object sender, RoutedEventArgs e)
        {
            FindWaresWin FWW = new FindWaresWin();
            //this.Visibility = Visibility.Visible;
            FWW.Show();
        }

        private void _ButtonHelp(object sender, RoutedEventArgs e)
        {
            // NavigationService navService = NavigationService.GetNavigationService(this);

            //navService.Navigate(ad);

            //ad.Visibility = Visibility.Visible;
            Admin ad = new Admin();
            ad.Show();
        }

        private void _OwnBag(object sender, RoutedEventArgs e)
        {

        }

        private void _BuyBag(object sender, RoutedEventArgs e)
        {

        }

        private void _ButtonPayment(object sender, RoutedEventArgs e)
        {

        }

        private IEnumerable<ReceiptWares> StartData()
        {
            var TerminalId = Guid.Parse("1bb89aa9-dbdf-4eb0-b7a2-094665c3fdd0");
            var RId=Bl.GetNewIdReceipt(TerminalId);
            Bl.AddWaresBarCode(RId, "4823086109988", 10);
            Bl.AddWaresBarCode(RId, "7622300813437", 1);
            Bl.AddWaresBarCode(RId, "2201652300229", 3);
            Bl.AddWaresBarCode(RId,"7775002160043", 1); //товар 2 кат
            Bl.AddWaresBarCode(RId,"1110011760218", 11);
            Bl.AddWaresBarCode(RId,"7773002160043", 1); //товар 2 кат
            return Bl.GetWaresReceipt();
        }

        

    }


    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }

        public BoolToVisibilityConverter()
        {
            // set defaults
            TrueValue = Visibility.Visible;
            FalseValue = Visibility.Collapsed;
        }

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                return null;
            return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (Equals(value, TrueValue))
                return true;
            if (Equals(value, FalseValue))
                return false;
            return null;
        }
    }


}
