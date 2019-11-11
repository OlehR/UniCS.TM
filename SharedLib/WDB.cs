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
        protected string SqlCreateReceiptTable = "";
        
        protected string SqlConfig = "";
        protected string SqlReplaceConfig = "";

        /// <summary>
        /// Процедура пошуку.(для Баз з можливістю stored procedure) oracle,mssql,...
        /// </summary>
        protected string SqlFind = "";
       
        /// <summary>
        /// 
        /// </summary>
        protected string SqlGetPersentDiscountClientByReceipt = "";
        protected string SqlGetTypeDiscountClientByReceipt = "";
        /// <summary>
        /// Запит, який вертає знайдених клієнтів
        /// </summary>
        protected string SqlFoundClient = "";
        /// <summary>
        /// Запит, який вертає знайдені товари
        /// </summary>
        protected string SqlFoundWares = "";
        /// <summary>
        /// Повертає доступні одиниці виміру по товару
        /// </summary>
        protected string SqlAdditionUnit = "";
        /// <summary>
        /// Добавляє чек в базу.
        /// </summary>
        protected string SqlAddReceipt = "";
        protected string SqlReplaceReceipt = "";
        /// <summary>
        /// Запит який вертає інформацію про товари в чеку
        /// </summary>
        protected string SqlViewReceipt = "";
        /// <summary>
        /// Запит який вертає інформацію про товари в чеку
        /// </summary>
        protected string SqlViewReceiptWares = "";
        // <summary>
        /// Міняє клієнта в чеку
        /// </summary>
        protected string SqlUpdateClient = "";
        protected string SqlCloseReceipt = "";
        /// <summary>
        /// Добавляє товарну позицію в чек
        /// </summary>
        protected string SqlAddWares = "";
        protected string SqlReplaceWaresReceipt = "";
        protected string SqlGetCountWares = "";
        protected string SqlUpdateQuantityWares = "";
        protected string SqlDeleteReceiptWares = "";
        protected string SqlRecalcHeadReceipt = "";
        /// <summary>
        /// Внесення винесення грошей.
        /// </summary>
        protected string SqlInputOutputMoney = "";
        /// <summary>
        /// Добавляє інформацію про Z-звіт
        /// </summary>
        protected string SqlAddZ = "";
        /// <summary>
        /// Добавляє інформацію в log файл
        /// </summary>
        protected string SqlAddLog = "";
        /// <summary>
        /// Запит для генерації кодів по робочому місці.(наприклад номер чека)
        /// </summary>
        //protected string SqlGenWorkPlace = "";
        protected string SqlGetNewCodeReceipt = "";
        
        protected string SqlLogin = "";
        protected string SqlGetPrice = "";
        /*
        protected string SqlPrepareLockFilterT1 = "";
        protected string SqlPrepareLockFilterT2 = "";
        protected string SqlPrepareLockFilterT3 = "";
        protected string SqlPrepareLockFilterT4 = "";
        protected string SqlPrepareLockFilterT5 = "";*/
        //protected string SqlListPS = "";
        protected string SqlUpdatePrice = "";
        protected string SqlGetMinPriceIndicative = "";

        /*        protected string SqlGetLastUseCodeEkka = "";
                protected string SqlAddWaresEkka = "";
                protected string SqlDeleteWaresEkka = "";
                protected string SqlGetCodeEKKA = "";*/

        protected string SqlTranslation = "";
        protected string SqlFieldInfo = "";
        protected string SqlGetPermissions = "";
        protected string SqlGetAllPermissions = "";
        protected string SqlCopyWaresReturnReceipt = "";

        protected string SqlReplaceUnitDimension = "";
        protected string SqlReplaceGroupWares = "";
        protected string SqlReplaceWares = "";
        protected string SqlReplaceAdditionUnit = "";
        protected string SqlReplaceBarCode = "";
        protected string SqlReplacePrice = "";
        protected string SqlReplaceTypeDiscount = "";
        protected string SqlReplaceClient = "";

        
        protected string SqlGetFastGroup = "";



        protected string SqlReplaceFastGroup = "";
        protected string SqlReplaceFastWares = "";
        protected string SqlReplacePromotionSale = "";
        protected string SqlReplacePromotionSaleData = "";
        protected string SqlReplacePromotionSaleFilter = "";        
        protected string SqlReplacePromotionSaleDealer = "";
        protected string SqlReplacePromotionSaleGroupWares = "";
        protected string SqlReplacePromotionSale2Category = "";
        protected string SqlReplacePromotionSaleGift = "";

        protected string SqlReplaceWaresReceiptPromotion = "";
        protected string SqlDeleteWaresReceiptPromotion = "";

        protected string SqlGetPricePromotionSale2Category = "";

        protected string SqlGetPriceDealer = "";

        protected string SqlCreateConfigTable = "";
        protected string SqlReplaceWorkPlace = "";
        protected string SqlGetWorkplace = "";

        protected string SqlMoveReceipt = "";
        protected string SqlReplacePayment = "";
        protected string SqlSetStateReceipt = "";
        protected string SqlInsertWeight = "";
        protected string SqlGetPayment = "";
        protected string SqlGetIdReceiptbyState = "";

        public WDB(string parFileSQL)
        {
            this.ReadSQL(parFileSQL);
            InitSQL();
   
        }
        protected bool BildWorkplace()
        {
            Global.WorkPlaceByTerminalId = new SortedList<Guid, WorkPlace>();
            Global.WorkPlaceByWorkplaceId = new SortedList<int, WorkPlace>();
            foreach (var el in GetWorkPlace())
            {
                Global.WorkPlaceByTerminalId.Add(el.TerminalGUID, el);
                Global.WorkPlaceByWorkplaceId.Add(el.IdWorkplace, el);
            }
            return true;

        }
        /*		public WDB(CallWriteLogSQL parCallWriteLogSQL=null)
                {
                    this.varCallWriteLogSQL=parCallWriteLogSQL;
                }
                */

        public virtual bool SetConfig<T>(string parName,T parValue)
        {
            parValue.GetType().ToString();
            this.db.ExecuteNonQuery<object>(this.SqlReplaceConfig, new { NameVar = parName, DataVar= parValue,@TypeVar= parValue.GetType().ToString() });
            return true;
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
        /// Повертає знайдений товар/товари
        /// </summary>
        public virtual IEnumerable<ReceiptWares> FindWares(string parBarCode = null, string parName = null, int parCodeWares = 0, int parCodeUnit = 0,int parCodeFastGroup=0,int parArticl=-1)
        {
            var Wares = this.db.Execute<object, ReceiptWares>(SqlFoundWares, new { CodeWares = parCodeWares, CodeUnit = parCodeUnit, BarCode = parBarCode, Name = parName == null ? null : "%" + parName + "%", CodeDealer = ModelMID.Global.DefaultCodeDealer , CodeFastGroup = parCodeFastGroup, Articl= parArticl });
            return Wares;
        }


        /// <summary>
        /// Повертає знайденого клієнта(клієнтів)
        /// </summary>
        /// <param name="parCodeWares">Код товару</param>
        /// <returns>
        ///Повертає  IEnumerable<Client> з клієнтами
        ///</returns>
        public virtual IEnumerable<Client> FindClient(string parBarCode = null,string parPhone=null ,string parName = null, int parCodeClient=0)
        {
            return this.db.Execute<object, Client>(SqlFoundClient, new { CodeClient=parCodeClient, Phone= parPhone, BarCode = parBarCode, Name = (parName == null ? null : "%" + parName + "%")});
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
        public virtual Receipt ViewReceipt(IdReceipt parIdReceipt, bool parWithDetail=false)
        {
            var res = this.db.Execute<IdReceipt, Receipt>(SqlViewReceipt, parIdReceipt);
            if (res.Count() == 1)
            {
                var r= res.FirstOrDefault();
                if (parWithDetail)
                {
                    r.Wares = ViewReceiptWares(parIdReceipt);
                    r.Payment = GetPayment(parIdReceipt);

                }
                return r;
            }
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
            SqlCreateReceiptTable = GetSQL("SqlCreateReceiptTable");
            
            SqlConfig = GetSQL("SqlConfig");
            SqlReplaceConfig= GetSQL("SqlReplaceConfig");


            SqlGetPersentDiscountClientByReceipt = GetSQL("SqlGetPersentDiscountClientByReceipt");
            SqlGetTypeDiscountClientByReceipt = GetSQL("SqlGetTypeDiscountClientByReceipt");

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
            //SqlInputOutputMoney = GetSQL("SqlInputOutputMoney");
            //SqlAddZ = GetSQL("SqlAddZ");
            //SqlAddLog = GetSQL("SqlAddLog");
            //SqlGenWorkPlace=GetSQL("SqlGenWorkPlace");
            SqlGetNewCodeReceipt = GetSQL("SqlGetNewCodeReceipt");

            /*SqlInsertGenWorkPlace = GetSQL("SqlInsertGenWorkPlace");
			SqlSelectGenWorkPlace = GetSQL("SqlSelectGenWorkPlace");
			SqlUpdateGenWorkPlace = GetSQL("SqlUpdateGenWorkPlace");*/
            SqlGetPrice = GetSQL("SqlGetPrice");
            SqlLogin = GetSQL("SqlLogin");
           /* SqlPrepareLockFilterT1 = GetSQL("SqlPrepareLockFilterT1");
            SqlPrepareLockFilterT2 = GetSQL("SqlPrepareLockFilterT2");
            SqlPrepareLockFilterT3 = GetSQL("SqlPrepareLockFilterT3");
            SqlPrepareLockFilterT4 = GetSQL("SqlPrepareLockFilterT4");
            SqlPrepareLockFilterT5 = GetSQL("SqlPrepareLockFilterT5");
            SqlListPS = GetSQL("SqlListPS");
                        SqlGetLastUseCodeEkka = GetSQL("SqlGetLastUseCodeEkka");
                        SqlAddWaresEkka = GetSQL("SqlAddWaresEkka");
                        SqlDeleteWaresEkka = GetSQL("SqlDeleteWaresEkka");
                        SqlGetCodeEKKA = GetSQL("SqlGetCodeEKKA");
            */
            SqlGetMinPriceIndicative= GetSQL("SqlGetMinPriceIndicative");
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
            
            SqlGetFastGroup = GetSQL("SqlGetFastGroup");

            SqlReplaceFastGroup = GetSQL("SqlReplaceFastGroup");
            SqlReplaceFastWares = GetSQL("SqlReplaceFastWares");
            SqlReplacePromotionSale = GetSQL("SqlReplacePromotionSale");
            SqlReplacePromotionSaleData = GetSQL("SqlReplacePromotionSaleData");
            SqlReplacePromotionSaleFilter = GetSQL("SqlReplacePromotionSaleFilter");
            SqlReplacePromotionSaleDealer = GetSQL("SqlReplacePromotionSaleDealer");
            SqlReplacePromotionSaleGroupWares = GetSQL("SqlReplacePromotionSaleGroupWares");
            SqlReplacePromotionSale2Category = GetSQL("SqlReplacePromotionSale2Category");
            SqlReplacePromotionSaleGift = GetSQL("SqlReplacePromotionSaleGift");

            SqlGetPricePromotionSale2Category = GetSQL("SqlGetPricePromotionSale2Category");


            SqlReplaceWaresReceiptPromotion = GetSQL("SqlReplaceWaresReceiptPromotion");
            SqlDeleteWaresReceiptPromotion = GetSQL("SqlDeleteWaresReceiptPromotion");
            SqlGetPriceDealer = GetSQL("SqlGetPriceDealer");

            SqlCreateConfigTable = GetSQL("SqlCreateConfigTable");
            SqlReplaceWorkPlace = GetSQL("SqlReplaceWorkplace");
            SqlGetWorkplace = GetSQL("SqlGetWorkplace");
            SqlReplacePayment= GetSQL("SqlReplacePayment");
            SqlSetStateReceipt = GetSQL("SqlSetStateReceipt");
            SqlInsertWeight = GetSQL("SqlInsertWeight");
            SqlGetPayment = GetSQL("SqlGetPayment");
            SqlGetIdReceiptbyState = GetSQL("SqlGetIdReceiptbyState");

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

        public virtual bool ReplacePromotionSaleGift(IEnumerable<PromotionSaleGift> parData)
        {
            db.BulkExecuteNonQuery<PromotionSaleGift>(SqlReplacePromotionSaleGift, parData);
            return true;
        }


        public virtual bool ReplacePromotionSale2Category(IEnumerable<PromotionSale2Category> parData)
        {
            db.BulkExecuteNonQuery<PromotionSale2Category>(SqlReplacePromotionSale2Category, parData);
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
            var PriceDealer = db.ExecuteScalar<ParameterPromotion, decimal>(SqlGetPriceDealer, parPromotion);
            var Res = new PricePromotion(){ Price = PriceDealer };
            foreach (var el in db.Execute<ParameterPromotion, PricePromotion>(SqlGetPrice, parPromotion))
            {
                if(el.CalcPrice(PriceDealer)< Res.Price)
                {
                    Res = el;
                    Res.Price = el.CalcPrice(PriceDealer);
                }

            }
            return Res;
        }

        public virtual Int64 GetPricePromotionSale2Category(IdReceiptWares parIdReceiptWares)
        {
            return db.ExecuteScalar<IdReceiptWares, Int64>(SqlGetPricePromotionSale2Category, parIdReceiptWares);           

        }

        
        [Obsolete("Устарівша функція використовувувати акційний механізм через GetTypeDiscountClientByReceipt")]
        public decimal GetPersentDiscountClientByReceipt(IdReceipt parIdReceipt)
        {
            return db.ExecuteScalar<IdReceipt, decimal>(SqlGetPersentDiscountClientByReceipt, parIdReceipt);
        }

        public int GetTypeDiscountClientByReceipt(IdReceipt parIdReceipt)
        {
            return db.ExecuteScalar<IdReceipt, int>(SqlGetTypeDiscountClientByReceipt, parIdReceipt);
        }
        


        public virtual MinPriceIndicative GetMinPriceIndicative(IdReceiptWares parIdReceiptWares)
        {
            var res = db.Execute<IdReceiptWares,MinPriceIndicative>(SqlGetMinPriceIndicative, parIdReceiptWares);
            if (res != null)
                return res.FirstOrDefault();
            return null;
        }
        

        public virtual bool DeleteWaresReceiptPromotion(IdReceipt parIdReceipt)
        {
            db.ExecuteNonQuery<IdReceipt>(SqlDeleteWaresReceiptPromotion, parIdReceipt);
            return true;
        }

        public virtual bool ReplaceWaresReceiptPromotion(IEnumerable<WaresReceiptPromotion> parData)
        {
            db.BulkExecuteNonQuery<WaresReceiptPromotion>(SqlReplaceWaresReceiptPromotion, parData);
            return true;
        }

        public virtual bool ReplaceWorkPlace(IEnumerable<WorkPlace> parData)
        {
            db.BulkExecuteNonQuery<WorkPlace>(SqlReplaceWorkPlace, parData);
            return true;
        }

        public virtual IEnumerable<WorkPlace> GetWorkPlace()
        {
            return db.Execute<WorkPlace>(SqlGetWorkplace);             
        }
        public virtual bool MoveReceipt(ParamMoveReceipt parMoveReceipt)
        {
            db.ExecuteNonQuery<ParamMoveReceipt>(SqlDeleteWaresReceiptPromotion, parMoveReceipt);
            return true;
        }
        public virtual bool ReplacePayment(IEnumerable<Payment> parData)
        {
            db.BulkExecuteNonQuery<Payment>(SqlReplacePayment, parData);
            return true;
        }

        public virtual bool SetStateReceipt(Receipt parReceipt)
        {
            db.ExecuteNonQuery<Receipt>(SqlSetStateReceipt, parReceipt);
            return true;
        }

        public virtual bool InsertWeight(Object parWeight)
        {
            db.ExecuteNonQuery<Object>(SqlInsertWeight, parWeight);
            return true;
        }
        
        public virtual IEnumerable<Payment> GetPayment(IdReceipt parIdReceipt)
        {      
            return db.Execute<IdReceipt, Payment>(SqlGetPayment, parIdReceipt);
        }
        public virtual IEnumerable<IdReceipt> GetIdReceiptbyState(eStateReceipt parState = eStateReceipt.Print) 
        { 
            return db.Execute<object, IdReceipt>(SqlGetIdReceiptbyState, new { StateReceipt= parState });
        }

    }

}
