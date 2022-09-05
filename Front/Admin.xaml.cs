﻿using Front.Equipments;
using ModelMID;
using ModelMID.DB;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utils;

namespace Front
{
    /// <summary>
    /// Interaction logic for Admin.xaml
    /// </summary>
    public partial class Admin : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        EquipmentFront EF;
        ObservableCollection<Receipt> Receipts;
        ObservableCollection<ParsLog> LogsCollection;
        public string TypeLog { get; set; } = "Full";
        BL Bl;
        public string ControlScaleWeightDouble { get; set; } = "0";
        MainWindow MW;
        Receipt curReceipt = null;
        public bool ClosedShift { get { return MW.IsLockSale; } }
        User AdminUser = null;


        public DateTime DateSoSearch { get; set; } = DateTime.Now.Date;
        public string KasaNumber { get { return Global.GetWorkPlaceByIdWorkplace(Global.IdWorkPlace).Name; } }

        public void ControlScale(double pWeight, bool pIsStable)
        {
            ControlScaleWeightDouble = Convert.ToString(pWeight/1000);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ControlScaleWeightDouble"));
        }

        public Admin(MainWindow pMW, EquipmentFront pEF)
        {

            MW = pMW;
            //EF = EquipmentFront.GetEquipmentFront;
            Bl = BL.GetBL;
            EF = pEF;

            EF.OnControlWeight += (pWeight, pIsStable) =>
            {
                double r = pWeight;
                ControlScaleWeightDouble = Convert.ToString(pWeight/1000);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ControlScaleWeightDouble"));
            };


            InitializeComponent();



            //поточний час
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();


        }
        public void Init(User AdminUser = null)
        {
            this.AdminUser = AdminUser; 
            
            if (EF != null)
                ListEquipment.ItemsSource = new ObservableCollection<Equipment>(EF.GetListEquipment);
        }
        private bool LogFilter(object item)
        {
            if (String.IsNullOrEmpty(TypeLog))
                return true;
            else
                return ((item as ParsLog).LineLog.IndexOf(TypeLog, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            lblTime.Content = DateTime.Now.ToShortTimeString();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MW.SetStateView(Models.eStateMainWindows.StartWindow);
            this.WindowState = WindowState.Minimized;
            // this.NavigationService.GoBack();
        }
        private void POS_X_Click(object sender, RoutedEventArgs e)
        {
           var r= EF.PosPrintX();
            var L = EF.GetLastReceiptPos();
            List<string> list = new() { "X-звіт постермінал", $"Всього: {r.DebitSum}({r.DebitCount})", $"Всього2: {r.CreditSum} ({r.CreditCount}) ", $"Crfcjdf: {r.CencelledSum} ({r.CencelledCount}) " };
            LogRRO d = new(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod() })
            { TypeOperation = eTypeOperation.XReportPOS, JSON = r.ToJSON(), TextReceipt = L.ToJSON() };
            Bl.InsertLogRRO(d);
            EF.PrintNoFiscalReceipt(list);
        }

        private void POS_Z_Click(object sender, RoutedEventArgs e)
        {
            EF.PosPrintZ();
        }

        private void POS_X_Copy_Click(object sender, RoutedEventArgs e)
        {
            //EF.RroPrintCopyReceipt
        }

        private void EKKA_X_Click(object sender, RoutedEventArgs e)
        {
            var task = Task.Run(() =>
            {
                var r = EF.RroPrintX(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod() });
                Bl.InsertLogRRO(r);
            });
        }

