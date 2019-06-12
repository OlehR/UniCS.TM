using ModelMID;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public ReceiptWares AddReceiptWares(ReceiptWares parW)
        {
            var Quantity = db.GetCountWares(parW);

            parW.Quantity += Quantity;

            if (Quantity > 0)
                db.UpdateQuantityWares(parW);
            else
                db.AddWares(parW);

            if (GlobalVar.RecalcPriceOnLine)
                db.RecalcPrice((IdReceipt)parW);

            return parW;

        }
        public bool AddReceipt(Receipt parReceipt)
        {
            return db.AddReceipt(parReceipt);
        }

        public int GetIdWorkplaceByTerminalId(Guid parTerminalId)
        {
            //!!!TMP Треба доробляти!!!
            return 140701;
        }
        public IdReceipt GetNewIdReceipt(Guid parTerminalId,int parCodePeriod=0)
        {            
            var idReceip = new IdReceipt() { IdWorkplace = GetIdWorkplaceByTerminalId(parTerminalId),CodePeriod= parCodePeriod };
            return db.GetNewCodeReceipt(idReceip);            
        }

        public bool UpdateReceiptFiscalNumber(IdReceipt receiptId,string parFiscalNumber)
        {
            return false;
        }
        public IdReceipt GetIdReceiptByReceiptId(Guid parReceiptId)
        {
            return null;
        }
        public ReceiptWares AddWaresBarCode(IdReceipt parReceipt, string parBarCode)
        {

            var r = db.FindData(parBarCode, TypeFind.Wares);
            if (r.Count == 1)
            {
                var w = db.FindWares().First();
                w.SetIdReceipt(parReceipt);
                w.Quantity = 1;

                //db.
                return AddReceiptWares(w);
                //Res = new ProductViewModel() {Id=w. };
            }
            else
                return null;
            //db.AddReceipt
        }
        public ReceiptWares AddWaresCode(IdReceipt parReceipt, Guid parProductId, decimal parQuantity = 0)
        {

            int CodeWares = 0;
            if (int.TryParse(parProductId.ToString().Substring(24), out CodeWares))
            {
                db.ClearT1();
                db.InsertT1(new T1 { Id = CodeWares, Data = 0 });
                var w = db.FindWares().First();
                w.SetIdReceipt(parReceipt);
                w.Quantity = parQuantity;
                return AddReceiptWares(w);
            }
            return null;
            //Res = new ProductViewModel() {Id=w. };         
        }

    }
}
