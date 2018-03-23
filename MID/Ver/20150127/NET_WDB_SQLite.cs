using System;
using System.Data;
using DatabaseLib;
using System.IO;

namespace MID
{
	public partial class NET_WDB_SQLite:WDB
	{
		public DbFacadeSQLite db;
		public Wares varWares = new Wares();
		
		public NET_WDB_SQLite(string parDB="MID.DB")
		{
			this.varVersion="SQLite.0.0.1";
			this.db = new DatabaseLib.DbFacadeSQLite(parDB);
			this.db.Open();
			this.ReadSQL(GlobalVar.varPathIni+ @"SQLite.sql");
			this.InitSQL();
			this.db.ExecuteNonQuery(this.varSqlCreateT);
		}
		
		public override bool InsertT1(ParametersCollection parParameters)
		{
			return this.db.ExecuteNonQuery (varSqlInsertT1,parParameters)==0;
		}
		
		public override DataRow Login (string parLogin,string parPassword)
		{
			ParametersCollection parColl = new ParametersCollection();
			parColl.Add("parLogin",parLogin,DbType.String);
			parColl.Add("parPassWord",parPassword,DbType.String);
			
			DataTable dt = db.Execute( varSqlLogin,parColl);
			if(dt == null && dt.Rows.Count==0)
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
			ParametersCollection varParameters = new ParametersCollection();
			varParameters.Add("parIdWorkPlace",parIdWorkPlace ,DbType.Int32 );
			DataTable varDT=this.db.Execute(this.varSqlInitGlobalVar, varParameters);
			if(varDT!=null&&varDT.Rows.Count>0)
				return varDT.Rows[0];
			else
				return null;
		}
		
		public override object GetConfig(string parStr)
		{
			ParametersCollection varParameters = new ParametersCollection();
			varParameters.Add("parNameVar",parStr ,DbType.String);
			DataTable varDT=this.db.Execute(this.varSqlConfig,varParameters);
			if(varDT!=null&&varDT.Rows.Count>0)
				return varDT.Rows[0][0];
			else
				return null;
		}
		
		public override int FindData( string parStr,int parTypeFind = 0)
		{   string varStr =parStr.Trim();
			Int64 varNumber = 0;
			Int64.TryParse(varStr, out varNumber);
			this.db.ExecuteNonQuery("delete from T$1");
			ParametersCollection parameters = new ParametersCollection();
			// Шукаемо Товар
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
					this.db.ExecuteNonQuery(varSqlFindWaresName+"'%"+varStr.Replace(" ","%")+"%'");
			}
			
			if( this.GetCountT1()>0) return 1;//Знайшли товар
			// ШукаемоКлієнта
			if(varNumber>0)
			{
				if(varStr.Length>= GlobalVar.varMinLenghtBarCodeClient)
				{//Шукаємо по штрихкоду
					
					parameters.Add("parCodeBar",varStr,DbType.String);
					this.db.ExecuteNonQuery(varSqlFindClientBar,parameters);
					
				} else
					if(GlobalVar.varTypeFindClient<2)
				{
					parameters.Add("parCodePrivat",varStr,DbType.String);
					this.db.ExecuteNonQuery(varSqlFindClientCode);
				}
			}
			else // Пошук по назві
			{
				if(GlobalVar.varTypeFindClient == 0)//Можна шукати по назві
					this.db.ExecuteNonQuery(varSqlFindClientName+"'%"+varStr.Replace(" ","%")+"%'");

			}
			if( this.GetCountT1()>0) return 2;//Знайшли клієнта
			return 0;
		}
		// Повертає знайдений товар/товари
		public override System.Data.DataTable FindWares(ParametersCollection parParameters = null)
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
			ParametersCollection parameters = new ParametersCollection();
			parameters.Add("parIdWorkplace",GlobalVar.varIdWorkPlace,DbType.Int32);
			parameters.Add("parCodePeriod",parCodePeriod,DbType.Int32);



