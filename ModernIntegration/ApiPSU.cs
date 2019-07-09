using System;
using System.Collections.Generic;
using System.Text;
using ModernIntegration.Models;
using ModernIntegration.ViewModels;
using ModernIntegration.Enums;
using SharedLib;
using ModelMID;
using System.Linq;
using Receipt = ModernIntegration.Models.Receipt;
using ModelMID.DB;

namespace ModernIntegration
{
    public class ApiPSU : Api
    {
        BL Bl;
        Dictionary<Guid, ModelMID.IdReceipt> Receipts = new Dictionary<Guid, ModelMID.IdReceipt>();
        public ApiPSU()
        {
            Bl = new BL();
        }
        public override ProductViewModel AddProductByBarCode(Guid parTerminalId, string parBarCode)
        {
            var CurReceipt = GetCurrentReceiptByTerminalId(parTerminalId);
            var RW = Bl.AddWaresBarCode(CurReceipt, parBarCode);
            return GetProductViewModel(RW);
        }
        public override ProductViewModel AddProductByProductId(Guid parTerminalId, Guid parProductId, decimal parQuantity = 0)
        {
            var CurReceipt = GetCurrentReceiptByTerminalId(parTerminalId);
            var g = CurReceipt.ReceiptId;
            Bl.AddWaresCode(CurReceipt, parProductId, parQuantity);
            ProductViewModel Res = null;
            return Res;
        }
        public override ReceiptViewModel ChangeQuantity(Guid parTerminalId, Guid parProductId, decimal parQuantity)
        {
            var CurReceipt = GetCurrentReceiptByTerminalId(parTerminalId);
            var CurReceiptWares = new IdReceiptWares(CurReceipt, parProductId);

            Bl.ChangeQuantity(CurReceiptWares, parQuantity);
            ReceiptViewModel Res = GetReceiptViewModel(CurReceipt);
            return Res;
        }
        public override ReceiptViewModel GetReciept(Guid parReceipt)
        {
            //var Receipt = new GetCurrentReceiptByTerminalId();
            var Res = GetReceiptViewModel(new IdReceipt(parReceipt));
            return Res;
        }
        public override bool AddPayment(Guid parReceiptId, ReceiptPayment[] parPayment) { return false; }
        public override bool AddFiscalNumber(Guid parReceiptId, string parFiscalNumber)
        {
            var receiptId = new IdReceipt(parReceiptId);
            Bl.UpdateReceiptFiscalNumber(receiptId, parFiscalNumber);
            ClearReceipt(parReceiptId);
            return true;
        }

        public override bool ClearReceipt(Guid parTerminalId)
        {
            Receipts[parTerminalId] = null;
            return true;
        }

        public override List<ProductViewModel> GetBags() { return null; }

        public override List<ProductCategory> GetAllCategories(Guid parTerminalId)
        {
            var Res = new List<ProductCategory>();
            var ct = 9;//TMP!!! Треба брати з налаштувань.
            var wr = Bl.db.GetFastGroup(ct);
            if (wr != null)
                foreach (var el in wr)
                    Res.Add(GetProductCategory(el));
            return Res;
        }
        public override List<ProductCategory> GetCategoriesByParentId(Guid parTerminalId, Guid categoryId)
        {
            return null;

        }
        public override List<ProductViewModel> GetProductsByCategoryId(Guid parTerminalId, Guid categoryId)
        {
            var Res = new List<ProductViewModel>();
            var ct = new FastGroup { FastGroupId = categoryId };
            var wr = Bl.db.GetWaresFromFastGroup(ct.CodeFastGroup);
            if (wr != null)
                foreach (var el in wr)
                    Res.Add(GetProductViewModel(el));
            return Res;
        }
        public override List<ProductViewModel> GetProductsByName(string parName)
        {
            var Res = new List<ProductViewModel>();
            var wr = Bl.GetProductsByName(parName);
            if (wr != null)
                foreach (var el in wr)
                    Res.Add(GetProductViewModel(el));
            return Res;
        }

        public override bool UpdateReceipt(ReceiptViewModel parReceipt) { return false; }
        public override TypeSend SendReceipt(Guid parReceipt) { return TypeSend.NotReady; }
        public override TypeSend GetStatusReceipt(Guid parReceipt) { return TypeSend.NotReady; }

        public override CustomerViewModel GetCustomerByBarCode(Guid parTerminalId, string parS)
        {
            var CM = Bl.GetClientByBarCode(parS);
            return GetCustomerViewModelByClient(CM);
        }
        public override CustomerViewModel GetCustomerByPhone(Guid parTerminalId, string parPhone)
        {
            var CM = Bl.GetClientByPhone(parPhone);
            return GetCustomerViewModelByClient(CM);
        }


