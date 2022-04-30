using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{

    public class Scaner : Equipment
    {
        protected Action<string, string> OnBarCode;
        public Scaner(Equipment pEquipment, IConfiguration pConfiguration, eModelEquipment pModelEquipment = eModelEquipment.NotDefined, Action<string, string> pLogger = null, Action<string, string> pOnBarCode = null) : base(pEquipment, pConfiguration,pModelEquipment, pLogger) { OnBarCode = pOnBarCode; }
    }

}
