using ModelMID;
using ModelMID.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Utils;

namespace SharedLib
{
    public class DataSync
    {
        public WDB_SQLite db { get { return bl?.db; } }
        public BL bl;
        private readonly object _locker = new object();
        public SoapTo1C soapTo1C;

        public eSyncStatus Status = eSyncStatus.NotDefine;
        public bool IsReady { get { return Status != eSyncStatus.StartedFullSync && Status != eSyncStatus.Error; } }
        public DataSync(BL pBL)
        {
            soapTo1C = new SoapTo1C();
            bl = pBL; ///!!!TMP Трохи костиль 
            StartSyncData();
        }

        public async Task<bool> SyncDataAsync(bool parIsFull = false)
        {
            bool res = false;
            FileLogger.WriteLogMessage($"BL.SyncDataAsync({parIsFull}) Start");
            try
            {
                res = SyncData(ref parIsFull);
                var CurDate = DateTime.Now;
                // не обміннюємось чеками починаючи з 23:45 до 1:00
                if (!((CurDate.Hour == 23 && CurDate.Minute > 44) || CurDate.Hour == 0))
                    await SendAllReceipt().ConfigureAwait(false);

                if (CurDate.Hour < 7)
                {
                    //  ds.LoadWeightKasa();
                    LoadWeightKasa2Period();
                }
                if (parIsFull)
                    _ = SendRWDeleteAsync();
                FileLogger.WriteLogMessage($"BL.SyncDataAsync({parIsFull}) => {res} Finish");
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"BL.SyncDataAsync Exception =>( pTerminalId=>{parIsFull}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = e, Status = eSyncStatus.NoFatalError, StatusDescription = $"SyncDataAsync() Exception e.Message{Environment.NewLine}" + new StackTrace().ToString() });
            }

            return res;
        }

        public void StartSyncData()
        {
            if (Global.DataSyncTime > 0)
            {
                var t = new System.Timers.Timer(Global.DataSyncTime);
                t.AutoReset = true;
                t.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                t.Start();
                //OnTimedEvent(null,null);
            }            
        }

        private async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            await SyncDataAsync();
        }

