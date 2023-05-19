﻿using SharedLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QRCoder;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Data.Common;

namespace PrintServer
{
    public class GenLabel
    {
        int current = 0;
        cPrice[] price;
        eBrandName BrandName;
        MSSQL db = new MSSQL();//("Server = SQLSRV2; Database=DW;Trusted_Connection=True;"
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        Image logo;// = Image.FromFile("D:\\Vopak.bmp");
        Image logo2;
        Image CurLogo;
        //int CodeWarehouse = 1;
        //string NamePrinter = "";
        //string NamePrinterYelow;
        string NameDocument;

        // Тестовий запит
        //{
        //  "CodeWares": "000151859,000137049,000148925,000139319,000090678,000139321,000187135,000122696,000176872",
        //  "CodeWarehouse": 68,
        //  "Login":"Gelo"
        //}
        public GenLabel()
        {
            //string Warehouse = System.Configuration.ConfigurationManager.AppSettings["CodeWarehouse"];
            //if (!string.IsNullOrEmpty(Warehouse))
            //CodeWarehouse = Convert.ToInt32(Warehouse);

            string PathLogo = System.Configuration.ConfigurationManager.AppSettings["PathLogo"];
            if (!string.IsNullOrEmpty(PathLogo))
                logo = Image.FromFile(PathLogo);
            PathLogo = System.Configuration.ConfigurationManager.AppSettings["PathLogo2"];
            if (!string.IsNullOrEmpty(PathLogo))
                logo2 = Image.FromFile(PathLogo);

            //NamePrinterYelow = System.Configuration.ConfigurationManager.AppSettings["NamePrinterYelow"];
            //NamePrinter = System.Configuration.ConfigurationManager.AppSettings["NamePrinter"];
        }

        public List<cPrice> GetCode(int parCodeWarehouse, string parCodeWares)
        {
            var L = new List<cPrice>();
            if (string.IsNullOrEmpty(parCodeWares))
                return L;

            foreach (var el in parCodeWares.Split(','))
            {
                int CodeWares;
                if (int.TryParse(el, out CodeWares))
                {
                    var pr = GetPrice(parCodeWarehouse, CodeWares);
                    L.Add(pr);
                }
            }
            return L;
        }

        public cPrice GetPrice(int parCodeWarehouse, int? parCodeWares, int? parArticle = null)
        {
            var Sql = "select dbo.GetPrice(@CodeWarehouse ,@CodeWares,null,@Article,1)";

            var param = new { CodeWarehouse = parCodeWarehouse, CodeWares = parCodeWares, Article = parArticle };
            var json = db.ExecuteScalar<object, string>(Sql, param);
            var price = JsonConvert.DeserializeObject<cPrice>(json);
            return price;
        }

        public void Print(IEnumerable<cPrice> parPrice, string parNamePrinter, string parNamePrinterYelow, string pNameDocument = null, eBrandName brandName = eBrandName.Vopak, bool isShort = true, bool isWarehouseNOV = false) // TMP isWarehouseNOV
        {
            CurLogo = (brandName == eBrandName.Vopak || logo2 == null ? logo : logo2);
            BrandName = brandName;
            current = 0;
            if (string.IsNullOrEmpty(parNamePrinterYelow))
            {
                price = parPrice.ToArray();
                if (price.Count() > 0)
                    PrintServer(parNamePrinter, pNameDocument, isShort);
            }
            else
            {
                price = parPrice.Where(el => el.ActionType == 0).ToArray();
                if (price.Count() > 0)
                    PrintServer(parNamePrinter, pNameDocument, isShort);
                current = 0;
                price = parPrice.Where(el => el.ActionType != 0).ToArray();
                if (price.Count() > 0)
                    PrintServer(parNamePrinterYelow, pNameDocument, true, true, isWarehouseNOV); //Жовті завжди короткі.
            }

        }

        public void PrintServer(string pNamePrinter, string pNameDoc = "Label", bool isShort = true, bool isYelow = false, bool isWarehouseNOV = false)
        {
            // объект для печати
            PrintDocument printDocument = new PrintDocument();

            // обработчик события печати

            if (isYelow && isWarehouseNOV) // новий для тесту
            {
                printDocument.PrintPage += PrintPageHandlerYelow;
                printDocument.DocumentName = $"{pNameDoc}_{price.Count()}";
                //стандартний папір
                //printDocument.DefaultPageSettings.PaperSize = new PaperSize("54 x 30 mm", 230, 130);

                //широкий  папір
                printDocument.DefaultPageSettings.PaperSize = new PaperSize("70 x 36 mm", 280, 130);
            }
            else
            {

                if (isShort)//звичайний цінник
                {
                    printDocument.PrintPage += PrintPageHandler;
                    printDocument.DocumentName = $"{pNameDoc}_{price.Count()}";
                    printDocument.DefaultPageSettings.PaperSize = new PaperSize("54 x 36 mm", 230, 130);
                }
                else //цінник для опту
                {
                    printDocument.PrintPage += PrintPageHandlerOpt;
                    printDocument.DocumentName = $"{pNameDoc}_{price.Count()}";
                    printDocument.DefaultPageSettings.PaperSize = new PaperSize("80 x 36 mm", 340, 130);
                }
            }



            // диалог настройки печати
            PrintDialog printDialog = new PrintDialog();

            // установка объекта печати для его настройки
            printDialog.Document = printDocument;

            //System.Drawing.Printing.PrinterSettings newSettings = new System.Drawing.Printing.PrinterSettings();
            printDialog.PrinterSettings.PrinterName = pNamePrinter;//newSettings.PrinterName;
            printDialog.Document.Print(); // печатаем
            if (!string.IsNullOrEmpty(NameDocument))//Друкуємо підсумок по документу.
            {
                if (isYelow)
                    printDocument.PrintPage -= PrintPageHandlerYelow;
                else
                {

                    if (isShort)
                        printDocument.PrintPage -= PrintPageHandler;
                    else
                        printDocument.PrintPage -= PrintPageHandlerOpt;
                    printDocument.PrintPage += PrintTotal;
                    printDialog.Document.Print();
                }
            }
        }

