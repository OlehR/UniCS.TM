using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Equipments.Model;
using Front.Control;
using Front.Equipments;
using Front.Models;
using ModelMID;
using ModelMID.DB;
using Newtonsoft.Json;
using SharedLib;
using Utils;
using Pr = Equipments.Model.Price;

namespace Front
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        int LastCodeWares = 0;

        void SetClient()
        {
            OnPropertyChanged(nameof(ClientName));
            OnPropertyChanged(nameof(Client));
            OnPropertyChanged(nameof(IsViewClientInfo));
        }
        public void InitAction()
        {
            #region Action
            EF.OnControlWeight += (pWeight, pIsStable) =>
            {
                ControlScaleCurrentWeight = pWeight;
                OnPropertyChanged(nameof(ControlScaleCurrentWeight));
                OnPropertyChanged(nameof(StrControlScaleCurrentWeightKg));
                OnPropertyChanged(nameof(IsOwnBag));
                CS.OnScalesData(pWeight, pIsStable);
            };

            EF.OnWeight += (pWeight, pIsStable) =>
            {
                Weight = pWeight / 1000;
                OnPropertyChanged(nameof(Weight));
                OnPropertyChanged(nameof(IsWeightMagellan));
            };

            EF.SetStatus += (info) =>
            {
                if (info.IsСritical == true)
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

                    // if (EquipmentInfo != null)  
                    //    PaymentWindowKSO_UC.EquipmentStatusInPayment.Text = EquipmentInfo; //TMP - не працює через гетер
                    OnPropertyChanged(nameof(EquipmentInfo));
                }));
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"SetStatus ({info.ToJSON()})", eTypeLog.Expanded);
                if (EF.StatCriticalEquipment != eStateEquipment.On)
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ErrorEquipment, null);
            };

            EquipmentFront.OnBarCode += Blf.GetBarCode;


            //!!!TMP Костиль бо не працює підписка на рівні IssueCardUC
            EquipmentFront.OnBarCode += (pBarCode, pTypeBarCode) =>
            {
                if (State == eStateMainWindows.WaitInputIssueCard)
                    IssueCardUC.SetBarCode(pBarCode, pTypeBarCode);
            };

            BLF.OnSetStateView += SetStateView;

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
                DatabaseUpdateStatus = SyncInfo.Status;
                OnPropertyChanged(nameof(DatabaseUpdateStatus));
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
            };

            Global.OnStatusChanged += (Status) =>
            {
                ExchangeRateBar = Status.StringColor;
                OnPropertyChanged(nameof(ExchangeRateBar));
            };

            Global.OnClientChanged += (pClient) =>
            {
                if (curReceipt != null && pClient != null)
                    curReceipt.Client = pClient;

                var r = Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    NumericPad.Visibility = Visibility.Collapsed;
                    Background.Visibility = Visibility.Collapsed;
                    BackgroundWares.Visibility = Visibility.Collapsed;
                    //if (Client != null) ShowClientBonus.Visibility = Visibility.Visible;
                }
                ));
                SetClient();
                if (Client.BirthDay > new DateTime(1900, 1, 1))
                    if (Client.BirthDay.AddYears(18).Date <= DateTime.Now.Date)
                        Bl.AddEventAge(curReceipt);

                if (Client != null && State == eStateMainWindows.FindClientByPhone)
                    State = eStateMainWindows.WaitInput;

                FileLogger.WriteLogMessage($"MainWindow.OnClientChanged(CodeReceipt=>{curReceipt?.CodeReceipt} Client.CodeClient=>{Client.CodeClient} Client.Wallet=> {pClient.Wallet} SumBonus=>{pClient.SumBonus})", eTypeLog.Full);
            };

            Global.Message += (pMessage, pTypeMessage) => CustomMessage.Show(pMessage, "Увага!", pTypeMessage);

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
            VR.SendMessage(Global.IdWorkPlace, $"{TypeAccessWait} =>{pUser.NameUser}", 0, 0, curReceipt?.SumTotal ?? 0, VR.eTypeVRMessage.Confirm);

            Bl.db.InsertReceiptEvent(
                new(curReceipt) {EventType=eReceiptEventType.Other,EventName= TypeAccessWait.ToString(),UserName=pUser.CodeUser.ToString(),CodeWares=CurWares?.CodeWares??0
            });
            switch (TypeAccessWait)
            {
                case eTypeAccess.DelWares:
                    if (curReceipt?.IsLockChange == false)
                    {
                        Bl.ChangeQuantity(CurWares, 0,pUser);
                        CurWares = null;//.Quantity = 0;
                        TypeAccessWait = eTypeAccess.NoDefine;
                        SetStateView(eStateMainWindows.WaitInput);
                    }
                    break;
                case eTypeAccess.DelReciept:
                    VR.SendMessage(Global.IdWorkPlace, $"{TypeAccessWait} => {pUser.NameUser}", 0, 0, curReceipt?.SumTotal ?? 0, VR.eTypeVRMessage.DelReceipt);
                    Bl.SetStateReceipt(curReceipt, eStateReceipt.Canceled);
                    SetCurReceipt(null);
                    TypeAccessWait = eTypeAccess.NoDefine;
                    SetStateView(eStateMainWindows.StartWindow);
                    break;
                case eTypeAccess.ReturnReceipt:
                    Bl.CreateRefund(AdminControl.curReceipt, IsFullReturn);
                    SetStateView(eStateMainWindows.WaitInputRefund);
                    break;

                case eTypeAccess.ConfirmAge:
                    Bl.AddEventAge(curReceipt);
                    TypeAccessWait = eTypeAccess.NoDefine;
                    Blf.PayAndPrint();
                    break;
                case eTypeAccess.ChoicePrice:
                    foreach (Pr el in Prices.ItemsSource)
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
                    var task = Task.Run(() => Blf.PrintAndCloseReceipt(null, eTypePay.Bonus, 0, 0, 0, Client?.SumMoneyBonus ?? 0m));
                    break;
            }
            return true;
        }

        void ShowAdmin(User pUser)
        {
            AdminControl.Init(pUser);
            SetStateView(eStateMainWindows.AdminPanel);
        }

        public void ShowWeightWares(GW pGV = null)
        {
            if (pGV != null)
            {
                CurW = pGV;
                Image im = null;
                foreach (var el in WeightWaresUC.GridWeightWares.Children)
                {
                    im = el as Image;
                    if (im != null)
                        break;
                }
                if (im != null)
                    WeightWaresUC.GridWeightWares.Children.Remove(im);
                if (File.Exists(CurW.Pictures))
                {
                    im = new Image
                    {
                        Source = new BitmapImage(new Uri(CurW.Pictures)),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(im, 1);
                    WeightWaresUC.GridWeightWares.Children.Add(im);
                }
                SetStateView(eStateMainWindows.WaitWeight);
                return;
            }
        }

        /*public void PayAndPrint()
        {
            if (curReceipt.StateReceipt < eStateReceipt.Pay && curReceipt.CountWeightGoods > 0 && !curReceipt.Wares.Any(x => x.CodeWares == Global.Settings.CodePackagesBag) && !curReceipt.IsPakagesAded && curReceipt.TypeReceipt == eTypeReceipt.Sale)
            {
                SetStateView(eStateMainWindows.AddMissingPackage);
                return;
            }

            if (curReceipt.StateReceipt < eStateReceipt.Pay && curReceipt.AgeRestrict > 0 && curReceipt.IsConfirmAgeRestrict == false)
            {
                SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ConfirmAge);
                return;
            }            
            EquipmentInfo = string.Empty;
            OnPropertyChanged(nameof(EquipmentInfo));
            if (Global.TypeWorkplaceCurrent == eTypeWorkplace.CashRegister && (curReceipt.StateReceipt == eStateReceipt.Prepare || curReceipt.StateReceipt == eStateReceipt.StartPay))
            {
                PaymentWindow.UpdatePaymentWindow();
                SetStateView(eStateMainWindows.ChoicePaymentMethod);
            }
            else
            {
                var task = Task.Run(() => Blf.PrintAndCloseReceipt());
            }
        }*/

        Status CallBackApi(string pDataApi)
        {
            Status Res = null;
            try
            {
                CommandAPI<dynamic> pC = JsonConvert.DeserializeObject<CommandAPI<dynamic>>(pDataApi);
                CommandAPI<int> CommandInt;
                CommandAPI<string> CommandString;
                CommandAPI<InfoRemoteCheckout> CommandRemoteInfo;
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
                            Res = new Status(0, $"Зміна відкрита:{u.NameUser}");
                        }
                        break;
                    case eCommand.GeneralCondition:
                        CommandRemoteInfo = JsonConvert.DeserializeObject<CommandAPI<InfoRemoteCheckout>>(pDataApi);
                        var r = Bl.GetUserByBarCode(CommandRemoteInfo.Data.UserBarcode);
                        RemoteCheckout = CommandRemoteInfo.Data;
                        RemoteWorkplace = Bl.db.GetWorkPlace().FirstOrDefault(el => el.IdWorkplace == RemoteCheckout.RemoteIdWorkPlace);
                        if (RemoteCheckout.RemoteCigarettesPrices.Count > 1)
                            RemotePrices.ItemsSource = RemoteCheckout.RemoteCigarettesPrices;
                        OnPropertyChanged(nameof(RemoteWorkplace));
                        OnPropertyChanged(nameof(RemoteCheckout));
                        Res = new Status(0, $"Загальний стан каси: {RemoteWorkplace.Name}");
                        break;
                    case eCommand.Confirm:
                        CommandRemoteInfo = JsonConvert.DeserializeObject<CommandAPI<InfoRemoteCheckout>>(pDataApi);
                        if (CommandRemoteInfo.Data.StateMainWindows == eStateMainWindows.BlockWeight || CommandRemoteInfo.Data.StateMainWindows == eStateMainWindows.ProblemWeight
                            || CommandRemoteInfo.Data.TypeAccess == eTypeAccess.FixWeight)
                        {
                            CS.RW.FixWeightQuantity = CS.RW.Quantity;
                            CS.RW.FixWeight += Convert.ToDecimal(CS.СurrentlyWeight);
                            CS.StateScale = eStateScale.Stabilized;
                            SetStateView(eStateMainWindows.WaitInput);
                        }
                        if (CommandRemoteInfo.Data.StateMainWindows == eStateMainWindows.WaitAdmin && CommandRemoteInfo.Data.TypeAccess == eTypeAccess.ChoicePrice)
                        {
                            Bl.AddEventAge(curReceipt);
                            Blf.AddWares(CurWares.CodeWares, CurWares.CodeUnit, CommandRemoteInfo.Data.QuantityCigarettes, CommandRemoteInfo.Data.SelectRemoteCigarettesPrice.price);
                            QuantityCigarettes = 1;
                            SetStateView(eStateMainWindows.WaitInput);
                        }
                        Res = new Status(0, $"{CommandRemoteInfo.Data.StateMainWindows} {CommandRemoteInfo.Data.TypeAccess}");
                        break;
                    case eCommand.DeleteReceipt:
                        if (curReceipt != null && curReceipt.StateReceipt == eStateReceipt.Prepare)
                            Bl.SetStateReceipt(curReceipt, eStateReceipt.Canceled);

                        SetCurReceipt(null);
                        SetStateView(eStateMainWindows.StartWindow);
                        Res = new Status(0, $"Чек видалено!");
                        break;
                }
            }
            catch (Exception ex) { Res = new Status(ex); }
            return Res;
        }
    }
}