        public bool SendReceiptTo1C(IdReceipt parIdReceipt)
        {
            var ldb = new WDB_SQLite(parIdReceipt.DTPeriod);
            var r = ldb.ViewReceipt(parIdReceipt, true);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _ = SendReceiptTo1CAsync(r, ldb);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return true;
        }
        public async Task<bool> SendReceiptTo1CAsync(Receipt pReceipt, WDB_SQLite parDB)
        {
            try
            {
                var rr = pReceipt.Wares.Where(r => r.Quantity != 0m);
                pReceipt.Wares = rr;

                var r = new Receipt1C(pReceipt);
                var body = soapTo1C.GenBody("JSONCheck", new Parameters[] { new Parameters("JSONSting", r.GetBase64()) });
                var res = await soapTo1C.RequestAsync(Global.Server1C, body, 240000, "application/json");

               if (!string.IsNullOrEmpty(res) && res.Equals("0"))
                {
                    pReceipt.StateReceipt = eStateReceipt.Send;
                    parDB.SetStateReceipt(pReceipt);//Змінюєм стан чека на відправлено.
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(pReceipt.IdWorkplace), Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "SendReceiptTo1CAsync=>" + pReceipt.ReceiptId.ToString() + " " + ex.Message + '\n' + new System.Diagnostics.StackTrace().ToString() });
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
        
        public bool SyncData(ref bool parIsFull)
        {
            lock (this._locker)
            {
                StringBuilder Log = new StringBuilder();
                Log.Append($"\n{DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} parIsFull=>{parIsFull}");
                try
                {
                    string varMidFile = db.GetCurrentMIDFile;
                    if (!parIsFull && !File.Exists(varMidFile)) //Якщо відсутній файл
                        parIsFull = true;

                    if (!parIsFull && File.Exists(varMidFile)) // Якщо база порожня.
                    {
                        int i = db.db.ExecuteScalar<int>("select count(*) from wares");
                        if (i == 0)
                            parIsFull = true;
                    }

                    //WDB_SQLite SQLite;
                    var TD = db.GetConfig<DateTime>("Load_Full");
                    if (!parIsFull)
                    {
                        if (TD == default(DateTime) || DateTime.Now.Date != TD.Date)
                            parIsFull = true;
                    }
                    Status = parIsFull ? eSyncStatus.StartedFullSync : eSyncStatus.NotDefine;
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Status = parIsFull ? eSyncStatus.StartedFullSync : eSyncStatus.StartedPartialSync, StatusDescription = "SendAllReceipt" });

                    Log.Append($"\n{DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} varMidFile=>{varMidFile}\n\tLoad_Full=>{TD:yyyy-MM-dd} parIsFull=>{parIsFull}");


                    if (parIsFull)
                    {
                        db.SetConfig<DateTime>("Load_Full", DateTime.Now.Date.AddDays(-1));
                        db.SetConfig<DateTime>("Load_Update", DateTime.Now.Date.AddDays(-1));
                        db.Close(true);

                        if (File.Exists(varMidFile))
                        {
                            Thread.Sleep(200);
                            Log.Append($"\n{DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} Try Delete file {varMidFile}");
                            File.Delete(varMidFile);
                        }
                        Log.Append($"\n{DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} Create New DB");
                        bl.db = new WDB_SQLite(default(DateTime), varMidFile);
                    }

                    var MsSQL = new WDB_MsSql();
                    var varMessageNMax = MsSQL.LoadData(db, parIsFull, Log);

                    if (parIsFull)
                    {
                        Log.Append($"\n{DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} Create MIDIndex");

                        db.CreateMIDIndex();
                        Log.Append($"\n{DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} Set config");
                        db.SetConfig<string>("Last_MID", varMidFile);
                    }

                    db.SetConfig<int>("MessageNo", varMessageNMax);
                    db.SetConfig<DateTime>("Load_" + (parIsFull ? "Full" : "Update"), DateTime.Now /*String.Format("{0:u}", DateTime.Now)*/);

                    Log.Append($"\n{DateTime.Now:yyyy-MM-dd h:mm:ss.fffffff} End");
                    Status = parIsFull ? eSyncStatus.SyncFinishedSuccess : eSyncStatus.NotDefine;
                    FileLogger.WriteLogMessage(Log.ToString());
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Status = eSyncStatus.SyncFinishedSuccess, StatusDescription = Log.ToString() });
                    
                }
                catch (Exception ex)
                {
                    Status = parIsFull ? eSyncStatus.Error : eSyncStatus.NotDefine;
                    FileLogger.WriteLogMessage(Log.ToString() + '\n' + ex.Message + '\n' + new System.Diagnostics.StackTrace().ToString(), eTypeLog.Error);
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = (parIsFull ? eSyncStatus.Error : eSyncStatus.NoFatalError), StatusDescription = "SyncData=>" + Log.ToString() + '\n' + ex.Message + '\n' + new System.Diagnostics.StackTrace().ToString() });
                   
                    Global.OnStatusChanged?.Invoke(db.GetStatus());
                    return false;
                }
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
                var ldb = new WDB_SQLite(Ldc);
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

        public async Task GetBonusAsync(Client pClient, int pIdWorkPlace)
        {
            try
            {
                decimal Sum;
                var body = soapTo1C.GenBody("GetBonusSum", new Parameters[] { new Parameters("CodeOfCard", pClient.BarCode) });
                var res = await soapTo1C.RequestAsync(Global.Server1C, body);
                res = res.Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                if (!string.IsNullOrEmpty(res) && decimal.TryParse(res, out Sum))
                    pClient.SumMoneyBonus = Sum; //!!!TMP
                body = soapTo1C.GenBody("GetMoneySum", new Parameters[] { new Parameters("CodeOfCard", pClient.BarCode) });
                res = await soapTo1C.RequestAsync(Global.Server1C, body);

                res = res.Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                if (!string.IsNullOrEmpty(res) && decimal.TryParse(res, out Sum))
                    pClient.Wallet = Sum;
            }
            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(pIdWorkPlace), Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = ex.Message });
            }
            Global.OnClientChanged?.Invoke(pClient, pIdWorkPlace);
        }

        public async Task<bool> CheckDiscountBarCodeAsync(IdReceipt pIdReceipt, string pBarCode, int pPercent)
        {
            bool isGood = true;
            decimal CountDiscount = 0; // На скільки товарів вже є знижка.
            try
            {
                var Cat2 = db.CheckLastWares2Cat(pIdReceipt);

                if (Cat2 == null || Cat2.Count() == 0)
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(pIdReceipt.IdWorkplace), Status = eSyncStatus.IncorectProductForDiscount });
                    return false;
                }
                var Cat2First = Cat2.First();
                Cat2First.BarCode2Category = pBarCode == null ? "" : pBarCode;
                Cat2First.Price = Cat2First.Price * (100m - (decimal)pPercent) / 100m;

                var LastQuantyity = db.GetLastQuantity(Cat2First);
                //Якщо не ваговий - то знижка на 1 шт.
                if (Cat2First.CodeUnit != Global.WeightCodeUnit && LastQuantyity > 0)
                    LastQuantyity = 1;

                var pr = db.GetReceiptWaresPromotion(new IdReceiptWares(pIdReceipt, Cat2First.CodeWares));

                if (pr != null && pr.Count() > 0)
                    CountDiscount = pr.Where(r => r.BarCode2Category.Length == 13).Sum(r => r.Quantity);

                if (CountDiscount > Cat2First.Quantity - LastQuantyity)
                    isGood = false;
                else
                {
                    Cat2First.Quantity = LastQuantyity;
                    try
                    {
                        var body = soapTo1C.GenBody("GetRestOfLabel", new Parameters[] { new Parameters("CodeOfLabel", pBarCode) });
                        var res = await soapTo1C.RequestAsync(Global.Server1C, body, 2000);
                        isGood = res.Equals("1");
                        Global.ErrorDiscountOnLine = 0;
                    }
                    catch (Exception ex)
                    {
                        Global.ErrorDiscountOnLine++;
                        Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(pIdReceipt.IdWorkplace), Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "CheckDiscountBarCodeAsync=>" + ex.Message });
                        Global.OnStatusChanged?.Invoke(db.GetStatus());

                    }
                }
                if (isGood)
                {
                    //Cat2.First()._Sum = Cat2.First().Sum; //Трохи костиль !!!!
                    //Cat2.First().Quantity = 0;
                    db.ReplaceWaresReceiptPromotion(Cat2);
                    db.InsertBarCode2Cat(Cat2First);
                    db.RecalcHeadReceipt(pIdReceipt);
                    var r = bl.GetReceiptHead(pIdReceipt, true);
                    Global.OnReceiptCalculationComplete?.Invoke(r);
                }
                else
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(pIdReceipt.IdWorkplace), Status = eSyncStatus.IncorectDiscountBarcode, StatusDescription = "CheckDiscountBarCodeAsync" });
                    return false;
                }
            }

            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(pIdReceipt.IdWorkplace), Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "CheckDiscountBarCodeAsync=>" + ex.Message + '\n' + new System.Diagnostics.StackTrace().ToString() });
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
      insert into barcode_out (bar_code, weight,Date) values ( @BarCode,@Weight,@Date,)
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
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "LoadWeightKasa=> " + ex.Message });
            }
        }

        public void LoadWeightKasa2Period(DateTime pDT = default(DateTime))
        {
            try
            {
                if (pDT == default(DateTime))
                {
                    pDT = db.GetConfig<DateTime>($"Load_Weight");
                    if (pDT == default(DateTime))
                        pDT = DateTime.Now.Date.AddDays(-1);
                }
                bool isCalc = false;
                while (pDT < DateTime.Now.Date)
                {
                    LoadWeightKasa2(pDT, 0);
                    LoadWeightKasa2(pDT, 1);
                    pDT = pDT.AddDays(1);
                    isCalc = true;
                }
                if (isCalc)
                {
                    db.SetConfig<DateTime>($"Load_Weight", pDT);
                }
            }
            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "LoadWeightKasa2Period=> " + ex.Message });
            }
        }

        public void LoadWeightKasa2(DateTime parDT, int TypeSource = 0)
        {
            try
            {
                var ldb = new WDB_SQLite(parDT);
                string SQLUpdate = @"insert into  DW.dbo.Weight_Receipt  (Type_Source,code_wares, weight,Date,ID_WORKPLACE, CODE_RECEIPT,QUANTITY) values (@TypeSource, @CodeWares,@Weight,@Date,@IdWorkplace,@CodeReceipt,@Quantity)";
                var dbMs = new MSSQL();

                var SqlSelect = TypeSource == 0 ? "select 0 as TypeSource,CODE_WARES as CodeWares,FIX_WEIGHT/1000.0 as WEIGHT,DATE_CREATE as date, ID_WORKPLACE as IdWorkplace, CODE_RECEIPT as CodeReceipt, QUANTITY as Quantity  from WARES_RECEIPT where FIX_WEIGHT>0" :
                    @"select 1  as TypeSource,re.CODE_WARES as CodeWares,re.PRODUCT_CONFIRMED_WEIGHT/1000.0 as WEIGHT,wr.DATE_CREATE as date, re.ID_WORKPLACE as IdWorkplace, re.CODE_RECEIPT as CodeReceipt, wr.QUANTITY as Quantity,wr.FIX_WEIGHT/1000.0 as FIX_WEIGHT
from RECEIPT_EVENT RE 
join WARES_RECEIPT wr on re.ID_WORKPLACE=wr.ID_WORKPLACE and wr.CODE_RECEIPT=re.CODE_RECEIPT and re.code_wares=wr.code_wares
where RE.EVENT_TYPE=1"
                    ;
                Console.WriteLine("Start LoadWeightKasa2");
                var r = ldb.db.Execute<WeightReceipt>(SqlSelect);
                Console.WriteLine(parDT.ToString() + " " + r.Count().ToString());
                dbMs.BulkExecuteNonQuery<WeightReceipt>(SQLUpdate, r);
                Console.WriteLine("Finish LoadWeightKasa2");
            }
            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "LoadWeightKasa2=> " + ex.Message });
            }
        }

        public async Task<string> GetQrCoffe(ReceiptWares pReceiptWares, int pOrder, int pWait = 5)
        {
            var Url = "https://dashboard.prostopay.net/api/v1/qreceipt/generate";
            string res = null;
            string Body = @"{
""pos"":1,
""till"": {Kassa},
""number"": {Order},
""created-at"": 0,
""ttl"": 1,
""ttl-type"": 0,
""amount"": 100,
""amount-base"": 3,
""plu-from"": {PLU},
""plu-to"": 0
}".Replace("{Order}", (++pOrder).ToString()).Replace("{PLU}", pReceiptWares.PLU.ToString()).Replace("{Kassa}", Math.Abs(pReceiptWares.IdWorkplace - 60).ToString());

            List<ReceiptEvent> rr = new List<ReceiptEvent> { new ReceiptEvent(pReceiptWares) { EventType = eReceiptEventType.AskQR, EventName = Body, CreatedAt = DateTime.Now } };
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMilliseconds(pWait * 1000);

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, Url);
                requestMessage.Headers.Add("X-API-KEY", "98e071c0-7177-4132-b249-9244464c97fb");
                requestMessage.Content = new StringContent(Body, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    res = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = null, Status = eSyncStatus.NoFatalError, StatusDescription = "RequestAsync=>" + response.RequestMessage });
                }
            }

            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(pReceiptWares.IdWorkplace), Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "GetQrCoffe=>" + ex.Message + '\n' + new System.Diagnostics.StackTrace().ToString() });
                rr.Add(new ReceiptEvent(pReceiptWares) { EventType = eReceiptEventType.ErrorQR, EventName = ex.Message, CreatedAt = DateTime.Now });
            }
            res = res.Replace("\"", "");
            rr.Add(new ReceiptEvent(pReceiptWares) { EventType = eReceiptEventType.AnswerQR, EventName = res, CreatedAt = DateTime.Now });
            bl.db.InsertReceiptEvent(rr);
            return res;
        }

        public async Task SendRWDeleteAsync()
        {
            var Ldc = db.GetConfig<DateTime>("LastDaySendDeleted");
            var today = DateTime.Now.Date;

            try
            {
                if (Ldc == default(DateTime))
                    Ldc = today.AddDays(-10);
                Ldc = Ldc.AddDays(1);
                while (Ldc < today)
                {
                    var ldb = new WDB_SQLite(Ldc);
                    var t = ldb.GetReceiptWaresDeleted();
                    var res = await Send1CReceiptWaresDeletedAsync(t);

                    if (res)
                        db.SetConfig<DateTime>("LastDaySendDeleted", Ldc);
                    else
                        break;
                    Ldc = Ldc.AddDays(1);
                }
            }
            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "SendRWDeleteAsync=>" + Ldc.ToString() + " " + ex.Message + '\n' + new System.Diagnostics.StackTrace().ToString() });
            }

        }

        public async Task<bool> Send1CReceiptWaresDeletedAsync(IEnumerable<ReceiptWaresDeleted1C> pRWD)
        {
            if (pRWD == null || pRWD.Count() == 0)
                return true;
            try
            {
                var d = new { data = pRWD };
                var r = JsonConvert.SerializeObject(d);
                var plainTextBytes = Encoding.UTF8.GetBytes(r);
                var resBase64 = Convert.ToBase64String(plainTextBytes);                
                var body = soapTo1C.GenBody("DeletedItemsInTheCheck", new Parameters[] { new Parameters("JSONSting", resBase64) });

                var res = await soapTo1C.RequestAsync(Global.Server1C, body, 60000, "application/json");

                if (!string.IsNullOrEmpty(res) && res.Equals("0"))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                var el = pRWD.First();
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(el.IdWorkplace), Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "Send1CReceiptWaresDeletedAsync=>" + el.CodePeriod.ToString() + " " + ex.Message + '\n' + new System.Diagnostics.StackTrace().ToString() });
                return false;
            }
        }
    }

    public class WeightReceipt
    {
        public int TypeSource { get; set; }
        public int CodeWares { get; set; }
        public decimal Weight { get; set; }
        public DateTime Date { get; set; }
        public int IdWorkplace { get; set; }
        public int CodeReceipt { get; set; }
        public decimal Quantity { get; set; }
    }
}
