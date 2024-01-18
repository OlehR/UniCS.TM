using Front.Models;
using Front.ViewModels;
using ModelMID;
using System.Windows;
using System.Windows.Controls;
using Utils;

namespace Front.Control
{


    /// <summary>
    /// Interaction logic for IssueCard.xaml
    /// </summary>
    public partial class IssueCard : UserControl
    {
        IssueCardVM IssueCardVM { get; set; }
        MainWindow MW;
        public void Init(MainWindow mw)
        {
            MW = mw;
        }
        public IssueCard()
        {


            InitializeComponent();
            IssueCardVM = DataContext as IssueCardVM;
            ButPhoneIssueCard.Click += (sender, e) =>
            {
                BorderNumPadIssueCard.Visibility = Visibility.Visible;
                NumPadIssueCard.Visibility = Visibility.Visible;
                bool status = true;
                NumPadIssueCard.Desciption = $"Введіть номер телефону";
                NumPadIssueCard.ValidationMask = "^[0-9]{10}$";
                NumPadIssueCard.Result = $"{IssueCardVM.PhoneIssueCard}";
                NumPadIssueCard.IsEnableComma = false;
                NumPadIssueCard.CallBackResult = (res) =>
                {

                    if (!string.IsNullOrEmpty(res))
                        (IssueCardVM.PhoneIssueCard, status) = PhoneCorrection(res);
                    else
                    {
                        IssueCardVM.PhoneIssueCard = string.Empty;
                        this.ButPhoneIssueCard.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                    }
                    IssueCardVM.OnPropertyChanged(nameof(IssueCardVM.PhoneIssueCard));

                    if (!status)
                    {
                        BorderNumPadIssueCard.Visibility = Visibility.Visible;
                        NumPadIssueCard.Visibility = Visibility.Visible;
                    }
                };
            };


            ButVerifySMS.Click += (sender, e) =>
            {
                BorderNumPadIssueCard.Visibility = Visibility.Visible;
                NumPadIssueCard.Visibility = Visibility.Visible;
                NumPadIssueCard.Desciption = $"Введіть код підтвердження";
                NumPadIssueCard.ValidationMask = "";
                NumPadIssueCard.Result = $"";
                NumPadIssueCard.IsEnableComma = false;

                NumPadIssueCard.CallBackResult = (res) =>
                {

                    if (!string.IsNullOrEmpty(res))
                        IssueCardVM.VerifyCode = res;
                    else
                    {
                        IssueCardVM.VerifyCode = string.Empty;
                    }
                    IssueCardVM.OnPropertyChanged(nameof(IssueCardVM.VerifyCode));
                    if (IssueCardVM.LastVerifyCode.Data == IssueCardVM.VerifyCode)
                        IssueCardVM.IsGetCard = true;
                    else
                    {
                        IssueCardVM.IsGetCard = false;
                        if (!string.IsNullOrEmpty(IssueCardVM.VerifyCode))
                            MW.CustomMessage.Show($"Введений код не вірний!", "Помилка!", eTypeMessage.Error);
                    }
                    IssueCardVM.OnPropertyChanged(nameof(IssueCardVM.IsGetCard));

                };


            };
        }
        public void SetBarCode(string pBarCode) => IssueCardVM.BarcodeIssueCard = pBarCode;


                private void CancelClick(object sender, RoutedEventArgs e)
                {
                    MW.SetStateView(eStateMainWindows.WaitInput);
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

        private void ButSendVerifyCode(object sender, RoutedEventArgs e)
        {
            MW.CustomMessage.Show($"Код підтвердження надіслано за номером {IssueCardVM.PhoneIssueCard}", "Успішно!", eTypeMessage.Information);
            this.ButVerifySMS.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        }

        private void IssueNewCardButton(object sender, RoutedEventArgs e)
        {
            eReturnClient eReturn = IssueCardVM.IssueNewCardButton();
            MW.CustomMessage.Show(eReturn.GetDescription(), eReturn != eReturnClient.Ok ? "Помилка! Карточка не збереглась на сервері!!!" : "Карточка успішно збережена.", eTypeMessage.Information);

        }
    }

}
