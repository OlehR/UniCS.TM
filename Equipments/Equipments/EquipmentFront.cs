using Front.Equipments;
//using Front.Equipments.Ingenico;
//using Front.Equipments.pRRO_SG;
using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Front.Equipments.Implementation;
using System.Threading.Tasks;
using Utils;
using Microsoft.Extensions.Logging;
using SharedLib;
using System.Threading;
using System.Collections;
using System.Windows;

namespace Front
{
    public class EquipmentFront
    {
        #region Init
        public bool IsFinishInit = false;
        public Action<double, bool> OnControlWeight { get; set; }
        public Action<double, bool> OnWeight { get; set; }
        public Action<string, string> OnBarCode { get; set; }

        private IEnumerable<Equipment> ListEquipment = new List<Equipment>();
        //eStateEquipment _State = eStateEquipment.Off;
        readonly BL Bl = BL.GetBL;
        Scaner Scaner;
        Scale Scale;
        Printer Printer;
        public CashMachine CashMachine;
        public Scale ControlScale;
        SignalFlag Signal;
        BankTerminal Terminal;
        ScanerKeyBoard KB;
        //Rro RRO;
        SortedList<int, Rro> RROs = new();
        IEnumerable<Rro> GetRROs(int pIdWorkplace)
        {
            return RROs.Where(el => el.Key >= pIdWorkplace * 100 && el.Key < pIdWorkplace * 100 + 99).Select(el => el.Value);
        }
        Rro GetRRO(Receipt pR) { return GetRRO(pR, null, pR.TypePay); }
        Rro GetRRO(IdReceipt pIdR, Rro pRRO = null, eTypePay pTypePay = eTypePay.None)
        {
            if (pRRO != null)
            {
                pIdR.IdWorkplacePay = pRRO.IdWorkplacePay;
                return pRRO;
            }
            if (RROs == null)
                return null;
            Rro Res = null;
            int Key = pIdR.IdWorkplacePay * 100 + (int)pTypePay;
            if (RROs.ContainsKey(Key))
                Res = RROs[Key];
            else
            {
                if (pTypePay != eTypePay.None)
                {
                    Key = pIdR.IdWorkplacePay * 100;
                    if (RROs.ContainsKey(Key))
                        Res = RROs[Key];
                }
            }
            return Res;
        }
        /// <summary>
        /// Текуча операція РРО для блокування операцій.
        /// </summary>
        //eTypeOperation curTypeOperation = eTypeOperation.NotDefine;
        /// <summary>
        /// Для віртуальної ваги куди йде подія.
        /// </summary>
        public bool IsControlScale = true;
        public IEnumerable<Equipment> GetBankTerminal { get { return ListEquipment.Where(e => e.Type == eTypeEquipment.BankTerminal && e.State == eStateEquipment.On); } }
        public void SetBankTerminal(BankTerminal pBT) { Terminal = pBT; }
        public int CountTerminal { get { return GetBankTerminal.Count(); } }
        public BankTerminal BankTerminal1 { get { return GetBankTerminal?.FirstOrDefault() as BankTerminal; } }
        public BankTerminal BankTerminal2 { get { return GetBankTerminal?.Skip(1).FirstOrDefault() as BankTerminal; } }

        public void ControlWeight(double pWeight, bool pIsStable) => OnControlWeight?.Invoke(pWeight, pIsStable);
        public void Weight(double pWeight, bool pIsStable) => OnWeight?.Invoke(pWeight, pIsStable);
        void BarCode(string pBarCode, string pType) => OnBarCode?.Invoke(pBarCode, pType);

        public eStateEquipment StatCriticalEquipment
        {
            get
            {
                var Res = ListEquipment.Where(el => !(el.State == eStateEquipment.On || el.State == eStateEquipment.Init || el.State == eStateEquipment.Off) && el.IsСritical);
                var aa = Res != null && Res.Any() ? eStateEquipment.Error : eStateEquipment.On;
                return aa;
            }
        }

        public eStateEquipment State
        {
            get
            {
                return ListEquipment.Where(el => el.IsСritical == true).Max(el => el.State);
            }
        }

        public Action<StatusEquipment> SetStatus { get; set; }

        static EquipmentFront sEquipmentFront;

        public static EquipmentFront GetEquipmentFront { get { return sEquipmentFront; } }

        readonly ILoggerFactory LF = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        static EquipmentFront sEF = null;
        public static EquipmentFront GetEF { get { return sEF ?? new EquipmentFront(); } }
        public EquipmentFront()
        {
            sEF = this;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            sEquipmentFront = this;
            //OnControlWeight += (pWeight, pIsStable) => { pSetControlWeight?.Invoke(pWeight, pIsStable); };                

            OnWeight += (pWeight, pIsStable) => { if (IsControlScale && ControlScale?.Model == eModelEquipment.VirtualControlScale) OnControlWeight?.Invoke(pWeight, pIsStable); };
            Task.Run(() => Init());
        }

