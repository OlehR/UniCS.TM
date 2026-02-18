using Equipments.Model;
using Model;
using ModelMID;
using ModelMID.DB;
using SharedLib;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using UtilNetwork;
using Utils;
using Pr = Equipments.Model.Price;

namespace Front.Equipments
{
    public partial class BLF
    {
        static BLF sBLF;
        public static BLF GetBLF { get { return sBLF ?? new BLF(); } }

        IMW MW;
        BL Bl { get { return MW?.Bl; } }
        EquipmentFront EF { get { return MW?.EF; } }
        public BLF()
        {
            sBLF = this;
        }
        public void Init(IMW pMW) => MW = pMW;


        public static Action<eStateMainWindows, eTypeAccess, ReceiptWares, CustomWindow, eSender> OnSetStateView { get; set; }
        public void SetStateView(eStateMainWindows pSMV = eStateMainWindows.NotDefine, eTypeAccess pTypeAccess = eTypeAccess.NoDefine, ReceiptWares pRW = null, CustomWindow pCW = null, eSender pS = eSender.NotDefine) =>
            OnSetStateView?.Invoke(pSMV, pTypeAccess, pRW, pCW, pS);

        public void GetBarCode(string pBarCode, string pTypeBarCode)
        {
            if (pBarCode?.StartsWith("]C1") == true)//!!!TMP 
                pBarCode = pBarCode[3..];
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"(pBarCode=>{pBarCode},  pTypeBarCode=>{pTypeBarCode})");
            if (string.IsNullOrEmpty(pBarCode)) return;
            if (pBarCode.ToUpper().StartsWith("HTTP") && !pBarCode.ToUpper().Contains("T.GOV.UA"))
                return; //Не обробляємо QR посилання на сайти.
            EF?.ForceGoodReadTone();

            if (MW.State == eStateMainWindows.StartWindow)
                SetStateView(eStateMainWindows.WaitInput);
            if (MW.State == eStateMainWindows.WaitInputIssueCard) return;


            if (Global.Settings.IsUseCardSparUkraine) //&& MW.State == eStateMainWindows.FindClientByPhone
            {
                // pBarCode = "MTE2MmZlMGNjLTNlZmQtNDYxZC05NThiLTFjYmI3NjQ4YjM1NDIzLjAxLjIwMjQgMTM6MDE6Mjg=";
                if (pBarCode.Length > 56)
                {
                    var QR = pBarCode.FromBase64();
                    if (!string.IsNullOrEmpty(QR) && QR.Length >= 56)
                    {
                        string BarCode = QR[1..37];
                        string Time = QR[37..56];
                        DateTime dt = Time.ToDateTime("dd.MM.yyyy HH:mm:ss");
                        if ((DateTime.Now - dt).TotalSeconds < 120)
                            Bl.GetDiscount(new FindClient { BarCode = BarCode }, MW.curReceipt);
                    }
                }
                else
                if (pBarCode.StartsWith("*1*"))
                {
                    var c = Bl.GetClientByBarCode(MW.curReceipt, pBarCode.ToLower());
                    if (c == null)
                        Bl.GetDiscount(new FindClient { BarCode = pBarCode }, MW.curReceipt);
                    return;
                }
            }

            var u = Bl.GetUserByBarCode(pBarCode);
            if (u != null)
            { Bl.OnAdminBarCode?.Invoke(u); return; }

            if (MW.TypeAccessWait == eTypeAccess.ExciseStamp)
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
                if (MW.IsAddNewWares) //&& (MW.State == eStateMainWindows.WaitInput || MW.State == eStateMainWindows.StartWindow)
                {
                    if (MW.curReceipt == null || !MW.curReceipt.IsLockChange)
                    {
                        if (MW.curReceipt == null)
                            NewReceipt();
                        w = Bl.AddWaresBarCode(MW.curReceipt, pBarCode, 1,false,true);
                        if (w != null && w.CodeWares > 0)
                        {
                            MW.CurWares = w;
                            IsPrises(1, 0);
                        }
                    }
                }
                else
                {
                    w = Bl.AddWaresBarCode(MW.curReceipt, pBarCode, 1, true, true);
                }
                if (w != null)
                    return;

                if (MW.curReceipt != null)
                {
                    var c = Bl.GetClientByBarCode(MW.curReceipt, pBarCode.ToLower());
                    if (c != null) return;
                }
                FileLogger.WriteLogMessage(this, "GetBarCode", $"Штрихкод {pBarCode} не знайдено State=>{MW.State} TypeAccessWait=>{MW.TypeAccessWait}");
                if (MW.IsAddNewWares)

