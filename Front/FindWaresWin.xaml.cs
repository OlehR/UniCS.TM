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
                WG = Bl.GetProductsByName(MW.curReceipt, (WaresName.Text.Length > 1 ? WaresName.Text : ""), OffSet, Limit, CodeFastGroup)?.Select(r => new GW(r));
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
                var Bt = new ToggleButton();
                var NameWaresGrid = new TextBlock();
                var Bor = new Border();
                Bor.BorderBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                Bor.BorderThickness = new Thickness(2.0);
                Bt.Name = el.GetName;// $"BtGr{el.Code}";				
                if (File.Exists(el.Pictures))
                    Bt.Content = new Image
                    {
                        Source = new BitmapImage(new Uri(el.Pictures)),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                else
                    Bt.Content = "Скоро буде ;)";

                Bt.Click += BtClick;
                Bt.Tag = el;
                Bt.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                Bt.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                //Кнопка з картинкою
                Grid.SetColumn(Bt, i);
                Grid.SetRow(Bt, j);
                if (el.Type == 1) //якщо група товарів тоді показати лише фото
                {
                    Grid.SetRowSpan(Bt, 2);
                }
                else // якщо самі товари то додати опис
                {
                    //ім'я товару під фоткою
                    Grid.SetColumn(NameWaresGrid, i);
                    Grid.SetRow(NameWaresGrid, j + 1);
                    PictureGrid.Children.Add(NameWaresGrid);

                }

                //рамка між товарами
                Grid.SetColumn(Bor, i);
                Grid.SetRow(Bor, j);
                Grid.SetRowSpan(Bor, 2);
                if (el.Name.Length > 19)
                    NameWaresGrid.Text = el.Name.Substring(0, 19);
                else
                    NameWaresGrid.Text = el.Name; //max 19 
                NameWaresGrid.FontFamily = new FontFamily("Source Sans Pro");
                NameWaresGrid.FontSize = 30;
                NameWaresGrid.FontWeight = FontWeights.DemiBold;
                NameWaresGrid.HorizontalAlignment = HorizontalAlignment.Center;
                NameWaresGrid.VerticalAlignment = VerticalAlignment.Bottom;

                PictureGrid.Children.Add(Bt);
                PictureGrid.Children.Add(Bor);
                if (++i >= 5)
                { j += 2; i = 0; }
                if (j >= 4) break;
            }
        }

        private void BtClick(object sender, RoutedEventArgs e)
        {
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
            Close();
        }
        private async void WaresName_Changed(object sender, TextChangedEventArgs e)
        {
            await Task.Delay(3000);
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
            Close();
        }

    }


}
