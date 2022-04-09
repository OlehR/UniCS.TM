using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Equipments.Implementation
{
    public class VirtualScale:Scale
    {
        
        public VirtualScale(IConfiguration pConfiguration, Action<string, string> pLogger, Action<double, bool> pOnScalesData) : base(pConfiguration, pLogger, pOnScalesData)
        {

        }

        
    };

}

