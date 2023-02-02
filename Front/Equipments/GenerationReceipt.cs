using Front.Equipments.Implementation.ModelVchasno;
using ModelMID;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Equipments
{
    public class GenerationReceipt
    {
        List<Receipt> receipts= new List<Receipt>();
        public void PrintReceipt()
        {
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += PrintPageReceipt;
            printDocument.DocumentName = $"{receipts[0].NumberReceipt}{receipts[1].NumberReceipt}";
            //printDocument.DefaultPageSettings.

        }

        private void PrintPageReceipt(object sender, PrintPageEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
