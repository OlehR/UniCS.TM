using Front.Equipments;
using Front.Models;
using ModelMID;
using ModelMID.DB;
using ModernExpo.SelfCheckout.Utils;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using UtilNetwork;
using Utils;

namespace Front.Control
{
    /// <summary>
    /// Interaction logic for Admin.xaml
    /// </summary>
    public partial class Admin_control : UserControl, INotifyPropertyChanged
    {
        Action<eCommand, WorkPlace, Status> OnSocket;
        public Access Access = Access.GetAccess();
        public event PropertyChangedEventHandler PropertyChanged;
        EquipmentFront EF;
        ObservableCollection<Receipt> Receipts;
        ObservableCollection<ParsLog> LogsCollection;
        ObservableCollection<LogRRO> SourcesListJournal;
        ObservableCollection<Equipment> AllEquipment = new ObservableCollection<Equipment>();
        ObservableCollection<Banknote> Banknotes = new ObservableCollection<Banknote>();

        public double TotalMoneyCounting
        {
            get
            {
                double sum = 0d;
                foreach (var item in Banknotes)
                {
                    sum += item.MonetarySum;
                }
                return sum;
            }
        }
        public Banknote SelectedBanknote { get; set; } = new();
        public string TypeLog { get; set; } = "Error";
        public string OrderNumber { get; set; } = "0";
        BL Bl;
        public string ControlScaleWeightDouble { get; set; } = "0";
        MainWindow MW;
        public Receipt curReceipt = null;

        LogRRO SelectedListJournal = null;
        public bool IsSelectedListJournal { get { return SelectedListJournal != null; } }
        Receipt ChangeStateReceipt = null;
        public eStateReceipt newStateReceipt;
        public bool ClosedShift { get { return MW.IsLockSale; } }
        public User AdminUser { get; set; }
        public SpeedScan SpeedScan { get { return Bl.db.SpeedScan(); } }
        public string NameAdminUserOpenShift { get { return $"{MW?.AdminSSC?.NameUser} ({MW?.AdminSSC?.TypeUser})"; } }
        public DateTime DataOpenShift { get { return MW.DTAdminSSC; } }
        public bool IsShowAllReceipts { get; set; }
        public int FindReceiptByNumber { get; set; } = 0;
        public bool IsShowAllJournal { get; set; }
        //BatchTotals LastReceipt = null;

        public bool IsCashRegister { get { return Global.TypeWorkplaceCurrent == eTypeWorkplace.CashRegister; } }

        public DateTime DateSoSearch { get; set; } = DateTime.Now.Date;
        public DateTime DateStartPeriodZ { get; set; } = DateTime.Now.Date;
        public DateTime DateEndPeriodZ { get; set; } = DateTime.Now.Date;
        public eCommand TypeAPIMessage { get; set; }
        public string KasaNumber { get { return Global.GetWorkPlaceByIdWorkplace(Global.IdWorkPlace).Name; } }
        public ObservableCollection<APIRadiobuton> TypeMessageRadiobuton { get; set; }
        public bool IsShortPeriodZ { get; set; } = true;
        public bool IsPrintCoffeQR { get; set; } = false;
        public bool IsСurReceipt { get; set; } = false;
        public bool IsDeferredReceipt { get; set; } = false;
        public bool IsUseOldDB { get; set; } = false;
        public IEnumerable<WorkPlace> WorkPlaces { get { return Global.GetIdWorkPlaces; } }
        WorkPlace _SelectedWorkPlace = null;
        public WorkPlace SelectedWorkPlace { get { return _SelectedWorkPlace != null ? _SelectedWorkPlace : WorkPlaces.First(); } set { _SelectedWorkPlace = value; } }
        public Rro SelectedCashRegister { get; set; }
        ObservableCollection<BankTerminal> ActiveTerminals = new();
        IEnumerable<string> TextReceipt;
        public StringBuilder SB { get; set; } = new();
        public IEnumerable<WorkPlace> ActiveWorkPlaces { get; set; }
        public bool IsControlScale { get { return AllEquipment.Where(x => x.Type == eTypeEquipment.ControlScale).Count() > 0; } }
        public eTypeWorkplace eTypeWorkplaceForXaml { get; set; } = Global.TypeWorkplaceCurrent;
        ObservableCollection<CurentTypeWorkplace> ListTypeWorkplace = new ObservableCollection<CurentTypeWorkplace>();
        eTypeWorkplace curWorkplace = Global.TypeWorkplaceCurrent == eTypeWorkplace.Both ? eTypeWorkplace.SelfServicCheckout : Global.TypeWorkplaceCurrent;

        public void ControlScale(double pWeight, bool pIsStable)
        {
            ControlScaleWeightDouble = $"{(pWeight / 1000):N3}";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ControlScaleWeightDouble"));
        }

        public Admin_control()
        {
            try
            {
                OnSocket += (Command, WorkPlace, Ansver) =>
                {
                    SB.AppendLine($"{Command} {WorkPlace.Name} {Ansver.TextState}");
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SB)));
                };
                Bl = BL.GetBL;
                TypeMessageRadiobuton = new ObservableCollection<APIRadiobuton>();
                foreach (eCommand item in Enum.GetValues(typeof(eCommand)))
                {
                    TypeMessageRadiobuton.Add(new APIRadiobuton() { ServerTypeMessage = item });
                }
                Init(AdminUser);


