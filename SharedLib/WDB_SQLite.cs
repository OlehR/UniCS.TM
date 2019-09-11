using System;
using System.Collections.Generic;
using System.Data;
//using DB_SQLite;
using System.IO;
using System.Linq;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parCallWriteLogSQL"></param>
        public WDB_SQLite(string parConnect = "") : base(Path.Combine(GlobalVar.PathIni, "SQLite.sql") )
        {
            varVersion = "SQLite.0.0.1";
            InitSQL();
            DateTime varD = DateTime.Today;
            string varReceiptFile = Path.Combine(GlobalVar.PathDB,$"{varD:yyyyMM}" ,$"Rc_{GlobalVar.IdWorkPlace}_{varD:yyyyMMdd}.db");
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

            db = new SQLite(string.IsNullOrEmpty(parConnect) ? Path.Combine(GlobalVar.PathDB,  @"MID.db") : parConnect);//,"",this.varCallWriteLogSQL);
                                                                  //this.db.ExecuteNonQuery("ATTACH ':memory:' AS m");
            db.ExecuteNonQuery(this.SqlCreateT);
            db.ExecuteNonQuery("ATTACH '" + varReceiptFile + "' AS rc");
        }

        private new bool InitSQL()
        {
            SqlCreateMIDTable = GetSQL("SqlCreateMIDTable");
            SqlCreateMIDIndex = GetSQL("SqlCreateMIDIndex");
            return true;
        }
        public override RezultFind FindData(string parStr, TypeFind parTypeFind = TypeFind.All)
        {
            RezultFind varRezult;
            varRezult.Count = 0;
            varRezult.TypeFind = TypeFind.All;
            string varStr = parStr.Trim();
            Int64 varNumber = 0;
            Int64.TryParse(varStr, out varNumber);
            this.db.ExecuteNonQuery("delete from T$1");
            // Шукаемо Товар

            if (parTypeFind != TypeFind.Client)
            {
                varRezult.TypeFind = TypeFind.Wares;
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

            if (parTypeFind != TypeFind.Wares)
            {
                varRezult.TypeFind = TypeFind.Client;
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
				varRezult.TypeFind=TypeFind.All;
				
			return varRezult;
			
		}

        public override RezultFind FindClientByPhone(string parPhone)
        {
            ClearT1();
            this.db.ExecuteNonQuery<object>(SqlFindClientPhone, new { Phone = parPhone });
            return new RezultFind() { Count = GetCountT1(), TypeFind = TypeFind.Client };
        }

        public override RezultFind FindWaresByName(string parName)
        {
            ClearT1();
            this.db.ExecuteNonQuery<object>(SqlFindWaresName, new { Name = "%"+ parName.Trim().Replace(" ","%") + "%" });
            return new RezultFind() { Count = GetCountT1(), TypeFind = TypeFind.Wares };
        }              				
	
        public override bool RecalcPrice(IdReceipt parIdReceipt)
        {
            var RH = ViewReceipt(parIdReceipt);

            var par = new ParameterPromotion() { CodeWarehouse = 9/*RH.CodeWarehouse*/, BirthDay= DateTime.Now.AddDays(-3).Date, Time=Convert.ToInt32( RH.DateReceipt.ToString("HHmm")), TypeCard=-1};

            var PercentDiscount = GetPersentDiscountClientByReceipt(parIdReceipt);
            var r = ViewReceiptWares(parIdReceipt);
            
            foreach (var RW in r)
            {
                var MPI = GetMinPriceIndicative((IdReceiptWares)RW);
                par.CodeWares = RW.CodeWares;
                var Res = GetPrice(par);
                if(Res!=null && Res.PriceDealer>0)
                {
                    RW.Price = MPI.GetPricePromotion( Res.PriceDealer);
                    RW.TypePrice = MPI.typePrice;
                    RW.ParPrice1 = Res.CodePs;                   
                }
                else
                {
                    RW.Price = MPI.GetPrice(RW.PriceDealer, PercentDiscount);
                    RW.TypePrice = MPI.typePrice;
                }
                ReplaceWaresReceipt(RW);
            }
            RecalcHeadReceipt(parIdReceipt);
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
            db.ExecuteNonQuery(SqlGetWaresFromFastGroup, new { CodeFastGroup= parCodeFastGroup });
            return FindWares();
        }                 

    }
}