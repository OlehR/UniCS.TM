using Front.Equipments;
using Front.Equipments.Ingenico;
using Front.Equipments.pRRO_SG;
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
using ModernExpo.SelfCheckout.Entities.CommandServer;
using SharedLib;


namespace Front
{
    public class EquipmentFront
    {
        public Action<double, bool> OnControlWeight { get; set; }
        public Action<double, bool> OnWeight { get; set; }
        private IEnumerable<Equipment> ListEquipment = new List<Equipment>();
        eStateEquipment _State = eStateEquipment.Off;

        BL Bl = BL.GetBL;
        Scaner Scaner;
        Scale Scale;
        public Scale ControlScale;
        SignalFlag Signal;
        BankTerminal Terminal;
        Rro RRO;
        /// <summary>
        /// Для віртуальної ваги куди йде подія.
        /// </summary>
        public bool IsControlScale = true;
        public IEnumerable<Equipment> GetBankTerminal { get { return ListEquipment.Where(e => e.Type == eTypeEquipment.BankTerminal); } }
        public void SetBankTerminal(BankTerminal pBT) { Terminal = pBT; }
        public int CountTerminal { get { return GetBankTerminal.Count(); } }
        public Equipment BankTerminal1 { get { return GetBankTerminal?.First(); } }
        public Equipment BankTerminal2 { get { return GetBankTerminal?.Skip(1).FirstOrDefault(); } }
        // eTypeAccess OperationWaitAccess { get; set; } = eTypeAccess.NoDefinition;

        /// <summary>
        /// Чи готове обладнання для оплати і фіскалізації
        /// </summary>
        public bool IsReadySale { get { return Terminal.State == eStateEquipment.On && RRO.State == eStateEquipment.On; } }

        public void ControlWeight(double pWeight, bool pIsStable)
        {
            OnControlWeight?.Invoke(pWeight, pIsStable);
        }
        public eStateEquipment StatCriticalEquipment
        {
            get
            {
                var Res = ListEquipment.Where(el => !(el.State == eStateEquipment.On || el.State == eStateEquipment.Init || el.State == eStateEquipment.Off) && el.IsСritical);
                var aa= Res != null && Res.Any() ? eStateEquipment.Error : eStateEquipment.On;
                return aa;
            }
        }
                
