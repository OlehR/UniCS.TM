/*
 * Created by SharpDevelop.
 * User: gelo
 * Date: 09.10.2013
 * Time: 9:51
 */
using System;
using System.IO;
using System.Data;
using System.Collections;
using ModelMID;
using System.Collections.Generic;
using ModelMID.DB;
using System.Linq;

//using DatabaseLib; // тимчасово для ParametersCollection
namespace SharedLib
{

    /// <summary>
    /// Клас з віртуальними методоами для доступу до БД (Work Data Base)
    /// </summary>
    public class WDB
    {

        public SQL db;
        protected Hashtable keySQL = new Hashtable();

        /*public delegate void CallWriteLogSQL(string parQvery, ParametersCollection parParameters = null);
        /// <summary>
        /// Функція для запису SQL запитів в локальну LOG базу 
        /// </summary>
		protected CallWriteLogSQL varCallWriteLogSQL; // это тот самый член-делегат :))
		*/
        public string varVersion = "0.0.1";
        protected string SqlCreateT = @"";
        protected string SqlInsertT1 = @"";
        protected string SqlClearT1 = @"";
        protected string SqlCreateReceiptTable = @"";
        protected string SqlInitGlobalVar = @"";
        protected string SqlConfig = @"";
        
        /// <summary>
        /// Процедура пошуку.(для Баз з можливістю stored procedure) oracle,mssql,...
        /// </summary>
        protected string SqlFind = @"";
        protected string SqlFindWaresBar = @"";
        protected string SqlFindWaresCode = @"";
        protected string SqlFindWaresName = @"";
        protected string SqlFindClientBar = @"";
        protected string SqlFindClientCode = @"";
        protected string SqlFindClientName = @"";
        protected string SqlFindClientPhone = @"";
        /// <summary>
        /// Запит, який вертає знайдених клієнтів
        /// </summary>
        protected string SqlFoundClient = @"";
        /// <summary>
        /// Запит, який вертає знайдені товари
        /// </summary>
        protected string SqlFoundWares = @"";
        /// <summary>
        /// Повертає доступні одиниці виміру по товару
        /// </summary>
        protected string SqlAdditionUnit = @"";
        /// <summary>
        /// Добавляє чек в базу.
        /// </summary>
        protected string SqlAddReceipt = @"";
        protected string SqlReplaceReceipt = @"";
        /// <summary>
        /// Запит який вертає інформацію про товари в чеку
        /// </summary>
        protected string SqlViewReceipt = @"";
        /// <summary>
        /// Запит який вертає інформацію про товари в чеку
        /// </summary>
        protected string SqlViewReceiptWares = @"";
        // <summary>
        /// Міняє клієнта в чеку
        /// </summary>
        protected string SqlUpdateClient = @"";
        protected string SqlCloseReceipt = @"";
        /// <summary>
        /// Добавляє товарну позицію в чек
        /// </summary>
        protected string SqlAddWares = @"";
        protected string SqlReplaceWaresReceipt = @"";
        protected string SqlGetCountWares = @"";
        protected string SqlUpdateQuantityWares = @"";
        protected string SqlDeleteReceiptWares = @"";
        protected string SqlRecalcHeadReceipt = @"";
        /// <summary>
        /// Внесення винесення грошей.
        /// </summary>
        protected string SqlInputOutputMoney = @"";
        /// <summary>
        /// Добавляє інформацію про Z-звіт
        /// </summary>
        protected string SqlAddZ = @"";
        /// <summary>
        /// Добавляє інформацію в log файл
        /// </summary>
        protected string SqlAddLog = @"";
        /// <summary>
        /// Запит для генерації кодів по робочому місці.(наприклад номер чека)
        /// </summary>
        //protected string SqlGenWorkPlace = @"";
        protected string SqlGetNewCodeReceipt = @"";
        //protected string SqlInsertGenWorkPlace = @"";
        //protected string SqlSelectGenWorkPlace = @"";
        //protected string SqlUpdateGenWorkPlace = @"";