        /*public void PrintPreview()
        {
            PrintPreviewDialog pd = new PrintPreviewDialog();

            // объект для печати
            PrintDocument printDocument = new PrintDocument();
            pd.Document = printDocument;
            // обработчик события печати
            printDocument.PrintPage += PrintPageHandler;
            printDocument.DefaultPageSettings.PaperSize = new PaperSize("54 x 60 mm", 230, 122);

            // диалог настройки печати
            PrintDialog printDialog = new PrintDialog();

            // установка объекта печати для его настройки
            printDialog.Document = printDocument;

            System.Drawing.Printing.PrinterSettings newSettings = new System.Drawing.Printing.PrinterSettings();
            //printDialog.PrinterSettings.PrinterName = "BTP-R580II(U) 1";//newSettings.PrinterName;

            pd.ShowDialog();
            //printDialog.Document.Print(); // печатаем
        }*/

        void PrintPageHandler(object sender, PrintPageEventArgs e)
        {
            if (price == null)
                return;
            while (current < price.Count())
            {
                PrintLabel(price[current], e);
                current++;
                e.HasMorePages = (current != price.Count());
                if (current != price.Count())
                    return;
            }
        }
        void PrintPageHandlerOpt(object sender, PrintPageEventArgs e)
        {
            if (price == null)
                return;
            while (current < price.Count())
            {
                PrintLabelOpt(price[current], e);
                current++;
                e.HasMorePages = (current != price.Count());
                if (current != price.Count())
                    return;
            }
        }
        void PrintPageHandlerYelow(object sender, PrintPageEventArgs e)
        {
            if (price == null)
                return;
            while (current < price.Count())
            {
                PrintLabelYelow70(price[current], e);
                current++;
                e.HasMorePages = (current != price.Count());
                if (current != price.Count())
                    return;
            }
        }

        void PrintTotal(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawString(NameDocument, new Font("Arial", 22), Brushes.Black, 0, 20);
            e.Graphics.DrawString($"Вcього:{price.Count()}", new Font("Arial", 22), Brushes.Black, 0, 20);
        }

