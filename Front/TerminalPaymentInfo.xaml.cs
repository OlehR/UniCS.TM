using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for TerminalPaymentInfo.xaml
    /// </summary>
    public partial class TerminalPaymentInfo : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    
        public EnteredDataFromTerminal enteredDataFromTerminal { get; set; }
        public bool CorrectWriteTextOne { get; set; } = false;
        public bool CorrectWriteTextTwo { get; set; } = false;
        public bool CorrectWriteTextThree { get; set; } = false;
        public bool CorrectWriteText { get; set; }= false;

        public TerminalPaymentInfo(Window owner)
        {
            InitializeComponent();
            this.Owner = owner;
            this.DataContext = this;
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            
            
            this.DialogResult = false;
        }

        private void Ok(object sender, RoutedEventArgs e)
        {
            enteredDataFromTerminal = new EnteredDataFromTerminal();
            enteredDataFromTerminal.CardNumber = NameCardNumber.Text;
            enteredDataFromTerminal.AuthorizationCode = NameAuthorizationCode.Text;
            enteredDataFromTerminal.RRN = NameRRN.Text;
            
            this.DialogResult = true;
        }

        private void ChangetWriteTextOne(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Regex regex = new Regex(@"^\w[0-9]{3}?$");
            CorrectWriteTextOne = regex.IsMatch(textBox.Text);
            ChangeOfState();
            //CorrectWriteText = CorrectWriteTextOne;
            //MessageBox.Show(CorrectWriteText.ToString());
        }

        private void ChangetWriteTextTwo(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Regex regex = new Regex(@"^\w[0-9]{5}?$");
            CorrectWriteTextTwo = regex.IsMatch(textBox.Text);
            ChangeOfState();
        }

        private void ChangetWriteTextTree(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Regex regex = new Regex(@"^\w[0-9]{11}?$");
            CorrectWriteTextThree = regex.IsMatch(textBox.Text);
            ChangeOfState();
        }
        private void ChangeOfState()
        {
            if (CorrectWriteTextOne && CorrectWriteTextTwo && CorrectWriteTextThree)
            {
                CorrectWriteText = true;
            }
            else CorrectWriteText = false;
        }
    }
    public class EnteredDataFromTerminal
    {
        public string CardNumber { get; set; }
        public string AuthorizationCode { get; set; }
        public string RRN { get; set; }
    }
}
