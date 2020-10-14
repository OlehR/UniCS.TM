using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.Extensions.Configuration;

namespace Front.Equipments
{
    public class SignalFlag:Equipment
    {
       public SignalFlag(string pSerialPortName, int pBaudRate, Action<string, string> pLogger) :base(pSerialPortName, pBaudRate) { }

       public SignalFlag(IConfiguration pConfiguration, Action<string, string> pLogger) : base(pConfiguration) { }
       public virtual void SwitchToColor(Color pColor) { throw new NotImplementedException(); }
       public virtual Color GetCurrentColor() { throw new NotImplementedException(); }
    }
}
