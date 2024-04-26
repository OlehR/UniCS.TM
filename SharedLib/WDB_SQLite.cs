using System;
using System.Collections.Generic;
using System.Data;
//using DB_SQLite;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
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
        static WDB_SQLite Instance = null;
        public static WDB_SQLite GetInstance { get { Instance ??= new WDB_SQLite(); return Instance; } }
        public string Version = "SQLite 0.0.3";
        public SQLite db;
        public eDBStatus DBStatus = eDBStatus.NotDefine;
        private static bool IsFirstStart = true;
        private bool isDisposed;
        private DateTime DT = DateTime.Today;

        SQLite dbConfig;
        SQLite dbMid;
        SQLite dbRC;

        private string Connect = null;
        private string ConfigFile { get { return Path.Combine(ModelMID.Global.PathDB, "config.db"); } }
        public string LastMidFile = null;
        private string MidFile { get { return string.IsNullOrEmpty(Connect) ? (string.IsNullOrEmpty(LastMidFile) ? GetMIDFile() : LastMidFile) : Connect; } }
        private string ReceiptFile { get { return GetReceiptFile(DT); } }
        private string GetReceiptFile(DateTime pDT) { return Path.Combine(ModelMID.Global.PathDB, $"{pDT:yyyyMM}", $"Rc_{ModelMID.Global.IdWorkPlace}_{pDT:yyyyMMdd}.db"); }

        public string GetMIDFile(DateTime pD = default, bool pTmp = false)
        {
            DateTime Date = pD == default ? DateTime.Today : pD;
            return Path.Combine(Global.PathDB, $"{Date:yyyyMM}", $"MID_{Date:yyyyMMdd}{(pTmp ? "_tmp" : "")}.db");
        }

        public WDB_SQLite(DateTime parD = default(DateTime), string pConnect = null, bool pIsUseOldDB = true)//, bool pIsCreateMidFile = false)
        {
            Connect = pConnect;
            DT = parD != default(DateTime) ? parD.Date : DateTime.Today.Date;

            if (!File.Exists(ConfigFile))
            {
                dbConfig = new SQLite(ConfigFile);
                dbConfig.ExecuteNonQuery(SqlCreateConfigTable);
                dbConfig.SetVersion(VerConfig);
            }
            else
                dbConfig = new SQLite(ConfigFile);

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

            CreateRC(ReceiptFile);
            GetDB();
            DBStatus = eDBStatus.Ok;
        }
        ~WDB_SQLite()
        {
            //Close();
        }

        void CreateRC(string pReceiptFile = null)
        {
            if (string.IsNullOrEmpty(pReceiptFile))
                pReceiptFile = ReceiptFile;
            if (!File.Exists(pReceiptFile))
            {
                var receiptFilePath = Path.GetDirectoryName(pReceiptFile);
                if (!Directory.Exists(receiptFilePath))
                    Directory.CreateDirectory(receiptFilePath);
                //Створюємо щоденну табличку з чеками.
                using var db = new SQLite(pReceiptFile);
                db.ExecuteNonQuery(SqlCreateReceiptTable);
                db.SetVersion(VerRC);
                db.Close();
            }
        }

        void NewDBRC()
        {
            if (DT != DateTime.Today)
            {
                DT = DateTime.Today;
                CreateRC();
                GetDB();
            }
        }

        public SQLite GetRC(DateTime pDT)
        {
            NewDBRC();
            return new SQLite(GetReceiptFile(pDT));
        }

        void FindLastMid()
        {
            //var db = new SQLite(ConfigFile);
            var LFDB = GetConfig<DateTime>("Load_Full", dbConfig).Date;
            if (LFDB < DateTime.Now.AddDays(-10))
                LFDB = DateTime.Now.Date;
            do
            {
                var vLastMidFile = GetMIDFile(LFDB);
                if (!string.IsNullOrEmpty(vLastMidFile) && File.Exists(vLastMidFile))
                    LastMidFile = vLastMidFile;
                LFDB = LFDB.AddDays(-1);
            } while (LastMidFile == null && LFDB > DateTime.Now.AddDays(-10));
        }

        public void GetDB()
        {
            FindLastMid();
            var DB = new SQLite(ConfigFile);
            if (File.Exists(MidFile))
                DB.ExecuteNonQuery("ATTACH '" + MidFile + "' AS mid");
            DB.ExecuteNonQuery("ATTACH '" + ReceiptFile + "' AS con");
            //dbConfig = new SQLite(ConfigFile);
            if (File.Exists(MidFile))
                dbMid = new SQLite(MidFile);
            dbRC = new SQLite(ReceiptFile);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"ConfigFile=>{ConfigFile} MidFile={MidFile} ReceiptFile={ReceiptFile}");
            db = DB;
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
        /// Оновлення структури бази даних
        /// </summary>       
        protected bool UpdateDB(ref bool pIsUseOld)
        {
            try
            {
                IsFirstStart = false;
                //int CurVerConfig = 0, CurVerMid = 0, CurVerRC = 0;
                //var Lines = SqlUpdateConfig.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                
                //Config
                 Parse(SqlUpdateConfig, dbConfig);                

                //Mid
                if (File.Exists(MidFile))
                {
                    using var dbMID = new SQLite(MidFile);
                    //CurVerMid = dbMID.GetVersion;// GetConfig<int>("VerMID",dbConfig);
                    //Lines = SqlUpdateMID.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    bool IsReload = Parse(SqlUpdateMID, dbMID);                    
                    dbMID.Close(true);
                    if (IsReload)
                    {
                        if (File.Exists(MidFile)) File.Delete(MidFile);
                        pIsUseOld = false;
                    }
                    //if (Ver != CurVerMid) SetConfig<int>("VerMID", Ver);
                }

                //RC                
                var cDT = DateTime.Now.Date.AddDays(-10);
                if (DT < cDT) UpdateRC(DT);
                while (cDT <= DateTime.Now.Date)
                {
                    UpdateRC(cDT);
                    cDT = cDT.AddDays(1);
                }
                return true;
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                return false;
            }
        }

        void UpdateRC(DateTime pDT)
        {
            if (File.Exists(GetReceiptFile(pDT)))
            {
                using var dbRC = GetRC(pDT);               
                Parse(SqlUpdateRC, dbRC);                
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

            var res = new StatusBD { ExchangeStatus = ExchangeStatus, Descriprion = $"DTFirstErrorDiscountOnLine=>{DTFirstErrorDiscountOnLine}, DTLastNotSendReceipt=>{DTLastNotSendReceipt},DTGetLastUpdateDirectory=>{DTGetLastUpdateDirectory}" };
           // res.SetColor(ExchangeStatus);
            return res;
        }

        protected bool Parse(string pComand, SQLite pDB)
        {
            string[] Lines = pComand.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int pCurVersion = pDB.GetVersion;
            int NewVer = pCurVersion, Ver;
            bool isReload = false;

            foreach (var el in Lines)
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
                                if (e.Message.IndexOf("duplicate column name:") == -1 && e.Message.IndexOf("already exists") == -1)
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
            if(NewVer!= pCurVersion)
                pDB.SetVersion(NewVer);
            return  isReload;
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
                        VR.SendMessage(parW.IdWorkplace, parW.NameWares, parW.Articl, parW.Quantity, parW.Sum, VR.eTypeVRMessage.AddWares, SumAll);
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
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "RecalcPrice=>" + ex.Message + '\n' + new System.Diagnostics.StackTrace().ToString() });
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
                            vPrice = Price * (100 - el.DataDiscount) / 100m;
                        }
                        var RWP = new WaresReceiptPromotion(parIdReceipt) { CodeWares = el.CodeWares, Quantity = AddQuantity, Price = vPrice, CodePS = el.CodePS, NumberGroup = el.NumberGroup };
                        varRes.Add(RWP);
                        ReceiptWares? rw = RW.FirstOrDefault(e => e.CodeWares == el.CodeWares);
                        if(rw != null) 
                        {
                            rw.TypePrice = eTypePrice.Promotion;
                            rw.ParPrice1 = RWP.CodePS;
                            rw.ParPrice2 = (long)RWP.TypeDiscount;
                            rw.ParPrice3 = RWP.Sum;
                            ReplaceWaresReceipt(rw);
                        }
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
            //dbConfig?.Close(isWait);
            dbMid?.Close(isWait);
            dbRC?.Close(isWait);
            db?.Close(isWait);
        }

        ///////////////////////////////////////////////////////////////////
        /// Переробляю через відкриття закриття конекта.
        ///////////////////////////////////////////////////////////////////
        public bool SetConfig<T>(string parName, T parValue)//, SQL pDB = null)
        {
            string SqlReplaceConfig = "replace into CONFIG  (Name_Var,Data_Var,Type_Var) values (@NameVar,@DataVar,@TypeVar);";
            return dbConfig.ExecuteNonQuery<object>(SqlReplaceConfig, new { NameVar = parName, DataVar = parValue, @TypeVar = parValue.GetType().ToString() }) > 0;
        }

        public bool ReplaceWorkPlace(IEnumerable<WorkPlace> parData)
        {
            string SqlReplaceWorkPlace = @"replace into WORKPLACE(ID_WORKPLACE, NAME, Video_Camera_IP, Video_Recorder_IP, Type_POS, Code_Warehouse, CODE_DEALER, Prefix, DNSName, TypeWorkplace, SettingsEx) values
            (@IdWorkplace, @Name,  @VideoCameraIP, @VideoRecorderIP, @TypePOS, @CodeWarehouse, @CodeDealer, @Prefix, @DNSName, @TypeWorkplace, @SettingsEx);";

            return dbConfig.BulkExecuteNonQuery<WorkPlace>(SqlReplaceWorkPlace, parData, true) > 0;
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
            return dbConfig.ExecuteNonQuery<Object>(SqlInsertWeight, parWeight) > 0;
        }

        public bool InsertAddWeight(AddWeight parAddWeight)
        {
            string SqlInsertAddWeight = "insert into ADD_WEIGHT(CODE_WARES, CODE_UNIT, WEIGHT) values(@CodeWares, @CodeUnit, @Weight);";
            return dbMid.ExecuteNonQuery<AddWeight>(SqlInsertAddWeight, parAddWeight) > 0;
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
            NewDBRC();
            string SqlGetNewReceipt = @"
INSERT OR ignore into GEN_WORKPLACE(ID_WORKPLACE, CODE_PERIOD, CODE_RECEIPT) values(@IdWorkplace, @CodePeriod, @CodeReceipt);
            update GEN_WORKPLACE set CODE_RECEIPT = CODE_RECEIPT + 1 where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod;
            --insert into receipt(id_workplace, code_period, code_receipt) values(@IdWorkplace, @CodePeriod, (select CODE_RECEIPT from GEN_WORKPLACE where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod));
            select CODE_RECEIPT from GEN_WORKPLACE where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod;";

            string SqlGetNewReceipt2 = @"insert into receipt (id_workplace, code_period, code_receipt) values (@IdWorkplace,@CodePeriod,@CodeReceipt);";

            lock (GetObjectForLockByIdWorkplace(pIdReceipt.IdWorkplace))
            {
                if (pIdReceipt.CodePeriod == 0)
                    pIdReceipt.CodePeriod = Global.GetCodePeriod();
                pIdReceipt.CodeReceipt = dbConfig.ExecuteScalar<IdReceipt, int>(SqlGetNewReceipt, pIdReceipt);
                dbRC.ExecuteNonQuery<IdReceipt>(SqlGetNewReceipt2, pIdReceipt);
            }
            return pIdReceipt;
        }

        public bool ReplaceReceipt(Receipt parReceipt) => dbRC.ExecuteNonQuery<Receipt>(SqlReplaceReceipt, parReceipt) > 0;

        public bool CloseReceipt(Receipt pR)
        {
            lock (GetObjectForLockByIdWorkplace(pR.IdWorkplace))
            {
                if (DT == pR.DTPeriod)
                    return dbRC.ExecuteNonQuery<Receipt>(SqlCloseReceipt, pR) > 0;
                else
                {
                    using var dbRCD = GetRC(pR.DTPeriod);// new SQLite(GetReceiptFile());
                    return dbRCD.ExecuteNonQuery<Receipt>(SqlCloseReceipt, pR) > 0;
                }
            }
        }

        /// <summary>
        /// Повертає фактичну кількість після вставки(добавляє до текучої кількості - -1 якщо помилка;
        /// </summary>
        /// <param name="parParameters"></param>
        /// <returns></returns>
        public bool AddWares(ReceiptWares parReceiptWares) => dbRC.ExecuteNonQuery<ReceiptWares>(SqlInsertWaresReceipt, parReceiptWares) > 0;

        public bool ReplaceWaresReceipt(ReceiptWares parReceiptWares) => dbRC.ExecuteNonQuery<ReceiptWares>(SqlReplaceWaresReceipt, parReceiptWares) > 0;

        public bool UpdateQuantityWares(ReceiptWares parReceiptWares)
        {
            lock (GetObjectForLockByIdWorkplace(parReceiptWares.IdWorkplace))
            {
                return dbRC.ExecuteNonQuery(SqlUpdateQuantityWares, parReceiptWares) > 0 /*&& RecalcHeadReceipt(parParameters)*/;
            }
        }

        public bool DeleteReceiptWares(ReceiptWares parIdReceiptWares) => dbRC.ExecuteNonQuery<IdReceiptWares>(SqlDeleteReceiptWares, parIdReceiptWares) > 0;

        public bool RecalcHeadReceipt(IdReceipt parReceipt) => dbRC.ExecuteNonQuery<IdReceipt>(SqlRecalcHeadReceipt, parReceipt) > 0;

        public bool DeleteWaresReceiptPromotion(IdReceipt parIdReceipt)
        {
            string SqlDeleteWaresReceiptPromotion = @"delete from  WARES_RECEIPT_PROMOTION 
   where id_workplace=@IdWorkplace and  code_period =@CodePeriod and  code_receipt=@CodeReceipt and CODE_PS<>999999 and (BARCODE_2_CATEGORY is null or length(BARCODE_2_CATEGORY)=0 );";
            return dbRC.ExecuteNonQuery<IdReceipt>(SqlDeleteWaresReceiptPromotion, parIdReceipt) > 0;
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
            dbRC.BulkExecuteNonQuery<WaresReceiptPromotion>(SqlReplaceWaresReceiptPromotion, parData);
            dbRC.BulkExecuteNonQuery<WaresReceiptPromotion>(Sql, parData);
            return true;
        }


        public bool ReplacePayment(Payment pData, bool pIsDel = false)
        {
            if (pIsDel)
            {
                string Sql = @"delete from payment where  id_workplace=@IdWorkplace and id_workplace_pay = @IdWorkplacePay and  code_period =@CodePeriod and  code_receipt=@CodeReceipt and Type_Pay=@TypePay";
                dbRC.ExecuteNonQuery<Payment>(Sql, pData);
            }
            dbRC.ExecuteNonQuery<Payment>(SqlReplacePayment, pData);
            return true;
        }

        public bool ReplacePayments(IEnumerable<Payment> parData)
        {

            if (parData != null && parData.Count() > 0)
            {
                if (parData.Count() == 1)//Костиль через проблеми з мультипоточністю BD
                    return ReplacePayment(parData.First());
                else
                    return dbRC.BulkExecuteNonQuery<Payment>(SqlReplacePayment, parData) > 0;
            }

            return true;
        }

        public bool SetStateReceipt(Receipt pR)
        {
            string SqlSetStateReceipt = @"update receipt  set STATE_RECEIPT = @StateReceipt where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod and CODE_RECEIPT = @CodeReceipt";
            if (DT == pR.DTPeriod)
                return dbRC.ExecuteNonQuery<Receipt>(SqlSetStateReceipt, pR) > 0;
            else
            {
                using var dbRCD = GetRC(pR.DTPeriod);
                return dbRCD.ExecuteNonQuery<Receipt>(SqlSetStateReceipt, pR) > 0;
            }
        }

        public bool InsertBarCode2Cat(WaresReceiptPromotion parWRP)
        {
            dbRC.ExecuteNonQuery<WaresReceiptPromotion>(SqlInsertBarCode2Cat, parWRP);
            return true;
        }

        public bool SetRefundedQuantity(ReceiptWares parReceiptWares)
        {
            string SqlSetRefundedQuantity = @"
update wares_receipt set Refunded_Quantity = Refunded_Quantity + @Quantity
  where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod and CODE_RECEIPT = @CodeReceipt and Code_wares = @CodeWares;";
            return dbRC.ExecuteNonQuery<ReceiptWares>(SqlSetRefundedQuantity, parReceiptWares) > 0;
        }

        public bool InsertReceiptEvent(IEnumerable<ReceiptEvent> parRE)
        {
            string SqlInsertReceiptEvent = @"
insert into RECEIPT_Event(
    ID_WORKPLACE, CODE_PERIOD, CODE_RECEIPT, CODE_WARES, CODE_UNIT,
    Product_Name,
    Event_Type,
    Event_Name,
    Product_Weight,
    Product_Confirmed_Weight,    
    User_Name,
    Created_At,
    Resolved_At,
    Refund_Amount,
    Fiscal_Number,
    Payment_Type,
    Total_Amount
    ) VALUES
    (@IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @CodeUnit,    
    @ProductName,
    @EventType,
    @EventName,
    @ProductWeight,
    @ProductConfirmedWeight,
    @UserName,
    @CreatedAt,
    @ResolvedAt,
    @RefundAmount,
    @FiscalNumber,
    @PaymentType,
    @TotalAmount);";
            return dbRC.BulkExecuteNonQuery<ReceiptEvent>(SqlInsertReceiptEvent, parRE) > 0;
        }

        public bool DeleteReceiptEvent(IdReceipt parIdReceipt)
        {
            string SqlDeleteReceiptEvent = @"delete from RECEIPT_Event where id_workplace = @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt; --and EVENT_TYPE > 0;";

            return dbRC.ExecuteNonQuery<IdReceipt>(SqlDeleteReceiptEvent, parIdReceipt) > 0;
        }

        public bool FixWeight(ReceiptWares parIdReceipt)
        {
            string SqlSetFixWeight = @"update wares_receipt set Fix_Weight = @FixWeight, Fix_Weight_Quantity = @FixWeightQuantity
  where ID_WORKPLACE = @IdWorkplace and CODE_PERIOD = @CodePeriod and CODE_RECEIPT = @CodeReceipt  and Code_wares = @CodeWares;";
            return dbRC.ExecuteNonQuery<ReceiptWares>(SqlSetFixWeight, parIdReceipt) > 0;
        }
        ////////////////////////// MID

        public bool CreateMIDTable() { return dbMid.ExecuteNonQuery(SqlCreateMIDTable) > 0; }
        public bool CreateMIDIndex(SQLite pDB) { return pDB.ExecuteNonQuery(SqlCreateMIDIndex) > 0; }

        public bool ReplaceUnitDimension(IEnumerable<UnitDimension> parData, SQLite pDB)
        {
            string SqlReplaceUnitDimension = "replace into UNIT_DIMENSION(CODE_UNIT, NAME_UNIT, ABR_UNIT) values(@CodeUnit, @NameUnit, @AbrUnit);";
            return pDB.BulkExecuteNonQuery<UnitDimension>(SqlReplaceUnitDimension, parData, true) > 0;
        }

        public bool ReplaceGroupWares(IEnumerable<GroupWares> parData, SQLite pDB)
        {
            string SqlReplaceGroupWares = @" replace into  GROUP_WARES(CODE_GROUP_WARES, CODE_PARENT_GROUP_WARES, NAME) values (@CodeGroupWares, @CodeParentGroupWares, @Name);";
            return pDB.BulkExecuteNonQuery<GroupWares>(SqlReplaceGroupWares, parData, true) > 0;
        }

        public bool ReplaceWares(IEnumerable<Wares> parData, SQLite pDB)
        {
            return pDB.BulkExecuteNonQuery<Wares>(SqlReplaceWares, parData, true) > 0;
        }

        public bool ReplaceAdditionUnit(IEnumerable<AdditionUnit> parData, SQLite pDB)
        {
            string SqlReplaceAdditionUnit = @"replace into  Addition_Unit(CODE_WARES, CODE_UNIT, COEFFICIENT, DEFAULT_UNIT, WEIGHT, WEIGHT_NET)
              values(@CodeWares, @CodeUnit, @Coefficient, @DefaultUnit, @Weight, @WeightNet);";
            return pDB.BulkExecuteNonQuery<AdditionUnit>(SqlReplaceAdditionUnit, parData, true) > 0;
        }

        public bool ReplaceBarCode(IEnumerable<Barcode> parData, SQLite pDB)
        {
            string SqlReplaceBarCode = "replace into  Bar_Code(CODE_WARES, CODE_UNIT, BAR_CODE) values(@CodeWares, @CodeUnit, @BarCode);";
            pDB.BulkExecuteNonQuery<Barcode>(SqlReplaceBarCode, parData, true);
            return true;
        }
        public bool ReplacePrice(IEnumerable<Price> parData, SQLite pDB)
        {
            string SqlReplacePrice = "replace into PRICE(CODE_DEALER, CODE_WARES, PRICE_DEALER) values(@CodeDealer, @CodeWares, @PriceDealer);";
            return pDB.BulkExecuteNonQuery<Price>(SqlReplacePrice, parData, true) > 0;
        }

        public bool ReplaceTypeDiscount(IEnumerable<TypeDiscount> parData, SQLite pDB)
        {
            string SqlReplaceTypeDiscount = @"replace into TYPE_DISCOUNT(TYPE_DISCOUNT, NAME, PERCENT_DISCOUNT) values(@CodeTypeDiscount, @Name, @PercentDiscount);";
            pDB.BulkExecuteNonQuery<TypeDiscount>(SqlReplaceTypeDiscount, parData, true);
            return true;
        }

        public bool ReplaceClient(IEnumerable<Client> parData, SQLite pDB)
        {
            string SqlReplaceClient = @"replace into CLIENT(CODE_CLIENT, NAME_CLIENT, TYPE_DISCOUNT, PHONE, Phone_Add, PERCENT_DISCOUNT, BARCODE, STATUS_CARD, view_code, BirthDay)
             values(@CodeClient, @NameClient, @TypeDiscount, @MainPhone, @PhoneAdd, @PersentDiscount, @BarCode, @StatusCard, @ViewCode, @BirthDay)";
            pDB.BulkExecuteNonQuery<Client>(SqlReplaceClient, parData, true);
            return true;
        }

        public bool ReplaceFastGroup(IEnumerable<FastGroup> parData, SQLite pDB)
        {
            string SqlReplaceFastGroup = @"replace into FAST_GROUP(CODE_UP, Code_Fast_Group, Name,Image) values(@CodeUp, @CodeFastGroup, @Name,@Image);";
            pDB.BulkExecuteNonQuery<FastGroup>(SqlReplaceFastGroup, parData, true);
            return true;
        }

        public bool ReplaceFastWares(IEnumerable<FastWares> parData, SQLite pDB)
        {
            string SqlReplaceFastWares = "replace into FAST_WARES(Code_Fast_Group, Code_wares, Order_Wares) values(@CodeFastGroup, @CodeWares, @OrderWares);";
            return pDB.BulkExecuteNonQuery<FastWares>(SqlReplaceFastWares, parData, true) > 0;
        }

        public bool ReplacePromotionSale(IEnumerable<PromotionSale> parData, SQLite pDB = null)
        {
            pDB.ExecuteNonQuery("delete from PROMOTION_SALE", new { }, pDB.Transaction);
            return pDB.BulkExecuteNonQuery<PromotionSale>(SqlReplacePromotionSale, parData, true) > 0;
        }

        public bool ReplacePromotionSaleData(IEnumerable<PromotionSaleData> parData, SQLite pDB)
        {
            pDB.ExecuteNonQuery("delete from PROMOTION_SALE_DATA", new { }, pDB.Transaction);
            string SqlReplacePromotionSaleData = @"
replace into PROMOTION_SALE_DATA(CODE_PS, NUMBER_GROUP, CODE_WARES, USE_INDICATIVE, TYPE_DISCOUNT, ADDITIONAL_CONDITION, DATA, DATA_ADDITIONAL_CONDITION)
                          values(@CodePS, @NumberGroup, @CodeWares, @UseIndicative, @TypeDiscount, @AdditionalCondition, @Data, @DataAdditionalCondition)";
            return pDB.BulkExecuteNonQuery<PromotionSaleData>(SqlReplacePromotionSaleData, parData, true) > 0;
        }

        public bool ReplacePromotionSaleFilter(IEnumerable<PromotionSaleFilter> parData, SQLite pDB)
        {
            pDB.ExecuteNonQuery("delete from PROMOTION_SALE_FILTER", new { }, pDB.Transaction);
            string SqlReplacePromotionSaleFilter = @"
replace into PROMOTION_SALE_FILTER(CODE_PS, CODE_GROUP_FILTER, TYPE_GROUP_FILTER, RULE_GROUP_FILTER, CODE_CHOICE, CODE_DATA, CODE_DATA_END)
                          values(@CodePS, @CodeGroupFilter, @TypeGroupFilter, @RuleGroupFilter, @CodeChoice, @CodeData, @CodeDataEnd)";
            return pDB.BulkExecuteNonQuery<PromotionSaleFilter>(SqlReplacePromotionSaleFilter, parData, true) > 0;
        }

        public bool ReplacePromotionSaleDealer(IEnumerable<PromotionSaleDealer> parData, SQLite pDB)
        {
            pDB.ExecuteNonQuery("delete from PROMOTION_SALE_DEALER", new { }, pDB.Transaction);
            string SqlReplacePromotionSaleDealer = @"replace into PROMOTION_SALE_DEALER(CODE_PS, Code_Wares, DATE_BEGIN, DATE_END, Code_Dealer, Priority) values(@CodePS, @CodeWares, @DateBegin, @DateEnd, @CodeDealer, @Priority); --@CodePS,@CodeWares,@DateBegin,@DateEnd,@CodeDealer";
            return pDB.BulkExecuteNonQuery<PromotionSaleDealer>(SqlReplacePromotionSaleDealer, parData, true) > 0;
        }

        public bool ReplacePromotionSaleGroupWares(IEnumerable<PromotionSaleGroupWares> parData, SQLite pDB)
        {
            pDB.ExecuteNonQuery("delete from PROMOTION_SALE_GROUP_WARES", new { }, pDB.Transaction);
            string SqlReplacePromotionSaleGroupWares = @"replace into PROMOTION_SALE_GROUP_WARES(CODE_GROUP_WARES_PS, CODE_GROUP_WARES) values(@CodeGroupWaresPS, @CodeGroupWares)";
            return pDB.BulkExecuteNonQuery<PromotionSaleGroupWares>(SqlReplacePromotionSaleGroupWares, parData, true) > 0;
        }

        public bool ReplacePromotionSaleGift(IEnumerable<PromotionSaleGift> parData, SQLite pDB)
        {
            pDB.ExecuteNonQuery("delete from PROMOTION_SALE_GIFT", new { }, pDB.Transaction);
            string SqlReplacePromotionSaleGift = @"replace into PROMOTION_SALE_GIFT(CODE_PS, NUMBER_GROUP, CODE_WARES, TYPE_DISCOUNT, DATA, QUANTITY, DATE_CREATE, USER_CREATE)
                          values(@CodePS, @NumberGroup, @CodeWares, @TypeDiscount, @Data, @Quantity, @DateCreate, @UserCreate);";
            return pDB.BulkExecuteNonQuery<PromotionSaleGift>(SqlReplacePromotionSaleGift, parData, true) > 0;
        }

        public bool ReplacePromotionSale2Category(IEnumerable<PromotionSale2Category> parData, SQLite pDB)
        {
            pDB.ExecuteNonQuery("delete from PROMOTION_SALE_2_category", new { }, pDB.Transaction);
            string SqlReplacePromotionSale2Category = "replace into PROMOTION_SALE_2_category(CODE_PS, CODE_WARES) values(@CodePS, @CodeWares)";
            return pDB.BulkExecuteNonQuery<PromotionSale2Category>(SqlReplacePromotionSale2Category, parData, true) > 0;
        }

        public bool UpdateQR(ReceiptWares pRW)
        {
            string SqlUpdateQR = "update wares_receipt set QR = @QR where id_workplace = @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt  and code_wares = @CodeWares";
            return dbRC.ExecuteNonQuery<ReceiptWares>(SqlUpdateQR, pRW) > 0;
            //return true;
        }

        public bool UpdateExciseStamp(IEnumerable<ReceiptWares> pRW)
        {
            string SqlUpdateExciseStamp = @"update wares_receipt set Excise_Stamp = @ExciseStamp
                     where id_workplace = @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt and code_wares = @CodeWares";
            return dbRC.BulkExecuteNonQuery<ReceiptWares>(SqlUpdateExciseStamp, pRW) > 0;
        }

        public IEnumerable<QR> GetQR(IdReceipt pR)
        {
            string SqlGetQR = @"select wr.QR,'(' || w.PLU || ')-' || w.name_wares as name from wares_receipt wr
    join wares w on wr.code_wares = w.code_wares
        where wr.id_workplace = @IdWorkplace and wr.code_period = @CodePeriod and wr.code_receipt = @CodeReceipt and QR is not null;";
            return db.Execute<IdReceipt, QR>(SqlGetQR, pR);
        }

        public bool ReplaceUser(IEnumerable<User> pUser, SQLite pDB = null)
        {
            string SqlReplaceUser = "replace into User(CODE_USER, NAME_USER, BAR_CODE, Type_User, LOGIN, PASSWORD) values(@CodeUser, @NameUser, @BarCode, @TypeUser, @Login, @PassWord);";

            return pDB.BulkExecuteNonQuery<User>(SqlReplaceUser, pUser, true) > 0;

        }
        public bool ReplaceSalesBan(IEnumerable<SalesBan> pSB, SQLite pDB)
        {
            string SqlReplaceSalesBan = "replace into  Sales_Ban(CODE_GROUP_WARES, Amount) values(@CodeGroupWares, @Amount);";
            return pDB.BulkExecuteNonQuery<SalesBan>(SqlReplaceSalesBan, pSB, true) > 0;
        }

        public bool InsertLogRRO(IEnumerable<LogRRO> pLog)
        {
            string SQL = @"insert into Log_RRO  (ID_WORKPLACE,ID_WORKPLACE_PAY,TypePay,CODE_PERIOD,CODE_RECEIPT,FiscalNumber, Number_Operation,Type_Operation, SUM ,Type_RRO,JSON, Text_Receipt,Error,CodeError, USER_CREATE) VALUES
                     (@IdWorkplace, @IdWorkplacePay,@TypePay,@CodePeriod,@CodeReceipt,@FiscalNumber,@NumberOperation,@TypeOperation,@SUM,@TypeRRO,@JSON,@TextReceipt,@Error,@CodeError,@UserCreate)";
            return dbRC.BulkExecuteNonQuery<LogRRO>(SQL, pLog) > 0;
        }

        public bool AddFiscalArticle(FiscalArticle pFiscalArticle)
        {
            var SQL = "replace into FiscalArticle (IdWorkplacePay,CodeWares,NameWares ,PLU ,Price) values (@IdWorkplacePay,@CodeWares,@NameWares ,@PLU ,@Price)";
            return dbConfig.ExecuteNonQuery<FiscalArticle>(SQL, pFiscalArticle) > 0;
        }


        public bool DelAllFiscalArticle()
        {
            var SQL = "delete from FiscalArticle";
            return dbConfig.ExecuteNonQuery(SQL) > 0;
        }

        public FiscalArticle GetFiscalArticle(ReceiptWares pRW)
        {
            var SQL = "select * from FiscalArticle where CodeWares=@CodeWares and IdWorkplacePay=@IdWorkplacePay";
            return dbConfig.Connection.QueryFirstOrDefault<FiscalArticle>(SQL, new FiscalArticle() { CodeWares = pRW.CodeWares, IdWorkplacePay = pRW.IdWorkplacePay });
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
            return dbRC.ExecuteNonQuery<IdReceipt>(SQL, pIdR) > 0;
        }

        public bool ReplaceClientNew(ClientNew pC)
        {
            string Sql = @"replace into  Client_New (ID_WORKPLACE, BARCODE_CLIENT, BARCODE_CASHIER,  PHONE) values  (@IdWorkplace, @BarcodeClient, @BarcodeCashier, @Phone);";
            return dbRC.ExecuteNonQuery<ClientNew>(Sql, pC) > 0;
        }

        public bool SetConfirmClientNew(ClientNew pC)
        {
            string Sql = @"update Client_New set STATE=1 where Barcode_Client = @BarcodeClient and Phone=@Phone";
            return dbRC.ExecuteNonQuery<ClientNew>(Sql, pC) > 0;
        }

        public bool ReplaceClientData(IEnumerable<ClientData> pCD, SQLite pDB = null)
        {
            string Sql = @"replace into ClientData (TypeData,CodeClient,Data) values (@TypeData,@CodeClient,@Data)";
            return pDB.BulkExecuteNonQuery<ClientData>(Sql, pCD, true) > 0;
        }
        /// <summary>
        /// Повертає знайденого клієнта(клієнтів)
        /// </summary>
        /// <param name="parCodeWares">Код товару</param>
        /// <returns>
        ///Повертає  IEnumerable<Client> з клієнтами
        ///</returns>
        public IEnumerable<Client> FindClient(string parBarCode = null, string parPhone = null, string parName = null, long parCodeClient = 0)
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
            if (DT == pR.DTPeriod)
                return dbRC.Execute<IdReceipt, LogRRO>(SqlGetLogRRO, pR);
            else
            {
                using var dbRCD = GetRC(pR.DTPeriod);
                return dbRCD.Execute<IdReceipt, LogRRO>(SqlGetLogRRO, pR);
            }
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
                        el.AdditionalWeights = db.Execute<object, decimal>(SqlAdditionalWeightsWares, new { el.CodeWares });

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
                        el.ReceiptWaresPromotions = wrp.Where(rr => ((IdReceiptWares)rr).Equals((IdReceiptWares)el)).ToArray();
                    el.WaresLink = GetLinkWares(el);
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
            string SqlGetWorkplace = @"select ID_WORKPLACE as IdWorkplace, NAME as Name,
       Video_Camera_IP as VideoCameraIP, Video_Recorder_IP as VideoRecorderIP , Type_POS as TypePOS,
       Code_Warehouse as CodeWarehouse ,CODE_DEALER as CodeDealer,Prefix, DNSName,TypeWorkplace ,SettingsEx from WORKPLACE;";
            return db.Execute<WorkPlace>(SqlGetWorkplace);
        }


        public virtual IEnumerable<FastGroup> GetFastGroup(int parCodeUpFastGroup)
        {
            string SqlGetFastGroup = "select code_up as CodeUp,CODE_FAST_GROUP as CodeFastGroup,NAME,Image from FAST_GROUP where code_up = @CodeUp order by CODE_FAST_GROUP";
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

        public bool ReplaceMRC(IEnumerable<MRC> parData, SQLite pDB)
        {
            string SqlReplaceMRC = " replace into  MRC(Code_Wares, Price, Type_Wares) values(@CodeWares, @Price, @TypeWares);";

            return pDB.BulkExecuteNonQuery<MRC>(SqlReplaceMRC, parData, true) > 0;

        }

        public IEnumerable<Payment> GetPayment(IdReceipt pIdR)
        {
            string SqlGetPayment = @"select id_workplace as IdWorkplace, id_workplace_pay as IdWorkplacePay,code_period as CodePeriod, code_receipt as CodeReceipt, 
 TYPE_PAY as TypePay, Code_Bank as CodeBank, CODE_WARES as CodeWares, SUM_PAY as SumPay, SUM_EXT as SumExt,
    NUMBER_TERMINAL as NumberTerminal,   NUMBER_RECEIPT as NumberReceipt, CODE_AUTHORIZATION as CodeAuthorization, NUMBER_SLIP as NumberSlip,
    Pos_Paid as PosPaid, Pos_Add_Amount as PosAddAmount, DATE_CREATE as DateCreate,Number_Card as NumberCard,
    Card_Holder as CardHolder ,Issuer_Name as IssuerName, Bank,TransactionId
   from payment
  where   id_workplace = @IdWorkplace and code_period = @CodePeriod and code_receipt = @CodeReceipt";
            if (DT == pIdR.DTPeriod)
                return db.Execute<IdReceipt, Payment>(SqlGetPayment, pIdR);
            else
            {
                using var dbRCD = GetRC(pIdR.DTPeriod);
                return dbRCD.Execute<IdReceipt, Payment>(SqlGetPayment, pIdR);
            }
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
            return db.Execute<object, Receipt>(SqlGetReceipts, new { StartDate = parStartDate, FinishDate = parFinishDate, IdWorkplace = parIdWorkPlace });
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
            if (DT == pR.DTPeriod)
                return db.ExecuteScalar<IdReceipt, eStateReceipt>(SqlGetStateReceipt, pR);
            else
            {
                using var dbRCD = GetRC(pR.DTPeriod);
                return dbRCD.ExecuteScalar<IdReceipt, eStateReceipt>(SqlGetStateReceipt, pR);
            }
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

        public bool ReplaceWaresWarehouse(IEnumerable<WaresWarehouse> pWW, SQLite pDB)
        {
            bool Res = false;
            if (pWW?.Any() == true)
            {
                string SQL = "replace into WaresWarehouse (CodeWarehouse,TypeData,Data) values (@CodeWarehouse,@TypeData,@Data)";

                Res = pDB.BulkExecuteNonQuery<WaresWarehouse>(SQL, pWW, true) > 0;
                if (Res)
                    BildWaresWarehouse(pWW);
            }
            return Res;
        }


        public bool ReplaceWaresLink(IEnumerable<WaresLink> pWW, SQLite pDB)
        {
            bool Res = false;
            if (pWW?.Any() == true)
            {
                string SQL = "replace into WaresLink (CodeWares,CodeWaresTo,IsDefault,Sort) values (@CodeWares,@CodeWaresTo,@IsDefault,@Sort)";
                Res = pDB.BulkExecuteNonQuery<WaresLink>(SQL, pWW, true) > 0;               
            }
            return Res;
        }

        public void BildWaresWarehouse(IEnumerable<WaresWarehouse> pWW = null)
        {
            pWW ??= GetWaresWarehouse();

            if (Global.Settings!=null&& Global.Settings.IdWorkPlaceLink > 0 && Global.Settings.CodeWarehouseLink > 0)
            {
                foreach (var el in pWW)
                {
                    if (el.TypeData == eTypeData.Directions && !Global.IdWorkPlacePayDirection.ContainsKey(el.Data))
                        Global.IdWorkPlacePayDirection.Add(el.Data, el.CodeWarehouse == Global.Settings.CodeWarehouseLink ? Global.Settings.IdWorkPlaceLink : Global.IdWorkPlace);

                    if (el.TypeData == eTypeData.Brand && !Global.IdWorkPlacePayTM.ContainsKey(el.Data))
                        Global.IdWorkPlacePayTM.Add(el.Data, el.CodeWarehouse == Global.Settings.CodeWarehouseLink ? Global.Settings.IdWorkPlaceLink : Global.IdWorkPlace);

                    if (el.TypeData == eTypeData.Group && !Global.IdWorkPlacePayGroup.ContainsKey(el.Data))
                        Global.IdWorkPlacePayGroup.Add(el.Data, el.CodeWarehouse == Global.Settings.CodeWarehouseLink ? Global.Settings.IdWorkPlaceLink : Global.IdWorkPlace);

                    if (el.TypeData == eTypeData.Wares && !Global.IdWorkPlacePayWares.ContainsKey(el.Data))
                        Global.IdWorkPlacePayWares.Add(el.Data, el.CodeWarehouse == Global.Settings.CodeWarehouseLink ? Global.Settings.IdWorkPlaceLink : Global.IdWorkPlace);
                }
            }
        }

        public bool IsUseBarCode2Category(string pBarCode)
        {
            string SQL = $"select count(BARCODE_2_CATEGORY)  from WARES_RECEIPT_PROMOTION where BARCODE_2_CATEGORY='{pBarCode}'";
            var Res = db.ExecuteScalar<bool>(SQL);
            return Res;
        }

        public bool ReplaceWaresReceiptLink(IEnumerable<WaresReceiptLink> pWRL,bool IsDelete=true)
        {
            try
            {
                string SQL = @"delete from  WaresReceiptLink where  IdWorkplace = @IdWorkplace and CodePeriod = @CodePeriod and CodeReceipt = @CodeReceipt and CodeWaresTo = @CodeWares";
                if (IsDelete && pWRL.Any())                
                    dbRC.ExecuteNonQuery<WaresReceiptLink>(SQL, pWRL.FirstOrDefault());
                

                SQL = @"replace into WaresReceiptLink  (IdWorkplace, CodePeriod, CodeReceipt, CodeWares, Sort, CodeWaresTo, Quantity) VALUES
                                                         (@IdWorkplace,@CodePeriod,@CodeReceipt,@CodeWares,@Sort,@CodeWaresTo,@Quantity)";
                return dbRC.BulkExecuteNonQuery<WaresReceiptLink>(SQL, pWRL) > 0;
            }catch (Exception e) { FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e); }
            return false;
        }

        public IEnumerable<GW> GetLinkWares(IdReceiptWares pIdRW)
        {
            try
            {
                string SQL = @"select 0 as Type, w.CODE_WARES as code, w.NAME_WARES as name, w.Code_Unit as CodeUnit, count(*) over() as TotalRows,
    case when wrl.CodeWares is not null then 1 else 0 end as IsSelected
    from WaresLink wl
    join wares w on (wl.CodeWares = w.Code_wares )
    left join WaresReceiptLink wrl on (wl.CodeWares = wrl.CodeWares and
    wrl.IdWorkplace = @IdWorkplace and wrl.CodePeriod = @CodePeriod and wrl.CodeReceipt = @CodeReceipt)
    where wl.CodeWaresTo = @CodeWares
 order by wl.sort;";
                    /*$@"select 0 as Type, w.CODE_WARES as code, w.NAME_WARES as name, w.Code_Unit as CodeUnit, count(*) over() as TotalRows 
    from WaresLink wl join  wares w on wl.CodeWares = w.Code_wares where wl.CodeWaresTo=@CodeWares order by wl.sort";*/
            var Res = db.Execute< IdReceiptWares,GW>(SQL,pIdRW);
            return Res;
            }
            catch (Exception e) { FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e); }
            return null;
        }
       
    }
}