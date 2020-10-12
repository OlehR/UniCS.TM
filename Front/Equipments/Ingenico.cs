using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class Ingenico:BankTerminal
    {
        public Ingenico(string pSerialPortName, int pBaudRate = 9600, Action<string, string> pLogger = null) : base(pSerialPortName, pBaudRate, pLogger) { }
    }
}
