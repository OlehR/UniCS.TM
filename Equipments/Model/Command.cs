using ModelMID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Equipments
{
    public class CommandAPI<t>
    {
        public eCommand Command { get; set; }        
        public t Data { get; set; }
    }
}
