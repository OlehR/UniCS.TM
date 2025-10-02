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
    /// Interaction logic for TypePaymentInKSO.xaml
    /// </summary>
    public partial class TypePaymentInKSO : UserControl
    {
        public Action<bool> IsCashPayment { get; set; }
        public TypePaymentInKSO()
        {
            InitializeComponent();
        }

        private void ChangeTypePayment(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "Cash":
                    IsCashPayment?.Invoke(true);
                    break;

                case "Terminal":
                    IsCashPayment?.Invoke(false);
                    break;

                default:
                    IsCashPayment?.Invoke(false);
                    break;

            }
        }
    }
}
