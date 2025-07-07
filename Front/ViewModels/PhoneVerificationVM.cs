using Front.Models;
using ModelMID;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Utils;
using UtilNetwork;

namespace Front.ViewModels
{
    public class PhoneVerificationVM : ViewModelBase
    {
        public Visibility NumPadVisibility { get; set; } = Visibility.Collapsed;
        public bool IsConfirmed { get; set; } = false;
        public Status<string> LastVerifyCode = new();
        private string _barcode = string.Empty;
        public string Barcode
        {
            get => _barcode;
            set
            {
                if (_barcode != value)
                {
                    _barcode = value;
                    OnPropertyChanged(nameof(Barcode));
                }
            }
        }
        private string _nameCard = string.Empty;
        public string NameCard
        {
            get => _nameCard;
            set
            {
                if (_nameCard != value)
                {
                    _nameCard = value;
                    OnPropertyChanged(nameof(NameCard));
                }
            }
        }
        public long CodeClient { get; set; } = 0;
        public string Phone { get; set; } = string.Empty;
        public string VerifyCode { get; set; } = string.Empty;

        public string UserBarCode { get; set; } = string.Empty;

        BL Bl;

        public PhoneVerificationVM()
        {
            Bl = BL.GetBL;
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
            Phone = string.Empty;
            Barcode = string.Empty;
            VerifyCode = string.Empty;
            NameCard = string.Empty;
            OnPropertyChanged(nameof(IsConfirmed));
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
        public void SendVerifyCode()
        {
            LastVerifyCode = Bl.ds.GetVerifySMS(Phone);
        }
        public Result ConfirmPhone()
        {
            SetPhone setPhone = new SetPhone() { CodeClient = CodeClient , Phone = Phone,
                UserBarCode = UserBarCode, CodeWarehouse= Global.CodeWarehouse,  IdWorkPlace = Global.IdWorkPlace };
        
            Result r = new(); 
            Task.Run(async () =>
            {
                r = await Bl.ds.SetPhoneNumber(setPhone);
            }).Wait();
            Reset();
            return r;
        }
    }
}
