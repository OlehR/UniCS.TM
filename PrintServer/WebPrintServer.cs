using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintServer
{
    class WebPrintServer: IWebPrintServer
    {
        GenLabel GL = new GenLabel();
        public string Print(Wares parWares) 
        {
            if (parWares == null)
                return "Bad input Data";
            Console.WriteLine(parWares.CodeWares);
            
            var ListWares=GL.GetCode(parWares.CodeWares);//"000140296,000055083,000055053"
            

            GL.PrintPreview();
            return "Print";
        }        

    }
}
