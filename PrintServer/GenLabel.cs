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

namespace PrintServer
{
    public class GenLabel
    {
        int current = 0;
        cPrice[] price;
        MSSQL db = new MSSQL();//("Server = SQLSRV2; Database=DW;Trusted_Connection=True;"
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        Image logo;// = Image.FromFile("D:\\Vopak.bmp");
        Image logo2;
        Image CurLogo;
        //int CodeWarehouse = 1;
        //string NamePrinter = "";
        //string NamePrinterYelow;
        string NameDocument;

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

        public void Print(IEnumerable<cPrice> parPrice, string parNamePrinter, string parNamePrinterYelow, string pNameDocument = null, bool isMainLogo = true, bool isShort = true)
        {
            CurLogo = (isMainLogo || logo2 == null ? logo : logo2);
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
                    PrintServer(parNamePrinterYelow, pNameDocument, isShort);
            }

        }

        public void PrintServer(string pNamePrinter, string pNameDoc = "Label", bool isShort = true)
        {
            // объект для печати
            PrintDocument printDocument = new PrintDocument();

            // обработчик события печати

            if (isShort)//звичайний цінник
            {
                printDocument.PrintPage += PrintPageHandler;
                printDocument.DocumentName = $"{pNameDoc}_{price.Count()}";
                printDocument.DefaultPageSettings.PaperSize = new PaperSize("54 x 30 mm", 230, 130);
            }
            else //цінник для опту
            {
                printDocument.PrintPage += PrintPageHandlerOpt;
                printDocument.DocumentName = $"{pNameDoc}_{price.Count()}";
                printDocument.DefaultPageSettings.PaperSize = new PaperSize("80 x 30 mm", 340, 130);
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
                if (isShort)
                    printDocument.PrintPage -= PrintPageHandler;
                else
                    printDocument.PrintPage -= PrintPageHandlerOpt;
                printDocument.PrintPage += PrintTotal;
                printDialog.Document.Print();
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
                e.Graphics.DrawImage(CurLogo, 265, 5);
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
                    LeftBill = 10;
                    LeftCoin = 55;
                    LeftBillTwo = 130;
                    LeftCoinTwo = 190;
                    break;
                case 2:
                    LeftBill = 10;
                    LeftCoin = 90;
                    LeftBillTwo = 120;
                    LeftCoinTwo = 220;
                    break;
                case 3:
                    LeftBill = 5;
                    LeftCoin = 90;
                    LeftBillTwo = 170;
                    LeftCoinTwo = 220;
                    if (parPrice.QuantityOpt == 0)
                    {
                        LeftBillTwo = 140;
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
                        LeftBillTwo = 180;
                    }
                    coef = 0.50f;
                    break;
            }

            Graphics gr = e.Graphics;
            GraphicsState state = gr.Save();

            Console.WriteLine(parPrice.PriceOpt);
            if (parPrice.QuantityOpt != 0)
            {
                gr.ResetTransform();
                gr.ScaleTransform(coef, 1.0f);
                e.Graphics.DrawString(price[0], new Font("Arial Black", 40), Brushes.Black, LeftBill, 15);
                e.Graphics.DrawString(priceOpt[0], new Font("Arial Black", 50), Brushes.Black, LeftBillTwo, 20);
                gr.Restore(state);

                //e.Graphics.DrawString(price[0], new Font("Arial Black", 35), Brushes.Black, LeftBill, 35);
                e.Graphics.DrawString(price[1], new Font("Arial Black", 16), Brushes.Black, LeftCoin, 25);
                e.Graphics.DrawString("грн", new Font("Arial", 10, FontStyle.Bold), Brushes.Black, LeftCoin + 3, 47);
                e.Graphics.DrawString(parPrice.StrUnit, new Font("Arial", 10), Brushes.Black, LeftCoin + 3, 60);

                //ОПТОВА ЦІНА
                e.Graphics.DrawString(priceOpt[1], new Font("Arial Black", 18), Brushes.Black, LeftCoinTwo, 35);
                e.Graphics.DrawString("від", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, LeftCoinTwo + 3, 60);
                e.Graphics.DrawString(parPrice.QuantityOpt.ToString() + " шт.", new Font("Arial", 12), Brushes.Black, LeftCoinTwo + 3, 80);
            }
            else
            {
                gr.ResetTransform();
                gr.ScaleTransform(coef, 1.0f);
                e.Graphics.DrawString(price[0], new Font("Arial Black", 50), Brushes.Black, LeftBillTwo - 50, 20);
                gr.Restore(state);
                e.Graphics.DrawString(price[1], new Font("Arial Black", 18), Brushes.Black, LeftCoinTwo - 50, 35);
                e.Graphics.DrawString("грн", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, LeftCoinTwo + 3 - 50, 60);
                e.Graphics.DrawString(parPrice.StrUnit, new Font("Arial", 12), Brushes.Black, LeftCoinTwo + 3 - 50, 80);
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
    }
}