using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Mint.Hardware.Lamps.Lamp;

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
        FlagLamp lamp;
        public SignalFlagModern(string pSerialPortName, int pBaudRate = 9600, Action<string, string> pLogger = null) : base(pSerialPortName, pBaudRate, pLogger)
        {
        //    lamp = new FlagLamp(pSerialPortName, pBaudRate, (w, s) => { Console.WriteLine($"Lamp Log - {DateTime.Now:dd-MM-yyyy HH:mm:ss}:{w} - {s}"); });
         //   lamp.Init();
        }
        public override void SwitchToColor(Color pColor) { lamp.SwitchToColor(pColor); }
        public override Color GetCurrentColor() { throw new NotImplementedException();/*var color = lamp.GetInfo().Result;*/ }

        public override void Enable() { lamp.Enable(); base.Enable(); }
        public override void Disable() { lamp.Disable(); base.Disable(); }




    }
}
