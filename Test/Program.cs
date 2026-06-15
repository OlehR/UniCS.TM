using Dapper;
using Front.Equipments;
using Front.Equipments.Implementation;
using IronBarCode;
using Microsoft.Extensions.Configuration;
using ModelMID;
using Newtonsoft.Json;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Front.Equipments.Implementation;
//using System.Printing;
//using Exellio;

namespace Test
{
    class ResNumber : IdReceipt
    {
        public string FiscalNumber { get; set; }
    }

    class ResReceipt : IdReceiptWares
    {
        public string ExciseStamp { get; set; }
        public string TextReceipt { get; set; }
    }

    class Program
    {        
        static async Task Main(string[] args)
        {
            string PathDB= "D:\\MID_KCO\\DB_Exellio";
            AddStamp(PathDB);
            AddQR(PathDB);
            Print(PathDB);
        }
        static void Print(string pPath = "D:\\MID_KCO\\DB_Exellio")
        {
            var CurDir = AppDomain.CurrentDomain.BaseDirectory;
            var AppConfiguration = new ConfigurationBuilder()
                .SetBasePath(CurDir)
                .AddJsonFile("appsettings.json").Build();

            var Printer = new Printer_Sam4sGcube102(new() {Model= eModelEquipment.Printer_Sam4sGcube102,IsСritical=false,State=eStateEquipment.On,Name= "Print" }, AppConfiguration);

            var Dir = Directory.GetDirectories(pPath).OrderBy(d => d);
            foreach (var d in Dir)
            {
                foreach (var f in Directory.GetFiles(d).OrderBy(f => f))
                {
                    var connectionString = new SQLiteConnectionStringBuilder("Data Source=" + f + ";Version=3;") //$"d:\\MID_KCO\\Ber1_2\\Rc_{pIdWorkPlace}_202411{i:D2}.db"
                    {
                        DefaultIsolationLevel = IsolationLevel.Serializable
                    }.ToString();

                    var Con = new SQLiteConnection(connectionString);
                    Con.Open();
                    try
                    {
                        Con.Execute("alter table Log_RRO add TextReceiptQR TEXT");
                    }
                    catch { }
                    var ResN = Con.Query<ResReceipt>($@"select Id_Workplace as IdWorkplace, Code_Period as CodePeriod, Code_Receipt as CodeReceipt, TextReceiptQR as TextReceipt 
from  Log_RRO lr where TextReceiptQR is not null");
                    foreach (var el in ResN)
                    {
                        Printer.Print(el.TextReceipt.Split("\r\n"));
                    }
                }
            }
        }

        static void AddStamp(string pPath = "D:\\MID_KCO\\DB_Exellio")
        {
            var Dir = Directory.GetDirectories(pPath);
            foreach (var d in Dir)
            {

                int CodeReceipt = 0;
                string TextReceipt = null;
                //var ConPG = new NpgsqlConnection(connectionString: "Server=10.1.0.33;Port=5432;User Id=dwreader;Password=DW_Reader;Database=DW;Timeout=300;CommandTimeout=300;Pooling=false");
                //ConPG.Open();

                var ConMid = new SQLiteConnection("Data Source=D:\\MID\\DB\\202605\\MID_9_20260529.db;Version=3;");
                ConMid.Open();

                foreach (var f in Directory.GetFiles(d))

                //for (int i = 1; i <= 30; i++)
                {
                    var connectionString = new SQLiteConnectionStringBuilder("Data Source=" + f + ";Version=3;") //$"d:\\MID_KCO\\Ber1_2\\Rc_{pIdWorkPlace}_202411{i:D2}.db"
                    {
                        DefaultIsolationLevel = IsolationLevel.Serializable
                    }.ToString();

                    var Con = new SQLiteConnection(connectionString);
                    Con.Open();
                    try
                    {
                        Con.Execute("alter table Log_RRO add TextReceipt TEXT");
                    }
                    catch { }                    

                    var ResN = Con.Query<ResReceipt>($@"select wr.Id_Workplace as IdWorkplace, wr.Code_Period as CodePeriod,  wr.Code_Receipt as CodeReceipt, wr.Code_Wares as CodeWares, wr.Excise_Stamp as ExciseStamp, Text_Receipt as TextReceipt 
from WARES_RECEIPT  wr
join  Log_RRO lr  on   wr.Code_Receipt= lr.Code_Receipt and Type_RRO=""RRO""
where wr.QUANTITY =1 and Excise_Stamp is not null and  lr.Id_Workplace_pay=  lr.Id_Workplace");
                    foreach (var el in ResN)
                    {
                        if (el.CodeReceipt != CodeReceipt) TextReceipt = el.TextReceipt;
                        if (!string.IsNullOrEmpty(TextReceipt))
                        {
                            var BarCodes = ConMid.Query<string>($"select BAR_CODE from BAR_CODE where CODE_WARES={el.CodeWares}");
                            foreach (var bc in BarCodes)
                            {
                                int r = TextReceipt.IndexOf(bc);
                                if (r > 0)
                                {
                                    r = TextReceipt.IndexOf("\n", r);
                                    if (r > 0)
                                    {
                                        string ExciseStamp = el.ExciseStamp.Replace(",None", "").Replace("None", "");
                                        if (ExciseStamp.Length >= 10)
                                        {
                                            TextReceipt = TextReceipt.Insert(r + 1, $"{ExciseStamp}\r\n");
                                            File.WriteAllText("d:/receipt.txt", TextReceipt);
                                            Con.Execute(@"update Log_RRO  set  TextReceipt = @TextReceipt where  Code_Period = @CodePeriod and Code_Receipt = @CodeReceipt and Id_Workplace = @IdWorkplace", new { el.IdWorkplace, el.CodePeriod, el.CodeReceipt, TextReceipt });
                                            Console.WriteLine($"{f} {el.IdWorkplace} {el.CodePeriod} {el.CodeReceipt} {el.CodeWares} {el.ExciseStamp}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Con.Close();
                }                
            }
        }

        static void AddQR(string pPath = "D:\\MID_KCO\\DB_Exellio")
        {
            var Dir = Directory.GetDirectories(pPath);
            foreach (var d in Dir)
            {
                foreach (var f in Directory.GetFiles(d))
                {
                    var connectionString = new SQLiteConnectionStringBuilder("Data Source=" + f + ";Version=3;") //$"d:\\MID_KCO\\Ber1_2\\Rc_{pIdWorkPlace}_202411{i:D2}.db"
                    {
                        DefaultIsolationLevel = IsolationLevel.Serializable
                    }.ToString();

                    var Con = new SQLiteConnection(connectionString);
                    Con.Open();
                    try
                    {
                        Con.Execute("alter table Log_RRO add TextReceiptQR TEXT");
                    }
                    catch { }
                    var ResN = Con.Query<ResReceipt>($@"select Id_Workplace as IdWorkplace, Code_Period as CodePeriod, Code_Receipt as CodeReceipt, TextReceipt as TextReceipt 
from  Log_RRO lr where TextReceipt is not null");
                    foreach (var el in ResN)
                    {
                        string OldQR = "", QR = null;
                        int sum = 0;
                        string FN = null, Time = null, FR = null;
                        var R = el.TextReceipt.Split("\r\n");
                        foreach (var l in R)
                        {
                            if (l.StartsWith("С У М А     "))
                            {
                                string Sum = l.Substring(8, 24).Replace(",", "").Replace(" ", "");
                                sum = Sum.ToInt();
                                break;
                            }
                        }
                        if (sum > 0)
                        {
                            int IsEnd = 0;
                            int i = 0;
                            foreach (var l in R)
                            {
                                if (l.StartsWith("      Ф I С К А Л Ь Н И Й   Ч Е К"))
                                {
                                    IsEnd = i;
                                    break;
                                }
                                i++;
                            }
                            if (IsEnd > 0)
                            {
                                FR = R[IsEnd - 4].Substring(0, 7);
                                FR = $"{FR.ToInt():D10}";
                                Time = R[IsEnd - 4][^19..];
                                string format = "dd-MM-yyyy HH:mm:ss";
                                DateTime dt = DateTime.ParseExact(Time, format, CultureInfo.InvariantCulture);
                                Time = dt.ToString("ddMMyyHHmm");

                                FN = R[IsEnd - 3][^10..];
                                OldQR = R[IsEnd - 2].Trim() + R[IsEnd - 1].Trim();
                                QR = $"QR=>{OldQR};{sum:D10};{Time};{FN};{FR}";

                                var ZZ = R.ToList();
                                ZZ.Insert(IsEnd - 2, QR);
                                string Receipt = string.Join("\r\n", ZZ);

                                Con.Execute(@"update Log_RRO  set  TextReceiptQR = @TextReceipt where  Code_Period = @CodePeriod and Code_Receipt = @CodeReceipt and Id_Workplace = @IdWorkplace", new { el.IdWorkplace, el.CodePeriod, el.CodeReceipt, TextReceipt = Receipt });
                                Console.WriteLine($"{f} {el.IdWorkplace} {el.CodePeriod} {el.CodeReceipt} QR=>{QR}");
                            }
                        }
                    }


                }
            }
        }

        /*
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
        }*/

        /*
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
        }*/

    }
    /*

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

    }*/

}
