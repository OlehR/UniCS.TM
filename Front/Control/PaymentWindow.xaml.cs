using Front.Equipments;
using Utils;
using ModelMID;
using SharedLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
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
using ModernExpo.SelfCheckout.Utils;

namespace Front.Control
{
    /// <summary>
    /// Interaction logic for PaymentWindow.xaml
    /// </summary>

    public partial class PaymentWindow : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string ChangeSumPaymant { get; set; } = "";
        decimal tempMoneySum;
        public decimal SumCashDisbursement { get; set; } = 0;
        public Receipt MWCurReceipt { get { return MW.curReceipt; } }
        public decimal RestMoney { get; set; }
        public bool IsCashPayment
        {
            get
            {
                if (SumCashDisbursement > 0)  return false;
                else
                {
                    //if (!decimal.IsNaN(ResMoney))
                        return RestMoney >= 0 ? true : false;
                    //else
                     //   return false;
                }
            }
        }

        //private decimal _MoneySumToRound;
        public decimal MoneySumToRound { get; set; }
        /*{
            get
            {
                return _MoneySumToRound;
            }
            set { _MoneySumToRound = value; }
        }*/
        public Receipt curReceipt;
        MainWindow MW;        
        
        public bool IsRounding
        {
            get
            {
                if (SumCashDisbursement > 0)
                    return false;
                if (MW.Client.IsNotNull())
                {
                    return true;
                }
                return false;
            }
        }

        public PaymentWindow()
        {           
            InitializeComponent();            
        }

