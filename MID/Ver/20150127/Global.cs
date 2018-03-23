using System;
using System.Data;
using System.IO;


namespace MID
{
	
	/// <summary>
	/// Глобальні змінні.
	/// </summary>
	public struct GlobalVar
	{
		/// <summary>
		/// Текучий чек (1-3) з масиву varReceipts
		/// </summary>
		public static int varCurrentReceipt = 0;
		/// <summary>
		/// Максимальна кількість відкритих чеків касиром. Максимум 3.
		/// </summary>
		public static int  varQuantityOpenReceipt = 3;
		/// <summary>
		/// Список чеків (0-немає чека)
		/// </summary>
		public static int[] varReceipts = {0,0,0};
		/// <summary>
		/// Перелік одиниць, по яким необхідно вводити кількість(при пошуку по назві чи коду)
		/// </summary>
		public static int[] varUnitMustInputQuantity ={7};
		/// <summary>
		/// тип пошуку товара (0 - без обмежень, 1 - штрихкод , код, 2 - штрихкод)
		/// </summary>
		public static int varTypeFindWares = 0;
		/// <summary>
		/// тип пошуку клієнта (0 - без обмежень, 1 - штрихкод , код, 2 - штрихкод)
		/// </summary>
		public static int varTypeFindClient = 1;
		/// <summary>
		/// Id робочого місця
		/// </summary>
		public static int varIdWorkPlace = 140701;
		
		public static int varCodeWarehouse = 8151; // 1407 Сента;
		/// <summary>
		/// Тип періоду документів (0 - Глобальний, 1- рік, 2 -місяць, 3 - день.)
		/// </summary>
		public static int varTypePeriod =2;
		/// <summary>
		/// Мінімальна кількість симвлолів в штрихкоді товару
		/// </summary>
		public static int varMinLenghtBarCodeWares=7;
		/// <summary>
		/// Мінімальна кількість симвлолів в штрихкоді клієнта
		/// </summary>
		public static int varMinLenghtBarCodeClient=13;
		/// <summary>
		/// Шаблон чеків по замовчуванню
		/// </summary>
		public static int varDefaultCodePatternReceipt=-1;
		
		public static int [] varDefaultCodeDealer={0,0,0,0,0};
		
		public static int varDefaultCodeClient=0;
		
		public static string varBillCoins ="грн:1:500,200,100,50,20,10,5,2,1;коп:0.01:50,25,10,5,2,1";
		
		/// <summary>
		/// Перераховувати ціни після кожної зміни в чеку
		/// </summary>
		public static bool varRecalcPriceOnLine = true;
		/*		/// <summary>
		/// Кількість пос-терміналів на касі.
		/// </summary>
		public static int varQuantityPos = 2;*/
		/// <summary>
		/// Присутній пос-термінал на касі -1 - Ні, 0 - так без автоматизації, >0 код моделі(драйвера)
		/// </summary>
		public static int [] varModelPos = {1,0,-1};
		
		/// <summary>
		/// Чи встановлено конект з POS.
		/// </summary>
		public static bool varIsPosConnect = false;

		/// <summary>
		/// Шлях до SqlLite бази
		/// </summary>
		public static string varPathDB = @"d:\";

		/// <summary>
		/// Шлях до папки, в яку пиcати LOG
		/// </summary>
		public static string varPathLog = @"d:\temp\";
		
		/// <summary>
		/// Шлях до MID.ini та Key.map
		/// </summary>
		public static string varPathIni = @"";
		
