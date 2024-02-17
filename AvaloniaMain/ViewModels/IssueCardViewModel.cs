using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaMain.ViewModels
{
    public class IssueCardViewModel:ViewModelBase
    {
        public event EventHandler? VisibilityChanged;
        private string _barCode;
        public string BarCode
        {
            get => _barCode;
            set=> this.RaiseAndSetIfChanged(ref _barCode, value);
        }
        private string _telephoneNum;
        public string TelephoneNum
        {
            get => _telephoneNum;
            set => this.RaiseAndSetIfChanged(ref _telephoneNum, value);
        }
        private string _verificationCode;
        public string VerificationCode
        {
            get => _verificationCode;
            set => this.RaiseAndSetIfChanged(ref _verificationCode, value);
        }
        public ViewModelBase? _currentPage;

        public ViewModelBase? CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }

        private ReactiveCommand<Unit, Unit> _showBarCode;
        public ReactiveCommand<Unit, Unit> ShowBarCode => _showBarCode;
        private ReactiveCommand<Unit, Unit> _closeCommand;
        public ReactiveCommand<Unit, Unit> CloseCommand => _closeCommand ??= ReactiveCommand.CreateFromTask(Close);
        private ReactiveCommand<Unit, Unit> _showTelNumber;
        public ReactiveCommand<Unit, Unit> ShowTelNumber => _showTelNumber;
        private ReactiveCommand<Unit, Unit> _showVerificationCode;
        public ReactiveCommand<Unit, Unit> ShowVerificationCode => _showVerificationCode;

        //ShowBarCode  ShowTelNumber ShowVerificationCode
        public IssueCardViewModel()
        {
            _showBarCode = ReactiveCommand.CreateFromTask(ShowBarCommand);
            _showTelNumber = ReactiveCommand.CreateFromTask(ShowNumberCommand);
            _showVerificationCode = ReactiveCommand.CreateFromTask(ShowVerificationCommand);
        }

        private async Task ShowBarCommand()
        {
            string mask;
            var parentViewModel = new NumPadViewModel(BarCode,false);
            parentViewModel.NumberChanged += ParentViewModel_BarCodeChanged;
            parentViewModel.VisibilityChanged += ParentViewModel_VisibilityChanged;
            CurrentPage = parentViewModel;
        }
        private async Task ShowNumberCommand()
        {
            string mask ="^[0-9]{10}$";
            var parentViewModel = new NumPadViewModel(TelephoneNum,false,mask);
            parentViewModel.NumberChanged += ParentViewModel_TelephoneChanged;
            parentViewModel.VisibilityChanged += ParentViewModel_VisibilityChanged;

            CurrentPage = parentViewModel;

        }
        private async Task ShowVerificationCommand()
        {
            var parentViewModel = new NumPadViewModel(VerificationCode, false);
            parentViewModel.NumberChanged += ParentViewModel_VerificationChanged;
            parentViewModel.VisibilityChanged += ParentViewModel_VisibilityChanged;
            CurrentPage = parentViewModel;

        }
        private void ParentViewModel_VisibilityChanged(object? sender, EventArgs? e)
        {
        }
        private void ParentViewModel_BarCodeChanged(object? sender, string newNumber)
        {
            BarCode = newNumber;
        }
        private void ParentViewModel_TelephoneChanged(object? sender, string newNumber)
        {
            TelephoneNum = newNumber;
        }
        private void ParentViewModel_VerificationChanged(object? sender, string newNumber)
        {
            VerificationCode = newNumber;
        }
        private async Task Close()
        {

            VisibilityChanged.Invoke(this, EventArgs.Empty);
        }
    }
}