        public void Init()
        {
            using ILoggerFactory loggerFactory =
           LoggerFactory.Create(builder =>
               builder.AddSimpleConsole(options =>
               {
                   options.IncludeScopes = true;
                   options.SingleLine = true;
                   options.TimestampFormat = "hh:mm:ss ";
               }));

            var NewListEquipment = new List<Equipment>();
            var config = GetConfig();
            Equipment ElEquipment;
            //State = eStateEquipment.Init;
            try
            {
                //Scaner
                Scaner Sc;
                foreach (var el in ListEquipment.Where(e => e.Type == eTypeEquipment.Scaner))
                {
                    try
                    {
                        switch (el.Model)
                        {
                            case eModelEquipment.MagellanScaner:
                                Sc = new MagellanScaner(el, config, LF, BarCode);
                                Scaner = Sc;
                                break;
                            case eModelEquipment.ZebraScaner:
                                Sc = new ZebraScaner(el, config, LF, BarCode);
                                Scaner = Sc; // потрібно для того, щоб працювало пікання при скануванні
                                break;
                            case eModelEquipment.VirtualScaner:
                                Sc = new VirtualScaner(el, config, LF, BarCode);
                                break;
                            case eModelEquipment.ScanerCom:
                                Sc = new ScanerCom(el, config, LF, BarCode);
                                break;
                            case eModelEquipment.ScanerKeyBoard:
                                Sc = KB = new ScanerKeyBoard(el, config, LF, BarCode);
                                break;
                            default:
                                Sc = new Scaner(el, config);
                                break;
                        }
                        NewListEquipment.Add(Sc);

                    }
                    catch (Exception e)
                    {
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + "\\ControlScale", e);
                    }
                }

                //Scale
                try
                {
                    ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Scale)?.First();
                    switch (ElEquipment.Model)
                    {
                        case eModelEquipment.MagellanScale:
                            Scale = new MagellanScale(((MagellanScaner)Scaner), Weight);//TMP!!! OnControlWeight - Нафіг                        
                            break;
                        case eModelEquipment.ZebraScale:
                            Scale = new ZebraScale(ElEquipment, config, LF, Weight);
                            break;
                        case eModelEquipment.VirtualScale:
                            Scale = new VirtualScale(ElEquipment, config, LF, Weight);
                            break;
                        case eModelEquipment.ScaleCom:
                            Scale = new ScaleCom(ElEquipment, config, LF, Weight);
                            // Scale.StartWeight();
                            break;
                        default:
                            Scale = new Scale(ElEquipment, config);
                            break;
                    }
                    //Scale.StartWeight();
                    NewListEquipment.Add(Scale);
                }
                catch { }
                ;
                //ControlScale
                IEnumerable<Equipment> Equipments;
                try
                {
                    Equipments = ListEquipment.Where(e => e.Type == eTypeEquipment.ControlScale);
                    if (Equipments.Any())
                    {
                        ElEquipment = Equipments.First();
                        switch (ElEquipment.Model)
                        {
                            case eModelEquipment.ScaleModern:
                                ControlScale = new ScaleModern(ElEquipment, config, LF, ControlWeight);
                                break;
                            case eModelEquipment.VirtualControlScale:
                                ControlScale = new VirtualControlScale(ElEquipment, config, LF, null);
                                Scale?.StartWeight();
                                break;
                            default:
                                ControlScale = new Scale(ElEquipment, config);
                                //NewListEquipment.Add(ControlScale);
                                break;
                        }
                        NewListEquipment.Add(ControlScale);
                    }
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + "\\ControlScale", e);
                }

