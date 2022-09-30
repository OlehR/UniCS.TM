using Front.Control;
using ModelMID;
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
    public partial class KeyPad : Window, INotifyPropertyChanged
    {
        public int TextBlockFontSize { get; set; } = 40;
        public string ValidationMask { get; set; }
        public bool IsEnableEnter { get; set; } = true;
        #region Public Properties

        private string _result;
        public string Result
        {
            get { return _result; }
            set { _result = value; this.OnPropertyChanged("Result"); }
        }
        public int firs = 0;
        private PaymentWindow paymentWindow;

        #endregion

        public KeyPad(Window owner)
        {
            InitializeComponent();
            //вікно по центру
            Window wnd = new Window(); //- название твоего окна в WPF
            wnd.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            //вікно справа знизу
            //var primaryMonitorArea = SystemParameters.WorkArea;
            //Left = primaryMonitorArea.Right - Width - 10;
            //Top = primaryMonitorArea.Bottom - Height - 100;
            //this.Owner = owner;
            //this.DataContext = this;
        }

        public KeyPad(PaymentWindow paymentWindow)
        {
            this.paymentWindow = paymentWindow;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (ValidationMask != null)
            {
                Regex regex = new Regex(ValidationMask);
                IsEnableEnter = regex.IsMatch(WrittenNumber.Text);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnableEnter"));
            }
            switch (button.CommandParameter.ToString())
            {
                case "ESC":
                    this.DialogResult = false;
                    break;

                case "RETURN":
                    this.DialogResult = true;
                    break;

                case "BACK":
                    if (Result.Length > 0)
                        Result = Result.Remove(Result.Length - 1);
                    break;

                default:
                    if (firs == 0)
                        Result = "";
                    firs++;
                    Result += button.CommandParameter.ToString();
                    break;
            }
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }
}
