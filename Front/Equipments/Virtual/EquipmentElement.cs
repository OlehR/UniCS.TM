using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class EquipmentElement
    {        
        public eTypeEquipment Type { get { return Model.GetTypeEquipment(); } }
        //public string StrType { get { return Type.ToString(); } }
        public eModelEquipment Model { get; set; }
        public Equipment Equipment  { get; set; }
        public string Name { get; set; }
        public bool IsСritical { get; set; } = true;
    }
}
 