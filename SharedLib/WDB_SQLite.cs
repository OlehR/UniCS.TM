using System;
using System.Collections.Generic;
using System.Data;
//using DB_SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModelMID;
using ModelMID.DB;
using Utils;

namespace SharedLib
{


    public partial class WDB_SQLite : WDB
    {
        //private SQLite db;
        //public SQLite db_receipt;
        private static bool IsFirstStart = true;


        protected string SqlCreateMIDTable = @"";
        protected string SqlCreateMIDIndex = @"";
        protected string SqlGetPricePromotionKit = @"";

        private DateTime DT = DateTime.Today;

        private string Connect = null;
        private string ConfigFile { get { return Path.Combine(ModelMID.Global.PathDB, "config.db"); } }
        private string LastMidFile = null;
        private string MidFile { get { return string.IsNullOrEmpty(Connect) ? (string.IsNullOrEmpty(LastMidFile) ? GetCurrentMIDFile : LastMidFile) : Connect; } }

        private string GetReceiptFile(DateTime pDT) { return Path.Combine(ModelMID.Global.PathDB, $"{pDT:yyyyMM}", $"Rc_{ModelMID.Global.IdWorkPlace}_{pDT:yyyyMMdd}.db"); }

        private string ReceiptFile { get { return GetReceiptFile(DT); } }

        /*if (pIsUseOldDB &&!File.Exists(MidFile) && string.IsNullOrEmpty(parConnect))
            {
                var varLastMidFile = GetConfig<string>("Last_MID");
                if (!string.IsNullOrEmpty(varLastMidFile))                
                    MidFile = varLastMidFile;
            }*/

        public string GetCurrentMIDFile
        {
            get
            {
                DateTime varD = DateTime.Today;
                return Path.Combine(Global.PathDB, $"{varD:yyyyMM}", $"MID_{varD:yyyyMMdd}.db");
            }
        }

        public WDB_SQLite(DateTime parD = default(DateTime), string parConnect = "", bool pIsUseOldDB = false) : base(Path.Combine(Global.PathIni, "SQLite.sql"))
        {
            Connect = parConnect;
            varVersion = "SQLite.0.0.1";
            DT = parD != default(DateTime) ? parD.Date : DateTime.Today.Date;
            InitSQL();


            if (IsFirstStart)
                UpdateDB(ref pIsUseOldDB);


            if (!File.Exists(ConfigFile))
            {
                db = new SQLite(ConfigFile);
                db.ExecuteNonQuery(SqlCreateConfigTable);
                db.Close();
            }
            //db = new SQLite(ConfigFile);//,"",this.varCallWriteLogSQL);


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


            if (!File.Exists(MidFile))
            {
                if (pIsUseOldDB)
                {
                    db = new SQLite(ConfigFile);
                    var varLastMidFile = GetConfig<string>("Last_MID");
                    db = null;
                    if (!string.IsNullOrEmpty(varLastMidFile) && File.Exists(varLastMidFile))
                        LastMidFile = varLastMidFile;
                }
                if (!pIsUseOldDB || string.IsNullOrEmpty(LastMidFile))
                {
                    var db = new SQLite(MidFile);
                    db.ExecuteNonQuery(SqlCreateMIDTable);
                    db.Close();
                    db = null;
                }
            }
            /*
            if (pTypeDb == eTypeDb.AllMid || pTypeDb == eTypeDb.AllRC)
            {
                if (pTypeDb == eTypeDb.AllMid)
                {
                    db = new SQLite(MidFile);
                    db.ExecuteNonQuery("ATTACH '" + ReceiptFile + "' AS rc");
                }
                else
                {
                    db = new SQLite(ReceiptFile);
                    db.ExecuteNonQuery("ATTACH '" + MidFile + "' AS mid");
                    //db.ExecuteNonQuery("ATTACH '" + ConfigFile + "' AS con");
                }
                db.ExecuteNonQuery("ATTACH '" + ConfigFile + "' AS con");
            }
           
            if(pTypeDb == eTypeDb.Mid)
                db = new SQLite(MidFile);

            if(pTypeDb == eTypeDb.RC)
                db = new SQLite(ReceiptFile);

            if (pTypeDb == eTypeDb.Config)
                db = new SQLite(ConfigFile);
            */

            db = new SQLite(ReceiptFile);
            db.ExecuteNonQuery("ATTACH '" + MidFile + "' AS mid");
            db.ExecuteNonQuery("ATTACH '" + ConfigFile + "' AS con");

            db.ExecuteNonQuery("PRAGMA synchronous = EXTRA;");
            db.ExecuteNonQuery("PRAGMA journal_mode = DELETE;");
            db.ExecuteNonQuery("PRAGMA wal_autocheckpoint = 5;");
        }
        ~WDB_SQLite()
        {
            //Close();
        }

