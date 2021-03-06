﻿using Front.Equipments;
using ModelMID;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Front
{
    /// <summary>
    /// Interaction logic for Admin.xaml
    /// </summary>
    public partial class Admin : Window
    {
        EquipmentFront EF;
        ObservableCollection<EquipmentElement> LE;
        ObservableCollection<Receipt> Receipts;
        BL Bl;
        public Admin()
        {
            EF  = EquipmentFront.GetEquipmentFront;
            Bl = BL.GetBL;
            InitializeComponent();
            if(EF!=null)
               LE = new ObservableCollection<EquipmentElement>(EF.GetListEquipment);  
            ListEquipment.ItemsSource = LE;           
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
           // this.NavigationService.GoBack();
        }
        private void POS_X_Click(object sender, RoutedEventArgs e)
        {
            EF.Terminal.PrintX();
        }

        private void POS_Z_Click(object sender, RoutedEventArgs e)
        {
            EF.Terminal.PrintZ();
        }

        private void POS_X_Copy_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EKKA_X_Click(object sender, RoutedEventArgs e)
        {
            EF.EKKA.PrintX();
        }

        private void EKKA_Z_Click(object sender, RoutedEventArgs e)
        {
            EF.EKKA.PrintZ();
        }
        private void EKKA_Z_Period_Click(object sender, RoutedEventArgs e)
        {

        }                

        private void EKKA_Copy_Click(object sender, RoutedEventArgs e)
        {
            EF.EKKA.PrintCopyReceipt();
        }

        private void WorkStart_Click(object sender, RoutedEventArgs e)
        {

        }

        private void WorkFinish_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseDay_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabHistory.IsSelected)
            {
                DateTime dt = DateTime.Now.Date;
                Receipts = new ObservableCollection<Receipt>(Bl.GetReceipts(dt,dt,Global.IdWorkPlace));
                ListReceipts.ItemsSource=Receipts;
            }
        }
    }
}
