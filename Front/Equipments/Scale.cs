using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class Scale:Equipment
    {
        public Scale(string pSerialPortName, int pBaudRate, Action<string, string> pLogger, Action<double, bool > pOnScalesData) : base(pSerialPortName, pBaudRate) { }
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
    }
}
