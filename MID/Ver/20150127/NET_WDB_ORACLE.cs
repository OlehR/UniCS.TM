using System;
using System.Data;

namespace MID
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	public class NET_WDB_ORACLE:WDB
	{
		public NET_WDB_ORACLE()
		{
			varVersion="Oracle.0.0.1";
		}
		public override int FindData( string parStr,int parTypeFind = 0)
		{
		 return 0;
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
