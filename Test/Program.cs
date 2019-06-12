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

            Console.WriteLine("Hello World!");
        }
    }
}
