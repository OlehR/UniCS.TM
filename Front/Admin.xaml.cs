using Front.Equipments;
using ModelMID;
using ModelMID.DB;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Front
{
    /// <summary>
    /// Interaction logic for Admin.xaml
    /// </summary>
    public partial class Admin : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        EquipmentFront EF;
        ObservableCollection<Equipment> LE;
        ObservableCollection<Receipt> Receipts;
        BL Bl;
        MainWindow MW;
        Receipt curReceipt = null;
        public Admin(User AdminUser, MainWindow pMW)
        {
            MW = pMW;
            EF = EquipmentFront.GetEquipmentFront;
            Bl = BL.GetBL;
            InitializeComponent();
            if (EF != null)
                LE = new ObservableCollection<Equipment>(EF.GetListEquipment);
            ListEquipment.ItemsSource = LE;
            //поточний час
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            adminastratorName.Text = AdminUser.NameUser;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            lblTime.Content = DateTime.Now.ToShortTimeString();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
            // this.NavigationService.GoBack();
        }
        private void POS_X_Click(object sender, RoutedEventArgs e)
        {
            EF.PosPrintX();
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
                var r = EF.RroPrintX();
                r.IdWorkplace = Global.IdWorkPlace;
                r.CodePeriod = Global.GetCodePeriod();
                Bl.InsertLogRRO(r);
            });
        }

        private void EKKA_Z_Click(object sender, RoutedEventArgs e)
        {
            var task = Task.Run(() =>
            {
                var r = EF.RroPrintZ();
                r.IdWorkplace = Global.IdWorkPlace;
                r.CodePeriod = Global.GetCodePeriod();
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
            MW.IsLockSale = false;
        }

        private void WorkFinish_Click(object sender, RoutedEventArgs e)
        {
            MW.IsLockSale = true;
        }

        private void CloseDay_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabHistory.IsSelected)
            {
                DateTime dt = DateTime.Now.Date;
                Receipts = new ObservableCollection<Receipt>(Bl.GetReceipts(dt, dt, Global.IdWorkPlace));
                ListReceipts.ItemsSource = Receipts;
            }
        }

        private void historiReceiptList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curReceipt = ListReceipts.SelectedItem as Receipt;
            //Якогось не працює через get як я хочу :) Тому пока реалізація через Ж.
            IsPrintReceipt = curReceipt?.StateReceipt == eStateReceipt.Pay;
            IsPayReceipt = curReceipt?.StateReceipt == eStateReceipt.Prepare;
            IsInputPay = curReceipt?.StateReceipt == eStateReceipt.Prepare;
            IsSendTo1C = curReceipt?.StateReceipt == eStateReceipt.Print;
            IsCreateReturn = (curReceipt?.StateReceipt == eStateReceipt.Send || curReceipt?.StateReceipt == eStateReceipt.Print) && curReceipt?.TypeReceipt == eTypeReceipt.Sale;
        }

        private void FiscalizCheckButton(object sender, RoutedEventArgs e)
        {
            var R = Bl.GetReceiptHead(curReceipt, true);
            Bl.SetStateReceipt(curReceipt, eStateReceipt.Canceled);
            var res = EF.PrintReceipt(R);
            Bl.InsertLogRRO(res);
            if (res.CodeError == 0)
            {
                Bl.UpdateReceiptFiscalNumber(R, res.FiscalNumber, res.SUM);                
            }
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
            MessageBox.Show("Реквізити на оплату");
        }

        private void Transfer1CButton(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Передати в 1С");
        }

        private void ReturnCheckButton(object sender, RoutedEventArgs e)
        {
            Bl.CreateRefund(curReceipt);
            Close();
        }

        //Якогось не працює через get як я хочу :) Тому пока реалізація через Ж.
        public bool IsPrintReceipt { get; set; } = false;// { get { return curReceipt?.StateReceipt == eStateReceipt.Pay; } }  //
        public bool IsPayReceipt { get; set; } = false;//{ get { return curReceipt?.StateReceipt == eStateReceipt.Prepare; } } // 
        public bool IsInputPay { get; set; } = false;// { get { return curReceipt?.StateReceipt == eStateReceipt.Prepare; } }
        public bool IsSendTo1C { get; set; } = false;// { get { return curReceipt?.StateReceipt == eStateReceipt.Print; } }
        public bool IsCreateReturn { get; set; } = false;// { get { return curReceipt?.StateReceipt == eStateReceipt.Send && curReceipt?.TypeReceipt == eTypeReceipt.Sale; } }

        private void ReturnAllCheckButton(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Повернути весь чек");
        }

        private void FindChecksByDate(object sender, RoutedEventArgs e)
        {
            if (TabHistory.IsSelected)
            {
                DateTime dt = dataHistori.SelectedDate.Value.Date;
                Receipts = new ObservableCollection<Receipt>(Bl.GetReceipts(dt, dt, Global.IdWorkPlace));
                ListReceipts.ItemsSource = Receipts;
            }
            // Поки не знаю як реалізувати пошук
        }
    }
}
