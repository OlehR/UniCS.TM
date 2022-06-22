﻿using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using ModernExpo.SelfCheckout.Devices.BST106M60S;
using System;

using System.Collections.Generic;
using System.Text;
using Utils;
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
        public ScaleModern(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<double, bool> pOnScalesData=null) : base(pEquipment, pConfiguration, eModelEquipment.ScaleModern, pLoggerFactory, pOnScalesData) 
        {
            try
            {
                State = eStateEquipment.Init;
                bst = new Scales(pConfiguration, null);
                bst.OnControlWeightChanged = pOnScalesData;
                bst.Init();
                State = eStateEquipment.On;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                State = eStateEquipment.Error;
            }
        }

        public override StatusEquipment TestDevice() 
        {
            var r=bst.TestDevice().Result;
            State = r==ModernExpo.SelfCheckout.Entities.Enums.Device.DeviceConnectionStatus.Enabled ? eStateEquipment.On : eStateEquipment.Error;
            return new StatusEquipment(Model, State,"");
        }

        public override string GetDeviceInfo()
        {
            return bst.GetInfo().Result;// $"pModelEquipment={Model} State={State} Port={SerialPort} BaudRate={BaudRate}{Environment.NewLine}";
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
