using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class MagellanScaner: Scaner
    {
        public MagellanScaner(string pSerialPortName, int pBaudRate, Action<string, string> pLogger, Action<string, string> pOnBarCode) : base(pSerialPortName, pBaudRate, pLogger, pOnBarCode) { }
    }
}
