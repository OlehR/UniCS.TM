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
using Front.ViewModels;
using ModelMID;
using UtilNetwork;

namespace Front.Control
{
    /// <summary>
    /// Interaction logic for PhoneVerification.xaml
    /// </summary>
    public partial class PhoneVerification : UserControl
    {
        PhoneVerificationVM PhoneVerificationVM { get; set; }
        MainWindow MW;
        public void Init(MainWindow mw)
        {
            MW = mw;
        }
        public void RefreshClient()
        {
            if (MW != null) {
                PhoneVerificationVM.Barcode = MW.Client?.BarCode;
                PhoneVerificationVM.NameCard = MW.Client?.NameClient;
                PhoneVerificationVM.CodeClient = (long)MW.Client?.CodeClient;
            }
        }
        public PhoneVerification()
        {
            InitializeComponent();
            PhoneVerificationVM = DataContext as PhoneVerificationVM;
            ButConfirmPhone.Click += (sender, e) =>
            {
                BorderNumPadPhoneVerification.Visibility = Visibility.Visible;
                NumPadPhoneVerification.Visibility = Visibility.Visible;
                bool status = true;
                NumPadPhoneVerification.Desciption = $"Введіть номер телефону";
                NumPadPhoneVerification.ValidationMask = "^\\d{10}(\\d{2})?$"; 
                NumPadPhoneVerification.Result = $"{PhoneVerificationVM.Phone}";
                NumPadPhoneVerification.IsEnableComma = false;

                NumPadPhoneVerification.CallBackResult = (res) =>
                {

                    if (!string.IsNullOrEmpty(res))
                    {
                        (PhoneVerificationVM.Phone, status) = PhoneCorrection(res);
                        MW.CustomMessage.Show($"Відправити SMS на номер {PhoneVerificationVM.Phone}?", "Підтвердження номеру телефону", eTypeMessage.Question);
                        MW.CustomMessage.Result = (bool response) =>
                        {
                            if (response) {
                                PhoneVerificationVM.SendVerifyCode();
                                this.ButVerifySMS.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                            }
                        };
                    }
                    else
                    {
                        PhoneVerificationVM.Phone = string.Empty;
                        this.ButConfirmPhone.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                    }
                    PhoneVerificationVM.OnPropertyChanged(nameof(PhoneVerificationVM.Phone));

                    if (!status)
                    {
                        BorderNumPadPhoneVerification.Visibility = Visibility.Visible;
                        NumPadPhoneVerification.Visibility = Visibility.Visible;
                    }
                };
            };

            ButVerifySMS.Click += (sender, e) =>
            {
                BorderNumPadPhoneVerification.Visibility = Visibility.Visible;
                NumPadPhoneVerification.Visibility = Visibility.Visible;
                NumPadPhoneVerification.Desciption = $"Введіть код підтвердження";
                NumPadPhoneVerification.ValidationMask = "^\\d{4}$";
                NumPadPhoneVerification.Result = $"";
                NumPadPhoneVerification.IsEnableComma = false;

                NumPadPhoneVerification.CallBackResult = (res) =>
                {
                    if (!string.IsNullOrEmpty(res))
                        PhoneVerificationVM.VerifyCode = res;
                    else
                    {
                        PhoneVerificationVM.VerifyCode = string.Empty;
                    }
                    PhoneVerificationVM.OnPropertyChanged(nameof(PhoneVerificationVM.VerifyCode));
                    if (PhoneVerificationVM.LastVerifyCode.Data == PhoneVerificationVM.VerifyCode)
                        PhoneVerificationVM.IsConfirmed = true;
                    else
                    {
                        PhoneVerificationVM.IsConfirmed = false;
                        if (!string.IsNullOrEmpty(PhoneVerificationVM.VerifyCode))
                            MW.CustomMessage.Show($"Введений код не вірний!", "Помилка!", eTypeMessage.Error);
                    }
                    PhoneVerificationVM.OnPropertyChanged(nameof(PhoneVerificationVM.IsConfirmed));
                };
            };
        }
        private (string, bool) PhoneCorrection(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return (phoneNumber, false);
            if (phoneNumber.IndexOf("38") == 0 && phoneNumber.Length == 12)
            {
                return (phoneNumber, true);
            }
            else
            {
                return ($"38{phoneNumber}", true);
            }
        }
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            MW.SetStateView(eStateMainWindows.WaitInput);
        }

        private void ConfirmNumber(object sender, RoutedEventArgs e)
        {
            
            Result result = PhoneVerificationVM.ConfirmPhone();
            MW.CustomMessage.Show(result.Success ? "Номер телефону збережено!" : result.TextError);
            MW.SetStateView(eStateMainWindows.WaitInput);
        }
    }
}
