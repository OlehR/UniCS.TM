using Microsoft.EntityFrameworkCore.Storage.Internal;
using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLib
{
    public class DataSync
    {
        public WDB_SQLite db;
        public BL bl;
        
        public SoapTo1C soapTo1C;
        public DataSync(BL parBL)
        {
            
            soapTo1C = new SoapTo1C();
            bl = parBL; ///!!!TMP Трохи костиль 
            db = parBL.db;
        }


        public bool SendReceiptTo1C(IdReceipt parIdReceipt)
        {
            var ldb = new WDB_SQLite( parIdReceipt.DTPeriod);
            var r = ldb.ViewReceipt(parIdReceipt, true);

            
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            SendReceiptTo1CAsync(r, ldb);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return true;
        }
        public async Task<bool> SendReceiptTo1CAsync(Receipt parReceipt, WDB_SQLite parDB)
        {
            try
            {
              
                var r = new Receipt1C(parReceipt);

                var body = soapTo1C.GenBody("JSONCheck", new Parameters[] { new Parameters("JSONSting", r.GetBase64()) });

                var res =  await soapTo1C.RequestAsync(Global.Server1C, body, 45000, "application/json");

                /*
                                 HttpClient client = new HttpClient();

                                 // Add a new Request Message
                                 HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, Global.Server1C);
                                 //requestMessage.Headers.Add("Accept", "application/vnd.github.v3+json");
                                 // Add our custom headers
                                 requestMessage.Content = new StringContent(//r.GetSOAP() 
                                     body, Encoding.UTF8, "application/json");
                                 var response = await client.SendAsync(requestMessage);

                                 if (response.IsSuccessStatusCode)
                                 {
                                     var res = await response.Content.ReadAsStringAsync();
                                     parReceipt.StateReceipt = eStateReceipt.Send;
                                     db.SetStateReceipt(parReceipt);//Змінюєм стан чека на відправлено.
                                     return true;
                                 }
                                 else
                                 {
                                     return false;
                                 }
                                */
                
                if (!string.IsNullOrEmpty(res) && res.Equals("0"))
                {
                    parReceipt.StateReceipt = eStateReceipt.Send;
                    parDB.SetStateReceipt(parReceipt);//Змінюєм стан чека на відправлено.
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(parReceipt.IdWorkplace), Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "SendReceiptTo1CAsync=>" + parReceipt.ReceiptId.ToString() + " " + ex.Message });
                return false;
            }
        }
        public async Task<bool> SendAllReceipt(WDB_SQLite parDB = null)
        {
            var varDB = (parDB == null ? db : parDB);
            var varReceipts = varDB.GetIdReceiptbyState(eStateReceipt.Print);
            foreach (var el in varReceipts)
                await SendReceiptTo1CAsync(varDB.ViewReceipt(el, true), varDB);
            if (parDB == null)
                SendOldReceipt();
            Global.OnStatusChanged?.Invoke(db.GetStatus());
            return true;
        }

        //async Task<bool>
        public bool SyncData(bool parIsFull)
        {
            StringBuilder Log = new StringBuilder();
            try
            {
                //WDB_SQLite SQLite;

                if (!parIsFull)
                {
                    var TD = db.GetConfig<DateTime>("Load_Full");
                    if (TD == default(DateTime) || DateTime.Now.Date != TD.Date)
                        parIsFull = true;
                }
                 
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Status = parIsFull ? eSyncStatus.StartedFullSync : eSyncStatus.StartedPartialSync });
                
                string varMidFile = db.GetCurrentMIDFile;

                if (parIsFull)
                {
                    Thread.Sleep(200);
                    DateTime varD = DateTime.Today;

                    if (File.Exists(varMidFile))
                    {
                        Log.Append($"{DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} Try Delete file{varMidFile}");
                        db.SetConfig<DateTime>("Load_Full", DateTime.Now.Date.AddDays(-1));
                        db.SetConfig<DateTime>("Load_Update", DateTime.Now.Date.AddDays(-1));

                        bl.db.Close(true);                     
                        File.Delete(varMidFile);
                        bl.db = new WDB_SQLite(default(DateTime), varMidFile);
                        db = bl.db;
                    }
                    Log.Append($"{ DateTime.Now:yyyy - MM - dd h: mm: ss.fffffff} Create New DB");                   
                    //SQLite.CreateMIDTable();
                }


                    var MsSQL = new WDB_MsSql();
                    var varMessageNMax = MsSQL.LoadData(db, parIsFull, Log);

                    if (parIsFull)
                    {
                        Log.Append($"{ DateTime.Now:yyyy - MM - dd h: mm: ss.fffffff} Create New DB");
                        Debug.WriteLine("CreateMIDIndex Start");
                        db.CreateMIDIndex();
                        
                        
                        db.SetConfig<string>("Last_MID", varMidFile);
                        Log.Append($"{ DateTime.Now:yyyy - MM - dd h: mm: ss.fffffff} Set config");
                        Debug.WriteLine("Set config");
                    }

                   

                    db.SetConfig<int>("MessageNo", varMessageNMax);
                    db.SetConfig<DateTime>("Load_" + (parIsFull ? "Full" : "Update"), DateTime.Now /*String.Format("{0:u}", DateTime.Now)*/);
                
                Log.Append($"{ DateTime.Now:yyyy - MM - dd h: mm: ss.fffffff} End");
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Status = eSyncStatus.SyncFinishedSuccess, StatusDescription = Log.ToString() });
                }
            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = (parIsFull ? eSyncStatus.Error: eSyncStatus.NoFatalError), StatusDescription = Log.ToString() });
                Global.OnStatusChanged?.Invoke(db.GetStatus());
                return false;
            }

            return true;
        }
        public class TableStruc
        {
            public int Cid { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Dflt_value { get; set; }
            public int PK { get; set; }
        }
        public string BildSqlUpdate(string parTableName)
        {
            var r = db.db.Execute<TableStruc>($"PRAGMA table_info('{parTableName}');");
            var ListField = "";
            var Where = "";
            var On = "";

            foreach (var el in r)
            {
                ListField += (ListField.Length > 0 ? ", " : "") + el.Name;
                if (el.PK == 1)
                    On += (On.Length > 0 ? " and " : "") + $"main.{el.Name}=upd.{el.Name}";
                else
                    Where += (Where.Length > 0 ? " or " : "") + $"main.{el.Name}!=upd.{el.Name}";
            }

            var Res = $"replace parTableName ({ListField}) \n select {ListField} from main.{parTableName}\n join upd.{parTableName} on ( {On})\n where {Where}";
            return Res;
        }

        public void SendOldReceipt()
        {
            var Ldc = db.GetConfig<DateTime>("LastDaySend");
            var today = DateTime.Now.Date;

            if (Ldc == default(DateTime))
                Ldc = today.AddDays(-10);

            while (Ldc < today)
            {
                var ldb = new WDB_SQLite( Ldc);
                var t = SendAllReceipt(ldb);
                t.Wait();
                var res = ldb.GetIdReceiptbyState(eStateReceipt.Print);
                if (res.Count() == 0)
                    db.SetConfig<DateTime>("LastDaySend", Ldc);
                else
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Status = eSyncStatus.NoFatalError, StatusDescription = $"SendOldReceipt => ErrorSend Date:{Ldc} Not Send => {res.Count()}" });
                    return;
                }
                Ldc = Ldc.AddDays(1);
            }
            //Перекидаємо лічильник на сьогодня.
            db.SetConfig<DateTime>("LastDaySend", Ldc);

        }
        public async Task GetBonusAsync(Client parClient, Guid parTerminalId)
        {
            try
            {
                decimal Sum;
                var body = soapTo1C.GenBody("GetBonusSum", new Parameters[] { new Parameters("CodeOfCard", parClient.BarCode) });
                var res = await soapTo1C.RequestAsync(Global.Server1C, body);
                res = res.Replace('.', ',');
                if (!string.IsNullOrEmpty(res) && decimal.TryParse(res, out Sum))
                    parClient.SumMoneyBonus = Sum; //!!!TMP
                body = soapTo1C.GenBody("GetMoneySum", new Parameters[] { new Parameters("CodeOfCard", parClient.BarCode) });
                res = await soapTo1C.RequestAsync(Global.Server1C, body);
                res = res.Replace('.', ',');
                if (!string.IsNullOrEmpty(res) && decimal.TryParse(res, out Sum))
                    parClient.Wallet = Sum;
                Global.OnClientChanged?.Invoke(parClient, parTerminalId);
            }
            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = parTerminalId, Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = ex.Message });
            }

            //return parClient;
        }

        public async Task<bool> CheckDiscountBarCodeAsync(IdReceipt parIdReceipt, string parBarCode, int parPercent)
        {
            try
            {
                var Cat2 = db.CheckLastWares2Cat(parIdReceipt);
                if (Cat2 == null || Cat2.Count() == 0)
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(parIdReceipt.IdWorkplace), Status = eSyncStatus.IncorectProductForDiscount });
                    return false;
                }
                var Cat2First = Cat2.First();
                Cat2First.BarCode2Category = parBarCode;
                Cat2First.Price = Cat2First.Price * (100m - (decimal)parPercent) / 100m;

                bool isGood = true;
                try
                {
                    var body = soapTo1C.GenBody("GetRestOfLabel", new Parameters[] { new Parameters("CodeOfLabel", parBarCode) });
                    var res = await soapTo1C.RequestAsync(Global.Server1C, body,5000);
                    isGood = res.Equals("1");

                    /*     string body = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                      "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd = \"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\n" +
                         $"<soap:Body>\n<GetRestOfLabel xmlns=\"vopak\">\n<CodeOfLabel>{parBarCode}</CodeOfLabel> \n </GetRestOfLabel> \n </soap:Body>\n </soap:Envelope>";

                         HttpClient client = new HttpClient();
                         client.Timeout = TimeSpan.FromMilliseconds(5000);
                         // Add a new Request Message
                         HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, Global.Server1C);
                         //requestMessage.Headers.Add("Accept", "application/vnd.github.v3+json");
                         // Add our custom headers
                         requestMessage.Content = new StringContent(body, Encoding.UTF8, "text/xml");
                         var response = await client.SendAsync(requestMessage);

                         if (response.IsSuccessStatusCode)
                         {
                             var res = await response.Content.ReadAsStringAsync();
                             res = res.Substring(res.IndexOf(@"-instance"">") + 11);
                             res = res.Substring(0, res.IndexOf("</m:return>")).Trim();
                             isGood = res.Equals("1");
                         }*/
                    Global.ErrorDiscountOnLine = 0;
                }
                catch (Exception ex)
                {
                    Global.ErrorDiscountOnLine++;
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(parIdReceipt.IdWorkplace), Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "CheckDiscountBarCodeAsync=>" + ex.Message });
                    Global.OnStatusChanged?.Invoke(db.GetStatus());

                }

                if (isGood)
                {
                    //Cat2.First()._Sum = Cat2.First().Sum; //Трохи костиль !!!!
                    //Cat2.First().Quantity = 0;
                    db.ReplaceWaresReceiptPromotion(Cat2);
                    db.InsertBarCode2Cat(Cat2First);
                    db.RecalcHeadReceipt(parIdReceipt);
                    var r = bl.ViewReceiptWares(new IdReceiptWares(parIdReceipt, 0),true);//вертаємо весь чек.
                    Global.OnReceiptCalculationComplete?.Invoke(r, Global.GetTerminalIdByIdWorkplace(parIdReceipt.IdWorkplace));
                }
                else
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(parIdReceipt.IdWorkplace), Status = eSyncStatus.IncorectDiscountBarcode });
                    return false;
                }
            }

            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(parIdReceipt.IdWorkplace), Exception = ex, Status = eSyncStatus.Error, StatusDescription = ex.Message });
            }
            return true;
        }
        public void LoadWeightKasa(DateTime parDT = default(DateTime))
        {
            try
            {
                if (parDT == default(DateTime))
                {
                    parDT = db.GetConfig<DateTime>("Load_Full");
                    var parDTU = db.GetConfig<DateTime>("Load_Update");
                    if (parDTU > parDT)
                        parDT = parDTU;
                }

                string SQLUpdate = @"
-- begin tran
   update barcode_out with (serializable) set weight=@Weight,Date=@Date
   where bar_code = @BarCode

   if @@rowcount = 0
   begin
      insert into barcode_out (bar_code, weight,Date) values ( @BarCode,@Weight,@Date)
   end
-- commit tran";

                var dbMs = new MSSQL();
                //var path = Path.Combine(ModelMID.Global.PathDB, "config.db");
               // var dbSqlite = new SQLite(path);
                var SqlSelect = @"select [barcode] as BarCode, weight as Weight, DATE_CREATE as Date,STATUS
from ( SELECT [barcode],DATE_CREATE,STATUS,weight,ROW_NUMBER() OVER (PARTITION BY barcode ORDER BY DATE_CREATE DESC) as [nn]  FROM WEIGHT where  DATE_CREATE>=:parDT ) dd
where nn=1 ";
                Console.WriteLine("Start LoadWeightKasa");
                var r = db.db.Execute<object, BarCodeOut>(SqlSelect, new { parDT = parDT });
                Console.WriteLine(r.Count().ToString());
                dbMs.BulkExecuteNonQuery<BarCodeOut>(SQLUpdate, r);
                Console.WriteLine("Finish LoadWeightKasa");
            }
             catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "LoadWeightKasa=> "+ ex.Message });                
            }
        }


    }
}