        protected string SqlLogin = @"";
        protected string SqlGetPrice = @"";

        protected string SqlPrepareLockFilterT1 = @"";
        protected string SqlPrepareLockFilterT2 = @"";
        protected string SqlPrepareLockFilterT3 = @"";
        protected string SqlPrepareLockFilterT4 = @"";
        protected string SqlPrepareLockFilterT5 = @"";
        protected string SqlListPS = @"";
        protected string SqlUpdatePrice = @"";

/*        protected string SqlGetLastUseCodeEkka = @"";
        protected string SqlAddWaresEkka = @"";
        protected string SqlDeleteWaresEkka = @"";
        protected string SqlGetCodeEKKA = @"";*/

        protected string SqlTranslation = @"";
        protected string SqlFieldInfo = @"";
        protected string SqlGetPermissions = @"";
        protected string SqlGetAllPermissions = @"";
        protected string SqlCopyWaresReturnReceipt = @"";

        protected string SqlReplaceUnitDimension= @"";
        protected string SqlReplaceGroupWares = @"";
        protected string SqlReplaceWares= @"";
        protected string SqlReplaceAdditionUnit= @"";
        protected string SqlReplaceBarCode= @"";
        protected string SqlReplacePrice= @"";
        protected string SqlReplaceTypeDiscount= @"";
        protected string SqlReplaceClient= @"";

        protected string SqlGetWaresFromFastGroup = "";
        protected string SqlGetFastGroup = "";


        protected string SqlGetDimUnitDimension = @"";
        protected string SqlGetDimGroupWares = @"";
        protected string SqlGetDimWares = @"";
        protected string SqlGetDimAdditionUnit = @"";
        protected string SqlGetDimBarCode = @"";
        protected string SqlGetDimPrice = @"";
        protected string SqlGetDimTypeDiscount = @"";
        protected string SqlGetDimClient = @"";
        protected string SqlReplaceFastGroup = @"";
        protected string SqlReplaceFastWares = @"";

        protected string SqlReplacePromotionSale = @"";
        protected string SqlReplacePromotionSaleData = @"";
        protected string SqlReplacePromotionSaleFilter = @"";
        protected string SqlReplacePromotionSaleGiff = @"";
        protected string SqlReplacePromotionSaleDealer = @"";
        protected string SqlReplacePromotionSaleGroupWares = @"";


        public WDB(string parFileSQL)
        {
            this.ReadSQL(parFileSQL);
            InitSQL();
        }
        /*		public WDB(CallWriteLogSQL parCallWriteLogSQL=null)
                {
                    this.varCallWriteLogSQL=parCallWriteLogSQL;
                }
                */


        /*		public virtual DataTable Execute(string parQuery, ParametersCollection parPrameters = null)
                {
                    return null;
                }

                public virtual int ExecuteNonQuery(string parQuery, ParametersCollection parParameters = null)
                {
                    return 0;
                }
                public virtual DataRow GetGlobalVar(int parIdWorkPlace)
                {
                    return null;
                }*/
        public virtual bool InsertT1(T1 parT1)
        {
            return this.db.ExecuteNonQuery(SqlInsertT1, parT1) == 0;
        }

