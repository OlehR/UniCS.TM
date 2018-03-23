/*
 * Created by SharpDevelop.
 * User: gelo
 * Date: 09.10.2013
 * Time: 9:45
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace MID
{
	partial class NET_MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NET_MainForm));
			this.NameWares = new System.Windows.Forms.TextBox();
			this.NameUnit = new System.Windows.Forms.ComboBox();
			this.Input = new System.Windows.Forms.TextBox();
			this.Client = new System.Windows.Forms.TextBox();
			this.NumberReceipt = new System.Windows.Forms.TextBox();
			this.Price = new System.Windows.Forms.TextBox();
			this.cashier = new System.Windows.Forms.TextBox();
			this.Rest = new System.Windows.Forms.TextBox();
			this.CurReceipt = new System.Windows.Forms.TextBox();
			this.Time = new System.Windows.Forms.TextBox();
			this.Quantity = new System.Windows.Forms.NumericUpDown();
			this.Sum = new System.Windows.Forms.TextBox();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.Wares = new System.Windows.Forms.DataGridView();
			this.PanelPrintReceipt = new System.Windows.Forms.Panel();
			this.LabelRest = new System.Windows.Forms.Label();
			this.PrintHotKeyBonusWithOutRest = new System.Windows.Forms.Label();
			this.PrintHotKeyQuickPOS = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.PrintPaySum = new System.Windows.Forms.NumericUpDown();
			this.PrintHotKeyBonusFromRest = new System.Windows.Forms.Label();
			this.PrintHotKeyNonCash = new System.Windows.Forms.Label();
			this.PrintHotKeyBonusToRest = new System.Windows.Forms.Label();
			this.PrintHotKeyBonus = new System.Windows.Forms.Label();
			this.PrintHotKeyPartiall = new System.Windows.Forms.Label();
			this.PrintHotKeyPos = new System.Windows.Forms.Label();
			this.PrintRest = new System.Windows.Forms.NumericUpDown();
			this.LabelSlip = new System.Windows.Forms.Label();
			this.PrintSlip = new System.Windows.Forms.NumericUpDown();
			this.PrintUsedBonus = new System.Windows.Forms.NumericUpDown();
			this.PrintPos = new System.Windows.Forms.NumericUpDown();
			this.LabelPos = new System.Windows.Forms.Label();
			this.LabelBonus = new System.Windows.Forms.Label();
			this.PrintHotKeyCash = new System.Windows.Forms.Label();
			this.LabelCash = new System.Windows.Forms.Label();
			this.PrintCash = new System.Windows.Forms.NumericUpDown();
			this.ButtonPrintReceipt = new System.Windows.Forms.Button();
			this.Bonus = new System.Windows.Forms.TextBox();
			this.contentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.indexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.customizeToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.optionsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.undoToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.redoToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
			this.cutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.copyToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.pasteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
			this.selectAllToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.newToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.saveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.printToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.printPreviewToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.customizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.PanelChoiceFound = new System.Windows.Forms.Panel();
			this.Cancel = new System.Windows.Forms.Button();
			this.Ok = new System.Windows.Forms.Button();
			this.ChoiceFound = new System.Windows.Forms.DataGridView();
			((System.ComponentModel.ISupportInitialize)(this.Quantity)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Wares)).BeginInit();
			this.PanelPrintReceipt.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PrintPaySum)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PrintRest)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PrintSlip)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PrintUsedBonus)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PrintPos)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PrintCash)).BeginInit();
			this.PanelChoiceFound.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChoiceFound)).BeginInit();
			this.SuspendLayout();
			// 
			// NameWares
			// 
			this.NameWares.Enabled = false;
			this.NameWares.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.NameWares.Location = new System.Drawing.Point(0, 27);
			this.NameWares.Name = "NameWares";
			this.NameWares.Size = new System.Drawing.Size(506, 26);
			this.NameWares.TabIndex = 0;
			// 
			// NameUnit
			// 
			this.NameUnit.Enabled = false;
			this.NameUnit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.NameUnit.FormattingEnabled = true;
			this.NameUnit.Location = new System.Drawing.Point(598, 27);
			this.NameUnit.Name = "NameUnit";
			this.NameUnit.Size = new System.Drawing.Size(63, 28);
			this.NameUnit.TabIndex = 1;
			this.NameUnit.Tag = "Одиниця виміру";
			this.NameUnit.SelectedIndexChanged += new System.EventHandler(this.NameUnitSelectedIndexChanged);
			this.NameUnit.KeyDown += new System.Windows.Forms.KeyEventHandler(this.QuantityKeyDown);
			// 
			// Input
			// 
			this.Input.Location = new System.Drawing.Point(0, 2);
			this.Input.Name = "Input";
			this.Input.Size = new System.Drawing.Size(180, 20);
			this.Input.TabIndex = 3;
			this.Input.TextChanged += new System.EventHandler(this.InputTextChanged);
			this.Input.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputKeyDown);
			this.Input.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.InputKeyPress);
			// 
			// Client
			// 
			this.Client.Enabled = false;
			this.Client.Location = new System.Drawing.Point(186, 3);
			this.Client.Name = "Client";
			this.Client.Size = new System.Drawing.Size(320, 20);
			this.Client.TabIndex = 6;
			// 
			// NumberReceipt
			// 
			this.NumberReceipt.Enabled = false;
			this.NumberReceipt.Location = new System.Drawing.Point(510, 478);
			this.NumberReceipt.Name = "NumberReceipt";
			this.NumberReceipt.Size = new System.Drawing.Size(63, 20);
			this.NumberReceipt.TabIndex = 7;
			// 
			// Price
			// 
			this.Price.Enabled = false;
			this.Price.Location = new System.Drawing.Point(598, 4);
			this.Price.Name = "Price";
			this.Price.Size = new System.Drawing.Size(63, 20);
			this.Price.TabIndex = 8;
			// 
			// cashier
			// 
			this.cashier.Enabled = false;
			this.cashier.Location = new System.Drawing.Point(2, 477);
			this.cashier.Name = "cashier";
			this.cashier.Size = new System.Drawing.Size(309, 20);
			this.cashier.TabIndex = 9;
			// 
			// Rest
			// 
			this.Rest.Enabled = false;
			this.Rest.Location = new System.Drawing.Point(581, 478);
			this.Rest.Name = "Rest";
			this.Rest.Size = new System.Drawing.Size(102, 20);
			this.Rest.TabIndex = 10;
			this.Rest.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CurReceipt
			// 
			this.CurReceipt.Enabled = false;
			this.CurReceipt.Location = new System.Drawing.Point(473, 478);
			this.CurReceipt.Name = "CurReceipt";
			this.CurReceipt.Size = new System.Drawing.Size(27, 20);
			this.CurReceipt.TabIndex = 11;
			this.CurReceipt.Text = "1/1";
			// 
			// Time
			// 
			this.Time.Enabled = false;
			this.Time.Location = new System.Drawing.Point(694, 477);
			this.Time.Name = "Time";
			this.Time.Size = new System.Drawing.Size(71, 20);
			this.Time.TabIndex = 12;
			this.Time.Tag = "Текучий час";
			// 
			// Quantity
			// 
			this.Quantity.Enabled = false;
			this.Quantity.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Quantity.Location = new System.Drawing.Point(514, 28);
			this.Quantity.Maximum = new decimal(new int[] {
			100000,
			0,
			0,
			0});
			this.Quantity.Name = "Quantity";
			this.Quantity.Size = new System.Drawing.Size(78, 26);
			this.Quantity.TabIndex = 13;
			this.Quantity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.Quantity.ValueChanged += new System.EventHandler(this.QuantityValueChanged);
			this.Quantity.KeyDown += new System.Windows.Forms.KeyEventHandler(this.QuantityKeyDown);
			// 
			// Sum
			// 
			this.Sum.Enabled = false;
			this.Sum.Font = new System.Drawing.Font("Microsoft Sans Serif", 26F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Sum.Location = new System.Drawing.Point(665, 5);
			this.Sum.Name = "Sum";
			this.Sum.Size = new System.Drawing.Size(100, 47);
			this.Sum.TabIndex = 16;
			this.Sum.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Interval = 200;
			this.timer.Tick += new System.EventHandler(this.TimerTick);
			// 
			// Wares
			// 
			this.Wares.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.Wares.Location = new System.Drawing.Point(2, 61);
			this.Wares.Name = "Wares";
			this.Wares.Size = new System.Drawing.Size(763, 410);
			this.Wares.TabIndex = 24;
			// 
			// PanelPrintReceipt
			// 
			this.PanelPrintReceipt.Controls.Add(this.LabelRest);
			this.PanelPrintReceipt.Controls.Add(this.PrintHotKeyBonusWithOutRest);
			this.PanelPrintReceipt.Controls.Add(this.PrintHotKeyQuickPOS);
			this.PanelPrintReceipt.Controls.Add(this.label1);
			this.PanelPrintReceipt.Controls.Add(this.PrintPaySum);
			this.PanelPrintReceipt.Controls.Add(this.PrintHotKeyBonusFromRest);
			this.PanelPrintReceipt.Controls.Add(this.PrintHotKeyNonCash);
			this.PanelPrintReceipt.Controls.Add(this.PrintHotKeyBonusToRest);
			this.PanelPrintReceipt.Controls.Add(this.PrintHotKeyBonus);
			this.PanelPrintReceipt.Controls.Add(this.PrintHotKeyPartiall);
			this.PanelPrintReceipt.Controls.Add(this.PrintHotKeyPos);
			this.PanelPrintReceipt.Controls.Add(this.PrintRest);
			this.PanelPrintReceipt.Controls.Add(this.LabelSlip);
			this.PanelPrintReceipt.Controls.Add(this.PrintSlip);
			this.PanelPrintReceipt.Controls.Add(this.PrintUsedBonus);
			this.PanelPrintReceipt.Controls.Add(this.PrintPos);
			this.PanelPrintReceipt.Controls.Add(this.LabelPos);
			this.PanelPrintReceipt.Controls.Add(this.LabelBonus);
			this.PanelPrintReceipt.Controls.Add(this.PrintHotKeyCash);
			this.PanelPrintReceipt.Controls.Add(this.LabelCash);
			this.PanelPrintReceipt.Controls.Add(this.PrintCash);
			this.PanelPrintReceipt.Controls.Add(this.ButtonPrintReceipt);
			this.PanelPrintReceipt.Location = new System.Drawing.Point(338, 105);
			this.PanelPrintReceipt.Name = "PanelPrintReceipt";
			this.PanelPrintReceipt.Size = new System.Drawing.Size(420, 270);
			this.PanelPrintReceipt.TabIndex = 25;
			this.PanelPrintReceipt.Visible = false;
			// 
			// LabelRest
			// 
			this.LabelRest.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.LabelRest.Location = new System.Drawing.Point(219, 192);
			this.LabelRest.Name = "LabelRest";
			this.LabelRest.Size = new System.Drawing.Size(74, 23);
			this.LabelRest.TabIndex = 33;
			this.LabelRest.Text = "Здача:";
			this.LabelRest.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// PrintHotKeyBonusWithOutRest
			// 
			this.PrintHotKeyBonusWithOutRest.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PrintHotKeyBonusWithOutRest.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.PrintHotKeyBonusWithOutRest.Location = new System.Drawing.Point(7, 157);
			this.PrintHotKeyBonusWithOutRest.Name = "PrintHotKeyBonusWithOutRest";
			this.PrintHotKeyBonusWithOutRest.Size = new System.Drawing.Size(170, 23);
			this.PrintHotKeyBonusWithOutRest.TabIndex = 32;
			this.PrintHotKeyBonusWithOutRest.Text = "F2-Бонуси без решти";
			this.PrintHotKeyBonusWithOutRest.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// PrintHotKeyQuickPOS
			// 
			this.PrintHotKeyQuickPOS.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PrintHotKeyQuickPOS.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.PrintHotKeyQuickPOS.Location = new System.Drawing.Point(7, 71);
			this.PrintHotKeyQuickPOS.Name = "PrintHotKeyQuickPOS";
			this.PrintHotKeyQuickPOS.Size = new System.Drawing.Size(210, 23);
			this.PrintHotKeyQuickPOS.TabIndex = 31;
			this.PrintHotKeyQuickPOS.Text = "Ctrl+F1..F3-Оплата по POS";
			this.PrintHotKeyQuickPOS.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label1.Location = new System.Drawing.Point(180, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(110, 29);
			this.label1.TabIndex = 30;
			this.label1.Text = "До сплати:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// PrintPaySum
			// 
			this.PrintPaySum.DecimalPlaces = 2;
			this.PrintPaySum.Enabled = false;
			this.PrintPaySum.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PrintPaySum.Location = new System.Drawing.Point(294, 13);
			this.PrintPaySum.Maximum = new decimal(new int[] {
			100000,
			0,
			0,
			0});
			this.PrintPaySum.Name = "PrintPaySum";
			this.PrintPaySum.Size = new System.Drawing.Size(120, 29);
			this.PrintPaySum.TabIndex = 29;
			this.PrintPaySum.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PrintHotKeyBonusFromRest
			// 
			this.PrintHotKeyBonusFromRest.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PrintHotKeyBonusFromRest.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.PrintHotKeyBonusFromRest.Location = new System.Drawing.Point(7, 196);
			this.PrintHotKeyBonusFromRest.Name = "PrintHotKeyBonusFromRest";
			this.PrintHotKeyBonusFromRest.Size = new System.Drawing.Size(184, 23);
			this.PrintHotKeyBonusFromRest.TabIndex = 28;
			this.PrintHotKeyBonusFromRest.Text = "F4-Здача на бонусний";
			this.PrintHotKeyBonusFromRest.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// PrintHotKeyNonCash
			// 
			this.PrintHotKeyNonCash.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PrintHotKeyNonCash.Location = new System.Drawing.Point(7, 48);
			this.PrintHotKeyNonCash.Name = "PrintHotKeyNonCash";
			this.PrintHotKeyNonCash.Size = new System.Drawing.Size(148, 23);
			this.PrintHotKeyNonCash.TabIndex = 27;
			this.PrintHotKeyNonCash.Text = "Alt+F9-Безготівка";
			this.PrintHotKeyNonCash.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// PrintHotKeyBonusToRest
			// 
			this.PrintHotKeyBonusToRest.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PrintHotKeyBonusToRest.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.PrintHotKeyBonusToRest.Location = new System.Drawing.Point(7, 175);
			this.PrintHotKeyBonusToRest.Name = "PrintHotKeyBonusToRest";
			this.PrintHotKeyBonusToRest.Size = new System.Drawing.Size(184, 23);
			this.PrintHotKeyBonusToRest.TabIndex = 26;
			this.PrintHotKeyBonusToRest.Text = "F3-Здача з бонусного";
			this.PrintHotKeyBonusToRest.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// PrintHotKeyBonus
			// 
			this.PrintHotKeyBonus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PrintHotKeyBonus.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.PrintHotKeyBonus.Location = new System.Drawing.Point(7, 140);
			this.PrintHotKeyBonus.Name = "PrintHotKeyBonus";
			this.PrintHotKeyBonus.Size = new System.Drawing.Size(93, 23);
			this.PrintHotKeyBonus.TabIndex = 25;
			this.PrintHotKeyBonus.Text = "F1-Бонуси";
			this.PrintHotKeyBonus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// PrintHotKeyPartiall
			// 
			this.PrintHotKeyPartiall.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PrintHotKeyPartiall.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.PrintHotKeyPartiall.Location = new System.Drawing.Point(7, 93);
			this.PrintHotKeyPartiall.Name = "PrintHotKeyPartiall";
			this.PrintHotKeyPartiall.Size = new System.Drawing.Size(184, 23);
			this.PrintHotKeyPartiall.TabIndex = 24;
			this.PrintHotKeyPartiall.Text = "Shift+F9-Часткова оплата";
			this.PrintHotKeyPartiall.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// PrintHotKeyPos
			// 
			this.PrintHotKeyPos.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PrintHotKeyPos.Location = new System.Drawing.Point(7, 25);
			this.PrintHotKeyPos.Name = "PrintHotKeyPos";
			this.PrintHotKeyPos.Size = new System.Drawing.Size(104, 23);
			this.PrintHotKeyPos.TabIndex = 23;
			this.PrintHotKeyPos.Text = "Ctrl+F9-POS";
			this.PrintHotKeyPos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// PrintRest
			// 
			this.PrintRest.DecimalPlaces = 2;
			this.PrintRest.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PrintRest.Location = new System.Drawing.Point(294, 189);
			this.PrintRest.Maximum = new decimal(new int[] {
			100000,
			0,
			0,
			0});
			this.PrintRest.Minimum = new decimal(new int[] {
			10000,
			0,
			0,
			0});
			this.PrintRest.Name = "PrintRest";
			this.PrintRest.Size = new System.Drawing.Size(120, 29);
			this.PrintRest.TabIndex = 21;
			this.PrintRest.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PrintRest.Value = new decimal(new int[] {
			10000,
			0,
			0,
			0});
			this.PrintRest.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PrintInputKeyDown);
			// 
			// LabelSlip
			// 
			this.LabelSlip.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.LabelSlip.Location = new System.Drawing.Point(190, 116);
			this.LabelSlip.Name = "LabelSlip";
			this.LabelSlip.Size = new System.Drawing.Size(100, 29);
			this.LabelSlip.TabIndex = 20;
			this.LabelSlip.Text = "Slip:";
			this.LabelSlip.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// PrintSlip
			// 
			this.PrintSlip.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PrintSlip.Location = new System.Drawing.Point(294, 118);
			this.PrintSlip.Maximum = new decimal(new int[] {
			9999,
			0,
			0,
			0});
			this.PrintSlip.Name = "PrintSlip";
			this.PrintSlip.Size = new System.Drawing.Size(120, 29);
			this.PrintSlip.TabIndex = 19;
			this.PrintSlip.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PrintSlip.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PrintInputKeyDown);
			// 
			// PrintUsedBonus
			// 
			this.PrintUsedBonus.DecimalPlaces = 2;
			this.PrintUsedBonus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PrintUsedBonus.Location = new System.Drawing.Point(294, 153);
			this.PrintUsedBonus.Maximum = new decimal(new int[] {
			100000,
			0,
			0,
			0});
			this.PrintUsedBonus.Minimum = new decimal(new int[] {
			1,
			0,
			0,
			0});
			this.PrintUsedBonus.Name = "PrintUsedBonus";
			this.PrintUsedBonus.Size = new System.Drawing.Size(120, 29);
			this.PrintUsedBonus.TabIndex = 18;
			this.PrintUsedBonus.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PrintUsedBonus.Value = new decimal(new int[] {
			1,
			0,
			0,
			0});
			this.PrintUsedBonus.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PrintInputKeyDown);
			// 
			// PrintPos
			// 
			this.PrintPos.DecimalPlaces = 2;
			this.PrintPos.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PrintPos.Location = new System.Drawing.Point(294, 83);
			this.PrintPos.Name = "PrintPos";
			this.PrintPos.Size = new System.Drawing.Size(120, 29);
			this.PrintPos.TabIndex = 17;
			this.PrintPos.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PrintPos.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PrintInputKeyDown);
			// 
			// LabelPos
			// 
			this.LabelPos.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.LabelPos.Location = new System.Drawing.Point(201, 84);
			this.LabelPos.Name = "LabelPos";
			this.LabelPos.Size = new System.Drawing.Size(89, 23);
			this.LabelPos.TabIndex = 16;
			this.LabelPos.Text = "Картка:";
			this.LabelPos.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// LabelBonus
			// 
			this.LabelBonus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.LabelBonus.Location = new System.Drawing.Point(219, 153);
			this.LabelBonus.Name = "LabelBonus";
			this.LabelBonus.Size = new System.Drawing.Size(74, 29);
			this.LabelBonus.TabIndex = 11;
			this.LabelBonus.Text = "Бонус:";
			this.LabelBonus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// PrintHotKeyCash
			// 
			this.PrintHotKeyCash.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PrintHotKeyCash.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.PrintHotKeyCash.Location = new System.Drawing.Point(7, 3);
			this.PrintHotKeyCash.Name = "PrintHotKeyCash";
			this.PrintHotKeyCash.Size = new System.Drawing.Size(93, 23);
			this.PrintHotKeyCash.TabIndex = 8;
			this.PrintHotKeyCash.Text = "F9-Готівка ";
			this.PrintHotKeyCash.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LabelCash
			// 
			this.LabelCash.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.LabelCash.Location = new System.Drawing.Point(201, 48);
			this.LabelCash.Name = "LabelCash";
			this.LabelCash.Size = new System.Drawing.Size(89, 29);
			this.LabelCash.TabIndex = 7;
			this.LabelCash.Text = "Готівка:";
			this.LabelCash.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// PrintCash
			// 
			this.PrintCash.DecimalPlaces = 2;
			this.PrintCash.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.PrintCash.Location = new System.Drawing.Point(294, 48);
			this.PrintCash.Maximum = new decimal(new int[] {
			100000,
			0,
			0,
			0});
			this.PrintCash.Name = "PrintCash";
			this.PrintCash.Size = new System.Drawing.Size(120, 29);
			this.PrintCash.TabIndex = 4;
			this.PrintCash.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PrintCash.ValueChanged += new System.EventHandler(this.PrintCashValueChanged);
			this.PrintCash.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PrintInputKeyDown);
			// 
			// ButtonPrintReceipt
			// 
			this.ButtonPrintReceipt.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.ButtonPrintReceipt.Location = new System.Drawing.Point(11, 225);
			this.ButtonPrintReceipt.Name = "ButtonPrintReceipt";
			this.ButtonPrintReceipt.Size = new System.Drawing.Size(403, 42);
			this.ButtonPrintReceipt.TabIndex = 0;
			this.ButtonPrintReceipt.Text = "Друк";
			this.ButtonPrintReceipt.UseVisualStyleBackColor = true;
			this.ButtonPrintReceipt.Click += new System.EventHandler(this.ButtonPrintReceiptClick);
			this.ButtonPrintReceipt.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PrintInputKeyDown);
			// 
			// Bonus
			// 
			this.Bonus.Enabled = false;
			this.Bonus.Location = new System.Drawing.Point(517, 4);
			this.Bonus.Name = "Bonus";
			this.Bonus.Size = new System.Drawing.Size(75, 20);
			this.Bonus.TabIndex = 26;
			// 
			// contentsToolStripMenuItem
			// 
			this.contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
			this.contentsToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
			this.contentsToolStripMenuItem.Text = "&Contents";
			// 
			// indexToolStripMenuItem
			// 
			this.indexToolStripMenuItem.Name = "indexToolStripMenuItem";
			this.indexToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
			this.indexToolStripMenuItem.Text = "&Index";
			// 
			// searchToolStripMenuItem
			// 
			this.searchToolStripMenuItem.Name = "searchToolStripMenuItem";
			this.searchToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
			this.searchToolStripMenuItem.Text = "&Search";
			// 
			// toolStripSeparator10
			// 
			this.toolStripSeparator10.Name = "toolStripSeparator10";
			this.toolStripSeparator10.Size = new System.Drawing.Size(6, 6);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
			this.aboutToolStripMenuItem.Text = "&About...";
			// 
			// customizeToolStripMenuItem1
			// 
			this.customizeToolStripMenuItem1.Name = "customizeToolStripMenuItem1";
			this.customizeToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			this.customizeToolStripMenuItem1.Text = "&Customize";
			// 
			// optionsToolStripMenuItem1
			// 
			this.optionsToolStripMenuItem1.Name = "optionsToolStripMenuItem1";
			this.optionsToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			this.optionsToolStripMenuItem1.Text = "&Options";
			// 
			// undoToolStripMenuItem1
			// 
			this.undoToolStripMenuItem1.Name = "undoToolStripMenuItem1";
			this.undoToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			this.undoToolStripMenuItem1.Text = "&Undo";
			// 
			// redoToolStripMenuItem1
			// 
			this.redoToolStripMenuItem1.Name = "redoToolStripMenuItem1";
			this.redoToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			this.redoToolStripMenuItem1.Text = "&Redo";
			// 
			// toolStripSeparator8
			// 
			this.toolStripSeparator8.Name = "toolStripSeparator8";
			this.toolStripSeparator8.Size = new System.Drawing.Size(6, 6);
			// 
			// cutToolStripMenuItem1
			// 
			this.cutToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("cutToolStripMenuItem1.Image")));
			this.cutToolStripMenuItem1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cutToolStripMenuItem1.Name = "cutToolStripMenuItem1";
			this.cutToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			this.cutToolStripMenuItem1.Text = "Cu&t";
			// 
			// copyToolStripMenuItem1
			// 
			this.copyToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("copyToolStripMenuItem1.Image")));
			this.copyToolStripMenuItem1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.copyToolStripMenuItem1.Name = "copyToolStripMenuItem1";
			this.copyToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			this.copyToolStripMenuItem1.Text = "&Copy";
			// 
			// pasteToolStripMenuItem1
			// 
			this.pasteToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("pasteToolStripMenuItem1.Image")));
			this.pasteToolStripMenuItem1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.pasteToolStripMenuItem1.Name = "pasteToolStripMenuItem1";
			this.pasteToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			this.pasteToolStripMenuItem1.Text = "&Paste";
			// 
			// toolStripSeparator9
			// 
			this.toolStripSeparator9.Name = "toolStripSeparator9";
			this.toolStripSeparator9.Size = new System.Drawing.Size(6, 6);
			// 
			// selectAllToolStripMenuItem1
			// 
			this.selectAllToolStripMenuItem1.Name = "selectAllToolStripMenuItem1";
			this.selectAllToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			this.selectAllToolStripMenuItem1.Text = "Select &All";
			// 
			// newToolStripMenuItem1
			// 
			this.newToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripMenuItem1.Image")));
			this.newToolStripMenuItem1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.newToolStripMenuItem1.Name = "newToolStripMenuItem1";
			this.newToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			this.newToolStripMenuItem1.Text = "&New";
			// 
			// openToolStripMenuItem1
			// 
			this.openToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem1.Image")));
			this.openToolStripMenuItem1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.openToolStripMenuItem1.Name = "openToolStripMenuItem1";
			this.openToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			this.openToolStripMenuItem1.Text = "&Open";
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(6, 6);
			// 
			// saveToolStripMenuItem1
			// 
			this.saveToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem1.Image")));
			this.saveToolStripMenuItem1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.saveToolStripMenuItem1.Name = "saveToolStripMenuItem1";
			this.saveToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			this.saveToolStripMenuItem1.Text = "&Save";
			// 
			// saveAsToolStripMenuItem1
			// 
			this.saveAsToolStripMenuItem1.Name = "saveAsToolStripMenuItem1";
			this.saveAsToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			this.saveAsToolStripMenuItem1.Text = "Save &As";
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(6, 6);
			// 
			// printToolStripMenuItem1
			// 
			this.printToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("printToolStripMenuItem1.Image")));
			this.printToolStripMenuItem1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.printToolStripMenuItem1.Name = "printToolStripMenuItem1";
			this.printToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			this.printToolStripMenuItem1.Text = "&Print";
			// 
			// printPreviewToolStripMenuItem1
			// 
			this.printPreviewToolStripMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("printPreviewToolStripMenuItem1.Image")));
			this.printPreviewToolStripMenuItem1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.printPreviewToolStripMenuItem1.Name = "printPreviewToolStripMenuItem1";
			this.printPreviewToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			this.printPreviewToolStripMenuItem1.Text = "Print Pre&view";
			// 
			// toolStripSeparator7
			// 
			this.toolStripSeparator7.Name = "toolStripSeparator7";
			this.toolStripSeparator7.Size = new System.Drawing.Size(6, 6);
			// 
			// exitToolStripMenuItem1
			// 
			this.exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
			this.exitToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
			this.exitToolStripMenuItem1.Text = "E&xit";
			// 
			// customizeToolStripMenuItem
			// 
			this.customizeToolStripMenuItem.Name = "customizeToolStripMenuItem";
			this.customizeToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
			this.customizeToolStripMenuItem.Text = "&Customize";
			// 
			// optionsToolStripMenuItem
			// 
			this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			this.optionsToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
			this.optionsToolStripMenuItem.Text = "&Options";
			// 
			// undoToolStripMenuItem
			// 
			this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
			this.undoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
			this.undoToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.undoToolStripMenuItem.Text = "&Undo";
			// 
			// redoToolStripMenuItem
			// 
			this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
			this.redoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
			this.redoToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.redoToolStripMenuItem.Text = "&Redo";
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(141, 6);
			// 
			// cutToolStripMenuItem
			// 
			this.cutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("cutToolStripMenuItem.Image")));
			this.cutToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
			this.cutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
			this.cutToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.cutToolStripMenuItem.Text = "Cu&t";
			// 
			// copyToolStripMenuItem
			// 
			this.copyToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("copyToolStripMenuItem.Image")));
			this.copyToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.copyToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.copyToolStripMenuItem.Text = "&Copy";
			// 
			// pasteToolStripMenuItem
			// 
			this.pasteToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pasteToolStripMenuItem.Image")));
			this.pasteToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
			this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.pasteToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.pasteToolStripMenuItem.Text = "&Paste";
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(141, 6);
			// 
			// selectAllToolStripMenuItem
			// 
			this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
			this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.selectAllToolStripMenuItem.Text = "Select &All";
			// 
			// PanelChoiceFound
			// 
			this.PanelChoiceFound.Controls.Add(this.Cancel);
			this.PanelChoiceFound.Controls.Add(this.Ok);
			this.PanelChoiceFound.Controls.Add(this.ChoiceFound);
			this.PanelChoiceFound.Location = new System.Drawing.Point(12, 82);
			this.PanelChoiceFound.Name = "PanelChoiceFound";
			this.PanelChoiceFound.Size = new System.Drawing.Size(294, 238);
			this.PanelChoiceFound.TabIndex = 28;
			this.PanelChoiceFound.Visible = false;
			// 
			// Cancel
			// 
			this.Cancel.Location = new System.Drawing.Point(199, 211);
			this.Cancel.Name = "Cancel";
			this.Cancel.Size = new System.Drawing.Size(75, 23);
			this.Cancel.TabIndex = 30;
			this.Cancel.Text = "Cancel";
			this.Cancel.UseVisualStyleBackColor = true;
			// 
			// Ok
			// 
			this.Ok.Location = new System.Drawing.Point(7, 211);
			this.Ok.Name = "Ok";
			this.Ok.Size = new System.Drawing.Size(75, 23);
			this.Ok.TabIndex = 29;
			this.Ok.Text = "Ok";
			this.Ok.UseVisualStyleBackColor = true;
			this.Ok.Click += new System.EventHandler(this.OkClick);
			// 
			// ChoiceFound
			// 
			this.ChoiceFound.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.ChoiceFound.Location = new System.Drawing.Point(7, 9);
			this.ChoiceFound.Name = "ChoiceFound";
			this.ChoiceFound.Size = new System.Drawing.Size(277, 190);
			this.ChoiceFound.TabIndex = 28;
			this.ChoiceFound.Visible = false;
			// 
			// NET_MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(770, 500);
			this.Controls.Add(this.PanelChoiceFound);
			this.Controls.Add(this.Bonus);
			this.Controls.Add(this.PanelPrintReceipt);
			this.Controls.Add(this.Wares);
			this.Controls.Add(this.Sum);
			this.Controls.Add(this.Quantity);
			this.Controls.Add(this.Time);
			this.Controls.Add(this.CurReceipt);
			this.Controls.Add(this.Rest);
			this.Controls.Add(this.cashier);
			this.Controls.Add(this.Price);
			this.Controls.Add(this.NumberReceipt);
			this.Controls.Add(this.Client);
			this.Controls.Add(this.Input);
			this.Controls.Add(this.NameUnit);
			this.Controls.Add(this.NameWares);
			this.Name = "NET_MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Робоче місце касира";
			((System.ComponentModel.ISupportInitialize)(this.Quantity)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Wares)).EndInit();
			this.PanelPrintReceipt.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PrintPaySum)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PrintRest)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PrintSlip)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PrintUsedBonus)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PrintPos)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PrintCash)).EndInit();
			this.PanelChoiceFound.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ChoiceFound)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
		private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
		private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
		private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem customizeToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
		private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem indexToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem contentsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem customizeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
		private System.Windows.Forms.Label PrintHotKeyQuickPOS;
		private System.Windows.Forms.Label PrintHotKeyBonusWithOutRest;
		private System.Windows.Forms.NumericUpDown PrintPaySum;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label PrintHotKeyBonusFromRest;
		private System.Windows.Forms.Label PrintHotKeyNonCash;
		private System.Windows.Forms.Label PrintHotKeyPos;
		private System.Windows.Forms.Label PrintHotKeyPartiall;
		private System.Windows.Forms.Label PrintHotKeyBonus;
		private System.Windows.Forms.Label PrintHotKeyBonusToRest;
		private System.Windows.Forms.Label LabelPos;
		private System.Windows.Forms.NumericUpDown PrintPos;
		private System.Windows.Forms.TextBox Bonus;
		private System.Windows.Forms.NumericUpDown PrintUsedBonus;
		private System.Windows.Forms.NumericUpDown PrintRest;
		private System.Windows.Forms.Label LabelRest;
		private System.Windows.Forms.Label LabelBonus;
		private System.Windows.Forms.Label LabelCash;
		private System.Windows.Forms.Label PrintHotKeyCash;
		private System.Windows.Forms.Label LabelSlip;
		private System.Windows.Forms.DataGridView Wares;
		private System.Windows.Forms.Button ButtonPrintReceipt;
		private System.Windows.Forms.NumericUpDown PrintCash;
		private System.Windows.Forms.NumericUpDown PrintSlip;
		private System.Windows.Forms.Panel PanelPrintReceipt;
		//private SourceGrid.Extensions.PingGrids.PingGrid WaresReceipt;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.TextBox Sum;
		private System.Windows.Forms.NumericUpDown Quantity;
		private System.Windows.Forms.TextBox CurReceipt;
		private System.Windows.Forms.TextBox Rest;
		private System.Windows.Forms.TextBox cashier;
		private System.Windows.Forms.TextBox Price;
		private System.Windows.Forms.TextBox NumberReceipt;
		private System.Windows.Forms.TextBox Client;
		private System.Windows.Forms.TextBox Time;
		private System.Windows.Forms.TextBox Input;
		private System.Windows.Forms.ComboBox NameUnit;
		private System.Windows.Forms.TextBox NameWares;
		private System.Windows.Forms.DataGridView ChoiceFound;
		private System.Windows.Forms.Panel PanelChoiceFound;
		private System.Windows.Forms.Button Cancel;
		private System.Windows.Forms.Button Ok;
		
	}
}
