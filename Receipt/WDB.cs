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
using SharedLib;
using Model;
//using DatabaseLib; // тимчасово для Parameter
namespace MID
{
	/// <summary>
	/// Клас з віртуальними методоами для доступу до БД (Work Data Base)
	/// </summary>
	public class WDB
	{
		public delegate void CallWriteLogSQL(string parQvery, Parameter[] parParameters = null);
        /// <summary>
        /// Функція для запису SQL запитів в локальну LOG базу 
        /// </summary>
		protected CallWriteLogSQL varCallWriteLogSQL; // это тот самый член-делегат :))
		
		public string varVersion="0.0.1";
		protected string varSqlCreateT = @"";
		protected string varSqlInsertT1 = @"";
		protected string varSqlCreateReceiptTable = @"";
		protected string varSqlInitGlobalVar= @"";
		protected string varSqlConfig= @"";
		/// <summary>
		/// Процедура пошуку.(для Баз з можливістю stored procedure) oracle,mssql,...
		/// </summary>
		protected string varSqlFind = @"";
		protected string varSqlFindWaresBar=@"";
		protected string varSqlFindWaresCode=@"";
		protected string varSqlFindWaresName=@"";
		protected string varSqlFindClientBar=@"";
		protected string varSqlFindClientCode=@"";
		protected string varSqlFindClientName=@"";
		/// <summary>
		/// Запит, який вертає знайдених клієнтів
		/// </summary>
		protected string varSqlFoundClient=@"";
		/// <summary>
		/// Запит, який вертає знайдені товари
		/// </summary>
		protected string varSqlFoundWares =@"";
		/// <summary>
		/// Повертає доступні одиниці виміру по товару
		/// </summary>
		protected string varSqlAdditionUnit = @"";
		/// <summary>
		/// Добавляє чек в базу.
		/// </summary>
		protected string varSqlAddReceipt =@"";
		/// <summary>
		/// Запит який вертає інформацію про товари в чеку
		/// </summary>
		protected string varSqlViewReceipt=@"";
		// <summary>
		/// Міняє клієнта в чеку
		/// </summary>
		protected string varSqlUpdateClient =@"";
		protected string varSqlCloseReceipt =@"";
		/// <summary>
		/// Добавляє товарну позицію в чек
		/// </summary>
		protected string varSqlAddWares = @"";
		protected string varSqlGetCountWares = @"";
		protected string varSqlUpdateQuantityWares = @"";
		protected string varSqlDeleteWaresReceipt = @"";
		protected string varSqlRecalcHeadReceipt = @"";
		/// <summary>
		/// Внесення винесення грошей.
		/// </summary>
		protected string varSqlInputOutputMoney = @"";
		/// <summary>
		/// Добавляє інформацію про Z-звіт
		/// </summary>
		protected string varSqlAddZ = @"";
		/// <summary>
		/// Добавляє інформацію в log файл
		/// </summary>
		protected string varSqlAddLog = @"";
		/// <summary>
		/// Запит для генерації кодів по робочому місці.(наприклад номер чека)
		/// </summary>
		protected string varSqlGenWorkPlace=@"";
		protected string varSqlInsertGenWorkPlace = @"";
		protected string varSqlSelectGenWorkPlace = @"";
		protected string varSqlUpdateGenWorkPlace = @"";
		
		protected string varSqlLogin = @"";
		protected string varSqlGetPrice = @"";
		
		protected string varSqlPrepareLockFilterT1 = @"";
		protected string varSqlPrepareLockFilterT2 = @"";
		protected string varSqlPrepareLockFilterT3 = @"";
		protected string varSqlPrepareLockFilterT4 = @"";
		protected string varSqlPrepareLockFilterT5 = @"";
		protected string varSqlListPS = @"";
		protected string varSqlUpdatePrice = @"";
			
		protected Hashtable keySQL = new Hashtable();
		
		protected string varSqlGetLastUseCodeEkka = @"";
		protected string varSqlAddWaresEkka = @"";
		protected string varSqlDeleteWaresEkka = @"";
		protected string varSqlGetCodeEKKA = @"";
		
		protected string varSqlTranslation = @"";
		protected string varSqlFieldInfo  = @"";
		protected string varSqlGetPermissions = @"";
		protected string varSqlGetAllPermissions = @"";
		protected string varSqlCopyWaresReturnReceipt = @"";
		
		public WDB(CallWriteLogSQL parCallWriteLogSQL=null)
		{
			this.varCallWriteLogSQL=parCallWriteLogSQL;
		}
		
