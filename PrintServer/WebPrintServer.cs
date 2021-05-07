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
            var PathLog = System.Configuration.ConfigurationManager.AppSettings["PathLog"];
            string now = DateTime.Now.ToString("yyyyMMdd");
            fileName = Path.Combine(PathLog, $"\\PrintServer_{now}.log");
            //y = 0;
        }
        public string Print(Wares parWares) 
        {
            try
            {
                if (parWares == null)
                    return "Bad input Data: Wares";
                Console.WriteLine(parWares.CodeWares);

                if (parWares.CodeWarehouse == 0)
                    return "Bad input Data:CodeWarehouse";
                
                var NamePrinterYelow = System.Configuration.ConfigurationManager.AppSettings[$"NamePrinterYelow_{parWares.CodeWarehouse}"];
                var NamePrinter = System.Configuration.ConfigurationManager.AppSettings[$"NamePrinter_{parWares.CodeWarehouse}"];
                if(string.IsNullOrEmpty( NamePrinter))
                    return $"Відсутній принтер: NamePrinter_{parWares.CodeWarehouse}";

                //int  x = 343 / y;
                var ListWares = GL.GetCode(parWares.CodeWarehouse,parWares.CodeWares);//"000140296,000055083,000055053"
                GL.Print(ListWares, NamePrinter, NamePrinterYelow, "Супер тест", parWares.CodeWarehouse<30);  //PrintPreview();
                return $"Print=>{ListWares.Count()}"; 

            }
            catch(Exception ex)
            {                
                File.AppendAllText(fileName, $"\n{DateTime.Now.ToString()}\nInputData=>{parWares.CodeWares}\n{ex.Message } \n{ex.StackTrace}");
                return "Error=>"+ex.Message;
            }
        }

        public string GetQueue()
        {
            var q = new MyQueue();
            return q.GetQueue();
        }
        
        public string ClearQueue()
        {
            var q = new MyQueue();
            return q.GetQueue();
        }

    }
}


