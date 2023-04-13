using ModelMID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Models
{
    public class CommandAPI<DataFormat>
    {
        public eCommand Command { get; set; }        
        public DataFormat Data { get; set; }
    }
}
