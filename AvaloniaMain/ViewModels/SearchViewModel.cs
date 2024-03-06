using ModelMID;
using ReactiveUI;
using SharedLib;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

using Avalonia.Controls.Primitives;
using ReactiveUI;
using SharedLib;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Reactive;
using System.Text;
using Front.Equipments;

using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using AvaloniaMain.ViewModels.Model;

namespace AvaloniaMain.ViewModels
{
    internal class SearchViewModel : ViewModelBase
    {
        BL Bl;
        BLF Blf;
        int CodeFastGroup =0;
        int OffSet = 0;
        int MaxPage = 0;
        
        int Limit = 10;
        int Current = 0;
        public ReactiveCommand<string, Unit> Slide { get; }
        public ReactiveCommand<GWA, Unit> GetCodeFastGroup { get; }

        public ReactiveCommand<Unit, Unit> Back { get; }

        private bool _IsLeftEnable = false;
        public bool IsLeftEnable
        {
            get => _IsLeftEnable;
            set
            {
                if (_IsLeftEnable != value)
                {
                    _IsLeftEnable = value;
                    OnPropertyChanged(nameof(IsLeftEnable));
                }
            }
        }
        private bool _IsRightEnable = true;
        public bool IsRightEnable
        {
            get => _IsRightEnable;
            set
            {
                if (_IsRightEnable != value)
                {
                    _IsRightEnable = value;
                    OnPropertyChanged(nameof(IsRightEnable));
                }
            }
        }
        private List<GWA> _onPageProductsTop;
        public List<GWA> OnPageProductsTop
        {
            get => _onPageProductsTop;
            set
            {
                if (_onPageProductsTop != value)
                {
                    _onPageProductsTop = value;
                    OnPropertyChanged(nameof(OnPageProductsTop));
                }
            }
        }

        private List<GWA> _onPageProductsBottom;
        public List<GWA> OnPageProductsBottom
        {
            get => _onPageProductsBottom;
            set
            {
                if (_onPageProductsBottom != value)
                {
                    _onPageProductsBottom = value;
                    OnPropertyChanged(nameof(OnPageProductsBottom));
                }
            }
        }

        private List<GWA> _onPageProducts;
        public List<GWA> OnPageProducts
        {
            get => _onPageProducts;
            set
            {
                if (_onPageProducts != value)
                {
                    _onPageProducts = value;
                    OnPropertyChanged(nameof(OnPageProducts));
                }
            }
        }

        private List<GWA> _AllProducts;
        public List<GWA> AllProducts
        {
            get => _AllProducts;
            set
            {
                if (_AllProducts != value)
                {
                    _AllProducts = value;
                    OnPropertyChanged(nameof(AllProducts));
                }
            }
        }
        public event EventHandler? VisibilityChanged;
        private ViewModelBase _currentPage;
        public ViewModelBase CurrentPage
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
        public event EventHandler<string>? NumberChanged;


        private string _currentText = "";
        public string CurrentText
        {
            get => _currentText;
            set
            {
                if (_currentText != value)
                {
                    _currentText = value;
                    OnPropertyChanged(nameof(CurrentText));
                }
            }
        }

public SearchViewModel()
        {
            Back = ReactiveCommand.CreateFromTask(BackAction);

            GetCodeFastGroup = ReactiveCommand.Create<GWA>(GetCodeAction);
            Slide = ReactiveCommand.Create<string>(SldeAction);
            var viewModel = new KeyBoardViewModel();
            viewModel.TextChanged += KeyBoard_TextChanged;
            CurrentPage = viewModel;
            Bl = BL.GetBL;
            Blf = BLF.GetBLF;
            Update();

        }

        private async Task BackAction()
        {
            CurrentText = "";
            Current = 0;
            CodeFastGroup = 0;
            Update();
           
        }
            private void KeyBoard_TextChanged(object? sender, string text)
        {
            CurrentText = text;
         
            Update();
        }
        
        
        private ReactiveCommand<Unit, Unit> _closeCommand;
        public ReactiveCommand<Unit, Unit> CloseCommand => _closeCommand ??= ReactiveCommand.CreateFromTask(Close);
        private async Task Close()
        {
            VisibilityChanged.Invoke(this, EventArgs.Empty);
        }
        private void UpdatePageProducts()
        {

            if (OnPageProducts != null)
            {
              
                int count = OnPageProducts.Count;
                if(count == 1) 
                {
                    OnPageProductsTop = OnPageProducts.GetRange(0,1);
                }
                else if(count%2==0)
                {
                    OnPageProductsTop = OnPageProducts.GetRange(0,count/2); //4 0 1
                    OnPageProductsBottom = OnPageProducts.GetRange(count/2,count/2); //4 2 3
                }
                else
                {
                    OnPageProductsTop = OnPageProducts.GetRange(0, count / 2+1); //5 0 1 2
                    OnPageProductsBottom = OnPageProducts.GetRange(count / 2+1, count / 2); //5 3 4
                }

              
            }
        }
        public void GetCodeAction(GWA selectedItem)
        {
            if (selectedItem.Type == 1)
            {
                CodeFastGroup = selectedItem.Code;
                Update();
            }
        }   

        public void SldeAction(string symbol)
        {
            if (symbol == "left")
            {

                Current--;
                OnPageProducts = AllProducts.GetRange(Limit *Current , Limit);

                UpdatePageProducts();
                IsRightEnable = true;
                if (Current == 0) IsLeftEnable = false;
            }
            else
            {
                Current++;
                if(Limit *Current+ Limit>AllProducts.Count)
                {
                    OnPageProducts = AllProducts.GetRange(Limit * Current, AllProducts.Count- Limit * Current);
                }  
                else OnPageProducts = AllProducts.GetRange(Limit * Current, Limit);
                UpdatePageProducts();
                IsLeftEnable = true;
                if (Current == MaxPage) IsRightEnable = false;
            }
        }
        public void Update()
        {
            Current = 0;
            OnPageProductsBottom = null;
            OnPageProductsTop = null;
            var Res = Blf.GetDataFindWares(CodeFastGroup, CurrentText, new ModelMID.IdReceipt(), ref OffSet, ref MaxPage, ref Limit);
            AllProducts = Res?.Select(el => new GWA(el)).ToList();
          
            if (AllProducts != null)
            {
                if (AllProducts.Count <= Limit)
                {
                    OnPageProducts = AllProducts;
                    IsRightEnable = false;
                }
                else
                {
                    OnPageProducts = AllProducts.GetRange(Limit * Current, Limit);
                    IsRightEnable = true;

                }
                UpdatePageProducts();
               

                IsLeftEnable = false;
            }
       

            
       
        }
    }
}