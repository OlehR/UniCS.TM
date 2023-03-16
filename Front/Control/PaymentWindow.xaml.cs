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
using ModernExpo.SelfCheckout.Utils;

namespace Front.Control
{ 
    public partial class PaymentWindow : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string ChangeSumPaymant { get; set; } = "";
        public decimal ChangeSumPaymantDecimal { get { decimal res=0; decimal.TryParse(ChangeSumPaymant, out res);  return res; } }

        decimal MoneySum;
        public decimal SumCashDisbursement { get; set; } = 0;
        public decimal SumMaxWallet { get; set; } = 0;
        public bool IsPaymentBonuses { get; set; } = false;
        decimal _SumUseWallet = 0;        
        public decimal SumUseWallet { get { return _SumUseWallet; } set { _SumUseWallet = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SumUseWalletUp)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SumUseWalletDown)));
                RoundSum.Text = (Math.Round(SumUseWalletUp, 2)).ToString();
                RoundSumDown.Text = (Math.Round(SumUseWalletDown, 2)).ToString();
            }
        } 
        public decimal SumUseWalletUp { get { return SumUseWallet>0? SumUseWallet : 0; } }
        public decimal SumUseWalletDown { get { return SumUseWallet < 0 ? -SumUseWallet : 0; } }
        public Receipt curReceipt { get { return MW?.curReceipt; } }
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
       
        public decimal MoneySumToRound { get; set; }    
        
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
            MoneySum = MW.MoneySum;
            SumMaxWallet = (MW.curReceipt?.MaxSumWallet < MW.Client?.Wallet ? MW.curReceipt?.MaxSumWallet : MW.Client?.Wallet) ?? 0;
            IsPaymentBonuses = MW.Client != null && MW.Client?.SumMoneyBonus >= MoneySum;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRounding"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SumMaxWallet"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsPaymentBonuses"));
            Rounding();
        }

        public void Init(MainWindow pMW) { MW = pMW; }

        public void TransferAmounts(decimal moneySum)
        {
            MoneySumToRound = moneySum;
            //MoneySumPayTextBox.Text = moneySum.ToString();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MoneySumToRound"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCashPayment"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ChangeSumPaymant"));
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
            //MoneySumPayTextBox.Text = ChangeSumPaymant;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCashPayment"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ChangeSumPaymant"));

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
            var task = Task.Run(() => MW.PrintAndCloseReceipt(null, eTypePay.Cash, ChangeSumPaymantDecimal, 0, -SumUseWallet));
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


        public decimal RoundingUpPrice(decimal price, decimal precision)
        {
            price = Convert.ToInt32(Math.Round(price * 100, 3));
            precision = Math.Round(precision, 2);            
            MoneySumToRound = Math.Round(Math.Ceiling(Math.Ceiling(price / precision / 100)) * precision, 2);
            SumUseWallet = MoneySumToRound - MoneySum;
            return MoneySumToRound;
        }

        public decimal RoundingDownPrice(decimal price, decimal precision)
        {
            price = Convert.ToInt32(Math.Round(price * 100, 3));
            precision = Math.Round(precision, 2);
            MoneySumToRound = Math.Round(Math.Floor(Math.Floor(price / precision / 100)) * precision, 2);
            
            if (MoneySum - MoneySumToRound > SumMaxWallet)
                MoneySumToRound = RoundingUpPrice(MoneySum - SumMaxWallet, 1.0m);            

            SumUseWallet = MoneySumToRound - MoneySum;
            return MoneySumToRound;
        }

        private void MoneySumPayChange(object sender, TextChangedEventArgs e)
        {
            CalculateReturn();
        }

        private void CalculateReturn()
        {              
                if (ChangeSumPaymantDecimal>0)
                    RestMoney = Math.Round((ChangeSumPaymantDecimal - Convert.ToDecimal(MoneySumToRound)), 2);
                else
                    RestMoney = 0;
        }

        private void Rounding(string Name = "")
        {
              decimal tmp = 0;
            switch (Name)
            {
                case "plus05":
                     RoundingUpPrice(MoneySum, 0.5m);                   
                    break;
                case "plus1":
                     RoundingUpPrice(MoneySum, 1.0m);
                    break;
                case "plus2":
                    RoundingUpPrice(MoneySum, 2.0m);                   
                    break;
                case "plus5":
                    RoundingUpPrice(MoneySum, 5.0m);
                    
                    break;
                case "plus10":
                     RoundingUpPrice(MoneySum, 10.0m);
                    break;
                case "minus1":
                    RoundingDownPrice(MoneySum,1);
                    break;
                case "minus2":
                    RoundingDownPrice(MoneySum, 2);
                    break;
                case "minus5":
                    RoundingDownPrice(MoneySum, 5);
                    break;
                case "minus10":
                    RoundingDownPrice(MoneySum, 10);
                    break;
                case "enterAmount":
                    MW.InputNumberPhone.Desciption = $"Максимальна сума списання: {SumMaxWallet}";
                    MW.InputNumberPhone.ValidationMask = "";
                    MW.InputNumberPhone.Result = "";
                    MW.NumericPad.Visibility = Visibility.Visible;
                    BackgroundPayment.Visibility = Visibility.Visible;
                    MW.InputNumberPhone.CallBackResult = (string result) =>
                    {
                        tmp = string.IsNullOrEmpty(result) ? 0 : Convert.ToDecimal(result);
                        if (tmp > MoneySum)
                        {
                            RoundingUpPrice(tmp, 1.0m);
                        }
                        else
                        {
                            if (MoneySum - tmp > SumMaxWallet)
                                tmp = MoneySum - SumMaxWallet;
                            RoundingUpPrice(tmp, 1.0m);
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
            ChangeSumPaymant = MoneySumToRound.ToString();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ChangeSumPaymant"));

        }

        private void _ButtonPaymentBonus(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Оплата бонусами!");
        }
    }
}
