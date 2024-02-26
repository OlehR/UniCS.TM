using System;
using System.Collections.Generic;
using System.Text;
using Front.Equipments;

namespace ModelMID
{
     public interface IMW
    {
        
        public Receipt curReceipt { get; set; }
        public ReceiptWares CurWares { get; set; }
        public Client Client { get { return curReceipt?.Client; } }

        public eStateMainWindows State { get; set; }
        public eTypeAccess TypeAccessWait { get; set; }
    }
}
