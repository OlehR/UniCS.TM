using System;
using System.Collections.Generic;
using ModernIntegration.Models;
using ModernIntegration.ViewModels;
using ModernIntegration.Enums;
using ModelMID;
using System.Linq;
using Receipt = ModernIntegration.Models.Receipt;
using ModelMID.DB;
using ModernIntegration.Model;

namespace ModernIntegration
{
    public partial class ApiPSU
    {
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
        /// <param name="pTerminalId">The par terminal identifier.</param>
        /// <returns></returns>
        public ModelMID.IdReceipt GetCurrentReceiptByTerminalId(Guid pTerminalId)
        {
            if (!Receipts.ContainsKey(pTerminalId) || Receipts[pTerminalId] == null)
            {
                var idReceipt = Bl.GetNewIdReceipt(Global.GetIdWorkplaceByTerminalId(pTerminalId));
                Receipts[pTerminalId] = new ModelMID.Receipt(idReceipt);
                //Bl.AddReceipt(Receipts[pTerminalId]);
            }
            return Receipts[pTerminalId];
        }

        /// <summary>
        /// Convert MID.ReceiptWares->ProductViewModel
        /// </summary>
        /// <param name="receiptWares"></param>
        /// <returns></returns>
        private ProductViewModel GetProductViewModel(ReceiptWares receiptWares)
        {
            var LWI = new List<WeightInfo>();
            if (receiptWares.IsWeight || receiptWares.WeightBrutto > 0)
                LWI.Add(
                      new WeightInfo()
                      {
                          Weight = (receiptWares.IsWeight ? Convert.ToDouble(receiptWares.Quantity) : Convert.ToDouble(receiptWares.WeightBrutto)),
                          DeltaWeight = Convert.ToDouble(receiptWares.WeightDelta) + Convert.ToDouble(Global.GetCoefDeltaWeight(
                              (receiptWares.IsWeight ? receiptWares.Quantity : receiptWares.WeightBrutto)) * (receiptWares.IsWeight ? receiptWares.Quantity : receiptWares.WeightBrutto))
                      }
              );
            if (!receiptWares.IsWeight && receiptWares.AdditionalWeights != null)
                foreach (var el in receiptWares.AdditionalWeights)
                    LWI.Add(new WeightInfo { DeltaWeight = Convert.ToDouble(receiptWares.WeightDelta) + Convert.ToDouble(Global.GetCoefDeltaWeight(el)) * Convert.ToDouble(el), Weight = Convert.ToDouble(el) });
            var varTags = (receiptWares.TypeWares > 0 || receiptWares.LimitAge > 0 || (!receiptWares.IsWeight && receiptWares.WeightBrutto == 0) ||(receiptWares.WeightFact == -1) || (receiptWares.IsMultiplePrices) )
                    ? new List<Tag>() : null; //!!!TMP // Різні мітки алкоголь, обмеження по часу.

            //Якщо алкоголь чи тютюн
            if (receiptWares.TypeWares > 0 || receiptWares.LimitAge > 0)
                varTags.Add(new Tag() { Key = "AgeRestricted", Id = 0 });
            //Якщо алкоголь обмеження по часу
            if (receiptWares.TypeWares == eTypeWares.Alcohol)
                varTags.Add(new Tag() { Key = "TimeRestricted", Id = 1, RuleValue = "{\"Start\":\"" + Global.AlcoholTimeStart + "\",\"Stop\":\"" + Global.AlcoholTimeStop + "\"}" });
            //Якщо алкоголь ввід Марки.
            if (receiptWares.TypeWares == eTypeWares.Alcohol)
                varTags.Add(new Tag() { Key = "NeedExcise", Id = 2 });

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
                Weight = (receiptWares.IsWeight ? Convert.ToDouble(receiptWares.Quantity) : (receiptWares.WeightBrutto == 0m ? 100000 : Convert.ToDouble(receiptWares.WeightBrutto))),
                DeltaWeight = Convert.ToDouble(receiptWares.WeightDelta) + Convert.ToDouble(Global.GetCoefDeltaWeight((receiptWares.IsWeight ? receiptWares.Quantity : receiptWares.WeightBrutto)) * (receiptWares.IsWeight ? receiptWares.Quantity : receiptWares.WeightBrutto)),
                AdditionalWeights = LWI,
                ProductWeightType = receiptWares.IsWeight ? ProductWeightType.ByWeight : ProductWeightType.ByBarcode,
                IsAgeRestrictedConfirmed = false, //Обмеження по віку алкоголь Підтверджено не потрібно посилати.
                Quantity = (receiptWares.IsWeight ? 1 : receiptWares.Quantity),
                DiscountName = receiptWares.GetStrWaresReceiptPromotion,
                WarningType = null, //!!! Не посилати                 
                Tags = varTags,
                HasSecurityMark = false, //!!!TMP // Магнітна мітка, яку треба знімати.
                TotalRows = receiptWares.TotalRows, //Сортування популярного.
                IsProductOnProcessing = false, //
                ///CategoryId=   !!!TMP Групи 1 рівня.
                TaxGroup = Global.GetTaxGroup(receiptWares.TypeVat,(int) receiptWares.TypeWares),
                Barcode = receiptWares.TypeWares > 0 ? receiptWares.BarCode : null,
                //FullPrice = receiptWares.Sum
                RefundedQuantity = receiptWares.RefundedQuantity,
                CalculatedWeight = Convert.ToDouble(receiptWares.FixWeight * 1000)
                ,
                Uktzed = receiptWares.TypeWares > 0 ? receiptWares.CodeUKTZED : null
                ,
                IsUktzedNeedToPrint = receiptWares.IsUseCodeUKTZED
                ,
                Excises = receiptWares.ExciseStamp?.Split(',').ToList()

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
            var receiptMID = Bl.GetReceiptHead(pReceipt, true);
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
                TotalAmount = receiptMID.SumReceipt - receiptMID.SumBonus - receiptMID.SumDiscount,
                CustomerId = new Client(receiptMID.CodeClient).ClientId,
                CreatedAt = receiptMID.DateCreate,
                UpdatedAt = receiptMID.DateReceipt,

                //ReceiptItems=
                //Customer /// !!!TMP Модель клієнта
                //PaymentInfo
            };
            var listReceiptItem = GetReceiptItem(receiptMID.Wares, pIsDetail); //GetReceiptItem(pReceipt);
            var Res = new ReceiptViewModel(receipt, listReceiptItem, null, null)
            { CustomId = receiptMID.NumberReceipt1C };

