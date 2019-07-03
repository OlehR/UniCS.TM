using ModelMID;
using ModernIntegration;
using SharedLib;
using System;
using System.IO;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var Sql = @"SELECT DC.code_card as CodeClient ,DC.name as NameClient ,TD.TYPE_DISCOUNT  AS TypeDiscount, CAST('' AS VARCHAR(10)) AS MainPhone, td.PERCENT_DISCOUNT as PersentDiscount,[bar_code] AS BarCode, DCC.CODE_STATUS_CARD  AS StatusCard,dc. view_code  as ViewCode--,dcc.*
  FROM  dbo.V1C_DIM_CARD DC
 LEFT  JOIN dbo.V1C_DIM_CARD_STATUS DCC ON DC.STATUS_CARD_RRef=DCC.STATUS_CARD_RRef
 JOIN DW.dbo.V1C_DIM_TYPE_DISCOUNT TD ON TD.TYPE_DISCOUNT_RRef =DC.TYPE_DISCOUNT_RRef
  WHERE  DCC.CODE_STATUS_CARD=0 AND [bar_code]<>''";

            var MsSQL = new MSSQL();
            
            var resS = MsSQL.Execute<Client>(Sql);

            DateTime varD = DateTime.Today;
            string varMidFile = GlobalVar.PathDB +  @"MID_"  + varD.ToString("yyyyMMdd") + ".db";
            if (!File.Exists(varMidFile))
            {
                var SqlCreateMIDTable = @"CREATE TABLE CLIENT (
    CODE INTEGER NOT NULL,
NAME   TEXT NOT NULL,
    TYPE_DISCOUNT INTEGER,
    PHONE            INTEGER,
    PERCENT_DISCOUNT NUMBER,
    BARCODE          TEXT NOT NULL,
    STATUS_CARD INTEGER DEFAULT(0),
    view_code INTEGER
);
                ";
                var db = new SQLite(varMidFile);
                db.ExecuteNonQuery(SqlCreateMIDTable);

                var SQLReplace = "replace into CLIENT (CODE,NAME,TYPE_DISCOUNT,PHONE, PERCENT_DISCOUNT,BARCODE,STATUS_CARD,view_code) values (@CodeClient ,@NameClient ,@TypeDiscount,@MainPhone,@PersentDiscount,@BarCode,@StatusCard,@ViewCode)";
                db.BulkExecuteNonQuery<Client>(SQLReplace, resS);
                db.Close();
            }




            return;
            var TerminalId = Guid.NewGuid();
            var ProductId = Guid.Parse("00000010-abcd-0000-0019-000000194748");
            var ReceiptId = Guid.Parse("00140701-FFFF-2019-0618-000000000008");
            var api = new ApiPSU();

            var Bl = new BL();
      //      var r=api.GetReceiptItem(new ModelMID.IdReceipt {CodePeriod=20190614,CodeReceipt=1,IdWorkplace= 140701});
            var res = api.AddProductByBarCode(TerminalId, "1376000062218");
             res = api.AddProductByBarCode(TerminalId, "1376000062218");

            // res=api.AddProductByProductId (TerminalId, Guid.Parse("1A3B944E-3632-467B-A53A-000000194748"),10);

            // api.ChangeQuantity(TerminalId, ProductId, 7);


            //var r=api.AddFiscalNumber(ReceiptId, "TRRF-1234");

            // var rr = api.GetReciept( ReceiptId);
            // var client = api.GetCustomerByBarCode("5550000923502");
            //0959330766
            //var client = api.GetCustomerByPhone("0959330766");
            //var f = api.GetProductsByName("апель");
            Console.WriteLine("Hello World!");
        }
    }
}
