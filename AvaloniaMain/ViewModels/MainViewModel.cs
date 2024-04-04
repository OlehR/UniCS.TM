using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaMain.Models;
using AvaloniaMain.ViewModels.Model;
using AvaloniaMain.Views;
using Front;
using Front.Equipments;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelMID;

using ReactiveUI;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Utils;

namespace AvaloniaMain.ViewModels
{
    public partial class MainViewModel : ViewModelBase, IMW
    {
        // public ObservableCollection<ReceiptWares> ListWares { get; set; }

        private Receipt _curReceipt { get; set; }
        public Receipt curReceipt
        {
            get => _curReceipt;
            set
            {
                if (_curReceipt != value)
                {
                    _curReceipt = value;
                    OnPropertyChanged(nameof(_curReceipt));
                    OnPropertyChanged(nameof(Client));
                    OnPropertyChanged(nameof(IsUserActive));
                }

            }
        }
        public bool IsUserActive 
        { get
            {
                if (curReceipt.Client != null) { return true; }
                else return false;
            }
     
            
        }
        public ReceiptWares CurWares { get; set; }
        public Client Client { get { return curReceipt?.Client; }}
        public Sound s { get; set; }
        public ReactiveCommand<ReceiptWares, Unit> Delete { get; }
        public ReactiveCommand<ReceiptWares, Unit> ChangeQuantityMinus { get; }
        public ReactiveCommand<ReceiptWares, Unit> ChangeQuantityPlus { get; }
        public ControlScale CS { get; set; }
        public BL Bl { get; set; }
        public EquipmentFront EF { get; set; }
        public eStateMainWindows State { get; set; }
        public eTypeAccess TypeAccessWait { get; set; }
        public bool IsShowWeightWindows { get; set; }
        public string EquipmentInfo { get; set; }
        public bool IsWaitAdminTitle { get; set; }
        public ModelMID.DB.User AdminSSC { get; set; } = null;
        public Status<string> LastVerifyCode { get; set; } = new();
        public Client client = new Client();

        public BLF Blf;

