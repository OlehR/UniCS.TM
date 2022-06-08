using Microsoft.Extensions.Configuration;
//using ModernExpo.SelfCheckout.Devices.Magellan9300SingleCable;
using System;
using System.Collections.Generic;
using System.Text;
using Utils;

namespace Front.Equipments
{
    public class MagellanScaner : Scaner
    {
        public Magellan9300S Magellan9300;
/*        public MagellanScaner(string pSerialPortName, int pBaudRate, Action<string, string> pLogger, Action<string, string> pOnBarCode) : base(pSerialPortName, pBaudRate, pLogger, pOnBarCode)
        {
            var AppConfiguration = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("appsettings.json").Build();

            Magellan9300 = new Magellan9300S(AppConfiguration, null);

            Magellan9300.Init();

            if (pOnBarCode != null)
                Magellan9300.OnBarcodeScannerChange += (BarCode) =>
            {
                pOnBarCode(BarCode, null);
            };
        }*/


        public MagellanScaner(Equipment pEquipment, IConfiguration pConfiguration, Action<string, string> pLogger, Action<string, string> pOnBarCode) : base(pEquipment, pConfiguration,eModelEquipment.MagellanScaner, pLogger, pOnBarCode)
        {
            State = eStateEquipment.Init;
            Magellan9300 = new Magellan9300S(pConfiguration, null);
            var Res=Magellan9300.Init();
            if (Res == DeviceConnectionStatus.Enabled)
            {
                State = eStateEquipment.On;
                if (pOnBarCode != null)
                    Magellan9300.OnBarcodeScannerChange += (BarCode) =>
                    {
                        pOnBarCode(BarCode, null);
                    };
            }
            else
                State = eStateEquipment.Error;
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Ініціалізація Ваги", State == eStateEquipment.On ? eTypeLog.Expanded:eTypeLog.Error);
            
        }
    }
}
