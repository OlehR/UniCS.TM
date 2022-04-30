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

        public MagellanScale(Magellan9300S pMagellan, Action<double, bool> pOnScalesData):base()
        {
            Magellan = pMagellan;
            if (Magellan != null)
                Magellan.OnWeightChanged += (Weight) => { pOnScalesData(Weight, true);};
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
