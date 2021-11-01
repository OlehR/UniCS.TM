using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class Exelio:Rro
    {
        public Exelio(string pSerialPortName, int pBaudRate, Action<string, string> pLogger) : base(pSerialPortName, pBaudRate,pLogger) { }
    }
}
