using Equipments.Equipments;
using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Utils;

namespace Front.Equipments
{
    class ZebraScale:Scale
    {
        ZebraCoreScanner Zebra;
        
        public ZebraScale(Equipment pEquipment, IConfiguration pConfiguration, ILoggerFactory pLoggerFactory = null, Action<double, bool> pOnScalesData=null):base(pEquipment, pConfiguration,eModelEquipment.MagellanScale,pLoggerFactory, pOnScalesData)
        {
            State = eStateEquipment.Init;
            ILogger<ZebraScaner> logger = LoggerFactory?.CreateLogger<ZebraScaner>();
            Zebra = ZebraCoreScanner.GetZebraCoreScanner;

            if (Zebra.CurrentStatus == eDeviceConnectionStatus.Enabled)
            {
                Zebra.OnWeightChanged += (Weight) =>
                {
                    pOnScalesData?.Invoke(Weight, true); pOnScalesData?.Invoke(Weight, true);
                };
                State = eStateEquipment.On;
            }else
                State = eStateEquipment.Error;
        }

        //public ZebraScale(MagellanScaner pMagellan, Action<double, bool> pOnScalesData, Action<double, bool> pOnScalesData2=null) :base()
        //{
        //    //Model = eModelEquipment.MagellanScale;

        //    //if(pMagellan==null)
        //    //{
        //    //    State = eStateEquipment.Error;
        //    //    return;
        //    //}
        //    Zebra = pMagellan.Magellan9300;

        //    if (pMagellan.State != eStateEquipment.On)
        //        return;

        //    if (Zebra != null)
        //        Zebra.OnWeightChanged += (Weight) => { pOnScalesData?.Invoke(Weight, true); pOnScalesData2?.Invoke(Weight, true); };

        //    State = pMagellan.State;
    
        //}

        public override void StartWeight() 
        {
            try
            {
                Zebra?.StartGetWeight();
            }catch
            {
                State=eStateEquipment.Error;
            }
        }

        public override void StopWeight()
        {           
            try
            {
                Zebra?.StopGetWeight();
            }
            catch
            {
                State = eStateEquipment.Error;
            }
        }

        public override StatusEquipment TestDevice() 
        {
            string Error = null;
            try
            {
                Zebra.Init();
                Error= Zebra.GetInfo();                
                State = eStateEquipment.On;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                Error = e.Message;
                State = eStateEquipment.Error;
            }
            return new StatusEquipment(Model,State,Error);
        }
               
    }
}
