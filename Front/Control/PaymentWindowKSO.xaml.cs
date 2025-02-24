using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

namespace Front.Control
{
    /// <summary>
    /// Interaction logic for PaymentWindowKSO.xaml
    /// </summary>
    public partial class PaymentWindowKSO : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        MainWindow MW;
        string _color = "#419e08";
        public string color { get=> _color; set {
                _color = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(color));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_color));
            } } 
        public void Init(MainWindow mw)
        {
            MW = mw;
        }
        public PaymentWindowKSO()
        {
            InitializeComponent();
        }

        private void CancelPayment(object sender, RoutedEventArgs e)
        {
            MW.EF.PosCancel();
        }
    }
}
