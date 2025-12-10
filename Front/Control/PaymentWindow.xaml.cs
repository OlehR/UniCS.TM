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
using Front.Models;
using ModernExpo.SelfCheckout.Entities.Models.Terminal;

namespace Front.Control
{
    public partial class PaymentWindow : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string ChangeSumPaymant { get; set; } = "";
        public decimal ChangeSumPaymantDecimal { get { decimal res = 0; decimal.TryParse(ChangeSumPaymant, out res); return res; } }

        decimal MoneySum;
        public decimal SumCashDisbursement { get; set; } = 0;
        public decimal SumMaxWallet { get; set; } = 0;
        public bool IsPaymentBonuses { get; set; } = false;
        public bool IsUseСertificate { get => MW?.Client?.IsСertificate == true; }
        public bool EnteringPriceManually { get; set; } = false;
        decimal _SumUseWallet = 0;
        public string TypeReturn { get; set; }
        public decimal SumUseWallet // скільки списуємо з гаманця
        {
            get { return _SumUseWallet; }
            set
            {
                _SumUseWallet = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SumUseWalletUp)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SumUseWalletDown)));

                //RoundSum.Text = (Math.Round(SumUseWalletUp, 2)).ToString();
                //RoundSumDown.Text = (Math.Round(SumUseWalletDown, 2)).ToString();
            }
        }
        public decimal SumUseWalletUp { get { return SumUseWallet > 0 ? SumUseWallet : 0; } }
        public decimal SumUseWalletDown { get { return SumUseWallet < 0 ? -SumUseWallet : 0; } }
        public Receipt curReceipt { get { return MW?.curReceipt; } }
        public decimal RestMoney { get; set; }
        public bool IsCashPayment
        {
            get
            {
                if (SumCashDisbursement > 0)
                {
                    ChangeSumPaymant = MoneySumToRound.ToString();
                    return false;
                }
                else
                    return RestMoney >= 0 ? true : false;
            }
        }

        public decimal MoneySumToRound { get; set; }
        public decimal CashBackMoneySum { get; set; }
        public bool IsCashBackPay { get; set; } = false;

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
            if (MW.curReceipt?.RefundId != null)
                MW.curReceipt.Payment = MW.Bl.GetPayment(MW.curReceipt.RefundId);
            if (MW.curReceipt.Payment.Count() > 0)
                RefreshTypePayment();


            MoneySum = MW.MoneySum;
            ChangeSumPaymant = MoneySum.ToString(); //вікно де змінюється сума округлення і т.д.
            SumMaxWallet = (MW.curReceipt?.MaxSumWallet < MW.Client?.Wallet ? MW.curReceipt?.MaxSumWallet : MW.Client?.Wallet) ?? 0; //максмсальна сума списання з гаманця
            IsPaymentBonuses = MW.Client != null && MW.Client?.SumMoneyBonus >= MoneySum && MW.curReceipt.IsOnlyOrdinary; // оплата бонусами
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRounding")); // вертає тру якщо є юзер і впливає на панель з можливою знижкою по гаманцю і на кнопки округлення
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SumMaxWallet"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsPaymentBonuses"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUseСertificate)));

            Rounding();
        }
        void RefreshTypePayment()
        {
            TypeReturn = "";
            if (MW.curReceipt.Payment.Any(x => x.TypePay == eTypePay.Cash))
            {
                TypeReturn = eTypePay.Cash.ToString();
            }
            else
            {
                BankTerminal bank1, bank2;
                bank1 = MW.EF?.BankTerminal1 as BankTerminal;

                if (MW.curReceipt.Payment.First().CodeBank == bank1.CodeBank)
                    TypeReturn = "FirstTerminal";
                if (MW.EF?.BankTerminal2 != null)
                {
                    bank2 = MW.EF?.BankTerminal2 as BankTerminal;
                    if (MW.curReceipt.Payment.First().CodeBank == bank2.CodeBank)
                        TypeReturn = "SecondTerminal";
                }
            }
            if (string.IsNullOrEmpty(TypeReturn))
                TypeReturn = "AllPayments";



        }
        public void Init(MainWindow pMW) { MW = pMW; }

        public void TransferAmounts(decimal pMoneySum, decimal pSumCashBack)
        {
            MW.IsCashBackPay = false;
            MoneySumToRound = pMoneySum;
            CashBackMoneySum = pSumCashBack;
            SumCashDisbursement = 0;
            IsCashBackPay = CashBackMoneySum > 0 && Global.IdWorkPlaces.Count() == 1;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCashBackPay)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MoneySumToRound)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CashBackMoneySum)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SumCashDisbursement)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCashPayment)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChangeSumPaymant)));

            EnteringPriceManually = false;
            CalculateReturn();
        }
        private void ChangeSumPaymentButton(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (!EnteringPriceManually)
            {
                ChangeSumPaymant = "0";
                EnteringPriceManually = true;
            }
            switch (btn.Content)
            {
                case "C":
                    ChangeSumPaymant = "0";
                    //if (ChangeSumPaymant.Length <= 1)// якщо видаляють всю суму тоді виводимо 0
                    //{
                    //    ChangeSumPaymant = "0";
                    //    break;
                    //}
                    //else
                    //    ChangeSumPaymant = ChangeSumPaymant.Remove(ChangeSumPaymant.Length - 1);//видаляємо останній елемент
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
            MW?.Bl.db.ReplacePayment(pPay);

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
            CancelCashDisbursement();
            MW.SetStateView(eStateMainWindows.WaitInput);
        }

        private void _ButtonPaymentBank(object sender, RoutedEventArgs e)
        {
            Rounding();
            var btn = sender as Button;
            var bank = btn.CommandParameter as BankTerminal;
            MW?.EF.SetBankTerminal(bank);
            var task = Task.Run(() => MW.Blf.PrintAndCloseReceipt(null, eTypePay.Card, 0, SumCashDisbursement));
            MW.GiveRest = 0;
        }

        private void _ButtonPaymentCash(object sender, RoutedEventArgs e)
        {
            //MW.PaymentWindowKSO_UC.EquipmentStatusInPayment.Text = "";
            MW.EquipmentInfo = string.Empty;
            MW.GiveRest = (double)RestMoney;
            var task = Task.Run(() => MW.Blf.PrintAndCloseReceipt(null, eTypePay.Cash, ChangeSumPaymantDecimal, 0, -SumUseWallet));
        }

        private void CancelCashDisbursementButton(object sender, RoutedEventArgs e)
        {
            CancelCashDisbursement();
        }
        private void CancelCashDisbursement()
        {
            SumCashDisbursement = 0;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCashPayment"));
        }

        private void CashDisbursement(object sender, RoutedEventArgs e)
        {
            IdReceipt IdR = new() { CodePeriod = MW.curReceipt.CodePeriod, IdWorkplacePay = Global.IdWorkPlace };
            decimal SumCash = MW.EF.GetSumInCash(IdR);// Bl.db.GetSumCash(IdR);
            PaymentWindowUC_NumericPad.Desciption = $"Введіть суму видачі. В касі {SumCash}₴";
            PaymentWindowUC_NumericPad.ValidationMask = "";
            PaymentWindowUC_NumericPad.Result = "";
            PaymentWindowUC_NumericPad.CallBackResult = (string result) =>
            {

                if (!string.IsNullOrEmpty(result))
                {
                    var res = Convert.ToDecimal(result);
                    if (res >= Global.Settings.SumDisbursementMin && res <= Global.Settings.SumDisbursementMax)
                        if (SumCash >= res)
                            SumCashDisbursement = res;
                        else
                            MW.CustomMessage.Show($"Недостатня кількість готівки в касі! Максимальна сума видачі {SumCash}₴", "Помилка!", eTypeMessage.Error);
                    //MW.ShowErrorMessage($"Недостатня кількість готівки в касі! Максимальна сума видачі {SumCash}₴");

                    else
                        MW.CustomMessage.Show($"Сума видачі готівки повинна бути більша за {Global.Settings.SumDisbursementMin}₴ і меньше за {Global.Settings.SumDisbursementMax}₴", "Помилка!", eTypeMessage.Error);
                    //MW.ShowErrorMessage($"Сума видачі готівки повинна бути більша за {Global.Settings.SumDisbursementMin}₴ і меньше за {Global.Settings.SumDisbursementMax}₴");

                }
                BackgroundPayment.Visibility = Visibility.Collapsed;
            };
            BackgroundPayment.Visibility = Visibility.Visible;
            PaymentWindow_NumericPad.Visibility = Visibility.Visible;
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
            TextBox sum = sender as TextBox;
            ChangeSumPaymant = sum.Text;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCashPayment"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ChangeSumPaymant"));
            CalculateReturn();
        }

        private void CalculateReturn()
        {
            if (ChangeSumPaymantDecimal > 0) // це поле це таж сама ціна просто з строки в decimal
                RestMoney = Math.Round((ChangeSumPaymantDecimal - Convert.ToDecimal(MoneySumToRound)), 2); //решта
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
                    RoundingDownPrice(MoneySum, 1);
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
                    PaymentWindowUC_NumericPad.Desciption = $"Максимальна сума списання: {SumMaxWallet}";
                    PaymentWindowUC_NumericPad.ValidationMask = "";
                    PaymentWindowUC_NumericPad.Result = "";
                    PaymentWindowUC_NumericPad.IsEnableComma = true;
                    PaymentWindow_NumericPad.Visibility = Visibility.Visible;
                    BackgroundPayment.Visibility = Visibility.Visible;
                    PaymentWindowUC_NumericPad.CallBackResult = (string result) =>
                    {
                        tmp = string.IsNullOrEmpty(result) ? 0 : Convert.ToDecimal(result);
                        //if (tmp > MoneySum)
                        //{
                        //    RoundingUpPrice(tmp, 1.0m);
                        //}
                        //else
                        //{
                        //    if (MoneySum - tmp > SumMaxWallet)
                        //        tmp = MoneySum - SumMaxWallet;
                        //    RoundingUpPrice(tmp, 1.0m);
                        //}
                        if (SumMaxWallet < tmp)
                        {
                            tmp = SumMaxWallet;
                            MW.CustomMessage.Show($"Вказана сума більша за максимально можлив", "Помилка!", eTypeMessage.Error);
                            //MW.ShowErrorMessage("Вказана сума більша за максимально можливу");
                        }


                        MoneySumToRound = MoneySum - tmp; // сума до якої ми округлюємо стоїть навпроти всього:
                        SumUseWallet = -tmp;
                        ChangeSumPaymant = MoneySumToRound.ToString();
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ChangeSumPaymant"));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MoneySumToRound"));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCashPayment"));
                        CalculateReturn();
                        BackgroundPayment.Visibility = Visibility.Collapsed;
                    };
                    break;
                default:
                    MoneySumToRound = (decimal)MW.MoneySum; //навпроти всього
                    SumUseWallet = 0; //?
                    break;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MoneySumToRound"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCashPayment"));
            CalculateReturn();
        }

        private void Round(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            EnteringPriceManually = false;
            Rounding(btn.Name);
            ChangeSumPaymant = MoneySumToRound.ToString();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ChangeSumPaymant"));

        }

        private void _ButtonPaymentBonus(object sender, RoutedEventArgs e)
        {
            // MessageBox.Show("Оплата бонусами!");
            MW.TypeAccessWait = eTypeAccess.UseBonus;
            if (!MW.Blf.SetConfirm(MW?.AdminSSC, true))
                MW.SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.UseBonus, null);
        }

        private void OpenMoneyBoxButton(object sender, RoutedEventArgs e)
        {
            MW.StartOpenMoneyBox();
        }

        private void _ButtonPaymentCashBack(object sender, RoutedEventArgs e)
        {
            Rounding();
            MW.IsCashBackPay = true;
            //MW?.EF.SetBankTerminal(bank);
            var task = Task.Run(() => MW.Blf.PrintAndCloseReceipt(null, eTypePay.Card, 0, SumCashDisbursement, 0, 0, true));
            MW.GiveRest = 0;
        }
    }
}
