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
using System.Threading.Tasks;

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
                    Console.WriteLine($"{receiptWarese.NameWares} - {receiptWarese.Price} Quantity=> {receiptWarese.Quantity} SumDiscount=>{receiptWarese.SumDiscount}");
                }
                OnProductsChanged?.Invoke(wareses.Select(s => GetProductViewModel(s)), guid);
            };
            Global.OnSyncInfoCollected = (SyncInfo) => OnSyncInfoCollected?.Invoke(SyncInfo);
            Global.OnStatusChanged = (Status) => OnStatusChanged?.Invoke(Status);
        }
        public override ProductViewModel AddProductByBarCode(Guid parTerminalId, string parBarCode, decimal parQuantity = 0)
        {
            var CurReceipt = GetCurrentReceiptByTerminalId(parTerminalId);
            var RW = Bl.AddWaresBarCode(CurReceipt, parBarCode, parQuantity);
            if (RW == null)
                return null;
            return GetProductViewModel(RW);
        }
        public override ProductViewModel AddProductByProductId(Guid parTerminalId, Guid parProductId, decimal parQuantity = 0)
        {
            var CurReceipt = GetCurrentReceiptByTerminalId(parTerminalId);
            var g = CurReceipt.ReceiptId;
            var RW = Bl.AddWaresCode(CurReceipt, parProductId, parQuantity);
            //TODO: OnReceiptChanged?.Invoke(receipt,terminalId);
            if (RW == null)
                return null;
            return GetProductViewModel(RW);
        }
        public override ReceiptViewModel ChangeQuantity(Guid parTerminalId, Guid parProductId, decimal parQuantity)
        {
            var CurReceipt = GetCurrentReceiptByTerminalId(parTerminalId);
            var CurReceiptWares = new IdReceiptWares(CurReceipt, parProductId);

            Bl.ChangeQuantity(CurReceiptWares, parQuantity);
            var Res = GetReceiptViewModel(CurReceipt);
            return Res;
        }
        public override ReceiptViewModel GetReciept(Guid parReceipt)
        {
            //var Receipt = new GetCurrentReceiptByTerminalId();
            var Res = GetReceiptViewModel(new IdReceipt(parReceipt));
            return Res;
        }

        public override ReceiptViewModel GetRecieptByTerminalId(Guid parTerminalId)
        {
            var receiptId = GetCurrentReceiptByTerminalId(parTerminalId);
            return GetReceiptViewModel(receiptId);

        }
        public override bool AddPayment(Guid parTerminalId, ReceiptPayment[] parPayment)
        {
            var receiptId = GetCurrentReceiptByTerminalId(parTerminalId);
            Bl.db.ReplacePayment(parPayment.Select(r => ReceiptPaymentToPayment(receiptId, r)));
            return Bl.SetStateReceipt(receiptId, eStateReceipt.Pay);

        }
        public override bool AddFiscalNumber(Guid parTerminalId, string parFiscalNumber)
        {
            var receiptId = new IdReceipt(GetCurrentReceiptByTerminalId(parTerminalId));
            Bl.UpdateReceiptFiscalNumber(receiptId, parFiscalNumber);
            ClearReceiptByReceiptId(receiptId);
            //          ClearReceipt(parReceiptId);
            return true;
        }

        public override bool ClearReceipt(Guid parTerminalId)
        {
            var receiptId = new IdReceipt(GetCurrentReceiptByTerminalId(parTerminalId));
            Bl.SetStateReceipt(receiptId, eStateReceipt.Canceled);
            Receipts[parTerminalId] = null;
            return true;
        }

        public override IEnumerable<ProductViewModel> GetBags()
        {
            return Bl.db.GetBags().Select(r => GetProductViewModel(r));
        }

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
        public override IEnumerable<ProductViewModel> GetProductsByName(Guid parTerminalId, string parName, int pageNumber = 0, bool excludeWeightProduct = false, Guid? categoryId = null, int parLimit = 10)
        {

            var receiptId = GetCurrentReceiptByTerminalId(parTerminalId);
            //int Limit = 10;
            var res = Bl.GetProductsByName(receiptId, parName.Replace(' ', '%').Trim(), pageNumber * parLimit, parLimit);
            if (res == null)
                return null;
            return res.Select(r => (GetProductViewModel(r)));
        }

        public override bool UpdateReceipt(ReceiptViewModel parReceipt)
        {
            throw new NotImplementedException();
            //return false;
        }
        public override TypeSend SendReceipt(Guid parReceipt)
        {
            Bl.SendReceiptTo1C(new IdReceipt(parReceipt));
            return TypeSend.NotReady;
        }
        public override TypeSend GetStatusReceipt(Guid parReceipt)
        {
            throw new NotImplementedException();
            //return TypeSend.NotReady; 
        }

        public override CustomerViewModel GetCustomerByBarCode(Guid parTerminalId, string parS)
        {
            var CM = Bl.GetClientByBarCode(GetCurrentReceiptByTerminalId(parTerminalId), parS);
            if (CM == null)
                return null;
            return GetCustomerViewModelByClient(CM);
        }
        public override CustomerViewModel GetCustomerByPhone(Guid parTerminalId, string parPhone)
        {
            var CM = Bl.GetClientByPhone(GetCurrentReceiptByTerminalId(parTerminalId), parPhone);
            if (CM == null)
                return null;
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

            if (!Receipts.ContainsKey(parTerminalId) || Receipts[parTerminalId] == null)
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
            var LWI = new List<WeightInfo>()
                    { new WeightInfo() { Weight = (receiptWares.IsWeight ? Convert.ToDouble(receiptWares.Quantity) : Convert.ToDouble(receiptWares.WeightBrutto)), DeltaWeight = 3 }
                    };
            if (receiptWares.AdditionalWeights != null)
                foreach (var el in receiptWares.AdditionalWeights)
                    LWI.Add(new WeightInfo { DeltaWeight = 0.07 * Convert.ToDouble(el), Weight = Convert.ToDouble(el) });
            var varTags = (receiptWares.TypeWares > 0 || (!receiptWares.IsWeight && receiptWares.WeightBrutto == 0))
                    ? new List<Tag>() : null; //!!!TMP // Різні мітки алкоголь, обмеження по часу.

            //Якщо алкоголь чи тютюн
            if (receiptWares.TypeWares > 0)
                varTags.Add(new Tag() { Key = "AgeRestricted", Id = 0 });
            //Якщо алкоголь обмеження по часу
            if (receiptWares.TypeWares == 1)
                varTags.Add(new Tag() { Key = "TimeRestricted", Id = 1, RuleValue = "{\"Start\":\"" + Global.AlcoholTimeStart + "\",\"Stop\":\"" + Global.AlcoholTimeStop + "\"}" });

            // Якщо немає ваги відключаємо її контроль 
            if (!receiptWares.IsWeight && receiptWares.WeightBrutto == 0)
                varTags.Add(new Tag { Id = 3, Key = "NoWeightedProduct" });

            var Res = new ProductViewModel()
            {
                Id = receiptWares.WaresId,
                Code = receiptWares.CodeWares,
                Name = receiptWares.NameWares,
                AdditionalDescription = receiptWares.NameWaresReceipt, //!!!TMP;
                Image = null,
                Price = receiptWares.SumDiscount > 0 ? ( receiptWares.Price > 0 ? receiptWares.Price : receiptWares.PriceDealer): receiptWares.PriceDealer,
                WeightCategory = 2, //вимірювання Похибки в відсотках,2 в грамах
                Weight = (receiptWares.IsWeight ? Convert.ToDouble(receiptWares.Quantity) : Convert.ToDouble(receiptWares.WeightBrutto)),
                DeltaWeight = 0.07 * (receiptWares.IsWeight ? Convert.ToDouble(receiptWares.Quantity) : Convert.ToDouble(receiptWares.WeightBrutto)),
                AdditionalWeights = LWI,
                ProductWeightType =
                    receiptWares.IsWeight ? ProductWeightType.ByWeight : ProductWeightType.ByPiece, //!!!TMP
                IsAgeRestrictedConfirmed =
                    false, //!!!TMP //Обмеження по віку алкоголь Підтверджено не потрібно посилати.
                Quantity = (receiptWares.IsWeight ? 1 : receiptWares.Quantity),
                DiscountValue = receiptWares.SumDiscount>0 ? receiptWares.SumDiscount : (receiptWares.PriceDealer - receiptWares.Price) * receiptWares.Quantity,
                DiscountName = receiptWares.NameDiscount,
                WarningType = null, //!!! Не посилати 
                CalculatedWeight = 0,
                Tags = varTags,
                HasSecurityMark = false, //!!!TMP // Магнітна мітка, яку треба знімати.
                TotalRows = receiptWares.Sort, //Сортування популярного.

                IsProductOnProcessing = false, //
                ///CategoryId=   !!!TMP Групи 1 рівня.
                TaxGroup = Global.GetTaxGroup(receiptWares.TypeVat, receiptWares.TypeWares),
                Barcode = receiptWares.BarCode,
            };
            return Res;
        }

        private ReceiptWares GetReceiptWares(ProductViewModel varPWM)
        {
            if (varPWM == null)
                return null;
            var Res = new ReceiptWares()
            {
                WaresId = varPWM.Id,
                CodeWares = varPWM.Code,
                NameWares = varPWM.Name,
                NameWaresReceipt = varPWM.AdditionalDescription, //!!!TMP;
                Price = varPWM.Price,
                Quantity = varPWM.Quantity,
                SumDiscount = varPWM.DiscountValue,
                NameDiscount = varPWM.DiscountName,
                Sort = varPWM.TotalRows //Сортування популярного.
            };
            return Res;
        }

        private ReceiptViewModel GetReceiptViewModel(IdReceipt parReceipt)
        {
            var receiptMID = Bl.GetReceiptHead(parReceipt,true);

            var receipt = new Receipt()
            {
                Id = receiptMID.ReceiptId,
                FiscalNumber = receiptMID.NumberReceipt,
                Status = (receiptMID.SumCash > 0 || receiptMID.SumCreditCard > 0
                    ? ReceiptStatusType.Paid
                    : ReceiptStatusType.Created), //!!!TMP Треба врахувати повернення
                TerminalId = receiptMID.TerminalId,
                Amount = receiptMID.SumReceipt, //!!!TMP Сума чека.
                Discount = receiptMID.SumDiscount,
                TotalAmount = receiptMID.SumReceipt - receiptMID.SumBonus,
                CustomerId = new Client(receiptMID.CodeClient).ClientId,
                CreatedAt = receiptMID.DateCreate,
                UpdatedAt = receiptMID.DateCreate, //!!!TMP

                //ReceiptItems=
                //Customer /// !!!TMP Модель клієнта
                //PaymentInfo
            };
            var listReceiptItem = GetReceiptItem(receiptMID.Wares); //GetReceiptItem(parReceipt);
            var Res = new ReceiptViewModel(receipt, listReceiptItem, null, null)
            { CustomId = receiptMID.NumberReceipt1C };
            
            if (receiptMID.Payment != null)
            {
                Res.PaidAmount = receiptMID.Payment.Sum(r => receipt.Amount);
                var SumCash = receiptMID.Payment.Where(r=> r.TypePay== eTypePay.Cash).Sum(r => receipt.Amount);
                var SumCard = receiptMID.Payment.Where(r => r.TypePay == eTypePay.Card).Sum(r => receipt.Amount);
                Res.PaymentType = (SumCash > 0 && SumCard > 0 ? PaymentType.Both : (SumCash == 0 && SumCard == 0 ? PaymentType.None : (SumCash > 0?PaymentType.Cash: PaymentType.Card)));
            }
            
            return Res;
        }

        private ModelMID.Receipt GetReceipt(ReceiptViewModel parRVM)
        {
            var receipt = new ModelMID.Receipt()
            {
                ReceiptId = parRVM.Id,
                NumberReceipt = parRVM.FiscalNumber,
                //Status = (SumCash > 0 || SumCreditCard > 0 ? ReceiptStatusType.Paid : ReceiptStatusType.Created),//!!!TMP Треба врахувати повернення
                TerminalId = parRVM.TerminalId,

                SumDiscount = parRVM.Discount,

                //CustomerId = new Client(CodeClient).ClientId,
                DateCreate = parRVM.UpdatedAt //!!!TMP              
            };
            return receipt;
        }

        private List<ReceiptItem> GetReceiptItem(IdReceipt parIdReceipt)
        {
            var res = Bl.ViewReceiptWares(parIdReceipt);//new ModelMID.IdReceipt { CodePeriod = 20190613, CodeReceipt = 1, IdWorkplace = 140701 }

            return GetReceiptItem(res);
            /*var Res = new List<ReceiptItem>();
            var res = Bl.ViewReceiptWares(parIdReceipt);//new ModelMID.IdReceipt { CodePeriod = 20190613, CodeReceipt = 1, IdWorkplace = 140701 }
            foreach (var el in res)
            {
                var PVM = this.GetProductViewModel(el);
                Res.Add(PVM.ToReceiptItem());
            }
            return Res;*/
        }

        private List<ReceiptItem> GetReceiptItem(IEnumerable<ReceiptWares> res)
        {
            var Res = new List<ReceiptItem>();
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
                ParentId = null,
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
        public Payment ReceiptPaymentToPayment(IdReceipt parIdReceipt, ReceiptPayment parRP)
        {
            return new Payment(parIdReceipt)
            {
                TypePay = (eTypePay)(int)parRP.PaymentType,
                SumPay = parRP.PayIn,
                NumberReceipt = parRP.TransactionId,
                NumberCard = parRP.CardPan,
                CodeAuthorization = parRP.TransactionCode, //RRN
                NumberTerminal=parRP.PosTerminalId,
                NumberSlip = parRP.PosAuthCode //код авторизації
            };
        }
        public override bool RefundReceipt(RefundReceiptViewModel parReceipt)
        {
            return false;
        }

        public override bool UpdateProductWeight(string parData, int parWeight, Guid? parWares = null)
        {
            return Bl.InsertWeight(parData, parWeight, parWares);
        }

        public override async Task RequestSyncInfo(bool parIsFull = false)
        {
            // TODO: check status
            OnSyncInfoCollected?.Invoke(new SyncInformation() { Status = parIsFull ? eSyncStatus.StartedFullSync : eSyncStatus.StartedPartialSync });

            var info = new SyncInformation();
            try
            {
                var res = await Task.Factory.StartNew(() => Bl.SyncData(parIsFull));
                info.Status = (res) ? eSyncStatus.SyncFinishedSuccess : eSyncStatus.SyncFinishedError;
            }
            catch (Exception ex)
            {
                info.Status = eSyncStatus.SyncFinishedError;
                info.StatusDescription = ex.Message;
                info.SyncData = ex;
            }
            OnSyncInfoCollected?.Invoke(info);

            await Bl.SendAllReceipt().ConfigureAwait(false);
        }

        public override IEnumerable<ProductViewModel> GetProduct(Guid parTerminalId)
        {

            var receiptId = GetCurrentReceiptByTerminalId(parTerminalId);

            var res = Bl.GetWaresReceipt(new IdReceipt(receiptId));
            if (res == null)
                return null;
            return res.Select(r => (GetProductViewModel(r)));
        }

        private static Api _instance;
        public static Api Instance = _instance ?? (_instance = new ApiPSU());

        public override ReceiptViewModel GetNoFinishReceipt(Guid parTerminalId)
        {
            var receipt = Bl.GetLastReceipt(parTerminalId);
            if (receipt == null)
                return null;
            if (receipt.StateReceipt != eStateReceipt.Prepare)
                return null;
            Receipts[parTerminalId] = new ModelMID.Receipt(receipt);

            return GetReceiptViewModel(receipt);
        }

        public override IEnumerable<ReceiptViewModel> GetReceipts(DateTime parStartDate, DateTime parFinishDate, Guid? parTerminalId = null)
        {

            int IdWorkplace = 0;
                if (parTerminalId != null)
                IdWorkplace=Global.GetIdWorkplaceByTerminalId(parTerminalId.Value);

            var res=Bl.GetReceipts(parStartDate, parFinishDate, IdWorkplace);

            return res.Select(r=>GetReceiptViewModel(r));
            
        }


    }
}