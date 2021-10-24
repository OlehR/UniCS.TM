using Microsoft.Extensions.Configuration;
//using ModernExpo.SelfCheckout.Entities.Pos;
using Front.Equipments.Ingenico;
using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class BankTerminal:Equipment
    {
        public BankTerminal(string pSerialPortName, int pBaudRate = 9600, Action<string, string> pLogger = null) : base(pSerialPortName, pBaudRate) { }

        public BankTerminal(IConfiguration pConfiguration, Action<string, string> pLogger = null) : base(pConfiguration) { }
        virtual public BatchTotals PrintZ()
        {
            throw new NotImplementedException();
        }

        virtual public BatchTotals PrintX()
        {
            throw new NotImplementedException();
        }
        public virtual PaymentResultModel Purchase(decimal pAmount) { throw new NotImplementedException(); }
        public virtual PaymentResultModel Refund(decimal pAmount, string pRRN) { throw new NotImplementedException(); }


    }
}
