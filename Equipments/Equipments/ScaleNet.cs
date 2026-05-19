using Front.Equipments.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelMID;
using RJCP.IO.Ports;
using System.Net;
using System.Text;
using System.Timers;
using UtilNetwork;

namespace Front.Equipments
{
    public class ScaleNet : Scale, IDisposable
    {
        BLF Blf;
        public bool IsReady =>true;
        IPAddress MyIP;
        public ScaleNet(Equipment pEquipment, IConfiguration pConfiguration, ILoggerFactory pLoggerFactory = null, Action<double, bool> pOnScalesData = null) : base(pEquipment, pConfiguration, eModelEquipment.ScaleCom, pLoggerFactory, pOnScalesData)
        {
            Blf = BLF.GetBLF;
            MyIP = Http.GetLocalIPAddress();
            Init();
        }

        public override void Init()
        {
            StopWeight();
        }

        public override StatusEquipment TestDevice() { Init(); return new StatusEquipment() { State = (int)State, TextState = State.ToString(), ModelEquipment = Model, StateEquipment = State }; }

        public override void StartWeight()=>Blf.SendRemoteComand<string>(new CommandAPI<string>(eCommand.StartWeightNet, MyIP.ToString()) ,IPAddress);

        public override void StopWeight()=>Blf.SendRemoteComand<string>(new CommandAPI<string>(eCommand.StopWeightNet, MyIP.ToString()) , IPAddress);
        public void Dispose()
        {
            StopWeight();
        }
    }
    
}
