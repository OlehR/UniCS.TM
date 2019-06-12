using System;
using System.Data;
using System.IO;
//using DatabaseLib;
//using System.Windows.Forms;

namespace MID
{
	/// <summary>
	/// Description of Work.
	/// </summary>
	public class Work
	{

		//public Binding  ReceiptBSum ;
		
		public WDB varWDB;
		public EKKA varEKKA;
		public LogSQL varLogSQL;
		public POS varPOS = new POS();
		/// <summary>
		/// Клас з інформацією про клієнта
		/// </summary>
		public User varUser = new User();
		/// <summary>
		/// Клас з інформацією про вибраного клієнта
		/// </summary>
		public Client varClient = new Client();
		/// <summary>
		/// Клас з інформацією про товар
		/// </summary>
		public Wares varWares = new Wares();
		/// <summary>
		/// Клас з інформацією про текучий чек.
		/// </summary>
		public Receipt varReceipt = new Receipt();
		
		
		
		public Work()
		{
			Global.InitIni(); // Ініціалізуємо середовище(шляхи, id-робочого місця);
			varLogSQL = new LogSQL();
			varWDB = new WDB_SQLite(varLogSQL.WriteSqlLog );
			Global.InitWDB(varWDB);
			varEKKA = new EKKA(varWDB);
		}
		
