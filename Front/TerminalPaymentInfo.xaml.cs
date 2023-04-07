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
using Front.Control;
using ModelMID;
using ModelMID.DB;


namespace Front
{
    /// <summary>
    /// Interaction logic for TerminalPaymentInfo.xaml
    /// </summary>
    public partial class TerminalPaymentInfo : Window, INotifyPropertyChanged
    {
        private PaymentWindow paymentWindow;

        public event PropertyChangedEventHandler PropertyChanged;
    
        public Payment enteredDataFromTerminal { get; set; }
        public bool CorrectWriteTextOne { get; set; } = false;
        public bool CorrectWriteTextTwo { get; set; } = false;
        public bool CorrectWriteTextThree { get; set; } = false;
        public bool CorrectWriteText { get; set; }= false;
        public MainWindow MW { get; set; }
        Receipt CurReceipt = null;
        public IEnumerable<WorkPlace> WorkPlaces = null;
        WorkPlace _SelectedWorkPlace = null;
        public WorkPlace SelectedWorkPlace { get { return _SelectedWorkPlace != null ? _SelectedWorkPlace : WorkPlaces.First(); } set { _SelectedWorkPlace = value; } }

        public TerminalPaymentInfo(Window owner, Receipt curReceipt, IEnumerable<WorkPlace> workPlaces )
        {
            InitializeComponent();
            MW = (MainWindow)owner;
            WorkPlaces = workPlaces;
            WorkPlacesListPayInf.ItemsSource = WorkPlaces;
            NameCardNumber.Focus();
            this.Owner = owner;
            this.DataContext = this;
            this.CurReceipt = curReceipt;
        }

        public TerminalPaymentInfo(PaymentWindow paymentWindow)
        {
            this.paymentWindow = paymentWindow;
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {           
            
            this.DialogResult = false;
        }

        private void Ok(object sender, RoutedEventArgs e)
        {
            enteredDataFromTerminal = new()
            {
                IsSuccess = true,
                IssuerName = "????",
                NumberCard = $"XXXXXXXXXXXX{NameCardNumber.Text}",
                NumberReceipt = long.Parse(NameAuthorizationCode.Text),
                CodeAuthorization = NameRRN.Text,
                TypePay = eTypePay.Card,
                NumberSlip = "123456",
                IdWorkplacePay = SelectedWorkPlace.IdWorkplace,
            };            
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

        private void CheckWorkPlaceId(object sender, RoutedEventArgs e)
        {
            RadioButton ChBtn = sender as RadioButton;
            if (ChBtn.DataContext is WorkPlace)
            {
                WorkPlace temp = ChBtn.DataContext as WorkPlace;
                if (ChBtn.IsChecked == true)
                {
                    SelectedWorkPlace = temp;
                    foreach (var workPlace in WorkPlaces)
                        workPlace.IsChoice = workPlace.IdWorkplace == temp.IdWorkplace;

                }

            }
        }
    }
    public class EnteredDataFromTerminal
    {
        public string CardNumber { get; set; }
        public string AuthorizationCode { get; set; }
        public string RRN { get; set; }
    }
}
