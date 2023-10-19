using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Front.Control;
using Front.Equipments;
using Front.Equipments.Virtual;
using Front.Models;
using ModelMID;
using ModelMID.DB;
using Newtonsoft.Json;
using SharedLib;
using Utils;

namespace Front
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        int LastCodeWares = 0;
        public void InitAction()
        {
            #region Action
            EF.OnControlWeight += (pWeight, pIsStable) =>
            {
                ControlScaleCurrentWeight = pWeight;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ControlScaleCurrentWeight"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StrControlScaleCurrentWeightKg"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsOwnBag"));

                CS.OnScalesData(pWeight, pIsStable);
            };

            EF.OnWeight += (pWeight, pIsStable) =>
            {
                Weight = pWeight / 1000;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsWeightMagellan"));
            };

            EF.SetStatus += (info) =>
            {
                if(info.IsСritical==true)
                {
                    LastErrorEquipment= info.TextState;
                    FileLogger.WriteLogMessage(this, $"EF.SetStatus {info.ModelEquipment}", $"{info.TextState}", eTypeLog.Error);
                    SetStateView(eStateMainWindows.WaitAdmin,eTypeAccess.ErrorEquipment);
                    return;
                }
                var r = Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    PosStatus PS = info as PosStatus;

                    if (PS != null)
                        EquipmentInfo = PS.Status.GetDescription();
                    else
                    {
                        RroStatus rroStatus = info as RroStatus;
                        if (rroStatus != null)
                            EquipmentInfo = rroStatus.Status.GetDescription();
                    }
                    if (EquipmentInfo != null)
                        EquipmentStatusInPayment.Text = EquipmentInfo; //TMP - не працює через гетер
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EquipmentInfo)));
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

                        if (pReceipt.GetLastWares != null && LastCodeWares!= pReceipt.GetLastWares.CodeWares)
                        {
                            LastCodeWares = pReceipt.GetLastWares.CodeWares;
                            ReceiptWares cl = (ReceiptWares)pReceipt.GetLastWares.Clone();
                            EF.ProgramingArticleAsync( cl);
                        }
                        else
                         EF.PutToDisplay(pReceipt,$"{CurWares.NameWaresReceipt}{Environment.NewLine}{CurWares.Quantity}x{CurWares.Price}={CurWares.SumTotal}", 0);
                    }
                    // if (curReceipt?.Wares?.Count() == 0 && curReceipt.OwnBag==0d) CS.WaitClear();

                    CS.StartWeightNewGoogs(curReceipt, IsDel ? CurWares : null);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage($"MainWindow.OnReceiptCalculationComplete Exception =>(pReceipt=>{pReceipt.ToJSON()}) => ({Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace})", eTypeLog.Error);
                }

                FileLogger.WriteLogMessage($"MainWindow.OnReceiptCalculationComplete(pReceipt=>{pReceipt.ToJSON()})", eTypeLog.Full);
            };

            Global.OnSyncInfoCollected += (SyncInfo) =>
            {
                DatabaseUpdateStatus = SyncInfo.Status;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DatabaseUpdateStatus)));
                //Почалось повне оновлення.
                if (SyncInfo.Status == eSyncStatus.StartedFullSync && !Bl.ds.IsUseOldDB)
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.StartFullUpdate);
                //Помилка оновлення.
                if (SyncInfo.Status == eSyncStatus.Error && !Bl.ds.IsUseOldDB)
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ErrorFullUpdate);
                
                if (SyncInfo.Status == eSyncStatus.ErrorDB)
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ErrorDB);

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
                    if (Client != null)
                        ShowClientBonus.Visibility = Visibility.Visible;
                }
                ));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClientName"));
                if (Client.BirthDay > new DateTime(1900, 1, 1))
                    if (Client.BirthDay.AddYears(18).Date <= DateTime.Now.Date)
                        Bl.AddEventAge(curReceipt);

                FileLogger.WriteLogMessage($"MainWindow.OnClientChanged(CodeReceipt=>{curReceipt?.CodeReceipt} Client.CodeClient=>{Client.CodeClient} Client.Wallet=> {pClient.Wallet} SumBonus=>{pClient.SumBonus})", eTypeLog.Full);
            };

            Bl.OnAdminBarCode += (pUser) =>
            {
                if (TypeAccessWait > 0)//TypeAccessWait != eTypeAccess.NoDefinition 
                {
                    SetConfirm(pUser);
                    return;
                }

                if (Access.GetRight(pUser, eTypeAccess.AdminPanel))
                    ShowAdmin(pUser);
                else
                    CustomMessage.Show($"Не достатньо прав на вхід в адмін панель для  {pUser.NameUser}", "Увага", eTypeMessage.Error);
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
                            State == eStateMainWindows.ProcessPrintReceipt || State == eStateMainWindows.ProcessPay || State == eStateMainWindows.StartWindow)
                            SetStateView(eStateMainWindows.WaitInput, eTypeAccess.NoDefine, null, null, eSender.ControlScale);
                        if (pRW != null)
                            Bl.FixWeight(pRW);
                        break;
                }
                SetPropertyChanged();
            };
        }
        #endregion

        public bool SetConfirm(User pUser, bool pIsFirst = false, bool pIsAccess = false)
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
            {
                IsConfirmAdmin = Access.GetRight(pUser, eTypeAccess.FixWeight);
                if (State == eStateMainWindows.BlockWeight)
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.FixWeight);
            }
            else
              if (TypeAccessWait == eTypeAccess.ExciseStamp)
                IsConfirmAdmin = Access.GetRight(pUser, eTypeAccess.ExciseStamp);

            if (TypeAccessWait == eTypeAccess.NoDefine || TypeAccessWait < 0)
                return false;
            if (!Access.GetRight(pUser, TypeAccessWait) && !pIsAccess)
            {
                if (!pIsFirst)
                    CustomMessage.Show($"Не достатньо прав для операції {TypeAccessWait} {Environment.NewLine}в {pUser.NameUser} з правами {pUser.TypeUser}", "Увага", eTypeMessage.Error);
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
                    SetCurReceipt(null);
                    TypeAccessWait = eTypeAccess.NoDefine;
                    SetStateView(eStateMainWindows.StartWindow);
                    break;
                case eTypeAccess.ReturnReceipt:                    
                    Bl.CreateRefund(AdminControl.curReceipt,IsFullReturn);
                    SetStateView(eStateMainWindows.WaitInputRefund);
                    break;

                case eTypeAccess.ConfirmAge:
                    Bl.AddEventAge(curReceipt);
                    TypeAccessWait = eTypeAccess.NoDefine;
                    PayAndPrint();
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
                    ShowAdmin(pUser);
                    break;
                case eTypeAccess.UseBonus:
                    var task = Task.Run(() => PrintAndCloseReceipt(null, eTypePay.Bonus, 0, 0, 0, Client?.SumMoneyBonus ?? 0m));
                    break;
            }
            return true;
        }

        void ShowAdmin(User pUser)
        {
            AdminControl.Init(pUser);
            SetStateView(eStateMainWindows.AdminPanel);            
        }

        public void GetBarCode(string pBarCode, string pTypeBarCode)
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"(pBarCode=>{pBarCode},  pTypeBarCode=>{pTypeBarCode})");
            if (State == eStateMainWindows.StartWindow)
                SetStateView(eStateMainWindows.WaitInput);
            if (State == eStateMainWindows.WaitInputIssueCard)
            {
                BarcodeIssueCard = pBarCode;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BarcodeIssueCard"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsBarcodeIssueCard"));
                
                return;
            }
            var u = Bl.GetUserByBarCode(pBarCode);
            if (u != null)
            { Bl.OnAdminBarCode?.Invoke(u); return; } 

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
                if (IsAddNewWares && (State == eStateMainWindows.WaitInput || State == eStateMainWindows.StartWindow))
                {
                    if (curReceipt == null || !curReceipt.IsLockChange)
                    {
                        if (curReceipt == null)
                            NewReceipt();
                        w = Bl.AddWaresBarCode(curReceipt, pBarCode, 1);
                        if (w != null && w.CodeWares > 0)
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

                if (curReceipt != null)
                {
                    var c = Bl.GetClientByBarCode(curReceipt, pBarCode.ToLower());
                    if (c != null) return;
                }
            }
           
            if ((State != eStateMainWindows.WaitInput && State != eStateMainWindows.StartWindow) || curReceipt?.IsLockChange == true || !IsAddNewWares)
                if (State != eStateMainWindows.ProcessPay && State != eStateMainWindows.ProcessPrintReceipt && State != eStateMainWindows.WaitCustomWindows)
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

        public void PayAndPrint()
        {
            if (curReceipt.StateReceipt < eStateReceipt.Pay && curReceipt.CountWeightGoods > 0 && !curReceipt.Wares.Any(x => x.CodeWares == Global.Settings.CodePackagesBag) && !curReceipt.IsPakagesAded && curReceipt.TypeReceipt == eTypeReceipt.Sale)
            {
                AddMissingPackage.CountPackeges = curReceipt.CountWeightGoods;
                AddMissingPackage.CallBackResult = (int res) =>
                {
                    Bl.AddEvent(curReceipt, eReceiptEventType.PackagesBag, res != 0 ? "Додавання пакетів в чек" : "Відміна додавання пакетів");
                    Bl.AddWaresCode(curReceipt, Global.Settings.CodePackagesBag, 19, res);
                    AddMissingPackage.Visibility = Visibility.Collapsed;
                    Background.Visibility = Visibility.Collapsed;
                    BackgroundWares.Visibility = Visibility.Collapsed;
                    Thread.Sleep(200);

                    PayAndPrint();
                };
                AddMissingPackage.Visibility = Visibility.Visible;
                Background.Visibility = Visibility.Visible;
                BackgroundWares.Visibility = Visibility.Visible;
                return;
            }

            if (curReceipt.StateReceipt < eStateReceipt.Pay &&  curReceipt.AgeRestrict > 0 && curReceipt.IsConfirmAgeRestrict == false)
            {
                SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ConfirmAge);
                return;
            }


            Dispatcher.BeginInvoke(new ThreadStart(() =>
            { EquipmentStatusInPayment.Text = ""; }));
            if (Global.TypeWorkplaceCurrent == eTypeWorkplace.CashRegister && (curReceipt.StateReceipt==eStateReceipt.Prepare || curReceipt.StateReceipt==eStateReceipt.StartPay))
            {
                PaymentWindow.UpdatePaymentWindow();
                SetStateView(eStateMainWindows.ChoicePaymentMethod);
            }
            else
            {
                var task = Task.Run(() => PrintAndCloseReceipt());
            }
        }

        object LockPayPrint = new object();
        /// <summary>
        /// Оплата і Друк чека.
        /// </summary>
        /// <returns></returns>
        public bool PrintAndCloseReceipt(Receipt pR = null, eTypePay pTP = eTypePay.Card, decimal pSumCash=0m, decimal pIssuingCash=0, decimal pSumWallet=0,decimal pSumBonus =0)
        {
            bool Res = false;
            string TextError = null;

            var R = Bl.GetReceiptHead(pR ?? curReceipt, true);
            curReceipt = R;
            R.NameCashier = AdminSSC?.NameUser;
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"pTP=>{pTP} pSumCash=>{pSumCash} pIssuingCash=>{pIssuingCash} pSumWallet=>{pSumWallet} pSumBonus=>{pSumBonus} curReceipt=> {curReceipt.ToJSON()}", eTypeLog.Expanded);

            int[] IdWorkplacePays = R.IdWorkplacePays;// Wares.Select(el => el.IdWorkplacePay).Distinct().OrderBy(el => el).ToArray();
            IsManyPayments = IdWorkplacePays.Length > 1;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsManyPayments"));
            FillPays(R);
            AmountManyPayments = "";
            foreach (var item in R.WorkplacePays)
                AmountManyPayments += $"{item.Sum} | ";
            AmountManyPayments = AmountManyPayments.Substring(0, AmountManyPayments.Length - 2);
            SumTotalManyPayments = $"Загальна сума: {R.SumTotal}₴";
            lock (LockPayPrint)
            {
                R.StateReceipt = Bl.GetStateReceipt(R);
                if (R.StateReceipt == eStateReceipt.Prepare || R.StateReceipt == eStateReceipt.PartialPay)
                {
                    try
                    {
                        if (R.TypeReceipt == eTypeReceipt.Sale)
                            Bl.GenQRAsync(R.Wares);
                        //var Pays = new List<Payment>();

                        IEnumerable<Payment> PayRefaund =( R.TypeReceipt== eTypeReceipt.Refund? Bl.GetPayment(R.RefundId):null);
                        string rrn = R.AdditionC1;
                        if (pSumWallet != 0 || pSumBonus!=0)
                        {
                            Bl.db.DelPayWalletBonus(R);
                            if(pSumWallet != 0)
                              Bl.db.ReplacePayment(  new Payment(R) {IdWorkplacePay=R.IdWorkplace, IsSuccess = true, TypePay = eTypePay.Wallet, SumPay = pSumWallet, SumExt = pSumWallet } );
                            if (pSumBonus != 0)
                                Bl.db.ReplacePayment(new Payment(R) { IdWorkplacePay = R.IdWorkplace, IsSuccess = true, TypePay = eTypePay.Bonus, SumPay = R.SumTotal, SumExt = pSumBonus, PosAddAmount=R.Client?.PercentBonus??Client?.PercentBonus??0m });
                            R.Payment = Bl.GetPayment(R);
                            if ((pSumWallet > 0 || pSumBonus>0) && R.ReCalc())
                            {
                                foreach (var el in R.Wares.Where(el => el.TypeWares == eTypeWares.Ordinary))
                                    Bl.db.ReplaceWaresReceipt(el);
                                
                                if (pSumBonus > 0)
                                {                                    
                                    Bl.db.ReplacePayment(new Payment(R) { IdWorkplacePay = R.IdWorkplace, IsSuccess = true, TypePay = eTypePay.Cash, SumPay = Math.Round(R.SumTotal, 1), SumExt = Math.Round(R.SumTotal, 1) });
                                    R.StateReceipt = eStateReceipt.Pay;
                                    Bl.SetStateReceipt(R, R.StateReceipt);
                                    R.Payment = Bl.GetPayment(R);
                                }
                            }                            
                            FillPays(R);
                        }
                        

                        for (var i = 0; i < IdWorkplacePays.Length; i++)
                        {
                            if (R.Payment != null && R.Payment.Any(el => el.IdWorkplacePay == IdWorkplacePays[i] && el.TypePay!=eTypePay.Wallet ))
                                continue;
                            R.StateReceipt = eStateReceipt.StartPay;
                            R.IdWorkplacePay = IdWorkplacePays[i];
                            Bl.SetStateReceipt(curReceipt, eStateReceipt.StartPay);
                            decimal sum = R.WorkplacePays[i].Sum;
                            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Sum={sum}", eTypeLog.Expanded);
                            SetStateView(eStateMainWindows.ProcessPay);
                            Payment pay = null;
                            if (pTP == eTypePay.Cash)
                            {
                                var SumCash = R.WorkplacePays[i].SumCash;
                                pay = new Payment(R) { IsSuccess = true, TypePay = eTypePay.Cash, SumPay = SumCash, SumExt = (i == IdWorkplacePays.Length - 1 ? Math.Round( pSumCash,1) : Math.Round( SumCash,1)) };
                                pSumCash -= SumCash;
                                if (pSumCash < 0) pSumCash = 0;
                                Bl.db.ReplacePayment(pay,true);                                
                            }
                            else
                            {
                                if (R.TypeReceipt == eTypeReceipt.Refund && PayRefaund != null)
                                {
                                    var PayRef = PayRefaund?.Where(el => el.IdWorkplacePay == R.IdWorkplacePay);
                                    if (PayRef != null && PayRef.Any())
                                        rrn = PayRef.First().CodeAuthorization;
                                }
                                pay = EF.PosPay(R, R.TypeReceipt == eTypeReceipt.Sale ? sum : -sum, rrn, pay, Global.IdWorkPlaceIssuingCash == IdWorkplacePays[i] ? pIssuingCash : 0);
                            }
                            if (pay != null && pay.IsSuccess)
                            {
                                R.StateReceipt = ( i ==IdWorkplacePays.Length-1? eStateReceipt.Pay: eStateReceipt.PartialPay);
                                R.CodeCreditCard = pay.NumberCard;
                                R.NumberReceiptPOS = pay.NumberReceipt;
                                //R.Client = null;
                                R.SumCreditCard = pay.SumPay;
                                Bl.db.ReplaceReceipt(R);
                                R.Payment = Bl.GetPayment(R);
                                
                            }
                            else
                            {
                                R.StateReceipt = R.Payment?.Any() == true ? eStateReceipt.PartialPay : eStateReceipt.Prepare;
                                Bl.SetStateReceipt(curReceipt, R.StateReceipt);
                                TextError = $"Оплата не пройшла: {EquipmentInfo}";
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        R.StateReceipt = eStateReceipt.Prepare;
                        Bl.SetStateReceipt(curReceipt, eStateReceipt.Prepare);
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    }
                    finally { R.IdWorkplacePay = 0; }
                }
                R.StateReceipt = Bl.GetStateReceipt(R);
                if (R.StateReceipt == eStateReceipt.Pay || R.StateReceipt == eStateReceipt.PartialPrint || R.StateReceipt==eStateReceipt.StartPrint )
                { 
                    LogRRO res = null;
                    //Відключаємо контроль контрольної ваги тимчасово до наступної зміни товарного складу.
                    CS.IsControl = false;
                    R.Client = Client;
                    R.StateReceipt = eStateReceipt.StartPrint;
                    Bl.SetStateReceipt(curReceipt, R.StateReceipt);
                    try
                    {
                        SetStateView(eStateMainWindows.ProcessPrintReceipt);
                        R.LogRROs = Bl.GetLogRRO(R);

                        for (var i = 0; i < IdWorkplacePays.Length; i++)
                        {
                            R.IdWorkplacePay = IdWorkplacePays[i];

                            if (!R.LogRROs.Any(el => el.TypeOperation == (R.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund) && el.IdWorkplacePay == IdWorkplacePays[i] && el.CodeError==0 ) )
                            {
                                res = EF.PrintReceipt(R);
                                if (res.CodeError == 0)
                                {
                                    R.SumFiscal += res.SUM;
                                    R.StateReceipt = (i == IdWorkplacePays.Length - 1 ? eStateReceipt.Print : eStateReceipt.PartialPrint);
                                }
                            }
                            if(R.IsPrintIssueOfCash)
                            {
                                res = EF.IssueOfCash(R);
                            }
                        }                       
                        if (res == null)
                            return true;
                        if (res.CodeError == 0)
                        {                         
                            R.NumberReceipt = res.FiscalNumber;
                            R.StateReceipt = eStateReceipt.Print;
                            R.UserCreate = Bl.GetUserIdbyWorkPlace(R.IdWorkplace);                            
                            R.DateReceipt = DateTime.Now;
                            Bl.UpdateReceiptFiscalNumber(R);
                            s.Play(eTypeSound.DoNotForgetProducts);
                            Bl.ds.SendReceiptTo1C(curReceipt);
                            SetCurReceipt(null);
                            Res = true;
                        }
                        else
                        {
                            Bl.SetStateReceipt(curReceipt, R.StateReceipt == eStateReceipt.StartPrint? eStateReceipt.Pay: eStateReceipt.StartPrint);
                            TextError = $"Помилка друку чеків:({res?.CodeError}){Environment.NewLine}{res.Error}";
                            //MessageBox.Show(res.Error, "Помилка друку чеків");
                            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Помилка друку чеків" + res.Error, eTypeLog.Error);
                        }
                    }
                    catch (Exception e)
                    {
                        Bl.SetStateReceipt(curReceipt, R.StateReceipt);
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    }
                    finally
                    { R.IdWorkplacePay = 0; }                    
                }
                if (R.StateReceipt == eStateReceipt.Print || R.StateReceipt == eStateReceipt.Send)
                {                    
                    EF.Print(R);
                }
                SetStateView(eStateMainWindows.WaitInput);
                if (TextError != null)
                {
                    Thread.Sleep(100);
                    CustomMessage.Show(TextError, "Увага!", eTypeMessage.Error);
                }
                return Res;
            }
        }

        void AddExciseStamp(string pES)
        {
            if (CurWares != null)
                if (CurWares.AddExciseStamp(pES) == true)
                {                 //Додання акцизноії марки до алкоголю
                    Bl.UpdateExciseStamp(new List<ReceiptWares>() { CurWares });
                    TypeAccessWait = eTypeAccess.NoDefine;
                    SetStateView(eStateMainWindows.WaitInput);
                }
                else
                    CustomMessage.Show("Дана акцизна марка вже використана!", "Увага", eTypeMessage.Error);
        }

       public void NewReceipt()
        {
            curReceipt = Bl.GetNewIdReceipt();
            s.NewReceipt(curReceipt.CodeReceipt);
            if (StartScan != DateTime.MinValue) StartScan = DateTime.Now;
            Dispatcher.BeginInvoke(new ThreadStart(() => { ShowClientBonus.Visibility = Visibility.Collapsed; }));
            EF.PutToDisplay(curReceipt);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"{curReceipt.ToJSON()}");
        }

        public void FillPays(Receipt pR)
        {
            int[] IdWorkplacePays = pR.IdWorkplacePays;//Wares.Select(el => el.IdWorkplacePay).Distinct().OrderBy(el => el).ToArray();
            pR.WorkplacePays = new WorkplacePay[IdWorkplacePays.Length];
            for (var i = 0; i < IdWorkplacePays.Length; i++)
            {
                pR.IdWorkplacePay = IdWorkplacePays[i];
                var r= new WorkplacePay() { IdWorkplacePay = IdWorkplacePays[i], Sum = EF.SumReceiptFiscal(pR), SumCash = EF.SumCashReceiptFiscal(pR) };
                pR.WorkplacePays[i] = r;
            }
            pR.IdWorkplacePay = 0;
        }

        Status CallBackApi(string pDataApi)
        {
            Status Res = null;
            try
            {
               
                CommandAPI<dynamic> pC = JsonConvert.DeserializeObject<CommandAPI<dynamic>>(pDataApi);
                CommandAPI<int> CommandInt;
                CommandAPI<string> CommandString;

                switch (pC.Command)
                {
                    case eCommand.GetCurrentReceipt:
                        Res = new Status(0, curReceipt?.ToJSON());
                        break;
                    case eCommand.GetReceipt:
                        var Command = JsonConvert.DeserializeObject<CommandAPI<IdReceipt>>(pDataApi);
                        Res = new Status(0, Bl.GetReceiptHead(Command.Data, true)?.ToJSON());
                        break;
                    case eCommand.XReport:
                        CommandInt = JsonConvert.DeserializeObject<CommandAPI<int>>(pDataApi);
                        Res = new Status(0, EF.RroPrintX(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = CommandInt.Data })?.ToJSON());
                        break;
                    case eCommand.OpenShift:
                        CommandString = JsonConvert.DeserializeObject<CommandAPI<string>>(pDataApi);
                        var u = Bl.GetUserByBarCode(CommandString.Data);
                        if (u != null)
                        {
                            AdminControl.OpenShift(u);
                            Res= new Status(0, $"Зміна відкрита:{u.NameUser}");
                        }
                        break;
                }
            }
            catch (Exception ex) { Res = new Status(ex); }
            return Res; 
        }
    }
}
