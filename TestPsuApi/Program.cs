using System;
using System.IO;
using System.Threading.Tasks;
using ModelMID;
using ModernIntegration;
using ModernIntegration.Enums;
using ModernIntegration.Models;
using Remotion.Linq.Clauses;
using SharedLib;
using Utils;

namespace TestPsuApi
{
    class Program
    {
        private static readonly Guid TerminalId = Guid.Parse("1BB89AA9-DBDF-4EB0-B7A2-094665C3FDD0");

        static async Task Main(string[] args)
        {
            long lastSize = 0;
            "Start".WriteConsoleDebug();

            var dateTime = DateTime.Now;
            var config = new Config("appsettings.json");

            var receiptName = $"Rc_62_{dateTime.Year}{dateTime.Month:D2}{dateTime.Day:D2}.db";
            var path = Path.Combine(Global.PathDB, $"{dateTime.Year}{dateTime.Month:D2}", receiptName)
                .Replace("/", "\\");

            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                $"{dateTime.Year}_{dateTime.Month}_{dateTime.Day}.log");

            if (File.Exists(path))
                File.Delete(path);
            // для бази в режимі wal
            if (File.Exists(path + "-shm"))
                File.Delete(path + "-shm");

            if (File.Exists(path + "-wal"))
                File.Delete(path + "-wal");

            if (File.Exists(logPath))
                File.Delete(logPath);

            await Task.Delay(10);

            "Config Inited".WriteConsoleDebug();
            var api = new ApiPSU();
            "Api psu inited".WriteConsoleDebug();

            for (int i = 0; i < 10000; i++)
            {
                if (i % 20 == 0 )
                {
                    $"Current iteration".WriteConsoleDebug();
                    var LastReceipt = api.Bl.db.db.ExecuteScalar<int>("select max(code_receipt) from RECEIPT");
                    var fi = new FileInfo(path);

                    var wal = "";
                    if (File.Exists(path + "-wal"))
                    {

                        var fiw = new FileInfo(path + "-wal");
                        wal = $" WAL =>{fiw.LastWriteTime}";
                    }
                    FileLogger.WriteLogMessage($"{i} - db=>{LastReceipt} [{path}]-{fi.LastWriteTime} -{fi.Length}" + wal);
                    if (fi.Length == lastSize)
                    {
                        Console.WriteLine("\n!!!!!!!!!! Error !!!!!!!!!!\n");
                        Console.ReadKey();
                    }
                    lastSize = fi.Length;

                    await api.RequestSyncInfo();
                }

                try
                {
                    var pr = api.AddProductByBarCode(TerminalId, "4820116280075", 1);
                    await Task.Delay(5);
                    pr = api.AddProductByBarCode(TerminalId, "2201652300489", 1);
                    await Task.Delay(5);
                    pr = api.AddProductByBarCode(TerminalId, "7775006620509", 1);
                    await Task.Delay(5);
                    pr = api.AddProductByBarCode(TerminalId, "8810005077387", 1);
                    await Task.Delay(5);
                    var receipt = api.GetRecieptByTerminalId(TerminalId);
                    await Task.Delay(5);
                    var isSucces = api.AddPayment(TerminalId, new[]
                    {
                        new ReceiptPayment
                        {
                            Id = Guid.NewGuid(),
                            CardPan = "card",
                            CreatedAt = DateTime.Now,
                            InvoiceNumber = i,
                            PayIn = receipt.TotalAmount,
                            PaymentType = PaymentType.Card,
                            PayOut = 0,
                            PosPaid = receipt.TotalAmount,
                            ReceiptId = receipt.Id,
                            TransactionCode = "code",
                            TransactionId = "trid",
                            TransactionStatus = "status",
                            PosAddAmount = 0,
                            PosAuthCode = "auth",
                            PosTerminalId = "posid",
                        }
                    }, receipt.Id);
                    await Task.Delay(10);
                    isSucces = api.AddFiscalNumber(TerminalId, i.ToString(), receipt.Id);
                    await Task.Delay(10);

                   
                }
                catch (Exception e)
                {
                    $"{e.Message} {e.StackTrace}".WriteConsoleDebug();
                    Console.ReadKey();
                }
                finally
                {
                    $"Current receipt number {i}".WriteConsoleDebug();
                }

                await Task.Delay(500);
            }

            Console.ReadKey();
        }
    }
}