﻿using System;
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
using Front.Control;
using System.Windows.Threading;
using System.Windows.Input;
using Equipments.Model;
using Pr = Equipments.Model.Price;
using LibVLCSharp.Shared;
using Equipments;
using Front.ViewModels;

namespace Front
{
    public partial class MainWindow : Window, INotifyPropertyChanged, IMW
    {
        public string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }
        public event PropertyChangedEventHandler PropertyChanged;
        public Access Access = Access.GetAccess();
        public BL Bl { get; set; } = null;
        public BLF Blf { get; set; } = null;
        public EquipmentFront EF { get; set; } = null;
        public ControlScale CS { get; set; }
        Action<eCommand, WorkPlace, Status> SocketAnsver;
        public WorkPlace MainWorkplace { get; set; } = new();

        //WorkPlace _RemoteWorkplace = new();
        public WorkPlace RemoteWorkplace { get; set; } = new();//{ get { return _RemoteWorkplace; } set { _RemoteWorkplace = value; OnPropertyChanged(nameof(RemoteWorkplace)); } } 
        public Sound s { get; set; }
        public string Clock { get; set; } = DateTime.Now.ToShortDateString();
        public User AdminSSC { get; set; } = null;
        public DateTime DTAdminSSC { get; set; }
        public Receipt curReceipt { get; set; } = null;
        public Receipt ReceiptPostpone = null;
        bool _IsWaitAdminTitle = false;
        public bool IsWaitAdminTitle { get => _IsWaitAdminTitle; set { _IsWaitAdminTitle = value; OnPropertyChanged(nameof(IsWaitAdminTitle)); } }
        /// <summary>
        /// Можливість правки кількості для замовлень
        /// </summary>
        public bool IsOrderReceipt { get { return !string.IsNullOrEmpty(curReceipt?.NumberOrder); } }
        public bool IsReceiptPostponeNotNull { get { return ReceiptPostpone == null; } }
        public bool IsFullReturn = false;
        public bool IsReceiptPostpone { get { return ReceiptPostpone == null || (curReceipt == null || curReceipt.Wares == null || !curReceipt.Wares.Any()); } }
        public ReceiptWares CurWares { get; set; } = null;
        public Client Client { get { return curReceipt?.Client; } }
        public string ClientName { get { return curReceipt != null && curReceipt.CodeClient == Client?.CodeClient ? Client?.NameClient : (string)Application.Current.FindResource("BonusCard"); } }
        public List<string> ClientPhoneNumvers = new List<string>();
        public Visibility IsViewClientInfo { get { return Client == null ? Visibility.Collapsed : Visibility.Visible; } }
        public GW CurW { get; set; } = null;
        public eStateMainWindows State { get; set; } = eStateMainWindows.StartWindow;
        public eTypeAccess TypeAccessWait { get; set; }
        public ObservableCollection<ReceiptWares> ListWares { get; set; }
        public CustomWindow customWindow { get; set; }
        public string WaresQuantity { get { return curReceipt?.Wares?.Count().ToString() ?? "0"; } }
        public decimal MoneySum { get { return EF.SumReceiptFiscal(curReceipt); } }
        public string EquipmentInfo { get; set; } = "test";
        bool _Volume = true;
        public bool Volume { get { return _Volume; } set { _Volume = value; if (s != null) s.IsSound = value; } }
        /// <summary>
        /// Треба переробити без цієї змінної!!!!
        /// </summary>
        public bool IsShowWeightWindows { get; set; } = false;
        public bool IsConfirmAdmin { get; set; }
        public bool IsExciseStamp { get; set; }
        public bool IsCheckReturn { get { return curReceipt?.TypeReceipt == eTypeReceipt.Refund ? true : false; } }
        public bool IsCheckPaid { get { return curReceipt?.StateReceipt == eStateReceipt.Pay ? true : false; } }

        SocketServer SocketServer;
        /// <summary>
        /// Чи заброкована зміна
        /// </summary>
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
        public bool IsEnabledPaymentButton
        {
            get
            {
                return (MoneySum >= 0 && WaresQuantity != "0" && (IsAddNewWares || State == eStateMainWindows.FindClientByPhone))
                    || (curReceipt?.TypeReceipt == eTypeReceipt.Refund && MoneySum > 0)
                    || curReceipt?.StateReceipt == eStateReceipt.Pay;
            }
        }

        /// <summary>
        /// чи активна кнопка пошуку
        /// </summary>
        public bool IsEnabledFindButton { get { return IsAddNewWares || State == eStateMainWindows.FindClientByPhone; } }
        public bool IsWeightMagellan { get { return Weight > 0 ? true : false; } }
        /// <summary>
        /// Чи можна підтвердити власну сумку
        /// </summary>
        public bool IsOwnBag { get { return ControlScaleCurrentWeight > 0 && ControlScaleCurrentWeight <= Global.MaxWeightBag; } }
        public string StrControlScaleCurrentWeightKg { get { return (ControlScaleCurrentWeight / 1000).ToString("N3"); } }
        public bool IsPresentFirstTerminal { get { return EF.BankTerminal1 != null && EF.BankTerminal1.State == eStateEquipment.On; } }
        public bool IsPresentSecondTerminal { get { return EF.BankTerminal2 != null; } }

        //bool IsViewProblemeWeight { get { return State == eStateMainWindows.WaitInput || State == eStateMainWindows.StartWindow; } }
        /// <summary>
        /// Чи треба вибирати ціну.
        /// </summary>
        bool IsChoicePrice { get { return CurWares != null && CurWares.IsMultiplePrices && curReceipt != null && curReceipt.GetLastWares?.CodeWares != CurWares.CodeWares && curReceipt.Equals(CurWares); } }
        /// <summary>
        /// теперішня вага
        /// </summary>
        public double ControlScaleCurrentWeight { get; set; } = 0d;
        /// <summary>
        /// Чи показувати супутні товари
        /// </summary>
        public bool IsShowRelatedProducts
        {
            get
            {
                if (curReceipt?.GetLastWares?.IsWaresLink == true)
                {
                    RelatedProductsUC.AddRelatedProducts(curReceipt?.GetLastWares);
                    ScrolDown();
                    return true;
                }
                else
                {
                    ScrolDown();
                    return false;
                }
                //return false;
            }
        }

