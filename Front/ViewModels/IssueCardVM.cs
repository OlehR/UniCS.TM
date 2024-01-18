using Front.Models;
using ModelMID;
using ModelMID.DB;
using SharedLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Utils;

namespace Front.ViewModels
{
    public class IssueCardVM : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        BL Bl;
        public string BarcodeIssueCard { get; set; } = string.Empty;
        public string PhoneIssueCard { get; set; } = string.Empty;
        public string VerifyCode { get; set; } = string.Empty;
        public StatusD<string> LastVerifyCode = new();
        public bool IsBarcodeIssueCard { get { return !string.IsNullOrEmpty(BarcodeIssueCard); } }
        public bool IsGetCard { get; set; } = false;

        public string NumPadDesciption { get; set; } = string.Empty;
        public Visibility NumPadVisibility { get; set; } = Visibility.Collapsed;

        public IssueCardVM()
        {
            Bl = new BL();
        }
        MainWindow MW;
        public void Init(MainWindow mw)
        {

            MW = mw;
        }
        public RelayCommand Cancel
        {
            get
            {
                return new RelayCommand((obj) =>
                  {
                      Reset();
                  });
            }
        }
        public void Reset()
        {
            PhoneIssueCard = string.Empty;
            BarcodeIssueCard = string.Empty;
            VerifyCode = string.Empty;
            OnPropertyChanged(nameof(IsBarcodeIssueCard));
        }
        public RelayCommand EnterPhone
        {
            get
            {
                return new RelayCommand((obj) =>
                {
                    NumPadVisibility = Visibility.Visible;
                });
            }
        }
        public RelayCommand EnterVerifyCode
        {
            get
            {
                return new RelayCommand((obj) =>
                {
                    NumPadVisibility = Visibility.Visible;
                });
            }
        }
        public RelayCommand SendVerifyCode
        {
            get
            {
                return new RelayCommand((obj) =>
                {
                    LastVerifyCode = Bl.ds.GetVerifySMS(PhoneIssueCard);
                });
            }
        }
        public eReturnClient IssueNewCardButton()
        {
            ClientNew clientNew = new ClientNew() { BarcodeCashier =  Bl.db.GetConfig<string>("CodeAdminSSC"), BarcodeClient = BarcodeIssueCard, IdWorkplace = Global.IdWorkPlace, Phone = PhoneIssueCard, DateCreate = DateTime.Now };
            eReturnClient r = eReturnClient.Error;
            Task.Run(async () =>
            {
                r = await Bl.ds.Send1CClientAsync(clientNew);
            }).Wait();

            clientNew.State = r == eReturnClient.Ok ? 1 : 0;
            Bl.db.ReplaceClientNew(clientNew);
            if (r == eReturnClient.Ok)
                Reset();
            return r;
        }
    }
}