		public static Language  varLanguage = Language.uk_UA ;
		//public static string var
		//public static DateTime varArxDate= new DateTime (1,1,1);\
	}
	/// <summary>
	/// Глобальні функції
	/// </summary>
	static  class Global
	{
		public static ReadINI2 varIniKeyMap;
		/// <summary>
		/// Повертає  код текучого періоду
		/// </summary>
		/// <returns>Код Текучого періоду</returns>
		static public int  GetCodePeriod()
		{
			return GetCodePeriod(DateTime.Today);
		}
		/// <summary>
		/// Повертає  код періоду по даті
		/// </summary>
		/// <param name="varD">дата поякій вернути період</param>
		/// <returns>Код періоду</returns>
		public static int  GetCodePeriod(DateTime varD  )
		{
			if( varD==null)
				varD=DateTime.Today;
			switch (GlobalVar.varTypePeriod)
			{
				case 0:
					return 0;
				case 1:
					return Convert.ToInt32( varD.ToString("yyyy"));
				case 2:
					return Convert.ToInt32(varD.ToString("yyyyMM"));
			}
			return 0;
		}
		public static bool  GetAccses(int parLevelAccess,int  parCodeAccess,int parTypeAccess = 0)
		{
			//int parCodeUser,
			return true;
		}
		
		public static bool  GetAccsesUser(int parCodeUser,int parLevelAccess,int  parCodeAccess,int parTypeAccess = 0)
		{
			//int parCodeUser,
			return true;
		}
		
		public static bool IsUnitMustInputQuantity(int parCodeUnit)
		{
			for(int i=0;i<GlobalVar.varUnitMustInputQuantity.Length;i++)
				if(GlobalVar.varUnitMustInputQuantity[i]==parCodeUnit)
					return true;
			return false;
		}

		public static int GetIndexDefaultUnit(DataTable parDt)
		{
			if(parDt==null)
				return -1;
			for(int i=0;i<parDt.Rows.Count;i++)
				if(parDt.Rows[i]["DEFAULT_UNIT"].ToString()=="Y" )
					return i;
			return 0;
		}

		public static int BildKeyCode(string parSection,string parKeyName)
		{
			int varKeyCode=0;
			string[] varStrKeys= varIniKeyMap.GetSetting(parSection,parKeyName).Split(new char[] { '+' }, 4);
			for(int i=0;i<varStrKeys.Length ;i++)
				varKeyCode+=Convert.ToInt32(varIniKeyMap.GetSetting("KeyCode",varStrKeys[i].Trim()));
			return 	varKeyCode;
		}

		public static void InitKeyMap()
		{
			varIniKeyMap = new ReadINI2(GlobalVar.varPathIni+@"Key.map");
			PayKeyMap.Init();
			MainInputKeyMap.Init();
		}
		
		public static void InitConfig(WDB parWDB)
		{
			GlobalVar.varDefaultCodeClient = Convert.ToInt32(  parWDB.GetConfig("DEFAULT_CODE_CLIENT"));
			GlobalVar.varDefaultCodeDealer[0] = Convert.ToInt32(  parWDB.GetConfig("DEFAULT_CODE_DEALER")); //TMP
			GlobalVar.varTypeFindClient = Convert.ToInt32(  parWDB.GetConfig("TYPE_FIND_CLIENT"));
			GlobalVar.varTypeFindWares = Convert.ToInt32(  parWDB.GetConfig("TYPE_FIND_WARES"));
			GlobalVar.varDefaultCodePatternReceipt = Convert.ToInt32(  parWDB.GetConfig("DEFAULT_CODE_PATTERN_RECEIPT"));
			GlobalVar.varBillCoins = Convert.ToString(  parWDB.GetConfig("BILL_COINS"));
			GlobalVar.varMinLenghtBarCodeClient = Convert.ToInt32(  parWDB.GetConfig("MIN_LENGHT_BAR_CODE_CLIENT"));
			GlobalVar.varMinLenghtBarCodeWares = Convert.ToInt32(  parWDB.GetConfig("MIN_LENGHT_BAR_CODE_WARES"));
			GlobalVar.varTypePeriod = Convert.ToInt32(  parWDB.GetConfig("TYPE_PERIOD"));
			GlobalVar.varUnitMustInputQuantity[0] =  Convert.ToInt32(  parWDB.GetConfig("UNIT_MUST_INPUT_QUANTITY")); //TMP
			GlobalVar.varQuantityOpenReceipt = Convert.ToInt32(  parWDB.GetConfig("QUANTITY_OPEN_RECEIPT")); //TMP
			
		}
		
