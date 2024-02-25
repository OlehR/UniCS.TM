using Avalonia.Controls.Primitives;
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
        int CodeFastGroup = 0;
        int OffSet = 0;
        int MaxPage = 0;
        int Limit = 12;

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
            var viewModel = new KeyBoardViewModel();
            viewModel.TextChanged += KeyBoard_TextChanged;
            CurrentPage = viewModel;
            Bl = BL.GetBL;
            var Res=Bl.GetDataFindWares(CodeFastGroup, CurrentText,new ModelMID.IdReceipt(),ref OffSet,ref MaxPage,ref Limit);
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
    }
}
