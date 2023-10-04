using System;
using System.Collections.Generic;
using System.Data;
//using DB_SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dapper;
using Microsoft.VisualBasic;
using ModelMID;
using ModelMID.DB;
using Utils;
using static Dapper.SqlMapper;

namespace SharedLib
{
    public partial class WDB_SQLite : IDisposable
    {
        public string Version = "SQLite 0.0.2";
        public SQL db;
        public eDBStatus DBStatus = eDBStatus.NotDefine;
        private static bool IsFirstStart = true;
        private bool isDisposed;
        private DateTime DT = DateTime.Today;

        private string Connect = null;
        private string ConfigFile { get { return Path.Combine(ModelMID.Global.PathDB, "config.db"); } }
        private string LastMidFile = null;
        private string MidFile { get { return string.IsNullOrEmpty(Connect) ? (string.IsNullOrEmpty(LastMidFile) ? GetMIDFile() : LastMidFile) : Connect; } }

        private string GetReceiptFile(DateTime pDT) { return Path.Combine(ModelMID.Global.PathDB, $"{pDT:yyyyMM}", $"Rc_{ModelMID.Global.IdWorkPlace}_{pDT:yyyyMMdd}.db"); }

        private string ReceiptFile { get { return GetReceiptFile(DT); } }

        public string GetMIDFile(DateTime pD = default, bool pTmp = false)
        {
            DateTime Date = pD == default ? DateTime.Today : pD;
            return Path.Combine(Global.PathDB, $"{Date:yyyyMM}", $"MID_{Date:yyyyMMdd}{(pTmp ? "_tmp" : "")}.db");
        }

        public WDB_SQLite(DateTime parD = default(DateTime), string pConnect = null, bool pIsUseOldDB = true, bool pIsCreateMidFile = false)
        {
            Connect = pConnect;

            DT = parD != default(DateTime) ? parD.Date : DateTime.Today.Date;
            InitSQL();
            if (IsFirstStart)
            {
                if (!UpdateDB(ref pIsUseOldDB))
                {
                    DBStatus = eDBStatus.ErrorUpdateDB;
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Status = eSyncStatus.Error, StatusDescription = $"DB={DBStatus}" });
                    return;
                };

                //видалення старих баз
                var d = DateTime.Now.Date.AddDays(-40);
                while (d < DateTime.Now.Date.AddDays(-14))
                {
                    try
                    {
                        string file = GetMIDFile(d);
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                    catch { }
                    d = d.AddDays(1);
                }

            }


            if (!File.Exists(ConfigFile))
            {
                db = new SQLite(ConfigFile);
                db.ExecuteNonQuery(SqlCreateConfigTable);
                db.Close();
            }

            if (!File.Exists(ReceiptFile))
            {
                var receiptFilePath = Path.GetDirectoryName(ReceiptFile);
                if (!Directory.Exists(receiptFilePath))
                    Directory.CreateDirectory(receiptFilePath);
                //Створюємо щоденну табличку з чеками.
                var db = new SQLite(ReceiptFile);
                db.ExecuteNonQuery(SqlCreateReceiptTable);
                db.Close();
                db = null;
            }

            //if (!File.Exists(MidFile))
            //{
            if (pIsCreateMidFile)
            {
                var db = new SQLite(MidFile);
                db.ExecuteNonQuery(SqlCreateMIDTable);
                db.Close();
                db = null;
            }
            else
            if (pIsUseOldDB)
            {
                db = new SQLite(ConfigFile);
                var LFDB = GetConfig<DateTime>("Load_Full").Date;
                if (LFDB < DateTime.Now.AddDays(-10))
                    LFDB = DateTime.Now.Date;

                db = null;
                do
                {
                    var vLastMidFile = GetMIDFile(LFDB);
                    if (!string.IsNullOrEmpty(vLastMidFile) && File.Exists(vLastMidFile))
                        LastMidFile = vLastMidFile;
                    LFDB = LFDB.AddDays(-1);
                } while (LastMidFile == null && LFDB > DateTime.Now.AddDays(-10));
            }
            //}

            db = new SQLite(ReceiptFile);
            if (File.Exists(MidFile))
                db.ExecuteNonQuery("ATTACH '" + MidFile + "' AS mid");
            db.ExecuteNonQuery("ATTACH '" + ConfigFile + "' AS con");

            db.ExecuteNonQuery("PRAGMA synchronous = EXTRA;");
            db.ExecuteNonQuery("PRAGMA journal_mode = DELETE;");
            db.ExecuteNonQuery("PRAGMA wal_autocheckpoint = 5;");
            DBStatus = eDBStatus.Ok;
        }
        ~WDB_SQLite()
        {
            //Close();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;
            if (disposing)
            {
                Close();
            }
            // free native resources if there are any.

            isDisposed = true;
        }


        /// <summary>
        /// Для блокування деяких асинхронних операцій по касі.
        /// </summary>
        private Dictionary<int, object> WorkplaceIdLockers = new Dictionary<int, object>();

        public object GetObjectForLockByIdWorkplace(int parIdWorkplace)
        {
            if (!WorkplaceIdLockers.ContainsKey(parIdWorkplace) || WorkplaceIdLockers[parIdWorkplace] == null)
                WorkplaceIdLockers[parIdWorkplace] = new object();
            return WorkplaceIdLockers[parIdWorkplace];
        }


        private new bool InitSQL()
        {
            return true;
        }

        public T GetConfig<T>(string pStr, SQL pDB = null)
        {
            string SqlConfig = "SELECT Data_Var  FROM CONFIG  WHERE UPPER(Name_Var) = UPPER(trim(@NameVar));";
            if (pDB == null) pDB = db;
            return pDB.ExecuteScalar<object, T>(SqlConfig, new { NameVar = pStr });
        }

        public DateTime GetLastNotSendReceipt()
        {
            string SqlGetDateFirstNotSendReceipt = "select ifnull(min(date_receipt), datetime('now','localtime'))   from receipt wr where state_receipt = 2";
            var Ldc = GetConfig<DateTime>("LastDaySend");
            if (Ldc != DateTime.Now.Date)
                return Ldc;

            return db.ExecuteScalar<DateTime>(SqlGetDateFirstNotSendReceipt);

        }

        public DateTime GetLastUpdateDirectory()
        {
            var strTDF = GetConfig<DateTime>("Load_Full");
            var strTDU = GetConfig<DateTime>("Load_Update");
            if (strTDU < strTDF)
                return strTDF;
            else
                return strTDU;
        }

        public Task RecalcPriceAsync(IdReceiptWares pIdReceiptWares)
        {
            return Task.Run(() => RecalcPrice(pIdReceiptWares)).ContinueWith(async res =>
            {
                if (await res)
                {
                    //Console.WriteLine(OnReceiptCalculationComplete != null);
                    var r = //ViewReceiptWares(new IdReceiptWares(pIdReceiptWares, 0), true);//вертаємо весь чек.
                    ViewReceipt(pIdReceiptWares, true);
                    Global.OnReceiptCalculationComplete?.Invoke(r);
                    if (r?.Wares == null || r?.Wares?.Count() == 0)
                        return;
                    var parW = r.Wares.Last();
                    if (parW != null)
                    {
                        var SumAll = r.Wares.Sum(d => d.Sum - d.SumDiscount);
                        _ = VR.SendMessageAsync(parW.IdWorkplace, parW.NameWares, parW.Articl, parW.Quantity, parW.Sum, VR.eTypeVRMessage.AddWares, SumAll);
                    }
                }
            });
        }

