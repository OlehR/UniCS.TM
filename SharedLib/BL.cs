using ModelMID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedLib
{

    public class BL
    {
        public WDB_SQLite db;

        public Action<IEnumerable<ReceiptWares>,Guid> OnReceiptCalculationComplete { get; set; }

        /// <summary>
        /// Для швидкого пошуку 
        /// </summary>
        SortedList<Guid, int> WorkId;
        public BL()
        {
            db = new WDB_SQLite();
            WorkId = new SortedList<Guid, int>();
            db.OnReceiptCalculationComplete = (wareses, guid) => OnReceiptCalculationComplete?.Invoke(wareses, guid);
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
               db.RecalcPriceAsync(parW);
            return parW;
        }

        public bool AddReceipt(IdReceipt parReceipt)
        {
            var receipt = new Receipt(parReceipt);
            return db.AddReceipt(receipt);
        }
        public bool AddReceipt(Receipt parReceipt)
        {
            return db.AddReceipt(parReceipt);
        }
        
        public int GetIdWorkplaceByTerminalId(Guid parTerminalId)
        {
            if (WorkId.ContainsKey(parTerminalId))
                return WorkId[parTerminalId];
            else
            {
                int WI = db.GetIdWorkplaceByTerminalId(parTerminalId.ToString());
                if(WI>0)
                {
                    WorkId.Add(parTerminalId, WI);
                    return WI;
                }
            }
            //!!!TMP Треба доробляти!!!
            return 0901;
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
           db.RecalcPriceAsync(receiptId);
            db.CloseReceipt(receipt);
            return true;
        }
        
        public ReceiptWares AddWaresBarCode(IdReceipt parReceipt, string parBarCode, decimal parQuantity = 0)
        {
            var w = db.FindWares(parBarCode);
            if (w.Count() == 1)
            {
                var W= w.First();
                if (parQuantity == 0)
                    return W;
                W.SetIdReceipt(parReceipt);
                W.Quantity = parQuantity;
                return AddReceiptWares(W);
            }
            else
                return null;

        }
        public ReceiptWares AddWaresCode(IdReceipt parReceipt, Guid parProductId, decimal parQuantity = 0)
        {
            int CodeWares = 0;
            if (int.TryParse(parProductId.ToString().Substring(24), out CodeWares))
            {
                var WId = new IdReceiptWares { WaresId = parProductId };

                var w = db.FindWares(null, null, WId.CodeWares, WId.CodeUnit);
                if (w.Count() == 1)
                {
                    var W = w.First();
                    if (parQuantity == 0)
                        return W;
                    W.SetIdReceipt(parReceipt);
                    W.Quantity = parQuantity;
                    return AddReceiptWares(W);
                }
            }
            return null;
        }

        public IEnumerable<ReceiptWares>  ViewReceiptWares(IdReceipt parIdReceipt)
        {
           var Res= db.ViewReceiptWares(parIdReceipt);
            //var El = Res.First();
            return Res;

        }
        public bool ChangeQuantity(IdReceiptWares parReceiptWaresId, decimal  parQuantity)
        {
            var W = db.FindWares(null, null, parReceiptWaresId.CodeWares, parReceiptWaresId.CodeUnit);
            if (W.Count() == 1)
            {
                var w = W.First();
                w.SetIdReceiptWares(parReceiptWaresId);
                w.Quantity = parQuantity;
                return db.UpdateQuantityWares(w);                
            }
            return false;
            
        }
        public Receipt GetReceiptHead(IdReceipt idReceipt)
        {
            return db.ViewReceipt(idReceipt);
        }

        public Client GetClientByBarCode(IdReceipt idReceipt,string parBarCode)
        {
            var r = db.FindClient(parBarCode);
            if (r.Count() == 1)
            {
                var client = r.First();
                UpdateClientInReceipt(idReceipt, client);
                return client;
            }
            return null;
        }

        public Client GetClientByPhone(IdReceipt idReceipt, string parPhone)
        {
            var r = db.FindClient(null, parPhone);
            if (r.Count() == 1)
            {
                var client = r.First();
                UpdateClientInReceipt(idReceipt, client);
                return client;
            }
            return null;


        }

        private void  UpdateClientInReceipt(IdReceipt idReceipt, Client parClient)
        {
            var RH = GetReceiptHead(idReceipt);
            RH.CodeClient = parClient.CodeClient;
            RH.PercentDiscount = parClient.PersentDiscount;
            db.ReplaceReceipt(RH);
        }

        
        public IEnumerable<ReceiptWares> GetProductsByName( string parName)
        {
            var r = db.FindWares(null,parName);
            if (r.Count() >0)
            {
                return r;
            }
            else
                return null;
        }

 

}
}
