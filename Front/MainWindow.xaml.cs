//using Hangfire.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Front.Equipments;
using Front.Models;
using Microsoft.Extensions.Configuration;
using ModelMID;
using SharedLib;

namespace Front
{

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        eStateMainWindows State = eStateMainWindows.StartWindow;
        public event PropertyChangedEventHandler PropertyChanged;

        public string WaresQuantity { get; set; }
        decimal _MoneySum;
        public string MoneySum { get; set; }
        public string EquipmentInfo { get; set; }
        public bool Volume { get; set; }
        /// <summary>
        /// Чи зчитано штрихкод Адміна.
        /// </summary>
        public bool IsAdminBarCode { get; set; }
        /// <summary>
        /// Вага з основної ваги
        /// </summary>
        public double Weight { get; set; } = 0d;

        public string WeightControl { get; set; }

        public GW CurW { get; set; } = null;

        public BL Bl;
        EquipmentFront EF;

        public ObservableCollection<ReceiptWares> ListWares { get; set; }
        //public ObservableCollection<decimal> Prices { get; set; } = new ObservableCollection<decimal>;

        public MainWindow()
        {
            var c = new Config("appsettings.json");// Конфігурація Програми(Шляхів до БД тощо)          

            Bl = new BL(true);
            EF = new EquipmentFront(Bl.GetBarCode, SetWeight, Bl.CS.OnScalesData);

            //SetBarCode += Bl.GetBarCode;// (pBarCode, pTypeBarCode) => { Bl.GetBarCode(pBarCode, pTypeBarCode); };
            //SetControlWeight += Bl.CS.OnScalesData; // (pWeight, isStable)=>{ });
            //ad =  new Admin();
            EF.SetStatus += (info) =>
            {
                EquipmentInfo = info.TextState;
            };
            Global.OnReceiptCalculationComplete += (wareses, guid) =>
            {
                try
                {
                    ListWares = new ObservableCollection<ReceiptWares>(wareses);
                    Dispatcher.BeginInvoke(new ThreadStart(() => { WaresList.ItemsSource = ListWares; Recalc(); }));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error OnReceiptCalculationComplete" + ex.Message);
                }
                Debug.WriteLine("\n==========================Start===================================");
                foreach (var receiptWarese in wareses)
                {
                    Debug.WriteLine($"Promotion=>{receiptWarese.GetStrWaresReceiptPromotion.Trim()} \n{receiptWarese.NameWares} - {receiptWarese.Price} Quantity=> {receiptWarese.Quantity} SumDiscount=>{receiptWarese.SumDiscount}");
                }
                Debug.WriteLine("===========================End==========================================\n");
            };

            Global.OnSyncInfoCollected += (SyncInfo) =>
            {
                Console.WriteLine($"OnSyncInfoCollected Status=>{SyncInfo.Status} StatusDescription=>{SyncInfo.StatusDescription}");
            };

            Global.OnStatusChanged += (Status) => { };
            Global.OnChangedStatusScale += (Status) => { };
            Global.OnClientChanged += (client, guid) =>
            {
                Debug.WriteLine($"Client.Wallet=> {client.Wallet} SumBonus=>{client.SumBonus} ");
            };
            Global.OnAdminBarCode += (pUser) => { IsAdminBarCode = true; SetStateView(eStateMainWindows.NotDefine, true); };

            WaresQuantity = "0";
            MoneySum = "0";
            Volume = true;

            InitializeComponent();


            ListWares = new ObservableCollection<ReceiptWares>(StartData());
            WaresList.ItemsSource = ListWares;// Wares;

            ua.Tag = new CultureInfo("uk");
            en.Tag = new CultureInfo("en");
            hu.Tag = new CultureInfo("hu");
            pln.Tag = new CultureInfo("pln");


            CultureInfo currLang = App.Language;
            Recalc();
        }

