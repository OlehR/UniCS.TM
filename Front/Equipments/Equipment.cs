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
        public eState State { get; set; } = eState.Off;
        public  virtual eState TestDevice() { throw new NotImplementedException(); }
        public virtual void Enable() { State=eState.Ok; }
        public virtual void Disable() { State = eState.Off; }        

    }
    public enum eState {Ok,Off,Error}
}
