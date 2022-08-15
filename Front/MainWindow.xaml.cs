using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
using Microsoft.Web.WebView2.Core;
using Microsoft.Extensions.Configuration;
using System.Windows.Media;
using System.Windows.Documents;

namespace Front
{

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly object _locker = new object();
        Access Access = Access.GetAccess();
        public BL Bl;
        EquipmentFront EF;
        public ControlScale CS { get; set; }


        Admin ad;

        public Receipt curReceipt;//{ get; set; } = null;
        public ReceiptWares CurWares { get; set; } = null;
        Client Client;
        public GW CurW { get; set; } = null;

        public eStateMainWindows State = eStateMainWindows.StartWindow;
        // eStateScale StateScale = eStateScale.NotDefine;
        public eTypeAccess TypeAccessWait { get; set; }
        public ObservableCollection<ReceiptWares> ListWares { get; set; }
        public CustomWindow customWindow { get; set; }
        //public ObservableCollection<CustomButton> customWindowButtons { get; set; }
        public string WaresQuantity { get; set; }
        decimal _MoneySum;
        double tempMoneySum;
        public decimal MoneySum { get; set; }
        public string MoneySumToRound { get; set; }
        public string EquipmentInfo { get; set; }
        public bool Volume { get; set; }
        public string ChangeSumPaymant { get; set; } = "0";

        public bool IsShowWeightWindows { get; set; } = false;
        public bool IsIgnoreExciseStamp { get; set; }
        public bool IsAddNewWeight { get; set; }
        public bool IsFixWeight { get; set; }
        public bool IsExciseStamp { get; set; }
        public bool IsCheckReturn { get { return curReceipt?.TypeReceipt == eTypeReceipt.Refund ? true : false; } }
        /// <summary>
        /// Чи заброкована зміна
        /// </summary>
        bool _IsLockSale = false;
        public bool IsLockSale { get { return _IsLockSale; } set { if (_IsLockSale != value) { SetStateView(!value && State == eStateMainWindows.WaitAdmin ? eStateMainWindows.WaitInput : eStateMainWindows.NotDefine); _IsLockSale = value; } } }

        /// <summary>
        /// чи є товар з обмеженням по віку
        /// </summary>
        public bool IsAgeRestrict { get { return curReceipt == null ? false : curReceipt?.AgeRestrict == 0 || curReceipt?.AgeRestrict != 0 && curReceipt.IsConfirmAgeRestrict; } }

        /// <summary>
        /// Чи можна добавляти товар 
        /// </summary>
        public bool IsAddNewWares { get { return curReceipt == null ? true : !curReceipt.IsLockChange && State == eStateMainWindows.WaitInput; } }
        /// <summary>
        /// Чи активна кнопка оплати
        /// </summary>
        //bool _IsEnabledPaymentButton;
        public bool IsEnabledPaymentButton { get { return _MoneySum >= 0 && WaresQuantity != "0" && IsAddNewWares; } }// set { _IsEnabledPaymentButton = value; } }
        /// <summary>
        /// чи активна кнопка пошуку
        /// </summary>
        public bool IsEnabledFindButton { get { return IsAddNewWares; } }
        /// <summary>
        /// Чи можна підтвердити власну сумку
        /// </summary>
        public bool IsOwnBag { get { return ControlScaleCurrentWeight > 0 && ControlScaleCurrentWeight <= Global.MaxWeightBag; } }
        public bool IsPresentFirstTerminal { get { return EF.BankTerminal1 != null; } }
        public bool IsPresentSecondTerminal { get { return EF.BankTerminal2 != null; } }

        bool IsViewProblemeWeight { get { return State == eStateMainWindows.WaitInput || State == eStateMainWindows.StartWindow; } }
        /// <summary>
        /// Чи треба вибирати ціну.
        /// </summary>
        bool IsChoicePrice { get { return CurWares != null && CurWares.IsMultiplePrices && curReceipt != null && curReceipt.GetLastWares?.CodeWares != CurWares.CodeWares; } }
        /// <summary>
        /// теперішня вага
        /// </summary>
        public double ControlScaleCurrentWeight { get; set; } = 0;
        public string ClientName { get { return curReceipt != null && curReceipt.CodeClient == Client?.CodeClient ? Client?.NameClient : "Відсутній клієнт"; } }
        //public string CurWaresName { get { return CurWares != null ? CurWares.NameWares : " "; } }
        public int QuantityCigarettes { get; set; } = 1;
        public string NameFirstTerminal { get { return IsPresentFirstTerminal ? EF?.BankTerminal1.Name : null; } }
        public string NameSecondTerminal { get { return IsPresentSecondTerminal ? EF?.BankTerminal2.Name : null; } }

