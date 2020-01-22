using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintServer
{
    class WebPrintServer: IWebPrintServer
    {
        public string Print(string Wares) 
        {
            Console.WriteLine(Wares);
            GenLabel GL = new GenLabel();
            GL.GetCode(Wares);//"000140296,000055083,000055053"
            GL.PrintPreview();

            return Wares;
        }
    }
}
