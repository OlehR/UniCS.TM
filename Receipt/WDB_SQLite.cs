using Model;
using SharedLib;
using System;
using System.Data;
//using DB_SQLite;
using System.IO;

namespace MID
{
	public partial class WDB_SQLite:WDB
	{
		public SQLite db;
		public SQLite db_receipt;
		public Wares varWares = new Wares();
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parCallWriteLogSQL"></param>
		 public WDB_SQLite(CallWriteLogSQL parCallWriteLogSQL=null):base(parCallWriteLogSQL)
		{
        	this.varVersion="SQLite.0.0.1";
        				
			this.ReadSQL(GlobalVar.varPathIni+ @"SQLite.sql");
			this.InitSQL();
			DateTime	varD=DateTime.Today;
			string varReceiptFile=GlobalVar.varPathDB +varD.ToString("yyyyMM")+@"\Rc_"+  GlobalVar.varIdWorkPlace.ToString()+"_"+varD.ToString("yyyyMMdd")+".db";
			if(!File.Exists( varReceiptFile))
			{
			//Створюємо щоденну табличку з чеками.
			this.db= new SQLite(varReceiptFile);
			this.db.ExecuteNonQuery(varSqlCreateReceiptTable);
			this.db.Close();
			}

            this.db = new SQLite(GlobalVar.varPathDB + @"MID.db");//,"",this.varCallWriteLogSQL);
			//this.db.ExecuteNonQuery("ATTACH ':memory:' AS m");
			this.db.ExecuteNonQuery(this.varSqlCreateT);
			this.db.ExecuteNonQuery("ATTACH '"+varReceiptFile+"' AS rc");
		}
		
		public override DataTable Execute(string parQuery, Parameter[] parParameters = null)
		{
			return this.db.Execute(parQuery,parParameters);
		}
		
		public override int ExecuteNonQuery(string parQuery, Parameter[] parParameters = null)
		{
			return this.db.ExecuteNonQuery(parQuery,parParameters);
		}
		
		
		public override bool InsertT1(Parameter[] parParameters)
		{
			return this.db.ExecuteNonQuery (varSqlInsertT1,parParameters)==0;
		}
		
		public override DataRow Login (string parLogin,string parPassword)
		{
			var parColl = new Parameter[] { new Parameter {ColumnName= "parLogin",Value= parLogin}, new Parameter { ColumnName = "parPassWord", Value = parPassword } };
			
			DataTable dt = db.Execute( varSqlLogin,parColl);
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
		}
		
		public override int GetCountT1()
		{
			DataTable dt = this.db.Execute(@"select count(*) cn from T$1");
			if (dt == null) return 0; else return Convert.ToInt32( dt.Rows[0][0]);
		}
		
		public override DataRow GetGlobalVar(int parIdWorkPlace)
		{
            var varParameters = new Parameter[] { new Parameter { ColumnName = "parIdWorkPlace", Value = parIdWorkPlace } };
            
			DataTable varDT=this.db.Execute(this.varSqlInitGlobalVar, varParameters);
			if(varDT!=null&&varDT.Rows.Count>0)
				return varDT.Rows[0];
			else
				return null;
		}
		
		public override object GetConfig(string parStr)
		{
            var varParameters = new Parameter[] { new Parameter { ColumnName = "parNameVar", Value = parStr } };
			DataTable varDT=this.db.Execute(this.varSqlConfig,varParameters);
			if(varDT!=null&&varDT.Rows.Count>0)
				return varDT.Rows[0][0];
			else
				return null;
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
				if(varStr.Length>= GlobalVar.varMinLenghtBarCodeWares)
				{//Шукаємо по штрихкоду
					this.db.ExecuteNonQuery(this.varSqlFindWaresBar+varStr);
				}else//Шукаемо по коду
				{ if(GlobalVar.varTypeFindWares<2)
						this.db.ExecuteNonQuery(this.varSqlFindWaresCode+varStr);
				}
			} else // Шукаємо по назві
			{
				if(GlobalVar.varTypeFindWares == 0)//Можна шукати по назві
					this.db.ExecuteNonQuery(varSqlFindWaresName+"'%"+varStr.ToUpper().Replace(" ","%")+"%'");
			}
			varRezult.Count=this.GetCountT1();
			
