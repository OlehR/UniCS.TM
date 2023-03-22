﻿    using Newtonsoft.Json;
using System;
    using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
    using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

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

        public static decimal ToDecimal(this string s, decimal pDefault = 0)
        {
            decimal res;
            if (!Decimal.TryParse(s, out res))
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

        public static DateTime ToDateTime(this string s,
                  string format = "ddMMyyyy", string cultureString = "tr-TR")
        {
            try
            {
                var r = DateTime.ParseExact(
                    s: s,
                    format: format,
                    provider: CultureInfo.GetCultureInfo(cultureString));
                return r;
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }            
        }

    }

    public static class ThreadExtensions
    {
        public static void RunAsync(this Task val, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task.Factory.StartNew((Func<Task>)async delegate
            {
                await val;
            }, cancellationToken);
        }

        public static void RunAsync(this Func<Task> val, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task.Factory.StartNew(val, cancellationToken);
        }

        public static void RunAsync(this Action action, CancellationToken cancellationToken = default(CancellationToken))
        {
            Task.Factory.StartNew(action, cancellationToken);
        }

        public static Task TaskDelayWithoutException(this object val, int timeoutInMillis, CancellationToken cancellationToken)
        {
            return Task.Delay(timeoutInMillis, cancellationToken).ContinueWith(delegate
            {
            });
        }

        public static Task TaskDelayWithoutException(this object val, int timeoutInMillis, CancellationTokenSource cts, Action<bool> onFinish = null, bool shouldDisposeToken = true)
        {
            return Task.Delay(timeoutInMillis, cts.Token).ContinueWith(delegate
            {
                onFinish?.Invoke(cts.Token.IsCancellationRequested);
                if (shouldDisposeToken)
                {
                    cts.Dispose();
                }
            });
        }
    }
}
