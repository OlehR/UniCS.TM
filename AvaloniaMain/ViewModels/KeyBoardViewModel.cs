using Avalonia.Animation.Easings;
using Avalonia.Metadata;
using AvaloniaMain.Models;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AvaloniaMain.ViewModels
{
    public class KeyBoardViewModel : ViewModelBase
    {
      List<KeyBoardButton> keys;
        private string _currentString;
        public string CurrentString
        {
            get => _currentString;
            set => this.RaiseAndSetIfChanged(ref _currentString, value);
        }
        private bool _shift;
        public bool Shift
        {
            get => _shift;
            set=>this.RaiseAndSetIfChanged(ref _shift, value);
        }
        private bool _en;
        public bool En
        {
            get => _en;
            set => this.RaiseAndSetIfChanged(ref _en, value);
        }
       
        public ReactiveCommand<string, Unit> LayoutChange { get; }
        private ReactiveCommand<Unit, Unit> _bksp;
        public ReactiveCommand<Unit, Unit> Bksp => _bksp ??= ReactiveCommand.CreateFromTask(BkspCommand);


        public event EventHandler<string>? TextChanged;


        private void LayoutChangeCommand(string LayoutChangeType)
        {
                switch (LayoutChangeType)
            {
                case "en":
                    {
                        En = !En;
                      
                    }
                    break;
                case "shift":
                    {
                        Shift = !Shift;
                    }
                    break;
                default:
                    {
                       
                    }
                    break;
                    
            }
            changeKeyboard();

        }

        private void changeKeyboard()
        {
            if (En == true)
            {
                if (Shift == true)
                {
                    foreach (var key in keys)
                    {
                        key.CurrentValue = key.EngShiftValue;
                    }
                }
                else
                {
                    foreach (var key in keys)
                    {
                        key.CurrentValue = key.EngValue;
                    }
                }
            }
            else
            {
                if (Shift == true)
                {
                    foreach (var key in keys)
                    {
                        key.CurrentValue = key.UkrShiftValue;
                    }
                }
                else
                {
                    foreach (var key in keys)
                    {
                        key.CurrentValue = key.UkrValue;
                    }
                }
            }
        }

        public ReactiveCommand<string, Unit> AddValue { get; }
    
    


        public KeyBoardViewModel()
        {
            init();

            
            AddValue = ReactiveCommand.Create<string>(AddValueCommand);
            LayoutChange = ReactiveCommand.Create<string>(LayoutChangeCommand);


        }

        private async Task BkspCommand()
        {
            if (!string.IsNullOrEmpty(CurrentString))
            {
               CurrentString = CurrentString.Remove(CurrentString.Length - 1);
            }
            TextChanged.Invoke(this, CurrentString);

        }
        private void AddValueCommand(string elemet)
        {
            
            CurrentString += keys[Convert.ToInt32(elemet)].CurrentValue;
            TextChanged.Invoke(this, CurrentString);
        }

        private void init()
        {
             keys = new List<KeyBoardButton>();

             KeyBoardButton1 = new KeyBoardButton("~", "`", "~", "~", "~");
            keys.Add(KeyBoardButton1);

             KeyBoardButton2 = new KeyBoardButton("1", "1", "1", "!", "!");
            keys.Add(KeyBoardButton2);

             KeyBoardButton3 = new KeyBoardButton("2", "2", "2", "@", "\"");
            keys.Add(KeyBoardButton3);

             KeyBoardButton4 = new KeyBoardButton("3", "3", "3", "#", "№");
            keys.Add(KeyBoardButton4);

             KeyBoardButton5 = new KeyBoardButton("4", "4", "4", ";", "$");
            keys.Add(KeyBoardButton5);

           KeyBoardButton6 = new KeyBoardButton("5", "5", "5", "%", "%");
            keys.Add(KeyBoardButton6);

             KeyBoardButton7 = new KeyBoardButton("6", "6", "6", ":", "^");
            keys.Add(KeyBoardButton7);

            KeyBoardButton8 = new KeyBoardButton("7", "7", "7", "?", "&");
            keys.Add(KeyBoardButton8);

            KeyBoardButton9 = new KeyBoardButton("8", "8", "8", "*", "*");
            keys.Add(KeyBoardButton9);

             KeyBoardButton10 = new KeyBoardButton("9", "9", "9", "(", "(");
            keys.Add(KeyBoardButton10);

             KeyBoardButton11 = new KeyBoardButton("0", "0", "0", ")", ")");
            keys.Add(KeyBoardButton11);

             KeyBoardButton12 = new KeyBoardButton("-", "-", "-", "_", "_");
            keys.Add(KeyBoardButton12);

             KeyBoardButton13 = new KeyBoardButton("=", "=", "=", "+", "+");
            keys.Add(KeyBoardButton13);

             KeyBoardButton14 = new KeyBoardButton("й", "q", "й", "Й", "Q");
            keys.Add(KeyBoardButton14);

             KeyBoardButton15 = new KeyBoardButton("ц", "w", "ц", "Ц", "W");
            keys.Add(KeyBoardButton15);

            KeyBoardButton16 = new KeyBoardButton("у", "e", "у", "У", "E");
            keys.Add(KeyBoardButton16);

             KeyBoardButton17 = new KeyBoardButton("к", "r", "к", "К", "R");
            keys.Add(KeyBoardButton17);

             KeyBoardButton18 = new KeyBoardButton("е", "t", "е", "Е", "T");
            keys.Add(KeyBoardButton18);

            KeyBoardButton19 = new KeyBoardButton("н", "y", "н", "Н", "Y");
            keys.Add(KeyBoardButton19);

            KeyBoardButton20 = new KeyBoardButton("г", "u", "г", "Г", "U");
            keys.Add(KeyBoardButton20);

             KeyBoardButton21 = new KeyBoardButton("ш", "i", "ш", "Ш", "I");
            keys.Add(KeyBoardButton21);

            KeyBoardButton22 = new KeyBoardButton("щ", "o", "щ", "Щ", "O");
            keys.Add(KeyBoardButton22);

             KeyBoardButton23 = new KeyBoardButton("з", "p", "з", "З", "P");
            keys.Add(KeyBoardButton23);

            KeyBoardButton24 = new KeyBoardButton("х", "[", "х", "Х", "{");
            keys.Add(KeyBoardButton24);

             KeyBoardButton25 = new KeyBoardButton("ї", "]", "ї", "Ї", "}");
            keys.Add(KeyBoardButton25);

             KeyBoardButton26 = new KeyBoardButton("г", "\\\\", "г", "Г", "|");
            keys.Add(KeyBoardButton26);

             KeyBoardButton27 = new KeyBoardButton("ф", "a", "ф", "Ф", "A");
            keys.Add(KeyBoardButton27);

            KeyBoardButton28 = new KeyBoardButton("і", "s", "і", "І", "S");
            keys.Add(KeyBoardButton28);

             KeyBoardButton29 = new KeyBoardButton("в", "d", "в", "В", "D");
            keys.Add(KeyBoardButton29);

             KeyBoardButton30 = new KeyBoardButton("а", "f", "а", "А", "F");
            keys.Add(KeyBoardButton30);

             KeyBoardButton31 = new KeyBoardButton("п", "g", "п", "П", "G");
            keys.Add(KeyBoardButton31);

          KeyBoardButton32 = new KeyBoardButton("р", "h", "р", "Р", "H");
            keys.Add(KeyBoardButton32);

             KeyBoardButton33 = new KeyBoardButton("о", "j", "о", "О", "J");
            keys.Add(KeyBoardButton33);

             KeyBoardButton34 = new KeyBoardButton("л", "k", "л", "Л", "K");
            keys.Add(KeyBoardButton34);

             KeyBoardButton35 = new KeyBoardButton("д", "l", "д", "Д", "L");
            keys.Add(KeyBoardButton35);

            KeyBoardButton36 = new KeyBoardButton("ж", ";", "ж", "Ж", ":");
            keys.Add(KeyBoardButton36);

            KeyBoardButton37 = new KeyBoardButton("є", "\"", "є", "Є", "`");
            keys.Add(KeyBoardButton37);

            KeyBoardButton38 = new KeyBoardButton("я", "z", "я", "Я", "Z");
            keys.Add(KeyBoardButton38);

             KeyBoardButton39 = new KeyBoardButton("ч", "x", "ч", "Ч", "X");
            keys.Add(KeyBoardButton39);

            KeyBoardButton40 = new KeyBoardButton("с", "c", "с", "С", "C");
            keys.Add(KeyBoardButton40);

            KeyBoardButton41 = new KeyBoardButton("м", "v", "м", "М", "V");
            keys.Add(KeyBoardButton41);

             KeyBoardButton42 = new KeyBoardButton("и", "b", "и", "И", "B");
            keys.Add(KeyBoardButton42);

            KeyBoardButton43 = new KeyBoardButton("т", "n", "т", "Т", "N");
            keys.Add(KeyBoardButton43);

             KeyBoardButton44 = new KeyBoardButton("ь", "m", "ь", "Ь", "M");
            keys.Add(KeyBoardButton44);

           KeyBoardButton45 = new KeyBoardButton("б", ",", "б", "Б", "<");
            keys.Add(KeyBoardButton45);

             KeyBoardButton46 = new KeyBoardButton("ю", ".", "ю", "Ю", ">");
            keys.Add(KeyBoardButton46);

             KeyBoardButton47 = new KeyBoardButton(".", "/", ".", ",", "?");
            keys.Add(KeyBoardButton47);
           
            

        }
        public KeyBoardButton KeyBoardButton1 { get; set; }
        public KeyBoardButton KeyBoardButton2 { get; set; }
        public KeyBoardButton KeyBoardButton3 { get; set; }
        public KeyBoardButton KeyBoardButton4 { get; set; }
        public KeyBoardButton KeyBoardButton5 { get; set; }
        public KeyBoardButton KeyBoardButton6 { get; set; }
        public KeyBoardButton KeyBoardButton7 { get; set; }
        public KeyBoardButton KeyBoardButton8 { get; set; }
        public KeyBoardButton KeyBoardButton9 { get; set; }
        public KeyBoardButton KeyBoardButton10 { get; set; }
        public KeyBoardButton KeyBoardButton11 { get; set; }
        public KeyBoardButton KeyBoardButton12 { get; set; }
        public KeyBoardButton KeyBoardButton13 { get; set; }
        public KeyBoardButton KeyBoardButton14 { get; set; }
        public KeyBoardButton KeyBoardButton15 { get; set; }
        public KeyBoardButton KeyBoardButton16 { get; set; }
        public KeyBoardButton KeyBoardButton17 { get; set; }
        public KeyBoardButton KeyBoardButton18 { get; set; }
        public KeyBoardButton KeyBoardButton19 { get; set; }
        public KeyBoardButton KeyBoardButton20 { get; set; }
        public KeyBoardButton KeyBoardButton21 { get; set; }
        public KeyBoardButton KeyBoardButton22 { get; set; }
        public KeyBoardButton KeyBoardButton23 { get; set; }
        public KeyBoardButton KeyBoardButton24 { get; set; }
        public KeyBoardButton KeyBoardButton25 { get; set; }
        public KeyBoardButton KeyBoardButton26 { get; set; }
        public KeyBoardButton KeyBoardButton27 { get; set; }
        public KeyBoardButton KeyBoardButton28 { get; set; }
        public KeyBoardButton KeyBoardButton29 { get; set; }
        public KeyBoardButton KeyBoardButton30 { get; set; }
        public KeyBoardButton KeyBoardButton31 { get; set; }
        public KeyBoardButton KeyBoardButton32 { get; set; }
        public KeyBoardButton KeyBoardButton33 { get; set; }
        public KeyBoardButton KeyBoardButton34 { get; set; }
        public KeyBoardButton KeyBoardButton35 { get; set; }
        public KeyBoardButton KeyBoardButton36 { get; set; }
        public KeyBoardButton KeyBoardButton37 { get; set; }
        public KeyBoardButton KeyBoardButton38 { get; set; }
        public KeyBoardButton KeyBoardButton39 { get; set; }
        public KeyBoardButton KeyBoardButton40 { get; set; }
        public KeyBoardButton KeyBoardButton41 { get; set; }
        public KeyBoardButton KeyBoardButton42 { get; set; }
        public KeyBoardButton KeyBoardButton43 { get; set; }
        public KeyBoardButton KeyBoardButton44 { get; set; }
        public KeyBoardButton KeyBoardButton45 { get; set; }
        public KeyBoardButton KeyBoardButton46 { get; set; }
        public KeyBoardButton KeyBoardButton47 { get; set; }
    }
}
