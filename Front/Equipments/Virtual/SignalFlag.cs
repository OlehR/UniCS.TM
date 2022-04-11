using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.Extensions.Configuration;

namespace Front.Equipments
{
    public class SignalFlag:Equipment
    {
       public SignalFlag(IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefined, Action<string, string> pLogger=null) : base(pConfiguration, pModelEquipment, pLogger) { }
       public virtual void SwitchToColor(Color pColor) { throw new NotImplementedException(); }
       public virtual Color GetCurrentColor() { throw new NotImplementedException(); }
    }
}
