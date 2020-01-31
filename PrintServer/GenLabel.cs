using SharedLib;
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

namespace PrintServer
{
    public class GenLabel
    {
        int current = 0;
        cPrice[] price;
        MSSQL db = new MSSQL();//("Server = SQLSRV2; Database=DW;Trusted_Connection=True;"
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        Image logo;// = Image.FromFile("D:\\Vopak.bmp");
        int CodeWarehouse = 1;
        string NamePrinter = "";
        string NamePrinterYelow;
        string NameDocument;

        public GenLabel()
        {
            string Warehouse = System.Configuration.ConfigurationManager.AppSettings["CodeWarehouse"];
            if (!string.IsNullOrEmpty(Warehouse))
                CodeWarehouse = Convert.ToInt32(Warehouse);

            string PathLogo = System.Configuration.ConfigurationManager.AppSettings["PathLogo"];
            if (!string.IsNullOrEmpty(PathLogo))
                logo = Image.FromFile(PathLogo);

            NamePrinterYelow = System.Configuration.ConfigurationManager.AppSettings["NamePrinterYelow"];
            NamePrinter = System.Configuration.ConfigurationManager.AppSettings["NamePrinter"];
        }

        public List<cPrice> GetCode(string parCodeWares)
        {
            if (string.IsNullOrEmpty(parCodeWares))
                return null;
            var L = new List<cPrice>();
            foreach (var el in parCodeWares.Split(','))
            {
                int CodeWares;
                if (int.TryParse(el, out CodeWares))
                {
                    var pr = GetPrice(CodeWares);
                    L.Add(pr);
                }
            }
            return L;           
        }
        public cPrice GetPrice(int? parCodeWares, int? parArticle = null)
        {
            var Sql = "select dbo.GetPrice(@CodeWarehouse ,@CodeWares,null,@Article,1)";

            var param = new { CodeWarehouse = this.CodeWarehouse, CodeWares = parCodeWares, Article = parArticle };
            var json = db.ExecuteScalar<object, string>(Sql, param);
            var price = JsonConvert.DeserializeObject<cPrice>(json);
            return price;
        }
        public void Print(IEnumerable<cPrice> parPrice, string parNameDocument = null)
        {
            current = 0;
            if (string.IsNullOrEmpty(NamePrinterYelow))
            {
                price = parPrice.ToArray();
                if (price.Count() > 0)
                    PrintServer(NamePrinter);
            }
            else
            {
                price = parPrice.Where(el => el.ActionType == 0).ToArray();
                if (price.Count() > 0)
                    PrintServer(NamePrinter);
                current = 0;
                price = parPrice.Where(el => el.ActionType != 0).ToArray();
                if(price.Count()>0)
                    PrintServer(NamePrinterYelow);
            }
            NameDocument = parNameDocument;
            if(string.IsNullOrEmpty(NameDocument))
            {

            }


        }
        public void PrintServer(string varNamePrinter)
        {
            // объект для печати
            PrintDocument printDocument = new PrintDocument();

            // обработчик события печати
            printDocument.PrintPage += PrintPageHandler;
            printDocument.DefaultPageSettings.PaperSize = new PaperSize("54 x 60 mm", 230, 130);

            // диалог настройки печати
            PrintDialog printDialog = new PrintDialog();

            // установка объекта печати для его настройки
            printDialog.Document = printDocument;

            //System.Drawing.Printing.PrinterSettings newSettings = new System.Drawing.Printing.PrinterSettings();
            printDialog.PrinterSettings.PrinterName = varNamePrinter;//newSettings.PrinterName;
            printDialog.Document.Print(); // печатаем
            if(!string.IsNullOrEmpty(NameDocument))//Друкуємо підсумок по документу.
            {
                printDocument.PrintPage -= PrintPageHandler;
                printDocument.PrintPage += PrintTotal;
                printDialog.Document.Print();
            }

        }


        public void PrintPreview()
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
        }

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

        void PrintTotal(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawString(NameDocument, new Font("Arial", 22), Brushes.Black, 0,20);
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

            string BarCodePrice = parPrice.Code.ToString() + "-" + parPrice.Price.ToString() + (parPrice.PriceOpt == 0 ? "" : "-" + parPrice.PriceOpt.ToString());


            e.Graphics.DrawImage(logo, 10, 0);
            e.Graphics.DrawString(DateTime.Now.ToString("dd/MM/yyyy H:mm"), new Font("Arial", 8), Brushes.Black, 120, 0); //Час

            var strPrice = parPrice.Price.ToString().Replace(",","");
            var qrCodeData = qrGenerator.CreateQrCode($"{parPrice.Code}-{strPrice}" , QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            e.Graphics.DrawImage(qrCode.GetGraphic(2), 165, 50);
            

            e.Graphics.DrawString(Name1, new Font("Arial", 11, FontStyle.Bold), Brushes.Black, 0, 16);
            e.Graphics.DrawString(Name2, new Font("Arial", 11, FontStyle.Bold), Brushes.Black, 0, 33);

            int LeftBill = 0, LeftCoin = 135;
            float coef=1;
            var price = parPrice.Price.ToString().Split(',');
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
            e.Graphics.DrawString("грн", new Font("Arial", 13,FontStyle.Bold), Brushes.Black, LeftCoin+3, 75);
            if (parPrice.Unit.Count() > 2)
                parPrice.Unit = parPrice.Unit.ToLower().Substring(0, 2);
            
            e.Graphics.DrawString(parPrice.Unit.ToLower().Substring(0,2), new Font("Arial", 14), Brushes.Black, LeftCoin + 3, 90);
            if (parPrice.BarCodes != null)
            {
                if (parPrice.BarCodes.Length > 27)
                    parPrice.BarCodes = parPrice.BarCodes.Substring(0, 27);
                e.Graphics.DrawString(parPrice.BarCodes, new Font("Arial", 7), Brushes.Black, 10, 120);
            }
            e.Graphics.DrawString(parPrice.Article.ToString(), new Font("Arial", 8,FontStyle.Bold), Brushes.Black, 170, 110);
            e.Graphics.DrawLine(new Pen(Color.Black, 1), 0, 133, 150, 133);
            //e.Graphics.DrawString(parPrice.Article.ToString(), new Font("Arial", 8), Brushes.Black, 170, 120);
        }

    }

    public class cPrice
    {
        public int Code { get; set; }
        public string Name { get; set; }
        public int Article { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public decimal PriceOpt { get; set; }
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

    }
}