        public virtual T GetConfig<T>(string parStr)
		{
            return this.db.ExecuteScalar<object, T>(this.SqlConfig, new { NameVar = parStr });
            //;
        }
		/// <summary>
		/// Конектиться до бази та повертає табличку з правами.
		/// </summary>
		/// <param name="parLogin">Логін</param>
		/// <param name="parPassword">Пароль</param>
		/// <returns>row(код,назва,логін,пароль) Код користувача, -1 - Відсутній зв'язок з базою, -2  - неправильний логін/пароль</returns>

/*		public virtual DataRow Login (string parLogin,string parPassword)
		{
			return null;
		}*/

/*
		public virtual DataTable RightOfAccess (int parCodeUser)
		{
			return null;
		}
*/		
		/// <summary>
		/// Шукає дані по строці вводу. Результат в тимчасову табличку.
		/// </summary>
		/// <param name="parStr">Рядок</param>
		/// <param name="parTypeFind">Що шукати 0 - все,1 - товари,2-клієнти,3-купони та акціїї </param>
		/// <returns>
		///Повертає що знайшли.
		///0 - нічого не знайшли,1 - товар, 2 - клієнт,3 - Купон.
		///</returns>
		public virtual ModelMID.RezultFind FindData( string parStr,TypeFind parTypeFind = TypeFind.All)
		{
            
            RezultFind varRez;
			varRez.TypeFind = TypeFind.All;
			varRez.Count=0;
			return varRez;
		}

		/// <summary>
		/// Повертає кількість знайдених елементів.
		/// </summary>
		/// <returns></returns>
		public virtual int GetCountT1()
		{
            return this.db.ExecuteScalar<int>(@"select count(*) cn from T$1");
        }

        public virtual bool ClearT1()
        {
            return this.db.ExecuteNonQuery(SqlClearT1) == 0;
        }
        /// <summary>
        /// Повертає знайдений товар/товари
        /// </summary>
        public virtual IEnumerable<ReceiptWares> FindWares(decimal parDiscount=0)
		{
            return this.db.Execute<object, ReceiptWares>(SqlFoundWares, new { Discount = parDiscount, @CodeDealer = 2 });
        }

        public virtual RezultFind FindWaresByName(string parPhone)
        {
            return new RezultFind() { Count = 0, TypeFind = TypeFind.Client };
        }

        /*
                /// <summary>
                /// Повертає Одиниці по товару
                /// </summary>
                /// <param name="parCodeWares">Код товару</param>
                /// <returns>
                ///Повертає DataTable з одиницями виміру
                ///</returns>
                public virtual System.Data.DataTable UnitWares(int parCodeWares)
                {
                    return null;;
                }
                */

        /// <summary>
        /// Повертає знайденого клієнта(клієнтів)
        /// </summary>
        /// <param name="parCodeWares">Код товару</param>
        /// <returns>
        ///Повертає  IEnumerable<Client> з клієнтами
        ///</returns>
        public virtual IEnumerable<Client> FindClient()
		{
            return this.db.Execute<Client>(SqlFoundClient);
        }


        public virtual RezultFind FindClientByPhone(string parPhone)
        {
            return new RezultFind() {Count=0,TypeFind= TypeFind.Client };
        }


        /// <summary>
        /// Вертає код Чека в зазначеному періоді
        /// </summary>
        /// <param name="parCodePeriod">Код періоду (201401 - січень 2014)</param>
        /// <returns>
        ///Повертає код чека
        ///</returns>
        public virtual IdReceipt GetNewCodeReceipt(IdReceipt parIdReceipt)
		{
            if (parIdReceipt.CodePeriod == 0)
                parIdReceipt.CodePeriod = Global.GetCodePeriod();
            parIdReceipt.CodeReceipt = this.db.ExecuteScalar<IdReceipt, int>(SqlGetNewCodeReceipt, parIdReceipt);
            return parIdReceipt;
        }


        /// <summary>
        /// Добавляє  чека в базу
        /// </summary>
        /// <param name="parRow">Рядок з інформацією про чек</param>
        /// <returns>
        ///Успішно чи ні виконана операція
        ///</returns>

        public virtual bool  AddReceipt(Receipt parReceipt)
		{
            return this.db.ExecuteNonQuery<Receipt>(SqlAddReceipt, parReceipt) == 0;
        }

        public virtual bool ReplaceReceipt(Receipt parReceipt)
        {
            return this.db.ExecuteNonQuery<Receipt>(SqlReplaceReceipt, parReceipt) == 0;
        }

        public virtual bool  UpdateClient(IdReceipt parIdReceipt, int parCodeClient)
		{
            return this.db.ExecuteNonQuery<IdReceipt>(SqlUpdateClient, parIdReceipt) == 0;
        }
		
