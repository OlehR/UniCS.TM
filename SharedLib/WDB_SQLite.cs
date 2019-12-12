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
        public SQLite db_receipt;
        
        protected string SqlCreateMIDTable = @"";
        protected string SqlCreateMIDIndex = @"";
        protected string SqlGetPricePromotionKit = @"";
        public string GetCurrentMIDFile 
                {get { DateTime varD = DateTime.Today;
                       return Path.Combine(Global.PathDB, $"{varD:yyyyMM}", $"MID_{varD:yyyyMMdd}.db"); } }


  
        public WDB_SQLite(string parConnect = "",bool IsMidMain=false,DateTime parD = default(DateTime))  : base(Path.Combine(Global.PathIni, "SQLite.sql") )
        {
            varVersion = "SQLite.0.0.1";
            InitSQL();

            DateTime varD = ( parD == default(DateTime)? DateTime.Today: parD);
            var ConfigFile = Path.Combine(ModelMID.Global.PathDB, "config.db");
            if (!File.Exists(ConfigFile))
            {
                db = new SQLite(ConfigFile);
                db.ExecuteNonQuery(SqlCreateConfigTable);
                db.Close();
            }
            db = new SQLite(ConfigFile);//,"",this.varCallWriteLogSQL);

            string varReceiptFile = Path.Combine(ModelMID.Global.PathDB, $"{varD:yyyyMM}", $"Rc_{ModelMID.Global.IdWorkPlace}_{varD:yyyyMMdd}.db");
            if (!File.Exists(varReceiptFile))
            {
                var receiptFilePath = Path.GetDirectoryName(varReceiptFile);
                if (!Directory.Exists(receiptFilePath))
                    Directory.CreateDirectory(receiptFilePath);
                //Створюємо щоденну табличку з чеками.
                var db = new SQLite(varReceiptFile);
                db.ExecuteNonQuery(SqlCreateReceiptTable);
                db.Close();                
            }
            
            var MidFile = string.IsNullOrEmpty(parConnect) ? Path.Combine(GetCurrentMIDFile) : parConnect;
            if (!File.Exists(MidFile) && string.IsNullOrEmpty(parConnect))
            {
                var varLastMidFile = GetConfig<string>("Last_MID");
                if (!string.IsNullOrEmpty(varLastMidFile))                
                    MidFile = varLastMidFile;
            }

            if (!File.Exists(MidFile))
            {
                var db = new SQLite(MidFile);
                db.ExecuteNonQuery(SqlCreateMIDTable);
                db.Close();                
            }

            //   db = new SQLite(string.IsNullOrEmpty(parConnect) ? Path.Combine(ModelMID.Global.PathDB,  @"MID.db") : parConnect);//,"",this.varCallWriteLogSQL);
            //this.db.ExecuteNonQuery("ATTACH ':memory:' AS m");
            
            if(IsMidMain)
            {
                db = new SQLite(MidFile);
                db.ExecuteNonQuery("ATTACH '" + ConfigFile + "' AS con");
            }
            else
            {
                db = new SQLite(ConfigFile);
                db.ExecuteNonQuery("ATTACH '" + MidFile + "' AS mid");
            }

            //db.ExecuteNonQuery("ATTACH '" + MidFile + "' AS mid");
            db.ExecuteNonQuery("ATTACH '" + varReceiptFile + "' AS rc");
            BildWorkplace();
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
                    var r = ViewReceiptWares(new IdReceiptWares(parIdReceiptWares,0));//вертаємо весь чек.
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
                            RW.ParPrice3 = (int)Res.Data;
                        }

                        ReplaceWaresReceipt(RW);
                    }
                    GetPricePromotionKit(parIdReceipt, parIdReceipt.CodeWares);
                    RecalcHeadReceipt(parIdReceipt);
                    startTime.Stop();
                    Console.WriteLine("RecalcPrice=>" + startTime.Elapsed);

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
                        var RWP = new WaresReceiptPromotion(parIdReceipt) { CodeWares = el.CodeWares, Quantity = AddQuantity, Price=el.Price,CodePS=el.CodePS,NumberGroup=el.NumberGroup};
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

        public bool CreateMIDTable()
        {
            db.ExecuteNonQuery(SqlCreateMIDTable);
            return true;
        }
        public bool CreateMIDIndex()
        {
            db.ExecuteNonQuery(SqlCreateMIDIndex);
            return true;
        }

        
        public override IEnumerable<ReceiptWares> GetWaresFromFastGroup(int parCodeFastGroup)
        {
            return FindWares(null, null, 0, 0, parCodeFastGroup);
        }

        public override IEnumerable<ReceiptWares> GetBags()
        {
            return FindWares(null, null, 0, 0, Global.CodeFastGroupBag);
        }



    }
}