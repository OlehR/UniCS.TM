using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Front.Equipments;
using Front.Models;
using ModelMID;
using ModelMID.DB;
using SharedLib;
using Utils;

namespace Front
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public void InitAction()
        {
            //return;
            EF.OnControlWeight += (pWeight, pIsStable) =>
            {
                ControlScaleCurrentWeight = pWeight;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ControlScaleCurrentWeight"));
                CS.OnScalesData(pWeight, pIsStable);
            };

            EF.OnWeight += (pWeight, pIsStable) => { Weight = pWeight; };

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
                    bool IsDel = false;
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
                            IsDel = true;
                            /*
                            CurWares.Quantity = 0;

                            foreach (var e in curReceipt.Wares.Where(el => el.IsLast = true))
                            {
                                e.IsLast = false;
                            }
                            var r = curReceipt.Wares.ToList();
                            CurWares.IsLast = true;
                            r.Add(CurWares);
                            curReceipt.Wares = r;
                            */
                        }
                    }
                    if (curReceipt?.Wares?.Count() == 0)
                        CS.WaitClear();

                    CS.StartWeightNewGoogs(curReceipt, IsDel ? CurWares : null);
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

            Bl.OnAdminBarCode += (pUser) =>
            {
                if (TypeAccessWait > 0 && !(TypeAccessWait == eTypeAccess.FixWeight && CS.StateScale == eStateScale.WaitClear))//TypeAccessWait != eTypeAccess.NoDefinition 
                {
                    SetConfirm(pUser);
                    return;
                }

                if (Access.GetRight(pUser, eTypeAccess.AdminPanel))
                {
                    SetStateView(eStateMainWindows.WaitInput);
                    //Admin ad = new Admin(U, this,EF);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ad.Init(pUser);
                        ad.WindowState = WindowState.Maximized;
                    }));
                }
                else
                {
                    ShowErrorMessage($"Не достатньо прав на вхід в адмін панель для  {pUser.NameUser}");
                    //                MessageBox.Show($"Не достатньо прав на вхід в адмін панель для  {U.NameUser}");
                }

            };

            Bl.OnCustomWindow += (pCW) => { customWindow = pCW; SetStateView(eStateMainWindows.WaitCustomWindows); };

            //Обробка стану контрольної ваги.
            CS.OnStateScale += (pStateScale, pRW, pСurrentlyWeight) =>
            {
                StateScale = pStateScale;

                switch (StateScale)
                {
                    case eStateScale.BadWeight:
                    case eStateScale.NotStabilized:
                    case eStateScale.WaitClear:
                    case eStateScale.WaitGoods:
                        if (State != eStateMainWindows.BlockWeight)
                            SetWaitConfirm(eTypeAccess.FixWeight, pRW); // SetStateView(eStateMainWindows.WaitWeight)

                        break;
                    case eStateScale.Stabilized:
                        if (State == eStateMainWindows.WaitWeight || State == eStateMainWindows.WaitAdmin)
                            SetStateView(eStateMainWindows.WaitInput);
                        if (pRW != null)
                            Bl.FixWeight(pRW);
                        break;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WaitAdminText"));
            };
}

        bool SetConfirm(User pUser, bool pIsFirst = false, bool pIsAccess = false)
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
                return true;
            }

            IsIgnoreExciseStamp = Access.GetRight(pUser, eTypeAccess.ExciseStamp);
            IsAddNewWeight = Access.GetRight(pUser, eTypeAccess.AddNewWeight);
            IsFixWeight = Access.GetRight(pUser, eTypeAccess.FixWeight);

            if (TypeAccessWait == eTypeAccess.NoDefinition || TypeAccessWait < 0)
                return false;
            if (!Access.GetRight(pUser, TypeAccessWait) && !pIsAccess)
            {
                if (!pIsFirst)
                    ShowErrorMessage($"Не достатньо прав для операції {TypeAccessWait} в {pUser.NameUser}");
                //                MessageBox.Show($"Не достатньо прав для операції {TypeAccessWait} в {pUser.NameUser}");
                return false;
            }

            switch (TypeAccessWait)
            {
                case eTypeAccess.DelWares:
                    if (curReceipt?.IsLockChange == false)
                    {
                        Bl.ChangeQuantity(CurWares, 0);
                        CurWares.Quantity = 0;
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
                case eTypeAccess.ExciseStamp:
                    TypeAccessWait = eTypeAccess.NoDefinition;
                    SetStateView(eStateMainWindows.WaitAdmin);
                    break;
                case eTypeAccess.AdminPanel:
                    TypeAccessWait = eTypeAccess.NoDefinition;
                    SetStateView(eStateMainWindows.WaitInput);
                    //Admin ad = new Admin(U, this,EF);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ad.Init(pUser);
                        ad.WindowState = WindowState.Maximized;
                    }));
                    break;
            }
            return true;
        }

        public void GetBarCode(string pBarCode, string pTypeBarCode)
        {
            if (State == eStateMainWindows.StartWindow)
                SetStateView(eStateMainWindows.WaitInput);

            if (TypeAccessWait == eTypeAccess.ExciseStamp)
            {
                string ExciseStamp = GetExciseStamp(pBarCode);
                if (!string.IsNullOrEmpty(ExciseStamp))
                    AddExciseStamp(ExciseStamp);
                return;
            }
            ReceiptWares w = null;
            if (State == eStateMainWindows.WaitInput || State == eStateMainWindows.StartWindow)
            {
                if (curReceipt?.IsLockChange == false)
                {
                    w = Bl.AddWaresBarCode(curReceipt, pBarCode, 1);
                    if (w != null)
                    {
                        CurWares = w;
                        IsPrises(1, 0);
                    }
                }
            }
            else
            {
                w = Bl.AddWaresBarCode(curReceipt, pBarCode, 1, true);
            }
            if (w != null)
                return;
            var c = Bl.GetClientByBarCode(curReceipt, pBarCode);
            if (c == null)
            {
                var u = Bl.GetUserByBarCode(pBarCode);
                if (u != null)
                    Bl.OnAdminBarCode?.Invoke(u);
            }
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
                    IsPrises(pQuantity, pPrice);
                }
            }
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
 
        CustomWindow GetCustomButton(eStateScale pST)
        {
            CustomWindow res = new CustomWindow() { Id = eWindows.ConfirmWeight, Buttons = customWindowButtons };
            if (TypeAccessWait == eTypeAccess.FixWeight)
                switch (StateScale)
                {
                    case eStateScale.WaitClear:
                        res.Buttons = new ObservableCollection<CustomButton>()
                    { new CustomButton() { Id = 4, Text = "Тарувати", IsAdmin = true } ,
                      new CustomButton() { Id = 5, Text = "Вхід в адмінку", IsAdmin = true } };
                        break;
                    case eStateScale.BadWeight:
                        res.Buttons = new ObservableCollection<CustomButton>()
                    { new CustomButton() { Id = 1, Text = "Підтвердити вагу", IsAdmin = true },
                      new CustomButton() { Id = 2, Text = "Добавити вагу", IsAdmin = true } ,
                      new CustomButton() { Id = -1, Text = "Закрити", IsAdmin = false }};
                        break;

                    case eStateScale.WaitGoods:
                        res.Buttons = new ObservableCollection<CustomButton>()
                    { new CustomButton() { Id = 1, Text = "Підтвердити вагу", IsAdmin = true },
                      new CustomButton() { Id = 3, Text = "Видалити товар", IsAdmin = true } };
                        break;
                    case eStateScale.NotStabilized:
                        res.Buttons = new ObservableCollection<CustomButton>()
                    { new CustomButton() { Id = 1, Text = "Підтвердити вагу", IsAdmin = true },
                      new CustomButton() { Id = -1, Text = "Закрити", IsAdmin = false }};
                        break;
                }
            return res;
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

        void AddExciseStamp(string pES)
        {
            if (CurWares.AddExciseStamp(pES))
            {                 //Додання акцизноії марки до алкоголю
                Bl.UpdateExciseStamp(new List<ReceiptWares>() { CurWares });
                TypeAccessWait = eTypeAccess.NoDefinition;
                SetStateView(eStateMainWindows.WaitInput);
            }
            else
                ShowErrorMessage($"Дана акцизна марка вже використана");
        }

    }
}
