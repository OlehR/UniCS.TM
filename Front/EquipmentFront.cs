﻿using Front.Equipments;
using Microsoft.Extensions.Configuration;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Front
{
    public class EquipmentFront
    {
        private List<EquipmentElement> ListEquipment = new List<EquipmentElement>();
        BL Bl;
        Scaner Scaner;
        Scale Scale;
        Scale ControlScale;
        SignalFlag Signal;

        BankTerminal Terminal;
        EKKA EKKA;


        public EquipmentFront(BL pBL) 
        {
            Bl = pBL;
            Config("appsettings.json");

            //Scaner
            var ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Scaner).First();
            if (ElEquipment.Model == eModel.MagellanScaner)
                ElEquipment.Equipment = new MagellanScaner(ElEquipment.Port, ElEquipment.BaudRate,null, GetBarCode);
            else
                ElEquipment.Equipment = new Scaner(ElEquipment.Port, ElEquipment.BaudRate, null, GetBarCode);
            Scaner = (Scaner)ElEquipment.Equipment;

            //Scale
            ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Scale).First();
            if (ElEquipment.Model == eModel.MagellanScale)
                ElEquipment.Equipment = new MagellanScale(ElEquipment.Port, ElEquipment.BaudRate, null, GetScale);
            else
                ElEquipment.Equipment = new Scale(ElEquipment.Port, ElEquipment.BaudRate, null, GetScale);
            Scale = (Scale)ElEquipment.Equipment;

            //ControlScale
            ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.ControlScale).First();
            if (ElEquipment.Model == eModel.ScaleModern)
                ElEquipment.Equipment = new ScaleModern(ElEquipment.Port, ElEquipment.BaudRate, null, GetControlScale);
            else
                ElEquipment.Equipment = new Scale(ElEquipment.Port, ElEquipment.BaudRate, null, GetControlScale);
            ControlScale = (Scale)ElEquipment.Equipment;

            //Flag
            ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.Signal).First();
            if (ElEquipment.Model == eModel.SignalFlagModern)
                ElEquipment.Equipment = new SignalFlagModern(ElEquipment.Port, ElEquipment.BaudRate, null);
            else
                ElEquipment.Equipment = new SignalFlag(ElEquipment.Port, ElEquipment.BaudRate, null);
            Signal = (SignalFlag)ElEquipment.Equipment;

            //Terminal
            ElEquipment = ListEquipment.Where(e => e.Type == eTypeEquipment.BankTerminal).First();
            if (ElEquipment.Model == eModel.Ingenico)
                ElEquipment.Equipment = new Ingenico(ElEquipment.Port, ElEquipment.BaudRate, null);
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

        private void GetBarCode(string pBarCode, string pTypeBarCode)
        {
            Bl.AddWaresBarCode( pBarCode, 1);
        }

        private void GetScale(double pWeight, bool pIsStable)
        {
            
        }

        private void GetControlScale(double pWeight, bool pIsStable)
        {

        }

    }
}