        public void UpdatePaymentWindow()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRounding"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MWCurReceipt"));
            Rounding();
        }

        public void Init(MainWindow pMW) { MW = pMW; }

        public void TransferAmounts(decimal moneySum)
        {
            MoneySumToRound = moneySum;
            MoneySumPayTextBox.Text = moneySum.ToString();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MoneySumToRound"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCashPayment"));
            CalculateReturn();
        }
        private void ChangeSumPaymentButton(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Content)
            {
                case "C":
                    if (ChangeSumPaymant.Length <= 1)// якщо видаляють всю суму тоді виводимо 0
                    {
                        ChangeSumPaymant = "0";
                        break;
                    }
                    else
                        ChangeSumPaymant = ChangeSumPaymant.Remove(ChangeSumPaymant.Length - 1);//видаляємо останній елемент
                    break;

                case ",":
                    if (ChangeSumPaymant.IndexOf(",") != -1) // можна поставити лише 1 кому
                    {
                        break;
                    }
                    ChangeSumPaymant += btn.Content;
                    break;
                case "0":
                    if (ChangeSumPaymant.StartsWith("0")) //заборона вводу купи нулів на початку
                    {
                        break;
                    }
                    ChangeSumPaymant += btn.Content;
                    break;
                default:
                    if (ChangeSumPaymant.StartsWith("0") && !ChangeSumPaymant.StartsWith("0,"))
                    {
                        ChangeSumPaymant = btn.Content.ToString();
                        break;
                    }
                    ChangeSumPaymant += btn.Content;
                    break;

            }
            MoneySumPayTextBox.Text = ChangeSumPaymant;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCashPayment"));
        }

        private void F5Button(object sender, RoutedEventArgs e)
        {
            TerminalPaymentInfo terminalPaymentInfo = new TerminalPaymentInfo(this);
            if (terminalPaymentInfo.ShowDialog() == true)
            {
                var Res = terminalPaymentInfo.enteredDataFromTerminal;
                Res.SetIdReceipt(curReceipt);
                SetManualPay(terminalPaymentInfo.enteredDataFromTerminal);
                //отримуємо введені дані
                //ShowErrorMessage(terminalPaymentInfo.enteredDataFromTerminal.AuthorizationCode);
                //                MessageBox.Show(terminalPaymentInfo.enteredDataFromTerminal.AuthorizationCode);//як приклад
            }
        }
        public void SetManualPay(Payment pPay)
        {
            pPay.SumPay = pPay.PosPaid = curReceipt.SumTotal;
            MW?.Bl.db.ReplacePayment(new List<Payment>() { pPay });

            curReceipt.StateReceipt = eStateReceipt.Pay;
            curReceipt.CodeCreditCard = pPay.NumberCard;
            curReceipt.NumberReceiptPOS = pPay.NumberReceipt;
            curReceipt.SumCreditCard = pPay.SumPay;
            MW?.Bl.db.ReplaceReceipt(curReceipt);
            curReceipt.Payment = new List<Payment>() { pPay };
        }




        private void _Cancel(object sender, RoutedEventArgs e)
        {
            ChangeSumPaymant = "0";
            MW.SetStateView(Models.eStateMainWindows.WaitInput);
        }

        private void _ButtonPaymentBank(object sender, RoutedEventArgs e)
        {
            Rounding();
            var btn = sender as Button;
            var str = btn.Content as TextBlock;
            var r = MW?.EF.GetBankTerminal.Where(el => str.Text.Equals(el.Name));
            decimal IssuingCash = MoneySumToRound;
            if (r.Count() == 1)
                MW?.EF.SetBankTerminal(r.First() as BankTerminal);

            var task = Task.Run(() => MW.PrintAndCloseReceipt(null, eTypePay.Card, 0, IssuingCash));
        }

        private void _ButtonPaymentCash(object sender, RoutedEventArgs e)
        {
            MW.EquipmentStatusInPayment.Text = "";
            var task = Task.Run(() => MW.PrintAndCloseReceipt(null, eTypePay.Cash, MoneySumToRound));
        }

        private void CancelCashDisbursement(object sender, RoutedEventArgs e)
        {
            SumCashDisbursement = 0;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCashPayment"));
        }

        private void CashDisbursement(object sender, RoutedEventArgs e)
        {
            MW.InputNumberPhone.Desciption = "Введіть суму видачі";
            MW.InputNumberPhone.ValidationMask = "";
            MW.InputNumberPhone.Result = "";
            MW.InputNumberPhone.CallBackResult = (string result) =>
            {
                SumCashDisbursement = string.IsNullOrEmpty(result) ? 0 : Convert.ToDecimal(result);
                BackgroundPayment.Visibility = Visibility.Collapsed;
            };
            BackgroundPayment.Visibility = Visibility.Visible;
            MW.NumericPad.Visibility = Visibility.Visible;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCashPayment"));
            Rounding();
        }


        public decimal RoundingPrice(decimal price, decimal precision)
        {
            price = Convert.ToInt32(Math.Round(price * 100, 3));
            precision = Math.Round(precision, 2);
            return Math.Round(Math.Ceiling(Math.Ceiling(price / precision / 100)) * precision, 2);
        }

        public decimal RoundingDownPrice(decimal price, decimal precision)
        {
            price = Convert.ToInt32(Math.Round(price * 100, 3));
            precision = Math.Round(precision, 2);
            return Math.Round(Math.Floor(Math.Floor(price / precision / 100)) * precision, 2);
        }

        private void MoneySumPayChange(object sender, TextChangedEventArgs e)
        {
            CalculateReturn();
        }

        private void CalculateReturn()
        {
                var ParsStr = decimal.TryParse(MoneySumPayTextBox.Text, out decimal res);
                if (ParsStr)
                    RestMoney = Math.Round((res - Convert.ToDecimal(MoneySumToRound)), 2);
                else
                    RestMoney = 0;
        }

        private void Rounding(string Name = "")
        {
            tempMoneySum = (decimal)MW.MoneySum;
            RoundSum.Text = "0";
            RoundSumDown.Text = "0";
            var maxAmount = MW.curReceipt?.MaxSumWallet < MW.Client?.Wallet ? MW.curReceipt?.MaxSumWallet : MW.Client?.Wallet;
            decimal tmp = 0;

            switch (Name)
            {

                case "plus05":
                    MoneySumToRound = RoundingPrice(tempMoneySum, 0.5m);
                    RoundSum.Text = (Math.Round(Convert.ToDecimal(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;
                case "plus1":
                    MoneySumToRound = RoundingPrice(tempMoneySum, 1.0m);
                    RoundSum.Text = (Math.Round(Convert.ToDecimal(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;
                case "plus2":
                    MoneySumToRound = RoundingPrice(tempMoneySum, 2.0m);
                    RoundSum.Text = (Math.Round(Convert.ToDecimal(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;
                case "plus5":
                    MoneySumToRound = RoundingPrice(tempMoneySum, 5.0m);
                    RoundSum.Text = (Math.Round(Convert.ToDecimal(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;
                case "plus10":
                    MoneySumToRound = RoundingPrice(tempMoneySum, 10.0m);
                    RoundSum.Text = (Math.Round(Convert.ToDecimal(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;
                case "minus1":
                    CalculationOfSurrender(1, (decimal)maxAmount);
                    break;
                case "minus2":
                    CalculationOfSurrender(2, (decimal)maxAmount);
                    break;
                case "minus5":
                    CalculationOfSurrender(5, (decimal)maxAmount);
                    break;
                case "minus10":
                    CalculationOfSurrender(10, (decimal)maxAmount);
                    break;
                case "enterAmount":

                    MW.InputNumberPhone.Desciption = $"Максимальна сума списання: {maxAmount}";
                    MW.InputNumberPhone.ValidationMask = "";
                    MW.InputNumberPhone.Result = "";
                    MW.NumericPad.Visibility = Visibility.Visible;
                    BackgroundPayment.Visibility = Visibility.Visible;
                    MW.InputNumberPhone.CallBackResult = (string result) =>
                    {
                        tmp = string.IsNullOrEmpty(result) ? 0 : Convert.ToDecimal(result);
                        if (tmp > tempMoneySum)
                        {
                            MoneySumToRound = RoundingPrice(tmp, 1.0m);
                            RoundSum.Text = (Math.Round(Convert.ToDecimal(MoneySumToRound) - tempMoneySum, 2)).ToString();
                        }
                        else
                        {
                            if (tempMoneySum - tmp > (decimal)maxAmount)
                                tmp = tempMoneySum - (decimal)maxAmount;

                            MoneySumToRound = RoundingPrice(tmp, 1.0m);
                            RoundSumDown.Text = (Math.Round(Convert.ToDecimal(MoneySumToRound) - tempMoneySum, 2)).ToString();
                        }
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MoneySumToRound"));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCashPayment"));
                        CalculateReturn();
                        BackgroundPayment.Visibility = Visibility.Collapsed;
                    };
                    break;
                default:
                    MoneySumToRound = (decimal)MW.MoneySum;
                    break;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MoneySumToRound"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCashPayment"));
            CalculateReturn();
        }

        private void Round(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Rounding(btn.Name);
        }

        private void CalculationOfSurrender (decimal roundUpTo, decimal maxAmount)
        {
            decimal tmp = RoundingDownPrice(tempMoneySum, roundUpTo);
            if (tempMoneySum - tmp > maxAmount)
            {
                MoneySumToRound = RoundingPrice(tempMoneySum - maxAmount, 1.0m);
            }
            else
                MoneySumToRound = tmp;
            RoundSumDown.Text = (Math.Round(Convert.ToDecimal(MoneySumToRound) - tempMoneySum, 2)).ToString();
        }
    }
}
