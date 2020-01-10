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

namespace Test
{

    class Program
    {
        static async Task Main(string[] args)
        {

            //LoadWeightKasa();
            // var R = await GetInfoBarcode("4823000920439");
            //var l = new GetGoodUrl();
            //l.LoadWeightURLAsync();
            //Thread.Sleep(1000000000);

           // return;
            var c = new Config("appsettings.json");// Конфігурація Програми(Шляхів до БД тощо)
            //CreateDataBase(false); //Створення бази
            //Thread.Sleep(10000);
            //TestKit();
            TestReceipt(); //
                           //CreateReceipDay();//Чеки на основі нового з провірочною інформацією.
                           //            var o = new SharedLib.Oracle();
                           //var r =  o.Execute<ReceiptWares>("select w.code_wares CodeWares,w.name_wares as NameWares from dw.wares w where w.code_wares in (54882,54883)");



            /*  string varMidFile = Path.Combine(GlobalVar.PathDB, @"MID.db");
              var SQLite = new WDB_SQLite(varMidFile);
              SQLite.RecalcPrice(new IdReceipt() { IdWorkplace = 140701, CodePeriod = 20190910, CodeReceipt = 12 });
              */
            Thread.Sleep(100000);

        }

        static void CreateDataBase(bool isFull = true)
        {
            var bl = new BL();
            bl.SyncData(isFull);
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
        static void TestReceipt()
        {
            var TerminalId = Guid.Parse("1bb89aa9-dbdf-4eb0-b7a2-094665c3fdd0");
            var ProductId = Guid.Parse("00000010-abcd-0000-0019-000000055004");
            var FastGroup = Guid.Parse("12345670-0987-0000-0000-000000009001");
            var api = new ApiPSU();
            ProductViewModel sd;

            var a = api.UpdateProductWeight("CODE:" + ProductId.ToString(),0);

           //var rrr= api.GetReceipts(DateTime.Parse("2020-01-09T00:00:00"), DateTime.Parse("2020-01-09T23:59:59.999"), TerminalId);
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

            var startTime = System.Diagnostics.Stopwatch.StartNew();

            //sd = api.AddProductByBarCode(TerminalId, "4823086109988", 1); // 1+1 Пельмені "Мішутка" Філейні 600г /Три ведмеді/
            //sd = api.AddProductByBarCode(TerminalId, "2201652300489", 1); //Морква
            //Thread.Sleep(1000);
            sd = api.AddProductByBarCode(TerminalId, "4823000916524", 1); //АРТЕК 
            sd = api.AddProductByBarCode(TerminalId, "7622300813437", 1);//Барн

            sd = api.AddProductByBarCode(TerminalId, "2201652300489", 1); //Морква
            //Thread.Sleep(1000);
            sd = api.AddProductByBarCode(TerminalId, "2201652300229", 1); //Морква
                                                                          //Thread.Sleep(1000);

            api.ChangeQuantity(TerminalId, sd.Id, 2);
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

            startTime.Stop();
            Console.WriteLine(startTime.Elapsed);
            startTime.Restart();

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

            var RId = api.GetCurrentReceiptByTerminalId(TerminalId).ReceiptId;

            var rr = api.GetProduct(TerminalId);
            ReceiptPayment[] pay = new ReceiptPayment[]
                {new ReceiptPayment {ReceiptId= RId ,
                                    PaymentType=ModernIntegration.Enums.PaymentType.Card ,PayIn=39.9m,PayOut=0.0m,
                    CardPan ="7548********8954",TransactionId="37108628262516415955665803737316905361" } };
            api.AddPayment(TerminalId, pay);
            var r = api.AddFiscalNumber(TerminalId, "TRRF-1234");

            api.SendReceipt(RId);
            Console.WriteLine("End");

            //169316+169316 4823086109988 Пельмені "Мішутка" Філейні 600г /Три ведмеді/
            //156727+169583 4823097403457+4823097405932 Майонез "Провансаль" 67% д/п 350г /Щедро/  Кетчуп "Лагідний" д/п 250г /Щедро/

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

        public static void LoadWeightKasa()
        {
            string varSQLUpdate = @"
-- begin tran
   update barcode_out with (serializable) set weight=@Weight,Date=@Date
   where bar_code = @BarCode

   if @@rowcount = 0
   begin
      insert into barcode_out (bar_code, weight,Date) values ( @BarCode,@Weight,@Date)
   end
-- commit tran";

            var dbMs = new MSSQL();
            var dbSqlite = new SQLite(@"C:\DB\config.db");
            var r = dbSqlite.Execute<BarCodeOut>(@"select BARCODE as BarCode, avg(WEIGHT) as Weight,max(DATE_CREATE) as Date from WEIGHT where Weight>0 group by BARCODE");
            dbMs.BulkExecuteNonQuery<BarCodeOut>(varSQLUpdate, r);

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
