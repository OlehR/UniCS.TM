﻿using System;
using Mint.Hardware.ControlScales.BST106M60S;
using System.Collections.Generic;
using System.Text;
/*
 Mint.Hardware.ControlScales.BST106M60S
Клас Scales
Properties
	bool IsReady - маркер готовності роботи
        double ScaleDeltaWeight - похибка ваг
        double GramMultiplier - множник конвертації значення ваг в грами
        Action<double, bool> OnControlWeightChanged - делегат в який будуть направлятися значення з ваг
Methods
	Scales(string serialPortName, int baudRate, Action<string, string> logger = null) - конструктор класу 
		string serialPortName - назва порта, 
		int baudRate - швидкість в бодах (115200), 
		Action<string, string> logger - делегат в який будуть направлятися логи пристрою. Перший параметр - рівень логу, другий - повідомлення
	Task CalibrateMax(double maxValue) - Калібрація ваг
		double maxValue - значення в грамах поклпденого на ваги вантажу
	Task CalibrateZero() - калібрація нуля
	Task<string> GetInfo() - отримати інформацію про пристрій
	bool Init() - запустити комунікацію
	Task<bool> TestDevice() - протестувати пристрій 
 */
namespace Front.Equipments
{
    public class ScaleModern:Scale
    {
        Scales bst;
        public ScaleModern(string pSerialPortName, int pBaudRate = 115200, Action<string, string> pLogger = null, Action<double, bool> pOnScalesData=null) : base(pSerialPortName, pBaudRate, pLogger, pOnScalesData) 
        {
            bst = new Scales(pSerialPortName, pBaudRate, pLogger);
            bst.OnControlWeightChanged = pOnScalesData;
            bst.Init();
        }

        public override eState TestDevice() 
        {
            var r=bst.TestDevice().Result;
            State = r ? eState.Ok : eState.Error;
            return State;
        }

        /// <summary>
        ///  Калібрування Ваги
        /// </summary>
        /// <param name="maxValue">значення в грамах покладеного на ваги вантажу</param>
        public override bool CalibrateMax(double maxValue)
        {
            bst.CalibrateMax(maxValue).Wait();
            return true;
        }

        /// <summary>
        ///  Калібрація нуля
        /// </summary>
        /// <returns></returns>
        public override bool CalibrateZero() 
        {
            bst.CalibrateZero().Wait();
            return true;
        }
    }
}
