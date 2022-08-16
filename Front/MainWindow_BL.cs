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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StrControlScaleCurrentWeightKg"));
                CS.OnScalesData(pWeight, pIsStable);
            };

            EF.OnWeight += (pWeight, pIsStable) => { Weight = pWeight; };

            EF.SetStatus += (info) =>
            {
                EquipmentInfo = info.TextState;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"SetStatus ({info.ToJSON()})", eTypeLog.Expanded);
                if (EF.StatCriticalEquipment != eStateEquipment.On)
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ErrorEquipment, null);
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
                   // if (curReceipt?.Wares?.Count() == 0 && curReceipt.OwnBag==0d) CS.WaitClear();

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
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.StartFullUpdate);
                //Помилка оновлення.
                if (SyncInfo.Status == eSyncStatus.Error)
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ErrorFullUpdate);

                if (TypeAccessWait == eTypeAccess.StartFullUpdate && SyncInfo.Status == eSyncStatus.SyncFinishedSuccess)
                {
                    TypeAccessWait = eTypeAccess.NoDefine;
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
                if (TypeAccessWait > 0 )//TypeAccessWait != eTypeAccess.NoDefinition 
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
                //StateScale = pStateScale;

                switch (pStateScale)
                {
                    case eStateScale.WaitGoods:
                        // if (State != eStateMainWindows.BlockWeight)
                        IsShowWeightWindows=false;
                        SetStateView(eStateMainWindows.BlockWeight);
                        break;
                    case eStateScale.BadWeight:
                    case eStateScale.NotStabilized:
                    case eStateScale.WaitClear:
                        IsShowWeightWindows = true;
                        SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.FixWeight, pRW);
                        break;
                    case eStateScale.Stabilized:
                        if (State == eStateMainWindows.WaitWeight || State == eStateMainWindows.BlockWeight || State == eStateMainWindows.WaitAdmin)
                            SetStateView(eStateMainWindows.WaitInput);
                        if (pRW != null)
                            Bl.FixWeight(pRW);
                        break;
                }
                SetPropertyChanged();                
            };
        }

        bool SetConfirm(User pUser, bool pIsFirst = false, bool pIsAccess = false)
        {
            if (pUser == null)
            {
                switch (TypeAccessWait)
                {  
                    case eTypeAccess.DelReciept:
                    case eTypeAccess.DelWares:
                        TypeAccessWait = eTypeAccess.NoDefine;
                        SetStateView(eStateMainWindows.WaitInput);
                        break;
                    default:
                        SetStateView(eStateMainWindows.WaitAdmin);
                        break;
                }
                return true;
            }

            //!!!TMP НЕ зовсім правильно треба провірити права перед розблокуванням кнопок
            if (pUser.TypeUser >= eTypeUser.AdminSSC && customWindow?.Buttons != null)
                foreach (var item in customWindow.Buttons)
                    item.IsAdmin = false;

            IsIgnoreExciseStamp = Access.GetRight(pUser, eTypeAccess.ExciseStamp);
            IsAddNewWeight = Access.GetRight(pUser, eTypeAccess.AddNewWeight);
            IsFixWeight = Access.GetRight(pUser, eTypeAccess.FixWeight);

            if (TypeAccessWait == eTypeAccess.NoDefine || TypeAccessWait < 0)
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
                        CurWares = null;//.Quantity = 0;
                        TypeAccessWait = eTypeAccess.NoDefine;
                        SetStateView(eStateMainWindows.WaitInput);
                    }
                    break;
                case eTypeAccess.DelReciept:
                    Bl.SetStateReceipt(curReceipt, eStateReceipt.Canceled);
                    curReceipt= Bl.GetNewIdReceipt();
                    TypeAccessWait = eTypeAccess.NoDefine;
                    SetStateView(eStateMainWindows.StartWindow);
                    break;
                case eTypeAccess.ConfirmAge:
                    Bl.AddEventAge(curReceipt);
                    TypeAccessWait = eTypeAccess.NoDefine;
                    PrintAndCloseReceipt();

                    break;
                case eTypeAccess.ChoicePrice:
                    foreach (Models.Price el in Prices.ItemsSource)
                        el.IsEnable = true;
                    TypeAccessWait = eTypeAccess.NoDefine;
                    break;
                case eTypeAccess.AddNewWeight:
                case eTypeAccess.FixWeight:
                    //SetStateView(eStateMainWindows.WaitAdmin);
                    break;
                case eTypeAccess.ExciseStamp:
                    TypeAccessWait = eTypeAccess.NoDefine;
                    SetStateView(eStateMainWindows.WaitAdmin);
                    break;
                case eTypeAccess.AdminPanel:
                    TypeAccessWait = eTypeAccess.NoDefine;
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
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"(pBarCode=>{pBarCode},  pTypeBarCode=>{pTypeBarCode}");
            if (State == eStateMainWindows.StartWindow)
                SetStateView(eStateMainWindows.WaitInput);

            if (TypeAccessWait == eTypeAccess.ExciseStamp)
            {
                string ExciseStamp = GetExciseStamp(pBarCode);
                if (!string.IsNullOrEmpty(ExciseStamp))
                {
                    AddExciseStamp(ExciseStamp);
                    return;
                }
            }
            else
            {
                ReceiptWares w = null;
                if (State == eStateMainWindows.WaitInput || State == eStateMainWindows.StartWindow)
                {
                    if (curReceipt == null || !curReceipt.IsLockChange)
                    {
                        if (curReceipt == null)
                            curReceipt = Bl.GetNewIdReceipt();
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
                if (c != null) return;
            }
           
                var u = Bl.GetUserByBarCode(pBarCode);
                if (u != null)
                    Bl.OnAdminBarCode?.Invoke(u);
            
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
                if (curReceipt == null)
                    curReceipt= Bl.GetNewIdReceipt();
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
                SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ConfirmAge);
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
                    if (pay.IsSuccess)
                    {
                        Bl.SetStateReceipt(curReceipt, eStateReceipt.Pay);
                        R.StateReceipt = eStateReceipt.Pay;
                        R.Payment = new List<Payment>() { pay };
                    }
                }
                if(pay == null || !pay.IsSuccess)
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
                        curReceipt = Bl.GetNewIdReceipt();
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
 
        private IEnumerable<ReceiptWares> StartData()
        {
            curReceipt = Bl.GetNewIdReceipt();
            //Bl.AddWaresBarCode(RId, "27833", 258m);
            //Bl.AddWaresBarCode(RId, "7622201819590", 1);
            Bl.GetClientByPhone(curReceipt, "0503399110");
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
                TypeAccessWait = eTypeAccess.NoDefine;
                SetStateView(eStateMainWindows.WaitInput);
            }
            else
                ShowErrorMessage($"Дана акцизна марка вже використана");
        }
   
    }
}
