using System;
using System.Collections.Generic;
using System.Data;
//using DB_SQLite;
using System.IO;
using System.Linq;
using ModelMID;

namespace SharedLib
{
    public partial class WDB_SQLite : WDB
    {
        private SQLite db;
        public SQLite db_receipt;
        public ReceiptWares varWares = new ReceiptWares();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parCallWriteLogSQL"></param>
        public WDB_SQLite() : base()
        {
            this.varVersion = "SQLite.0.0.1";

            this.ReadSQL(GlobalVar.PathIni + @"SQLite.sql");
            this.InitSQL();
            DateTime varD = DateTime.Today;
            string varReceiptFile = GlobalVar.PathDB + varD.ToString("yyyyMM") + @"\Rc_" + GlobalVar.IdWorkPlace.ToString() + "_" + varD.ToString("yyyyMMdd") + ".db";
            if (!File.Exists(varReceiptFile))
            {
                //Створюємо щоденну табличку з чеками.
                this.db = new SQLite(varReceiptFile);
                this.db.ExecuteNonQuery(SqlCreateReceiptTable);
                this.db.Close();
                
            }

            this.db = new SQLite(GlobalVar.PathDB + @"MID.db");//,"",this.varCallWriteLogSQL);
                                                                  //this.db.ExecuteNonQuery("ATTACH ':memory:' AS m");
            this.db.ExecuteNonQuery(this.SqlCreateT);
            this.db.ExecuteNonQuery("ATTACH '" + varReceiptFile + "' AS rc");
        }


        public override bool InsertT1(T1 parT1)
        {
            return this.db.ExecuteNonQuery(SqlInsertT1, parT1) == 0;
        }

        public override bool ClearT1()
        {
            return this.db.ExecuteNonQuery(SqlClearT1)==0;
        }

        /*		public override DataRow Login (string parLogin,string parPassword)
                {
                    var parColl = new Parameter[] { new Parameter {ColumnName= "parLogin",Value= parLogin}, new Parameter { ColumnName = "parPassWord", Value = parPassword } };

                    DataTable dt = db.Execute( SqlLogin,parColl);
                    if(dt == null || dt.Rows.Count==0)
                    {
                        //DataTable dt = new DataTable();
                        //dt.Columns.Add("code_user", typeof(int));
                        //dt.Columns.Add("name_user", typeof(string));
                        //dt.Columns.Add("login", typeof(string));
                        //dt.Columns.Add("password", typeof(string));
                        dt.Rows.Add(-2,"Неправильний логін чи пароль",parLogin,parPassword);
                        return dt.Rows[0];
                    }
                    return dt.Rows[0];
                }*/

        public override int GetCountT1()
        {
            return this.db.ExecuteScalar<int>(@"select count(*) cn from T$1");
        }

        /*		public override DataRow GetGlobalVar(int parIdWorkPlace)
                {
                    var varParameters = new Parameter[] { new Parameter { ColumnName = "parIdWorkPlace", Value = parIdWorkPlace } };

                    DataTable varDT=this.db.Execute(this.SqlInitGlobalVar, varParameters);
                    if(varDT!=null&&varDT.Rows.Count>0)
                        return varDT.Rows[0];
                    else
                        return null;
                }*/

        public override T GetConfig<T>(string parStr)
		{
			return this.db.ExecuteScalar<object,T>(this.SqlConfig,new {NameVar=parStr});
		
		}
		

