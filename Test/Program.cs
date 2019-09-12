using ModelMID;
using ModernIntegration;
using SharedLib;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{

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
        public decimal Disc_Perc_Auto { get; set; }
        public int Is_Promotion { get; set; }
        public string Comment { get; set; }
        public string Type_Promotion { get; set; }

    }
    class Program
    {
        static void Main(string[] args)
        {
            var c = new Config("appsettings.json");
             CreateDataBase();
            //TestReceipt();
            //            var o = new SharedLib.Oracle();
            //var r =  o.Execute<ReceiptWares>("select w.code_wares CodeWares,w.name_wares as NameWares from dw.wares w where w.code_wares in (54882,54883)");
            //CreateReceipDay();//Чеки на основі нового з провірочною інформацією.
            
          /*  string varMidFile = Path.Combine(GlobalVar.PathDB, @"MID.db");
            var SQLite = new WDB_SQLite(varMidFile);
            SQLite.RecalcPrice(new IdReceipt() { IdWorkplace = 140701, CodePeriod = 20190910, CodeReceipt = 12 });
            */
        }

        static void CreateDataBase()
        {
            var MsSQL = new WDB_MsSql();

            DateTime varD = DateTime.Today;
            string varMidFile = Path.Combine(GlobalVar.PathDB, @"MID.db"); /*_" + varD.ToString("yyyyMMdd") + "*/
            if (File.Exists(varMidFile))
                File.Delete(varMidFile);


            var SQLite = new WDB_SQLite(varMidFile);
            SQLite.CreateMIDTable();

            var resS = MsSQL.LoadData(SQLite);
            SQLite.CreateMIDIndex();
            return;
        }

        static void TestReceipt()
        {
            var TerminalId = Guid.NewGuid();
            var ProductId = Guid.Parse("00000010-abcd-0000-0019-000000055004");
            var FastGroup = Guid.Parse("12345670-0987-0000-0000-000000009001");
            //Guid.Parse("00140701-FFFF-2019-0618-000000000008");
            var api = new ApiPSU();
            var Bl = new BL();

            var Cat = api.GetAllCategories(TerminalId);
            var war = api.GetProductsByCategoryId(TerminalId, FastGroup);
            //      var r=api.GetReceiptItem(new ModelMID.IdReceipt {CodePeriod=20190614,CodeReceipt=1,IdWorkplace= 140701});
            var res = api.AddProductByBarCode(TerminalId, "4820016250604");
            res = api.AddProductByBarCode(TerminalId, "5449000006271");
            res = api.AddProductByProductId(TerminalId, ProductId, 10);
            var Rec = api.ChangeQuantity(TerminalId, ProductId, 7);

            var f = api.GetProductsByName("апель");
            var ReceiptId = Rec.Id;
            var r = api.AddFiscalNumber(ReceiptId, "TRRF-1234");

            var rr = api.GetReciept(ReceiptId);

            var client = api.GetCustomerByBarCode(TerminalId, "8800000499710");
            //0959330766
            client = api.GetCustomerByPhone(TerminalId, "0959330766");

        }


        static void CreateReceipDay()
        {
            var SQLGetReceipt = @"SELECT top(100) ISNULL(td.PERCENT_DISCOUNT,0) AS PERCENT_DISCOUNT, dc.bar_code,  dr.number,dr.date_time
  ,w.Code_Wares
  ,dbo.GetCodeUnit(ud.code_unit)  AS Code_Unit
  , drw.amount
  ,drw.price
  ,drw.sum+drw.sum_bonus AS sum
  ,[disc_perc_auto]
  ,[is_promotion]
  ,dr.comment
  --,drw._Fld11310_RRRef
  ,SUBSTRING( sc._Description,4,2) +' '+CONVERT(nchar(32), _Fld17312RRef, 2) AS type_Promotion
  --,SUBSTRING( sc._Description,4,2)  AS barcode_SC
 -- COUNT(*) 
  FROM dbo.V1C_doc_receipt dr 
  JOIN dbo.V1C_doc_receipt_wares drw ON dr._IDRRef = drw._IDRRef
  JOIN dbo.Wares w ON drw.nomen_RRef=w._IDRRef
  LEFT JOIN dbo.V1C_DIM_CARD dc ON dr.card_RRef = dc.Card_RRef
  LEFT JOIN dbo.V1C_DIM_TYPE_DISCOUNT TD ON TD.TYPE_DISCOUNT_RRef =DC.TYPE_DISCOUNT_RRef
  JOIN dbo.V1C_dim_addition_unit au ON drw.uom_RRef=au._IDRRef
  JOIN  dbo.V1C_DIM_UNIT_DIMENSION ud ON au.Unit_dimention_RRef=ud.UNIT_DIMENSION_RRef 
  JOIN   UTPPSU.dbo._Reference18060 sc ON drw.barcode_2 = sc._IDRRef
    

  WHERE dr._Date_Time BETWEEN CONVERT(DATE,DATEADD(DAY,-1,DATEADD(YEAR,2000,GETDATE()))) AND CONVERT(DATE,DATEADD(DAY,1,DATEADD(YEAR,2000,GETDATE())))
  --AND ROUND(drw.amount*drw.price,2)<>drw.sum+drw.sum_bonus
--and is_promotion=1
  AND dr.warehouse_RRef= 0xB7A3001517DE370411DF7DD82E29F000
  --AND td.PERCENT_DISCOUNT<>[disc_perc_auto]
--  AND dr.number='К0800250773'
--  AND drw.sum_bonus>0
  ORDER BY dr._IDRRef";
            var TerminalId = Guid.NewGuid();
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
                
                var p = Api.AddProductByProductId(TerminalId, IdWares.WaresId,L.Amount);
                //Api.ChangeQuantity(TerminalId, IdWares.WaresId, L.Amount);

                if (!L.Number.Equals(LastLine.Number))
                {
                    LastReceipt = Api.GetCurrentReceiptByTerminalId(TerminalId);
                    if(L.Bar_Code!=null)
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
                RWE.AdditionN2 = L.Sum;
                RWE.AdditionN3 = L.Is_Promotion;
                RWE.AdditionC1 = L.Type_Promotion;
                Api.Bl.db.ReplaceWaresReceipt(RWE);

                LastLine = L;
            }
            if (LastLine.Number != null)
            {
                Api.AddFiscalNumber(LastReceipt.ReceiptId, LastLine.Number);
            }


        }

        //private CancellationTokenSource ts = new CancellationTokenSource();

        //private async Task<int> SomeFunction()
        //{
        //    var token = ts.Token;
        //    ts.Cancel();
        //    var b = await AAsynFunc(token);
        //    return b ?? 0;
        //}
        //private Task<int?> AAsynFunc(CancellationToken token)
        //{
        //    return Task<int?>.Run(() =>
        //    {
        //        while (!token.IsCancellationRequested)
        //        {
        //            //DO something
        //        }

        //        return (int?) 10;
        //    });
        //}

        //private async Task SomeFunction2()
        //{
        //    AAsynFunc().ContinueWith(async r => SomeFunction(await r));
        //}
    }
}