		public static void InitIni()
		{
			ReadINI2 varIni = new ReadINI2(GlobalVar.varPathIni+@"MID.ini");
			GlobalVar.varIdWorkPlace = Convert.ToInt32( varIni.GetSetting(@"MAIN",@"ID_WORKPLACE"));
			GlobalVar.varPathDB =  varIni.GetSetting(@"MAIN",@"PATH_DB").Trim();
			GlobalVar.varPathLog = varIni.GetSetting(@"MAIN",@"PATH_LOG").Trim();
		}
		public static void WriteToFile(string parFile,string parStr)
		{
			StreamWriter sw;
			FileInfo fi = new FileInfo(parFile);
			sw = fi.AppendText();
			sw.WriteLine(parStr);
			sw.Flush();
			sw.Close();
		}
		
		public static void Log(CodeLog parCodeLog,int parCodeOperationLog = 0 ,int parCodeOperationExLog = 0,string parString = null )
		{
			WriteToFile(GlobalVar.varPathLog+ @"\mid"+ DateTime.Today.ToString("yyyyMMDD")+ @".log" ,"\"" + parCodeLog.ToString().Trim()+"\","+ parCodeOperationLog.ToString().Trim()+"\","+"\","+ parCodeOperationExLog.ToString().Trim()+"\"," );
		}
		public static string Translation(string parStr,Language parLanguage = Language.uk_UA)
		{
			return parStr;
		}
		
		public static ColumInfo InfoColumnForGrid(string parName,Language parLanguage = Language.uk_UA)
		{
			ColumInfo varColumInfo;
			varColumInfo.Name=parName;
			varColumInfo.With=80;
			varColumInfo.TypeFilter=0;
			varColumInfo.PrintName= Translation(parName,parLanguage);
			varColumInfo.ExInfo="";
			return varColumInfo;
		}
		public static bool BildGrid(System.Windows.Forms.DataGridView parGrid, DataTable parDT)
		{
			parGrid.DataSource=parDT;
			parGrid.Top=50;
			parGrid.Left=20;
			parGrid.Height=	parGrid.Parent.Height-2*parGrid.Top;
			parGrid.Width=parGrid.Parent.Width-2*parGrid.Left;
	
			foreach ( DataColumn col in parDT.Columns)
			{
				ColumInfo varColumInfo;
				varColumInfo=InfoColumnForGrid(col.ColumnName);
				parGrid.Columns[col.ColumnName].Width=varColumInfo.With;
				parGrid.Columns[col.ColumnName].Tag= "TAG"+varColumInfo.PrintName;
				parGrid.Columns[col.ColumnName].HeaderText="HT"+varColumInfo.PrintName;
			}
			parGrid.Visible=parGrid.Enabled=true;
			return true;
		}
		
		
	}

	/// <summary>
	/// Зберігає інформацію про клієнта
	/// </summary>
	public class Client
	{
		/// <summary>
		/// Код клієнта
		/// </summary>
		public int varCodeClient;
		/// <summary>
		///  Назва клієнта
		/// </summary>
		public string varNameClient;
		/// <summary>
		/// Тип знижки
		/// </summary>
		public int varTypeDiscount;
		/// <summary>
		/// Відсоток знижки / надбавки
		/// </summary>
		public double varDiscount;
		/// <summary>
		/// Код дилерської категорії
		/// </summary>
		public int varCodeDealer;
		/// <summary>
		/// Сума накопичених бонусів
		/// </summary>
		public decimal varSumBonus;
		/// <summary>
		/// Сума накопичених бонусів в грошовому еквіваленті
		/// </summary>
		public decimal varSumMoneyBonus;
		/// <summary>
		/// Чи можна списувати бонуси за рахунок здачі
		/// </summary>
		public bool varIsUseBonusToRest;
		/// <summary>
		/// Чи можна нараховувати бонуси з здачі
		/// </summary>
		public bool varIsUseBonusFromRest;
		
