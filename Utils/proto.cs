    using Newtonsoft.Json;
using System;
    using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
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


        public static string ToS(this decimal s)
        {
            return Convert.ToString(s, CultureInfo.InvariantCulture);            
         }
        public static string GetDescription(this Enum value)
        {
            var enumMember = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var descriptionAttribute =
                enumMember == null
                    ? default(DescriptionAttribute)
                    : enumMember.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
            return
                descriptionAttribute == null
                    ? value.ToString()
                    : descriptionAttribute.Description;
        }

    }
}
