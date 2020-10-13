using Microsoft.Extensions.Configuration;
//using ModernExpo.SelfCheckout.Devices.Magellan9300SingleCable;
using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class MagellanScaner: Scaner
    {
        public Magellan9300S Magellan9300;
        public MagellanScaner(string pSerialPortName, int pBaudRate, Action<string, string> pLogger, Action<string, string> pOnBarCode) : base(pSerialPortName, pBaudRate, pLogger, pOnBarCode) 
        {
            var AppConfiguration = new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("appsettings.json").Build();
            Magellan9300 = new Magellan9300S(AppConfiguration, null);

            
            var s = AppConfiguration["Devices:Magellan9300S:Port"];

            Magellan9300.Init();
            var aa =  Magellan9300.GetDeviceStatus();


            var zz = aa.Result;

            Magellan9300.OnBarcodeScannerChange += (BarCode) => 
            { 
                pOnBarCode(BarCode, null); 
            };
        }
    }
}
