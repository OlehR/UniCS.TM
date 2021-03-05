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
        public Dictionary<Guid, ModelMID.IdReceipt> Receipts = new Dictionary<Guid, ModelMID.IdReceipt>();
        public ApiPSU()
        {
            Bl = new BL();
            Global.OnReceiptCalculationComplete += (wareses, guid) =>
            {
                Console.WriteLine("\n==========================Start===================================");
                foreach (var receiptWarese in wareses)
                {
                    Console.WriteLine($"Promotion=>{receiptWarese.GetStrWaresReceiptPromotion.Trim()} \n{receiptWarese.NameWares} - {receiptWarese.Price} Quantity=> {receiptWarese.Quantity} SumDiscount=>{receiptWarese.SumDiscount}");
                }
                //var r = wareses.Select(s => GetProductViewModel(s));
                OnProductsChanged?.Invoke(wareses.Select(s => GetProductViewModel(s)), guid);
                Console.WriteLine("===========================End==========================================\n");
            };
            
            Global.OnSyncInfoCollected += (SyncInfo) =>
            {
                Console.WriteLine($"OnSyncInfoCollected Status=>{SyncInfo.Status} StatusDescription=>{SyncInfo.StatusDescription}");
                /*
                if (SyncInfo.Status== eSyncStatus.SyncFinishedSuccess)
                {
                    try
                    {
                        var r= GetBags();
                    }
                    catch (Exception ex)
                    {
                        var e = ex.Message;
                    }

                }*/
                                
                OnSyncInfoCollected?.Invoke(SyncInfo);
            };
            
            Global.OnStatusChanged += (Status) => OnStatusChanged?.Invoke(Status);

            Global.OnChangedStatusScale += (Status) => OnChangedStatusScale?.Invoke(Status);

            Global.OnClientChanged += (client, guid) =>
            {
                Console.WriteLine($"Client.Wallet=> {client.Wallet} SumBonus=>{client.SumBonus} ");                
                OnCustomerChanged?.Invoke(GetCustomerViewModelByClient(client), guid);
            };
        }

        public override ProductViewModel AddProductByBarCode(Guid parTerminalId, string parBarCode, decimal parQuantity = 0)
        {
            var CurReceipt = GetCurrentReceiptByTerminalId(parTerminalId);
            var RW = Bl.AddWaresBarCode(CurReceipt, parBarCode, parQuantity);
            if (RW == null)
                return null;
            return GetProductViewModel(RW);
        }
        public override ProductViewModel AddProductByProductId(Guid parTerminalId, Guid parProductId, decimal parQuantity = 0, decimal parPrice=0)
        {
            var CurReceipt = GetCurrentReceiptByTerminalId(parTerminalId);
            //var g = CurReceipt.ReceiptId;
            var WId = new IdReceiptWares { WaresId = parProductId };
            var RW = Bl.AddWaresCode(CurReceipt, WId.CodeWares, WId.CodeUnit, parQuantity,parPrice);
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

        public override ReceiptViewModel GetRecieptByTerminalId(Guid parTerminalId, bool pIsDetail = false)
        {
            var receiptId = GetCurrentReceiptByTerminalId(parTerminalId); 
            return GetReceiptViewModel(receiptId, pIsDetail);

        }
        public override bool AddPayment(Guid parTerminalId, ReceiptPayment[] parPayment, Guid? parReceiptId = null)
        {
            IdReceipt receiptId = (parReceiptId != null && parReceiptId != Guid.Empty)?  new IdReceipt(parReceiptId.Value): GetCurrentReceiptByTerminalId(parTerminalId);
            Bl.db.ReplacePayment(parPayment.Select(r => ReceiptPaymentToPayment(receiptId, r)));
            return Bl.SetStateReceipt(receiptId, eStateReceipt.Pay);

        }
        public override bool AddFiscalNumber(Guid parTerminalId, string parFiscalNumber, Guid? parReceiptId = null)
        {
            IdReceipt receiptId = (parReceiptId != null && parReceiptId != Guid.Empty) ? new IdReceipt(parReceiptId.Value) : GetCurrentReceiptByTerminalId(parTerminalId);
            Bl.UpdateReceiptFiscalNumber(receiptId, parFiscalNumber);
            ClearReceiptByReceiptId(receiptId);
            //          ClearReceipt(parReceiptId);
            return true;
        }

        public override bool ClearReceipt(Guid parTerminalId, Guid? parReceiptId = null)
        {
            IdReceipt receiptId = (parReceiptId != null && parReceiptId != Guid.Empty) ? new IdReceipt(parReceiptId.Value) : GetCurrentReceiptByTerminalId(parTerminalId);
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

            FastGroup fastGroup =( categoryId == null ? new FastGroup() : new FastGroup(categoryId.Value));

            var receiptId = GetCurrentReceiptByTerminalId(parTerminalId);
            //int Limit = 10;
            var res = Bl.GetProductsByName(receiptId, parName.Replace(' ', '%').Trim(), pageNumber * parLimit, parLimit, fastGroup.CodeFastGroup);
            if (res == null)
                return null;
            return res.Select(r => (GetProductViewModel(r)));
        }
        //Зберігає
        public override bool UpdateReceipt(ReceiptViewModel parReceipt)
        {
            // throw new NotImplementedException();
            if (parReceipt!=null && parReceipt.ReceiptEvents!=null)
            {
                var RE = parReceipt.ReceiptEvents.Select(r => GetReceiptEvent(r));
                return Bl.SaveReceiptEvents(RE);
            }
            return false;
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
            _ = Bl.GetBonusAsync(CM, parTerminalId);
            return GetCustomerViewModelByClient(CM);
        }
        public override CustomerViewModel GetCustomerByPhone(Guid parTerminalId, string parPhone)
        {
            var CM = Bl.GetClientByPhone(GetCurrentReceiptByTerminalId(parTerminalId), parPhone);
            if (CM == null)
                return null;
            _ = Bl.GetBonusAsync(CM, parTerminalId);
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
            if (Receipts != null)
                foreach (var el in Receipts)
                {
                    if (el.Value != null && el.Value.Equals(idReceipt))
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
                //Bl.AddReceipt(Receipts[parTerminalId]);
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
            var LWI = new List<WeightInfo>();
            if(receiptWares.IsWeight || receiptWares.WeightBrutto>0)
              LWI.Add(      
                    new WeightInfo() {
                        Weight = (receiptWares.IsWeight ? Convert.ToDouble(receiptWares.Quantity) : Convert.ToDouble(receiptWares.WeightBrutto)),
                        DeltaWeight = Convert.ToDouble(receiptWares.WeightDelta)+ Convert.ToDouble(Global.GetCoefDeltaWeight(
                            (receiptWares.IsWeight ? receiptWares.Quantity : receiptWares.WeightBrutto))* (receiptWares.IsWeight ? receiptWares.Quantity : receiptWares.WeightBrutto))  }                    
            );
            if (!receiptWares.IsWeight && receiptWares.AdditionalWeights != null)
                foreach (var el in receiptWares.AdditionalWeights)
                    LWI.Add(new WeightInfo { DeltaWeight = Convert.ToDouble(receiptWares.WeightDelta) + Convert.ToDouble(Global.GetCoefDeltaWeight(el))*Convert.ToDouble(el), Weight = Convert.ToDouble(el) });
            var varTags = (receiptWares.TypeWares > 0 || receiptWares.LimitAge > 0 || (!receiptWares.IsWeight && receiptWares.WeightBrutto == 0))
                    ? new List<Tag>() : null; //!!!TMP // Різні мітки алкоголь, обмеження по часу.

            //Якщо алкоголь чи тютюн
            if (receiptWares.TypeWares > 0 || receiptWares.LimitAge>0)
                varTags.Add(new Tag() { Key = "AgeRestricted", Id = 0 });
            //Якщо алкоголь обмеження по часу
            if (receiptWares.TypeWares == 1)
                varTags.Add(new Tag() { Key = "TimeRestricted", Id = 1, RuleValue = "{\"Start\":\"" + Global.AlcoholTimeStart + "\",\"Stop\":\"" + Global.AlcoholTimeStop + "\"}" });

            // Якщо немає ваги відключаємо її контроль 
            if (!receiptWares.IsWeight && LWI.Count() == 0 && receiptWares.WeightFact != -1)
                varTags.Add(new Tag { Id = 3, Key = "AutoAcceptRule" });

            // Товар не потрібно зважувати. FoodToGo
            if (receiptWares.WeightFact == -1)
            {
                varTags.Add(new Tag { Id = 4, Key = "DoNotUseScales" });
                varTags.Add(new Tag { Id = 5, Key = "CanBeDeletedByCustomer" });
            }

            if (receiptWares.IsMultiplePrices)
                varTags.Add(new Tag { Id = 6, Key = "MultiplePrices", RuleValue = receiptWares.GetPrices }); 

            var Res = new ProductViewModel()
            {
                Id = receiptWares.WaresId,
                Code = receiptWares.CodeWares,
                Name = receiptWares.NameWares,
                AdditionalDescription = receiptWares.NameWaresReceipt, //!!!TMP;
                Image = null,
                Price = receiptWares.PriceEKKA,//(receiptWares.Priority==1? receiptWares.Price : (receiptWares.Price > receiptWares.PriceDealer ? receiptWares.Price : receiptWares.PriceDealer)),
                //receiptWares.SumDiscount > 0 ? receiptWares.PriceDealer : (receiptWares.Price > 0 ? receiptWares.Price : receiptWares.PriceDealer),
                //receiptWares.SumDiscount > 0 ? ( receiptWares.Price > 0 ? receiptWares.Price : receiptWares.PriceDealer): (receiptWares.Price>receiptWares.PriceDealer ? receiptWares.Price:receiptWares.PriceDealer),
                DiscountValue = receiptWares.DiscountEKKA, //= receiptWares.SumDiscount+ ( receiptWares.Priority == 1?0 : (receiptWares.PriceDealer > receiptWares.Price ? (receiptWares.PriceDealer * receiptWares.Quantity - receiptWares.Sum) : 0)),
                //receiptWares.SumDiscount > 0 ? receiptWares.SumDiscount : 0,
                //Global.RoundDown(receiptWares.SumDiscount>0 ? receiptWares.SumDiscount : (receiptWares.PriceDealer > receiptWares.Price ? (receiptWares.PriceDealer * receiptWares.Quantity - receiptWares.Sum):0)),
                WeightCategory = 2, //вимірювання Похибки в відсотках,2 в грамах
                Weight = (receiptWares.IsWeight ? Convert.ToDouble(receiptWares.Quantity) : (receiptWares.WeightBrutto==0m? 100000 : Convert.ToDouble(receiptWares.WeightBrutto))),
                DeltaWeight = Convert.ToDouble(receiptWares.WeightDelta)+Convert.ToDouble(Global.GetCoefDeltaWeight((receiptWares.IsWeight ? receiptWares.Quantity : receiptWares.WeightBrutto)) * (receiptWares.IsWeight ? receiptWares.Quantity : receiptWares.WeightBrutto)),
                AdditionalWeights = LWI,
                ProductWeightType =  receiptWares.IsWeight ? ProductWeightType.ByWeight : ProductWeightType.ByBarcode, 
                IsAgeRestrictedConfirmed = false, //Обмеження по віку алкоголь Підтверджено не потрібно посилати.
                Quantity = (receiptWares.IsWeight ? 1 : receiptWares.Quantity),               
                DiscountName = receiptWares.GetStrWaresReceiptPromotion,
                WarningType = null, //!!! Не посилати                 
                Tags = varTags,
                HasSecurityMark = false, //!!!TMP // Магнітна мітка, яку треба знімати.
                TotalRows = receiptWares.TotalRows, //Сортування популярного.
                IsProductOnProcessing = false, //
                ///CategoryId=   !!!TMP Групи 1 рівня.
                TaxGroup = Global.GetTaxGroup(receiptWares.TypeVat, receiptWares.TypeWares),
                Barcode = receiptWares.BarCode,
                //FullPrice = receiptWares.Sum
                RefundedQuantity= receiptWares.RefundedQuantity,
                CalculatedWeight= Convert.ToDouble(receiptWares.FixWeight*1000)
                ,Uktzed= receiptWares.CodeUKTZED
                ,IsUktzedNeedToPrint= receiptWares.IsUseCodeUKTZED


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
               // NameDiscount = varPWM.DiscountName,
                Sort = varPWM.TotalRows //Сортування популярного.
            };
            return Res;
        }

        public ReceiptViewModel GetReceiptViewModel(IdReceipt pReceipt, bool pIsDetail = false)
        {
            if (pReceipt == null)
                return null;
            var receiptMID = Bl.GetReceiptHead(pReceipt,true);
            if (receiptMID == null)
                return null;
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
                TotalAmount = receiptMID.SumReceipt - receiptMID.SumBonus- receiptMID.SumDiscount,
                CustomerId = new Client(receiptMID.CodeClient).ClientId,
                CreatedAt = receiptMID.DateCreate,
                UpdatedAt = receiptMID.DateReceipt, 

                //ReceiptItems=
                //Customer /// !!!TMP Модель клієнта
                //PaymentInfo
            };
            var listReceiptItem = GetReceiptItem(receiptMID.Wares,pIsDetail); //GetReceiptItem(parReceipt);
            var Res = new ReceiptViewModel(receipt, listReceiptItem, null, null)
            { CustomId = receiptMID.NumberReceipt1C };
            
            if (receiptMID.Payment != null && receiptMID.Payment.Count()>0)
            {
                Res.PaidAmount = receiptMID.Payment.Sum(r => receipt.Amount);
                var SumCash = receiptMID.Payment.Where(r=> r.TypePay== eTypePay.Cash).Sum(r => receipt.Amount);
                var SumCard = receiptMID.Payment.Where(r => r.TypePay == eTypePay.Card).Sum(r => receipt.Amount);
                Res.PaymentType = (SumCash > 0 && SumCard > 0 ? PaymentType.Card | PaymentType.Card : (SumCash == 0 && SumCard == 0 ? PaymentType.None : (SumCash > 0?PaymentType.Cash: PaymentType.Card)));
                Res.PaymentInfo = PaymentToReceiptPayment(receiptMID.Payment.First());                
            }
            
            if (receiptMID.ReceiptEvent != null && receiptMID.ReceiptEvent.Count() > 0)
                Res.ReceiptEvents = receiptMID.ReceiptEvent.Select(r => GetReceiptEvent(r)).ToList();
            if(pIsDetail)
                Bl.GenQRAsync(receiptMID.Wares);
            return Res;
        }

        

        private List<ReceiptItem> GetReceiptItem(IdReceipt parIdReceipt)
        {
            var res = Bl.ViewReceiptWares(parIdReceipt);
            return GetReceiptItem(res);            
        }

        private List<ReceiptItem> GetReceiptItem(IEnumerable<ReceiptWares> res, bool IsDetail = false)
        {
            var Res = new List<ReceiptItem>();
            foreach (var el in res)
            {
               
                decimal PromotionQuantity = 0;
                IEnumerable<WaresReceiptPromotion> PromotionPrice = null;

                if (el.ReceiptWaresPromotions != null)
                {
                    PromotionPrice = el.ReceiptWaresPromotions.Where(r => r.TypeDiscount == eTypeDiscount.Price);
                    PromotionQuantity = PromotionPrice.Sum(r => r.Quantity);
                }

                if (IsDetail && PromotionQuantity > 0)
                {
                    decimal AllQuantity = el.Quantity;
                    var OtherPromotion = el.ReceiptWaresPromotions.Where(r => r.TypeDiscount != eTypeDiscount.Price);
                    el.ReceiptWaresPromotions = null;
                    
                    if (PromotionQuantity < AllQuantity)
                    {
                        var SumDiscount = OtherPromotion.Sum(r => r.Sum);
                        var QuantityDiscount= OtherPromotion.Sum(r => r.Quantity);
                        el.SumDiscount = QuantityDiscount * (el.Price - SumDiscount / QuantityDiscount);
                        el.ReceiptWaresPromotions = OtherPromotion;
                        el.Quantity = AllQuantity - PromotionQuantity;
                        var PVM = this.GetProductViewModel(el);
                        Res.Add(PVM.ToReceiptItem());
                    }
                    el.SumDiscount = 0;
                    int i = 1;
                    foreach (var p in PromotionPrice)
                    {
                        el.Quantity = p.Quantity;
                        el.Price = p.Price;
                        el.PriceDealer= p.Price;
                        el.Order = i++;
                        var PVM = this.GetProductViewModel(el);
                        Res.Add(PVM.ToReceiptItem());
                    }
                }
                else
                {
                    var PVM = this.GetProductViewModel(el);
                    Res.Add(PVM.ToReceiptItem());
                }
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
                Bonuses = parClient.SumMoneyBonus,
                //LoyaltyPointsTotal 
                Wallet = parClient.Wallet,
                PhoneNumber= parClient.MainPhone
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
                NumberReceipt =  parRP.InvoiceNumber, //parRP.TransactionId,
                NumberCard = parRP.CardPan,
                CodeAuthorization = parRP.TransactionCode, //RRN
                NumberTerminal=parRP.PosTerminalId,
                NumberSlip = parRP.PosAuthCode, //код авторизації
                PosPaid =parRP.PosPaid,
                PosAddAmount=parRP.PosAddAmount,
                DateCreate=parRP.CreatedAt,
                 
            };
        }

        public ReceiptPayment PaymentToReceiptPayment( Payment parRP)
        {
            return new ReceiptPayment()
            {
                PaymentType = (PaymentType)(int)parRP.TypePay,
                PayIn = parRP.SumPay,
                InvoiceNumber = parRP.NumberReceipt,
                CardPan = parRP.NumberCard,
                TransactionCode = parRP.CodeAuthorization, //RRN
                PosTerminalId = parRP.NumberTerminal,
                PosAuthCode = parRP.NumberSlip, //код авторизації
                PosPaid = parRP.PosPaid,
                PosAddAmount = parRP.PosAddAmount,
                CreatedAt = parRP.DateCreate
            };
        }

 
        public override bool UpdateProductWeight(string parData, int parWeight, Guid parWares, TypeSaveWeight parTypeSaveWeight)
        {
            return Bl.InsertWeight(parData, parWeight, parWares, parTypeSaveWeight);
        }

        public override async Task RequestSyncInfo(bool parIsFull = false)
        {
            // TODO: check status
            try
            {
                var res = await Bl.SyncDataAsync(parIsFull);                
            }
            catch (Exception ex)
            {
                var info = new SyncInformation();
                info.Status = eSyncStatus.SyncFinishedError;
                info.StatusDescription = ex.Message;
                info.SyncData = ex;
                OnSyncInfoCollected?.Invoke(info);
            }
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

        public override ReceiptViewModel GetReceiptByNumber(Guid pTerminalId, string pFiscalNumber)
        {
             var IdWorkplace = Global.GetIdWorkplaceByTerminalId(pTerminalId);
             var res = Bl.GetReceiptByFiscalNumber(IdWorkplace, pFiscalNumber);             
             return GetReceiptViewModel(res);
        }

        public override bool RefundReceipt(Guid parTerminalId, RefundReceiptViewModel parReceipt)
        {
            var receipt = ReceiptViewModelToReceipt(parTerminalId,parReceipt);
            receipt.UserCreate = Bl.GetUserIdbyWorkPlace(receipt.IdWorkplace);
            return Bl.SaveReceipt(receipt);
        }


        public ModelMID.Receipt ReceiptViewModelToReceipt(Guid parTerminalId, ReceiptViewModel parReceiptRVM)
        {
            
            if (parReceiptRVM == null)
                return null;
            RefundReceiptViewModel RefundRVM = null;
            if (parReceiptRVM is RefundReceiptViewModel)
                RefundRVM = (RefundReceiptViewModel)parReceiptRVM;
            

                var receipt = new ModelMID.Receipt(RefundRVM == null ? new IdReceipt (parReceiptRVM.Id) :Bl.GetNewIdReceipt(parTerminalId))
            {
                //ReceiptId  = parReceiptRVM.Id,
                StateReceipt = (string.IsNullOrEmpty(parReceiptRVM.FiscalNumber)?(parReceiptRVM.PaymentInfo != null ? eStateReceipt.Pay: eStateReceipt.Prepare) : eStateReceipt.Print),
                TypeReceipt = (RefundRVM == null? eTypeReceipt.Sale:eTypeReceipt.Refund),
                NumberReceipt = parReceiptRVM.FiscalNumber,
               /* Status = (parReceiptRVM.SumCash > 0 || parReceiptRVM.SumCreditCard > 0
                    ? ReceiptStatusType.Paid
                    : ReceiptStatusType.Created), //!!!TMP Треба врахувати повернення*/
                TerminalId = parReceiptRVM.TerminalId,
                SumReceipt = parReceiptRVM.Amount, //!!!TMP Сума чека.
                SumDiscount = parReceiptRVM.Discount,
                //!!TMP TotalAmount = parReceiptRVM.SumReceipt - parReceiptRVM.SumBonus,
                ///CustomerId = new Client(parReceiptRVM.CodeClient).ClientId,
                DateCreate = parReceiptRVM.CreatedAt,
                DateReceipt = (parReceiptRVM.UpdatedAt==default(DateTime)?DateTime.Now: parReceiptRVM.UpdatedAt)
                //ReceiptItems=
                //Customer /// !!!TMP Модель клієнта
                //PaymentInfo
                
            };

            if (RefundRVM!=null)
               receipt.RefundId = (RefundRVM.IdPrimary == null ? null : new IdReceipt(RefundRVM.IdPrimary));            

            if (parReceiptRVM.PaymentInfo!=null)
             receipt.Payment = new Payment[] { ReceiptPaymentToPayment(receipt, parReceiptRVM.PaymentInfo) };

            if(parReceiptRVM.ReceiptItems!=null)
            receipt.Wares = parReceiptRVM.ReceiptItems.Select(r=>GetReceiptWaresFromReceiptItem(receipt, r));

            //if(parReceiptRVM.ReceiptEvents!=null)
            //    receipt.ReceiptEvent= parReceiptRVM.ReceiptEvent


            return receipt;
        }

        private ReceiptWares GetReceiptWaresFromReceiptItem(IdReceipt parIdReceipt, ReceiptItem receiptItem)
        {
            var Res = new ReceiptWares(parIdReceipt, receiptItem.ProductId)
            {
                //WaresId = receiptItem.ProductId,
                //CodeWares = receiptItem.Code,
                NameWares = receiptItem.ProductName,
                BarCode = receiptItem.ProductBarcode,
                PriceDealer = receiptItem.ProductPrice,
                Price = (receiptItem.FullPrice>0?receiptItem.FullPrice: receiptItem.TotalPrice) / receiptItem.ProductQuantity, 
                WeightBrutto = receiptItem.ProductWeight / 1000m,
                Quantity= receiptItem.ProductQuantity,
//                TaxGroup = Global.GetTaxGroup(receiptItem.TypeVat, receiptItem.TypeWares),               
                //FullPrice = receiptItem.Sum
                RefundedQuantity = receiptItem.RefundedQuantity
            };
            return Res;
        }

        private ModernIntegration.Models.ReceiptEvent GetReceiptEvent(ModelMID.DB.ReceiptEvent RE)
        {
            return new ModernIntegration.Models.ReceiptEvent()
            {
                Id = RE.Id,
                MobileDeviceId = RE.MobileDeviceId,
                ReceiptId = RE.ReceiptId,
                ReceiptItemId = RE.ReceiptItemId,
                ProductName = RE.ProductName,
                EventType = (Enums.ReceiptEventType)(int)RE.EventType,
                EventName = RE.EventName,
                ProductWeight = RE.ProductWeight,
                ProductConfirmedWeight = RE.ProductConfirmedWeight,
                UserId = RE.UserId,
                UserName = RE.UserName,
                CreatedAt = RE.CreatedAt,
                ResolvedAt = RE.ResolvedAt,
                RefundAmount = RE.RefundAmount,
                FiscalNumber = RE.FiscalNumber,
                PaymentType = (Enums.PaymentType)(int)RE.PaymentType,
                TotalAmount = RE.TotalAmount
            };
        
        }

        private ModelMID.DB.ReceiptEvent GetReceiptEvent(ModernIntegration.Models.ReceiptEvent RE)
        {
            return new ModelMID.DB.ReceiptEvent()
            {
                Id = RE.Id,
                MobileDeviceId = RE.MobileDeviceId,
                ReceiptId = RE.ReceiptId,
                ReceiptItemId = RE.ReceiptItemId,
                ProductName = RE.ProductName,
                EventType = (ModelMID.ReceiptEventType)(int)RE.EventType,
                EventName = RE.EventName,
                ProductWeight = RE.ProductWeight,
                ProductConfirmedWeight = RE.ProductConfirmedWeight,
                UserId = RE.UserId,
                UserName = RE.UserName,
                CreatedAt = RE.CreatedAt,
                ResolvedAt = RE.ResolvedAt,
                RefundAmount = RE.RefundAmount,
                FiscalNumber = RE.FiscalNumber,
                PaymentType =(ModelMID.eTypePayment)(int) RE.PaymentType,
                TotalAmount = RE.TotalAmount
            };

        }
        /*
        public override double GetMidlWeight()
        {
            return Bl.GetMidlWeight();
        }

       
        public override void StartWeightNewGoogs(IEnumerable<WeightInfo> pWeight, int pCount)
        {
            WaitWeight[] res;
            if (pCount > 0)
                res = pWeight.Select(r => new WaitWeight() { Min = (double)pCount * (r.Weight - r.DeltaWeight), Max = (double)pCount * (r.Weight + r.DeltaWeight) }).ToArray();
            else
                res = pWeight.Select(r => new WaitWeight() { Min = (double)pCount * (r.Weight + r.DeltaWeight), Max = (double)pCount * (r.Weight - r.DeltaWeight) }).ToArray();

            Bl.StartWeightNewGoogs(res);
        }

       
        public override bool FixedWeight()
        {
            return Bl.FixedWeight();
        }

        public override bool WaitClearScale()
        {
            return Bl.WaitClearScale();
        }
        */

        public override void CloseDb()
        { 
            Bl.CloseDB();
        }

        public override void StartWork(Guid pTerminalId, string pBarCodeCashier)
        {
           var IdWorkplace = Global.GetIdWorkplaceByTerminalId(pTerminalId);
           Bl.StartWork(IdWorkplace, pBarCodeCashier);
        }

        public override void StopWork(Guid pTerminalId)
        {
            var IdWorkplace = Global.GetIdWorkplaceByTerminalId(pTerminalId);
            Bl.StoptWork(IdWorkplace);
        }
        public override bool SetWeight(Guid pTerminalId, Guid pProductId, decimal pWaight)
        {
            var CurReceipt = GetCurrentReceiptByTerminalId(pTerminalId);          
            return Bl.FixWeight(CurReceipt, pProductId, pWaight/1000m);  
        }

         public override IEnumerable<QRDefinitions> GetQR(Guid pTerminalId) 
        {
            List<QRDefinitions> res=new List<QRDefinitions>();
            var QR = Bl.GetQR(GetCurrentReceiptByTerminalId(pTerminalId));
            foreach(var el in QR)
            {
               foreach(var qr in  el.Qr.Split(','))
                {
                    res.Add(new QRDefinitions() { Caption = el.Name, Data = qr });
                }
            }

            return res;
        }
    }




}