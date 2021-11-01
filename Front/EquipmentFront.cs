using Front.Equipments;
using Front.Equipments.Ingenico;
using Microsoft.Extensions.Configuration;
using ModelMID;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;


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

        public static Action<eStateEquipment> SetState { get; set; }

        static EquipmentFront sEquipmentFront;

        public EquipmentFront(Action<string, string> pSetBarCode, Action<double, bool> pSetWeight, Action<double, bool> pSetControlWeight)
        {            
            //public static Action<IEnumerable<ReceiptWares>, Guid> OnReceiptCalculationComplete { get; set; }
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            
            sEquipmentFront = this;
            var config = Config("appsettings.json");          

            //Scaner
            var ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Scaner).First();
            if (ElEquipment.Model == eModel.MagellanScaner)
                ElEquipment.Equipment = new MagellanScaner(config, null, pSetBarCode);
            else
                ElEquipment.Equipment = new Scaner(ElEquipment.Port, ElEquipment.BaudRate, null, pSetBarCode);
            Scaner = (Scaner)ElEquipment.Equipment;

            //Scale
            ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Scale).First();
            if (ElEquipment.Model == eModel.MagellanScale)
                ElEquipment.Equipment = new MagellanScale(((MagellanScaner)Scaner).Magellan9300, pSetWeight); //MagellanScale(ElEquipment.Port, ElEquipment.BaudRate, null, GetScale);
            else
                ElEquipment.Equipment = new Scale(ElEquipment.Port, ElEquipment.BaudRate, null, pSetWeight);
            Scale = (Scale)ElEquipment.Equipment;

            //ControlScale
            ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.ControlScale).First();
            if (ElEquipment.Model == eModel.ScaleModern)
                ElEquipment.Equipment = new ScaleModern(config, null, pSetControlWeight);
            else
                ElEquipment.Equipment = new Scale(ElEquipment.Port, ElEquipment.BaudRate, null, pSetControlWeight);
            ControlScale = (Scale)ElEquipment.Equipment;

            //Flag
            ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Signal).First();
            if (ElEquipment.Model == eModel.SignalFlagModern)
                ElEquipment.Equipment = new SignalFlagModern(config, null);
            else
                ElEquipment.Equipment = new SignalFlag(ElEquipment.Port, ElEquipment.BaudRate, null);
            Signal = (SignalFlag)ElEquipment.Equipment;
            
            //Terminal
            ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.BankTerminal).First();
            if (ElEquipment.Model == eModel.Ingenico)
                ElEquipment.Equipment = new IngenicoH(config, null, aPosStatus);
            else
                ElEquipment.Equipment = new BankTerminal(ElEquipment.Port, ElEquipment.BaudRate, null);
            Terminal = (BankTerminal)ElEquipment.Equipment;


            //EKKA
            ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.EKKA).First();
            if (ElEquipment.Model == eModel.Ingenico)
                ElEquipment.Equipment = new Exelio(ElEquipment.Port, ElEquipment.BaudRate, null);
            else
                ElEquipment.Equipment = new Rro(ElEquipment.Port, ElEquipment.BaudRate, null);
            RRO = (Rro)ElEquipment.Equipment;            
        }

        public static EquipmentFront GetEquipmentFront { get { return sEquipmentFront; } }

        public IEnumerable<EquipmentElement> GetListEquipment { get { return ListEquipment; } }
        
        /// <summary>
        /// Друк чека
        /// </summary>
        public bool PrintReceipt(Receipt pReceipt) 
        {
            return false;
        }


        public bool RroPrintX()
        {
            return RRO.PrintX();
        }

        public bool RroPrintZ()
        {
            return RRO.PrintZ();
        }

        public bool RroPrintCopyReceipt()
        {
            return RRO.PrintCopyReceipt();
        }

        /// <summary>
        /// Оплата по банківському терміналу
        /// </summary>
        /// <param name="pSum">Власне сума</param>
        /// <returns></returns>
        public PaymentResultModel Purchase(decimal pSum) {
            return Terminal.Purchase(pSum);
        }

        /// <summary>
        /// Повернення покупцю грошей по банківському терміналу
        /// </summary>
        /// <param name="pSum"></param>
        /// <param name="pRNN"></param>
        /// <returns></returns>
        public PaymentResultModel RE(decimal pSum,string pRNN)
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
             RRO.PrintZ();
            return true;
        }

        

        /// <summary>
        /// Статус банківського термінала (Очікуєм карточки, Очікуєм підтвердження і ТД) 
        /// </summary>
        /// <param name="ww"></param>
        void aPosStatus(Front.Equipments.Ingenico.IPosStatus ww)
        {
            if (ww is Front.Equipments.Ingenico.PosStatus status)
            {
                //TMP!!!!
                //Bl.PosStatus = status.Status. GetPosStatusFromStatus();
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
