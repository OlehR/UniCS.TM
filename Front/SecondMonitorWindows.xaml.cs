using Front.Equipments;
using ModelMID;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static System.Windows.Forms.AxHost;

namespace Front
{
    /// <summary>
    /// Interaction logic for SecondMonitorWindows.xaml
    /// </summary>
    public partial class SecondMonitorWindows : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public IMW MW { get; set; }
        public ObservableCollection<ReceiptWares> ListWares { get; set; }
        private DispatcherTimer timer;
        private DateTime lastUpdateTime;
        public SecondMonitorWindows(IMW pMW)
        {
            InitializeComponent();

            MW = pMW;
            ListWares = MW.ListWares;
            WaresList.ItemsSource = ListWares;

            //Показ відео при старті
            if (MW.PathVideo != null && MW.PathVideo.Length != 0)
            {
                StartVideo.Source = new Uri(MW.PathVideo[0]);
                StartVideo.Play();
                StartVideo.MediaEnded += (object sender, RoutedEventArgs e) =>
                {
                    StartVideo.Position = new TimeSpan(0, 0, 0, 0, 1);
                    StartVideo.Play();
                };

                SecondVideo.Source = new Uri(MW.PathVideo[0]);
                SecondVideo.Stop();
                SecondVideo.MediaEnded += (object sender, RoutedEventArgs e) =>
                {
                    StartVideo.Position = new TimeSpan(0, 0, 0, 0, 1);
                    StartVideo.Play();
                };
            }
            // Ініціалізуємо таймер
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5); // Встановлюємо інтервал таймера на 5 хвилин
            timer.Tick += Timer_Tick;
            timer.Start();

            Global.OnReceiptCalculationComplete += (pReceipt) =>
            {
                try
                {
                    if (pReceipt == null)
                    {
                        Dispatcher.BeginInvoke(new ThreadStart(() => { ListWares?.Clear(); }));
                    }
                    else
                    {
                        Dispatcher.BeginInvoke(new ThreadStart(() =>
                        {
                            if (pReceipt.Wares?.Any() == true)
                                ListWares = new ObservableCollection<ReceiptWares>(pReceipt.Wares);
                            else
                                ListWares?.Clear();

                            WaresList.ItemsSource = ListWares;
                            if (WaresList.Items.Count > 0)
                                WaresList.SelectedIndex = WaresList.Items.Count - 1;
                            ScrolDown();
                        }));
                    }
                }
                catch (Exception e) { }
            };
         }

        public void ScrolDown()
        {
            if (VisualTreeHelper.GetChildrenCount(WaresList) > 0)
            {
                Border border = (Border)VisualTreeHelper.GetChild(WaresList, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }
        public void UpdateSecondMonitor()
        {
            if (MW.State == eStateMainWindows.StartWindow)
            {

                StartVideo.Play();
                SecondVideo.Pause();
                StartShopping.Visibility = Visibility.Visible;
                WaresList.Visibility = Visibility.Collapsed;
            }
            else
            {
                StartVideo.Pause();
                SecondVideo.Play();
                StartShopping.Visibility = Visibility.Collapsed;
                WaresList.Visibility = Visibility.Visible;
            }
            /*
            ListWares = MW.ListWares;
            WaresList.ItemsSource = ListWares;
            OnPropertyChanged(nameof(ListWares));
            if (VisualTreeHelper.GetChildrenCount(WaresList) > 0)
            {
                Border border = (Border)VisualTreeHelper.GetChild(WaresList, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
            if (WaresList.Items.Count > 0)
                WaresList.SelectedIndex = WaresList.Items.Count - 1;*/
            lastUpdateTime = DateTime.Now;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Перевіряємо різницю в часі між останнім оновленням і поточним часом
            TimeSpan elapsedTime = DateTime.Now - lastUpdateTime;
            // Якщо касир не тикав нічого 5 хв тоді перевести в режим КСО
            if (elapsedTime.TotalSeconds >= 5) //TotalMinutes >= 5
            {
                // Якщо пройшло більше 5 хвилин з останнього оновлення, відображаємо кнопку Start
                StartBuyButton.Visibility = Visibility.Visible;
            }
            else
            {
                // Інакше, продовжуємо відлік
                StartBuyButton.Visibility = Visibility.Collapsed;
                timer.Start();
            }
        }
        private void OnPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private void StartBuy(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Придумати що тоді робити");
        }
    }
}
