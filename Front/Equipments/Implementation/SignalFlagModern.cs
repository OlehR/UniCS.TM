using Microsoft.Extensions.Configuration;
using ModernExpo.SelfCheckout.Devices.CustomFlagLamp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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
        public SignalFlagModern(IConfiguration pConfiguration, Action<string, string> pLogger = null) : base(pConfiguration,eModelEquipment.SignalFlagModern, pLogger)
        {
            try
            {
                lamp = new CustomFlagLamp(pConfiguration, null);
                lamp.Init();
            }
            catch (Exception Ex)
            { };
        }

        public override void SwitchToColor(Color pColor) { lamp.SwitchToColor(pColor); }
        public override Color GetCurrentColor() { throw new NotImplementedException();}

        public override void Enable() { lamp.Enable(); base.Enable(); }
        public override void Disable() { lamp.Disable(); base.Disable(); }




    }
}