        public int QuantityCigarettes { get; set; } = 1;
        public BankTerminal FirstTerminal { get { return IsPresentFirstTerminal ? EF?.BankTerminal1 : null; } }
        public BankTerminal SecondTerminal { get { return IsPresentSecondTerminal ? EF?.BankTerminal2 : null; } }
        public string GetBackgroundColor { get { return curReceipt?.TypeReceipt == eTypeReceipt.Refund ? "#ff9999" : "#FFFFFF"; } }
        public double GiveRest { get; set; } = 0;
        public string VerifyCode { get; set; } = string.Empty;
        public Status<string> LastVerifyCode { get; set; } = new();
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
        public bool IsSecondMonitor { get; set; } = false;
        SecondMonitorWindows SecondMonitorWin;

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
        InfoRemoteCheckout _RemoteCheckout = new();
        public InfoRemoteCheckout RemoteCheckout
        {
            get { return _RemoteCheckout; }
            set
            {
                _RemoteCheckout = value;
                if (RemoteCheckout.RemoteCigarettesPrices.Count > 1)
                    RemotePrices.ItemsSource = RemoteCheckout.RemoteCigarettesPrices;
                RemoteWorkplace = Bl.db.GetWorkPlace().FirstOrDefault(el => el.IdWorkplace == RemoteCheckout.RemoteIdWorkPlace);
                OnPropertyChanged(nameof(RemoteWorkplace));
                OnPropertyChanged(nameof(RemoteCheckout));
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
        public string[] PathVideo { get; set; } = null;
        public bool IsManyPayments { get { return curReceipt?.IdWorkplacePays?.Length > 1; } }
        public string AmountManyPayments
        {
            get
            {
                var res = "";
                if (curReceipt?.WorkplacePays?.Any() == true)
                {
                    foreach (var item in curReceipt.WorkplacePays)
                        res += $"{item.Sum} | ";
                    res = res.Substring(0, res.Length - 2);
                }
                return res;
            }
        }
        public string SumTotalManyPayments { get { return $"Загальна сума: {curReceipt?.SumTotal ?? 0}₴"; } }
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
                    WaitAdminTitle.Text = (string)Application.Current.FindResource("WaitAdmin");   //"Будь ласка очікуйте охорону!";
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
                        IsWaitAdminTitle = false;
                        //WaitAdminTitle.Visibility = Visibility.Collapsed;
                        tb.Inlines.Add("Зміна заблокована");
                        break;
                    case eTypeAccess.FixWeight:
                        tb.Inlines.Add(new Run(CS.Info) { FontWeight = FontWeights.Bold, Foreground = Brushes.Red, FontSize = 32 });
                        tb.Inlines.Add(new Run(CS.InfoEx) { Foreground = Brushes.Black, FontSize = 20 });
                        break;
                    case eTypeAccess.ConfirmAge:
                        IsWaitAdminTitle = false;
                        //WaitAdminTitle.Visibility = Visibility.Collapsed;
                        WaitAdminImage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/18PlusRed.png"));
                        tb.Inlines.Add(new Run(IsCashRegister ? "Клієнту виповнилось 18?" : "Вам виповнилось 18 років?") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red, FontSize = 32 });
                        break;
                    case eTypeAccess.ExciseStamp:
                        IsWaitAdminTitle = false;
                        // WaitAdminTitle.Visibility = Visibility.Collapsed;
                        tb.Inlines.Add(new Run("Відскануйте акцизну марку!") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red, FontSize = 32 });
                        break;
                    case eTypeAccess.UseBonus:
                        IsWaitAdminTitle = false;
                        //WaitAdminTitle.Visibility = Visibility.Collapsed;
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
            DataContext = this;
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Ver={Version}", eTypeLog.Expanded);
            Bl = new();
            Blf = BLF.GetBLF;
            Blf.Init(this);
            EF = EquipmentFront.GetEF;
            CS = new ControlScale(this, 10d);
            s = Sound.GetSound(CS);

            if (Global.PortAPI > 0)
            {
                SocketServer = new SocketServer(Global.PortAPI, Blf.CallBackApi);
                _ = SocketServer.StartAsync();
            }
            SocketAnsver += (Command, WorkPlace, Ansver) =>
            {
                FileLogger.WriteLogMessage($"SocketAnsver: {Environment.NewLine}Command: {Command} {Environment.NewLine}WorkPlaceName: {WorkPlace.Name}{Environment.NewLine}IdWorkPlace: {WorkPlace.IdWorkplace}{Environment.NewLine}Ansver: {Ansver.TextState}", eTypeLog.Full);
            };

            Volume = (Global.TypeWorkplaceCurrent == eTypeWorkplace.SelfServicCheckout);
            var fc = new List<FlagColor>();
            Config.GetConfiguration().GetSection("MID:FlagColor").Bind(fc);
            foreach (var el in fc)
                if (!FC.ContainsKey(el.State))
                    FC.Add(el.State, el.Color);

            KeyUp += SetKey;
            InitAction();
            calculateWidthHeaderReceipt(TypeMonitor);

            InitializeComponent();
            MainWorkplace = Bl.db.GetWorkPlace().FirstOrDefault(el => el.CodeWarehouse == Global.CodeWarehouse && el.IdWorkplace == Global.Settings.IdWorkPlaceMain);

            //поточний час
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            string DirName = Path.Combine(Global.PathPictures, "Video");
            if (Directory.Exists(DirName))
                PathVideo = Directory.GetFiles(DirName);

