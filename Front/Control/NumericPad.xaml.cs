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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Front.Control
{
    /// <summary>
    /// Interaction logic for NumericPad.xaml
    /// </summary>
    public partial class NumericPad : UserControl, INotifyPropertyChanged
    {
        public int TextBlockFontSize { get; set; } = 40;
        public string ValidationMask { get; set; }
        public bool IsEnableEnter
        {
            get
            {
                bool res = true;
                if (ValidationMask != null)
                {
                    Regex regex = new Regex(ValidationMask);
                    res = regex.IsMatch(Result);
                }
                return res;
            }
        }
        public string Desciption { get; set; }
        #region Public Properties

        private string _result = "";
        public string Result
        {
            get { return _result; }
            set { _result = value; WrittenNumber.Text = Result; OnPropertyChanged("Result"); }
        }
        public int firs = 0;

        #endregion

        public Action<string> CallBackResult {get; set; }

        public NumericPad()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;            
            
            switch (button.CommandParameter.ToString())
            {
                case "ESC":
                  ((FrameworkElement)  this.Parent).Visibility = Visibility.Collapsed;
                    break;

                case "RETURN":
                    ((FrameworkElement)this.Parent).Visibility = Visibility.Collapsed;
                    //this.Visibility = Visibility.Collapsed;
                    CallBackResult?.Invoke(Result);
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
           
            button16.IsEnabled = IsEnableEnter;
            //OnPropertyChanged("IsEnableEnter"); //!!!TMP Розібратись чому не працює нормально біндінг в UserControl
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String info)
        {
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));            
        }

        #endregion
    }
}
