using ModelMID.DB;
using SharedLib;
using System.Collections.Generic;
using System.Windows;

namespace Front
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class FindWaresWin : Window
	{
		BL Bl;
		IEnumerable<FastGroup> Wr;

		public FindWaresWin()
		{
			InitializeComponent();
			WindowState = WindowState.Maximized;
			//WindowStyle = WindowStyle.None;
			Bl = BL.GetBL;

			OnScreenKeyboardControl.Keyboard.OnScreenKeyboard bb = new OnScreenKeyboardControl.Keyboard.OnScreenKeyboard();

			var ct = 9;//TMP!!! Треба брати з налаштувань.
			 Wr = Bl.db.GetFastGroup(ct);
		}
	}
}
