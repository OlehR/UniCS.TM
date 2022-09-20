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
using Front.Equipments.Virtual;
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
            EF.OnControlWeight += (pWeight, pIsStable) =>
            {
                ControlScaleCurrentWeight = pWeight;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ControlScaleCurrentWeight"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StrControlScaleCurrentWeightKg"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsOwnBag"));

                CS.OnScalesData(pWeight, pIsStable);
            };

            EF.OnWeight += (pWeight, pIsStable) => { Weight = pWeight / 1000; };

            EF.SetStatus += (info) =>
            {
                var r = Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    PosStatus PS = info as PosStatus;
                    //if (info.TypeEquipment == eTypeEquipment.BankTerminal || info.TypeEquipment == eTypeEquipment.RRO)
                    //{
                    //info.Status
                    //}
                    //EquipmentStatusInPayment.Text = PS.Status.GetDescription(); //TMP - не працює через гетер
                    if (PS != null)
                        EquipmentStatusInPayment.Text = PS.Status.GetDescription();
                    //EquipmentInfo = PS.Status.GetDescription();
                    else
                    {
                        RroStatus rroStatus = info as RroStatus;
                        if (rroStatus != null)
                            EquipmentStatusInPayment.Text = rroStatus.Status.GetDescription();//TMP - не працює через гетер
                                                                                              //EquipmentInfo = rroStatus.Status.GetDescription();
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EquipmentInfo"));
                }));
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
                        }

                        if (pReceipt.GetLastWares != null)
                        {
                            EF.ProgramingArticleAsync(new List<ReceiptWares>() { pReceipt.GetLastWares });
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

                var r = Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    NumericPad.Visibility = Visibility.Collapsed;
                    Background.Visibility = Visibility.Collapsed;
                    BackgroundWares.Visibility = Visibility.Collapsed;
                    if (Client?.Wallet != 0 || Client?.SumMoneyBonus != 0 || Client?.SumBonus != 0)
                    {
                        ShowClientBonus.Visibility = Visibility.Visible;
                    }
                }
                ));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClientName"));
                if (Client.BirthDay > new DateTime(1990, 1, 1))
                    if (Client.BirthDay.AddYears(18).Date <= DateTime.Now.Date)
                        Bl.AddEventAge(curReceipt);

                FileLogger.WriteLogMessage($"MainWindow.OnClientChanged(Client.Wallet=> {pClient.Wallet} SumBonus=>{pClient.SumBonus})", eTypeLog.Full);
            };

            Bl.OnAdminBarCode += (pUser) =>
            {
                if (TypeAccessWait > 0)//TypeAccessWait != eTypeAccess.NoDefinition 
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

            Bl.OnCustomWindow += (pCW) =>
            {
                SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, pCW);
            };

            //Обробка стану контрольної ваги.
            CS.OnStateScale += (pStateScale, pRW, pСurrentlyWeight) =>
            {
                //Якщо повернення ігноруємо вагу.
                if (curReceipt?.TypeReceipt == eTypeReceipt.Refund && pStateScale != eStateScale.Stabilized)
                    return;
                switch (pStateScale)
                {
                    case eStateScale.WaitGoods:
                        // if (State != eStateMainWindows.BlockWeight)
                        IsShowWeightWindows = false;
                        SetStateView(eStateMainWindows.BlockWeight);
                        break;

                    case eStateScale.NotStabilized:
                        IsShowWeightWindows = true;
                        if ((State == eStateMainWindows.WaitAdmin && (TypeAccessWait == eTypeAccess.DelReciept || TypeAccessWait == eTypeAccess.DelWares)))
                            break;
                        SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.FixWeight, pRW, null, eSender.ControlScale);
                        break;
                    case eStateScale.BadWeight:
                    case eStateScale.WaitClear:
                        IsShowWeightWindows = true;
                        SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.FixWeight, pRW, null, eSender.ControlScale);
                        break;
                    case eStateScale.Stabilized:
                        if (State == eStateMainWindows.WaitWeight || State == eStateMainWindows.BlockWeight || State == eStateMainWindows.WaitAdmin ||
                            State == eStateMainWindows.ProcessPrintReceipt || State == eStateMainWindows.ProcessPay)
                            SetStateView(eStateMainWindows.WaitInput, eTypeAccess.NoDefine, null, null, eSender.ControlScale);
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
            //if (pUser.TypeUser >= eTypeUser.AdminSSC && customWindow?.Buttons != null)
            // foreach (var item in customWindow.Buttons)
            //   item.IsNeedAdmin = false;

            //IsIgnoreExciseStamp = Access.GetRight(pUser, eTypeAccess.ExciseStamp);
            if (TypeAccessWait == eTypeAccess.FixWeight)
                IsConfirmAdmin = Access.GetRight(pUser, eTypeAccess.FixWeight);
            else
              if (TypeAccessWait == eTypeAccess.ExciseStamp)
                IsConfirmAdmin = Access.GetRight(pUser, eTypeAccess.ExciseStamp);



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

                    //NewReceipt(); //!!!TMP Трохи через Ж Пізніше зроблю краще.
                    SetCurReceipt(null);

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
                    {
                        el.IsEnable = true;
                        el.IsConfirmAge = true;
                    }
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
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"(pBarCode=>{pBarCode},  pTypeBarCode=>{pTypeBarCode})");
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
                            NewReceipt();
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
                //SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.AdminPanel);
            }

            var u = Bl.GetUserByBarCode(pBarCode);
            if (u != null)
            { Bl.OnAdminBarCode?.Invoke(u); return; }
            if ((State != eStateMainWindows.WaitInput && State != eStateMainWindows.StartWindow) || curReceipt?.IsLockChange == true)
                if (State != eStateMainWindows.ProcessPay && State != eStateMainWindows.ProcessPrintReceipt)
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.AdminPanel);

        }

        public void AddWares(int pCodeWares, int pCodeUnit = 0, decimal pQuantity = 0m, decimal pPrice = 0m, GW pGV = null)
        {
            if (pGV != null)
            {
                CurW = pGV;
                NameWares.Text = CurW.Name;

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
                    NewReceipt();
                CurWares = Bl.AddWaresCode(curReceipt, pCodeWares, pCodeUnit, pQuantity, pPrice);

                if (CurWares != null)
                {
                    IsPrises(pQuantity, pPrice);
                }
            }
        }

        object LockPayPrint = new object();
        /// <summary>
        /// Безготівкова оплата і Друк чека.
        /// </summary>
        /// <returns></returns>
        public bool PrintAndCloseReceipt(Receipt pR = null)
        {

            var R = Bl.GetReceiptHead(pR ?? curReceipt, true);
            curReceipt = R;
            R.NameCashier = AdminSSC?.NameUser;
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"{curReceipt.ToJSON()}", eTypeLog.Expanded);

            if (R.AgeRestrict > 0 && R.IsConfirmAgeRestrict == false)
            {
                SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ConfirmAge);
                return true;
            }


            lock (LockPayPrint)
            {

                if (R.StateReceipt == eStateReceipt.Prepare)
                {

                    try
                    {
                        if (R.TypeReceipt == eTypeReceipt.Sale)
                            Bl.GenQRAsync(R.Wares);
                        R.StateReceipt = eStateReceipt.StartPay;
                        Bl.SetStateReceipt(curReceipt, eStateReceipt.StartPay);
                        decimal sum = R.Wares.Sum(r => (r.SumTotal)); //TMP!!!Треба переробити
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Sum={sum}", eTypeLog.Expanded);
                        SetStateView(eStateMainWindows.ProcessPay);
                        var pay = R.TypeReceipt == eTypeReceipt.Sale ? EF.PosPurchase(R, sum) : EF.PosRefund(R, sum, R.AdditionC1);

                        if (pay != null && pay.IsSuccess)
                        {
                            R.StateReceipt = eStateReceipt.Pay;
                            R.CodeCreditCard = pay.NumberCard;
                            R.NumberReceiptPOS = pay.NumberReceipt;
                            //R.Client = null;
                            R.SumCreditCard = pay.SumPay;
                            Bl.db.ReplaceReceipt(R);
                            R.Payment = new List<Payment>() { pay };
                        }
                        else
                        {
                            SetStateView(eStateMainWindows.WaitInput);
                            R.StateReceipt = eStateReceipt.Prepare;
                            Bl.SetStateReceipt(curReceipt, eStateReceipt.Prepare);
                        }
                    }
                    catch (Exception e)
                    {
                        R.StateReceipt = eStateReceipt.Prepare;
                        Bl.SetStateReceipt(curReceipt, eStateReceipt.Prepare);
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    }
                }

                if (R.StateReceipt == eStateReceipt.Pay || R.StateReceipt == eStateReceipt.StartPrint)
                {
                    R.Client = Client;
                    R.StateReceipt = eStateReceipt.StartPrint;
                    Bl.SetStateReceipt(curReceipt, eStateReceipt.StartPrint);
                    try
                    {
                        SetStateView(eStateMainWindows.ProcessPrintReceipt);
                        //Bl.SetStateReceipt(curReceipt, eStateReceipt.Canceled);
                        var res = EF.PrintReceipt(R);
                        SetStateView(eStateMainWindows.WaitInput);
                        if (res.CodeError == 0)
                        {
                            R.StateReceipt = eStateReceipt.Print;
                            Bl.UpdateReceiptFiscalNumber(R, res.FiscalNumber, res.SUM);
                            s.Play(eTypeSound.DoNotForgetProducts);

                            if (R.TypeReceipt == eTypeReceipt.Sale)
                            {
                                var QR = Bl.GetQR(R);
                                if (QR != null && QR.Count() > 0)
                                {
                                    foreach (var el in QR)
                                    {
                                        foreach (string elQr in el.Qr.Split(","))
                                        {
                                            List<string> list = new List<string>() { el.Name, $"QR=>{elQr}" };
                                            EF.PrintNoFiscalReceipt(list);
                                        }
                                    }
                                }
                            }
                            Bl.ds.SendReceiptTo1C(curReceipt);
                            SetCurReceipt(null);
                            //NewReceipt();
                            //Global.OnReceiptCalculationComplete?.Invoke(new List<ReceiptWares>(), Global.IdWorkPlace);

                            return true;
                        }
                        else
                        {
                            R.StateReceipt = eStateReceipt.Pay;
                            Bl.SetStateReceipt(curReceipt, eStateReceipt.Pay);
                            ShowErrorMessage("Помилка друку чеків" + res.Error);
                            //MessageBox.Show(res.Error, "Помилка друку чеків");
                            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Помилка друку чеків" + res.Error, eTypeLog.Error);
                        }
                        SetStateView(eStateMainWindows.WaitInput);
                    }
                    catch (Exception e)
                    {
                        R.StateReceipt = eStateReceipt.Pay;
                        Bl.SetStateReceipt(curReceipt, eStateReceipt.Pay);
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    }
                }
                return false;
            }
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

        void NewReceipt()
        {
            curReceipt = Bl.GetNewIdReceipt();
            s.NewReceipt(curReceipt.CodeReceipt);
            Dispatcher.BeginInvoke(new ThreadStart(() => { ShowClientBonus.Visibility = Visibility.Collapsed; }));
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"{curReceipt.ToJSON()}");
        }

    }
}