		public Client()
		{
			Clear();
		}
		public void Clear()
		{
			varCodeClient=0 ;
			varNameClient="" ;
			varTypeDiscount=0;
			varDiscount=0;
			varCodeDealer=0;
			varSumBonus=0;
			varSumMoneyBonus=0;
			varIsUseBonusFromRest=false;
			varIsUseBonusToRest=false;
			
		}
		
		public virtual void  SetClient(DataRow parRw)
		{
			Clear();
			varCodeClient=Convert.ToInt32(parRw["Code_Client"]);
			varNameClient =  Convert.ToString(parRw["Name_Client"]);
			varTypeDiscount=Convert.ToInt32(parRw["Type_Discount"]);
			varDiscount=Convert.ToInt32(parRw["Discount"]);
			varCodeDealer=Convert.ToInt32(parRw["Code_Dealer"]);
			varSumBonus=Convert.ToDecimal(parRw["Sum_Bonus"]);
			varSumMoneyBonus=Convert.ToDecimal(parRw["Sum_Money_Bonus"]);
			varIsUseBonusFromRest = (Convert.ToInt32(parRw["Is_Use_Bonus_From_Rest"]) == 1);
			varIsUseBonusToRest= (Convert.ToInt32(parRw["Is_Use_Bonus_To_Rest"]) == 1);

			if(varCodeDealer<=0)
				varCodeDealer=GlobalVar.varDefaultCodeDealer[-varCodeDealer];
		}
		
		
	}
	/// <summary>
	/// Зберігає інформацію про користувача.
	/// </summary>
	public class  User
	{
		/// <summary>
		/// Код користувача
		/// </summary>
		public int varCodeUser;
		/// <summary>
		/// Назва користувача
		/// </summary>
		public string varNameUser;
		/// <summary>
		///  Логін користувача
		/// </summary>
		public string varLogin;
		/// <summary>
		/// Пароль користувача
		/// </summary>
		public string varPassword;
		//varContext = 0; 	// Текучий контекст
		public User()
		{
			Clear();
		}
		public void Clear()
		{
			varCodeUser=0;
			varNameUser="";
			
			varPassword="";
		}
		public void SetUser(DataRow parRw)
		{
			varCodeUser = Convert.ToInt32(parRw["code_user"]);
			varNameUser =  Convert.ToString(parRw["name_user"]);
			varLogin =  Convert.ToString(parRw["Login"]);
			varPassword =  Convert.ToString(parRw["password"]);
		}
	}

	/// <summary>
	/// Зберігає інформацію про чек
	/// </summary>
	public class Receipt
	{
		/// <summary>
		/// Код Чека
		/// </summary>
		public int varCodeReceipt;
		/// <summary>
		/// Дата Чека
		/// </summary>
		public DateTime varDateReceipt;
		/// <summary>
		/// Код періода
		/// </summary>
		public int varCodePeriod;
		
		
		public decimal varSumReceipt;
		public decimal varVatReceipt;
		public decimal varSumRest;
		/// <summary>
		/// Оплачено Готівкою
		/// </summary>
		public decimal varSumCash;
		/// <summary>
		/// Сума оплачена Кредиткою
		/// </summary>
		public decimal varSumCreditCard;
		/// <summary>
		/// Сума використаних бонусних грн.
		/// </summary>
		public decimal varSumBonus;
		public Int64 varCodeCreditCard;
		public Int64 varNumberSlip;
		/// <summary>
		/// Послідній рядочов в чеку
		/// </summary>
		public int varSort;
		/// <summary>
		/// Шаблони чеків. ????
		/// </summary>
		public int varCodePattern ;
		public Receipt()
		{
			Clear();
		}
		public void Clear()
		{
			varCodeReceipt=0 ;  // Код Чека
			varDateReceipt= new DateTime(1,1,1);  // Дата Чека
			varCodePeriod=0;	// Код періода
			varSort=0;			// Послідній рядочов в чеку
			varCodePattern = 0;
			varSumReceipt = 0;
			varVatReceipt = 0;
			varSumCash = 0;
			varSumCreditCard = 0;
			varCodeCreditCard = 0;
			varSumBonus = 0;
			varNumberSlip = 0;
			
		}
		public void  SetReceipt(int parCodeReceipt,DateTime parDateReceipt = new DateTime())
		{
			if (parDateReceipt==new DateTime())
				parDateReceipt=DateTime.Today;
			
			Clear();
			varCodeReceipt=parCodeReceipt;
			varDateReceipt=parDateReceipt;
			varCodePeriod=Global.GetCodePeriod(parDateReceipt);
			varSort=0;
			varCodePattern=GlobalVar.varDefaultCodePatternReceipt;
		}
		
		
	}
	



