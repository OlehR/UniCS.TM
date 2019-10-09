using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ModernIntegration.Models;
using ModernIntegration.ViewModels;
using ModernIntegration.Enums;
using SharedLib;
using ModelMID;
using System.Linq;
using Receipt = ModernIntegration.Models.Receipt;
using ModelMID.DB;
using ModernIntegration.Model;

namespace ModernIntegration
{
    public class ApiPSU : Api
    {
        public BL Bl;
        Dictionary<Guid, ModelMID.IdReceipt> Receipts = new Dictionary<Guid, ModelMID.IdReceipt>();
        public ApiPSU()
        {
            Bl = new BL();
            Bl.OnReceiptCalculationComplete = (wareses, guid) =>
            {
                Debug.WriteLine("=========================================================================");
                foreach (var receiptWarese in wareses)
                {
                    Debug.WriteLine($"{receiptWarese.NameWares} - {receiptWarese.Price}");
                }
                OnProductsChanged?.Invoke(wareses.Select(s => GetProductViewModel(s)), guid);
            };
        }
        public override ProductViewModel AddProductByBarCode(Guid parTerminalId, string parBarCode, decimal parQuantity = 0)
        {
            var CurReceipt = GetCurrentReceiptByTerminalId(parTerminalId);
            var RW = Bl.AddWaresBarCode(CurReceipt, parBarCode, parQuantity);
            return GetProductViewModel(RW);
        }
        public override ProductViewModel AddProductByProductId(Guid parTerminalId, Guid parProductId, decimal parQuantity = 0)
        {
            var CurReceipt = GetCurrentReceiptByTerminalId(parTerminalId);
            var g = CurReceipt.ReceiptId;
            Bl.AddWaresCode(CurReceipt, parProductId, parQuantity);
            ProductViewModel Res = null;
            //TODO: OnReceiptChanged?.Invoke(receipt,terminalId);
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
        public override bool AddPayment(Guid parReceiptId, ReceiptPayment[] parPayment)
        {
            return Bl.db.ReplacePayment(parPayment.Select(r=> ReceiptPaymentToPayment(r)));
        }
        public override bool AddFiscalNumber(Guid parReceiptId, string parFiscalNumber)
        {
            var receiptId = new IdReceipt(parReceiptId);
            Bl.UpdateReceiptFiscalNumber(receiptId, parFiscalNumber);
            ClearReceiptByReceiptId(receiptId);
//          ClearReceipt(parReceiptId);
            return true;
        }

        public override bool ClearReceipt(Guid parTerminalId)
        {
            Receipts[parTerminalId] = null;
            return true;
        }

        //public override List<ProductViewModel> GetBags() { return null; }

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
            throw new NotImplementedException();
           // return null;

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

        public override bool UpdateReceipt(ReceiptViewModel parReceipt)
        {
            throw new NotImplementedException();
            //return false;
        }
        public override TypeSend SendReceipt(Guid parReceipt)
        {

            Bl.SendReceiptTo1C(new IdReceipt(parReceipt));
            //throw new NotImplementedException();
            return TypeSend.NotReady;
        }
        public override TypeSend GetStatusReceipt(Guid parReceipt) {
            throw new NotImplementedException();
            //return TypeSend.NotReady; 
        }

        public override CustomerViewModel GetCustomerByBarCode(Guid parTerminalId, string parS)
        {
            var CM = Bl.GetClientByBarCode(GetCurrentReceiptByTerminalId(parTerminalId),parS);            
            return GetCustomerViewModelByClient(CM);
        }
        public override CustomerViewModel GetCustomerByPhone(Guid parTerminalId, string parPhone)
        {
            var CM = Bl.GetClientByPhone(GetCurrentReceiptByTerminalId(parTerminalId), parPhone);
            return GetCustomerViewModelByClient(CM);
        }


        // Допоміжні методи
        /// <summary>
        /// Clears the receipt by receipt identifier.
        /// </summary>
        /// <param name="idReceipt">The identifier receipt.</param>
        /// <returns></returns>
        public bool ClearReceiptByReceiptId(IdReceipt idReceipt)
        {

            foreach (var el in Receipts)
            {
                if (el.Value.Equals(idReceipt))
                {
                    Receipts[el.Key] = null;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the current receipt by terminal identifier.
        /// </summary>
        /// <param name="parTerminalId">The par terminal identifier.</param>
        /// <returns></returns>
        public ModelMID.IdReceipt GetCurrentReceiptByTerminalId(Guid parTerminalId)
        {

            if (!Receipts.ContainsKey(parTerminalId)|| Receipts[parTerminalId] == null)
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
                AdditionalDescription = receiptWares.NameWaresReceipt, //!!!TMP;
                Image = null,
                Price = receiptWares.Price > 0 ? receiptWares.Price : receiptWares.PriceDealer,
                Weight = 0, //!!!TMP
                DeltaWeight = 0, //!!!TMP
                ProductWeightType =
                    receiptWares.IsWeight ? ProductWeightType.ByWeight : ProductWeightType.ByPiece, //!!!TMP
                IsAgeRestrictedConfirmed =
                    false, //!!!TMP //Обмеження по віку алкоголь Підтверджено не потрібно посилати.
                Quantity = receiptWares.Quantity,
                DiscountValue = receiptWares.SumDiscount,
                DiscountName = "",
                WarningType = null, //!!! Не посилати 
                CalculatedWeight = 0,
                Tags = (receiptWares.TypeWares > 0
                    ? new List<Tag>()
                        {new Tag() {Key = "AgeRestricted", Id = 0}, new Tag() {Key = "TimeRestricted", Id = 1}}
                    : null), //!!!TMP // Різні мітки алкоголь, обмеження по часу. 
                HasSecurityMark = false, //!!!TMP // Магнітна мітка, яку треба знімати.
                TotalRows = receiptWares.Sort, //Сортування популярного.
                WeightCategory = 1, //вимірювання Похибки в відсотках,2 в грамах
                IsProductOnProcessing = false, //
                ///CategoryId=   !!!TMP Групи 1 рівня.
                TaxGroup = Global.GetTaxGroup(receiptWares.TypeVat),
            };
            return Res;
        }

        private ReceiptWares GetReceiptWares(ProductViewModel varPWM)
        {
            var Res = new ReceiptWares()
            {
                WaresId = varPWM.Id,
                CodeWares = varPWM.Code,
                NameWares = varPWM.Name,
                NameWaresReceipt = varPWM.AdditionalDescription, //!!!TMP;
                Price = varPWM.Price,
                Quantity = varPWM.Quantity,
                SumDiscount = varPWM.DiscountValue,
                Sort = varPWM.TotalRows //Сортування популярного.
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
                Status = (receiptMID.SumCash > 0 || receiptMID.SumCreditCard > 0
                    ? ReceiptStatusType.Paid
                    : ReceiptStatusType.Created), //!!!TMP Треба врахувати повернення
                TerminalId = receiptMID.TerminalId,
                Amount = 0, //!!!TMP Сума чека.
                Discount = receiptMID.SumDiscount,
                TotalAmount = 0, //!!!TMP з врахуванням знижки (знижка на чек.)
                CustomerId = new Client(receiptMID.CodeClient).ClientId,
                CreatedAt = receiptMID.DateCreate,
                UpdatedAt = receiptMID.DateCreate, //!!!TMP

                //PaymentType= PaymentType.None,//!!!TMP
                //PaidAmount=0,//Скільки фактично оплатили.
                //ReceiptItems=
                //Customer /// !!!TMP Модель клієнта
                //PaymentInfo
            };
            var listReceiptItem = GetReceiptItem(parReceipt);
            var Res = new ReceiptViewModel(receipt, listReceiptItem, null, null)
                {CustomId = receiptMID.NumberReceipt1C};

            return Res;
        }

        private ModelMID.Receipt GetReceipt(ReceiptViewModel parRVM)
        {
            var receipt = new ModelMID.Receipt()
            {
                ReceiptId= parRVM.Id,
                NumberReceipt = parRVM.FiscalNumber  ,
                //Status = (SumCash > 0 || SumCreditCard > 0 ? ReceiptStatusType.Paid : ReceiptStatusType.Created),//!!!TMP Треба врахувати повернення
                TerminalId = parRVM.TerminalId,

                SumDiscount = parRVM.Discount  ,

                //CustomerId = new Client(CodeClient).ClientId,
                DateCreate= parRVM.UpdatedAt //!!!TMP              
            };
            return receipt;
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
                //LoyaltyPoints 
                Bonuses = Convert.ToDecimal(parClient.SumBonus),
                //LoyaltyPointsTotal 
                Wallet = Convert.ToDecimal(parClient.SumMoneyBonus)
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

        /// <summary>
        /// Terminalses the specified terminals.
        /// </summary>
        /// <param name="terminals">The terminals.</param>
        /// <returns></returns>
        public override bool Terminals(List<Terminal> terminals)
        {
            Bl.UpdateWorkPlace(terminals.Select(r => new WorkPlace() { IdWorkplace = r.CustomerId, Name = r.DisplayName, TerminalGUID = r.Id }));
            return false;
        }
        public override bool MoveSessionToAnotherTerminal(Guid firstTerminalId, Guid secondTerminalId)
        {
            //Якщо чек незакрито на терміналі куди переносити тоді помилка.
            if (Receipts.ContainsKey(secondTerminalId) && Receipts[secondTerminalId] != null)
                return false;
            var idReceipt = Bl.GetNewIdReceipt(secondTerminalId);
            if (Bl.MoveReceipt(GetCurrentReceiptByTerminalId(firstTerminalId), idReceipt))
            {
                Receipts[secondTerminalId] = new ModelMID.Receipt(idReceipt);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Convert ReceiptPayment->Payment
        /// </summary>
        /// <param name="parRP"></param>
        /// <returns></returns>
        public Payment ReceiptPaymentToPayment(ReceiptPayment parRP)
        {
            return new Payment(parRP.ReceiptId)
            {
                TypePay = (eTypePay)(int)parRP.PaymentType,
                SumPay = parRP.PayIn,
                NumberReceipt = parRP.CardPan,
                CodeAuthorization = parRP.TransactionCode,
                //NumberTerminal=parRP.,
            };
        }
        public override bool RefundReceipt(RefundReceiptViewModel parReceipt)
        {
            return false;
        }

        public override bool UpdateProductWeight(string parS, int weight)
        {
            return Bl.InsertWeight(parS, weight);
        }

        private static Api _instance;
        public static Api Instance = _instance ?? (_instance = new ApiPSU());
    }
}
