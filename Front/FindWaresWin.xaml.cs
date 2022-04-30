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
            //KB.SetInput(WaresName);
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
                Bt.Name = el.GetName;// $"BtGr{el.Code}";				
                if (File.Exists(el.Pictures))
                    Bt.Content = new Image
                    {
                        Source = new BitmapImage(new Uri(el.Pictures)),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                else
                    Bt.Content = el.Name;
                Bt.Click += BtClick;
                Bt.Tag = el;
                Grid.SetColumn(Bt, i);
                Grid.SetRow(Bt, j);
                PictureGrid.Children.Add(Bt);
                if (++i >= 5)
                { j++; i = 0; }
                if (j >= 2) break;
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
            MW?.AddWares(pCodeWares, pCodeUnit, pQuantity, 0m, pGW);
            Close();
        }
        private void WaresName_Changed(object sender, TextChangedEventArgs e)
        {
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
            Close();
        }

    }


}
