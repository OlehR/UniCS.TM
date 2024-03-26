using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaMain.Models;
using AvaloniaMain.Views;
using Front;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModelMID;
using ModelMID.DB;
using ReactiveUI;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace AvaloniaMain.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        public void InitAction()
        {
            #region Action
            EF.OnControlWeight += (pWeight, pIsStable) =>
            {
                /*ControlScaleCurrentWeight = pWeight;
                OnPropertyChanged(nameof(ControlScaleCurrentWeight));
                OnPropertyChanged(nameof(StrControlScaleCurrentWeightKg));
                OnPropertyChanged(nameof(IsOwnBag));*/
                CS.OnScalesData(pWeight, pIsStable);
            };

            EF.OnWeight += (pWeight, pIsStable) =>
            {
                /*Weight = pWeight / 1000;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Weight)));
                OnPropertyChanged(nameof(IsWeightMagellan));*/
            };

            EF.SetStatus += (info) =>
            {
                Dispavaloniaatcher.BeginInvoke(new ThreadStart(() =>{ }));
                /*if (info.IsСritical == true)
                {
                    LastErrorEquipment = info.TextState;
                    FileLogger.WriteLogMessage(this, $"EF.SetStatus {info.ModelEquipment}", $"{info.TextState}", eTypeLog.Error);
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ErrorEquipment);
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
                        PaymentWindowKSO_UC.EquipmentStatusInPayment.Text = EquipmentInfo; //TMP - не працює через гетер
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EquipmentInfo)));
                }));
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"SetStatus ({info.ToJSON()})", eTypeLog.Expanded);
                if (EF.StatCriticalEquipment != eStateEquipment.On)
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ErrorEquipment, null);*/
            };

            int LastCodeWares = 0;

            EquipmentFront.OnBarCode += Blf.GetBarCode;

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

                        if (pReceipt.GetLastWares != null && LastCodeWares != pReceipt.GetLastWares.CodeWares)
                        {
                            LastCodeWares = pReceipt.GetLastWares.CodeWares;
                            ReceiptWares cl = (ReceiptWares)pReceipt.GetLastWares.Clone();
                            EF.ProgramingArticleAsync(cl);
                        }
                        else
                            EF.PutToDisplay(pReceipt, $"{CurWares.NameWaresReceipt}{Environment.NewLine}{CurWares.Quantity}x{CurWares.Price}={CurWares.SumTotal}", 0);
                    }
                    // if (curReceipt?.Wares?.Count() == 0 && curReceipt.OwnBag==0d) CS.WaitClear();
                    CS.StartWeightNewGoogs(curReceipt, IsDel ? CurWares : null);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage($"MainWindow.OnReceiptCalculationComplete Exception =>(pReceipt=>{pReceipt.ToJSON()}) => ({Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace})", eTypeLog.Error);
                }

                FileLogger.WriteLogMessage($"MainWindow.OnReceiptCalculationComplete(pReceipt=>{pReceipt.ToJson()})", eTypeLog.Full);
            };

            Global.OnSyncInfoCollected += (SyncInfo) =>
            {
                /*DatabaseUpdateStatus = SyncInfo.Status;
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

                if (eSyncStatus.IncorectDiscountBarcode == SyncInfo.Status || eSyncStatus.IncorectProductForDiscount == SyncInfo.Status)
                {
                    CustomMessage.Show(SyncInfo.StatusDescription, "Увага!!!", eTypeMessage.Warning);
                }
                FileLogger.WriteLogMessage($"MainWindow.OnSyncInfoCollected Status=>{SyncInfo.Status} StatusDescription=>{SyncInfo.StatusDescription}", eTypeLog.Full);
            */
            };

            //Зміна Статусу обладнання
            Global.OnStatusChanged += (Status) =>
            {
                /*ExchangeRateBar = Status.StringColor;
                OnPropertyChanged(nameof(ExchangeRateBar));*/
            };

            //Приходить подія про зміну клієнта.
            Global.OnClientChanged += (pClient) =>
            {
                if (curReceipt != null && pClient != null)
                    curReceipt.Client = pClient;

                /*var r = Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    NumericPad.Visibility = Visibility.Collapsed;
                    Background.Visibility = Visibility.Collapsed;
                    BackgroundWares.Visibility = Visibility.Collapsed;
                    //if (Client != null) ShowClientBonus.Visibility = Visibility.Visible;
                }
                ));
                SetClient();*/
                if (Client?.BirthDay > new DateTime(1900, 1, 1))
                    if (Client.BirthDay.AddYears(18).Date <= DateTime.Now.Date)
                        Bl.AddEventAge(curReceipt);
                if (Client != null && State == eStateMainWindows.FindClientByPhone)
                    State = eStateMainWindows.WaitInput;
                FileLogger.WriteLogMessage($"MainWindow.OnClientChanged(CodeReceipt=>{curReceipt?.CodeReceipt} Client.CodeClient=>{pClient?.CodeClient} Client.Wallet=> {pClient?.Wallet} SumBonus=>{pClient?.SumBonus})", eTypeLog.Full);
            };

            //Global.Message += (pMessage, pTypeMessage) => CustomMessage.Show(pMessage, "Увага!", pTypeMessage);

            Bl.OnAdminBarCode += (pUser) =>
            {
               /* if (TypeAccessWait > 0)//TypeAccessWait != eTypeAccess.NoDefinition 
                {
                    SetConfirm(pUser);
                    return;
                }

                if (Access.GetRight(pUser, eTypeAccess.AdminPanel))
                    ShowAdmin(pUser);
                else
                    CustomMessage.Show($"Не достатньо прав на вхід в адмін панель для  {pUser.NameUser}", "Увага", eTypeMessage.Error);*/
            };

            Bl.OnCustomWindow += (pCW) =>
            {
                //SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, pCW);
            };

            //Обробка стану контрольної ваги.
            /*CS.OnStateScale += (pStateScale, pRW, pСurrentlyWeight) =>
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
        };*/
        }
        #endregion

        public void SetCurReceipt(Receipt pReceipt, bool IsRefresh = true)
        {
            try
            {
                var OldClient = curReceipt?.Client;
                curReceipt = pReceipt;
                if (curReceipt == null)
                {
                        //Dispatcher.BeginInvoke(new ThreadStart(() => { 
                        ListWares?.Clear(); //}));
                    CS.WaitClear();
                }
                else
                {
                    //Dispatcher.BeginInvoke(new ThreadStart(() =>
                    //{
                        if (pReceipt.Wares?.Any() == true)
                            ListWares = new ObservableCollection<ReceiptWares>(pReceipt.Wares);
                        else
                            ListWares?.Clear();

                        /*WaresList.ItemsSource = ListWares;
                        if (WaresList.Items.Count > 0)
                            WaresList.SelectedIndex = WaresList.Items.Count - 1;
                        if (VisualTreeHelper.GetChildrenCount(WaresList) > 0)
                        {
                            Border border = (Border)VisualTreeHelper.GetChild(WaresList, 0);
                            ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                            scrollViewer.ScrollToBottom();
                        }*/
                   // }));
                    if (OldClient?.CodeClient != 0 && curReceipt.CodeClient != 0 && curReceipt.Client == null && OldClient.CodeClient == curReceipt.CodeClient)
                    {
                        curReceipt.Client = OldClient;
                    }

                    if (curReceipt.CodeClient != 0 && string.IsNullOrEmpty(curReceipt.Client?.NameClient))
                        Bl.GetClientByCode(curReceipt, curReceipt.CodeClient);

                    //if (curReceipt?.IsNeedExciseStamp == true)
                    //    SetStateView(eStateMainWindows.WaitInput);
                    //SetClient();
                }
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            //SetPropertyChanged();
        }

        public void GetBarCode(string pBarCode, string pTypeBarCode)
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"(pBarCode=>{pBarCode},  pTypeBarCode=>{pTypeBarCode})");
            /*if (State == eStateMainWindows.StartWindow)
                SetStateView(eStateMainWindows.WaitInput);
            if (State == eStateMainWindows.WaitInputIssueCard)
            {
                IssueCardUC.SetBarCode(pBarCode);
                return;
            }*/

            //Точно треба зробити через стан eStateMainWindows
            /*if (Global.Settings.IsUseCardSparUkraine && NumericPad.Visibility == Visibility.Visible && "Введіть номер телефону".Equals(InputNumberPhone.Desciption))
            {
                // pBarCode = "MTE2MmZlMGNjLTNlZmQtNDYxZC05NThiLTFjYmI3NjQ4YjM1NDIzLjAxLjIwMjQgMTM6MDE6Mjg=";
                if (pBarCode.Length > 56)
                {
                    var QR = pBarCode.FromBase64();
                    if (!string.IsNullOrEmpty(QR) && "1".Equals(QR[..1]) && QR.Length >= 56)
                    {
                        string BarCode = QR[1..37];
                        string Time = QR[37..56];
                        DateTime dt = Time.ToDateTime("dd.MM.yyyy HH:mm:ss");
                        if ((DateTime.Now - dt).TotalSeconds < 120)
                            Bl.GetDiscount(new FindClient { BarCode = BarCode }, curReceipt);
                    }
                }
            }*/

            var u = Bl.GetUserByBarCode(pBarCode);
            if (u != null)
            { Bl.OnAdminBarCode?.Invoke(u); return; }

            /*if (TypeAccessWait == eTypeAccess.ExciseStamp)
            {
                string ExciseStamp = GetExciseStamp(pBarCode);
                if (!string.IsNullOrEmpty(ExciseStamp))
                {
                    AddExciseStamp(ExciseStamp);
                    return;
                }
            }
            else*/
            {
                ReceiptWares w = null;
                //if (IsAddNewWares && (State == eStateMainWindows.WaitInput || State == eStateMainWindows.StartWindow))
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
                //else
                //{
                //    w = Bl.AddWaresBarCode(curReceipt, pBarCode, 1, true);
                //}
                //if (w != null)  return;

                if (curReceipt != null)
                {
                    var c = Bl.GetClientByBarCode(curReceipt, pBarCode.ToLower());
                    if (c != null) return;
                }
            }

            //if ((State != eStateMainWindows.WaitInput && State != eStateMainWindows.StartWindow) || curReceipt?.IsLockChange == true || !IsAddNewWares)
            //    if (State != eStateMainWindows.ProcessPay && State != eStateMainWindows.ProcessPrintReceipt && State != eStateMainWindows.WaitCustomWindows)
            //        SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.AdminPanel);
       }

        public void NewReceipt()
        {
            SetCurReceipt(Bl.GetNewIdReceipt());
           // if (curReceipt != null) s.NewReceipt(curReceipt.CodeReceipt);
           // if (StartScan != DateTime.MinValue) StartScan = DateTime.Now;
            //Dispatcher.BeginInvoke(new ThreadStart(() => { ShowClientBonus.Visibility = Visibility.Collapsed; }));
            EF.PutToDisplay(curReceipt);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"CodeReceipt=>{curReceipt?.CodeReceipt}");
        }

        void IsPrises(decimal pQuantity = 0m, decimal pPrice = 0m)
        {
            if (CurWares.TypeWares == eTypeWares.Alcohol && CurWares?.Price > 0m)
            {
                //SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ExciseStamp, CurWares);
                return;
            }

            if (CurWares.Price == 0) //Повідомлення Про відсутність ціни
            {
                //SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, new CustomWindow(eWindows.NoPrice, CurWares.NameWares));
            }
            if (CurWares.Prices != null && pPrice == 0m) //Меню з вибором ціни. Сигарети.
            {
                if (CurWares.IsMultiplePrices)
                {
                    //SetStateView(eStateMainWindows.WaitInputPrice, eTypeAccess.NoDefine, CurWares);
                }
            }
            if (CurWares.IsMultiplePrices && pPrice > 0m)
                CurWares = null;
        }

    }
}


/*  private bool _numPadVIsibility = false;
          public bool NumPadVIsibility
          {
              get => _numPadVIsibility;
              set => this.RaiseAndSetIfChanged(ref _numPadVIsibility, value);
          }
          private bool _issueCardVisibility = false;
          public bool IssueCardVisibility
          {
              get => _issueCardVisibility;
              set => this.RaiseAndSetIfChanged(ref _issueCardVisibility, value);
          }
          private bool _productsVisibility=true;
          public bool ProductsVisibility
          {
              get => _productsVisibility;
              set=>this.RaiseAndSetIfChanged(ref _productsVisibility, value);
          }
        
               public bool MainVIsibility
        {
            get => _mainVIsibility;
            set => this.RaiseAndSetIfChanged(ref _mainVIsibility, value);
        }
        private bool _keyBoardVIsibility = true;

        public bool KeyBoardVIsibility
        {
            get => _keyBoardVIsibility;
            set => this.RaiseAndSetIfChanged(ref _keyBoardVIsibility, value);
        }*/