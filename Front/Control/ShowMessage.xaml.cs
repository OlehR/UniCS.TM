using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace Front.Control
{

    public partial class ShowMessage : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string TextMessage { get; set; }
        public string TextTypeMessage { get; set; }
        public eTypeMessage TypeMessage { get; set; }
        public Action<bool> Result { get; set; }
        MainWindow MW;
        public void Init(MainWindow pMW) { MW = pMW; }

        public ShowMessage()
        {
            InitializeComponent();
        }
        
        public void Show(string textMessage, string textTypeMessage = "Увага!", eTypeMessage typeMessage = eTypeMessage.Information)
        {
            ShowWindow();
            TextMessage = textMessage;
            TextTypeMessage = textTypeMessage;
            TypeMessage = typeMessage;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextMessage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextTypeMessage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TypeMessage)));
            YesButton.Visibility= Visibility.Collapsed;
            NoButton.Visibility= Visibility.Collapsed;
            OkButton.Visibility= Visibility.Collapsed;
            switch (TypeMessage)
            {
                case eTypeMessage.Warning:
                    ImageTypeMessage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/warning.png"));
                    OkButton.Visibility = Visibility.Visible;
                    break;
                case eTypeMessage.Error:
                    ImageTypeMessage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/Error.png"));
                    OkButton.Visibility = Visibility.Visible;
                    break;
                case eTypeMessage.Information:
                    ImageTypeMessage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/information.png"));
                    OkButton.Visibility = Visibility.Visible;
                    break;
                case eTypeMessage.Question:
                    ImageTypeMessage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/question.png"));
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }

        }

        private void YesOrNoButtonClik(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            switch (button.Name)
            {
                case "YesButton":
                    Result?.Invoke(true);
                    break;
                case "NoButton":
                    Result?.Invoke(false);
                    break;
            }
            ShowWindow(false);

        }
        void ShowWindow(bool show = true)
        {
            if (show)
            {
                MW.CustomMessage.Visibility = Visibility.Visible;
                MW.TotalBackground.Visibility = Visibility.Visible;
            }
            else
            {
                MW.CustomMessage.Visibility = Visibility.Collapsed;
                MW.TotalBackground.Visibility = Visibility.Collapsed;
            }
            
        }
        private void OkButtonClik(object sender, RoutedEventArgs e)
        {
            ShowWindow(false);
        }
    }
    public enum eTypeMessage
    {
        Error,
        Warning,
        Information,
        Question
    }
}
