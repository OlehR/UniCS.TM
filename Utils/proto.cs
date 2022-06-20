    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
namespace Utils
{
    public static class proto
    {
        public static JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            DateFormatString = "dd.MM.yyyy",
            FloatParseHandling = FloatParseHandling.Decimal,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        }; //, Culture = MyCulture 
        public static string ToJSON(this object s)
        {
            var res = JsonConvert.SerializeObject(s, JsonSettings);
            return res;
        }
        public static string ToXMLString(this string s)
        {
            return s.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("'", "&apos;").Replace("<", "&lt;").Replace(">", "&gt;");//.Replace(".", "&period;");
        }

        public static int ToInt(this string s,int pDefault=0)
        {
            int res;
            if (!Int32.TryParse(s, out res))
                res = pDefault;
            return res;
        }

    }
}
