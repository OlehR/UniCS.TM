using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Front.Equipments
{
    public class SignalFlag:Equipment
    {
       public SignalFlag(Equipment pEquipment, IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefined, ILoggerFactory pLoggerFactory = null) : base(pEquipment, pConfiguration,pModelEquipment, pLoggerFactory) { }
       public virtual void SwitchToColor(Color pColor) { throw new NotImplementedException(); }
       public virtual Color GetCurrentColor() { throw new NotImplementedException(); }
    }
}