        private new bool InitSQL()
        {
            SqlCreateMIDTable = GetSQL("SqlCreateMIDTable");
            SqlCreateMIDIndex = GetSQL("SqlCreateMIDIndex");
            SqlGetPricePromotionKit = GetSQL("SqlGetPricePromotionKit");
            return true;
        }
        /*
                public override eRezultFind FindData(string parStr, eTypeFind parTypeFind = eTypeFind.All)
                {
                    eRezultFind varRezult;
                    varRezult.Count = 0;
                    varRezult.TypeFind = eTypeFind.All;
                    string varStr = parStr.Trim();
                    Int64 varNumber = 0;
                    Int64.TryParse(varStr, out varNumber);
                    this.db.ExecuteNonQuery("delete from T$1");
                    // Шукаемо Товар

                    if (parTypeFind != eTypeFind.Client)
                    {
                        varRezult.TypeFind = eTypeFind.Wares;
                        if (varNumber > 0)
                        {
                            if (varStr.Length >= GlobalVar.MinLenghtBarCodeWares)
                            {//Шукаємо по штрихкоду
                                this.db.ExecuteNonQuery(this.SqlFindWaresBar, new { BarCode = varStr });
                            }
                            else//Шукаемо по коду
                            {
                                if (GlobalVar.TypeFindWares < 2)
                                    this.db.ExecuteNonQuery(this.SqlFindWaresCode + varStr);
                            }
                        }
                        else // Шукаємо по назві
                        {
                            if (GlobalVar.TypeFindWares == 0)//Можна шукати по назві
                                this.db.ExecuteNonQuery(SqlFindWaresName + "'%" + varStr.ToUpper().Replace(" ", "%") + "%'");
                        }
                        varRezult.Count = this.GetCountT1();

                        if (varRezult.Count > 0) return varRezult;//Знайшли товар
                    }
                    // ШукаемоКлієнта

                    if (parTypeFind != eTypeFind.Wares)
                    {
                        varRezult.TypeFind = eTypeFind.Client;
                        if (varNumber > 0)
                        {
                            if (varStr.Length >= GlobalVar.MinLenghtBarCodeClient)
                            {//Шукаємо по штрихкоду

                                this.db.ExecuteNonQuery(SqlFindClientBar, new { BarCode = varStr });

                            }
                            else
                                if (GlobalVar.TypeFindClient < 2)
                            {
                                this.db.ExecuteNonQuery<object>(SqlFindClientCode, new { CodePrivat = varStr });
                            }
                        }
                        else // Пошук по назві
                        {
                            if (GlobalVar.TypeFindClient == 0)//Можна шукати по назві
                                this.db.ExecuteNonQuery(SqlFindClientName + "'%" + varStr.Replace(" ", "%") + "%'");

                        }
                    }
                    varRezult.Count=this.GetCountT1();

                    if( varRezult.Count==0) 
                        varRezult.TypeFind=eTypeFind.All;

                    return varRezult;

                }
                */
        public Task RecalcPriceAsync(IdReceiptWares pIdReceiptWares)
        {
            return Task.Run(() => RecalcPrice(pIdReceiptWares)).ContinueWith(async res =>
            {
                if (await res)
                {
                    //Console.WriteLine(OnReceiptCalculationComplete != null);
                    var r = ViewReceiptWares(new IdReceiptWares(pIdReceiptWares, 0), true);//вертаємо весь чек.
                    Global.OnReceiptCalculationComplete?.Invoke(r, pIdReceiptWares);
                    if (r == null || r.Count() == 0)
                        return;
                    var parW = r.Last();
                    if (parW != null)
                    {
                        var SumAll = r.Sum(d => d.Sum - d.SumDiscount);
                        _ = VR.SendMessageAsync(parW.IdWorkplace, parW.NameWares, parW.Articl, parW.Quantity, parW.Sum, VR.eTypeVRMessage.AddWares, SumAll);
                    }
                }
            });
        }

