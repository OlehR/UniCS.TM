using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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


        public MagellanScaner(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<string, string> pOnBarCode=null) : base(pEquipment, pConfiguration,eModelEquipment.MagellanScaner, pLoggerFactory, pOnBarCode)
        {
            try
            {
                State = eStateEquipment.Init;
                ILogger<Magellan9300S> logger = LoggerFactory?.CreateLogger<Magellan9300S>();
                Magellan9300 = new Magellan9300S(pConfiguration, logger);
                var Res = Magellan9300.Init();
                if (Res == DeviceConnectionStatus.Enabled)
                {                    
                    if (pOnBarCode != null)
                        Magellan9300.OnBarcodeScannerChange += (BarCode) =>
                        {
                            pOnBarCode(BarCode, null);
                        };
                    State = eStateEquipment.On;
                }
                else
                    State = eStateEquipment.Error;
            }
            catch(Exception e)
            {
                State = eStateEquipment.Error;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return;
            }
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Ініціалізація Ваги", State == eStateEquipment.On ? eTypeLog.Expanded:eTypeLog.Error);
            
        }

        public override StatusEquipment TestDevice()
        {
            string Error = null;
            string Res = null;
            try
            {
                Magellan9300.Init();

                Res = Magellan9300.GetInfo().Result;
            }
            catch (Exception e)
            {
                Error = e.Message;
                State = eStateEquipment.Error;
            }
            return new StatusEquipment(Model, State,$"{Error} { Environment.NewLine } {Res}" );
        }
        public override string GetDeviceInfo()
        {
            return $"pModelEquipment={Model} State={State} Port={SerialPort} BaudRate={BaudRate}{Environment.NewLine}";
        }
    }
}
