using Microsoft.Extensions.Configuration;
using ModernExpo.SelfCheckout.Entities.Pos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class Ingenico:BankTerminal
    {
        ModernExpo.SelfCheckout.Devices.Ingenico.Ingenico EquipmentIngenico;
        public Ingenico(string pSerialPortName, int pBaudRate = 9600, Action<string, string> pLogger = null) : base(pSerialPortName, pBaudRate, pLogger) { }
        public Ingenico(IConfiguration pConfiguration, Action<string, string> pLogger = null, Action<IPosStatus> pActionStatus = null) : base(pConfiguration, pLogger) 
        {
            EquipmentIngenico = new ModernExpo.SelfCheckout.Devices.Ingenico.Ingenico(pConfiguration, null);
            EquipmentIngenico.OnStatus += pActionStatus;
        }

        public override PaymentResultModel Purchase(decimal pAmount) 
        {
            return EquipmentIngenico.Purchase( Convert.ToDouble(pAmount)).Result;
        }
        public override PaymentResultModel Refund(decimal pAmount, string pRRN)
        {
            return EquipmentIngenico.Refund(Convert.ToDouble(pAmount), pRRN).Result;
        }

        public override BatchTotals PrintZ()
        {
            var r=EquipmentIngenico.GetBatchTotals();
            return r.Result;
        }

        public override BatchTotals PrintX()
        {
            var r = EquipmentIngenico.GetBatchTotals();
            return r.Result;
        }

        public override eStateEquipment TestDevice() 
        { 
            EquipmentIngenico.TestDeviceSync();
            return eStateEquipment.Ok;
        }


    }
}
