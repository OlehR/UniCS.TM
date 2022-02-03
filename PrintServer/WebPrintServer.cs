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
            fileName = Path.Combine(PathLog, $"PrintServer_{now}.log");
            //y = 0;
        }
        public string Print(Wares pWares)
        {
            try
            {
                if (pWares == null)
                    return "Bad input Data: Wares";
                Console.WriteLine(pWares.CodeWares);

                if (pWares.CodeWarehouse == 0)
                    return "Bad input Data:CodeWarehouse";

                var NamePrinterYelow = System.Configuration.ConfigurationManager.AppSettings[$"NamePrinterYelow_{pWares.CodeWarehouse}"];
                var NamePrinter = System.Configuration.ConfigurationManager.AppSettings[$"NamePrinter_{pWares.CodeWarehouse}"];
                if (string.IsNullOrEmpty(NamePrinter))
                    return $"Відсутній принтер: NamePrinter_{pWares.CodeWarehouse}";

                //int  x = 343 / y;
                var ListWares = GL.GetCode(pWares.CodeWarehouse, pWares.CodeWares);//"000140296,000055083,000055053"
                if (ListWares.Count() > 0)
                    GL.Print(ListWares, NamePrinter, NamePrinterYelow, $"Label_{pWares.NameDCT}_{pWares.Login}", pWares.CodeWarehouse < 30);  //PrintPreview();
                File.AppendAllText(fileName, $"\n{DateTime.Now.ToString()} Warehouse=> {pWares.CodeWarehouse} Count=> {ListWares.Count()} Login=>{pWares.Login} SN=>{pWares.SerialNumber} NameDCT=> {pWares.NameDCT} \n Wares=>{pWares.CodeWares}");

                return $"Print=>{ListWares.Count()}";

            }
            catch (Exception ex)
            {
                File.AppendAllText(fileName, $"\n{DateTime.Now.ToString()}\nInputData=>{pWares.CodeWares}\n{ex.Message } \n{ex.StackTrace}");
                return "Error=>" + ex.Message;
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


