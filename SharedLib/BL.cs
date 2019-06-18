using ModelMID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedLib
{

    public class BL
    {
        private WDB_SQLite db;
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

        public bool AddReceipt(IdReceipt parReceipt)
        {
            var receipt = new Receipt()
            {
                IdWorkplace = parReceipt.IdWorkplace,
                CodePeriod = parReceipt.CodePeriod,
                CodeReceipt = parReceipt.CodeReceipt
            };

            return db.AddReceipt(receipt);
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

            var receipt = new Receipt(receiptId);
            receipt.NumberReceipt = parFiscalNumber;
            receipt.StateReceipt = 2;
            db.CloseReceipt(receipt);
            return true;
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

        public IEnumerable<ReceiptWares>  ViewReceiptWares(IdReceipt parIdReceipt)
        {
           var Res= db.ViewReceiptWares(parIdReceipt);
            //var El = Res.First();
            return Res;

        }
        public bool ChangeQuantity(IdReceiptWares parReceiptWaresId, decimal  parQuantity)
        {
            db.ClearT1();
            db.InsertT1(new T1 { Id = parReceiptWaresId.CodeWares, Data = parReceiptWaresId.CodeUnit });
            var w = db.FindWares().First();
            w.SetIdReceiptWares(parReceiptWaresId);
            w.Quantity = parQuantity;
            db.UpdateQuantityWares(w);
            return true;
            
        }
        public Receipt GetReceiptHead(IdReceipt idReceipt)
        {
            return db.ViewReceipt(idReceipt);
        }
    }
}
