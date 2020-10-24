using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class BankTerminal:Equipment
    {
        public BankTerminal(string pSerialPortName, int pBaudRate = 9600, Action<string, string> pLogger = null) : base(pSerialPortName, pBaudRate) { }

        public BankTerminal(IConfiguration pConfiguration, Action<string, string> pLogger = null) : base(pConfiguration) { }
        virtual public bool PrintZ()
        {
            throw new NotImplementedException();
        }

        virtual public bool PrintX()
        {
            throw new NotImplementedException();
        }
        public virtual void Purchase(decimal pAmount) { throw new NotImplementedException(); }
        public virtual void Refund(decimal pAmount, string pRRN) { throw new NotImplementedException(); }


    }
}