		public virtual bool  CloseReceipt(Receipt parReceipt)
		{
            return this.db.ExecuteNonQuery<Receipt>(SqlCloseReceipt, parReceipt) == 0;
        }


		/// <summary>
		/// Повертає фактичну кількість після вставки(добавляє до текучої кількості - -1 якщо помилка;
		/// </summary>
		/// <param name="parParameters"></param>
		/// <returns></returns>
		public virtual bool AddWares(ReceiptWares parReceiptWares)
		{
            return this.db.ExecuteNonQuery<ReceiptWares>(SqlAddWares, parReceiptWares) == 0 /*&& RecalcHeadReceipt((IdReceipt)parReceiptWares)*/;
        }


  
        public virtual bool ReplaceWaresReceipt(ReceiptWares parReceiptWares)
        {
            return this.db.ExecuteNonQuery<ReceiptWares>(SqlReplaceWaresReceipt, parReceiptWares) == 0 /*&& RecalcHeadReceipt((IdReceipt)parReceiptWares)*/;
        }

        

        public virtual decimal GetCountWares(IdReceiptWares parIdReceiptWares)
		{
            return db.ExecuteScalar<IdReceiptWares, decimal>(SqlGetCountWares, parIdReceiptWares);
        }
		
		public virtual bool UpdateQuantityWares(ReceiptWares parIdReceiptWares)
		{
            return this.db.ExecuteNonQuery(SqlUpdateQuantityWares, parIdReceiptWares) == 0 /*&& RecalcHeadReceipt(parParameters)*/;
        }
		
		
		public virtual bool DeleteReceiptWares(IdReceiptWares parIdReceiptWares)
		{
            return this.db.ExecuteNonQuery<IdReceiptWares>(SqlDeleteReceiptWares, parIdReceiptWares) == 0 /*&& RecalcHeadReceipt(parParameters)*/;
        }
        public virtual Receipt ViewReceipt(IdReceipt parIdReceipt)
        {
            var res = this.db.Execute<IdReceipt, Receipt>(SqlViewReceipt, parIdReceipt);
            if (res.Count() == 1)
              return res.FirstOrDefault();
           return null;
        }

        /// <summary>
        /// Показує інформацю про товари в чеку 
        /// </summary>
        /// <param name="parParameters"> </param>
        /// <returns></returns>
     	public virtual IEnumerable<ReceiptWares> ViewReceiptWares(IdReceipt parIdReceipt)
        {
            return this.db.Execute<IdReceipt, ReceiptWares>(SqlViewReceiptWares, parIdReceipt);
        }
        /// <summary>
        /// Перераховує ціни з врахуванням акцій, кількостей і ТД
        /// </summary>
        /// <param name="parCodeReceipt">Код чека</param>
        /// <returns>
        ///Успішно чи ні виконана операція
        ///</returns>
        public virtual bool  RecalcPrice(IdReceipt parIdReceipt)
		{
			return false;
		}

		public virtual bool  InputOutputMoney(decimal parMany)
		{
            return this.db.ExecuteNonQuery(SqlInputOutputMoney) == 0;
        }
		
