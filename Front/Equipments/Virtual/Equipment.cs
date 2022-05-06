using Front.Equipments.Implementation;
using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using System;

namespace Front.Equipments
{
    public class Equipment
    {
        public string Name { get; set; }
        public bool IsСritical { get; set; } = true;
        public bool IsReady { get; set; } = false;
        public eModelEquipment Model { get; set; } = eModelEquipment.NotDefined;
        public eTypeEquipment Type { get { return Model.GetTypeEquipment(); } }

        protected string SerialPort;
        protected int BaudRate;
        protected string IP;
        protected int IpPort;
        protected IConfiguration Configuration;

        protected Action<string, string> Logger = null;
        protected Action<StatusEquipment> ActionStatus;

        public Action<eStateEquipment, eModelEquipment> SetState { get; set; }

        private eStateEquipment _State = eStateEquipment.Off;
        public eStateEquipment State { get { return _State; } set { _State = value; SetState?.Invoke(value, Model); } }

        public Equipment() { }

        /*public Equipment(IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefined, Action<string, string> pLogger = null)
        {
            Configuration = pConfiguration;
            Model = pModelEquipment;
            Logger = pLogger;
        }*/

        public Equipment(Equipment pEquipment, IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefined, Action<string, string> pLogger = null)
        {
            Configuration = pConfiguration;
            Logger = pLogger;
            Model = pModelEquipment;
            if (pEquipment != null)
            {
                Name = pEquipment.Name;
                IsСritical = pEquipment.IsСritical;
                Model = pEquipment.Model;
            }
        }

        public virtual eStateEquipment TestDevice() { throw new NotImplementedException(); }
        public virtual string GetDeviceInfo() { throw new NotImplementedException(); }
        public virtual void Enable() { State = eStateEquipment.Ok; }
        public virtual void Disable() { State = eStateEquipment.Off; }
    }
}