        private void EKKA_Z_Click(object sender, RoutedEventArgs e)
        {
            var task = Task.Run(() =>
            {
                var r = EF.RroPrintZ(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod() });
                Bl.InsertLogRRO(r);
            });
        }

        private void EKKA_Z_Period_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EKKA_Copy_Click(object sender, RoutedEventArgs e)
        {
            EF.RroPrintCopyReceipt();
        }

        private void WorkStart_Click(object sender, RoutedEventArgs e)
        {
            MW.AdminSSC = AdminUser;
            MW.Bl.db.SetConfig<DateTime>("DateAdminSSC", DateTime.Now);
            MW.Bl.db.SetConfig<string>("CodeAdminSSC", AdminUser.BarCode);            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClosedShift"));
        }

        private void WorkFinish_Click(object sender, RoutedEventArgs e)
        {
            MW.AdminSSC = null;
            MW.Bl.db.SetConfig<string>("CodeAdminSSC", string.Empty);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClosedShift"));
        }

        private void CloseDay_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            //EF.PosPrintX();
            Button btn = sender as Button;
            if (btn != null)
            {
                Equipment Eq = btn.DataContext as Equipment;
                if (Eq != null)
                {
                    var res = Eq.TestDevice();
                    MessageBox.Show($"{res.StateEquipment} {res.TextState}", res.ModelEquipment.ToString());
                }
            }
        }

        private void Info_Click(object sender, RoutedEventArgs e)
        {
            //EF.PosPrintX();
            Button btn = sender as Button;
            if (btn != null)
            {
                Equipment Eq = btn.DataContext as Equipment;
                if (Eq != null)
                {
                    var res = Eq.GetDeviceInfo();
                    MessageBox.Show(res, Eq.Model.ToString());
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime dt = DateTime.Now.Date;
            if (TabHistory.IsSelected && DateSoSearch == dt)
            {
                Receipts = new ObservableCollection<Receipt>(Bl.GetReceipts(dt, dt, Global.IdWorkPlace));
                ListReceipts.ItemsSource = Receipts.Reverse();
            }
            string tabItem = ((sender as TabControl).SelectedItem as TabItem).Header as string;

            switch (tabItem)
            {
                case "Зміна":
                    break;

                case "Пристрої":
                    Init(AdminUser);
                    break;

                case "Історія":
                    break;
                case "Помилки":
                    RefreshLog();
                    break;

                default:
                    return;
            }
        }
        private void RefreshLog()
        {
            string AllLog = File.ReadAllText(Utils.FileLogger.GetFileName);
            string[] temp = AllLog.Split($"{Environment.NewLine}[");
            LogsCollection = new ObservableCollection<ParsLog>();
            foreach (string item in temp)
            {
                if (item.Contains("Error"))
                    LogsCollection.Add(new ParsLog() { LineLog = "[" + item, TypeLog = eTypeLog.Error });
                if (item.Contains("Expanded"))
                    LogsCollection.Add(new ParsLog() { LineLog = "[" + item, TypeLog = eTypeLog.Expanded });
                else
                    LogsCollection.Add(new ParsLog() { LineLog = "[" + item, TypeLog = eTypeLog.Full });
            }
            //LogsCollection = new ObservableCollection<string>(AllLog.Split($"{Environment.NewLine}[").Select(a => "[" + a));

            ListLog.ItemsSource = LogsCollection;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListLog.ItemsSource);
            view.Filter = LogFilter;
        }
        private void historiReceiptList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curReceipt = ListReceipts.SelectedItem as Receipt;
            //Якогось не працює через get як я хочу :) Тому пока реалізація через Ж.
            IsPrintReceipt = curReceipt?.StateReceipt == eStateReceipt.Pay || curReceipt?.StateReceipt == eStateReceipt.StartPrint;
            IsPayReceipt = curReceipt?.StateReceipt == eStateReceipt.Prepare || curReceipt?.StateReceipt == eStateReceipt.StartPay;
            IsInputPay = curReceipt?.StateReceipt == eStateReceipt.Prepare || curReceipt?.StateReceipt == eStateReceipt.StartPay;
            IsSendTo1C = curReceipt?.StateReceipt == eStateReceipt.Print;
            IsCreateReturn = (curReceipt?.StateReceipt == eStateReceipt.Send || curReceipt?.StateReceipt == eStateReceipt.Print) && curReceipt?.TypeReceipt == eTypeReceipt.Sale;
        }

        private void FiscalizCheckButton(object sender, RoutedEventArgs e)
        {
            MW.PrintAndCloseReceipt(curReceipt);
            
            //MessageBox.Show("Фiскалізовано");
        }

        private void PayAdminPanelButton(object sender, RoutedEventArgs e)
        {
            var R = Bl.GetReceiptHead(curReceipt, true);
            if (R.StateReceipt == eStateReceipt.Prepare)
            {
                decimal sum = R.Wares.Sum(r => r.Sum); //TMP!!!Треба переробити

                var pay = EF.PosPurchase(sum);
                if (pay != null)
                {
                    pay.SetIdReceipt(R);
                    Bl.db.ReplacePayment(new List<Payment>() { pay });
                    Bl.SetStateReceipt(R, eStateReceipt.Pay);
                    //curReceipt.StateReceipt = eStateReceipt.Pay;
                }
            }
            //MessageBox.Show("Оплачено");
        }

        private void PaymentDetailsAdminPanelButton(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Реквізити на оплату");
            TerminalPaymentInfo terminalPaymentInfo = new TerminalPaymentInfo(this);
            if (terminalPaymentInfo.ShowDialog() == true)
            {
                var Res = terminalPaymentInfo.enteredDataFromTerminal;
            }
        }

        private void Transfer1CButton(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Передати в 1С");
        }

        private void ReturnCheckButton(object sender, RoutedEventArgs e)
        {
           
            Bl.CreateRefund(curReceipt);
            this.WindowState = WindowState.Minimized;
            //Close();
        }

        //Якогось не працює через get як я хочу :) Тому пока реалізація через Ж.
        public bool IsPrintReceipt { get; set; } = false;// { get { return curReceipt?.StateReceipt == eStateReceipt.Pay; } }  //
        public bool IsPayReceipt { get; set; } = false;//{ get { return curReceipt?.StateReceipt == eStateReceipt.Prepare; } } // 
        public bool IsInputPay { get; set; } = false;// { get { return curReceipt?.StateReceipt == eStateReceipt.Prepare; } }
        public bool IsSendTo1C { get; set; } = false;// { get { return curReceipt?.StateReceipt == eStateReceipt.Print; } }
        public bool IsCreateReturn { get; set; } = false;// { get { return curReceipt?.StateReceipt == eStateReceipt.Send && curReceipt?.TypeReceipt == eTypeReceipt.Sale; } }

        private void ReturnAllCheckButton(object sender, RoutedEventArgs e)
        {
            Bl.CreateRefund(curReceipt,true);
            this.WindowState = WindowState.Minimized;
            //MessageBox.Show("Повернути весь чек");
        }

        private void FindChecksByDate(object sender, RoutedEventArgs e)
        {
            if (TabHistory.IsSelected)
            {
                //DateTime dt = dataHistori.SelectedDate.Value.Date;
                Receipts = new ObservableCollection<Receipt>(Bl.GetReceipts(DateSoSearch, DateSoSearch, Global.IdWorkPlace));
                ListReceipts.ItemsSource = Receipts.Reverse();
            }
            // Поки не знаю як реалізувати пошук
        }

        private void PowerOff(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вимкнути касу?", "Увага!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0");
            }
        }

        private void RebootPC(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Перезавантажити касу?", "Увага!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
            }
        }

        async private void RefreshDataButton(object sender, RoutedEventArgs e)
        {
            await Bl.ds.SyncDataAsync(true);
        }

        private void CheckTypeLog(object sender, RoutedEventArgs e)
        {
            RadioButton btn = sender as RadioButton;
            switch (btn.Content)
            {
                case "Error":
                    TypeLog = "Error";
                    break;
                case "Expanded":
                    TypeLog = "Expanded";
                    break;
                case "Full":
                    TypeLog = "Full";
                    break;
            }
            CollectionViewSource.GetDefaultView(ListLog.ItemsSource).Refresh();
        }

        private void FindFogTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            TypeLog = textBox.Text;
            CollectionViewSource.GetDefaultView(ListLog.ItemsSource).Refresh();
        }

        private void CalibrationControlScaleButton(object sender, RoutedEventArgs e)
        {
            KeyPad keyPad = new KeyPad(this);
            keyPad.productNameChanges.Text = "Введіть вагу в грамах";
            if (keyPad.ShowDialog() == true)
                EF.ControlScaleCalibrateMax(Convert.ToDouble(keyPad.Result));
        }

        private void TarControlScaleButton(object sender, RoutedEventArgs e)
        {
            EF.ControlScaleCalibrateZero();
        }

        private void CloseApplicationButton(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Завершити роботу програми?", "Увага!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }

    public class ParsLog
    {
        public string LineLog { get; set; }
        public eTypeLog TypeLog { get; set; }
    }


}
