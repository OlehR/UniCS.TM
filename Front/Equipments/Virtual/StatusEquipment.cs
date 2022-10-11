using Front.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Equipments.Virtual
{
    public class StatusEquipment : Status
    {
        public eStateEquipment StateEquipment { get; set; }
        public eModelEquipment ModelEquipment { get; set; }
        public bool IsСritical { get; set; } = true;
        public eTypeEquipment TypeEquipment  { get{ return ModelEquipment.GetTypeEquipment(); } }
        public StatusEquipment():base() {  }
        public StatusEquipment(eModelEquipment pME, eStateEquipment pStateEquipment,string pTextState=null) :base((int)pStateEquipment, pTextState?? pStateEquipment.ToString())
        {
            StateEquipment = pStateEquipment;
            ModelEquipment = pME;
        }
    }
    public class PosStatus : StatusEquipment
    {
        public eStatusPos Status { get; set; }
    }

    public class RroStatus : StatusEquipment
    {
        public eStatusRRO Status { get; set; }
    }
}
