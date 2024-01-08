using System;

namespace ModelMID.DB
{
    public enum eStateExciseStamp {Return=-9 ,Cancel=-1, Try = 0, Used = 1 }
    public class ExciseStamp : IdReceiptWares
    {
        public ExciseStamp(IdReceiptWares pIdRW, string pES, eStateExciseStamp pState = eStateExciseStamp.Try) : base(pIdRW)
        {
            Stamp = pES;
            State = pState;
        }
        public ExciseStamp() : base() { }
        public string Stamp { get; set; }
        public eStateExciseStamp State { get; set; } = eStateExciseStamp.Try;
        public DateTime DateCreate { get; set; }
        public long UserCreate { get; set; }
    }
}