	public class Wares
	{
		/// <summary>
		/// Код товару
		/// </summary>
		public int varCodeWares ;
		/// <summary>
		/// Назва товару
		/// </summary>
		public string varNameWares ;
		/// <summary>
		/// Назва для чека.
		/// </summary>
		public string varNameWaresReceipt;
		//public int   varTypeVat ;  			// Тип ставка ПДВ  ()
		/// <summary>
		/// % Ставки ПДВ (0 -0 ,20 -20%)
		/// </summary>
		public int varPercentVat ;
		/// <summary>
		/// Код одиниці виміру позамовчуванню
		/// </summary>
		/// 
		public int varTypeVat;
		public int varCodeDefaultUnit ;
		/// <summary>
		/// Коефіцієнт одиниці виіру по замовчуванню
		/// </summary>
		public int varCoefficientDefaultUnit ;

		// Інформація про ціноутворення
		/// <summary>
		/// 
		/// </summary>
		public decimal varPrice	; // ціна за базову одиницю.
		/// <summary>
		/// 
		/// </summary>
		public int varCodeDealer ; // Дилерська категорія.
		/// <summary>
		/// Тип ціноутворення ( 1 - ділерська категорія - 2 ділерська категорія+знижка,3 -фіксація ціни,4-Обмеження по нижньому індикативу, 5-Обмеження по верхньому індикативу, 9 -акція)
		/// </summary>
		public int varTypePrice ;

		/// <summary>
		/// 
		/// </summary>
		public int varSumDiscount ;
		//  varDiscount = // 
		
		// Інформація по знайденому товару
		/// <summary>
		/// Тип знайденої позиції 0-невідомо, 1 - по коду, 2 - По штрихкоду,  3- По штрихкоду - родичі. 4 - По назві
		/// </summary>
		public int varTypeFound;
		/// <summary>
		/// Текуча одиниця виміру
		/// </summary>
		public int varCodeUnit;
		/// <summary>
		/// Коефіцієнт текучої одиниці виміру
		/// </summary>
		public decimal varCoefficient ;
		/// <summary>
		/// Авривіатура текучої одиниці виміру
		/// </summary>
		public int varAbrUnit;
		/// <summary>
		/// Кількість товару
		/// </summary>
		public decimal varQuantity ;
		/// <summary>
		/// код періорду прихідної
		/// </summary>
		public int varCodePeriodIncome;
		/// <summary>
		/// Код прихідної
		/// </summary>
		public int varCodeIncome;
		/// <summary>
		/// 
		/// </summary>
		public bool varIsSave;
		public Wares()
		{
			Clear();
		}
		public void Clear()
		{
			varCodeWares = 0;
			varNameWares = "";
			varNameWaresReceipt="";
			varPercentVat = 0;
			varTypeVat = 0;
			varCodeDefaultUnit = 0;
			varCoefficientDefaultUnit = 0;
			varPrice	= 0;
			varCodeDealer = 0;
			varTypePrice =0;
			varSumDiscount = 0;
			varTypeFound=0;
			varCodeUnit=0;
			varCoefficient =0;
			varCodePeriodIncome=0;
			varCodeIncome=0;
			varQuantity =0;
			varIsSave=false;
		}
		public virtual void  SetWares(DataRow parRw, int parTypeFound=0)
		{
			Clear();
			if(parRw!=null)
			{
				varCodeWares = Convert.ToInt32(parRw["code_wares"]);
				varNameWares =  Convert.ToString(parRw["name_wares"]);
				varNameWaresReceipt = Convert.ToString(parRw["name_wares_receipt"]);
				varPercentVat= Convert.ToInt32(parRw["percent_vat"]);
				varCodeUnit = Convert.ToInt32(parRw["code_unit"]);
				varPrice = Convert.ToDecimal(parRw["price_dealer"]);
				varCoefficient = Convert.ToInt32(parRw["coefficient"]);
				varTypeFound = Convert.ToInt32(parTypeFound);
				varTypePrice= Convert.ToInt32(parRw["Type_Price"]);
				varTypeVat = Convert.ToInt32(parRw["Type_Vat"]);
			}
			
		}
		
	}
	
	
	public enum TypePay
	{
		Partiall = 0,
		Cash = 1,
		Pos  = 2,
		NonCash = 3
	}

