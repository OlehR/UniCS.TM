using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using Microsoft.Extensions.Configuration;
using System.Windows.Media;
using System.Windows.Documents;
using System.Reflection;

namespace Front
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly object _locker = new object();
        Access Access = Access.GetAccess();
        public BL Bl;
        EquipmentFront EF;
        public ControlScale CS { get; set; }

        Sound s = Sound.GetSound();
        public User AdminSSC { get; set; } = null;
        public DateTime DTAdminSSC { get; set; }

        Admin ad;
        //public int TextBlockFontSize { get; set; } = 40;

        public Receipt curReceipt;//{ get; set; } = null;
        public ReceiptWares CurWares { get; set; } = null;
        public Client Client { get; set; }
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
        bool _Volume = true;
        public bool Volume { get { return _Volume; } set { _Volume = value; if (s != null) s.IsSound = value; } }
        public string ChangeSumPaymant { get; set; } = "0";

        public bool IsShowWeightWindows { get; set; } = false;
        //public bool IsIgnoreExciseStamp { get; set; }
        public bool IsConfirmAdmin { get; set; }
        // public bool IsFixWeight { get; set; }
        public bool IsExciseStamp { get; set; }
        public bool IsCheckReturn { get { return curReceipt?.TypeReceipt == eTypeReceipt.Refund ? true : false; } }
        public bool IsCheckPaid { get { return curReceipt?.StateReceipt == eStateReceipt.Pay ? true : false; } }
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
        public bool IsAddNewWares { get { return curReceipt == null ? true : !curReceipt.IsLockChange && State == eStateMainWindows.WaitInput; } }
        /// <summary>
        /// Чи активна кнопка оплати
        /// </summary>
        //bool _IsEnabledPaymentButton;
        public bool IsEnabledPaymentButton { get { return (_MoneySum >= 0 && WaresQuantity != "0" && IsAddNewWares) || curReceipt?.TypeReceipt == eTypeReceipt.Refund; } }// set { _IsEnabledPaymentButton = value; } }
        /// <summary>
        /// чи активна кнопка пошуку
        /// </summary>
        public bool IsEnabledFindButton { get { return IsAddNewWares; } }
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

        /// <summary>
        /// треба переробити(інтегрувати в основну форму)
        /// </summary>
        //[Obsolete]
        // KeyPad keyPad;

        public System.Drawing.Color GetFlagColor(eStateMainWindows pStateMainWindows, eTypeAccess pTypeAccess, eStateScale pSS)
        {
            if (pTypeAccess == eTypeAccess.ExciseStamp)
                return System.Drawing.Color.Green;

            System.Drawing.Color c = FC.ContainsKey(pStateMainWindows) ? FC[pStateMainWindows] : System.Drawing.Color.Black;
            if (pSS == eStateScale.WaitGoods)
                return System.Drawing.Color.Yellow;
            if (pSS == eStateScale.BadWeight || pSS == eStateScale.NotStabilized)
                return System.Drawing.Color.Red;
            return c;
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
                    case eTypeAccess.StartFullUpdate:
                        tb.Inlines.Add(new Run("Повне оновлення БД") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red });
                        break;
                    case eTypeAccess.ErrorFullUpdate:
                        tb.Inlines.Add(new Run("Помилка повного оновлення БД") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red, FontSize = 20 });
                        break;
                    case eTypeAccess.ErrorEquipment:
                        tb.Inlines.Add(new Run("Проблема з критично важливим обладнанням") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red });
                        break;
                    case eTypeAccess.LockSale:
                        tb.Inlines.Add("Зміна заблокована");
                        break;
                    case eTypeAccess.FixWeight:
                        tb.Inlines.Add(new Run(CS.Info) { FontWeight = FontWeights.Bold, Foreground = Brushes.Red, FontSize = 32 });
                        tb.Inlines.Add(new Run(CS.InfoEx) { Foreground = Brushes.Black, FontSize = 20 });
                        break;
                    case eTypeAccess.ConfirmAge:
                        tb.Inlines.Add("Вам виповнилось 18 років?");
                        break;
                    case eTypeAccess.ExciseStamp:
                        WaitAdminTitle.Visibility = Visibility.Collapsed;
                        tb.Inlines.Add(new Run("Відскануйте акцизну марку!") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red, FontSize = 32 });
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

            //Провіряємо чи зміна відкрита.
            string BarCodeAdminSSC = Bl.db.GetConfig<string>("CodeAdminSSC");
            if (!string.IsNullOrEmpty(BarCodeAdminSSC))
            {
                DateTime TimeAdminSSC = Bl.db.GetConfig<DateTime>("DateAdminSSC");
                if (TimeAdminSSC.Date == DateTime.Now.Date)
                {
                    AdminSSC = Bl.GetUserByBarCode(BarCodeAdminSSC);
                    DTAdminSSC = TimeAdminSSC;
                    Bl.StartWork(Global.IdWorkPlace, BarCodeAdminSSC);//!!!TMP треба штрихкод
                }
                else BarCodeAdminSSC = null;

            }

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
                SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, new CustomWindow(eWindows.RestoreLastRecipt, LastR.SumReceipt.ToString()));
                return;
            }
            else
                SetStateView(eStateMainWindows.StartWindow);
        }

        void SetCurReceipt(Receipt pReceipt)
        {
            curReceipt = pReceipt;
            if (curReceipt == null)
            {
                Dispatcher.BeginInvoke(new ThreadStart(() => ListWares?.Clear()));
                // = new ObservableCollection<ReceiptWares>();
                Client = null;
                CS.WaitClear();
            }
            SetPropertyChanged();
        }

        void SetPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GetBackgroundColor"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabledPaymentButton"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCheckReturn"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCheckPaid"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClientName"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsAgeRestrict"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabledFindButton"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsLockSale"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsAddNewWares"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WaitAdminText"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CS"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Client"));
            //ChangeWaitAdminText();
        }

        public void SetStateView(eStateMainWindows pSMV = eStateMainWindows.NotDefine, eTypeAccess pTypeAccess = eTypeAccess.NoDefine, ReceiptWares pRW = null, CustomWindow pCW = null, eSender pS = eSender.NotDefine)
        {

            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"pSMV={pSMV}/{State}, pTypeAccess={pTypeAccess}/{TypeAccessWait}, pRW ={pRW} , pCW={pCW},  pS={pS}", eTypeLog.Full);


            if (State == eStateMainWindows.WaitOwnBag && pTypeAccess == eTypeAccess.FixWeight)
                return;
            //Подія по вазі 
            if (pS == eSender.ControlScale)
            {
                // під час оплати - ігноруємо її
                if ((State == eStateMainWindows.ProcessPay || State == eStateMainWindows.ProcessPrintReceipt))
                {
                    if (pTypeAccess == eTypeAccess.FixWeight)
                        EF.StartMultipleTone();
                    else
                        EF.StopMultipleTone();
                    return;
                }
                //Під час відновлення чека
                if (customWindow?.Id == eWindows.RestoreLastRecipt)
                    return;
                //Якщо очікуємо ввід ціни.
                if (State == eStateMainWindows.WaitInputPrice) return;
            }

            if ((pSMV != eStateMainWindows.ProcessPay && pSMV != eStateMainWindows.ProcessPrintReceipt) && (State == eStateMainWindows.ProcessPay || State == eStateMainWindows.ProcessPrintReceipt))
                EF.StopMultipleTone();

            if (pSMV == eStateMainWindows.StartWindow && curReceipt != null)
                pSMV = eStateMainWindows.WaitInput;
            if (pSMV == eStateMainWindows.WaitInput && curReceipt == null)
                pSMV = eStateMainWindows.StartWindow;
            //lock (this._locker)
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

                    //Якщо 
                    if (pSMV == eStateMainWindows.NotDefine) return;
                    else
                    {
                        State = pSMV;
                        SetPropertyChanged();
                        EF.SetColor(GetFlagColor(State, TypeAccessWait, CS.StateScale));
                    }


                    //Генеруємо з кастомні вікна
                    if (TypeAccessWait == eTypeAccess.FixWeight)
                        customWindow = new CustomWindow(CS.StateScale, CS.RW?.Quantity == 1 && CS.RW?.FixWeightQuantity == 0, CS.StateScale == eStateScale.WaitClear && (curReceipt?.OwnBag ?? 0) > 0);
                    else
                    if (TypeAccessWait == eTypeAccess.ConfirmAge)
                        customWindow = new CustomWindow(eWindows.ConfirmAge);
                    else
                        if (TypeAccessWait == eTypeAccess.ExciseStamp)
                        customWindow = new CustomWindow(eWindows.ExciseStamp);
                    else
                        customWindow = (State == eStateMainWindows.WaitCustomWindows ? pCW : null);


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


                    //StartVideo.Stop();

                    switch (State)
                    {
                        case eStateMainWindows.StartWindow:
                            StartShopping.Visibility = Visibility.Visible;
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

                            break;
                        case eStateMainWindows.WaitAdmin:
                            WaitAdmin.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
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
                        case eStateMainWindows.WaitInput:
                            //IsIgnoreExciseStamp = false;
                            //IsAddNewWeight = false;
                            //IsFixWeight = false;
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
            //Background.Visibility = Visibility.Visible;
            //BackgroundWares.Visibility = Visibility.Visible;
            //KeyPad keyPad = new KeyPad(this);
            //Button btn = sender as Button;
            //if (btn.DataContext is ReceiptWares)
            //{
            //    decimal tempQuantity = 0;
            //    ReceiptWares temp = btn.DataContext as ReceiptWares;
            //    keyPad.productNameChanges.Text = Convert.ToString(temp.NameWares);
            //    keyPad.Result = Convert.ToString(temp.Quantity);
            //    if (keyPad.ShowDialog() == true)
            //        tempQuantity = Convert.ToDecimal(keyPad.Result);
            //    if (tempQuantity != 0)
            //    {

            //        temp.Quantity = tempQuantity;
            //        if (curReceipt?.TypeReceipt == eTypeReceipt.Refund && tempQuantity > temp.MaxRefundQuantity)
            //        {
            //            temp.Quantity = (decimal)temp.MaxRefundQuantity;
            //        }
            //        if (curReceipt?.IsLockChange == false)
            //        {
            //            Bl.ChangeQuantity(temp, temp.Quantity);
            //        }
            //    }
            //    Background.Visibility = Visibility.Collapsed;
            //    BackgroundWares.Visibility = Visibility.Collapsed;
            //}
            Button btn = sender as Button;
            if (btn.DataContext is ReceiptWares)
            {
                ReceiptWares temp = btn.DataContext as ReceiptWares;
                InputNumberPhone.Desciption = Convert.ToString(temp.NameWares);
                InputNumberPhone.Result = "";//Convert.ToString(temp.Quantity);
                InputNumberPhone.ValidationMask = "";
                NumericPad.Visibility = Visibility.Visible;
                Background.Visibility = Visibility.Visible;
                BackgroundWares.Visibility = Visibility.Visible;

                InputNumberPhone.CallBackResult = (string result) =>
                {
                    decimal tempQuantity;
                    if (result != "" && result != "0")
                    {
                        tempQuantity = Convert.ToDecimal(result);

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

                SetCurReceipt(null);
                //NewReceipt();
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
        }

        private void _ButtonHelp(object sender, RoutedEventArgs e)
        {
            SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.AdminPanel);
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
            //NewReceipt();
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
            EquipmentStatusInPayment.Text = "";
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
            var U = Bl.GetUserByLogin(LoginTextBlock.Text, PasswordTextBlock.Text);
            if (U == null)
            {
                ShowErrorMessage("Не вірний логін чи пароль");
                return;
            }
            SetStateView(eStateMainWindows.WaitAdmin);
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
            Bl.AddEventAge(curReceipt);
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
                    }
                    if (res.Id == 2)
                        NewReceipt();
                    SetStateView(eStateMainWindows.WaitInput);
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
                            PrintAndCloseReceipt();
                        }
                        ));
                    }
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

        /*
        private void FindClientByPhoneClick(object sender, RoutedEventArgs e)
        {
            s.Play(eTypeSound.ScanCustomerCardOrEnterPhone);
            //SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, eWindows.PhoneClient);
            if (curReceipt == null)
                NewReceipt();
            Background.Visibility = Visibility.Visible;
            BackgroundWares.Visibility = Visibility.Visible;
            keyPad = new KeyPad(this);
            keyPad.ValidationMask = @"^[0-9]{9,12}$";
            keyPad.productNameChanges.Text = Convert.ToString("Введіть номер телефону");
            //keyPad.Result = Convert.ToString(0); 0503720278
            if (keyPad.ShowDialog() == true)
            {
                if (keyPad.Result.Length >= 10 )
                {

                    var r = new CustomWindowAnswer()
                    {
                        idReceipt = curReceipt,
                        Id = eWindows.PhoneClient,
                        IdButton = 1,
                        Text = keyPad.Result,
                        ExtData = CS?.RW
                    };
                    Bl.SetCustomWindows(r);                   
                }
            }
            keyPad = null;
            if (Client?.Wallet !=0 || Client?.SumMoneyBonus != 0 || Client?.SumBonus !=0)
            {
                ShowClientBonus.Visibility = Visibility.Visible;

            }
            Background.Visibility = Visibility.Collapsed;
            BackgroundWares.Visibility = Visibility.Collapsed;
        }
        */
        private void FindClientByPhoneClick(object sender, RoutedEventArgs e)
        {
            s.Play(eTypeSound.ScanCustomerCardOrEnterPhone);
            InputNumberPhone.Desciption = "Введіть номер телефону";
            InputNumberPhone.ValidationMask = "^[0-9]{10,13}$";
            InputNumberPhone.Result = "";
            InputNumberPhone.CallBackResult = FindClientByPhone;
            NumericPad.Visibility = Visibility.Visible;
            Background.Visibility = Visibility.Visible;
            BackgroundWares.Visibility = Visibility.Visible;
        }

        private void FindClientByPhone(string pResult)
        {

            //SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, eWindows.PhoneClient);
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
            Background.Visibility = Visibility.Collapsed;
            BackgroundWares.Visibility = Visibility.Collapsed;
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

        private void CancelPayment(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Прихована кнопка відміни оплати");
            EF.PosCancel();
        }
    }

}
