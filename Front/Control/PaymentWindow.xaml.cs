﻿using Front.Equipments;
using Front.Models;
using ModelMID;
using SharedLib;
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
    /// Interaction logic for PaymentWindow.xaml
    /// </summary>
    
    public partial class PaymentWindow : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string ChangeSumPaymant { get; set; } = "";
        double tempMoneySum;
        private double _MoneySumToRound;
        public double MoneySumToRound { get 
            {
                MoneySumPayTextBox.Text = _MoneySumToRound.ToString();
                CheckAmountTextBlock.Text = _MoneySumToRound.ToString();
                return _MoneySumToRound;
            } set { _MoneySumToRound = value; } }
        public Receipt curReceipt;
        MainWindow MW;
        EquipmentFront EF;
        BL Bl;
        public PaymentWindow()
        {
            Bl = BL.GetBL;
            InitializeComponent();
            //MessageBox.Show(MoneySumToRound.ToString());
        }
        public void Init(MainWindow pMW)
        {
            MW = pMW;
            if (MW != null)
            {
                EF = MW?.EF;
            }

        }

        private void ChangeSumPaymentButton(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Content)
            {
                case "C":
                    if (ChangeSumPaymant.Length <= 1)
                    {
                        ChangeSumPaymant = "";
                        break;
                    }
                    else
                        ChangeSumPaymant = ChangeSumPaymant.Remove(ChangeSumPaymant.Length - 1);
                    break;

                case ",":
                    if (ChangeSumPaymant.IndexOf(",") != -1)
                    {
                        break;
                    }
                    ChangeSumPaymant += btn.Content;
                    break;
                default:
                    ChangeSumPaymant += btn.Content;
                    break;

            }
            MoneySumPayTextBox.Text = ChangeSumPaymant;
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
            Bl.db.ReplacePayment(new List<Payment>() { pPay });

            curReceipt.StateReceipt = eStateReceipt.Pay;
            curReceipt.CodeCreditCard = pPay.NumberCard;
            curReceipt.NumberReceiptPOS = pPay.NumberReceipt;
            curReceipt.SumCreditCard = pPay.SumPay;
            Bl.db.ReplaceReceipt(curReceipt);
            curReceipt.Payment = new List<Payment>() { pPay };
        }




        private void _Cancel(object sender, RoutedEventArgs e)
        {
            MW.SetStateView(Models.eStateMainWindows.WaitInput);
        }

        private void _ButtonPaymentBank(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var str = btn.Content as TextBlock;
            var r = EF.GetBankTerminal.Where(el => str.Text.Equals(el.Name));
            if (r.Count() == 1)
                EF.SetBankTerminal(r.First() as BankTerminal);

            var task = Task.Run(() => MW.PrintAndCloseReceipt());
        }

        private void _ButtonPayment(object sender, RoutedEventArgs e)
        {
            MW.EquipmentStatusInPayment.Text = "";
            if (Global.TypeWorkplace == eTypeWorkplace.СashRegister)
                MW.SetStateView(eStateMainWindows.ChoicePaymentMethod);
            else
            {
                var task = Task.Run(() => MW.PrintAndCloseReceipt());
            }
        }

        private void CancelCashDisbursement(object sender, RoutedEventArgs e)
        {
            CashDisbursementTextBox.Text = "0";
        }

        private void CashDisbursement(object sender, RoutedEventArgs e)
        {

            MW.InputNumberPhone.Desciption = "Введіть номер телефону";
            MW.InputNumberPhone.ValidationMask = "";
            MW.InputNumberPhone.Result = "";
            MW.InputNumberPhone.CallBackResult = (string result) => CashDisbursementTextBox.Text = result;
            MW.NumericPad.Visibility = Visibility.Visible;
        }

       
        public double RoundingPrice(double price, double precision)
        {
            price = Convert.ToInt32(Math.Round(price * 100, 3));
            precision = Math.Round(precision, 2);
            return Math.Round(Math.Ceiling(Math.Ceiling(price / precision / 100)) * precision, 2);
        }
        public double RoundingDownPrice(double price, double precision)
        {
            price = Convert.ToInt32(Math.Round(price * 100, 3));
            precision = Math.Round(precision, 2);
            return Math.Round(Math.Floor(Math.Floor(price / precision / 100)) * precision, 2);
        }

        private void MoneySumPayChange(object sender, TextChangedEventArgs e)
        {
            try
            {
                ResMoney.Text = Math.Round((Convert.ToDouble(MoneySumPayTextBox.Text) - Convert.ToDouble(MoneySumToRound)), 2).ToString();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //MW.ShowErrorMessage(ex.Message);
            }
        }

        private void Round(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            tempMoneySum = (double)MW.MoneySum;
            RoundSum.Text = "0";
            RoundSumDown.Text = "0";
            switch (btn.Name)
            {
                case "plus01":
                    MoneySumToRound = RoundingPrice(tempMoneySum, 0.1);
                    RoundSum.Text = (Math.Round(Convert.ToDouble(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;
                case "plus05":
                    MoneySumToRound = RoundingPrice(tempMoneySum, 0.5);
                    RoundSum.Text = (Math.Round(Convert.ToDouble(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;
                case "plus1":
                    MoneySumToRound = RoundingPrice(tempMoneySum, 1.0);
                    RoundSum.Text = (Math.Round(Convert.ToDouble(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;
                case "plus2":
                    MoneySumToRound = RoundingPrice(tempMoneySum, 2.0);
                    RoundSum.Text = (Math.Round(Convert.ToDouble(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;
                case "plus5":
                    MoneySumToRound = RoundingPrice(tempMoneySum, 5.0);
                    RoundSum.Text = (Math.Round(Convert.ToDouble(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;
                case "minus1":
                    MoneySumToRound = RoundingDownPrice(tempMoneySum, 1.0);
                    RoundSumDown.Text = (Math.Round(Convert.ToDouble(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;
                default:
                    MoneySumToRound = (double)MW.MoneySum;
                    break;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MoneySumToRound"));
        }

    }
}
