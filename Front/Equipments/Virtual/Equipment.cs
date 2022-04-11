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
        protected Action<string, string> Logger = null;
        public Equipment() { }
       
        public Equipment(IConfiguration pConfiguration, eModelEquipment pModelEquipment  = eModelEquipment.NotDefined, Action<string, string> pLogger = null)
        {
            Configuration = pConfiguration;
            ModelEquipment = pModelEquipment;
            Logger = pLogger;
        }
        public bool IsReady { get; set; } = false;
        public eModelEquipment ModelEquipment { get; set; } = eModelEquipment.NotDefined;

        private eStateEquipment _State=eStateEquipment.Off;
        public eStateEquipment State { get { return _State; } set { _State = value; SetState?.Invoke(value); } }
        public static Action<eStateEquipment> SetState { get; set; }
        public  virtual eStateEquipment TestDevice() { throw new NotImplementedException(); }
        public virtual void Enable() { State=eStateEquipment.Ok; }
        public virtual void Disable() { State = eStateEquipment.Off; }  
    }
    
}