		public virtual bool  AddZ(System.Data.DataRow parRow )
		{
            return this.db.ExecuteNonQuery(SqlAddZ) == 0;
        }
		
/*		public virtual bool  AddLog(System.Data.DataRow parRow )
		{
            return this.db.ExecuteNonQuery(SqlAddLog) == 0;
        }
*/

/*		
		public virtual System.Data.DataRow GetPrice(int parCodeDealer, int parCodeWares, decimal parDiscount)
		{
			return null;
		}
		*/
		protected bool ReadSQL(String iniPath)
		{
			TextReader iniFile = null;
			String strLine = null;
			String currentRoot = "";
			String value = "";

			if (File.Exists(iniPath))
			{
				try
				{
					iniFile = new StreamReader(iniPath);
					strLine = iniFile.ReadLine();
					while (strLine != null)
					{
						strLine = strLine.Trim().ToUpper();
						if (strLine != "")
						{
							if (strLine.StartsWith("[") && strLine.EndsWith("]"))
							{
								if(currentRoot.Length>0 && value.Length>0)
									keySQL.Add(currentRoot, value);
								currentRoot = strLine.Substring(1, strLine.Length - 2).ToUpper();
								value="";
							}
							else
							{
								if(value.Length>0)
									value+="\n"+strLine;
								else
									value=strLine;
							}
						}
						strLine = iniFile.ReadLine();
					}
					if(currentRoot.Length>0 && value.Length>0)
						keySQL.Add(currentRoot, value);
				}
				catch (Exception ex)
				{
					throw ex;
				}
				finally
				{
					if (iniFile != null)
						iniFile.Close();
				}
			}
			else
				throw new FileNotFoundException("Unable to locate " + iniPath);
			return true;

		}
		