			this.db.ExecuteNonQuery(varSqlUpdateGenWorkPlace,parameters);
			DataTable varS = this.db.Execute(varSqlSelectGenWorkPlace,parameters);
			if(varS==null || varS.Rows.Count==0)
			{
				this.db.ExecuteNonQuery( varSqlInsertGenWorkPlace,parameters );
				
				return 1;
			}else
			{
				return Convert.ToInt32(varS.Rows[0][0]);
			}
			
		}
		
		public override System.Data.DataTable ViewWaresReceipt(ParametersCollection parParameters )
		{
			return this.db.Execute(varSqlViewReceipt,parParameters);
		}

		public override bool  AddReceipt(ParametersCollection parParameters)
		{
			return this.db.ExecuteNonQuery (varSqlAddReceipt,parParameters)==0;
		}
		
		public override bool  UpdateClient(ParametersCollection parParameters)
		{
			return this.db.ExecuteNonQuery (varSqlUpdateClient,parParameters)==0;
		}
		
		public override bool  CloseReceipt(ParametersCollection parParameters)
		{
			return this.db.ExecuteNonQuery (varSqlCloseReceipt,parParameters)==0;
		}

		
		public override bool  AddWares(ParametersCollection parParameters )
		{
			return this.db.ExecuteNonQuery (varSqlAddWares,parParameters)==0;
		}

		public override DataTable GetCountWares(ParametersCollection parParameters )
		{
			return db.Execute(varSqlGetCountWares,parParameters);
		}
		
		
		public override bool UpdateQuantityWares(ParametersCollection parParameters )
		{
			return this.db.ExecuteNonQuery (varSqlUpdateQuantityWares,parParameters)==0;
		}
		

		public override bool DeleteWaresReceipt(ParametersCollection parParameters )
		{
			return this.db.ExecuteNonQuery (varSqlDeleteWaresReceipt,parParameters)==0;
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
		public override System.Data.DataRow GetPrice(ParametersCollection parParameters)
		{
			return this.db.Execute(varSqlGetPrice,parParameters).Rows[0];
		}
		
		public override bool  RecalcPrice(int parCodeReceipt =0)
		{
			ParametersCollection varParameters = new ParametersCollection();;
			varParameters.Add("parIdWorkplace",GlobalVar.varIdWorkPlace,DbType.Int32);
			varParameters.Add("parCodePeriod",Global.GetCodePeriod(),DbType.Int32);
			varParameters.Add("parDefaultCodeDealer",GlobalVar.varDefaultCodeDealer[0],DbType.Int32);
			varParameters.Add("parCodeReceipt",parCodeReceipt,DbType.Int32);
			
			DataTable varDT=this.db.Execute(this.varSqlListPS,varParameters);
			for (int i = 0; i < varDT.Rows.Count; i++)
			{
				//wr.code_wares,  wr.code_unit, w.vat, w.vat_operation, ps.code_ps, ps.priority, psd.type_discount, psd.data, pd.price_dealer, pdd.price_dealer default_price_dealer,
				int varCodeWares=Convert.ToInt32(varDT.Rows[i]["code_wares"] );
				int varCodeUnit=Convert.ToInt32(varDT.Rows[i]["code_unit"] );
				int varCodePS=Convert.ToInt32(varDT.Rows[i]["code_ps"] );
				
				varParameters.Clear();
				varParameters.Add("parIdWorkplace",GlobalVar.varIdWorkPlace,DbType.Int32);
				varParameters.Add("parCodePeriod",Global.GetCodePeriod(),DbType.Int32);
				varParameters.Add("parCodeReceipt",parCodeReceipt,DbType.Int32);
				varParameters.Add("parCodeWares", varCodeWares,DbType.Int32);
				varParameters.Add("parCodeUnit", varCodeUnit,DbType.Int32);
				varParameters.Add("parCodePS",varCodePS,DbType.Int32);
				//varParameters.Add("parTypeDiscount",varTypeDiscount,DbType.Int32);
				
				varWares.SetWares(varDT.Rows[i]);
				varParameters.Add("parVatOperation",varWares.varTypeVat,DbType.Int32);
				varParameters.Add("parSum",(varWares.varPrice*varWares.varQuantity)*(1+varWares.varPercentVat)* varWares.varCoefficient ,DbType.Decimal);
			    varParameters.Add("parSumVat",varWares.varPrice*varWares.varQuantity*varWares.varPercentVat* varWares.varCoefficient,DbType.Decimal);

				this.db.ExecuteNonQuery(this.varSqlUpdatePrice,varParameters);
			}
			
		
			return true;
		}

		public override int GetLastUseCodeEkka()
		{
			DataTable varDT=this.db.Execute(this.varSqlGetLastUseCodeEkka);
            if(varDT.Rows.Count>0)
            	return ( Convert.ToInt32(varDT.Rows[0][0]));
            else
                return 0;
		
		}
		
		public override bool AddWaresEkka(ParametersCollection parParameters )
		{
			return db.ExecuteNonQuery(this.varSqlAddWaresEkka,parParameters)==0;
		}
		
		public override bool DeleteWaresEkka()
		{
			return this.db.ExecuteNonQuery(this.varSqlDeleteWaresEkka)==0;
		}
		public override int GetCodeEKKA(ParametersCollection parParameters )
		{
			DataTable varDT = this.db.Execute(this.varSqlGetCodeEKKA,parParameters);
            if(varDT.Rows.Count>0)
                return ( Convert.ToInt32(varDT.Rows[0][0]));
            else
                return 0;
		}

	}
}