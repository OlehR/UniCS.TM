using System;
using System.Collections.Generic;
using System.Text;
using ModernExpo.SelfCheckout.Entities.Models;
using ModernExpo.SelfCheckout.Entities.ViewModels;
using ModernExpo.SelfCheckout.Entities.Enums;
using SharedLib;
using ModelMID;
using System.Linq;

namespace ModernIntegration
{
    public class ApiPSU:Api
    {
        BL Bl;
        Dictionary<Guid, ModelMID.Receipt> Receipts = new Dictionary <Guid,ModelMID.Receipt>();
        public ApiPSU()
        {
            Bl = new BL();
        }
        private ModelMID.Receipt GetCurrentReceiptByTerminalId(Guid parTerminalId)
        {
            if (!Receipts.ContainsKey(parTerminalId))
            {
                var idReceipt = Bl.GetNewIdReceipt(parTerminalId);
                Receipts[parTerminalId] = new ModelMID.Receipt(idReceipt);
                Bl.AddReceipt(Receipts[parTerminalId]);
            }
            return Receipts[parTerminalId];

        }

        private ProductViewModel GetProductViewModel(ReceiptWares receiptWares)
        {
            var Res = new ProductViewModel()
            {
                Id = receiptWares.WaresId,
                Code = receiptWares.CodeWares,
                Name = receiptWares.NameWares,
                AdditionalDescription = receiptWares.NameWaresReceipt,//!!!TMP;
                Image = null,
                Price = receiptWares.Price,
                Weight = 0,//!!!TMP
                DeltaWeight = 0,//!!!TMP
                ProductWeightType = ProductWeightType.ByWeight,//!!!TMP
                IsAgeRestrictedConfirmed = false,//!!!TMP
                Quantity = receiptWares.Quantity,
                DiscountValue = receiptWares.SumDiscount,
                DiscountName = "",
                WarningType = null,//!!!TMP
                CalculatedWeight = 0,
                Tags=null,//!!!TMP
                HasSecurityMark=false,//!!!TMP
                TotalRows= receiptWares.Sort,
                WeightCategory=0,//!!!TMP
                IsProductOnProcessing=false//!!!TMP
                ///CategoryId=   !!!TMP


            };
            return Res;
        }
        public override ProductViewModel AddProductByBarCode(Guid parTerminalId, string parBarCode)
        {      
            var CurReceipt = GetCurrentReceiptByTerminalId(parTerminalId);            
            var RW=Bl.AddWaresBarCode(CurReceipt, parBarCode);
            return GetProductViewModel(RW);
        }
        public override ProductViewModel AddProductByProductId(Guid parTerminalId, Guid parProductId, decimal parQuantity = 0)
        {
            var CurReceipt = GetCurrentReceiptByTerminalId(parTerminalId);
            Bl.AddWaresCode(CurReceipt, parProductId, parQuantity);
            ProductViewModel Res = null;
            return Res;
        }
        public override ReceiptViewModel ChangeQuanity(Guid parTerminalId, Guid parProductId, decimal parQuantity) { return null; }
        public override ReceiptViewModel GetReciept(Guid parReceipt) { return null; }
        public override bool AddPayment(Guid parTerminalId, Guid parReceiptId, ReceiptPayment[] parPayment) { return false; }
        public override bool AddFiscalNumber(Guid parReceiptId, string parFiscalNumber)
        {
            var receiptId = Bl.GetIdReceiptByReceiptId(parReceiptId);
            Bl.UpdateReceiptFiscalNumber(receiptId, parFiscalNumber);
            //ClearReceipt(parTerminalId);
            return true;
        }
        public override bool ClearReceipt(Guid parTerminalId)
        {
            Receipts[parTerminalId] = null;
            return true;
        }

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
