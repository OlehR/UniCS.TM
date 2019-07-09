using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{
    public class FastGroup
    {
        public int CodeFastGroup { get; set; }
        public int CodeUp { get; set; }
        public string Name { get; set; }

        public Guid FastGroupId
        {
            get
            {
                var Const="12345670-0987-0000-0000-";
                var strFastGroup = new String('0', 12) + CodeFastGroup.ToString();
                strFastGroup = strFastGroup.Substring(strFastGroup.Length - 12);
                return Guid.Parse(Const  + strFastGroup);
            }
            set
            {
                int Code;
                CodeFastGroup = (int.TryParse(value.ToString().Substring(24), out Code) ? Code : 0);
            }
        }
    }
}
