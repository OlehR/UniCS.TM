using ReactiveUI;
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
        public event EventHandler? VisibilityChanged;
        private ViewModelBase _currentPage;
        public ViewModelBase CurrentPage
        {
            get=> _currentPage;
            set=> this.RaiseAndSetIfChanged(ref _currentPage, value);
        }
        private string _currentText;
        public string CurrentText
        {
            get => _currentText;
            set => this.RaiseAndSetIfChanged(ref _currentText, value);
        }
     
        public SearchViewModel()
        {
            var viewModel = new KeyBoardViewModel();
            viewModel.TextChanged += KeyBoard_TextChanged;
            CurrentPage = viewModel;
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
