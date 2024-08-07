using Avalonia.Controls;
using ModelMID;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using QRCoder;
namespace AvaloniaMain.ViewModels
{
    public class ShowMessageViewModel  : ViewModelBase
    {
        public ReactiveCommand<string, Unit> YesNoChoise { get; }
        public event EventHandler? VisibilityChanged;
        public ReactiveCommand<Unit, Unit> Close { get; }
        public string TextMessage { get; set; }
        public string TextTypeMessage { get; set; }
        public eTypeMessage TypeMessage { get; set; }
        public Action<bool> Result { get; set; }
        private Bitmap _ImageBit;
        public Bitmap ImageBit
        {
            get => _ImageBit;
            set
            {
                if(value!= _ImageBit)
                {
                    _ImageBit= value;
                    OnPropertyChanged(nameof(ImageBit));

                }
            }
        }



        private void YesNoCommand(string res)
        {
            if(res=="yes")
            {
                Result.Invoke(true);
                VisibilityChanged.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Result.Invoke(false);
                VisibilityChanged.Invoke(this, EventArgs.Empty);
            }
                
        }
        public ShowMessageViewModel(string textMessage, string textTypeMessage= "Увага!", eTypeMessage typeMessage = eTypeMessage.Information)
        {
            TextMessage = textMessage;
            TextTypeMessage= textTypeMessage;
            typeMessage = typeMessage;
            Close = ReactiveCommand.CreateFromTask(CloseTask);
            YesNoChoise = ReactiveCommand.Create<string>(YesNoCommand);
            switch (typeMessage)
            {
                case eTypeMessage.Warning:
                   // OkButton.Visibility = Visibility.Visible;
                    break;
                case eTypeMessage.Error:
                   // OkButton.Visibility = Visibility.Visible;
                    break;
                case eTypeMessage.Information:
                   // OkButton.Visibility = Visibility.Visible;
                    break;
                case eTypeMessage.Question:
               {
           
                    try
                    {
                         ImageBit= new Bitmap("/MID/Work/UniCS.TM/AvaloniaMain/Assets/question.ico");
                    }
                      catch (Exception ex)
                    {
                         Console.WriteLine($"Error loading image from file : {ex.Message}");
                    }
               
               }
        


       // ImageTypeMessage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/question.png"));
                   // YesButton.Visibility = Visibility.Visible;
                    //NoButton.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }

        }


        public async Task CloseTask()
        {
            VisibilityChanged.Invoke(this, EventArgs.Empty);
        }

    }
}
