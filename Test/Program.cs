using ModelMID;
using ModernIntegration;
using SharedLib;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new Config("appsettings.json");
            CreateDataBase();
            //TestReceipt();
//            var o = new SharedLib.Oracle();
            //var r =  o.Execute<ReceiptWares>("select w.code_wares CodeWares,w.name_wares as NameWares from dw.wares w where w.code_wares in (54882,54883)");

        }

        static void CreateDataBase()
        {
            var MsSQL = new WDB_MsSql();

            DateTime varD = DateTime.Today;
            string varMidFile = Path.Combine( GlobalVar.PathDB , @"MID.db"); /*_" + varD.ToString("yyyyMMdd") + "*/
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
            var FastGroup =Guid.Parse("12345670-0987-0000-0000-000000009001");
            //Guid.Parse("00140701-FFFF-2019-0618-000000000008");
            var api = new ApiPSU();
            var Bl = new BL();

            var Cat = api.GetAllCategories(TerminalId);
            var war = api.GetProductsByCategoryId(TerminalId,FastGroup);
            //      var r=api.GetReceiptItem(new ModelMID.IdReceipt {CodePeriod=20190614,CodeReceipt=1,IdWorkplace= 140701});
            var res = api.AddProductByBarCode(TerminalId, "4823037501403");
                res = api.AddProductByBarCode(TerminalId, "9062300108665");
                res = api.AddProductByProductId(TerminalId, ProductId, 10);
                var Rec= api.ChangeQuantity(TerminalId, ProductId, 7);

            var f = api.GetProductsByName("апель");
            var ReceiptId = Rec.Id;
            var r =api.AddFiscalNumber(ReceiptId, "TRRF-1234");

            var rr = api.GetReciept(ReceiptId);

            var client = api.GetCustomerByBarCode(TerminalId, "8800000499710");
            //0959330766
             client = api.GetCustomerByPhone(TerminalId,"0959330766");
            
        }
    }
}
