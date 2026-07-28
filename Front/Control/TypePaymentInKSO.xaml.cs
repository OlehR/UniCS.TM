using Front.Equipments;
using ModelMID;
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
    /// Interaction logic for TypePaymentInKSO.xaml
    /// </summary>
    public partial class TypePaymentInKSO : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Action<bool> IsCashPayment { get; set; }
        public string TypeReturn { get; set; }
        MainWindow MW;
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
        public void Init(MainWindow pMW) { MW = pMW; }
        public void UpdateTypePayment()
        {
            MW.curReceipt.Payment = MW.Bl.GetPayment(MW.curReceipt);
            if (MW.curReceipt?.RefundId != null)
                MW.curReceipt.Payment = MW.Bl.GetPayment(MW.curReceipt.RefundId);
            if (MW.curReceipt.Payment?.Count() > 0)
                RefreshTypePayment();
        }
        void RefreshTypePayment()
        {
            TypeReturn = "";
            if (MW.curReceipt.Payment.Any(x => x.TypePay == eTypePay.CashMachine))
            {
                TypeReturn = eTypePay.CashMachine.ToString();
            }
            else
            {
                TypeReturn = eTypePay.Card.ToString();
            }
            if (string.IsNullOrEmpty(TypeReturn))
                TypeReturn = "AllPayments";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TypeReturn)));
        }
    }
}
