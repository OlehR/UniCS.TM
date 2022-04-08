using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Equipments.Implementation
{
    public class VirtualScaner:Scaner
    {
       
        public VirtualScaner(IConfiguration pConfiguration, Action<string, string> pLogger, Action<string, string> pOnBarCode) : base(pConfiguration, pLogger, pOnBarCode)
        {
            if (pOnBarCode != null)
            { }
                
        }

    }
}
