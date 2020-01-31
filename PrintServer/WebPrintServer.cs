using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintServer
{
    class WebPrintServer: IWebPrintServer
    {
        GenLabel GL = new GenLabel();
        //string PathLog = "";
        //
        string fileName;
        //int y = 1;
        public WebPrintServer()
        {
            var PathLog = System.Configuration.ConfigurationManager.AppSettings["CodeWarehouse"];
            string now = DateTime.Now.ToString("yyyyMMdd");
            fileName = Path.Combine(PathLog, $"\\PrintServer_{now}.log");
            //y = 0;
        }
        public string Print(Wares parWares) 
        {
            try
            {
                if (parWares == null)
                    return "Bad input Data";
                Console.WriteLine(parWares.CodeWares);

                //int  x = 343 / y;
                var ListWares = GL.GetCode(parWares.CodeWares);//"000140296,000055083,000055053"
                GL.Print(ListWares, "Супер тест");  //PrintPreview();
                return $"Print=>{ListWares.Count()}"; 

            }
            catch(Exception ex)
            {                
                File.AppendAllText(fileName, $"\n{DateTime.Now.ToString()}\nInputData=>{parWares.CodeWares}\n{ex.Message } \n{ex.StackTrace}");
                return "Error=>"+ex.Message;
            }
        }        

    }
}


