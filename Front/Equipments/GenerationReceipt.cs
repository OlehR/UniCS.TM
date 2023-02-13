using ModelMID;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Front.Equipments
{
    public class GenerationReceipt
    {
        public string NamePrinter = "Microsoft Print to PDF";
        List<Receipt> receipts= new List<Receipt>();
        public void PrintReceipt()
        {
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += PrintPageReceipt;
            printDocument.DocumentName = "Test";  //$"{receipts[0].NumberReceipt}{receipts[1].NumberReceipt}";
            printDocument.DefaultPageSettings.PaperSize = new PaperSize();
            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDocument;
            printDialog.PrinterSettings.PrinterName = NamePrinter;
            printDialog.Document.Print(); // печатаем
        }

        private void PrintPageReceipt(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawString(DateTime.Now.ToString("dd/MM/yyyy H:mm"), new Font("Arial", 40), Brushes.Black, 0, 0); //Час
        }
    }
}
