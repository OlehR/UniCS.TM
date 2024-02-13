using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaMain.Models
{
    public   class KeyBoardButton :ReactiveObject
    {
        private string _CurrentValue;
        public string CurrentValue
        {
            get => _CurrentValue;
            set => this.RaiseAndSetIfChanged(ref _CurrentValue, value);
        }
        private string _UkrValue;

        public string UkrValue
       
        {
            get => _UkrValue;
            set => this.RaiseAndSetIfChanged(ref _UkrValue, value);
        }
        private string _UkrShiftValue;
        public string UkrShiftValue
        {
            get => _UkrShiftValue;
            set => this.RaiseAndSetIfChanged(ref _UkrShiftValue, value);
        }
        private string _EngValue;

        public string EngValue
        {
            get => _EngValue;
            set => this.RaiseAndSetIfChanged(ref _EngValue, value);
        }
        private string _EngShiftValue;

        public KeyBoardButton(string currentValue, string engValue, string ukrValue, string ukrShiftValue, string engShiftValue )
        {
            _CurrentValue = currentValue;
            _UkrValue = ukrValue;
            _UkrShiftValue = ukrShiftValue;
            _EngShiftValue = engShiftValue;
            _EngValue = engValue;

        }

        public string EngShiftValue
      
        {
            get => _EngShiftValue;
            set => this.RaiseAndSetIfChanged(ref _EngShiftValue, value);
        }
    }
}