                //Flag
                try
                {
                    Equipments = ListEquipment.Where(e => e.Type == eTypeEquipment.Signal);
                    if (Equipments.Any())
                    {
                        ElEquipment = Equipments.First();
                        if (ElEquipment.Model == eModelEquipment.SignalFlagModern)
                            Signal = new SignalFlagModern(ElEquipment, config);
                        else
                            Signal = new SignalFlag(ElEquipment, config);
                        NewListEquipment.Add(Signal);
                    }
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + "\\Flag", e);
                }

                //Bank Pos Terminal
                foreach (var el in ListEquipment.Where(e => e.Type == eTypeEquipment.BankTerminal))
                {
                    try
                    {
                        ElEquipment = el;

                        switch (ElEquipment.Model)
                        {
                            case eModelEquipment.Ingenico:
                                Terminal = new BT_Ingenico(el, config, LF, PosStatus);
                                break;
                            case eModelEquipment.BT_Android:
                                Terminal = new BT_Android(el, config, LF, PosStatus);
                                break;
                            case eModelEquipment.VirtualBankPOS:
                                Terminal = new VirtualBankPOS(el, config, LF, PosStatus);
                                break;
                            default:
                                Terminal = new BankTerminal(el, config);
                                break;
                        }
                        NewListEquipment.Add(Terminal);
                    }
                    catch (Exception e)
                    {
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + "\\BankPos", e);
                    }
                }
                //Принтер
                try
                {
                    ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Printer)?.FirstOrDefault();
                    if (ElEquipment != null)
                        switch (ElEquipment.Model)
                        {
                            case eModelEquipment.Printer_Sam4sGcube102:
                                Printer = new Printer_Sam4sGcube102(ElEquipment, config, LF);
                                break;
                            default:
                                Printer = new Printer(ElEquipment, config);
                                break;
                        }
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + "\\Принтер", e);
                }
                //Кеш-машина
                try
                {
                    ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.CashMachine)?.FirstOrDefault();
                    if (ElEquipment != null)
                    {
                        switch (ElEquipment.Model)
                        {
                            case eModelEquipment.GloryCash:
                                CashMachine = new GloryCash(ElEquipment, config, LF);
                                break;
                            default:
                                CashMachine = new GloryCash(ElEquipment, config);
                                break;
                        }
                        NewListEquipment.Add(CashMachine);
                    }
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + "\\Кеш-машина", e);
                }
                //RRO               
                foreach (var el in ListEquipment.Where(e => e.Type == eTypeEquipment.RRO))
                {
                    try
                    {
                        Rro RRO;
                        switch (el.Model)
                        {
                            case eModelEquipment.ExellioFP:
                                RRO = new Equipments.RRO_ExellioFP(el, config, LF);
                                break;
                            case eModelEquipment.pRRO_SG:
                                RRO = new Front.Equipments.pRRO_SG.pRRO_SG(el, config, LF);
                                break;
                            case eModelEquipment.pRRo_WebCheck:
                                RRO = new pRRO_WebCheck(el, config, LF);
                                RRO.Init();
                                break;
                            case eModelEquipment.RRO_Maria:
                                RRO = new RRO_Maria(el, config, LF);
                                break;
                            case eModelEquipment.VirtualRRO:
                                RRO = new VirtualRRO(el, config, LF, null, Printer, this);
                                break;
                            case eModelEquipment.RRO_FP700:
                                RRO = new RRO_FP700(el, config, LF);
                                break;
                            case eModelEquipment.pRRO_Vchasno:
                                RRO = new pRRO_Vchasno(el, config, LF);
                                break;
                            default:
                                RRO = new Rro(el, config);
                                break;
                        }
                        NewListEquipment.Add(RRO);
                        RROs.Add(RRO.IdWorkplacePay * 100 + (int)RRO.TypePay, RRO);
                    }
                    catch (Exception e)
                    {
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + "\\RRO", e);
                    }
                }

                ListEquipment = NewListEquipment;

                //Передаємо зміни станусів наверх.
                foreach (var el in NewListEquipment)
                {
                    el.ActionStatus += (Status) => { SetStatus?.Invoke(Status); };
                }

                //State = eStateEquipment.On;
                SetStatus?.Invoke(new StatusEquipment(eModelEquipment.NotDefine, eStateEquipment.On));
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                //State = eStateEquipment.Error;
            }
            finally { IsFinishInit = true; }
        }

        public IConfiguration GetConfig()
        {
            var AppConfiguration = Config.AppConfiguration;
            AppConfiguration.GetSection("MID:Equipment").Bind(ListEquipment);
            //var r = AppConfiguration.GetSection("MID:Equipment");
            return AppConfiguration;
        }

        public IEnumerable<Equipment> GetListEquipment { get { return ListEquipment; } }
        #endregion

        #region RRO

        LogRRO WaitRRO(Rro pRRO, IdReceipt pReceipt, eTypeOperation pTypeOperation, int pMilisecond = 500, bool pIsStop = true)
        {
            if (pRRO == null)
                return new LogRRO(pReceipt) { TypeOperation = pTypeOperation, CodeError = -1, Error = $"Не знайдено RRO для IdWorkplacePay={pReceipt?.IdWorkplacePay}" };

            LogRRO Res = null;

            lock (pRRO)
            {
                //var RRO = GetRRO(pReceipt.IdWorkplacePay);
                try
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Start pRRO=> {pRRO.GetHashCode()} TypeOperation=>{pTypeOperation} pMilisecond={pMilisecond}");

                    while (pMilisecond > 0 && pRRO.TypeOperation != eTypeOperation.NotDefine)
                    {
                        if (pIsStop)
                            pRRO?.Stop();
                        Thread.Sleep(50);
                        pMilisecond -= 50;
                    }
                    if (pRRO.TypeOperation == eTypeOperation.NotDefine)
                    {
                        pRRO.TypeOperation = pTypeOperation;
                        pRRO.LockDT = DateTime.Now;
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Finish pRRO=> {pRRO.GetHashCode()} TypeOperation=>{pTypeOperation} pMilisecond={pMilisecond}");
                    }
                    else
                    {
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"pRRO=> {pRRO.GetHashCode()} LastTypeOperation=>{pRRO.TypeOperation} LockDT=> {pRRO.LockDT} TryTypeOperation=>{pTypeOperation} pMilisecond={pMilisecond}", eTypeLog.Error);
                        Res = new LogRRO(pReceipt) { TypeOperation = pTypeOperation, TypeRRO = pRRO.Model.ToString(), CodeError = -1, Error = $"Не вдалось виконати текучу операцію {pTypeOperation} на RRO{Environment.NewLine}Оскільки не виконалась попередня: LastTypeOperation=>{pRRO.TypeOperation} LockDT=> {pRRO.LockDT}" };
                    }
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                }
            }
            return Res;
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        public LogRRO PrintReceipt(Receipt pReceipt)
        {
            var aa = pReceipt.Payment.ToList();
            string NameMetod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            if (pReceipt == null || pReceipt.Payment == null || !pReceipt.Payment.Any(el => el.TypePay == eTypePay.Card || el.TypePay == eTypePay.Cash))
                return new LogRRO(pReceipt) { CodeError = -1, Error = $"Відсутня оплата по Робочому місцю № ({pReceipt.IdWorkplacePay})" };
            var r = Task.Run<LogRRO>((Func<LogRRO>)(() =>
            {
                var RRO = GetRRO(pReceipt);
                LogRRO Res;
                try
                {
                    Res = WaitRRO(RRO, pReceipt, eTypeOperation.Sale);
                    if (Res == null)
                    {
                        FileLogger.WriteLogMessage(this, NameMetod, "Start Print Receipt");
                        if (pReceipt?.TypeReceipt == eTypeReceipt.Refund && pReceipt.Wares != null)
                        {
                            var rr = pReceipt.Wares.Where(r => r.Quantity != 0m);
                            pReceipt.Wares = rr;
                        }
                        Res = RRO?.PrintReceipt(pReceipt);
                        var Log = pReceipt.LogRROs?.ToList() ?? new List<LogRRO>();
                        Log.Add(Res);
                        pReceipt.LogRROs = Log;
                        FileLogger.WriteLogMessage(this, NameMetod, "End Print Receipt");
                    }
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, NameMetod, e);
                    if (RRO != null) RRO.State = eStateEquipment.Error;
                    SetStatus?.Invoke(new StatusEquipment(RRO.Model, eStateEquipment.Error, e.Message) { IsСritical = true });
                    Res = new LogRRO(pReceipt) { TypeOperation = eTypeOperation.Sale, TypeRRO = RRO.Model.ToString(), CodeError = -1, Error = e.Message };
                }
                finally
                {
                    if (RRO != null)
                        RRO.TypeOperation = eTypeOperation.NotDefine;
                }
                Res.IdWorkplacePay = pReceipt.IdWorkplacePay;
                return Res;
            })).Result;
            if (r.CodeError == 0 && pReceipt.TypeReceipt == eTypeReceipt.Sale && pReceipt.IdWorkplacePay == Global.IdWorkPlace)
                PrintQR(pReceipt);


            GetLastReceipt(new(pReceipt), r);
            return r;
        }


        void GetLastReceipt(IdReceipt pIdR, LogRRO r, bool IsZReport = false)
        {
            Task.Run(() =>
           {
               try
               {
                   var RRO = GetRRO(pIdR);
                   if (r.CodeError == 0)
                   {
                       if (string.IsNullOrEmpty(r.TextReceipt))//|| RRO.Model == eModelEquipment.RRO_FP700)
                       {
                           LogRRO Res;
                           try
                           {
                               Res = WaitRRO(RRO, pIdR, eTypeOperation.LastReceipt, 500, false);
                               if (Res == null)
                               {
                                   r.TextReceipt = RRO.GetTextLastReceipt(IsZReport);
                                   if (r.SUM == 0)
                                       r.SUM = RRO.GetSumFromTextReceipt(r.TextReceipt);
                               }
                           }
                           catch (Exception e)
                           {
                               FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                           }
                           finally
                           {
                               if (RRO != null)
                                   RRO.TypeOperation = eTypeOperation.NotDefine;
                           }
                       }
                   }
                   Bl.InsertLogRRO(r);
               }
               catch (Exception e)
               {
                   FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
               }
           });
        }



        public LogRRO RroPrintX(IdReceipt pIdR, Rro pRRO = null)
        {

            var r = Task.Run<LogRRO>((Func<LogRRO>)(() =>
            {
                var RRO = GetRRO(pIdR, pRRO);
                LogRRO Res;


                try
                {
                    Res = WaitRRO(RRO, pIdR, eTypeOperation.XReport);
                    if (Res == null)
                        Res = RRO?.PrintX(pIdR);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    SetStatus?.Invoke(new StatusEquipment(RRO.Model, eStateEquipment.Error, e.Message) { IsСritical = true });
                    Res = new LogRRO(pIdR) { TypeOperation = eTypeOperation.XReport, TypeRRO = RRO.Model.ToString(), CodeError = -1, Error = e.Message };
                }
                finally
                {
                    if (RRO != null)
                        RRO.TypeOperation = eTypeOperation.NotDefine;
                }
                return Res;
            }
            )).Result;
            GetLastReceipt(pIdR, r);
            return r;
        }

        public LogRRO RroPrintZ(IdReceipt pIdR, Rro pRRO = null)
        {
            var r = Task.Run<LogRRO>((Func<LogRRO>)(() =>
            {
                var RRO = GetRRO(pIdR, pRRO);
                LogRRO Res;

                try
                {
                    Res = WaitRRO(RRO, pIdR, eTypeOperation.ZReport);
                    if (Res == null)
                        Res = RRO?.PrintZ(pIdR);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    SetStatus?.Invoke(new StatusEquipment(RRO.Model, eStateEquipment.Error, e.Message) { IsСritical = true });
                    Res = new LogRRO(pIdR) { TypeOperation = eTypeOperation.ZReport, TypeRRO = RRO.Model.ToString(), CodeError = -1, Error = e.Message };
                }
                finally
                {
                    if (RRO != null)
                        RRO.TypeOperation = eTypeOperation.NotDefine;
                }
                return Res;
            }
            )).Result;
            GetLastReceipt(pIdR, r, true);
            return r;
        }

        public LogRRO RroPeriodZReport(IdReceipt pIdR, DateTime pBegin, DateTime pEnd, bool IsFull = true, Rro pRRO = null)
        {
            var r = Task.Run<LogRRO>((Func<LogRRO>)(() =>
            {
                var RRO = GetRRO(pIdR, pRRO);
                LogRRO Res;

                try
                {
                    Res = WaitRRO(RRO, pIdR, eTypeOperation.PeriodZReport);
                    if (Res == null)
                        RRO?.PeriodZReport(pIdR, pBegin, pEnd, IsFull);
                    Res = new LogRRO(pIdR) { TypeOperation = eTypeOperation.PeriodZReport, TypeRRO = RRO.Model.ToString(), TextReceipt = $"{pBegin} {pEnd} {IsFull}" };
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    SetStatus?.Invoke(new StatusEquipment(RRO.Model, eStateEquipment.Error, e.Message) { IsСritical = true });
                    Res = new LogRRO(pIdR) { TypeOperation = eTypeOperation.PeriodZReport, TypeRRO = RRO.Model.ToString(), CodeError = -1, Error = e.Message };
                }
                finally
                {
                    if (RRO != null)
                        RRO.TypeOperation = eTypeOperation.NotDefine;
                }
                return Res;
            })).Result;
            return r;
        }

        public LogRRO RroPrintCopyReceipt(Rro pRRO = null)
        {
            var r = pRRO?.PrintCopyReceipt();
            Bl.InsertLogRRO(r);
            return r;
        }

        public decimal SumReceiptFiscal(Receipt pR)
        {
            if (pR == null || pR.Wares == null || !pR.Wares.Any()) return 0;
            var RRO = GetRRO(pR);
            decimal sum = 0;
            sum = RRO != null ? RRO.SumReceiptFiscal(pR) : pR.Wares.Sum(r => (r.SumTotal));
            return sum;
        }

        public LogRRO RroMoveMoney(decimal pSum, IdReceipt pIdR, Rro pRRO = null)
        {

            var r = Task.Run<LogRRO>((Func<LogRRO>)(() =>
            {
                var RRO = GetRRO(pIdR, pRRO);
                LogRRO Res;

                try
                {
                    Res = WaitRRO(RRO, pIdR, pSum > 0 ? eTypeOperation.MoneyIn : eTypeOperation.MoneyOut);
                    if (Res == null)
                        Res = RRO?.MoveMoney(pSum, pIdR);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    SetStatus?.Invoke(new StatusEquipment(RRO.Model, eStateEquipment.Error, e.Message) { IsСritical = true });
                    Res = new LogRRO(pIdR) { TypeOperation = pSum > 0 ? eTypeOperation.MoneyIn : eTypeOperation.MoneyOut, SUM = pSum, TypeRRO = RRO.Model.ToString(), CodeError = -1, Error = e.Message };
                }
                finally
                {
                    if (RRO != null)
                        RRO.TypeOperation = eTypeOperation.NotDefine;
                }
                return Res;
            }
            )).Result;
            GetLastReceipt(pIdR, r);
            return r;
        }


        public LogRRO IssueOfCash(Receipt pReceipt)
        {
            string NameMetod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            var r = Task.Run<LogRRO>((Func<LogRRO>)(() =>
            {
                var RRO = GetRRO(pReceipt);
                LogRRO Res;

                try
                {
                    Res = WaitRRO(RRO, pReceipt, eTypeOperation.Sale);
                    if (Res == null)
                    {

                        Res = RRO?.IssueOfCash(pReceipt);
                        var Log = pReceipt.LogRROs?.ToList() ?? new List<LogRRO>();
                        Log.Add(Res);
                        pReceipt.LogRROs = Log;
                    }
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, NameMetod, e);
                    if (RRO != null) RRO.State = eStateEquipment.Error;
                    SetStatus?.Invoke(new StatusEquipment(RRO.Model, eStateEquipment.Error, e.Message) { IsСritical = true });
                    Res = new LogRRO(pReceipt) { TypeOperation = eTypeOperation.IssueOfCash, TypeRRO = RRO.Model.ToString(), CodeError = -1, Error = e.Message };
                }
                finally
                {
                    if (RRO != null)
                        RRO.TypeOperation = eTypeOperation.NotDefine;
                }
                Res.IdWorkplacePay = pReceipt.IdWorkplacePay;
                return Res;
            })).Result;

            GetLastReceipt(new(pReceipt), r);
            return r;
        }

        public bool OpenMoneyBox(int pTime = 15)
        {
            var RRO = GetRRO(new IdReceipt() { IdWorkplacePay = Global.IdWorkPlace });
            return RRO?.OpenMoneyBox(pTime) ?? false;
        }
        /// <summary>
        /// Розрахунок суми готівкуою з врахуванням завкруглення фіскалки
        /// </summary>
        /// <param name="pR"></param>
        /// <param name="pIdWorkplacePay"></param>
        /// <returns></returns>
        public decimal SumCashReceiptFiscal(Receipt pR)
        {
            decimal Sum = 0;
            var Id = pR.IdWorkplacePay;
            int[] IdWorkplacePays;
            if (pR.IdWorkplacePay > 0)
                IdWorkplacePays = new int[1] { pR.IdWorkplacePay };
            else
                IdWorkplacePays = pR.IdWorkplacePays;

            for (var i = 0; i < IdWorkplacePays.Length; i++)
            {
                if (IdWorkplacePays[i] == pR.IdWorkplacePay || pR.IdWorkplacePay > 0)
                {
                    pR.IdWorkplacePay = IdWorkplacePays[i];
                    var RRO = GetRRO(pR);
                    Sum += RRO?.SumCashReceiptFiscal(pR) ?? 0;
                }
            }
            pR.IdWorkplacePay = Id;
            return Sum;
        }

        public LogRRO PrintNoFiscalReceipt(IdReceipt pReceipt, IEnumerable<string> pR)
        {
            if (pR != null && pR.Any())
            {
                var r = Task.Run<LogRRO>((Func<LogRRO>)(() =>
                {
                    LogRRO Res;

                    if (Printer != null)
                    {
                        bool r = Printer.Print(pR);
                        Res = new LogRRO() { TypeOperation = eTypeOperation.NoFiscalReceipt, TypeRRO = Printer.Model.ToString(), CodeError = r ? 0 : -1 };
                    }
                    else
                    {
                        var RRO = GetRRO(pReceipt);

                        try
                        {
                            Res = WaitRRO(RRO, pReceipt, eTypeOperation.NoFiscalReceipt);
                            if (Res == null)
                                Res = RRO?.PrintNoFiscalReceipt(pR);
                            Bl.InsertLogRRO(Res);
                        }
                        catch (Exception e)
                        {
                            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                            SetStatus?.Invoke(new StatusEquipment(RRO.Model, eStateEquipment.Error, e.Message) { IsСritical = false });
                            Res = new LogRRO() { TypeOperation = eTypeOperation.NoFiscalReceipt, TypeRRO = RRO?.Model.ToString(), CodeError = -1, Error = e.Message };
                        }

                        finally
                        {
                            if (RRO != null)
                                RRO.TypeOperation = eTypeOperation.NotDefine;
                        }
                    }
                    return Res;
                }
            )).Result;
                return r;
            }
            return null;
        }

        public void PrintQR(IdReceipt pIdR)
        {
            var QR = Bl.GetQR(pIdR);
            if (QR != null && QR.Any())
            {
                foreach (var el in QR)
                {
                    foreach (string elQr in el.Qr.Split(","))
                    {
                        List<string> list = new List<string>() { el.Name, $"QR=>{elQr}" };
                        PrintNoFiscalReceipt(pIdR, list);
                    }
                }
            }
        }

        public void ProgramingArticleAsync(ReceiptWares pRW)
        {
            Task.Run(() =>
            {

                var RRO = GetRRO(pRW);
                if (RRO == null)
                    return false;
                LogRRO Res;
                try
                {
                    Res = WaitRRO(RRO, pRW, eTypeOperation.ProgramingArticle);
                    if (Res == null)
                    {
                        RRO.ProgramingArticle(pRW);
                        RRO.PutToDisplay($"{pRW.NameWaresReceipt}{Environment.NewLine}{pRW.Quantity}x{pRW.Price}={pRW.SumTotal}", 0);
                    }
                }
                catch (Exception e)
                {
                    if (RRO != null)
                        RRO.State = eStateEquipment.Error;
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    //SetStatus?.Invoke(new StatusEquipment(RRO.Model, eStateEquipment.Error, e.Message) { IsСritical = true });
                }
                finally
                {
                    if (RRO != null)
                        RRO.TypeOperation = eTypeOperation.NotDefine;
                }
                return true;
            }
            );
        }

        /// <summary>
        /// Сума готівки з РРО
        /// </summary>
        /// <param name="pIdR"></param>
        /// <returns></returns>
        public decimal GetSumInCash(IdReceipt pIdR)
        {
            var RRO = GetRRO(pIdR, null, eTypePay.Cash);
            LogRRO Res;

            decimal Sum = -1;
            try
            {
                Res = WaitRRO(RRO, pIdR, eTypeOperation.SumInCash);
                if (Res == null)
                    Sum = RRO?.GetSumInCash(pIdR) ?? -1;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            finally
            {
                if (RRO != null)
                    RRO.TypeOperation = eTypeOperation.NotDefine;
            }
            return Sum;
        }

        public void PutToDisplay(IdReceipt pIdR, string pText = null, int pLine = 1)
        {
            //return;//!!!!TMP  Треба придумати щось щоб не було проблем
            Task.Run(() =>
            {
                var RRO = GetRRO(pIdR);
                LogRRO Res;
                decimal Sum = -1;
                try
                {
                    Res = WaitRRO(RRO, pIdR, eTypeOperation.PutToDisplay, 0, false);
                    if (Res == null)
                        RRO?.PutToDisplay(pText, pLine);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                }
                finally
                {
                    if (RRO != null)
                        RRO.TypeOperation = eTypeOperation.NotDefine;
                }
            });
        }
        #endregion
        /// <summary>
        /// Друк чека на (термо)принтері
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        public bool Print(Receipt pR)
        {
            return Printer?.PrintReceipt(pR) ?? true;
        }

        #region POS        

        /// <summary>
        /// Повернення покупцю грошей по банківському терміналу
        /// </summary>
        /// <param name="pSum"></param>
        /// <param name="pRNN"></param>
        /// <returns></returns>
        public Payment PosPay(IdReceipt pIdR, decimal pSum, string pRNN, Payment pP = null, decimal pIssuingCash = 0)
        {
            Payment r = null;
            try
            {
                if (pP != null)
                    r = pP;
                else
                {
                    if (pSum < 0)
                        r = Terminal?.Refund(-pSum, pRNN, pIdR.IdWorkplacePay);
                    else
                        r = Terminal?.Purchase(pSum, pIssuingCash, pIdR.IdWorkplacePay);
                    r.SetIdReceipt(pIdR);
                }
                if (r.IsSuccess)
                {
                    var LP = new List<Payment>() { r };
                    if (pIssuingCash > 0)
                    {
                        r.SumPay -= pIssuingCash;
                        Payment C = (Payment)r.Clone();
                        C.TypePay = eTypePay.IssueOfCash;
                        C.SumPay = pIssuingCash;
                        C.SumExt = pIssuingCash;
                        LP.Add(C);
                    }

                    Bl.db.ReplacePayments(LP);

                    Task.Run(() =>
                    {
                        if (r.Receipt == null || !r.Receipt.Any())
                            r.Receipt = Terminal?.GetLastReceipt();
                        LogRRO d = new(pIdR)
                        { TypeOperation = pSum > 0 ? eTypeOperation.SalePOS : eTypeOperation.RefundPOS, TypeRRO = r.TypePay == eTypePay.Card ? "Ingenico" : "Cash", JSON = r.ToJSON(), TextReceipt = r.Receipt == null ? null : string.Join(Environment.NewLine, r.Receipt) };
                        Bl.InsertLogRRO(d);
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"(pIdR=>{pIdR.ToJSON()},pSum={pSum})=>{r.ToJSON()}", eTypeLog.Expanded);

                    });
                }
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return r;
        }

        public BatchTotals PosPrintX(IdReceipt IdR, bool IsPrint = true)
        {
            //var IdR = new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod() };
            BatchTotals r = null;
            try
            {
                r = Terminal.PrintX(IdR.IdWorkplacePay);
                LogRRO d = new()
                { TypeOperation = eTypeOperation.XReportPOS, TypeRRO = "Ingenico", JSON = r.ToJSON(), TextReceipt = r.Receipt == null ? null : string.Join(Environment.NewLine, r.Receipt) };
                Bl.InsertLogRRO(d);
                if (IsPrint && r.Receipt != null)
                    PrintNoFiscalReceipt(IdR, r.Receipt);
                return r;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return r;
        }

        public BatchTotals PosPrintZ(IdReceipt IdR)
        {
            BatchTotals r = null;
            //var IdR = new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod() };
            try
            {
                r = Terminal.PrintZ(IdR.IdWorkplacePay);
                LogRRO d = new(IdR)
                { TypeOperation = eTypeOperation.ZReportPOS, TypeRRO = "Ingenico", JSON = r.ToJSON(), TextReceipt = r.Receipt == null ? null : string.Join(Environment.NewLine, r.Receipt) };
                Bl.InsertLogRRO(d);
                if (r.Receipt != null)
                    PrintNoFiscalReceipt(IdR, r.Receipt);
                return r;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return r;
        }

        public IEnumerable<string> GetLastReceiptPos() { return Terminal.GetLastReceipt(); }

        /// <summary>
        /// Скасування оплати.
        /// </summary>
        public void PosCancel() { Terminal.Cancel(); }

        /// <summary>
        /// Статус банківського термінала (Очікуєм карточки, Очікуєм підтвердження і ТД) 
        /// </summary>
        /// <param name="ww"></param>
        void PosStatus(StatusEquipment ww)
        {
            if (ww is PosStatus status)
            {
                SetStatus?.Invoke(ww /*new StatusEquipment(Terminal.Model, status.Status.GetStateEquipment(), $"{status.TextState} {status.Status.GetDescription}")*/);
                FileLogger.WriteLogMessage($"EquipmentFront.PosStatus {Terminal.Model} {status.TextState} {status.Status}");
            }
        }
        #endregion

        /// <summary>
        /// Зміна кольору прапорця
        /// </summary>
        /// <param name="pColor">Власне на який колір</param>
        public void SetColor(Color pColor)
        {
            try
            {
                Task.Run(() =>
                { Signal?.SwitchToColor(pColor); });
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKeyCode"></param>
        /// <param name="pCh"></param>
        public void SetKey(int pKeyCode, char pCh)
        {
            KB?.SetKey(pKeyCode, pCh);
        }
        #region Weight
        /// <summary>
        /// Початок зважування на основній вазі
        /// </summary>
        public void StartWeight()
        {
            try
            {
                IsControlScale = false;
                if (ControlScale == null || ControlScale.Model != eModelEquipment.VirtualControlScale)
                    Scale?.StartWeight();
            }
            catch (Exception e)
            { FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e); }//Необхідна обробка коли немає обладнання!!!TMP
        }

        /// <summary>
        /// Завершення зважування на основній вазі
        /// </summary>
        public void StoptWeight()
        {
            try
            {
                IsControlScale = true;
                if (ControlScale?.Model != eModelEquipment.VirtualControlScale)
                    Scale?.StopWeight();
            }
            catch (Exception e)
            { FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e); }//Необхідна обробка коли немає обладнання!!!TMP
        }

        /// <summary>
        /// Калібрування контрольної ваги
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public bool ControlScaleCalibrateMax(double maxValue) { return ControlScale.CalibrateMax(maxValue); }

        /// <summary>
        ///  Калібрація нуля
        /// </summary>
        /// <returns></returns>
        public bool ControlScaleCalibrateZero() { return ControlScale.CalibrateZero(); }

        public void StartMultipleTone() { Scaner?.StartMultipleTone(); }
        public void StopMultipleTone() { Scaner?.StopMultipleTone(); }

        public void ForceGoodReadTone() { Scaner?.ForceGoodReadTone(); }

        #endregion
    }

}
