using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelMID
{
    public enum eTypeDB { All, Config, RC, MID }

    public enum eSQLQueryType { Execute, Scalar }
    public class SQLQuery
    {
        public eTypeDB TypeDB { get; set; }
        public eSQLQueryType QueryType { get; set; }
        public int CodePeriod { get; set; }
        public string SQL { get; set; }

    }
}
