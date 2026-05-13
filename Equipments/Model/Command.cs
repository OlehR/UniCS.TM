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
        public CommandAPI() { }
        public CommandAPI(eCommand pCommand, t pData)
        {
            Command = pCommand;
            Data = pData;
        }
        public eCommand Command { get; set; }
        public t Data { get; set; }
    }
}
