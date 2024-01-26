using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Utils
{
    public static class proto_
    {
        public static bool IsBase64(this string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out int bytesParsed);
        }
        public static string FromBase64(this string base64) 
        {
            string res = null;
            try
            {
                if (string.IsNullOrEmpty(base64)) return null;

                if (base64.IsBase64())
                {
                    byte[] data = Convert.FromBase64String(base64);
                    res = System.Text.Encoding.UTF8.GetString(data);
                }
            } catch { }
            return res;

        }
        
    }

}