        void SetStateView(eStateMainWindows pSMV = eStateMainWindows.NotDefine, bool pIsBarCodeAdmin = false)
        {
            Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    if (pSMV != eStateMainWindows.NotDefine)
                        State = pSMV;

                    ExciseStamp.Visibility = Visibility.Collapsed;
                    ChoicePrice.Visibility = Visibility.Collapsed;
                    Background.Visibility = Visibility.Collapsed;
                    WaitAdmin.Visibility = Visibility.Collapsed;
                    WaitAdminLogin.Visibility = Visibility.Collapsed;
                    WeightWares.Visibility = Visibility.Collapsed;
                    WaitPayment.Visibility = Visibility.Collapsed;
                    StartShopping.Visibility = Visibility.Collapsed;
                    textWaresQuantity.Visibility = Visibility.Visible;
                    valueWaresQuantity.Visibility = Visibility.Visible;
                    textInAll.Visibility = Visibility.Visible;
                    valueInAll.Visibility = Visibility.Visible;
                    StartVideo.Close();

                    switch (State)
                    {
                        case eStateMainWindows.StartWindow:
                            StartShopping.Visibility = Visibility.Visible;
                            textWaresQuantity.Visibility = Visibility.Collapsed;
                            valueWaresQuantity.Visibility = Visibility.Collapsed;
                            textInAll.Visibility = Visibility.Collapsed;
                            valueInAll.Visibility = Visibility.Collapsed;
                            StartVideo.Play();
                            break;
                        case eStateMainWindows.WaitInputPrice:
                            Prices.ItemsSource = new ObservableCollection<decimal>(CurWares.Prices/*.Select(r=>Convert.ToString(r))*/);
                            Background.Visibility = Visibility.Visible;
                            ChoicePrice.Visibility = Visibility.Visible;
                            break;
                        case eStateMainWindows.WaitExciseStamp:
                            ExciseStamp.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            TBExciseStamp.Focus();
                            break;
                        case eStateMainWindows.WaitWeight:
                            EF.StartWeight();
                            WeightWares.Visibility = Visibility.Visible;
                            break;
                        case eStateMainWindows.WaitAdmin:
                            WaitAdmin.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            break;
                        case eStateMainWindows.WaitAdminLogin:
                            WaitAdminLogin.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            LoginTextBlock.Focus();
                            break;
                        case eStateMainWindows.WaitFindWares:
                            FindWaresWin FWW = new FindWaresWin(this);
                            FWW.Show();
                            break;
                        case eStateMainWindows.ProcessPay:
                            WaitPayment.Visibility = Visibility.Visible;
                            Background.Visibility = Visibility.Visible;
                            break;
                        case eStateMainWindows.ProcessPrintReceipt:
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
                //ListWares.Remove((ReceiptWares)btn.DataContext);
                Bl.ChangeQuantity(el, 0);
            }

        }

