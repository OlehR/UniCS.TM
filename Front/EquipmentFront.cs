using Front.Equipments;
using Microsoft.Extensions.Configuration;
using ModernExpo.SelfCheckout.Entities.Pos;
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
        BL Bl; //!!!!костиль.
        //MainWindow MW;
        Scaner Scaner;
        Scale Scale;
        Scale ControlScale;
        SignalFlag Signal;

        public BankTerminal Terminal;
        public EKKA EKKA;

        
        eStateEquipment _State = eStateEquipment.Off;
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

        public EquipmentFront(BL pBL)
        {
            Bl = pBL;
            //MW = pMW;
            sEquipmentFront = this;
            var config = Config("appsettings.json");

            //Scaner
            var ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Scaner).First();
            if (ElEquipment.Model == eModel.MagellanScaner)
                ElEquipment.Equipment = new MagellanScaner(config, null, Bl.GetBarCode);
            else
                ElEquipment.Equipment = new Scaner(ElEquipment.Port, ElEquipment.BaudRate, null, Bl.GetBarCode);
            Scaner = (Scaner)ElEquipment.Equipment;

            //Scale
            ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Scale).First();
            if (ElEquipment.Model == eModel.MagellanScale)
                ElEquipment.Equipment = new MagellanScale(((MagellanScaner)Scaner).Magellan9300, Bl.GetScale); //MagellanScale(ElEquipment.Port, ElEquipment.BaudRate, null, GetScale);
            else
                ElEquipment.Equipment = new Scale(ElEquipment.Port, ElEquipment.BaudRate, null, Bl.GetScale);
            Scale = (Scale)ElEquipment.Equipment;

            //ControlScale
            ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.ControlScale).First();
            if (ElEquipment.Model == eModel.ScaleModern)
                ElEquipment.Equipment = new ScaleModern(config, null, Bl.CS.OnScalesData);
            else
                ElEquipment.Equipment = new Scale(ElEquipment.Port, ElEquipment.BaudRate, null, Bl.CS.OnScalesData);
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
                ElEquipment.Equipment = new Ingenico(config, null, aPosStatus);
            else
                ElEquipment.Equipment = new BankTerminal(ElEquipment.Port, ElEquipment.BaudRate, null);
            Terminal = (BankTerminal)ElEquipment.Equipment;


            //EKKA
            ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.EKKA).First();
            if (ElEquipment.Model == eModel.Ingenico)
                ElEquipment.Equipment = new Exelio(ElEquipment.Port, ElEquipment.BaudRate, null);
            else
                ElEquipment.Equipment = new EKKA(ElEquipment.Port, ElEquipment.BaudRate, null);
            EKKA = (EKKA)ElEquipment.Equipment;

        }

        public static EquipmentFront GetEquipmentFront { get { return sEquipmentFront; } }

        public IEnumerable<EquipmentElement> GetListEquipment { get { return ListEquipment; } }
        public void PrintReceipt() { }
        public void Pay() { }

        public IConfiguration Config(string settingsFilePath)
        {
            var CurDir = AppDomain.CurrentDomain.BaseDirectory;
            var AppConfiguration = new ConfigurationBuilder()
                .SetBasePath(CurDir)
                .AddJsonFile(settingsFilePath).Build();
           
            AppConfiguration.GetSection("MID:Equipment").Bind(ListEquipment);
            return AppConfiguration;
        }

               
       
        public void SetColor(Color pColor)
        {
            Signal.SwitchToColor(pColor);
        }

       
        void aPosStatus(IPosStatus ww)
        {
            if (ww is PosStatus status)
            {
                Bl.PosStatus = status.Status.GetPosStatusFromStatus();
            }
        }
    }
}
