using ModernIntegration;
using SharedLib;
using System;


namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var TerminalId = Guid.NewGuid();
            var ProductId = Guid.Parse("00000010-abcd-0000-0019-000000194748");
            var ReceiptId = Guid.Parse("00140701-FFFF-2019-0618-000000000008");
            var api = new ApiPSU();

            var Bl = new BL();
      //      var r=api.GetReceiptItem(new ModelMID.IdReceipt {CodePeriod=20190614,CodeReceipt=1,IdWorkplace= 140701});
          //  var res = api.AddProductByBarCode(TerminalId, "1376000062218");
           // res=api.AddProductByProductId (TerminalId, Guid.Parse("1A3B944E-3632-467B-A53A-000000194748"),10);

           // api.ChangeQuantity(TerminalId, ProductId, 7);


            //var r=api.AddFiscalNumber(ReceiptId, "TRRF-1234");

            var rr = api.GetReciept( ReceiptId);

            Console.WriteLine("Hello World!");
        }
    }
}