        private void _Minus(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.DataContext is ReceiptWares)
            {
                ReceiptWares temp = btn.DataContext as ReceiptWares;
                temp.Quantity--;
                Bl.ChangeQuantity(temp, temp.Quantity);
            }
        }

        private void _Plus(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.DataContext is ReceiptWares)
            {
                ReceiptWares temp = btn.DataContext as ReceiptWares;
                temp.Quantity++;
                Bl.ChangeQuantity(temp, temp.Quantity);
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
            //!!!TMP
            var rand = new Random();
            string sql = @"select CODE_WARES from (select CODE_WARES,row_number() over (order by code_wares)  as nn from price p where p.code_DEALER=2)
                        where nn=  cast(abs(random()/9223372036854775808.0)*1000 as int)";
            var CodeWares = Bl.db.db.ExecuteScalar<int>(sql);
            if (CodeWares > 0)
                Bl.AddWaresCode(CodeWares, 0, Math.Round(1M + 5M * rand.Next() / (decimal)int.MaxValue));
        }

        private void _Search(object sender, RoutedEventArgs e)
        {
            SetStateView(eStateMainWindows.WaitFindWares);
        }

        public ReceiptWares CurWares { get; set; } = null;

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
                CurWares = Bl.AddWaresCode(pCodeWares, pCodeUnit, pQuantity, pPrice);

                if (CurWares != null)
                {
                    if (CurWares.TypeWares == 1)
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
                            Bl.AddWaresCode(pCodeWares, pCodeUnit, pQuantity, CurWares.Prices.First());
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
            SetStateView(eStateMainWindows.WaitExciseStamp);
        }

        private void _BuyBag(object sender, RoutedEventArgs e)
        {
            SetStateView(eStateMainWindows.StartWindow);
        }

        private void _Cancel(object sender, RoutedEventArgs e)
        {
            SetStateView(eStateMainWindows.WaitInput);
        }

        private void _ButtonPayment(object sender, RoutedEventArgs e)
        {

          /*  var pay=EF.PosPurchase(_MoneySum);
            pay.SetIdReceipt(Bl.curReciptId);
            Bl.db.ReplacePayment(new List<Payment>() { pay });            
            var r=Bl.GetReceiptHead(Bl.curReciptId,true);
            */
            var task = Task.Run(() => PrintAndCloseReceipt());
            //var result = task.Result;            
        }

        /// <summary>
        /// Безготівкова оплата і Друк чека.
        /// </summary>
        /// <returns></returns>
        bool PrintAndCloseReceipt()
        {
            decimal sum = ListWares.Sum(r => r.Sum); //Треба переробити
            SetStateView(eStateMainWindows.ProcessPay);
            var pay = EF.PosPurchase(sum);
            if (pay != null)
            {
                pay.SetIdReceipt(Bl.curReciptId);
                Bl.db.ReplacePayment(new List<Payment>() { pay });
            }
            else
                SetStateView(eStateMainWindows.WaitInput);

            SetStateView(eStateMainWindows.ProcessPrintReceipt);
            var R = Bl.GetReceiptHead(Bl.curReciptId, true);
            var res = EF.PrintReceipt(R);
            Bl.InsertLogRRO(res);
            if (res.CodeError == 0)
            {
                Bl.UpdateReceiptFiscalNumber(R, res.FiscalNumber, res.SUM);
                var r = Bl.GetNewIdReceipt();
                Global.OnReceiptCalculationComplete?.Invoke(new List<ReceiptWares>(), Global.GetTerminalIdByIdWorkplace(Global.IdWorkPlace));
                SetStateView(eStateMainWindows.WaitInput);
                return true;
            }
            SetStateView(eStateMainWindows.WaitInput);
            return false;

        }

        /// <summary>
        /// Добавляєм товар(сигарери) з списку цін
        /// </summary>
        /// <param name="sender">Кнопка з ціною</param>
        /// <param name="e"></param>
        private void _AddWaresPrice(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            try
            {
                if (btn != null)
                {
                    TextBlock Tb = btn.Content as TextBlock;
                    if (Tb != null)
                    {
                        decimal Pr = Convert.ToDecimal(Tb.Tag);
                        AddWares(CurWares.CodeWares, CurWares.CodeUnit, 1, Pr);
                    }
                }
            }
            catch { }
            SetStateView(eStateMainWindows.WaitInput);
        }

        private IEnumerable<ReceiptWares> StartData()
        {

            var RId = Bl.GetNewIdReceipt();
            Bl.AddWaresBarCode(RId, "4823086109988", 10);
            //Bl.AddWaresBarCode(RId, "7622300813437", 1);
            Bl.AddWaresBarCode(RId, "2201652300229", 3);
            Bl.AddWaresBarCode(RId, "7775002160043", 1); //товар 2 кат
                                                         //Bl.AddWaresBarCode(RId,"1110011760218", 11);
                                                         //Bl.AddWaresBarCode(RId,"7773002160043", 1); //товар 2 кат
            return Bl.GetWaresReceipt();
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
            //LWeight.re

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
            SetStateView(eStateMainWindows.WaitInput);
            Admin ad = new Admin();
            ad.Show();
            MessageBox.Show("Ви залогінились!!! Но поки це не точно((");
        }

        private void TextLoginChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextPasswordChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void AddExciseStamp(object sender, RoutedEventArgs e)
        {
            //Додання акцизноії марки до алкоголю
            SetStateView(eStateMainWindows.WaitInput);
        }

        public void SetState(eStatusRRO pStatus)
        {

        }
    }


}
