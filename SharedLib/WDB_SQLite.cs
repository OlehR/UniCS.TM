using System;
using System.Collections.Generic;
using System.Data;
//using DB_SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModelMID;
using ModelMID.DB;

namespace SharedLib
{

    
    public partial class WDB_SQLite : WDB
    {
        //private SQLite db;
        //public SQLite db_receipt;
        
   

        protected string SqlCreateMIDTable = @"";
        protected string SqlCreateMIDIndex = @"";
        protected string SqlGetPricePromotionKit = @"";

        private DateTime DT = DateTime.Today;

        private string Connect = null;
        private string ConfigFile { get { return Path.Combine(ModelMID.Global.PathDB, "config.db"); } }
        private string MidFile { get { return string.IsNullOrEmpty(Connect) ? Path.Combine(GetCurrentMIDFile) : Connect; } }

        private string GetReceiptFile() { return Path.Combine(ModelMID.Global.PathDB, $"{DT:yyyyMM}", $"Rc_{ModelMID.Global.IdWorkPlace}_{DT:yyyyMMdd}.db"); }

        private string ReceiptFile { get { return GetReceiptFile(); } }

        /*if (pIsUseOldDB &&!File.Exists(MidFile) && string.IsNullOrEmpty(parConnect))
            {
                var varLastMidFile = GetConfig<string>("Last_MID");
                if (!string.IsNullOrEmpty(varLastMidFile))                
                    MidFile = varLastMidFile;
            }*/

        public string GetCurrentMIDFile 
                {get { DateTime varD = DateTime.Today;
                       return Path.Combine(Global.PathDB, $"{varD:yyyyMM}", $"MID_{varD:yyyyMMdd}.db"); } }
  
        public WDB_SQLite( DateTime parD = default(DateTime), string parConnect = "", bool pIsUseOldDB = false)  : base(Path.Combine(Global.PathIni, "SQLite.sql") )
        {
            Connect = parConnect;
            varVersion = "SQLite.0.0.1";
            InitSQL();

            if(parD != default(DateTime))
                DT=parD.Date;
            
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
                var db = new SQLite(MidFile);
                db.ExecuteNonQuery(SqlCreateMIDTable);
                db.Close();
                db = null;
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
            SqlGetPricePromotionKit= GetSQL("SqlGetPricePromotionKit");
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
        public Task RecalcPriceAsync(IdReceiptWares parIdReceiptWares)
        {
            return Task.Run(() => RecalcPrice(parIdReceiptWares)).ContinueWith(async res =>
            {
                if (await res)
                {
                    //Console.WriteLine(OnReceiptCalculationComplete != null);
                    var r = ViewReceiptWares(new IdReceiptWares(parIdReceiptWares,0),true);//вертаємо весь чек.
                    Global.OnReceiptCalculationComplete?.Invoke(r, Global.GetTerminalIdByIdWorkplace(parIdReceiptWares.IdWorkplace));
                }
            });
        }

        public override bool RecalcPrice(IdReceiptWares parIdReceipt)
        {
            var startTime = System.Diagnostics.Stopwatch.StartNew();

            lock (GetObjectForLockByIdWorkplace(parIdReceipt.IdWorkplace))
            {
                try
                {
                    var RH = ViewReceipt(parIdReceipt);
                    ParameterPromotion par;
                    var InfoClient = GetInfoClientByReceipt(parIdReceipt);
                    if (InfoClient.Count() == 1)
                        par = InfoClient.First();                        
                    else
                      par = new ParameterPromotion();                    
                    
                    //par.BirthDay = DateTime.Now.Date; Test
                    par.CodeWarehouse = Global.CodeWarehouse;
                    par.Time = Convert.ToInt32(RH.DateReceipt.ToString("HHmm"));
                    par.CodeDealer = Global.DefaultCodeDealer;

                    var r = ViewReceiptWares(parIdReceipt);

                    foreach (var RW in r)
                    {
                        var MPI = GetMinPriceIndicative((IdReceiptWares)RW);
                        par.CodeWares = RW.CodeWares;
                        var Res = GetPrice(par);

                        if (Res != null)
                        {
                            RW.Price = MPI.GetPrice(Res.Price, Res.IsIgnoreMinPrice == 0, Res.CodePs > 0);
                            RW.TypePrice = MPI.typePrice;
                            RW.ParPrice1 = Res.CodePs;
                            RW.ParPrice2 = (int)Res.TypeDiscont;
                            RW.ParPrice3 = Res.Data;
                            RW.Priority = Res.Priority;
                        }

                        ReplaceWaresReceipt(RW);
                    }
                    GetPricePromotionKit(parIdReceipt, parIdReceipt.CodeWares);
                    RecalcHeadReceipt(parIdReceipt);
                    startTime.Stop();
                    Console.WriteLine($"RecalcPrice=>{startTime.Elapsed}  {r?.Count()}");
                    /*foreach (var RW in r)
                        Console.WriteLine($"{RW.NameWares}");*/

                    return true;
                }
                catch (Exception ex)
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(parIdReceipt.IdWorkplace), Exception = ex, Status =eSyncStatus.Error,StatusDescription=ex.Message });
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
        public bool GetPricePromotionKit(IdReceipt parIdReceipt,int parCodeWares)
        {
            if (parCodeWares > 0 && !IsWaresInPromotionKit(parCodeWares)) 
                return true;

            var varRes = new List<WaresReceiptPromotion>(); 
            var par = new ParamPricePromotionKit(parIdReceipt, ModelMID.Global.CodeWarehouse);
            var r=db.Execute<ParamPricePromotionKit, PromotionWaresKit>(SqlGetPricePromotionKit, par);
            int NumberGroup = 0;
            decimal Quantity = 0, AddQuantity=0;
            Int64 CodePS = 0;
            var RW = ViewReceiptWares(parIdReceipt);
            foreach (var el in r )//цикл по Можливим позиціям з знижкою.
            {
                if(el.CodePS!= CodePS||el.NumberGroup!=NumberGroup)
                {
                    Quantity = el.Quantity;
                    CodePS = el.CodePS;
                    NumberGroup = el.NumberGroup;
                }
                if (Quantity > 0) // Надаєм знижку на інші позиції набору.
                {
                    var varQuantityReceipt = RW.Where(e => e.CodeWares == el.CodeWares).Sum(e => e.Quantity);
                    var varQuantityUsed = varRes.Where(e => e.CodeWares == el.CodeWares && e.NumberGroup==el.NumberGroup).Sum(e => e.Quantity);
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
                        if (el.TypeDiscount==eTypeDiscount.PercentDiscount)
                        {
                            var Price= varQuantityReceipt = RW.Where(e => e.CodeWares == el.CodeWares).Sum(e => e.Price);
                            vPrice = Price * el.DataDiscount / 100m;
                        }                      

                        var RWP = new WaresReceiptPromotion(parIdReceipt) { CodeWares = el.CodeWares, Quantity = AddQuantity, Price=vPrice,CodePS=el.CodePS,NumberGroup=el.NumberGroup};
                        varRes.Add(RWP);
                    }
                }

            }
            DeleteWaresReceiptPromotion(parIdReceipt);
            if(varRes.Count>0)
              ReplaceWaresReceiptPromotion(varRes);

            return true;

        }