		public bool Login(string parLogin,string parPassword)
		{
			
			DataRow dr=varWDB.Login(parLogin,parPassword);
			varUser.SetUser(dr);
			Global.Log(CodeEvent.Login,varUser.varCodeUser,0,parLogin+ @"\"+varUser.varNameUser);
			if(varUser.varCodeUser>0)
			{
				Global.InitPermissions(varUser.varCodeUser);
				return true;
			}
			else
				return false;
			
			
		}
		
		public DataTable ViewWaresReceipt()
		{
			ParametersCollection varParameters = new ParametersCollection();
			varParameters.Add("parIdWorkplace",GlobalVar.varIdWorkPlace ,DbType.Int32);
			varParameters.Add("parCodePeriod", varReceipt.varCodePeriod,DbType.Int32);
			varParameters.Add("parCodeReceipt",varReceipt.varCodeReceipt ,DbType.Int32);
			DataTable varDtViewWaresReceipt=varWDB.ViewWaresReceipt(varParameters);
			object sumObject;
			sumObject = varDtViewWaresReceipt.Compute("Sum(sum)", "");
			this.varReceipt.varSumReceipt= ( sumObject.ToString() != "" ?   Math.Round( Convert.ToDecimal(sumObject),2):0.0m);
			//this.varReceipt.varStSumReceipt = this.varReceipt.varSumReceipt.ToString();
			
			return varDtViewWaresReceipt;
			
		}

		public void NewReceipt()
		{
			varClient.Clear();
			SetDefaultClient();
			varReceipt.SetReceipt(varWDB.GetCodeReceipt());
			
			
			ParametersCollection varParameters = new ParametersCollection();
			varParameters.Add("parIdWorkplace",GlobalVar.varIdWorkPlace ,DbType.Int32);
			varParameters.Add("parCodePeriod", varReceipt.varCodePeriod,DbType.Int32);
			varParameters.Add("parCodeReceipt",varReceipt.varCodeReceipt ,DbType.Int32);
			varParameters.Add("parDateReceipt",varReceipt.varDateReceipt,DbType.DateTime);
			varParameters.Add("parCodeWarehouse",GlobalVar.varCodeWarehouse ,DbType.Int32);
			varParameters.Add("parCodePattern",varReceipt.varCodePattern,DbType.Int32);
			varParameters.Add("parCodeClient", varClient.varCodeClient,DbType.Int32);
			varParameters.Add("parNumberCashier",varUser.varCodeUser,DbType.Int32);
			varParameters.Add("parUserCreate",varUser.varCodeUser,DbType.Int32);
			
			varWDB.AddReceipt(varParameters);
			
			Global.Log(CodeEvent.NewReceipt,varReceipt.varCodePeriod,varReceipt.varCodeReceipt,varReceipt.varDateReceipt.ToString());
		}
		
		public void SetDefaultClient()
		{
			ParametersCollection varParameters = new ParametersCollection();
			varParameters.Add("parId",GlobalVar.varDefaultCodeClient, DbType.Int32);
			varParameters.Add("parData",-1, DbType.Int32);
			varWDB.InsertT1(varParameters);
			SetClient(varWDB.FindClient().Rows[0]);
			
		}

		public void SetClient(DataRow parRw)
		{
			
			varClient.SetClient(parRw);
			
			ParametersCollection varParameters = new ParametersCollection();
			varParameters.Add("parIdWorkplace",GlobalVar.varIdWorkPlace ,DbType.Int32);
			varParameters.Add("parCodePeriod", varReceipt.varCodePeriod,DbType.Int32);
			varParameters.Add("parCodeReceipt",varReceipt.varCodeReceipt ,DbType.Int32);
			varParameters.Add("parCodeClient", varClient.varCodeClient,DbType.Int32);
			varWDB.UpdateClient(varParameters);
			
			RecalcPrice();
			Global.Log(CodeEvent.SetClient,varClient.varCodeClient,0,this.varClient.varNameClient );
			
		}
		
		public void RecalcPrice()
		{
			Global.Log(CodeEvent.RecalcPrice);
			
		}

		public decimal AddWaresReceipt(decimal parQuantity )
		{
			decimal varQuantity;
			ParametersCollection varParameters = new ParametersCollection();
			varParameters.Add("parIdWorkplace",GlobalVar.varIdWorkPlace ,DbType.Int32);
			varParameters.Add("parCodePeriod", varReceipt.varCodePeriod,DbType.Int32);
			varParameters.Add("parCodeReceipt",varReceipt.varCodeReceipt ,DbType.Int32);
			varParameters.Add("parCodeWares",varWares.varCodeWares ,DbType.Int32);
			varParameters.Add("parCodeUnit",varWares.varCodeUnit ,DbType.Int32);
			varParameters.Add("parSort",varReceipt.varSort,DbType.Int32);
			varQuantity= varWDB.GetCountWares(varParameters);
			
			varWares.varQuantity=varQuantity+parQuantity;
			varParameters.Add("parTypePrice",0 ,DbType.Int32);
			varParameters.Add("parCodeWarehouse",GlobalVar.varCodeWarehouse ,DbType.Int32);
			varParameters.Add("parSum",(varWares.varPrice*varWares.varQuantity)*(1+varWares.varPercentVat)* varWares.varCoefficient ,DbType.Decimal);
			varParameters.Add("parSumVat",varWares.varPrice*varWares.varQuantity*varWares.varPercentVat* varWares.varCoefficient,DbType.Decimal);
			varParameters.Add("parQuantity", varWares.varQuantity,DbType.Decimal);
			varParameters.Add("parPrice", (varWares.varPrice)*(1+varWares.varPercentVat)* varWares.varCoefficient,DbType.Decimal);
			varParameters.Add("parParPrice1",varWares.varCodeDealer,DbType.Int32);
			varParameters.Add("parParPrice2", varWares.varTypePrice,DbType.Int32);
			varParameters.Add("parSumDiscount",varWares.varSumDiscount ,DbType.Decimal);
			varParameters.Add("parTypeVat",varWares.varTypeVat,DbType.Int32);
			varParameters.Add("parUserCreate",varUser.varCodeUser,DbType.Int32);
			if(varQuantity>0)
				varWDB.UpdateQuantityWares(	varParameters);
			else
				varWDB.AddWares(varParameters);
			
			if (GlobalVar.varRecalcPriceOnLine)
				varWDB.RecalcPrice(varReceipt.varCodeReceipt);
			
			return varWares.varQuantity;
		}
		
		
		
		public void PrintZ()
		{
			Global.Log(CodeEvent.PrintZ);
			varEKKA.PrintZ();
		}
		public void PrintX()
		{
			varEKKA.PrintX();
			Global.Log(CodeEvent.PrintX);
		}
		
		public void PrintPosZ()
		{
			Global.Log(CodeEvent.PrintBankPosZ);
			varPOS.PrintZ();
		}
		public void PrintPosX()
		{
			varPOS.PrintX();
			Global.Log(CodeEvent.PrintBankPosX);
		}

		
		
		public void InputOutputMoney(decimal varMoney)
		{
			Global.Log(CodeEvent.InputOutputMoney,0,0,varMoney.ToString());
			varWDB.InputOutputMoney(varMoney);
		}
		public void WriteReceipt()
		{
			ParametersCollection varParameters = new ParametersCollection();
			varParameters.Add("parIdWorkplace",GlobalVar.varIdWorkPlace ,DbType.Int32);
			varParameters.Add("parCodePeriod", varReceipt.varCodePeriod,DbType.Int32);
			varParameters.Add("parCodeReceipt",varReceipt.varCodeReceipt ,DbType.Int32);
			
			varParameters.Add("parStateReceipt", 1 ,DbType.Int32);
			varParameters.Add("parSumReceipt",varReceipt.varSumReceipt, DbType.Decimal);
			varParameters.Add("parVatReceipt",varReceipt.varVatReceipt ,DbType.Decimal);
			
			varParameters.Add("parSumCash",varReceipt.varSumCash ,DbType.Decimal);
			varParameters.Add("parSumCreditCard",varReceipt.varSumCreditCard ,DbType.Decimal);
			varParameters.Add("parCodeCreditCard",varReceipt.varCodeCreditCard ,DbType.Int64);
			varParameters.Add("parNumberSlip",varReceipt.varNumberSlip ,DbType.Int64);
			varWDB.CloseReceipt(varParameters);
			Global.Log(CodeEvent.CloseReceipt);
		}
		public void UpdateData()
		{
			// Працювати і працювати.
			varWDB.LoadDataFromFile(@"D:\TXT\PRICE_DEALER.csv");
		}
		
		public void GetPrice()
		{
			ParametersCollection varParameters =new ParametersCollection();
			varParameters.Add("parCodeDealer",varClient.varCodeDealer , DbType.Int32);
			varParameters.Add("parCodeWares",varWares.varCodeWares , DbType.Int32);
			varParameters.Add("parDiscount",varClient.varDiscount , DbType.Decimal);
			DataRow varDR=varWDB.GetPrice(varParameters);
			varWares.varPrice= Convert.ToDecimal(varDR[0]);
			varWares.varTypePrice=Convert.ToInt32(varDR[1]);
		}
		
		/// <summary>
		/// Видаляємо товар з чека
		/// </summary>
		/// <param name="varTypeDelete"></param>
		public void DeleteWaresReceipt(TypeDeleteWaresReceipt varTypeDelete,int parCodeWares=0,int parCodeUnit=0)
		{
			ParametersCollection varParameters = new ParametersCollection();
			varParameters.Add("parIdWorkplace",GlobalVar.varIdWorkPlace ,DbType.Int32);
			varParameters.Add("parCodePeriod", varReceipt.varCodePeriod,DbType.Int32);
			varParameters.Add("parCodeReceipt",varReceipt.varCodeReceipt ,DbType.Int32);
			
			switch ( varTypeDelete)
			{
				case TypeDeleteWaresReceipt.All:
					varParameters.Add("parCodeWares", 0,DbType.Int32);
					varParameters.Add("parCodeUnit", 0,DbType.Int32);
					break;
				case TypeDeleteWaresReceipt.Current:
					varParameters.Add("parCodeWares", varWares.varCodeWares,DbType.Int32);
					varParameters.Add("parCodeUnit", varWares.varCodeUnit,DbType.Int32);
					break;
				case TypeDeleteWaresReceipt.Choice:
					varParameters.Add("parCodeWares", parCodeWares,DbType.Int32);
					varParameters.Add("parCodeUnit", parCodeUnit,DbType.Int32);
					break;
			}
			varWDB.DeleteWaresReceipt(varParameters);
			//ViewWaresReceipt();
			Global.Log(CodeEvent.DeleteWaresReceipt,(int)varTypeDelete);
		}

		public void ChangeDataTypeBonys(TypeBonus parTypeBonus)
		{
			switch (parTypeBonus)
			{
				case TypeBonus.NonBonus:
					varReceipt.varSumBonus = 0;
					break;
				case TypeBonus.Bonus: //Треба врахуватикількість позицій по чеку.
					varReceipt.varSumBonus = (varReceipt.varSumReceipt<=varClient.varSumMoneyBonus ?varReceipt.varSumReceipt : varClient.varSumMoneyBonus);
					break;
				case TypeBonus.BonusWithOutRest: //Треба врахуватикількість позицій по чеку.
					if(varReceipt.varSumReceipt<=varClient.varSumMoneyBonus )
					varReceipt.varSumBonus = varReceipt.varSumReceipt -1; else
						varReceipt.varSumBonus = Math.Truncate(varClient.varSumMoneyBonus) + varReceipt.varSumReceipt-Math.Truncate(varReceipt.varSumReceipt)+
							((varReceipt.varSumReceipt-Math.Truncate(varReceipt.varSumReceipt)>= varClient.varSumMoneyBonus -Math.Truncate(varClient.varSumMoneyBonus) ) ? -1.0m:0.0m) ;
					break;
				case TypeBonus.BonusFromRest:
					varReceipt.varSumBonus = -(1 - ( varReceipt.varSumReceipt-Math.Truncate(varReceipt.varSumReceipt)));
					break;
				case TypeBonus.BonusToRest:
					varReceipt.varSumBonus =  varReceipt.varSumReceipt-Math.Truncate(varReceipt.varSumReceipt);
					break;
			}
		}
		
		public bool ReturnLastReceipt(int parIdWorkPlace, int parCodePeriod,int parCodeReceipt)
		{
			ParametersCollection varParameters = new ParametersCollection();
			varParameters.Add("parIdWorkplace", parIdWorkPlace ,DbType.Int32);
			varParameters.Add("parCodePeriod", parCodePeriod,DbType.Int32);
			varParameters.Add("parCodeReceipt", parCodeReceipt ,DbType.Int32);
			varParameters.Add("parIdWorkplaceReturn", GlobalVar.varIdWorkPlace ,DbType.Int32);
			varParameters.Add("parCodePeriodReturn", varReceipt.varCodePeriod,DbType.Int32);
			varParameters.Add("parCodeReceiptReturn", varReceipt.varCodeReceipt ,DbType.Int32);
			varParameters.Add("parUserCreate",varUser.varCodeUser,DbType.Int32);
			
			if(GlobalVar.varIdWorkPlace==parIdWorkPlace)
			{
				
				if(parCodePeriod!=varReceipt.varCodePeriod)
				{
					string varReceiptFile=GlobalVar.varPathDB +parCodePeriod.ToString().Substring(0,6) +@"\Rc_"+  parIdWorkPlace.ToString()+"_"+parCodePeriod.ToString()+".db";
					if(File.Exists( varReceiptFile))
					{
						varWDB.ExecuteNonQuery("ATTACH '"+varReceiptFile+"' AS rrc");
						bool varRez=varWDB.CopyWaresReturnReceipt(varParameters);
						varWDB.ExecuteNonQuery("DETACH DATABASE rrc");
						return varRez;
					} else return false;
				}
				else
				{
					return varWDB.CopyWaresReturnReceipt(varParameters);
				}

			}else // Якщо інше робоче місце
			{
				//return false;
			}
			return false;
		}
		
		
	}
}
