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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Front.Equipments;
using Front.Models;
using ModelMID;
using ModelMID.DB;
using SharedLib;
using Utils;

namespace Front
{
    public class Price
    {
        //public static bool isFirst = true;
        public Price(decimal pPrice, bool pIsEnable,eTypeWares pTypeWares) //, bool pIsEnable = false
        {
            price = pPrice;
            IsEnable = pIsEnable;
            TypeWares = pTypeWares;
            //  IsEnable = isFirst;
            //if(isFirst)
            //  { isFirst = false; }             
        }
        public eTypeWares TypeWares { get; set; }
        public decimal price { get; set; }
        public string StrPrice { get { return $"{price.ToString("n2", CultureInfo.InvariantCulture)} ₴"; } }
        public bool IsEnable { get; set; }
        public Brush BackGroundColor
        {
            get
            {
                return new SolidColorBrush(IsEnable ? Color.FromArgb(20, 100, 100, 100) : Color.FromArgb(50, 100, 0, 0));
            }
        }
    }

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        Access Access = Access.GetAccess();
        public BL Bl;
        EquipmentFront EF;
        public ControlScale CS = new ControlScale();

        public Receipt curReceipt;//{ get; set; } = null;

        eStateMainWindows State = eStateMainWindows.StartWindow;
        public string WaresQuantity { get; set; }
        decimal _MoneySum;
        double tempMoneySum;
        public string MoneySum { get; set; }
        public string MoneySumToRound { get; set; }
        public string EquipmentInfo { get; set; }
        public bool Volume { get; set; }
        public string ChangeSumPaymant { get; set; } = "0";
        public bool IsIgnoreExciseStamp { get; set; }
        public bool isExciseStamp { get; set; }
        public string GetBackgroundColor { get { return curReceipt?.TypeReceipt == eTypeReceipt.Refund ? "#FFE5E5" : "#FFFFFF"; } }
        public bool IsCheckReturn { get { return curReceipt?.TypeReceipt == eTypeReceipt.Refund ? true : false; } }
        public bool PriceIsNotZero
        {
            get
            {
                if (_MoneySum >= 0 && WaresQuantity != "0")
                {
                    return true;
                }
                else return false;
            }
        }

