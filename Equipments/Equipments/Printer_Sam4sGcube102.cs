﻿using Microsoft.Extensions.FileSystemGlobbing;
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

using System.CodeDom.Compiler;
using System.Diagnostics.Metrics;
using Front.Equipments.Utils;
using System.Globalization;

namespace Front.Equipments.Implementation
{
    public class Printer_Sam4sGcube102 : Printer
    {

        Receipt Receipt = new();
        IEnumerable<string> ArrayStr;
        const int FONTSIZE = 6;
        const int SECONDFONTSIZE = 8;
        const int CharInLine = 34;
        const int WIDTHPAGE = 200;//e.PageBounds.Width; // ширина паперу принтера
        public Font MainFont = new("Courier", FONTSIZE, FontStyle.Bold, GraphicsUnit.Point);
        public Font SecondFont = new("Courier", SECONDFONTSIZE, FontStyle.Bold, GraphicsUnit.Point);
        public bool IsPrintReceipt = true;

        public int TopIndent;
        QRCodeGenerator qrGenerator = new();

        public Printer_Sam4sGcube102(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null) :
                         base(pEquipment, pConfiguration, eModelEquipment.pRRO_SG, pLoggerFactory)
        {
            NamePrinter = Configuration?.GetValue<string>($"{KeyPrefix}NamePrinter");
            IsPrintReceipt = Configuration?.GetValue<bool>($"{KeyPrefix}IsPrintReceipt") ?? true;
        }

