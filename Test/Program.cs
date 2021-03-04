using ModelMID;
using ModernIntegration;
using SharedLib;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using ModernIntegration.Model;
using ModernIntegration.Models;
using System.Text.RegularExpressions;
using ModernIntegration.ViewModels;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

using IronBarCode;
//using System.Printing;

namespace Test
{

    class Program
    {
        static async Task Main(string[] args)
        {

            //LocalPrintServer ps = new LocalPrintServer();
            //  var pq = ps.GetPrintQueues(); 
            //LoadWeightKasa();            return;
            // var R = await GetInfoBarcode("4823000920439");
            //var l = new GetGoodUrl();
            //l.LoadWeightURLAsync();
            //Thread.Sleep(1000000000);

            Console.WriteLine("Start");
            var c = new Config("appsettings.json");// Конфігурація Програми(Шляхів до БД тощо)

            await CreateDataBaseAsync(true);


               TestReceiptAsync();

            //CreateBarCode();
            //TestKit();
            //all_bag();
            //LoadReceiptJson();
            //new ApiPSU().Bl.ds.LoadWeightKasa2Period();

            //new ApiPSU().Bl.ds.LoadWeightKasa2Period(new DateTime(2020, 8, 1),1); 


            Console.WriteLine("Sleep");

            Thread.Sleep(10000000);

        }

        static async Task CreateDataBaseAsync(bool isFull = true)
        {
            //var bl = new BL();            bl.SyncDataAsync(isFull);
            var api = new ApiPSU();
           
            await api.RequestSyncInfo(isFull);
        }



