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
using ModernExpo.SelfCheckout.Entities.Models.Terminal;
using System.Windows;

namespace Front
{
    public class EquipmentFront
    {
        #region Init
        public Action<double, bool> OnControlWeight { get; set; }
        public Action<double, bool> OnWeight { get; set; }
        private IEnumerable<Equipment> ListEquipment = new List<Equipment>();
        //eStateEquipment _State = eStateEquipment.Off;
        readonly BL Bl = BL.GetBL;
        Scaner Scaner;
        Scale Scale;
        Printer Printer;
        public Scale ControlScale;
        SignalFlag Signal;
        BankTerminal Terminal;
        //Rro RRO;
        SortedList<int,Rro> RROs=new();
        Rro GetRRO(int pIdWorkplace) { return RROs?.ContainsKey(pIdWorkplace)==true ? RROs[pIdWorkplace] : (pIdWorkplace==0 && RROs?.ContainsKey(Global.IdWorkPlace) == true ? RROs[Global.IdWorkPlace] :null);}
        /// <summary>
        /// Текуча операція РРО для блокування операцій.
        /// </summary>
        eTypeOperation curTypeOperation = eTypeOperation.NotDefine;
        /// <summary>
        /// Для віртуальної ваги куди йде подія.
        /// </summary>
        public bool IsControlScale = true;
        public IEnumerable<Equipment> GetBankTerminal { get { return ListEquipment.Where(e => e.Type == eTypeEquipment.BankTerminal); } }
        public void SetBankTerminal(BankTerminal pBT) { Terminal = pBT; }
        public int CountTerminal { get { return GetBankTerminal.Count(); } }
        public Equipment BankTerminal1 { get { return GetBankTerminal?.First(); } }
        public Equipment BankTerminal2 { get { return GetBankTerminal?.Skip(1).FirstOrDefault(); } }
        public Window W;
        // eTypeAccess OperationWaitAccess { get; set; } = eTypeAccess.NoDefinition;

        /// <summary>
        /// Чи готове обладнання для оплати і фіскалізації
        /// </summary>
        //public bool IsReadySale { get { return Terminal.State == eStateEquipment.On && RROs.Where(el=>el.State == eStateEquipment.On); } }