                InitializeComponent();
                CreateBanknote();
                ComboBoxWorkPlaces.ItemsSource = WorkPlaces;

                this.DataContext = this;
                //Список команд для віддаленого керування
                //ListRadioButtonAPI.ItemsSource = TypeMessageRadiobuton;

                ActiveWorkPlaces = Bl.db.GetWorkPlace().Where(el => el.CodeWarehouse == Global.CodeWarehouse);


                foreach (eTypeWorkplace type in Enum.GetValues(typeof(eTypeWorkplace)))
                {
                    if (type != eTypeWorkplace.NotDefine && type != eTypeWorkplace.Both)
                    {
                        ListTypeWorkplace.Add(new CurentTypeWorkplace()
                        {
                            TypeWorkplace_ = type,

                        });
                    }
                }
                ComboBoxChooseTypeCheckout.ItemsSource = ListTypeWorkplace;
                RefreshJournal();
            }
            catch (Exception ex) { FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex); }

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsControlScale"));



            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                int i = 0;
                while (!EF.IsFinishInit && i++ < 200)
                {
                    Thread.Sleep(100);
                }

                try
                {
                    ComboBoxChooseTypeCheckout.SelectedItem = ListTypeWorkplace.First(x => x.TypeWorkplace_ == curWorkplace);
                    if (EF != null && EF.GetListEquipment?.Count() > 0)
                    {
                        AllEquipment = new ObservableCollection<Equipment>(EF.GetListEquipment);
                        ListEquipment.ItemsSource = AllEquipment;
                        var RROs = AllEquipment.Where(x => x.Type == eTypeEquipment.RRO);
                        ComboBoxCashRegisters.ItemsSource = RROs;
                        if (RROs.Count() > 0)
                        {
                            SelectedCashRegister = RROs.First() as Rro;
                            ComboBoxCashRegisters.SelectedItem = SelectedCashRegister;
                        }

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
            bool isFirst = true;
            foreach (BankTerminal item in EF.GetListEquipment.Where(el => el is BankTerminal)?.Select(el => el as BankTerminal))
            {
                item.IsSelected = isFirst;
                ActiveTerminals.Add(item);
                if (isFirst)
                {
                    MW?.EF.SetBankTerminal(item);
                }
                isFirst = false;
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
            if (IsShowAllReceipts && FindReceiptByNumber == 0)
            {
                return true;
            }
            else if (FindReceiptByNumber > 0)
            {
                return ((item as Receipt).CodeReceipt == FindReceiptByNumber);
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



        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MW.SetStateView(eStateMainWindows.StartWindow);
            TabAdmin.SelectedIndex = 0;
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
            MW.CustomMessage.Show("Ви хочете зробити Z-звіт на банківському терміналі?", "Увага!", eTypeMessage.Question);
            MW.CustomMessage.Result = (bool res) =>
            {
                if (res)
                {
                    var task = Task.Run(() =>
                    {
                        var LastReceipt = EF.PosPrintZ(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = SelectedWorkPlace.IdWorkplace });
                        ViewReceipt(LastReceipt.Receipt);
                    });
                }
            };

            //if (MessageBox.Show("Ви хочете зробити Z-звіт на банківському терміналі?", "Увага!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            //{
            //    var task = Task.Run(() =>
            //    {
            //    var LastReceipt = EF.PosPrintZ(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = SelectedWorkPlace.IdWorkplace });
            //    ViewReceipt(LastReceipt.Receipt);
            //    });
            //}
        }
        private void OffLineClick(object sender, RoutedEventArgs e)
        {
            MW.Bl.ds.IsUseOldDB = !MW.Bl.ds.IsUseOldDB;
            IsUseOldDB = MW.Bl.ds.IsUseOldDB;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsUseOldDB"));

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
            if (pLastReceipt.TextReceipt != null)
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
                var r = EF.RroPrintX(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(DateSoSearch), IdWorkplacePay = SelectedWorkPlace.IdWorkplace }, SelectedCashRegister);
                if (r.CodeError == 0)
                    ViewReceiptFiscal(r);
                else
                {
                    Thread.Sleep(100);
                    MW.CustomMessage.Show($"Помилка друку звіта:({r.CodeError}){Environment.NewLine}{r.Error}", "Помилка!", eTypeMessage.Error);
                    //MW.ShowErrorMessage($"Помилка друку звіта:({r.CodeError}){Environment.NewLine}{r.Error}");
                }
            });
        }

        private void EKKA_Z_Click(object sender, RoutedEventArgs e)
        {
            if ((MW.curReceipt == null || MW.curReceipt.Wares == null || !MW.curReceipt.Wares.Any()) && (MW.ReceiptPostpone == null || MW.ReceiptPostpone.Wares == null || !MW.ReceiptPostpone.Wares.Any()))
            {


                MW.CustomMessage.Show("Ви хочете зробити Z - звіт на фіскальному апараті ? ", "Увага!", eTypeMessage.Question);
                MW.CustomMessage.Result = (bool res) =>
                {
                    if (res)
                    {
                        var task = Task.Run(() =>
                        {
                            var r = EF.RroPrintZ(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(DateSoSearch), IdWorkplacePay = SelectedWorkPlace.IdWorkplace }, SelectedCashRegister);
                            if (r.CodeError == 0)
                                ViewReceiptFiscal(r);
                            else
                            {
                                Thread.Sleep(100);
                                MW.CustomMessage.Show($"Помилка друку звіта:({r.CodeError}){Environment.NewLine}{r.Error}", "Помилка!", eTypeMessage.Error);
                                //MW.ShowErrorMessage($"Помилка друку звіта:({r.CodeError}){Environment.NewLine}{r.Error}");
                            }
                        });
                    }
                };
                //if (MessageBox.Show("Ви хочете зробити Z-звіт на фіскальному апараті?", "Увага!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                //{
                //    var task = Task.Run(() =>
                //{
                //    var r = EF.RroPrintZ(new IdReceipt() { IdWorkplace = SelectedWorkPlace.IdWorkplace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = SelectedWorkPlace.IdWorkplace });
                //    if (r.CodeError == 0)
                //        ViewReceiptFiscal(r);
                //    else
                //    {
                //        Thread.Sleep(100);
                //        MW.ShowErrorMessage($"Помилка друку звіта:({r.CodeError}){Environment.NewLine}{r.Error}");
                //    }
                //});
                //}
            }
            else
            {
                MW.SetStateView(eStateMainWindows.StartWindow);
                TabAdmin.SelectedIndex = 0;
                MW.CustomMessage.Show("Існує відкритий чек! Для закриття зміни потрібно закрити всі чеки!", "Увага!", eTypeMessage.Warning);
                //MW.ShowErrorMessage("Існує відкритий чек! Для закриття зміни потрібно закрити всі чеки!");
            }
        }

        private void EKKA_Z_Period_Click(object sender, RoutedEventArgs e)
        {
            var task = Task.Run(() =>
            {
                var tmpReceipt = new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = SelectedWorkPlace.IdWorkplace };
                var r = EF.RroPeriodZReport(tmpReceipt, DateStartPeriodZ, DateEndPeriodZ, !IsShortPeriodZ, SelectedCashRegister);
            });
        }

        private void EKKA_Copy_Click(object sender, RoutedEventArgs e)
        {
            EF.RroPrintCopyReceipt(SelectedCashRegister);
        }

        private void WorkStart_Click(object sender, RoutedEventArgs e)
        {
            OpenShift(AdminUser);
        }

        public void OpenShift(User pU)
        {
            MW.AdminSSC = pU;
            if (Global.TypeWorkplace == eTypeWorkplace.CashRegister)
                MW.Access.СurUser = pU;
            MW.DTAdminSSC = DateTime.Now;
            MW.Bl.db.SetConfig<DateTime>("DateAdminSSC", DateTime.Now);
            MW.Bl.db.SetConfig<string>("CodeAdminSSC", pU.BarCode);
            MW.Bl.StartWork(Global.IdWorkPlace, pU.BarCode);
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClosedShift"));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NameAdminUserOpenShift"));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DataOpenShift"));
            OpenShiftShow.Visibility = Visibility.Visible;
            Init();
            if (MW.State == eStateMainWindows.WaitAdmin)
                MW.SetStateView(eStateMainWindows.StartWindow);
        }

        private void WorkFinish_Click(object sender, RoutedEventArgs e)
        {
            if ((MW.curReceipt == null || MW.curReceipt.Wares == null || !MW.curReceipt.Wares.Any()) && (MW.ReceiptPostpone == null || MW.ReceiptPostpone.Wares == null || !MW.ReceiptPostpone.Wares.Any()))
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
            else
            {
                MW.SetStateView(eStateMainWindows.StartWindow);
                TabAdmin.SelectedIndex = 0;
                MW.CustomMessage.Show("Існує відкритий чек! Для закриття зміни потрібно закрити всі чеки!", "Увага!", eTypeMessage.Warning);
                //MW.ShowErrorMessage("Існує відкритий чек! Для закриття зміни потрібно закрити всі чеки!");
            }
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
                    MW.CustomMessage.Show($"{res.StateEquipment} {res.TextState}", res.ModelEquipment.ToString(), eTypeMessage.Information);
                    // MessageBox.Show($"{res.StateEquipment} {res.TextState}", res.ModelEquipment.ToString());
                    Init(AdminUser);
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
                    MW.CustomMessage.Show(res, Eq.Model.ToString(), eTypeMessage.Information);
                    //MessageBox.Show(res, Eq.Model.ToString());
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
            var tabControl = (sender as TabControl).SelectedItem;
            string tabItem = tabControl.IsNotNull() ? (tabControl as TabItem).Header as string : "Зміна";

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
                    Exit_Click(null, null);
                    break;
                case "Вихід з програми":
                    Exit_Program_Click(null, null);
                    break;
                case "Додаткові функції":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SpeedScan)));
                    break;
                default:

                    return;
            }
        }

        private void RefreshJournal()
        {
            try
            {
                var TMPIdRecipt = new IdReceipt { CodePeriod = Global.GetCodePeriod(DateSoSearch), CodeReceipt = 0, IdWorkplace = Global.GetWorkPlaceByIdWorkplace(Global.IdWorkPlace).IdWorkplace };
                SourcesListJournal = new ObservableCollection<LogRRO>(Bl.GetLogRRO(TMPIdRecipt));
                ListJournal.ItemsSource = SourcesListJournal.Reverse();
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListJournal.ItemsSource);
                view.Filter = JournalFilter;
            }
            catch (Exception ex) { FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex); }
        }

        private void RefreshLog()
        {
            var PathToFileLog = FileLogger.GetFileNameDate(DateSoSearch);
            if (!File.Exists(PathToFileLog))
            {
                MW.CustomMessage.Show($"За обраною датою: {DateSoSearch.ToString("dd/MM/yyyy")} лог відсутній", "Увага!", eTypeMessage.Warning);
                //MessageBox.Show($"За обраною датою: {DateSoSearch.ToString("dd/MM/yyyy")} лог відсутній");
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
                curReceipt.IdWorkplacePay = Global.IdWorkPlace;
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
            Bl.db.ReplacePayment(pPay);

            curReceipt.StateReceipt = eStateReceipt.Pay;
            curReceipt.CodeCreditCard = pPay.NumberCard;
            curReceipt.NumberReceiptPOS = pPay.NumberReceipt;
            curReceipt.SumCreditCard = pPay.SumPay;
            Bl.db.ReplaceReceipt(curReceipt);
            curReceipt.Payment = new List<Payment>() { pPay };
        }

        private void Transfer1CButton(object sender, RoutedEventArgs e)
        {
            Bl.SendReceiptTo1C(curReceipt);
            //MessageBox.Show("Передати в 1С");
        }

        private void ReturnCheckButton(object sender, RoutedEventArgs e)
        {
            CreateReturn();
        }

        private void ReturnAllCheckButton(object sender, RoutedEventArgs e)
        {
            CreateReturn(true);
        }

        void CreateReturn(bool pIsFull = false)
        {
            MW.IsFullReturn = pIsFull;
            MW.TypeAccessWait = eTypeAccess.ReturnReceipt;

            if (!MW.SetConfirm(AdminUser ?? MW?.AdminSSC, true))
                MW.SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ReturnReceipt, null);
        }

        //Якогось не працює через get як я хочу :) Тому пока реалізація через Ж.
        public bool IsPrintReceipt { get; set; } = false;// { get { return curReceipt?.StateReceipt == eStateReceipt.Pay; } }  //
        public bool IsPayReceipt { get; set; } = false;//{ get { return curReceipt?.StateReceipt == eStateReceipt.Prepare; } } // 
        public bool IsInputPay { get; set; } = false;// { get { return curReceipt?.StateReceipt == eStateReceipt.Prepare; } }
        public bool IsSendTo1C { get; set; } = false;// { get { return curReceipt?.StateReceipt == eStateReceipt.Print; } }
        public bool IsCreateReturn { get; set; } = false;// { get { return curReceipt?.StateReceipt == eStateReceipt.Send && curReceipt?.TypeReceipt == eTypeReceipt.Sale; } }



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
            MW.CustomMessage.Show("Вимкнути касу?", "Увага!", eTypeMessage.Question);
            MW.CustomMessage.Result = (bool res) =>
            {
                if (res)
                    System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0");

            };
            //if (MessageBox.Show("Вимкнути касу?", "Увага!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            //{
            //    System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0");
            //}
        }

        private void RebootPC(object sender, RoutedEventArgs e)
        {
            MW.CustomMessage.Show("Перезавантажити касу?", "Увага!", eTypeMessage.Question);
            MW.CustomMessage.Result = (bool res) =>
            {
                if (res)
                    System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");

            };
            //if (MessageBox.Show("Перезавантажити касу?", "Увага!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            //{
            //    System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
            //}
        }

        private void RefreshDataButton(object sender, RoutedEventArgs e)
        {
            MW.CustomMessage.Show($"Запустити повне оновлення бази даних?{Environment.NewLine}{Bl.db.LastMidFile} {Bl.db.GetConfig<DateTime>("Load_Full")}", "Оновлення бази даних", eTypeMessage.Question);
            MW.CustomMessage.Result = (bool res) =>
            {
                if (res)
                {
                    Task.Run(() => Bl.ds.SyncDataAsync(true));
                }
            };
            
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
            MW.CustomMessage.Show("Завершити роботу програми?", "Увага!", eTypeMessage.Question);
            MW.CustomMessage.Result = (bool res) =>
            {
                if (res)
                {
                    if (!Global.IsTest)
                        System.Diagnostics.Process.Start("explorer.exe");
                    Application.Current.Shutdown();
                }
            };
            
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

        private void SendAPIMessage(object sender, RoutedEventArgs e)
        {

            //var aa = Global.AllWorkPlaces;
            //Task.Run(async () =>
            //{
            //    SocketClient Client = new("", Global.PortAPI);
            //    string Response = await Client.StartAsync(APITextMessage.Text);
            //    MessageBox.Show("Відповідь сервера: " + Response);
            //});
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
                    if (type == eStateReceipt.Canceled || type == eStateReceipt.Pay || type == eStateReceipt.Prepare || type == eStateReceipt.Print)
                    {
                        if (type == ChangeStateReceipt.StateReceipt) select_ = true;
                        ListStateReceiptRadiobuton.Add(new StateReceiptRadiobuton()
                        {
                            StateReceipt_ = type,
                            Selected = select_

                        });
                    }
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
            MW.CustomMessage.Show($"Змінити стан чека з \"{ChangeStateReceipt.StateReceipt.GetDescription()}\" на \"{newStateReceipt.GetDescription()}\"?", "Увага!", eTypeMessage.Question);
            MW.CustomMessage.Result = (bool res) =>
            {
                if (res)
                {
                    Bl.SetStateReceipt(ChangeStateReceipt, newStateReceipt);
                    WindowChangeReceiptStatus.Visibility = Visibility.Collapsed;
                    BackgroundReceipts.Visibility = Visibility.Collapsed;
                    FindChecksByDate(null, null);
                    var R = Bl.GetReceiptHead(MW.curReceipt, true);
                    if (R.StateReceipt == eStateReceipt.Canceled)
                        MW.NewReceipt();
                    else
                        Global.OnReceiptCalculationComplete?.Invoke(R);

                    MW.SetPropertyChanged();
                }

            };
            //if (MessageBox.Show($"Змінити стан чека з \"{ChangeStateReceipt.StateReceipt.GetDescription()}\" на \"{newStateReceipt.GetDescription()}\"?", "Увага!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            //{
            //    //ChangeStateReceipt.StateReceipt = newStateReceipt;
            //    Bl.SetStateReceipt(ChangeStateReceipt, newStateReceipt);
            //    WindowChangeReceiptStatus.Visibility = Visibility.Collapsed;
            //    BackgroundReceipts.Visibility = Visibility.Collapsed;
            //    FindChecksByDate(null, null);
            //}
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
            var Receipts = Bl.GetReceipts(DateSoSearch, DateSoSearch, Global.IdWorkPlace);

            decimal Sum1CTotal = 0, Sum = 0;
            Res.Append($"Звіт за {DateSoSearch} {Environment.NewLine}");

            decimal Total = 0;// Receipts.Where(el => el.StateReceipt >= eStateReceipt.Pay).Sum(el => el.SumTotal);



            foreach (var IdWP in Global.GetIdWorkPlaces)
            {
                Total = 0;
                var R1C = MsSQL.GetReceipt1C(DateSoSearch, IdWP.IdWorkplace);
                Sum = R1C.Sum(el => el.Value);
                Sum1CTotal += Sum;
                Res.Append($"Всього 1С => {IdWP.Name}-{Sum}{Environment.NewLine}");

                foreach (var el in Receipts.Where(el => el.StateReceipt >= eStateReceipt.Pay))
                {
                    var r = Bl.GetReceiptHead(el, true);
                    Total += r.SumTotal;
                    MW.FillPays(r);
                    decimal SumPr = 0, Sum1c = 0;
                    SumPr = r.WorkplacePays?.Where(e => e.IdWorkplacePay == IdWP.IdWorkplace)?.Sum(e => e.Sum) ?? 0m;
                    if (SumPr > 0)
                    {
                        if (R1C.ContainsKey(r.NumberReceipt1C))
                        {
                            try { Sum1c = R1C[r.NumberReceipt1C]; } catch (Exception ex) { Res.Append($"{r.NumberReceipt1C} {ex.Message}"); }

                            SumPr = r.WorkplacePays?.Where(e => e.IdWorkplacePay == IdWP.IdWorkplace)?.Sum(e => e.Sum) ?? 0m;
                            if (SumPr > 0 && Math.Abs(SumPr - Sum1c) > 0.00m)
                                Res.Append($"{r.NumberReceipt1C} Сума чека:{SumPr} В 1с:{Sum1c}{Environment.NewLine}");
                        }
                        else
                        {
                            if (r.StateReceipt >= eStateReceipt.Pay)
                            {
                                Res.Append($"{r.NumberReceipt1C} Відсутній чек в 1С на суму {r.SumTotal:n2} {r.StateReceipt}{Environment.NewLine}");
                            }
                        }
                    }
                }
                Res.Append($"Програма {IdWP.CodeWarehouse} Total={Total}{Environment.NewLine}");

                foreach (var el in R1C)
                {
                    if (!Receipts.Any(e => e.NumberReceipt1C.Equals(el.Key)))
                        Res.Append($"{el.Key} Відсутній чек в базі на суму {el.Value:n2} {Environment.NewLine}");
                }
            }
            Res.Append($"Всього 1С => {Sum1CTotal}{Environment.NewLine}");

            Sum = Receipts.Where(el => el.StateReceipt >= eStateReceipt.Pay).Sum(el => el.SumTotal);
            Res.Append($"Всього Програма => {Sum}{Environment.NewLine}");
            Sum = Receipts.Where(el => el.StateReceipt >= eStateReceipt.Pay && el.StateReceipt < eStateReceipt.Send).Sum(el => el.SumTotal);
            Res.Append($"Всього в проміжних станах => {Sum}{Environment.NewLine}");

            string SQL = @"select ""№""||Pay.code_receipt ||"" Delta=>""||round(Pay.sum-R.Sum,2)|| "" SumPay=>""|| Pay.sum ||"" SumR=>""||R.Sum as res from 
--(select  code_receipt,json_extract(json,'$.SumPay') as sum from LOG_RRO where TYPE_OPERATION=20100) as Pay
(select CODE_RECEIPT,sum_pay as sum from payment where TYPE_PAY in(1,2) )  as Pay
left join 
(
select code_receipt,TEXT_RECEIPT, 
0.00+replace( replace( substring(TEXT_RECEIPT,instr(TEXT_RECEIPT,'С У М А        ')+8,24),' ',''),',','.')+replace( replace( substring(TEXT_RECEIPT,instr(TEXT_RECEIPT,'ЗАОКРУГЛЕННЯ  ')+12,24),' ',''),',','.')  as sum  
from LOG_RRO where TYPE_OPERATION=0 and instr(TEXT_RECEIPT,'С У М А        ')>0) R
on pay.code_receipt=R.code_receipt
where round(Pay.sum-R.Sum,2) >0
order by Pay.code_receipt";

            using var db = new WDB_SQLite(DateSoSearch);
            var ss = db.db.Execute<string>(SQL);
            if (ss.Any())
            {
                Res.AppendLine("Чеки з розбіжностями в олаті");
                foreach (var el in ss)
                    Res.AppendLine(el);
            }

            SQL = @"select '№'||p.code_RECEIPT || ' Фіскальний=>'||p.sum_pay||'  Програма=>'|| round(  r.SUM_RECEIPt-r.SUM_DISCOUNT-r.sum_bonus-coalesce(pr.SUM_PAY,0),2) as sum_r--,r.SUM_RECEIPT,r.SUM_BONUS,r.SUM_DISCOUNT
from RECEIPT r
 left join PAYMENT p on r.code_receipt=p.code_receipt and p.type_pay=7
  left join PAYMENT pr on r.code_receipt=pr.code_receipt and pr.type_pay=5
 where  round( p.sum_pay,2)<>round(  r.SUM_RECEIPt-r.SUM_DISCOUNT-r.sum_bonus-coalesce(pr.SUM_PAY,0),2)
 and r.state_receipt=9";
            ss = db.db.Execute<string>(SQL);
            if (ss.Any())
            {
                Res.AppendLine("Чеки з розбіжностями фіскалки і програми");
                foreach (var el in ss)
                    Res.AppendLine(el);
            }

            MW.CustomMessage.Show(Res.ToString(), "Звірка з 1С", eTypeMessage.Information);
            //MessageBox.Show(Res.ToString());

        }


        private void PrintSelectRecript(object sender, RoutedEventArgs e)
        {
            if (curReceipt != null)
            {
                IEnumerable<string> ArrayFiscalLine = null;
                var TMPvalue = Bl.GetLogRRO(curReceipt);
                foreach (var elem in TMPvalue)
                {
                    if (elem.TypeOperation == (curReceipt.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund) && !string.IsNullOrEmpty(elem.TextReceipt))
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
                MW.CustomMessage.Show($"{pDesciption} {pRes}?", "Увага!", eTypeMessage.Question);
                MW.CustomMessage.Result = (bool resMes) =>
                {
                    if (resMes)
                    {
                        decimal pSumMoveMoney = pRes.ToDecimal();
                        //pRes = pRes.Replace(",", ".");
                        //var res = Decimal.TryParse(pRes, out decimal pSumMoveMoney);
                        if (pSumMoveMoney > 0)
                        {
                            if (IsRemoveMoney)
                                pSumMoveMoney = pSumMoveMoney * (-1); // для вилучення відємне значення
                            var task = Task.Run(() =>
                            {
                                var r = EF.RroMoveMoney(pSumMoveMoney, new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = SelectedWorkPlace.IdWorkplace }, SelectedCashRegister);
                                if (r.CodeError == 0)
                                    MW.CustomMessage.Show("Успішно!", "Операції з готівкою", eTypeMessage.Information);
                                //MessageBox.Show("Успішно!");
                                else
                                {
                                    Thread.Sleep(100);
                                    MW.CustomMessage.Show($"Помилка: ({r.CodeError}){Environment.NewLine}{r.Error}", "Помилка!", eTypeMessage.Error);

                                    //MW.ShowErrorMessage($"Помилка: ({r.CodeError}){Environment.NewLine}{r.Error}");
                                }
                            });
                        }
                        else MW.CustomMessage.Show($"Введіть коректну суму!", "Помилка!", eTypeMessage.Error);
                        //MW.ShowErrorMessage("Введіть коректну суму!");
                    }

                };
                //if (MessageBox.Show($"{pDesciption} {pRes}?", "Увага!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                //{
                //    var res = Decimal.TryParse(pRes, out decimal pSumMoveMoney);
                //    if (res)
                //    {
                //        if (IsRemoveMoney)
                //            pSumMoveMoney = pSumMoveMoney * (-1); // для вилучення відємне значення
                //        var task = Task.Run(() =>
                //        {
                //            var r = EF.RroMoveMoney(pSumMoveMoney, new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = SelectedWorkPlace.IdWorkplace });
                //            if (r.CodeError == 0)
                //                MessageBox.Show("Успішно!");
                //            else
                //            {
                //                Thread.Sleep(100);
                //                MW.ShowErrorMessage($"Помилка: ({r.CodeError}){Environment.NewLine}{r.Error}");
                //            }
                //        });
                //    }
                //    else MW.ShowErrorMessage("Введіть коректну суму!");
                //}
            }
            MW.NumericPad.Visibility = Visibility.Collapsed;
            BackgroundShift.Visibility = Visibility.Collapsed;
        }

        private void CheckTypeTerminal(object sender, RoutedEventArgs e)
        {
            RadioButton ChBtn = sender as RadioButton;
            var aa = ChBtn.DataContext.GetType();
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

        private void OpenShiftButton(object sender, RoutedEventArgs e)
        {
            foreach (var el in ActiveWorkPlaces)
            {
                if (el?.IP != null)
                    Task.Run(async () =>
                    {
                        CommandAPI<string> Command = new() { Command = eCommand.OpenShift, Data = AdminUser?.BarCode ?? MW.AdminSSC?.BarCode };
                        try
                        {
                            var r = new SocketClient(el.IP, Global.PortAPI);
                            var Ansver = await r.StartAsync(Command.ToJSON());
                            OnSocket?.Invoke(eCommand.OpenShift, el, Ansver);
                        }
                        catch (Exception ex)
                        {
                            FileLogger.WriteLogMessage(this, $"OpenShiftButton DNSName=>{el.DNSName} {Command} ", ex);
                            OnSocket?.Invoke(eCommand.OpenShift, el, new Status(ex));
                        }
                    });
            }
        }

        private void MoneyCounting_Click(object sender, RoutedEventArgs e)
        {
            MoneyCounting.Visibility = Visibility.Visible;
        }
        private void CreateBanknote()
        {
            Banknotes = new ObservableCollection<Banknote> {
            new Banknote() {MonetaryValue = 1000,MonetaryAmount = 0},
            new Banknote() {MonetaryValue = 500, MonetaryAmount = 0 },
            new Banknote() {MonetaryValue = 200,MonetaryAmount = 0},
            new Banknote() {MonetaryValue = 100,MonetaryAmount = 0},
            new Banknote() {MonetaryValue = 50,MonetaryAmount = 0},
            new Banknote() {MonetaryValue = 20,MonetaryAmount = 0},
            new Banknote() {MonetaryValue = 10,MonetaryAmount = 0},
            new Banknote() {MonetaryValue = 5,MonetaryAmount = 0},
            new Banknote() {MonetaryValue = 2,MonetaryAmount = 0},
            new Banknote() {MonetaryValue = 1,MonetaryAmount = 0},
            new Banknote() {MonetaryValue = 0.50,MonetaryAmount = 0},
            new Banknote() {MonetaryValue = 0.10,MonetaryAmount = 0},
            };
            ListBanknotes.ItemsSource = Banknotes;
        }

        private void ChangeBanknoteAmountClick(object sender, RoutedEventArgs e)
        {

        }

        private void MoneyCountingCancel(object sender, RoutedEventArgs e)
        {
            MoneyCounting.Visibility = Visibility.Collapsed;
        }

        private void ClearMoneyCounting(object sender, RoutedEventArgs e)
        {
            foreach (var item in Banknotes)
            {
                item.MonetaryAmount = 0;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalMoneyCounting"));
        }

        private void MoneyCounting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedBanknote = ListBanknotes.SelectedItem as Banknote;
            NumericPadInputCountBanknote.Visibility = Visibility.Visible;
            InputCountBanknote.Visibility = Visibility.Visible;

            InputCountBanknote.Desciption = $"Введіть кількість {SelectedBanknote.MonetaryValue}₴";
            InputCountBanknote.ValidationMask = "^[0-9]{1,4}$";
            InputCountBanknote.Result = $"";
            InputCountBanknote.IsEnableComma = false;
            InputCountBanknote.CallBackResult = (string res) =>
            {
                if (!string.IsNullOrEmpty(res))
                    SelectedBanknote.MonetaryAmount = Convert.ToInt32(res);
                else
                    SelectedBanknote.MonetaryAmount = 0;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalMoneyCounting"));

                if (ListBanknotes.Items.Count > 0 && ListBanknotes.SelectedIndex != ListBanknotes.Items.Count - 1)
                    ListBanknotes.SelectedIndex++;
                NumericPadInputCountBanknote.Visibility = Visibility.Visible;
                InputCountBanknote.Visibility = Visibility.Visible;
            };
        }

        private void OpenMoneyBoxButton(object sender, RoutedEventArgs e)
        {
            MW.StartOpenMoneyBox();
        }

        private void Exit_Program_Click(object sender, RoutedEventArgs e)
        {
            MW.CustomMessage.Show("Завершити роботу програми?", "Увага!", eTypeMessage.Question);
            MW.CustomMessage.Result = (bool resMes) =>
            {
                if (resMes)
                {
                    if (!Global.IsTest)
                        System.Diagnostics.Process.Start("explorer.exe");
                    Application.Current.Shutdown();
                }
                else TabAdmin.SelectedIndex = 0;
            };            
        }

        private void ChangeOrderNumber(object sender, RoutedEventArgs e)
        {
            MW.InputNumberPhone.Desciption = "Номер замовлення";
            MW.InputNumberPhone.ValidationMask = "";
            MW.InputNumberPhone.Result = "";
            MW.InputNumberPhone.IsEnableComma = false;
            MW.InputNumberPhone.CallBackResult = (string res) =>
            {
                OrderNumber = res;
                MW.NumericPad.Visibility = Visibility.Collapsed;
                BackgroundOtherFunction.Visibility = Visibility.Collapsed;
            };
            MW.NumericPad.Visibility = Visibility.Visible;
            BackgroundOtherFunction.Visibility = Visibility.Visible;
        }

        private void ImportClientOrder(object sender, RoutedEventArgs e)
        {
            if (MW.curReceipt == null || MW.curReceipt.Wares == null || !MW.curReceipt.Wares.Any())
            {
                if (OrderNumber.Length > 0)
                {
                    Bl.ds.GetClientOrder1C(OrderNumber);
                    Thread.Sleep(3000);
                    MW.CurWares = MW.curReceipt.GetLastWares;
                    MW.SetStateView(eStateMainWindows.StartWindow);
                    TabAdmin.SelectedIndex = 0;
                    OrderNumber = "0";
                    //MW.ShowErrorMessage("Ви втягнули замовлення! Вітаю");
                }
                else MW.CustomMessage.Show("Введіть коректний номер замовлення!", "Увага!", eTypeMessage.Warning);
                //MW.ShowErrorMessage("Введіть коректний номер замовлення!");

            }
            else
            {
                MW.SetStateView(eStateMainWindows.StartWindow);
                TabAdmin.SelectedIndex = 0;
                MW.CustomMessage.Show("Існує відкритий чек! Для отримання замовлення закрийте або видаліть текучий чек!", "Увага!", eTypeMessage.Warning);
                //MW.ShowErrorMessage("Існує відкритий чек! Для отримання замовлення закрийте або видаліть текучий чек!");
            }
        }

        private void FindChecksByNumber(object sender, RoutedEventArgs e)
        {
            MW.InputNumberPhone.Desciption = "№ чека який шукаєте";
            MW.InputNumberPhone.ValidationMask = "";
            MW.InputNumberPhone.Result = "";
            MW.InputNumberPhone.IsEnableComma = false;
            MW.InputNumberPhone.CallBackResult = (string res) =>
            {
                if (!string.IsNullOrEmpty(res))
                    FindReceiptByNumber = Convert.ToInt32(res);
                else
                    FindReceiptByNumber = 0;
                CollectionViewSource.GetDefaultView(ListReceipts.ItemsSource).Refresh();
                MW.NumericPad.Visibility = Visibility.Collapsed;
                BackgroundReceipts.Visibility = Visibility.Collapsed;
            };
            MW.NumericPad.Visibility = Visibility.Visible;
            BackgroundReceipts.Visibility = Visibility.Visible;
        }

        private void WorkPlacesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxWorkPlaces.SelectedItem is WorkPlace SelWorkPlace)
            {
                SelectedWorkPlace = SelWorkPlace;
                foreach (var workPlace in WorkPlaces)
                    workPlace.IsChoice = workPlace.IdWorkplace == SelWorkPlace.IdWorkplace;

            }
        }

        private void ComboBoxWorkPlaces_Loaded(object sender, RoutedEventArgs e)
        {
            // Знайти елемент з полем IsChoice == true
            var selectedWorkPlace = WorkPlaces.FirstOrDefault(wp => wp.IsChoice);

            if (selectedWorkPlace != null)
            {
                // Встановити вибраним елементом елемент з полем IsChoice == true
                ComboBoxWorkPlaces.SelectedItem = selectedWorkPlace;
            }
        }


        private void CashRegistersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxCashRegisters.SelectedItem is Rro SelCashRegister)
                SelectedCashRegister = SelCashRegister;
        }

        private void ChooseTypeCheckoutComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxChooseTypeCheckout.SelectedItem is CurentTypeWorkplace SelTypeCheckout)
            {
                Global.TypeWorkplaceCurrent = SelTypeCheckout.TypeWorkplace_;
                MW.SetWorkPlace();
                Access.Init();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCashRegister)));
            }
        }

        private void RemovePayment(object sender, RoutedEventArgs e)
        {
            MW.CustomMessage.Show("Дійсно видалити всі оплати?", "Увага!", eTypeMessage.Question);
            MW.CustomMessage.Result = (bool resMes) =>
            {
                if (resMes)
                {
                    // Код який видаляє оплату
                }
            };
        }
    }

    public class APIRadiobuton
    {
        public eCommand ServerTypeMessage { get; set; }
        public string TranslateServerTypeMessage { get { return ServerTypeMessage.GetDescription(); } }
    }

    public class Banknote : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public double MonetaryValue { get; set; }
        private double _MonetaryAmount = 0;
        public double MonetaryAmount
        {
            get { return _MonetaryAmount; }
            set
            {
                _MonetaryAmount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MonetarySum"));
            }
        }
        public double MonetarySum { get { return MonetaryValue * MonetaryAmount; } }
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
    public class CurentTypeWorkplace
    {
        public eTypeWorkplace TypeWorkplace_ { get; set; }
        public string TranslateTypeWorkplace_ { get { return TypeWorkplace_.GetDescription(); } }
    }




}
