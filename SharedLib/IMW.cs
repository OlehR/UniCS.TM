using System;
using System.Collections.Generic;
using System.Text;
using Front.Equipments;
using ModelMID;

namespace SharedLib
{
     public interface IMW
    {
        
        public Receipt curReceipt { get; set; }
        public ReceiptWares CurWares { get; set; }
        public Client Client { get { return curReceipt?.Client; } }
        Sound s { get; set; }       
        public ControlScale CS { get; set; }
        public eStateMainWindows State { get; set; }
        public eTypeAccess TypeAccessWait { get; set; }
    }
}