		protected String GetSQL(String parNameVar)
		{
			try
			{
				return keySQL[parNameVar.ToUpper()].ToString();
			}
			catch 
			{
				return null;
			}
			
		}
        protected bool InitSQL()
        {
            SqlCreateT = GetSQL("SqlCreateT");
            SqlInsertT1 = GetSQL("SqlInsertT1");
            SqlClearT1 = GetSQL("SqlClearT1");
            SqlCreateReceiptTable = GetSQL("SqlCreateReceiptTable");
            SqlInitGlobalVar = GetSQL("SqlInitGlobalVar");
            SqlConfig = GetSQL("SqlConfig");
            SqlFind = GetSQL("SqlFind");
            SqlFindWaresBar = GetSQL("SqlFindWaresBar");
            SqlFindWaresCode = GetSQL("SqlFindWaresCode");
            SqlFindWaresName = GetSQL("SqlFindWaresName");
            SqlFindClientBar = GetSQL("SqlFindClientBar");
            SqlFindClientCode = GetSQL("SqlFindClientCode");
            SqlFindClientName = GetSQL("SqlFindClientName");
            SqlFindClientPhone = GetSQL("SqlFindClientPhone");
            SqlFoundClient = GetSQL("SqlFoundClient");
            SqlFoundWares = GetSQL("SqlFoundWares");
            SqlAdditionUnit = GetSQL("SqlAdditionUnit");
            SqlViewReceipt = GetSQL("SqlViewReceipt");
            SqlViewReceiptWares = GetSQL("SqlViewReceiptWares");
            SqlAddReceipt = GetSQL("SqlAddReceipt");
            if(SqlAddReceipt!=null)
              SqlReplaceReceipt = SqlAddReceipt.Replace("INSERT ", "replace ");
            SqlUpdateClient = GetSQL("SqlUpdateClient");
            SqlCloseReceipt = GetSQL("SqlCloseReceipt");

            SqlAddWares = GetSQL("SqlAddWares");
            if (SqlAddWares != null)
                SqlReplaceWaresReceipt = SqlAddWares.Replace("INSERT ", "replace ");

            SqlRecalcHeadReceipt = GetSQL("SqlRecalcHeadReceipt");
            SqlGetCountWares = GetSQL("SqlGetCountWares");
            SqlUpdateQuantityWares = GetSQL("SqlUpdateQuantityWares");
            SqlDeleteReceiptWares = GetSQL("SqlDeleteReceiptWares");
            SqlInputOutputMoney = GetSQL("SqlInputOutputMoney");
            SqlAddZ = GetSQL("SqlAddZ");
            SqlAddLog = GetSQL("SqlAddLog");
            //SqlGenWorkPlace=GetSQL("SqlGenWorkPlace");
            SqlGetNewCodeReceipt = GetSQL("SqlGetNewCodeReceipt");

            /*SqlInsertGenWorkPlace = GetSQL("SqlInsertGenWorkPlace");
			SqlSelectGenWorkPlace = GetSQL("SqlSelectGenWorkPlace");
			SqlUpdateGenWorkPlace = GetSQL("SqlUpdateGenWorkPlace");*/
            SqlGetPrice = GetSQL("SqlGetPrice");
            SqlLogin = GetSQL("SqlLogin");
            SqlPrepareLockFilterT1 = GetSQL("SqlPrepareLockFilterT1");
            SqlPrepareLockFilterT2 = GetSQL("SqlPrepareLockFilterT2");
            SqlPrepareLockFilterT3 = GetSQL("SqlPrepareLockFilterT3");
            SqlPrepareLockFilterT4 = GetSQL("SqlPrepareLockFilterT4");
            SqlPrepareLockFilterT5 = GetSQL("SqlPrepareLockFilterT5");
            SqlListPS = GetSQL("SqlListPS");
/*            SqlGetLastUseCodeEkka = GetSQL("SqlGetLastUseCodeEkka");
            SqlAddWaresEkka = GetSQL("SqlAddWaresEkka");
            SqlDeleteWaresEkka = GetSQL("SqlDeleteWaresEkka");
            SqlGetCodeEKKA = GetSQL("SqlGetCodeEKKA");
*/
            SqlTranslation = GetSQL("SqlTranslation");
            SqlFieldInfo = GetSQL("SqlFieldInfo");
            SqlGetAllPermissions = GetSQL("SqlGetAllPermissions");
            SqlGetPermissions = GetSQL("SqlGetPermissions");
            SqlCopyWaresReturnReceipt = GetSQL("SqlCopyWaresReturnReceipt");

            SqlReplaceUnitDimension = GetSQL("SqlReplaceUnitDimension");
            SqlReplaceGroupWares = GetSQL("SqlReplaceGroupWares");
            SqlReplaceWares = GetSQL("SqlReplaceWares");
            SqlReplaceAdditionUnit = GetSQL("SqlReplaceAdditionUnit");
            SqlReplaceBarCode = GetSQL("SqlReplaceBarCode");
            SqlReplacePrice = GetSQL("SqlReplacePrice");
            SqlReplaceTypeDiscount = GetSQL("SqlReplaceTypeDiscount");
            SqlReplaceClient = GetSQL("SqlReplaceClient");
            SqlGetWaresFromFastGroup = GetSQL("SqlGetWaresFromFastGroup");
            SqlGetFastGroup = GetSQL("SqlGetFastGroup");

            SqlGetDimUnitDimension = GetSQL("SqlGetDimUnitDimension");
           
            SqlGetDimGroupWares = GetSQL("SqlGetDimGroupWares");
            SqlGetDimWares = GetSQL("SqlGetDimWares");
            SqlGetDimAdditionUnit = GetSQL("SqlGetDimAdditionUnit");
            SqlGetDimBarCode = GetSQL("SqlGetDimBarCode");
            SqlGetDimPrice = GetSQL("SqlGetDimPrice");
            SqlGetDimTypeDiscount = GetSQL("SqlGetDimTypeDiscount");
            SqlGetDimClient = GetSQL("SqlGetDimClient");
            SqlReplaceFastGroup = GetSQL("SqlReplaceFastGroup");
            SqlReplaceFastWares = GetSQL("SqlReplaceFastWares");
            SqlReplacePromotionSale = GetSQL("SqlReplacePromotionSale");
            SqlReplacePromotionSaleData = GetSQL("SqlReplacePromotionSaleData");
            SqlReplacePromotionSaleFilter = GetSQL("SqlReplacePromotionSaleFilter");
            SqlReplacePromotionSaleGiff = GetSQL("SqlReplacePromotionSaleGiff");
            SqlReplacePromotionSaleDealer = GetSQL("SqlReplacePromotionSaleDealer");
            SqlReplacePromotionSaleGroupWares = GetSQL("SqlReplacePromotionSaleGroupWares");


            return true;
        }

