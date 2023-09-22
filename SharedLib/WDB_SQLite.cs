using System;
using System.Collections.Generic;
using System.Data;
//using DB_SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ModelMID;
using ModelMID.DB;
using Utils;

namespace SharedLib
{
    public partial class WDB_SQLite : WDB
    {
        private static bool IsFirstStart = true;   
        
        protected string SqlCreateMIDTable = @"";
        protected string SqlCreateMIDIndex = @"";
        protected string SqlGetPricePromotionKit = @"";

        private DateTime DT = DateTime.Today;

        private string Connect = null;
        private string ConfigFile { get { return Path.Combine(ModelMID.Global.PathDB, "config.db"); } }
        private string LastMidFile = null;
        private string MidFile { get { return string.IsNullOrEmpty(Connect) ? (string.IsNullOrEmpty(LastMidFile) ? GetMIDFile() : LastMidFile) : Connect; } }

        private string GetReceiptFile(DateTime pDT) { return Path.Combine(ModelMID.Global.PathDB, $"{pDT:yyyyMM}", $"Rc_{ModelMID.Global.IdWorkPlace}_{pDT:yyyyMMdd}.db"); }

        private string ReceiptFile { get { return GetReceiptFile(DT); } }

        public string GetMIDFile(DateTime pD = default,bool pTmp=false)
        {
            DateTime Date = pD == default ? DateTime.Today : pD;
            return Path.Combine(Global.PathDB, $"{Date:yyyyMM}", $"MID_{Date:yyyyMMdd}{(pTmp?"_tmp":"")}.db");
        }

        public WDB_SQLite(DateTime parD = default(DateTime), string pConnect = null, bool pIsUseOldDB = true, bool pIsCreateMidFile=false ) : base(Path.Combine(Global.PathIni, "SQLite.sql"))
        {
            Connect = pConnect;
            Version = "SQLite.0.0.1";
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
                    LFDB=LFDB.AddDays(-1);
                } while (LastMidFile==null && LFDB > DateTime.Now.AddDays(-10));
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

        private new bool InitSQL()
        {
            SqlCreateMIDTable = GetSQL("SqlCreateMIDTable");
            SqlCreateMIDIndex = GetSQL("SqlCreateMIDIndex");
            SqlGetPricePromotionKit = GetSQL("SqlGetPricePromotionKit");
            return true;
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

        public override bool RecalcPrice(IdReceiptWares pIdReceiptWares)
        {
            var startTime = System.Diagnostics.Stopwatch.StartNew();
            var State = GetStateReceipt(pIdReceiptWares);
            if (State != eStateReceipt.Prepare)
                return true;
            var Type = GetTypeReceipt(pIdReceiptWares);
            if (Type==eTypeReceipt.Refund)
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
                        par.Quantity =RW.Quantity;
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
                db?.Close(isWait);
        }

        ///////////////////////////////////////////////////////////////////
        /// Переробляю через відкриття закриття конекта.
        ///////////////////////////////////////////////////////////////////
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
            using (var DB = new SQLite(MidFile))
            {
                return DB.ExecuteNonQuery<AddWeight>(SqlInsertAddWeight, parAddWeight) > 0;
            }
        }
        ////////////////// RC

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdReceipt"></param>
        /// <returns></returns>
        public override IdReceipt GetNewReceipt(IdReceipt pIdReceipt)
        {
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

        public bool ReplacePayment(Payment pData,bool pIsDel=false)
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

        public override bool ReplacePayments(IEnumerable<Payment> parData)
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

        public  bool ReplacePromotionSale(IEnumerable<PromotionSale> parData)
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
                return DB.BulkExecuteNonQuery<User>(SqlReplaceUser, pUser,true) > 0;
            }
        }
        public override bool ReplaceSalesBan(IEnumerable<SalesBan> pSB)
        {
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
                return DB.ExecuteNonQuery<FiscalArticle>(SQL,pFiscalArticle) > 0;
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
                return DB.connection.QueryFirstOrDefault<FiscalArticle>(SQL,new FiscalArticle() { CodeWares=pRW.CodeWares,IdWorkplacePay=pRW.IdWorkplacePay} );
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
        protected override bool UpdateDB(ref bool pIsUseOld)
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

        public  bool ReplaceClientNew(ClientNew pC)
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
        public override IEnumerable<Client> FindClient(string parBarCode = null, string parPhone = null, string parName = null, int parCodeClient = 0)
        {
           
            var Res= db.Execute<object, Client>(SqlFindClient, new { CodeClient = parCodeClient, Phone = parPhone, BarCode = parBarCode, Name = (parName == null ? null : "%" + parName + "%") });
            return Res != null && Res.Any(el => el.StatusCard == eStatusCard.Active)?Res.Where(el => el.StatusCard == eStatusCard.Active) : Res ;
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


    }
}