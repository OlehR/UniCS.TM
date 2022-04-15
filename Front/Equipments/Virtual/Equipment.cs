using Microsoft.Extensions.Configuration;
using System;

namespace Front.Equipments
{
    public class Equipment
    {
        protected string SerialPortName;
        protected int BaudRat;
        protected IConfiguration Configuration;        
        
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
        public eStateEquipment State { get { return _State; } set { _State = value; SetState?.Invoke(value, ModelEquipment); } }
        public Action<eStateEquipment, eModelEquipment> SetState { get; set; }

        public  virtual eStateEquipment TestDevice() { throw new NotImplementedException(); }
        public virtual void Enable() { State=eStateEquipment.Ok; }
        public virtual void Disable() { State = eStateEquipment.Off; }  
    }    
}