        public eStateEquipment State
        {
            get { return _State; }
            set
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
                            if (el.State == eStateEquipment.Off)
                                st = eStateEquipment.Off;
                        }
                        _State = st;
                    }
                if (_State != value)
                    SetState?.Invoke(_State);
            }
        }

        public Action<StatusEquipment> SetStatus { get; set; }

        public Action<eStateEquipment> SetState { get; set; }

        static EquipmentFront sEquipmentFront;

        public static EquipmentFront GetEquipmentFront { get { return sEquipmentFront; } }

        ILoggerFactory LF = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            //builder.AddDebug();
        });

        public EquipmentFront(Action<string, string> pSetBarCode,  Action<StatusEquipment> pActionStatus = null)
        {
            ILogger<EquipmentFront> logger = LF?.CreateLogger<EquipmentFront>();
            logger.LogDebug("Fp700 getInfo error");
            logger.LogInformation("LogInformation Fp700 getInfo error");

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            sEquipmentFront = this;
            //OnControlWeight += (pWeight, pIsStable) => { pSetControlWeight?.Invoke(pWeight, pIsStable); };                

            OnWeight += (pWeight, pIsStable) =>
            { if (IsControlScale && ControlScale.Model == eModelEquipment.VirtualControlScale) OnControlWeight?.Invoke(pWeight, pIsStable); };
            Task.Run(() =>  Init(pSetBarCode, pActionStatus) );
            
        }

       /* public void ControlWeight(double pWeight,bool IsStable)
        {
            OnControlWeight?.Invoke(pWeight, IsStable);
        }*/
        public void Init(Action<string, string> pSetBarCode,  Action<StatusEquipment> pActionStatus = null)
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
            State = eStateEquipment.Init;
            try
            {
                //Scaner
                var ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Scaner).First();
                switch (ElEquipment.Model)
                {
                    case eModelEquipment.MagellanScaner:
                        Scaner = new MagellanScaner(ElEquipment, config, LF, pSetBarCode);
                        break;
                    case eModelEquipment.VirtualScaner:
                        Scaner = new VirtualScaner(ElEquipment, config, LF, pSetBarCode);
                        break;
                    default:
                        Scaner = new Scaner(ElEquipment, config);
                        break;
                }
                NewListEquipment.Add(Scaner);

                //Scale
                ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Scale)?.First();
                switch (ElEquipment.Model)
                {
                    case eModelEquipment.MagellanScale:
                        Scale = new MagellanScale(((MagellanScaner)Scaner), OnWeight);//TMP!!! OnControlWeight - Нафіг                        
                        break;
                    case eModelEquipment.VirtualScale:
                        Scale = new VirtualScale(ElEquipment, config, LF, OnWeight);
                        break;
                    default:
                        Scale = new Scale(ElEquipment, config);
                        break;
                }
                NewListEquipment.Add(Scale);

                //ControlScale               
                var Equipments = ListEquipment.Where(e => e.Type == eTypeEquipment.ControlScale);
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
                //Flag
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
                //Bank Pos Terminal

                foreach (var el in GetBankTerminal)
                {
                    ElEquipment = el;

                    switch (ElEquipment.Model)
                    {
                        case eModelEquipment.Ingenico:
                            Terminal = new IngenicoH(el, config, LF, PosStatus);
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

                //RRO                
                Equipments = ListEquipment.Where(e => e.Type == eTypeEquipment.RRO);
                if (Equipments.Any())
                {
                    ElEquipment = Equipments.First();

                    switch (ElEquipment.Model)
                    {
                        case eModelEquipment.ExellioFP:
                            RRO = new Equipments.ExellioFP(ElEquipment, config, LF);
                            break;
                        case eModelEquipment.pRRO_SG:
                            RRO = new pRRO_SG(ElEquipment, config, LF, pActionStatus);
                            break;
                        case eModelEquipment.pRRo_WebCheck:
                            RRO = new pRRO_WebCheck(ElEquipment, config, LF, pActionStatus);
                            RRO.Init();
                            break;
                        case eModelEquipment.Maria:
                            RRO = new RRO_Maria(ElEquipment, config, LF, pActionStatus);
                            break;
                        case eModelEquipment.VirtualRRO:
                            RRO = new VirtualRRO(ElEquipment, config, LF, pActionStatus);
                            break;
                        case eModelEquipment.FP700:
                            RRO = new RRO_FP700(ElEquipment, config, LF, pActionStatus);
                            break;
                        default:
                            RRO = new Rro(ElEquipment, config);
                            break;
                    }
                    NewListEquipment.Add(RRO);

                    ListEquipment = NewListEquipment;

                }
                //Передаємо зміни станусів наверх.
                foreach (var el in ListEquipment)
                {
                    el.ActionStatus += (Status) => { SetStatus?.Invoke(Status); };
                }

                State = eStateEquipment.On;
                SetStatus?.Invoke(new StatusEquipment(eModelEquipment.NotDefine, eStateEquipment.On));
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"EquipmentFront Exception => Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                State = eStateEquipment.Error;
            }
        }



    public IEnumerable<Equipment> GetListEquipment { get { return ListEquipment; } }

        /// <summary>
        /// Друк чека
        /// </summary>
        public LogRRO PrintReceipt(Receipt pReceipt)
        {
            var r = RRO.PrintReceiptAsync(pReceipt).Result;
            Bl.InsertLogRRO(r);
            return r;
        }

        public LogRRO RroPrintX(IdReceipt pIdR)
        {
            var r= RRO.PrintXAsync(pIdR).Result;
            Bl.InsertLogRRO(r);
            return r;
        }

        public LogRRO RroPrintZ(IdReceipt pIdR)
        {
            var r= RRO.PrintZAsync(pIdR).Result;
            Bl.InsertLogRRO(r);
            return r;
        }

        public LogRRO RroPrintCopyReceipt()
        {
            var r= RRO.PrintCopyReceipt();
            Bl.InsertLogRRO(r);
            return r;
        }

        public LogRRO PrintNoFiscalReceipt(IEnumerable<string> pR)
        {
            var r = RRO.PrintNoFiscalReceiptAsync(pR).Result;
            Bl.InsertLogRRO(r);
            return r;
        }

        public bool ProgramingArticleAsync(IEnumerable<ReceiptWares> pRW)
        {
             RRO?.ProgramingArticleAsync(pRW);
            return true;
        }

        /// <summary>
        /// Оплата по банківському терміналу
        /// </summary>
        /// <param name="pSum">Власне сума</param>
        /// <returns></returns>
        public Payment PosPurchase(IdReceipt pIdR, decimal pSum)
        {
            Payment r = null;
            try
            {
                r = Terminal?.Purchase(pSum);
                if (r.IsSuccess)
                {                   
                    LogRRO d = new(pIdR)
                    { TypeOperation = eTypeOperation.SalePOS, TypeRRO = "Ingenico", JSON = r.ToJSON(), TextReceipt = r.Receipt == null ? null : string.Join(Environment.NewLine, r.Receipt) };
                    Bl.InsertLogRRO(d);
                    r.SetIdReceipt(pIdR);
                    Bl.db.ReplacePayment(new List<Payment>() { r });
                }
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"(pIdR=>{pIdR.ToJSON()},pSum={pSum})=>{r.ToJSON()}",eTypeLog.Expanded);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return r;
        }

        /// <summary>
        /// Повернення покупцю грошей по банківському терміналу
        /// </summary>
        /// <param name="pSum"></param>
        /// <param name="pRNN"></param>
        /// <returns></returns>
        public Payment PosRefund(IdReceipt pIdR, decimal pSum, string pRNN)
        {
            Payment r = null;
            try
            {
                r = Terminal?.Refund(pSum, pRNN);
                if (r.IsSuccess)
                {
                    LogRRO d = new(pIdR)
                    { TypeOperation = eTypeOperation.Refund, TypeRRO = "Ingenico", JSON = r.ToJSON(), TextReceipt = r.Receipt == null ? null : string.Join(Environment.NewLine, r.Receipt) };
                    Bl.InsertLogRRO(d);
                    r.SetIdReceipt(pIdR);
                    Bl.db.ReplacePayment(new List<Payment>() { r });
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"(pIdR=>{pIdR.ToJSON()},pSum={pSum})=>{r.ToJSON()}", eTypeLog.Expanded);
                }
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return r;             
        }

        public BatchTotals PosPrintX()
        {
            BatchTotals r = null;
            try
            {
                r = Terminal.PrintX();
                LogRRO d = new(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod() })
                { TypeOperation = eTypeOperation.XReportPOS, TypeRRO= "Ingenico",  JSON = r.ToJSON(), TextReceipt = r.Receipt == null ? null : string.Join(Environment.NewLine, r.Receipt) };
                Bl.InsertLogRRO(d);
                if (r.Receipt != null)
                    PrintNoFiscalReceipt(r.Receipt);
                return r;               
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);                
            }
            return r;
        }

        public BatchTotals PosPrintZ()
        {
            BatchTotals r = null;
            try
            {
                r = Terminal.PrintZ();
                LogRRO d = new(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod() })
                { TypeOperation = eTypeOperation.ZReportPOS, TypeRRO = "Ingenico", JSON = r.ToJSON(), TextReceipt = r.Receipt == null ? null : string.Join(Environment.NewLine, r.Receipt) };
                Bl.InsertLogRRO(d);
                if (r.Receipt != null)
                    PrintNoFiscalReceipt(r.Receipt);
                return r;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);               
            }
            return r;

        }

        public IEnumerable<string> GetLastReceiptPos()
        {
            return Terminal.GetLastReceipt();
        }

        /// <summary>
        /// Статус банківського термінала (Очікуєм карточки, Очікуєм підтвердження і ТД) 
        /// </summary>
        /// <param name="ww"></param>
        void PosStatus(StatusEquipment ww)
        {
            if (ww is PosStatus status)
            {
                SetStatus?.Invoke(new StatusEquipment(Terminal.Model, status.Status.GetStateEquipment(), $"{status.TextState} {status.Status}"));
                FileLogger.WriteLogMessage($"EquipmentFront.PosStatus {Terminal.Model} {status.TextState} {status.Status}");
            }
        }

        public IConfiguration GetConfig()
        {            
            var AppConfiguration = Config.AppConfiguration;
            AppConfiguration.GetSection("MID:Equipment").Bind(ListEquipment);
            //var r = AppConfiguration.GetSection("MID:Equipment");
            return AppConfiguration;
        }

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
            catch(Exception e)
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
                if(ControlScale.Model!=eModelEquipment.VirtualControlScale)
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
        public bool ControlScaleCalibrateMax(double maxValue)  { return ControlScale.CalibrateMax(maxValue); }

        /// <summary>
        ///  Калібрація нуля
        /// </summary>
        /// <returns></returns>
        public bool ControlScaleCalibrateZero() {  return ControlScale.CalibrateZero(); }

        public  void StartMultipleTone() { Scaner.StartMultipleTone(); }
        public  void StopMultipleTone() { Scaner.StopMultipleTone(); }
    }
}
