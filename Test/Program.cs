using ModelMID;

using SharedLib;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

using IronBarCode;
using System.IO.Packaging;
using System.Reflection;
//using System.Printing;
//using Exellio;


namespace Test
{

    class Program
    {
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("Start");
            var c = new Config("appsettings.json");// Конфігурація Програми(Шляхів до БД тощо)
                                                   //var sort = new SortImg();
                                                   // sort.SortPhoto(); // cортування фото



            var a = new GetGoodUrl();

            await a.LoadListex();
            return;

            System.Media.SoundPlayer player = new System.Media.SoundPlayer();
             player.SoundLocation = @"D:\MID\Sound\uk\WaitForAdministrator.wav";
                player.Load();
                player.Play();
            Thread.Sleep(2000);
                player.Stop();

            Thread.Sleep(1000000);
            return;
          
            Console.WriteLine("Sleep");

            Thread.Sleep(10000000);
        }

        static string ParserQRCode(string QRCode)
        {
            //string QRCode = "https://t.gov.ua/ABST773366/0035184264";
            string Res = null;
            if (QRCode.Contains("t.gov.ua"))
            {
                Res = QRCode.Substring(QRCode.IndexOf("t.gov.ua") + 9);
                Res = Res.Substring(0, Res.Length - 11);
            }
            Console.WriteLine(Res);
            return Res;
        }

        


        
  static void CreateBarCode()
        {


            string dataDir = @"d:\temp\BarCode\";
            string sql = @"SELECT w.code_wares as CodeWares ,w.name_wares as NameWares,b.bar_code as BarCode
    FROM dbo.Wares w
     JOIN dbo.barcode b ON w.code_wares = b.code_wares
  WHERE w.type_wares = 2
  --AND substring(w.name_wares,1,4)<> 'Пиво'
  AND w.name_group IN('Цигарки','Сигари')
  AND w.is_old = 0
  ORDER BY w.name_wares";
            var MsSQL = new WDB_MsSql();
            var W = MsSQL.db.Execute<ReceiptWares>(sql);
            // Instantiate barcode object and set differnt barcode properties
            foreach (var el in W)
            {

                var bb = BarcodeWriter.CreateBarcode(el.BarCode, el.BarCode.Length == 13 ? BarcodeWriterEncoding.EAN13 : BarcodeWriterEncoding.Code128, 250, 100);

                bb.SaveAsJpeg(dataDir + el.NameWares.Replace('\\', ' ').Replace('/', ' ').Replace("\"", "'").Replace("*", "x") + " " + el.BarCode + ".jpg");
                // BarcodeGenerator generator = new BarcodeGenerator(el.BarCode.Length==13? EncodeTypes.EAN13: EncodeTypes.Code128, el.BarCode);
                // generator.Parameters.Barcode.XDimension.Millimeters = 1f;

                // Save the image to your system and set its image format to Jpeg
                // generator.Save(dataDir + el.NameWares.Replace('\\',' ' ).Replace('/',' '). Replace("\"","'")+" "+ el.BarCode+".jpg", BarCodeImageFormat.Jpeg);
            }

        }
        
     
        public static void all_bag()
        {
            DateTime pDT = new DateTime(2020, 9, 1);

            List<int> Weight = new List<int>();

            while (pDT < DateTime.Now.Date)
            {
                var r = LoadOwnBag(pDT);
                pDT = pDT.AddDays(1);
                Weight.AddRange(r);
            }

            var singleString = string.Join(",", Weight.OrderBy(r => r).ToArray());
            Console.WriteLine(singleString);
        }

        static IEnumerable<int> LoadOwnBag(DateTime parDT)
        {
            try
            {
                var ldb = new WDB_SQLite(parDT);

                var dbMs = new MSSQL();

                var SqlSelect = "select PRODUCT_WEIGHT from RECEIPT_EVENT where EVENT_TYPE = 9"
                    ;
                Console.WriteLine("Start OwnBag");
                var r = ldb.db.Execute<int>(SqlSelect);
                return r;
            }
            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "LoadWeightKasa2=> " + ex.Message });
            }
            return null;
        }
        
    }


    public class TestReceipt
    {
        public decimal Percent_Discount { get; set; }
        public string Bar_Code { get; set; }
        public string Number { get; set; }
        public DateTime Date_Time { get; set; }
        public int Code_Wares { get; set; }
        public int Code_Unit { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public decimal Sum { get; set; }
        public decimal Disc_perc_manual { get; set; }
        public decimal Disc_Perc_Auto { get; set; }
        public int Is_Promotion { get; set; }
        public string Comment { get; set; }
        public string Type_Promotion { get; set; }
        public string BarCode2Category { get; set; }

    }

}
