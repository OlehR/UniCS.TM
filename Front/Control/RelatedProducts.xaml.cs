using Front.Equipments;
using ModelMID;
using ModelMID.DB;
using ModernExpo.SelfCheckout.Utils;
using SharedLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Front.Control
{
    /// <summary>
    /// Interaction logic for RelatedProducts.xaml
    /// </summary>
    public partial class RelatedProducts : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        BL Bl;
        BLF Blf;
        public WDB_SQLite db;
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
        public string TextRelatedProducts { get => LastWares.IsNotNull() ? $"Додаткові товари до: {LastWares.NameWares}" : "Помилка"; }
        public bool IsShowLinkWares { get; set; } = true;
        ReceiptWares LastWares = new();
        MainWindow MW;
        public void Init(MainWindow mw)
        {
            MW = mw;
            Bl = BL.GetBL;
            Blf = BLF.GetBLF;
            CountRowWares = IsHorizontalScreen ? 5 : 2;
            CountColumWares = IsHorizontalScreen ? 3 : 5;
            db = WDB_SQLite.GetInstance;
            CreateGridForWares(); //створення гріду з товарами
        }
        public void AddRelatedProducts(ReceiptWares lastWares)
        {
            LastWares = lastWares.IsNotNull() ? lastWares : new ReceiptWares();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextRelatedProducts)));
            NewB();
        }
        public RelatedProducts()
        {
            InitializeComponent();
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
        void NewB()
        {
            List<GW> WG = LastWares.WaresLink.ToList(); //Blf.GetDataFindWares(CodeFastGroup, string.Empty, MW.curReceipt, ref OffSet, ref MaxPage, ref Limit);
            for (int i = 0; i < Limit * OffSet; i++)
                WG.RemoveAt(0);
            //ButtonUp.IsEnabled = IsUp;
            MaxPage = WG.Count() / Limit;
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
                    ImageStackPanel.Height = TypeMonitor == eTypeMonitor.HorisontalMonitorRegular || TypeMonitor == eTypeMonitor.AnotherTypeMonitor ? 80 : 160; //180;
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
                if (TypeMonitor == eTypeMonitor.HorisontalMonitorRegular || TypeMonitor == eTypeMonitor.AnotherTypeMonitor)
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
                NameWares.TextWrapping = TextWrapping.Wrap;
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
                var IsSelectedImg = new Image();
                Uri resourceUriIsSel = new Uri("/icons/check-mark.png", UriKind.Relative);
                IsSelectedImg.Source = new BitmapImage(resourceUriIsSel);
                IsSelectedImg.Width = IsSelectedImg.Height = 50;
                IsSelectedImg.HorizontalAlignment = HorizontalAlignment.Right;
                IsSelectedImg.VerticalAlignment = VerticalAlignment.Top;
                if (el.IsSelected)
                    IsSelectedImg.Visibility = Visibility.Visible;
                else
                    IsSelectedImg.Visibility = Visibility.Collapsed;

                IsSelectedImg.Tag = el.Code;
                Grid.SetColumn(IsSelectedImg, i);
                Grid.SetRow(IsSelectedImg, j);

                PictureGrid.Children.Add(IsSelectedImg);
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

            if (Gw.IsSelected)
                Gw.IsSelected = false;
            else 
                Gw.IsSelected = true;
            foreach (UIElement element in PictureGrid.Children)
            {
                if (element is Image) // якщо ваші зображення містяться в кнопках
                {
                    Image image = element as Image;
                    if (image != null && image.Tag.ToString() == Gw.Code.ToString())
                    {
                        image.Visibility = Gw.IsSelected? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }

            if (Gw != null)
                if (Gw.Type == 1)
                {
                    CodeFastGroup = Gw.Code;
                    NewB();
                }
                else
                {
                    //Додаємо супутній товар
                    if (Gw.IsSelected)
                        db.ReplaceWaresReceiptLink(new List<WaresReceiptLink> { new WaresReceiptLink
                    {
                        CodeWares = Gw.Code,
                        Quantity = 1,
                        CodeWaresTo = LastWares.CodeWares,
                        CodePeriod = LastWares.CodePeriod,
                        CodeReceipt = MW.curReceipt.CodeReceipt,
                        CodeUnit = LastWares.CodeUnit,
                        IdWorkplace = LastWares.IdWorkplace,
                        IdWorkplacePay = LastWares.IdWorkplacePay,
                        Order = LastWares.Order,
                        Parent = LastWares.Parent,
                        Sort = LastWares.Sort,

                    } });
                    else
                        MW.CustomMessage.Show($"Потрібно додати видалення позицій з яких зняли вибір", "Помилка!", eTypeMessage.Error);
                    //Close(Gw, Blf.GetQuantity(WaresName.Text, Gw.CodeUnit));
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

        private void Hide_ShowWaresLink(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Name)
            {
                case "ShowWaresLink":
                    IsShowLinkWares = true;
                    break;
                case "HideWaresLink":
                    IsShowLinkWares = false;
                    break;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsShowLinkWares)));
            MW.ScrolDown();

        }
    }
}
