using ModernIntegration;
using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var api = new ApiPSU();
            var res = api.AddProductByBarCode(Guid.NewGuid(), "1376000062218");
            res=api.AddProductByProductId (Guid.NewGuid(),Guid.Parse("1A3B944E-3632-467B-A53A-000000194748"),10);
            Console.WriteLine("Hello World!");
        }
    }
}