        public virtual bool LoadDataFromFile(string parFile)
        {


            using (StreamReader streamReader = new StreamReader(parFile)) //Открываем файл для чтения)
            {
                string varHead = "insert into " + Path.GetFileNameWithoutExtension(parFile) + "(" + streamReader.ReadLine() + ") values ";
                db.BeginTransaction();
                while (!streamReader.EndOfStream)
                {
                    db.ExecuteNonQuery(varHead + "(" + streamReader.ReadLine() + ")");
                }
                db.CommitTransaction();
                return true;
            }
        }

        /*
		public virtual int GetLastUseCodeEkka()
		{
            DataTable varDT = this.db.Execute(this.SqlGetLastUseCodeEkka);
            if (varDT.Rows.Count > 0 && !DBNull.Value.Equals(varDT.Rows[0][0]))
                return (Convert.ToInt32(varDT.Rows[0][0]));
            else
                return 0;
        }
        		public virtual bool AddWaresEkka(ParametersCollection parParameters )
                {
                    return db.ExecuteNonQuery(this.SqlAddWaresEkka,parParameters)==0;
                }
        public virtual bool DeleteWaresEkka()
		{
            return this.db.ExecuteNonQuery(this.SqlDeleteWaresEkka) == 0;
        }

        		public virtual int GetCodeEKKA(ParametersCollection parParameters )
                {
                     DataTable varDT = this.db.Execute(this.SqlGetCodeEKKA,parParameters);
                            if(varDT.Rows.Count>0)
                                return ( Convert.ToInt32(varDT.Rows[0][0]));
                            else
                                return 0;
                }
*/
/*        public virtual DataTable Translation(ParametersCollection parParameters )
                        {
                            return  db.Execute(this.SqlTranslation,parParameters); 
                        }

                        public virtual DataTable FieldInfo(ParametersCollection parParameters = null)
                        {
                            return  db.Execute(this.SqlFieldInfo,parParameters);
                        }
*/
        public virtual bool RecalcHeadReceipt(IdReceipt parReceipt)
		{
            return this.db.ExecuteNonQuery<IdReceipt>(this.SqlRecalcHeadReceipt, parReceipt) == 0;
        }
        /*
                public virtual System.Data.DataTable GetAllPermissions(int parCodeUser)
                {
                    			Parameter[] varParameters = new Parameter[] { new Parameter { ColumnName= "parCodeUser", Value= parCodeUser} };
			DataTable varDT=this.db.Execute(this.SqlGetAllPermissions,varParameters);
			return varDT;

                }

                public virtual TypeAccess GetPermissions(int parCodeUser, CodeEvent parEvent)
                {
                    Parameter[] varParameters = new Parameter[] { new Parameter { ColumnName = "parCodeUser", Value = parCodeUser }, new Parameter { ColumnName = "parCodeAccess", Value = (int)parEvent }, };
            
			DataTable varDT=this.db.Execute(this.SqlGetAllPermissions,varParameters);
			if(varDT!=null&&varDT.Rows.Count>0)
				return (TypeAccess) varDT.Rows[0][0];
			else
				return TypeAccess.No;
                }
                */
        /// <summary>
        /// </summary>
        /// <param name="parParameters"></param>
        /// <param name="parIsCurrentDay"> Якщо повернення в день продажу. Важливо тільки для SQLite </param>
        /// <returns></returns>
        public virtual bool CopyWaresReturnReceipt(IdReceipt parIdReceipt, bool parIsCurrentDay = true )
		{
			return false;
		}

       
        public virtual bool ReplaceUnitDimension(IEnumerable<UnitDimension> parData)
        {
            db.BulkExecuteNonQuery<UnitDimension>(SqlReplaceUnitDimension, parData);
            return true;
        }

        public virtual bool ReplaceGroupWares(IEnumerable<GroupWares> parData)
        {
            db.BulkExecuteNonQuery<GroupWares>(SqlReplaceGroupWares, parData);
            return true;
        }

