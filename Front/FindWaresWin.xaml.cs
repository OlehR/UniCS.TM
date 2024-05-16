using ModelMID.DB;
using ModelMID;
using SharedLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Windows.Media;
using Front.Equipments;
using System.Threading.Tasks;
using static Front.MainWindow;
using Utils;

namespace Front
{

    public partial class FindWaresWin : Window
    {
        public event PropertyChangedEventHandler PropertyChanged;

        BL Bl;
        BLF Blf;
        int CodeFastGroup = 0;
        int OffSet = 0;
        int MaxPage = 0;

        public bool IsUp { get { return CodeFastGroup > 0; } }
        public bool Volume { get; set; }
        public bool IsHorizontalScreen { get { return SystemParameters.PrimaryScreenWidth < SystemParameters.PrimaryScreenHeight ? true : false; } }
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
            }
        }
        public int HeightKeyboard { get; set; } = 400;
        private void CalculateHeightKeyboard()
        {
            switch (TypeMonitor)
            {
                case eTypeMonitor.HorisontalMonitorKSO:
                    HeightKeyboard = 400;
                    break;
                case eTypeMonitor.VerticalMonitorKSO:
                    HeightKeyboard = 400;
                    break;
                case eTypeMonitor.HorisontalMonitorRegular:
                    HeightKeyboard = 320;
                    break;
                default:
                    HeightKeyboard =320;
                    break;
            }
        }
        public int WidthScreen { get { return (int)SystemParameters.PrimaryScreenWidth; } }
        public int HeightScreen { get { return (int)SystemParameters.PrimaryScreenHeight; } }
        /// <summary>
        /// максимальна кількість товарів на вікні
        /// </summary>
        int Limit = SystemParameters.PrimaryScreenWidth < SystemParameters.PrimaryScreenHeight ? 15 : 10;
        /// <summary>
        /// Кількість рядків в пошуку (залежить від розміру екрану)
        /// </summary>
        int CountRowWares;
        /// <summary>
        /// Кількість колонок в пошуку (залежить від розміру екрану)
        /// </summary>
        int CountColumWares;
        MainWindow MW;
        public FindWaresWin(MainWindow pMW)
        {
            CalculateHeightKeyboard();
            InitializeComponent();
           // WindowState = WindowState.Maximized;
            //WindowStyle = WindowStyle.None;
            CountRowWares = IsHorizontalScreen ? 5 : 2;
            CountColumWares = IsHorizontalScreen ? 3 : 5;
            CreateGridForWares(); //створення гріду з товарами
            MW = pMW;
            Bl = BL.GetBL;
            Blf = BLF.GetBLF;
            KB.SetInput(WaresName);
            NewB();

        }
        void CreateGridForWares()
        {
            for (int i = 0; i < CountRowWares; i++)
            {
                PictureGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i < CountColumWares; i++)
            {
                PictureGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

        }

        string LastStr = null;
        void NewB()
        {
            var WG=Blf.GetDataFindWares(CodeFastGroup, WaresName.Text, MW.curReceipt, ref OffSet, ref MaxPage, ref Limit);            

            ButtonUp.IsEnabled = IsUp;
            ButtonLeft.IsEnabled = (OffSet > 0);
            ButtonRight.IsEnabled = (OffSet < MaxPage);
            BildButtom(WG);
        }

        void BildButtom(IEnumerable<GW> pGW)
        {
            PictureGrid.Children.Clear();
            if (pGW == null)
                return;

            int i = 0, j = 0;

            foreach (var el in pGW)
            {
                //кнопка в якій знаходиться фото та назва
                var Bt = new ToggleButton();
                //назва товару
                var NameWares = new TextBlock();
                //рамка для розділення 
                var Bor = new Border();
                //для групування фото на назви
                var StackP = new StackPanel();
                //фото
                var ImageStackPanel = new Image();

                Bor.BorderBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                Bor.BorderThickness = new Thickness(2.0);
                Bor.CornerRadius = new CornerRadius(15);
                Bor.Margin = new Thickness(5);

                Bt.Name = el.GetName;// $"BtGr{el.Code}";				
                if (File.Exists(el.Pictures))
                {
                    ImageStackPanel.Source = new BitmapImage(new Uri(el.Pictures));
                    ImageStackPanel.Height = TypeMonitor == eTypeMonitor.HorisontalMonitorRegular || TypeMonitor == eTypeMonitor.AnotherTypeMonitor ? 100 : 180; //180;
                    ImageStackPanel.Margin = new Thickness(5);
                    //Bt.Content = new Image
                    //{
                    //    Source = new BitmapImage(new Uri(el.Pictures)),
                    //    VerticalAlignment = VerticalAlignment.Center
                    //};
                }

                else
                    Bt.Content = "Скоро буде ;)";

                Bt.Click += BtClick;
                Bt.Tag = el;
                Bt.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                Bt.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                //Розміщення кнопки
                Grid.SetColumn(Bt, i);
                Grid.SetRow(Bt, j);



                //розташування рамки між товарами
                Grid.SetColumn(Bor, i);
                Grid.SetRow(Bor, j);
                //Grid.SetRowSpan(Bor, 2);
                //Розділення на 2 радки якщо текст завеликий
                int leng;
                if (el.Name != null) leng = el.Name.Length;
                else leng = 0;

                var lengthText = 20;
                NameWares.TextWrapping = TextWrapping.Wrap;
                if (TypeMonitor ==  eTypeMonitor.HorisontalMonitorRegular || TypeMonitor == eTypeMonitor.AnotherTypeMonitor)
                {
                    NameWares.FontSize = 12;
                    NameWares.Text = el.Name;//.Insert(lengthText, Environment.NewLine);
                }
                else
                {
                    NameWares.FontSize = 20;
                    NameWares.Text = el.Name; //max 19 

                }
                NameWares.FontFamily = new FontFamily("Source Sans Pro");
                NameWares.FontWeight = FontWeights.DemiBold;
                NameWares.HorizontalAlignment = HorizontalAlignment.Center;
                NameWares.VerticalAlignment = VerticalAlignment.Center;
                NameWares.Margin = new Thickness(5, 0, 5, 0);
                NameWares.TextAlignment = TextAlignment.Center;
                if (el.Type == 1) //якщо група товарів тоді показати лише фото
                {
                    StackP.Children.Add(ImageStackPanel);
                    //Grid.SetRowSpan(Bt, 2);
                }
                else // якщо самі товари то додати опис
                {
                    StackP.Children.Add(ImageStackPanel);
                    StackP.Children.Add(NameWares);
                    //розташування назви
                    //Grid.SetColumn(NameWares, i);
                    //Grid.SetRow(NameWares, j+1 );
                    //PictureGrid.Children.Add(NameWaresGrid);

                }

                Bt.Content = StackP;

                PictureGrid.Children.Add(Bt);
                PictureGrid.Children.Add(Bor);
                // Якщо товар ваговий тоді малюємо значок
                if (el.IsWeight)
                {
                    var WeightImg = new Image();
                    Uri resourceUri = new Uri("/icons/weight.png", UriKind.Relative);
                    WeightImg.Source = new BitmapImage(resourceUri);
                    WeightImg.Width = WeightImg.Height = 50;
                    WeightImg.HorizontalAlignment = HorizontalAlignment.Right;
                    WeightImg.VerticalAlignment = VerticalAlignment.Top;
                    Grid.SetColumn(WeightImg, i);
                    Grid.SetRow(WeightImg, j);
                    PictureGrid.Children.Add(WeightImg);
                }
                if (++i >= CountColumWares)
                { j++; i = 0; }
                if (j >= CountRowWares) break;
            }
        }

        private void BtClick(object sender, RoutedEventArgs e)
        {
            OffSet = 0;
            ToggleButton aa = (ToggleButton)sender;
            GW Gw = aa.Tag as GW;
            if (Gw != null)
                if (Gw.Type == 1)
                {
                    CodeFastGroup = Gw.Code;
                    NewB();
                }
                else
                {                    
                    Close( Gw, Blf.GetQuantity(WaresName.Text, Gw.CodeUnit));
                }
        }

        private void Close(GW pGW, decimal pQuantity = 0m)
        {
            if (MW != null)
            {
                if (pGW.CodeUnit == Global.WeightCodeUnit && pQuantity == 0)
                    MW.ShowWeightWares(pGW);
                else
                {
                    MW.Blf.AddWares(pGW.Code, pGW.CodeUnit, pQuantity, 0m);
                    if (MW.State == eStateMainWindows.WaitFindWares)
                        MW.SetStateView(eStateMainWindows.WaitInput);
                }
            }
            KB.SetInput(null);
            Close();
        }

        private async void WaresName_Changed(object sender, TextChangedEventArgs e)
        {
            await Task.Delay(1500);
            NewB();
        }

        private void ClickButtonUp(object sender, RoutedEventArgs e)
        {
            if (CodeFastGroup > 0)
            {
                CodeFastGroup = 0;
                OffSet = 0;
                NewB();
            }
        }
        private void ClickButtonLeft(object sender, RoutedEventArgs e)
        {
            if (OffSet > 0)
            {
                OffSet--;
                NewB();
            }
        }
        private void ClickButtonRight(object sender, RoutedEventArgs e)
        {
            if (OffSet < MaxPage)
            {
                OffSet++;
                NewB();
            }
        }

        private void ClickButtonCancel(object sender, RoutedEventArgs e)
        {
            MW?.SetStateView(eStateMainWindows.WaitInput);
            KB.SetInput(null);
            Close();
        }

        private void _ButtonHelp(object sender, RoutedEventArgs e)
        {
            MW?.SetStateView(eStateMainWindows.WaitAdmin);
            KB.SetInput(null);
            Close();
        }
    }


}
