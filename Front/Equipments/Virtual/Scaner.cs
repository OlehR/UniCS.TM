using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{

    public class Scaner : Equipment
    {
        protected Action<string, string> OnBarCode;
        public Scaner(Equipment pEquipment, IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefine, ILoggerFactory pLoggerFactory = null, Action<string, string> pOnBarCode = null) : base(pEquipment, pConfiguration,pModelEquipment, pLoggerFactory) { OnBarCode = pOnBarCode; }
        public virtual void ForceGoodReadTone() { throw new NotImplementedException(); }
        public virtual void StartMultipleTone() { throw new NotImplementedException(); }
        public virtual void StopMultipleTone() { throw new NotImplementedException(); }
    }

}