        private ObservableCollection<ReceiptWares> _ListWares;
        public ObservableCollection<ReceiptWares> ListWares
        {
            get => _ListWares;
            set
            {
                if (_ListWares != value)
                {
                    _ListWares = value;
                    OnPropertyChanged(nameof(ListWares));
                }
            }
        }
        private ViewModelBase? _currentPage;
        public ViewModelBase? CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged(nameof(CurrentPage));
                }
            }
        }
         private ViewModelBase? _NumPadPage;
        public ViewModelBase? NumPadPage
        {
            get => _NumPadPage;
            set
            {
                if (_NumPadPage != value)
                {
                    _NumPadPage = value;
                    OnPropertyChanged(nameof(NumPadPage));
                }
            }
        }
        
        private bool _currentPageVisibility = false;
        public bool CurrentPageVisibility
        {
            get => _currentPageVisibility;
            set
            {
                if (_currentPageVisibility != value)
                {
                    _currentPageVisibility = value;
                    OnPropertyChanged(nameof(CurrentPageVisibility));
                }
            }
        }
        private bool _NumPadPageVisibility = false;
        public bool NumPadPageVisibility
        {
            get => _NumPadPageVisibility;
            set
            {
                if (_NumPadPageVisibility != value)
                {
                    _NumPadPageVisibility = value;
                    OnPropertyChanged(nameof(NumPadPageVisibility));
                }
            }
        }

        private bool _backgroundVisibility = false;
        public bool BackgroundVisibility
        {
            get => _backgroundVisibility;
            set
            {
                if (_backgroundVisibility != value)
                {
                    _backgroundVisibility = value;
                    OnPropertyChanged(nameof(BackgroundVisibility));
                }
            }
        }

        // Додаткові властивості можна змінити аналогічно...

        private bool _SearchViewVisibility = false;
        public bool SearchViewVisibility
        {
            get => _SearchViewVisibility;
            set
            {
                if (_SearchViewVisibility != value)
                {
                    _SearchViewVisibility = value;
                    OnPropertyChanged(nameof(SearchViewVisibility));
                }
            }
        }

        private bool _clientInfoVIsibility = false;
        public bool ClientInfoVIsibility
        {
            get => _clientInfoVIsibility;
            set
            {
                if (_clientInfoVIsibility != value)
                {
                    _clientInfoVIsibility = value;
                    OnPropertyChanged(nameof(ClientInfoVIsibility));
                }
            }
        }

        private bool _mainVIsibility = true;
        public bool MainVIsibility
        {
            get => _mainVIsibility;
            set
            {
                if (_mainVIsibility != value)
                {
                    _mainVIsibility = value;
                    OnPropertyChanged(nameof(MainVIsibility));
                }
            }
        }

        private string _userNumber;
        public string UserNumber
        {
            get => _userNumber;
            set
            {
                if (_userNumber != value)
                {
                    _userNumber = value;
                    OnPropertyChanged(nameof(UserNumber));
                }
            }
        }

        private double _UserMoneyBox;
        public double UserMoneyBox
        {
            get => _UserMoneyBox;
            set
            {
                if (_UserMoneyBox != value)
                {
                    _UserMoneyBox = value;
                    OnPropertyChanged(nameof(UserMoneyBox));
                }
            }
        }

        private double _UserMoneyBonus;
        public double UserMoneyBonus
        {
            get => _UserMoneyBonus;
            set
            {
                if (_UserMoneyBonus != value)
                {
                    _UserMoneyBonus = value;
                    OnPropertyChanged(nameof(UserMoneyBonus));
                }
            }
        }

       

        private string _buttonColor = "#419e08";
        public string ButtonColor
        {
            get => _buttonColor;
            set
            {
                if (_buttonColor != value)
                {
                    _buttonColor = value;
                    OnPropertyChanged(nameof(ButtonColor));
                }
            }
        }



        private ReactiveCommand<Unit, Unit> _changeColorCommand;
        private ReactiveCommand<Unit, Unit> _showNumPad;

        private ReactiveCommand<Unit, Unit> _showUserInfo;
        private ReactiveCommand<Unit, Unit> _showIssueCard;
        public ReactiveCommand<Unit, Unit> _showSearchView;
        public ReactiveCommand<CustomWindow, Unit> _showCustomWindow;


        public MainViewModel()
        {
            Delete = ReactiveCommand.Create<ReceiptWares>(DeleteItem);
            ChangeQuantityMinus = ReactiveCommand.Create<ReceiptWares>(MinusItem);
            ChangeQuantityPlus = ReactiveCommand.Create<ReceiptWares>(PlusItem);
            Bl = BL.GetBL;
            Blf = new BLF();
            
            Blf.Init(this);
            EF = new EquipmentFront();
            InitAction();
            EF.Init();
            State = eStateMainWindows.WaitInput;
            EquipmentFront.OnBarCode += (pBarCode, pTypeBarCode) => GetBarCode(pBarCode, pTypeBarCode);

            _showSearchView = ReactiveCommand.CreateFromTask(SearchViewModel);
            _showUserInfo = ReactiveCommand.CreateFromTask(ShowUser);
            _showIssueCard = ReactiveCommand.CreateFromTask(ShowIssueCardAsync);
            _showNumPad = ReactiveCommand.CreateFromTask(NumPad);
            _showCustomWindow = ReactiveCommand.Create<CustomWindow>(ShowCustomWindowAsync);

            UserMoneyBonus = 17.10;
            UserMoneyBox = 121.35;

            NewReceipt();
            ListWares = new ObservableCollection<ReceiptWares>();

        }
        public ReactiveCommand<Unit, Unit> ShowUserInfo => _showUserInfo;
        public ReactiveCommand<Unit, Unit> ShowSearchView => _showSearchView;
        public ReactiveCommand<Unit, Unit> ShowNumPad => _showNumPad;
        public ReactiveCommand<Unit, Unit> ShowIssueCard => _showIssueCard;
        public ReactiveCommand<CustomWindow, Unit> ShowCustomWindow => _showCustomWindow;


        private async Task ShowUser()
        {
            CurrentPage = null;
            CurrentPage = new ClientInfoViewModel(this, curReceipt.Client);
            CurrentPageVisibility = true;
            BackgroundVisibility = true;

        }

        private async Task ShowIssueCardAsync()
        {
            CurrentPage = null;
            var issueCardViewModel = new IssueCardViewModel();
            issueCardViewModel.VisibilityChanged += IssueCard_VisibilityChanged;
            CurrentPage = issueCardViewModel;
            BackgroundVisibility = true;
            CurrentPageVisibility = true;

        }
        private void ShowCustomWindowAsync(CustomWindow pCw)
        {
            CurrentPage = null;
            var CwViewModel = new CustomWindowViewModel(pCw);
            CwViewModel.VisibilityChanged+= ShowCustomWindowVisibilityChanged;
            CwViewModel.SelectClientEvent += CustomWindowClickButton;
            CurrentPage = CwViewModel;
            BackgroundVisibility = true;
            CurrentPageVisibility = true;

        }

        private async Task NumPad()
        {
            string mask = "^[0-9]{10}$";
            NumPadPage = null;
            var parentViewModel = new NumPadViewModel("", false,mask);
            parentViewModel.NumberChanged += NumPadViewModel_NumberChanged;
            parentViewModel.VisibilityChanged += NumPadViewModel_VisibilityChanged;
            NumPadPage = parentViewModel;
            BackgroundVisibility = true;
            NumPadPageVisibility = true;
        }
        private async Task SearchViewModel()
        {

            CurrentPage = null;
            var searchViewModel = new SearchViewModel();
            searchViewModel.VisibilityChanged += SearchView_VisibilityChanged;
            searchViewModel.WareSelect += AddWare;
            CurrentPage = searchViewModel;
            SearchViewVisibility = true;
        }

        private void NumPadViewModel_NumberChanged(object? sender, string pResult)
        {
            if (curReceipt == null)
                Blf.NewReceipt();
            if (pResult.Length == 4)
            {
                if (int.TryParse(pResult.Substring(0, 4), out int res))
                    Bl.GetDiscount(new FindClient { PinCode = res }, curReceipt);
                return;
            }

            if (pResult.Length >= 10)
            {
                var r = new CustomWindowAnswer()
                {
                    idReceipt = curReceipt,
                    Id = eWindows.PhoneClient,
                    IdButton = 1,
                    Text = pResult,
                    ExtData = CS?.RW
                };
                
                Bl.SetCustomWindows(r);
                curReceipt = curReceipt;    
            }
          
        }
        private void AddWare(object? sender, GWA ware)
        {
            AddWares(ware.Code, ware.CodeUnit);
        }
        public void AddWares(int pCodeWares, int pCodeUnit = 0, decimal pQuantity = 0m, decimal pPrice = 0m, GW pGV = null)
        {
            if (pCodeWares > 0)
            {
                if (curReceipt == null)
                    Blf.NewReceipt();
                CurWares = Bl.AddWaresCode(curReceipt, pCodeWares, pCodeUnit, 1, pPrice);

                if (CurWares != null)
                    Blf.IsPrises(pQuantity, pPrice);
            }

            /*if (pCodeWares > 0)
            {
                if (curReceipt == null)
                    Blf.NewReceipt();
                CurWares = Bl.AddWaresCode(curReceipt, pCodeWares, pCodeUnit, pQuantity, pPrice);
                CurWares.Quantity = 1;
                if (CurWares != null)
                {
                    Receipt receipt = curReceipt;
                    if (receipt.Wares != null)
                    {
                        List<ReceiptWares> waresList = new List<ReceiptWares>(receipt.Wares);
                        waresList.Add(CurWares);
                        receipt.Wares = waresList;
                    }
                    else
                    {
                        List<ReceiptWares> waresList = new List<ReceiptWares>();

                        waresList.Add(CurWares);
                        receipt.Wares = waresList;
                    }

                    SetCurReceipt(receipt);
                }
            }*/
        }

        private void NumPadViewModel_VisibilityChanged(object? sender, EventArgs? e)
        {
           NumPadPageVisibility = false;
            NumPadClose();
        }
        
        private void ShowCustomWindowVisibilityChanged(object? sender, EventArgs? e)
        {
            CurrentPageVisibility = false;
            Close();
        }
        private void SearchView_VisibilityChanged(object? sender, EventArgs? e)
        {
            SearchViewVisibility = false;
            Close();

        }
        private void IssueCard_VisibilityChanged(object? sender, EventArgs? e)
        {
            CurrentPageVisibility = false;
            Close();
        }

    
        public void Close()
        {
            BackgroundVisibility = false;
            CurrentPage = null;
        }
        public void NumPadClose()
        {
            BackgroundVisibility = false;
            NumPadPage = null;
        }
     
        public void DeleteItem (ReceiptWares rp)
        {
            List<ReceiptWares> list = (List<ReceiptWares>)curReceipt.Wares;
            list.Remove(rp);
            Bl.ChangeQuantity(rp, 0);

            curReceipt.Wares = list;
            SetCurReceipt(curReceipt);
        }
        public void MinusItem(ReceiptWares rp)
        {
            Bl.ChangeQuantity(rp, rp.Quantity - 1);
       

        }
        public void PlusItem(ReceiptWares rp)
        {
          

            Bl.ChangeQuantity(rp, rp.Quantity + 1);

        }
        private void CustomWindowClickButton(object sender, CustomButton cb, string Text)
        {

            var r = new CustomWindowAnswer()
            {
                
                idReceipt = curReceipt,
                Id = cb.CustomWindow?.Id ?? eWindows.NoDefinition,
                IdButton = cb.Id,
                Text = Text,
                ExtData = cb.CustomWindow?.Id == eWindows.ConfirmWeight ? CS?.RW : null
            };
            Bl.SetCustomWindows(r);
        }

    }
}





/*  private bool _numPadVIsibility = false;
          public bool NumPadVIsibility
          {
              get => _numPadVIsibility;
              set => this.RaiseAndSetIfChanged(ref _numPadVIsibility, value);
          }
          private bool _issueCardVisibility = false;
          public bool IssueCardVisibility
          {
              get => _issueCardVisibility;
              set => this.RaiseAndSetIfChanged(ref _issueCardVisibility, value);
          }
          private bool _productsVisibility=true;
          public bool ProductsVisibility
          {
              get => _productsVisibility;
              set=>this.RaiseAndSetIfChanged(ref _productsVisibility, value);
          }
        
               public bool MainVIsibility
        {
            get => _mainVIsibility;
            set => this.RaiseAndSetIfChanged(ref _mainVIsibility, value);
        }
        private bool _keyBoardVIsibility = true;

        public bool KeyBoardVIsibility
        {
            get => _keyBoardVIsibility;
            set => this.RaiseAndSetIfChanged(ref _keyBoardVIsibility, value);
        }*/