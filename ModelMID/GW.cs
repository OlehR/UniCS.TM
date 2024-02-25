using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelMID
{
	public class GW
	{
		public GW(FastGroup pFG)
		{
			Type = 1;
			Name = pFG.Name;
			Code = pFG.CodeFastGroup;
            Image = pFG.Image;
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
		public string Image { get; set; }
		public string Pictures { get {
				string Pictures= Path.Combine(Global.PathPictures, (Type == 1 ? "Categories" : "Products"), string.IsNullOrEmpty(Image)? $"{Code.ToString("D9")}" : Image);
				if (File.Exists(Pictures + ".png"))
					return Pictures + ".png";
				else
                    return Pictures + ".jpg";
                //return Path.Combine(Global.PathPictures, (Type == 1 ? "Categories" : "Products"), $"{Code.ToString("D9")}.jpg"); 
			} }
		public bool IsWeight { get{ return Type == 0 && CodeUnit == Global.WeightCodeUnit; } }
	}
}
