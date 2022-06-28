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

namespace Front
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Access Access = Access.GetAccess();
        public BL Bl;
        EquipmentFront EF;
        public ControlScale CS = new ControlScale();

        Admin ad;

        public Receipt curReceipt;//{ get; set; } = null;
        public ReceiptWares CurWares;//{ get; set; } = null;
        Client Client;
        public GW CurW { get; set; } = null;

        eStateMainWindows State = eStateMainWindows.StartWindow;
        eStateScale StateScale = eStateScale.NotDefine;
        public eTypeAccess TypeAccessWait { get; set; }
        public ObservableCollection<ReceiptWares> ListWares { get; set; }
        public CustomWindow customWindow { get; set; }
        public ObservableCollection<CustomButton> customWindowButtons { get; set; }        
        public string WaresQuantity { get; set; }
        decimal _MoneySum;
        double tempMoneySum;
        public string MoneySum { get; set; }
        public string MoneySumToRound { get; set; }
        public string EquipmentInfo { get; set; }
        public bool Volume { get; set; }
        public string ChangeSumPaymant { get; set; } = "0";
        public bool IsIgnoreExciseStamp { get; set; }
        public bool IsAddNewWeight { get; set; }
        public bool IsFixWeight { get; set; }
        public bool IsExciseStamp { get; set; }
        bool _IsLockSale = false;
        /// <summary>
        /// Чи заброкована зміна
        /// </summary>
        public bool IsLockSale { get { return _IsLockSale; } set { if (_IsLockSale != value) { SetStateView(!value && State == eStateMainWindows.WaitAdmin ? eStateMainWindows.WaitInput : eStateMainWindows.NotDefine); _IsLockSale = value; } } }
        public string GetBackgroundColor { get { return curReceipt?.TypeReceipt == eTypeReceipt.Refund ? "#FFE5E5" : "#FFFFFF"; } }
        public bool IsCheckReturn { get { return curReceipt?.TypeReceipt == eTypeReceipt.Refund ? true : false; } }
        /// <summary>
        /// чи є товар з обмеженням по віку
        /// </summary>
        public bool IsAgeRestrict { get  { return curReceipt.AgeRestrict == 0 || curReceipt.AgeRestrict != 0 && curReceipt.IsConfirmAgeRestrict;}}
        public bool PriceIsNotZero { get { return _MoneySum >= 0 && WaresQuantity != "0"; } }
        public string ClientName { get { return curReceipt != null && curReceipt.CodeClient == Client?.CodeClient ? Client?.NameClient : "Відсутній клієнт"; } }
        public bool IsPresentFirstTerminal { get { return EF.BankTerminal1 != null; } }                
        public bool IsPresentSecondTerminal { get { return EF.BankTerminal2 != null; } }        
        public string NameFirstTerminal
        {
            get
            {
                if (IsPresentFirstTerminal)
                {
                    return EF.BankTerminal1.Name;
                }
                else return null;
            }
        }
        public string NameSecondTerminal
        {
            get
            {
                if (IsPresentSecondTerminal)
                {
                    return EF.BankTerminal2.Name;
                }
                else return null;
            }
        }
        public string WaitAdminText
        {
            get
            {
                switch (TypeAccessWait)
                {
                    case eTypeAccess.DelWares: return ($"Видалення товару: {CurWares?.NameWares}");
                    case eTypeAccess.DelReciept: return "Видалити чек";
                    case eTypeAccess.StartFullUpdate: return "Повне оновлення БД";
                    case eTypeAccess.ErrorFullUpdate: return "Помилка повного оновлення БД";
                    case eTypeAccess.ErrorEquipment: return "Проблема з критично важливим обладнанням";
                    case eTypeAccess.LockSale: return "Зміна заблокована";
                    case eTypeAccess.FixWeight: return CS.Info;
                    case eTypeAccess.ConfirmAge: return "Підтвердження віку";
                }
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
        public System.Drawing.Color GetFlagColor(eStateMainWindows pStateMainWindows)
        {
            return FC.ContainsKey(pStateMainWindows) ? FC[pStateMainWindows] : System.Drawing.Color.Black;
        }
        public MainWindow()
        {
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
            EF = new EquipmentFront(GetBarCode, SetWeight, null /*CS.OnScalesData*/);

            EF.OnControlWeight += (pWeight, pIsStable) => 
            { 
                CS.OnScalesData(pWeight, pIsStable); 
            };            

            EF.SetStatus += (info) =>
            {
                EquipmentInfo = info.TextState;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"SetStatus ({info.ToJSON()})", eTypeLog.Expanded);
                if (EF.StatCriticalEquipment != eStateEquipment.On)
                    SetWaitConfirm(eTypeAccess.ErrorEquipment, null);
            };

            Global.OnReceiptCalculationComplete += (pReceipt) =>
            {
                try
                {
                    SetCurReceipt(pReceipt);
                    string ExciseStamp = null;
                    if (CurWares != null)
                    {
                        var lw = curReceipt?.Wares?.Where(r => r.CodeWares == CurWares.CodeWares);
                        if (lw != null && lw.Count() == 1)
                        {
                            ExciseStamp = lw.First()?.ExciseStamp;

                            if (!string.IsNullOrEmpty(ExciseStamp))
                                CurWares.ExciseStamp = ExciseStamp;
                        }
                        //Видалили товар але список не пустий.
                        if ((lw == null || lw.Count() == 0) && CurWares != null && curReceipt != null && curReceipt.Wares != null && curReceipt.Wares.Any() && curReceipt.Equals(CurWares))
                        {
                            CurWares.Quantity = 0;

                            foreach (var e in curReceipt.Wares.Where(el => el.IsLast = true))
                            {
                                e.IsLast = false;
                            }
                            var r = curReceipt.Wares.ToList();
                            CurWares.IsLast = true;
                            r.Add(CurWares);
                            CS.StartWeightNewGoogs(r);
                            return;
                        }
                    }
                    if (curReceipt?.Wares?.Count() == 0)
                        CS.WaitClear();

                    CS.StartWeightNewGoogs(curReceipt?.Wares);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage($"MainWindow.OnReceiptCalculationComplete Exception =>(pReceipt=>{pReceipt.ToJSON()}) => ({Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace})", eTypeLog.Error);
                }

                try
                {
                    ListWares = new ObservableCollection<ReceiptWares>(pReceipt?.Wares);
                    Dispatcher.BeginInvoke(new ThreadStart(() => { WaresList.ItemsSource = ListWares; Recalc(); }));
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage($"MainWindow.OnReceiptCalculationComplete Exception =>(pReceipt=>{pReceipt.ToJSON()}) => ({Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace})", eTypeLog.Error);
                }
                FileLogger.WriteLogMessage($"MainWindow.OnReceiptCalculationComplete Exception =>(pReceipt=>{pReceipt.ToJSON()})", eTypeLog.Full);
            };

            Global.OnSyncInfoCollected += (SyncInfo) =>
            {
                //Почалось повне оновлення.
                if (SyncInfo.Status == eSyncStatus.StartedFullSync)
                    SetWaitConfirm(eTypeAccess.StartFullUpdate);
                //Помилка оновлення.
                if (SyncInfo.Status == eSyncStatus.Error)
                    SetWaitConfirm(eTypeAccess.ErrorFullUpdate);

                if (TypeAccessWait == eTypeAccess.StartFullUpdate && SyncInfo.Status == eSyncStatus.SyncFinishedSuccess)
                {
                    TypeAccessWait = eTypeAccess.NoDefinition;
                    SetStateView(eStateMainWindows.WaitInput);
                }

                FileLogger.WriteLogMessage($"MainWindow.OnSyncInfoCollected Status=>{SyncInfo.Status} StatusDescription=>{SyncInfo.StatusDescription}", eTypeLog.Full);
            };

            Global.OnStatusChanged += (Status) =>
            {
                ExchangeRateBar = Status.StringColor;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExchangeRateBar"));
            };

            Global.OnClientChanged += (pClient, pIdWorkPlace) =>
            {
                Client = pClient;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClientName"));
                FileLogger.WriteLogMessage($"MainWindow.OnClientChanged(Client.Wallet=> {pClient.Wallet} SumBonus=>{pClient.SumBonus})", eTypeLog.Full);
            };

            Bl.OnAdminBarCode += (pUser) => { SetConfirm(pUser, false); };

            Bl.OnCustomWindow += (pCW) => { customWindow = pCW; SetStateView(eStateMainWindows.WaitCustomWindows); };
            
            //Обробка стану контрольної ваги.
            CS.OnStateScale += (pStateScale, pRW, pСurrentlyWeight) =>
            {
                StateScale = pStateScale;

                switch (StateScale)
                {
                    case eStateScale.BadWeight:
                        customWindowButtons = new ObservableCollection<CustomButton>() { new CustomButton() { Id = 1, Text = "Підтвердити вагу", IsAdmin = true },
                                                                                      new CustomButton() { Id = 2, Text = "Добавити вагу", IsAdmin = true } };
                        customWindow = new CustomWindow() { Id = eWindows.ConfirmWeight, Buttons = customWindowButtons };
                        break;
                    case eStateScale.WaitGoods:
                        customWindowButtons = new ObservableCollection<CustomButton>() { new CustomButton() { Id = 1, Text = "Підтвердити вагу", IsAdmin = true } };
                        customWindow = new CustomWindow() { Id = eWindows.ConfirmWeight, Buttons = customWindowButtons };
                        break;
                    default:
                        customWindowButtons = null;
                        break;
                }

                switch (StateScale)
                {
                    case eStateScale.BadWeight:
                    case eStateScale.NotStabilized:
                    case eStateScale.WaitClear:
                    case eStateScale.WaitGoods:
                        
                        SetWaitConfirm(eTypeAccess.FixWeight, pRW); // SetStateView(eStateMainWindows.WaitWeight);
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WaitAdminText"));
                        break;
                    case eStateScale.Stabilized:
                        if (State == eStateMainWindows.WaitWeight || State == eStateMainWindows.WaitAdmin)
                            SetStateView(eStateMainWindows.WaitInput);
                        if (pRW != null)
                            Bl.FixWeight(pRW);
                        break;
                }
            };

            WaresQuantity = "0";
            MoneySum = "0";
            Volume = true;

            ad = new Admin(new User() { NameUser="xxxxx"}, this, EF);
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

            ListWares = new ObservableCollection<ReceiptWares>(StartData());
            WaresList.ItemsSource = ListWares;// Wares;

            ua.Tag = new CultureInfo("uk");
            en.Tag = new CultureInfo("en");
            hu.Tag = new CultureInfo("hu");
            pln.Tag = new CultureInfo("pl");

            CultureInfo currLang = App.Language;
            Recalc();
            if (State == eStateMainWindows.StartWindow)
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
        private void SetCurReceipt(Receipt pReceipt)
        {
            curReceipt = pReceipt;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GetBackgroundColor"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PriceIsNotZero"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCheckReturn"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClientName"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsAgeRestrict"));
        }        
        public void GetBarCode(string pBarCode, string pTypeBarCode)
        {
            if (State == eStateMainWindows.WaitExciseStamp)
            {
                string ExciseStamp = GetExciseStamp(pBarCode);
                if (!string.IsNullOrEmpty(ExciseStamp))
                    AddExciseStamp(ExciseStamp);
                return;
            }
            if (State == eStateMainWindows.WaitInput)
            {
                if (curReceipt?.IsLockChange == false)
                    Bl.GetBarCode(pBarCode, pTypeBarCode);
                return;
                //else // В данbq чек добавити товар не можна
            }            
            
                var u = Bl.GetUserByBarCode(pBarCode);
                if (u != null)
                    Bl.OnAdminBarCode?.Invoke(u);
            
        }
        void SetConfirm(User pUser, bool pIsFirst)
        {
            if (pUser == null)
            {
                switch (TypeAccessWait)
                {
                    /*case eTypeAccess.ConfirmAge:
                    case eTypeAccess.ChoicePrice:
                    case eTypeAccess.AddNewWeight:                    
                    case eTypeAccess.ErrorEquipment:        
                        TypeAccessWait = eTypeAccess.NoDefinition;
                        SetStateView(eStateMainWindows.WaitAdmin);
                        break;*/
                    case eTypeAccess.DelReciept:
                    case eTypeAccess.DelWares:
                        TypeAccessWait = eTypeAccess.NoDefinition;
                        SetStateView(eStateMainWindows.WaitInput);
                        break;
                    default:
                        SetStateView(eStateMainWindows.WaitAdmin);
                        break;
                }
                return;
            }

            IsIgnoreExciseStamp = Access.GetRight(pUser, eTypeAccess.ExciseStamp);
            IsAddNewWeight = Access.GetRight(pUser, eTypeAccess.AddNewWeight);
            IsFixWeight = Access.GetRight(pUser, eTypeAccess.FixWeight);

            if (TypeAccessWait == eTypeAccess.NoDefinition || TypeAccessWait < 0)
                return;
            if (!Access.GetRight(pUser, TypeAccessWait))
            {
                ShowErrorMessage($"Не достатньо прав для операції {TypeAccessWait} в {pUser.NameUser}");
//                MessageBox.Show($"Не достатньо прав для операції {TypeAccessWait} в {pUser.NameUser}");
                return;
            }

            switch (TypeAccessWait)
            {
                case eTypeAccess.DelWares:
                    if (curReceipt?.IsLockChange == false)
                    {
                        Bl.ChangeQuantity(CurWares, 0);
                        TypeAccessWait = eTypeAccess.NoDefinition;
                        SetStateView(eStateMainWindows.WaitInput);
                    }
                    break;
                case eTypeAccess.DelReciept:
                    Bl.SetStateReceipt(curReceipt, eStateReceipt.Canceled);
                    Bl.GetNewIdReceipt();
                    TypeAccessWait = eTypeAccess.NoDefinition;
                    SetStateView(eStateMainWindows.StartWindow);
                    break;
                case eTypeAccess.ConfirmAge:
                    Bl.AddEventAge(curReceipt);
                    TypeAccessWait = eTypeAccess.NoDefinition;
                    PrintAndCloseReceipt();

                    break;
                case eTypeAccess.ChoicePrice:
                      foreach (Models.Price el in Prices.ItemsSource)
                        el.IsEnable = true;                    
                    TypeAccessWait = eTypeAccess.NoDefinition;
                    break;
                case eTypeAccess.AddNewWeight:
                case eTypeAccess.FixWeight:
                    SetStateView(eStateMainWindows.WaitAdmin);
                    break;
            }
           
        }  
        void SetWaitConfirm(eTypeAccess pTypeAccess, ReceiptWares pRW = null)
        {
            CurWares = pRW;
            TypeAccessWait = pTypeAccess;
            SetStateView(eStateMainWindows.WaitAdmin);
        }
        void SetStateView(eStateMainWindows pSMV = eStateMainWindows.NotDefine)
        {
            Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    if ( !( !IsLockSale && EF.StatCriticalEquipment == eStateEquipment.On && Bl.ds.IsReady && CS.IsOk && curReceipt?.IsNeedExciseStamp==false) &&
                     !(pSMV == eStateMainWindows.WaitAdmin || pSMV == eStateMainWindows.WaitAdminLogin) )
                    {
                        eTypeAccess Res = eTypeAccess.NoDefinition;
                        if (EF.StatCriticalEquipment != eStateEquipment.On) Res = eTypeAccess.ErrorEquipment;
                        else
                           if (!Bl.ds.IsReady) Res = (Bl.ds.Status == eSyncStatus.Error ? eTypeAccess.ErrorFullUpdate : eTypeAccess.StartFullUpdate);
                        else
                            if (IsLockSale) Res = eTypeAccess.LockSale;
                        else
                            if (!CS.IsOk) Res = eTypeAccess.FixWeight;
                        else
                            if(curReceipt?.IsNeedExciseStamp==true) Res = eTypeAccess.ExciseStamp;
                        SetWaitConfirm(Res);
                        return;
                    }
                   
                        if (pSMV != eStateMainWindows.NotDefine)
                        {
                            State = pSMV;
                            EF.SetColor(GetFlagColor(State));                            
                        }
                    if (IsLockSale && State != eStateMainWindows.WaitAdmin && State != eStateMainWindows.WaitAdminLogin)
                    {
                        SetWaitConfirm(eTypeAccess.LockSale);
                        return;
                    }
                    if (State != eStateMainWindows.WaitAdmin && State != eStateMainWindows.WaitAdminLogin)
                        TypeAccessWait = eTypeAccess.NoDefinition;



                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsAgeRestrict"));

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
                    textInAll.Visibility = Visibility.Visible;
                    valueInAll.Visibility = Visibility.Visible;
                    ConfirmAgeMessage.Visibility = Visibility.Collapsed;
                    ConfirmAge.Visibility = Visibility.Collapsed;
                    WaitKashier.Visibility = Visibility.Collapsed;
                    CustomWindows.Visibility = Visibility.Collapsed;

                    CaptionCustomWindows.Visibility = Visibility.Visible;
                    ImageCustomWindows.Visibility = Visibility.Visible;
                    CancelCustomWindows.Visibility = Visibility.Visible;
                    TextBoxCustomWindows.Visibility = Visibility.Visible;
                    KeyboardCustomWindows.Visibility = Visibility.Visible;
                    StartVideo.Stop();

                    switch (State)
                    {
                        case eStateMainWindows.StartWindow:
                            StartShopping.Visibility = Visibility.Visible;
                            textInAll.Visibility = Visibility.Collapsed;
                            valueInAll.Visibility = Visibility.Collapsed;
                            StartVideo.Play();
                            break;
                        case eStateMainWindows.WaitInputPrice:
                            TypeAccessWait = eTypeAccess.ChoicePrice;
                            var rrr = new ObservableCollection<Models.Price>(CurWares.Prices.OrderByDescending(r => r.Price).Select(r => new Models.Price(r.Price, Access.GetRight(TypeAccessWait), r.TypeWares)));
                            rrr.First().IsEnable = true;

                            Prices.ItemsSource = rrr;//new ObservableCollection<Price>(rr);

                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            ChoicePrice.Visibility = Visibility.Visible;

                            break;
                        case eStateMainWindows.WaitExciseStamp:
                            TBExciseStamp.Text = "";
                            ExciseStamp.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;
                            TBExciseStamp.Focus();
                            break;
                        case eStateMainWindows.WaitWeight:
                            EF.StartWeight();
                            WeightWares.Visibility = Visibility.Visible;

                            break;
                        case eStateMainWindows.WaitAdmin:
                            WaitAdmin.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            BackgroundWares.Visibility = Visibility.Visible;

                            switch (TypeAccessWait)
                            {
                                case eTypeAccess.FixWeight:
                                case eTypeAccess.AddNewWeight:
                                    //case eTypeAccess.
                                    CustomButtonsWaitAdmin.ItemsSource = customWindowButtons;
                                    break;
                                default:
                                    CustomButtonsWaitAdmin.ItemsSource = null;
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
                            MoneySumToRound = MoneySum;
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
                                CustomWindowsItemControl.ItemsSource = new ObservableCollection<CustomButton>(customWindow.Buttons);
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
                        case eStateMainWindows.WaitInput:
                            IsIgnoreExciseStamp = false;
                            IsAddNewWeight = false;
                            IsFixWeight = false;
                            break;
                        default:
                            break;
                    }
                }));
        }

        private void _Delete(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.DataContext is ReceiptWares)
            {
                var el = btn.DataContext as ReceiptWares;
                if (el == null)
                {
                    return;
                }
                if (Access.GetRight(eTypeAccess.DelWares) || !el.IsConfirmDel)
                {
                    if (curReceipt?.IsLockChange == false)
                    {
                        Bl.ChangeQuantity(el, 0);
                    }
                }
                else
                    SetWaitConfirm(eTypeAccess.DelWares, el);
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
                temp.Quantity = tempQuantity;
                if (curReceipt?.TypeReceipt == eTypeReceipt.Refund && tempQuantity > temp.MaxRefundQuantity)
                {
                    temp.Quantity = (decimal)temp.MaxRefundQuantity;
                }
                if (curReceipt?.IsLockChange == false)
                {
                    Bl.ChangeQuantity(temp, temp.Quantity);
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
            _MoneySum = ListWares.Sum(r => r.SumTotal);
            MoneySum = _MoneySum.ToString();
            WaresQuantity = ListWares.Count().ToString();
            SV_WaresList.ScrollToEnd();
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
            pln.Style = (Style)pln.FindResource("Default");
            switch (btn.Name)
            {
                case "uk":
                    ua.Style = (Style)ua.FindResource("yelowButton");
                    break;
                case "en":
                    en.Style = (Style)en.FindResource("yelowButton");
                    break;
                case "hu":
                    hu.Style = (Style)hu.FindResource("yelowButton");
                    break;
                case "pln":
                    pln.Style = (Style)pln.FindResource("yelowButton");
                    break;
            }
        }

        private void _Back(object sender, RoutedEventArgs e)
        {
            // Правильний блок.
            if (Access.GetRight(eTypeAccess.DelReciept) || curReceipt?.SumReceipt == 0)
            {
                Bl.SetStateReceipt(curReceipt, eStateReceipt.Canceled);
                Bl.GetNewIdReceipt();
                SetStateView(eStateMainWindows.StartWindow);
            }
            else
                SetWaitConfirm(eTypeAccess.DelReciept, null);
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

        public void AddWares(int pCodeWares, int pCodeUnit = 0, decimal pQuantity = 0m, decimal pPrice = 0m, GW pGV = null)
        {
            if (pGV != null)
            {
                CurW = pGV;
                NameWares.Content = CurW.Name;

                Image im = null;
                foreach (var el in GridWeightWares.Children)
                {
                    im = el as Image;
                    if (im != null)
                        break;
                }
                if (im != null)
                    GridWeightWares.Children.Remove(im);
                if (File.Exists(CurW.Pictures))
                {
                    im = new Image
                    {
                        Source = new BitmapImage(new Uri(CurW.Pictures)),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    //Grid.SetColumn(Bt, i);
                    Grid.SetRow(im, 1);
                    GridWeightWares.Children.Add(im);
                }

                //GridWeightWares.Children.Clear();

                SetStateView(eStateMainWindows.WaitWeight);
                return;
            }


            if (pCodeWares > 0)
            {
                CurWares = Bl.AddWaresCode(curReceipt, pCodeWares, pCodeUnit, pQuantity, pPrice);

                if (CurWares != null)
                {
                    if (CurWares.TypeWares == eTypeWares.Alcohol)
                    {
                        SetStateView(eStateMainWindows.WaitExciseStamp);
                        return;
                    }

                    if (CurWares.Price == 0) //Повідомлення Про відсутність ціни
                    {

                    }
                    if (CurWares.Prices != null && pPrice == 0m) //Меню з вибором ціни. Сигарети.
                    {
                        if (CurWares.Prices.Count() > 1)
                        {
                            SetStateView(eStateMainWindows.WaitInputPrice);
                        }
                        else
                            if (CurWares.Prices.Count() == 1)
                            Bl.AddWaresCode(curReceipt, pCodeWares, pCodeUnit, pQuantity, CurWares.Prices.First().Price);
                    }

                }
            }
        }

        private void _ButtonHelp(object sender, RoutedEventArgs e)
        {
            SetStateView(eStateMainWindows.WaitAdmin);
        }

        private void _OwnBag(object sender, RoutedEventArgs e)
        {
            SetStateView(eStateMainWindows.ChoicePaymentMethod);
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

        /// <summary>
        /// Безготівкова оплата і Друк чека.
        /// </summary>
        /// <returns></returns>
        bool PrintAndCloseReceipt()
        {
            var R = Bl.GetReceiptHead(curReceipt, true);
            curReceipt = R;
            if (R.AgeRestrict > 0 && R.IsConfirmAgeRestrict == false)
            {
                SetWaitConfirm(eTypeAccess.ConfirmAge);
                return true;
            }

            if (R.StateReceipt == eStateReceipt.Prepare)
            {
                R.StateReceipt = eStateReceipt.StartPay;
                Bl.SetStateReceipt(curReceipt, eStateReceipt.StartPay);
                decimal sum = R.Wares.Sum(r => (r.SumTotal)); //TMP!!!Треба переробити
                SetStateView(eStateMainWindows.ProcessPay);
                var pay = EF.PosPurchase(sum);
                if (pay != null)
                {
                    pay.SetIdReceipt(R);
                    Bl.db.ReplacePayment(new List<Payment>() { pay });
                    Bl.SetStateReceipt(curReceipt, eStateReceipt.Pay);
                    R.StateReceipt = eStateReceipt.Pay;
                    R.Payment = new List<Payment>() { pay };
                }
                else
                {
                    SetStateView(eStateMainWindows.WaitInput);
                    R.StateReceipt = eStateReceipt.Prepare;
                    Bl.SetStateReceipt(curReceipt, eStateReceipt.Prepare);
                }
            }

            if (R.StateReceipt == eStateReceipt.Pay)
            {
                R.Client = Client;
                R.StateReceipt = eStateReceipt.StartPrint;
                Bl.SetStateReceipt(curReceipt, eStateReceipt.StartPrint);
                try
                {
                    SetStateView(eStateMainWindows.ProcessPrintReceipt);
                    //Bl.SetStateReceipt(curReceipt, eStateReceipt.Canceled);
                    var res = EF.PrintReceipt(R);
                    Bl.InsertLogRRO(res);
                    SetStateView(eStateMainWindows.WaitInput);
                    if (res.CodeError == 0)
                    {
                        R.StateReceipt = eStateReceipt.Print;
                        Bl.UpdateReceiptFiscalNumber(R, res.FiscalNumber, res.SUM);
                        var r = Bl.GetNewIdReceipt();
                        //Global.OnReceiptCalculationComplete?.Invoke(new List<ReceiptWares>(), Global.IdWorkPlace);

                        return true;
                    }
                    else
                    {
                        R.StateReceipt = eStateReceipt.Pay;
                        Bl.SetStateReceipt(curReceipt, eStateReceipt.Pay);
                        ShowErrorMessage(res.Error + "Помилка друку чеків");
                        //MessageBox.Show(res.Error, "Помилка друку чеків");
                    }
                    SetStateView(eStateMainWindows.WaitInput);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(e.Message, eTypeLog.Error);
                }
            }
            return false;
        }
        private void ShowErrorMessage(string ErrorMessage)
        {
            Background.Visibility = Visibility.Visible;
            ErrorWindows.Visibility = Visibility.Visible;
            ErrorText.Text = ErrorMessage;
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
                        AddWares(CurWares.CodeWares, CurWares.CodeUnit, 1, price.price);
                    }
                }
            }
            catch { }
            SetStateView(eStateMainWindows.WaitInput);
        }

        private IEnumerable<ReceiptWares> StartData()
        {
            var RId = Bl.GetNewIdReceipt();
            //Bl.AddWaresBarCode(RId, "27833", 258m);
            //Bl.AddWaresBarCode(RId, "7622201819590", 1);
            Bl.GetClientByPhone(RId, "0503399110");
            //Bl.AddWaresBarCode(RId, "2201652300229", 2);
            //Bl.AddWaresBarCode(RId, "7775002160043", 1); //товар 2 кат
            //Bl.AddWaresBarCode(RId,"1110011760218", 11);
            //Bl.AddWaresBarCode(RId,"7773002160043", 1); //товар 2 кат
            return Bl.GetWaresReceipt(curReceipt);
        }

        /// <summary>
        /// Обробка ваги з основної ваги(Магелан)
        /// </summary>
        /// <param name="pWeight">Власне вага</param>
        /// <param name="pIsStable">Чи платформа стабілізувалась</param>
        public void SetWeight(double pWeight, bool pIsStable) { Weight = pWeight;}
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
            SetConfirm(null, true);
        }
        private void LoginButton(object sender, RoutedEventArgs e)
        {
            var U = Bl.GetUserByLogin(LoginTextBlock.Text, PasswordTextBlock.Text);
            if (U == null)
            {
                ShowErrorMessage("Не вірний логін чи пароль");
//                MessageBox.Show("Не вірний логін чи пароль");
                return;
            }

            if (TypeAccessWait > 0 &&   !(TypeAccessWait == eTypeAccess.FixWeight && CS.StateScale ==  eStateScale.WaitClear))//TypeAccessWait != eTypeAccess.NoDefinition 
            {
                SetConfirm(U, true);
                return;
            }

            if (Access.GetRight(U, eTypeAccess.AdminPanel))
            {
                SetStateView(eStateMainWindows.WaitInput);
                //Admin ad = new Admin(U, this,EF);
                
                ad.WindowState = WindowState.Maximized;
            }
            else
            {
                ShowErrorMessage($"Не достатньо прав на вхід в адмін панель для  {U.NameUser}");
//                MessageBox.Show($"Не достатньо прав на вхід в адмін панель для  {U.NameUser}");
            }

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

        void AddExciseStamp(string pES)
        {
            if (CurWares.AddExciseStamp(pES))
            {                 //Додання акцизноії марки до алкоголю
                SetStateView(eStateMainWindows.WaitInput);
                Bl.UpdateExciseStamp(new List<ReceiptWares>() { CurWares });
            }
            else
                ShowErrorMessage($"Дана акцизна марка вже використана");
//                MessageBox.Show($"Дана акцизна марка вже використана");
        }


        private void ChangedExciseStamp(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            IsExciseStamp = !string.IsNullOrEmpty(GetExciseStamp(textBox.Text));
        }

        private void ExciseStampNone(object sender, RoutedEventArgs e)
        {
            CurWares.AddExciseStamp("None");
            SetStateView(eStateMainWindows.WaitInput);
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
//                MessageBox.Show(ex.Message, "Eror", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MoneySumToRound = MoneySum;
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
            SetStateView(eStateMainWindows.WaitInput);

            Button btn = sender as Button;
            CustomButton res = btn.DataContext as CustomButton;

            if (res == null)
            {
                if (btn.Name.Equals("OKCustomWindows"))
                    res = new CustomButton() { Id = 1 };
            }
            if (res != null)
            {
                if (customWindow.Id == eWindows.ConfirmWeight && CS.RW!=null)
                {
                    CS.RW.FixWeightQuantity = CS.RW.Quantity;
                    CS.RW.FixWeight += Convert.ToDecimal(CS.СurrentlyWeight);
                    // Bl.FixWeight(CS.RW);
                    CS.StateScale = eStateScale.Stabilized;// OnScalesData(CS.curFullWeight,true);

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
            }

        }

        private void FindClientByPhoneClick(object sender, RoutedEventArgs e)
        {
            customWindow = new CustomWindow()
            {
                Id = eWindows.PhoneClient,
                Text = "Введіть ваш номер!",
                Caption = "Пошук за номером телефону",
                AnswerRequired = true,
                ValidationMask = @"^[+]{0,1}[0-9]{10,13}$",
                // Buttons = new List<CustomButton>() {new CustomButton() { Id = 666, Text = "Пошук картки" } }
            };

            SetStateView(eStateMainWindows.WaitCustomWindows);
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
    }

}
