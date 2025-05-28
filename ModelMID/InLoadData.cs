using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{    public class InLoadData
    {
        public int IdWorkPlace { get; set; }
        public bool IsFull { get; set; }
        public int MessageNoMin { get; set; }
        public bool IsReloadFull { get; set; } = false;
    }
}