                    Global.Message?.Invoke($"Даний штрихкод {pBarCode} не знайдено в базі!!!", eTypeMessage.Error);
                else
                {
                    if (MW.CS?.IsProblem == true)
                        Global.Message?.Invoke($"Не можливо додати товар оскільки є проблеми з ваговою платформою!", eTypeMessage.Error);
                    else
                        if (!Global.Settings.IsUseCardSparUkraine || MW.State != eStateMainWindows.FindClientByPhone)
                        Global.Message?.Invoke($"Не можливо додати товар в  режимі {MW.State.GetDescription()}", eTypeMessage.Error);
                }

                return;
            }

            if ((MW.State != eStateMainWindows.WaitInput && MW.State != eStateMainWindows.StartWindow) || MW.curReceipt?.IsLockChange == true || !MW.IsAddNewWares)
                if (MW.State != eStateMainWindows.ProcessPay && MW.State != eStateMainWindows.ProcessPrintReceipt && MW.State != eStateMainWindows.WaitCustomWindows)
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.AdminPanel);

        }

        public string GetExciseStamp(string pBarCode)
        {
            pBarCode = pBarCode.ToUpper();
            if (pBarCode.Contains("T.GOV.UA"))
            {
                string Res = pBarCode.Substring(pBarCode.IndexOf("T.GOV.UA") + 9);
                pBarCode = Res.Substring(0, Res.Length - 11);
            }

            Regex regex = new Regex(@"^[A-Z]{4}\d{6}$");

            if (regex.IsMatch(pBarCode))
                return pBarCode;
            return null;
        }

        public void AddExciseStamp(string pES)
        {
            if (MW.CurWares == null)
                MW.CurWares = MW.curReceipt.GetLastWares;
            if (MW.CurWares != null)
            {
                if (!"None".Equals(pES))
                {
                    if (Global.Settings.IsCheckExciseStamp)
                    {
                        var res = Bl.ds.CheckExciseStamp(new ExciseStamp(MW.CurWares, pES));
                        if (res != null)
                        {
                            if (!res.Equals(MW.CurWares)) //&& res.State >= 0
                            {
                                Global.Message?.Invoke($"Дана акцизна марка {pES} вже використана {res.CodePeriod} Касове місце=>{res.IdWorkplace} Чек=>{res.CodeReceipt} CodeWares=>{res.CodeWares}!", eTypeMessage.Error);
                                return;
                            }
                        }
                    }
                }
                if (MW.CurWares.AddExciseStamp(pES))
                {                 //Додання акцизноії марки до алкоголю
                    Bl.UpdateExciseStamp(new List<ReceiptWares>() { MW.CurWares });
                    MW.TypeAccessWait = eTypeAccess.NoDefine;
                    SetStateView(eStateMainWindows.WaitInput);
                }
                else
                    Global.Message?.Invoke("Дана акцизна марка вже використана!", eTypeMessage.Error);
            }
        }

        public void IsPrises(decimal pQuantity = 0m, decimal pPrice = 0m)
        {
            if (MW.CurWares.TypeWares == eTypeWares.Alcohol && MW.CurWares?.Price > 0m)
            {
                SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ExciseStamp, MW.CurWares);
                return;
            }

            if (MW.CurWares?.Price == 0) //Повідомлення Про відсутність ціни
            {
                SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, new CustomWindow(eWindows.NoPrice, MW.CurWares.NameWares));
            }
            if (MW.CurWares?.Prices != null && pPrice == 0m) //Меню з вибором ціни. Сигарети.
            {
                if (MW.CurWares.IsMultiplePrices)
                {
                    SetStateView(eStateMainWindows.WaitInputPrice, eTypeAccess.NoDefine, MW.CurWares);
                }
            }
            if (MW.CurWares?.IsMultiplePrices == true && pPrice > 0m)
                MW.CurWares = null;
        }

        DateTime StartScan = DateTime.MinValue;

        public void TimeScan(bool? pIsSave = null)
        {
            if ((MW.State == eStateMainWindows.WaitAdmin && !MW.CS.IsProblem) || MW.State == eStateMainWindows.AdminPanel || MW.State == eStateMainWindows.WaitAdminLogin ||
                       MW.State == eStateMainWindows.ChoicePaymentMethod || MW.State == eStateMainWindows.ProcessPay || MW.State == eStateMainWindows.StartWindow || pIsSave == true)
            {
                if (StartScan != DateTime.MinValue)
                {
                    Bl.db.InsertReceiptEvent(new ReceiptEvent(MW.curReceipt) { ResolvedAt = StartScan, EventType = eReceiptEventType.TimeScanReceipt, EventName = "Час сканування чека" });
                    StartScan = DateTime.MinValue;
                }
            }
            else
            {
                if (pIsSave == false || (StartScan == DateTime.MinValue && (
                        MW.State == eStateMainWindows.WaitInput || MW.State == eStateMainWindows.WaitFindWares || MW.State == eStateMainWindows.WaitInputPrice || MW.State == eStateMainWindows.WaitInputIssueCard)))
                    StartScan = DateTime.Now;
            }
        }

        public void NewReceipt()
        {
            Bl.GetNewIdReceipt();
            if (MW.curReceipt != null)
                MW.s.NewReceipt(MW.curReceipt.CodeReceipt);
            if (StartScan != DateTime.MinValue) StartScan = DateTime.Now;
            //Dispatcher.BeginInvoke(new ThreadStart(() => { ShowClientBonus.Visibility = Visibility.Collapsed; }));
            EF.PutToDisplay(MW.curReceipt);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"CodeReceipt=>{MW.curReceipt?.CodeReceipt}");
        }

        public void SetCurReceipt(Receipt pReceipt, bool IsRefresh = true)
        {
            try
            {
                var OldClient = MW.curReceipt?.Client;
                MW.curReceipt = new(); //Через дивний баг коли curReceipt.Wares залишалось порожне. а в pReceipt було з записами.
                MW.curReceipt = pReceipt;

                if (MW.curReceipt == null)
                {
                    MW.RunOnUiThread(() => { MW.ListWares?.Clear(); });
                    MW.CS.WaitClear();
                }
                else
                {
                    MW.RunOnUiThread(() =>
                    {
                        if (pReceipt.Wares?.Any() == true)
                            MW.ListWares = new ObservableCollection<ReceiptWares>(pReceipt.Wares);
                        else
                            MW.ListWares?.Clear();
                    });
                    if (OldClient?.CodeClient != 0 && MW.curReceipt.CodeClient != 0 && MW.curReceipt.Client == null && OldClient.CodeClient == MW.curReceipt.CodeClient)
                    {
                        MW.curReceipt.Client = OldClient;
                    }

                    if (MW.curReceipt.CodeClient != 0 && string.IsNullOrEmpty(MW.curReceipt.Client?.NameClient))
                        Bl.GetClientByCode(MW.curReceipt, MW.curReceipt.CodeClient);

                    if (MW.curReceipt?.IsNeedExciseStamp == true)
                        SetStateView(eStateMainWindows.WaitInput);
                }
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
        }

        public void AddWares(long pCodeWares, int pCodeUnit = 0, decimal pQuantity = 0m, decimal pPrice = 0m)
        {
            if (pCodeWares > 0)
            {
                if (MW.curReceipt == null)
                    NewReceipt();
                MW.CurWares = Bl.AddWaresCode(MW.curReceipt, pCodeWares, pCodeUnit, pQuantity, pPrice);

                if (MW.CurWares != null)
                    IsPrises(pQuantity, pPrice);
            }
        }

        public void ExciseStampNone()
        {
            AddExciseStamp("None");
            Bl.AddEventAge(MW.curReceipt);
        }

        public void CustomWindowClickButton(CustomButton res)
        {
            if (res != null)
            {
                if (res.CustomWindow?.Id == eWindows.NoPricePromotion && MW.curReceipt?.Client != null)
                {
                    PricePromotion PP = res.CustomWindow.DataEx as PricePromotion;

                    Bl.db.ReplaceReceiptWaresPromotionNoPrice(new ReceiptWaresPromotionNoPrice(new IdReceiptWares(MW.curReceipt))
                    { CodePS = PP?.CodePs ?? 0, DataEx = res.Id, TypeDiscount = PP?.TypeDiscount ?? eTypeDiscount.NotDefine, Data = 1 });
                    if (PP?.IsOneTime == true)
                        Bl.db.ReplaceOneTime(new OneTime(MW.curReceipt) { CodePS = PP.CodePs, CodeData = MW.curReceipt?.Client?.CodeClient ?? 0, TypeData = eTypeCode.Client });
                    PayAndPrint();
                    return;
                }

                if (res.CustomWindow?.Id == eWindows.RestoreLastRecipt)
                {
                    if (res.Id == 1)
                    {
                        var R = Bl.GetLastReceipt();
                        SetCurReceipt(R);
                        Bl.db.RecalcPriceAsync(new IdReceiptWares(R));
                        SetStateView(eStateMainWindows.WaitInput);
                    }
                    if (res.Id == 2)
                    {
                        var Res = Bl.GetLastReceipt();
                        Bl.SetStateReceipt(Res, eStateReceipt.Canceled);
                        SetStateView(eStateMainWindows.StartWindow);
                    }
                    return;
                }

                if (res.CustomWindow?.Id == eWindows.ConfirmWeight)
                {
                    if (res.Id == -1)
                    {
                        MW.IsShowWeightWindows = false;
                        SetStateView(eStateMainWindows.BlockWeight);
                        return;
                    }
                    if (res.Id == 4)
                    {
                        MW.IsShowWeightWindows = false;
                        EF.ControlScaleCalibrateZero();
                        return;
                    }

                    if (res.Id == 6)
                    {
                        NewReceipt();
                        SetStateView(eStateMainWindows.StartWindow);
                        return;
                    }
                    if (MW.CS.RW != null)
                    {
                        MW.CS.RW.FixWeightQuantity = MW.CS.RW.Quantity;
                        MW.CS.RW.FixWeight += Convert.ToDecimal(MW.CS.СurrentlyWeight);
                        MW.CS.StateScale = eStateScale.Stabilized;
                    }

                }

                if (res.CustomWindow?.Id == eWindows.ExciseStamp)
                {
                    if (res.Id == 32)
                    {
                        MW.IsWaitAdminTitle = true;
                        //TMP!!!!
                        //MW.WaitAdminTitle.Visibility = Visibility.Visible;
                        EF.SetColor(System.Drawing.Color.Violet);
                        MW.s.Play(eTypeSound.WaitForAdministrator);
                    }
                    else
                    if (res.Id == 33)
                    {
                        ExciseStampNone();
                    }
                    if (res.Id == 34) // видалити позицію в якій відсутній акциз або він затертий
                    {
                        MW.TypeAccessWait = eTypeAccess.DelWares;
                        SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.DelWares);
                    }
                    return;
                }
                if (res.CustomWindow?.Id == eWindows.ConfirmAge)
                {
                    MW.TypeAccessWait = eTypeAccess.NoDefine;
                    SetStateView(eStateMainWindows.WaitInput);

                    if (res.Id == 1)
                        Task.Run(new Action(() => { Bl.AddEventAge(MW.curReceipt); PayAndPrint(); }));
                    return;
                }

                if (res.CustomWindow?.Id == eWindows.UseBonus)
                {
                    //Напевно краще зробити через подіхї. MW.LastVerifyCode = Bl.ds.GetVerifySMS(MW.ClientPhoneNumvers[(int)res.Id]);
                    MW.LastVerifyCode = Bl.ds.GetVerifySMS(res.Text);
                    Global.Message?.Invoke($"Код підтвердження надіслано за номером {res.Text}", eTypeMessage.Information);
                    return;
                }

                var r = new CustomWindowAnswer()
                {
                    idReceipt = MW.curReceipt,
                    Id = res.CustomWindow?.Id ?? eWindows.NoDefinition,
                    IdButton = res.Id,
                    Text = res.CustomWindow?.InputText,
                    ExtData = res.CustomWindow?.Id == eWindows.ConfirmWeight ? MW.CS?.RW : null
                };
                Bl.SetCustomWindows(r);
                SetStateView(eStateMainWindows.WaitInput);
            }

        }
        public global::UtilNetwork.Result CallBackApi(string pDataApi)
        {
            global::UtilNetwork.Result Res = null;
            try
            {
                CommandAPI<dynamic> pC = JsonSerializer.Deserialize<CommandAPI<dynamic>>(pDataApi);
                CommandAPI<int> CommandInt;
                CommandAPI<string> CommandString;
                CommandAPI<InfoRemoteCheckout> CommandRemoteInfo;
                switch (pC.Command)
                {
                    case eCommand.GetCurrentReceipt:
                        Res = new global::UtilNetwork.Result(0, MW.curReceipt?.ToJson());
                        break;
                    case eCommand.GetReceipt:
                        var Command = JsonSerializer.Deserialize<CommandAPI<IdReceipt>>(pDataApi);
                        Res = new global::UtilNetwork.Result(0, Bl.GetReceiptHead(Command.Data, true)?.ToJSON());
                        break;
                    case eCommand.XReport:
                        CommandInt = JsonSerializer.Deserialize<CommandAPI<int>>(pDataApi);
                        Res = new global::UtilNetwork.Result(0, EF.RroPrintX(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = CommandInt.Data })?.ToJSON());
                        break;
                    case eCommand.OpenShift:
                        CommandString = JsonSerializer.Deserialize<CommandAPI<string>>(pDataApi);
                        var u = Bl.GetUserByBarCode(CommandString.Data);
                        if (u != null)
                        {
                            OpenShift(u);
                            Res = new global::UtilNetwork.Result(0, $"Зміна відкрита:{u.NameUser}");
                        }
                        break;
                    case eCommand.GeneralCondition:
                        CommandRemoteInfo = JsonSerializer.Deserialize<CommandAPI<InfoRemoteCheckout>>(pDataApi);
                        var r = Bl.GetUserByBarCode(CommandRemoteInfo.Data.UserBarcode);
                        MW.RemoteCheckout = CommandRemoteInfo.Data;

                        Res = new global::UtilNetwork.Result(0, $"Загальний стан каси: {MW.RemoteWorkplace.Name}");
                        break;
                    case eCommand.Confirm:
                        CommandRemoteInfo = JsonSerializer.Deserialize<CommandAPI<InfoRemoteCheckout>>(pDataApi);
                        if (CommandRemoteInfo.Data.StateMainWindows == eStateMainWindows.BlockWeight || CommandRemoteInfo.Data.StateMainWindows == eStateMainWindows.ProblemWeight
                            || CommandRemoteInfo.Data.TypeAccess == eTypeAccess.FixWeight)
                        {
                            MW.CS.RW.FixWeightQuantity = MW.CS.RW.Quantity;
                            MW.CS.RW.FixWeight += Convert.ToDecimal(MW.CS.СurrentlyWeight);
                            MW.CS.StateScale = eStateScale.Stabilized;
                            SetStateView(eStateMainWindows.WaitInput);
                        }
                        if (CommandRemoteInfo.Data.StateMainWindows == eStateMainWindows.WaitAdmin && CommandRemoteInfo.Data.TypeAccess == eTypeAccess.ChoicePrice)
                        {
                            Bl.AddEventAge(MW.curReceipt);
                            AddWares(MW.CurWares.CodeWares, MW.CurWares.CodeUnit, CommandRemoteInfo.Data.QuantityCigarettes, CommandRemoteInfo.Data.SelectRemoteCigarettesPrice.price);
                            MW.QuantityCigarettes = 1;
                            SetStateView(eStateMainWindows.WaitInput);
                        }
                        Res = new global::UtilNetwork.Result(0, $"{CommandRemoteInfo.Data.StateMainWindows} {CommandRemoteInfo.Data.TypeAccess}");
                        break;
                    case eCommand.DeleteReceipt:
                        if (MW.curReceipt != null && MW.curReceipt.StateReceipt == eStateReceipt.Prepare)
                            Bl.SetStateReceipt(MW.curReceipt, eStateReceipt.Canceled);

                        SetCurReceipt(null);
                        SetStateView(eStateMainWindows.StartWindow);
                        Res = new global::UtilNetwork.Result(0, $"Чек видалено!");
                        break;
                }
            }
            catch (Exception ex) { Res = new global::UtilNetwork.Result(ex); }
            return Res;
        }


        public void SendRemoteComand(eCommand comand, InfoRemoteCheckout remoteInfo, string LogText = "Confirm")
        {
            if (MW.RemoteWorkplace != null)
                Task.Run(async () =>
                {
                    CommandAPI<InfoRemoteCheckout> Command = new() { Command = comand, Data = remoteInfo };
                    try
                    {
                        var r = new SocketClient(MW.RemoteWorkplace.IP, Global.PortAPI);
                        var Ansver = await r.StartAsync(Command.ToJSON());
                        MW.SocketAnsver?.Invoke(comand, MW.MainWorkplace, Ansver);
                    }
                    catch (Exception ex)
                    {
                        FileLogger.WriteLogMessage(this, $"{LogText} DNSName=>{MW.RemoteWorkplace.DNSName} {Command} ", ex);
                        MW.SocketAnsver?.Invoke(comand, MW.MainWorkplace, new global::UtilNetwork.Result(ex));
                    }
                });
        }


        public bool SetConfirm(User pUser, bool pIsFirst = false, bool pIsAccess = false)
        {
            if (pUser == null)
            {
                switch (MW.TypeAccessWait)
                {
                    case eTypeAccess.DelReciept:
                    case eTypeAccess.DelWares:
                        MW.TypeAccessWait = eTypeAccess.NoDefine;
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
            if (MW.TypeAccessWait == eTypeAccess.FixWeight)
            {
                MW.IsConfirmAdmin = Access.GetRight(pUser, eTypeAccess.FixWeight);
                if (MW.State == eStateMainWindows.BlockWeight)
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.FixWeight);
            }
            else
              if (MW.TypeAccessWait == eTypeAccess.ExciseStamp)
                MW.IsConfirmAdmin = Access.GetRight(pUser, eTypeAccess.ExciseStamp);

            if (MW.TypeAccessWait == eTypeAccess.NoDefine || MW.TypeAccessWait < 0)
                return false;
            if (!Access.GetRight(pUser, MW.TypeAccessWait) && !pIsAccess)
            {
                if (!pIsFirst)
                    Global.Message?.Invoke($"Не достатньо прав для операції {MW.TypeAccessWait} {Environment.NewLine}в {pUser.NameUser} з правами {pUser.TypeUser}", eTypeMessage.Error);              
                return false;
            }
            VR.SendMessage(Global.IdWorkPlace, $"{MW.TypeAccessWait} =>{pUser.NameUser}", 0, 0, MW.curReceipt?.SumTotal ?? 0, VR.eTypeVRMessage.Confirm);

            Bl.db.InsertReceiptEvent(
                new(MW.curReceipt)
                {
                    EventType = eReceiptEventType.Other,
                    EventName = MW.TypeAccessWait.ToString(),
                    UserName = pUser.CodeUser.ToString(),
                    CodeWares = MW.CurWares?.CodeWares ?? 0
                });
            switch (MW.TypeAccessWait)
            {
                case eTypeAccess.DelWares:
                    if (MW.curReceipt?.IsLockChange == false)
                    {
                        Bl.ChangeQuantity(MW.CurWares, 0, pUser);
                        MW.CurWares = null;//.Quantity = 0;
                        MW.TypeAccessWait = eTypeAccess.NoDefine;
                        SetStateView(eStateMainWindows.WaitInput);
                    }
                    break;
                case eTypeAccess.DelReciept:
                    VR.SendMessage(Global.IdWorkPlace, $"{MW.TypeAccessWait} => {pUser.NameUser}", 0, 0, MW.curReceipt?.SumTotal ?? 0, VR.eTypeVRMessage.DelReceipt);
                    Bl.SetStateReceipt(MW.curReceipt, eStateReceipt.Canceled);
                    SetCurReceipt(null);
                    MW.TypeAccessWait = eTypeAccess.NoDefine;
                    SetStateView(eStateMainWindows.StartWindow);
                    break;
                case eTypeAccess.ReturnReceipt:
                    Bl.CreateRefund(MW.AC.curReceipt, MW.IsFullReturn);
                    SetStateView(eStateMainWindows.WaitInputRefund);
                    break;

                case eTypeAccess.ConfirmAge:
                    Bl.AddEventAge(MW.curReceipt);
                    MW.TypeAccessWait = eTypeAccess.NoDefine;
                    PayAndPrint();
                    break;
                case eTypeAccess.ChoicePrice:
                    foreach (Pr el in MW.OCPrices)
                    {
                        el.IsEnable = true;
                        el.IsConfirmAge = true;
                    }
                    MW.TypeAccessWait = eTypeAccess.NoDefine;
                    break;
                case eTypeAccess.AddNewWeight:
                case eTypeAccess.FixWeight:
                    //SetStateView(eStateMainWindows.WaitAdmin);
                    break;
                case eTypeAccess.ExciseStamp:
                    MW.TypeAccessWait = eTypeAccess.NoDefine;
                    SetStateView(eStateMainWindows.WaitAdmin);
                    break;
                case eTypeAccess.AdminPanel:
                    MW.TypeAccessWait = eTypeAccess.NoDefine;
                    ShowAdmin(pUser);
                    break;
                case eTypeAccess.UseBonus:
                    var task = Task.Run(() => PrintAndCloseReceipt(null, eTypePay.Bonus, 0, 0, 0, MW.Client?.SumMoneyBonus ?? 0m));
                    break;
            }
            return true;
        }

        public void ShowAdmin(User pUser)
        {
            MW.AC.Init(pUser);
            SetStateView(eStateMainWindows.AdminPanel);
        }


    }
}
