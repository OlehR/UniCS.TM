using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvaloniaMain.ViewModels
{
    public class ParentViewModel : ViewModelBase
    {
        private bool _enterEnable=false;
        public bool EnterEnable
        {
            get => _enterEnable;
            set => this.RaiseAndSetIfChanged(ref _enterEnable, value);
        }
        private string ValidationMask;
        private bool _comaEnable;
        public bool ComaEnable
        {
            get => _comaEnable;
            set => this.RaiseAndSetIfChanged(ref _comaEnable, value);
        }
        private bool _backGroundVisibility;
        public bool BackGroundVisibility
        {
            get => _backGroundVisibility;
            set => this.RaiseAndSetIfChanged(ref _backGroundVisibility, value);
        }
       
        public event EventHandler<string>? NumberChanged;
        public event EventHandler? VisibilityChanged;
        private string _number = "";
        public string Number
        {
            get => _number;
            set => this.RaiseAndSetIfChanged(ref _number, value);
        }
        public ReactiveCommand<string, Unit> ExampleCommand { get; }
        private ReactiveCommand<Unit, Unit> _deleteCommand;
        private ReactiveCommand<Unit, Unit> _saveCommand;
        private ReactiveCommand<Unit, Unit> _closeCommand;
        public ReactiveCommand<Unit, Unit> SaveCommand => _saveCommand ??= ReactiveCommand.CreateFromTask(Save);
        public ReactiveCommand<Unit, Unit> DeleteCommand => _deleteCommand ??= ReactiveCommand.CreateFromTask(DeleteLast);
        public ReactiveCommand<Unit, Unit> CloseCommand => _closeCommand ??= ReactiveCommand.CreateFromTask(Close);

        public ParentViewModel(string number, bool Coma)
            
        {
            
            ComaEnable = Coma;
            Number = number;
            EnterEnable = true;
            ExampleCommand = ReactiveCommand.Create<string>(PerformAction);

        }
        public ParentViewModel(string number, bool Coma, string validationMask)

        {
            ValidationMask = validationMask;
            ComaEnable = Coma;
            Number = number;
            ExampleCommand = ReactiveCommand.Create<string>(PerformAction);

        }

        public ParentViewModel()
        {
         
            ExampleCommand = ReactiveCommand.Create<string>(PerformAction);
            EnterEnable = true;
            ComaEnable=true;
        }

    

        private async Task DeleteLast()
        {
            if (!string.IsNullOrEmpty(Number))
            {
                Number = Number.Remove(Number.Length - 1);
            }
        }
        private async Task Save()
        {
            NumberChanged?.Invoke(this, Number);
            Close();
        }
        private async Task Close()
        {

            VisibilityChanged.Invoke(this, EventArgs.Empty);
        }


        private void PerformAction(string symbol)
        {
            Number += symbol;
            EnableEnter();
        }
        private void EnableEnter()
        {
            bool res = true;
            if (ValidationMask != null)
            {
                Regex regex = new Regex(ValidationMask);
                res = regex.IsMatch(Number);
            }
            EnterEnable = res;
        }  
        
    }
}
