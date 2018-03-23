/*
 * Created by SharpDevelop.
 * User: gelo
 * Date: 09.10.2013
 * Time: 9:45
  */
using System;
using System.Windows.Forms;

namespace MID
{
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	internal sealed class Program
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			if(args.Length>=1)
				GlobalVar.varPathIni=args[0];
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new NET_MainForm());
		}
		
	}
}
