using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using ECRCommXLib;
using Newtonsoft.Json;
using ModelMID;
using Utils;
using Front.Equipments.Utils;


namespace Front.Equipments
{
    public class GloryCash :BankTerminal, IDisposable
    {

        public GloryCash(Equipment pEquipment, IConfiguration pConfiguration, ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) : 
                                base(pEquipment, pConfiguration, eModelEquipment.Ingenico, pLoggerFactory,pActionStatus)
        {            
           
        }

        public void Dispose()
        {
            
        }
    }

}