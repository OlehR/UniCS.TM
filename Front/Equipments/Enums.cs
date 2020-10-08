using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public enum eTypeEquipment
    {
        NotDefined,
        Scaner,
        Scale,
        ControlScale,
        BankTerminal,
        Signal,
        EKKA
    }

    public enum eModel
    {
        NotDefined,
        MagellanScaner,
        MagellanScale,
        ScaleModern,
        SignalFlagModern,
        Ingenico,
        Exelio
    }

    static class ModelMethods
    {

        public static eTypeEquipment GetTypeEquipment(this eModel pModel)
        {
            switch (pModel)
            {
                case eModel.MagellanScaner:
                    return eTypeEquipment.Scaner;
                case eModel.MagellanScale:
                    return eTypeEquipment.Scale;
                case eModel.ScaleModern:
                    return eTypeEquipment.ControlScale;
                case eModel.SignalFlagModern:
                    return eTypeEquipment.Signal;
                case eModel.Ingenico:
                    return eTypeEquipment.BankTerminal;
                case eModel.Exelio:
                    return eTypeEquipment.EKKA;
                default:
                    return eTypeEquipment.NotDefined;
            }
        }
        
    }
}
