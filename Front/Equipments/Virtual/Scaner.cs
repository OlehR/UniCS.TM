using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    
        public class Scaner : Equipment
        {
            protected Action<string, string> OnBarCode;            
            public Scaner(IConfiguration pConfiguration, Action<string, string> pLogger, Action<string, string> pOnBarCode) : base(pConfiguration) { OnBarCode = pOnBarCode; }
        }
    
}
