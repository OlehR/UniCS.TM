using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Front.Equipments;
using Front.Models;
using ModelMID;
using ModelMID.DB;
using SharedLib;
using Utils;
using UtilNetwork;
using Microsoft.Extensions.Configuration;
using System.Windows.Media;
using System.Windows.Documents;
using System.Reflection;
using System.IO;
using System.Security.AccessControl;
using Front.Control;
using System.Windows.Threading;

namespace Front
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }
        public event PropertyChangedEventHandler PropertyChanged;
        public Access Access = Access.GetAccess();
        public BL Bl;
        public EquipmentFront EF;
        public ControlScale CS { get; set; }

        Sound s;
        public string Clock { get; set; } = DateTime.Now.ToShortDateString();
        public User AdminSSC { get; set; } = null;
        public DateTime DTAdminSSC { get; set; }

        public Receipt curReceipt;//{ get; set; } = null;
        public Receipt ReceiptPostpone = null;
        /// <summary>
        /// Можливість правки кількості для замовлень
        /// </summary>
        public bool IsOrderReceipt { get { return !string.IsNullOrEmpty(curReceipt?.NumberOrder); } }
        public bool IsReceiptPostponeNotNull { get { return ReceiptPostpone == null; } }
        public bool IsFullReturn = false;
        public bool IsReceiptPostpone
        {
            get
            {
                if (ReceiptPostpone == null)
                    return true;
                else
                    if (curReceipt == null || curReceipt.Wares == null || !curReceipt.Wares.Any())
                    return true;
                else
                    return false;
            }
        }


        public ReceiptWares CurWares { get; set; } = null;
        public Client Client { get; set; }
        public GW CurW { get; set; } = null;

        public eStateMainWindows State = eStateMainWindows.StartWindow;
        // eStateScale StateScale = eStateScale.NotDefine;
        public eTypeAccess TypeAccessWait { get; set; }
        public ObservableCollection<ReceiptWares> ListWares { get; set; }
        public CustomWindow customWindow { get; set; }
        //public ObservableCollection<CustomButton> customWindowButtons { get; set; }
        public string WaresQuantity { get { return curReceipt?.Wares?.Count().ToString() ?? "0"; } }

        //double tempMoneySum;
        public decimal MoneySum { get { return EF.SumReceiptFiscal(curReceipt); } } //return curReceipt?.Wares?.Sum(r => r.SumTotal) ?? 0; } }

        public string EquipmentInfo { get; set; }
        bool _Volume = true;
        public bool Volume { get { return _Volume; } set { _Volume = value; if (s != null) s.IsSound = value; } }

        public bool IsShowWeightWindows { get; set; } = false;
        //public bool IsIgnoreExciseStamp { get; set; }
        public bool IsConfirmAdmin { get; set; }
        // public bool IsFixWeight { get; set; }
        public bool IsExciseStamp { get; set; }
        public bool IsCheckReturn { get { return curReceipt?.TypeReceipt == eTypeReceipt.Refund ? true : false; } }
        public bool IsCheckPaid { get { return curReceipt?.StateReceipt == eStateReceipt.Pay ? true : false; } }

        SocketServer SocketServer;
        /// <summary>
        /// Чи заброкована зміна
        /// </summary>
        //bool _IsLockSale = true;
        public bool IsLockSale { get { return AdminSSC == null; } }// set { if (_IsLockSale != value) { SetStateView(!value && State == eStateMainWindows.WaitAdmin ? eStateMainWindows.WaitInput : eStateMainWindows.NotDefine); _IsLockSale = value; } } }

        /// <summary>
        /// чи є товар з обмеженням по віку
        /// </summary>
        public bool IsAgeRestrict { get { return curReceipt == null ? true : curReceipt?.AgeRestrict == 0 || curReceipt?.AgeRestrict != 0 && curReceipt.IsConfirmAgeRestrict; } }

        /// <summary>
        /// Чи можна добавляти товар 
        /// </summary>
        public bool IsAddNewWares { get { return curReceipt == null ? true : !curReceipt.IsLockChange && State == eStateMainWindows.WaitInput && !CS.IsProblem; } }
        /// <summary>
        /// Чи активна кнопка оплати
        /// </summary>
        //bool _IsEnabledPaymentButton;
        public bool IsEnabledPaymentButton
        {
            get
            {
                return (MoneySum >= 0 && WaresQuantity != "0" && IsAddNewWares)
                                                           || (curReceipt?.TypeReceipt == eTypeReceipt.Refund && MoneySum > 0) || curReceipt?.StateReceipt == eStateReceipt.Pay;
            }
        }
        // set { _IsEnabledPaymentButton = value; } }
        /// <summary>
        /// чи активна кнопка пошуку
        /// </summary>
        public bool IsEnabledFindButton { get { return IsAddNewWares; } }
        public bool IsWeightMagellan { get { return Weight > 0 ? true : false; } }
        /// <summary>
        /// Чи можна підтвердити власну сумку
        /// </summary>
        public bool IsOwnBag { get { return ControlScaleCurrentWeight > 0 && ControlScaleCurrentWeight <= Global.MaxWeightBag; } }
        public string StrControlScaleCurrentWeightKg { get { return (ControlScaleCurrentWeight / 1000).ToString("N3"); } }
        public bool IsPresentFirstTerminal { get { return EF.BankTerminal1 != null; } }
        public bool IsPresentSecondTerminal { get { return EF.BankTerminal2 != null; } }

        bool IsViewProblemeWeight { get { return State == eStateMainWindows.WaitInput || State == eStateMainWindows.StartWindow; } }
        /// <summary>
        /// Чи треба вибирати ціну.
        /// </summary>
        bool IsChoicePrice { get { return CurWares != null && CurWares.IsMultiplePrices && curReceipt != null && curReceipt.GetLastWares?.CodeWares != CurWares.CodeWares && curReceipt.Equals(CurWares); } }
        /// <summary>
        /// теперішня вага
        /// </summary>
        public double ControlScaleCurrentWeight { get; set; } = 0d;
        public string ClientName { get { return curReceipt != null && curReceipt.CodeClient == Client?.CodeClient ? Client?.NameClient : "Проскануйте бонусну картку"; } }
        //public string CurWaresName { get { return CurWares != null ? CurWares.NameWares : " "; } }
        public int QuantityCigarettes { get; set; } = 1;
        public string NameFirstTerminal { get { return IsPresentFirstTerminal ? EF?.BankTerminal1.Name : null; } }
        public string NameSecondTerminal { get { return IsPresentSecondTerminal ? EF?.BankTerminal2.Name : null; } }

        public string GetBackgroundColor { get { return curReceipt?.TypeReceipt == eTypeReceipt.Refund ? "#ff9999" : "#FFFFFF"; } }
        public double GiveRest { get; set; } = 0;
        public string BarcodeIssueCard { get; set; } = string.Empty;
        public string PhoneIssueCard { get; set; } = string.Empty;
        public string VerifyCode { get; set; } = string.Empty;
        private StatusD<string> LastVerifyCode = new();
        public bool IsBarcodeIssueCard { get { return !string.IsNullOrEmpty(BarcodeIssueCard); } }
        public bool IsGetCard { get; set; } = false;
        public List<string> ClientPhoneNumvers = new List<string>();
        public eSyncStatus DatabaseUpdateStatus { get; set; } = eSyncStatus.SyncFinishedSuccess;

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
        public class WidthHeaderReceipt
        {
            public int WidthName { get; set; }
            public int WidthWastebasket { get; set; }
            public int WidthCountWares { get; set; }
            public int WidthWeight { get; set; }
            public int WidthPrice { get; set; }
            public int WidthTotalPrise { get; set; }
            public WidthHeaderReceipt(int widthName, int widthWastebasket, int widthCountWares, int widthWeight, int widthPrice, int widthTotalPrise)
            {
                WidthName = widthName;
                WidthWastebasket = widthWastebasket;
                WidthCountWares = widthCountWares;
                WidthWeight = widthWeight;
                WidthPrice = widthPrice;
                WidthTotalPrise = widthTotalPrise;
            }
        }
        public WidthHeaderReceipt widthHeaderReceipt { get; set; }
        public void calculateWidthHeaderReceipt(eTypeMonitor TypeMonitor)
        {
            var coefWidth = WidthScreen / 100;
            switch (TypeMonitor)
            {
                case eTypeMonitor.HorisontalMonitorKSO:
                    widthHeaderReceipt = new WidthHeaderReceipt(1100, 100, 290, 130, 130, 140);
                    break;
                case eTypeMonitor.VerticalMonitorKSO:
                    widthHeaderReceipt = new WidthHeaderReceipt(400, 90, 230, 120, 100, 150);
                    break;
                case eTypeMonitor.HorisontalMonitorRegular:
                    widthHeaderReceipt = new WidthHeaderReceipt(465, 70, 200, 90, 80, 100);
                    break;
                default:
                    widthHeaderReceipt = new WidthHeaderReceipt(56 * coefWidth, 5 * coefWidth, 15 * coefWidth, 8 * coefWidth, 8 * coefWidth, 8 * coefWidth);
                    break;
            }
        }
        public bool IsHorizontalScreen { get { return SystemParameters.PrimaryScreenWidth < SystemParameters.PrimaryScreenHeight ? true : false; } }
        public int WidthScreen { get { return (int)SystemParameters.PrimaryScreenWidth; } }
        public int HeightScreen { get { return (int)SystemParameters.PrimaryScreenHeight; } }
        public int HeightStartVideo { get { return SystemParameters.PrimaryScreenWidth < SystemParameters.PrimaryScreenHeight ? 1300 : 700; } }
        public string[] PathVideo = null;
        public bool IsManyPayments { get; set; } = false;
        public string AmountManyPayments { get; set; } = "";
        public string SumTotalManyPayments { get; set; } = "Загальна сума: ";
        public bool IsCashRegister { get { return (Global.TypeWorkplaceCurrent == eTypeWorkplace.CashRegister); } }


        public System.Drawing.Color GetFlagColor(eStateMainWindows pStateMainWindows, eTypeAccess pTypeAccess, eStateScale pSS)
        {
            if (pTypeAccess == eTypeAccess.ExciseStamp)
                return System.Drawing.Color.Green;

            System.Drawing.Color c = FC.ContainsKey(pStateMainWindows) ? FC[pStateMainWindows] : System.Drawing.Color.Black;
            if (pSS == eStateScale.WaitGoods)
                return System.Drawing.Color.Red;
            if (pSS == eStateScale.BadWeight)
                return System.Drawing.Color.Orange;
            if (pSS == eStateScale.NotStabilized)
                return System.Drawing.Color.Yellow;
            return c;
        }

        string LastErrorEquipment = null;
        public string WaitAdminText
        {
            get
            {
                TextBlock tb = new TextBlock();
                tb.TextWrapping = TextWrapping.Wrap;
                tb.TextAlignment = TextAlignment.Center;
                tb.FontSize = 24;
                tb.Margin = new Thickness(10);
                WaitAdminImage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/clock.png"));
                if (IsCashRegister)
                    WaitAdminTitle.Text = "Будь ласка очікуйте охорону!";
                // WaitAdminTitle.Visibility = Visibility.Visible;
                switch (TypeAccessWait)
                {
                    case eTypeAccess.DelWares:
                        tb.Inlines.Add("Видалення товару: "); // додає просто текс - налаштування тексту бере ті, що задамо вище
                        tb.Inlines.Add(new Run(CurWares?.NameWares) { FontWeight = FontWeights.Bold, Foreground = Brushes.Red }); // додає текст але можна змінити будь-які його параметри
                        break;
                    case eTypeAccess.DelReciept:
                        tb.Inlines.Add("Видалити чек ");
                        break;
                    case eTypeAccess.ReturnReceipt:
                        tb.Inlines.Add("Створити чек повернення");
                        break;
                    case eTypeAccess.StartFullUpdate:
                        tb.Inlines.Add(new Run("Повне оновлення БД") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red });
                        break;
                    case eTypeAccess.ErrorFullUpdate:
                        tb.Inlines.Add(new Run("Помилка повного оновлення БД") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red, FontSize = 20 });
                        break;
                    case eTypeAccess.ErrorDB:
                        tb.Inlines.Add(new Run("Помилка зміни структури БД") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red, FontSize = 20 });
                        break;
                    case eTypeAccess.ErrorEquipment:
                        tb.Inlines.Add(new Run("Проблема з критично важливим обладнанням") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red });
                        if (!string.IsNullOrEmpty(LastErrorEquipment))
                            tb.Inlines.Add(new Run(LastErrorEquipment) { FontWeight = FontWeights.Bold, Foreground = Brushes.Red });
                        break;
                    case eTypeAccess.LockSale:
                        WaitAdminTitle.Visibility = Visibility.Collapsed;
                        tb.Inlines.Add("Зміна заблокована");
                        break;
                    case eTypeAccess.FixWeight:
                        tb.Inlines.Add(new Run(CS.Info) { FontWeight = FontWeights.Bold, Foreground = Brushes.Red, FontSize = 32 });
                        tb.Inlines.Add(new Run(CS.InfoEx) { Foreground = Brushes.Black, FontSize = 20 });
                        break;
                    case eTypeAccess.ConfirmAge:
                        WaitAdminTitle.Visibility = Visibility.Collapsed;
                        WaitAdminImage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/18PlusRed.png"));
                        tb.Inlines.Add(new Run(IsCashRegister ? "Клієнту виповнилось 18?" : "Вам виповнилось 18 років?") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red, FontSize = 32 });
                        break;
                    case eTypeAccess.ExciseStamp:
                        WaitAdminTitle.Visibility = Visibility.Collapsed;
                        tb.Inlines.Add(new Run("Відскануйте акцизну марку!") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red, FontSize = 32 });
                        break;
                    case eTypeAccess.UseBonus:
                        WaitAdminTitle.Visibility = Visibility.Collapsed;
                        tb.Inlines.Add(new Run("Для списання бонусів потрібно підтвердження охорони!") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red, FontSize = 32 });
                        break;
                }
                StackPanelWaitAdmin.Children.Clear();
                StackPanelWaitAdmin.Children.Add(tb);
                return null;
            }
        }
        /// <summary>
        /// Вага з основної ваги
        /// </summary>
        public double Weight { get; set; } = 0d;

        /// <summary>
        /// полоса стану обміну
        /// </summary>
        public string ExchangeRateBar { get; set; } = "LightGreen";
        /// <summary>
        /// Показати кнопку "Ок" якщо текст введений правильно
        /// </summary>
        public bool CustomWindowValidText { get; set; }

        SortedList<eStateMainWindows, System.Drawing.Color> FC = new();

        public MainWindow()
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Ver={Version}", eTypeLog.Expanded);

            if (Global.PortAPI > 0)
            {
                SocketServer = new SocketServer(Global.PortAPI, CallBackApi);
                _ = SocketServer.StartAsync();
            }

            CS = new ControlScale(10d, Global.TypeWorkplaceCurrent == eTypeWorkplace.SelfServicCheckout);
            s = Sound.GetSound(CS);
            Volume = (Global.TypeWorkplaceCurrent == eTypeWorkplace.SelfServicCheckout);
            var fc = new List<FlagColor>();
            Config.GetConfiguration().GetSection("MID:FlagColor").Bind(fc);
            foreach (var el in fc)
                if (!FC.ContainsKey(el.State))
                    FC.Add(el.State, el.Color);

            Access.СurUser = new User() { TypeUser = eTypeUser.Client, CodeUser = 99999999, Login = "Client", NameUser = "Client" };

            Bl = new BL(true);
            EF = new EquipmentFront(GetBarCode, null, this);
            InitAction();

            calculateWidthHeaderReceipt(TypeMonitor);

            InitializeComponent();

            //поточний час
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            string DirName = Path.Combine(Global.PathPictures, "Video");

            if (Directory.Exists(DirName))
                PathVideo = Directory.GetFiles(DirName);

            if (PathVideo != null && PathVideo.Length != 0)
            {


                //VideoPlayer.Source = new Uri(PathVideo[0]); // TMP ПЕРЕРОБИТИ
                StartVideo.Source = new Uri(PathVideo[0]);
                StartVideo.Play();
                StartVideo.MediaEnded += (object sender, RoutedEventArgs e) =>
                {
                    StartVideo.Position = new TimeSpan(0, 0, 0, 0, 1);
                    StartVideo.Play();
                };
            }

            DirName = Path.Combine(Global.PathPictures, "Logo");
            if (Directory.Exists(DirName))
            {
                var PathLogo = Directory.GetFiles(DirName, "*.png");
                if (PathLogo != null && PathLogo.Any())
                    StartLogo.Source = new BitmapImage(new Uri(PathLogo[0]));
            }


            AdminControl.Init(this);
            PaymentWindow.Init(this);
            CustomMessage.Init(this);

            //Провіряємо чи зміна відкрита.
            string BarCodeAdminSSC = Bl.db.GetConfig<string>("CodeAdminSSC");
            if (!string.IsNullOrEmpty(BarCodeAdminSSC))
            {
                DateTime TimeAdminSSC = Bl.db.GetConfig<DateTime>("DateAdminSSC");
                if (TimeAdminSSC.Date == DateTime.Now.Date)
                {
                    if (!string.IsNullOrEmpty(BarCodeAdminSSC))
                    {
                        try
                        {
                            AdminSSC = Bl.GetUserByBarCode(BarCodeAdminSSC);
                        }
                        catch (Exception e) { };
                        if (Global.TypeWorkplaceCurrent == eTypeWorkplace.CashRegister)
                            Access.СurUser = AdminSSC;
                    }
                    DTAdminSSC = TimeAdminSSC;
                    Bl.StartWork(Global.IdWorkPlace, BarCodeAdminSSC);//!!!TMP треба штрихкод
                }
                else BarCodeAdminSSC = null;
            }

            ua.Tag = new CultureInfo("uk");
            en.Tag = new CultureInfo("en");
            hu.Tag = new CultureInfo("hu");
            pl.Tag = new CultureInfo("pl");

            SetCurReceipt(null);
            Receipt LastR = null;
            if (Global.TypeWorkplaceCurrent == eTypeWorkplace.SelfServicCheckout)
            {
                try
                {
                    LastR = Bl.GetLastReceipt();
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, e);
                }
                if (LastR != null && LastR.SumReceipt > 0 && LastR.StateReceipt != eStateReceipt.Canceled && LastR.StateReceipt != eStateReceipt.Print && LastR.StateReceipt != eStateReceipt.Send)
                {
                    //curReceipt = LastR;               
                    SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, new CustomWindow(eWindows.RestoreLastRecipt, LastR.SumReceipt.ToString()));
                    return;
                }
            }
            else //Касове місце
            {
                try
                {
                    var r = Bl.GetReceipts(DateTime.Now, DateTime.Now, Global.IdWorkPlace);
                    var rr = r.Where(el => el.SumTotal > 0 && el.StateReceipt != eStateReceipt.Canceled && el.StateReceipt != eStateReceipt.Print && el.StateReceipt != eStateReceipt.Send).OrderByDescending(el => el.CodeReceipt);
                    if (rr != null && rr.Any())
                    {
                        if (rr.Count() <= 2)
                        {
                            if (rr.Count() == 2)
                            {
                                ReceiptPostpone = Bl.GetReceiptHead(rr.Last(), true);
                            }
                        }
                        else //Повідомлення 
                        {
                            CustomMessage.Show($"Увас відкладено {rr.Count()} чеків!", "Увага!", eTypeMessage.Error);
                            //MessageBox.Show($"Увас відкладено {rr.Count()} чеків", "Увага!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        SetStateView(eStateMainWindows.WaitInput);
                        var Receipt = Bl.GetReceiptHead(rr.FirstOrDefault(), true);
                        Global.OnReceiptCalculationComplete?.Invoke(Receipt);
                    }
                    else
                    {

                    }
                }
                catch (Exception ex)
                {
                    var ms = ex.Message;
                }

            }

            SetStateView(eStateMainWindows.StartWindow);

            Task.Run(() => Bl.ds.SyncDataAsync());
        }

        public void SetWorkPlace()
        {
            CS.SetOnOff(Global.TypeWorkplaceCurrent == eTypeWorkplace.SelfServicCheckout);
            Volume = (Global.TypeWorkplaceCurrent == eTypeWorkplace.SelfServicCheckout);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCashRegister)));

        }

        void timer_Tick(object sender, EventArgs e)
        {
            Clock = DateTime.Now.ToShortTimeString();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Clock)));
        }

        public void SetCurReceipt(Receipt pReceipt)
        {
            try
            {
                curReceipt = pReceipt;
                if (curReceipt == null)
                {
                    Dispatcher.BeginInvoke(new ThreadStart(() => { ListWares?.Clear(); }));
                    Client = null;
                    CS.WaitClear();
                }
                else
                {
                    ListWares = new ObservableCollection<ReceiptWares>(pReceipt?.Wares);
                    Dispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        WaresList.ItemsSource = ListWares;
                        if (WaresList.Items.Count > 0)
                            WaresList.SelectedIndex = WaresList.Items.Count - 1;
                        if (VisualTreeHelper.GetChildrenCount(WaresList) > 0)
                        {
                            Border border = (Border)VisualTreeHelper.GetChild(WaresList, 0);
                            ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                            scrollViewer.ScrollToBottom();
                        }
                    }));
                    if (curReceipt.CodeClient > 0 && (Client == null || Client.CodeClient != curReceipt.CodeClient))
                    {
                        Client = new Client(curReceipt.CodeClient);
                        Bl.GetClientByCode(curReceipt, curReceipt.CodeClient);
                    }
                    if(curReceipt?.IsNeedExciseStamp == true)
                    {
                        SetStateView(eStateMainWindows.WaitInput);
                    }
                }
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            SetPropertyChanged();
        }

        public void SetPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GetBackgroundColor)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabledPaymentButton)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCheckReturn)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCheckPaid)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClientName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAgeRestrict)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabledFindButton)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLockSale)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAddNewWares)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WaitAdminText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CS)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Client)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MoneySum)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WaresQuantity)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsReceiptPostpone)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsReceiptPostponeNotNull)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsOrderReceipt)));
            //ChangeWaitAdminText();
        }

        DateTime StartScan = DateTime.MinValue;
        public void SetStateView(eStateMainWindows pSMV = eStateMainWindows.NotDefine, eTypeAccess pTypeAccess = eTypeAccess.NoDefine, ReceiptWares pRW = null, CustomWindow pCW = null, eSender pS = eSender.NotDefine)
        {

            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"pSMV={pSMV}/{State}, pTypeAccess={pTypeAccess}/{TypeAccessWait}, pRW ={pRW} , pCW={pCW},  pS={pS}", eTypeLog.Full);
            SetPropertyChanged();

            if (State == eStateMainWindows.WaitOwnBag && pTypeAccess == eTypeAccess.FixWeight)
                return;
            //Подія по вазі 
            if (pS == eSender.ControlScale)
            {
                // під час оплати - ігноруємо її
                if ((State == eStateMainWindows.ProcessPay)) //|| State == eStateMainWindows.ProcessPrintReceipt
                {
                    if (pTypeAccess == eTypeAccess.FixWeight)
                        EF.StartMultipleTone();
                    else
                        EF.StopMultipleTone();
                    return;
                }
                //Ігноруємо в  Адмін панелі. //Під час відновлення чека  //Якщо очікуємо ввід ціни.
                if (State == eStateMainWindows.AdminPanel || customWindow?.Id == eWindows.RestoreLastRecipt || State == eStateMainWindows.WaitInputPrice) return;
            }

            if (pSMV == eStateMainWindows.StartWindow && curReceipt != null)
                pSMV = eStateMainWindows.WaitInput;
            if (pSMV == eStateMainWindows.WaitInput && curReceipt == null)
                pSMV = eStateMainWindows.StartWindow;
            //lock (this._locker)
            {
                var r = Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    if ((EF.StatCriticalEquipment != eStateEquipment.On || !Bl.ds.IsReady || IsLockSale || CS.IsProblem || curReceipt?.IsNeedExciseStamp == true || IsChoicePrice) &&
                       pSMV != eStateMainWindows.WaitAdminLogin && pSMV != eStateMainWindows.AdminPanel)
                    {
                        eTypeAccess Res = eTypeAccess.NoDefine;
                        if (EF.StatCriticalEquipment != eStateEquipment.On) Res = eTypeAccess.ErrorEquipment;
                        else
                           if (!Bl.ds.IsReady)
                        // Res = (Bl.ds.Status == eSyncStatus.Error ? eTypeAccess.ErrorFullUpdate : eTypeAccess.StartFullUpdate);
                        {

                            switch (Bl.ds.Status)
                            {
                                case eSyncStatus.Error:
                                    Res = eTypeAccess.ErrorFullUpdate; break;
                                case eSyncStatus.StartedFullSync:
                                    Res = eTypeAccess.StartFullUpdate; break;
                                case eSyncStatus.ErrorDB:
                                    Res = eTypeAccess.ErrorDB; break;
                            }
                        }
                        else
                            if (IsLockSale) Res = eTypeAccess.LockSale;
                        else
                        if (!(pTypeAccess == eTypeAccess.DelReciept || pTypeAccess == eTypeAccess.DelWares ||
                        ((TypeAccessWait == eTypeAccess.DelReciept || TypeAccessWait == eTypeAccess.DelWares)
                            && !(pTypeAccess == eTypeAccess.NoDefine && pSMV == eStateMainWindows.WaitInput))))
                        {
                            if (curReceipt?.IsNeedExciseStamp == true) Res = eTypeAccess.ExciseStamp;
                            else
                            if (IsChoicePrice)
                            { pSMV = eStateMainWindows.WaitInputPrice; pRW = CurWares; }
                            else
                            if (CS.IsProblem)
                            {
                                if (State != eStateMainWindows.ProcessPay && customWindow?.Id != eWindows.RestoreLastRecipt)
                                {
                                    if (!IsShowWeightWindows && pSMV != eStateMainWindows.WaitAdmin) //--&&  && pSMV != eStateMainWindows.BlockWeight && pSMV != eStateMainWindows.WaitOwnBag && pSMV != eStateMainWindows.WaitAdmin)
                                        pSMV = eStateMainWindows.BlockWeight;
                                    else
                                        Res = eTypeAccess.FixWeight;
                                }
                            }
                        }

                        if (Res != eTypeAccess.NoDefine && Res != eTypeAccess.ChoicePrice)// && pSMV != eStateMainWindows.BlockWeight)
                        {
                            pSMV = eStateMainWindows.WaitAdmin;
                            pTypeAccess = Res;
                        }
                    }

                    if (pRW != null)
                        CurWares = pRW;
                    if (pSMV != eStateMainWindows.WaitAdminLogin)
                    {
                        TypeAccessWait = pTypeAccess;
                    }

                    if (pSMV != eStateMainWindows.StartWindow && State == eStateMainWindows.StartWindow)
                        StartVideo.Pause();
                    if (pSMV == eStateMainWindows.StartWindow && State != eStateMainWindows.StartWindow && IsCashRegister == false)
                        StartVideo.Play();

                    //Якщо 
                    if (pSMV == eStateMainWindows.NotDefine)
                    {
                        SetPropertyChanged();
                        return;
                    }
                    else
                    {
                        State = pSMV;
                        SetPropertyChanged();
                        EF.SetColor(GetFlagColor(State, TypeAccessWait, CS.StateScale));
                    }

                    //Зупиняєм пищання сканера
                    if (State != eStateMainWindows.ProcessPay)
                        EF.StopMultipleTone();

                    TimeScan();

                    //Генеруємо з кастомні вікна
                    if (TypeAccessWait == eTypeAccess.FixWeight)
                        customWindow = new CustomWindow(CS.StateScale, CS.RW?.Quantity == 1 && CS.RW?.FixWeightQuantity == 0, CS.StateScale == eStateScale.WaitClear && (curReceipt?.OwnBag ?? 0) > 0);
                    else
                    if (TypeAccessWait == eTypeAccess.ConfirmAge)
                        customWindow = new CustomWindow(eWindows.ConfirmAge, IsCashRegister);
                    else
                        if (TypeAccessWait == eTypeAccess.ExciseStamp)
                        customWindow = new CustomWindow(eWindows.ExciseStamp, IsCashRegister);
                    else
                    if (TypeAccessWait == eTypeAccess.UseBonus)
                    {
                        ClientPhoneNumvers = new List<string>();
                        (string phone1, bool res1) = PhoneCorrection(Client.MainPhone);
                        (string phone2, bool res2) = PhoneCorrection(Client.PhoneAdd);
                        if (res1)
                            ClientPhoneNumvers.Add(phone1);
                        if (res2)
                            ClientPhoneNumvers.Add(phone2);
                        customWindow = new CustomWindow(eWindows.UseBonus, ClientPhoneNumvers);
                    }

                    else
                        customWindow = (State == eStateMainWindows.WaitCustomWindows ? pCW : null);

                    if (State == eStateMainWindows.StartWindow)
                        Volume = (Global.TypeWorkplaceCurrent == eTypeWorkplace.SelfServicCheckout);
                    if (TypeAccessWait == eTypeAccess.FixWeight || !IsConfirmAdmin)
                        s.Play(State, TypeAccessWait, CS.StateScale, 0);
                    //if ((State == eStateMainWindows.WaitAdmin || State == eStateMainWindows.WaitAdminLogin) && TypeAccessWait == eTypeAccess.ExciseStamp)
                    //    customWindow = new CustomWindow(pCW, pStr);

                    WaitAdminWeightButtons.ItemsSource = null;
                    if (customWindow?.Buttons != null)
                        WaitAdminWeightButtons.ItemsSource = new ObservableCollection<CustomButton>(customWindow?.Buttons);
                    else
                        WaitAdminWeightButtons.ItemsSource = null;

                    if (State != eStateMainWindows.WaitAdmin && State != eStateMainWindows.WaitAdminLogin)
                        TypeAccessWait = eTypeAccess.NoDefine;

                    if (TypeAccessWait == eTypeAccess.DelReciept && (curReceipt?.Wares?.Count() ?? 0) == 0)
                    {
                        TypeAccessWait = eTypeAccess.NoDefine;
                        State = eStateMainWindows.StartWindow;
                    }

                    ExciseStamp.Visibility = Visibility.Collapsed;
                    ChoicePrice.Visibility = Visibility.Collapsed;
                    Background.Visibility = Visibility.Collapsed;
                    BackgroundWares.Visibility = Visibility.Collapsed;
                    WaitAdmin.Visibility = Visibility.Collapsed;
                    WaitAdminLogin.Visibility = Visibility.Collapsed;
                    WeightWares.Visibility = Visibility.Collapsed;
                    WaitPayment.Visibility = Visibility.Collapsed;
                    StartShopping.Visibility = Visibility.Collapsed;
                    StartShoppingLogo.Visibility = Visibility.Collapsed;
                    //textInAll.Visibility = Visibility.Visible;
                    //valueInAll.Visibility = Visibility.Visible;
                    ConfirmAge.Visibility = Visibility.Collapsed;
                    //WaitKashier.Visibility = Visibility.Collapsed;
                    CustomWindows.Visibility = Visibility.Collapsed;
                    ErrorBackground.Visibility = Visibility.Collapsed;
                    OwnBagWindows.Visibility = Visibility.Collapsed;
                    PaymentWindow.Visibility = Visibility.Collapsed;
                    ClientDetails.Visibility = Visibility.Collapsed;

                    CaptionCustomWindows.Visibility = Visibility.Visible;
                    ImageCustomWindows.Visibility = Visibility.Visible;
                    CancelCustomWindows.Visibility = Visibility.Visible;
                    TextBoxCustomWindows.Visibility = Visibility.Visible;
                    KeyboardCustomWindows.Visibility = Visibility.Visible;

                    WaitAdminImage.Visibility = Visibility.Visible;
                    WaitAdminCancel.Visibility = Visibility.Visible;
                    TBExciseStamp.Visibility = Visibility.Collapsed;
                    KBAdmin.Visibility = Visibility.Collapsed;
                    ExciseStampButtons.Visibility = Visibility.Collapsed;
                    ExciseStampNameWares.Visibility = Visibility.Collapsed;
                    WaitAdminTitle.Visibility = Visibility.Visible;
                    CodeSMS.Visibility = Visibility.Collapsed;
                    WaitAdminWeightButtons.Visibility = Visibility.Visible;
                    WaitAdminAdditionalText.Visibility = Visibility.Collapsed;
                    IssueCard.Visibility = Visibility.Collapsed;
                    AdminControl.Visibility = (State == eStateMainWindows.AdminPanel ? Visibility.Visible : Visibility.Collapsed);
                    AddMissingPackage.Visibility = Visibility.Collapsed;

                    //StartVideo.Stop();

                    switch (State)
                    {
                        case eStateMainWindows.StartWindow:
                            if (PathVideo != null && PathVideo.Length != 0 && IsCashRegister == false)
                            {
                                StartShopping.Visibility = Visibility.Visible;
                            }
                            else
                                StartShoppingLogo.Visibility = Visibility.Visible;

                            //textInAll.Visibility = Visibility.Collapsed;
                            //valueInAll.Visibility = Visibility.Collapsed;
                            //StartVideo.Play();
                            break;
                        case eStateMainWindows.WaitInputPrice:
                            TypeAccessWait = eTypeAccess.ChoicePrice;
                            if (CurWares != null && CurWares.Prices != null && CurWares.Prices.Count() > 0)
                            {
                                var rrr = new ObservableCollection<Models.Price>(CurWares.Prices.OrderByDescending(r => r.Price).Select(r => new Models.Price(r.Price, Access.GetRight(TypeAccessWait), r.TypeWares)));
                                rrr.First().IsEnable = true;
                                Prices.ItemsSource = rrr;//new ObservableCollection<Price>(rr);
                            }
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            ChoicePrice.Visibility = Visibility.Visible;

                            break;
                        //case eStateMainWindows.WaitExciseStamp:
                        //TBExciseStamp.Text = "";
                        //ExciseStamp.Visibility = Visibility.Visible;
                        //Background.Visibility = Visibility.Visible;
                        //BackgroundWares.Visibility = Visibility.Visible;
                        //TBExciseStamp.Focus();
                        // break;
                        case eStateMainWindows.WaitWeight:
                            EF.StartWeight();
                            WeightWares.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            break;
                        case eStateMainWindows.WaitAdmin:
                            WaitAdmin.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            TBExciseStamp.IsReadOnly = false;
                            if (customWindow?.Buttons != null && customWindow.Buttons.Count > 0)
                            {
                                WaitAdminCancel.Visibility = Visibility.Collapsed;
                            }
                            // CustomButtonsWaitAdmin.ItemsSource = null;
                            switch (TypeAccessWait)
                            {
                                case eTypeAccess.FixWeight:
                                    if (CS.StateScale == eStateScale.WaitClear)
                                    {
                                        WaitAdminTitle.Visibility = Visibility.Collapsed;
                                        WaitAdminImage.Visibility = Visibility.Collapsed;
                                    }
                                    break;
                                case eTypeAccess.AddNewWeight:
                                    //case eTypeAccess.
                                    // WaitAdminWeightButtons.ItemsSource = customWindow.Buttons;
                                    break;
                                case eTypeAccess.ExciseStamp:
                                    //customWindow = GetCustomButton(eWindows.ExciseStamp);
                                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurWaresName"));
                                    TBExciseStamp.Text = "";
                                    WaitAdminImage.Visibility = Visibility.Collapsed;
                                    WaitAdminCancel.Visibility = Visibility.Collapsed;
                                    TBExciseStamp.Visibility = Visibility.Visible;
                                    KBAdmin.Visibility = Visibility.Collapsed;
                                    ExciseStampButtons.Visibility = Visibility.Visible;
                                    ExciseStampNameWares.Visibility = Visibility.Visible;
                                    WaitAdminTitle.Visibility = Visibility.Collapsed;
                                    CodeSMS.Visibility = Visibility.Collapsed;
                                    // CustomButtonsWaitAdmin.ItemsSource = customWindow.Buttons;

                                    break;
                                case eTypeAccess.UseBonus:
                                    if (customWindow?.Buttons != null && customWindow.Buttons.Count > 0)
                                    {
                                        WaitAdminAdditionalText.Text = "Можливе підтвердження списання за номером телефону!";
                                        TBExciseStamp.Text = "";
                                        WaitAdminImage.Visibility = Visibility.Visible;
                                        WaitAdminCancel.Visibility = Visibility.Visible;
                                        TBExciseStamp.Visibility = Visibility.Collapsed;
                                        KBAdmin.Visibility = Visibility.Collapsed;
                                        ExciseStampButtons.Visibility = Visibility.Collapsed;
                                        ExciseStampNameWares.Visibility = Visibility.Collapsed;
                                        WaitAdminTitle.Visibility = Visibility.Collapsed;
                                        CodeSMS.Visibility = Visibility.Visible;
                                        WaitAdminAdditionalText.Visibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        WaitAdminCancel.Visibility = Visibility.Visible;
                                        WaitAdminWeightButtons.Visibility = Visibility.Collapsed;
                                    }

                                    break;
                                default:

                                    break;
                            }
                            //CustomButtonsWaitAdmin.ItemsSource = customWindowButtons;
                            break;
                        case eStateMainWindows.WaitAdminLogin:
                            LoginTextBlock.Text = "";
                            PasswordTextBlock.Password = "";
                            WaitAdminLogin.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            LoginTextBlock.Focus();
                            break;
                        case eStateMainWindows.WaitFindWares:
                            FindWaresWin FWW = new FindWaresWin(this);
                            FWW.Show();
                            break;
                        case eStateMainWindows.ProcessPay:
                            PaymentImage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/newPaymentTerminal.png"));
                            WaitPayment.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            //PaymentImage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/paymentTerminal.png"));
                            break;
                        case eStateMainWindows.ProcessPrintReceipt:
                            PaymentImage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/newReceipt.png"));
                            WaitPayment.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            //Client.NameClient = null;
                            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClientName"));
                            break;

                        case eStateMainWindows.ChoicePaymentMethod:
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPresentSecondTerminal)));
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPresentFirstTerminal)));

                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameSecondTerminal)));
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameFirstTerminal)));

                            PaymentWindow.TransferAmounts(MoneySum);
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            PaymentWindow.Visibility = Visibility.Visible;
                            break;
                        case eStateMainWindows.WaitCustomWindows:
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            TextBoxCustomWindows.Text = null;
                            if (customWindow?.Buttons != null)
                            {
                                CustomWindowsItemControl.ItemsSource = new ObservableCollection<CustomButton>(customWindow.Buttons);
                                OKCustomWindows.Visibility = Visibility.Collapsed;
                                CancelCustomWindows.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                OKCustomWindows.Visibility = Visibility.Visible;
                                CancelCustomWindows.Visibility = Visibility.Visible;
                            }

                            ButtonsCustomWindows.Visibility = customWindow?.Buttons == null ? Visibility.Collapsed : Visibility.Visible;
                            if (customWindow == null) return;
                            if (customWindow.Caption == null) CaptionCustomWindows.Visibility = Visibility.Collapsed;
                            if (customWindow.PathPicture == null) ImageCustomWindows.Visibility = Visibility.Collapsed;
                            if (customWindow.AnswerRequired == false) CancelCustomWindows.Visibility = Visibility.Collapsed;
                            if (customWindow.ValidationMask == null)
                            {
                                TextBoxCustomWindows.Visibility = Visibility.Collapsed;
                                KeyboardCustomWindows.Visibility = Visibility.Collapsed;
                            }
                            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("customWindow"));
                            CustomWindows.Visibility = Visibility.Visible;
                            TextBoxCustomWindows.Focus();
                            break;
                        case eStateMainWindows.BlockWeight:
                            TypeAccessWait = eTypeAccess.FixWeight;
                            //IsEnabledPaymentButton = false;
                            //IsEnabledFindButton = false;
                            ///PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabledPaymentButton"));
                            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabledFindButton"));
                            break;
                        case eStateMainWindows.WaitInputIssueCard:
                            IssueCard.Visibility = Visibility.Visible;
                            EnterPhoneIssueCard(null, null);
                            break;
                        case eStateMainWindows.WaitInput:
                            //IsIgnoreExciseStamp = false;
                            //IsAddNewWeight = false;
                            //IsFixWeight = false;
                            WaresList.Focus(); //Для сканера через імітацію клавіатури
                            break;
                        case eStateMainWindows.WaitOwnBag:
                            StartShopping.Visibility = Visibility.Collapsed;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            OwnBagWindows.Visibility = Visibility.Visible;
                            break;
                        default:
                            break;
                    }
                    SetPropertyChanged();
                }));
                var res = r.Wait(new TimeSpan(0, 0, 0, 150));
                if (res != DispatcherOperationStatus.Completed)
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"res=>{res} pSMV={pSMV}/{State}, pTypeAccess={pTypeAccess}/{TypeAccessWait}, pRW ={pRW} , pCW={pCW},  pS={pS}", eTypeLog.Error);

            }
        }

        void TimeScan(bool? pIsSave = null)
        {
            if ((State == eStateMainWindows.WaitAdmin && !CS.IsProblem) || State == eStateMainWindows.AdminPanel || State == eStateMainWindows.WaitAdminLogin ||
                       State == eStateMainWindows.ChoicePaymentMethod || State == eStateMainWindows.ProcessPay || State == eStateMainWindows.StartWindow || pIsSave==true)
            {
                if (StartScan != DateTime.MinValue)
                {
                    Bl.SaveReceiptEvents(new List<ReceiptEvent>() { new ReceiptEvent(curReceipt) { ResolvedAt = StartScan, EventType = eReceiptEventType.TimeScanReceipt, EventName = "Час сканування чека" } }, false);
                    StartScan = DateTime.MinValue;
                }
            }
            else
            {
                if (pIsSave == false || (StartScan == DateTime.MinValue && (
                        State == eStateMainWindows.WaitInput || State == eStateMainWindows.WaitFindWares || State == eStateMainWindows.WaitInputPrice || State == eStateMainWindows.WaitInputIssueCard)))
                    StartScan = DateTime.Now;
            }
        }

        private void _Delete(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.DataContext is ReceiptWares)
            {
                var el = btn.DataContext as ReceiptWares;
                if (el == null)
                    return;
                CurWares = el;
                TypeAccessWait = eTypeAccess.DelWares;
                if (!SetConfirm(Access.СurUser, true, !el.IsConfirmDel))
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.DelWares, el);
            }
        }

        private void BtnClickMinusPlus(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.DataContext is ReceiptWares)
            {
                ReceiptWares temp = btn.DataContext as ReceiptWares;
                if (curReceipt?.IsLockChange == false)
                {
                    Bl.ChangeQuantity(temp, temp.Quantity + (btn.Name.Equals("Plus") ? 1 : -1));
                }
            }
        }

        private void _ChangeCountWares(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.DataContext is ReceiptWares)
            {
                ReceiptWares temp = btn.DataContext as ReceiptWares;
                InputNumberPhone.Desciption = Convert.ToString(temp.NameWares);
                InputNumberPhone.Result = "";//Convert.ToString(temp.Quantity);
                InputNumberPhone.ValidationMask = "";
                if (temp.IsWeight) InputNumberPhone.IsEnableComma = true;
                else InputNumberPhone.IsEnableComma = false;

                NumericPad.Visibility = Visibility.Visible;
                Background.Visibility = Visibility.Visible;
                BackgroundWares.Visibility = Visibility.Visible;

                InputNumberPhone.CallBackResult = (string result) =>
                {
                    decimal tempQuantity;
                    if (result != "" && result != "0")
                    {
                        //result.Replace(",", ".");
                        tempQuantity = result.ToDecimal();//Convert.ToDecimal(result);

                        temp.Quantity = tempQuantity;
                        if (curReceipt?.TypeReceipt == eTypeReceipt.Refund && tempQuantity > temp.MaxRefundQuantity)
                        {
                            temp.Quantity = (decimal)temp.MaxRefundQuantity;
                        }
                        if (curReceipt?.IsLockChange == false)
                        {
                            Bl.ChangeQuantity(temp, temp.Quantity);
                        }
                    }
                    Background.Visibility = Visibility.Collapsed;
                    BackgroundWares.Visibility = Visibility.Collapsed;
                };
            }
        }

        private void _VolumeButton(object sender, RoutedEventArgs e)
        {
            Volume = !Volume;
        }

        private void _ChangeLanguage(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            try
            {
                if (btn != null)
                {
                    if (btn.Tag is CultureInfo lang)
                    {
                        App.Language = lang;
                    }
                }
            }
            catch { }
            ua.Style = (Style)ua.FindResource("WhiteNotBorderButton");
            en.Style = (Style)en.FindResource("WhiteNotBorderButton");
            hu.Style = (Style)hu.FindResource("WhiteNotBorderButton");
            pl.Style = (Style)pl.FindResource("WhiteNotBorderButton");
            switch (btn.Name)
            {
                case "ua":
                    ua.Style = (Style)ua.FindResource("WhiteButton");
                    break;
                case "en":
                    en.Style = (Style)en.FindResource("WhiteButton");
                    break;
                case "hu":
                    hu.Style = (Style)hu.FindResource("WhiteButton");
                    break;
                case "pl":
                    pl.Style = (Style)pl.FindResource("WhiteButton");
                    break;
            }
        }

        private void _Back(object sender, RoutedEventArgs e)
        {
            // Правильний блок.
            if (Access.GetRight(eTypeAccess.DelReciept) || curReceipt?.SumReceipt == 0 || curReceipt?.StateReceipt >= eStateReceipt.Print)
            {
                if (curReceipt != null && curReceipt.StateReceipt == eStateReceipt.Prepare)
                    Bl.SetStateReceipt(curReceipt, eStateReceipt.Canceled);

                SetCurReceipt(null);
                SetStateView(eStateMainWindows.StartWindow);
            }
            else
                SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.DelReciept, null);
        }

        private void _Search(object sender, RoutedEventArgs e)
        {
            SetStateView(eStateMainWindows.WaitFindWares);
        }

        void IsPrises(decimal pQuantity = 0m, decimal pPrice = 0m)
        {
            if (CurWares.TypeWares == eTypeWares.Alcohol && CurWares?.Price > 0m)
            {
                SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ExciseStamp, CurWares);
                return;
            }

            if (CurWares.Price == 0) //Повідомлення Про відсутність ціни
            {
                SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, new CustomWindow(eWindows.NoPrice, CurWares.NameWares));
            }
            if (CurWares.Prices != null && pPrice == 0m) //Меню з вибором ціни. Сигарети.
            {
                if (CurWares.IsMultiplePrices)
                {
                    SetStateView(eStateMainWindows.WaitInputPrice, eTypeAccess.NoDefine, CurWares);
                }
                /* else
                     if (CurWares.Prices.Count() == 1)
                     Bl.AddWaresCode(curReceipt, CurWares.CodeWares, CurWares.CodeUnit, pQuantity, CurWares.Prices.First().Price);*/
            }
            if (CurWares.IsMultiplePrices && pPrice > 0m)
                CurWares = null;
        }

        private void _ButtonHelp(object sender, RoutedEventArgs e)
        {
            SetStateView(Global.TypeWorkplaceCurrent == eTypeWorkplace.SelfServicCheckout || AdminSSC == null ? eStateMainWindows.WaitAdmin : eStateMainWindows.AdminPanel, eTypeAccess.AdminPanel);
        }

        private void _OwnBag(object sender, RoutedEventArgs e)
        {
            if (ControlScaleCurrentWeight > 0 && ControlScaleCurrentWeight < Global.MaxWeightBag)
            {
                NewReceipt();
                Bl.AddOwnBag(curReceipt, Convert.ToDecimal(ControlScaleCurrentWeight));
                SetStateView(eStateMainWindows.WaitInput);
            }
            //SetStateView(eStateMainWindows.ChoicePaymentMethod);
        }

        private void _BuyBag(object sender, RoutedEventArgs e)
        {
            Bl.GetClientByPhone(curReceipt, "0664417744"); //0503720278
            //CreateCustomWindiws();
            //SetStateView(eStateMainWindows.WaitCustomWindows);
        }

        private void _Cancel(object sender, RoutedEventArgs e)
        {
            SetStateView(eStateMainWindows.WaitInput);
        }

        private void StartBuy(object sender, RoutedEventArgs e)
        {
            NewReceipt();
            SetStateView(eStateMainWindows.WaitInput);
        }

        private void Cigarettes_Cancel(object sender, RoutedEventArgs e)
        {
            CurWares = null;
            QuantityCigarettes = 1;
            SetStateView(eStateMainWindows.WaitInput);
        }

        private void _ButtonPaymentBank(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var str = btn.Content as TextBlock;
            var r = EF.GetBankTerminal.Where(el => str.Text.Equals(el.Name));
            if (r.Count() == 1)
                EF.SetBankTerminal(r.First() as BankTerminal);

            var task = Task.Run(() => PrintAndCloseReceipt());
        }

        private void _ButtonPayment(object sender, RoutedEventArgs e)
        {
            PayAndPrint();
        }

        /// <summary>
        /// Добавляєм товар(сигарери) з списку цін
        /// </summary>
        /// <param name="sender">Кнопка з ціною</param>
        /// <param name="e"></param>
        private void _AddWaresPrice(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn != null)
                {
                    var price = btn.DataContext as Models.Price;

                    TextBlock Tb = btn.Content as TextBlock;
                    if (Tb != null)
                    {
                        if (price.IsConfirmAge)
                            Bl.AddEventAge(curReceipt);
                        AddWares(CurWares.CodeWares, CurWares.CodeUnit, QuantityCigarettes, price.price);
                        QuantityCigarettes = 1;
                    }
                }
            }
            catch { }
            SetStateView(eStateMainWindows.WaitInput);
        }

        private void ClickButtonOk(object sender, RoutedEventArgs e)
        {
            AddWares(CurW.Code, CurW.CodeUnit, Convert.ToDecimal(Weight * 1000));
            ClickButtonCancel(sender, e);
        }
        private void ClickButtonCancel(object sender, RoutedEventArgs e)
        {
            EF.StoptWeight();
            SetStateView(eStateMainWindows.WaitInput);
            Weight = 0d;
        }
        private void ButtonAdmin(object sender, RoutedEventArgs e)
        {
            SetStateView(eStateMainWindows.WaitAdminLogin);
        }
        private void LoginCancel(object sender, RoutedEventArgs e)
        {
            SetConfirm(null);
        }
        private void LoginButton(object sender, RoutedEventArgs e)
        {
            var U = Bl.GetUserByLogin(LoginTextBlock.Text, PasswordTextBlock.Password);
            if (U == null)
            {
                CustomMessage.Show("Не вірний логін чи пароль", "Увага!", eTypeMessage.Warning);
                //ShowErrorMessage("Не вірний логін чи пароль");
                return;
            }
            //SetStateView(eStateMainWindows.WaitAdmin);
            Bl.OnAdminBarCode?.Invoke(U);
        }

        private void TextLoginChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void TextPasswordChanged(object sender, TextChangedEventArgs e)
        {
        }

        private string GetExciseStamp(string pBarCode)
        {
            if (pBarCode.Contains("t.gov.ua"))
            {
                string Res = pBarCode.Substring(pBarCode.IndexOf("t.gov.ua") + 9);
                pBarCode = Res.Substring(0, Res.Length - 11);
            }

            Regex regex = new Regex(@"^\w{4}[0-9]{6}?$");
            if (regex.IsMatch(pBarCode))
                return pBarCode;
            return null;
        }
        public void StartOpenMoneyBox()
        {
            EF.OpenMoneyBox();
        }
        private void AddExciseStamp(object sender, RoutedEventArgs e)
        {
            AddExciseStamp(TBExciseStamp.Text);
        }

        private void ChangedExciseStamp(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            IsExciseStamp = !string.IsNullOrEmpty(GetExciseStamp(textBox.Text));
        }

        private void ExciseStampNone(object sender, RoutedEventArgs e)
        {
            AddExciseStamp("None");
            Bl.AddEventAge(curReceipt);
        }


        private void CustomWindowClickButton(object sender, RoutedEventArgs e)
        {
            //SetStateView(eStateMainWindows.WaitInput);
            IsConfirmAdmin = false;
            Button btn = sender as Button;
            CustomButton res = btn.DataContext as CustomButton;

            if (res == null)
            {
                if (btn.Name.Equals("OKCustomWindows"))
                    res = new CustomButton() { Id = 1 };
                if (btn.Name.Equals("CancelCustomWindows"))
                {
                    res = new CustomButton() { Id = 1 };
                    SetStateView(eStateMainWindows.WaitInput);
                    return;
                }
            }

            if (res != null)
            {
                if (res.CustomWindow?.Id == eWindows.RestoreLastRecipt)
                {
                    if (res.Id == 1)
                    {
                        curReceipt = Bl.GetLastReceipt();
                        Bl.db.RecalcPriceAsync(new IdReceiptWares(curReceipt));
                        SetStateView(eStateMainWindows.WaitInput);
                    }
                    if (res.Id == 2)
                    {
                        var Res = Bl.GetLastReceipt();
                        Bl.SetStateReceipt(Res, eStateReceipt.Canceled);
                        SetStateView(eStateMainWindows.StartWindow);
                    }
                    return;
                }
                if (res.CustomWindow?.Id == eWindows.ConfirmWeight)
                {

                    if (res.Id == -1)
                    {
                        IsShowWeightWindows = false;
                        SetStateView(eStateMainWindows.BlockWeight);
                        return;
                    }
                    if (res.Id == 4)
                    {
                        IsShowWeightWindows = false;
                        EF.ControlScaleCalibrateZero();
                        return;
                    }

                    if (res.Id == 6)
                    {
                        NewReceipt();
                        SetStateView(eStateMainWindows.StartWindow);
                        return;
                    }
                    if (CS.RW != null)
                    {
                        CS.RW.FixWeightQuantity = CS.RW.Quantity;
                        CS.RW.FixWeight += Convert.ToDecimal(CS.СurrentlyWeight);
                        // Bl.FixWeight(CS.RW);
                        CS.StateScale = eStateScale.Stabilized;// OnScalesData(CS.curFullWeight,true);
                    }

                }

                if (res.CustomWindow?.Id == eWindows.ExciseStamp)
                {
                    //if(res.Id==31)

                    if (res.Id == 32)
                    {
                        WaitAdminTitle.Visibility = Visibility.Visible;
                        EF.SetColor(System.Drawing.Color.Violet);
                        s.Play(eTypeSound.WaitForAdministrator);
                    }
                    else
                    if (res.Id == 33)
                    {
                        AddExciseStamp("None");
                        Bl.AddEventAge(curReceipt);
                    }
                    return;
                }
                if (res.CustomWindow?.Id == eWindows.ConfirmAge)
                {
                    TypeAccessWait = eTypeAccess.NoDefine;
                    SetStateView(eStateMainWindows.WaitInput);

                    if (res.Id == 1)
                    {
                        Task.Run(new Action(() =>
                        {
                            Bl.AddEventAge(curReceipt);
                            //Thread.Sleep(1000);
                            PayAndPrint();
                        }
                        ));
                    }
                    return;
                }
                if (res.CustomWindow?.Id == eWindows.UseBonus)
                {
                    //LastVerifyCode = new StatusD<string> { Data = "1234", State = 0, TextState = "OK" };
                    LastVerifyCode = Bl.ds.GetVerifySMS(ClientPhoneNumvers[res.Id]);
                    CustomMessage.Show($"Код підтвердження надіслано за номером {ClientPhoneNumvers[res.Id]}", "Увага!", eTypeMessage.Information);
                    //MessageBox.Show($"Код підтвердження надіслано за номером {ClientPhoneNumvers[res.Id]}", "Увага!", MessageBoxButton.OK, MessageBoxImage.Information);

                    return;
                }

                var r = new CustomWindowAnswer()
                {
                    idReceipt = curReceipt,
                    Id = res.CustomWindow?.Id ?? eWindows.NoDefinition,
                    IdButton = res.Id,
                    Text = TextBoxCustomWindows.Text,
                    ExtData = res.CustomWindow?.Id == eWindows.ConfirmWeight ? CS?.RW : null
                };
                Bl.SetCustomWindows(r);
                SetStateView(eStateMainWindows.WaitInput);
            }
        }

        private void FindClientByPhoneClick(object sender, RoutedEventArgs e)
        {
            s.Play(eTypeSound.ScanCustomerCardOrEnterPhone);
            InputNumberPhone.Desciption = "Введіть номер телефону";
            InputNumberPhone.ValidationMask = "^[0-9]{10,13}$";
            InputNumberPhone.Result = "";
            InputNumberPhone.IsEnableComma = false;
            InputNumberPhone.CallBackResult = FindClientByPhone;
            NumericPad.Visibility = Visibility.Visible;
            Background.Visibility = Visibility.Visible;
            BackgroundWares.Visibility = Visibility.Visible;
        }

        private void FindClientByPhone(string pResult)
        {
            if (curReceipt == null)
                NewReceipt();

            if (pResult.Length >= 10)
            {
                var r = new CustomWindowAnswer()
                {
                    idReceipt = curReceipt,
                    Id = eWindows.PhoneClient,
                    IdButton = 1,
                    Text = pResult,
                    ExtData = CS?.RW
                };
                Bl.SetCustomWindows(r);
            }
            if (pResult.Length == 0)
            {
                Background.Visibility = Visibility.Collapsed;
                BackgroundWares.Visibility = Visibility.Collapsed;
            }

        }

        private void CustomWindowVerificationText(object sender, TextChangedEventArgs e)
        {
            CustomWindowValidText = true;
            if (customWindow?.ValidationMask != null)
            {
                TextBox textBox = (TextBox)sender;
                Regex regex = new Regex(customWindow.ValidationMask);
                CustomWindowValidText = regex.IsMatch(textBox.Text);
            }
        }



        private void PlusOrMinusCigarettes(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            QuantityCigarettes = btn.Name.Equals("PlusCigarettesButton") ? QuantityCigarettes + 1 : QuantityCigarettes - 1;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("QuantityCigarettes"));
        }

        private void StartOwnBag(object sender, RoutedEventArgs e)
        {
            SetStateView(eStateMainWindows.WaitOwnBag);
        }

        private void CancelPayment(object sender, RoutedEventArgs e)
        {
            EF.PosCancel();
        }

        private void TextPasswordChanged(object sender, RoutedEventArgs e)
        {

        }

        private void ShowClientDetails(object sender, RoutedEventArgs e)
        {
            var aa = Client;
            ClientDetails.Visibility = Visibility.Visible;
            Background.Visibility = Visibility.Visible;
            BackgroundWares.Visibility = Visibility.Visible;

        }

        private void PostponeCheck(object sender, RoutedEventArgs e)
        {
            if (ReceiptPostpone == null)
            {
                CustomMessage.Show("Ви дійсно хочете відкласти чек?", "Відкладення чеку", eTypeMessage.Question);
                CustomMessage.Result = (bool res) =>
                {
                    if (res)
                    {
                        TimeScan(true);
                        ReceiptPostpone = curReceipt;
                        NewReceipt();
                        WaresList.Focus();
                    }
                };                
            }
            else
            {
                if (curReceipt == null || curReceipt.Wares != null && !curReceipt.Wares.Any())
                {
                    if (Client != null)
                        ShowClientBonus.Visibility = Visibility.Visible;
                    TimeScan(false);
                    Global.OnReceiptCalculationComplete?.Invoke(ReceiptPostpone);                    
                    ReceiptPostpone = null;
                    WaresList.Focus();
                }
                else
                    CustomMessage.Show("Неможливо відновити чек не закривши текучий", "Увага!", eTypeMessage.Information); 
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsReceiptPostpone"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsReceiptPostponeNotNull"));

        }

        private void IssueCardButton(object sender, RoutedEventArgs e)
        {
            SetStateView(eStateMainWindows.WaitInputIssueCard);
            //IssueCard.Visibility = Visibility.Visible;
            //EnterBarcodeIssueCard(null,null);
        }

        private void EnterBarcodeIssueCard(object sender, RoutedEventArgs e)
        {
            //BorderNumPadIssueCard.Visibility = Visibility.Visible;
            //NumPadIssueCard.Visibility = Visibility.Visible;

            //NumPadIssueCard.Desciption = $"Введіть штрихкод";
            //NumPadIssueCard.ValidationMask = "^[0-9]{13,15}$";
            //NumPadIssueCard.Result = $"{BarcodeIssueCard}";
            //NumPadIssueCard.IsEnableComma = false;
            //NumPadIssueCard.CallBackResult = (string res) =>
            //{
            //    if (!string.IsNullOrEmpty(res))
            //        BarcodeIssueCard = res;
            //    else
            //        BarcodeIssueCard = string.Empty;
            //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BarcodeIssueCard"));

            //    BorderNumPadIssueCard.Visibility = Visibility.Visible;
            //    NumPadIssueCard.Visibility = Visibility.Visible;
            //};
        }

        private void EnterPhoneIssueCard(object sender, RoutedEventArgs e)
        {
            bool status = true;
            BorderNumPadIssueCard.Visibility = Visibility.Visible;
            NumPadIssueCard.Visibility = Visibility.Visible;

            NumPadIssueCard.Desciption = $"Введіть Номер телефону";
            NumPadIssueCard.ValidationMask = "^[0-9]{10}$";
            NumPadIssueCard.Result = $"{PhoneIssueCard}";
            NumPadIssueCard.IsEnableComma = false;
            NumPadIssueCard.CallBackResult = (string res) =>
            {
                if (!string.IsNullOrEmpty(res))
                    (PhoneIssueCard, status) = PhoneCorrection(res);
                else
                {
                    PhoneIssueCard = string.Empty;
                    EnterPhoneIssueCard(sender, e);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PhoneIssueCard"));

                if (!status)
                {
                    BorderNumPadIssueCard.Visibility = Visibility.Visible;
                    NumPadIssueCard.Visibility = Visibility.Visible;
                }

            };
        }
        private (string, bool) PhoneCorrection(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return (phoneNumber, false);
            if (phoneNumber.IndexOf("38") == 0 && phoneNumber.Length == 12)
            {
                return (phoneNumber, true);
            }
            else
            {
                return ($"38{phoneNumber}", true);
            }


        }
        private void IssueNewCardButton(object sender, RoutedEventArgs e)
        {
            ClientNew clientNew = new ClientNew() { BarcodeCashier = AdminSSC?.BarCode, BarcodeClient = BarcodeIssueCard, IdWorkplace = Global.IdWorkPlace, Phone = PhoneIssueCard, DateCreate = DateTime.Now };
            eReturnClient r = eReturnClient.Error;
            Task.Run(async () =>
            {
                r = await Bl.ds.Send1CClientAsync(clientNew);
            }).Wait();

            //if (r != eReturnClient.Ok)
            //{
            CustomMessage.Show(r.GetDescription(), r != eReturnClient.Ok ? "Помилка! Карточка не збереглась на сервері!!!" : "Карточка успішно збережена.", eTypeMessage.Information);

            // MessageBox.Show(r.GetDescription(), r != eReturnClient.Ok ? "Помилка! Карточка не збереглась на сервері!!!" : "Карточка успішно збережена.");
            // }
            //else
            // {
            clientNew.State = r == eReturnClient.Ok ? 1 : 0;
            Bl.db.ReplaceClientNew(clientNew);
            if (r == eReturnClient.Ok)
                CancelIssueCard(null, null);
            //}

        }

        private void CancelIssueCard(object sender, RoutedEventArgs e)
        {
            SetStateView(eStateMainWindows.WaitInput);
            PhoneIssueCard = string.Empty;
            BarcodeIssueCard = string.Empty;
            VerifyCode = string.Empty;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBarcodeIssueCard)));

        }

        private void EnterVerifyCode(object sender, RoutedEventArgs e)
        {
            BorderNumPadIssueCard.Visibility = Visibility.Visible;
            NumPadIssueCard.Visibility = Visibility.Visible;

            NumPadIssueCard.Desciption = $"Введіть код підтвердження";
            NumPadIssueCard.ValidationMask = "";
            NumPadIssueCard.Result = $"";
            NumPadIssueCard.IsEnableComma = false;
            NumPadIssueCard.CallBackResult = (string res) =>
            {
                if (!string.IsNullOrEmpty(res))
                    VerifyCode = res;
                else
                {
                    VerifyCode = string.Empty;
                    //EnterVerifyCode(sender, e);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VerifyCode)));
                if (LastVerifyCode.Data == VerifyCode)
                    IsGetCard = true;
                else
                {
                    IsGetCard = false;
                    if (!string.IsNullOrEmpty(VerifyCode))
                        CustomMessage.Show($"Введений код не вірний!", "Помилка!", eTypeMessage.Error);
                    // MessageBox.Show($"Введений код не вірний!", "Помилка!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGetCard)));
            };
        }

        private void ButSendVerifyCode(object sender, RoutedEventArgs e)
        {

            LastVerifyCode = Bl.ds.GetVerifySMS(PhoneIssueCard);
            //LastVerifyCode = new StatusD<string> { Data = "123456", State = 0, TextState = "OK" };
            CustomMessage.Show($"Код підтвердження надіслано за номером {PhoneIssueCard}", "Успішно!", eTypeMessage.Information);
            //MessageBox.Show($"Код підтвердження надіслано за номером {PhoneIssueCard}", "Увага!", MessageBoxButton.OK, MessageBoxImage.Information);
            EnterVerifyCode(null, null);
        }

        private void CodeSMSUseBonusBtn(object sender, RoutedEventArgs e)
        {
            BorderNumPadUseBonus.Visibility = Visibility.Visible;
            NumPadUseBonus.Visibility = Visibility.Visible;
            BackgroundWaitAdmin.Visibility = Visibility.Visible;

            NumPadUseBonus.Desciption = $"Введіть код підтвердження";
            NumPadUseBonus.ValidationMask = "";
            NumPadUseBonus.Result = $"";
            NumPadUseBonus.IsEnableComma = false;
            NumPadUseBonus.CallBackResult = (string res) =>
                          {
                              if (!string.IsNullOrEmpty(res))
                                  VerifyCode = res;
                              else
                              {
                                  VerifyCode = string.Empty;
                              }
                              PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VerifyCode)));
                              if (LastVerifyCode.Data == VerifyCode)
                                  SetConfirm(AdminSSC, false, true);

                              else
                              {
                                  CustomMessage.Show($"Введений код не вірний!", "Помилка!", eTypeMessage.Error);
                                  //MessageBox.Show($"Введений код не вірний!", "Помилка!", MessageBoxButton.OK, MessageBoxImage.Error);
                              }
                              BackgroundWaitAdmin.Visibility = Visibility.Collapsed;
                          };
        }
    }

}
