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
using SharedLib;
using static Front.MainWindow;
using LibVLCSharp.Shared;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;

namespace Front
{
    /// <summary>
    /// Interaction logic for SecondMonitorWindows.xaml
    /// </summary>
    public partial class SecondMonitorWindows : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public MainWindow MW { get; set; }
        public ObservableCollection<ReceiptWares> ListWares { get; set; }
        private DispatcherTimer timer;
        private DateTime lastUpdateTime;
        System.Windows.Forms.Screen mainScreen = System.Windows.Forms.Screen.AllScreens[0];
        System.Windows.Forms.Screen additionalScreen = System.Windows.Forms.Screen.AllScreens.Count() > 1 ? System.Windows.Forms.Screen.AllScreens[1] : null;
        public bool IsKSO { get; set; } = false;
        public bool IsShowStartWindows { get => MW.State == eStateMainWindows.StartWindow && !IsKSO; }
        public int WidthScreen { get { return (int)SystemParameters.PrimaryScreenWidth; } }
        public WidthHeaderReceipt widthHeaderReceiptSM { get; set; }
        bool IsLoading = false;

        private DispatcherTimer timerSlideshow;
        private List<string> imageFiles;
        private int currentIndex = 0;

        public eTypeMonitor TypeMonitor
        {
            get
            {
                if (SystemParameters.PrimaryScreenWidth > SystemParameters.PrimaryScreenHeight && SystemParameters.PrimaryScreenWidth <= 1024)
                    return eTypeMonitor.HorisontalMonitorRegular;
                else if (SystemParameters.PrimaryScreenWidth > SystemParameters.PrimaryScreenHeight && SystemParameters.PrimaryScreenWidth == 1920)
                    return eTypeMonitor.HorisontalMonitorKSO;
                else if (SystemParameters.PrimaryScreenWidth < SystemParameters.PrimaryScreenHeight)
                    return eTypeMonitor.VerticalMonitorKSO;
                else
                    return eTypeMonitor.AnotherTypeMonitor;
                //return eTypeMonitor.HorisontalMonitorRegular;
            }
        }
        public void calculateWidthHeaderReceipt(eTypeMonitor TypeMonitor)
        {
            var coefWidth = WidthScreen / 241;
            switch (TypeMonitor)
            {
                //case eTypeMonitor.HorisontalMonitorKSO:
                //    widthHeaderReceiptSM = new WidthHeaderReceipt(1100, 100, 290, 130, 130, 140);
                //    break;
                //case eTypeMonitor.VerticalMonitorKSO:
                //    widthHeaderReceiptSM = new WidthHeaderReceipt(400, 90, 230, 120, 100, 150);
                //    break;
                //int widthName, int widthWastebasket, int widthCountWares, int widthWeight, int widthPrice, int widthTotalPrise
                case eTypeMonitor.HorisontalMonitorRegular:
                    widthHeaderReceiptSM = new WidthHeaderReceipt(140, 0, 65, 0, 60, 90);//new WidthHeaderReceipt(260, 0, 95, 0, 75, 90);
                    break;
                default:
                    widthHeaderReceiptSM = new WidthHeaderReceipt(54 * coefWidth, 5 * coefWidth, 15 * coefWidth, 8 * coefWidth, 10 * coefWidth, 12 * coefWidth);
                    break;
            }
        }
        public SecondMonitorWindows(MainWindow pMW)
        {
            InitializeComponent();

            MW = pMW;
            ListWares = MW.ListWares;
            WaresList.ItemsSource = ListWares;
            //слайдшоу реклами
            LoadImagesFromFolder();
            // Ініціалізуємо таймер
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5); // Встановлюємо інтервал таймера на 5 секунд
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
            UpdateTypeWorkplace();
            calculateWidthHeaderReceipt(TypeMonitor);
            Loaded += SecondMonitor_Loaded;
            // Loaded += (a, b) => { IsLoading = true; StarVideo(); };

        }


        private void LoadImagesFromFolder()
        {
            string folderPath = Path.Combine(Global.PathPictures, "Advertising");
            if (CheckIfImagesExist(folderPath))
            {
                imageFiles = new List<string>();
                imageFiles.AddRange(Directory.GetFiles(folderPath, "*.jpg"));
                imageFiles.AddRange(Directory.GetFiles(folderPath, "*.png"));
            }
            else
            {
                folderPath = Path.Combine(Global.PathPictures, "Logo");
                if (CheckIfImagesExist(folderPath))
                    imageFiles = new List<string>(Directory.GetFiles(folderPath, "*.png"));
                //MessageBox.Show("Folder does not exist.");
            }
        }
        private bool CheckIfImagesExist(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                return false;
            }

            var jpgFiles = Directory.GetFiles(folderPath, "*.jpg");
            var pngFiles = Directory.GetFiles(folderPath, "*.png");

            return jpgFiles.Any() || pngFiles.Any();
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
        public LibVLC _LibVLC = null;
        public Media _Media = null;
        public LibVLCSharp.Shared.MediaPlayer _MediaPlayer;
        private void SecondMonitor_Loaded(object sender, RoutedEventArgs e)
        {
            Core.Initialize();
            _LibVLC = new LibVLC();
            _MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_LibVLC);
            //StartVideo.MediaPlayer = _MediaPlayer;

            MW.LoadVideos();
            MW.PlayNextVideo(_MediaPlayer, true);

            _MediaPlayer.EndReached += MediaPlayer_EndReached;
        }
        public void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MW._currentVideoIndex = (MW._currentVideoIndex + 1) % MW._videoFiles.Length;
                MW.PlayNextVideo(_MediaPlayer);
            });
        }
        public void UpdateSecondMonitor()
        {
            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                if (StartVideo != null && (StartVideo.MediaPlayer == null || StartVideo.MediaPlayer.Media == null) && MW.State == eStateMainWindows.StartWindow)
                {
                    StartVideo.MediaPlayer = _MediaPlayer;
                    StartVideo.MediaPlayer.Play(MW.Media);
                    //SecondVideo.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(MW.LibVLC);
                    //SecondVideo.MediaPlayer.Play(MW.Media);
                    CurrentImage.Visibility = Visibility.Visible;
                    NextImage.Visibility = Visibility.Visible;
                }

                OnPropertyChanged(nameof(IsShowStartWindows));
                lastUpdateTime = DateTime.Now;
                if (StartVideo.MediaPlayer == null) return;

                if (MW.State == eStateMainWindows.StartWindow)
                {
                    StartVideo.MediaPlayer.SetPause(false);
                    StartVideo.Visibility = Visibility.Visible;
                    StartShoppingButtons.Visibility = Visibility.Visible;

                    //SecondVideo.MediaPlayer.SetPause(true);
                    //SecondVideo.Visibility = Visibility.Collapsed;
                    CurrentImage.Visibility = Visibility.Collapsed;
                    NextImage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    StartVideo.Visibility = Visibility.Collapsed;
                    StartVideo.MediaPlayer.SetPause(true);
                    StartShoppingButtons.Visibility = Visibility.Collapsed;

                    if (!IsKSO)
                    {
                        //SecondVideo.MediaPlayer.SetPause(false);
                        //SecondVideo.Visibility = Visibility.Visible;
                        CurrentImage.Visibility = Visibility.Visible;
                        NextImage.Visibility = Visibility.Visible;
                    }
                }
            }));

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Перевіряємо різницю в часі між останнім оновленням і поточним часом
            TimeSpan elapsedTime = DateTime.Now - lastUpdateTime;
            // Якщо касир не тикав нічого 30 сек тоді перевести в режим КСО
            if (elapsedTime.TotalSeconds >= 30 && !IsKSO && (MW?.State == eStateMainWindows.StartWindow || (MW?.State == eStateMainWindows.WaitInput && MW.curReceipt?.SumReceipt == 0 && MW.curReceipt?.StateReceipt <= eStateReceipt.Prepare))) //TotalMinutes >= 5
            {
                // Якщо пройшло більше 30 секунд з останнього оновлення, відображаємо кнопку Start
                StartShoppingButtons.Visibility = Visibility.Visible;
                // Повернення на початковий екран якщо чек пустий
                if (MW?.State == eStateMainWindows.WaitInput && MW.curReceipt?.SumReceipt == 0 && MW.curReceipt?.StateReceipt <= eStateReceipt.Prepare)
                    MW.CancelReceipt();
            }
            else
            {
                // Інакше, продовжуємо відлік
                StartShoppingButtons.Visibility = Visibility.Collapsed;
                timer.Start();
            }

            if (imageFiles != null && imageFiles.Count > 0 && !IsKSO)
            {
                currentIndex = (currentIndex + 1) % imageFiles.Count;
                string nextImage = imageFiles[currentIndex];

                BitmapImage bitmap = new BitmapImage(new Uri(nextImage));
                NextImage.Source = bitmap;

                // Анімація для плавної зміни зображення
                DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(1));
                DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1));

                fadeOut.Completed += (s, a) =>
                {
                    CurrentImage.Source = NextImage.Source;
                    CurrentImage.BeginAnimation(OpacityProperty, fadeIn);
                };

                CurrentImage.BeginAnimation(OpacityProperty, fadeOut);
                NextImage.BeginAnimation(OpacityProperty, fadeIn);
            }
        }
        private void OnPropertyChanged(String info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private void StartBuy(object sender, RoutedEventArgs e)
        {
            SwapScrean(true);
        }

        private void UpdateTypeWorkplace(bool isKSO = false)
        {
            IsKSO = isKSO;

            Global.TypeWorkplaceCurrent = IsKSO ? eTypeWorkplace.SelfServicCheckout : eTypeWorkplace.CashRegister;
            MW.SetWorkPlace();
            Access.GetAccess()?.Init(MW.AdminSSC);

            OnPropertyChanged(nameof(IsKSO));
            OnPropertyChanged(nameof(IsShowStartWindows));

        }
        private void SwapScrean(bool isKSO)
        {
            UpdateTypeWorkplace(isKSO);
            this.WindowState = WindowState.Normal;
            MW.WindowState = WindowState.Normal;
            if (isKSO)
            {
                this.Left = mainScreen.WorkingArea.Left;
                this.Top = mainScreen.WorkingArea.Top;


                MW.Left = additionalScreen.WorkingArea.Left;
                MW.Top = additionalScreen.WorkingArea.Top;
                MW.StartAddWares();
            }
            else
            {
                this.Left = additionalScreen.WorkingArea.Left;
                this.Top = additionalScreen.WorkingArea.Top;


                MW.Left = mainScreen.WorkingArea.Left;
                MW.Top = mainScreen.WorkingArea.Top;
            }

            MW.WindowState = WindowState.Maximized;
            this.WindowState = WindowState.Maximized;
            UpdateSecondMonitor();
        }

        private void StartNormal(object sender, RoutedEventArgs e)
        {
            SwapScrean(false);
        }
    }
}
