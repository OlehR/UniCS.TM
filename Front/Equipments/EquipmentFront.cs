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

namespace Front
{
    public class EquipmentFront
    {
        public Action<double, bool> OnControlWeight { get; set; }
        public Action<double, bool> OnWeight { get; set; }
        private IEnumerable<Equipment> ListEquipment = new List<Equipment>();
        eStateEquipment _State = eStateEquipment.Off;

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
                
        /*public eStateEquipment SaleIsReady
        {
            get
            {
                var Res = ListEquipment.Where(el => (el.Type == eTypeEquipment.RRO || el.Type == eTypeEquipment.BankTerminal) && el.State != eStateEquipment.On && el.IsСritical);
                return Res != null && Res.Any() ? eStateEquipment.Error : eStateEquipment.On;
            }
        }*/
        
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

        public EquipmentFront(Action<string, string> pSetBarCode,  Action<StatusEquipment> pActionStatus = null)
        {
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
            var config = Config("appsettings.json");
            State = eStateEquipment.Init;
            try
            {
                //Scaner
                var ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Scaner).First();
                switch (ElEquipment.Model)
                {
                    case eModelEquipment.MagellanScaner:
                        Scaner = new MagellanScaner(ElEquipment, config, null, pSetBarCode);
                        break;
                    case eModelEquipment.VirtualScaner:
                        Scaner = new VirtualScaner(ElEquipment, config, null, pSetBarCode);
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
                        Scale = new VirtualScale(ElEquipment, config, null, OnWeight);
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
                            ControlScale = new ScaleModern(ElEquipment, config, null, ControlWeight);
                            break;
                        case eModelEquipment.VirtualControlScale: 
                            ControlScale = new VirtualControlScale(ElEquipment, config,null,null);
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
                            Terminal = new IngenicoH(el, config, null, PosStatus);
                            break;
                        case eModelEquipment.VirtualBankPOS:
                            Terminal = new VirtualBankPOS(el, config, null, PosStatus);
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
                            RRO = new Equipments.ExellioFP(ElEquipment, config, null);
                            break;
                        case eModelEquipment.pRRO_SG:
                            RRO = new pRRO_SG(ElEquipment, config, null, pActionStatus);
                            break;
                        case eModelEquipment.pRRo_WebCheck:
                            RRO = new pRRO_WebCheck(ElEquipment, config, null, pActionStatus);
                            RRO.Init();
                            break;
                        case eModelEquipment.Maria:
                            RRO = new RRO_Maria(ElEquipment, config, null, pActionStatus);
                            break;
                        case eModelEquipment.VirtualRRO:
                            RRO = new VirtualRRO(ElEquipment, config, null, pActionStatus);
                            break;
                        case eModelEquipment.FP700:
                            RRO = new RRO_FP700(ElEquipment, config, null, pActionStatus);
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
            return RRO.PrintReceiptAsync(pReceipt).Result;
        }

        public LogRRO RroPrintX(IdReceipt pIdR)
        {
            return RRO.PrintXAsync(pIdR).Result;
        }

        public LogRRO RroPrintZ(IdReceipt pIdR)
        {
            return RRO.PrintZAsync(pIdR).Result;
        }

        public LogRRO RroPrintCopyReceipt()
        {
            return RRO.PrintCopyReceipt();
        }

        public LogRRO PrintNoFiscalReceipt(IEnumerable<string> pR)
        {
            return RRO.PrintNoFiscalReceiptAsync(pR).Result;
        }

        /// <summary>
        /// Оплата по банківському терміналу
        /// </summary>
        /// <param name="pSum">Власне сума</param>
        /// <returns></returns>
        public Payment PosPurchase(decimal pSum)
        {
            return Terminal?.Purchase(pSum);
        }

        /// <summary>
        /// Повернення покупцю грошей по банківському терміналу
        /// </summary>
        /// <param name="pSum"></param>
        /// <param name="pRNN"></param>
        /// <returns></returns>
        public Payment PosRefund(decimal pSum, string pRNN)
        {
            return Terminal.Refund(pSum, pRNN);
        }

        public BatchTotals PosPrintX()
        {
            return Terminal.PrintX();             
        }

        public BatchTotals PosPrintZ()
        {
            return Terminal.PrintZ();       
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

        public IConfiguration Config(string settingsFilePath)
        {
            var CurDir = AppDomain.CurrentDomain.BaseDirectory;
            var AppConfiguration = new ConfigurationBuilder()
                .SetBasePath(CurDir)
                .AddJsonFile(settingsFilePath).Build();

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
            catch (Exception) { }//Необхідна обробка коли немає обладнання !!!TMP
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
            catch (Exception) { }//Необхідна обробка коли немає обладнання!!!TMP

        }

        /// <summary>
        /// Калібрування контрольної ваги
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public bool ControlScaleCalibrateMax(double maxValue)
        {
            return ControlScale.CalibrateMax(maxValue);
        }

        /// <summary>
        ///  Калібрація нуля
        /// </summary>
        /// <returns></returns>
        public bool ControlScaleCalibrateZero()
        {
            return ControlScale.CalibrateZero();
        }

        public  void StartMultipleTone() { Scaner.StartMultipleTone(); }
        public  void StopMultipleTone() { Scaner.StopMultipleTone(); }
    }
}
