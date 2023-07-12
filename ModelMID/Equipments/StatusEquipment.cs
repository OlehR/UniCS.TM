using Utils;

namespace Front.Equipments
{
    public class StatusEquipment : Status
    {
        public eStateEquipment StateEquipment { get; set; }
        public eModelEquipment ModelEquipment { get; set; }
        public bool IsСritical { get; set; } = false;
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
        public PosStatus() : base() { }
        public eStatusPos Status { get; set; }
    }

    public class RroStatus : StatusEquipment
    {
        public RroStatus() : base() { }
        public RroStatus(eModelEquipment pME, eStateEquipment pStateEquipment, string pTextState = null) : base(pME, pStateEquipment, pTextState) 
        { }
        public eStatusRRO Status { get; set; }
    }
}
