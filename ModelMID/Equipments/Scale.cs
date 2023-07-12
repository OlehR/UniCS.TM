using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class Scale : Equipment
    {
        protected Action<double, bool> OnScalesData;
        public Scale() { }
        /*public Scale(string pSerialPortName, int pBaudRate, Action<string, string> pLogger, Action<double, bool> pOnScalesData) : base(pSerialPortName, pBaudRate)
        { OnScalesData = pOnScalesData; }*/

        public Scale(Equipment pEquipment, IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefine, ILoggerFactory pLoggerFactory = null, Action<double, bool> pOnScalesData=null) : base(pEquipment, pConfiguration,pModelEquipment, pLoggerFactory)
        { OnScalesData = pOnScalesData; }
        /// <summary>
        ///  Калібрування Ваги
        /// </summary>
        /// <param name="maxValue">значення в грамах покладеного на ваги вантажу</param>
        public virtual bool CalibrateMax(double maxValue) { throw new NotImplementedException(); }

        /// <summary>
        ///  Калібрація нуля
        /// </summary>
        /// <returns></returns>
        public virtual bool CalibrateZero() { throw new NotImplementedException(); }
        //public override void Enable() { base.Enable(); }
        //public override void Disable() { base.Disable(); }

        public virtual void StartWeight() { throw new NotImplementedException(); }

        public virtual void StopWeight(){ throw new NotImplementedException();  }

    }
}
