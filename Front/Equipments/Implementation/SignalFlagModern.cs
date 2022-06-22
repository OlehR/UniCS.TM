using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModernExpo.SelfCheckout.Devices.CustomFlagLamp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Utils;

/*
  Mint.Hardware.Lamps.Lamp
Клас FlagLamp
Properties
	bool IsReady - маркер готовності роботи
Methods
	FlagLamp(string serialPortName, int baudRate, Action<string, string> logger) - конструктор класу 
		string serialPortName - назва порта, 
		int baudRate - швидкість в бодах (9600), 
		Action<string, string> logger - делегат в який будуть направлятися логи пристрою. Перший параметр - рівень логу, другий - повідомлення
	void Disable() - вимкнути лампу та заборонити зміну її стану
        void Enable() - дозволити зміну стану лампи
        Task<string> GetInfo() - отримати інформацію про пристрій
        bool Init() - запустити комунікацію
        bool SwitchToColor(Color? color) - встановити певний колір
		System.Drawing.Color? color - колір лампи
        Task<bool> TestDevice() - протестувати пристрій
 */

namespace Front.Equipments
{
    class SignalFlagModern : SignalFlag
    {
        CustomFlagLamp lamp;
        public SignalFlagModern(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null) : base(pEquipment, pConfiguration,eModelEquipment.SignalFlagModern, pLoggerFactory)
        {
            try
            {
                State = eStateEquipment.Init;
                ILogger<CustomFlagLamp> logger = LoggerFactory?.CreateLogger<CustomFlagLamp>();
                lamp = new CustomFlagLamp(pConfiguration, logger);
                lamp.Init();
                State = eStateEquipment.On;
            }
            catch (Exception e)
            {
                State = eStateEquipment.Error;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            };
        }

        public override void SwitchToColor(Color pColor) { lamp.SwitchToColor(pColor); }
        public override Color GetCurrentColor() { throw new NotImplementedException();}

        public override void Enable() { lamp.Enable(); base.Enable(); }
        public override void Disable() { lamp.Disable(); base.Disable(); }

        public override string GetDeviceInfo()
        {
            return $"pModelEquipment={Model} State={State} Port={SerialPort} BaudRate={BaudRate}{Environment.NewLine}";
        }

        public override StatusEquipment TestDevice()
        {
            string Error = null;
            string Res = null;
            try
            {                
                State = eStateEquipment.Init;
                lamp.Init();
                lamp.Enable();
                Res =lamp.GetInfo().Result;
                lamp.SwitchToColor(Color.Yellow);

                lamp.SwitchToColor(Color.Blue);
                lamp.SwitchToColor(Color.Black);

                State = eStateEquipment.On;
            }
            catch (Exception e)
            {
                Error = e.Message;
                State = eStateEquipment.Error;
            }
            return new StatusEquipment(Model, State, $"{Error} {Environment.NewLine} {Res}");
        }



    }
}
