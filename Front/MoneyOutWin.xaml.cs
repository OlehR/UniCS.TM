using Front.Equipments;
using Front.Models;
using Front.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Front
{
    /// <summary>
    /// Interaction logic for MoneyOutWin.xaml
    /// </summary>
    public partial class MoneyOutWin : Window
    {
        public MoneyOutVM ViewModel { get; }

 
        public MoneyOutWin(IEnumerable<Equipment> pRRO)
        {
            InitializeComponent();
            ViewModel = new MoneyOutVM(pRRO);
            DataContext = ViewModel;        
            Loaded += OnLoaded;
            AdminUC_NumericPad.Desciption = "Введіть суму";
            AdminUC_NumericPad.ValidationMask = "";
            AdminUC_NumericPad.Result = "";
            AdminUC_NumericPad.IsEnableComma = false;
            AdminUC_NumericPad.CallBackResult = (string res) =>
            {
                Admin_NumericPad.Visibility = Visibility.Visible;
            };
            Admin_NumericPad.Visibility = Visibility.Visible;

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
           
        }
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }
        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GotFocusTotal(object sender, RoutedEventArgs e)
        {
            AdminUC_NumericPad.CallBackResult = (string res) =>
            {
                ViewModel.TopAmount = res;
                Admin_NumericPad.Visibility = Visibility.Visible;
                AdminUC_NumericPad.Result = "";
            };
        }

        private void GotFocusElement(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox) return;
            if (textBox.DataContext is not CashItem item) return;

            AdminUC_NumericPad.CallBackResult = (string res) =>
            {
                item.InputQty = res;
                Admin_NumericPad.Visibility = Visibility.Visible;
                AdminUC_NumericPad.Result = "";
            };
        }
    }
}
