using Microsoft.Extensions.FileSystemGlobbing;
using ModelMID;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using QRCoder;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Extensions.Configuration;
using ModernExpo.SelfCheckout.Utils;

namespace Front.Equipments.Implementation
{
    public class Printer_Sam4sGcube102 : Printer
    {
        const int CharInLine = 34;
        Receipt Receipt = new();
        IEnumerable<string> ArrayStr;
        const int FONTSIZE = 6;
        const int SECONDFONTSIZE = 8;
        const int WIDTHPAGE = 200;//e.PageBounds.Width; // ширина паперу принтера
        public Font MainFont = new("Courier", FONTSIZE, FontStyle.Bold, GraphicsUnit.Point);
        public Font SecondFont = new("Courier", SECONDFONTSIZE, FontStyle.Bold, GraphicsUnit.Point);

        public int TopIndent;
        QRCodeGenerator qrGenerator = new();

        public Printer_Sam4sGcube102(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null) :
                         base(pEquipment, pConfiguration, eModelEquipment.pRRO_SG, pLoggerFactory)
        {
            NamePrinter = Configuration?.GetValue<string>($"{KeyPrefix}NamePrinter");
        }

        override public bool PrintReceipt(Receipt R)
        {
            Receipt = R;
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += PrintPageReceipt;
            printDocument.DocumentName = "Receipt";
            PrintDialog printDialog = new();
            printDialog.Document = printDocument;
            printDialog.PrinterSettings.PrinterName = NamePrinter;
            printDialog.Document.Print(); // печатаем
            return true;
        }

        override public bool Print(IEnumerable<string> pR)
        {
            var count = pR.Count();
            if (count > 0)
            {
                ArrayStr = pR;

                if (count == 2)
                {
                    PrintDocument printDocument = new PrintDocument();
                    printDocument.PrintPage += PrintCoffeQR;
                    printDocument.DocumentName = "Coffe QR";
                    PrintDialog printDialog = new();
                    printDialog.Document = printDocument;
                    printDialog.PrinterSettings.PrinterName = NamePrinter;
                    printDialog.Document.Print(); // печатаем

                }
                else
                {
                    PrintDocument printDocument = new PrintDocument();
                    printDocument.PrintPage += PrintArrayStrings;
                    printDocument.DocumentName = "ArrayStrings";
                    PrintDialog printDialog = new();
                    printDialog.Document = printDocument;
                    printDialog.PrinterSettings.PrinterName = NamePrinter;
                    printDialog.Document.Print(); // печатаем
                }


                return true;
            }
            else return false;

        }

