using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class BankTerminal:Equipment
    {
        public BankTerminal(string pSerialPortName, int pBaudRate = 9600, Action<string, string> pLogger = null) : base(pSerialPortName, pBaudRate) { }
    }
}