        public override bool RecalcPrice(IdReceiptWares pIdReceiptWares)
        {
            var startTime = System.Diagnostics.Stopwatch.StartNew();

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
                        var Res = GetPrice(par);

                        if (Res != null && RW.ParPrice1 != 999999 && (Res.Priority > 0 || string.IsNullOrEmpty(RW.BarCode2Category)))//Не перераховуємо для  Сигарет s для 2 категорії окрім пріоритет 1
                        {
                            RW.Price = MPI.GetPrice(Res.Price, Res.IsIgnoreMinPrice == 0, Res.CodePs > 0);
                            RW.TypePrice = MPI.typePrice;
                            RW.ParPrice1 = Res.CodePs;
                            RW.ParPrice2 = (int)Res.TypeDiscont;
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
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(pIdReceiptWares.IdWorkplace), Exception = ex, Status = eSyncStatus.Error, StatusDescription = "RecalcPrice=>" + ex.Message + '\n' + new System.Diagnostics.StackTrace().ToString() });
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



        public override bool CopyWaresReturnReceipt(IdReceipt parIdReceipt, bool parIsCurrentDay = true)
        {
            string SqlCopyWaresReturnReceipt = (parIsCurrentDay ? this.SqlCopyWaresReturnReceipt.Replace("RRC.", "RC.") : this.SqlCopyWaresReturnReceipt);
            return (this.db.ExecuteNonQuery(SqlCopyWaresReturnReceipt, parIdReceipt) > 0);
        }



        public override IEnumerable<ReceiptWares> GetWaresFromFastGroup(int parCodeFastGroup)
        {
            return FindWares(null, null, 0, 0, parCodeFastGroup);
        }

        public override IEnumerable<ReceiptWares> GetBags()
        {
            return FindWares(null, null, 0, 0, Global.CodeFastGroupBag);
        }

        public override void Close(bool isWait = false)
        {
            if (db != null)
                db.Close(isWait);
        }

        ///////////////////////////////////////////////////////////////////
        /// Переробляю через відкриття закриття конекта.
        ///////////////////////////////////////////////////////////////////
        ///

        ///////////////// Config

        public override bool SetConfig<T>(string parName, T parValue, SQL pDB = null)
        {
            using (var DB = new SQLite(ConfigFile))
            {

                if (pDB == null)
                    DB.ExecuteNonQuery<object>(this.SqlReplaceConfig, new { NameVar = parName, DataVar = parValue, @TypeVar = parValue.GetType().ToString() });
                else
                    pDB.ExecuteNonQuery<object>(this.SqlReplaceConfig, new { NameVar = parName, DataVar = parValue, @TypeVar = parValue.GetType().ToString() });
            }
            return true;
        }

        public override bool ReplaceWorkPlace(IEnumerable<WorkPlace> parData)
        {
            using (var DB = new SQLite(ConfigFile))
            {
                DB.BulkExecuteNonQuery<WorkPlace>(SqlReplaceWorkPlace, parData);
            }
            return true;
        }

        public override bool InsertWeight(Object parWeight)
        {
            using (var DB = new SQLite(ConfigFile))
            {
                return DB.ExecuteNonQuery<Object>(SqlInsertWeight, parWeight) > 0;
            }
        }

