using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{
    public class WorkPlace
    {
        public int IdWorkplace { get; set; }
        public string Name { get; set; }
        public Guid TerminalGUID { get; set; }
        public string  StrTerminalGUID { get { return TerminalGUID.ToString(); } set { TerminalGUID=Guid.Parse(value); } }

        public string VideoCameraIP { get; set; }
        public string VideoRecorderIP { get; set; }
        public eBank TypePOS { get; set; }
        public int CodeWarehouse { get; set; }
        public string StrCodeWarehouse { get { return $"{CodeWarehouse:D9}"; } }
        public int CodeDealer { get; set; }
        public string Prefix { get; set; }
        public bool IsChoice { get; set; }
        public string DNSName { get; set; }
        public eTypeWorkplace TypeWorkplace { get; set; }

    }
}

