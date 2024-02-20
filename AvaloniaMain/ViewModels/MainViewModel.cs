using AvaloniaMain.Models;
using AvaloniaMain.Views;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

namespace AvaloniaMain.ViewModels
{
    public class MainViewModel : ViewModelBase
    {




        public ViewModelBase? _currentPage;

        public ViewModelBase? CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }
        private bool _currentPageVisibility = false;
        public bool CurrentPageVisibility
        {
            get => _currentPageVisibility;
            set => this.RaiseAndSetIfChanged(ref _currentPageVisibility, value);
        }

      

        private bool _backgroundVisibility = false;
        public bool BackgroundVisibility
        {
            get => _backgroundVisibility;
            set => this.RaiseAndSetIfChanged(ref _backgroundVisibility, value);
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

        private bool _SearchViewVisibility = false;
        public bool SearchViewVisibility
        {
            get => _SearchViewVisibility;
            set => this.RaiseAndSetIfChanged(ref _SearchViewVisibility, value);
        }
        private bool _clientInfoVIsibility = false;
        public bool ClientInfoVIsibility
        {
            get => _clientInfoVIsibility;
            set => this.RaiseAndSetIfChanged(ref _clientInfoVIsibility, value);
        }
        private bool _mainVIsibility = true;

  
        private string _userNumber;
        public string UserNumber
        {
            get => (_userNumber);
            set => this.RaiseAndSetIfChanged(ref _userNumber, value);
        }

        private double _UserMoneyBox;
        public double UserMoneyBox
        {
            get => (_UserMoneyBox);
            set => this.RaiseAndSetIfChanged(ref _UserMoneyBox, value);
        }
        private double _UserMoneyBonus;
        public double UserMoneyBonus
        {
            get => (_UserMoneyBonus);
            set => this.RaiseAndSetIfChanged(ref _UserMoneyBonus, value);
        }

        private ReactiveCommand<Unit, Unit> _changeColorCommand;
        private ReactiveCommand<Unit, Unit> _showNumPad;
        private ReactiveCommand<Unit, Unit> _showUserInfo;
        private ReactiveCommand<Unit, Unit> _showIssueCard;
        public ReactiveCommand<Unit, Unit>  _showSearchView;
        private bool _visibility = false;
        public bool Visibility
        {
            get => _visibility;
            set => this.RaiseAndSetIfChanged(ref _visibility, value);
        }

        private string _buttonColor = "#419e08";



        public ObservableCollection<RecieptPositionModel> RecieptPosition { get; set; }

        public MainViewModel()
        {
            _showSearchView = ReactiveCommand.CreateFromTask(SearchViewModel);
            _changeColorCommand = ReactiveCommand.CreateFromTask(ChangeColorAsync);
            _showUserInfo=ReactiveCommand.CreateFromTask(ShowUser);
            _showIssueCard = ReactiveCommand.CreateFromTask(ShowIssueCardAsync);
            _showNumPad = ReactiveCommand.CreateFromTask(NumPad);

            UserMoneyBonus = 17.10;
            UserMoneyBox = 121.35;

            RecieptPosition = new ObservableCollection<RecieptPositionModel>
            (new List<RecieptPositionModel>
            {

                new RecieptPositionModel
                {
                    Name = "Яблука (Голден)",
                    PricePerOne = 20,
                    Weight = 1.253,
                    Count = 1,
                    CountByWeight = true,
                    TotalPrice = (decimal)(1.253 * 20)
                },
                  new RecieptPositionModel
                {
                    Name = "Яблука (Голден)",
                    PricePerOne = 20,
                    Weight = 1.253,
                    Count = 1,
                    CountByWeight = true,
                    TotalPrice = (decimal)(1.253 * 20)
                },
                    new RecieptPositionModel
                {
                    Name = "Яблука (Голден)",
                    PricePerOne = 20,
                    Weight = 1.253,
                    Count = 1,
                    CountByWeight = true,
                    TotalPrice = (decimal)(1.253 * 20)
                },
                      new RecieptPositionModel
                {
                    Name = "Яблука (Голден)",
                    PricePerOne = 20,
                    Weight = 1.253,
                    Count = 1,
                    CountByWeight = true,
                    TotalPrice = (decimal)(1.253 * 20)
                },
                        new RecieptPositionModel
                {
                    Name = "Яблука (Голден)",
                    PricePerOne = 20,
                    Weight = 1.253,
                    Count = 1,
                    CountByWeight = true,
                    TotalPrice = (decimal)(1.253 * 20)
                },
                          new RecieptPositionModel
                {
                    Name = "Яблука (Голден)",
                    PricePerOne = 20,
                    Weight = 1.253,
                    Count = 1,
                    CountByWeight = true,
                    TotalPrice = (decimal)(1.253 * 20)
                },
                            new RecieptPositionModel
                {
                    Name = "Яблука (Голден)",
                    PricePerOne = 20,
                    Weight = 1.253,
                    Count = 1,
                    CountByWeight = true,
                    TotalPrice = (decimal)(1.253 * 20)
                },
                              new RecieptPositionModel
                {
                    Name = "Яблука (Голден)",
                    PricePerOne = 20,
                    Weight = 1.253,
                    Count = 1,
                    CountByWeight = true,
                    TotalPrice = (decimal)(1.253 * 20)
                },
                                new RecieptPositionModel
                {
                    Name = "Яблука (Голден)Яблука (Голден)Яблука (Голден)Яблука (Голден)Яблука (Голден) Яблука (Голден) Яблука (Голден)",
                    PricePerOne = 20,
                    Weight = 1.253,
                    Count = 1,
                    CountByWeight = true,
                    TotalPrice = (decimal)(1.253 * 20)
                },
                new RecieptPositionModel
                {
                    Name = "Шоколад молочний Рошен",
                    PricePerOne = 20,
                    Weight = 1.253,
                    Count = 1,
                    CountByWeight = false,
                    TotalPrice = 20
                },
                 new RecieptPositionModel
                {
                    Name = "Шоколад молочний Рyшен",
                    PricePerOne = 20,
                    Weight = 1.253,
                    Count = 1,
                    CountByWeight = false,
                    TotalPrice = 20
                },
            });
        }
        public ReactiveCommand<Unit, Unit> ShowUserInfo => _showUserInfo;
        public ReactiveCommand<Unit, Unit> ShowSearchView => _showSearchView;
        public ReactiveCommand<Unit, Unit> ShowNumPad => _showNumPad;
        public ReactiveCommand<Unit, Unit> ShowIssueCard => _showIssueCard;



        public ReactiveCommand<Unit, Unit> ChangeColorCommand => _changeColorCommand;

        public string ButtonColor
        {
            get => _buttonColor;
            set => this.RaiseAndSetIfChanged(ref _buttonColor, value);
        }
        private async Task ShowUser()
        {
            CurrentPage = null;
            CurrentPage = new ClientInfoViewModel(this);
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
            CurrentPage = searchViewModel;
            SearchViewVisibility = true;
        }

        private void NumPadViewModel_NumberChanged(object? sender, string newNumber)
        {
            UserNumber = newNumber;
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
     
    }
}