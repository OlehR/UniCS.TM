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
using Front.Models;
using System.Threading.Tasks;

namespace Front
{

    public partial class FindWaresWin : Window
    {
        public event PropertyChangedEventHandler PropertyChanged;

        BL Bl;
        int CodeFastGroup = 0;
        int OffSet = 0;
        int Limit = 10;
        int MaxPage = 0;
        public bool IsUp { get { return CodeFastGroup > 0; } }
        public bool Volume { get; set; }
        MainWindow MW;
        public FindWaresWin(MainWindow pMW)
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
            //WindowStyle = WindowStyle.None;

            MW = pMW;
            Bl = BL.GetBL;
            KB.SetInput(WaresName);
            NewB();
        }

        string LastStr=null;
        void NewB()
        {
            IEnumerable<GW> WG = null;
            if (CodeFastGroup == 0 && WaresName.Text.Length == 0)
            {
                var aa = Bl.db.GetFastGroup(Global.CodeWarehouse)?.Select(r => new GW(r)).ToList();
                MaxPage = aa.Count() / Limit;
                for (int i = 0; i < Limit * OffSet; i++)
                    aa.RemoveAt(0);

                WG = aa;
            }
            else
            {
                if(WaresName.Text!= LastStr)
                {
                    LastStr= WaresName.Text;
                    OffSet = 0;
                }
                WG = Bl.GetProductsByName(MW.curReceipt, (WaresName.Text.Length > 1 ? WaresName.Text : ""),  OffSet * Limit, Limit, CodeFastGroup)?.Select(r => new GW(r));
                if (WG != null)
                    MaxPage = WG.First().TotalRows / Limit;
                else
                    MaxPage = 0;

            }
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

                Bt.Name = el.GetName;// $"BtGr{el.Code}";				
                if (File.Exists(el.Pictures))
                {
                    ImageStackPanel.Source = new BitmapImage(new Uri(el.Pictures));
                    ImageStackPanel.Height = 180;
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
                var leng = el.Name.Length;
                var lengthText = 20;
                if (leng > lengthText)
                {
                    NameWares.FontSize = 20;
                    NameWares.TextWrapping = TextWrapping.Wrap;
                    NameWares.Text = el.Name;//.Insert(lengthText, Environment.NewLine);
                }
                else
                {
                    NameWares.FontSize = 30;
                    NameWares.Text = el.Name; //max 19 

                }
                NameWares.FontFamily = new FontFamily("Source Sans Pro");
                NameWares.FontWeight = FontWeights.DemiBold;
                NameWares.HorizontalAlignment = HorizontalAlignment.Center;
                NameWares.VerticalAlignment = VerticalAlignment.Center;
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
                if (++i >= 5)
                { j ++; i = 0; }
                if (j >= 2) break;
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
                    if (Gw.CodeUnit == Global.WeightCodeUnit)
                    {
                        Close(Gw.Code, Gw.CodeUnit, 0, Gw);
                    }
                    else
                        Close(Gw.Code, Gw.CodeUnit, 1m);
                }
        }

        private void Close(int pCodeWares, int pCodeUnit = 0, decimal pQuantity = 0m, GW pGW = null)
        {
            if (MW != null)
            {
                MW.AddWares(pCodeWares, pCodeUnit, pQuantity, 0m, pGW);
                if (MW.State == eStateMainWindows.WaitFindWares)
                    MW.SetStateView(eStateMainWindows.WaitInput);
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
