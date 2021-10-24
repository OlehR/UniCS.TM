using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class EquipmentElement
    {        
        public eTypeEquipment Type { get { return Model.GetTypeEquipment(); } }
        public string StrType { get { return Type.ToString(); } }
        public eModel Model { get; set; }
        public Equipment Equipment  { get; set; }
        public string Port { get; set; }
        public int BaudRate { get; set; }
    }
}
 