        // Допоміжні методи
        public bool ClearReceiptByReceiptId(IdReceipt idReceipt)
        {

            foreach (var el in Receipts)
            {
                if (el.Equals(idReceipt))
                    Receipts[el.Key] = null;
            }
            return true;
        }

        private ModelMID.IdReceipt GetCurrentReceiptByTerminalId(Guid parTerminalId)
        {
            if (!Receipts.ContainsKey(parTerminalId))
            {
                var idReceipt = Bl.GetNewIdReceipt(parTerminalId);
                Receipts[parTerminalId] = new ModelMID.Receipt(idReceipt);
                Bl.AddReceipt(Receipts[parTerminalId]);
            }
            return Receipts[parTerminalId];

        }
        /// <summary>
        /// Convert MID.ReceiptWares->ProductViewModel
        /// </summary>
        /// <param name="receiptWares"></param>
        /// <returns></returns>
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
                ProductWeightType = receiptWares.IsWeight ? ProductWeightType.ByWeight : ProductWeightType.ByPiece,//!!!TMP
                IsAgeRestrictedConfirmed = false,//!!!TMP //Обмеження по віку алкоголь Підтверджено
                Quantity = receiptWares.Quantity,
                DiscountValue = receiptWares.SumDiscount,
                DiscountName = "",
                WarningType = null,//!!!TMP 
                CalculatedWeight = 0,
                Tags = null,//!!!TMP // Різні мітки алкоголь, обмеження по часу. 
                HasSecurityMark = false,//!!!TMP // Магнітна мітка, яку треба знімати.
                TotalRows = receiptWares.Sort, //Сортування популярного.
                WeightCategory = 0,//!!!TMP
                IsProductOnProcessing = false//!!!TMP  Останній робочий продукт.
                ///CategoryId=   !!!TMP


            };
            return Res;
        }

        private ReceiptViewModel GetReceiptViewModel(IdReceipt parReceipt)
        {
            var receiptMID = Bl.GetReceiptHead(parReceipt);

            var receipt = new Receipt()
            {
                Id = receiptMID.ReceiptId,
                FiscalNumber = receiptMID.NumberReceipt,
                Status = (receiptMID.SumCash > 0 || receiptMID.SumCreditCard > 0 ? ReceiptStatusType.Paid : ReceiptStatusType.Created),//!!!TMP Треба врахувати повернення
                TerminalId = receiptMID.TerminalId,
                Amount = 0,//!!!TMP
                Discount = receiptMID.SumDiscount,
                TotalAmount = 0,//!!!TMP
                CustomerId = new Client(receiptMID.CodeClient).ClientId,
                CreatedAt = receiptMID.DateCreate,
                UpdatedAt = receiptMID.DateCreate//!!!TMP

                //PaymentType= PaymentType.None,//!!!TMP
                //PaidAmount=0,//
                //ReceiptItems=
                //Customer
                //PaymentInfo

            };
            var listReceiptItem = GetReceiptItem((IdReceipt)parReceipt);
            var Res = new ReceiptViewModel(receipt, listReceiptItem, null, null);

            return Res;

        }

        private List<ReceiptItem> GetReceiptItem(IdReceipt parIdReceipt)
        {
            var Res = new List<ReceiptItem>();
            var res = Bl.ViewReceiptWares(parIdReceipt);//new ModelMID.IdReceipt { CodePeriod = 20190613, CodeReceipt = 1, IdWorkplace = 140701 }
            foreach (var el in res)
            {
                var PVM = this.GetProductViewModel(el);
                Res.Add(PVM.ToReceiptItem());
            }
            return Res;
        }
        private CustomerViewModel GetCustomerViewModelByClient(Client parClient)
        {
            if (parClient == null)
                return null;
            return new CustomerViewModel()
            {
                Id = parClient.ClientId,
                CustomerId = parClient.CodeClient.ToString(),
                Name = parClient.NameClient,
                DiscountPercent = Convert.ToDouble(parClient.PersentDiscount),
                LoyaltyPoints = Convert.ToDouble(parClient.SumBonus),
                LoyaltyPointsTotal = Convert.ToDouble(parClient.SumMoneyBonus)
            };
        }


        private ProductCategory GetProductCategory(FastGroup parFG)
        {
            if (parFG == null)
                return null;
            var Parrent = new FastGroup { CodeFastGroup = parFG.CodeUp };

            return new ProductCategory
            {
                Id = parFG.FastGroupId,
                ParentId = Parrent.FastGroupId,
                Name = parFG.Name,
                Language = null,
                CustomId = null,
                Description = parFG.Name,
                Image = "",
                HasChildren = false,
                HasProducts = true,
                Tags = null
            };

        }

    }
}
