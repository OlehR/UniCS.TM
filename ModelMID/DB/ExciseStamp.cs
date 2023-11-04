using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{
    public enum StateExciseStamp { Try=0,Used=1}
    public class ExciseStamp:IdReceiptWares
    {
        public string Stamp { get; set; }
        public StateExciseStamp State { get; set; } = StateExciseStamp.Try;
        public DateTime DateCreate { get; set; }
        public long UserCreate { get; set; }
    }
}
