using Equipments.Equipments;
using Front.Equipments.Utils;
using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
//using ModernExpo.SelfCheckout.Devices.Magellan9300SingleCable;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using System.Timers;
using Utils;

namespace Front.Equipments
{
    public class ZebraScaner : Scaner
    {

        System.Timers.Timer mTimer;
        public bool IsMultipleTone { get; set; } = true;
        Regex Reg = new (@"^[0-9]{8,13}[S]{1}[0-9]{7}$");
        public ZebraCoreScanner Zebra;
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


        public ZebraScaner(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<string, string> pOnBarCode=null) : base(pEquipment, pConfiguration,eModelEquipment.MagellanScaner, pLoggerFactory, pOnBarCode)
        {
            try
            {
                State = eStateEquipment.Init;
                ILogger<ZebraScaner> logger = LoggerFactory?.CreateLogger<ZebraScaner>();
                Zebra =  ZebraCoreScanner.GetZebraCoreScanner;
                
                if (Zebra.CurrentStatus == eDeviceConnectionStatus.Enabled)
                {                    
                    if (pOnBarCode != null)
                        Zebra.OnBarcodeScannerChange += (BarCode) =>
                        {
                            // Інколи з штрихкодом приходитьвага. Відрізаємо вагу
                            //if  (Reg.IsMatch(BarCode))                           
                            //     BarCode = BarCode.Substring(0, BarCode.IndexOf('S'));// BarCode.IndexOf('S'));                                
                            
                            pOnBarCode(BarCode, null);
                            //ForceGoodReadTone();
                        };
                    State = eStateEquipment.On;
                }
                else
                    State = eStateEquipment.Error;

                mTimer = new System.Timers.Timer(120);
                mTimer.AutoReset = true;
                mTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
                //mTimer.Start();
            }
            catch(Exception e)
            {
                State = eStateEquipment.Error;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return;
            }
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Ініціалізація Ваги", State == eStateEquipment.On ? eTypeLog.Expanded:eTypeLog.Error);
            
        }

        private async void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (IsMultipleTone)
                ForceGoodReadTone();
        }

        public override StatusEquipment TestDevice()
        {
            string Error = null;
            string Res = null;
            try
            {
                Zebra.Init();

                Res = Zebra.GetInfo();
                State = string.IsNullOrEmpty(Res) ? eStateEquipment.Error: eStateEquipment.On;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                Error = e.Message;
                State = eStateEquipment.Error;
            }
            return new StatusEquipment(Model, State,$"{Error} { Environment.NewLine } {Res}" );
        }
        
        public override void ForceGoodReadTone()
        {
            Zebra?.ForceGoodReadTone();
        }

        public override void StartMultipleTone() { mTimer.Start(); }
        public override void StopMultipleTone() { mTimer.Stop(); }
    }
}
