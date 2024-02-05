using System;
using System.Collections.Generic;
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
    public partial class PaymentWindowKSO : UserControl
    {
        MainWindow MW;
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