        public bool IsPresentFirstTerminal
        {
            get
            {
                if (EF.BankTerminal1 != null)
                {
                    return true;
                }
                else return false;
            }
        }
        public bool IsPresentSecondTerminal
        {
            get
            {
                if (EF.BankTerminal2 != null)
                {
                    return true;
                }
                else return false;
            }
        }
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
                }
                return null;
            }
        }
        /// <summary>
        /// Вага з основної ваги
        /// </summary>
        public double Weight { get; set; } = 0d;

        public string WeightControl { get; set; }

        public GW CurW { get; set; } = null;

        public eTypeAccess TypeAccessWait { get; set; }

        public ReceiptWares CurWares { get; set; } = null;
        public ObservableCollection<ReceiptWares> ListWares { get; set; }
        public ObservableCollection<CustomButton> customWindowButtons { get; set; } 
        public CustomWindow customWindow { get; set; }



        public MainWindow()
        {
            var c = new Config("appsettings.json");// Конфігурація Програми(Шляхів до БД тощо)

            //Для касового місця Запит логін пароль.
            if (Global.TypeWorkplace == eTypeWorkplace.SelfServicCheckout)
                Access.СurUser = new User() { TypeUser = eTypeUser.Client, CodeUser = 99999999, Login = "Client", NameUser = "Client" };

            Bl = new BL(true);
            EF = new EquipmentFront(Bl.GetBarCode, SetWeight, CS.OnScalesData);

            //SetBarCode += Bl.GetBarCode;// (pBarCode, pTypeBarCode) => { Bl.GetBarCode(pBarCode, pTypeBarCode); };
            //SetControlWeight += Bl.CS.OnScalesData; // (pWeight, isStable)=>{ });
            //ad =  new Admin();
            EF.SetStatus += (info) =>
            {
                EquipmentInfo = info.TextState;
            };
            Global.OnReceiptCalculationComplete += (pReceipt) =>
            {
                SetCurReceipt(pReceipt);

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
                FileLogger.WriteLogMessage($"MainWindow.OnSyncInfoCollected Status=>{SyncInfo.Status} StatusDescription=>{SyncInfo.StatusDescription}", eTypeLog.Full);
            };

            Global.OnStatusChanged += (Status) => { };            
            Global.OnClientChanged += (pClient, pIdWorkPlace) =>
            {
                FileLogger.WriteLogMessage($"MainWindow.OnClientChanged(Client.Wallet=> {pClient.Wallet} SumBonus=>{pClient.SumBonus})", eTypeLog.Full);
            };

            Global.OnAdminBarCode += (pUser) => { SetConfirm(pUser, false); };

            Global.OnReceiptChanged += (pReceipt) =>
            {
                SetCurReceipt(pReceipt);
            };
            //Обробка стану контрольної ваги.
            CS.OnStateScale += (pStateScale)=>
            { 
            
            };

            WaresQuantity = "0";
            MoneySum = "0";
            Volume = true;

            InitializeComponent();

            //MessageBox.Show(NameFirstTerminal);

            ListWares = new ObservableCollection<ReceiptWares>(StartData());
            WaresList.ItemsSource = ListWares;// Wares;

            ua.Tag = new CultureInfo("uk");
            en.Tag = new CultureInfo("en");
            hu.Tag = new CultureInfo("hu");
            pln.Tag = new CultureInfo("pl");

            CultureInfo currLang = App.Language;
            Recalc();
        }

        private void SetCurReceipt(Receipt pReceipt)
        {
            curReceipt = pReceipt;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GetBackgroundColor"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PriceIsNotZero"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCheckReturn"));
        }
        public void CreateCustomWindiws()
        {
            customWindow = new CustomWindow();
            customWindow.Text = "Текст вікна";
            customWindow.PathPicture = @"icons\Signal.png";
            customWindow.Caption = "Назва вікна";
            customWindow.AnswerRequired = true;
            customWindow.ValidationMask = "Щось)";

            customWindow.Buttons = new List<CustomButton>()
            {
                new CustomButton(){Id =1, Text="First button" },
                new CustomButton(){Id =2, Text="Two Button"},
                new CustomButton(){Id =3, Text="asdvsadvsdfvsdf button" },
                new CustomButton(){Id =4, Text="asdvssdvsdfadvsdfvsdf button" },
            };


            CastomWindowsItemControl.ItemsSource = new ObservableCollection<CustomButton>(customWindow.Buttons);
            if (customWindow.Caption == null) CaptionCastomWindows.Visibility = Visibility.Collapsed;
            if (customWindow.PathPicture == null) ImageCastomWindows.Visibility = Visibility.Collapsed;
            if (customWindow.AnswerRequired == false) CancelCastomWindows.Visibility = Visibility.Collapsed;
            if (customWindow.ValidationMask == null)
            {
                TextBoxCastomWindows.Visibility = Visibility.Collapsed;
                KeyboardCustomWindows.Visibility = Visibility.Collapsed;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("customWindow"));
        }
        public void GetBarCode(string pBarCode, string pTypeBarCode)
        {
            if (State == eStateMainWindows.WaitInput)
            { 
                if (curReceipt?.IsLockChange == false)
                    Bl.GetBarCode(pBarCode, pTypeBarCode);
                //else // В данbq чек добавити товар не можна

            } else
            {
                var u = Bl.GetUserByBarCode(pBarCode);
                if (u != null)
                    Global.OnAdminBarCode?.Invoke(u);
            }
        }

        void SetConfirm(User pUser, bool pIsFirst)
        {
            IsIgnoreExciseStamp = Access.GetRight(pUser, eTypeAccess.ExciseStamp);

            if (TypeAccessWait == eTypeAccess.NoDefinition)
                return;
            if (!Access.GetRight(pUser, TypeAccessWait))
            {
                MessageBox.Show($"Не достатньо прав для операції {TypeAccessWait} в {pUser.NameUser}");
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
                    PrintAndCloseReceipt();
                    TypeAccessWait = eTypeAccess.NoDefinition;
                    break;
                case eTypeAccess.ChoicePrice:
                    //var rrr = new ObservableCollection<Price>(CurWares.Prices.OrderByDescending(r => r).Select(r => new Price(r, true)));
                    foreach (Price el in Prices.ItemsSource)
                        el.IsEnable = true;
                    //Prices.ItemsSource = rrr;
                    //SetStateView(eStateMainWindows.WaitInputPrice);
                    TypeAccessWait = eTypeAccess.NoDefinition;
                    break;
            }
            // TypeAccessWait = eTypeAccess.NoDefinition;
        }


        // public ReceiptWares ReceiptWaresWait { get; set; }

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
                    if (pSMV != eStateMainWindows.NotDefine)
                        State = pSMV;
                    if (State != eStateMainWindows.WaitAdmin && State != eStateMainWindows.WaitAdminLogin)
                        TypeAccessWait = eTypeAccess.NoDefinition;

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
                    CastomWindows.Visibility = Visibility.Collapsed;
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
                            var rrr = new ObservableCollection<Price>(CurWares.Prices.OrderByDescending(r => r).Select(r => new Price(r.Price, Access.GetRight(TypeAccessWait),r.TypeWares)));
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
                            CreateCustomWindiws();
                            CastomWindows.Visibility = Visibility.Visible;
                            break;
                        case eStateMainWindows.WaitInput:
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
            _MoneySum = ListWares.Sum(r => r.Sum);
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
            SetStateView(eStateMainWindows.WaitCustomWindows);
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
            if (R.Wares.Where(el => el.TypeWares > 0).Count() > 0 && R.ReceiptEvent.Where(el => el.EventType == ReceiptEventType.AgeRestrictedProduct).Count() == 0)
            {
                SetWaitConfirm(eTypeAccess.ConfirmAge);
                return true;
            }

            if (R.StateReceipt == eStateReceipt.Prepare)
            {
                decimal sum = R.Wares.Sum(r => r.Sum); //TMP!!!Треба переробити
                SetStateView(eStateMainWindows.ProcessPay);
                var pay = EF.PosPurchase(sum);
                if (pay != null)
                {
                    pay.SetIdReceipt(R);
                    Bl.db.ReplacePayment(new List<Payment>() { pay });
                    Bl.SetStateReceipt(curReceipt, eStateReceipt.Pay);
                    R.StateReceipt = eStateReceipt.Pay;
                }
                else
                    SetStateView(eStateMainWindows.WaitInput);
            }


            if (R.StateReceipt == eStateReceipt.Pay)
            {
                try
                {
                    SetStateView(eStateMainWindows.ProcessPrintReceipt);
                    Bl.SetStateReceipt(curReceipt, eStateReceipt.Canceled);
                    var res = EF.PrintReceipt(R);
                    Bl.InsertLogRRO(res);
                    if (res.CodeError == 0)
                    {
                        Bl.UpdateReceiptFiscalNumber(R, res.FiscalNumber, res.SUM);
                        var r = Bl.GetNewIdReceipt();
                        //Global.OnReceiptCalculationComplete?.Invoke(new List<ReceiptWares>(), Global.IdWorkPlace);
                        SetStateView(eStateMainWindows.WaitInput);
                        return true;
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
                    var price = btn.DataContext as Price;

                    TextBlock Tb = btn.Content as TextBlock;
                    if (Tb != null)
                    {
                        //decimal Pr = Convert.ToDecimal(Tb.Tag);
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
            Bl.AddWaresBarCode(RId, "27833", 258m);
            Bl.AddWaresBarCode(RId, "7622300813437", 1);
            Bl.AddWaresBarCode(RId, "2201652300229", 2);
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
        public void SetWeight(double pWeight, bool pIsStable)
        {
            Weight = pWeight;
            Debug.WriteLine(Weight);
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

        private void LoginButton(object sender, RoutedEventArgs e)
        {
            var U = Bl.GetUserByLogin(LoginTextBlock.Text, PasswordTextBlock.Text);
            if (U == null)
            {
                MessageBox.Show("Не вірний логін чи пароль");
                return;
            }

            if (TypeAccessWait != eTypeAccess.NoDefinition)
            {
                SetConfirm(U, true);
                return;
            }

            if (Access.GetRight(U, eTypeAccess.AdminPanel))
            {
                SetStateView(eStateMainWindows.WaitInput);
                Admin ad = new Admin();
                ad.Show();
            }
            else
            {
                MessageBox.Show($"Не достатньо прав на вхід в адмін панель для  {U.NameUser}");
            }

        }

        private void TextLoginChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void TextPasswordChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void AddExciseStamp(object sender, RoutedEventArgs e)
        {
            if (CurWares.AddExciseStamp(TBExciseStamp.Text))
                //Додання акцизноії марки до алкоголю
                SetStateView(eStateMainWindows.WaitInput);
            else
                MessageBox.Show($"Дана акцизна марка вже використана");
        }


        private void ChangedExciseStamp(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Regex regex = new Regex(@"^\w{4}[0-9]{6}?$");
            isExciseStamp = regex.IsMatch(textBox.Text);
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
                MessageBox.Show(ex.Message, "Eror", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(terminalPaymentInfo.enteredDataFromTerminal.AuthorizationCode);//як приклад
            }
        }
    }
}
