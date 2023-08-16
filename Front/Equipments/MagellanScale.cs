using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Utils;

namespace Front.Equipments
{
    class MagellanScale:Scale
    {
        Magellan9300S Magellan;
        
        public MagellanScale(Equipment pEquipment, IConfiguration pConfiguration, ILoggerFactory pLoggerFactory = null, Action<double, bool> pOnScalesData=null):base(pEquipment, pConfiguration,eModelEquipment.MagellanScale,pLoggerFactory, pOnScalesData)
        {

        }

        public MagellanScale(MagellanScaner pMagellan, Action<double, bool> pOnScalesData, Action<double, bool> pOnScalesData2=null) :base()
        {
            Model = eModelEquipment.MagellanScale;

            if(pMagellan==null)
            {
                State = eStateEquipment.Error;
                return;
            }
            Magellan = pMagellan.Magellan9300;

            if (pMagellan.State != eStateEquipment.On)
                return;

            if (Magellan != null)
                Magellan.OnWeightChanged += (Weight) => { pOnScalesData?.Invoke(Weight, true); pOnScalesData2?.Invoke(Weight, true); };

            State = pMagellan.State;
    
        }

        public override void StartWeight() 
        {
            try
            {
                Magellan?.StartGetWeight();
            }catch
            {
                State=eStateEquipment.Error;
            }
        }

        public override void StopWeight()
        {           
            try
            {
                Magellan?.StopGetWeight();
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
                Magellan.Init();
                Error= Magellan.GetInfo();                
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
