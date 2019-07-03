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

//using DatabaseLib; // тимчасово для ParametersCollection
namespace SharedLib
{

    /// <summary>
    /// Клас з віртуальними методоами для доступу до БД (Work Data Base)
    /// </summary>
    public class WDB
    {
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

        protected Hashtable keySQL = new Hashtable();

        protected string SqlGetLastUseCodeEkka = @"";
        protected string SqlAddWaresEkka = @"";
        protected string SqlDeleteWaresEkka = @"";
        protected string SqlGetCodeEKKA = @"";

        protected string SqlTranslation = @"";
        protected string SqlFieldInfo = @"";
        protected string SqlGetPermissions = @"";
        protected string SqlGetAllPermissions = @"";
        protected string SqlCopyWaresReturnReceipt = @"";

        protected string SqlReplaceUnitDimension= @"";
        protected string SqlReplaceWares= @"";
        protected string SqlReplaceAdditionUnit= @"";
        protected string SqlReplaceBarCode= @"";
        protected string SqlReplacePrice= @"";
        protected string SqlReplaceTypeDiscount= @"";
        protected string SqlReplaceClient= @"";

        protected string SqlCreateMIDTable = @"";
        protected string SqlCreateMIDIndex = @"";

        protected string SqlGetDimUnitDimension = @"";
        protected string SqlGetDimWares = @"";
        protected string SqlGetDimAdditionUnit = @"";
        protected string SqlGetDimBarCode = @"";
        protected string SqlGetDimPrice = @"";
        protected string SqlGetDimTypeDiscount = @"";
        protected string SqlGetDimClient = @"";
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
            
            return false;
        }

        public virtual T GetConfig<T>(string parStr)
		{
            return default(T);//null;
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

		public virtual DataTable RightOfAccess (int parCodeUser)
		{
			return null;
		}
		
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
			return 0;
		}

        public virtual bool ClearT1()
        {
            return true;
        }
        /// <summary>
        /// Повертає знайдений товар/товари
        /// </summary>
        public virtual IEnumerable<ReceiptWares> FindWares(decimal parDiscount=0)
		{
            return null;
		}

        public virtual RezultFind FindWaresByName(string parPhone)
        {
            return new RezultFind() { Count = 0, TypeFind = TypeFind.Client };
        }


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
		
		/// <summary>
		/// Повертає знайденого клієнта(клієнтів)
		/// </summary>
		/// <param name="parCodeWares">Код товару</param>
		/// <returns>
		///Повертає DataTable з клієнтами
		///</returns>
		public virtual IEnumerable<Client> FindClient()
		{
			return null;
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
			return null;
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
			return false;
		}
		public virtual bool  UpdateClient(IdReceipt parIdReceipt, int parCodeClient)
		{
			return false;
		}
		
		public virtual bool  CloseReceipt(Receipt parReceipt)
		{
			return false;
		}


		/// <summary>
		/// Повертає фактичну кількість після вставки(добавляє до текучої кількості - -1 якщо помилка;
		/// </summary>
		/// <param name="parParameters"></param>
		/// <returns></returns>
		public virtual bool AddWares(ReceiptWares parReceiptWares)
		{
			return false;
		}
		
		public virtual decimal GetCountWares(IdReceiptWares parIdReceiptWares)
		{
			return 0;
		}
		
		public virtual bool UpdateQuantityWares(ReceiptWares parIdReceiptWares)
		{
			return false;
		}
		
		
		public virtual bool DeleteReceiptWares(IdReceiptWares parIdReceiptWares)
		{
			return false;
		}
        public virtual Receipt ViewReceipt(IdReceipt par)
        {
            return null;
        }

        /// <summary>
        /// Показує інформацю про товари в чеку 
        /// </summary>
        /// <param name="parParameters"> </param>
        /// <returns></returns>
        public virtual IEnumerable<ReceiptWares> ViewReceiptWares(IdReceipt par)
		{
			return null;
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
			return false;
		}
		
		public virtual bool  AddZ(System.Data.DataRow parRow )
		{
			return false;
		}
		
		public virtual bool  AddLog(System.Data.DataRow parRow )
		{
			return false;
		}
		
		public virtual System.Data.DataRow GetPrice(int parCodeDealer, int parCodeWares, decimal parDiscount)
		{
			return null;
		}
		
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
            SqlUpdateClient = GetSQL("SqlUpdateClient");
            SqlCloseReceipt = GetSQL("SqlCloseReceipt");
            SqlAddWares = GetSQL("SqlAddWares");
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
            SqlGetLastUseCodeEkka = GetSQL("SqlGetLastUseCodeEkka");
            SqlAddWaresEkka = GetSQL("SqlAddWaresEkka");
            SqlDeleteWaresEkka = GetSQL("SqlDeleteWaresEkka");
            SqlGetCodeEKKA = GetSQL("SqlGetCodeEKKA");
            SqlTranslation = GetSQL("SqlTranslation");
            SqlFieldInfo = GetSQL("SqlFieldInfo");
            SqlGetAllPermissions = GetSQL("SqlGetAllPermissions");
            SqlGetPermissions = GetSQL("SqlGetPermissions");
            SqlCopyWaresReturnReceipt = GetSQL("SqlCopyWaresReturnReceipt");

            SqlReplaceUnitDimension = GetSQL("SqlReplaceUnitDimension");
            SqlReplaceWares = GetSQL("SqlReplaceWares");
            SqlReplaceAdditionUnit = GetSQL("SqlReplaceAdditionUnit");
            SqlReplaceBarCode = GetSQL("SqlReplaceBarCode");
            SqlReplacePrice = GetSQL("SqlReplacePrice");
            SqlReplaceTypeDiscount = GetSQL("SqlReplaceTypeDiscount");
            SqlReplaceClient = GetSQL("SqlReplaceClient");

            SqlCreateMIDTable = GetSQL("SqlCreateMIDTable");
            SqlCreateMIDIndex = GetSQL("SqlCreateMIDIndex");


            SqlGetDimUnitDimension = GetSQL("SqlGetDimUnitDimension");
            SqlGetDimWares = GetSQL("SqlGetDimWares");
            SqlGetDimAdditionUnit = GetSQL("SqlGetDimAdditionUnit");
            SqlGetDimBarCode = GetSQL("SqlGetDimBarCode");
            SqlGetDimPrice = GetSQL("SqlGetDimPrice");
            SqlGetDimTypeDiscount = GetSQL("SqlGetDimTypeDiscount");
            SqlGetDimClient = GetSQL("SqlGetDimClient");

            return true;
        }
		
		public virtual bool LoadDataFromFile(string parFile)
		{
			return false;
		}
		public virtual int GetLastUseCodeEkka()
		{
			return -1;
		}
/*		public virtual bool AddWaresEkka(ParametersCollection parParameters )
		{
			return false;
		}*/
		public virtual bool DeleteWaresEkka()
		{
			return false;
		}
		
/*		public virtual int GetCodeEKKA(ParametersCollection parParameters )
		{
			return  -1;
		}

		public virtual DataTable Translation(ParametersCollection parParameters )
		{
			return  null ;
		}
        */
/*		public virtual DataTable FieldInfo(ParametersCollection parParameters = null)
		{
			return  null;
		}*/
		public virtual bool RecalcHeadReceipt(IdReceipt parReceipt)
		{
			return  false;
		}
        /*
                public virtual System.Data.DataTable GetAllPermissions(int parCodeUser)
                {
                    return null;
                }

                public virtual TypeAccess GetPermissions(int parCodeUser, CodeEvent parEvent)
                {
                    return TypeAccess.No ;
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

		
	}

}