			if( varRezult.Count>0) return varRezult;//Знайшли товар
			// ШукаемоКлієнта
			varRezult.TypeFind=TypeFind.Client;
			if(varNumber>0)
			{
				if(varStr.Length>= GlobalVar.varMinLenghtBarCodeClient)
				{//Шукаємо по штрихкоду
                    var varParameters = new Parameter[] { new Parameter { ColumnName = "parCodeBar", Value = varStr } };
					this.db.ExecuteNonQuery(varSqlFindClientBar, varParameters);
					
				} else
					if(GlobalVar.varTypeFindClient<2)
				{
                    var varParameters = new Parameter[] { new Parameter { ColumnName = "parCodePrivat", Value = varStr } };                    
					this.db.ExecuteNonQuery(varSqlFindClientCode);
				}
			}
			else // Пошук по назві
			{
				if(GlobalVar.varTypeFindClient == 0)//Можна шукати по назві
					this.db.ExecuteNonQuery(varSqlFindClientName+"'%"+varStr.Replace(" ","%")+"%'");

			}
			varRezult.Count=this.GetCountT1();

			if( varRezult.Count==0) 
				varRezult.TypeFind=TypeFind.All;
				
			return varRezult;
			
		}
		// Повертає знайдений товар/товари
		public override System.Data.DataTable FindWares(Parameter[] parParameters = null)
		{
			return this.db.Execute(varSqlFoundWares,parParameters);
		}
		// Повертає знайдені Одиниці по товару.
		public override System.Data.DataTable UnitWares(int parCodeWares)
		{
			return this.db.Execute(varSqlAdditionUnit+parCodeWares.ToString());
		}
		// Повертає знайденого клієнта(клієнтів)
		public override System.Data.DataTable FindClient()
		{
			return this.db.Execute(varSqlFoundClient);
		}
		
		public override int GetCodeReceipt( int parCodePeriod = 0)
		{
			if (parCodePeriod==0)
				parCodePeriod = Global.GetCodePeriod();
            var varParameters = new Parameter[] { new Parameter { ColumnName = "parIdWorkplace", Value = GlobalVar.varIdWorkPlace }, new Parameter { ColumnName = "parCodePeriod", Value = parCodePeriod } };
            

			this.db.ExecuteNonQuery(varSqlUpdateGenWorkPlace, varParameters);
			DataTable varS = this.db.Execute(varSqlSelectGenWorkPlace, varParameters);
			if(varS==null || varS.Rows.Count==0)
			{
				this.db.ExecuteNonQuery( varSqlInsertGenWorkPlace, varParameters);
				
				return 1;
			}else
			{
				return Convert.ToInt32(varS.Rows[0][0]);
			}
			
		}
		
		public override System.Data.DataTable ViewWaresReceipt(Parameter[] parParameters )
		{
			return this.db.Execute(varSqlViewReceipt,parParameters);
		}

		public override bool  AddReceipt(Parameter[] parParameters)
		{
			return this.db.ExecuteNonQuery (varSqlAddReceipt,parParameters)==0;
		}
		
		public override bool  UpdateClient(Parameter[] parParameters)
		{
			return this.db.ExecuteNonQuery (varSqlUpdateClient,parParameters)==0;
		}
		
		public override bool  CloseReceipt(Parameter[] parParameters)
		{
			return this.db.ExecuteNonQuery (varSqlCloseReceipt,parParameters)==0 ;
		}

		
		public override bool  AddWares(Parameter[] parParameters )
		{
			return this.db.ExecuteNonQuery (varSqlAddWares,parParameters)==0 && RecalcHeadReceipt(parParameters);
		}

		public override decimal GetCountWares(Parameter[] parParameters )
		{ 
			DataTable varDt = db.Execute(varSqlGetCountWares,parParameters); 
			return (varDt!=null && varDt.Rows.Count>0 && !DBNull.Value.Equals(varDt.Rows[0][0]) ?  Convert.ToDecimal(varDt.Rows[0][0]) :0);
		}
		
		
		public override bool UpdateQuantityWares(Parameter[] parParameters )
		{
			return this.db.ExecuteNonQuery (varSqlUpdateQuantityWares,parParameters)==0 && RecalcHeadReceipt(parParameters);
		}
		

		public override bool DeleteWaresReceipt(Parameter[] parParameters )
		{
			return this.db.ExecuteNonQuery (varSqlDeleteWaresReceipt,parParameters)==0 && RecalcHeadReceipt(parParameters);
		}
		

