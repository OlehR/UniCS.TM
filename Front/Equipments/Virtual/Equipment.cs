using Front.Equipments.Implementation;
using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Utils;

namespace Front.Equipments
{
    public class Equipment
    {
        // <summary>
        // Компанія для обладнання Важливо для РРО  (Кілька підприємців)
        // </summary>
        //public int Company { get; set; }
        public string Name { get; set; }
        public bool IsСritical { get; set; } = true;
        public bool IsReady { get; set; } = false;
        public eModelEquipment Model { get; set; } = eModelEquipment.NotDefine;
        public eTypeEquipment Type { get { return Model.GetTypeEquipment(); } }

        protected string SerialPort;
        protected int BaudRate;
        protected string IP;
        protected int IpPort;
        protected IConfiguration Configuration;

        //protected Action<string, string> Logger = null;
        protected ILoggerFactory LoggerFactory;
        public Action<StatusEquipment> ActionStatus;

        //public Action<eStateEquipment, eModelEquipment> SetState { get; set; }
        public string InfoConnect { get { return (!string.IsNullOrEmpty(SerialPort) && BaudRate > 0) ? $" Port={SerialPort} BaudRate={BaudRate}" : $"IP ={IP} IpPort = {IpPort}"; } }

        private eStateEquipment _State = eStateEquipment.Off;
        public eStateEquipment State { get { return _State; } set
            {
                if (_State != value || value != eStateEquipment.On)
                { _State = value;
                    ActionStatus?.Invoke(new StatusEquipment(Model, _State));
                    FileLogger.WriteLogMessage($"Equipment.SetState( {_State}) {Model}");
                }
            }
        }

        public Equipment() { }

        /*public Equipment(IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefined, Action<string, string> pLogger = null)
        {
            Configuration = pConfiguration;
            Model = pModelEquipment;
            Logger = pLogger;
        }*/

        public Equipment(Equipment pEquipment, IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefine, ILoggerFactory pLoggerFactory= null)
        {
            Configuration = pConfiguration;
            LoggerFactory = pLoggerFactory;
            Model = pModelEquipment;
            if (pEquipment != null)
            {
                Name = pEquipment.Name;
                IsСritical = pEquipment.IsСritical;
                Model = pEquipment.Model;
            }
        }

        public virtual StatusEquipment TestDevice() { throw new NotImplementedException(); }
        public virtual string GetDeviceInfo() { throw new NotImplementedException(); }
        public virtual void Enable() { State = eStateEquipment.On; }
        public virtual void Disable() { State = eStateEquipment.Off; }
    }
}
