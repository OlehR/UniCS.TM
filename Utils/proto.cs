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

        public static JsonSerializerSettings JsonSettings = new JsonSerializerSettings() { DateFormatString = "dd.MM.yyyy", FloatParseHandling = FloatParseHandling.Decimal, NullValueHandling = NullValueHandling.Ignore }; //, Culture = MyCulture 
        public static string ToJSON(this object s)
        {
            var res = JsonConvert.SerializeObject(s, JsonSettings);
            return res;
        }
    }
}
