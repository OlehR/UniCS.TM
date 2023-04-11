using Front.API;
using Front.Equipments;
//using Front.Equipments.Ingenico;
using Front.Models;
using ModelMID;
using ModelMID.DB;
using ModernExpo.SelfCheckout.Utils;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Utils;

namespace Front.Control
{
    /// <summary>
    /// Interaction logic for Admin.xaml
    /// </summary>
    public partial class Admin_control : UserControl, INotifyPropertyChanged
    {
        public Access Access = Access.GetAccess();
        public event PropertyChangedEventHandler PropertyChanged;
        EquipmentFront EF;
        ObservableCollection<Receipt> Receipts;
        ObservableCollection<ParsLog> LogsCollection;
        ObservableCollection<LogRRO> SourcesListJournal;
        ObservableCollection<Equipment> AllEquipment = new ObservableCollection<Equipment>();
        public string TypeLog { get; set; } = "Error";
        BL Bl;
        public string ControlScaleWeightDouble { get; set; } = "0";
        MainWindow MW;
        public Receipt curReceipt = null;
        public bool IsFullReturn = false;
        LogRRO SelectedListJournal = null;
        public bool IsSelectedListJournal { get { return SelectedListJournal != null; } }
        Receipt ChangeStateReceipt = null;
        public eStateReceipt newStateReceipt;
        public bool ClosedShift { get { return MW.IsLockSale; } }
        public User AdminUser { get; set; }
        public string NameAdminUserOpenShift { get { return MW?.AdminSSC?.NameUser; } }
        public DateTime DataOpenShift { get { return MW.DTAdminSSC; } }
        public bool IsShowAllReceipts { get; set; }
        public bool IsShowDeferredReceipts { get; set; }
        public bool IsShowAllJournal { get; set; }
        //BatchTotals LastReceipt = null;

        public bool IsCashRegister { get { return Global.TypeWorkplace == eTypeWorkplace.CashRegister; } }

        public DateTime DateSoSearch { get; set; } = DateTime.Now.Date;
        public DateTime DateStartPeriodZ { get; set; } = DateTime.Now.Date;
        public DateTime DateEndPeriodZ { get; set; } = DateTime.Now.Date;
        public eTypeMessage TypeAPIMessage { get; set; }
        public string KasaNumber { get { return Global.GetWorkPlaceByIdWorkplace(Global.IdWorkPlace).Name; } }
        public ObservableCollection<APIRadiobuton> TypeMessageRadiobuton { get; set; }
        public bool IsShortPeriodZ { get; set; } = true;
        public bool IsPrintCoffeQR { get; set; } = false;
        public bool IsСurReceipt { get; set; } = false;
        public bool IsDeferredReceipt { get; set; } = false;
        public IEnumerable<WorkPlace> WorkPlaces { get { return Global.GetIdWorkPlaces; } }
        WorkPlace _SelectedWorkPlace = null;
        public WorkPlace SelectedWorkPlace { get { return _SelectedWorkPlace != null ? _SelectedWorkPlace : WorkPlaces.First(); } set { _SelectedWorkPlace = value; } }
        ObservableCollection<Equipment> ActiveTerminals = new ObservableCollection<Equipment>();
        IEnumerable<string> TextReceipt;

        public void ControlScale(double pWeight, bool pIsStable)
        {
            ControlScaleWeightDouble = $"{(pWeight / 1000):N3}";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ControlScaleWeightDouble"));
        }

        public Admin_control()
        {
            Bl = BL.GetBL;
            TypeMessageRadiobuton = new ObservableCollection<APIRadiobuton>();
            foreach (eTypeMessage item in Enum.GetValues(typeof(eTypeMessage)))
            {
                TypeMessageRadiobuton.Add(new APIRadiobuton() { ServerTypeMessage = item });
            }
            Init(AdminUser);

            InitializeComponent();
            WorkPlacesList.ItemsSource = WorkPlaces;
            this.DataContext = this;
            ListRadioButtonAPI.ItemsSource = TypeMessageRadiobuton;



            RefreshJournal();
            //поточний час
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

        }

        public void Init(MainWindow pMW)
        {
            MW = pMW;
            ProgramVersion.Text = $"Версія КСО: {MW?.Version}";

            if (MW != null)
            {
                EF = MW?.EF;
                EF.OnControlWeight += (pWeight, pIsStable) =>
                {
                    ControlScale(pWeight, pIsStable);
                };
            };

        }