	public enum TypeBonus
	{
		NonBonus = 0, // 
		Bonus = 1,
		BonusWithOutRest =2, // Використовувати бонус з врахуванням здачі( якщо бонуса не вистачає - берем таким чином щоб здача була кругла)
		BonusToRest= 3,
		BonusFromRest= 4
			
	}

	public enum TypeDeleteWaresReceipt
	{
		All = -1,
		Choice = 0,
		Current =1
	}
	
	
	public class MainInputKeyMap
	{
		public static int ClearInfoWares=0;
		public static int IncrementWares=0;
		public static int DecrementWares=0;
		public static int PrintReceipt_Cash=0;
		public static int PrintReceipt_Pos=0;
		public static int PrintReceipt_NonCash=0;
		public static int PrintReceipt_Partially=0;
		public static int AddWaresReceipt=0;
		public static int DeleteWaresReceipt_Current=0;
		public static int DeleteWaresReceipt_All=0;
		public static int DeleteWaresReceipt_Choice=0;
		public static int EditQuantityWares=0;
		public static int ChangeCurrentReceipt=0;
		public static int NewReceipt=0;
		public static int RecalcPrice=0;
		public static int Help=0;
		public static int PrintZ=0;
		public static int PrintX=0;
		public static int CountCash=0;
		public static int UpdateData=0;
		public static int InputOutputMoney=0;
		public static int PrintPosZ=0;
		public static int PrintPosX=0;
		public static void Init()
		{
			ClearInfoWares = Global.BildKeyCode("MainInput","ClearInfoWares");
			//int i=this.ClearInfoWares;
			IncrementWares = Global.BildKeyCode("MainInput","IncrementWares");
			DecrementWares = Global.BildKeyCode("MainInput","DecrementWares");
			PrintReceipt_Cash = Global.BildKeyCode("MainInput","PrintReceipt_Cash");
			PrintReceipt_Pos = Global.BildKeyCode("MainInput","PrintReceipt_POS");
			PrintReceipt_NonCash = Global.BildKeyCode("MainInput","PrintReceipt_NonCash");
			PrintReceipt_Partially = Global.BildKeyCode("MainInput","PrintReceipt_Partially");
			AddWaresReceipt = Global.BildKeyCode("MainInput","AddWaresReceipt");
			DeleteWaresReceipt_Current = Global.BildKeyCode("MainInput","DeleteWaresReceipt_Current");
			DeleteWaresReceipt_All = Global.BildKeyCode("MainInput","DeleteWaresReceipt_All");
			DeleteWaresReceipt_Choice = Global.BildKeyCode("MainInput","DeleteWaresReceipt_Choice");
			EditQuantityWares = Global.BildKeyCode("MainInput","EditQuantityWares");
			ChangeCurrentReceipt = Global.BildKeyCode("MainInput","ChangeCurrentReceipt");
			NewReceipt = Global.BildKeyCode("MainInput","NewReceipt");
			RecalcPrice = Global.BildKeyCode("MainInput","RecalcPrice");
			Help = Global.BildKeyCode("MainInput","Help");
			PrintZ = Global.BildKeyCode("MainInput","PrintZ");
			PrintX = Global.BildKeyCode("MainInput","PrintX");
			InputOutputMoney = Global.BildKeyCode("MainInput","InputOutputMoney");
			CountCash = Global.BildKeyCode("MainInput","CountCash");
			UpdateData = Global.BildKeyCode("MainInput","UpdateData");
			PrintPosX = Global.BildKeyCode("MainInput","PrintPosX");
			PrintPosZ = Global.BildKeyCode("MainInput","PrintPosZ");
		}
	}

