using ModelMID;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaMain.ViewModels
{
    public class PaymentViewModel:ViewModelBase
    {
        public event EventHandler? VisibilityChanged;
        private ReactiveCommand<Unit, Unit> _closeCommand;
        public ReactiveCommand<Unit, Unit> CloseCommand => _closeCommand ??= ReactiveCommand.CreateFromTask(Close);
        MainViewModel MW;
        public string ChangeSumPaymant { get; set; } = "";
        decimal MoneySum;
        public bool IsPaymentBonuses { get; set; } = false;
        public decimal SumCashDisbursement { get; set; } = 0;
        public decimal SumMaxWallet { get; set; } = 0;
        public decimal MoneySumToRound { get; set; }
        private bool _visibility = false;
        public bool Visibility
        {
            get => _visibility;
            set
            {
                if (_visibility != value)
                {
                    _visibility = value;
                    OnPropertyChanged(nameof(Visibility));
                }
            }
        }
        private NumPadViewModel _numpad;
        public NumPadViewModel numpad
        {
            get=>_numpad;
            set
            {
                if(value != null)
                {
                    _numpad = value;
                    OnPropertyChanged(nameof(numpad));
                }
            }
        }
        decimal _SumUseWallet = 0;
        Client client { get => MW.Client; }
        public decimal RestMoney { get; set; }
        public decimal ChangeSumPaymantDecimal { get { decimal res = 0; decimal.TryParse(ChangeSumPaymant, out res); return res; } }
        public decimal SumUseWalletUp { get { return SumUseWallet > 0 ? SumUseWallet : 0; } }
        public decimal SumUseWalletDown { get { return SumUseWallet < 0 ? -SumUseWallet : 0; } }
        public bool IsRounding
        {
            get
            {
                if (SumCashDisbursement > 0)
                    return false;
                if (MW.Client!=null)
                {
                    return true;
                }
                return false;
            }
        }
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
        /*public static bool IsNotNull<T>(this T obj)
    {
        return !obj.IsNull();
    }
*/
        public decimal SumUseWallet
        {
            get { return _SumUseWallet; }
            set
            {
                _SumUseWallet = value;
               OnPropertyChanged(nameof(SumUseWalletUp));
                OnPropertyChanged(nameof(SumUseWalletDown));

                //RoundSum.Text = (Math.Round(SumUseWalletUp, 2)).ToString();
                //RoundSumDown.Text = (Math.Round(SumUseWalletDown, 2)).ToString();
            }
        }
        public ReactiveCommand<string, Unit> buttonRoundPressCommand { get; }
        public ReactiveCommand<string, Unit> ChangeSumButtonCommand { get; }

        public PaymentViewModel(MainViewModel mW)
        {
            MW = mW;
            numpad = new NumPadViewModel("", true, "", "123");
            UpdatePaymentWindow();
            buttonRoundPressCommand = ReactiveCommand.Create<string>(buttonRoundPress);
            ChangeSumButtonCommand = ReactiveCommand.Create<string>(ChangeSumButton);
        }
        public PaymentViewModel()
        {

        }
        public void UpdatePaymentWindow()
        {
            MoneySum = MW.MoneySum;
            ChangeSumPaymant = MoneySum.ToString();
            IsPaymentBonuses = MW.Client != null && MW.Client?.SumMoneyBonus >= MoneySum && MW.curReceipt.IsOnlyOrdinary;
            SumMaxWallet = (MW.curReceipt?.MaxSumWallet < MW.Client?.Wallet ? MW.curReceipt?.MaxSumWallet : MW.Client?.Wallet) ?? 0; //макс округлення
            OnPropertyChanged(nameof(SumMaxWallet));
            OnPropertyChanged(nameof(ChangeSumPaymant));
            OnPropertyChanged(nameof(IsPaymentBonuses));
            OnPropertyChanged(nameof(client));
            Rounding();
        }
        private void buttonRoundPress(string Name)
        {
            Rounding(Name);
            ChangeSumPaymant = MoneySumToRound.ToString();
            CalculateReturn();
            OnPropertyChanged(nameof(ChangeSumPaymant));
        }
        private void ChangeSumButton(string Name)
        {
          /*  if (!EnteringPriceManually)
            {
                ChangeSumPaymant = "0";
                EnteringPriceManually = true;
            }*/
            switch (Name)
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
                    ChangeSumPaymant += Name;
                    break;
                case "0":
                    if (ChangeSumPaymant.StartsWith("0")) //заборона вводу купи нулів на початку
                    {
                        break;
                    }
                    ChangeSumPaymant += Name;
                    break;
                default:
                    if (ChangeSumPaymant.StartsWith("0") && !ChangeSumPaymant.StartsWith("0,"))
                    {
                        ChangeSumPaymant = Name.ToString();
                        OnPropertyChanged(nameof(IsCashPayment));

                        OnPropertyChanged(nameof(ChangeSumPaymant));
                        CalculateReturn();
                        break;
                     
                    }
                    ChangeSumPaymant += Name;
                    break;

            }
            //MoneySumPayTextBox.Text = ChangeSumPaymant;
            OnPropertyChanged(nameof(IsCashPayment));
            
            OnPropertyChanged(nameof(ChangeSumPaymant));
            CalculateReturn();
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
                    var np = new NumPadViewModel("", true, "", $"максимальна сума списання {SumMaxWallet}");
                    np.VisibilityChanged += CloseNumPad;

                    /*     PaymentWindowUC_NumericPad.Desciption = $"Максимальна сума списання: {SumMaxWallet}";
                         PaymentWindowUC_NumericPad.ValidationMask = "";
                         PaymentWindowUC_NumericPad.Result = "";
                         PaymentWindowUC_NumericPad.IsEnableComma = true;
                         PaymentWindow_NumericPad.Visibility = Visibility.Visible;
                         BackgroundPayment.Visibility = Visibility.Visible;
                    np.NumberChanged +=;*/

                    Visibility = true;
                    break;
                default:
                    MoneySumToRound = (decimal)MW.MoneySum;
                    SumUseWallet = 0;
                    break;
            }
           OnPropertyChanged(nameof(MoneySumToRound));
           OnPropertyChanged(nameof(IsCashPayment));
            
            CalculateReturn();

        }
        private void CalculateReturn()
        {
            if (ChangeSumPaymantDecimal > 0)
                RestMoney = Math.Round((ChangeSumPaymantDecimal - Convert.ToDecimal(MoneySumToRound)), 2);
            else
                RestMoney = 0;
            OnPropertyChanged(nameof(RestMoney));
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
        public decimal RoundingUpPrice(decimal price, decimal precision)
        {
            price = Convert.ToInt32(Math.Round(price * 100, 3));
            precision = Math.Round(precision, 2);
            MoneySumToRound = Math.Round(Math.Ceiling(Math.Ceiling(price / precision / 100)) * precision, 2);
            SumUseWallet = MoneySumToRound - MoneySum;
            return MoneySumToRound;
        }



        public void CloseNumPad(object? sender, EventArgs? e)
        {
            Visibility = false;
            numpad = null;
        }
       
        private async Task Close()
        {

            VisibilityChanged.Invoke(this, EventArgs.Empty);
        }
    }
}
