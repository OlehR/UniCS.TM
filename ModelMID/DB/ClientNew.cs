using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelMID.DB
{
    public class ClientNew
    {
        public int IdWorkplace { get; set; }
        public int State { get; set; }
        public string BarcodeClient{ get; set; }
        public string BarcodeCashier { get; set; }
        public string Phone { get; set;}
        public DateTime DateCreate { get; set;}
    }
}
