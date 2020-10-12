using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class Equipment
    {
        protected string SerialPortName;
        protected int BaudRat;
       //public Equipment(string serialPortName, int baudRate, Action<string, string> logger) { throw new NotImplementedException(); }
        public Equipment(string pSerialPortName, int pBaudRate) 
        {
            SerialPortName = pSerialPortName;
            BaudRat = pBaudRate;
        }
        public bool IsReady { get; set; } = false;
        public eState State { get; set; } = eState.Off;
        public  virtual eState TestDevice() { throw new NotImplementedException(); }
        public virtual void Enable() { State=eState.Ok; }
        public virtual void Disable() { State = eState.Off; }        

    }
    public enum eState {Ok,Off,Error}
}
