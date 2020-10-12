using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    
        public class Scaner : Equipment
        {
            public Scaner(string pSerialPortName, int pBaudRate, Action<string, string> pLogger, Action<string, string> pOnBarCode) : base(pSerialPortName, pBaudRate) { }           

         
        }
    
}
