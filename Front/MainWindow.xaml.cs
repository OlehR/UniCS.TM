//using Hangfire.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public ObservableCollection<ReceiptWares> ListWares { get; set; }



        public MainWindow()
        {
            WaresQuantity = "0";
            MoneySum = "0";


            InitializeComponent();

            MainWindowCS Processing = new MainWindowCS();

            ListWares = new ObservableCollection<ReceiptWares>(Processing.GetData().ToList());

            WaresQuantity = ListWares.Count.ToString();

            WaresList.ItemsSource = ListWares;// Wares;
        }

        private void _Delete(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            ListWares.Remove((ReceiptWares)btn.DataContext);
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
        }
    }
}
