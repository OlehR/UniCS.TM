using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class Exelio : Rro
    {
        public Exelio(IConfiguration pConfiguration, Action<string, string> pLogger = null) : base(pConfiguration) { }
        //public Exelio(string pSerialPortName, int pBaudRate, Action<string, string> pLogger) : base(pSerialPortName, pBaudRate,pLogger) { }
    }
}
