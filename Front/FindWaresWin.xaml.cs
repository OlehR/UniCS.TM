using ModelMID.DB;
using ModelMID;
using SharedLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Front
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class FindWaresWin : Window
	{
		BL Bl;
		int CodeFastGroup=0;
		public FindWaresWin()
		{
			InitializeComponent();
			WindowState = WindowState.Maximized;
			//WindowStyle = WindowStyle.None;
			Bl = BL.GetBL;

			OnScreenKeyboardControl.Keyboard.OnScreenKeyboard bb = new OnScreenKeyboardControl.Keyboard.OnScreenKeyboard();
			NewB();
		}

		void NewB()
        {
			IEnumerable<GW> WG = null;
			if (CodeFastGroup == 0 && WaresName.Text.Length==0)
			{
				WG = Bl.db.GetFastGroup(ModelMID.Global.CodeWarehouse).Select(r => new GW(r));
			}
			else
			{
				WG = Bl.GetProductsByName(null, WaresName.Text, -1, 10, CodeFastGroup).Select(r => new GW(r)); ;
			}
			BildButtom(WG);

		}
		void BildButtom(IEnumerable<GW> pGW)
        {
			//Wr = Wrx;
			int i = 0;
			int j = 0;
			PictureGrid.Children.Clear();
			foreach (var el in pGW)
			{
				var Bt = new Button();
				Bt.Name = el.GetName;// $"BtGr{el.Code}";				
				if (File.Exists(el.Pictures))
					Bt.Content = new Image
					{
						Source = new BitmapImage(new Uri(el.Pictures)),
						VerticalAlignment = VerticalAlignment.Center
					};
				else
					Bt.Content = el.Name;
				Bt.Click += BtClick;
				Grid.SetColumn(Bt, i);
				Grid.SetRow(Bt, j);
				PictureGrid.Children.Add(Bt);
				if (++i >= 5)
				{ j++; i = 0; }
			}
		}

		private void BtClick(object sender, RoutedEventArgs e)
		{
			Button aa = (Button) sender;
			int Code= Convert.ToInt32(aa.Name.Substring(1));
			if (aa.Name.Substring(0,1).Equals("G"))
            {
				CodeFastGroup = Code;
				NewB();
			}
            else 
			{
				Close(Code);
			}	
		}

		private void Close(int pCodeWares)
        {
			if(pCodeWares>0)
            {
				Bl.AddWaresCode(pCodeWares, 0, 1);
            }
			Close();
		}
		
	}

	public class GW
	{
		public GW(FastGroup pFG)
		{
			
			Type = 1;
			Name = pFG.Name;
			Code = pFG.CodeFastGroup;
			//Pictures = $"{PathPicture}0000{Code}.jpg";
		}
		public GW(ReceiptWares pFG)
		{
			
			Type = 0;
			Name = pFG.NameWares;
			Code = pFG.CodeWares;			
		}
		int Type { get; set; }
		public string Name { get; set; }
		public int Code { get; set; }
		//public string Pictures { get; set; }
		public string GetName { get { return (Type == 1 ? "G" : "W") + Code.ToString(); } }
		public string Pictures { get { return $"D:\\Pictures\\{(Type == 1 ? "Categories" : "Products")}\\{Code.ToString("D9")}.jpg"; } }
	}
}
