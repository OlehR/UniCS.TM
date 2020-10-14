using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class Ingenico:BankTerminal
    {
        ModernExpo.SelfCheckout.Devices.Ingenico.Ingenico EquipmentIngenico;
        public Ingenico(string pSerialPortName, int pBaudRate = 9600, Action<string, string> pLogger = null) : base(pSerialPortName, pBaudRate, pLogger) { }
        public Ingenico(IConfiguration pConfiguration, Action<string, string> pLogger = null) : base(pConfiguration, pLogger) 
        {
            EquipmentIngenico = new ModernExpo.SelfCheckout.Devices.Ingenico.Ingenico(pConfiguration, null);
        }

        public override void Purchase(decimal pAmount) 
        {
            EquipmentIngenico.Purchase( Convert.ToDouble(pAmount));
        }
        public override void Refund(decimal pAmount, string pRRN) 
        {
            EquipmentIngenico.Refund(Convert.ToDouble(pAmount), pRRN);
        }

    }
}
