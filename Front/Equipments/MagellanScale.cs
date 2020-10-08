using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    class MagellanScale:Scale
    {
        public MagellanScale(string pSerialPortName, int pBaudRate, Action<string, string> pLogger, Action<double, bool> pOnScalesData) : base(pSerialPortName, pBaudRate, pLogger, pOnScalesData) { }
    }
}
