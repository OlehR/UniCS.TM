using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using UtilNetwork;
using Utils;
using System.IO.Compression;

//using Model;

namespace SharedLib
{
    public class DataSync
    {
        public WDB_SQLite db { get { return bl?.db; } }
        BL bl;
        private readonly object _locker = new object();
        public bool IsUseOldDB { get; set; } = true;

        public eSyncStatus Status { get; set; } = eSyncStatus.NotDefine;
        public bool IsReady
        {
            get
            {
                if (db.DBStatus != eDBStatus.Ok) Status = eSyncStatus.ErrorDB;
                return IsUseOldDB || (Status != eSyncStatus.StartedFullSync && Status != eSyncStatus.Error && Status != eSyncStatus.ErrorDB);
            }
        }
        public DataSync(BL pBL)
        {
            if (pBL != null)
            {
                bl = pBL; ///!!!TMP Трохи костиль 
                StartSyncData();
            }
        }
        bool IsSync(int p) => true;

        public async Task<bool> SyncDataAsync(bool parIsFull = false)
        {
            bool res = false;
            if (!IsSync(Global.CodeWarehouse))
            {
                FileLogger.WriteLogMessage(this, "SyncDataAsync", $"Обмін заблоковано", eTypeLog.Expanded);
                return res;
            }
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
                    await LoadWeightKasa2PeriodAsync();
                }
                if (parIsFull)
                    _ = SendRWDeleteAsync();
                _ = Send1CClientAsync();
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
            }
        }

        private async void OnTimedEvent(Object source, ElapsedEventArgs e) => await SyncDataAsync();

        public bool SendReceiptTo1C(IdReceipt pIdR)
        {
            using var ldb = new WDB_SQLite(pIdR.DTPeriod);
            if (!IsSync(Global.CodeWarehouse))
            {
                FileLogger.WriteLogMessage(this, "SendReceiptTo1C", $"Обмін заблоковано CodeReceipt=>{pIdR.CodeReceipt}", eTypeLog.Expanded);
                return false;
            }
            var R = ldb.ViewReceipt(pIdR, true);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            //_ = DataSync1C.SendReceiptTo1CAsync(R, ldb);
            _ = SendReceipt(R); // В сховище чеків
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return true;
        }

        public async Task<bool> SendAllReceipt(WDB_SQLite parDB = null)
        {
            var varDB = (parDB ?? db);
            var varReceipts = varDB.GetIdReceiptbyState(eStateReceipt.Print);
            foreach (var el in varReceipts)
            {
                var R = varDB.ViewReceipt(el, true);
               // await DataSync1C.SendReceiptTo1CAsync(R, varDB);
                _ = SendReceipt(R); // В сховище чеків
            }
            if (parDB == null)
                SendOldReceipt();
            Global.OnStatusChanged?.Invoke(db.GetStatus());
            return true;
        }

        public bool SyncData(ref bool pIsFull)
        {
            bool IsFull = pIsFull;
            lock (this._locker)
            {
                FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, $"parIsFull=>{IsFull}");
                string varMidFile = db.GetMIDFile();
                try
                {
                    if (!IsFull && !File.Exists(varMidFile)) //Якщо відсутній файл
                    {
                        IsFull = true;
                        FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, $"Відсутній файл {varMidFile} parIsFull=>{IsFull} ");
                    }
                    if (!IsFull && File.Exists(varMidFile)) // Якщо база порожня.
                    {
                        try
                        {
                            int i = db.db.ExecuteScalar<int>("select count(*) from wares");
                            if (i == 0)
                            {
                                IsFull = true;
                                FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, $"Відсутні дані {varMidFile} parIsFull=>{IsFull} ");
                            }
                        }
                        catch (Exception) { IsFull = true; }
                    }

                    var TD = db.GetConfig<DateTime>("Load_Full");
                    if (!IsFull)
                    {
                        if (TD == default(DateTime) || DateTime.Now.Date != TD.Date)
                            IsFull = true;
                        FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, $"Устарівші дані {TD:yyyy-MM-dd} {varMidFile} parIsFull=>{IsFull} ");
                    }

                    Status = IsFull ? eSyncStatus.StartedFullSync : eSyncStatus.NotDefine;
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Status = IsFull ? eSyncStatus.StartedFullSync : eSyncStatus.StartedPartialSync, StatusDescription = "SendAllReceipt" });

                    FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, $"varMidFile=>{varMidFile}\n\tLoad_Full=>{TD:yyyy-MM-dd} parIsFull=>{IsFull}");

                    string NameDB = db.GetMIDFile(default, IsFull);
                    int MessageNoMin = db.GetConfig<int>("MessageNo");
                    if (IsFull)
                    {
                        db.SetConfig<DateTime>("Load_Full", DateTime.Now.Date.AddDays(-1).Date);
                        db.SetConfig<DateTime>("Load_Update", DateTime.Now.Date.AddDays(-1).Date);

                        Exception Ex = null;
                        if (File.Exists(varMidFile))
                        {
                            Thread.Sleep(200);
                            FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, $"Try Delete file {varMidFile}");
                            try
                            {
                                File.Delete(varMidFile);
                            }
                            catch (Exception e)
                            {
                                FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, e);
                            }
                        }

                        if (File.Exists(NameDB))
                        {
                            Thread.Sleep(200);
                            FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, $"Try Delete file {NameDB}");
                            try
                            {
                                File.Delete(NameDB);
                            }
                            catch (Exception e)
                            {
                                FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, e);
                            }
                        }

                        if (File.Exists(varMidFile) || File.Exists(NameDB))
                        {
                            Status = eSyncStatus.Error;
                            Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = Ex, Status = eSyncStatus.Error, StatusDescription = $"SyncData Error=> Помилка видалення файла {Ex?.Message}" });
                            return false;
                        }
                        FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, "Create New DB");
                    }

                    if (!IsSync(Global.CodeWarehouse)) return false;

                    SQLiteMid pD = null;
                    MidData r = null;
                    //if (Global.IsHttp){
                    bool IsReloadFull = pIsFull;
                    r = AsyncHelper.RunSync(() => LoadDataAsync(Global.IdWorkPlace, IsFull, NameDB, MessageNoMin, IsReloadFull));
                    pD = new(NameDB);
                    if (!IsFull)
                    {
                        /* (IsFull)
                        {
                            pD.ExecuteNonQuery(SQLiteMid.SqlCreateMIDTable);
                            pD.SetVersion(SQLiteMid.VerMID);
                        }*/
                        LoadDataSQL(pD, r);
                    }
                    /*}
                    else
                    {
                        pD= new(NameDB);
                        if (IsFull)
                        {
                            pD.ExecuteNonQuery(SQLiteMid.SqlCreateMIDTable);
                            pD.SetVersion(SQLiteMid.VerMID);
                        }
                        r = MsSQL.LoadData(Global.IdWorkPlace, IsFull, pD, MessageNoMin);
                    }*/
                    if (r == null)
                    {
                        Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Status = (IsFull ? eSyncStatus.Error : eSyncStatus.NoFatalError), StatusDescription = $"LoadData return null value" });
                        return false;
                    }
                    if (r?.WorkPlace?.Any() == true)
                    {
                        FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, $"Replace SqlGetDimWorkplace => {r?.WorkPlace?.Count()}");
                        db.ReplaceWorkPlace(r.WorkPlace);
                        FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, $"Write SqlGetDimWorkplace => {r?.WorkPlace?.Count()}");
                        Global.BildWorkplace(r.WorkPlace);
                    }
                    int MessageNMax = r?.MessageNoMax ?? 0;
                    if (IsFull)
                    {
                        int CW = pD.ExecuteScalar<int>("select count(*) from wares");
                        if (CW > 1000)
                        {
                            FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, "Create MIDIndex");
                            // (!Global.IsHttp)  pD?.CreateMIDIndex();
                            pD?.Close();
                            try
                            {
                                db.Close(true);
                                File.Move(NameDB, varMidFile);
                                db.SetConfig<DateTime>("Load_Full", DateTime.Now);
                                db.GetDB();
                                FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, $"Set config LastMidFile=> {db.LastMidFile}");
                            }
                            catch (Exception e)
                            {
                                FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, e);
                            }
                        }
                        else
                        {
                            FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, $"Wares=>{CW}");
                            return false;
                        }
                    }
                    db.BildWaresWarehouse();
                    db.SetConfig<int>("MessageNo", MessageNMax);
                    db.SetConfig<DateTime>("Load_Update", DateTime.Now);

                    FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, "End");
                    Status = eSyncStatus.SyncFinishedSuccess;
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Status = eSyncStatus.SyncFinishedSuccess, StatusDescription = "SyncData=>Ok" });
                }
                catch (Exception ex)
                {
                    //bl.db = new WDB_SQLite(default);
                    Status = IsFull ? eSyncStatus.Error : eSyncStatus.NotDefine;
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = (IsFull ? eSyncStatus.Error : eSyncStatus.NoFatalError), StatusDescription = $"SyncData Error=>{ex.Message}" });
                    Global.OnStatusChanged?.Invoke(db.GetStatus());
                    FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, ex);
                    return false;
                }
            }
            return true;
        }

        public void SendOldReceipt()
        {
            var Ldc = db.GetConfig<DateTime>("LastDaySend");
            var today = DateTime.Now.Date;

            if (Ldc == default(DateTime))
                Ldc = today.AddDays(-10);

            while (Ldc < today)
            {
                using var ldb = new WDB_SQLite(Ldc);
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

        public async Task LoadWeightKasa2PeriodAsync(DateTime pDT = default(DateTime))
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
                    isCalc = await LoadWeightKasa2Async(pDT);
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

        public async Task<bool> LoadWeightKasa2Async(DateTime parDT)
        {
            try
            {
                using var ldb = new WDB_SQLite(parDT);
                var r = ldb.GetWeightReceipt();

                HttpClient client = new();
                client.Timeout = TimeSpan.FromMilliseconds(3000);

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, Global.Api + "CashRegister/SetWeightReceipt");
                string data = r.ToJson();
                requestMessage.Content = new StringContent(data, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(res))
                    {
                        var Res = Newtonsoft.Json.JsonConvert.DeserializeObject<Status>(res);
                        return Res?.status ?? false;
                    }
                }
            }
            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "LoadWeightKasa2=> " + ex.Message });
            }
            return false;
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
""amount"": {Price},
""amount-base"": 3,
""plu-from"": {PLU},
""plu-to"": 0
}".Replace("{Order}", (++pOrder).ToString()).Replace("{PLU}", pReceiptWares.PLU.ToString()).
Replace("{Kassa}", Math.Abs(pReceiptWares.IdWorkplace - 60).ToString()).Replace("{Price}", ((int)(pReceiptWares.PriceDealer * 100m)).ToString());

            bl.db.InsertReceiptEvent(new ReceiptEvent(pReceiptWares) { EventType = eReceiptEventType.AskQR, EventName = Body, CreatedAt = DateTime.Now });
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
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "GetQrCoffe=>" + ex.Message + '\n' + new System.Diagnostics.StackTrace().ToString() });
                bl.db.InsertReceiptEvent(new ReceiptEvent(pReceiptWares) { EventType = eReceiptEventType.ErrorQR, EventName = ex.Message, CreatedAt = DateTime.Now });
            }
            res = res?.Replace("\"", "");
            bl.db.InsertReceiptEvent(new ReceiptEvent(pReceiptWares) { EventType = eReceiptEventType.AnswerQR, EventName = res, CreatedAt = DateTime.Now });
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
                    using var ldb = new WDB_SQLite(Ldc);
                    var t = ldb.GetReceiptWaresDeleted();
                    //var res = await DataSync1C.Send1CReceiptWaresDeletedAsync(t);
                    HttpClient client = new();
                    client.Timeout = TimeSpan.FromMilliseconds(3000);

                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, Global.Api + "Send1CReceiptWaresDeleted");
                    string data = t.ToJson();
                    requestMessage.Content = new StringContent(data, Encoding.UTF8, "application/json");
                    var response = await client.SendAsync(requestMessage);
                    bool res = false;
                    if (response.IsSuccessStatusCode)
                    {
                        var Res = await response.Content.ReadAsStringAsync();
                        res = "true".Equals(Res);                        
                    }

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

        public async Task<eReturnClient> SendClientAsync(ClientNew pC) => await Send1CClientAsync(pC);//DataSync1C.Send1CClientAsync(pC);

        public async Task Send1CClientAsync()
        {
            DateTime Ldc, Today = DateTime.Now.Date;

            try
            {
                Ldc = db.GetConfig<DateTime>("LastDaySendClient");
            }
            catch { Ldc = Today.AddDays(-10); }

            try
            {
                if (Ldc == default(DateTime) || Ldc < Today.AddDays(-5))
                    Ldc = Today.AddDays(-5);
                Ldc = Ldc.AddDays(1);
                while (Ldc < Today)
                {
                    using var ldb = new WDB_SQLite(Ldc);
                    IEnumerable<ClientNew> Cl = ldb.GetClientNewNotSend();
                    bool Res = true;
                    foreach (var el in Cl)
                    {
                        eReturnClient res = await SendClientAsync(el);
                        if (res == eReturnClient.Ok || res == eReturnClient.ErrorCardIsUse || res == eReturnClient.ErrorCardIsAlreadyPresent)
                            ldb.SetConfirmClientNew(el);
                        else
                            Res = false;
                    }

                    if (Res)
                        db.SetConfig<DateTime>("LastDaySendClient", Ldc);
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

        public bool GetClientOrder1C(string pNumberOrder)
        {
            Task.Run(async () =>
            {
                try
                {
                    string s = "ПСЮ00000000";
                    pNumberOrder = pNumberOrder.Trim();
                    pNumberOrder = s.Substring(0, 11 - pNumberOrder.Length) + pNumberOrder;
                    if (!IsSync(Global.CodeWarehouse)) return;

                    var Order = await GetClientOrder(pNumberOrder);// MsSQL.GetClientOrder(pNumberOrder);
                    if (Order != null && Order.Any())
                    {
                        var curReceipt = bl.GetNewIdReceipt();
                        curReceipt.NumberOrder = pNumberOrder;
                        curReceipt.TypeReceipt = eTypeReceipt.Sale;
                        db.ReplaceReceipt(curReceipt);
                        Thread.Sleep(300);
                        foreach (var el in Order)
                        {
                            bl.AddWaresCode(curReceipt, el.CodeWares, el.CodeUnit, (el.CodeUnit == Global.WeightCodeUnit ? 1000 : 1) * el.Quantity, el.Price, true);
                        }
                    }
                }
                catch (Exception ex)
                { FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, ex); }
            });
            return true;
        }

        public Status<string> GetVerifySMS(string pPhone)
        {
            Status<string> Res = new();
            Task.Run(async () =>
           {
               try
               {
                   string parUrl = Global.Settings.Api + "SMS";
                   var a = new { Phone = pPhone, Company = Global.Settings.CodeTM == Model.eShopTM.Spar ? "1" : "2" };
                   string pBody = a.ToJSON();
                   int parWait = 2000;
                   string parContex = "application/json";
                   string res = null;
                   HttpClient client = new HttpClient();
                   client.Timeout = TimeSpan.FromMilliseconds(parWait);

                   HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, parUrl);

                   if (!string.IsNullOrEmpty(pBody))
                       requestMessage.Content = new StringContent(pBody, Encoding.UTF8, parContex);
                   var response = await client.SendAsync(requestMessage);

                   if (response.IsSuccessStatusCode)
                   {
                       res = await response.Content.ReadAsStringAsync();
                       Res = Newtonsoft.Json.JsonConvert.DeserializeObject<Status<string>>(res);
                       return;
                   }
               }
               catch (Exception e) { new Status<string>(e); return; }

               Res = new Status<string>(-1, "Не отримано код");
           }).Wait();
            return Res;
        }

        public async Task<ExciseStamp> CheckExciseStampAsync(ExciseStamp pES, int pWait = 1000)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMilliseconds(pWait);

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, Global.Api + "CheckExciseStamp");
                string data = pES.ToJson();
                requestMessage.Content = new StringContent(data, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(res))
                    {
                        var Res = //JsonConvert.DeserializeObject
                            Newtonsoft.Json.JsonConvert.DeserializeObject<Status<ExciseStamp>>(res);
                        return Res.Data;
                    }
                }
                else
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = null, Status = eSyncStatus.NoFatalError, StatusDescription = "RequestAsync=>" + response.RequestMessage });
                }
            }
            catch (Exception e)
            { FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, e); }
            return null;
        }

        public ExciseStamp CheckExciseStamp(ExciseStamp pES, int pWait = 1000) => AsyncHelper.RunSync(() => CheckExciseStampAsync(pES, pWait));

        public async Task<Status> SendReceipt(Receipt pR)
        {
            string JSON = pR.ToJson();
            if (Global.IsTest)
            {
                pR.StateReceipt = eStateReceipt.Send;
                db.SetStateReceipt(pR);
                return null;
            }
            try
            {
                HttpClient client = new() { Timeout = TimeSpan.FromMilliseconds(20000) };

                HttpRequestMessage requestMessage = new(HttpMethod.Post, Global.Api + "Receipt");
                string data = pR.ToJson();
                requestMessage.Content = new StringContent(data, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(res))
                    {
                        var Res = Newtonsoft.Json.JsonConvert.DeserializeObject<Status>(res);
                        if (Res.State == 0 ) //&& !Global.Settings.IsSend1C)
                        {
                            pR.StateReceipt = eStateReceipt.Send;
                            db.SetStateReceipt(pR);
                        }
                        FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, $"CodeReceipt=>{pR.CodeReceipt} State=>${Res.State} Text=>{Res.TextState}");
                        return Res;
                    }
                }
                else
                {
                    FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, response.StatusCode.ToString());
                }
            }
            catch (Exception e)
            { FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, e); }
            return null;
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
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Status = eSyncStatus.IncorectProductForDiscount, StatusDescription = "Для даного товару не можливо застосувати знижку 2 категорії." });
                    return false;
                }
                if (db.IsUseBarCode2Category(pBarCode))
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Status = eSyncStatus.IncorectDiscountBarcode, StatusDescription = $"Даний штрихкод =>{pBarCode} другої категорії вже використаний!" });
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
                        ///!!!TMP треба буде перейти на цей механізм
                        var R = await CheckOneTime(new OneTime(pIdReceipt) { CodePS = Cat2First.CodePS, TypeData = Model.eTypeCode.BarCode2Category, CodeData = pBarCode.ToLong() });
                        isGood = !(R == null || !R.status || R.Data == null || !pIdReceipt.Equals(R.Data));

                            //isGood = await DataSync1C.IsUseDiscountBarCode(pBarCode);
                        Global.ErrorDiscountOnLine = 0;
                    }
                    catch (Exception ex)
                    {
                        Global.ErrorDiscountOnLine++;
                        Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "CheckDiscountBarCodeAsync=>" + ex.Message });
                        Global.OnStatusChanged?.Invoke(db.GetStatus());
                    }
                }
                if (isGood)
                {
                    db.ReplaceWaresReceiptPromotion(Cat2);
                    db.InsertBarCode2Cat(Cat2First);
                    db.RecalcHeadReceipt(pIdReceipt);
                    var r = bl.GetReceiptHead(pIdReceipt, true);
                    Global.OnReceiptCalculationComplete?.Invoke(r);
                }
                else
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Status = eSyncStatus.IncorectDiscountBarcode, StatusDescription = $"Даний штрихкод =>{pBarCode} другої категорії вже використаний!" });
                    return false;
                }
            }

            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "CheckDiscountBarCodeAsync=>" + ex.Message + '\n' + new System.Diagnostics.StackTrace().ToString() });
            }
            return true;
        }

        public async Task<ModelMID.Client> GetDiscount(FindClient pFC)
        {
            Client Result = null;
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, Global.Api + "api/GetDiscount");
                string data = pFC.ToJson();
                requestMessage.Content = new StringContent(data, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, $"res=>{res}");
                    if (!string.IsNullOrEmpty(res))
                    {
                        var Res = Newtonsoft.Json.JsonConvert.DeserializeObject<Status<Client>>(res);
                        if (Res?.State == 0)
                        {
                            Result = Res.Data;
                        }
                    }
                }
                else
                {
                    FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, response.StatusCode.ToString());
                }
            }
            catch (Exception e)
            { FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, e); }
            if (Result == null && !string.IsNullOrEmpty(pFC.Phone))
                bl.OnCustomWindow?.Invoke(new CustomWindow(eWindows.Info, $"Клієнта з номером {pFC.Phone} не знайдено в базі!"));
            return Result;
        }

        public async Task<Status<OneTime>> CheckOneTime(OneTime pRC)
        {
            try
            {
                HttpClient client = new();
                client.Timeout = TimeSpan.FromMilliseconds(3000);

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, Global.Api + "CheckOneTime");
                string data = pRC.ToJson();
                requestMessage.Content = new StringContent(data, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(res))
                    {
                        var Res = Newtonsoft.Json.JsonConvert.DeserializeObject<Status<OneTime>>(res);
                        return Res;
                    }
                }
                else return new Status<OneTime>(response.StatusCode);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, e);
                return new Status<OneTime>(e);
            }
            return null;
        }

        public async Task<eReturnClient> Send1CClientAsync(ClientNew pC)
        {
            try
            {
                HttpClient client = new HttpClient
                {
                    Timeout = TimeSpan.FromMilliseconds(2000)
                };

                HttpRequestMessage requestMessage = new(HttpMethod.Post, Global.Api + "Send1CClient");
                string data = pC.ToJson();
                requestMessage.Content = new StringContent(data, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(res))
                    {
                        return (eReturnClient)res.ToInt();
                    }
                }
            }
            catch (Exception e) { FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, e); }
            return eReturnClient.Error;
        }

        public async Task<Dictionary<string, decimal>> GetReceipt1CAsync(DateTime pDT, int pIdWorkplace)
        {
            try
            {
                HttpClient client = new HttpClient
                {
                    Timeout = TimeSpan.FromMilliseconds(5000)
                };

                HttpRequestMessage requestMessage = new(HttpMethod.Post, Global.Api + "GetReceipt1C");
                string data = (new IdReceipt() { IdWorkplace = pIdWorkplace, DTPeriod = pDT }).ToJson();
                requestMessage.Content = new StringContent(data, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(res))
                    {
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, decimal>>(res);
                    }
                }
            }
            catch (Exception e) { FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, e); }
            return null;
        }

        public async Task<IEnumerable<ReceiptWares>> GetClientOrder(string pNumberOrder)
        {
            try
            {
                HttpClient client = new HttpClient
                {
                    Timeout = TimeSpan.FromMilliseconds(5000)
                };
                HttpRequestMessage requestMessage = new(HttpMethod.Post, Global.Api + "GetClientOrder");
                string data = pNumberOrder.ToJson();
                requestMessage.Content = new StringContent(data, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(res))
                    {
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<ReceiptWares>>(res);
                    }
                }
            }
            catch (Exception e) { FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, e); }
            return null;
        }

        public async Task<Result> SetPhoneNumber(SetPhone pSP)
        {
            try
            {
                HttpClient client = new();
                client.Timeout = TimeSpan.FromMilliseconds(3000);

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, Global.Api + "CashRegister/SetPhoneNumber");
                string data = pSP.ToJson();
                requestMessage.Content = new StringContent(data, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(res))
                    {
                        var Res = Newtonsoft.Json.JsonConvert.DeserializeObject<Result>(res);
                        return Res;
                    }
                }
                else return new Result(response.StatusCode);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, e);
                return new Result(e);
            }
            return null;
        }

        public async Task<MidData> LoadDataAsync(int pIdWorkPlace, bool pIsFull, string pPathDB, int pMessageNoMin, bool pIsReloadFull)
        {
            var res = await LoadDataAsync(new InLoadData() { IdWorkPlace = pIdWorkPlace, IsFull = pIsFull, MessageNoMin = pMessageNoMin, IsReloadFull = pIsReloadFull });
            var Res = res?.Data;
            if (!string.IsNullOrEmpty(Res?.PathMid))
            {
                string Zip = Path.Combine(Path.GetDirectoryName(pPathDB), Path.GetFileName(Res.PathMid));
                if (Res.PathMid.StartsWith("http"))
                    Http.LoadFile(Res.PathMid, Zip);
                else
                    File.Copy(Res.PathMid, Zip, true);
                if (File.Exists(Zip))
                {
                    ZipFile.ExtractToDirectory(Zip, Path.GetDirectoryName(pPathDB), true);
                    File.Delete(Zip);
                    FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, $"LoadDataAsync => {Path.Combine(pPathDB, Path.GetFileName(Res.PathMid))}");
                }
                else
                {
                    FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, $"LoadDataAsync => File not found {Path.Combine(pPathDB, Path.GetFileName(Res.PathMid))}");
                    Res = null;
                }
            }
            return Res;
        }
        public async Task<Result<MidData>> LoadDataAsync(InLoadData pData)
        {
            try
            {
                HttpClient client = new()
                {
                    Timeout = TimeSpan.FromMilliseconds(90000)
                };
                HttpRequestMessage requestMessage = new(HttpMethod.Post, Global.Api + "CashRegister/LoadData");

                requestMessage.Content = new StringContent(pData.ToJson(), Encoding.UTF8, "application/json");
                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(res))
                    {
                        var r = Newtonsoft.Json.JsonConvert.DeserializeObject
                            //JsonSerializer.Deserialize
                            <Result<MidData>>(res);
                        return r;
                    }
                }
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, MethodBase.GetCurrentMethod().Name, e);
                return new Result<MidData>(e);
            }
            return null;
        }

        public bool LoadDataSQL(SQLiteMid pD, MidData pMD)
        {
            if (pMD == null)
                return false;
            string MethodName = MethodBase.GetCurrentMethod().Name ?? "DataSync.LoadDataSQL";
            try
            {
                pD.ReplaceWaresLink(pMD.WaresLink);
                FileLogger.WriteLogMessage(this, MethodName, $"Write ReplaceWaresLink => {pMD.WaresLink?.Count()}");

                pD.ReplaceWaresWarehouse(pMD.WaresWarehouse);
                FileLogger.WriteLogMessage(this, MethodName, $"Write ReplaceWaresWarehouse => {pMD.WaresWarehouse?.Count()}");

                pD.ReplaceClientData(pMD.ClientData);
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetClientData => {pMD.ClientData?.Count()}");

                pD.ReplacePrice(pMD.Price);
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimPrice => {pMD.Price?.Count()}");

                pD.ReplaceUser(pMD.User);
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetUser => {pMD.User?.Count()}");

                pD.ReplaceSalesBan(pMD.SalesBan);
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlSalesBan => {pMD.SalesBan?.Count()}");

                try
                {
                    if (pD != null) pD.BeginTransaction();

                    pD.ReplacePromotionSaleGift(pMD.PromotionSaleGift);
                    FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetPromotionSaleGift => {pMD.PromotionSaleGift?.Count()}");

                    if (pMD.PromotionSaleGroupWares != null)
                    {
                        pD.ReplacePromotionSaleGroupWares(pMD.PromotionSaleGroupWares);
                        FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetPromotionSaleGroupWares => {pMD.PromotionSaleGroupWares?.Count()}");
                    }

                    pD.ReplacePromotionSale(pMD.PromotionSale);
                    FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetPromotionSale => {pMD.PromotionSale?.Count()}");

                    pD.ReplacePromotionSaleFilter(pMD.PromotionSaleFilter);
                    FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetPromotionSaleFilter => {pMD.PromotionSaleFilter?.Count()}");

                    pD.ReplacePromotionSaleData(pMD.PromotionSaleData);
                    FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetPromotionSaleData => {pMD.PromotionSaleData?.Count()}");

                    pD.ReplacePromotionSaleDealer(pMD.PromotionSaleDealer);
                    FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetPromotionSaleDealer =>{pMD.PromotionSaleDealer?.Count()}");


                    pD.ReplacePromotionSale2Category(pMD.PromotionSale2Category);
                    FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetPromotionSale2Category => {pMD.PromotionSale2Category?.Count()}");

                    pD.CommitTransaction();
                }
                catch (Exception e)
                {
                    pD.RollbackTransaction();
                    FileLogger.WriteLogMessage(this, MethodName, e);
                    throw;
                }

                pD.ReplaceMRC(pMD.MRC);
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetMRC => {pMD.MRC?.Count()}");

                pD.ReplaceClient(pMD.Client);
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimClient => {pMD.Client?.Count()}");

                pD.ReplaceWares(pMD.Wares);
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimWares => {pMD.Wares?.Count()}");

                pD.ReplaceAdditionUnit(pMD.AdditionUnit);
                FileLogger.WriteLogMessage(this, MethodName, $"SqlGetDimAdditionUnit => {pMD.AdditionUnit?.Count()}");

                pD.ReplaceBarCode(pMD.Barcode);
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimBarCode => {pMD.Barcode?.Count()}");

                pD.ReplaceUnitDimension(pMD.UnitDimension);
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimUnitDimension => {pMD.UnitDimension?.Count()}");

                pD.ReplaceGroupWares(pMD.GroupWares);
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimGroupWares => {pMD.GroupWares?.Count()}");

                pD.ReplaceTypeDiscount(pMD.TypeDiscount);
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimTypeDiscount => {pMD.TypeDiscount?.Count()}");

                pD.ReplaceFastGroup(pMD.FastGroup);
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimFastGroup => {pMD.FastGroup?.Count()}");

                pD.ReplaceFastWares(pMD.FastWares);
                FileLogger.WriteLogMessage(this, MethodName, $"Write SqlGetDimFastWares => {pMD.FastWares?.Count()}");
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, MethodName, e);
                return false;
            }
            return true;
        }
    }
}
