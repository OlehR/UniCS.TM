using Front.Equipments;
using Front.Equipments.Ingenico;
using Front.Equipments.pRRO_SG;
using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Front.Equipments.Implementation;
using System.Threading.Tasks;
using System.Diagnostics;
using Utils;

namespace Front
{
    public class EquipmentFront
    {
        private List<EquipmentElement> ListEquipment = new List<EquipmentElement>();
        eStateEquipment _State = eStateEquipment.Off;

        Scaner Scaner;
        Scale Scale;
        Scale ControlScale;
        SignalFlag Signal;
        BankTerminal Terminal;
        Rro RRO;


        public IEnumerable<EquipmentElement> GetBankTerminal { get { return ListEquipment.Where(e => e.Type == eTypeEquipment.BankTerminal); } }
        public void SetBankTerminal(BankTerminal pBT) { Terminal = pBT; }
        public int CountTerminal { get { return GetBankTerminal.Count(); } }

       // eTypeAccess OperationWaitAccess { get; set; } = eTypeAccess.NoDefinition;

        public eStateEquipment State
        {
            get { return _State; }
            set
            {
                if (_State != value)
                    if (value == eStateEquipment.Error)
                        _State = value;
                    else
                      if (value == eStateEquipment.Ok)
                    {
                        eStateEquipment st = eStateEquipment.Ok;
                        foreach (var el in ListEquipment)
                        {
                            if (el.Equipment.State == eStateEquipment.Off)
                                st = eStateEquipment.Off;
                        }
                        _State = st;
                    }
                if (_State != value)
                    SetState?.Invoke(_State);
            }
        }

        public  Action<StatusEquipment> SetStatus { get; set; }        

        public  Action<eStateEquipment> SetState { get; set; }

        static EquipmentFront sEquipmentFront;

        public static EquipmentFront GetEquipmentFront { get { return sEquipmentFront; } }

        public EquipmentFront(Action<string, string> pSetBarCode, Action<double, bool> pSetWeight, Action<double, bool> pSetControlWeight, Action<eStatusRRO> pActionStatus = null)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            sEquipmentFront = this;
           
            Task.Run( ()=>Init(pSetBarCode, pSetWeight, pSetControlWeight, pActionStatus));
        }
        
        public void Init(Action<string, string> pSetBarCode, Action<double, bool> pSetWeight, Action<double, bool> pSetControlWeight, Action<eStatusRRO> pActionStatus = null)
        {
            var config = Config("appsettings.json");
            State = eStateEquipment.Init;            
            try
            { 
                //Scaner
                var ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Scaner).First();
                switch (ElEquipment.Model)
                {
                    case eModelEquipment.MagellanScaner:
                        ElEquipment.Equipment = new MagellanScaner(config, null, pSetBarCode);
                        break;
                    case eModelEquipment.VirtualScaner:
                        ElEquipment.Equipment = new VirtualScaner(config, null, pSetBarCode);
                        break;
                    default:
                        ElEquipment.Equipment = new Scaner(config);
                        break;
                }
                Scaner = (Scaner)ElEquipment.Equipment;

                //Scale
                ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Scale).First();
                switch (ElEquipment.Model)
                {
                    case eModelEquipment.MagellanScale:
                        ElEquipment.Equipment = new MagellanScale(((MagellanScaner)Scaner).Magellan9300, pSetWeight);
                        break;
                    case eModelEquipment.VirtualScale:
                        ElEquipment.Equipment = new VirtualScale(config, null, pSetWeight);
                        break;
                    default:
                        ElEquipment.Equipment = new Scale(config);
                        break;
                }
                Scale = (Scale) ElEquipment.Equipment;

