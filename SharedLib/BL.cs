using ModelMID;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLib
{
    
    public class BL
    {
        private WDB db;
        public BL()
        {
            db = new WDB_SQLite();
        }
        public decimal AddReceiptWares(ReceiptWares parW)
        {       
            var Quantity = db.GetCountWares(parW);

             parW.Quantity+= Quantity;
            
            if (parW.Quantity > 0)
                db.UpdateQuantityWares(parW);
            else
                db.AddWares(parW);

            if (GlobalVar.RecalcPriceOnLine)
                db.RecalcPrice((IdReceipt)parW);

            return parW.Quantity;
        }
    }
}