        public void ControlWeight(double pWeight, bool pIsStable)
        {
            OnControlWeight?.Invoke(pWeight, pIsStable);
        }
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
                 return ListEquipment.Where(el=>el.IsСritical==true).Max(el => el.State);  
            }
           /* set
            {
                if (_State != value)
                    if (value == eStateEquipment.Error)
                        _State = value;
                    else
                      if (value == eStateEquipment.On)
                    {
                        eStateEquipment st = eStateEquipment.On;
                        foreach (var el in ListEquipment)
                        {
                            if (el.State == eStateEquipment.Off || el.State == eStateEquipment.Error)
                            {
                                st = el.State;
                                break;
                            }
                        }
                        _State = st;
                    }
               // if (_State != value) SetState?.Invoke(_State);
            }*/
        }

        public Action<StatusEquipment> SetStatus { get; set; }

        //public Action<eStateEquipment> SetState { get; set; }

        static EquipmentFront sEquipmentFront;

        public static EquipmentFront GetEquipmentFront { get { return sEquipmentFront; } }

        readonly ILoggerFactory LF = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();            
        });

        public EquipmentFront(Action<string, string> pSetBarCode, Action<StatusEquipment> pActionStatus = null, Window pW=null)
        {
            ILogger<EquipmentFront> logger = LF?.CreateLogger<EquipmentFront>();
            logger.LogDebug("Fp700 getInfo error");
            logger.LogInformation("LogInformation Fp700 getInfo error");

            W = pW;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            sEquipmentFront = this;
            //OnControlWeight += (pWeight, pIsStable) => { pSetControlWeight?.Invoke(pWeight, pIsStable); };                


            OnWeight += (pWeight, pIsStable) =>
            { if (IsControlScale && ControlScale?.Model == eModelEquipment.VirtualControlScale) OnControlWeight?.Invoke(pWeight, pIsStable); };
            Task.Run(() => Init(pSetBarCode, pActionStatus));
        }
        
        public void Init(Action<string, string> pSetBarCode, Action<StatusEquipment> pActionStatus = null)
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
                try
                {
                    //Scaner
                    ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Scaner).First();
                    switch (ElEquipment.Model)
                    {
                        case eModelEquipment.MagellanScaner:
                            Scaner = new MagellanScaner(ElEquipment, config, LF, pSetBarCode);
                            break;
                        case eModelEquipment.VirtualScaner:
                            Scaner = new VirtualScaner(ElEquipment, config, LF, pSetBarCode);
                            break;
                        case eModelEquipment.ScanerKeyBoard:
                            ScanerKeyBoard Sc = new ScanerKeyBoard(ElEquipment, config, LF, pSetBarCode);
                            if (W != null)
                                W.KeyUp += Sc.Key_UP;
                            Scaner = Sc;
                            break;
                        default:
                            Scaner = new Scaner(ElEquipment, config);
                            break;
                    }
                    NewListEquipment.Add(Scaner);
                }
                catch { };
                //Scale
                try
                {
                    ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Scale)?.First();
                    switch (ElEquipment.Model)
                    {
                        case eModelEquipment.MagellanScale:
                            Scale = new MagellanScale(((MagellanScaner)Scaner), OnWeight);//TMP!!! OnControlWeight - Нафіг                        
                            break;
                        case eModelEquipment.VirtualScale:
                            Scale = new VirtualScale(ElEquipment, config, LF, OnWeight);
                            break;
                        case eModelEquipment.ScaleCom:
                            Scale = new ScaleCom(ElEquipment, config, LF, OnWeight);
                           // Scale.StartWeight();
                            break;
                        default:
                            Scale = new Scale(ElEquipment, config);
                            break;
                    }
                    //Scale.StartWeight();
                    NewListEquipment.Add(Scale);
                }
                catch { };
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
                catch { }
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
                } catch { }
                //Bank Pos Terminal
                try
                {
                    foreach (var el in GetBankTerminal)
                    {
                        ElEquipment = el;

                        switch (ElEquipment.Model)
                        {
                            case eModelEquipment.Ingenico:
                                Terminal = new Ingenico(el, config, LF, PosStatus);
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
                } catch { }

                //RRO
                try
                {
                    foreach (var el in ListEquipment.Where(e => e.Type == eTypeEquipment.RRO))
                    {
                        Rro RRO;
                        switch (el.Model)
                        {
                            case eModelEquipment.ExellioFP:
                                RRO = new Equipments.ExellioFP(el, config, LF);
                                break;
                            case eModelEquipment.pRRO_SG:
                                RRO = new Front.Equipments.pRRO_SG.pRRO_SG(el, config, LF, pActionStatus);
                                break;
                            case eModelEquipment.pRRo_WebCheck:
                                RRO = new pRRO_WebCheck(el, config, LF, pActionStatus);
                                RRO.Init();
                                break;
                            case eModelEquipment.Maria:
                                RRO = new RRO_Maria(el, config, LF, pActionStatus);
                                break;
                            case eModelEquipment.VirtualRRO:
                                RRO = new VirtualRRO(el, config, LF, pActionStatus);
                                break;
                            case eModelEquipment.RRO_FP700:
                                RRO = new RRO_FP700(el, config, LF, pActionStatus);
                                break;
                            case eModelEquipment.pRRO_Vchasno:
                                RRO = new pRRO_Vchasno(el, config, LF, pActionStatus);
                                break;
                            default:
                                RRO = new Rro(el, config);
                                break;
                        }
                        NewListEquipment.Add(RRO);
                        RROs.Add(RRO.IdWorkplacePay, RRO);
                    }
                  
                } catch { }

                //Принтер
                try
                {
                    ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Printer)?.First();
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
                catch { }
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
        object Lock =new object();
        DateTime LockDT;
        LogRRO WaitRRO(IdReceipt pReceipt, eTypeOperation pTypeOperation,int pMilisecond = 500,bool pIsStop=true)
        {
            LogRRO Res = null;
            lock (Lock)
            {
                var RRO = GetRRO(pReceipt.IdWorkplacePay);
                try
                {
                    if (pIsStop)
                        RRO?.Stop();
                    while (pMilisecond > 0 && curTypeOperation != eTypeOperation.NotDefine)
                    {
                        Thread.Sleep(50);
                        pMilisecond -= 50;
                    }
                    if (curTypeOperation == eTypeOperation.NotDefine)
                    {
                        curTypeOperation = pTypeOperation;
                        LockDT = DateTime.Now;
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"TypeOperation=>{pTypeOperation}");
                    }
                    else
                    {
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"LastTypeOperation=>{curTypeOperation} LockDT=> {LockDT} TryTypeOperation=>{pTypeOperation}",eTypeLog.Error);
                        Res = new LogRRO(pReceipt) { TypeOperation = pTypeOperation, TypeRRO = RRO.Model.ToString(), CodeError = -1, Error = $"Не вдалось виконати текучу операцію {pTypeOperation} на RRO{Environment.NewLine}Оскільки не виконалась попередня: LastTypeOperation=>{curTypeOperation} LockDT=> {LockDT}" };
                    }
                }
                catch(Exception e)
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
            string NameMetod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            if (pReceipt == null || pReceipt.Payment == null || !pReceipt.Payment.Any(el => el.TypePay == eTypePay.Card || el.TypePay == eTypePay.Cash))
                return new LogRRO(pReceipt) { CodeError = -1, Error = $"Відсутня оплата по Робочому місцю № ({pReceipt.IdWorkplacePay})" };
            var r = Task.Run<LogRRO>((Func<LogRRO>)(() =>
            {
                var RRO = GetRRO(pReceipt.IdWorkplacePay);
                
                LogRRO Res;
                try
                {
                    Res = WaitRRO(pReceipt, eTypeOperation.Sale);
                    if (Res == null)
                    {
                        curTypeOperation = eTypeOperation.Sale;
                        FileLogger.WriteLogMessage(this, NameMetod, "Start Print Receipt");
                        if(pReceipt?.TypeReceipt==eTypeReceipt.Refund && pReceipt.Wares!=null)
                        {
                            var rr = pReceipt.Wares.Where(r => r.Quantity != 0m);
                            pReceipt.Wares = rr;
                        }
                        Res = RRO?.PrintReceipt(pReceipt);
                        var Log = pReceipt.LogRROs?.ToList()?? new List<LogRRO>();
                        Log.Add(Res);
                        pReceipt.LogRROs=Log;
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
                    curTypeOperation = eTypeOperation.NotDefine;                    
                }
                Res.IdWorkplacePay = pReceipt.IdWorkplacePay;
                return Res;
            })).Result;
            if (r.CodeError == 0 && pReceipt.TypeReceipt == eTypeReceipt.Sale && pReceipt.IdWorkplacePay ==Global.IdWorkPlace)
                PrintQR(pReceipt);
            GetLastReceipt(pReceipt, r);
            return r;
        }

        void GetLastReceipt(IdReceipt pIdR, LogRRO r)
        {
            Task.Run(() =>
           {
               try
               {
                   var RRO = GetRRO(pIdR.IdWorkplacePay);
                   if (r.CodeError == 0)
                   {
                       if (string.IsNullOrEmpty(r.TextReceipt) || RRO.Model == eModelEquipment.RRO_FP700)
                       {
                           LogRRO Res;
                           try
                           {
                               Res = WaitRRO(pIdR, eTypeOperation.LastReceipt,  500, false);
                               if (Res == null)
                               {
                                   r.TextReceipt = RRO.GetTextLastReceipt();
                                   r.SUM = RRO.GetSumFromTextReceipt(r.TextReceipt);
                               }
                           }
                           catch (Exception e)
                           {
                               FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                           }
                           finally
                           {
                               curTypeOperation = eTypeOperation.NotDefine;
                           }
                       }
                       Bl.InsertLogRRO(r);
                   }
               }
               catch (Exception e)
               {
                   FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
               }
           });
        }

        public LogRRO RroPrintX(IdReceipt pIdR)
        {
           
            var r = Task.Run<LogRRO>((Func<LogRRO>)(() =>
            {
                var RRO = GetRRO(pIdR.IdWorkplacePay);
                LogRRO Res;
                try
                {
                    Res = WaitRRO(pIdR, eTypeOperation.XReport);
                    if (Res == null)                    
                        Res= RRO?.PrintX(pIdR);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    SetStatus?.Invoke(new StatusEquipment(RRO.Model, eStateEquipment.Error, e.Message) { IsСritical = true });
                    Res= new LogRRO(pIdR) { TypeOperation = eTypeOperation.XReport, TypeRRO = RRO.Model.ToString(), CodeError = -1, Error = e.Message };
                }
                finally
                {
                    curTypeOperation = eTypeOperation.NotDefine;
                }
                return Res;
            }
            )).Result;
            GetLastReceipt(pIdR, r);
            return r;
        }

        public LogRRO RroPrintZ(IdReceipt pIdR)
        {
            var r = Task.Run<LogRRO>((Func<LogRRO>)(() =>
            {
                var RRO = GetRRO(pIdR.IdWorkplacePay);
                LogRRO Res;
                try
                {
                    Res = WaitRRO(pIdR, eTypeOperation.ZReport);
                    if (Res == null)
                        Res = RRO?.PrintZ(pIdR);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    SetStatus?.Invoke(new StatusEquipment(RRO.Model, eStateEquipment.Error, e.Message) { IsСritical=true});
                    Res= new LogRRO(pIdR) { TypeOperation = eTypeOperation.ZReport, TypeRRO = RRO.Model.ToString(), CodeError = -1, Error = e.Message };
                }
                finally
                {
                    curTypeOperation = eTypeOperation.NotDefine;
                }
                return Res;
            }
            )).Result;
            GetLastReceipt(pIdR, r);
            return r;
        }

        public LogRRO RroPeriodZReport(IdReceipt pIdR, DateTime pBegin, DateTime pEnd, bool IsFull = true)
        {
            var r = Task.Run<LogRRO>((Func<LogRRO>)(() =>
            {
                var RRO = GetRRO(pIdR.IdWorkplacePay);
                LogRRO Res;
                try
                {
                    Res = WaitRRO(pIdR, eTypeOperation.PeriodZReport);
                    if (Res == null)
                        RRO?.PeriodZReport(pBegin, pEnd, IsFull);
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
                    curTypeOperation = eTypeOperation.NotDefine;
                }
                return Res;
            })).Result;
            return r;
        }

        public LogRRO RroPrintCopyReceipt(int pIdWorkplacePay)
        {
            var RRO = GetRRO(pIdWorkplacePay);
            var r = RRO?.PrintCopyReceipt();
            Bl.InsertLogRRO(r);
            return r;
        }

        public decimal SumReceiptFiscal(Receipt pR)
        {
            if (pR == null || pR.Wares == null || !pR.Wares.Any()) return 0;
            var RRO = GetRRO(pR.IdWorkplacePay);
            decimal sum = 0;
            sum = RRO != null ? RRO.SumReceiptFiscal(pR) : pR.Wares.Sum(r => (r.SumTotal)) ;
            return sum;
        }

        public LogRRO RroMoveMoney(decimal pSum, IdReceipt pIdR)
        {

            var r = Task.Run<LogRRO>((Func<LogRRO>)(() =>
            {
                var RRO = GetRRO(pIdR.IdWorkplacePay);
                LogRRO Res;
                try
                {
                    Res = WaitRRO(pIdR,pSum>0? eTypeOperation.MoneyIn:eTypeOperation.MoneyOut);
                    if (Res == null)
                        Res = RRO?.MoveMoney(pSum,pIdR);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    SetStatus?.Invoke(new StatusEquipment(RRO.Model, eStateEquipment.Error, e.Message) { IsСritical = true });
                    Res = new LogRRO(pIdR) { TypeOperation = pSum>0?eTypeOperation.MoneyIn:eTypeOperation.MoneyOut , TypeRRO = RRO.Model.ToString(), CodeError = -1, Error = e.Message };
                }
                finally
                {
                    curTypeOperation = eTypeOperation.NotDefine;
                }
                return Res;
            }
            )).Result;
            GetLastReceipt(pIdR, r);
            return r;
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
                if (IdWorkplacePays[i]== pR.IdWorkplacePay || pR.IdWorkplacePay>0)
                {
                    pR.IdWorkplacePay = IdWorkplacePays[i];
                    var RRO = GetRRO(pR.IdWorkplacePay);
                    Sum += RRO.SumCashReceiptFiscal(pR);
                }
            }
            pR.IdWorkplacePay = Id;
            return Sum;
        }

        public LogRRO PrintNoFiscalReceipt(IdReceipt pReceipt ,IEnumerable<string> pR)
        {
            if (pR != null && pR.Any())
            {
                var r = Task.Run<LogRRO>((Func<LogRRO>)(() =>
                {
                    LogRRO Res;
                    if (Printer != null)
                    {
                        bool r=Printer.Print(pR);
                        Res=new LogRRO() { TypeOperation = eTypeOperation.NoFiscalReceipt, TypeRRO = Printer.Model.ToString(), CodeError = r?0:-1 };
                    }
                    else
                    {
                        var RRO = GetRRO(pReceipt.IdWorkplacePay);
                        
                        try
                        {
                            Res = WaitRRO(pReceipt, eTypeOperation.NoFiscalReceipt);
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
                            curTypeOperation = eTypeOperation.NotDefine;
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

        public void ProgramingArticleAsync( ReceiptWares pRW)
        {
            Task.Run(() =>
            {
                
                var RRO = GetRRO(pRW.IdWorkplacePay);
                LogRRO Res;
                try
                {                    
                    Res = WaitRRO(pRW, eTypeOperation.ProgramingArticle);
                    if (Res == null)
                        RRO?.ProgramingArticle(pRW);
                }
                catch (Exception e)
                {
                if (RRO != null)
                    RRO.State = eStateEquipment.Error;
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    SetStatus?.Invoke(new StatusEquipment(RRO.Model, eStateEquipment.Error, e.Message) { IsСritical = true });
                }
                finally
                {
                    curTypeOperation = eTypeOperation.NotDefine;
                }
                return true;
            }            
            );
        }
        #endregion
        /// <summary>
        /// Друк чека на (термо)принтері
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        public bool Print(Receipt pR)
        {
           return Printer?.PrintReceipt(pR)??true;
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
                        Payment C = (Payment)r.Clone();
                        C.TypePay = eTypePay.IssueOfCash;
                        C.SumPay = pIssuingCash;
                        C.SumExt= pIssuingCash;
                        LP.Add(C);
                    }

                    Bl.db.ReplacePayment(LP);

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

        public BatchTotals PosPrintX( IdReceipt IdR, bool IsPrint = true)
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
                    PrintNoFiscalReceipt(IdR,r.Receipt);
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
        public void PosCancel()  { Terminal.Cancel();}
        
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
        /// Початок зважування на основній вазі
        /// </summary>
        public void StartWeight()
        {
            try
            {
                IsControlScale = false;
                if (ControlScale.Model != eModelEquipment.VirtualControlScale)
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
                if (ControlScale.Model != eModelEquipment.VirtualControlScale)
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
    }
}