            if (receiptMID.Payment != null && receiptMID.Payment.Count() > 0)
            {
                Res.PaidAmount = receiptMID.Payment.Sum(r => receipt.Amount);
                var SumCash = receiptMID.Payment.Where(r => r.TypePay == eTypePay.Cash).Sum(r => receipt.Amount);
                var SumCard = receiptMID.Payment.Where(r => r.TypePay == eTypePay.Card).Sum(r => receipt.Amount);
                Res.PaymentType = (SumCash > 0 && SumCard > 0 ? PaymentType.Card | PaymentType.Card : (SumCash == 0 && SumCard == 0 ? PaymentType.None : (SumCash > 0 ? PaymentType.Cash : PaymentType.Card)));
                Res.PaymentInfo = PaymentToReceiptPayment(receiptMID.Payment.First());
            }

            if (receiptMID.ReceiptEvent != null && receiptMID.ReceiptEvent.Count() > 0)
                Res.ReceiptEvents = receiptMID.ReceiptEvent.Select(r => GetReceiptEvent(r)).ToList();
            if (pIsDetail)
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
                        var QuantityDiscount = OtherPromotion.Sum(r => r.Quantity);
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
                        el.PriceDealer = p.Price;
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
                PhoneNumber = parClient.MainPhone
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
        public ModelMID.Receipt ReceiptViewModelToReceipt(Guid pTerminalId, ReceiptViewModel pReceiptRVM)
        {

            if (pReceiptRVM == null)
                return null;
            RefundReceiptViewModel RefundRVM = null;
            if (pReceiptRVM is RefundReceiptViewModel)
                RefundRVM = (RefundReceiptViewModel)pReceiptRVM;


            var receipt = new ModelMID.Receipt(RefundRVM == null ? new IdReceipt(pReceiptRVM.Id) : Bl.GetNewIdReceipt(Global.GetIdWorkplaceByTerminalId(pTerminalId)))
            {
                //ReceiptId  = pReceiptRVM.Id,
                StateReceipt = (string.IsNullOrEmpty(pReceiptRVM.FiscalNumber) ? (pReceiptRVM.PaymentInfo != null ? eStateReceipt.Pay : eStateReceipt.Prepare) : eStateReceipt.Print),
                TypeReceipt = (RefundRVM == null ? eTypeReceipt.Sale : eTypeReceipt.Refund),
                NumberReceipt = pReceiptRVM.FiscalNumber,
                /* Status = (pReceiptRVM.SumCash > 0 || pReceiptRVM.SumCreditCard > 0
                     ? ReceiptStatusType.Paid
                     : ReceiptStatusType.Created), //!!!TMP Треба врахувати повернення*/
                TerminalId = pReceiptRVM.TerminalId,
                SumReceipt = pReceiptRVM.Amount, //!!!TMP Сума чека.
                SumDiscount = pReceiptRVM.Discount,
                //!!TMP TotalAmount = pReceiptRVM.SumReceipt - pReceiptRVM.SumBonus,
                ///CustomerId = new Client(pReceiptRVM.CodeClient).ClientId,
                DateCreate = pReceiptRVM.CreatedAt,
                DateReceipt = (pReceiptRVM.UpdatedAt == default(DateTime) ? DateTime.Now : pReceiptRVM.UpdatedAt)
                //ReceiptItems=
                //Customer /// !!!TMP Модель клієнта
                //PaymentInfo

            };

            if (RefundRVM != null)
                receipt.RefundId = (RefundRVM.IdPrimary == null ? null : new IdReceipt(RefundRVM.IdPrimary));

            if (pReceiptRVM.PaymentInfo != null)
                receipt.Payment = new Payment[] { ReceiptPaymentToPayment(receipt, pReceiptRVM.PaymentInfo) };

            if (pReceiptRVM.ReceiptItems != null)
                receipt.Wares = pReceiptRVM.ReceiptItems.Select(r => GetReceiptWaresFromReceiptItem(receipt, r));

            //if(pReceiptRVM.ReceiptEvents!=null)
            //    receipt.ReceiptEvent= pReceiptRVM.ReceiptEvent


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
                Price = (receiptItem.FullPrice > 0 ? receiptItem.FullPrice : receiptItem.TotalPrice) / receiptItem.ProductQuantity,
                WeightBrutto = receiptItem.ProductWeight / 1000m,
                Quantity = receiptItem.ProductQuantity,
                //                TaxGroup = Global.GetTaxGroup(receiptItem.TypeVat, receiptItem.TypeWares),               
                //FullPrice = receiptItem.Sum
                RefundedQuantity = receiptItem.RefundedQuantity

            };
            if (receiptItem.Excises != null)
                Res.ExciseStamp = String.Join(",", receiptItem.Excises.ToArray());
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
                EventType = (ModelMID.eReceiptEventType)(int)RE.EventType,
                EventName = RE.EventName,
                ProductWeight = RE.ProductWeight,
                ProductConfirmedWeight = RE.ProductConfirmedWeight,
                UserId = RE.UserId,
                UserName = RE.UserName,
                CreatedAt = RE.CreatedAt,
                ResolvedAt = RE.ResolvedAt,
                RefundAmount = RE.RefundAmount,
                FiscalNumber = RE.FiscalNumber,
                PaymentType = (ModelMID.eTypePayment)(int)RE.PaymentType,
                TotalAmount = RE.TotalAmount
            };

        }

        /// <summary>
        /// Convert ReceiptPayment->Payment
        /// </summary>
        /// <param name="pRP"></param>
        /// <returns></returns>
        public Payment ReceiptPaymentToPayment(IdReceipt parIdReceipt, ReceiptPayment pRP)
        {
            return new Payment(parIdReceipt)
            {
                TypePay = (eTypePay)(int)pRP.PaymentType,
                SumPay = pRP.PayIn,
                NumberReceipt = pRP.InvoiceNumber, //parRP.TransactionId,
                NumberCard = pRP.CardPan,
                CodeAuthorization = pRP.TransactionCode, //RRN
                NumberTerminal = pRP.PosTerminalId,
                NumberSlip = pRP.PosAuthCode, //код авторизації
                PosPaid = pRP.PosPaid,
                PosAddAmount = pRP.PosAddAmount,
                DateCreate = pRP.CreatedAt,
                CardHolder = pRP.CardHolder,
                IssuerName = pRP.IssuerName,
                Bank = pRP.Bank,
                TransactionId = pRP.TransactionId


            };
        }

        public ReceiptPayment PaymentToReceiptPayment(Payment pRP)
        {
            return new ReceiptPayment()
            {
                PaymentType = (PaymentType)(int)pRP.TypePay,
                PayIn = pRP.SumPay,
                InvoiceNumber = pRP.NumberReceipt,
                CardPan = pRP.NumberCard,
                TransactionCode = pRP.CodeAuthorization, //RRN
                PosTerminalId = pRP.NumberTerminal,
                PosAuthCode = pRP.NumberSlip, //код авторизації
                PosPaid = pRP.PosPaid,
                PosAddAmount = pRP.PosAddAmount,
                CardHolder = pRP.CardHolder,
                IssuerName = pRP.IssuerName,
                Bank = pRP.Bank,
                TransactionId = pRP.TransactionId,
                CreatedAt = pRP.DateCreate
            };
        }



    }
}
