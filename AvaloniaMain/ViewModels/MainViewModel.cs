using AvaloniaMain.Models;
using AvaloniaMain.ViewModels.Model;
using AvaloniaMain.Views;
using Front;
using Front.Equipments;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelMID;
using ReactiveUI;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

namespace AvaloniaMain.ViewModels
{
    public partial class MainViewModel : ViewModelBase,IMW
    {
       // public ObservableCollection<ReceiptWares> ListWares { get; set; }

        public Receipt curReceipt { get; set; }
        public ReceiptWares CurWares { get; set; }
        public Client Client { get { return curReceipt?.Client; } }
        public Sound s { get; set; }
        public ControlScale CS { get; set; }
        public BL Bl { get; set; }
        public EquipmentFront EF { get; set; }
        public eStateMainWindows State { get; set; }
        public eTypeAccess TypeAccessWait { get; set; }
        public bool IsShowWeightWindows { get; set; }

        public Client client=new Client();

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


        public MainViewModel()
        {

            Bl = BL.GetBL;
            Blf = new BLF();
            Blf.Init(this);
            EF = new EquipmentFront();

            EquipmentFront.OnBarCode += (pBarCode, pTypeBarCode) => GetBarCode(pBarCode, pTypeBarCode);

            _showSearchView = ReactiveCommand.CreateFromTask(SearchViewModel);
            _changeColorCommand = ReactiveCommand.CreateFromTask(ChangeColorAsync);
            _showUserInfo=ReactiveCommand.CreateFromTask(ShowUser);
            _showIssueCard = ReactiveCommand.CreateFromTask(ShowIssueCardAsync);
            _showNumPad = ReactiveCommand.CreateFromTask(NumPad);

            UserMoneyBonus = 17.10;
            UserMoneyBox = 121.35;

            NewReceipt();
            ListWares = new ObservableCollection<ReceiptWares>
            (new List<ReceiptWares>
            {

                new ReceiptWares
                {
                  NameWares="Яблуко",
                  PercentVat=0,
                  TypeVat=0,
                                    WeightBrutto=10,

                  Price=10,
                  SumDiscount=10.00M,
                  TypeFound=0,
                  Coefficient=0,
                  Quantity=1,

                },
                  new ReceiptWares
                {
                  NameWares="Яблуко",
                  PercentVat=0,
                  TypeVat=0,
                  Price=10,
                  SumDiscount=10,
                  TypeFound=0,
                                    WeightBrutto=10,

                  Coefficient=0,
                  Quantity=1,

                },
                    new ReceiptWares
                {
                  NameWares="Яблуко",
                  PercentVat=0,
                  TypeVat=0,
                  Price=10,
                  SumDiscount=10,
                                    WeightBrutto=10,

                  TypeFound=0,
                  Coefficient=0,
                  Quantity=1,

                },
                      new ReceiptWares
                {
                  NameWares="Яблуко",
                  PercentVat=0,
                  TypeVat=0,
                  Price=10,
                  SumDiscount=10,
                  WeightBrutto=10,
                  TypeFound=0,
                  Coefficient=0,
                  Quantity=1,

                },

            }) ;
        }
        public ReactiveCommand<Unit, Unit> ShowUserInfo => _showUserInfo;
        public ReactiveCommand<Unit, Unit> ShowSearchView => _showSearchView;
        public ReactiveCommand<Unit, Unit> ShowNumPad => _showNumPad;
        public ReactiveCommand<Unit, Unit> ShowIssueCard => _showIssueCard;



        public ReactiveCommand<Unit, Unit> ChangeColorCommand => _changeColorCommand;

       
        private async Task ShowUser()
        {
            CurrentPage = null;
            CurrentPage = new ClientInfoViewModel(this, client);
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

        private async Task NumPad()
        {
            CurrentPage = null;
            var parentViewModel = new NumPadViewModel("",false);
            parentViewModel.NumberChanged += NumPadViewModel_NumberChanged;
            parentViewModel.VisibilityChanged += NumPadViewModel_VisibilityChanged;
            CurrentPage = parentViewModel;
            BackgroundVisibility = true;
            CurrentPageVisibility = true;
        }
        private async Task SearchViewModel()
        {
          
            CurrentPage = null;
            var searchViewModel= new SearchViewModel();
            searchViewModel.VisibilityChanged += SearchView_VisibilityChanged;
            searchViewModel.WareSelect += AddWare;
            CurrentPage = searchViewModel;
            SearchViewVisibility = true;
        }

        private void NumPadViewModel_NumberChanged(object? sender, string newNumber)
        {
            UserNumber = newNumber;
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
                CurWares = Bl.AddWaresCode(curReceipt, pCodeWares, pCodeUnit, pQuantity, pPrice);

                if(CurWares!=null)
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
                        List<ReceiptWares> waresList=new List<ReceiptWares> ();

                        waresList.Add(CurWares);
                        receipt.Wares = waresList;
                    }
                   
                    SetCurReceipt(receipt);
                }
            }
        }

        private void NumPadViewModel_VisibilityChanged(object? sender, EventArgs? e)
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

        private async Task ChangeColorAsync()
        {
            Visibility = true;       
        }
        public void Close()
        {
            BackgroundVisibility = false;
            CurrentPage = null;
        }
        public void InitClient()
        {
            client.BirthDay= DateTime.Now;
            client.NameClient = "Станіслав Денис Іванович";
            client.Wallet = 100;
            client.SumBonus = 101;
            client.SumMoneyBonus = 102;
            client.MainPhone = "0995130322";
            client.PhoneAdd = "0995130323";
            client.PersentDiscount = 10;
           
            client.BarCode = "1233021030114";
            client.StatusCard = 0;
            
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