        static void TestKit()
        {
            var a = new string[] //{ "4820083903205", "4820083900990", "4820083900617", "4820083900198", "4820083900037" };
              { "2509100900008","4779046030109" };
            var api = new ApiPSU();
            var TerminalId = Guid.NewGuid();
            foreach (var el in a)
                api.AddProductByBarCode(TerminalId, el, 2);
        }
        static async Task TestReceiptAsync()
        {
            //var TerminalId = Guid.Parse("1bb89aa9-dbdf-4eb0-b7a2-094665c3fdd0");//14
            var TerminalId = Guid.Parse("27aaa6d3-8824-475d-a7d4-3269472ba950");//19
            var ProductId = Guid.Parse("00000000-abcd-0000-0019-000000159475");
            var FastGroup = Guid.Parse("12345670-0987-0000-0000-000000009000");
            var ReceiptId = Guid.Parse("00000062-ffff-2020-0326-000000000008");
            var api = new ApiPSU();
            ProductViewModel sd;

            //api.Bl.ds.SendReceiptTo1C(new IdReceipt() { CodePeriod = 20201119, IdWorkplace = 72, CodeReceipt = 8}); return;


          //  sd = api.AddProductByBarCode(TerminalId, "5900857007793", 1);

         //   sd = api.AddProductByBarCode(TerminalId, "8710671155382", 1);

            //return;

           //var ccc = api.GetCustomerByPhone(TerminalId,"0666672818");

            sd = api.AddProductByBarCode(TerminalId, "2211794601924", 1);
            var QR = await api.Bl.ds.GetQrCoffe(null, 7, 2);
            Console.WriteLine($"QR=>{QR}");
            //return;
            api.Bl.AddWaresCode(159478, 19, 2);
            api.Bl.AddWaresCode(159474, 19, 3);
            api.Bl.AddWaresCode(162571, 19, 1);
            api.Bl.AddWaresCode(177830, 19, 1);
            Thread.Sleep(4000);

            var ssssqq = api.GetQR(TerminalId);
            

            return;
            sd = api.AddProductByProductId(TerminalId, sd.Id, 1, 45.71M);

            sd = api.AddProductByBarCode(TerminalId, "4820000536202", 1);

            var rrr =api.AddProductByProductId(TerminalId, ProductId, 1);

            var clff = api.GetCustomerByBarCode(TerminalId, "8810005077387"); //Моя карточка 7%

            sd = api.AddProductByBarCode(TerminalId, "2201652301489", 1); //Морква 
            //Thread.Sleep(2000);
            sd = api.AddProductByBarCode(TerminalId, "7773002160043", 1); //товар 2 кат
            sd = api.AddProductByBarCode(TerminalId, "4823086109988", 1);

            sd = api.AddProductByBarCode(TerminalId, "2201651902226", 1); //
         
         //   sd = api.AddProductByBarCode(TerminalId, "7775006620509", 1); //товар 2 кат*/
            Thread.Sleep(4000);

           
 //           var clf = api.GetCustomerByBarCode(TerminalId, "8810005077387"); //Моя карточка 7%


            return;

            // sd = api.AddProductByBarCode(TerminalId, "8887290101608", 1);  return;

            //var Recvm = api.GetReceiptViewModel(new IdReceipt { CodePeriod = 20200915, IdWorkplace = 72, CodeReceipt = 29 }); return;

            //sd = api.AddProductByBarCode(TerminalId, "7613035603257", 1);
            //Thread.Sleep(2000);
            //return;
            //var c = api.GetCustomerByPhone(TerminalId,"0503729543");
            //var c = api.GetCustomerByBarCode(TerminalId, "8810005077479");
            //var ddd=api.GetReceiptByNumber(TerminalId, "55");


            

            //api.Bl.ds.SendReceiptTo1C(new IdReceipt() { CodePeriod = 20200920, IdWorkplace = 72, CodeReceipt = 21 }); return;

            api.Bl.GetClientByBarCode(new IdReceipt() { CodePeriod = 20201005, IdWorkplace = 68, CodeReceipt = 27}, "8810005077387"); return;

            // api.Bl.ds.SendReceiptTo1C(new IdReceipt() { CodePeriod = 20200730, IdWorkplace = 68, CodeReceipt = 8}); return;

            //for(int i = 8720; i<= 8720; i++)
            //api.Bl.ds.SendReceiptTo1C(new IdReceipt() { CodePeriod = 20200706, IdWorkplace = 62, CodeReceipt = i }); 
            //return;

            //api.Bl.ds.SendReceiptTo1C(new IdReceipt() { CodePeriod = 20200706, IdWorkplace = 62, CodeReceipt = i }); 

            //sd = api.AddProductByBarCode(TerminalId, "7613036939874", 1);

            sd = api.AddProductByBarCode(TerminalId, "4820000534642", 1); 
            sd= api.AddProductByProductId(TerminalId, sd.Id, 4, 10.00M);
            sd = api.AddProductByBarCode(TerminalId, "4820000534741", 1);
            sd = api.AddProductByProductId(TerminalId, sd.Id, 3, 20.00M);

           var rr = api.GetRecieptByTerminalId(TerminalId, true);
            return;

            sd = api.AddProductByBarCode(TerminalId, "4823021808778", 1); 
                                                                         
            sd = api.AddProductByBarCode(TerminalId, "1110716760019", 1); //хліб житній
            //api.SetWeight(TerminalId, sd.Id, 321);           return;
            sd = api.AddProductByBarCode(TerminalId, "7773002160043", 1); //товар 2 кат
            //Thread.Sleep(2000);
            sd = api.AddProductByBarCode(TerminalId, "1110716760019", 1); //хліб житній
           // Thread.Sleep(2000);
            sd = api.AddProductByBarCode(TerminalId, "7773002160029", 1); //товар 2 кат

            var cl = api.GetCustomerByBarCode(TerminalId, "8810005077387"); //Моя карточка 7%

            sd = api.AddProductByBarCode(TerminalId, "2201652301489", 1); //Морква
            //Thread.Sleep(2000);
            sd = api.AddProductByBarCode(TerminalId, "7773002160043", 1); //товар 2 кат

            sd = api.AddProductByBarCode(TerminalId, "2201651902226", 1); //
            Thread.Sleep(2000);
            //sd = api.AddProductByBarCode(TerminalId, "7775006620509", 1); //товар 2 кат*/
            Thread.Sleep(2000);
           // api.AddFiscalNumber(TerminalId, "1234567");


            //var rrr = api.GetReceipts(DateTime.Parse("2020-06-24T00:00:00"), DateTime.Parse("2020-06-24T23:59:59.999"), TerminalId);
            //            var n = rrr.Count();
            //7667,7676,7677

            //sd = api.AddProductByProductId(TerminalId, ProductId, 1); return;

            sd = api.AddProductByBarCode(TerminalId, "4820207930056", 1); //
            //sd = api.AddProductByBarCode(TerminalId, "2201651902226", 1); //
            //sd = api.AddProductByBarCode(Guid.Parse("5c1413f5-66fe-4c2e-9c4c-c354c79952ea"), "7622210653031", 2); //
            
            var RId = api.GetCurrentReceiptByTerminalId(TerminalId).ReceiptId;
            var Rec = api.GetReciept(RId);

            return;

            /*var rrr=api.GetReceiptViewModel(new IdReceipt {CodePeriod=20200504,IdWorkplace=62,CodeReceipt=12} );

            foreach (var el in rrr.ReceiptItems)
                Console.WriteLine($"{el.ProductName.Substring(0,7)} PP=> {el.ProductPrice } \t Discount=> { el.Discount} \t{el.ProductPrice*el.ProductQuantity*(el.ProductWeightType==ModernIntegration.Enums.ProductWeightType.ByWeight?1000:1 )- el.Discount} "); //FullPrice=>  {el.FullPrice}   TotalPrice=>{el.TotalPrice} 
            */
            //var dddd=api.GetAllCategories(TerminalId);
            
            
            //api.Bl.ds.LoadWeightKasa(new DateTime(2020,02,17));return;
            //api.Bl.SendOldReceipt(); return;
            var r2rr=api.GetBags();
            //api.Bl.SendAllReceipt();return;

            sd = api.AddProductByBarCode(TerminalId, "4823086109988", 1); // 1+1 Пельмені "Мішутка" Філейні 600г /Три ведмеді/

            var recipt = api.GetProductsByName(TerminalId,"",0,false, FastGroup);
            var ddd1  = api.GetProductsByName(TerminalId, "", 1, false, FastGroup);
            var ddd2 = api.GetProductsByName(TerminalId, "пом", 0, false, FastGroup);

            sd = api.AddProductByBarCode(TerminalId, "2206140307779", 1); //

          

            return;

            //var rrr= api.GetReceipts(DateTime.Parse("2020-02-03T00:00:00"), DateTime.Parse("2020-02-03T23:59:59.999"), TerminalId);

            //Thread.Sleep(1000000);
            //var reseipt = api.GetReceipts(DateTime.Now.Date, DateTime.Now.Date);

            //var cl = api.GetCustomerByBarCode(TerminalId, "8810005077387"); //Моя карточка 7%


            // var rrrr = api.GetNoFinishReceipt(TerminalId);
            //var aa=api.Bl.db.GetConfig<DateTime>("Load_Full__");
            //sd =api.AddProductByBarCode(TerminalId, "4820197006205", 1);
            // sd = api.AddProductByBarCode(TerminalId, "4820198091002", 1);

            //Console.WriteLine("var cl = api.AddProductByBarCode(TerminalId, \"4820048481960\");");
            //Console.WriteLine(sd.Name);
            //          var cl = api.GetCustomerByBarCode(TerminalId, "4820220980229");

            //api.RequestSyncInfo(false);
            //Thread.Sleep(100000);


            sd = api.AddProductByBarCode(TerminalId, "30886", 1);

            var startTime = System.Diagnostics.Stopwatch.StartNew();

            //sd = api.AddProductByBarCode(TerminalId, "4823086109988", 1); // 1+1 Пельмені "Мішутка" Філейні 600г /Три ведмеді/
            //sd = api.AddProductByBarCode(TerminalId, "2201652300489", 1); //Морква
            //Thread.Sleep(1000);

            return;

            sd = api.AddProductByBarCode(TerminalId, "4823000916524", 2); //АРТЕК 
            sd = api.AddProductByBarCode(TerminalId, "22970558", 0);
            sd = api.AddProductByBarCode(TerminalId, "7622300813437", 1);//Барн
            sd = api.AddProductByProductId(TerminalId, ProductId, 1);

          sd = api.AddProductByBarCode(TerminalId, "2201652300489", 1); //Морква
            //Thread.Sleep(1000);
            sd = api.AddProductByBarCode(TerminalId, "2201652300229", 1); //Морква
                                                                          //Thread.Sleep(1000);

            var rssss=api.GetRecieptByTerminalId(TerminalId);
            
            //Thread.Sleep(1000);
            api.ChangeQuantity(TerminalId, sd.Id, 3);
            //Thread.Sleep(1000);
            api.ChangeQuantity(TerminalId, sd.Id, 4);
            //Thread.Sleep(1000);
            api.ChangeQuantity(TerminalId, sd.Id, 3);
            // Thread.Sleep(1000);
            api.ChangeQuantity(TerminalId, sd.Id, 2);
            //Thread.Sleep(100000);


            //            startTime.Stop();
            //            Console.WriteLine( startTime.Elapsed);
            //            startTime.Restart();

            sd = api.AddProductByBarCode(TerminalId, "1111622770010", 1);

            startTime.Stop();
            Console.WriteLine(startTime.Elapsed);
            startTime.Restart();

            sd = api.AddProductByBarCode(TerminalId, "7622300813437", 2);//Барн

            //startTime.Stop();
            Console.WriteLine(startTime.Elapsed);
            //startTime.Restart();

            //            sd = api.AddProductByBarCode( TerminalId, "2201652300489",1); //Морква
            /*sd = api.AddProductByBarCode(TerminalId, "1110867180018", 1); //Хліб
            sd = api.AddProductByBarCode(TerminalId, "40804927", 1);
            sd = api.AddProductByBarCode(TerminalId, "1110011760018", 1); //КІВІ ВАГОВІ 2 кат*/
            sd = api.AddProductByBarCode(TerminalId, "2201652300489", 1); //Морква
            sd = api.AddProductByBarCode(TerminalId, "7775006620509", 1); //товар 2 кат
            //Thread.Sleep(1000);
            /*sd =api.AddProductByBarCode( TerminalId, "5903154545623", 1); //Суміш овочева "Семикомпонентна" 400г /Рудь/ акція 1+1
            sd = api.AddProductByBarCode(TerminalId, "7622300813437", 5);//Барн*/
            sd = api.AddProductByBarCode(TerminalId, "4823097403457", 5);//Майонез "Провансаль" 67% д/п 350г /Щедро/
            sd = api.AddProductByBarCode(TerminalId, "4823097405932", 7);//Кетчуп "Лагідний" д/п 250г /Щедро/*/
            api.ChangeQuantity(TerminalId, sd.Id, 0);


            //api.AddPayment(TerminalId, Pay);
            //api.ClearReceipt(TerminalId);
            //var rrrr = api.GetNoFinishReceipt(TerminalId);
        
            //var RId = api.GetCurrentReceiptByTerminalId(TerminalId).ReceiptId;

            

            var rr11 = api.GetProduct(TerminalId);
            var Pay = new ReceiptPayment[] {
                new ReceiptPayment
            {
                Id = Guid.Parse("9e960928-1070-457d-aec3-14672adf3e9b"),
                ReceiptId = Guid.Parse("00000062-ffff-2020-0116-000000000007"),
                PaymentType = ModernIntegration.Enums.PaymentType.Card,
                PayIn = 25.9M,
                PayOut = 0.0M,
                CardPan = "XXXXXXXXXXXX2520",
                IsPayOutSuccess = null,
                TransactionId = "2",
                TransactionCode = "039177601652",
                TransactionStatus = "Àâòîðèçàö³ÿ ç áàíêîì",
                PosAuthCode = null,
                PosTerminalId = null,
                CreatedAt = DateTime.Now
            } };


           
            api.AddPayment(TerminalId, Pay);
            var r = api.AddFiscalNumber(TerminalId, "TRRF-1234");

            var receipt = api.GetReciept(RId);

            var sz = JsonConvert.SerializeObject(receipt);
            var RefoundReceipt = JsonConvert.DeserializeObject<RefundReceiptViewModel>(sz);
            RefoundReceipt.IdPrimary = RefoundReceipt.Id;
      
            var resRef=api.RefundReceipt(TerminalId, RefoundReceipt);


            //           api.SendReceipt(RId);
           // var Rec=api.GetReciept(RId);
            
            
            Console.WriteLine("End");


            //169316+169316 4823086109988 Пельмені "Мішутка" Філейні 600г /Три ведмеді/
            //156727+169583 4823097403457+4823097405932 Майонез "Провансаль" 67% д/п 350г /Щедро/  Кетчуп "Лагідний" д/п 250г /Щедро/

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

                var bb = BarcodeWriter.CreateBarcode(el.BarCode, el.BarCode.Length == 13 ? BarcodeWriterEncoding.EAN13 : BarcodeWriterEncoding.Code128,250,100);
            
                    bb.SaveAsJpeg(dataDir + el.NameWares.Replace('\\', ' ').Replace('/', ' ').Replace("\"", "'").Replace("*", "x") + " " + el.BarCode + ".jpg");
                // BarcodeGenerator generator = new BarcodeGenerator(el.BarCode.Length==13? EncodeTypes.EAN13: EncodeTypes.Code128, el.BarCode);
                // generator.Parameters.Barcode.XDimension.Millimeters = 1f;

                // Save the image to your system and set its image format to Jpeg
                // generator.Save(dataDir + el.NameWares.Replace('\\',' ' ).Replace('/',' '). Replace("\"","'")+" "+ el.BarCode+".jpg", BarCodeImageFormat.Jpeg);
            }

        }
        static void CreateReceipDay()
        {
            var SQLGetReceipt = @"SELECT top(500) ISNULL(td.PERCENT_DISCOUNT,0) AS PERCENT_DISCOUNT, dc.bar_code,  dr.number,dr.date_time
  ,w.Code_Wares
  ,dbo.GetCodeUnit(ud.code_unit)  AS Code_Unit
  , drw.amount
  ,drw.price
  ,[disc_perc] as disc_perc_manual
  ,drw.sum+drw.sum_bonus AS sum
  ,[disc_perc_auto]
  ,[is_promotion]
  ,dr.comment
  --,drw._Fld11310_RRRef
  ,CONVERT(nchar(32), _Fld17312RRef, 2) AS type_Promotion
  , sc._Description  AS BarCode2Category
 -- COUNT(*) 
  FROM dbo.V1C_doc_receipt dr 
  JOIN dbo.V1C_doc_receipt_wares drw ON dr._IDRRef = drw._IDRRef
  JOIN dbo.Wares w ON drw.nomen_RRef=w._IDRRef
  LEFT JOIN dbo.V1C_DIM_CARD dc ON dr.card_RRef = dc.Card_RRef
  LEFT JOIN dbo.V1C_DIM_TYPE_DISCOUNT TD ON TD.TYPE_DISCOUNT_RRef =DC.TYPE_DISCOUNT_RRef
  JOIN dbo.V1C_dim_addition_unit au ON drw.uom_RRef=au._IDRRef
  JOIN  dbo.V1C_DIM_UNIT_DIMENSION ud ON au.Unit_dimention_RRef=ud.UNIT_DIMENSION_RRef 
  LEFT JOIN   UTPPSU.dbo._Reference18060 sc ON drw.barcode_2 = sc._IDRRef
  WHERE dr._Date_Time BETWEEN CONVERT(DATE,DATEADD(DAY,0,DATEADD(YEAR,2000,GETDATE()))) AND CONVERT(DATE,DATEADD(DAY,1,DATEADD(YEAR,2000,GETDATE())))
  --AND ROUND(drw.amount*drw.price,2)<>drw.sum+drw.sum_bonus
--and is_promotion=1
  AND dr.warehouse_RRef= 0xB7A3001517DE370411DF7DD82E29F000 --
  --AND _Fld17312RRef=0xAF5E2CDABF65241E4EB3EC36EC1F11E2 --Комплект
  --AND _Fld17312RRef=0xA6F61431ECE9ED4646ECAA3A735174ED --По виду дисконтних карт
/*0x8CA05E08A127F853433EF4373AE9DC39 --Скидка на день рождения
0xA19CCECEDC498AF84560C115E6F7418A  --Количество одного товара в документе превысило
0xA6F61431ECE9ED4646ECAA3A735174ED  --По виду дисконтных карт
0xAD63C44DBEEA7E344A9E865F34168F14  --Вторая категория
0xAF5E2CDABF65241E4EB3EC36EC1F11E2  --Комплект*/

  --AND td.PERCENT_DISCOUNT<>[disc_perc_auto]
  --AND dr.number='К1300008702'
--  AND drw.sum_bonus>0
  ORDER BY dr._IDRRef";
            var TerminalId = Guid.Parse("abb75469-0f34-4124-8c53-c5392115269d");
            var Api = new ApiPSU();
            var LastLine = new TestReceipt();
            var MsSQL = new WDB_MsSql();
            var Receipt = MsSQL.db.Execute<TestReceipt>(SQLGetReceipt);
            IdReceiptWares IdWares;
            var LastReceipt = new IdReceipt();
            foreach (var L in Receipt)
            {
                IdWares = new IdReceiptWares() { CodeWares = L.Code_Wares, CodeUnit = L.Code_Unit };

                if (!L.Number.Equals(LastLine.Number))
                {
                    if (LastLine.Number != null)
                    {
                        Api.AddFiscalNumber(LastReceipt.ReceiptId, LastLine.Number);
                    }

                }

                var p = Api.AddProductByProductId(TerminalId, IdWares.WaresId, L.Amount);
                //Api.ChangeQuantity(TerminalId, IdWares.WaresId, L.Amount);

                if (!L.Number.Equals(LastLine.Number))
                {
                    LastReceipt = Api.GetCurrentReceiptByTerminalId(TerminalId);
                    if (L.Bar_Code != null)
                        Api.GetCustomerByBarCode(TerminalId, L.Bar_Code);
                    var RH = Api.Bl.GetReceiptHead(LastReceipt);
                    RH.AdditionN1 = L.Percent_Discount;
                    RH.AdditionC1 = L.Number;
                    RH.AdditionD1 = L.Date_Time;
                    RH.DateReceipt = L.Date_Time;

                    Api.Bl.db.ReplaceReceipt(RH);
                }
                var RW = Api.Bl.db.ViewReceiptWares(LastReceipt);
                var RWE = RW.FirstOrDefault(d => d.CodeWares == L.Code_Wares);
                RWE.AdditionN1 = L.Disc_Perc_Auto;
                RWE.AdditionN2 = L.Sum * (100m - L.Disc_perc_manual) / 100;
                RWE.AdditionN3 = L.Is_Promotion;
                RWE.AdditionC1 = L.Price.ToString() + " " + L.Type_Promotion;
                RWE.BarCode2Category = L.BarCode2Category;
                RWE.Description = L.Disc_perc_manual.ToString();
                Api.Bl.db.ReplaceWaresReceipt(RWE);

                LastLine = L;
            }
            if (LastLine.Number != null)
            {
                Api.AddFiscalNumber(LastReceipt.ReceiptId, LastLine.Number);
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

            var singleString = string.Join(",", Weight.OrderBy(r=>r).ToArray());
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



        public class mm
        {
            public ReceiptViewModel receipt { get; set; }
        }
        static void LoadReceiptJson()
        {
            string path = @"D:\DB\log\all.log";
            var TerminalId = Guid.Parse("1bb89aa9-dbdf-4eb0-b7a2-094665c3fdd0");
            var api = new ApiPSU();
            if (File.Exists(path))
            {
                // Open the file to read from.
                string[] readText = File.ReadAllLines(path);
                for (int i=0; i<readText.Length;i++)
                {
                    if (readText[i].Contains("[METHODEXECUTIONTIME] - AddFiscalNumber"))
                    {
                        i++;
                        //var r= l.Trim().Substring(1, l.Trim().Length - 2);
                        var res = JsonConvert.DeserializeObject<mm[]>(readText[i]);
                        var r = api.ReceiptViewModelToReceipt(TerminalId, res[0].receipt);
                        int CR = Convert.ToInt32(r.NumberReceipt);
                        r.CodeReceipt = CR;
                        foreach (var l in r.Payment)
                            l.CodeReceipt = CR;
                        foreach (var l in r.Wares)
                            l.CodeReceipt = CR;


                        api.Bl.SaveReceipt(r, false);
                    }
                
                
                }


            }


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