        public override bool CopyWaresReturnReceipt(IdReceipt parIdReceipt, bool parIsCurrentDay = true)
		{
			string SqlCopyWaresReturnReceipt=(parIsCurrentDay? this.SqlCopyWaresReturnReceipt.Replace("RRC.","RC.") : this.SqlCopyWaresReturnReceipt ) ;
			return (this.db.ExecuteNonQuery(SqlCopyWaresReturnReceipt, parIdReceipt) ==0);
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

        public override bool SetConfig<T>(string parName, T parValue)
        {
            using (var DB = new SQLite(ConfigFile))
            {
                parValue.GetType().ToString();
                DB.ExecuteNonQuery<object>(this.SqlReplaceConfig, new { NameVar = parName, DataVar = parValue, @TypeVar = parValue.GetType().ToString() });
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
                return DB.ExecuteNonQuery<Object>(SqlInsertWeight, parWeight)>0;
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
                return DB.ExecuteNonQuery<Receipt>(SqlAddReceipt, parReceipt) == 0;
            }
        }

        public override bool ReplaceReceipt(Receipt parReceipt)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<Receipt>(SqlReplaceReceipt, parReceipt) == 0;
            }
        }

        public override bool UpdateClient(IdReceipt parIdReceipt, int parCodeClient)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                lock (GetObjectForLockByIdWorkplace(parIdReceipt.IdWorkplace))
                {
                    return DB.ExecuteNonQuery<IdReceipt>(SqlUpdateClient, parIdReceipt) == 0;
                }
            }
        }

        public override bool CloseReceipt(Receipt parReceipt)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                lock (GetObjectForLockByIdWorkplace(parReceipt.IdWorkplace))
                {
                    return DB.ExecuteNonQuery<Receipt>(SqlCloseReceipt, parReceipt) == 0;
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
                return DB.ExecuteNonQuery<ReceiptWares>(SqlInsertWaresReceipt, parReceiptWares) == 0 /*&& RecalcHeadReceipt((IdReceipt)parReceiptWares)*/;
            }
        }

        public override bool ReplaceWaresReceipt(ReceiptWares parReceiptWares)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<ReceiptWares>(SqlReplaceWaresReceipt, parReceiptWares) == 0;
            }
        }

        public override bool UpdateQuantityWares(ReceiptWares parIdReceiptWares)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                lock (GetObjectForLockByIdWorkplace(parIdReceiptWares.IdWorkplace))
                {
                    return DB.ExecuteNonQuery(SqlUpdateQuantityWares, parIdReceiptWares) == 0 /*&& RecalcHeadReceipt(parParameters)*/;
                }
            }
        }

        public override bool DeleteReceiptWares(IdReceiptWares parIdReceiptWares)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<IdReceiptWares>(SqlDeleteReceiptWares, parIdReceiptWares) == 0 /*&& RecalcHeadReceipt(parParameters)*/;
            }
        }

        public override bool RecalcHeadReceipt(IdReceipt parReceipt)
        {
            using (var DB = new SQLite(ReceiptFile))
            {
                return DB.ExecuteNonQuery<IdReceipt>(this.SqlRecalcHeadReceipt, parReceipt) == 0;
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
            using (var DB = new SQLite(ReceiptFile))
            {
                DB.BulkExecuteNonQuery<WaresReceiptPromotion>(SqlReplaceWaresReceiptPromotion, parData);
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

    }
}