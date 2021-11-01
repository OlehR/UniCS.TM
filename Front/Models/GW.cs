using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Models
{
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
			TotalRows = pFG.TotalRows;
			CodeUnit = pFG.CodeUnit;
		}
		public int Type { get; set; }
		public string Name { get; set; }
		public int Code { get; set; }
		//public string Pictures { get; set; }
		public int TotalRows { get; set; }
		public int CodeUnit { get; set; }

		public string GetName { get { return (Type == 1 ? "G" : "W") + Code.ToString(); } }
		public string Pictures { get { return $"D:\\Pictures\\{(Type == 1 ? "Categories" : "Products")}\\{Code.ToString("D9")}.jpg"; } }
	}
}