		public override RezultFind FindData( string parStr,TypeFind parTypeFind = TypeFind.All)			
		{   
			RezultFind varRezult;
			varRezult.Count=0;
			string varStr =parStr.Trim();
			Int64 varNumber = 0;
			Int64.TryParse(varStr, out varNumber);
			this.db.ExecuteNonQuery("delete from T$1");			
			// Шукаемо Товар
			varRezult.TypeFind=TypeFind.Wares;
			if (varNumber>0)
			{
				if(varStr.Length>= GlobalVar.MinLenghtBarCodeWares)
				{//Шукаємо по штрихкоду
					this.db.ExecuteNonQuery(this.SqlFindWaresBar+varStr);
				}else//Шукаемо по коду
				{ if(GlobalVar.TypeFindWares<2)
						this.db.ExecuteNonQuery(this.SqlFindWaresCode+varStr);
				}
			} else // Шукаємо по назві
			{
				if(GlobalVar.TypeFindWares == 0)//Можна шукати по назві
					this.db.ExecuteNonQuery(SqlFindWaresName+"'%"+varStr.ToUpper().Replace(" ","%")+"%'");
			}
			varRezult.Count=this.GetCountT1();
			
			if( varRezult.Count>0) return varRezult;//Знайшли товар
			// ШукаемоКлієнта
			varRezult.TypeFind=TypeFind.Client;
			if(varNumber>0)
			{
				if(varStr.Length>= GlobalVar.MinLenghtBarCodeClient)
				{//Шукаємо по штрихкоду
                    
					this.db.ExecuteNonQuery(SqlFindClientBar, new { CodeBar = varStr });
					
				} else
					if(GlobalVar.TypeFindClient<2)
				{                    
					this.db.ExecuteNonQuery<object>(SqlFindClientCode,new { CodePrivat= varStr });
				}
			}
			else // Пошук по назві
			{
				if(GlobalVar.TypeFindClient == 0)//Можна шукати по назві
					this.db.ExecuteNonQuery(SqlFindClientName+"'%"+varStr.Replace(" ","%")+"%'");

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

        // Повертає знайдений товар/товари
        public override IEnumerable<ReceiptWares> FindWares(decimal parDiscount=0)
		{
            return this.db.Execute<object, ReceiptWares>(SqlFoundWares,new { Discount= parDiscount });
		}
		// Повертає знайдені Одиниці по товару.
/*		public override System.Data.DataTable UnitWares(int parCodeWares)
		{
			return this.db.Execute(SqlAdditionUnit+parCodeWares.ToString());
		}*/
		// Повертає знайденого клієнта(клієнтів)
		public override IEnumerable<Client> FindClient()
		{
			return this.db.Execute<Client>(SqlFoundClient);
		}
		
		public override IdReceipt GetNewCodeReceipt( IdReceipt parIdReceipt)
		{
			if (parIdReceipt.CodePeriod == 0)
                parIdReceipt.CodePeriod = Global.GetCodePeriod();

            parIdReceipt.CodeReceipt = this.db.ExecuteScalar<IdReceipt, int>(SqlGetNewCodeReceipt, parIdReceipt);

            return parIdReceipt;
			
		}
		
		public override IEnumerable<ReceiptWares> ViewReceiptWares(IdReceipt parIdReceipt)
        {
			return this.db.Execute<IdReceipt, ReceiptWares>(SqlViewReceiptWares, parIdReceipt);
		}

        public override Receipt ViewReceipt(IdReceipt parIdReceipt)
        {
            var res= this.db.Execute<IdReceipt, Receipt>(SqlViewReceipt, parIdReceipt);
            if (res.Count() == 1)
                return res.First();
           
            return null;
        }

        public override bool  AddReceipt(Receipt parReceipt)
		{
            return this.db.ExecuteNonQuery<Receipt> (SqlAddReceipt, parReceipt) ==0;
		}
		
		public override bool  UpdateClient(IdReceipt parIdReceipt, int parCodeClient)
		{
			return this.db.ExecuteNonQuery<IdReceipt> (SqlUpdateClient, parIdReceipt) ==0;
		}
		
		public override bool  CloseReceipt(Receipt parReceipt)
		{
			return this.db.ExecuteNonQuery<Receipt> (SqlCloseReceipt, parReceipt) ==0 ;
		}

		
		public override bool  AddWares(ReceiptWares parReceiptWares)
		{
			return this.db.ExecuteNonQuery<ReceiptWares> (SqlAddWares, parReceiptWares) ==0 /*&& RecalcHeadReceipt((IdReceipt)parReceiptWares)*/;
		}

		public override decimal GetCountWares(IdReceiptWares parIdReceiptWares)
		{ 
			return db.ExecuteScalar<IdReceiptWares,decimal>(SqlGetCountWares, parIdReceiptWares); 
			
		}
		
		
		public override bool UpdateQuantityWares(ReceiptWares parIdReceiptWares)
		{
			return this.db.ExecuteNonQuery (SqlUpdateQuantityWares, parIdReceiptWares) == 0 /*&& RecalcHeadReceipt(parParameters)*/;
		}
		

		public override bool DeleteReceiptWares(IdReceiptWares parIdReceiptWares)
		{
			return this.db.ExecuteNonQuery<IdReceiptWares> (SqlDeleteReceiptWares, parIdReceiptWares) ==0 /*&& RecalcHeadReceipt(parParameters)*/;
		}
		

		public override bool  InputOutputMoney(decimal parMany)
		{
			return this.db.ExecuteNonQuery  (SqlInputOutputMoney)==0;
		}
		
		public override bool  AddZ(System.Data.DataRow parRow )
		{
			return this.db.ExecuteNonQuery(SqlAddZ)==0;
		}
		
		public override bool  AddLog(System.Data.DataRow parRow )
		{
			return this.db.ExecuteNonQuery(SqlAddLog)==0;
		}
		
		public override bool LoadDataFromFile(string parFile)
		{
			
			using ( StreamReader streamReader = new StreamReader(parFile)) //Открываем файл для чтения)
			{
				string varHead = "insert into "+  Path.GetFileNameWithoutExtension(parFile)+"("+streamReader.ReadLine() + ") values ";
				db.BeginTransaction();
				while (!streamReader.EndOfStream)
				{
					db.ExecuteNonQuery( varHead +"("+ streamReader.ReadLine() +")");
				}
				db.CommitTransaction();
				return true;
			}
		}
        /*public override System.Data.DataRow GetPrice(Parameter[] parParameters)
		{
			return this.db.Execute(SqlGetPrice,parParameters).Rows[0];
		}*/


        public override bool RecalcPrice(IdReceipt parIdReceipt)
        {
            /*Parameter[] varParameters = new Parameter[] {
                new Parameter("parIdWorkplace", "parIdWorkplace") ,
                new Parameter("parCodePeriod", Global.GetCodePeriod()) ,
                new Parameter("parDefaultCodeDealer", GlobalVar.DefaultCodeDealer[0]) ,
                new Parameter("parCodeReceipt", parCodeReceipt) 
            };

            DataTable varDT=this.db.Execute(this.SqlListPS,varParameters);
            for (int i = 0; i < varDT.Rows.Count; i++)
            {
                //wr.code_wares,  wr.code_unit, w.vat, w.vat_operation, ps.code_ps, ps.priority, psd.type_discount, psd.data, pd.price_dealer, pdd.price_dealer default_price_dealer,
                int varCodeWares=Convert.ToInt32(varDT.Rows[i]["code_wares"] );
                int varCodeUnit=Convert.ToInt32(varDT.Rows[i]["code_unit"] );
                int varCodePS=Convert.ToInt32(varDT.Rows[i]["code_ps"] );

                varParameters = new Parameter[] {
                new Parameter("parIdWorkplace", "parIdWorkplace") ,
                new Parameter("parCodePeriod", Global.GetCodePeriod()) ,
                new Parameter("parCodeReceipt", parCodeReceipt),
                new Parameter("parCodeWares", varCodeWares),
                new Parameter("parCodeUnit", varCodeUnit),
                new Parameter("parCodePS", varCodePS),
                new Parameter("parVatOperation", varWares.varTypeVat),
                new Parameter("parSum", (varWares.varPrice*varWares.varQuantity)*(1+varWares.varPercentVat)* varWares.varCoefficient),
                new Parameter("parSumVat", varWares.varPrice*varWares.varQuantity*varWares.varPercentVat* varWares.varCoefficient)
            };
                this.db.ExecuteNonQuery(this.SqlUpdatePrice,varParameters);

            }*/
            RecalcHeadReceipt(parIdReceipt);
            return true;
        }
        
        /*		public override int GetLastUseCodeEkka()
                {
                    DataTable varDT=this.db.Execute(this.SqlGetLastUseCodeEkka);
                    if(varDT.Rows.Count>0 && !DBNull.Value.Equals(varDT.Rows[0][0]))
                        return ( Convert.ToInt32(varDT.Rows[0][0] ));
                    else
                        return 0;

                }

                public override bool AddWaresEkka(Parameter[] parParameters )
                {
                    return db.ExecuteNonQuery(this.SqlAddWaresEkka,parParameters)==0;
                }

                public override bool DeleteWaresEkka()
                {
                    return this.db.ExecuteNonQuery(this.SqlDeleteWaresEkka)==0;
                }
                public override int GetCodeEKKA(Parameter[] parParameters )
                {
                    DataTable varDT = this.db.Execute(this.SqlGetCodeEKKA,parParameters);
                    if(varDT.Rows.Count>0)
                        return ( Convert.ToInt32(varDT.Rows[0][0]));
                    else
                        return 0;
                }

                public override DataTable Translation(Parameter[] parParameters )
                {
                    return  db.Execute(this.SqlTranslation,parParameters); 
                }
                public override DataTable FieldInfo(Parameter[] parParameters = null)
                {
                    return  db.Execute(this.SqlFieldInfo,parParameters);
                }
                */

        public override bool RecalcHeadReceipt(IdReceipt parReceipt)
		{
			return this.db.ExecuteNonQuery<IdReceipt>(this.SqlRecalcHeadReceipt, parReceipt) ==0;;
		}
        /*
		public override System.Data.DataTable GetAllPermissions(int parCodeUser)
		{
			Parameter[] varParameters = new Parameter[] { new Parameter { ColumnName= "parCodeUser", Value= parCodeUser} };
			DataTable varDT=this.db.Execute(this.SqlGetAllPermissions,varParameters);
			return varDT;
		}
		
		public override TypeAccess GetPermissions(int parCodeUser, CodeEvent parEvent)
		{
            Parameter[] varParameters = new Parameter[] { new Parameter { ColumnName = "parCodeUser", Value = parCodeUser }, new Parameter { ColumnName = "parCodeAccess", Value = (int)parEvent }, };
            
			DataTable varDT=this.db.Execute(this.SqlGetAllPermissions,varParameters);
			if(varDT!=null&&varDT.Rows.Count>0)
				return (TypeAccess) varDT.Rows[0][0];
			else
				return TypeAccess.No;
			
		}
		*/
        public override bool CopyWaresReturnReceipt(IdReceipt parIdReceipt, bool parIsCurrentDay = true)
		{
			string SqlCopyWaresReturnReceipt=(parIsCurrentDay? this.SqlCopyWaresReturnReceipt.Replace("RRC.","RC.") : this.SqlCopyWaresReturnReceipt ) ;
			return (this.db.ExecuteNonQuery(SqlCopyWaresReturnReceipt, parIdReceipt) ==0);
		}

	}
}