	public class PayKeyMap
	{
		public static int Exit = 0;
		public static int UseBonus = 0;
		public static int UseBonusWithOutRest =0;
		public static int UseBonusToRest = 0;
		public static int UseBonusFromRest = 0;
		
		public static int PrintReceipt = 0;
		public static int UsePos1 = 0;
		public static int UsePos2 = 0;
		public static int UsePos3 = 0;
		public static void Init()
		{
			PayKeyMap.UseBonus = Global.BildKeyCode("Pay","UseBonus");
			PayKeyMap.UseBonusWithOutRest = Global.BildKeyCode("Pay","UseBonusWithOutRest");
			PayKeyMap.UseBonusToRest = Global.BildKeyCode("Pay","UseBonusToRest");
			PayKeyMap.UseBonusFromRest = Global.BildKeyCode("Pay","UseBonusFromRest");
			PayKeyMap.PrintReceipt = Global.BildKeyCode("Pay","PrintReceipt");
			PayKeyMap.UsePos1 = Global.BildKeyCode("Pay","UsePos1");
			PayKeyMap.UsePos2 = Global.BildKeyCode("Pay","UsePos2");
			PayKeyMap.UsePos3 = Global.BildKeyCode("Pay","UsePos3");
			PayKeyMap.Exit = Global.BildKeyCode("Pay","Exit");
		}
	}
	/// <summary>
	/// Будується запитом select listagg( c01||' = ' || t.code_data,','||chr(13)||chr(10))  within group(order by t.code_data ) from C.DATA_NAME t where t.data_level=210
	/// </summary>
	public enum CodeLog
	{
		Main = 0,
		Login = 1,
		NewReceipt = 2,
		SetClient = 3,
		RecalcPrice = 4,
		ChangeReceipt = 5,
		CloseReceipt = 9,
		FindMain = 10,
		Find = 11,
		Wares = 20,
		ClearInfoWares = 21,
		AddWaresReceipt = 22,
		IncrementWares = 23,
		DecrementWares = 24,
		EditQuantityWares = 25,
		ChangeUnit = 26,
		DeleteWaresReceipt = 27,
		EKKA = 30,
		PrintReceipt = 31,
		PrintX = 32,
		PrintZ = 33,
		InputOutputMoney = 34,
		POS = 40,
		PrintPosX = 42,
		PrintPosZ = 43
			
	}
	public struct ColumInfo
	{
		public int With;
		public int TypeFilter;
		public string Name;
		public string PrintName;
		public string ExInfo;
	}
	/// <summary>
	/// Мови. (Тимчасово! Має ж бути системна така фігня.)
	/// </summary>
	public enum Language {
		def=0,
		ar_XA,	//Арабська
		bg,//Болгарська
		hr,//Хорватська
		cs,//Чеська
		da	,//Данська
		de,//Німецька
		el,//Грецька
		en,//Англійська
		et,//Естонська
		es,//Іспанська
		fi,//Фінська
		fr,//Французька
		ga,//Ірландська
		hi,//Хінді
		hu,//Угорська
		he,//Іврит
		it	,//Італійська
		ja,//Японська
		ko	,//Корейська
		lv,//Латвійська
		lt,//Литовська
		nl,//Нідерландська
		no	,//Норвезька
		pl,//Польська
		pt	,//Портуґальська
		sv,//Шведська
		ro,//Румунська
		ru,//Російська
		sr_CS	,//Сербська
		sk,//Словацька
		sl	,//Словенська
		th,//Тайська
		tr	,//Турецька
		uk_UA,//Українська
		zh_chs,//Китайська (спрощене письмо)
		zh_cht//Китайська (традиційне письмо)
	};
}
