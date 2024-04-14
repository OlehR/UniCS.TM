using ModelMID;
using ReactiveUI;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AvaloniaMain.ViewModels
{
    public class CustomWindowViewModel : ViewModelBase
    {
        public event EventHandler? VisibilityChanged;
        public event Action<object, CustomButton, string> SelectClientEvent;
        public ReactiveCommand<CustomButton, Unit> Select { get; }
        public ReactiveCommand<Unit, Unit> Close { get; }


        public void SelectClient(CustomButton cb)
        {
            SelectClientEvent?.Invoke(this, cb, Text);
            VisibilityChanged.Invoke(this, EventArgs.Empty);
        }
        public CustomWindowViewModel()
        {
           
        }
        private CustomWindow CW;
        public CustomWindowViewModel(CustomWindow cW)
        {
            Select = ReactiveCommand.Create<CustomButton>(SelectClient);
            Close= ReactiveCommand.CreateFromTask(CloseTask);
            buttons = new ObservableCollection<CustomButton>();
            CW = cW;
            Text = cW.Text;
            Caption=cW.Caption;
            buttons = CW.Buttons;
        }
        private ViewModelBase _CurrentPage;
        public ViewModelBase CurrentPage
        {
            get => _CurrentPage;
            set
            {
                if (_CurrentPage != value)
                {
                    _CurrentPage = value;
                    OnPropertyChanged(nameof(CurrentPage));
                }
            }
        }
        private string _Caption;
        public string Caption
        {
            get => _Caption;
            set
            {
                if(_Caption != value)
                {
                    _Caption = value;
                    OnPropertyChanged(nameof(Caption));
                }
            }
        }
        private string _Text;
        public string Text
        {
            get => _Text;
            set
            {
                if (_Text != value)
                {
                    _Text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }
        public async Task CloseTask()
        {
            VisibilityChanged.Invoke(this, EventArgs.Empty);
        }
        private ObservableCollection<CustomButton> _buttons;
        public ObservableCollection<CustomButton> buttons
        {
            get => _buttons;
            set
            {
                if (_buttons != value)
                {
                    _buttons = value;
                    OnPropertyChanged(nameof(buttons));
                }
            }
        }
      
    }
}