        public void PrintLabel(cPrice parPrice, PrintPageEventArgs e)
        {
            int LengthName = 28;
            string Name1, Name2 = "";
            if (parPrice.Name.Length < LengthName)
                Name1 = parPrice.Name;
            else
            {
                int pos = parPrice.Name.Substring(0, LengthName).LastIndexOf(" ");
                Name1 = parPrice.Name.Substring(0, pos);
                Name2 = parPrice.Name.Substring(pos);
                if (Name2.Length < LengthName)
                    Name2 = new string(' ', (LengthName - Name2.Length) / 2) + Name2;
            }
            Name1 = new string(' ', ((LengthName - Name1.Length) / 2)) + Name1;
            if (Name2.Length > LengthName + 3)
                Name2 = Name2.Substring(0, LengthName + 3);

            if (CurLogo != null)
                e.Graphics.DrawImage(CurLogo, 10, 0);
            e.Graphics.DrawString(DateTime.Now.ToString("dd/MM/yyyy H:mm"), new Font("Arial", 8), Brushes.Black, 120, 0); //Час

            //string BarCodePrice = parPrice.Code.ToString() + "-" + parPrice.Price.ToString() + (parPrice.PriceOpt == 0 ? "" : "-" + parPrice.PriceOpt.ToString());
            int strPrice = ((int)(parPrice.Price * 100M));
            var qrCodeData = qrGenerator.CreateQrCode($"{parPrice.Code}-{strPrice}", QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            e.Graphics.DrawImage(qrCode.GetGraphic(2), 165, 50);

            e.Graphics.DrawString(Name1, new Font("Arial", 11, FontStyle.Bold), Brushes.Black, 0, 16);
            e.Graphics.DrawString(Name2, new Font("Arial", 11, FontStyle.Bold), Brushes.Black, 0, 33);

            int LeftBill = 0, LeftCoin = 135;
            float coef = 1;
            var price = parPrice.StrPrice.Split('.');
            //price[0] = "4293";
            switch (price[0].Count())
            {
                case 1:
                    LeftBill = 40;
                    LeftCoin = 100;
                    break;
                case 2:
                    LeftBill = 20;
                    LeftCoin = 120;
                    break;
                case 3:
                    LeftBill = 5;
                    LeftCoin = 135;
                    coef = 0.9f;
                    break;
                default:
                    LeftBill = 0;
                    LeftCoin = 135;
                    coef = 0.70f;
                    break;
            }

            Graphics gr = e.Graphics;
            GraphicsState state = gr.Save();
            gr.ResetTransform();
            gr.ScaleTransform(coef, 1.0f);
            e.Graphics.DrawString(price[0], new Font("Arial Black", 50), Brushes.Black, LeftBill, 35);
            gr.Restore(state);

            //e.Graphics.DrawString(price[0], new Font("Arial Black", 35), Brushes.Black, LeftBill, 35);
            e.Graphics.DrawString(price[1], new Font("Arial Black", 18), Brushes.Black, LeftCoin, 50);
            e.Graphics.DrawString("грн", new Font("Arial", 13, FontStyle.Bold), Brushes.Black, LeftCoin + 3, 75);

            e.Graphics.DrawString(parPrice.StrUnit, new Font("Arial", 14), Brushes.Black, LeftCoin + 3, 93);
            if (parPrice.BarCodes != null)
            {
                if (parPrice.BarCodes.Length > 27)
                    parPrice.BarCodes = parPrice.BarCodes.Substring(0, 27);
                e.Graphics.DrawString(parPrice.BarCodes, new Font("Arial", 7), Brushes.Black, 10, 120);
            }
            e.Graphics.DrawString(parPrice.Article.ToString(), new Font("Arial", 8, FontStyle.Bold), Brushes.Black, 170, 110);
            e.Graphics.DrawLine(new Pen(Color.Black, 1), 0, 133, 150, 133);
            //e.Graphics.DrawString(parPrice.Article.ToString(), new Font("Arial", 8), Brushes.Black, 170, 120);
        }

        /// <summary>
        /// Жовті цінники звичайної ширини 54мм
        /// </summary>
        /// <param name="parPrice"></param>
        /// <param name="e"></param>
        public void PrintLabelYelow(cPrice parPrice, PrintPageEventArgs e)
        {
            int LengthName = 18;
            string Name1, Name2 = "", Name3 = "";
            int startSecondColumn = 170;
            if (parPrice.Name.Length < LengthName)
                Name1 = parPrice.Name;
            else
            {
                int pos = parPrice.Name.Substring(0, LengthName).LastIndexOf(" ");
                Name1 = parPrice.Name.Substring(0, pos);
                Name2 = parPrice.Name.Substring(pos);
                if (Name2.Length > LengthName)
                {
                    pos = Name2.Substring(0, LengthName).LastIndexOf(" ");
                    Name3 = Name2.Substring(pos);
                    Name2 = Name2.Substring(0, pos);
                    //if (Name3.Length < LengthName)
                    //    Name3 = new string(' ', (LengthName - Name3.Length) / 2) + Name3;

                }

            }
            //Name1 = new string(' ', ((LengthName - Name1.Length) / 2)) + Name1;
            //Name2 = new string(' ', (LengthName - Name2.Length) / 2) + Name2;
            if (Name3.Length > LengthName + 3)
                Name3 = Name3.Substring(0, LengthName);

            //QR
            int strPrice = ((int)(parPrice.Price * 100M));
            var qrCodeData = qrGenerator.CreateQrCode($"{parPrice.Code}-{strPrice}", QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            e.Graphics.DrawImage(qrCode.GetGraphic(2), startSecondColumn, 10);
            //назви
            e.Graphics.DrawString(" " + Name1, new Font("Arial", 11, FontStyle.Bold), Brushes.Black, 3, 0);
            e.Graphics.DrawString(Name2, new Font("Arial", 11, FontStyle.Bold), Brushes.Black, 3, 17);
            e.Graphics.DrawString(Name3, new Font("Arial", 11, FontStyle.Bold), Brushes.Black, 3, 34);
            //Час
            e.Graphics.DrawString(DateTime.Now.ToString("dd/MM/yy H:mm"), new Font("Arial", 5), Brushes.Black, startSecondColumn, 0);
            //штрихкод
            if (parPrice.BarCodes != null)
            {
                if (parPrice.BarCodes.Length > 13)
                    parPrice.BarCodes = parPrice.BarCodes.Substring(0, 13);
                e.Graphics.DrawString(parPrice.BarCodes, new Font("Arial", 5), Brushes.Black, startSecondColumn + 3, 7);
            }
            //артикул
            e.Graphics.DrawString(parPrice.Article.ToString(), new Font("Arial", 5, FontStyle.Bold), Brushes.Black, startSecondColumn + 15, 62);



            int leftIndentMainPrice = 0, topIndentMainPrice = 55, LeftCoinMain = 135;
            int leftIndentSecondPrice = 0, topIndentSecondPrice = 75, LeftCoinSecond = 190;
            float coef = 1;
            var price = parPrice.StrPrice.Split('.');
            var priceNormal = parPrice.StrPriceNormal.Split('.');
            //price[0] = "4293";
            switch (price[0].Count())
            {
                case 1:
                    leftIndentMainPrice = 30;
                    LeftCoinMain = 75;
                    leftIndentSecondPrice = 170;
                    LeftCoinSecond = 195;
                    break;
                case 2:
                    leftIndentMainPrice = 15;
                    LeftCoinMain = 100;
                    leftIndentSecondPrice = 155;
                    LeftCoinSecond = 205;
                    break;
                case 3:
                    leftIndentMainPrice = 23;
                    LeftCoinMain = 95;
                    leftIndentSecondPrice = 218;
                    LeftCoinSecond = 205;
                    coef = 0.7f;
                    break;
                default:
                    leftIndentMainPrice = 50;
                    LeftCoinMain = 100;
                    leftIndentSecondPrice = 305;
                    LeftCoinSecond = 205;
                    coef = 0.50f;
                    break;
            }
            Pen myPen = new Pen(Color.Black, 1);
            e.Graphics.DrawRectangle(myPen, 10, 55, 135, 74);
            Graphics gr = e.Graphics;
            GraphicsState state = gr.Save();
            gr.ResetTransform();
            gr.ScaleTransform(coef, 1.0f);
            e.Graphics.DrawString(price[0], new Font("Arial Black", 40), Brushes.Black, leftIndentMainPrice, topIndentMainPrice);
            if (parPrice.Price < parPrice.PriceNormal)
                e.Graphics.DrawString(priceNormal[0], new Font("Arial Black", 25), Brushes.Black, leftIndentSecondPrice, topIndentSecondPrice);
            gr.Restore(state);

            //e.Graphics.DrawString(price[0], new Font("Arial Black", 35), Brushes.Black, LeftBill, 35);
            e.Graphics.DrawString(price[1], new Font("Arial Black", 15), Brushes.Black, LeftCoinMain, topIndentMainPrice += 15);
            e.Graphics.DrawString("грн", new Font("Arial", 10, FontStyle.Bold), Brushes.Black, LeftCoinMain + 3, topIndentMainPrice += 20);
            e.Graphics.DrawString(parPrice.StrUnit, new Font("Arial", 11), Brushes.Black, LeftCoinMain + 3, topIndentMainPrice += 13);

            if (parPrice.Price < parPrice.PriceNormal)
            {
                e.Graphics.DrawString(priceNormal[1], new Font("Arial Black", 8), Brushes.Black, LeftCoinSecond, topIndentSecondPrice += 10);
                e.Graphics.DrawString("грн", new Font("Arial", 6, FontStyle.Bold), Brushes.Black, LeftCoinSecond + 2, topIndentSecondPrice += 10);
                e.Graphics.DrawString(parPrice.StrUnit, new Font("Arial", 6), Brushes.Black, LeftCoinSecond + 2, topIndentSecondPrice += 10);
            }

            if (parPrice.IsOnlyCard)
            {
                if (BrandName == eBrandName.Spar)
                {
                    e.Graphics.DrawString("Ціна з карткою \"Мій Spar\"", new Font("Arial Black", 6), Brushes.Black, 13, 58);
                }
                else
                    e.Graphics.DrawString("Ціна з карткою лояльності", new Font("Arial Black", 6), Brushes.Black, 13, 58);
                e.Graphics.DrawString("Звичайна ціна", new Font("Arial Black", 6), Brushes.Black, 156, topIndentSecondPrice - 30);

            }
            else
                e.Graphics.DrawLine(new Pen(Color.Black, 2), leftIndentSecondPrice * coef - 7, topIndentSecondPrice += 15, LeftCoinSecond + 20, 80);



            // e.Graphics.DrawLine(new Pen(Color.Black, 1), 0, 133, 231, 133);
            // e.Graphics.DrawLine(new Pen(Color.Black, 1), 231, 0, 231, 130);
            //e.Graphics.DrawString(parPrice.Article.ToString(), new Font("Arial", 8), Brushes.Black, 170, 120);
        }
        /// <summary>
        /// Широкі жовті цінники 70 мм
        /// </summary>
        /// <param name="parPrice">Інформація про товар </param>
        /// <param name="e">PrintPageEvent</param>
        public void PrintLabelYelow70(cPrice parPrice, PrintPageEventArgs e)
        {
            if (!parPrice.IsOnlyCard)
            {


                int LengthName = 24;
                int leftIntentQR = 200;
                int topIntentQR = 0;
                //parPrice.Name = "назва1 назва3 назва4 назва5 назва6 назва7 назва8 назва9 назва10 назва11 назва12 назва назва назва назва назва назва назва назва назва назва назва назва назва"; //20 21 22 23 24 25 26 27 28 29 30
                int leftIntendName = 8;
                int topIntendName = -10;
                Font FontForNames = new Font("Arial", 12, FontStyle.Bold);
                string name = parPrice.Name;
                string tmpVar = name;
                int maxCharProdukts = LengthName;
                int countLine = 0;
                //QR
                int strPrice = ((int)(parPrice.Price * 100M));
                var qrCodeData = qrGenerator.CreateQrCode($"{parPrice.Code}-{strPrice}", QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);
                var imageQR = qrCode.GetGraphic(2);
                e.Graphics.DrawImage(imageQR, leftIntentQR += 5, topIntentQR);

                while (name.Length > 0 && countLine<3)
                {
                    int pos = tmpVar.Length > maxCharProdukts ? tmpVar.Substring(0, maxCharProdukts).LastIndexOf(" ") +1: tmpVar.Length;
                    name = tmpVar.Length > pos ? tmpVar.Substring(0, pos) : tmpVar;
                    if (!string.IsNullOrEmpty(name))
                        e.Graphics.DrawString(name, FontForNames, Brushes.Black, leftIntendName, topIntendName += 16);
                    tmpVar = tmpVar.Length > pos ? tmpVar = tmpVar.Substring(pos) : "";
                    name = tmpVar;
                    countLine++;
                }





                FontFamily fontFamily = new FontFamily("Arial");
                Font font = new Font(fontFamily, 6, FontStyle.Bold);
                var aa = font.Height;
                StringFormat stringFormat = new StringFormat();
                SolidBrush solidBrush = new SolidBrush(Color.Black);

                stringFormat.FormatFlags = StringFormatFlags.DirectionVertical;
                //артикул
                e.Graphics.DrawString(parPrice.Article.ToString(), new Font("Arial", 6, FontStyle.Bold), Brushes.Black, leftIntentQR += 7, topIntentQR += imageQR.Height-2);
                //штрихкод
                if (parPrice.BarCodes != null)
                {
                    if (parPrice.BarCodes.Length > 13)
                        parPrice.BarCodes = parPrice.BarCodes.Substring(0, 13);

                    PointF pointBarCodes = new PointF(leftIntentQR += 47, 5);
                    e.Graphics.DrawString(parPrice.BarCodes, font, solidBrush, pointBarCodes, stringFormat);
                    //e.Graphics.DrawString(parPrice.BarCodes, new Font("Arial", 6, FontStyle.Bold), Brushes.Black, leftIntentQR, topIntentQR += 7);
                }
                //Час
                PointF pointDateTime = new PointF(leftIntentQR+=7, 5);
                e.Graphics.DrawString(DateTime.Now.ToString("dd/MM/yyyy H:mm"), font, solidBrush, pointDateTime, stringFormat);
                //e.Graphics.DrawString(DateTime.Now.ToString("dd/MM/yyyy H:mm"), new Font("Arial", 6, FontStyle.Bold), Brushes.Black, leftIntentQR, topIntentQR += 7);
                




                int leftIndentMainPrice = 0, topIndentMainPrice = 50, LeftCoinMain = 135;
                int leftIndentSecondPrice = 0, topIndentSecondPrice = 85, LeftCoinSecond = 190;
                int intentLine = topIndentSecondPrice;
                float coef = 1;
                var price = parPrice.StrPrice.Split('.');
                var priceNormal = parPrice.StrPriceNormal.Split('.');
                //price[0] = "12345";
                //priceNormal[0] = "22345";
                switch (price[0].Count())
                {
                    case 1:
                        leftIndentMainPrice = 100;
                        LeftCoinMain = 95;
                        coef = 0.65f;
                        break;
                    case 2:
                        leftIndentMainPrice = 70;
                        LeftCoinMain = 110;
                        coef = 0.65f;
                        break;
                    case 3:
                        leftIndentMainPrice = 40; //55;
                        LeftCoinMain = 125; //170;
                        coef = 0.65f;
                        break;
                    case 4:
                        leftIndentMainPrice = 20; //90
                        LeftCoinMain = 140; //170
                        coef = 0.65f; //0.7
                        break;
                    default:
                        leftIndentMainPrice = 30; //90
                        LeftCoinMain = 138; //170
                        coef = 0.5f; //0.7
                        break;
                }
                switch (priceNormal[0].Count())
                {
                    case 1:
                        leftIndentSecondPrice = 210;
                        LeftCoinSecond = 229;
                        break;
                    case 2:
                        leftIndentSecondPrice = 205;
                        LeftCoinSecond = 239;
                        break;
                    case 3:
                        leftIndentSecondPrice = 195;
                        LeftCoinSecond = 245;
                        break;
                    case 4:
                        leftIndentSecondPrice = 185;
                        LeftCoinSecond = 250;
                        break;
                    default:
                        leftIndentSecondPrice = 175;
                        LeftCoinSecond = 255;
                        break;
                }
                e.Graphics.DrawRectangle(new Pen(Color.Black, 1), 0, 60, 180, 80);

                Graphics gr = e.Graphics;
                GraphicsState state = gr.Save();
                gr.ResetTransform();
                gr.ScaleTransform(coef, 1.0f);

                e.Graphics.DrawString(price[0], new Font("Arial Black", 50), Brushes.Black, leftIndentMainPrice, topIndentMainPrice);

               // if (parPrice.Price < parPrice.PriceNormal)

                gr.Restore(state);

                //e.Graphics.DrawString(price[0], new Font("Arial Black", 35), Brushes.Black, LeftBill, 35);
                e.Graphics.DrawString(price[1], new Font("Arial Black", 16), Brushes.Black, LeftCoinMain, topIndentMainPrice += 20);
                e.Graphics.DrawString("грн", new Font("Arial", 10, FontStyle.Bold), Brushes.Black, LeftCoinMain + 3, topIndentMainPrice += 25);
                e.Graphics.DrawString(parPrice.StrUnit, new Font("Arial", 11), Brushes.Black, LeftCoinMain + 3, topIndentMainPrice += 13);

                if (parPrice.Price < parPrice.PriceNormal)
                {
                    e.Graphics.DrawString(priceNormal[0], new Font("Arial", 20, FontStyle.Bold), Brushes.Black, leftIndentSecondPrice , topIndentSecondPrice); //White

                    e.Graphics.DrawString(priceNormal[1], new Font("Arial Black", 9), Brushes.Black, LeftCoinSecond, topIndentSecondPrice );//White
                    e.Graphics.DrawString("грн", new Font("Arial", 9, FontStyle.Bold), Brushes.Black, LeftCoinSecond, topIndentSecondPrice += 10);//White
                    e.Graphics.DrawString(parPrice.StrUnit, new Font("Arial", 9), Brushes.Black, LeftCoinSecond, topIndentSecondPrice += 10);//White

                    e.Graphics.DrawLine(new Pen(Color.Black, 1), leftIndentSecondPrice, topIndentSecondPrice += 7, LeftCoinSecond + 20, intentLine + 4);//White

                }


                
            }
            else
            {



                int LengthName = 34;
                string Name1, Name2 = "";
                int leftIntentQR = 5;
                int topIntentQR = 5;

                //QR
                int strPrice = ((int)(parPrice.Price * 100M));
                var qrCodeData = qrGenerator.CreateQrCode($"{parPrice.Code}-{strPrice}", QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);
                var imageQR = qrCode.GetGraphic(2);
                e.Graphics.DrawImage(imageQR, leftIntentQR, topIntentQR + 3);

                //артикул
                e.Graphics.DrawString(parPrice.Article.ToString(), new Font("Arial", 6, FontStyle.Bold), Brushes.Black, leftIntentQR += 7, topIntentQR += imageQR.Height-2);
                //Час
                e.Graphics.DrawString(DateTime.Now.ToString("dd/MM/yy H:mm"), new Font("Arial", 6, FontStyle.Bold), Brushes.Black, leftIntentQR, topIntentQR += 7);
                //штрихкод
                if (parPrice.BarCodes != null)
                {
                    if (parPrice.BarCodes.Length > 13)
                        parPrice.BarCodes = parPrice.BarCodes.Substring(0, 13);
                    e.Graphics.DrawString(parPrice.BarCodes, new Font("Arial", 6, FontStyle.Bold), Brushes.Black, leftIntentQR, topIntentQR += 7);
                }
                e.Graphics.DrawLine(new Pen(Color.Black, 2), 0, topIntentQR+=10, 300, topIntentQR);


                Point[] points = {
                                new Point(50, 0),
                                new Point(60, 15),
                                new Point(215, 15),
                                new Point(225, 0)};
                GraphicsPath path = new GraphicsPath();
                path.StartFigure(); // Start the second figure.
                path.AddLines(points);
                path.CloseFigure(); // Second figure is closed.
                e.Graphics.FillPath(new SolidBrush(Color.Black), path);

                if (BrandName == eBrandName.Spar)
                {
                    e.Graphics.DrawString("Ціна з карткою \"Мій Spar\"", new Font("Arial Black", 7), Brushes.White, 63, 0);
                }
                else
                    e.Graphics.DrawString("Ціна з карткою лояльності", new Font("Arial Black", 7), Brushes.White, 63, 0);
                e.Graphics.DrawString("Звичайна", new Font("Arial Black", 7), Brushes.Black, 213, 33);
                e.Graphics.DrawString("ціна", new Font("Arial Black", 7), Brushes.Black, 228, 42);

                int leftIndentMainPrice = 0, topIndentMainPrice = 8, LeftCoinMain = 135;
                int leftIndentSecondPrice = 0, topIndentSecondPrice = 8, LeftCoinSecond = 190;
                int intentLine = topIndentSecondPrice;
                float coef = 1;
                var price = parPrice.StrPrice.Split('.');
                var priceNormal = parPrice.StrPriceNormal.Split('.');
                //price[0] = "9";
                //priceNormal[0] = "9";
                switch (price[0].Count())
                {
                    case 1:
                        leftIndentMainPrice = 100;
                        LeftCoinMain = 140;
                        break;
                    case 2:
                        leftIndentMainPrice = 80;
                        LeftCoinMain = 160;
                        break;
                    case 3:
                        leftIndentMainPrice = 55;
                        LeftCoinMain = 175;
                        break;
                    default:
                        leftIndentMainPrice = 95;
                        LeftCoinMain = 175;
                        coef = 0.70f;
                        break;
                }
                switch (priceNormal[0].Count())
                {
                    case 1:
                        leftIndentSecondPrice = 225;
                        LeftCoinSecond = 245;
                        break;
                    case 2:
                        leftIndentSecondPrice = 218;
                        LeftCoinSecond = 253;
                        break;
                    case 3:
                        leftIndentSecondPrice = 210;
                        LeftCoinSecond = 258;
                        //coef = 0.7f;
                        break;
                    default:
                        leftIndentSecondPrice = 305;
                        LeftCoinSecond = 367;
                        break;
                }
                //Pen myPen = new Pen(Color.Black, 1);
                //e.Graphics.DrawRectangle(myPen, 10, 55, 135, 74);

                Graphics gr = e.Graphics;
                GraphicsState state = gr.Save();
                gr.ResetTransform();
                gr.ScaleTransform(coef, 1.0f);

                e.Graphics.DrawString(price[0], new Font("Arial Black", 40), Brushes.Black, leftIndentMainPrice, topIndentMainPrice);

                if (parPrice.Price < parPrice.PriceNormal)
                {
                    e.Graphics.DrawString(priceNormal[0], new Font("Arial Black", 16), Brushes.Black, leftIndentSecondPrice, topIndentSecondPrice);
                    e.Graphics.DrawString(priceNormal[1], new Font("Arial Black", 6), Brushes.Black, LeftCoinSecond, topIndentSecondPrice += 5);

                }
                else
                {
                    e.Graphics.DrawString(price[0], new Font("Arial Black", 16), Brushes.Black, leftIndentSecondPrice, topIndentSecondPrice);
                    e.Graphics.DrawString(price[1], new Font("Arial Black", 6), Brushes.Black, LeftCoinSecond, topIndentSecondPrice += 5);
                }
                e.Graphics.DrawString("грн", new Font("Arial", 6, FontStyle.Bold), Brushes.Black, LeftCoinSecond, topIndentSecondPrice += 7);
                e.Graphics.DrawString(parPrice.StrUnit, new Font("Arial", 6), Brushes.Black, LeftCoinSecond, topIndentSecondPrice += 7);
                gr.Restore(state);

                //e.Graphics.DrawString(price[0], new Font("Arial Black", 35), Brushes.Black, LeftBill, 35);
                e.Graphics.DrawString(price[1], new Font("Arial Black", 15), Brushes.Black, LeftCoinMain, topIndentMainPrice += 10);
                e.Graphics.DrawString("грн", new Font("Arial", 10, FontStyle.Bold), Brushes.Black, LeftCoinMain + 3, topIndentMainPrice += 23);
                e.Graphics.DrawString(parPrice.StrUnit, new Font("Arial", 11), Brushes.Black, LeftCoinMain + 3, topIndentMainPrice += 13);



                



                if (parPrice.Name.Length < LengthName)
                    Name1 = parPrice.Name;
                else
                {
                    int pos = parPrice.Name.Substring(0, LengthName).LastIndexOf(" ");
                    Name1 = parPrice.Name.Substring(0, pos);
                    Name2 = parPrice.Name.Substring(pos);
                }


                //назви
                e.Graphics.DrawString(" " + Name1, new Font("Arial", 11, FontStyle.Bold), Brushes.Black, 8, topIntentQR+=5);
                e.Graphics.DrawString(Name2, new Font("Arial", 11, FontStyle.Bold), Brushes.Black, 8, topIntentQR + 18);

            }


            // Межі
            e.Graphics.DrawLine(new Pen(Color.Black, 2), 280, 0, 280, 140);
            e.Graphics.DrawLine(new Pen(Color.Black, 2), 0, 140, 280, 140);





        }
        public void PrintLabelOpt(cPrice parPrice, PrintPageEventArgs e)
        {
            int LengthName = 28;
            string Name1, Name2 = "";
            if (parPrice.Name.Length < LengthName)
                Name1 = parPrice.Name;
            else
            {
                int pos = parPrice.Name.Substring(0, LengthName).LastIndexOf(" ");
                Name1 = parPrice.Name.Substring(0, pos);
                Name2 = parPrice.Name.Substring(pos);
                if (Name2.Length < LengthName)
                    Name2 = new string(' ', (LengthName - Name2.Length) / 2) + Name2;
            }
            Name1 = new string(' ', ((LengthName - Name1.Length) / 2)) + Name1;
            if (Name2.Length > LengthName + 3)
                Name2 = Name2.Substring(0, LengthName + 3);

            if (CurLogo != null)
                e.Graphics.DrawImage(CurLogo, 245, 5);
            e.Graphics.DrawString(DateTime.Now.ToString("dd/MM/yyyy H:mm"), new Font("Arial", 8), Brushes.Black, 215, 120); //Час

            //string BarCodePrice = parPrice.Code.ToString() + "-" + parPrice.Price.ToString() + (parPrice.PriceOpt == 0 ? "" : "-" + parPrice.PriceOpt.ToString());
            int strPrice = ((int)(parPrice.Price * 100M));
            var qrCodeData = qrGenerator.CreateQrCode($"{parPrice.Code}-{strPrice}", QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            e.Graphics.DrawImage(qrCode.GetGraphic(2), 250, 25);

            e.Graphics.DrawString(Name1, new Font("Arial", 11, FontStyle.Bold), Brushes.Black, 5, 0);
            e.Graphics.DrawString(Name2, new Font("Arial", 11, FontStyle.Bold), Brushes.Black, 5, 16);

            int LeftBill = 0, LeftCoin = 135, LeftBillTwo = 0, LeftCoinTwo = 135;
            float coef = 1;
            var price = parPrice.StrPrice.Split('.');
            var priceOpt = parPrice.StrPriceOpt.Split('.');
            //var price = "5.90".Split('.');
            //var priceOpt = "2.90".Split('.');

            //price[0] = "4293";
            switch (price[0].Count())
            {
                case 1:
                    LeftBill = 30;
                    LeftCoin = 75;
                    LeftBillTwo = 130;
                    LeftCoinTwo = 190;
                    if (parPrice.QuantityOpt == 0)
                    {
                        LeftBillTwo = 130;
                        LeftCoinTwo = 210;
                    }
                    break;
                case 2:
                    LeftBill = 10;
                    LeftCoin = 90;
                    LeftBillTwo = 120;
                    LeftCoinTwo = 220;
                    if (parPrice.QuantityOpt == 0)
                    {
                        LeftBillTwo = 90;
                        LeftCoinTwo = 230;
                    }
                    break;
                case 3:
                    LeftBill = 5;
                    LeftCoin = 90;
                    LeftBillTwo = 170;
                    LeftCoinTwo = 220;
                    if (parPrice.QuantityOpt == 0)
                    {
                        LeftBillTwo = 110;
                        LeftCoinTwo = 230;
                    }
                    coef = 0.7f;
                    break;
                default:
                    LeftBill = 0;
                    LeftCoin = 75;
                    LeftBillTwo = 230;
                    LeftCoinTwo = 210;
                    if (parPrice.QuantityOpt == 0)
                    {
                        LeftBillTwo = 150;
                        LeftCoinTwo = 230;
                    }
                    coef = 0.50f;
                    break;
            }

            Graphics gr = e.Graphics;
            GraphicsState state = gr.Save();

            if (parPrice.QuantityOpt != 0)
            {
                gr.ResetTransform();
                gr.ScaleTransform(coef, 1.0f);
                e.Graphics.DrawString(price[0], new Font("Arial Black", 40), Brushes.Black, LeftBill, 15);
                e.Graphics.DrawString(priceOpt[0], new Font("Arial Black", 50), Brushes.Black, LeftBillTwo, 40);
                gr.Restore(state);

                //e.Graphics.DrawString(price[0], new Font("Arial Black", 35), Brushes.Black, LeftBill, 35);
                e.Graphics.DrawString(price[1], new Font("Arial Black", 16), Brushes.Black, LeftCoin, 25);
                e.Graphics.DrawString("грн", new Font("Arial", 10, FontStyle.Bold), Brushes.Black, LeftCoin + 3, 47);
                e.Graphics.DrawString(parPrice.StrUnit, new Font("Arial", 10), Brushes.Black, LeftCoin + 3, 60);

                //ОПТОВА ЦІНА
                e.Graphics.DrawString(priceOpt[1], new Font("Arial Black", 18), Brushes.Black, LeftCoinTwo, 55);
                e.Graphics.DrawString("від", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, LeftCoinTwo + 3, 80);
                e.Graphics.DrawString(parPrice.QuantityOpt.ToString() + " шт.", new Font("Arial", 12), Brushes.Black, LeftCoinTwo + 3, 100);
            }
            else
            {
                gr.ResetTransform();
                gr.ScaleTransform(coef, 1.0f);
                e.Graphics.DrawString(price[0], new Font("Arial Black", 70), Brushes.Black, LeftBillTwo - 50, 10);
                gr.Restore(state);
                e.Graphics.DrawString(price[1], new Font("Arial Black", 26), Brushes.Black, LeftCoinTwo - 50, 35);
                e.Graphics.DrawString("грн", new Font("Arial", 16, FontStyle.Bold), Brushes.Black, LeftCoinTwo + 3 - 50, 70);
                e.Graphics.DrawString(parPrice.StrUnit, new Font("Arial", 16), Brushes.Black, LeftCoinTwo + 3 - 50, 90);
            }

            if (parPrice.BarCodes != null)
            {
                if (parPrice.BarCodes.Length > 27)
                    parPrice.BarCodes = parPrice.BarCodes.Substring(0, 27);
                e.Graphics.DrawString(parPrice.BarCodes, new Font("Arial", 7), Brushes.Black, 10, 120);
            }
            e.Graphics.DrawString(parPrice.Article.ToString(), new Font("Arial", 8, FontStyle.Bold), Brushes.Black, 270, 80);
            //e.Graphics.DrawLine(new Pen(Color.Black, 1), 0, 129, 150, 130);
            //e.Graphics.DrawString(parPrice.Article.ToString(), new Font("Arial", 8), Brushes.Black, 170, 120);
        }

    }

    public class cPrice
    {
        public int Code { get; set; }
        public string Name { get; set; }
        public int Article { get; set; }
        public string Unit { get; set; }
        public string StrUnit { get { return (Is100g && Unit.ToLower().Equals("кг") ? "100г" : ((Unit.Count() > 2) ? Unit.ToLower().Substring(0, 2) : Unit.ToLower())); } }
        public decimal Price { get; set; }
        public string StrPrice { get { return (Is100g && Unit.ToLower().Equals("кг") ? Price / 10m : Price).ToString("F", (IFormatProvider)CultureInfo.GetCultureInfo("en-US")); } }
        public decimal PriceOpt { get; set; }
        public string StrPriceOpt { get { return (Is100g && Unit.ToLower().Equals("кг") ? PriceOpt / 10m : PriceOpt).ToString("F", (IFormatProvider)CultureInfo.GetCultureInfo("en-US")); } }
        public int QuantityOpt { get; set; }
        public decimal Rest { get; set; }
        public int ActionType { get; set; }
        public decimal PriceBase { get; set; }
        public decimal MinPercent { get; set; }
        public decimal PriceMin { get; set; }
        public decimal PriceIndicative { get; set; }
        public string PromotionName { get; set; }
        public decimal PriceMain { get; set; }
        public decimal Sum { get; set; }
        public string BarCodes { get; set; }
        public bool Is100g { get; set; } = false;
        public bool IsOnlyCard { get; set; }
        public decimal PriceNormal { get; set; }
        public string StrPriceNormal { get { return (Is100g && Unit.ToLower().Equals("кг") ? PriceNormal / 10m : PriceNormal).ToString("F", (IFormatProvider)CultureInfo.GetCultureInfo("en-US")); } }
        public string PromotionBegin { get; set; }
        public string PromotionEnd { get; set; }
    }
}