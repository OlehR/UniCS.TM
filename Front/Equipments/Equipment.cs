using System;
using System.Collections.Generic;
using System.Text;

namespace Front.Equipments
{
    public class Equipment
    {
        public eState State { get; set; }
        public  virtual eState Test() { throw new NotImplementedException(); }

    }
    public enum eState {Ok,Off,Error}
}