        public override bool InsertAddWeight(AddWeight parAddWeight)
        {
            using (var DB = new SQLite(ConfigFile))
            {
                return DB.ExecuteNonQuery<AddWeight>(SqlInsertAddWeight, parAddWeight) > 0;
            }
        }
        ////////////////// RC

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parIdReceipt"></param>
        /// <returns></returns>
        public override IdReceipt GetNewReceipt(IdReceipt parIdReceipt)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                lock (GetObjectForLockByIdWorkplace(parIdReceipt.IdWorkplace))
                {
                    if (parIdReceipt.CodePeriod == 0)
                        parIdReceipt.CodePeriod = Global.GetCodePeriod();
                    parIdReceipt.CodeReceipt = DB.ExecuteScalar<IdReceipt, int>(SqlGetNewReceipt, parIdReceipt);
                }
            }
            return parIdReceipt;
        }


        public override bool AddReceipt(Receipt parReceipt)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<Receipt>(SqlAddReceipt, parReceipt) > 0;
            }
        }

        public override bool ReplaceReceipt(Receipt parReceipt)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<Receipt>(SqlReplaceReceipt, parReceipt) > 0;
            }
        }

        public override bool UpdateClient(IdReceipt parIdReceipt, int parCodeClient)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                lock (GetObjectForLockByIdWorkplace(parIdReceipt.IdWorkplace))
                {
                    return DB.ExecuteNonQuery<IdReceipt>(SqlUpdateClient, parIdReceipt) > 0;
                }
            }
        }

        public override bool CloseReceipt(Receipt parReceipt)
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
        public override bool AddWares(ReceiptWares parReceiptWares)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<ReceiptWares>(SqlInsertWaresReceipt, parReceiptWares) > 0 /*&& RecalcHeadReceipt((IdReceipt)parReceiptWares)*/;
            }
        }

        public override bool ReplaceWaresReceipt(ReceiptWares parReceiptWares)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<ReceiptWares>(SqlReplaceWaresReceipt, parReceiptWares) > 0;
            }
        }

        public override bool UpdateQuantityWares(ReceiptWares parReceiptWares)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                lock (GetObjectForLockByIdWorkplace(parReceiptWares.IdWorkplace))
                {
                    return DB.ExecuteNonQuery(SqlUpdateQuantityWares, parReceiptWares) > 0 /*&& RecalcHeadReceipt(parParameters)*/;
                }
            }
        }

        public override bool DeleteReceiptWares(ReceiptWares parIdReceiptWares)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<IdReceiptWares>(SqlDeleteReceiptWares, parIdReceiptWares) > 0 /*&& RecalcHeadReceipt(parParameters)*/;
            }
        }

        public override bool RecalcHeadReceipt(IdReceipt parReceipt)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<IdReceipt>(this.SqlRecalcHeadReceipt, parReceipt) > 0;
            }
        }

        public override bool DeleteWaresReceiptPromotion(IdReceipt parIdReceipt)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                DB.ExecuteNonQuery<IdReceipt>(SqlDeleteWaresReceiptPromotion, parIdReceipt);
            }
            return true;
        }

        public override bool ReplaceWaresReceiptPromotion(IEnumerable<WaresReceiptPromotion> parData)
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

        public override bool MoveReceipt(ParamMoveReceipt parMoveReceipt)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                DB.ExecuteNonQuery<ParamMoveReceipt>(SqlMoveReceipt, parMoveReceipt);
            }
            return true;
        }
        public override bool ReplacePayment(IEnumerable<Payment> parData)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                if (parData != null && parData.Count() > 0)
                {
                    if (parData.Count() == 1)//Костиль через проблеми з мультипоточністю BD
                        DB.ExecuteNonQuery<Payment>(SqlReplacePayment, parData.First());
                    else
                        DB.BulkExecuteNonQuery<Payment>(SqlReplacePayment, parData);
                }
            }
            return true;
        }

        public override bool SetStateReceipt(Receipt parReceipt)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                DB.ExecuteNonQuery<Receipt>(SqlSetStateReceipt, parReceipt);
            }
            return true;
        }

        public override bool InsertBarCode2Cat(WaresReceiptPromotion parWRP)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                DB.ExecuteNonQuery<WaresReceiptPromotion>(SqlInsertBarCode2Cat, parWRP);
            }
            return true;
        }

        public override bool SetRefundedQuantity(ReceiptWares parReceiptWares)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<ReceiptWares>(SqlSetRefundedQuantity, parReceiptWares) > 0;
            }
        }

        public override bool InsertReceiptEvent(IEnumerable<ReceiptEvent> parRE)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.BulkExecuteNonQuery<ReceiptEvent>(SqlInsertReceiptEvent, parRE) > 0;
            }
        }

        public override bool DeleteReceiptEvent(IdReceipt parIdReceipt)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<IdReceipt>(SqlDeleteReceiptEvent, parIdReceipt) > 0;
            }
        }

        public override bool FixWeight(ReceiptWares parIdReceipt)
        {
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

        public override bool ReplaceUnitDimension(IEnumerable<UnitDimension> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<UnitDimension>(SqlReplaceUnitDimension, parData);
            }
            return true;
        }

        public override bool ReplaceGroupWares(IEnumerable<GroupWares> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<GroupWares>(SqlReplaceGroupWares, parData);
            }
            return true;
        }

        public override bool ReplaceWares(IEnumerable<Wares> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<Wares>(SqlReplaceWares, parData);
            }
            return true;
        }
        public override bool ReplaceAdditionUnit(IEnumerable<AdditionUnit> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<AdditionUnit>(SqlReplaceAdditionUnit, parData);
            }
            return true;
        }
        public override bool ReplaceBarCode(IEnumerable<Barcode> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<Barcode>(SqlReplaceBarCode, parData);
            }
            return true;
        }
        public override bool ReplacePrice(IEnumerable<Price> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<Price>(SqlReplacePrice, parData);
            }
            return true;
        }
        public override bool ReplaceTypeDiscount(IEnumerable<TypeDiscount> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<TypeDiscount>(SqlReplaceTypeDiscount, parData);
            }
            return true;
        }
        public override bool ReplaceClient(IEnumerable<Client> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<Client>(SqlReplaceClient, parData);
            }
            return true;
        }

        public override bool ReplaceFastGroup(IEnumerable<FastGroup> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<FastGroup>(SqlReplaceFastGroup, parData);
            }
            return true;
        }
        public override bool ReplaceFastWares(IEnumerable<FastWares> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<FastWares>(SqlReplaceFastWares, parData);
            }
            return true;
        }

        public override bool ReplacePromotionSale(IEnumerable<PromotionSale> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<PromotionSale>(SqlReplacePromotionSale, parData);
            }
            return true;
        }

        public override bool ReplacePromotionSaleData(IEnumerable<PromotionSaleData> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<PromotionSaleData>(SqlReplacePromotionSaleData, parData);
            }
            return true;
        }

        public override bool ReplacePromotionSaleFilter(IEnumerable<PromotionSaleFilter> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<PromotionSaleFilter>(SqlReplacePromotionSaleFilter, parData);
            }
            return true;
        }

        public override bool ReplacePromotionSaleDealer(IEnumerable<PromotionSaleDealer> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<PromotionSaleDealer>(SqlReplacePromotionSaleDealer, parData);
            }
            return true;
        }

        public override bool ReplacePromotionSaleGroupWares(IEnumerable<PromotionSaleGroupWares> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<PromotionSaleGroupWares>(SqlReplacePromotionSaleGroupWares, parData);
            }
            return true;
        }

        public override bool ReplacePromotionSaleGift(IEnumerable<PromotionSaleGift> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<PromotionSaleGift>(SqlReplacePromotionSaleGift, parData);
            }
            return true;
        }

        public override bool ReplacePromotionSale2Category(IEnumerable<PromotionSale2Category> parData)
        {
            using (var DB = new SQLite(MidFile))
            {
                DB.BulkExecuteNonQuery<PromotionSale2Category>(SqlReplacePromotionSale2Category, parData);
            }
            return true;
        }

        public override bool UpdateQR(ReceiptWares pRW)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<ReceiptWares>(SqlUpdateQR, pRW) > 0;
            }
            //return true;
        }

        public override bool UpdateExciseStamp(IEnumerable<ReceiptWares> pRW)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.BulkExecuteNonQuery<ReceiptWares>(SqlUpdateExciseStamp, pRW) > 0;
            }
        }

        public override bool ReplaceUser(IEnumerable<User> pUser)
        {
            using (var DB = new SQLite(MidFile))
            {
                return DB.BulkExecuteNonQuery<User>(SqlReplaceUser, pUser) > 0;
            }
        }
        public override bool ReplaceSalesBan(IEnumerable<SalesBan> pSB)
        {
            using (var DB = new SQLite(MidFile))
            {
                return DB.BulkExecuteNonQuery<SalesBan>(SqlReplaceSalesBan, pSB) > 0;
            }
        }

        public override bool InsertLogRRO(LogRRO pLog)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<LogRRO>(SqlInsertLogRRO, pLog) > 0;
            }
        }
        /// <summary>
        /// Оновлення структури бази даних
        /// </summary>       
        protected override void UpdateDB(ref bool pIsUseOld)
        {
            try
            {
                IsFirstStart = false;
                int CurVerConfig = 0, CurVerMid = 0, CurVerRC = 0;
                var Lines = SqlUpdateConfig.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                SQLite dbC = new SQLite(ConfigFile);
                var wDB = new WDB(Path.Combine(Global.PathIni, "SQLite.sql"), dbC);

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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected (int, bool) Parse(string[] pLines, int pCurVersion, SQLite pDB)
        {
            int NewVer = pCurVersion, Ver;
            bool isReload = false;

            foreach (var el in pLines)
            {
                var i = el.IndexOf("VER=>");
                if (i >= 0)
                {
                    string str = el.Substring(i + 5);
                    string[] All = str.Split(';');
                    if (int.TryParse(All[0], out Ver))
                    {
                        if (Ver > pCurVersion)
                            try
                            {
                                pDB.ExecuteNonQuery(el);
                                if (All.Length > 1 && All[1].Equals("Reload".ToUpper()))
                                    isReload = true;
                            }
                            catch (Exception e)
                            {
                                if (e.Message.IndexOf("duplicate column name:") == -1)
                                {
                                    FileLogger.WriteLogMessage($"WDB_SQLite.Parse Exception =>( el=>{el},pCurVersion=>{pCurVersion}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                                    throw new Exception(e.Message, e);
                                }
                            }
                        if (NewVer <= Ver)
                            NewVer = Ver;
                    }
                }
            }

            return (NewVer, isReload);
        }
    }
}