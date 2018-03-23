/*
 * Created by SharpDevelop.
 * User: gelo
 * Date: 09.10.2013
 * Time: 9:45
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
//using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
//using System.Diagnostics;
using DatabaseLib; // тимчасово для ParametersCollection
//using SourceGrid;
//using SourceGrid.Extensions.PingGrids;
//using SourceGrid.PingGrid.Backends.DSet;
//using System.math

namespace MID
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class NET_MainForm : Form
	{
		WDB varWDB;
		
		EKKA varEKKA;
		//NET_DATEX_3530T varEKKA = new NET_DATEX_3530T();
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

		/// <summary>
		/// Тип оплати
		/// </summary>
		TypePay varTypePay = TypePay.Cash;
		TypeBonus varTypeBonus = TypeBonus.NonBonus;
		DataTable varDtWares;
//		DataTable varDtClient;
		DataTable varDtUnit;
		DataTable varDtViewWaresReceipt;
		
		//private DataTable  m_custTable		;
		public NET_MainForm()
		{
			
			varWDB = new NET_WDB_SQLite(GlobalVar.varPathDB + @"MID.db");
			varEKKA = new EKKA(varWDB);
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			Global.InitIni();
			Global.InitConfig(varWDB);
			
			Global.InitKeyMap();
			InitializeComponent();
			SetDoubleBuffered(this.Wares,true);
			
			Login("gelo","1");
			
			NewReceipt();
			ViewWaresReceipt();
			//varBindQuantity
			
		}
		
		void Login(string parLogin,string parPassword)
		{
			DataRow dr=varWDB.Login(parLogin,parPassword);
			varUser.SetUser(dr);
			cashier.Text=varUser.varNameUser;
			Global.Log(CodeLog.Login,varUser.varCodeUser,0,parLogin+ @"\"+cashier.Text);
		}
		
		void Message(string parMassage)
		{
			var result = MessageBox.Show(parMassage, "Попередження",
			                             MessageBoxButtons.OK,
			                             MessageBoxIcon.Error);
		}
		
		void Clear()
		{
			NameWares.Text="";
			Quantity.Value=0;
			Price.Text="0.0";
			if (varDtUnit!= null)
				varDtUnit.Clear();
			NameUnit.Refresh();
			Client.Text="";
			
		}
		
		void ViewWaresReceipt()
		{
			ParametersCollection varParameters = new ParametersCollection();
			varParameters.Add("parIdWorkplace",GlobalVar.varIdWorkPlace ,DbType.Int32);
			varParameters.Add("parCodePeriod", varReceipt.varCodePeriod,DbType.Int32);
			varParameters.Add("parCodeReceipt",varReceipt.varCodeReceipt ,DbType.Int32);
			varDtViewWaresReceipt=varWDB.ViewWaresReceipt(varParameters);
			object sumObject;
			sumObject = varDtViewWaresReceipt.Compute("Sum(sum)", "");
			
			this.varReceipt.varSumReceipt= ( sumObject.ToString() != "" ?   Math.Round( Convert.ToDecimal(sumObject),2):0.0m);
			Sum.Text=this.varReceipt.varSumReceipt.ToString();
			Wares.DataSource=varDtViewWaresReceipt;
			Wares.Refresh();
			
		}
		
		
		void NewReceipt()
		{
			varClient.Clear();
			SetDefaultClient();
			varReceipt.SetReceipt(varWDB.GetCodeReceipt());
			NumberReceipt.Text=varReceipt.varCodeReceipt.ToString();
			Rest.Text="0.00";
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
			Clear();
			ViewWaresReceipt();
			Global.Log(CodeLog.NewReceipt,varReceipt.varCodePeriod,varReceipt.varCodeReceipt,varReceipt.varDateReceipt.ToString());
		}
		
		void SetDefaultClient()
		{

			ParametersCollection varParameters = new ParametersCollection();
			varParameters.Add("parId",GlobalVar.varDefaultCodeClient, DbType.Int32);
			varParameters.Add("parData",-1, DbType.Int32);
			varWDB.InsertT1(varParameters);
			SetClient(varWDB.FindClient().Rows[0]);
			
		} 
		void SetClient(DataRow parRw)
		{
			
			varClient.SetClient(parRw);
			Client.Text=varClient.varNameClient;

			ParametersCollection varParameters = new ParametersCollection();
			varParameters.Add("parIdWorkplace",GlobalVar.varIdWorkPlace ,DbType.Int32);
			varParameters.Add("parCodePeriod", varReceipt.varCodePeriod,DbType.Int32);
			varParameters.Add("parCodeReceipt",varReceipt.varCodeReceipt ,DbType.Int32);
			varParameters.Add("parCodeClient", varClient.varCodeClient,DbType.Int32);
			varWDB.UpdateClient(varParameters);
			Bonus.Text=varClient.varSumMoneyBonus.ToString();
			RecalcPrice();
			Global.Log(CodeLog.SetClient,varClient.varCodeClient,0,this.varClient.varNameClient );
			
		}
		
		void RecalcPrice()
		{
			Global.Log(CodeLog.RecalcPrice);
			
		}
		void AddWaresReceipt()
		{
			ParametersCollection varParameters = new ParametersCollection();
			varParameters.Add("parIdWorkplace",GlobalVar.varIdWorkPlace ,DbType.Int32);
			varParameters.Add("parCodePeriod", varReceipt.varCodePeriod,DbType.Int32);
			varParameters.Add("parCodeReceipt",varReceipt.varCodeReceipt ,DbType.Int32);
			varParameters.Add("parCodeWares",varWares.varCodeWares ,DbType.Int32);
			varParameters.Add("parCodeUnit",varWares.varCodeUnit ,DbType.Int32);
			varParameters.Add("parSort",varReceipt.varSort,DbType.Int32);
			DataTable varDt= varWDB.GetCountWares(varParameters);
			
			if(varDt!=null && varDt.Rows.Count>0 && Convert.ToInt32(varDt.Rows[0][1])!=varReceipt.varSort )
			{
				Quantity.Value+=Convert.ToDecimal(varDt.Rows[0][0]);
				
			}
			QuantityValueChanged(null,null);
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
			if(varDt!=null && varDt.Rows.Count>0 )
				varWDB.UpdateQuantityWares(	varParameters);
			else
				varWDB.AddWares(varParameters);
			
			if (GlobalVar.varRecalcPriceOnLine)
				varWDB.RecalcPrice(varReceipt.varCodeReceipt);
			ViewWaresReceipt();
			
			
			
			// add columns
			
			//WaresReceipt.Refresh();
			//bindingSource1.DataSource=varDtViewWaresReceipt;
			Wares.DataSource=varDtViewWaresReceipt;
			Wares.Refresh();
			Input.Focus();
			//grid1.DataBindings.Add(  bindingSource1);
			
			/* @parIdWorkplace, @parCodePeriod, @parCodeReceipt, @parCodeWares, @parCodeUnit,
  @parTypePrice, @parCodeWarehouse, @parSum, @parSumVat,@parQuantity,
  @parCodeDk, @parCodeDiscount, @parSumDiscount, @parTypeVat, @parSort,
  @parUserCreate*/
			
		}
		
		void InputKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == 13)
				if(Input.Text.Length>0)
					FindData( Input.Text);
			//Debug.WriteLine(Convert.ToInt32( e.KeyChar));
		}
		/// <summary>
		/// Вибирає товар з списту (необхідно доробити)
		/// </summary>
		/// <returns></returns>
		int ChoiceWares(DataTable parDT)
		{
			Global.BildGrid(this.ChoiceFound,parDT);
			

			return 1;
		}
		/// <summary>
		/// Вибирає клієнта з списту (необхідно доробити)
		/// </summary>
		/// <returns></returns>
		
		int ChoiceClient()
		{
			return 1;
		}
		/// <summary>
		/// Власне шукає товар/клієнта.
		/// </summary>
		/// <param name="parStr"></param>
		void FindData(string parStr)
		{
			int varTypeFind = varWDB.FindData(parStr);
			Global.Log(CodeLog.Find ,varTypeFind,0,parStr);
			switch (varTypeFind)
			{
				case 0: //нічого не знайшли
					Message("Нічого не знайшли!!!");
					Clear();
					break;
				case 1: //Товар

					ParametersCollection varParameters = new ParametersCollection();
					varParameters.Add("parDiscount",varClient.varDiscount,DbType.Int32);
					varDtWares = varWDB.FindWares(varParameters);
					int varChoiceRow;
					if(varDtWares.Rows.Count==1)
						varChoiceRow=0;
					else
						varChoiceRow=ChoiceWares(varDtWares);
					if(varChoiceRow>=0)
					{
						varWares.SetWares(varDtWares.Rows[varChoiceRow]);
						NameWares.Text=varWares.varNameWares;
						//double varPriceDealer= Convert.ToDouble( varDtWares.Rows[varChoiceRow]["price_dealer"]);
						if (varWares.varPrice>0)
						{
							if(varClient.varDiscount!=0)
								GetPrice();
							else
								varWares.varTypePrice=1;
							
							varReceipt.varSort++;
							if( varWares.varCodeUnit==0)
							{
								varDtUnit=varWDB.UnitWares( varWares.varCodeWares);
								//DataView dv = new DataView(varDtUnit);
								NameUnit.DataSource = varDtUnit;
								NameUnit.DisplayMember  =  "abr_unit";
								NameUnit.ValueMember =  "code_unit";
								NameUnit.SelectedIndex = Global.GetIndexDefaultUnit(varDtUnit);
								//NameUnit.DataBind();
								
							} else // Одиниця виміру опреділена
							{   if(varDtUnit==null)
									varDtUnit=varWDB.UnitWares(123);
								varDtUnit.Clear();
								varDtUnit.Rows.Add(new Object[] {varWares.varCodeUnit,varWares.varAbrUnit,varWares.varCoefficient ,"Y"});
							}
							
							
							NameUnit.Enabled=(varDtUnit.Rows.Count>1);
							
							NameUnit.Refresh();
							NameUnitSelectedIndexChanged(null,null);
							
							
							
							if(Global.IsUnitMustInputQuantity(varWares.varCodeUnit))
							{
								Quantity.DecimalPlaces=3;
								Quantity.Value = 0;
								Quantity.Enabled=true;
								Quantity.Focus();
							}
							else
							{
								Quantity.DecimalPlaces=1;
								Quantity.Value = 1;
								Quantity.Enabled=false;//необхідно обробляти налаштування.
								this.AddWaresReceipt();
							}
							
							//this.AddWares();

						} else //Незадано продажної ціни
						{
							Message("Не задана продажна ціна"); //
							Clear();
						}
						
					};
					
					break;
				case 2: //Клієнт
					SetClient( varWDB.FindClient().Rows[0]);
					break;
			}
			Input.Text="";
		}
		
		void ClearInfoWares()
		{
			NameWares.Text="";
			varWares.Clear();
			Quantity.Value=0;
			Quantity.Enabled=false;
			NameUnit.Enabled=false;
			Price.Text="0";
		}
		

		//ClearInfoWares = Escape
		//AddWaresReceipt = Enter

		void PrintReceipt(TypePay parTypePay)
		{
			
			Clear();
			SwitchMainPrint(false);
			SetViewPay(parTypePay);
			
		}
		
		void SwitchMainPrint(bool varMain)
		{
			this.Input.Enabled = varMain;
			this.Wares.Enabled = varMain;
			PanelPrintReceipt.Visible= !varMain;
			if(varMain)
				this.Input.Focus();
		}
		
		
		void IncrementWares()
		{
			
			Quantity.Value++;
			QuantityValueChanged(null, null);
			this.AddWaresReceipt();
			Global.Log(CodeLog.IncrementWares);
		}
		void DecrementWares()
		{
			if(Quantity.Value>0)
				Quantity.Value--;
			QuantityValueChanged(null, null);
			this.AddWaresReceipt();
			Global.Log(CodeLog.DecrementWares);
		}

		/// <summary>
		/// Видаляємо товар з чека
		/// </summary>
		/// <param name="varTypeDelete"></param>
		void DeleteWaresReceipt(TypeDeleteWaresReceipt varTypeDelete)
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
				case TypeDeleteWaresReceipt.Choice:
					varParameters.Add("parCodeWares", varWares.varCodeWares,DbType.Int32);
					varParameters.Add("parCodeUnit", varWares.varCodeUnit,DbType.Int32);
				break;
			}
			varWDB.DeleteWaresReceipt(varParameters);
			ViewWaresReceipt();
			Global.Log(CodeLog.DeleteWaresReceipt,(int)varTypeDelete);
		}
		/// <summary>
		/// Редагуємо кількість в довільному рядку (не зроблено)
		/// </summary>
		void EditQuantityWares()
		{
			
			Global.Log(CodeLog.EditQuantityWares);
		}
		/// <summary>
		/// Змінюємо поточний чек поколу
		/// </summary>
		void ChangeCurrentReceipt()
		{
			
		}
		
		void Help()
		{
			
		}
		void PrintZ()
		{
			Global.Log(CodeLog.PrintZ);
			varEKKA.PrintZ();
		}
		void PrintX()
		{
			varEKKA.PrintX();
			Global.Log(CodeLog.PrintX);
		}
		
		void InputOutputMoney()
		{
			Global.Log(CodeLog.InputOutputMoney);
			varWDB.InputOutputMoney(0);
		}
		void CountCash()
		{
			
		}
		
		void TimerTick(object sender, EventArgs e)
		{
			Time.Text=DateTime.Now.ToString("HH:mm:ss");
		}
		
		

		void NameUnitSelectedIndexChanged(object sender, EventArgs e)
		{
			varWares.varCodeUnit=Convert.ToInt32(varDtUnit.Rows[NameUnit.SelectedIndex]["code_unit"]);
			varWares.varCoefficient = Convert.ToDecimal(varDtUnit.Rows[NameUnit.SelectedIndex]["coefficient"]);
			Price.Text = string.Format("{0:0000.00}", varWares.varPrice* (1+varWares.varPercentVat)* varWares.varCoefficient);
		}
		
		void QuantityValueChanged(object sender, EventArgs e)
		{
			this.varWares.varQuantity=this.Quantity.Value;
			this.varWares.varIsSave=false;
		}

		void InputKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			KeyProcedureMainInput(e.KeyData);
		}

		void QuantityKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			KeyProcedureMainInput(e.KeyData,1);
		}

		
		bool KeyProcedureMainInput(Keys parKeys,int parOption =0)
		{
			int varKeys = (int) parKeys;
			if( MainInputKeyMap.ClearInfoWares==varKeys) ClearInfoWares();
			else if(MainInputKeyMap.IncrementWares==varKeys && parOption!=1) IncrementWares();
			else if(MainInputKeyMap.DecrementWares==varKeys && parOption!=1) DecrementWares();
			else if(MainInputKeyMap.PrintReceipt_Cash==varKeys) PrintReceipt(TypePay.Cash);
			else if(MainInputKeyMap.PrintReceipt_Pos==varKeys) PrintReceipt(TypePay.Pos );
			else if(MainInputKeyMap.PrintReceipt_NonCash==varKeys) PrintReceipt(TypePay.NonCash );
			else if(MainInputKeyMap.AddWaresReceipt==varKeys && parOption==1) AddWaresReceipt();
			else if(MainInputKeyMap.DeleteWaresReceipt_Current==varKeys) DeleteWaresReceipt(TypeDeleteWaresReceipt.Current );
			else if(MainInputKeyMap.DeleteWaresReceipt_All==varKeys) DeleteWaresReceipt(TypeDeleteWaresReceipt.All );
			else if(MainInputKeyMap.DeleteWaresReceipt_Choice==varKeys) DeleteWaresReceipt(TypeDeleteWaresReceipt.Choice );
			else if(MainInputKeyMap.EditQuantityWares==varKeys) EditQuantityWares();
			else if(MainInputKeyMap.ChangeCurrentReceipt==varKeys) ChangeCurrentReceipt();
			else if(MainInputKeyMap.NewReceipt==varKeys) NewReceipt();
			else if(MainInputKeyMap.RecalcPrice==varKeys) RecalcPrice();
			else if(MainInputKeyMap.Help==varKeys) Help();
			else if(MainInputKeyMap.PrintZ==varKeys) PrintZ();
			else if(MainInputKeyMap.PrintX==varKeys) PrintX();
			else if(MainInputKeyMap.CountCash==varKeys) CountCash();
			else if(MainInputKeyMap.UpdateData==varKeys) UpdateData();
			else if(MainInputKeyMap.PrintPosX == varKeys) PrintX();
			else if(MainInputKeyMap.PrintPosZ == varKeys) PrintZ();
			else if(MainInputKeyMap.InputOutputMoney == varKeys) InputOutputMoney();
			else return false;
			return true;
		}

		void PrintInputKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			KeyProcedurePrint(e.KeyData);
		}
		
		
		bool KeyProcedurePrint(Keys parKeys)
		{
			int varKeys = (int) parKeys;
			//if(PayKeyMap.PrintReceipt==varKeys) {if(PayKeyMap.PrintReceipt==this.varTypePay) PrintReceipt() else SetViewPay( this.varTypePay,TypeBonus.Bonus)};
			//else
			if(MainInputKeyMap.PrintReceipt_Cash==varKeys)  {if(TypePay.Cash == this.varTypePay) PrintReceipt(); else SetViewPay( TypePay.Cash, this.varTypeBonus);}
			else if(MainInputKeyMap.PrintReceipt_Pos==varKeys) {if(TypePay.Pos==this.varTypePay) PrintReceipt(); else SetViewPay( TypePay.Pos,this.varTypeBonus);}
			else if(MainInputKeyMap.PrintReceipt_Pos==varKeys) {if(TypePay.Pos==this.varTypePay) PrintReceipt(); else SetViewPay( TypePay.Pos,this.varTypeBonus);}
			else if(PayKeyMap.Exit==varKeys) ExitPay();
			else if(PayKeyMap.UseBonus==varKeys) SetViewPay( this.varTypePay,TypeBonus.Bonus);
			else if(PayKeyMap.UseBonusWithOutRest==varKeys) SetViewPay( this.varTypePay,TypeBonus.BonusWithOutRest);
			else if(PayKeyMap.UseBonusToRest==varKeys) SetViewPay(this.varTypePay,TypeBonus.BonusToRest );
			else if(PayKeyMap.UseBonusFromRest==varKeys) SetViewPay(this.varTypePay,TypeBonus.BonusFromRest );
			else if(MainInputKeyMap.PrintReceipt_Partially ==varKeys) SetViewPay(TypePay.Partiall,this.varTypeBonus);
			else return false;
			return true;
		}
		
		void ExitPay()
		{
			SwitchMainPrint(true);
		}
		
		void PrintReceipt()
		{
			//Друк чека!!!
			//varEKKA.p
			//Збереження роздукованого чека
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
			Global.Log(CodeLog.CloseReceipt);
			NewReceipt(); //Відкримаємо новий чек.
		}
		
		
		/// <summary>
		/// Будує вікно оплати в залежності від вибраного методу.
		/// </summary>
		/// <param name="varTypePay"></param>
		/// <param name="varTypeBonus"></param>
		void SetViewPay(TypePay parTypePay =  TypePay.Cash,TypeBonus parTypeBonus = TypeBonus.NonBonus)
		{
		
			System.Drawing.Font varFontBold   = new Font(PrintHotKeyPos.Font, FontStyle.Bold);
			System.Drawing.Font varFontNormal = new Font(PrintHotKeyPos.Font, FontStyle.Regular);
			
			
			if((parTypePay == TypePay.Partiall || parTypePay == TypePay.Pos)  && GlobalVar.varModelPos[0]==-1 )
				parTypePay=TypePay.Cash;
			
			// засткова оплата бонусами і здача тільки в готівковому режимі.
			if( parTypePay != TypePay.Cash && (parTypeBonus != TypeBonus.NonBonus || parTypeBonus != TypeBonus.Bonus ))
			   parTypeBonus = TypeBonus.NonBonus;
			
			this.varTypePay=parTypePay;
			this.varTypeBonus=parTypeBonus;
			
			
			this.PrintHotKeyPos.Visible = (GlobalVar.varModelPos[0]>=0);
			this.PrintHotKeyNonCash.Visible = true;
			this.PrintHotKeyPartiall.Visible = false; // Доробити по прававам чи налаштуванням системи.

			this.PrintHotKeyBonus.Visible = (varClient.varSumMoneyBonus>0);
			this.PrintHotKeyBonusWithOutRest.Visible = (varClient.varSumMoneyBonus>0) && (varTypePay== TypePay.Cash);
			this.PrintHotKeyBonusFromRest.Visible = varClient.varIsUseBonusFromRest && (varTypePay== TypePay.Cash);
			this.PrintHotKeyBonusToRest.Visible =   varClient.varIsUseBonusToRest && (varTypePay== TypePay.Cash) && (varClient.varSumMoneyBonus -Math.Truncate(varClient.varSumMoneyBonus)  <=  varReceipt.varSumReceipt )  ;
			

			// Виділяємо текучий Режим.
			this.PrintHotKeyCash.Font     =  (this.varTypePay == TypePay.Cash)      ? varFontBold:varFontNormal ;
			this.PrintHotKeyPos.Font      =  (this.varTypePay == TypePay.Pos)       ? varFontBold:varFontNormal ;
			this.PrintHotKeyNonCash.Font  =  (this.varTypePay == TypePay.NonCash)   ? varFontBold:varFontNormal ;
			this.PrintHotKeyPartiall.Font =  (this.varTypePay == TypePay.Partiall)  ? varFontBold:varFontNormal ;


			// Виділяємо текучий режим списання бонусів.
			this.PrintHotKeyBonus.Font     			=  (this.varTypeBonus == TypeBonus.Bonus )      	? varFontBold:varFontNormal ;
			this.PrintHotKeyBonusWithOutRest.Font   =  (this.varTypeBonus == TypeBonus.BonusWithOutRest )  ? varFontBold:varFontNormal ;
			this.PrintHotKeyBonusToRest.Font     	=  (this.varTypeBonus == TypeBonus.BonusToRest )    ? varFontBold:varFontNormal ;
			this.PrintHotKeyBonusFromRest.Font     	=  (this.varTypeBonus == TypeBonus.BonusFromRest )  ? varFontBold:varFontNormal ;
			switch (this.varTypeBonus)
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
			
			
			this.LabelBonus.Text= this.varTypeBonus == TypeBonus.BonusFromRest ? "На бонусний" : "Вик. Бонусів";
			this.PrintPaySum.Value = varReceipt.varSumReceipt-varReceipt.varSumBonus;
			this.PrintUsedBonus.Value= varReceipt.varSumBonus;
		
			this.PrintUsedBonus.Enabled = (this.varTypeBonus ==TypeBonus.Bonus);
				
			this.PrintHotKeyPos.Visible = (GlobalVar.varModelPos[0]>=0);
			this.PrintHotKeyQuickPOS.Visible = (GlobalVar.varModelPos[1]>=0); //Якщо більше 1пос термінала
			
			this.LabelPos.Visible   = this.PrintPos.Visible   = (GlobalVar.varModelPos[0]>=0);
			this.LabelSlip.Visible  = this.PrintSlip.Visible  = (GlobalVar.varModelPos[0]>=0);
			
			this.LabelCash.Enabled  = this.PrintCash.Enabled  = (parTypePay==TypePay.Partiall || parTypePay==TypePay.Cash);
			this.LabelPos.Enabled   = this.PrintPos.Enabled   = (parTypePay==TypePay.Partiall );
			this.LabelSlip.Enabled  = this.PrintSlip.Enabled  = (parTypePay==TypePay.Partiall || parTypePay==TypePay.Pos) && GlobalVar.varModelPos[0]>=0 && !GlobalVar.varIsPosConnect ;
			//this.LabelBonus.Enabled = this.PrintToBonus.Enabled = this.PrintUsedBonus.Enabled = (parTypeBonus==TypeBonus.NonBonus && varClient.varSumMoneyBonus>0) ;
			
			this.LabelRest.Enabled  = this.PrintRest.Enabled  = false;
			this.LabelRest.Visible  = this.PrintRest.Visible  = (parTypePay==TypePay.Partiall || parTypePay==TypePay.Cash);

//			RecalcPrintSum();
			if(parTypePay==TypePay.Cash || parTypePay==TypePay.Partiall)
				this.PrintCash.Focus();
			else if(parTypePay==TypePay.Pos)
				if(GlobalVar.varModelPos[0]>=0 && GlobalVar.varIsPosConnect) this.ButtonPrintReceipt.Focus(); else this.PrintSlip.Focus();
			
		}
		
		
		void UpdateData()
		{
			// Працювати і працювати.
			varWDB.LoadDataFromFile(@"D:\TXT\PRICE_DEALER.csv");
		}
		
		void SetDoubleBuffered ( Control c, bool value ) {
			PropertyInfo pi = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic);
			if (pi != null) {
				pi.SetValue(c, value, null);
				
				MethodInfo mi = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic);
				if (mi != null) {
					mi.Invoke(c, new object[] { ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true });
				}
				
				mi = typeof(Control).GetMethod("UpdateStyles", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic);
				if (mi != null) {
					mi.Invoke(c, null);
				}
			}
		}
		
		
		void ButtonPrintReceiptClick(object sender, System.EventArgs e)
		{
			PrintReceipt();
			SwitchMainPrint(true);
		}
		
		
		void GetPrice()
		{
			ParametersCollection varParameters =new ParametersCollection();
			varParameters.Add("parCodeDealer",varClient.varCodeDealer , DbType.Int32);
			varParameters.Add("parCodeWares",varWares.varCodeWares , DbType.Int32);
			varParameters.Add("parDiscount",varClient.varDiscount , DbType.Decimal);
			DataRow varDR=varWDB.GetPrice(varParameters);
			varWares.varPrice= Convert.ToDecimal(varDR[0]);
			varWares.varTypePrice=Convert.ToInt32(varDR[1]);
		}
		
		
		void PrintCashValueChanged(object sender, EventArgs e)
		{
			varReceipt.varSumRest = this.PrintCash.Value - varReceipt.varSumReceipt + this.PrintUsedBonus.Value;
			this.PrintRest.Value = varReceipt.varSumRest; 
		}
		void InputTextChanged(object sender, EventArgs e)
		{
	
		}
		void OkClick(object sender, EventArgs e)
		{
	
		}
		
		
	}
}