        public bool RecalcPrice(IdReceiptWares pIdReceiptWares)
        {
            var startTime = System.Diagnostics.Stopwatch.StartNew();
            var State = GetStateReceipt(pIdReceiptWares);
            if (State != eStateReceipt.Prepare)
                return true;
            var Type = GetTypeReceipt(pIdReceiptWares);
            if (Type == eTypeReceipt.Refund)
                return true;
            lock (GetObjectForLockByIdWorkplace(pIdReceiptWares.IdWorkplace))
            {
                try
                {
                    var RH = ViewReceipt(pIdReceiptWares);
                    ParameterPromotion par;
                    var InfoClient = GetInfoClientByReceipt(pIdReceiptWares);
                    if (InfoClient.Count() == 1)
                        par = InfoClient.First();
                    else
                        par = new ParameterPromotion();

                    //par.BirthDay = DateTime.Now.Date; Test
                    par.CodeWarehouse = Global.CodeWarehouse;
                    par.Time = Convert.ToInt32(RH.DateReceipt.ToString("HHmm"));
                    par.CodeDealer = Global.DefaultCodeDealer;

                    var r = ViewReceiptWares(pIdReceiptWares);

                    foreach (var RW in r)
                    {
                        var MPI = GetMinPriceIndicative((IdReceiptWares)RW);
                        par.CodeWares = RW.CodeWares;
                        par.Quantity = RW.Quantity;
                        var Res = GetPrice(par);

                        if (Res != null && RW.ParPrice1 != 999999 && (Res.Priority > 0 || string.IsNullOrEmpty(RW.BarCode2Category)))//Не перераховуємо для  Сигарет s для 2 категорії окрім пріоритет 1
                        {
                            RW.Price = MPI.GetPrice(Res.Price, Res.IsIgnoreMinPrice == 0, Res.CodePs > 0);
                            RW.TypePrice = MPI.typePrice;
                            RW.ParPrice1 = Res.CodePs;
                            RW.ParPrice2 = (long)Res.TypeDiscont;
                            RW.ParPrice3 = Res.Data;
                            RW.Priority = Res.Priority;
                        }
                        //Якщо друга категорія - перераховуємо на основі Роздрібної ціни.
                        if (Res.Priority == 0 && !string.IsNullOrEmpty(RW.BarCode2Category) && RW.BarCode2Category.Length == 13)
                            RW.Price = RW.PriceDealer;

                        ReplaceWaresReceipt(RW);
                    }
                    GetPricePromotionKit(pIdReceiptWares, pIdReceiptWares.CodeWares);
                    RecalcHeadReceipt(pIdReceiptWares);
                    startTime.Stop();
                    Console.WriteLine($"RecalcPrice=>{startTime.Elapsed}  {r?.Count()}");
                    /*foreach (var RW in r)
                        Console.WriteLine($"{RW.NameWares}");*/

                    return true;
                }
                catch (Exception ex)
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(pIdReceiptWares.IdWorkplace), Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "RecalcPrice=>" + ex.Message + '\n' + new System.Diagnostics.StackTrace().ToString() });
                    return false;
                }
            }
        }
        /// <summary>
        /// Розраховуємо знижки по наборах
        /// Можливо зумію це зробити колись на рівні БД
        /// </summary>
        /// <param name="parIdReceipt"></param>
        /// <returns></returns>
        public bool GetPricePromotionKit(IdReceipt parIdReceipt, int parCodeWares)
        {
            if (parCodeWares > 0 && !IsWaresInPromotionKit(parCodeWares))
                return true;

            var varRes = new List<WaresReceiptPromotion>();
            var par = new ParamPricePromotionKit(parIdReceipt, ModelMID.Global.CodeWarehouse);
            var r = db.Execute<ParamPricePromotionKit, PromotionWaresKit>(SqlGetPricePromotionKit, par);
            int NumberGroup = 0;
            decimal Quantity = 0, AddQuantity = 0;
            Int64 CodePS = 0;
            var RW = ViewReceiptWares(parIdReceipt);
            foreach (var el in r)//цикл по Можливим позиціям з знижкою.
            {
                if (el.CodePS != CodePS || el.NumberGroup != NumberGroup)
                {
                    Quantity = el.Quantity;
                    CodePS = el.CodePS;
                    NumberGroup = el.NumberGroup;
                }
                if (Quantity > 0) // Надаєм знижку на інші позиції набору.
                {
                    var varQuantityReceipt = RW.Where(e => e.CodeWares == el.CodeWares).Sum(e => e.Quantity);
                    // Можливо поправить ситуацію з неправильно заведенеми акцічми.
                    var varQuantityUsed = varRes.Where(e => e.CodeWares == el.CodeWares /*&& e.NumberGroup==el.NumberGroup*/).Sum(e => e.Quantity);
                    if (varQuantityReceipt - varQuantityUsed > 0) //Якщо ще можемо дати знижку на позицію
                    {
                        if (varQuantityReceipt - varQuantityUsed >= Quantity)
                        {
                            AddQuantity = Quantity;
                            Quantity = 0;
                        }
                        else
                        {
                            AddQuantity = varQuantityReceipt - varQuantityUsed;
                            Quantity -= varQuantityReceipt - varQuantityUsed;
                        }
                        decimal vPrice = el.Price;
                        if (el.TypeDiscount == eTypeDiscount.PercentDiscount)
                        {
                            var Price = RW.Where(e => e.CodeWares == el.CodeWares).Sum(e => e.Price);
                            vPrice = Price * el.DataDiscount / 100m;
                        }

                        var RWP = new WaresReceiptPromotion(parIdReceipt) { CodeWares = el.CodeWares, Quantity = AddQuantity, Price = vPrice, CodePS = el.CodePS, NumberGroup = el.NumberGroup };
                        varRes.Add(RWP);
                    }
                }

            }
            DeleteWaresReceiptPromotion(parIdReceipt);
            if (varRes.Count > 0)
                ReplaceWaresReceiptPromotion(varRes);

            return true;

        }

        public bool CopyWaresReturnReceipt(IdReceipt parIdReceipt, bool parIsCurrentDay = true)
        {
            string SqlCopyWaresReturnReceipt = (parIsCurrentDay ? this.SqlCopyWaresReturnReceipt.Replace("RRC.", "RC.") : this.SqlCopyWaresReturnReceipt);
            return (this.db.ExecuteNonQuery(SqlCopyWaresReturnReceipt, parIdReceipt) > 0);
        }



        public IEnumerable<ReceiptWares> GetWaresFromFastGroup(int parCodeFastGroup)
        {
            return FindWares(null, null, 0, 0, parCodeFastGroup);
        }

        public IEnumerable<ReceiptWares> GetBags()
        {
            return FindWares(null, null, 0, 0, Global.CodeFastGroupBag);
        }

        public void Close(bool isWait = false)
        {
            db?.Close(isWait);
        }

        ///////////////////////////////////////////////////////////////////
        /// Переробляю через відкриття закриття конекта.
        ///////////////////////////////////////////////////////////////////
        public bool SetConfig<T>(string parName, T parValue, SQL pDB = null)
        {
            using (var DB = new SQLite(ConfigFile))
            {
                string SqlReplaceConfig = "replace into CONFIG  (Name_Var,Data_Var,Type_Var) values (@NameVar,@DataVar,@TypeVar);";

                if (pDB == null)
                    DB.ExecuteNonQuery<object>(SqlReplaceConfig, new { NameVar = parName, DataVar = parValue, @TypeVar = parValue.GetType().ToString() });
                else
                    pDB.ExecuteNonQuery<object>(SqlReplaceConfig, new { NameVar = parName, DataVar = parValue, @TypeVar = parValue.GetType().ToString() });
            }
            return true;
        }

        public bool ReplaceWorkPlace(IEnumerable<WorkPlace> parData)
        {
            string SqlReplaceWorkPlace = @"replace into WORKPLACE(ID_WORKPLACE, NAME, Terminal_GUID, Video_Camera_IP, Video_Recorder_IP, Type_POS, Code_Warehouse, CODE_DEALER, Prefix, DNSName, TypeWorkplace, SettingsEx) values
            (@IdWorkplace, @Name, @StrTerminalGUID, @VideoCameraIP, @VideoRecorderIP, @TypePOS, @CodeWarehouse, @CodeDealer, @Prefix, @DNSName, @TypeWorkplace, @SettingsEx);";

            using (var DB = new SQLite(ConfigFile))
            {
                DB.BulkExecuteNonQuery<WorkPlace>(SqlReplaceWorkPlace, parData);
            }
            return true;
        }

        /// <summary>
        /// Повертає знайдений товар/товари
        /// </summary>
        public virtual IEnumerable<ReceiptWares> FindWares(string parBarCode = null, string parName = null, int parCodeWares = 0, int parCodeUnit = 0, int parCodeFastGroup = 0, int parArticl = -1, int parOffSet = -1, int parLimit = 10)
        {
            string SqlGetPricesMRC = "select CODE_WARES as CodeWares,PRICE as Price,Type_Wares as TypeWares from MRC where CODE_WARES = @CodeWares order by PRICE desc";
            var Lim = parOffSet >= 0 ? $" limit {parLimit} offset {parOffSet}" : "";
            var Wares = this.db.Execute<object, ReceiptWares>(SqlFoundWares + Lim, new { CodeWares = parCodeWares, CodeUnit = parCodeUnit, BarCode = parBarCode, NameUpper = (parName == null ? null : "%" + parName.ToUpper().Replace(" ", "%") + "%"), CodeDealer = ModelMID.Global.DefaultCodeDealer, CodeFastGroup = parCodeFastGroup, Articl = parArticl });
            foreach (var el in Wares)
            {
                el.AdditionalWeights = db.Execute<object, decimal>(SqlAdditionalWeightsWares, new { CodeWares = el.CodeWares });
                el.Prices = db.Execute<object, MRC>(SqlGetPricesMRC, new { CodeWares = el.CodeWares });
            }
            return Wares;
        }



        public bool InsertWeight(Object parWeight)
        {
            string SqlInsertWeight = "insert into Weight(BarCode, Weight, STATUS) values(@BarCode, @Weight, @Status);";
            using (var DB = new SQLite(ConfigFile))
            {
                return DB.ExecuteNonQuery<Object>(SqlInsertWeight, parWeight) > 0;
            }
        }

        public bool InsertAddWeight(AddWeight parAddWeight)
        {
            string SqlInsertAddWeight = "insert into ADD_WEIGHT(CODE_WARES, CODE_UNIT, WEIGHT) values(@CodeWares, @CodeUnit, @Weight);";
            using (var DB = new SQLite(MidFile))
            {
                return DB.ExecuteNonQuery<AddWeight>(SqlInsertAddWeight, parAddWeight) > 0;
            }
        }
        ////////////////// RC

        /// <summary>
        /// Вертає код Чека в зазначеному періоді
        /// </summary>
        /// <param name="parCodePeriod">Код періоду (201401 - січень 2014)</param>
        /// <returns>
        ///Повертає код чека
        ///</returns>
        public IdReceipt GetNewReceipt(IdReceipt pIdReceipt)
        {
            string SqlGetNewReceipt = @"
INSERT OR ignore into GEN_WORKPLACE(ID_WORKPLACE, CODE_PERIOD, CODE_RECEIPT) values(@IdWorkplace, @CodePeriod, @CodeReceipt);
            update GEN_WORKPLACE set CODE_RECEIPT = CODE_RECEIPT + 1 where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod;
            --insert into receipt(id_workplace, code_period, code_receipt) values(@IdWorkplace, @CodePeriod, (select CODE_RECEIPT from GEN_WORKPLACE where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod));
            select CODE_RECEIPT from GEN_WORKPLACE where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod;";

            string SqlGetNewReceipt2 = @"insert into receipt (id_workplace, code_period, code_receipt) values (@IdWorkplace,@CodePeriod,@CodeReceipt);";
            using (var DB = new SQLite(ConfigFile))
            {
                lock (GetObjectForLockByIdWorkplace(pIdReceipt.IdWorkplace))
                {
                    if (pIdReceipt.CodePeriod == 0)
                        pIdReceipt.CodePeriod = Global.GetCodePeriod();
                    pIdReceipt.CodeReceipt = DB.ExecuteScalar<IdReceipt, int>(SqlGetNewReceipt, pIdReceipt);

                    using (var DB_R = new SQLite(ReceiptFile))
                    {
                        DB_R.ExecuteNonQuery<IdReceipt>(SqlGetNewReceipt2, pIdReceipt);
                    }
                }
                return pIdReceipt;
            }
        }

        public bool ReplaceReceipt(Receipt parReceipt)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<Receipt>(SqlReplaceReceipt, parReceipt) > 0;
            }
        }

        public bool CloseReceipt(Receipt parReceipt)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                lock (GetObjectForLockByIdWorkplace(parReceipt.IdWorkplace))
                {
                    return DB.ExecuteNonQuery<Receipt>(SqlCloseReceipt, parReceipt) > 0;
                }
            }
        }

        /// <summary>
        /// Повертає фактичну кількість після вставки(добавляє до текучої кількості - -1 якщо помилка;
        /// </summary>
        /// <param name="parParameters"></param>
        /// <returns></returns>
        public bool AddWares(ReceiptWares parReceiptWares)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<ReceiptWares>(SqlInsertWaresReceipt, parReceiptWares) > 0 /*&& RecalcHeadReceipt((IdReceipt)parReceiptWares)*/;
            }
        }

        public bool ReplaceWaresReceipt(ReceiptWares parReceiptWares)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<ReceiptWares>(SqlReplaceWaresReceipt, parReceiptWares) > 0;
            }
        }

        public bool UpdateQuantityWares(ReceiptWares parReceiptWares)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                lock (GetObjectForLockByIdWorkplace(parReceiptWares.IdWorkplace))
                {
                    return DB.ExecuteNonQuery(SqlUpdateQuantityWares, parReceiptWares) > 0 /*&& RecalcHeadReceipt(parParameters)*/;
                }
            }
        }

        public bool DeleteReceiptWares(ReceiptWares parIdReceiptWares)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<IdReceiptWares>(SqlDeleteReceiptWares, parIdReceiptWares) > 0 /*&& RecalcHeadReceipt(parParameters)*/;
            }
        }

        public bool RecalcHeadReceipt(IdReceipt parReceipt)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<IdReceipt>(SqlRecalcHeadReceipt, parReceipt) > 0;
            }
        }

        public bool DeleteWaresReceiptPromotion(IdReceipt parIdReceipt)
        {
            string SqlDeleteWaresReceiptPromotion = @"delete from  WARES_RECEIPT_PROMOTION 
   where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt and CODE_PS<>999999 and (BARCODE_2_CATEGORY is null or length(BARCODE_2_CATEGORY)=0 );";

            using (var DB = new SQLite(ReceiptFile))
            {
                DB.ExecuteNonQuery<IdReceipt>(SqlDeleteWaresReceiptPromotion, parIdReceipt);
            }
            return true;
        }

        public bool ReplaceWaresReceiptPromotion(IEnumerable<WaresReceiptPromotion> parData)
        {
            var Sql = @" update WARES_RECEIPT 
set price = ifnull( (select  max( (0.000+wrp.sum)/wrp.QUANTITY)
 from WARES_RECEIPT_PROMOTION wrp
where wrp.id_workplace=@IdWorkplace and  wrp.code_period =@CodePeriod and  wrp.code_receipt=@CodeReceipt and  wrp.code_wares=WARES_RECEIPT.code_wares and Type_Discount=@TypeDiscount) 
,price)
, sum =quantity* ifnull( (select  max( (0.000+wrp.sum)/wrp.QUANTITY)
 from WARES_RECEIPT_PROMOTION wrp
where wrp.id_workplace=@IdWorkplace and  wrp.code_period =@CodePeriod and  wrp.code_receipt=@CodeReceipt and  wrp.code_wares=WARES_RECEIPT.code_wares and Type_Discount=@TypeDiscount) 
,price)
,Type_Price=9
,par_price_1=999999
, PRIORITY=1
, PRICE_DEALER= ifnull((select  max( (0.000+wrp.sum)/wrp.QUANTITY)
 from WARES_RECEIPT_PROMOTION wrp
where wrp.id_workplace=@IdWorkplace and  wrp.code_period =@CodePeriod and  wrp.code_receipt=@CodeReceipt and  wrp.code_wares=WARES_RECEIPT.code_wares and Type_Discount=@TypeDiscount) 
,PRICE_DEALER)
where WARES_RECEIPT.id_workplace=@IdWorkplace and  WARES_RECEIPT.code_period =@CodePeriod and  WARES_RECEIPT.code_receipt=@CodeReceipt and WARES_RECEIPT.code_wares=@CodeWares
and @TypeDiscount=11; ";
            using (var DB = new SQLite(ReceiptFile))
            {
                DB.BulkExecuteNonQuery<WaresReceiptPromotion>(SqlReplaceWaresReceiptPromotion, parData);
                DB.BulkExecuteNonQuery<WaresReceiptPromotion>(Sql, parData);
            }
            return true;
        }


        public bool ReplacePayment(Payment pData, bool pIsDel = false)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                if (pIsDel)
                {
                    string Sql = @"delete from payment where  id_workplace=@IdWorkplace and id_workplace_pay = @IdWorkplacePay and  code_period =@CodePeriod and  code_receipt=@CodeReceipt and Type_Pay=@TypePay";
                    DB.ExecuteNonQuery<Payment>(Sql, pData);
                }
                DB.ExecuteNonQuery<Payment>(SqlReplacePayment, pData);
            }
            return true;
        }

        public bool ReplacePayments(IEnumerable<Payment> parData)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                if (parData != null && parData.Count() > 0)
                {
                    if (parData.Count() == 1)//Костиль через проблеми з мультипоточністю BD
                        ReplacePayment(parData.First());
                    else
                        DB.BulkExecuteNonQuery<Payment>(SqlReplacePayment, parData);
                }
            }
            return true;
        }

        public bool SetStateReceipt(Receipt parReceipt)
        {
            string SqlSetStateReceipt = @"update receipt  set STATE_RECEIPT = @StateReceipt where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod and CODE_RECEIPT = @CodeReceipt";
            using (var DB = new SQLite(ReceiptFile))
            {
                DB.ExecuteNonQuery<Receipt>(SqlSetStateReceipt, parReceipt);
            }
            return true;
        }

        public bool InsertBarCode2Cat(WaresReceiptPromotion parWRP)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                DB.ExecuteNonQuery<WaresReceiptPromotion>(SqlInsertBarCode2Cat, parWRP);
            }
            return true;
        }

        public bool SetRefundedQuantity(ReceiptWares parReceiptWares)
        {
            string SqlSetRefundedQuantity = @"
update wares_receipt set Refunded_Quantity = Refunded_Quantity + @Quantity
  where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod and CODE_RECEIPT = @CodeReceipt and Code_wares = @CodeWares;";
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<ReceiptWares>(SqlSetRefundedQuantity, parReceiptWares) > 0;
            }
        }

        public bool InsertReceiptEvent(IEnumerable<ReceiptEvent> parRE)
        {
            string SqlInsertReceiptEvent = @"
insert into RECEIPT_Event(
    ID_WORKPLACE, CODE_PERIOD, CODE_RECEIPT, CODE_WARES, CODE_UNIT,
    ID_GUID,
    Mobile_Device_Id_GUID,
    Product_Name,
    Event_Type,
    Event_Name,
    Product_Weight,
    Product_Confirmed_Weight,
    UserId_GUID,
    User_Name,
    Created_At,
    Resolved_At,
    Refund_Amount,
    Fiscal_Number,
    Payment_Type,
    Total_Amount
    ) VALUES
    (@IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @CodeUnit,
    @IDGUID,
    @MobileDeviceIdGUID,
    @ProductName,
    @EventType,
    @EventName,
    @ProductWeight,
    @ProductConfirmedWeight,
    @UserIdGUID,
    @UserName,
    @CreatedAt,
    @ResolvedAt,
    @RefundAmount,
    @FiscalNumber,
    @PaymentType,
    @TotalAmount);";
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.BulkExecuteNonQuery<ReceiptEvent>(SqlInsertReceiptEvent, parRE) > 0;
            }
        }

        public bool DeleteReceiptEvent(IdReceipt parIdReceipt)
        {
            string SqlDeleteReceiptEvent = @"delete from RECEIPT_Event where id_workplace = @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt; --and EVENT_TYPE > 0;";

            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<IdReceipt>(SqlDeleteReceiptEvent, parIdReceipt) > 0;
            }
        }

        public bool FixWeight(ReceiptWares parIdReceipt)
        {
            string SqlSetFixWeight = @"update wares_receipt set Fix_Weight = @FixWeight, Fix_Weight_Quantity = @FixWeightQuantity
  where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod and CODE_RECEIPT = @CodeReceipt  and Code_wares = @CodeWares;";
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<ReceiptWares>(SqlSetFixWeight, parIdReceipt) > 0;
            }
        }
        ////////////////////////// MID

        public bool CreateMIDTable()
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.ExecuteNonQuery(SqlCreateMIDTable);
            }
            return true;
        }
        public bool CreateMIDIndex()
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.ExecuteNonQuery(SqlCreateMIDIndex);
            }
            return true;
        }

        public bool ReplaceUnitDimension(IEnumerable<UnitDimension> parData)
        {
            string SqlReplaceUnitDimension = "replace into UNIT_DIMENSION(CODE_UNIT, NAME_UNIT, ABR_UNIT) values(@CodeUnit, @NameUnit, @AbrUnit);";
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<UnitDimension>(SqlReplaceUnitDimension, parData);
            }
            return true;
        }

        public bool ReplaceGroupWares(IEnumerable<GroupWares> parData)
        {
            string SqlReplaceGroupWares = @" replace into  GROUP_WARES(CODE_GROUP_WARES, CODE_PARENT_GROUP_WARES, NAME) values (@CodeGroupWares, @CodeParentGroupWares, @Name);";

            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<GroupWares>(SqlReplaceGroupWares, parData);
            }
            return true;
        }

        public bool ReplaceWares(IEnumerable<Wares> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<Wares>(SqlReplaceWares, parData);
            }
            return true;
        }

        public bool ReplaceAdditionUnit(IEnumerable<AdditionUnit> parData)
        {
            string SqlReplaceAdditionUnit = @"replace into  Addition_Unit(CODE_WARES, CODE_UNIT, COEFFICIENT, DEFAULT_UNIT, WEIGHT, WEIGHT_NET)
              values(@CodeWares, @CodeUnit, @Coefficient, @DefaultUnit, @Weight, @WeightNet);";
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<AdditionUnit>(SqlReplaceAdditionUnit, parData);
            }
            return true;
        }

        public bool ReplaceBarCode(IEnumerable<Barcode> parData)
        {
            string SqlReplaceBarCode = "replace into  Bar_Code(CODE_WARES, CODE_UNIT, BAR_CODE) values(@CodeWares, @CodeUnit, @BarCode);";
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<Barcode>(SqlReplaceBarCode, parData);
            }
            return true;
        }
        public bool ReplacePrice(IEnumerable<Price> parData)
        {
            string SqlReplacePrice = "replace into PRICE(CODE_DEALER, CODE_WARES, PRICE_DEALER) values(@CodeDealer, @CodeWares, @PriceDealer);";
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<Price>(SqlReplacePrice, parData);
            }
            return true;
        }

        public bool ReplaceTypeDiscount(IEnumerable<TypeDiscount> parData)
        {
            string SqlReplaceTypeDiscount = @"replace into TYPE_DISCOUNT(TYPE_DISCOUNT, NAME, PERCENT_DISCOUNT) values(@CodeTypeDiscount, @Name, @PercentDiscount);";
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<TypeDiscount>(SqlReplaceTypeDiscount, parData);
            }
            return true;
        }

        public bool ReplaceClient(IEnumerable<Client> parData)
        {
            string SqlReplaceClient = @"replace into CLIENT(CODE_CLIENT, NAME_CLIENT, TYPE_DISCOUNT, PHONE, Phone_Add, PERCENT_DISCOUNT, BARCODE, STATUS_CARD, view_code, BirthDay)
             values(@CodeClient, @NameClient, @TypeDiscount, @MainPhone, @PhoneAdd, @PersentDiscount, @BarCode, @StatusCard, @ViewCode, @BirthDay)";
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<Client>(SqlReplaceClient, parData);
            }
            return true;
        }

        public bool ReplaceFastGroup(IEnumerable<FastGroup> parData)
        {
            string SqlReplaceFastGroup = @"replace into FAST_GROUP(CODE_UP, Code_Fast_Group, Name) values(@CodeUp, @CodeFastGroup, @Name);";
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<FastGroup>(SqlReplaceFastGroup, parData);
            }
            return true;
        }
        public bool ReplaceFastWares(IEnumerable<FastWares> parData)
        {
            string SqlReplaceFastWares = "replace into FAST_WARES(Code_Fast_Group, Code_wares, Order_Wares) values(@CodeFastGroup, @CodeWares, @OrderWares);";
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<FastWares>(SqlReplaceFastWares, parData);
            }
            return true;
        }

        public bool ReplacePromotionSale(IEnumerable<PromotionSale> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<PromotionSale>(SqlReplacePromotionSale, parData);
            }
            return true;
        }

        public bool ReplacePromotionSaleData(IEnumerable<PromotionSaleData> parData)
        {
            string SqlReplacePromotionSaleData = @"
replace into PROMOTION_SALE_DATA(CODE_PS, NUMBER_GROUP, CODE_WARES, USE_INDICATIVE, TYPE_DISCOUNT, ADDITIONAL_CONDITION, DATA, DATA_ADDITIONAL_CONDITION)
                          values(@CodePS, @NumberGroup, @CodeWares, @UseIndicative, @TypeDiscount, @AdditionalCondition, @Data, @DataAdditionalCondition)";
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<PromotionSaleData>(SqlReplacePromotionSaleData, parData);
            }
            return true;
        }

        public bool ReplacePromotionSaleFilter(IEnumerable<PromotionSaleFilter> parData)
        {
            string SqlReplacePromotionSaleFilter = @"
replace into PROMOTION_SALE_FILTER(CODE_PS, CODE_GROUP_FILTER, TYPE_GROUP_FILTER, RULE_GROUP_FILTER, CODE_CHOICE, CODE_DATA, CODE_DATA_END)
                          values(@CodePS, @CodeGroupFilter, @TypeGroupFilter, @RuleGroupFilter, @CodeChoice, @CodeData, @CodeDataEnd)";
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<PromotionSaleFilter>(SqlReplacePromotionSaleFilter, parData);
            }
            return true;
        }

        public bool ReplacePromotionSaleDealer(IEnumerable<PromotionSaleDealer> parData)
        {
            string SqlReplacePromotionSaleDealer = @"replace into PROMOTION_SALE_DEALER(CODE_PS, Code_Wares, DATE_BEGIN, DATE_END, Code_Dealer, Priority) values(@CodePS, @CodeWares, @DateBegin, @DateEnd, @CodeDealer, @Priority); --@CodePS,@CodeWares,@DateBegin,@DateEnd,@CodeDealer";
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<PromotionSaleDealer>(SqlReplacePromotionSaleDealer, parData);
            }
            return true;
        }

        public bool ReplacePromotionSaleGroupWares(IEnumerable<PromotionSaleGroupWares> parData)
        {
            string SqlReplacePromotionSaleGroupWares = @"replace into PROMOTION_SALE_GROUP_WARES(CODE_GROUP_WARES_PS, CODE_GROUP_WARES) values(@CodeGroupWaresPS, @CodeGroupWares)";
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<PromotionSaleGroupWares>(SqlReplacePromotionSaleGroupWares, parData);
            }
            return true;
        }

        public bool ReplacePromotionSaleGift(IEnumerable<PromotionSaleGift> parData)
        {
            string SqlReplacePromotionSaleGift = @"replace into PROMOTION_SALE_GIFT(CODE_PS, NUMBER_GROUP, CODE_WARES, TYPE_DISCOUNT, DATA, QUANTITY, DATE_CREATE, USER_CREATE)
                          values(@CodePS, @NumberGroup, @CodeWares, @TypeDiscount, @Data, @Quantity, @DateCreate, @UserCreate);";
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<PromotionSaleGift>(SqlReplacePromotionSaleGift, parData);
            }
            return true;
        }

        public bool ReplacePromotionSale2Category(IEnumerable<PromotionSale2Category> parData)
        {
            string SqlReplacePromotionSale2Category = "replace into PROMOTION_SALE_2_category(CODE_PS, CODE_WARES) values(@CodePS, @CodeWares)";
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<PromotionSale2Category>(SqlReplacePromotionSale2Category, parData);
            }
            return true;
        }

        public bool UpdateQR(ReceiptWares pRW)
        {
            string SqlUpdateQR = "update wares_receipt set QR = @QR where id_workplace = @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt  and code_wares = @CodeWares";
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<ReceiptWares>(SqlUpdateQR, pRW) > 0;
            }
            //return true;
        }

        public bool UpdateExciseStamp(IEnumerable<ReceiptWares> pRW)
        {
            string SqlUpdateExciseStamp = @"update wares_receipt set Excise_Stamp = @ExciseStamp
                     where id_workplace = @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt and code_wares = @CodeWares";

            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.BulkExecuteNonQuery<ReceiptWares>(SqlUpdateExciseStamp, pRW) > 0;
            }
        }

        public IEnumerable<QR> GetQR(IdReceipt pR)
        {
            string SqlGetQR = @"select wr.QR,'(' || w.PLU || ')-' || w.name_wares as name from wares_receipt wr
    join wares w on wr.code_wares = w.code_wares
        where wr.id_workplace = @IdWorkplace and wr.code_period = @CodePeriod and wr.code_receipt = @CodeReceipt and QR is not null;";
            return db.Execute<IdReceipt, QR>(SqlGetQR, pR);
        }

        public bool ReplaceUser(IEnumerable<User> pUser)
        {
            string SqlReplaceUser = "replace into User(CODE_USER, NAME_USER, BAR_CODE, Type_User, LOGIN, PASSWORD) values(@CodeUser, @NameUser, @BarCode, @TypeUser, @Login, @PassWord);";
            using (var DB = new SQLite(MidFile))
            {
                return DB.BulkExecuteNonQuery<User>(SqlReplaceUser, pUser, true) > 0;
            }
        }
        public bool ReplaceSalesBan(IEnumerable<SalesBan> pSB)
        {
            string SqlReplaceSalesBan = "replace into  Sales_Ban(CODE_GROUP_WARES, Amount) values(@CodeGroupWares, @Amount);";
            using (var DB = new SQLite(MidFile))
            {
                return DB.BulkExecuteNonQuery<SalesBan>(SqlReplaceSalesBan, pSB) > 0;
            }
        }

        public bool InsertLogRRO(IEnumerable<LogRRO> pLog)
        {
            string SQL = @"insert into Log_RRO  (ID_WORKPLACE,ID_WORKPLACE_PAY,TypePay,CODE_PERIOD,CODE_RECEIPT,FiscalNumber, Number_Operation,Type_Operation, SUM ,Type_RRO,JSON, Text_Receipt,Error,CodeError, USER_CREATE) VALUES
                     (@IdWorkplace, @IdWorkplacePay,@TypePay,@CodePeriod,@CodeReceipt,@FiscalNumber,@NumberOperation,@TypeOperation,@SUM,@TypeRRO,@JSON,@TextReceipt,@Error,@CodeError,@UserCreate)
";
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.BulkExecuteNonQuery<LogRRO>(SQL, pLog) > 0;
            }
        }

        public bool AddFiscalArticle(FiscalArticle pFiscalArticle)
        {
            var SQL = "replace into FiscalArticle (IdWorkplacePay,CodeWares,NameWares ,PLU ,Price) values (@IdWorkplacePay,@CodeWares,@NameWares ,@PLU ,@Price)";
            using (var DB = new SQLite(ConfigFile))
            {
                return DB.ExecuteNonQuery<FiscalArticle>(SQL, pFiscalArticle) > 0;
            }
        }

        public bool DelAllFiscalArticle()
        {
            var SQL = "delete from FiscalArticle";
            using (var DB = new SQLite(ConfigFile))
            {
                return DB.ExecuteNonQuery(SQL) > 0;
            }
        }

        public FiscalArticle GetFiscalArticle(ReceiptWares pRW)
        {
            var SQL = "select * from FiscalArticle where CodeWares=@CodeWares and IdWorkplacePay=@IdWorkplacePay";
            using (var DB = new SQLite(ConfigFile))
            {
                return DB.connection.QueryFirstOrDefault<FiscalArticle>(SQL, new FiscalArticle() { CodeWares = pRW.CodeWares, IdWorkplacePay = pRW.IdWorkplacePay });
            }
        }

        public bool DelPayWalletBonus(IdReceipt pIdR)
        {
            var SQL = @"Delete from PAYMENT
Where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt
   and TYPE_PAY in (5,6);
Update WARES_RECEIPT set SUM_WALLET = 0, Sum_bonus = 0
Where ID_WORKPLACE = @IdWorkplace
   and CODE_PERIOD = @CodePeriod
   and CODE_RECEIPT = @CodeReceipt;";

            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<IdReceipt>(SQL, pIdR) > 0;
            }
        }
        /// <summary>
        /// Оновлення структури бази даних
        /// </summary>       
        protected bool UpdateDB(ref bool pIsUseOld)
        {
            try
            {
                IsFirstStart = false;
                int CurVerConfig = 0, CurVerMid = 0, CurVerRC = 0;
                var Lines = SqlUpdateConfig.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                SQLite dbC = new SQLite(ConfigFile);
                var wDB = new WDB_SQLite(); //Path.Combine(Global.PathIni, "SQLite.sql"), dbC);

                CurVerConfig = wDB.GetConfig<int>("VerConfig");
                CurVerMid = wDB.GetConfig<int>("VerMid");
                CurVerRC = wDB.GetConfig<int>("VerRC");

                //Config
                var (Ver, IsReload) = Parse(Lines, CurVerConfig, dbC);
                if (Ver != CurVerConfig)
                    wDB.SetConfig<int>("VerConfig", Ver);

                //Mid
                if (File.Exists(MidFile))
                {
                    var dbMID = new SQLite(MidFile);
                    CurVerMid = wDB.GetConfig<int>("VerMID");
                    Lines = SqlUpdateMID.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    (Ver, IsReload) = Parse(Lines, CurVerMid, dbMID);
                    dbMID.Close(true);
                    dbMID = null;
                    if (IsReload)
                    {
                        if (File.Exists(MidFile)) File.Delete(MidFile);
                        pIsUseOld = false;

                    }
                    if (Ver != CurVerMid)
                        wDB.SetConfig<int>("VerMID", Ver);
                }
                //RC
                CurVerRC = wDB.GetConfig<int>("VerRC");
                var cDT = DateTime.Now.Date.AddDays(-10);

                while (cDT <= DateTime.Now.Date)
                {
                    if (File.Exists(GetReceiptFile(cDT)))
                    {
                        var dbRC = new SQLite(GetReceiptFile(cDT));
                        Lines = SqlUpdateRC.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        (Ver, IsReload) = Parse(Lines, CurVerRC, dbRC);
                    }
                    cDT = cDT.AddDays(1);
                }
                if (Ver != CurVerRC)
                    wDB.SetConfig<int>("VerRC", Ver);
                return true;
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                return false;
            }
        }

        public virtual StatusBD GetStatus()
        {
            var DTFirstErrorDiscountOnLine = Global.FirstErrorDiscountOnLine;
            var DTLastNotSendReceipt = GetLastNotSendReceipt();
            var DTGetLastUpdateDirectory = GetLastUpdateDirectory();
            var ExchangeStatus = Global.GetExchangeStatus(DTFirstErrorDiscountOnLine);
            var ExchangeStatus1 = Global.GetExchangeStatus(DTGetLastUpdateDirectory);
            if (ExchangeStatus1 > ExchangeStatus)
                ExchangeStatus = ExchangeStatus1;
            ExchangeStatus1 = Global.GetExchangeStatus(DTLastNotSendReceipt);
            if (ExchangeStatus1 > ExchangeStatus)
                ExchangeStatus = ExchangeStatus1;

            var res = new StatusBD { Descriprion = $"DTFirstErrorDiscountOnLine=>{DTFirstErrorDiscountOnLine}, DTLastNotSendReceipt=>{DTLastNotSendReceipt},DTGetLastUpdateDirectory=>{DTGetLastUpdateDirectory}" };
            res.SetColor(ExchangeStatus);
            return res;
        }

        protected (int, bool) Parse(string[] pLines, int pCurVersion, SQLite pDB)
        {
            int NewVer = pCurVersion, Ver;
            bool isReload = false;

            foreach (var el in pLines)
            {
                var i = el.ToUpper().IndexOf("VER=>");
                if (i >= 0)
                {
                    string str = el.Substring(i + 5);
                    string[] All = str.Split(';');
                    if (int.TryParse(All[0], out Ver))
                    {
                        if (Ver > pCurVersion)
                            try
                            {
                                FileLogger.WriteLogMessage($"WDB_SQLite.Parse ( el=>{el},pCurVersion=>{pCurVersion}) => (Ver=>{Ver}){Environment.NewLine}");
                                pDB.ExecuteNonQuery(el);
                                if (All.Length > 1 && All[1].ToUpper().Equals("Reload".ToUpper()))
                                    isReload = true;
                                if (NewVer <= Ver)
                                    NewVer = Ver;
                            }
                            catch (Exception e)
                            {
                                if (e.Message.IndexOf("duplicate column name:") == -1)
                                {
                                    FileLogger.WriteLogMessage($"WDB_SQLite.Parse Exception =>( el=>{el},pCurVersion=>{pCurVersion}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                                    throw new Exception(e.Message, e);
                                }
                                else
                                    if (NewVer <= Ver)
                                    NewVer = Ver;
                            }
                    }
                }
            }

            return (NewVer, isReload);
        }

        public bool ReplaceClientNew(ClientNew pC)
        {
            string Sql = @"replace into  Client_New (ID_WORKPLACE, BARCODE_CLIENT, BARCODE_CASHIER,  PHONE) values  (@IdWorkplace, @BarcodeClient, @BarcodeCashier, @Phone);";
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<ClientNew>(Sql, pC) > 0;
            }
        }

        public bool SetConfirmClientNew(ClientNew pC)
        {
            string Sql = @"update Client_New set STATE=1 where Barcode_Client = @BarcodeClient and Phone=@Phone";
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<ClientNew>(Sql, pC) > 0;
            }
        }

        public bool ReplaceClientData(IEnumerable<ClientData> pCD)
        {
            string Sql = @"replace into ClientData (TypeData,CodeClient,Data) values (@TypeData,@CodeClient,@Data)";
            using (var DB = new SQLite(MidFile))
            {
                return DB.BulkExecuteNonQuery<ClientData>(Sql, pCD) > 0;
            }
        }
        /// <summary>
        /// Повертає знайденого клієнта(клієнтів)
        /// </summary>
        /// <param name="parCodeWares">Код товару</param>
        /// <returns>
        ///Повертає  IEnumerable<Client> з клієнтами
        ///</returns>
        public IEnumerable<Client> FindClient(string parBarCode = null, string parPhone = null, string parName = null, int parCodeClient = 0)
        {
            var Res = db.Execute<object, Client>(SqlFindClient, new { CodeClient = parCodeClient, Phone = parPhone, BarCode = parBarCode, Name = (parName == null ? null : "%" + parName + "%") });
            return Res != null && Res.Any(el => el.StatusCard == eStatusCard.Active) ? Res.Where(el => el.StatusCard == eStatusCard.Active) : Res;
        }

        public SpeedScan SpeedScan()
        {
            SpeedScan Res = new SpeedScan();
            try
            {
                var res = db.Execute<SpeedScan>(SqlLineReceipt);
                if (res != null)
                    Res = res.FirstOrDefault();

                Res.Speed = db.ExecuteScalar<int>(SqlSpeedScan);
            }
            catch (Exception ex)
            {
                var r = ex.Message;
            }
            return Res;
        }

        public virtual IEnumerable<LogRRO> GetLogRRO(IdReceipt pR)
        {
            return db.Execute<IdReceipt, LogRRO>(SqlGetLogRRO, pR);
        }

        public virtual Receipt ViewReceipt(IdReceipt parIdReceipt, bool parWithDetail = false)
        {
            var res = this.db.Execute<IdReceipt, Receipt>(SqlViewReceipt, parIdReceipt);
            if (res.Count() == 1)
            {
                var r = res.FirstOrDefault();
                if (parWithDetail)
                {
                    r.Wares = ViewReceiptWares(r, true);
                    foreach (var el in r.Wares)
                        el.AdditionalWeights = db.Execute<object, decimal>(SqlAdditionalWeightsWares, new { CodeWares = el.CodeWares });

                    r.Payment = GetPayment(parIdReceipt);
                    r.ReceiptEvent = GetReceiptEvent(parIdReceipt);
                    r.LogRROs = GetLogRRO(parIdReceipt);
                }
                return r;
            }
            return null;
        }

        public IEnumerable<ParameterPromotion> GetInfoClientByReceipt(IdReceipt parIdReceipt)
        {
            return db.Execute<IdReceipt, ParameterPromotion>(SqlGetInfoClientByReceipt, parIdReceipt);
        }

        public IEnumerable<ReceiptWares> ViewReceiptWares(IdReceiptWares parIdReceiptWares, bool pIsReceiptWaresPromotion = false)
        {
            var vReceipt = parIdReceiptWares.Parent as Receipt;
            IEnumerable<WaresReceiptPromotion> wrp = null;
            var r = this.db.Execute<IdReceiptWares, ReceiptWares>(SqlViewReceiptWares, parIdReceiptWares);
            if (pIsReceiptWaresPromotion)
            {
                wrp = GetReceiptWaresPromotion(parIdReceiptWares);
            }
            if (r != null)
            {
                foreach (var el in r)
                {
                    if (vReceipt != null)
                        el.Parent = vReceipt;
                    if (wrp != null)
                        el.ReceiptWaresPromotions = wrp.Where(rr => ((IdReceiptWares)rr).Equals((IdReceiptWares)el));
                }
            }
            return r;
        }

        public virtual IEnumerable<IdReceipt> GetIdReceiptbyState(eStateReceipt parState = eStateReceipt.Print)
        {
            string SqlGetIdReceiptbyState = "select ID_WORKPLACE as IdWorkplace,CODE_PERIOD as CodePeriod, CODE_RECEIPT as CodeReceipt from receipt where STATE_RECEIPT = @StateReceipt";
            return db.Execute<object, IdReceipt>(SqlGetIdReceiptbyState, new { StateReceipt = parState });
        }

        public virtual decimal GetCountWares(IdReceiptWares parIdReceiptWares)
        {
            string SqlGetCountWares = @"select sum(wr.quantity) quantity from wares_receipt wr
                     where wr.id_workplace = @IdWorkplace and wr.code_period = @CodePeriod and wr.code_receipt = @CodeReceipt
                     and wr.code_wares = @CodeWares and wr.code_unit = @CodeUnit";
            return db.ExecuteScalar<IdReceiptWares, decimal>(SqlGetCountWares, parIdReceiptWares);
        }

        public virtual PricePromotion GetPrice(ParameterPromotion parPromotion)
        {
            string SqlGetPriceDealer = "select p.PRICE_DEALER as PriceDealer from PRICE p where p.CODE_DEALER = @CodeDealer and p.CODE_WARES = @CodeWares";
            var PriceDealer = db.ExecuteScalar<ParameterPromotion, decimal>(SqlGetPriceDealer, parPromotion);
            var Res = new PricePromotion() { Price = PriceDealer };
            foreach (var el in db.Execute<ParameterPromotion, PricePromotion>(SqlGetPrice, parPromotion))
            {
                if ((el.CalcPrice(PriceDealer) < Res.Price && Res.Priority <= el.Priority) || Res.Priority < el.Priority)
                {
                    var IsUsePrice = (Res.Priority == el.Priority);
                    Res = el;
                    Res.Price = el.CalcPrice(PriceDealer, IsUsePrice);
                }
            }
            return Res;
        }

        public Int64 GetPricePromotionSale2Category(IdReceiptWares parIdReceiptWares)
        {
            return db.ExecuteScalar<IdReceiptWares, Int64>(SqlGetPricePromotionSale2Category, parIdReceiptWares);
        }

        public virtual IEnumerable<WorkPlace> GetWorkPlace()
        {
            string SqlGetWorkplace = @"select ID_WORKPLACE as IdWorkplace, NAME as Name, Terminal_GUID as StrTerminalGUID, 
       Video_Camera_IP as VideoCameraIP, Video_Recorder_IP as VideoRecorderIP , Type_POS as TypePOS,
       Code_Warehouse as CodeWarehouse ,CODE_DEALER as CodeDealer,Prefix, DNSName,TypeWorkplace ,SettingsEx from WORKPLACE;";
            return db.Execute<WorkPlace>(SqlGetWorkplace);
        }

        public bool BildWorkplace(IEnumerable<WorkPlace> pWP = null)
        {
            var WP = pWP ?? GetWorkPlace();
            var WorkPlaceByTerminalId = new SortedList<Guid, WorkPlace>();
            var WorkPlaceByWorkplaceId = new SortedList<int, WorkPlace>();
            foreach (var el in WP)
            {
                WorkPlaceByTerminalId.Add(el.TerminalGUID, el);
                WorkPlaceByWorkplaceId.Add(el.IdWorkplace, el);
                if (el.IdWorkplace == Global.IdWorkPlace && el.Settings != null)
                    Global.Settings = el.Settings;
            }

            Global.WorkPlaceByTerminalId = WorkPlaceByTerminalId;
            Global.WorkPlaceByWorkplaceId = WorkPlaceByWorkplaceId;

            return true;
        }

        public virtual IEnumerable<FastGroup> GetFastGroup(int parCodeUpFastGroup)
        {
            string SqlGetFastGroup = "select code_up as CodeUp,CODE_FAST_GROUP as CodeFastGroup,NAME from FAST_GROUP where code_up = @CodeUp order by CODE_FAST_GROUP";
            var FG = new FastGroup { CodeUp = parCodeUpFastGroup };
            return db.Execute<FastGroup, FastGroup>(SqlGetFastGroup, FG);
        }

        public MinPriceIndicative GetMinPriceIndicative(IdReceiptWares parIdReceiptWares)
        {
            string SqlGetMinPriceIndicative = @"
Select min(case when CODE_DEALER = -888888  then PRICE_DEALER else null end) as MinPrice
,min(case when CODE_DEALER = -999999  then PRICE_DEALER else null end) as Indicative
 from price where CODE_DEALER in(-999999, -888888) and CODE_WARES = @CodeWares";
            var res = db.Execute<IdReceiptWares, MinPriceIndicative>(SqlGetMinPriceIndicative, parIdReceiptWares);
            if (res != null)
                return res.FirstOrDefault();
            return null;
        }

        public bool ReplaceMRC(IEnumerable<MRC> parData)
        {
            string SqlReplaceMRC = " replace into  MRC(Code_Wares, Price, Type_Wares) values(@CodeWares, @Price, @TypeWares);";

            db.BulkExecuteNonQuery<MRC>(SqlReplaceMRC, parData);
            return true;
        }

        public IEnumerable<Payment> GetPayment(IdReceipt parIdReceipt)
        {
            string SqlGetPayment = @"
select id_workplace as IdWorkplace, id_workplace_pay as IdWorkplacePay,code_period as CodePeriod, code_receipt as CodeReceipt, 
 TYPE_PAY as TypePay, Code_Bank as CodeBank, CODE_WARES as CodeWares, SUM_PAY as SumPay, SUM_EXT as SumExt,
    NUMBER_TERMINAL as NumberTerminal,   NUMBER_RECEIPT as NumberReceipt, CODE_AUTHORIZATION as CodeAuthorization, NUMBER_SLIP as NumberSlip,
    Pos_Paid as PosPaid, Pos_Add_Amount as PosAddAmount, DATE_CREATE as DateCreate,Number_Card as NumberCard,
    Card_Holder as CardHolder ,Issuer_Name as IssuerName, Bank,TransactionId
   from payment
  where   id_workplace = @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt";
            return db.Execute<IdReceipt, Payment>(SqlGetPayment, parIdReceipt);
        }

        public IEnumerable<WaresReceiptPromotion> CheckLastWares2Cat(IdReceipt parIdReceipt)
        {
            return db.Execute<IdReceipt, WaresReceiptPromotion>(SqlCheckLastWares2Cat, parIdReceipt);
        }

        public Receipt GetLastReceipt(IdReceipt parIdReceipt)
        {
            var r = this.db.Execute<IdReceipt, Receipt>(SqlGetLastReceipt, parIdReceipt);
            if (r != null && r.Count() == 1)
                return r.First();
            return null;
        }
        public IEnumerable<Receipt> GetReceipts(DateTime parStartDate, DateTime parFinishDate, int parIdWorkPlace)
        {
            string SqlGetReceipts = @"select id_workplace IdWorkplace, code_period CodePeriod, code_receipt CodeReceipt, date_receipt DateReceipt, Type_Receipt as TypeReceipt,
sum_receipt SumReceipt, vat_receipt VatReceipt, code_pattern CodePattern, state_receipt as StateReceipt, code_client as CodeClient,
 number_cashier as NumberCashier, number_receipt NumberReceipt, code_discount as CodeDiscount, sum_discount as SumDiscount, percent_discount as PercentDiscount, 
 code_bonus as CodeBonus, sum_bonus as SumBonus, sum_cash as SumCash, sum_credit_card as SumCreditCard, code_outcome as CodeOutcome, 
 code_credit_card as CodeCreditCard, number_slip as NumberSlip,Number_Receipt_POS as NumberReceiptPOS, number_tax_income as NumberTaxIncome,USER_CREATE as UseCreate, Date_Create as DateCreate,
 ADDITION_N1 as AdditionN1,ADDITION_N2 as AdditionN2, ADDITION_N3 as AdditionN3,
 ADDITION_C1 as AdditionC1,ADDITION_D1 as AdditionD1,Number_Order as NumberOrder,Sum_Fiscal as SumFiscal
 from receipt
 where ID_WORKPLACE = case when @IdWorkplace = 0 then ID_WORKPLACE else @IdWorkplace end
 and DateReceipt between @StartDate and @FinishDate;";
            return db.Execute<object, Receipt>(SqlGetReceipts, new { StartDate = parStartDate, FinishDate = parFinishDate, IdWorkPlace = parIdWorkPlace });
        }

        public IEnumerable<Receipt> GetReceiptByFiscalNumber(int pIdWorkPlace, string pFiscalNumber, DateTime pStartDate = default(DateTime), DateTime pFinishDate = default(DateTime))
        {
            return db.Execute<object, Receipt>(SqlReceiptByFiscalNumbers, new { StartDate = pStartDate, FinishDate = pFinishDate, IdWorkPlace = pIdWorkPlace, NumberReceipt = pFiscalNumber });
        }

        public IEnumerable<ReceiptEvent> GetReceiptEvent(IdReceipt parIdReceipt)
        {
            return db.Execute<IdReceipt, ReceiptEvent>(SqlGetReceiptEvent, parIdReceipt);
        }

        public IEnumerable<WaresReceiptPromotion> GetReceiptWaresPromotion(IdReceiptWares parIdReceiptWares)
        {
            return this.db.Execute<IdReceipt, WaresReceiptPromotion>(SqlGetReceiptWaresPromotion, parIdReceiptWares);
        }

        public virtual IEnumerable<WaresReceiptPromotion> GetReceiptWaresPromotion(IdReceipt parIdReceipt)
        {
            return GetReceiptWaresPromotion(new IdReceiptWares(parIdReceipt));
        }

        public decimal GetLastQuantity(IdReceiptWares parIdReceiptWares)
        {
            string SqlGetLastQuantity = @"select QUANTITY from(
SELECT QUANTITY - QUANTITY_OLD as QUANTITY, ROW_NUMBER()   OVER(ORDER BY  DATE_CREATE DESC) AS nn FROM WARES_RECEIPT_HISTORY
 where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod and CODE_RECEIPT = @CodeReceipt and Code_wares = @CodeWares
  ) where nn = 1";
            return db.ExecuteScalar<IdReceiptWares, decimal>(SqlGetLastQuantity, parIdReceiptWares);
        }

        public virtual IEnumerable<ReceiptWaresDeleted1C> GetReceiptWaresDeleted()
        {
            return this.db.Execute<ReceiptWaresDeleted1C>(SqlGetReceiptWaresDeleted);
        }


        public virtual IEnumerable<User> GetUser(User pUser)
        {
            string SqlGetUser = @"select CODE_USER as CodeUser, NAME_USER as NameUser,  BAR_CODE as BarCode, Type_User as TypeUser, LOGIN, PASSWORD from USER
    where(Login = @Login and Password = @PassWord) or BAR_CODE = @BarCode;";
            return db.Execute<User, User>(SqlGetUser, pUser);
        }

        public virtual eStateReceipt GetStateReceipt(IdReceipt pR)
        {
            string SqlGetStateReceipt = @"select max(STATE_RECEIPT) StateReceipt   FROM RECEIPT
 Where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod and CODE_RECEIPT = @CodeReceipt";
            return db.ExecuteScalar<IdReceipt, eStateReceipt>(SqlGetStateReceipt, pR);
        }

        public eTypeReceipt GetTypeReceipt(IdReceipt pR)
        {
            string Sql = @"select max(Type_Receipt) Type_Receipt FROM RECEIPT
    Where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod and CODE_RECEIPT = @CodeReceipt";
            return db.ExecuteScalar<IdReceipt, eTypeReceipt>(Sql, pR);
        }

        public decimal GetSumCash(IdReceipt pR)
        {
            string Sql = @"select  sum(sum) from
(select sum(sum) as sum from LOG_RRO l
where TYPE_OPERATION = 2 and not (length (error) > 0) and ID_WORKPLACE_PAY = @IdWorkplacePay  and CODE_PERIOD = @CodePeriod
union all
select sum( sum_pay* case when TYPE_PAY in (4) then -1 else 1 end) as sum from payment p where TYPE_PAY in (2,4) and ID_WORKPLACE_PAY = @IdWorkplacePay and CODE_PERIOD = @CodePeriod) d";
            return db.ExecuteScalar<IdReceipt, decimal>(Sql, pR);
        }

        public IEnumerable<ClientNew> GetClientNewNotSend()
        {
            string Sql = @"select Id_Workplace as IdWorkplace,State,Barcode_Client as BarcodeClient,Barcode_Cashier as BarcodeCashier,Phone,Date_Create as DateCreate from Client_New where state=0";
            return db.Execute<ClientNew>(Sql);
        }

        /// <summary>
        /// Показує інформацю про товари в чеку 
        /// </summary>
        /// <param name="parParameters"> </param>
        /// <returns></returns>
     	public IEnumerable<ReceiptWares> ViewReceiptWares(IdReceipt parIdReceipt, bool pIsReceiptWaresPromotion = false)
        {
            return ViewReceiptWares(new IdReceiptWares(parIdReceipt), pIsReceiptWaresPromotion); //this.db.Execute<IdReceipt, ReceiptWares>(SqlViewReceiptWares, parIdReceipt);
        }

        public bool IsWaresInPromotionKit(int parCodeWares)
        {
            return db.ExecuteScalar<object, int>(SqlIsWaresInPromotionKit, new { CodeWares = parCodeWares }) > 0;
        }

        public IEnumerable<WaresWarehouse> GetWaresWarehouse()
        {
            return db.Execute<WaresWarehouse>("Select * from WaresWarehouse");
        }

        public bool ReplaceWaresWarehouse(IEnumerable<WaresWarehouse> pWW)
        {
            if (pWW?.Any() == true)
            {
                string SQL = "replace into WaresWarehouse (CodeWarehouse,TypeData,Data) values (@CodeWarehouse,@TypeData,@Data)";
                if (db.BulkExecuteNonQuery<WaresWarehouse>(SQL, pWW) > 0)
                {
                    BildWaresWarehouse(pWW);
                    return true;
                }
            }
            return false;
        }

        public void BildWaresWarehouse(IEnumerable<WaresWarehouse> pWW)
        {
            if (pWW == null)
                pWW = GetWaresWarehouse();

            if (Global.Settings.IdWorkPlaceLink > 0 && Global.Settings.CodeWarehouseLink > 0)
            {
                foreach (var el in pWW.Where(el => el.CodeWarehouse == Global.Settings.CodeWarehouseLink))
                {
                    if (el.TypeData == eTypeData.Directions && !Global.IdWorkPlacePayDirection.ContainsKey(el.Data))
                        Global.IdWorkPlacePayDirection.Add(el.Data, Global.Settings.IdWorkPlaceLink);

                    if (el.TypeData == eTypeData.Brand && !Global.IdWorkPlacePayTM.ContainsKey(el.Data))
                        Global.IdWorkPlacePayTM.Add(el.Data, Global.Settings.IdWorkPlaceLink);
                }
            }
        }

    }
}