		public virtual DataTable Execute(string parQuery, Parameter[] parPrameters = null)
		{
			return null;
		}
		
		public virtual int ExecuteNonQuery(string parQuery, Parameter[] parParameters = null)
		{
			return 0;
		}
		public virtual DataRow GetGlobalVar(int parIdWorkPlace)
		{
			return null;
		}
		public virtual bool InsertT1(Parameter[] parParameters)
		{
			return false;
		}
		
		public virtual object GetConfig(string parStr)
		{
			return null;
		}
		/// <summary>
		/// Конектиться до бази та повертає табличку з правами.
		/// </summary>
		/// <param name="parLogin">Логін</param>
		/// <param name="parPassword">Пароль</param>
		/// <returns>row(код,назва,логін,пароль) Код користувача, -1 - Відсутній зв'язок з базою, -2  - неправильний логін/пароль</returns>

		public virtual DataRow Login (string parLogin,string parPassword)
		{
			return null;
		}

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
		public virtual RezultFind FindData( string parStr,TypeFind parTypeFind = TypeFind.All)
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
		/// <summary>
		/// Повертає знайдений товар/товари
		/// </summary>
		public virtual System.Data.DataTable FindWares(Parameter[] parParameters=null)
		{
			return null;
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
		public virtual System.Data.DataTable FindClient()
		{
			return null;
		}
		
		/// <summary>
		/// Вертає код Чека в зазначеному періоді
		/// </summary>
		/// <param name="parCodePeriod">Код періоду (201401 - січень 2014)</param>
		/// <returns>
		///Повертає код чека
		///</returns>
		public virtual int GetCodeReceipt( int parCodePeriod = 0)
		{
			return 0;
		}
		
		/// <summary>
		/// Повертає іформацію про товари в чеку
		/// </summary>
		public virtual System.Data.DataTable ViewWarReceipt(Parameter[] parParameters)
		{
			return null;
		}
		/// <summary>
		/// Добавляє ттовар до чека в базу
		/// </summary>
		/// <param name="parRow">Рядок з інформацією про товар - ціну, кількість і тд.</param>
		/// <returns>
		///Успішно чи ні виконана операція
		///</returns>
		public virtual bool  AddReceipt(System.Data.DataRow parRow )
		{
			return false;
		}
		public virtual bool  AddReceipt(Parameter[] parParameters)
		{
			return false;
		}
		public virtual bool  UpdateClient(Parameter[] parParameters)
		{
			return false;
		}
		
		public virtual bool  CloseReceipt(Parameter[] parParameters)
		{
			return false;
		}

		/// <summary>
		/// Повертає фактичну кількість після вставки(добавляє до текучої кількості - -1 якщо помилка;
		/// </summary>
		/// <param name="parParameters"></param>
		/// <returns></returns>
		public virtual bool AddWares(Parameter[] parParameters )
		{
			return false;
		}
		
		public virtual decimal GetCountWares(Parameter[] parParameters )
		{
			return 0;
		}
		
		public virtual bool UpdateQuantityWares(Parameter[] parParameters )
		{
			return false;
		}
		
		
		public virtual bool DeleteWaresReceipt(Parameter[] parParameters )
		{
			return false;
		}
		/// <summary>
		/// Показує інформацю про товар
		/// </summary>
		/// <param name="parParameters"> </param>
		/// <returns></returns>
		public virtual System.Data.DataTable ViewWaresReceipt(Parameter[] parParameters )
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
		public virtual bool  RecalcPrice(int parCodeReceipt =0)
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
		
		public virtual System.Data.DataRow GetPrice(Parameter[] parParameters)
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
			varSqlCreateT = GetSQL("varSqlCreateT");
			varSqlInsertT1 = GetSQL("varSqlInsertT1");
			varSqlCreateReceiptTable = GetSQL("varSqlCreateReceiptTable");
			varSqlInitGlobalVar = GetSQL("varSqlInitGlobalVar");
			varSqlConfig =GetSQL("varSqlConfig");
			varSqlFind = GetSQL("varSqlFind");
			varSqlFindWaresBar=GetSQL("varSqlFindWaresBar");
			varSqlFindWaresCode=GetSQL("varSqlFindWaresCode");
			varSqlFindWaresName=GetSQL("varSqlFindWaresName");
			varSqlFindClientBar=GetSQL("varSqlFindClientBar");
			varSqlFindClientCode=GetSQL("varSqlFindClientCode");
			varSqlFindClientName=GetSQL("varSqlFindClientName");
			varSqlFoundClient=GetSQL("varSqlFoundClient");
			varSqlFoundWares =GetSQL("varSqlFoundWares");
			varSqlAdditionUnit = GetSQL("varSqlAdditionUnit");
			varSqlViewReceipt= GetSQL("varSqlViewReceipt");
			varSqlAddReceipt =GetSQL("varSqlAddReceipt");
			varSqlUpdateClient =GetSQL("varSqlUpdateClient");
			varSqlCloseReceipt =GetSQL("varSqlCloseReceipt");
			varSqlAddWares = GetSQL("varSqlAddWares");
			varSqlRecalcHeadReceipt = GetSQL("varSqlRecalcHeadReceipt");
			varSqlGetCountWares= GetSQL("varSqlGetCountWares");
			varSqlUpdateQuantityWares = GetSQL("varSqlUpdateQuantityWares");
			varSqlDeleteWaresReceipt = GetSQL("varSqlDeleteWaresReceipt");
 			varSqlInputOutputMoney = GetSQL("varSqlInputOutputMoney");
			varSqlAddZ = GetSQL("varSqlAddZ");
			varSqlAddLog = GetSQL("varSqlAddLog");
			varSqlGenWorkPlace=GetSQL("varSqlGenWorkPlace");
			varSqlInsertGenWorkPlace=GetSQL("varSqlInsertGenWorkPlace");
			varSqlInsertGenWorkPlace = GetSQL("varSqlInsertGenWorkPlace");
			varSqlSelectGenWorkPlace = GetSQL("varSqlSelectGenWorkPlace");
			varSqlUpdateGenWorkPlace = GetSQL("varSqlUpdateGenWorkPlace");
			varSqlGetPrice = GetSQL("varSqlGetPrice");
			varSqlLogin = GetSQL("varSqlLogin");
			varSqlPrepareLockFilterT1=GetSQL("varSqlPrepareLockFilterT1");
			varSqlPrepareLockFilterT2=GetSQL("varSqlPrepareLockFilterT2");
			varSqlPrepareLockFilterT3=GetSQL("varSqlPrepareLockFilterT3");
			varSqlPrepareLockFilterT4=GetSQL("varSqlPrepareLockFilterT4");
			varSqlPrepareLockFilterT5=GetSQL("varSqlPrepareLockFilterT5");
			varSqlListPS=GetSQL("varSqlListPS");
			varSqlGetLastUseCodeEkka = GetSQL("varSqlGetLastUseCodeEkka");
			varSqlAddWaresEkka= GetSQL("varSqlAddWaresEkka");
			varSqlDeleteWaresEkka = GetSQL("varSqlDeleteWaresEkka");
			varSqlGetCodeEKKA = GetSQL("varSqlGetCodeEKKA");
			varSqlTranslation = GetSQL("varSqlTranslation");
		    varSqlFieldInfo = GetSQL("varSqlFieldInfo");
		    varSqlGetAllPermissions = GetSQL("varSqlGetAllPermissions");
		    varSqlGetPermissions = GetSQL("varSqlGetPermissions");
		    varSqlCopyWaresReturnReceipt = GetSQL("varSqlCopyWaresReturnReceipt");		    
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
		public virtual bool AddWaresEkka(Parameter[] parParameters )
		{
			return false;
		}
		public virtual bool DeleteWaresEkka()
		{
			return false;
		}
		
		public virtual int GetCodeEKKA(Parameter[] parParameters )
		{
			return  -1;
		}

		public virtual DataTable Translation(Parameter[] parParameters )
		{
			return  null ;
		}
		public virtual DataTable FieldInfo(Parameter[] parParameters = null)
		{
			return  null;
		}
		public virtual bool RecalcHeadReceipt(Parameter[] parParameters = null)
		{
			return  false;
		}

		public virtual System.Data.DataTable GetAllPermissions(int parCodeUser)
		{
			return null;
		}
		
		public virtual TypeAccess GetPermissions(int parCodeUser, CodeEvent parEvent)
		{
			return TypeAccess.No ;
		}

		/// <summary>
		/// </summary>
		/// <param name="parParameters"></param>
		/// <param name="parIsCurrentDay"> Якщо повернення в день продажу. Важливо тільки для SQLite </param>
		/// <returns></returns>
		public virtual bool CopyWaresReturnReceipt(Parameter[] parParameters = null, bool parIsCurrentDay = true )
		{
			return false;
		}

		
	}

}
