using Front.Equipments.Virtual;
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
            Model = eModelEquipment.MagellanScale;

            if(pMagellan==null)
            {
                State = eStateEquipment.Error;
                return;
            }
            Magellan = pMagellan.Magellan9300;

            if (State != eStateEquipment.On)
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
                Magellan?.StopGetWeight();
                Magellan?.StartGetWeight();
                Magellan?.StopGetWeight();

            }
            catch (Exception e)
            {
                Error = e.Message;
                State = eStateEquipment.Error;
            }
            return new StatusEquipment(Model,State,Error);
        }
        public override string GetDeviceInfo() 
        { 
            return $"State={State} Port={SerialPort} BaudRate={BaudRate}"; 
        }
    }
}
