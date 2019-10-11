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
        protected string SqlGetPricePromotionKit= @"";
        public Action<IEnumerable<ReceiptWares>, Guid> OnReceiptCalculationComplete { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parCallWriteLogSQL"></param>
        public WDB_SQLite(string parConnect = "") : base(Path.Combine(ModelMID.Global.PathIni, "SQLite.sql") )
        {
            varVersion = "SQLite.0.0.1";
            InitSQL();
            DateTime varD = DateTime.Today;
            var ConfigFile = Path.Combine(ModelMID.Global.PathDB, "config.db");
            if (!File.Exists(ConfigFile))
            {
                db = new SQLite(ConfigFile);
                db.ExecuteNonQuery(SqlCreateConfigTable);
                db.Close();
            }
            string varReceiptFile = Path.Combine(ModelMID.Global.PathDB,$"{varD:yyyyMM}" ,$"Rc_{ModelMID.Global.IdWorkPlace}_{varD:yyyyMMdd}.db");
            if (!File.Exists(varReceiptFile))
            {
                var receiptFilePath = Path.GetDirectoryName(varReceiptFile);
                if (!Directory.Exists(receiptFilePath))
                    Directory.CreateDirectory(receiptFilePath);
                //Створюємо щоденну табличку з чеками.
                db = new SQLite(varReceiptFile);
                db.ExecuteNonQuery(SqlCreateReceiptTable);
                db.Close();
                
            }

            db = new SQLite(string.IsNullOrEmpty(parConnect) ? Path.Combine(ModelMID.Global.PathDB,  @"MID.db") : parConnect);//,"",this.varCallWriteLogSQL);
                                                                                                                        //this.db.ExecuteNonQuery("ATTACH ':memory:' AS m");
            db.ExecuteNonQuery("ATTACH '" + ConfigFile + "' AS con");
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

        public Task RecalcPriceAsync(IdReceipt parIdReceipt)
        {
            return Task.Run(() => RecalcPrice(parIdReceipt)).ContinueWith(res =>
            {
                if (res.Result)
                {
                    var r = ViewReceiptWares(parIdReceipt);          

                    OnReceiptCalculationComplete?.Invoke(ViewReceiptWares(parIdReceipt),Global.GetTerminalIdByIdWorkplace(parIdReceipt.IdWorkplace));
                }
            });
        }

        public override bool RecalcPrice(IdReceipt parIdReceipt)
        {
            var RH = ViewReceipt(parIdReceipt);

            var par = new ParameterPromotion() {
                CodeWarehouse = ModelMID.Global.CodeWarehouse,
                BirthDay = DateTime.Now.AddDays(-3).Date,
                Time = Convert.ToInt32( RH.DateReceipt.ToString("HHmm")),
                TypeCard = GetTypeDiscountClientByReceipt(parIdReceipt),
                CodeDealer= ModelMID.Global.DefaultCodeDealer
            };

            //var PercentDiscount = GetPersentDiscountClientByReceipt(parIdReceipt);

            var r = ViewReceiptWares(parIdReceipt);
            
            foreach (var RW in r)
            {
                var MPI = GetMinPriceIndicative((IdReceiptWares)RW);
                par.CodeWares = RW.CodeWares;
                var Res = GetPrice(par);

                Int64 CodePS2Cat = 0;
                decimal Percent2Cat = 0;
                if (RW.BarCode2Category != null && RW.BarCode2Category.Length == 13)
                {
                    Percent2Cat = Convert.ToInt32(RW.BarCode2Category.Substring(3, 2));
                    CodePS2Cat = GetPricePromotionSale2Category((IdReceiptWares)RW);
                }

                if(Res!=null )//&& Res.CodePs>0)
                {
                    RW.Price = MPI.GetPrice(Res.Price,Res.IsIgnoreMinPrice==0);
                    if (CodePS2Cat > 0 && Percent2Cat > 0)
                    {
                        RW.TypePrice = eTypePrice.Promotion;
                        RW.ParPrice1 = CodePS2Cat;
                    }
                    else
                    {
                        RW.TypePrice = MPI.typePrice;
                        RW.ParPrice1 = Res.CodePs;
                        RW.ParPrice2 = (int)Res.TypeDiscont;
                        RW.ParPrice3 = (int) Res.Data;
                    }
                    RW.Price = RW.Price * (100M - Percent2Cat) / 100;
                }
/*                else
                {
                    
                    if (CodePS2Cat > 0 && Percent2Cat > 0)
                    {
                        RW.Price = RW.PriceDealer * (100M - Percent2Cat) / 100;
                        RW.TypePrice = eTypePrice.Promotion;
                        RW.ParPrice1 = CodePS2Cat;
                    }
                    else
                    {
                        RW.Price = MPI.GetPrice(RW.PriceDealer, );
                        RW.TypePrice = MPI.typePrice;
                    }                        

                }*/

                ReplaceWaresReceipt(RW);
            }
            GetPricePromotionKit(parIdReceipt, r);
            RecalcHeadReceipt(parIdReceipt);
            return true;
        }
        /// <summary>
        /// Розраховуємо знижки по наборах
        /// Можливо зумію це зробити колись на рівні БД
        /// </summary>
        /// <param name="parIdReceipt"></param>
        /// <returns></returns>
        public bool GetPricePromotionKit(IdReceipt parIdReceipt,IEnumerable<ReceiptWares> parRW)
        {
            var varRes = new List<WaresReceiptPromotion>(); 
            var par = new ParamPricePromotionKit(parIdReceipt, ModelMID.Global.CodeWarehouse);
            var r=db.Execute<ParamPricePromotionKit, PromotionWaresKit>(SqlGetPricePromotionKit, par);
            int NumberGroup = 0;
            decimal Quantity = 0, AddQuantity=0;
            Int64 CodePS = 0;
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
                    var varQuantityReceipt = parRW.Where(e => e.CodeWares == el.CodeWares).Sum(e => e.Quantity);
                    var varQuantityUsed = varRes.Where(e => e.CodeWares == el.CodeWares).Sum(e => e.Quantity);
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

        

    }
}