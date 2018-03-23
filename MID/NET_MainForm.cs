/*
 * Created by SharpDevelop.
 * User: gelo
 * Date: 09.10.2013
 * Time: 9:45
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

namespace MID
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class NET_MainForm : Form
	{
		protected bool varWait=false;
		
		Work varWork;
		ModeInterface varModeInterface = ModeInterface.EditWares;

		/// <summary>
		/// Тип оплати
		/// </summary>
		TypePay varTypePay = TypePay.Cash;
		TypeBonus varTypeBonus = TypeBonus.NonBonus;
		DataTable varDtWares;
//		DataTable varDtClient;
		DataTable varDtUnit;
		//DataTable varDtViewWaresReceipt;
		
		//private DataTable  m_custTable		;
		public NET_MainForm()
		{
			
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
//			Sum.DataBindings.Add("Text",varWork.ReceiptBSum,"varStSumReceipt");
			InitSize();
			SetDoubleBuffered(this.Wares,true);
			SetDoubleBuffered(this.ChoiceFound,true);
			
			ToCenter((Control)this.PanelInputOutputMoney,(Control)this.Wares);
			ToCenter((Control)this.PanelPrintReceipt,(Control)this.Wares);
			ToCenter((Control)this.PanelLogin ,(Control)this.Wares);

			//tmp 
			PanelLoginTextBoxLogin.Text="gelo";
			PanelLoginTextBoxPassWord.Text="nataly";
			varWork = new Work(); // Ініціалізуються основні дані
			SetView(ModeInterface.Login);
			
			
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
			DataTable varDtViewWaresReceipt = varWork.ViewWaresReceipt();
			
			Sum.Text=varWork.varReceipt.varSumReceipt.ToString();
			
			Wares.DataSource=varDtViewWaresReceipt;
			Wares.Refresh();
		}
		
		
		void NewReceipt()
		{
			Rest.Text="0.00";
			varWork.NewReceipt();
			Clear();
			ViewWaresReceipt();
			NumberReceipt.Text= varWork.varReceipt.varCodeReceipt.ToString();
		}
		
		void AddWaresReceipt()
		{
			Quantity.Value = varWork.AddWaresReceipt(Quantity.Value);
			QuantityValueChanged(null,null);  //Можливо і не потрібно.
			ViewWaresReceipt();
			Input.Focus();
		}
		
		/// <summary>
		/// Подія для провірки завершення вводу
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == 13)
				if(Input.Text.Length>0)
					FindData( Input.Text);
		}


		/// <summary>
		/// Вибирає товар з списту (необхідно доробити)
		/// </summary>
		/// <returns></returns>
		void ChoiceWares(DataTable parDT)
		{
			varModeInterface = ModeInterface.ChoiceWares;
			Global.BildGrid(this.ChoiceFound,parDT,"CODE_WARES,NAME_WARES,ABR_UNIT_DEFAULT,PRICE_DEALER,TYPE_PRICE");
			this.ChoiceFound.Visible=this.ChoiceFound.Enabled=true;
			SetView();
		}
		/// <summary>
		/// Вибирає клієнта з списту (необхідно доробити)
		/// </summary>
		/// <returns></returns>
		
		void ChoiceClient()
		{

		}

		/// <summary>
		/// Власне шукає товар/клієнта.
		/// </summary>
		/// <param name="parStr"></param>
		void FindData(string parStr)
		{
			RezultFind varRezult = varWork.varWDB.FindData(parStr);
			Global.Log(CodeEvent.Find ,(int)varRezult.TypeFind,varRezult.Count,parStr);
			
			if(varRezult.Count==0)
			{
				Message("Нічого не знайшли!!!");
				return;
			}
			
			
			switch (varRezult.TypeFind)
			{
				case  TypeFind.Wares: //Товар
					ParametersCollection varParameters = new ParametersCollection();
					varParameters.Add("parDiscount",varWork.varClient.varDiscount,DbType.Int32);
					varDtWares = varWork.varWDB.FindWares(varParameters);

					if(varDtWares.Rows.Count==1)
					{
						varWork.varWares.SetWares(varDtWares.Rows[0]);
						EditQuantity();
					}
					else
						ChoiceWares(varDtWares);
					break;
				case TypeFind.Client: //Клієнт
					if(varRezult.Count>1)
						ChoiceClient();
					else
						SetClient( varWork.varWDB.FindClient().Rows[0]);
					break;
			}
			Input.Text="";
		}

		void EditQuantity()
		{
			NameWares.Text=varWork.varWares.varNameWares;
			//double varPriceDealer= Convert.ToDouble( varDtWares.Rows[varChoiceRow]["price_dealer"]);
			if (varWork.varWares.varPrice>0)
			{
				if(varWork.varClient.varDiscount!=0)
					varWork.GetPrice();
				else
					varWork.varWares.varTypePrice=1;
				
				varWork.varReceipt.varSort++;
				if( varWork.varWares.varCodeUnit==0)
				{
					varDtUnit=varWork.varWDB.UnitWares( varWork.varWares.varCodeWares);
					//DataView dv = new DataView(varDtUnit);
					NameUnit.DataSource = varDtUnit;
					NameUnit.DisplayMember  =  "abr_unit";
					NameUnit.ValueMember =  "code_unit";
					NameUnit.SelectedIndex = Global.GetIndexDefaultUnit(varDtUnit);
					//NameUnit.DataBind();
					
				} else // Одиниця виміру опреділена
				{   if(varDtUnit==null)
						varDtUnit=varWork.varWDB.UnitWares(123);
					varDtUnit.Clear();
					varDtUnit.Rows.Add(new Object[] {varWork.varWares.varCodeUnit,varWork.varWares.varAbrUnit,varWork.varWares.varCoefficient ,"Y"});
				}
				
				
				NameUnit.Enabled=(varDtUnit.Rows.Count>1);
				
				NameUnit.Refresh();
				NameUnitSelectedIndexChanged(null,null);
				
				
				
				if(Global.IsUnitMustInputQuantity(varWork.varWares.varCodeUnit))
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
			
		}
		

		
		void ClearInfoWares()
		{
			NameWares.Text="";
			varWork.varWares.Clear();
			Quantity.Value=0;
			Quantity.Enabled=false;
			NameUnit.Enabled=false;
			Price.Text="0";
		}
		

		//ClearInfoWares = Escape
		//AddWaresReceipt = Enter

		void PrintReceipt(TypePay parTypePay)
		{
			switch(Global.GetTypeAccess(CodeEvent.PrintReceipt))
			{
				case TypeAccess.NoErrorAccess:
					ErrorAccess(CodeEvent.PrintReceipt);
					break;
				case TypeAccess.Question:
					
				case TypeAccess.Yes :
					Clear();
					SetView(ModeInterface.PrintReceipt);
					SetViewPay(parTypePay);
					break;
			}
			
		}
		
		/// <summary>
		///  Показує запит на введення логіна пароля користувача з розширеними правами. 
		/// </summary>
		bool GetExAccess(CodeEvent parCodeEvent)
		{
			SetView(ModeInterface.GetExAccess);
			// Необхідно очікувати завершення вводу!!! Треба подумати над реалізацією.
			Wait();
			return false;
		}
			
		void ErrorAccess(CodeEvent parCodeEvend,string varExMessage = null)
		{
			SetView(ModeInterface.ErrorAccess);	
			// Необхідно очікувати завершення вводу!!! Треба подумати над реалізацією.			
		}
		/// <summary>
		/// очіцуємо вибору в інтерфейсі.
		/// </summary>
		void Wait() 
		{
			varWait=true;
			do{
				System.Threading.Thread.Sleep(200); //не найкраща реалізація треба щоcь з засипанням потоку  і його просинанням. 
				}while (varWait);
		
		}
		
		
		void IncrementWares()
		{
			
			Quantity.Value++;
			QuantityValueChanged(null, null);
			this.AddWaresReceipt();
			Global.Log(CodeEvent.IncrementWares);
		}
		void DecrementWares()
		{
			if(Quantity.Value>0)
				Quantity.Value--;
			QuantityValueChanged(null, null);
			this.AddWaresReceipt();
			Global.Log(CodeEvent.DecrementWares);
		}


		/// <summary>
		/// Редагуємо кількість в довільному рядку (не зроблено)
		/// </summary>
		void EditQuantityWares()
		{
			SetView(ModeInterface.EditWares);
			
			//Global.Log(CodeEvent.EditQuantityWares);
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
		
		void CountCash()
		{
			
		}
		void InputOutputMoney()
		{
			SetView(ModeInterface.InputOutputMoney);
		}
		void TimerTick(object sender, EventArgs e)
		{
			Time.Text=DateTime.Now.ToString("HH:mm:ss");
		}
		
		

		void NameUnitSelectedIndexChanged(object sender, EventArgs e)
		{
			varWork.varWares.varCodeUnit=Convert.ToInt32(varDtUnit.Rows[NameUnit.SelectedIndex]["code_unit"]);
			varWork.varWares.varCoefficient = Convert.ToDecimal(varDtUnit.Rows[NameUnit.SelectedIndex]["coefficient"]);
			Price.Text = string.Format("{0:0000.00}", varWork.varWares.varPrice* (1+varWork.varWares.varPercentVat)* varWork.varWares.varCoefficient);
		}
		
		void QuantityValueChanged(object sender, EventArgs e)
		{
			varWork.varWares.varQuantity=this.Quantity.Value;
			varWork.varWares.varIsSave=false;
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
			else if(MainInputKeyMap.IncrementWares==varKeys && parOption!=1 && Global.GetTypeAccess(CodeEvent.IncrementWares)==TypeAccess.Yes ) IncrementWares();
			else if(MainInputKeyMap.DecrementWares==varKeys && parOption!=1 && Global.GetTypeAccess(CodeEvent.DecrementWares)==TypeAccess.Yes) DecrementWares();
			else if(MainInputKeyMap.PrintReceipt_Cash==varKeys ) PrintReceipt(TypePay.Cash);
			else if(MainInputKeyMap.PrintReceipt_Pos==varKeys ) PrintReceipt(TypePay.Pos );
			else if(MainInputKeyMap.PrintReceipt_NonCash==varKeys ) PrintReceipt(TypePay.NonCash );
			else if(MainInputKeyMap.AddWaresReceipt==varKeys && parOption==1) AddWaresReceipt();
			else if(MainInputKeyMap.DeleteWaresReceipt_Current==varKeys) DeleteWaresReceipt(TypeDeleteWaresReceipt.Current );
			else if(MainInputKeyMap.DeleteWaresReceipt_All==varKeys) DeleteWaresReceipt(TypeDeleteWaresReceipt.All );
			else if(MainInputKeyMap.DeleteWaresReceipt_Choice==varKeys) DeleteWaresReceipt(TypeDeleteWaresReceipt.Choice );
			else if(MainInputKeyMap.EditQuantityWares==varKeys) EditQuantityWares();
			else if(MainInputKeyMap.ChangeCurrentReceipt==varKeys) ChangeCurrentReceipt();
			else if(MainInputKeyMap.NewReceipt==varKeys) NewReceipt();
			else if(MainInputKeyMap.RecalcPrice==varKeys) varWork.RecalcPrice();
			else if(MainInputKeyMap.Help==varKeys) Help();
			else if(MainInputKeyMap.PrintZ==varKeys) varWork.PrintZ();
			else if(MainInputKeyMap.PrintX==varKeys) varWork.PrintX();
			else if(MainInputKeyMap.CountCash==varKeys) CountCash();
			else if(MainInputKeyMap.UpdateData==varKeys) varWork.UpdateData();
			else if(MainInputKeyMap.PrintPosX == varKeys) varWork.PrintPosX();
			else if(MainInputKeyMap.PrintPosZ == varKeys) varWork.PrintPosZ();
			else if(MainInputKeyMap.InputOutputMoney == varKeys) InputOutputMoney();
			else if(MainInputKeyMap.ReturnLastReceipt == varKeys) ReturnLastReceipt();
			else if(MainInputKeyMap.ReturnOtherWorkplace == varKeys) ReturnOtherWorkplace();
			else if(MainInputKeyMap.ReturnReceipt == varKeys) ReturnReceipt();
			else if(MainInputKeyMap.ReturnAnyReceipt == varKeys) ReturnAnyReceipt();
			else return false;
			return true;
		}

		// Треба зробити!!
		void ReturnLastReceipt()
		{
			//Message("Ви хочете зробити повернення чека № на Суму?" );
			varWork.ReturnLastReceipt(140701,20160325,1);
			SetView(ModeInterface.RetunReceipt);
			ViewWaresReceipt();			
			
			
		}
		
		void ReturnOtherWorkplace()
		{
			Message("ReturnOtherWorkplace Ще не реалізовано" );
		}
		
		void ReturnReceipt()
		{
			Message("ReturnReceipt Ще не реалізовано" );
		}
		
		void ReturnAnyReceipt()
		{
			Message("ReturnAnyReceipt Ще не реалізовано" );
		}
		
		
		
		void DeleteWaresReceipt(TypeDeleteWaresReceipt parTypeDeleteWaresReceipt)
		{
			if( TypeDeleteWaresReceipt.Choice==parTypeDeleteWaresReceipt)
			{
				ClearInfoWares();
				SetView(ModeInterface.DeleteWares);
			}
			else
			{
				varWork.DeleteWaresReceipt(parTypeDeleteWaresReceipt);
				ClearInfoWares();
				ViewWaresReceipt();
			}
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
			SetView(ModeInterface.InputData);
		}
		
		void PrintReceipt()
		{
			//Друк чека!!!
			//varEKKA.p
			//Збереження роздукованого чека
			varWork.WriteReceipt();
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

			this.PrintHotKeyBonus.Visible = (varWork.varClient.varSumMoneyBonus>0);
			this.PrintHotKeyBonusWithOutRest.Visible = (varWork.varClient.varSumMoneyBonus>0) && (varTypePay== TypePay.Cash);
			this.PrintHotKeyBonusFromRest.Visible = varWork.varClient.varIsUseBonusFromRest && (varTypePay== TypePay.Cash);
			this.PrintHotKeyBonusToRest.Visible =   varWork.varClient.varIsUseBonusToRest && (varTypePay== TypePay.Cash) && (varWork.varClient.varSumMoneyBonus -Math.Truncate(varWork.varClient.varSumMoneyBonus)  <=  varWork.varReceipt.varSumReceipt )  ;
			

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
			varWork.ChangeDataTypeBonys(this.varTypeBonus);
			
			
			this.LabelBonus.Text= this.varTypeBonus == TypeBonus.BonusFromRest ? "На бонусний" : "Вик. Бонусів";
			this.PrintPaySum.Value = varWork.varReceipt.varSumReceipt-varWork.varReceipt.varSumBonus;
			this.PrintUsedBonus.Value= varWork.varReceipt.varSumBonus;
			
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
			SetView(ModeInterface.InputData);
		}
		
		
		void PrintCashValueChanged(object sender, EventArgs e)
		{
			varWork.varReceipt.varSumRest = this.PrintCash.Value - varWork.varReceipt.varSumReceipt + this.PrintUsedBonus.Value;
			this.PrintRest.Value = varWork.varReceipt.varSumRest;
		}

		void InputTextChanged(object sender, EventArgs e)
		{
			
		}
		public void SetClient(DataRow parRw)
		{
			varWork.SetClient(parRw);
			Client.Text=varWork.varClient.varNameClient;
			Bonus.Text=varWork.varClient.varSumMoneyBonus.ToString();
		}
		
		/// <summary>
		/// Змінює режим перегляду  в залежності від varModeInterface.
		/// </summary>
		void SetView(ModeInterface parModeInterface = ModeInterface.Default )
		{
			if (parModeInterface != ModeInterface.Default)
				varModeInterface=parModeInterface;
			Input.Enabled = (varModeInterface == ModeInterface.InputData || varModeInterface==ModeInterface.RetunReceipt) ;
			Wares.Enabled = (varModeInterface == ModeInterface.InputData || varModeInterface == ModeInterface.ChoiceWares || varModeInterface == ModeInterface.DeleteWares);
			PanelPrintReceipt.Visible= (varModeInterface == ModeInterface.PrintReceipt);
			if((varModeInterface == ModeInterface.InputData))
				Input.Focus();
			PanelChoiceFound.Visible = (varModeInterface == ModeInterface.ChoiceWares || varModeInterface == ModeInterface.ChoiceClient );
			if((varModeInterface == ModeInterface.ChoiceWares || varModeInterface == ModeInterface.ChoiceClient ))
				ChoiceFound.Focus();
			
			PanelInputOutputMoney.Visible = (varModeInterface == ModeInterface.InputOutputMoney);
			if(varModeInterface == ModeInterface.InputOutputMoney)
				InputOutputMoneySum.Focus();
			if(varModeInterface == ModeInterface.DeleteWares)
				Wares.Focus();
			
			PanelMessage.Visible = (parModeInterface == ModeInterface.Message) || (parModeInterface == ModeInterface.ErrorAccess);
			if(parModeInterface == ModeInterface.Message)
				PanelMessageOk.Focus();
				
			PanelLogin.Visible = 	(parModeInterface == ModeInterface.Login);
			if(parModeInterface == ModeInterface.Login)
				PanelLoginTextBoxLogin.Focus();
			
		}
		void CancelClick(object sender, EventArgs e)
		{
			SetView(ModeInterface.InputData);
			
		}
		
		void OkClick(object sender, EventArgs e)
		{
			SetView(ModeInterface.InputData);
			varWork.varWares.SetWares(varDtWares.Rows[ChoiceFound.CurrentRow.Index]);
			EditQuantity();
		}
		
		
		void InitSize()
		{
			PanelChoiceFound.Top=Wares.Top;
			PanelChoiceFound.Left=Wares.Left;
			PanelChoiceFound.Width=Wares.Width;
			PanelChoiceFound.Height=Wares.Height;
		}
		void ChoiceFoundKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case  Keys.Enter :
					OkClick(null,null);
					break;
				case  Keys.Escape:
					CancelClick(null,null);
					break;
					
			}
		}
		void InputOutputMoneyOkClick(object sender, EventArgs e)
		{
			varWork.InputOutputMoney( this.InputOutputMoneySum.Value);
			SetView(ModeInterface.InputData);
		}
		void InputOutputMoneyCancelClick(object sender, EventArgs e)
		{
			SetView(ModeInterface.InputData);
		}
		
		void ToCenter(Control varSource, Control varExemplar)
		{
			varSource.Top = varExemplar.Height/2+ varExemplar.Top - varSource.Height/2;
			varSource.Left =  varExemplar.Width/2+ varExemplar.Left - varSource.Width/2;
		}


		void InputOutputMoneySumKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case  Keys.Enter :
					InputOutputMoneyOkClick(null,null);
					break;
				case  Keys.Escape:
					InputOutputMoneyCancelClick(null,null);
					break;
					
			}
		}
		void WaresKeyDown(object sender, KeyEventArgs e)
		{
			if(varModeInterface==ModeInterface.DeleteWares || varModeInterface==ModeInterface.EditWares)
			{
				DataTable varDt=(DataTable)Wares.DataSource;
				DataRow varRow=varDt.Rows[Wares.CurrentRow.Index];
				switch (e.KeyData)
				{
					case  Keys.Enter :
						varWork.DeleteWaresReceipt(TypeDeleteWaresReceipt.Choice,Convert.ToInt32(varRow["Code_Wares"]),Convert.ToInt32(varRow["Code_unit"] ));
						ViewWaresReceipt();
						SetView(ModeInterface.InputData);
						break;
					case  Keys.Escape:
						SetView(ModeInterface.InputData);
						break;
				}
			}
		}

		void PanelLoginOkClick(object sender, EventArgs e)
		{
			if(varWork.Login(PanelLoginTextBoxLogin.Text,PanelLoginTextBoxPassWord.Text))
			{
			 cashier.Text=varWork.varUser.varNameUser;				
			 SetView(ModeInterface.InputData);
			 NewReceipt();
			 ViewWaresReceipt();
			}
			else
			{
				Message("Ви ввели неправильний Пароль чи Логін");
				PanelLoginTextBoxPassWord.Text="";
				PanelLoginTextBoxPassWord.Focus();
			}
		
		
		}
	}
}

