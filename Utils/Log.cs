using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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

        /*static async Task Write( string pText,string pFilePath = null,bool pAddInfo=true)
        {
            var varD = DateTime.Now;
            var filePath = pFilePath !=null? pFilePath: Path.Combine(Global.PathLog, $"Log_{varD:yyyyMMdd}.log");

            var LogFilePath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(LogFilePath))
                Directory.CreateDirectory(LogFilePath);


            var text= pAddInfo?$"/n{varD:yyyy-MM-dd h:mm:ss.fffffff} /n /t {pText} /n": pText;
            
            byte[] encodedText = Encoding.Unicode.GetBytes(text);

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Append, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }*/
    }
}