            /*if (PathVideo != null && PathVideo.Length != 0)
            {                
                   StartVideo.Source = new Uri(PathVideo[0]);
                   StartVideo.Play();
                   StartVideo.MediaEnded += (object sender, RoutedEventArgs e) =>
                   {
                       StartVideo.Position = new TimeSpan(0, 0, 0, 0, 1);
                       StartVideo.Play();
                   };
            }*/

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
            IssueCardUC.Init(this);
            PhoneVerificationUC.Init(this);
            ClientDetailsUC.Init(this);
            WeightWaresUC.Init(this);
            PaymentWindowKSO_UC.Init(this);
            RelatedProductsUC.Init(this);


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
                    //return;
                }
                else
                    SetStateView(eStateMainWindows.StartWindow);
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
                }
                catch (Exception ex)
                {
                    var ms = ex.Message;
                }
                SetStateView(eStateMainWindows.StartWindow);
            }

            // Спочатку перевірте наявність додаткових екранів
            if (System.Windows.Forms.Screen.AllScreens.Length > 1 && Global.TypeWorkplace == eTypeWorkplace.Both)
            {
                IsSecondMonitor = true;
                // Отримайте інформацію про всі екрани
                System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;

                // Виберіть другий екран (індекс 1, бо нульовий екран - це основний)
                System.Windows.Forms.Screen additionalScreen = screens[1];

                // Створіть новий об'єкт вікна SecondMonitorWindows
                SecondMonitorWin = new SecondMonitorWindows(this);

                // Встановіть розміри вікна на весь екран додаткового монітора
                SecondMonitorWin.WindowStartupLocation = WindowStartupLocation.Manual;
                SecondMonitorWin.Left = additionalScreen.WorkingArea.Left;
                SecondMonitorWin.Top = additionalScreen.WorkingArea.Top;
                SecondMonitorWin.Width = additionalScreen.WorkingArea.Width;
                SecondMonitorWin.Height = additionalScreen.WorkingArea.Height;

                // Покажіть вікно
                SecondMonitorWin.Show();
                SecondMonitorWin.WindowState = WindowState.Maximized;
            }

            SetWorkPlace();
            Task.Run(() => Bl.ds.SyncDataAsync());
            Loaded += MainWindow_Loaded;
            //Loaded += (a, b) => VideoView_Loaded(); // { IsLoading = true; StarVideo(); };

            Global.OnClientChanged += (pClient) =>
            {
                if (string.IsNullOrEmpty(pClient.MainPhone))
                {
                    CustomMessage.Show("Відсутній номер телефону! Додати номер телефону для отримання унікальних пропозицій?", "Відсутній номер телефону", eTypeMessage.Question);
                    CustomMessage.Result = (bool res) =>
                    {
                        if (res) 
                        {
                            SetStateView(eStateMainWindows.PhoneVerification);
                        }
                    };
                }
            };
        }

        // bool IsLoading = false;
        //string ss = "";
        public void SetKey(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            var aa = key.ToString();
            // ss += $"{e.Device}  {e.InputSource} {e.Handled} {e.SystemKey} {e.KeyStates} {e.DeadCharProcessedKey} {e.ImeProcessedKey} {e.IsDown } {e.IsUp} {key} {aa} {Environment.NewLine}";
            if (!(key == Key.Enter || key == Key.Return || key == Key.LeftShift || key == Key.RightShift || ((int)key >= 34 && (int)key <= 69)))
                return;
            var Ch = aa.Length == 2 && aa[0] == 'D' ? aa[1] : aa[0];
            EF.SetKey((int)key, Ch);
        }


        public LibVLC LibVLC = null;
        public Media Media = null;
        public LibVLCSharp.Shared.MediaPlayer MediaPlayer;
        //LibVLCSharp.Shared.MediaPlayer MediaPlayer = null;
        bool? IsBild = null;
        public string[] _videoFiles;
        public int _currentVideoIndex = 0;
        private readonly string _videoDirectory = Path.Combine(Global.PathPictures, "Video");
        //private void VideoView_Loaded(object sender = null, RoutedEventArgs e = null)
        //{
        //    if (IsBild == null && PathVideo?.Length > 0)
        //    {
        //        IsBild = false;
        //        LibVLC = new LibVLC(); //enableDebugLogs: true);                
        //        MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(LibVLC);
        //        //LoadVideos();
        //        //MediaPlayer.EndReached += MediaPlayer_EndReached;
        //        //MediaPlayer.EndReached += (s, e) => Environment.Exit(0);
        //        //MediaPlayer.PositionChanged //= new System.EventHandler<Vlc.DotNet.Core.VlcMediaPlayerPositionChangedEventArgs>(this.vlcControl1_PositionChanged);
        //        //MediaPlayer.Playing                
        //        Media = new Media(LibVLC, new Uri(PathVideo[0]));// "D:\\pictures\\Video\\1.mp4"));
        //        //MediaList list = new MediaList(LibVLC);
        //        //list.AddMedia(Media);
        //        //list.SetMedia()

        //        Media.AddOption(":input-repeat=65535");
        //        Task.Delay(1000);
        //        IsBild = true;
        //        StarVideo();
        //    }

        //}
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Core.Initialize();
            LibVLC = new LibVLC();
            MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(LibVLC);
            //StartVideo.MediaPlayer = MediaPlayer;

            LoadVideos();
            PlayNextVideo(MediaPlayer, true);

            MediaPlayer.EndReached += MediaPlayer_EndReached;
            IsBild = true;
        }
        public void LoadVideos()
        {
            if (Directory.Exists(_videoDirectory))
            {
                _videoFiles = Directory.GetFiles(_videoDirectory, "*.mp4");
            }
            else
            {
                _videoFiles = Array.Empty<string>();
            }
        }

        public async void PlayNextVideo(LibVLCSharp.Shared.MediaPlayer Player, bool IsFirstStart = false)
        {
            IsBild = false;
            if (_videoFiles.Length == 0) return;

            // Зупиняємо відтворення перед зміною відео
            // MediaPlayer.Stop();

            // Очищаємо попереднє медіа (важливо!)
            Player?.Media?.Dispose();

            // Робимо невелику затримку, щоб уникнути зависання
            await Task.Delay(300);

            // Вибираємо наступне відео
            var videoPath = _videoFiles[_currentVideoIndex];
            Media = new Media(LibVLC, new Uri(videoPath));

            // Встановлюємо нове медіа та відтворюємо його
            Player.Media = Media;
            if (Global.TypeWorkplace != eTypeWorkplace.CashRegister && !IsFirstStart)
                Player.Play();
            IsBild = true;
        }

        public void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _currentVideoIndex = (_currentVideoIndex + 1) % _videoFiles.Length;
                PlayNextVideo(MediaPlayer);
            });
        }

        void StarVideo(eStateMainWindows? pState = null)
        {
            if (pState == null)
                pState = State;

            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                if (StartVideo != null && StartVideo.MediaPlayer == null && IsBild == true && !IsCashRegister && pState == eStateMainWindows.StartWindow)
                //if (StartVideo.Visibility == Visibility.Visible)
                {
                    StartVideo.MediaPlayer = MediaPlayer;
                    StartVideo.MediaPlayer.Play(Media);
                }
                if (StartVideo.MediaPlayer != null) StopStartVideo(pState);

            }));
        }

        private void StopStartVideo(eStateMainWindows? pState)
        {
            if (StartVideo?.MediaPlayer != null)
            {
                if (pState != eStateMainWindows.StartWindow && StartVideo.MediaPlayer.IsPlaying)
                {
                    StartVideo.MediaPlayer.SetPause(true);
                    StartVideo.Visibility = Visibility.Collapsed;
                }
                if (pState == eStateMainWindows.StartWindow && !IsCashRegister && !StartVideo.MediaPlayer.IsPlaying)
                {
                    StartVideo.Visibility = Visibility.Visible;
                    StartVideo.MediaPlayer.SetPause(false);
                }
            }
        }

        /*
        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            return;
            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                StartVideo.MediaPlayer.Pause();
                StartVideo.MediaPlayer.Play(media);
            }
            ));
        }*/

        public void SetWorkPlace()
        {
            Volume = (Global.TypeWorkplaceCurrent == eTypeWorkplace.SelfServicCheckout);
            OnPropertyChanged(nameof(IsCashRegister));
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Clock = DateTime.Now.ToShortTimeString();
            OnPropertyChanged(nameof(Clock));
        }

        public void SetCurReceipt(Receipt pReceipt, bool IsRefresh = true)
        {
            try
            {
                var OldClient = curReceipt?.Client;
                curReceipt = new(); //Через дивний баг коли curReceipt.Wares залишалось порожне. а в pReceipt було з записами.
                curReceipt = pReceipt;

                if (curReceipt == null)
                {
                    Dispatcher.BeginInvoke(new ThreadStart(() => { ListWares?.Clear(); }));
                    CS.WaitClear();
                }
                else
                {
                    Dispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        if (pReceipt.Wares?.Any() == true)
                            ListWares = new ObservableCollection<ReceiptWares>(pReceipt.Wares);
                        else
                            ListWares?.Clear();

                        WaresList.ItemsSource = ListWares;
                        if (WaresList.Items.Count > 0)
                            WaresList.SelectedIndex = WaresList.Items.Count - 1;
                        ScrolDown();
                    }));
                    if (OldClient?.CodeClient != 0 && curReceipt.CodeClient != 0 && curReceipt.Client == null && OldClient.CodeClient == curReceipt.CodeClient)
                    {
                        curReceipt.Client = OldClient;
                    }

                    if (curReceipt.CodeClient != 0 && string.IsNullOrEmpty(curReceipt.Client?.NameClient))
                        Bl.GetClientByCode(curReceipt, curReceipt.CodeClient);

                    if (curReceipt?.IsNeedExciseStamp == true)
                        SetStateView(eStateMainWindows.WaitInput);
                    SetClient();
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
            OnPropertyChanged(nameof(GetBackgroundColor));
            OnPropertyChanged(nameof(IsEnabledPaymentButton));
            OnPropertyChanged(nameof(IsCheckReturn));
            OnPropertyChanged(nameof(IsCheckPaid));
            OnPropertyChanged(nameof(ClientName));
            OnPropertyChanged(nameof(IsAgeRestrict));
            OnPropertyChanged(nameof(IsEnabledFindButton));
            OnPropertyChanged(nameof(IsLockSale));
            OnPropertyChanged(nameof(IsAddNewWares));
            OnPropertyChanged(nameof(WaitAdminText));
            OnPropertyChanged(nameof(CS));
            OnPropertyChanged(nameof(Client));
            OnPropertyChanged(nameof(MoneySum));
            OnPropertyChanged(nameof(WaresQuantity));
            OnPropertyChanged(nameof(IsReceiptPostpone));
            OnPropertyChanged(nameof(IsReceiptPostponeNotNull));
            OnPropertyChanged(nameof(IsOrderReceipt));
            OnPropertyChanged(nameof(IsManyPayments));
            OnPropertyChanged(nameof(AmountManyPayments));
            OnPropertyChanged(nameof(SumTotalManyPayments));
            OnPropertyChanged(nameof(IsShowRelatedProducts));
            if (IsSecondMonitor)
                SecondMonitorWin.UpdateSecondMonitor();
            SetClient();

        }

        DateTime StartScan = DateTime.MinValue;
        public void SetStateView(eStateMainWindows pSMV = eStateMainWindows.NotDefine, eTypeAccess pTypeAccess = eTypeAccess.NoDefine, ReceiptWares pRW = null, CustomWindow pCW = null, eSender pS = eSender.NotDefine)
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Start pSMV={pSMV}/{State}, pTypeAccess={pTypeAccess}/{TypeAccessWait}, pRW ={pRW} , pCW={pCW},  pS={pS}", eTypeLog.Full);
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
                    if (((EF.StatCriticalEquipment != eStateEquipment.On || !Bl.ds.IsReady || IsLockSale || CS.IsProblem || curReceipt?.IsNeedExciseStamp == true || IsChoicePrice) &&
                       pSMV != eStateMainWindows.WaitAdminLogin && pSMV != eStateMainWindows.AdminPanel) && pSMV != eStateMainWindows.ChangeCountWares)
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

                    /*if (pSMV != eStateMainWindows.StartWindow && State == eStateMainWindows.StartWindow)
                        StartVideo.Pause();
                    if (pSMV == eStateMainWindows.StartWindow && State != eStateMainWindows.StartWindow && IsCashRegister == false)
                        StartVideo.Play();*/

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

                    Blf.TimeScan();

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

                    if (MainWorkplace != null)
                        Task.Run(async () =>
                        {
                            ObservableCollection<Pr> prices = new();
                            if (CurWares?.Prices?.Any() == true)
                            {
                                prices = new ObservableCollection<Pr>(CurWares.Prices.OrderByDescending(r => r.Price).Select(r => new Pr(r.Price, true, r.TypeWares)));
                            }
                            InfoRemoteCheckout remoteInfo = new() { StateMainWindows = pSMV, TypeAccess = TypeAccessWait, TextInfo = $"{CS.InfoEx}", UserBarcode = AdminSSC?.BarCode, RemoteCigarettesPrices = prices };
                            CommandAPI<InfoRemoteCheckout> Command = new() { Command = eCommand.GeneralCondition, Data = remoteInfo };

                            try
                            {
                                var r = new SocketClient(MainWorkplace.IP, Global.PortAPI);
                                var Ansver = await r.StartAsync(Command.ToJSON());
                                SocketAnsver?.Invoke(eCommand.GeneralCondition, MainWorkplace, Ansver);
                            }
                            catch (Exception ex)
                            {
                                FileLogger.WriteLogMessage(this, $"GeneralCondition DNSName=>{MainWorkplace.DNSName} {Command} ", ex);
                                SocketAnsver?.Invoke(eCommand.GeneralCondition, MainWorkplace, new Status(ex));
                            }
                        });

                    ExciseStamp.Visibility = Visibility.Collapsed;
                    ChoicePrice.Visibility = Visibility.Collapsed;
                    Background.Visibility = Visibility.Collapsed;
                    BackgroundWares.Visibility = Visibility.Collapsed;
                    WaitAdmin.Visibility = Visibility.Collapsed;
                    WaitAdminLogin.Visibility = Visibility.Collapsed;
                    WeightWaresUC.Visibility = Visibility.Collapsed;
                    PaymentWindowKSO_UC.Visibility = Visibility.Collapsed;
                    PaymentWindowKSO_UC_2.Visibility = Visibility.Collapsed;
                    StartShopping.Visibility = Visibility.Collapsed;
                    StartShoppingLogo.Visibility = Visibility.Collapsed;
                    StartShoppingButtons.Visibility = Visibility.Collapsed;
                    ConfirmAge.Visibility = Visibility.Collapsed;
                    CustomWindows.Visibility = Visibility.Collapsed;
                    ErrorBackground.Visibility = Visibility.Collapsed;
                    OwnBagWindows.Visibility = Visibility.Collapsed;
                    PaymentWindow.Visibility = Visibility.Collapsed;
                    ClientDetailsUC.Visibility = Visibility.Collapsed;
                    RemoteCashRegister.Visibility = Visibility.Collapsed;

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
                    IsWaitAdminTitle = true;
                    //WaitAdminTitle.Visibility = Visibility.Visible;
                    CodeSMS.Visibility = Visibility.Collapsed;
                    WaitAdminWeightButtons.Visibility = Visibility.Visible;
                    WaitAdminAdditionalText.Visibility = Visibility.Collapsed;
                    IssueCardUC.Visibility = Visibility.Collapsed;
                    PhoneVerificationUC.Visibility = Visibility.Collapsed;
                    AdminControl.Visibility = (State == eStateMainWindows.AdminPanel ? Visibility.Visible : Visibility.Collapsed);
                    AddMissingPackage.Visibility = Visibility.Collapsed;

                    switch (State)
                    {
                        case eStateMainWindows.StartWindow:
                            if (PathVideo != null && PathVideo.Length != 0 && IsCashRegister == false)
                            {
                                StartShoppingButtons.Visibility = Visibility.Visible;
                                StartShopping.Visibility = Visibility.Visible;
                            }
                            else
                                StartShoppingLogo.Visibility = Visibility.Visible;

                            break;
                        case eStateMainWindows.WaitInputPrice:
                            TypeAccessWait = eTypeAccess.ChoicePrice;
                            if (CurWares?.Prices?.Any() == true)
                            {
                                var rrr = new ObservableCollection<Pr>(CurWares.Prices.OrderByDescending(r => r.Price).Select(r => new Pr(r.Price, Access.GetRight(TypeAccessWait), r.TypeWares)));
                                rrr.First().IsEnable = true;
                                Prices.ItemsSource = rrr;
                            }
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            ChoicePrice.Visibility = Visibility.Visible;
                            break;

                        case eStateMainWindows.WaitWeight:
                            EF.StartWeight();
                            WeightWaresUC.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            break;
                        case eStateMainWindows.WaitAdmin:
                            WaitAdmin.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            TBExciseStamp.IsReadOnly = false;
                            if (customWindow?.Buttons != null && customWindow.Buttons.Count > 0)
                                WaitAdminCancel.Visibility = Visibility.Collapsed;

                            switch (TypeAccessWait)
                            {
                                case eTypeAccess.FixWeight:
                                    if (CS.StateScale == eStateScale.WaitClear)
                                    {
                                        IsWaitAdminTitle = false;
                                        //WaitAdminTitle.Visibility = Visibility.Collapsed;
                                        WaitAdminImage.Visibility = Visibility.Collapsed;
                                    }
                                    break;
                                case eTypeAccess.AddNewWeight:
                                    break;
                                case eTypeAccess.ExciseStamp:
                                    //OnPropertyChanged(nameof(CurWaresName));
                                    TBExciseStamp.Text = "";
                                    WaitAdminImage.Visibility = Visibility.Collapsed;
                                    WaitAdminCancel.Visibility = Visibility.Collapsed;
                                    TBExciseStamp.Visibility = Visibility.Visible;
                                    KBAdmin.Visibility = Visibility.Visible;
                                    KBAdmin.SetInput(TBExciseStamp);
                                    ExciseStampButtons.Visibility = Visibility.Visible;
                                    ExciseStampNameWares.Visibility = Visibility.Visible;
                                    IsWaitAdminTitle = false;
                                    //WaitAdminTitle.Visibility = Visibility.Collapsed;
                                    CodeSMS.Visibility = Visibility.Collapsed;
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
                                        IsWaitAdminTitle = false;
                                        //WaitAdminTitle.Visibility = Visibility.Collapsed;
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
                            break;
                        case eStateMainWindows.WaitAdminLogin:
                            KB.SetInput(null);
                            LoginTextBlock.Text = "";
                            PasswordTextBlock.Password = "";
                            WaitAdminLogin.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            LoginTextBlock.Focus();
                            break;
                        case eStateMainWindows.WaitFindWares:
                            FindWaresWin FWW = new FindWaresWin(this);

                            if (IsSecondMonitor)
                            {
                                System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;

                                // Виберіть другий екран (індекс 1, бо нульовий екран - це основний)
                                System.Windows.Forms.Screen additionalScreen = Global.TypeWorkplaceCurrent == eTypeWorkplace.CashRegister ? screens[0] : screens[1];

                                FWW.WindowStartupLocation = WindowStartupLocation.Manual;
                                FWW.Left = additionalScreen.WorkingArea.Left;
                                FWW.Top = additionalScreen.WorkingArea.Top;
                                FWW.Width = additionalScreen.WorkingArea.Width;
                                FWW.Height = additionalScreen.WorkingArea.Height;

                                // Покажіть вікно
                                FWW.Show();
                                FWW.WindowState = WindowState.Maximized;

                            }
                            else
                            {

                                FWW.WindowState = WindowState.Maximized;
                                FWW.Show();
                            }


                            break;
                        case eStateMainWindows.ProcessPay:
                            PaymentWindowKSO_UC.PaymentImage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/newPaymentTerminal.png"));
                            PaymentWindowKSO_UC.Visibility = Visibility.Visible;
                            if (false && IsManyPayments)
                            {
                                PaymentWindowKSO_UC_2.PaymentImage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/newPaymentTerminal.png"));
                                PaymentWindowKSO_UC_2.Visibility = Visibility.Visible;
                            }
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            break;
                        case eStateMainWindows.ProcessPrintReceipt:
                            PaymentWindowKSO_UC.PaymentImage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/newReceipt.png"));
                            PaymentWindowKSO_UC.Visibility = Visibility.Visible;
                            if (false && IsManyPayments)
                            {
                                PaymentWindowKSO_UC_2.PaymentImage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/newReceipt.png"));
                                PaymentWindowKSO_UC_2.Visibility = Visibility.Visible;
                            }
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            break;

                        case eStateMainWindows.ChoicePaymentMethod:
                            PaymentWindow.UpdatePaymentWindow();
                            OnPropertyChanged(nameof(IsPresentSecondTerminal));
                            OnPropertyChanged(nameof(IsPresentFirstTerminal));
                            OnPropertyChanged(nameof(SecondTerminal));
                            OnPropertyChanged(nameof(FirstTerminal));

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
                            OnPropertyChanged(nameof(customWindow));
                            CustomWindows.Visibility = Visibility.Visible;
                            TextBoxCustomWindows.Focus();
                            break;
                        case eStateMainWindows.BlockWeight:
                            TypeAccessWait = eTypeAccess.FixWeight;
                            break;
                        case eStateMainWindows.WaitInputIssueCard:
                            IssueCardUC.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            IssueCardUC.ButPhoneIssueCard.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                            break;
                        case eStateMainWindows.PhoneVerification:
                            PhoneVerificationUC.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            PhoneVerificationUC.ButConfirmPhone.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                            PhoneVerificationUC.RefreshClient();
                            break;
                        case eStateMainWindows.WaitInput:
                            WaresList.Focus(); //Для сканера через імітацію клавіатури

                            break;
                        case eStateMainWindows.WaitOwnBag:
                            StartShopping.Visibility = Visibility.Collapsed;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            OwnBagWindows.Visibility = Visibility.Visible;
                            break;
                        case eStateMainWindows.FindClientByPhone:
                            s.Play(eTypeSound.ScanCustomerCardOrEnterPhone);
                            UC_NumericPad.Desciption = (string)Application.Current.FindResource("EnterPhoneNumber");//"Введіть номер телефону";
                            UC_NumericPad.ValidationMask = Global.Settings.IsUseCardSparUkraine ? "^\\d{4}$|^\\d{10}$|^\\d{12}$" : "^[0-9]{10,13}$";
                            UC_NumericPad.Result = "";
                            UC_NumericPad.IsEnableComma = false;
                            UC_NumericPad.CallBackResult = FindClientByPhone;
                            NumericPad.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            break;
                        case eStateMainWindows.ChangeCountWares:
                            UC_NumericPad.Desciption = Convert.ToString(pRW.NameWares);
                            UC_NumericPad.Result = "";//Convert.ToString(temp.Quantity);
                            UC_NumericPad.ValidationMask = "";
                            if (pRW.IsWeight) UC_NumericPad.IsEnableComma = true;
                            else UC_NumericPad.IsEnableComma = false;

                            NumericPad.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;

                            UC_NumericPad.CallBackResult = (string result) =>
                            {
                                decimal tempQuantity;
                                if (result != "" && result != "0")
                                {
                                    tempQuantity = result.ToDecimal();//Convert.ToDecimal(result);

                                    pRW.Quantity = tempQuantity;
                                    if (curReceipt?.TypeReceipt == eTypeReceipt.Refund && tempQuantity > pRW.MaxRefundQuantity)
                                    {
                                        pRW.Quantity = (decimal)pRW.MaxRefundQuantity;
                                    }
                                    if (curReceipt?.IsLockChange == false)
                                    {
                                        Bl.ChangeQuantity(pRW, pRW.Quantity);
                                    }
                                }
                                Background.Visibility = Visibility.Collapsed;
                                BackgroundWares.Visibility = Visibility.Collapsed;
                                SetStateView(eStateMainWindows.WaitInput);
                            };
                            break;
                        case eStateMainWindows.AddMissingPackage:
                            AddMissingPackage.CountPackeges = curReceipt.CountWeightGoods;
                            AddMissingPackage.CallBackResult = (int res) =>
                            {
                                Bl.AddEvent(curReceipt, eReceiptEventType.PackagesBag, res != 0 ? "Додавання пакетів в чек" : "Відміна додавання пакетів");
                                Bl.AddWaresCode(curReceipt, Global.Settings.CodePackagesBag, 19, res);
                                AddMissingPackage.Visibility = Visibility.Collapsed;
                                Background.Visibility = Visibility.Collapsed;
                                BackgroundWares.Visibility = Visibility.Collapsed;
                                Thread.Sleep(200);
                                Blf.PayAndPrint();
                            };
                            AddMissingPackage.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            break;
                        default:
                            break;
                    }
                    SetPropertyChanged();
                }));
                var res = r.Wait(new TimeSpan(0, 0, 0, 0, 200));

                StarVideo();
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"End res=>{res} pSMV={pSMV}/{State}, pTypeAccess={pTypeAccess}/{TypeAccessWait}, pRW ={pRW} , pCW={pCW},  pS={pS}", eTypeLog.Full);
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
        public void ChangePaymentWindows()
        {
            return;
            PaymentWindowKSO_UC.Width = 1000;
            PaymentWindowKSO_UC.Height = 550;
            PaymentWindowKSO_UC_2.Width = 1000;
            PaymentWindowKSO_UC_2.Height = 550;
            if (IsManyPayments && Global.TypeWorkplace == eTypeWorkplace.SelfServicCheckout && TypeMonitor == eTypeMonitor.VerticalMonitorKSO)
            {
                if (!curReceipt._Payment.Any())
                {
                    PaymentWindowKSO_UC.color = "#419e08";
                    PaymentWindowKSO_UC_2.color = "Gray";
                    PaymentWindowKSO_UC.Width = 1000;
                    PaymentWindowKSO_UC.Height = 550;
                    PaymentWindowKSO_UC_2.Width = 900;
                    PaymentWindowKSO_UC_2.Height = 500;
                }
                else
                {
                    PaymentWindowKSO_UC.Width = 900;
                    PaymentWindowKSO_UC.Height = 500;
                    PaymentWindowKSO_UC_2.Width = 1000;
                    PaymentWindowKSO_UC_2.Height = 550;
                    PaymentWindowKSO_UC_2.color = "#419e08";
                    PaymentWindowKSO_UC.color = "Gray";
                }
            }
            //PaymentWindowKSO_UC.Width = 900;
            //PaymentWindowKSO_UC.Height = 500;
            //PaymentWindowKSO_UC_2.Width = 900;
            //PaymentWindowKSO_UC_2.Height = 500;
            OnPropertyChanged(nameof(PaymentWindowKSO_UC));
            OnPropertyChanged(nameof(PaymentWindowKSO_UC_2));
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
                SetStateView(eStateMainWindows.ChangeCountWares, eTypeAccess.NoDefine, temp);
            }
        }

        private void _VolumeButton(object sender, RoutedEventArgs e) => Volume = !Volume;

        private void _ChangeLanguage(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            try
            {
                if (btn != null && btn.Tag is CultureInfo lang) App.Language = lang;
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
            OnPropertyChanged(nameof(ClientName));
        }

        private void _Back(object sender, RoutedEventArgs e) => CancelReceipt();
        public void CancelReceipt()
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

        private void _Search(object sender, RoutedEventArgs e) => SetStateView(eStateMainWindows.WaitFindWares);

        private void _ButtonHelp(object sender, RoutedEventArgs e) => SetStateView(Global.TypeWorkplaceCurrent == eTypeWorkplace.SelfServicCheckout || AdminSSC == null ? eStateMainWindows.WaitAdmin : eStateMainWindows.AdminPanel, eTypeAccess.AdminPanel);

        private void _OwnBag(object sender, RoutedEventArgs e)
        {
            if (ControlScaleCurrentWeight > 0 && ControlScaleCurrentWeight < Global.MaxWeightBag)
            {
                Blf.NewReceipt();
                Bl.AddOwnBag(curReceipt, Convert.ToDecimal(ControlScaleCurrentWeight));
                SetStateView(eStateMainWindows.WaitInput);
            }
        }

        private void _BuyBag(object sender, RoutedEventArgs e) => Bl.GetClientByPhone(curReceipt, "0664417744");


        private void _Cancel(object sender, RoutedEventArgs e)
        {
            QuantityCigarettes = 1;
            SetStateView(eStateMainWindows.WaitInput);
        }

        private void StartBuy(object sender, RoutedEventArgs e) => StartAddWares();
        public void StartAddWares()
        {
            Blf.NewReceipt();
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
            var task = Task.Run(() => Blf.PrintAndCloseReceipt());
        }

        //Тестовий варіант роботи замовлень!!!
        private void _ButtonPayment(object sender, RoutedEventArgs e) => Blf.PayAndPrint();

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
                    var price = btn.DataContext as Pr;

                    TextBlock Tb = btn.Content as TextBlock;
                    if (Tb != null)
                    {
                        if (price.IsConfirmAge)
                            Bl.AddEventAge(curReceipt);
                        Blf.AddWares(CurWares.CodeWares, CurWares.CodeUnit, QuantityCigarettes, price.price);
                        QuantityCigarettes = 1;
                    }
                }
            }
            catch { }
            SetStateView(eStateMainWindows.WaitInput);
        }

        private void ButtonAdmin(object sender, RoutedEventArgs e) => SetStateView(eStateMainWindows.WaitAdminLogin);

        private void LoginCancel(object sender, RoutedEventArgs e) => SetConfirm(null);

        private void LoginButton(object sender, RoutedEventArgs e)
        {
            if (LoginTextBlock.Text == string.Empty || PasswordTextBlock.Password == string.Empty)
            {
                CustomMessage.Show("Введіть коректний логін та пароль!", "Увага!", eTypeMessage.Warning);
                return;
            }
            var U = Bl.GetUserByLogin(LoginTextBlock.Text, PasswordTextBlock.Password);
            if (U == null)
            {
                CustomMessage.Show("Не вірний логін чи пароль", "Увага!", eTypeMessage.Warning);
                return;
            }
            Bl.OnAdminBarCode?.Invoke(U);
        }

        private void TextLoginChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void TextPasswordChanged(object sender, TextChangedEventArgs e)
        {
        }

        public void StartOpenMoneyBox() => EF.OpenMoneyBox();

        private void AddExciseStamp(object sender, RoutedEventArgs e)
        {
            Blf.AddExciseStamp(TBExciseStamp.Text);
            KBAdmin.SetInput(null);
        }

        private void ChangedExciseStamp(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            IsExciseStamp = !string.IsNullOrEmpty(Blf.GetExciseStamp(textBox.Text));
            if (IsExciseStamp)
                textBox.Text = textBox.Text.ToString().ToUpper();
        }

        private void ExciseStampNone(object sender, RoutedEventArgs e) => Blf.ExciseStampNone();

        private void CustomWindowClickButton(object sender, RoutedEventArgs e)
        {
            KBAdmin.SetInput(null);
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
            Blf.CustomWindowClickButton(res);
        }

        private void FindClientByPhoneClick(object sender, RoutedEventArgs e) => SetStateView(eStateMainWindows.FindClientByPhone);

        private void FindClientByPhone(string pResult)
        {
            if (curReceipt == null)
                Blf.NewReceipt();
            if (pResult.Length == 4)
            {
                if (int.TryParse(pResult.Substring(0, 4), out int res))
                    Bl.GetDiscount(new FindClient { PinCode = res }, curReceipt);
                return;
            }

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
            QuantityCigarettes = btn.Name.Equals("PlusCigarettesButton") || btn.Name.Equals("RemotePlusCigarettesButton") ? QuantityCigarettes + 1 : QuantityCigarettes - 1;
            OnPropertyChanged(nameof(QuantityCigarettes));
        }

        private void StartOwnBag(object sender, RoutedEventArgs e) => SetStateView(eStateMainWindows.WaitOwnBag);

        private void TextPasswordChanged(object sender, RoutedEventArgs e)
        {
        }

        private void ShowClientDetails(object sender, RoutedEventArgs e)
        {
            ClientDetailsUC.Visibility = Visibility.Visible;
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
                        Blf.TimeScan(true);
                        ReceiptPostpone = curReceipt;
                        Blf.NewReceipt();
                        WaresList.Focus();
                    }
                };
            }
            else
            {
                if (curReceipt == null || curReceipt.Wares?.Any() != true)
                {
                    Blf.TimeScan(false);
                    Global.OnReceiptCalculationComplete?.Invoke(ReceiptPostpone);
                    ReceiptPostpone = null;
                    WaresList.Focus();
                }
                else
                    CustomMessage.Show("Неможливо відновити чек не закривши текучий", "Увага!", eTypeMessage.Information);
            }
            OnPropertyChanged(nameof(IsReceiptPostpone));
            OnPropertyChanged(nameof(IsReceiptPostponeNotNull));
        }

        private (string, bool) PhoneCorrection(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return (phoneNumber, false);
            return (phoneNumber.IndexOf("38") == 0 && phoneNumber.Length == 12) ? (phoneNumber, true) : ($"38{phoneNumber}", true);
        }

        private void UpdateDB(object sender, RoutedEventArgs e)
        {
            CustomMessage.Show($"Запустити оновлення бази даних?{Environment.NewLine}{Bl.db.LastMidFile} {Bl.db.GetConfig<DateTime>("Load_Update")}", $"Оновлення бази даних", eTypeMessage.Question);
            CustomMessage.Result = (bool res) =>
            {
                if (res)
                    Task.Run(() => Bl.ds.SyncDataAsync());
            };
        }

        private void ShowRemoteState(object sender, RoutedEventArgs e)
        {
            RemoteCashRegister.Visibility = Visibility.Visible;
            Background.Visibility = Visibility.Visible;
            BackgroundWares.Visibility = Visibility.Visible;
        }

        private void ChangeStateRemoteCashRegister(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            eCommand comand = btn.Name == "DeleteRemoteReceipt" ? eCommand.DeleteReceipt : eCommand.Confirm;
            InfoRemoteCheckout remoteInfo = new() { StateMainWindows = RemoteCheckout.StateMainWindows, TypeAccess = RemoteCheckout.TypeAccess, UserBarcode = AdminSSC?.BarCode };
            if (comand == eCommand.DeleteReceipt)
            {
                CustomMessage.Show("Ви дійсно видалити чек?", "Видалення чеку", eTypeMessage.Question);
                CustomMessage.Result = (bool res) =>
                {
                    if (res)
                        SendRemoteComand(comand, remoteInfo, "DeleteReceipt");
                };
            }
            else
                SendRemoteComand(comand, remoteInfo, "Confirm");
        }

        private void AddPriceRemoteWares(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                var price = btn.DataContext as Pr;

                InfoRemoteCheckout remoteInfo = new()
                {
                    StateMainWindows = eStateMainWindows.WaitAdmin,
                    TypeAccess = eTypeAccess.ChoicePrice,
                    UserBarcode = AdminSSC?.BarCode,
                    SelectRemoteCigarettesPrice = price,
                    QuantityCigarettes = QuantityCigarettes
                };
                SendRemoteComand(eCommand.Confirm, remoteInfo, RemoteCheckout.TypeAccess.GetDescription());
            }
        }

        public void SendRemoteComand(eCommand comand, InfoRemoteCheckout remoteInfo, string LogText = "Confirm")
        {
            if (RemoteWorkplace != null)
                Task.Run(async () =>
                {
                    CommandAPI<InfoRemoteCheckout> Command = new() { Command = comand, Data = remoteInfo };
                    try
                    {
                        var r = new SocketClient(RemoteWorkplace.IP, Global.PortAPI);
                        var Ansver = await r.StartAsync(Command.ToJSON());
                        SocketAnsver?.Invoke(comand, MainWorkplace, Ansver);
                    }
                    catch (Exception ex)
                    {
                        FileLogger.WriteLogMessage(this, $"{LogText} DNSName=>{RemoteWorkplace.DNSName} {Command} ", ex);
                        SocketAnsver?.Invoke(comand, MainWorkplace, new Status(ex));
                    }
                });
        }

        private void IssueCardButton(object sender, RoutedEventArgs e) => SetStateView(eStateMainWindows.WaitInputIssueCard);

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
                OnPropertyChanged(nameof(VerifyCode));
                if (LastVerifyCode.Data == VerifyCode)
                    SetConfirm(AdminSSC, false, true);
                else
                    CustomMessage.Show($"Введений код не вірний!", "Помилка!", eTypeMessage.Error);

                BackgroundWaitAdmin.Visibility = Visibility.Collapsed;
            };
        }
        public void ScrolDown()
        {
            if (VisualTreeHelper.GetChildrenCount(WaresList) > 0)
            {
                Border border = (Border)VisualTreeHelper.GetChild(WaresList, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }
        private void OnPropertyChanged(String info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }
}