        private void PrintCoffeQR(object sender, PrintPageEventArgs e)
        {
            int position = 0;
            int maxChar = (e.PageBounds.Width - 70) / SECONDFONTSIZE;
            TopIndent = SECONDFONTSIZE + 1;

            foreach (var item in ArrayStr)
            {
                if (item.Contains("QR=>"))
                {
                    string QRInfo = item.Replace("QR=>", string.Empty);
                    var qrCodeData = qrGenerator.CreateQrCode(QRInfo, QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new QRCode(qrCodeData);
                    var QRImage = qrCode.GetGraphic(2);
                    e.Graphics.DrawImage(QRImage, (int)((WIDTHPAGE - QRImage.Width) * 0.85 / 2), position += 10);
                }
                else position = PrintLine(e, item, position, maxChar, SecondFont);
            }
        }

        private void PrintArrayStrings(object sender, PrintPageEventArgs e)
        {
            int position = 0;
            int maxChar = (e.PageBounds.Width - 70) / SECONDFONTSIZE;

            foreach (var item in ArrayStr)
            {
                position = PrintLine(e, ClearStr(item), position, maxChar, SecondFont);
            }
        }

        string ClearStr(string pS)
        {
            string Res = pS;
            if (string.IsNullOrEmpty(pS) || pS.Length <= CharInLine)
                return pS;
            string Space = new string(' ', pS.Length - CharInLine + 1);
            int ind = pS.IndexOf(Space);
            if (ind >= 0)
                Res = pS.Substring(0, ind) + pS.Substring(ind + Space.Length - 1);
            return Res;
        }

        private void PrintPageReceipt(object sender, PrintPageEventArgs e)
        {
            int[] IdWorkplacePays = Receipt.IdWorkplacePays;
            int leftPosition = 0, topPosition = 0;
            int maxChar = (e.PageBounds.Width - 40) / FONTSIZE;
            TopIndent = FONTSIZE + 1;


            for (var i = 0; i < IdWorkplacePays.Length; i++)
            {
                Receipt.IdWorkplacePay = IdWorkplacePays[i];

                string pointOfSale = Receipt.Fiscal?.Head; //"Супермаркет ВОПАК";            
                                                           //
                string nameCashier = $"Касир: {Receipt.NameCashier}";
                //Друк шапки

                //topPosition = PrintCenter(e, companyName, topPosition - TopIndent, maxChar, MainFont);
                topPosition = PrintCenter(e, pointOfSale, topPosition, maxChar, MainFont);
                //topPosition = PrintCenter(e, address, topPosition, maxChar, MainFont);

                // касир, номер чеку в 1С, власник картки та бонуси 
                topPosition = PrintLine(e, nameCashier, topPosition, maxChar, MainFont);
                topPosition = PrintLine(e, Receipt.NumberReceipt1C, topPosition, maxChar, MainFont);
                if (!string.IsNullOrEmpty(Receipt.Client?.NameClient))
                {
                    topPosition = PrintLine(e, Receipt.Client?.NameClient, topPosition, maxChar, MainFont);
                    topPosition = PrintLine(e, $"Бонуси: {Receipt.Client?.SumBonus}", topPosition, maxChar, MainFont);
                    topPosition = PrintLine(e, $"Скарбничка: {Receipt.Client?.Wallet}", topPosition, maxChar, MainFont);

                }

                //розділювач
                e.Graphics.DrawString("------------------------------------", MainFont, Brushes.Black, 0, topPosition += TopIndent);


                // Блок друку товарів
                foreach (var item in Receipt.Wares)
                {

                    topPosition = PrintLine(e, item.NameWares, topPosition, maxChar - 8, MainFont);
                    StringFormat stringFormatQuantity = new StringFormat();
                    stringFormatQuantity.Alignment = StringAlignment.Near;
                    stringFormatQuantity.LineAlignment = StringAlignment.Center;

                    StringFormat stringFormatPrice = new StringFormat();
                    stringFormatPrice.Alignment = StringAlignment.Far;
                    stringFormatPrice.LineAlignment = StringAlignment.Center;

                    Rectangle rectPrice = new Rectangle(leftPosition, topPosition += TopIndent + 1, WIDTHPAGE - 10, TopIndent);
                    string Quantity_x_Price = $"{item.Quantity} x {item.PriceEKKA}";
                    e.Graphics.DrawString(Quantity_x_Price, MainFont, Brushes.Black, rectPrice, stringFormatQuantity);
                    e.Graphics.DrawString($"{item.Price} {item.VatChar}", MainFont, Brushes.Black, rectPrice, stringFormatPrice);

                    if (item.DiscountEKKA > 0)
                    {
                        Rectangle rectDiscount = new Rectangle(leftPosition, topPosition += TopIndent, WIDTHPAGE - 10, TopIndent);
                        e.Graphics.DrawString("Знижка", MainFont, Brushes.Black, rectDiscount, stringFormatQuantity);
                        e.Graphics.DrawString(item.DiscountEKKA.ToString("f2"), MainFont, Brushes.Black, rectDiscount, stringFormatPrice);
                    }

                }

                //розділювач
                e.Graphics.DrawString("------------------------------------", MainFont, Brushes.Black, 0, topPosition += TopIndent);

                // Друк суми
                Font FontTotalSum = new Font("Courier", FONTSIZE + 2, FontStyle.Bold, GraphicsUnit.Point);
                StringFormat stringFormatSum = new StringFormat();
                stringFormatSum.Alignment = StringAlignment.Near;
                stringFormatSum.LineAlignment = StringAlignment.Center;

                StringFormat stringFormatTotalPrice = new StringFormat();
                stringFormatTotalPrice.Alignment = StringAlignment.Far;
                stringFormatTotalPrice.LineAlignment = StringAlignment.Center;

                Rectangle rectTotalSum = new Rectangle(leftPosition, topPosition + TopIndent, WIDTHPAGE - 10, TopIndent + 2);

                e.Graphics.DrawString("Сума", FontTotalSum, Brushes.Black, rectTotalSum, stringFormatSum);
                e.Graphics.DrawString(Receipt.SumCreditCard.ToString("C2"), FontTotalSum, Brushes.Black, rectTotalSum, stringFormatTotalPrice);
                if (Receipt.Fiscal?.Taxes?.Count() > 0)
                    foreach (var item in (Receipt.Fiscal.Taxes))
                    {
                        topPosition = PrintTwoColum(e, item.Name, item.Sum.ToString("f2"), topPosition + FONTSIZE + 2, maxChar - 8);
                    }

                //розділювач
                e.Graphics.DrawString("------------------------------------", MainFont, Brushes.Black, 0, topPosition += TopIndent);


                //інформація про банк
                if (Receipt.Payment.Count() > 0)
                {
                    foreach (var item in Receipt.Payment)
                    {
                        string IDTerminal = "Ідент. еквайра";
                        string EPZ = "ЕПЗ";
                        string CardHolder = "Платіжна система";
                        string RRN = "RRN";
                        string CodeAuthorization = "Код авт.";
                        if (item.TypePay == eTypePay.Card)
                        {
                            topPosition = PrintTwoColum(e, IDTerminal, item.NumberTerminal, topPosition, maxChar - 8);
                            topPosition = PrintTwoColum(e, EPZ, item.NumberCard, topPosition, maxChar - 8);
                            topPosition = PrintTwoColum(e, CardHolder, item.CardHolder, topPosition, maxChar - 8);
                            topPosition = PrintTwoColum(e, RRN, item.CodeAuthorization, topPosition, maxChar - 8); //ТРЕБА УТОЧНИТИ
                            topPosition = PrintTwoColum(e, CodeAuthorization, item.NumberSlip, topPosition, maxChar - 8);
                            //розділювач
                            e.Graphics.DrawString("------------------------------------", MainFont, Brushes.Black, 0, topPosition += TopIndent);
                        }
                    }
                }


                topPosition = PrintLine(e, $"ФН чеку {Receipt.Fiscal?.Number}", topPosition, maxChar, MainFont);
                topPosition = PrintLine(e, $"ФН ПРРО {Receipt.Fiscal?.Id}", topPosition, maxChar, MainFont);
                topPosition = PrintLine(e, DateTime.Now.ToString("dd/MM/yyyy H:mm"), topPosition, maxChar, MainFont);
                string QRInfo = string.IsNullOrEmpty(Receipt.Fiscal?.QR) ? "no data available" : Receipt.Fiscal?.QR;
                var qrCodeData = qrGenerator.CreateQrCode(QRInfo, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);
                var QRImage = qrCode.GetGraphic(1);
                e.Graphics.DrawImage(QRImage, (int)((WIDTHPAGE - QRImage.Width) * 0.85 / 2), topPosition += TopIndent + 1);
                topPosition += QRImage.Height;

                if (Receipt.TypeReceipt == eTypeReceipt.Sale)
                    topPosition = PrintCenter(e, "Фіскальний чек", topPosition, maxChar, FontTotalSum);
                else
                    topPosition = PrintCenter(e, "Видатковий чек", topPosition, maxChar, FontTotalSum);

                topPosition += TopIndent + 5;

            }
            Receipt.IdWorkplacePay = 0;

        }
        public int PrintCenter(PrintPageEventArgs e, string str, int topPosition, int maxChar, Font font)
        {
            if (str == string.Empty) return topPosition;
            int leftPosition = 0;
            string tmpVar = str;
            int pos = 0;
            if (str.Contains(Environment.NewLine))
            {
                pos = str.IndexOf(Environment.NewLine);
                str = str.Replace(Environment.NewLine, " ");
                tmpVar = str;
            }
            else
                pos = tmpVar.Length > maxChar ? tmpVar.Substring(0, maxChar).LastIndexOf(" ") : tmpVar.Length;

            while (str.Length > 0)
            {
                str = tmpVar.Substring(0, pos);
                leftPosition = Math.Abs((WIDTHPAGE - (FONTSIZE * str.Length)) / 2);
                if (!string.IsNullOrEmpty(str))
                    e.Graphics.DrawString(str, MainFont, Brushes.Black, leftPosition, topPosition += TopIndent);
                tmpVar = tmpVar.Substring(pos);
                pos = tmpVar.Length > maxChar ? tmpVar.Substring(0, maxChar).LastIndexOf(" ") : tmpVar.Length;
            }

            return topPosition;
        }
        public int PrintLine(PrintPageEventArgs e, string str, int topPosition, int maxChar, Font font)
        {
            if (str == string.Empty) return topPosition;
            string tmpVar = str;
            int maxCharProdukts = maxChar;

            while (str.Length > 0)
            {
                str = tmpVar.Length > maxCharProdukts ? tmpVar.Substring(0, maxCharProdukts) : tmpVar;
                if (!string.IsNullOrEmpty(str))
                    e.Graphics.DrawString(str, font, Brushes.Black, 0, topPosition += TopIndent);
                tmpVar = tmpVar.Length > maxCharProdukts ? tmpVar = tmpVar.Substring(maxCharProdukts) : "";
                str = tmpVar;
            }
            return topPosition;
        }

        public int PrintTwoColum(PrintPageEventArgs e, string str, string str2, int topPosition, int maxChar)
        {
            if (str == string.Empty && str2 == string.Empty) return topPosition;
            StringFormat stringFormatFirst = new StringFormat();
            stringFormatFirst.Alignment = StringAlignment.Near;
            stringFormatFirst.LineAlignment = StringAlignment.Center;

            StringFormat stringFormatSecond = new StringFormat();
            stringFormatSecond.Alignment = StringAlignment.Far;
            stringFormatSecond.LineAlignment = StringAlignment.Center;

            Rectangle rectPrice = new Rectangle(0, topPosition += TopIndent + 1, WIDTHPAGE - 10, TopIndent);

            e.Graphics.DrawString(str, MainFont, Brushes.Black, rectPrice, stringFormatFirst);
            e.Graphics.DrawString(str2, MainFont, Brushes.Black, rectPrice, stringFormatSecond);
            return topPosition;
        }

    }
}