		public override bool  InputOutputMoney(decimal parMany)
		{
			return this.db.ExecuteNonQuery  (varSqlInputOutputMoney)==0;
		}
		
		public override bool  AddZ(System.Data.DataRow parRow )
		{
			return this.db.ExecuteNonQuery(varSqlAddZ)==0;
		}
		
		public override bool  AddLog(System.Data.DataRow parRow )
		{
			return this.db.ExecuteNonQuery(varSqlAddLog)==0;
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
		public override System.Data.DataRow GetPrice(Parameter[] parParameters)
		{
			return this.db.Execute(varSqlGetPrice,parParameters).Rows[0];
		}
		
		public override bool  RecalcPrice(int parCodeReceipt =0)
		{
			Parameter[] varParameters = new Parameter[] {
                new Parameter("parIdWorkplace", "parIdWorkplace") ,
                new Parameter("parCodePeriod", Global.GetCodePeriod()) ,
                new Parameter("parDefaultCodeDealer", GlobalVar.varDefaultCodeDealer[0]) ,
                new Parameter("parCodeReceipt", parCodeReceipt) 
            };
			
			DataTable varDT=this.db.Execute(this.varSqlListPS,varParameters);
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
				this.db.ExecuteNonQuery(this.varSqlUpdatePrice,varParameters);
				RecalcHeadReceipt(varParameters);
			}
			return true;
		}

		public override int GetLastUseCodeEkka()
		{
			DataTable varDT=this.db.Execute(this.varSqlGetLastUseCodeEkka);
			if(varDT.Rows.Count>0 && !DBNull.Value.Equals(varDT.Rows[0][0]))
            	return ( Convert.ToInt32(varDT.Rows[0][0] ));
            else
                return 0;
		
		}
		
		public override bool AddWaresEkka(Parameter[] parParameters )
		{
			return db.ExecuteNonQuery(this.varSqlAddWaresEkka,parParameters)==0;
		}
		
		public override bool DeleteWaresEkka()
		{
			return this.db.ExecuteNonQuery(this.varSqlDeleteWaresEkka)==0;
		}
		public override int GetCodeEKKA(Parameter[] parParameters )
		{
			DataTable varDT = this.db.Execute(this.varSqlGetCodeEKKA,parParameters);
            if(varDT.Rows.Count>0)
                return ( Convert.ToInt32(varDT.Rows[0][0]));
            else
                return 0;
		}
		
		public override DataTable Translation(Parameter[] parParameters )
		{
			return  db.Execute(this.varSqlTranslation,parParameters); 
		}
		public override DataTable FieldInfo(Parameter[] parParameters = null)
		{
			return  db.Execute(this.varSqlFieldInfo,parParameters);
		}

		public override bool RecalcHeadReceipt(Parameter[] parParameters = null)
		{
			return this.db.ExecuteNonQuery(this.varSqlDeleteWaresEkka)==0;;
		}

		public override System.Data.DataTable GetAllPermissions(int parCodeUser)
		{
			Parameter[] varParameters = new Parameter[] { new Parameter { ColumnName= "parCodeUser", Value= parCodeUser} };
			DataTable varDT=this.db.Execute(this.varSqlGetAllPermissions,varParameters);
			return varDT;
		}
		
		public override TypeAccess GetPermissions(int parCodeUser, CodeEvent parEvent)
		{
            Parameter[] varParameters = new Parameter[] { new Parameter { ColumnName = "parCodeUser", Value = parCodeUser }, new Parameter { ColumnName = "parCodeAccess", Value = (int)parEvent }, };
            
			DataTable varDT=this.db.Execute(this.varSqlGetAllPermissions,varParameters);
			if(varDT!=null&&varDT.Rows.Count>0)
				return (TypeAccess) varDT.Rows[0][0];
			else
				return TypeAccess.No;
			
		}
		
		public override bool CopyWaresReturnReceipt(Parameter[] parParameters = null, bool parIsCurrentDay = true )
		{
			string varSqlCopyWaresReturnReceipt=(parIsCurrentDay? this.varSqlCopyWaresReturnReceipt.Replace("RRC.","RC.") : this.varSqlCopyWaresReturnReceipt ) ;
			return (this.db.ExecuteNonQuery(varSqlCopyWaresReturnReceipt,parParameters)==0);
		}

	}
}