using ModelMID;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Front.Equipments
{
    public class GenerationReceipt
    {
        public string NamePrinter = "SAM4S";
        Receipt Receipt = new Receipt();
        public void PrintReceipt(Receipt R)
        {
            Receipt = R;
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += PrintPageReceipt;
            printDocument.DocumentName = "Test";  //$"{receipts[0].NumberReceipt}{receipts[1].NumberReceipt}";
            //printDocument.DefaultPageSettings.PaperSize = new PaperSize();
            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDocument;
            printDialog.PrinterSettings.PrinterName = NamePrinter;
            printDialog.Document.Print(); // печатаем
        }

        private void PrintPageReceipt(object sender, PrintPageEventArgs e)
        {
            int leftPosition = 0, topPosition = 0;
            int widthPage = 190;//e.PageBounds.Width; // ширина паперу принтера
            int fontSize = 10;
            int maxChar = widthPage / fontSize;
            string companyName = "ТОВ \"Ужгород ПСЮ\"";
            string pointOfSale = "Супермаркет ВОПАК";
            string address = "Закарпатська обл, м. Ужгород, вул. Бестужева, буд. 9";
            Font mainFont = new Font("Courier", fontSize, FontStyle.Bold, GraphicsUnit.Point);


            //Друк шапки



            //if (companyName.Length < maxChar)
            //    companyName = new string(' ', (maxChar - companyName.Length) / 2 ) + companyName;
            //e.Graphics.DrawString(companyName, mainFont, Brushes.Black, left, topPosition+=fontSize);

            topPosition = CenterPrint(e, companyName, widthPage, fontSize, topPosition, maxChar, mainFont);
            topPosition = CenterPrint(e, pointOfSale, widthPage, fontSize, topPosition, maxChar, mainFont);
            topPosition = CenterPrint(e, address, widthPage, fontSize, topPosition, maxChar, mainFont);

            //розділювач
            e.Graphics.DrawString("------------------------------------", mainFont, Brushes.Black, 0, topPosition += fontSize);

            // Блок друку товарів
            foreach (var item in Receipt.Wares)
            {
                string price = "";
                //if (item.NameWares.Length < maxChar-9)
                {
                    StringFormat stringFormatName = new StringFormat();
                    stringFormatName.Alignment = StringAlignment.Near;
                    stringFormatName.LineAlignment = StringAlignment.Center;

                    StringFormat stringFormatPrice = new StringFormat();
                    stringFormatPrice.Alignment = StringAlignment.Far;
                    stringFormatPrice.LineAlignment = StringAlignment.Center;

                    Rectangle rect1 = new Rectangle(leftPosition, topPosition + fontSize, widthPage, fontSize);

                    e.Graphics.DrawString(item.NameWares, mainFont, Brushes.Black, rect1, stringFormatName);
                    e.Graphics.DrawString(item.Price.ToString(), mainFont, Brushes.Black, rect1, stringFormatPrice);

                    //e.Graphics.DrawString(item.NameWares, mainFont, Brushes.Black, leftPosition, topPosition + fontSize);
                    //price = new string(' ', (maxChar - 9) ) + item.Price.ToString();
                    //e.Graphics.DrawString(price, mainFont, Brushes.Black, leftPosition, topPosition + fontSize);

                }
                // else
                {
                    //string tmpVar = str;
                    //int pos = tmpVar.Substring(0, maxChar).LastIndexOf(" ");
                    //str = tmpVar.Substring(0, pos);
                    //str2 = tmpVar.Substring(pos);
                    //leftPosition = (widthPage - (fontSize * str.Length)) / 2;
                    //e.Graphics.DrawString(str, mainFont, Brushes.Black, leftPosition, topPosition += fontSize);
                    //leftPosition = (widthPage - (fontSize * str2.Length)) / 2;
                    //e.Graphics.DrawString(str2, mainFont, Brushes.Black, leftPosition, topPosition += fontSize);
                }
                //e.Graphics.DrawString(item.NameWares, mainFont, Brushes.Black, leftPosition, topPosition + fontSize);
                topPosition += fontSize;
            }

            //розділювач
            e.Graphics.DrawString("------------------------------------", mainFont, Brushes.Black, 0, topPosition += fontSize);

            StringFormat stringFormatSum = new StringFormat();
            stringFormatSum.Alignment = StringAlignment.Near;
            stringFormatSum.LineAlignment = StringAlignment.Center;

            StringFormat stringFormatTotalPrice = new StringFormat();
            stringFormatTotalPrice.Alignment = StringAlignment.Far;
            stringFormatTotalPrice.LineAlignment = StringAlignment.Center;

            Rectangle rectTotalSum = new Rectangle(leftPosition, topPosition + fontSize, widthPage, fontSize);

            e.Graphics.DrawString("Сума", mainFont, Brushes.Black, rectTotalSum, stringFormatSum);
            e.Graphics.DrawString(Receipt.SumTotal.ToString(), mainFont, Brushes.Black, rectTotalSum, stringFormatTotalPrice);


            //for (int i = 0; i < 5; i++)
            //{
            //    e.Graphics.DrawString(DateTime.Now.ToString("dd/MM/yyyy H:mm"), mainFont, Brushes.Black, leftPosition, topPosition += fontSize); //Час

            //}

            //int charactersOnPage = 0;
            //int linesPerPage = 0;
            //StringFormat stringFormat = new StringFormat();
            //stringFormat.Alignment = StringAlignment.Center;
            //stringFormat.LineAlignment = StringAlignment.Center;
            //Rectangle rect = new Rectangle(leftPosition, topPosition + fontSize, widthPage, fontSize* (address.Length/maxChar+2));

            //while (address.Length > 0)
            //{
            //    e.Graphics.MeasureString(address, mainFont, rect.Size, stringFormat, out charactersOnPage, out linesPerPage);
            //    e.Graphics.DrawString(address, mainFont, Brushes.Black, rect, stringFormat);
            //    address = address.Substring(charactersOnPage);
            //}



        }
        public int CenterPrint(PrintPageEventArgs e, string str, int widthPage, int fontSize, int topPosition, int maxChar, Font mainFont)
        {
            int leftPosition = 0;
            string tmpVar = str;
            int pos = pos = tmpVar.Length > maxChar ? tmpVar.Substring(0, maxChar).LastIndexOf(" ") : tmpVar.Length;

            while (str.Length > 0)
            {
                str = tmpVar.Substring(0, pos);
                leftPosition = (widthPage - (fontSize * str.Length)) / 2;
                if (!string.IsNullOrEmpty(str))
                    e.Graphics.DrawString(str, mainFont, Brushes.Black, leftPosition, topPosition += fontSize);
                tmpVar = tmpVar.Substring(pos);
                pos = tmpVar.Length > maxChar ? tmpVar.Substring(0, maxChar).LastIndexOf(" ") : tmpVar.Length;
            }

            return topPosition;
        }
    }
}
