//using Hangfire.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Front.Models;
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
        public ObservableCollection<ReceiptWares> ListWares { get; set; }

        public MainWindow()
        {
            Global.OnReceiptCalculationComplete += (wareses, guid) =>
            {

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

            ListWares = new ObservableCollection<ReceiptWares>(Processing.GetData().ToList());

            WaresQuantity = ListWares.Count.ToString();

            ua.Tag = new CultureInfo("ua");
            en.Tag = new CultureInfo("en");
            hu.Tag = new CultureInfo("hu");
            pln.Tag = new CultureInfo("pln");

            WaresList.ItemsSource = ListWares;// Wares;

            CultureInfo currLang = App.Language;
            Recalc();

        }

        private void _Delete(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            ListWares.Remove((ReceiptWares)btn.DataContext);
            Recalc();            
        }

        private void _Minus(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.DataContext is ReceiptWares)
            {
                ReceiptWares temp = btn.DataContext as ReceiptWares;
                temp.Quantity--;
                WaresList.Items.Refresh();
            }
            Recalc();
        }

        private void _Plus(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.DataContext is ReceiptWares)
            {
                ReceiptWares temp = btn.DataContext as ReceiptWares;
                temp.Quantity ++;
                WaresList.Items.Refresh();
            }
            Recalc();
        }

        private void _VolumeButton(object sender, RoutedEventArgs e)
        {
            Volume = !Volume;
        }

        private void Recalc()
        {
            var Sum=ListWares.Sum(r => r.Sum);
            MoneySum = Sum.ToString();
            WaresQuantity = ListWares.Count().ToString();
        }

        private void _ChangeLanguage(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            try
            {
                if (btn!=null)
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

        }

        private void _Search(object sender, RoutedEventArgs e)
        {

        }

        private void _ButtonHelp(object sender, RoutedEventArgs e)
        {

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
    }
}
