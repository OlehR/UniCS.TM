using System;
using System.Data;

namespace MID
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	public class WDB_ORACLE:WDB
	{
		public WDB_ORACLE()
		{
			varVersion="Oracle.0.0.1";
		}
		public override RezultFind FindData( string parStr,TypeFind parTypeFind = TypeFind.All)
		{
		 	RezultFind varRez;
			varRez.TypeFind = TypeFind.All;
			varRez.Count=0;
			return varRez;
		}

		// Повертає знайденого клієнта(клієнтів)
		public override System.Data.DataTable FindClient()
		{
		 return null;
		}
		
/*		public override bool  AddWares(System.Data.DataRow parRow )
		{
		 return true;
		}*/
		
		public override bool  RecalcPrice(int parCodeReceipt =0)
		{
		 return true;
		}

		public override bool  InputOutputMoney(decimal parMany)
		{
		 return true;
		}
		
		public override bool  AddZ(System.Data.DataRow parRow )
		{
		 return true;
		}
		
		public override bool  AddLog(System.Data.DataRow parRow )
		{
		 return true;
		}

	}
}
