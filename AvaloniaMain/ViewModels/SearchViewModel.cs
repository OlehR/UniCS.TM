using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using ModelMID;
using ReactiveUI;
using SharedLib;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Reactive;
using System.Text;

using System.Threading.Tasks;

namespace AvaloniaMain.ViewModels
{
    internal class SearchViewModel : ViewModelBase
    {
        BL Bl;
        private string _path = "C:/Pictures/Categories\\000009001.jpg";
        public Bitmap _imageFromBinding;
        public Bitmap ImageFromBinding
        {
            get => _imageFromBinding;
            set
            {
                if (_imageFromBinding != value)
                {
                    _imageFromBinding = value;
                    OnPropertyChanged(nameof(ImageFromBinding));
                }
            }
        }
        public string path 
        {
            get => _path;
            set
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged(nameof(path));
                }
            }
        }
        int CodeFastGroup = 0;
        int OffSet = 0;
        int MaxPage = 0;
        int Limit = 10;
        int Current = 0;
        private List<GW> _onPageProductsTop;
        public List<GW> OnPageProductsTop
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

        private List<GW> _onPageProductsBottom;
        public List<GW> OnPageProductsBottom
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

        private List<GW> _onPageProducts;
        public List<GW> OnPageProducts
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

        private List<GW> _AllProducts;
        public List<GW> AllProducts
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
            get=> _currentPage;
            set
            {
                if(_currentPage!=value)
                {
                    _currentPage = value;
                    OnPropertyChanged(nameof(CurrentPage));
                }    
            }
        }
      
        private string _currentText="";
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
            try
            {
                ImageFromBinding =
                    ImageHelper.LoadFromFile("C:/Pictures/Categories/000009001.jpg");
            }
            catch (Exception ex) { }
            
            var viewModel = new KeyBoardViewModel();
            viewModel.TextChanged += KeyBoard_TextChanged;
            CurrentPage = viewModel;
            Bl = BL.GetBL;
            var Res=Bl.GetDataFindWares(CodeFastGroup, CurrentText,new ModelMID.IdReceipt(),ref OffSet,ref MaxPage,ref Limit);
            AllProducts = (List<GW>)Res;
            OnPageProducts = AllProducts.GetRange(Limit * Current, Limit);
            UpdatePageProducts();


        }

        private void KeyBoard_TextChanged(object? sender, string text)
        {
            CurrentText = text;
        }

        private ReactiveCommand<Unit, Unit> _closeCommand;
        public ReactiveCommand<Unit, Unit> CloseCommand => _closeCommand ??= ReactiveCommand.CreateFromTask(Close);
        private async Task Close()
        {
            VisibilityChanged.Invoke(this, EventArgs.Empty);
        }
        private void UpdatePageProducts()
        {
            if (OnPageProducts != null )
            {
                OnPageProductsTop = OnPageProducts.GetRange(0, Limit / 2);
                OnPageProductsBottom = OnPageProducts.GetRange(Limit / 2, Limit / 2);
            }
        }
    }
}
