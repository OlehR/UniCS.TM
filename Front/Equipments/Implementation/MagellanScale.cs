using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    class MagellanScale:Scale
    {
        Magellan9300S Magellan;
        
        public MagellanScale(Equipment pEquipment, IConfiguration pConfiguration, Action<string, string> pLogger, Action<double, bool> pOnScalesData):base(pEquipment, pConfiguration,eModelEquipment.MagellanScale,pLogger, pOnScalesData)
        {

        }

        public MagellanScale(MagellanScaner pMagellan, Action<double, bool> pOnScalesData, Action<double, bool> pOnScalesData2=null) :base()
        {
            State = pMagellan.State;
            if (State != eStateEquipment.On)
                return;
            Magellan = pMagellan.Magellan9300;
           
            if (Magellan != null)
                Magellan.OnWeightChanged += (Weight) => { pOnScalesData?.Invoke(Weight, true); pOnScalesData2?.Invoke(Weight, true); };
        }
        public override void StartWeight() 
        {            
            Magellan?.StartGetWeight();
        }

        public override void StopWeight()
        {
            Magellan?.StopGetWeight();
        }

    }
}
