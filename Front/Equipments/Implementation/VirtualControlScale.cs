using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments.Implementation
{
    public class VirtualControlScale : Scale
    {       
        public VirtualControlScale() { }
        /*public Scale(string pSerialPortName, int pBaudRate, Action<string, string> pLogger, Action<double, bool> pOnScalesData) : base(pSerialPortName, pBaudRate)
        { OnScalesData = pOnScalesData; }*/

        public VirtualControlScale(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<double, bool> pOnScalesData = null) 
            :  base(pEquipment, pConfiguration, eModelEquipment.VirtualControlScale, pLoggerFactory, pOnScalesData)
        { OnScalesData = pOnScalesData; }
        /// <summary>
        ///  Калібрування Ваги
        /// </summary>
        /// <param name="maxValue">значення в грамах покладеного на ваги вантажу</param>
        public override bool CalibrateMax(double maxValue) { throw new NotImplementedException(); }

        /// <summary>
        ///  Калібрація нуля
        /// </summary>
        /// <returns></returns>
        public override bool CalibrateZero() { throw new NotImplementedException(); }
        //public override void Enable() { base.Enable(); }
        //public override void Disable() { base.Disable(); }

        public override void StartWeight() { throw new NotImplementedException(); }

        public override void StopWeight() { throw new NotImplementedException(); }

    }
}