        public void Init(User pAdminUser = null)
        {
            if (pAdminUser != null)
                AdminUser = pAdminUser;

            //adminastratorName.Text = AdminUser.NameUser;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClosedShift"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AdminUser"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NameAdminUserOpenShift"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DataOpenShift"));
            //TB_NameAdminUserOpenShift.Text = NameAdminUserOpenShift;
            //TB_DataOpenShift.Text= $"{DataOpenShift}";
            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                try
                {
                    if (EF != null && EF.GetListEquipment?.Count() > 0)
                    {
                        AllEquipment = new ObservableCollection<Equipment>(EF.GetListEquipment);
                        ListEquipment.ItemsSource = AllEquipment;
                    }
                    if (ActiveTerminals.Count == 0)
                    {
                        SearchTerminal();
                    }
                }
                catch (Exception e) { }

            }));

        }
        private void SearchTerminal()
        {
            ActiveTerminals = new ObservableCollection<Equipment>();
            bool isFirst = true;
            foreach (var item in AllEquipment)
            {
                if (item.Type == eTypeEquipment.BankTerminal)
                {
                    item.IsSelected = isFirst;
                    ActiveTerminals.Add(item);
                    if (isFirst)
                    {
                        MW?.EF.SetBankTerminal(item as BankTerminal);
                    }
                    isFirst = false;
                }
            }
            TerminalList.ItemsSource = ActiveTerminals;
        }

        private bool LogFilter(object item)
        {
            if (String.IsNullOrEmpty(TypeLog))
                return true;
            else
                return ((item as ParsLog).LineLog.IndexOf(TypeLog, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private bool ReceiptFilter(object item)
        {
            if (IsShowAllReceipts && !IsShowDeferredReceipts)
            {
                return true;
            }
            else if (IsShowDeferredReceipts)
            {
                return ((item as Receipt).StateReceipt == eStateReceipt.Prepare);
            }
            else return ((item as Receipt).SumReceipt > 0);

        }

        private bool JournalFilter(object item)
        {
            if (IsShowAllJournal)
            {
                return true;
            }
            else return ((item as LogRRO).TypeOperation == eTypeOperation.ZReport ||
                    (item as LogRRO).TypeOperation == eTypeOperation.ZReportPOS ||
                    (item as LogRRO).TypeOperation == eTypeOperation.XReport ||
                    (item as LogRRO).TypeOperation == eTypeOperation.XReportPOS);

        }

        void timer_Tick(object sender, EventArgs e)
        {
            lblTime.Content = DateTime.Now.ToShortTimeString();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MW.SetStateView(Models.eStateMainWindows.StartWindow);
            //this.WindowState = WindowState.Minimized;
            // this.NavigationService.GoBack();
        }

        private void POS_X_Click(object sender, RoutedEventArgs e)
        {
            var task = Task.Run(() =>
            {
                var LastReceipt = EF.PosPrintX(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = SelectedWorkPlace.IdWorkplace }, false);
                ViewReceipt(LastReceipt.Receipt);
            });
        }

        private void POS_Z_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Ви хочете зробити Z-звіт на банківському терміналі?", "Увага!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var task = Task.Run(() =>
            {
                var LastReceipt = EF.PosPrintZ(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = SelectedWorkPlace.IdWorkplace });
                ViewReceipt(LastReceipt.Receipt);
            });
            }
        }
        private void OffLineClick(object sender, RoutedEventArgs e)
        {
            MW.Bl.ds.IsUseOldDB = !MW.Bl.ds.IsUseOldDB;
        }

        void ViewReceipt(IEnumerable<string> pText)
        {
            if (pText?.Any() == true)
            {
                TextReceipt = pText;
                Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    RevisionText.Text = string.Join(Environment.NewLine, TextReceipt);
                    Revision.Visibility = Visibility.Visible;
                    BackgroundShift.Visibility = Visibility.Visible;
                    RevisionScrollViewer.ScrollToEnd();
                }));
            }
        }

        void ViewReceiptFiscal(LogRRO pLastReceipt)
        {
            ViewReceipt(pLastReceipt.TextReceipt.Split(Environment.NewLine));
        }

        private void POS_X_Copy_Click(object sender, RoutedEventArgs e)
        {
            //EF.RroPrintCopyReceipt
        }

        private void EKKA_X_Click(object sender, RoutedEventArgs e)
        {
            var task = Task.Run(() =>
            {
                var r = EF.RroPrintX(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = SelectedWorkPlace.IdWorkplace });
                if (r.CodeError == 0)
                    ViewReceiptFiscal(r);
                else
                {
                    Thread.Sleep(100);
                    MW.ShowErrorMessage($"Помилка друку звіта:({r.CodeError}){Environment.NewLine}{r.Error}");
                }
            });
        }

        private void EKKA_Z_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Ви хочете зробити Z-звіт на фіскальному апараті?", "Увага!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var task = Task.Run(() =>
            {
                var r = EF.RroPrintZ(new IdReceipt() { IdWorkplace = SelectedWorkPlace.IdWorkplace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = SelectedWorkPlace.IdWorkplace });
                if (r.CodeError == 0)
                    ViewReceiptFiscal(r);
                else
                {
                    Thread.Sleep(100);
                    MW.ShowErrorMessage($"Помилка друку звіта:({r.CodeError}){Environment.NewLine}{r.Error}");
                }
            });
            }
        }

        private void EKKA_Z_Period_Click(object sender, RoutedEventArgs e)
        {
            var task = Task.Run(() =>
            {
                var tmpReceipt = new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = SelectedWorkPlace.IdWorkplace };
                var r = EF.RroPeriodZReport(tmpReceipt, DateStartPeriodZ, DateEndPeriodZ, !IsShortPeriodZ);
            });
        }

        private void EKKA_Copy_Click(object sender, RoutedEventArgs e)
        {
            EF.RroPrintCopyReceipt(SelectedWorkPlace.IdWorkplace);
        }

        private void WorkStart_Click(object sender, RoutedEventArgs e)
        {
            MW.AdminSSC = AdminUser;
            if (Global.TypeWorkplace == eTypeWorkplace.CashRegister)
                MW.Access.СurUser = AdminUser;
            MW.DTAdminSSC = DateTime.Now;
            MW.Bl.db.SetConfig<DateTime>("DateAdminSSC", DateTime.Now);
            MW.Bl.db.SetConfig<string>("CodeAdminSSC", AdminUser.BarCode);
            MW.Bl.StartWork(Global.IdWorkPlace, AdminUser.BarCode);
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClosedShift"));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NameAdminUserOpenShift"));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DataOpenShift"));
            OpenShiftShow.Visibility = Visibility.Visible;
            Init();
        }

        private void WorkFinish_Click(object sender, RoutedEventArgs e)
        {
            MW.AdminSSC = null;
            MW.Bl.db.SetConfig<string>("CodeAdminSSC", string.Empty);
            MW.Bl.StoptWork(Global.IdWorkPlace);
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClosedShift"));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NameAdminUserOpenShift"));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DataOpenShift"));
            OpenShiftShow.Visibility = Visibility.Collapsed;
            Init();
        }

        private void CloseDay_Click(object sender, RoutedEventArgs e)
        {
            //TMP!!!! Треба задати питання.
            return;
            var task = Task.Run(() =>
            {
                var IdR = new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = SelectedWorkPlace.IdWorkplace };
                EF.PosPrintZ(IdR);
                var r = EF.RroPrintZ(IdR);
            });
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
                IsShowAllReceipts = (bool)AllReceiptsCheckBox.IsChecked;
                ListReceipts.ItemsSource = Receipts.Reverse();
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListReceipts.ItemsSource);
                view.Filter = ReceiptFilter;

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
                case "Журнал":

                    break;
                case "Помилки":
                    RefreshLog();
                    break;
                case "Вихід":
                    MW.SetStateView(eStateMainWindows.StartWindow);
                    TabAdmin.SelectedIndex = 0;
                    //this.WindowState = WindowState.Minimized;
                    break;
                case "Вихід з програми":
                    if (MessageBox.Show("Завершити роботу програми?", "Увага!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start("explorer.exe");
                        Application.Current.Shutdown();
                    }
                    else TabAdmin.SelectedIndex = 0;
                    break;
                default:

                    return;
            }
        }

        private void RefreshJournal()
        {

            var TMPIdRecipt = new IdReceipt { CodePeriod = Global.GetCodePeriod(DateSoSearch), CodeReceipt = 0, IdWorkplace = Global.GetWorkPlaceByIdWorkplace(Global.IdWorkPlace).IdWorkplace };
            SourcesListJournal = new ObservableCollection<LogRRO>(Bl.GetLogRRO(TMPIdRecipt));
            ListJournal.ItemsSource = SourcesListJournal.Reverse();
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListJournal.ItemsSource);
            view.Filter = JournalFilter;
        }

        private void RefreshLog()
        {
            var PathToFileLog = FileLogger.GetFileNameDate(DateSoSearch);
            if (!File.Exists(PathToFileLog))
            {
                MessageBox.Show($"За обраною датою: {DateSoSearch.ToString("dd/MM/yyyy")} лог відсутній");
                ListLog.ItemsSource = null; ListLog.Items.Clear();
            }
            else
            {

                string AllLog = File.ReadAllText(FileLogger.GetFileNameDate(DateSoSearch));
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

                ListLog.ItemsSource = LogsCollection.Reverse();
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListLog.ItemsSource);
                view.Filter = LogFilter;
            }

        }

        private void historiReceiptList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curReceipt = ListReceipts.SelectedItem as Receipt;
            if (curReceipt != null)
            {
                curReceipt.Wares = Bl.GetWaresReceipt(curReceipt);
                //Якогось не працює через get як я хочу :) Тому пока реалізація через Ж.
                IsPrintReceipt = curReceipt?.StateReceipt != eStateReceipt.Print && curReceipt?.StateReceipt != eStateReceipt.Send;
                IsPayReceipt = curReceipt?.StateReceipt == eStateReceipt.Prepare || curReceipt?.StateReceipt == eStateReceipt.StartPay;
                IsInputPay = curReceipt?.StateReceipt == eStateReceipt.Prepare || curReceipt?.StateReceipt == eStateReceipt.StartPay;
                IsSendTo1C = curReceipt?.StateReceipt == eStateReceipt.Print || curReceipt?.StateReceipt == eStateReceipt.Send;
                IsCreateReturn = (curReceipt?.StateReceipt == eStateReceipt.Send || curReceipt?.StateReceipt == eStateReceipt.Print) && curReceipt?.TypeReceipt == eTypeReceipt.Sale;
                IsPrintCoffeQR = (bool)curReceipt?.IsQR();
                IsСurReceipt = true;
                IsDeferredReceipt = curReceipt?.StateReceipt == eStateReceipt.Prepare;
            }
            else
            {
                IsPrintReceipt = IsPayReceipt = IsInputPay = IsSendTo1C = IsCreateReturn = IsPrintCoffeQR = false;
            }
        }

        private void FiscalizCheckButton(object sender, RoutedEventArgs e)
        {
            MW.curReceipt = curReceipt;
            MW.PayAndPrint();
            //MessageBox.Show("Фiскалізовано");
        }

        private void PaymentDetailsAdminPanelButton(object sender, RoutedEventArgs e)
        {           
            TerminalPaymentInfo terminalPaymentInfo = new TerminalPaymentInfo(MW, curReceipt, WorkPlaces);
            if (terminalPaymentInfo.ShowDialog() == true && curReceipt != null)
            {
                var Res = terminalPaymentInfo.enteredDataFromTerminal;
                int Id = Res.IdWorkplacePay;
                Res.SetIdReceipt(curReceipt);
                Res.DateCreate = DateTime.Now;
                Res.IdWorkplacePay = Id;
                SetManualPay(Res);
            }
        }

        public void SetManualPay(Payment pPay)
        {
            pPay.SumPay = pPay.PosPaid = curReceipt.SumTotal;
            pPay.PosPaid = pPay.SumPay;
            pPay.NumberTerminal = "Manual";
            Bl.db.ReplacePayment(new List<Payment>() { pPay });

            curReceipt.StateReceipt = eStateReceipt.Pay;
            curReceipt.CodeCreditCard = pPay.NumberCard;
            curReceipt.NumberReceiptPOS = pPay.NumberReceipt;
            curReceipt.SumCreditCard = pPay.SumPay;
            Bl.db.ReplaceReceipt(curReceipt);
            curReceipt.Payment = new List<Payment>() { pPay };
        }

        private void Transfer1CButton(object sender, RoutedEventArgs e)
        {
            Bl.ds.SendReceiptTo1C(curReceipt);
            //MessageBox.Show("Передати в 1С");
        }

        private void ReturnCheckButton(object sender, RoutedEventArgs e)
        {
            CreateReturn();
        }

        void CreateReturn(bool pIsFull = false)
        {
            IsFullReturn = pIsFull;
            MW.TypeAccessWait = eTypeAccess.ReturnReceipt;

            if (!MW.SetConfirm(AdminUser?? MW?.AdminSSC, true))
                MW.SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ReturnReceipt, null);
        }

        //Якогось не працює через get як я хочу :) Тому пока реалізація через Ж.
        public bool IsPrintReceipt { get; set; } = false;// { get { return curReceipt?.StateReceipt == eStateReceipt.Pay; } }  //
        public bool IsPayReceipt { get; set; } = false;//{ get { return curReceipt?.StateReceipt == eStateReceipt.Prepare; } } // 
        public bool IsInputPay { get; set; } = false;// { get { return curReceipt?.StateReceipt == eStateReceipt.Prepare; } }
        public bool IsSendTo1C { get; set; } = false;// { get { return curReceipt?.StateReceipt == eStateReceipt.Print; } }
        public bool IsCreateReturn { get; set; } = false;// { get { return curReceipt?.StateReceipt == eStateReceipt.Send && curReceipt?.TypeReceipt == eTypeReceipt.Sale; } }

        private void ReturnAllCheckButton(object sender, RoutedEventArgs e)
        {
            CreateReturn(true);
        }

        private void FindChecksByDate(object sender, RoutedEventArgs e)
        {
            if (TabHistory.IsSelected)
            {
                //DateTime dt = dataHistori.SelectedDate.Value.Date;
                Receipts = new ObservableCollection<Receipt>(Bl.GetReceipts(DateSoSearch, DateSoSearch, Global.IdWorkPlace));
                ListReceipts.ItemsSource = Receipts.Reverse();
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListReceipts.ItemsSource);
                view.Filter = ReceiptFilter;
            }
            // Поки не знаю як реалізувати пошук

            RefreshJournal();
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
            //!!!TMP
            KeyPad keyPad = new KeyPad(MW);
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
                System.Diagnostics.Process.Start("explorer.exe");
                Application.Current.Shutdown();
            }
        }

        private void CloneReceipt(object sender, RoutedEventArgs e)
        {
            Bl.CreateRefund(curReceipt, true, false);
            MW.SetStateView(eStateMainWindows.WaitInput);
        }

        private void ShowDetailsReceiptClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var tmpReceipt = btn.DataContext as Receipt;
            if (tmpReceipt != null)
            {
                DetailsReceiptBorder.Visibility = Visibility.Visible;
                BackgroundReceipts.Visibility = Visibility.Visible;
                string fiscalCheckText = "", posCheckText = "";
                var TMPvalue = Bl.GetLogRRO(tmpReceipt);
                foreach (var elem in TMPvalue)
                {
                    if (elem.TypeOperation == (tmpReceipt.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.SalePOS : eTypeOperation.RefundPOS))
                    {
                        posCheckText += elem.TextReceipt;
                        posCheckText += $"{Environment.NewLine}@-@-@-@-@-@-@-@-@-@-@-@{Environment.NewLine}";
                    }
                    if (elem.TypeOperation == (tmpReceipt.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund))
                    {
                        fiscalCheckText += elem.TextReceipt;
                        fiscalCheckText += $"{Environment.NewLine}@-@-@-@-@-@-@-@-@-@-@-@-@-@{Environment.NewLine}";
                    }
                }
                PosCheckText.Text = posCheckText;//TMPvalue?.Where(e => e.TypeOperation == (tmpReceipt.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.SalePOS : eTypeOperation.RefundPOS)).FirstOrDefault()?.TextReceipt;
                FiscalCheckText.Text = fiscalCheckText;//TMPvalue?.Where(e => e.TypeOperation == (tmpReceipt.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund)).FirstOrDefault()?.TextReceipt;
                var curReceiptWares = Bl.GetWaresReceipt(tmpReceipt);
                ListWaresReceipt.ItemsSource = curReceiptWares;
            }

        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            RevisionText.Text = "";
            SettingPeriodZReport.Visibility = Visibility.Collapsed;
            Revision.Visibility = Visibility.Collapsed;
            BackgroundShift.Visibility = Visibility.Collapsed;
            DetailsReceiptBorder.Visibility = Visibility.Collapsed;
            BackgroundReceipts.Visibility = Visibility.Collapsed;
        }

        private void IsShowAllreceiptsChecked(object sender, RoutedEventArgs e)
        {
            if (Receipts != null)
            {
                IsShowAllReceipts = (bool)AllReceiptsCheckBox.IsChecked;
                IsShowDeferredReceipts = (bool)DeferredReceiptsCheckBox.IsChecked;
                CollectionViewSource.GetDefaultView(ListReceipts.ItemsSource).Refresh();
            }

        }

        private void Print(object sender, RoutedEventArgs e)
        {
            if (TextReceipt?.Any() == true)
            {
                try
                {
                    IdReceipt IdR = new() { CodePeriod = Global.GetCodePeriod(), IdWorkplace = Global.IdWorkPlace, IdWorkplacePay = Global.IdWorkPlace };
                    EF.PrintNoFiscalReceipt(IdR, TextReceipt);
                }
                finally { }
            }
        }

        private void ListJournalSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            JornalText.Text = "";
            SelectedListJournal = ListJournal.SelectedItem as LogRRO;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSelectedListJournal"));
            if (SelectedListJournal != null)
            {
                JornalText.Text = SelectedListJournal.TextReceipt != null ? SelectedListJournal.TextReceipt : SelectedListJournal.JSON;
            }
        }

        private void FindJournalDate(object sender, RoutedEventArgs e)
        {
            if (TabPrintJournal.IsSelected)
                RefreshJournal();

        }

        private void IsAllJournalChecked(object sender, RoutedEventArgs e)
        {
            if (SourcesListJournal != null)
            {
                IsShowAllJournal = (bool)AllListJornal.IsChecked;
                CollectionViewSource.GetDefaultView(ListJournal.ItemsSource).Refresh();
            }

        }

        private void CheckTypeMessage(object sender, RoutedEventArgs e)
        {
            RadioButton rbtn = sender as RadioButton;
            if (rbtn.DataContext is APIRadiobuton)
            {
                APIRadiobuton temp = rbtn.DataContext as APIRadiobuton;
                TypeAPIMessage = temp.ServerTypeMessage;
            }
        }

        private void SetdAPIMessage(object sender, RoutedEventArgs e)
        {
            KasaSocketClient socketKasaClient = new KasaSocketClient();
            string Response = socketKasaClient.SendMessage(APITextMessage.Text, TypeAPIMessage, "127.0.0.1", 8068);
            MessageBox.Show("Відповідь сервера: " + Response);
        }

        private void IsPeriodZClick(object sender, RoutedEventArgs e)
        {
            IsShortPeriodZ = (bool)PeriodZCheckBox.IsChecked;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsShortPeriodZ"));
        }

        private void EKKA_Z_Period_Show(object sender, RoutedEventArgs e)
        {
            SettingPeriodZReport.Visibility = Visibility.Visible;
            BackgroundShift.Visibility = Visibility.Visible;
            DetailsReceiptBorder.Visibility = Visibility.Visible;
            BackgroundReceipts.Visibility = Visibility.Visible;
        }

        private void ChoiceReceiptStatus(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            ChangeStateReceipt = btn.DataContext as Receipt;
            if (ChangeStateReceipt != null)
            {
                WindowChangeReceiptStatus.Visibility = Visibility.Visible;
                BackgroundReceipts.Visibility = Visibility.Visible;


                var ListStateReceiptRadiobuton = new ObservableCollection<StateReceiptRadiobuton>();
                foreach (eStateReceipt type in Enum.GetValues(typeof(eStateReceipt)))
                {
                    bool select_ = false;
                    if (type == ChangeStateReceipt.StateReceipt) select_ = true;
                    ListStateReceiptRadiobuton.Add(new StateReceiptRadiobuton()
                    {
                        StateReceipt_ = type,
                        Selected = select_

                    });

                }
                ListReceiptState.ItemsSource = ListStateReceiptRadiobuton;
            }
        }

        private void CancelChangeReceiptStatus(object sender, RoutedEventArgs e)
        {
            WindowChangeReceiptStatus.Visibility = Visibility.Collapsed;
            BackgroundReceipts.Visibility = Visibility.Collapsed;
        }

        private void ChangeReceiptStatus(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Змінити стан чека з \"{ChangeStateReceipt.StateReceipt.GetDescription()}\" на \"{newStateReceipt.GetDescription()}\"?", "Увага!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                //ChangeStateReceipt.StateReceipt = newStateReceipt;
                Bl.SetStateReceipt(ChangeStateReceipt, newStateReceipt);
                WindowChangeReceiptStatus.Visibility = Visibility.Collapsed;
                BackgroundReceipts.Visibility = Visibility.Collapsed;
            }
        }

        private void CheckReceiptState(object sender, RoutedEventArgs e)
        {
            RadioButton btn = sender as RadioButton;
            var selectedState = btn.DataContext as StateReceiptRadiobuton;
            if (selectedState.IsNotNull())
                newStateReceipt = selectedState.StateReceipt_;

        }

        private void PrintCoffeeQR(object sender, RoutedEventArgs e)
        {
            EF.PrintQR(curReceipt);
        }

        private void Check1C_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder Res = new();
            var MsSQL = new WDB_MsSql();
            var Receipts = new ObservableCollection<Receipt>(Bl.GetReceipts(DateSoSearch, DateSoSearch, Global.IdWorkPlace));
            decimal Sum1CTotal = 0, Sum = 0;
            Res.Append($"Звіт за {DateSoSearch} {Environment.NewLine}");
            foreach (var IdWP in Global.GetIdWorkPlaces)
            {
                var R1C = MsSQL.GetReceipt1C(DateSoSearch, IdWP.IdWorkplace);
                Sum = R1C.Sum(el => el.Value);
                Sum1CTotal += Sum;
                Res.Append($"Всього 1С => {IdWP.Name}-{Sum}{Environment.NewLine}");

                foreach (var el in Receipts)
                {
                    decimal SumPr = 0, Sum1c = 0;
                    SumPr = el.WorkplacePays?.Where(e => e.IdWorkplacePay == IdWP.IdWorkplace)?.Sum(e => e.Sum) ?? 0m;
                    if (SumPr > 0)
                    {
                        if (R1C.ContainsKey(el.NumberReceipt1C))
                        {

                            try { Sum1c = R1C[el.NumberReceipt1C]; } catch (Exception) { }
                            SumPr = el.WorkplacePays?.Where(e => e.IdWorkplacePay == IdWP.IdWorkplace)?.Sum(e => e.Sum) ?? 0m;
                            if (SumPr > 0 && Math.Abs(SumPr - Sum1c) > 0.01m)
                                Res.Append($"{el.NumberReceipt1C} Сума чека:{SumPr} В 1с:{Sum1c}{Environment.NewLine}");
                        }
                        else
                        {
                            if (el.StateReceipt >= eStateReceipt.Pay)
                            {
                                Res.Append($"{el.NumberReceipt1C} Відсутній чек в 1С на суму {el.SumTotal:n2} {el.StateReceipt}{Environment.NewLine}");
                            }
                        }
                    }
                }
            }
            Res.Append($"Всього 1С => {Sum1CTotal}{Environment.NewLine}");
            Sum = Receipts.Where(el => el.StateReceipt >= eStateReceipt.Pay).Sum(el => el.SumTotal);
            Res.Append($"Всього Програма => {Sum}{Environment.NewLine}");
            Sum = Receipts.Where(el => el.StateReceipt >= eStateReceipt.Pay && el.StateReceipt < eStateReceipt.Send).Sum(el => el.SumTotal);
            Res.Append($"Всього в проміжних станах => {Sum}{Environment.NewLine}");

            MessageBox.Show(Res.ToString());
        }

        private void CheckWorkPlaceId(object sender, RoutedEventArgs e)
        {
            RadioButton ChBtn = sender as RadioButton;
            if (ChBtn.DataContext is WorkPlace)
            {
                WorkPlace temp = ChBtn.DataContext as WorkPlace;
                if (ChBtn.IsChecked == true)
                {
                    SelectedWorkPlace = temp;
                    foreach (var workPlace in WorkPlaces)
                        workPlace.IsChoice = workPlace.IdWorkplace == temp.IdWorkplace;

                }

            }
        }

        private void PrintSelectRecript(object sender, RoutedEventArgs e)
        {
            if (curReceipt != null)
            {
                IEnumerable<string> ArrayFiscalLine = null;
                var TMPvalue = Bl.GetLogRRO(curReceipt);
                foreach (var elem in TMPvalue)
                {
                    if (elem.TypeOperation == (curReceipt.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund))
                    {
                        ArrayFiscalLine = elem.TextReceipt.Split(Environment.NewLine);
                    }
                }
                try
                {
                    IdReceipt IdR = new() { CodePeriod = Global.GetCodePeriod(), IdWorkplace = Global.IdWorkPlace, IdWorkplacePay = Global.IdWorkPlace };
                    EF.PrintNoFiscalReceipt(IdR, ArrayFiscalLine);
                }
                finally { }
            }
        }

        private void PrintSelectRecriptJornal(object sender, RoutedEventArgs e)
        {
            if (SelectedListJournal != null)
            {
                IEnumerable<string> ArrayFiscalLine = null;


                ArrayFiscalLine = SelectedListJournal.TextReceipt.Split(Environment.NewLine);


                try
                {
                    IdReceipt IdR = new() { CodePeriod = Global.GetCodePeriod(), IdWorkplace = Global.IdWorkPlace, IdWorkplacePay = Global.IdWorkPlace };
                    EF.PrintNoFiscalReceipt(IdR, ArrayFiscalLine);
                }
                finally { }
            }
        }

        private void RefreshLogButton(object sender, RoutedEventArgs e)
        {
            RefreshLog();
        }

        private void EKKA_MoveMoney_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string DesciptionOparation;
            bool IsRemoveMoney;
            if (btn.Name == "EKKA_Introduction")
            {
                DesciptionOparation = "Службове внесення";
                IsRemoveMoney = false;
            }
            else
            {
                DesciptionOparation = "Службове вилучення";
                IsRemoveMoney = true;
            }

            MW.InputNumberPhone.Desciption = DesciptionOparation;
            MW.InputNumberPhone.ValidationMask = "";
            MW.InputNumberPhone.Result = "";
            MW.InputNumberPhone.IsEnableComma = true;
            MW.InputNumberPhone.CallBackResult = (string res) => AddOrRemoveMoney(res, IsRemoveMoney, DesciptionOparation);
            MW.NumericPad.Visibility = Visibility.Visible;
            BackgroundShift.Visibility = Visibility.Visible;

        }

        private void AddOrRemoveMoney(string pRes, bool IsRemoveMoney, string pDesciption)
        {
            if (pRes.Length != 0)
            {
                if (MessageBox.Show($"{pDesciption} {pRes}?", "Увага!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var res = Decimal.TryParse(pRes, out decimal pSumMoveMoney);
                    if (res)
                    {
                        if (IsRemoveMoney)
                            pSumMoveMoney = pSumMoveMoney * (-1); // для вилучення відємне значення
                        var task = Task.Run(() =>
                        {
                            var r = EF.RroMoveMoney(pSumMoveMoney, new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = SelectedWorkPlace.IdWorkplace });
                            if (r.CodeError == 0)
                                MessageBox.Show("Успішно!");
                            else
                            {
                                Thread.Sleep(100);
                                MW.ShowErrorMessage($"Помилка: ({r.CodeError}){Environment.NewLine}{r.Error}");
                            }
                        });
                    }
                    else MW.ShowErrorMessage("Введіть коректну суму!");
                }
            }
            MW.NumericPad.Visibility = Visibility.Collapsed;
            BackgroundShift.Visibility = Visibility.Collapsed;
        }

        private void CheckTypeTerminal(object sender, RoutedEventArgs e)
        {
            RadioButton ChBtn = sender as RadioButton;
            if (ChBtn.DataContext is Equipment)
            {
                BankTerminal temp = ChBtn.DataContext as BankTerminal;
                if (temp != null)
                {
                    MW?.EF.SetBankTerminal(temp);
                }
            }
        }

        private void RestoreSelectRecript(object sender, RoutedEventArgs e)
        {
            MW.Client = null;
            MW.SetCurReceipt(Bl.GetReceiptHead(curReceipt, true));
            MW.SetStateView(eStateMainWindows.WaitInput);
        }
    }

    public class APIRadiobuton
    {
        public eTypeMessage ServerTypeMessage { get; set; }
        public string TranslateServerTypeMessage { get { return ServerTypeMessage.GetDescription(); } }
    }

    public class ParsLog
    {
        public string LineLog { get; set; }
        public eTypeLog TypeLog { get; set; }
    }
    public class StateReceiptRadiobuton
    {
        public eStateReceipt StateReceipt_ { get; set; }
        public string TranslateStateReceipt_ { get { return StateReceipt_.GetDescription(); } }
        public bool Selected { get; set; } = false;
    }




}
