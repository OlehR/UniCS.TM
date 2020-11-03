using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class Equipment
    {
        protected string SerialPortName;
        protected int BaudRat;
        protected IConfiguration Configuration;
        //public Equipment(string serialPortName, int baudRate, Action<string, string> logger) { throw new NotImplementedException(); }

        public Equipment() { }
        public Equipment(string pSerialPortName, int pBaudRate) 
        {
            SerialPortName = pSerialPortName;
            BaudRat = pBaudRate;
        }
        public Equipment(IConfiguration pConfiguration)
        {
            Configuration = pConfiguration;
        }
        public bool IsReady { get; set; } = false;
        private eStateEquipment _State=eStateEquipment.Off;
        public eStateEquipment State { get { return _State; } set { _State = value; SetState?.Invoke(value); } }
        public static Action<eStateEquipment> SetState { get; set; }
        public  virtual eStateEquipment TestDevice() { throw new NotImplementedException(); }
        public virtual void Enable() { State=eStateEquipment.Ok; }
        public virtual void Disable() { State = eStateEquipment.Off; }  
    }
    public enum eStateEquipment {Ok,Off,Error}
}