        override public bool PrintReceipt(Receipt R)
        {
            if (IsPrintReceipt)
            {
                Receipt = R;
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrintPage += PrintPageReceipt;
                printDocument.DocumentName = "Receipt";
                PrintDialog printDialog = new();
                printDialog.Document = printDocument;
                printDialog.PrinterSettings.PrinterName = NamePrinter;
                printDialog.Document.Print(); // печатаем
            }
            return true;
        }
        object LockPrint = new object();
        override public bool Print(IEnumerable<string> pR)
        {
            lock (LockPrint)
            {
                var count = pR.Count();
                if (count > 0)
                {
                    ArrayStr = pR;

                    if (pR.FirstOrDefault().Contains("Номер замовлення")) // переробити
                    {
                        PrintDocument printDocument = new PrintDocument();
                        printDocument.PrintPage += PrintOrderNumber;
                        printDocument.DocumentName = "Order print";
                        PrintDialog printDialog = new();
                        printDialog.Document = printDocument;
                        printDialog.PrinterSettings.PrinterName = NamePrinter;
                        printDialog.Document.Print(); // печатаем
                    }
                    else if (pR.FirstOrDefault().Contains("Список замовлення")) // переробити
                    {
                        PrintDocument printDocument = new PrintDocument();
                        printDocument.PrintPage += PrintWaresOrder;
                        printDocument.DocumentName = "Print Wares order";
                        PrintDialog printDialog = new();
                        printDialog.Document = printDocument;
                        printDialog.PrinterSettings.PrinterName = NamePrinter;
                        printDialog.Document.Print(); // печатаем
                    }
                    else if (count == 2)
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
                    position = PrintQR(e, position, item);
                }
                else position = PrintLine(e, item, position, maxChar, SecondFont);
            }
        }
        private void PrintOrderNumber(object sender, PrintPageEventArgs e)
        {
            int position = 0;
            int fontSize = 12;
            int maxChar = (e.PageBounds.Width - 70) / fontSize;
            Font CustomFont = new("Courier", fontSize, FontStyle.Bold, GraphicsUnit.Point);
            Font CustomFont2 = new("Courier", 36, FontStyle.Bold, GraphicsUnit.Point);
            TopIndent = fontSize + 1;

            foreach (var item in ArrayStr)
            {
                if (item.Contains("Номер замовлення"))
                {
                    position = PrintLine(e, item, position, maxChar, CustomFont);
                }
                else
                    e.Graphics.DrawString(item, CustomFont2, Brushes.Black, 30, position + fontSize);
                //position = PrintLine(e, item, position, maxChar, CustomFont2);
            }
        }
        private void PrintWaresOrder(object sender, PrintPageEventArgs e)
        {
            int position = 0;
            int fontSize = 12;
            int maxChar = (e.PageBounds.Width - 70) / fontSize;
            Font CustomFont = new("Courier", fontSize, FontStyle.Bold, GraphicsUnit.Point);
            TopIndent = fontSize + 1;

            foreach (var item in ArrayStr)
            {
                //if (item.Contains("=>"))
                //{
                //    string[] substrings = item.Split(new string[] { "=>" }, StringSplitOptions.None);
                //    position = PrintTwoColum(e, substrings[0], substrings[1], position, maxChar );
                //}
                //else
                position = PrintLine(e, item, position +2, maxChar, CustomFont);

                //position = PrintLine(e, item, position, maxChar, CustomFont2);
            }
        }
        private void PrintArrayStrings(object sender, PrintPageEventArgs e)
        {
            int position = 0;
            int maxChar = 35;//(e.PageBounds.Width - 40) / FONTSIZE;
            TopIndent = FONTSIZE + 1;


            foreach (var item in ArrayStr)
            {
                if (item.Contains("QR=>"))
                {
                    position = PrintQR(e, position, item);
                }
                else
                    position = PrintLine(e, ClearStr(item), position, maxChar, MainFont);
            }
        }
        private int PrintQR(PrintPageEventArgs e, int position, string line)
        {
            string QRInfo = line.Replace("QR=>", string.Empty);
            var qrCodeData = qrGenerator.CreateQrCode(QRInfo, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            var QRImage = qrCode.GetGraphic(2);
            e.Graphics.DrawImage(QRImage, (int)((WIDTHPAGE - QRImage.Width) * 0.85 / 2), position += 10);
            position += QRImage.Height;
            return position;
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

                string pointOfSale = Receipt.Fiscal?.Head; //$"ТОВ \"Ужгород П.С.Ю.\"{Environment.NewLine}СУПЕРМАРКЕТ ВОПАК ЗАКАРПАТСЬКА ОБЛ, М. УЖГОРОД, ВУЛИЦЯ БЕСТУЖЕВА, БУД. 9{Environment.NewLine} ПН 32953730";            

                string nameCashier = $"Касир: {Receipt.NameCashier}";
                //Друк шапки

                topPosition = PrintCenter(e, pointOfSale, topPosition, maxChar, MainFont);

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
                    string CodeUKTZED = string.Empty;
                    if (item.IsUseCodeUKTZED)
                    {
                        CodeUKTZED = $"УКТЗЕД:{item.CodeUKTZED} ШК:{item.BarCode}";
                        topPosition = PrintLine(e, CodeUKTZED, topPosition, maxChar, MainFont);
                    }
                    if (item.ExciseStamp != null)
                    {
                        var ExciseStamp = item.ExciseStamp.Split(',');
                        for (int j = 0; j < ExciseStamp.Length; j++)
                        {
                            topPosition = PrintLine(e, $"Акцизна марка: {ExciseStamp[j]}", topPosition, maxChar, MainFont);
                        }
                    }


                    string Quantity_x_Price = $"{item.Quantity} x {item.PriceEKKA}";
                    string Price_VatChar = (item.PriceEKKA * item.Quantity).ToString("F2") + item.VatChar;
                    if (item.Quantity == 1)
                    {
                        topPosition = PrintTwoColum(e, item.NameWares, Price_VatChar, topPosition, maxChar - 16);
                    }
                    else
                    {
                        topPosition = PrintLine(e, item.NameWares, topPosition, maxChar - 8, MainFont);
                        topPosition = PrintTwoColum(e, Quantity_x_Price, Price_VatChar, topPosition, maxChar - 8);
                    }



                    if (item.SumDiscountEKKA > 0)
                    {
                        topPosition = PrintTwoColum(e, "Знижка", item.SumDiscountEKKA.ToString("f2"), topPosition, maxChar - 8);
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

                string TotalSum = "";
                if (Receipt.TypePay == eTypePay.Cash)
                    TotalSum = Receipt.SumCash.ToString("F2");
                else if (Receipt.TypePay == eTypePay.Card)
                    TotalSum = Receipt.SumCreditCard.ToString("F2");
                else TotalSum = Receipt.SumFiscal.ToString("F2");
                e.Graphics.DrawString(TotalSum, FontTotalSum, Brushes.Black, rectTotalSum, stringFormatTotalPrice);
                if (Receipt.Fiscal?.Taxes?.Count() > 0)
                {
                    int intentTaxes = FONTSIZE + 2;
                    foreach (var item in (Receipt.Fiscal.Taxes))
                    {
                        topPosition = PrintTwoColum(e, item.Name, item.Sum.ToString("f2"), topPosition + intentTaxes, maxChar - 8);
                        intentTaxes = 0;
                    }
                }
                else topPosition += 4;


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

                if (Receipt.Footer.Any())
                {
                    foreach (var item in Receipt.Footer)
                    {
                        topPosition = PrintLine(e, item, topPosition, maxChar, MainFont);
                    }
                    //розділювач
                    e.Graphics.DrawString("------------------------------------", MainFont, Brushes.Black, 0, topPosition += TopIndent);
                }

                topPosition = PrintLine(e, $"ФН чеку {Receipt.Fiscal?.Number}", topPosition, maxChar, MainFont);
                topPosition = PrintLine(e, $"ФН ПРРО {Receipt.Fiscal?.Id}", topPosition, maxChar, MainFont);
                topPosition = PrintLine(e, $"{Receipt.Fiscal.DT.ToString("dd/MM/yyyy H:mm")}", topPosition, maxChar, MainFont);
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
            if (string.IsNullOrEmpty(str)) return topPosition;
            int leftPosition = 0;
            string tmpVar = str;
            int pos = 0;
            if (str.Contains(Environment.NewLine))
            {
                pos = str.IndexOf(Environment.NewLine);
                pos += 2;
                // str = str.Substring(0, pos+4);//str.Replace(Environment.NewLine, " ");
                // tmpVar = str;
            }
            else
                pos = tmpVar.Length > maxChar ? tmpVar.Substring(0, maxChar).LastIndexOf(" ") : tmpVar.Length;

            while (str.Length > 0)
            {
                str = tmpVar.Substring(0, pos);
                if (str.Contains(Environment.NewLine))
                {
                    pos = str.IndexOf(Environment.NewLine);
                    pos += 2;
                    str = tmpVar.Substring(0, pos);
                }
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
            if (string.IsNullOrEmpty(str)) return topPosition;
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
            if (string.IsNullOrEmpty(str) && string.IsNullOrEmpty(str2)) return topPosition;
            StringFormat stringFormatFirst = new StringFormat();
            stringFormatFirst.Alignment = StringAlignment.Near;
            stringFormatFirst.LineAlignment = StringAlignment.Center;

            StringFormat stringFormatSecond = new StringFormat();
            stringFormatSecond.Alignment = StringAlignment.Far;
            stringFormatSecond.LineAlignment = StringAlignment.Center;

            if (str.Length > maxChar)
            {
                string tmpVar = str;
                List<string> listLine = new List<string>();
                while (str.Length > 0)
                {
                    str = tmpVar.Length > maxChar ? tmpVar.Substring(0, maxChar) : tmpVar;
                    if (!string.IsNullOrEmpty(str))
                        listLine.Add(str);
                    tmpVar = tmpVar.Length > maxChar ? tmpVar = tmpVar.Substring(maxChar) : "";
                    str = tmpVar;
                }
                int counter = 0;
                foreach (var item in listLine)
                {
                    Rectangle rectPrice = new Rectangle(0, topPosition += TopIndent + 1, WIDTHPAGE - 10, TopIndent);

                    e.Graphics.DrawString(item, MainFont, Brushes.Black, rectPrice, stringFormatFirst);
                    if (counter == 0)
                        e.Graphics.DrawString(str2, MainFont, Brushes.Black, rectPrice, stringFormatSecond);
                    counter++;
                }
            }

            else
            {
                Rectangle rectPrice = new Rectangle(0, topPosition += TopIndent + 1, WIDTHPAGE - 10, TopIndent);

                e.Graphics.DrawString(str, MainFont, Brushes.Black, rectPrice, stringFormatFirst);
                e.Graphics.DrawString(str2, MainFont, Brushes.Black, rectPrice, stringFormatSecond);
            }

            return topPosition;
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
    }
}
