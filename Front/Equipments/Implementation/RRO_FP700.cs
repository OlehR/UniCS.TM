using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using Front.Equipments.Virtual;
using ModernExpo.SelfCheckout.Devices.FP700;
using ModernExpo.SelfCheckout.TerminalAdmin.DAL.DataControllers;
using Microsoft.Extensions.Logging;
using ModelMID.DB;
using ModelMID;
using System.Threading.Tasks;
using ModernExpo.SelfCheckout.Entities.FiscalPrinter;
using ModernExpo.SelfCheckout.Entities.ViewModels;
using SharedLib;
using System.Collections.Generic;

using Receipt = ModernExpo.SelfCheckout.Entities.Models.Receipt;
using ModernExpo.SelfCheckout.Entities.Enums;
using ModernExpo.SelfCheckout.Entities.Models;

namespace Front.Equipments
{
    internal class RRO_FP700 : Rro
    {
        Fp700 Fp700;

        public RRO_FP700(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) : base(pEquipment, pConfiguration, eModelEquipment.FP700, pLoggerFactory, pActionStatus)
        {
            Fp700DataController fp = new Fp700DataController(pConfiguration);
            Fp700 = new Fp700(pConfiguration, fp);
        }


        public override bool OpenWorkDay()
        {
            throw new NotImplementedException();
        }

        public override LogRRO PrintCopyReceipt(int parNCopy = 1)
        {
           var res= Fp700.CopyReceipt();

            return null;
            
        }

        public override async Task<LogRRO> PrintZAsync(IdReceipt pIdR)
        {

            bool res = Fp700.ZReport();
            return null;//throw new NotImplementedException();
        }

        public override Task<LogRRO> PrintXAsync(IdReceipt pIdR)
        {
            bool res = Fp700.XReport();
            return null;//throw new NotImplementedException();
        }


        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        public override async Task<LogRRO> MoveMoneyAsync(decimal pSum, IdReceipt pIdR = null)
        {
            var d = new MoneyMovingModel() { Sum = pSum };
            Fp700.MoneyMoving(d);
            return null;//throw new NotImplementedException();
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        public override async Task<LogRRO> PrintReceiptAsync(ModelMID.Receipt pR)
        {
            var d = GetReceiptViewModel(pR);
            if(pR.TypeReceipt==eTypeReceipt.Sale) Fp700.PrintReceipt(d);
            else
                Fp700.ReturnReceipt(d);
            return null; //throw new NotImplementedException();
        }


        public override bool PutToDisplay(string ptext)
        {
            throw new NotImplementedException();
        }

        public override bool PeriodZReport(DateTime pBegin, DateTime pEnd, bool IsFull = true)
        {
            Fp700.FullReportByDate(pBegin, pEnd);
            return false;
        }

        public override async Task<LogRRO> PrintNoFiscalReceiptAsync(IEnumerable<string> pR)
        {
            List<ReceiptText> d = pR.Select(el => new ReceiptText() { Text = el,RenderType = RenderAs.Text }).ToList();
            Fp700.PrintSeviceReceipt(d);
            return null; //throw new NotImplementedException();
        }


        public ReceiptViewModel GetReceiptViewModel(ModelMID.Receipt receiptMID)
        {
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
            var listReceiptItem = GetReceiptItem(receiptMID.Wares, true); //GetReceiptItem(pReceipt);
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
           // if (pIsDetail) !!!!TMP
              //  Bl.GenQRAsync(receiptMID.Wares);
            return Res;
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

        public ReceiptPayment PaymentToReceiptPayment(Payment pRP)
        {
            return new ReceiptPayment()
            {
                PaymentType = (PaymentType)(int)pRP.TypePay,
                PosPayIn = pRP.SumPay,
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

        private ModernExpo.SelfCheckout.Entities.Models.ReceiptEvent GetReceiptEvent(ModelMID.DB.ReceiptEvent RE)
        {
            return new ModernExpo.SelfCheckout.Entities.Models.ReceiptEvent()
            {
                Id = RE.Id,
                MobileDeviceId = RE.MobileDeviceId,
                ReceiptId = RE.ReceiptId,
                ReceiptItemId = RE.ReceiptItemId,
                ProductName = RE.ProductName,
                EventType = (ReceiptEventType)(int)RE.EventType,
                EventName = RE.EventName,
                ProductWeight = RE.ProductWeight,
                ProductConfirmedWeight = RE.ProductConfirmedWeight,
                UserId = RE.UserId,
                UserName = RE.UserName,
                CreatedAt = RE.CreatedAt,
                ResolvedAt = RE.ResolvedAt,
                RefundAmount = RE.RefundAmount,
                FiscalNumber = RE.FiscalNumber,
                PaymentType = (PaymentType)(int)RE.PaymentType,
                TotalAmount = RE.TotalAmount
            };

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
            var varTags = (receiptWares.TypeWares > 0 || receiptWares.LimitAge > 0 || (!receiptWares.IsWeight && receiptWares.WeightBrutto == 0) || (receiptWares.WeightFact == -1) || (receiptWares.IsMultiplePrices))
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
                //Code = receiptWares.CodeWares,
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
                TaxGroup = Global.GetTaxGroup(receiptWares.TypeVat, (int)receiptWares.TypeWares),
                Barcode = receiptWares.TypeWares > 0 ? receiptWares.BarCode : null,
                //FullPrice = receiptWares.Sum
                //RefundedQuantity = receiptWares.RefundedQuantity,
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

    }



}