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
        public eModelEquipment ModelEquipment { get; set; }
        public eTypeEquipment TypeEquipment  { get{ return ModelEquipment.GetTypeEquipment(); } }
        public StatusEquipment():base() {  }
        public StatusEquipment(eModelEquipment pME, int pState = 0, string pTextState = "Ok"):base(pState, pTextState)
        {
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
