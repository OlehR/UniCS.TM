using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvaloniaMain.ViewModels
{
    public class NumPadViewModel : ViewModelBase

    {

        public event EventHandler<string>? NumberChanged;
        public event EventHandler? VisibilityChanged;
        private bool _enterEnable = false;
        public bool EnterEnable
        {
            get => _enterEnable;
            set
            {
                if (_enterEnable != value)
                {
                    _enterEnable = value;
                    OnPropertyChanged(nameof(EnterEnable));
                }
            }
        }

        private string _validationMask;
        public string ValidationMask
        {
            get => _validationMask;
            set
            {
                if (_validationMask != value)
                {
                    _validationMask = value;
                    OnPropertyChanged(nameof(ValidationMask));
                }
            }
        }

        private bool _comaEnable;
        public bool ComaEnable
        {
            get => _comaEnable;
            set
            {
                if (_comaEnable != value)
                {
                    _comaEnable = value;
                    OnPropertyChanged(nameof(ComaEnable));
                }
            }
        }

        private bool _backGroundVisibility;
        public bool BackGroundVisibility
        {
            get => _backGroundVisibility;
            set
            {
                if (_backGroundVisibility != value)
                {
                    _backGroundVisibility = value;
                    OnPropertyChanged(nameof(BackGroundVisibility));
                }
            }
        }

        private string _number = "";
        public string Number
        {
            get => _number;
            set
            {
                if (_number != value)
                {
                    _number = value;
                    OnPropertyChanged(nameof(Number));
                }
            }
        }
        public ReactiveCommand<string, Unit> ExampleCommand { get; }
        private ReactiveCommand<Unit, Unit> _deleteCommand;
        private ReactiveCommand<Unit, Unit> _saveCommand;
        private ReactiveCommand<Unit, Unit> _closeCommand;
        public ReactiveCommand<Unit, Unit> SaveCommand => _saveCommand ??= ReactiveCommand.CreateFromTask(Save);
        public ReactiveCommand<Unit, Unit> DeleteCommand => _deleteCommand ??= ReactiveCommand.CreateFromTask(DeleteLast);
        public ReactiveCommand<Unit, Unit> CloseCommand => _closeCommand ??= ReactiveCommand.CreateFromTask(Close);

        public NumPadViewModel(string number, bool Coma)
            
        {
            
            ComaEnable = Coma;
            Number = number;
            EnterEnable = true;
            ExampleCommand = ReactiveCommand.Create<string>(PerformAction);

        }
        public NumPadViewModel(string number, bool Coma, string validationMask)

        {
            ValidationMask = validationMask;
            ComaEnable = Coma;
            Number = number;
            ExampleCommand = ReactiveCommand.Create<string>(PerformAction);

        }

        public NumPadViewModel()
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
