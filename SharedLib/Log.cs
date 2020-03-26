using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ModelMID;

namespace SharedLib
{
    public class Log
    {
        //static string filePath="";
        //DateTime varD;
        public Log()
        {
            //varD = DateTime.Now;
            //filePath = Path.Combine(Global.PathLog, $"Log_{varD:yyyyMMdd}.log");
        }
        static async Task Write( string text)
        {
            var varD = DateTime.Now;
            var filePath = Path.Combine(Global.PathLog, $"Log_{varD:yyyyMMdd}.log");

            text=$"/n{varD:yyyy-MM-dd h:mm:ss.fffffff} /n /t {text} /n";
            
            byte[] encodedText = Encoding.Unicode.GetBytes(text);

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Append, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }
    }
}
