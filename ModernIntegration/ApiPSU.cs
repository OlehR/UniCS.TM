using System;
using System.Collections.Generic;
using System.Text;
using ModernExpo.SelfCheckout.Entities.Models;
using ModernExpo.SelfCheckout.Entities.ViewModels;
using SharedLib;
using ModelMID;
using System.Linq;

namespace ModernIntegration
{
    public class ApiPSU:Api
    {
        WDB db;
        Dictionary<Guid, ModelMID.Receipt> Recepts = new Dictionary <Guid,ModelMID.Receipt>();
        public ApiPSU()
        {
            db = new WDB_SQLite();
        }
        public override ProductViewModel AddProductByBarCode(Guid parTerminalId, string parBarCode)
        {
            if (Recepts[parTerminalId] == null)
            {
                Recepts[parTerminalId] = new ModelMID.Receipt();
                db.AddReceipt(Recepts[parTerminalId]);
            }
            var CurReceipt = Recepts[parTerminalId];
            ProductViewModel Res = null;

            var r = db.FindData(parBarCode, TypeFind.Wares);
            if(r.Count==1)
            {
                var w = db.FindWares().First();
                //db.
                db.AddWares(w);
                //Res = new ProductViewModel() {Id=w. };
            }
            return Res;
        }
        public override ProductViewModel AddProductByProductId(Guid parTerminalId, Guid paparProductId, decimal parQuantity = 0) { return null; }
        public override ReceiptViewModel ChangeQuanity(Guid parTerminalId, Guid parProductId, decimal parQuantity) { return null; }
        public override ReceiptViewModel GetReciept(Guid parReceipt) { return null; }
        public override bool AddPayment(Guid parTerminalId, Guid parReceiptId, ReceiptPayment[] parPayment) { return false; }
        public override bool AddFiscalNumber(Guid parReceiptId, string parFiscalNumber) { return false; }
        public override bool ClearReceipt(Guid parTerminalId) { return false; }

        public override List<ProductViewModel> GetBags() { return null; }

        public override List<ProductCategory> GetAllCategories() { return null; }
        public override List<ProductCategory> GetCategoriesByParentId(Guid categoryId) { return null; }
        public override List<ProductViewModel> GetProductsByCategoryId(Guid categoryId) { return null; }

        public override bool UpdateReceipt(ReceiptViewModel parReceipt) { return false; }
        public override TypeSend SendReceipt(Guid parReceipt) { return TypeSend.NotReady; }
        public override TypeSend GetStatusReceipt(Guid parReceipt) { return TypeSend.NotReady; }

        public override CustomerViewModel GetCustomerByBarCode(string parS) { return null; }
        public override CustomerViewModel GetCustomerByPhone(string parS) { return null; }

    }
}
