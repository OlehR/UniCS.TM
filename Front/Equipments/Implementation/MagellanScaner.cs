using Microsoft.Extensions.Configuration;
//using ModernExpo.SelfCheckout.Devices.Magellan9300SingleCable;
using System;
using System.Collections.Generic;
using System.Text;

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
            Magellan9300 = new Magellan9300S(pConfiguration, null);
            Magellan9300.Init();
            if (pOnBarCode != null)
                Magellan9300.OnBarcodeScannerChange += (BarCode) =>
                {
                    pOnBarCode(BarCode, null);
                };
        }
    }
}