        public virtual bool ReplaceWares(IEnumerable<Wares> parData)
        {
            db.BulkExecuteNonQuery<Wares>(SqlReplaceWares, parData);
            return true;
        }
        public virtual bool ReplaceAdditionUnit(IEnumerable<AdditionUnit> parData)
        {
            db.BulkExecuteNonQuery<AdditionUnit>(SqlReplaceAdditionUnit, parData);
            return true;
        }
        public virtual bool ReplaceBarCode(IEnumerable<Barcode> parData)
        {
            db.BulkExecuteNonQuery<Barcode>(SqlReplaceBarCode, parData);
            return true;
        }
        public virtual bool ReplacePrice(IEnumerable<Price> parData)
        {
            db.BulkExecuteNonQuery<Price>(SqlReplacePrice, parData);
            return true;
        }
        public virtual bool ReplaceTypeDiscount(IEnumerable<TypeDiscount> parData)
        {
            db.BulkExecuteNonQuery<TypeDiscount>(SqlReplaceTypeDiscount, parData);
            return true;
        }
        public virtual bool ReplaceClient(IEnumerable<Client> parData)
        {
            db.BulkExecuteNonQuery<Client>(SqlReplaceClient, parData);
            return true;
        }

        public virtual bool ReplaceFastGroup(IEnumerable<FastGroup> parData)
        {
            db.BulkExecuteNonQuery<FastGroup>(SqlReplaceFastGroup, parData);
            return true;
        }
        public virtual bool ReplaceFastWares(IEnumerable<FastWares> parData)
        {
            db.BulkExecuteNonQuery<FastWares>(SqlReplaceFastWares, parData);
            return true;
        }

        public virtual bool ReplacePromotionSale(IEnumerable<PromotionSale> parData)
        {
            db.BulkExecuteNonQuery<PromotionSale>(SqlReplacePromotionSale, parData);
            return true;
        }

        public virtual bool ReplacePromotionSaleData(IEnumerable<PromotionSaleData> parData)
        {
            db.BulkExecuteNonQuery<PromotionSaleData>(SqlReplacePromotionSaleData, parData);
            return true;
        }

        public virtual bool ReplacePromotionSaleFilter(IEnumerable<PromotionSaleFilter> parData)
        {
            db.BulkExecuteNonQuery<PromotionSaleFilter>(SqlReplacePromotionSaleFilter, parData);
            return true;
        }

/*        public virtual bool ReplacePromotionGiff(IEnumerable<PromotionGiff> parData)
        {
            db.BulkExecuteNonQuery<PromotionGiff>(SqlReplacePromotionGiff, parData);
            return true;
        }*/


        public virtual bool ReplacePromotionSaleDealer(IEnumerable<PromotionSaleDealer> parData)
        {
            db.BulkExecuteNonQuery<PromotionSaleDealer>(SqlReplacePromotionSaleDealer, parData);
            return true;
        }

        public virtual bool ReplacePromotionSaleGroupWares(IEnumerable<PromotionSaleGroupWares> parData)
        {
            db.BulkExecuteNonQuery<PromotionSaleGroupWares>(SqlReplacePromotionSaleGroupWares, parData);
            return true;
        }

        public virtual IEnumerable<ReceiptWares> GetWaresFromFastGroup(int parCodeFastGroup)   { return null; }
        public virtual IEnumerable<FastGroup> GetFastGroup(int parCodeUpFastGroup)
        {
            var FG = new FastGroup { CodeUp = parCodeUpFastGroup };
            return db.Execute<FastGroup, FastGroup>(SqlGetFastGroup, FG);
        }

        public virtual PricePromotion GetPrice(ParameterPromotion parPromotion)
        {
            /*var  res=db.Execute<ParameterPromotion, PricePromotion>(SqlGetPrice, parPromotion);
            if (res != null)
                res.FirstOrDefault();*/
            return null;

        }
    }

}