                //ControlScale
                ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.ControlScale).First();
                if (ElEquipment.Model == eModelEquipment.ScaleModern)
                    ElEquipment.Equipment = new ScaleModern(config, null, pSetControlWeight);
                else
                    ElEquipment.Equipment = new Scale(config); ;
                ControlScale = (Scale)ElEquipment.Equipment;

                //Flag
                ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Signal).First();
                if (ElEquipment.Model == eModelEquipment.SignalFlagModern)
                    ElEquipment.Equipment = new SignalFlagModern(config);
                else
                    ElEquipment.Equipment = new SignalFlag(config);
                Signal = (SignalFlag)ElEquipment.Equipment;

                //Bank Pos Terminal           
                foreach (var el in GetBankTerminal)
                {
                    ElEquipment = el;

                    switch (ElEquipment.Model)
                    {
                        case eModelEquipment.Ingenico:
                            ElEquipment.Equipment = new IngenicoH(config, null, PosStatus);
                            break;
                        case eModelEquipment.VirtualBankPOS:
                            ElEquipment.Equipment = new VirtualBankPOS(config, null, PosStatus);
                            break;
                        default:
                            ElEquipment.Equipment = new BankTerminal(config);
                            break;
                    }
                    Terminal = (BankTerminal)ElEquipment.Equipment;
                }

                //RRO
                ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.RRO).First();
                switch (ElEquipment.Model)
                {
                    case eModelEquipment.ExellioFP:
                        ElEquipment.Equipment = new Equipments.ExellioFP(config, null);
                        break;
                    case eModelEquipment.pRRO_SG:
                        ElEquipment.Equipment = new pRRO_SG(config, null, pActionStatus);
                        break;
                    case eModelEquipment.pRRo_WebCheck:
                        ElEquipment.Equipment = new pRRO_WebCheck(config, null, pActionStatus);
                        break;
                    case eModelEquipment.Maria:
                        ElEquipment.Equipment = new RRO_Maria(config, null, pActionStatus);
                        break;
                    default:
                        ElEquipment.Equipment = new Rro(config);                        
                        break;
                }
                RRO = (Rro)ElEquipment.Equipment;

                State = eStateEquipment.Ok;

               foreach(var el in  ListEquipment)
                {
                    el.Equipment.SetState += (pStateEquipment, pModelEquipment) => { SetStatus?.Invoke(new StatusEquipment() {ModelEquipment= pModelEquipment,State =(int)pStateEquipment, TextState=$"" }); };
                }

            }
            catch (Exception e)
            { 
                FileLogger.WriteLogMessage($"EquipmentFront Exception => Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                State = eStateEquipment.Error;
            }
        }
              

        public IEnumerable<EquipmentElement> GetListEquipment { get { return ListEquipment; } }
        
        /// <summary>
        /// Друк чека
        /// </summary>
        public LogRRO PrintReceipt(Receipt pReceipt) 
        {  
            return RRO.PrintReceiptAsync(pReceipt).Result;
        }

        public LogRRO RroPrintX()
        {
            return RRO.PrintXAsync(null).Result;
        }

        public LogRRO RroPrintZ()
        {
            return RRO.PrintZAsync(null).Result;
        }

        public LogRRO RroPrintCopyReceipt()
        {
            return RRO.PrintCopyReceipt();
        }

        /// <summary>
        /// Оплата по банківському терміналу
        /// </summary>
        /// <param name="pSum">Власне сума</param>
        /// <returns></returns>
        public Payment PosPurchase(decimal pSum) {
            return Terminal.Purchase(pSum);
        }

        /// <summary>
        /// Повернення покупцю грошей по банківському терміналу
        /// </summary>
        /// <param name="pSum"></param>
        /// <param name="pRNN"></param>
        /// <returns></returns>
        public Payment PosRefund(decimal pSum,string pRNN)
        {
            return Terminal.Refund(pSum, pRNN);
        }

        public bool PosPrintX()
        {
            Terminal.PrintX();
            return true;
        }

        public bool PosPrintZ()
        {
             Terminal.PrintZ();
            return true;
        }
                

        /// <summary>
        /// Статус банківського термінала (Очікуєм карточки, Очікуєм підтвердження і ТД) 
        /// </summary>
        /// <param name="ww"></param>
        void PosStatus(IPosStatus ww)
        {
            if (ww is PosStatus status)
            {
                SetStatus?.Invoke(new StatusEquipment(Terminal.ModelEquipment, (int)status.Status, $"{status.MsgDescription} {status.Status}"));
                Debug.WriteLine($"{DateTime.Now} {Terminal.ModelEquipment} {status.MsgDescription} {status.Status}");
            }
        }

        public IConfiguration Config(string settingsFilePath)
        {
            var CurDir = AppDomain.CurrentDomain.BaseDirectory;
            var AppConfiguration = new ConfigurationBuilder()
                .SetBasePath(CurDir)
                .AddJsonFile(settingsFilePath).Build();
           
            AppConfiguration.GetSection("MID:Equipment").Bind(ListEquipment);
            return AppConfiguration;
        }
        
        /// <summary>
        /// Зміна кольору прапорця
        /// </summary>
        /// <param name="pColor">Власне на який колір</param>
        public void SetColor(Color pColor)
        {
            Signal?.SwitchToColor(pColor);
        }
          
        /// <summary>
        /// Початок зважування на основній вазі
        /// </summary>
        public void StartWeight()
        {
            try
            {
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
                Scale?.StopWeight();
            }
            catch (Exception) { }//Необхідна обробка коли немає обладнання!!!TMP

        }

        /// <summary>
        /// Калібрування контрольної ваги
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public  bool ControlScaleCalibrateMax(double maxValue)
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
    }
}