        public string GetBackgroundColor { get { return curReceipt?.TypeReceipt == eTypeReceipt.Refund ? "#FFE5E5" : "#FFFFFF"; } }

        public System.Drawing.Color GetFlagColor(eStateMainWindows pStateMainWindows)
        {
            return FC.ContainsKey(pStateMainWindows) ? FC[pStateMainWindows] : System.Drawing.Color.Black;
        }

        public string WaitAdminText
        {
            get
            {
                TextBlock tb = new TextBlock();
                tb.TextWrapping = TextWrapping.Wrap;
                tb.TextAlignment = TextAlignment.Center;
                tb.FontSize = 24;
                tb.Margin = new Thickness(10);
                switch (TypeAccessWait)
                {
                    case eTypeAccess.DelWares:
                        tb.Inlines.Add("Видалення товару: "); // додає просто текс - налаштування тексту бере ті, що задамо вище
                        tb.Inlines.Add(new Run(CurWares?.NameWares) { FontWeight = FontWeights.Bold, Foreground = Brushes.Red }); // додає текст але можна змінити будь-які його параметри
                        break;
                    case eTypeAccess.DelReciept:
                        tb.Inlines.Add("Видалити чек ");
                        break;
                    case eTypeAccess.StartFullUpdate:
                        tb.Inlines.Add(new Run("Повне оновлення БД") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red });
                        break;
                    case eTypeAccess.ErrorFullUpdate:
                        tb.Inlines.Add(new Run("Помилка повного оновлення БД") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red, FontSize=20 });
                        break;
                    case eTypeAccess.ErrorEquipment:
                        tb.Inlines.Add(new Run("Проблема з критично важливим обладнанням") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red });
                        break;
                    case eTypeAccess.LockSale:
                        tb.Inlines.Add("Зміна заблокована");
                        break;
                    case eTypeAccess.FixWeight:
                        tb.Inlines.Add(new Run(CS.Info) { FontWeight = FontWeights.Bold, Foreground = Brushes.Red, FontSize = 32});
                        tb.Inlines.Add(new Run(CS.InfoEx) { Foreground = Brushes.Black,FontSize=20 });
                        break;
                    case eTypeAccess.ConfirmAge:
                        tb.Inlines.Add("Підтвердження віку");
                        break;
                    case eTypeAccess.ExciseStamp:
                        tb.Inlines.Add("Ввід акцизної марки");
                        break;
                }
                StackPanelWaitAdmin.Children.Clear();
                StackPanelWaitAdmin.Children.Add(tb);

                //WaitAdminTextProblem.Text = new Run(("Повне оновлення БД"),  (FontWeight = FontWeights.Bold, Foreground = Brushes.Red) );

                //switch (TypeAccessWait)
                //{
                //    case eTypeAccess.DelWares: return ($"Видалення товару: {CurWares?.NameWares}");
                //    case eTypeAccess.DelReciept: return "Видалити чек";
                //    case eTypeAccess.StartFullUpdate: return "Повне оновлення БД";
                //    case eTypeAccess.ErrorFullUpdate: return "Помилка повного оновлення БД";
                //    case eTypeAccess.ErrorEquipment: return "Проблема з критично важливим обладнанням";
                //    case eTypeAccess.LockSale: return "Зміна заблокована";
                //    case eTypeAccess.FixWeight: return CS.Info;
                //    case eTypeAccess.ConfirmAge: return "Підтвердження віку";
                //    case eTypeAccess.ExciseStamp: return "Ввід акцизної марки";
                //}
                return null;
            }
        }
        /// <summary>
        /// Вага з основної ваги
        /// </summary>
        public double Weight { get; set; } = 0d;
        //public string WeightControl { get; set; }

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
            CS = new ControlScale();
            var c = new Config("appsettings.json");// Конфігурація Програми(Шляхів до БД тощо)

            var fc = new List<FlagColor>();
            Config.GetConfiguration().GetSection("MID:FlagColor").Bind(fc);
            foreach (var el in fc)
                if (!FC.ContainsKey(el.State))
                    FC.Add(el.State, el.Color);

            //Для касового місця Запит логін пароль.
            if (Global.TypeWorkplace == eTypeWorkplace.SelfServicCheckout)
                Access.СurUser = new User() { TypeUser = eTypeUser.Client, CodeUser = 99999999, Login = "Client", NameUser = "Client" };

            Bl = new BL(true);
            EF = new EquipmentFront(GetBarCode);
            InitAction();

            WaresQuantity = "0";
            MoneySum = 0;
            Volume = true;

            ad = new Admin(this, EF);
            ad.WindowState = WindowState.Minimized;
            ad.Show();
            InitializeComponent();

            //string HTMLContent = "<font style=\"vertical - align: inherit; \">Крок 1 - Встановіть Visual Studio</font>";
            //try
            //{
            //    Init(HTMLContent);
            //}
            //catch (Exception e)
            //{
            //    FileLogger.WriteLogMessage(e.Message, eTypeLog.Error);
            //}

            // ListWares = new ObservableCollection<ReceiptWares>(StartData());
            //WaresList.ItemsSource = ListWares;// Wares;

            ua.Tag = new CultureInfo("uk");
            en.Tag = new CultureInfo("en");
            hu.Tag = new CultureInfo("hu");
            pl.Tag = new CultureInfo("pl");

            //CultureInfo currLang = App.Language;
            Recalc();
            Task.Run(() => Bl.ds.SyncDataAsync());

            var LastR = Bl.GetLastReceipt();
            if (LastR != null && LastR.SumReceipt > 0 && LastR.StateReceipt != eStateReceipt.Canceled && LastR.StateReceipt != eStateReceipt.Print && LastR.StateReceipt != eStateReceipt.Send)
            {
                //curReceipt = LastR;               
                SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, eWindows.RestoreLastRecipt, LastR.SumReceipt.ToString());
                return;
            }
            SetStateView(eStateMainWindows.StartWindow);
        }

        private async void Init(string HTMLContent)
        {
            try
            {
                //await WebView2.EnsureCoreWebView2Async();
                //WebView2.CoreWebView2.NavigateToString(HTMLContent);
                //MyWebView.CoreWebView2.Navigate("ms-appx-web:///www/index.html");
                //WebView2.CoreWebView2.OpenDevToolsWindow();
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(e.Message, eTypeLog.Error);
            }
        }

        void SetCurReceipt(Receipt pReceipt)
        {
            curReceipt = pReceipt;
            SetPropertyChanged();
        }
        void SetPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GetBackgroundColor"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabledPaymentButton"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCheckReturn"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClientName"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsAgeRestrict"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabledFindButton"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsLockSale"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsAddNewWares"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WaitAdminText"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CS"));
            //ChangeWaitAdminText();
        }

        public void SetStateView(eStateMainWindows pSMV = eStateMainWindows.NotDefine, eTypeAccess pTypeAccess = eTypeAccess.NoDefine, ReceiptWares pRW = null, eWindows pCW = eWindows.NoDefinition, string pStr = null)
        {
            if (State == eStateMainWindows.WaitOwnBag && pTypeAccess == eTypeAccess.FixWeight)
                return;
            lock (this._locker)
            {
                var r = Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    if ((EF.StatCriticalEquipment != eStateEquipment.On || !Bl.ds.IsReady || IsLockSale || CS.IsProblem || curReceipt?.IsNeedExciseStamp == true || IsChoicePrice) &&
                       pSMV != eStateMainWindows.WaitAdminLogin)
                    {
                        eTypeAccess Res = eTypeAccess.NoDefine;
                        if (EF.StatCriticalEquipment != eStateEquipment.On) Res = eTypeAccess.ErrorEquipment;
                        else
                           if (!Bl.ds.IsReady) Res = (Bl.ds.Status == eSyncStatus.Error ? eTypeAccess.ErrorFullUpdate : eTypeAccess.StartFullUpdate);
                        else
                            if (IsLockSale) Res = eTypeAccess.LockSale;
                        else
                        if (!(pTypeAccess == eTypeAccess.DelReciept || pTypeAccess == eTypeAccess.DelWares || TypeAccessWait == eTypeAccess.DelReciept || TypeAccessWait == eTypeAccess.DelWares))
                        {
                            if (curReceipt?.IsNeedExciseStamp == true) Res = eTypeAccess.ExciseStamp;
                            else
                            if (IsChoicePrice) { pSMV = eStateMainWindows.WaitInputPrice; pRW = CurWares; }
                            else
                            if (CS.IsProblem)
                            {
                                if (!IsShowWeightWindows && pSMV != eStateMainWindows.WaitAdmin) //--&&  && pSMV != eStateMainWindows.BlockWeight && pSMV != eStateMainWindows.WaitOwnBag && pSMV != eStateMainWindows.WaitAdmin)
                                    pSMV = eStateMainWindows.BlockWeight;
                                else
                                    Res = eTypeAccess.FixWeight;
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
                        TypeAccessWait = pTypeAccess;

                    if (pSMV != eStateMainWindows.NotDefine)
                    {
                        State = pSMV;
                        SetPropertyChanged();
                        EF.SetColor(GetFlagColor(State));
                    }


                    //Генеруємо з кастомні вікна
                    if (TypeAccessWait == eTypeAccess.FixWeight)
                        customWindow = new CustomWindow(CS.StateScale);
                    else
                        if (State != eStateMainWindows.NotDefine)
                        customWindow = new CustomWindow(pCW, pStr);

                    if ((State == eStateMainWindows.WaitAdmin || State == eStateMainWindows.WaitAdminLogin) && TypeAccessWait == eTypeAccess.ExciseStamp)
                        customWindow = new CustomWindow(pCW, pStr);

                    WaitAdminWeightButtons.ItemsSource = customWindow.Buttons;


                    if (State != eStateMainWindows.WaitAdmin && State != eStateMainWindows.WaitAdminLogin)
                        TypeAccessWait = eTypeAccess.NoDefine;

                    ErrorWindows.Visibility = Visibility.Collapsed;
                    ExciseStamp.Visibility = Visibility.Collapsed;
                    ChoicePrice.Visibility = Visibility.Collapsed;
                    Background.Visibility = Visibility.Collapsed;
                    BackgroundWares.Visibility = Visibility.Collapsed;
                    WaitAdmin.Visibility = Visibility.Collapsed;
                    WaitAdminLogin.Visibility = Visibility.Collapsed;
                    WeightWares.Visibility = Visibility.Collapsed;
                    WaitPayment.Visibility = Visibility.Collapsed;
                    StartShopping.Visibility = Visibility.Collapsed;
                    //textInAll.Visibility = Visibility.Visible;
                    //valueInAll.Visibility = Visibility.Visible;
                    ConfirmAge.Visibility = Visibility.Collapsed;
                    WaitKashier.Visibility = Visibility.Collapsed;
                    CustomWindows.Visibility = Visibility.Collapsed;
                    ErrorBackground.Visibility = Visibility.Collapsed;
                    OwnBagWindows.Visibility = Visibility.Collapsed;

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

                    StartVideo.Stop();

                    switch (State)
                    {
                        case eStateMainWindows.StartWindow:
                            StartShopping.Visibility = Visibility.Visible;
                            //textInAll.Visibility = Visibility.Collapsed;
                            //valueInAll.Visibility = Visibility.Collapsed;
                            StartVideo.Play();
                            break;
                        case eStateMainWindows.WaitInputPrice:
                            TypeAccessWait = eTypeAccess.ChoicePrice;
                            if (CurWares != null && CurWares.Prices != null && CurWares.Prices.Count() > 0)
                            {
                                var rrr = new ObservableCollection<Models.Price>(CurWares.Prices.OrderByDescending(r => r.Price).Select(r => new Models.Price(r.Price, Access.GetRight(TypeAccessWait), r.TypeWares)));
                                //rrr.First().IsEnable = true;
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

                            break;
                        case eStateMainWindows.WaitAdmin:
                            WaitAdmin.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            if (customWindow.Buttons != null && customWindow.Buttons.Count > 0)
                            {
                                WaitAdminCancel.Visibility = Visibility.Collapsed;
                            }
                            // CustomButtonsWaitAdmin.ItemsSource = null;
                            switch (TypeAccessWait)
                            {
                                case eTypeAccess.FixWeight:
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
                                    KBAdmin.Visibility = Visibility.Visible;
                                    ExciseStampButtons.Visibility = Visibility.Visible;
                                    ExciseStampNameWares.Visibility = Visibility.Visible;
                                    WaitAdminTitle.Visibility = Visibility.Collapsed;
                                    // CustomButtonsWaitAdmin.ItemsSource = customWindow.Buttons;

                                    break;
                                default:

                                    break;
                            }
                            //CustomButtonsWaitAdmin.ItemsSource = customWindowButtons;
                            break;
                        case eStateMainWindows.WaitAdminLogin:
                            LoginTextBlock.Text = "";
                            PasswordTextBlock.Text = "";
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
                            PaymentImage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/paymentTerminal.png"));
                            WaitPayment.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            //PaymentImage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/paymentTerminal.png"));
                            break;
                        case eStateMainWindows.ProcessPrintReceipt:
                            PaymentImage.Source = BitmapFrame.Create(new Uri(@"pack://application:,,,/icons/receipt.png"));
                            WaitPayment.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            //Client.NameClient = null;
                            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClientName"));
                            break;

                        case eStateMainWindows.ChoicePaymentMethod:
                            MoneySumToRound = Convert.ToString(MoneySum);
                            ChangeSumPaymant = "0";
                            CashDisbursementTextBox.Text = "0";
                            RoundSum.Text = "0";
                            RoundSumDown.Text = "0";
                            WaitKashier.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            break;
                        case eStateMainWindows.WaitCustomWindows:
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
                            break;
                        case eStateMainWindows.BlockWeight:
                            TypeAccessWait = eTypeAccess.FixWeight;
                            //IsEnabledPaymentButton = false;
                            //IsEnabledFindButton = false;
                            ///PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabledPaymentButton"));
                            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabledFindButton"));
                            break;
                        case eStateMainWindows.WaitInput:
                            IsIgnoreExciseStamp = false;
                            IsAddNewWeight = false;
                            IsFixWeight = false;
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
                }));
                r.Wait(new TimeSpan(0, 0, 0, 100));
                SetPropertyChanged();
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
            Background.Visibility = Visibility.Visible;
            BackgroundWares.Visibility = Visibility.Visible;
            KeyPad keyPad = new KeyPad(this);
            Button btn = sender as Button;
            if (btn.DataContext is ReceiptWares)
            {
                decimal tempQuantity = 0;
                ReceiptWares temp = btn.DataContext as ReceiptWares;
                keyPad.productNameChanges.Text = Convert.ToString(temp.NameWares);
                keyPad.Result = Convert.ToString(temp.Quantity);
                if (keyPad.ShowDialog() == true)
                    tempQuantity = Convert.ToDecimal(keyPad.Result);
                if (tempQuantity != 0)
                {

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
            }
        }

        private void _VolumeButton(object sender, RoutedEventArgs e)
        {
            Volume = !Volume;
        }

        private void Recalc()
        {
            _MoneySum = ListWares?.Sum(r => r.SumTotal) ?? 0;
            MoneySum = _MoneySum;
            WaresQuantity = ListWares?.Count().ToString() ?? "0";
            if (VisualTreeHelper.GetChildrenCount(WaresList) > 0)
            {
                Border border = (Border)VisualTreeHelper.GetChild(WaresList, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
            //SV_WaresList.ScrollToEnd();
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
            ua.Style = (Style)ua.FindResource("Default");
            en.Style = (Style)en.FindResource("Default");
            hu.Style = (Style)hu.FindResource("Default");
            pl.Style = (Style)pl.FindResource("Default");
            switch (btn.Name)
            {
                case "ua":
                    ua.Style = (Style)ua.FindResource("yelowButton");
                    break;
                case "en":
                    en.Style = (Style)en.FindResource("yelowButton");
                    break;
                case "hu":
                    hu.Style = (Style)hu.FindResource("yelowButton");
                    break;
                case "pl":
                    pl.Style = (Style)pl.FindResource("yelowButton");
                    break;
            }
        }

        private void _Back(object sender, RoutedEventArgs e)
        {
            // Правильний блок.
            if (Access.GetRight(eTypeAccess.DelReciept) || curReceipt?.SumReceipt == 0)
            {
                Bl.SetStateReceipt(curReceipt, eStateReceipt.Canceled);
                curReceipt = Bl.GetNewIdReceipt();
                SetStateView(eStateMainWindows.StartWindow);
            }
            else
                SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.DelReciept, null);
            /*//!!!TMP
            var rand = new Random();
            string sql = @"select CODE_WARES from (select CODE_WARES,row_number() over (order by code_wares)  as nn from price p where p.code_DEALER=2)
                        where nn=  cast(abs(random()/9223372036854775808.0)*1000 as int)";
            var CodeWares = Bl.db.db.ExecuteScalar<int>(sql);
            if (CodeWares > 0)
                Bl.AddWaresCode(CodeWares, 0, Math.Round(1M + 5M * rand.Next() / (decimal)int.MaxValue));
            */
        }

        private void _Search(object sender, RoutedEventArgs e)
        {
            SetStateView(eStateMainWindows.WaitFindWares);
        }

        void IsPrises(decimal pQuantity = 0m, decimal pPrice = 0m)
        {
            if (CurWares.TypeWares == eTypeWares.Alcohol)
            {
                SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ExciseStamp, CurWares);
                return;
            }

            if (CurWares.Price == 0) //Повідомлення Про відсутність ціни
            {
                SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, eWindows.NoPrice, CurWares.NameWares);
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
        }

        private void _ButtonHelp(object sender, RoutedEventArgs e)
        {
            SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.AdminPanel);
        }

        private void _OwnBag(object sender, RoutedEventArgs e)
        {
            if (ControlScaleCurrentWeight > 0 && ControlScaleCurrentWeight < Global.MaxWeightBag)
            {
                curReceipt = Bl.GetNewIdReceipt();
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
            if (Global.TypeWorkplace == eTypeWorkplace.СashRegister)
                SetStateView(eStateMainWindows.ChoicePaymentMethod);
            else
            {
                var task = Task.Run(() => PrintAndCloseReceipt());
            }
            //var result = task.Result;            
        }

        private void ShowErrorMessage(string ErrorMessage)
        {
            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                ErrorBackground.Visibility = Visibility.Visible;
                ErrorWindows.Visibility = Visibility.Visible;
                ErrorText.Text = ErrorMessage;
            }));
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
            AddWares(CurW.Code, CurW.CodeUnit, Convert.ToDecimal(Weight));
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
            var U = Bl.GetUserByLogin(LoginTextBlock.Text, PasswordTextBlock.Text);
            if (U == null)
            {
                ShowErrorMessage("Не вірний логін чи пароль");
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
        }

        private void ChangeSumPaymentButton(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Content)
            {
                case "C":
                    if (ChangeSumPaymant.Length <= 1)
                    {
                        ChangeSumPaymant = "0";
                        break;
                    }
                    else
                        ChangeSumPaymant = ChangeSumPaymant.Remove(ChangeSumPaymant.Length - 1);
                    break;

                case ",":
                    if (ChangeSumPaymant.IndexOf(",") != -1)
                    {
                        break;
                    }
                    ChangeSumPaymant += btn.Content;
                    break;
                default:
                    ChangeSumPaymant += btn.Content;
                    break;

            }
            MoneySumPayTextBox.Text = ChangeSumPaymant;
        }

        private void MoneySumPayChange(object sender, TextChangedEventArgs e)
        {
            try
            {
                ResMoney.Text = Math.Round((Convert.ToDouble(MoneySumPayTextBox.Text) - Convert.ToDouble(MoneySumToRound)), 2).ToString();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }

        }

        private void CashDisbursement(object sender, RoutedEventArgs e)
        {
            Background.Visibility = Visibility.Visible;
            BackgroundWares.Visibility = Visibility.Visible;
            KeyPad keyPad = new KeyPad(this);

            keyPad.productNameChanges.Text = "Введіть суму видачі";
            keyPad.Result = "100";
            if (keyPad.ShowDialog() == true)
                CashDisbursementTextBox.Text = keyPad.Result;
        }

        private void CancelCashDisbursement(object sender, RoutedEventArgs e)
        {
            CashDisbursementTextBox.Text = "0";
        }


        public double RoundingPrice(double price, double precision)
        {
            price = Convert.ToInt32(Math.Round(price * 100, 3));
            precision = Math.Round(precision, 2);
            return Math.Round(Math.Ceiling(Math.Ceiling(price / precision / 100)) * precision, 2);
        }
        public double RoundingDownPrice(double price, double precision)
        {
            price = Convert.ToInt32(Math.Round(price * 100, 3));
            precision = Math.Round(precision, 2);
            return Math.Round(Math.Floor(Math.Floor(price / precision / 100)) * precision, 2);
        }

        private void Round(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            tempMoneySum = Convert.ToDouble(MoneySum);
            RoundSum.Text = "0";
            RoundSumDown.Text = "0";

            switch (btn.Name)
            {
                case "plus01":
                    MoneySumToRound = RoundingPrice(tempMoneySum, 0.1).ToString();
                    RoundSum.Text = (Math.Round(Convert.ToDouble(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;
                case "plus05":
                    MoneySumToRound = RoundingPrice(tempMoneySum, 0.5).ToString();
                    RoundSum.Text = (Math.Round(Convert.ToDouble(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;
                case "plus1":
                    MoneySumToRound = RoundingPrice(tempMoneySum, 1.0).ToString();
                    RoundSum.Text = (Math.Round(Convert.ToDouble(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;
                case "plus2":
                    MoneySumToRound = RoundingPrice(tempMoneySum, 2.0).ToString();
                    RoundSum.Text = (Math.Round(Convert.ToDouble(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;
                case "plus5":
                    MoneySumToRound = RoundingPrice(tempMoneySum, 5.0).ToString();
                    RoundSum.Text = (Math.Round(Convert.ToDouble(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;
                case "minus1":
                    MoneySumToRound = RoundingDownPrice(tempMoneySum, 1.0).ToString();
                    RoundSumDown.Text = (Math.Round(Convert.ToDouble(MoneySumToRound) - tempMoneySum, 2)).ToString();
                    break;

                default:
                    MoneySumToRound = Convert.ToString(MoneySum);
                    break;
            }
        }

        private void F5Button(object sender, RoutedEventArgs e)
        {
            TerminalPaymentInfo terminalPaymentInfo = new TerminalPaymentInfo(this);
            if (terminalPaymentInfo.ShowDialog() == true)
            {
                //отримуємо введені дані
                ShowErrorMessage(terminalPaymentInfo.enteredDataFromTerminal.AuthorizationCode);
                //                MessageBox.Show(terminalPaymentInfo.enteredDataFromTerminal.AuthorizationCode);//як приклад
            }
        }

        private void CustomWindowClickButton(object sender, RoutedEventArgs e)
        {
            //SetStateView(eStateMainWindows.WaitInput);

            Button btn = sender as Button;
            CustomButton res = btn.DataContext as CustomButton;

            if (res == null)
            {
                if (btn.Name.Equals("OKCustomWindows"))
                    res = new CustomButton() { Id = 1 };
            }
            if (res != null)
            {
                if (customWindow.Id == eWindows.RestoreLastRecipt)
                {
                    if (res.Id == 1)
                    {
                        Bl.db.RecalcPriceAsync(new IdReceiptWares(Bl.GetLastReceipt()));
                    }
                    if (res.Id == 2)
                        curReceipt = Bl.GetNewIdReceipt();
                    SetStateView(eStateMainWindows.WaitInput);
                    return;
                }
                if (customWindow.Id == eWindows.ConfirmWeight)
                {
                    //Ховаєм адмінпанель щоб управляти товаром.
                    if (res.Id == -1)
                    {
                        IsShowWeightWindows = false;
                        SetStateView(eStateMainWindows.BlockWeight);
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
                var r = new CustomWindowAnswer()
                {
                    idReceipt = curReceipt,
                    Id = customWindow.Id,
                    IdButton = res.Id,
                    Text = TextBoxCustomWindows.Text,
                    ExtData = customWindow.Id == eWindows.ConfirmWeight ? CS?.RW : null
                };
                Bl.SetCustomWindows(r);
                SetStateView(eStateMainWindows.WaitInput);
            }
        }

        private void FindClientByPhoneClick(object sender, RoutedEventArgs e)
        {
            SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, eWindows.PhoneClient);
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

        private void ButtonOkErrorWin(object sender, RoutedEventArgs e)
        {
            ErrorBackground.Visibility = Visibility.Collapsed;
            ErrorWindows.Visibility = Visibility.Collapsed;
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
        //private void ChangeWaitAdminText()
        //{


        //    //StackPanelWaitAdmin.Children[4] = tb;

        //}
    }
}
