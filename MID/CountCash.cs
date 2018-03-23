
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

    public partial class CountCash : Form
    {
//        ymax = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height;
//        ymax = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
//        ymax = System.Windows.Forms.SystemInformation.PrimaryMonitorMaximizedWindowSize.Height;
        
        int displayExtensionY = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
        
        int sizeFormX ;
        int sizeFormY ;
        int sizeElY;
        
        int ElRetreatX = 8;// відступ між елементами в групі
        int InitialRetreatX = 12;
        int InitialRetreatY = 12;
        
        // Шрифти
        string lGroupeFont = "Microsoft Sans Serif"; //найменування
        string tbGroupeFont = "Microsoft Sans Serif"; //кількість
        string resultFont = "Microsoft Sans Serif"; // сума
        string gbGroupeFont = "Microsoft Sans Serif";
        
        int lGroupeFontSize;
        int tbGroupeFontSize;
        int resultFontSize;
        int gbGroupeFontSize = 18;
        
        //---
        bool nonNumberEntered;
        
        
        
        getNominal nominal = new getNominal ("грн:1:500,200,100,50,20,10,5,2,1;коп:0.01:100,50,25,10,5,2,1");
        
        private System.Windows.Forms.TextBox result;
        
        private System.Windows.Forms.Label [][] lGroupe;
        private System.Windows.Forms.TextBox [][] tbGroupe;
        
        public CountCash()
        {
            getSizeElement ();
            InitializeElement();
            InitializeComponent();
            
        }
        
        
        
        private void InitializeElement()
        {
            this.result = new System.Windows.Forms.TextBox();
            this.result.AutoSize = false;
            this.result.Size = new System.Drawing.Size((sizeElY*8+2*InitialRetreatX), sizeElY*2);
            this.result.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.result.ReadOnly = true;
            this.result.TabStop = false;
            this.result.Font =  new System.Drawing.Font(resultFont, resultFontSize);
            this.result.Location  = new System.Drawing.Point(InitialRetreatX, InitialRetreatY);
            this.Controls.Add(this.result);
            
            this.lGroupe = new System.Windows.Forms.Label[nominal.cashStruct.Length][];
            this.tbGroupe = new System.Windows.Forms.TextBox[nominal.cashStruct.Length][];
            
            int pointGbY = sizeElY*2+InitialRetreatY;
            // розмір елементів
            int sizeElX = sizeElY*2;
            // розмір GroupBox
            int GbSizeX = sizeElX*4 + InitialRetreatX*2;
            int GbSizeY;
            // Шрифти
            
            // Зміщення елементів
            int pointOffsetX = sizeElX*2;
            int pointOffsetY = sizeElY+ElRetreatX;
            //
            int x = InitialRetreatX;
            int y = InitialRetreatY+gbGroupeFontSize;
            
            for (int i = 0; i < nominal.cashStruct.Length; i ++)
            {
                this.lGroupe[i] =  new System.Windows.Forms.Label[nominal.cashStruct[i].Nominal.Length];
                this.tbGroupe[i] = new System.Windows.Forms.TextBox[nominal.cashStruct[i].Nominal.Length];
                
                GbSizeY = (int)((InitialRetreatX*2) + sizeElY * (Math.Ceiling((decimal)(nominal.cashStruct[i].Nominal.Length)/2))
                                + ElRetreatX * (Math.Ceiling((decimal)(nominal.cashStruct[i].Nominal.Length)/2)-1) + gbGroupeFontSize);
                
                GroupBox[] gbGroupe = new System.Windows.Forms.GroupBox[nominal.cashStruct.Length];
                gbGroupe[i] = new System.Windows.Forms.GroupBox();
                gbGroupe[i].Location  = new System.Drawing.Point(InitialRetreatX, pointGbY);
                gbGroupe[i].Size = new System.Drawing.Size(GbSizeX, GbSizeY);
                gbGroupe[i].Text = nominal.cashStruct[i].Name;
                gbGroupe[i].Font = new System.Drawing.Font(gbGroupeFont, gbGroupeFontSize);
                Controls.Add(gbGroupe[i]);
                
                for ( int j = 0; j < nominal.cashStruct[i].Nominal.Length; j ++)
                {
                    if (j%2==0 && j !=0) {x = InitialRetreatX; y = y+pointOffsetY;}
                    
                    this.lGroupe[i][j] = new System.Windows.Forms.Label();
                    this.lGroupe[i][j].Size = new System.Drawing.Size(sizeElX, sizeElY);
                    this.lGroupe[i][j].TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                    this.lGroupe[i][j].Location = new System.Drawing.Point(x, y);
                    this.lGroupe[i][j].Font = new System.Drawing.Font(lGroupeFont, lGroupeFontSize);
                    this.lGroupe[i][j].BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                    this.lGroupe[i][j].Text = "" +nominal.cashStruct[i].Nominal[j];
                    
                    
                    this.tbGroupe[i][j] = new System.Windows.Forms.TextBox();
                    this.tbGroupe[i][j].Size = new System.Drawing.Size(sizeElX, sizeElY);
                    this.tbGroupe[i][j].TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                    this.tbGroupe[i][j].Location = new System.Drawing.Point((x + sizeElX), y);
                    this.tbGroupe[i][j].Font = new System.Drawing.Font(tbGroupeFont, tbGroupeFontSize);
                    this.tbGroupe[i][j].AutoSize = false;
                    this.tbGroupe[i][j].Text = "0";
                    this.tbGroupe[i][j].KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbGroupeNumberCompetitorKeyDown);
                    this.tbGroupe[i][j].KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbGroupeNumberCompetitorKeyPress);
                    this.tbGroupe[i][j].TextChanged += new System.EventHandler(this.tbGroupeTextChanged);
                    
                    
                    
                    
                    gbGroupe[i].Controls.Add(lGroupe[i][j]);
                    gbGroupe[i].Controls.Add(tbGroupe[i][j]);
                    
                    x += pointOffsetX;
                }
                
                // зміщення координат наступного GroupBox
                pointGbY += GbSizeY;
                // початкові координати елементів у групі
                x = InitialRetreatX;
                y = InitialRetreatY+gbGroupeFontSize;
            }
            this.sizeFormX = GbSizeX + 2*InitialRetreatX;
            this.sizeFormY = pointGbY+InitialRetreatX;
        }
        
        
        private void calculation()
        {
            decimal sum = 0;
            
            for (int i = 0; i < nominal.cashStruct.Length; i ++)
            {
                for(int j = 0; j < nominal.cashStruct[i].Nominal.Length; j ++)
                    
                {
                    if (this.tbGroupe[i][j].Text != null & this.tbGroupe[i][j].Text.Length>0)
                    {
                        sum = sum +  nominal.cashStruct[i].Coef * Convert.ToDecimal(nominal.cashStruct[i].Nominal[j]) * Convert.ToDecimal(this.tbGroupe[i][j].Text);
                    }
                }
            }
            this.result.Text = Convert.ToString(sum); //""+sizeFormY;//
        }

        
        void tbGroupeTextChanged(object sender, System.EventArgs e)
        {
            calculation();
        }
        
        
        private void getSizeElement ()
        {
            int quantityColumn = 2;
            int[] quantityLine = new int[nominal.cashStruct.Length];
            int line = 0;
            
            for (int i = 0; i < nominal.cashStruct.Length; i++)
            {
                quantityLine[i] = Convert.ToInt32 (Math.Ceiling((decimal)(nominal.cashStruct[i].Nominal.Length)/quantityColumn));
                line += Convert.ToInt32 (Math.Ceiling((decimal)(nominal.cashStruct[i].Nominal.Length)/quantityColumn));
            }
            
            
            int SpaceForElement = displayExtensionY - (2*InitialRetreatY + nominal.cashStruct.Length*InitialRetreatY) - (gbGroupeFontSize*nominal.cashStruct.Length) -
                (2*InitialRetreatY*nominal.cashStruct.Length) - (line - nominal.cashStruct.Length)*ElRetreatX;
            
            this.sizeElY =  (SpaceForElement /(line+2));
            this.lGroupeFontSize = this.tbGroupeFontSize =  (int)Math.Floor(sizeElY*0.65-4.41);
            this.resultFontSize = tbGroupeFontSize*2;
            this.gbGroupeFontSize = Convert.ToInt32 (Math.Floor( (double)tbGroupeFontSize*0.7));
            
        }
        
        
        void tbGroupeNumberCompetitorKeyDown(object sender, KeyEventArgs e)
        {
            nonNumberEntered = false;
            if (e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9)
            {
                if (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9)
                {
                    if(e.KeyCode != Keys.Back && e.KeyCode != Keys.Enter && e.KeyCode != Keys.Delete)
                    {
                        nonNumberEntered = true;
                    }
                }
            }
            if (Control.ModifierKeys == Keys.Shift) {
                nonNumberEntered = true;
            }
        }
        
        void tbGroupeNumberCompetitorKeyPress(object sender, KeyPressEventArgs e)
        {
            if (nonNumberEntered == true)
            {
                e.Handled = true;
            }
        }

    }
    
    
    
    
    class getNominal
    {
        public cash[] cashStruct;
        
        public getNominal (string nominalLine)
        {
            
            string[] nominalArrLevel1 = nominalLine.Split(';');
            
            cashStruct = new cash[nominalArrLevel1.Length];
            
            for (int i = 0; i < nominalArrLevel1.Length; i ++)
            {
                string[] nominalArrLevel2 = nominalArrLevel1[i].Split(':');
                
                for (int j = 0; j < nominalArrLevel2.Length; j++)
                {
                    if (j == 0) {cashStruct[i].Name = nominalArrLevel2[j];}
                    else if (j == 1) {cashStruct[i].Coef = Convert.ToDecimal(nominalArrLevel2[j]);}
                    else if (j == 2)
                    {
                        string[] nominalArrLevel3 = nominalArrLevel2[j].Split(',');
                        Int32[] nominalArrLevel3Int = new Int32[nominalArrLevel3.Length];
                        
                        for (int k = 0; k < nominalArrLevel3.Length; k ++)
                        {
                            nominalArrLevel3Int[k] = Convert.ToInt32(nominalArrLevel3[k]);
                        }
                        
                        cashStruct[i].Nominal = nominalArrLevel3Int;
                    }
                }
            }
        }
    }
    
    
    public struct cash
    {
        public string Name;
        public decimal Coef;
        public int[] Nominal;
    }

    
    

