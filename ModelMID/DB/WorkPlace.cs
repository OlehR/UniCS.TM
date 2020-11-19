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
        public eTypePOS TypePOS { get; set; }

    }
}

