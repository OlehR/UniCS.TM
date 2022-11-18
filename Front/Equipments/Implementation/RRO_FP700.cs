using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using Front.Equipments.Virtual;
using Microsoft.Extensions.Logging;
using ModelMID.DB;
using ModelMID;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.Text;
using System.Timers;
using Utils;
using Front.Equipments.FP700;
using Dapper;
using System.Data.SQLite;

using ModernExpo.SelfCheckout.Entities.FiscalPrinter;
using ModernExpo.SelfCheckout.Entities.ViewModels;
using ModernExpo.SelfCheckout.Entities.Enums;
using ModernExpo.SelfCheckout.Entities.Models;
using ModernExpo.SelfCheckout.Utils;

using Receipt = ModernExpo.SelfCheckout.Entities.Models.Receipt;
using System.Threading;
using System.Xml.Linq;


namespace Front.Equipments
{


    internal class RRO_FP700 : Rro
    {
        Fp700 Fp700;
        object Lock = new();
        TimeSpan TimeOut = TimeSpan.FromMilliseconds(500);
        bool LockTaken = false;

        public RRO_FP700(Equipment pEquipment, IConfiguration pConfiguration, ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) : base(pEquipment, pConfiguration, eModelEquipment.FP700, pLoggerFactory, pActionStatus)
        {
            try
            {
                ILogger<Fp700> logger = pLoggerFactory?.CreateLogger<Fp700>();
                Fp700DataController fp = new Fp700DataController(pConfiguration);
                Fp700 = new Fp700(pConfiguration, fp, logger, pActionStatus);
                Fp700.Init();
                State = Fp700.IsReady ? eStateEquipment.On : eStateEquipment.Error;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                State = eStateEquipment.Error;
            }
        }

        public override bool OpenWorkDay() { return true; }

        public override LogRRO PrintCopyReceipt(int parNCopy = 1)
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Before Lock");
            lock (Lock)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock");
                var res = Fp700.CopyReceipt();
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock End");
            }
            return new LogRRO(new IdReceipt() { CodePeriod = Global.GetCodePeriod(), IdWorkplace = Global.IdWorkPlace }) { TypeOperation = eTypeOperation.CopyReceipt, TypeRRO = Type.ToString(), FiscalNumber = Fp700.GetLastZReportNumber() };
        }

        public override LogRRO PrintZ(IdReceipt pIdR)
        {
            string res;
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Before Lock");
            lock (Lock)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock");
                res = Fp700.ZReport();
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock End");

            }
            return new LogRRO(pIdR) { TypeOperation = eTypeOperation.ZReport, TypeRRO = Type.ToString(), FiscalNumber = Fp700.GetLastZReportNumber(), TextReceipt = res };
        }

        public override LogRRO PrintX(IdReceipt pIdR)
        {
            string res;
            try
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock");
                Monitor.TryEnter(Lock, TimeOut, ref LockTaken);
                if (LockTaken)
                {
                    res = Fp700.XReport();
                }
            }
            finally
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Lock End Locked=>{LockTaken}");
                // Ensure that the lock is released.
                if (LockTaken)
                {
                    Monitor.Exit(Lock);
                }
            }
            if (LockTaken)
                return new LogRRO(pIdR) { TypeOperation = eTypeOperation.XReport, TypeRRO = Type.ToString(), FiscalNumber = Fp700.GetLastZReportNumber(), TextReceipt = res };
            else
                return new LogRRO(pIdR) { TypeOperation = eTypeOperation.XReport, TypeRRO = Type.ToString(),  CodeError = -1, Error = "Операція заблокована. Спрбуйте повторно." };

            /*            
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Before Lock");
            lock (Lock)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock");
                res = Fp700.XReport();
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock End");
            }
            return new LogRRO(pIdR) { TypeOperation = eTypeOperation.XReport, TypeRRO = Type.ToString(), FiscalNumber = Fp700.GetLastZReportNumber(), TextReceipt = res };
        */
        }

        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        public override LogRRO MoveMoney(decimal pSum, IdReceipt pIdR = null)
        {
            var d = new MoneyMovingModel() { Sum = pSum };
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Before Lock");
            lock (Lock)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock");
                Fp700.MoneyMoving(d);
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock End");
            }
            return new LogRRO(pIdR) { TypeOperation = pSum > 0 ? eTypeOperation.MoneyIn : eTypeOperation.MoneyIn, TypeRRO = Type.ToString(), FiscalNumber = Fp700.GetLastZReportNumber() };
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        public override LogRRO PrintReceipt(ModelMID.Receipt pR)
        {
            string FiscalNumber = null;
            List<ReceiptText> Comments = null;
            if (pR?.ReceiptComments?.Count() > 0)
                Comments = pR.ReceiptComments.Select(r => new ReceiptText() { Text = r, RenderType = RenderAs.Text }).ToList();
            var d = GetReceiptViewModel(pR);
            //FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Before Lock");
            try
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock");
                Monitor.TryEnter(Lock, TimeOut, ref LockTaken);
                if (LockTaken)
                {                   
                    if (pR.TypeReceipt == eTypeReceipt.Sale) FiscalNumber = Fp700.PrintReceipt(d, Comments);
                    else
                        FiscalNumber = Fp700.ReturnReceipt(d);                    
                    pR.NumberReceipt = FiscalNumber;                    
                }
            }
            finally
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Lock End Locked=>{LockTaken}");
                // Ensure that the lock is released.
                if (LockTaken)
                {
                    Monitor.Exit(Lock);
                }
            }
            if (LockTaken)
                return new LogRRO(pR) { TypeOperation = pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund, TypeRRO = Type.ToString(), FiscalNumber = FiscalNumber, SUM = pR.SumReceipt - pR.SumBonus, };
            else
                return new LogRRO(pR) { TypeOperation = pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund, TypeRRO = Type.ToString(),CodeError= -1,Error="Операція заблокована. Спрбуйте повторно."};
        }


        public override bool PutToDisplay(string ptext)
        {
            return true;
        }

        public override bool PeriodZReport(DateTime pBegin, DateTime pEnd, bool IsFull = true)
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Before Lock");
            lock (Lock)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock");
                return Fp700.FullReportByDate(pBegin, pEnd,IsFull);
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock End");
            }
        }

        public override LogRRO PrintNoFiscalReceipt(IEnumerable<string> pR)
        {
            List<ReceiptText> d = pR.Select(el => new ReceiptText() { Text = el.StartsWith("QR=>") ? el.SubString(4) : el, RenderType = el.StartsWith("QR=>") ? RenderAs.QR : RenderAs.Text }).ToList();

            try
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock");
                Monitor.TryEnter(Lock, TimeOut, ref LockTaken);
                if (LockTaken)
                {
                    Fp700.PrintSeviceReceipt(d);
                }
            }
            finally
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Lock End Locked=>{LockTaken}");
                // Ensure that the lock is released.
                if (LockTaken)
                {
                    Monitor.Exit(Lock);
                }
            }
            if (LockTaken)
                return new LogRRO(new IdReceipt() { CodePeriod = Global.GetCodePeriod(), IdWorkplace = Global.IdWorkPlace }) { TypeOperation = eTypeOperation.NoFiscalReceipt, TypeRRO = Type.ToString(), JSON = pR.ToJSON() };
            else
                return new LogRRO(new IdReceipt() { CodePeriod = Global.GetCodePeriod(), IdWorkplace = Global.IdWorkPlace }) { TypeOperation = eTypeOperation.NoFiscalReceipt, TypeRRO = Type.ToString(), CodeError = -1, Error = "Операція заблокована. Спрбуйте повторно." };

            /*
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Before Lock");
            lock (Lock)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock");
                Fp700.PrintSeviceReceipt(d);
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock End");
            }
            return new LogRRO(new IdReceipt() { CodePeriod = Global.GetCodePeriod(), IdWorkplace = Global.IdWorkPlace }) { TypeOperation = eTypeOperation.NoFiscalReceipt, TypeRRO = Type.ToString(), JSON = pR.ToJSON() };*/
        }

        public override StatusEquipment TestDevice()
        {
            try
            {
                DeviceConnectionStatus res;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Before Lock");
                lock (Lock)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock");
                    res = Fp700.TestDeviceSync();
                    State = eStateEquipment.On;
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock End");
                }
                return new StatusEquipment() { TextState = res.ToString() };
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                State = eStateEquipment.Error;
                return new StatusEquipment() { State = -1, TextState = e.Message };
            }
        }

        public override string GetDeviceInfo()
        {
            try
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Before Lock");
                lock (Lock)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock");
                    return Fp700.GetInfoSync();
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock End");
                }
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return e.Message;
            }
        }

        override public bool ProgramingArticle(IEnumerable<ReceiptWares> pRW)
        {
            if (pRW != null && pRW.Count() > 0)
            {
                var xx = GetReceiptItem(pRW, true);
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Before Lock");
                lock (Lock)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock");
                    Fp700.SetupArticleTable(xx);
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock End");
                }
            }
            return true;
        }

        public override string GetTextLastReceipt()
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Before Lock");
            lock (Lock)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock");
                var r = Fp700.GetLastReceiptNumber();
                return Fp700.KSEFGetReceipt(r);
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock End");
            }
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
            Res.Customer = GetCustomerViewModelByClient(receiptMID.Client);
            Res.Cashier = receiptMID.NameCashier;
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
                        var PVM = GetProductViewModel(el);
                        Res.Add(PVM.ToReceiptItem());
                    }
                    el.SumDiscount = 0;
                    int i = 1;
                    decimal FullQuantity = el.Quantity;
                    foreach (var p in PromotionPrice)
                    {
                        ReceiptWares c = el.Clone() as ReceiptWares;
                        if (c != null)
                        {
                            if (p.Quantity <= FullQuantity)
                            {
                                c.Quantity = p.Quantity;
                                FullQuantity -= p.Quantity;
                            }
                            else
                            {
                                c.Quantity = FullQuantity;
                                FullQuantity = 0;
                            }

                            c.Price = p.Price;
                            c.PriceDealer = p.Price;
                            c.Order = i++;
                            if (c.Quantity > 0)
                            {
                                var PVM = GetProductViewModel(c);
                                if (PVM.Excises == null)
                                    PVM.Excises = new();
                                Res.Add(PVM.ToReceiptItem());
                            }
                        }
                    }
                }
                else
                {
                    var PVM = GetProductViewModel(el);
                    if (PVM.Excises == null)
                        PVM.Excises = new();
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
                Weight = (receiptWares.IsWeight ? Convert.ToDouble(receiptWares.Quantity * 1000m) : (receiptWares.WeightBrutto == 0m ? 100000 : Convert.ToDouble(receiptWares.WeightBrutto))),
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
                TaxGroup = receiptWares.TaxGroup,//Global.GetTaxGroup(receiptWares.TypeVat, (int)receiptWares.TypeWares),
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
    }
}

namespace Front.Equipments.FP700
{
    public class Fp700 : IDisposable //IFiscalPrinter,   IBaseDevice,
    {
        private readonly Fp700DataController _fp700DataController;
        private readonly IConfiguration _configuration;
        private readonly SerialPortStreamWrapper _serialDevice;
        private PrinterStatus _currentPrinterStatus;
        private volatile bool _isReady = true;
        private volatile bool _isWaiting = true;
        private volatile bool _isError;
        private volatile bool _hasCriticalError;
        private int _sequenceNumber = 90;
        private readonly ILogger<Fp700> _logger;
        //private readonly PrinterUtils _printerUtils;
        private string DateFormat = "dd-MM-yy HH:mm:ss";
        private Dictionary<Command, Action<string>> _commandsCallbacks;
        private readonly List<byte> _packageBuffer = new List<byte>();
        private readonly Timer _packageBufferTimer;
        private const string ReportDateFormat = "ddMMyy";


        private string _port => _configuration["Devices:Fp700:Port"];

        private int _baudRate => _configuration.GetValue<int>("Devices:Fp700:BaudRate");

        private int _tillNumber => _configuration.GetValue<int>("Devices:Fp700:TillNumber");

        private int _operatorCode => 1;

        private string _operatorPassword => _configuration["Devices:Fp700:OperatorPassword"];

        private string _adminPassword => _configuration["Devices:Fp700:AdminPassword"];

        private int _maxItemLength => _configuration.GetValue<int>("Devices:Fp700:MaxItemLength");

        public bool IsZReportAlreadyDone { get; private set; }

        //public Action<IFiscalPrinterResponse> OnFiscalPrinterResponse { get; set; }

        //public Action<DeviceLog> OnDeviceWarning { get; set; }

        public bool IsReady
        {
            get
            {
                SerialPortStreamWrapper serialDevice = _serialDevice;
                return serialDevice != null && serialDevice.IsOpen;
            }
        }

        Action<StatusEquipment> ActionStatus { get; set; }
       
        public Fp700(
          IConfiguration configuration,
          Fp700DataController fp700DataController,
          ILogger<Fp700> logger = null,
          Action<StatusEquipment> pActionStatus = null
           // ,PrinterUtils printerUtils = null
            )
        {
            _fp700DataController = fp700DataController;
            _configuration = configuration;
            _logger = logger;
            ActionStatus = pActionStatus;
            //_printerUtils = printerUtils;
            _commandsCallbacks = new Dictionary<Command, Action<string>>();
            _currentPrinterStatus = new PrinterStatus();
            SerialPortStreamWrapper portStreamWrapper = new SerialPortStreamWrapper(_port, _baudRate, onReceivedData: new Func<byte[], bool>(OnDataReceived));
            portStreamWrapper.Encoding = Encoding.GetEncoding(1251);
            _serialDevice = portStreamWrapper;
            _packageBufferTimer = new Timer();
            _packageBufferTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            _packageBufferTimer.Interval = 5000.0;
            _packageBufferTimer.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (_packageBuffer.Count > 0)
                _packageBuffer.Clear();
            _packageBufferTimer.Stop();
        }        

        public DeviceConnectionStatus Init()
        {
            try
            {
                if (_serialDevice.PortName == null || _serialDevice.BaudRate == 0)
                    return DeviceConnectionStatus.InitializationError;
                ILogger<Fp700> logger = _logger;
                if (logger != null)
                    logger.LogDebug("Fp700 init started");
                ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.Init, "[FP700] - Start Initialization") 
                                                  { Status =eStatusRRO.Init });                 
                CloseIfOpened();
                _serialDevice.Open();
                ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.Init, "[FP700] - Get info about printer")
                { Status = eStatusRRO.Init });
              
                string infoSync = GetInfoSync();
                ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.Init, $"[FP700] - Initialization result {infoSync}")
                { Status = eStatusRRO.Init });
                
                if (string.IsNullOrEmpty(infoSync))
                {
                    ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.Error, "Cannot read data from printer")
                    { Status = eStatusRRO.Error,IsСritical=true });                    
                    return DeviceConnectionStatus.InitializationError;
                }
                int num = IsZReportDone() ? 1 : 0;
                if (num == 0)
                {
                    ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.Error, "[FP700] - Should end previous day and create Z-Report")
                    { Status = eStatusRRO.Error, IsСritical = true });                    
                }
                ClearDisplay();
                return (num & (OnSynchronizeWaitCommandResult(Command.PaperCut) ? 1 : 0)) != 0 ? DeviceConnectionStatus.Enabled : DeviceConnectionStatus.InitializationError;
            }
            catch (Exception ex)
            {
                ILogger<Fp700> logger = _logger;
                if (logger != null)
                    logger.LogError(ex, ex.Message);
                ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.Error, "Device not connected")
                { Status = eStatusRRO.Error, IsСritical = true });                
                return DeviceConnectionStatus.NotConnected;
            }
        }

        public string GetInfoSync()
        {
            try
            {
                DiagnosticInfo diagnosticInfo = GetDiagnosticInfo();
                DocumentNumbers lastNumbers = GetLastNumbers();
                if (diagnosticInfo == null || lastNumbers == null)
                    throw new Exception("Fp700 getInfo error");
                return _currentPrinterStatus.TextError + "Model: " + diagnosticInfo.Model + "\n" + "SoftVersion: " + diagnosticInfo.SoftVersion + "\n" + string.Format("SoftReleaseDate: {0}\n", (object)diagnosticInfo.SoftReleaseDate) + "SerialNumber: " + diagnosticInfo.SerialNumber + "\n" + "RegistrationNumber: " + diagnosticInfo.FiscalNumber + "\n" + string.Format("BaudRate: {0}\n", (object)_serialDevice.BaudRate) + "ComPort: " + _serialDevice.PortName + "\n" + "LastDocumentNumber: " + lastNumbers.LastDocumentNumber + "\n" + "LastReceiptNumber: " + lastNumbers.LastFiscalDocumentNumber + "\n" + "LastZReportNumber: " + GetLastZReportNumber() + "\n" + string.Format("IsZReportDone: {0}\n", (object)IsZReportDone()) + string.Format("CurentTime: {0}\n", (object)(GetCurrentFiscalPrinterDate() ?? DateTime.MinValue));
            }
            catch (Exception ex)
            {
                _logger?.LogDebug("Fp700 getInfo error");
                _logger?.LogError(ex, ex.Message);
                ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.Error, "Get info error" + _currentPrinterStatus.TextError)
                                                   { Status = eStatusRRO.Error, IsСritical = true });
                return (string)null;
            }
        }

        public Task<string> GetInfo() => Task.Run<string>((Func<string>)(() => GetInfoSync()));

        public DeviceConnectionStatus GetDeviceStatusSync()
        {
            try
            {
                _logger?.LogDebug("Fp700 init started");
                CloseIfOpened();
                _logger?.LogDebug("Fp700 PORT " + _serialDevice.PortName);
                _logger?.LogDebug(string.Format("Fp700 BAUD {0}", (object)_serialDevice.BaudRate));
                if (_serialDevice.PortName == null || _serialDevice.BaudRate == 0)
                    return DeviceConnectionStatus.InitializationError;
                _serialDevice.Open();
                _logger?.LogDebug("Fp700 after open");
                return GetDiagnosticInfo() == null ? DeviceConnectionStatus.NotConnected : DeviceConnectionStatus.Enabled;
            }
            catch (Exception ex)
            {
                _logger?.LogDebug("Fp700 open error");                
                _logger?.LogError(ex, ex.Message);
                ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.Error, _currentPrinterStatus.TextError+ex.Message)
                { Status = eStatusRRO.Error, IsСritical = true });
                
                return DeviceConnectionStatus.NotConnected;
            }
        }

        public Task<DeviceConnectionStatus> GetDeviceStatus() => Task.Run<DeviceConnectionStatus>((Func<DeviceConnectionStatus>)(() => GetDeviceStatusSync()));

        public DeviceConnectionStatus TestDeviceSync()
        {
            try
            {
                _hasCriticalError = false;
                ObliterateFiscalReceipt();
                if (!ReopenPort())
                    return DeviceConnectionStatus.InitializationError;
                ILogger<Fp700> logger = _logger;
                if (logger != null)
                    logger.LogDebug("Fp700 after open");
                ClearDisplay();
                IsZReportDone();
                if (SendPackage(Command.PrintDiagnosticInformation))
                    return DeviceConnectionStatus.Enabled;
                return _serialDevice.IsOpen ? DeviceConnectionStatus.InitializationError : DeviceConnectionStatus.NotConnected;
            }
            catch (Exception ex)
            {
                _logger?.LogDebug("Fp700 open error");
                _logger?.LogError(ex, ex.Message);
                ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.Error, ex.Message)
                { Status = eStatusRRO.Error, IsСritical = true });
                return DeviceConnectionStatus.NotConnected;
            }
        }

        private bool ReopenPort()
        {
            ILogger<Fp700> logger1 = _logger;
            if (logger1 != null)
                logger1.LogDebug("Fp700 init started");
            CloseIfOpened();
            ILogger<Fp700> logger2 = _logger;
            if (logger2 != null)
                logger2.LogDebug("Fp700 PORT " + _serialDevice.PortName);
            ILogger<Fp700> logger3 = _logger;
            if (logger3 != null)
                logger3.LogDebug(string.Format("Fp700 BAUD {0}", (object)_serialDevice.BaudRate));
            if (_serialDevice.PortName == null || _serialDevice.BaudRate == 0)
                return false;
            _serialDevice.Open();
            _isReady = true;
            _isError = false;
            _isWaiting = false;
            return true;
        }

        public Task<DeviceConnectionStatus> TestDevice() => Task.Run<DeviceConnectionStatus>(new Func<DeviceConnectionStatus>(TestDeviceSync));

        public string PrintReceipt(ReceiptViewModel receipt, List<ReceiptText> Comments = null)
        {
            ObliterateFiscalReceipt();
            string s = GetLastReceiptNumber();

            _logger?.LogDebug("{START_PRINTING}");
            _logger?.LogDebug("{LAST_RECEIPTNUMBER}" + s);
            if (string.IsNullOrEmpty(s))
                s = "0";
            int result1;
            int.TryParse(s, out result1);
            OpenReceipt(receipt?.Cashier);
            if (Comments != null && Comments.Count() > 0)
                PrintFiscalComments(Comments);
            FillUpReceiptItems(receipt.ReceiptItems);
            if (!PayReceipt(receipt))
            {
                ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.Error, "Check was not printed")
                { Status = eStatusRRO.Error, IsСritical = true });                
            }
            CloseReceipt();
            ClearDisplay();
            int result2;
            if (!int.TryParse(GetLastReceiptNumber(), out result2))
                return (string)null;
            _logger?.LogDebug($"[ FP700 ] newLastReceipt = {result2} / lastReceipt = {result1}");
            string str = result2 > result1 ? result2.ToString() : (string)null;
            if (str != null)
                return str;
            ObliterateFiscalReceipt();
            // var rrrrr=KSEFGetReceipt(str);
            return str;
        }

        public bool OpenReceipt(string pCashier=null)
        {
            if (!string.IsNullOrEmpty(pCashier))
            {
                string str = pCashier;
                if (str.Length >= 24)
                    str = str.Remove(23);
                OnSynchronizeWaitCommandResult(Command.SetOperatorName, string.Format("{0},{1},{2}", (object)_operatorCode, (object)_operatorPassword, (object)str));
            }
            ClearDisplay();
            OnSynchronizeWaitCommandResult(Command.OpenFiscalReceipt, string.Format("{0},{1},{2}", (object)_operatorCode, (object)_operatorPassword, (object)_tillNumber), (Action<string>)(res => Console.WriteLine(res)));
            return true;
        }

        public bool FillUpReceiptItems(List<ReceiptItem> receiptItems)
        {
            receiptItems = UngroupByExcise(receiptItems);
            List<ProductArticle> productArticleList = receiptItems != null && receiptItems.Count != 0 ? SetupArticleTable(receiptItems) : throw new Exception("Cannot register clear receipt items");
            for (int index = 0; index < productArticleList.Count; ++index)
            {
                ProductArticle productArticle = productArticleList[index];
                ReceiptItem receiptItem = receiptItems[index];
                string str1 = string.Empty;
                Decimal num1;
                if (productArticle.Price != (double)receiptItem.ProductPrice)
                {
                    num1 = receiptItem.ProductPrice;
                    str1 = "#" + num1.ToString((IFormatProvider)CultureInfo.InvariantCulture);
                }
                // ISSUE: variable of a boxed type
                //__Boxed<int> plu = (ValueType)productArticle.PLU;
                var plu = productArticle.PLU;
                num1 = receiptItem.ProductQuantity;
                string str2 = num1.ToString((IFormatProvider)CultureInfo.InvariantCulture);
                string str3 = str1;
                string data = string.Format("{0}*{1}{2}", (object)plu, (object)str2, (object)str3);
                if (receiptItem.Discount != 0M)
                {
                    string str4 = receiptItem.Discount > 0M ? "-" : "+";
                    string str5 = data;
                    string str6 = str4;
                    num1 = Math.Abs(receiptItem.Discount);
                    string str7 = num1.ToString((IFormatProvider)CultureInfo.InvariantCulture);
                    data = str5 + ";" + str6 + str7;
                }
                if (!string.IsNullOrWhiteSpace(receiptItem.ProductBarcode))
                    data = data + "&" + receiptItem.ProductBarcode;
                if (receiptItem.Excises != null && receiptItem.Excises.Count == 1 && !string.IsNullOrWhiteSpace(receiptItem.Excises[0]))
                    data = data + "!" + receiptItem.Excises[0];
                int num2 = OnSynchronizeWaitCommandResult(Command.RegisterProductInReceiptWithDisplay, data, (Action<string>)(res => { })) ? 1 : 0;
                string errCode = string.Empty;
                OnSynchronizeWaitCommandResult(Command.GetLastError, onResponseCallback: ((Action<string>)(res => errCode = res)));
                _logger?.LogDebug("[ FP700 ] FillUpItemLastCode: " + errCode);
                if (num2 == 0)
                    throw new Exception("Registed product in receipt");
            }
            return true;
        }

        public bool PrintFiscalComments(List<ReceiptText> comments)
        {
            foreach (ReceiptText comment in comments)
            {
                if (!string.IsNullOrWhiteSpace(comment.Text))
                    OnSynchronizeWaitCommandResult(Command.PrintFiscalComment, comment.Text);
            }
            return true;
        }

        public bool PayReceipt(ReceiptViewModel receipt)
        {
            StringBuilder stringBuilder = new StringBuilder();
            ILogger<Fp700> logger1 = _logger;
            if (logger1 != null)
                logger1.LogDebug(string.Format("[FP700] PayReceipt {0}", (object)receipt?.PaymentType));
            switch (receipt.PaymentType)
            {
                case PaymentType.Card:
                    Decimal totalAmount = 0M;
                    OnSynchronizeWaitCommandResult(Command.FiscalTransactionStatus, onResponseCallback: ((Action<string>)(res =>
                    {
                        string[] strArray = res.Split(',');
                        if (strArray.Length == 1)
                            totalAmount = Decimal.Parse(res.Substring(4, 12)) / 100M;
                        else
                            totalAmount = Decimal.Parse(strArray[2]) / 100M;
                    })));
                    if (receipt.PaymentInfo == null)
                    {
                        stringBuilder.Append("D");
                        break;
                    }
                    stringBuilder.Append(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "D+{0:0.00}", (object)totalAmount));
                    stringBuilder.Append("," + receipt.PaymentInfo.TransactionCode);
                    stringBuilder.Append(",Магазин");
                    stringBuilder.Append("," + receipt.PaymentInfo.PosTerminalId);
                    stringBuilder.Append("," + (string.IsNullOrWhiteSpace(receipt.PaymentInfo.IssuerName) ? "картка" : receipt.PaymentInfo.IssuerName));
                    stringBuilder.Append(string.IsNullOrEmpty(receipt.FiscalNumber) ? ",оплата" : ",повернення");
                    stringBuilder.Append("," + receipt.PaymentInfo.CardPan);
                    stringBuilder.Append("," + receipt.PaymentInfo.PosAuthCode);
                    stringBuilder.Append(",0.00");
                    break;
                case PaymentType.Cash:
                    stringBuilder.Append("P+" + receipt.TotalAmount.ToString("F2", (IFormatProvider)CultureInfo.InvariantCulture));
                    break;
                default:
                    throw new Exception("Cannot pay receipt with incorrect payment type");
            }
            string data = string.Format("\n\t{0}", (object)stringBuilder);
            bool paySuccess = false;
            char paidCode = char.MinValue;
            int amount = 0;
            OnSynchronizeWaitCommandResult(Command.PayInfoFiscalReceipt, data, (Action<string>)(res =>
            {
                ILogger<Fp700> logger2 = _logger;
                if (logger2 != null)
                    logger2.LogDebug("[ FP700 ] PayInfoFiscalReceipt res = " + res);
                paidCode = res[0];
                int.TryParse(res.Substring(1), out amount);
                paySuccess = paidCode == 'D' && amount == 0 || paidCode == 'R';
            }));
            if (!paySuccess)
            {
                if (amount == 0)
                {
                    string errCode = string.Empty;
                    OnSynchronizeWaitCommandResult(Command.GetLastError, onResponseCallback: ((Action<string>)(res => errCode = res)));
                    ILogger<Fp700> logger3 = _logger;
                    if (logger3 != null)
                        logger3.LogDebug("[ FP700 ] GetLastError: " + errCode);
                }
                throw new Exception("NotPaid");
            }
            return paySuccess;
        }

        public string CloseReceipt()//ReceiptViewModel receipt
        {
            string res = (string)null;
            OnSynchronizeWaitCommandResult(Command.CloseFiscalReceipt, onResponseCallback: ((Action<string>)(response =>
            {
                if (string.IsNullOrEmpty(response))
                    return;
                string[] strArray = response.Split(',');
                if (strArray.Length < 3)
                    return;
                res = strArray[3];
            })));
            ClearDisplay();
            return res;
        }

        public void PrintDividerLine(bool shouldPrintBeforeFiscalInfo) => OnSynchronizeWaitCommandResult(Command.PrintDividerLine);

        public bool CopyReceipt() => OnSynchronizeWaitCommandResult(Command.FiscalReceiptCopy, "1");

        public bool MoneyMoving(MoneyMovingModel moneyMovingModel)
        {
            string str = "";
            switch (moneyMovingModel.MoneyDestination)
            {
                case MoneyMovingDestination.Input:
                    str = "+";
                    break;
                case MoneyMovingDestination.Output:
                    str = "-";
                    break;
            }
            bool res = false;
            OnSynchronizeWaitCommandResult(Command.ServiceCashInOut, str + moneyMovingModel.Sum.ToString((IFormatProvider)CultureInfo.InvariantCulture), (Action<string>)(response =>
            {
                string[] strArray = response.Split(',');
                if (strArray.Length < 4)
                    return;
                if (strArray[0].Equals("P"))
                {
                    res = true;
                }
                else
                {
                    if (!strArray[0].Equals("F"))
                        return;
                    res = false;
                }
            }));
            return res;
        }

        public bool FullReportByDate(DateTime startDate, DateTime? endDate,bool IsFull)
        {
            string str = "";
            if (endDate.HasValue)
                str = "," + endDate.Value.ToString("ddMMyy");
            return OnSynchronizeWaitCommandResult(IsFull?Command.FullReportByPeriod: Command.ShortReportByPeriod, _operatorPassword + "," + startDate.ToString("ddMMyy") + str);
        }

        public void OpenReturnReceipt(ReceiptViewModel receipt) => OnSynchronizeWaitCommandResult(Command.ReturnReceipt, string.Format("{0},{1},{2}", (object)_operatorCode, (object)_operatorPassword, (object)_tillNumber), (Action<string>)(res => _logger.LogDebug("[ FP700 ] ReturnReceipt res = " + res)));

        public string ReturnReceipt(ReceiptViewModel receipt)
        {
            ObliterateFiscalReceipt();
            List<ProductArticle> productArticleList = SetupArticleTable(receipt.ReceiptItems);
            string s = GetLastRefundReceiptNumber();
            if (string.IsNullOrEmpty(s))
                s = "0";
            int result1;
            int.TryParse(s, out result1);
            OpenReturnReceipt(receipt);
            for (int index = 0; index < productArticleList.Count; ++index)
            {
                ProductArticle productArticle = productArticleList[index];
                ReceiptItem receiptItem = receipt.ReceiptItems[index];
                string str1 = string.Empty;
                string str2 = string.Empty;
                if (productArticle.Price != (double)receiptItem.ProductPrice)
                    str1 = "#" + receiptItem.ProductPrice.ToString((IFormatProvider)CultureInfo.InvariantCulture);
                if (receiptItem.Discount != 0M)
                    str2 = ";" + (receiptItem.Discount > 0M ? "-" : "+") + receiptItem.Discount.ToString((IFormatProvider)CultureInfo.InvariantCulture);
                OnSynchronizeWaitCommandResult(Command.RegisterProductInReceipt, string.Format("{0}*{1}{2}{3}", (object)productArticle.PLU, (object)receiptItem.ProductQuantity.ToString((IFormatProvider)CultureInfo.InvariantCulture), (object)str1, (object)str2), (Action<string>)(res => _logger.LogDebug("[ FP700 ] RegisterProductInReceipt res = " + res)));
            }
            PayReceipt(receipt);
            OnSynchronizeWaitCommandResult(Command.CloseFiscalReceipt, onResponseCallback: ((Action<string>)(res => _logger.LogDebug("[ FP700 ] CloseFiscalReceipt res = " + res))));
            int result2;
            if (!int.TryParse(GetLastRefundReceiptNumber(), out result2))
                return (string)null;
            _logger?.LogDebug(string.Format("[ FP700 ] newLastReceipt = {0} / lastReceipt = {1}", (object)result2, (object)result1));
            return result2 <= result1 ? (string)null : result2.ToString();
        }

        public bool PrintSeviceReceipt(List<ReceiptText> texts)
        {
            ObliterateFiscalReceipt();
            ClearDisplay();
            OnSynchronizeWaitCommandResult(Command.OpenNonFiscalReceipt);
            foreach (ReceiptText text in texts)
            {
                switch (text.RenderType)
                {
                    case RenderAs.Text:
                        OnSynchronizeWaitCommandResult(Command.PrintNonFiscalComment, text.Text);
                        continue;
                    case RenderAs.QR:
                        OnSynchronizeWaitCommandResult(Command.PrintCode, "Q," + text.Text);
                        continue;
                    default:
                        continue;
                }
            }
            OnSynchronizeWaitCommandResult(Command.CloseNonFiscalReceipt);
            return true;
        }

        public bool OpenServiceReceipt()
        {
            ObliterateFiscalReceipt();
            ClearDisplay();
            OnSynchronizeWaitCommandResult(Command.OpenNonFiscalReceipt);
            return true;
        }

        public bool CloseServiceReceipt()
        {
            OnSynchronizeWaitCommandResult(Command.CloseNonFiscalReceipt);
            return true;
        }

        public bool PrintServiceLine(ReceiptText text)
        {
            if (string.IsNullOrEmpty(text?.Text))
                return true;
            OnSynchronizeWaitCommandResult(Command.PrintNonFiscalComment, text.Text);
            return true;
        }

        public bool PrintServiceLines(List<ReceiptText> texts)
        {
            foreach (ReceiptText text in texts)
                PrintServiceLine(text);
            return true;
        }

        public bool SetupReceipt(IFiscalReceiptConfiguration configuration)
        {
            if (!(configuration is Fp700ReceiptConfiguration receiptConfiguration))
                return false;
            SendPackage(Command.ReceiptDetailsPrintSetupAdditionalSettings, "0" + receiptConfiguration.HeaderLine1);
            SendPackage(Command.ReceiptDetailsPrintSetupAdditionalSettings, "1" + receiptConfiguration.HeaderLine2);
            SendPackage(Command.ReceiptDetailsPrintSetupAdditionalSettings, "2" + receiptConfiguration.HeaderLine3);
            SendPackage(Command.ReceiptDetailsPrintSetupAdditionalSettings, "3" + receiptConfiguration.HeaderLine4);
            SendPackage(Command.ReceiptDetailsPrintSetupAdditionalSettings, "4" + receiptConfiguration.HeaderLine5);
            SendPackage(Command.ReceiptDetailsPrintSetupAdditionalSettings, "5" + receiptConfiguration.HeaderLine6);
            SendPackage(Command.ReceiptDetailsPrintSetupAdditionalSettings, "6" + receiptConfiguration.FooterLine1);
            SendPackage(Command.ReceiptDetailsPrintSetupAdditionalSettings, "7" + receiptConfiguration.FooterLine2);
            return SendPackage(Command.ReceiptDetailsPrintSetupAdditionalSettings, string.Format("L{0}", (object)(receiptConfiguration.ShouldPrintLogo ? 1 : 0)));
        }

        public bool SetupPrinter() => true;

        public bool SetupPaperWidth(FiscalPrinterPaperWidthEnum width) => OnSynchronizeWaitCommandResult(Command.ReceiptDetailsPrintSetupAdditionalSettings, "N" + (width == FiscalPrinterPaperWidthEnum.Width80mm ? "0" : "1"));

        public bool SetupTime(DateTime time) => OnSynchronizeWaitCommandResult(Command.SetDateTime, time.ToString(DateFormat));

        public string XReport()
        {
            string Res = null;
            ObliterateFiscalReceipt();
            OnSynchronizeWaitCommandResult(Command.EveryDayReport, _operatorPassword + ",2", ((Action<string>)(response => Res = response)));

            return Res;
        }

        public string ZReport()
        {
            string Res = null;
            ObliterateFiscalReceipt();
            OnSynchronizeWaitCommandResult(Command.EveryDayReport, _operatorPassword + ",0", ((Action<string>)(response => Res = response)));
            IsZReportAlreadyDone = true;
            DeleteAllArticles();
            return Res;
        }

        public bool ArticleReport() => OnSynchronizeWaitCommandResult(Command.ArticleReport, string.Format("{0},{1}", (object)_operatorPassword, (object)ArticleReportType.S));

        private void DeleteArticle(int plu) => _fp700DataController?.DeleteArticle(plu).Wait();

        public void DeleteAllProgrammingArticles() => _fp700DataController?.DeleteAllArticles().Wait();

        public bool ObliterateFiscalReceipt()
        {
            OnSynchronizeWaitCommandResult(Command.CloseNonFiscalReceipt);
            OnSynchronizeWaitCommandResult(Command.ObliterateFiscalReceipt, onResponseCallback: ((Action<string>)(res => _logger.LogDebug("[ FP700 ] ObliterateFiscalReceipt res = " + res))));
            return true;
        }

        public string GetLastReceiptNumber() => GetLastNumbers().LastDocumentNumber;

        public string GetLastRefundReceiptNumber() => GetLastNumbers().LastDocumentNumber;

        public string GetLastZReportNumber()
        {
            string result = "";
            OnSynchronizeWaitCommandResult(Command.LastZReportInfo, "0", (Action<string>)(res =>
            {
                if (string.IsNullOrEmpty(res))
                    return;
                string[] strArray = res.Split(',');
                if (strArray.Length < 7)
                    return;
                result = strArray[0];
            }));
            return result;
        }

        private bool IsZReportDone()
        {
            bool result = false;
            OnSynchronizeWaitCommandResult(Command.ShiftInfo, onResponseCallback: ((Action<string>)(res =>
            {
                if (string.IsNullOrWhiteSpace(res))
                    return;
                switch (res[0])
                {
                    case 'F':
                        result = false;
                        break;
                    case 'P':
                        result = true;
                        string[] strArray = res.Split(',');
                        if (strArray.Length < 2)
                            break;
                        int.TryParse(strArray[1], out int _);
                        break;
                    case 'Z':
                        result = true;
                        break;
                }
            })));
            IsZReportAlreadyDone = result;
            return result;
        }

        private DocumentNumbers GetLastNumbers()
        {
            DocumentNumbers documentNumbers = new DocumentNumbers();
            OnSynchronizeWaitCommandResult(Command.LastDocumentsNumbers, onResponseCallback: ((Action<string>)(res =>
            {
                ILogger<Fp700> logger = _logger;
                if (logger != null)
                    logger.LogDebug("FP700 [GetLastNumbers] " + res);
                if (string.IsNullOrEmpty(res))
                    return;
                string[] strArray = res.Split(',');
                if (strArray.Length < 3)
                    return;
                documentNumbers.LastDocumentNumber = strArray[0];
                documentNumbers.LastFiscalDocumentNumber = strArray[1];
                documentNumbers.LastRefundFiscalDocumentNumber = strArray[2];
                if (strArray.Length > 3)
                    documentNumbers.GlobalDocumentNumber = strArray[3];
            })));
            return documentNumbers;
        }

        public DateTime? GetCurrentFiscalPrinterDate()
        {
            DateTime? currentDate = new DateTime?();
            OnSynchronizeWaitCommandResult(Command.GetDateTime, onResponseCallback: ((Action<string>)(response => currentDate = new DateTime?(DateTime.ParseExact(response, DateFormat, (IFormatProvider)CultureInfo.InvariantCulture)))));
            return currentDate;
        }

        private void DeleteAllArticles() => OnSynchronizeWaitCommandResult(Command.ArticleProgramming, "DA," + _operatorPassword, (Action<string>)(res =>
        {
            if (res.Trim().ToUpper().StartsWith("F"))
            {
                ILogger<Fp700> logger = _logger;
                if (logger == null)
                    return;
                logger.LogDebug("[Fp700] Error during articles deleting");
            }
            else
            {
                if (!res.Trim().ToUpper().StartsWith("P"))
                    return;
                _fp700DataController.DeleteAllArticles().RunAsync();
            }
        }));

        private List<ProductArticle> ReadAllArticles()
        {
            List<ProductArticle> list = new List<ProductArticle>();
            bool isEnd = false;
            OnSynchronizeWaitCommandResult(Command.ArticleProgramming, "F", (Action<string>)(res =>
            {
                if (res.Trim().ToUpper().StartsWith("F"))
                {
                    ILogger<Fp700> logger = _logger;
                    if (logger != null)
                        logger.LogDebug("[Fp700] No articles in memory");
                    isEnd = true;
                }
                else
                {
                    if (!res.Trim().ToUpper().StartsWith("P"))
                        return;
                    ILogger<Fp700> logger = _logger;
                    if (logger != null)
                        logger.LogDebug("[Fp700] Found first article");
                    res = res.Remove(0, 1);
                    string[] strArray = res.Split(',');
                    ReceiptItem receiptItem = new ReceiptItem();
                    int result1;
                    int.TryParse(strArray[0], out result1);
                    double result2;
                    double.TryParse(strArray[3], out result2);
                    list.Add(new ProductArticle()
                    {
                        Barcode = string.Empty,
                        Price = result2,
                        ProductName = strArray[8].Replace("...", ""),
                        PLU = result1
                    });
                }
            }));
            while (!isEnd)
                OnSynchronizeWaitCommandResult(Command.ArticleProgramming, "N", (Action<string>)(res =>
                {
                    if (res.Trim().ToUpper().StartsWith("F"))
                    {
                        ILogger<Fp700> logger = _logger;
                        if (logger != null)
                            logger.LogDebug("[Fp700] No articles in memory");
                        isEnd = true;
                    }
                    else
                    {
                        if (!res.Trim().ToUpper().StartsWith("P"))
                            return;
                        ILogger<Fp700> logger = _logger;
                        if (logger != null)
                            logger.LogDebug("[Fp700] Found first article");
                        res = res.Remove(0, 1);
                        string[] strArray = res.Split(',');
                        ReceiptItem receiptItem = new ReceiptItem();
                        int result3;
                        int.TryParse(strArray[0], out result3);
                        double result4;
                        double.TryParse(strArray[3], out result4);
                        list.Add(new ProductArticle()
                        {
                            Barcode = string.Empty,
                            Price = result4,
                            ProductName = strArray[8].Replace("...", ""),
                            PLU = result3
                        });
                    }
                }));
            return list;
        }

        public List<ProductArticle> SetupArticleTable(List<ReceiptItem> products)
        {
            List<ProductArticle> articles = new List<ProductArticle>();
            int firstFreeArticle = FindFirstFreeArticle();
            ILogger<Fp700> logger1 = _logger;
            if (logger1 != null)
                logger1.LogDebug("{firstPluNumber} " + firstFreeArticle.ToString());
            foreach (ReceiptItem product1 in products)
            {

                ReceiptItem product = product1;
                Task<ProductArticle> articleById = _fp700DataController?.GetArticleById(product.ProductId);

                articleById?.Wait();
                if (articleById?.Result == null)
                {
                    if (!(product.ProductPrice == 0M))
                    {
                        bool isSuccess = true;
                        int number = firstFreeArticle;
                        string str = string.Empty;
                        if (!string.IsNullOrWhiteSpace(product.Uktzed))
                            str = "^" + product.Uktzed + ",";
                        string data = string.Format("P{0}{1},1,{2}{3},{4},", (object)product.TaxGroup, (object)number, (object)str, (object)product.ProductPrice.ToString("F2", (IFormatProvider)CultureInfo.InvariantCulture), (object)_operatorPassword) + product.ProductName.LimitCharactersForTwoLines(_maxItemLength, '\t');
                        ILogger<Fp700> logger2 = _logger;
                        if (logger2 != null)
                            logger2.LogDebug("[FP700] SetupArticleTable " + data);
                        OnSynchronizeWaitCommandResult(Command.ArticleProgramming, data, (Action<string>)(res =>
                        {
                            if (res.Trim().ToUpper().Equals("F"))
                            {
                                ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.Error, "Fp700 writing article FALSE: " + res)
                                { Status = eStatusRRO.Error, IsСritical = true });
                                
                                isSuccess = false;
                                ILogger<Fp700> logger3 = _logger;
                                if (logger3 != null)
                                    logger3.LogDebug("Fp700 writing article FALSE: " + res);
                                ILogger<Fp700> logger4 = _logger;
                                if (logger4 == null)
                                    return;
                                logger4.LogDebug(string.Format("Product {{ Name :{0}, PLU={1},  Price={2}}}", (object)product.ProductName, (object)number, (object)product.ProductPrice));
                            }
                            else
                            {
                                if (!res.Trim().ToUpper().Equals("P"))
                                    return;
                                ILogger<Fp700> logger5 = _logger;
                                if (logger5 != null)
                                    logger5.LogDebug("Fp700 writing article TRUE: " + res);
                                ILogger<Fp700> logger6 = _logger;
                                if (logger6 != null)
                                    logger6.LogDebug(string.Format("PLU: {0}", (object)number));
                                _fp700DataController?.CreateOrUpdateArticle(product, number).Wait();
                                articles.Add(new ProductArticle()
                                {
                                    Barcode = product.ProductBarcode,
                                    Price = (double)product.ProductPrice,
                                    ProductName = product.ProductName,
                                    PLU = number
                                });
                            }
                        }));
                        if (!isSuccess)
                            throw new Exception("Fp700 writing article FALSE");
                        ++firstFreeArticle;
                    }
                }
                else
                    articles.Add(new ProductArticle()
                    {
                        Barcode = product.ProductBarcode,
                        Price = articleById.Result.Price,
                        ProductName = product.ProductName,
                        PLU = articleById.Result.PLU
                    });
            }
            return articles;
        }

        private void ClearDisplay()
        {
            ILogger<Fp700> logger1 = _logger;
            if (logger1 != null)
                logger1.LogDebug("Fp700 clear display start");
            OnSynchronizeWaitCommandResult(Command.ClearDisplay);
            ILogger<Fp700> logger2 = _logger;
            if (logger2 == null)
                return;
            logger2.LogDebug("Fp700 clear display finish");
        }

        private int FindFirstFreeArticle()
        {
            int firstPluNumber = 1;
            OnSynchronizeWaitCommandResult(Command.ArticleProgramming, "X", (Action<string>)(response =>
            {
                int startIndex = -1;
                for (int index = 0; index < response.Length; ++index)
                {
                    int result;
                    if (int.TryParse(response[index].ToString(), out result) && result != 0)
                    {
                        startIndex = index;
                        break;
                    }
                }
                if (startIndex == -1)
                    return;
                int.TryParse(response.Substring(startIndex), out firstPluNumber);
            }));
            return firstPluNumber;
        }

        private DiagnosticInfo GetDiagnosticInfo()
        {
            DiagnosticInfo diagnosticInfo = (DiagnosticInfo)null;
            OnSynchronizeWaitCommandResult(Command.DiagnosticInfo, onResponseCallback: ((Action<string>)(res =>
            {
                try
                {
                    if (string.IsNullOrEmpty(res))
                        return;
                    string[] strArray1 = res.Split(',');
                    if (strArray1.Length < 10)
                        return;
                    string[] strArray2 = strArray1[1].Split(' ');
                    diagnosticInfo = new DiagnosticInfo()
                    {
                        Model = strArray1[0],
                        SoftVersion = strArray2[0],
                        SoftReleaseDate = DateTime.ParseExact(strArray2[1], "ddMMMyy", (IFormatProvider)CultureInfo.InvariantCulture),
                        Check = strArray1[2],
                        Switchers = strArray1[3],
                        CountryCode = strArray1[4],
                        SerialNumber = strArray1[5],
                        FiscalNumber = strArray1[6],
                        Id_Dev = strArray1[7],
                        Id_Acq = strArray1[8],
                        Id_Sam = strArray1[9]
                    };
                }
                catch (Exception ex)
                {
                    ILogger<Fp700> logger = _logger;
                    if (logger == null)
                        return;
                    logger.LogError(ex, ex.Message);
                }
            })));
            return diagnosticInfo;
        }

        public bool CanOpenReceipt() => CheckModemStatus() && IsZReportDone();

        private bool CheckModemStatus(bool tryToReopen = false)
        {
            bool result = false;
            bool shouldThrow = false;
            OnSynchronizeWaitCommandResult(Command.StateOfDataTransmission, onResponseCallback: ((Action<string>)(res =>
            {
                if (string.IsNullOrWhiteSpace(res))
                {
                    if (!tryToReopen)
                    {
                        try
                        {
                            if (ReopenPort())
                            {
                                result = CheckModemStatus(true);
                                return;
                            }
                            shouldThrow = true;
                            return;
                        }
                        catch
                        {
                            shouldThrow = true;
                            return;
                        }
                    }
                }
                string[] strArray = res.Split(',');
                if (strArray.Length < 6)
                    return;
                result = strArray[5].EqualsIgnoreCase("ok");
            })));
            if (shouldThrow)
                throw new Exception("[FP700] Printer not ready");
            return result;
        }

        private bool OnSynchronizeWaitCommandResult(
          Command command,
          string data = "",
          Action<string> onResponseCallback = null,
          Action<Exception> onExceptionCallback = null)
        {
            bool isResultGot = false;
            if (!StaticTimer.Wait((Func<bool>)(() => _commandsCallbacks.ContainsKey(command)), 2))
                _commandsCallbacks.Remove(command);
            _commandsCallbacks.Add(command, (Action<string>)(response =>
            {
                try
                {
                    ILogger<Fp700> logger = _logger;
                    if (logger != null)
                        logger.LogDebug(string.Format("[FP700] Response for command {0}", (object)command));
                    Action<string> action = onResponseCallback;
                    if (action != null)
                        action(response);
                    isResultGot = true;
                }
                catch (Exception ex)
                {
                    Action<Exception> action = onExceptionCallback;
                    if (action == null)
                        return;
                    action(ex);
                }
                finally
                {
                    _commandsCallbacks.Remove(command);
                }
            }));
            try
            {
                if (!SendPackage(command, data))
                    return false;
                StaticTimer.Wait((Func<bool>)(() => !isResultGot));
                return isResultGot;
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("FP 700 has critical error"))
                    _commandsCallbacks.Remove(command);
                throw;
            }
        }

        public bool SendPackage(Command command, string data = "", int waitingTimeout = 10)
        {
            ILogger<Fp700> logger1 = _logger;
            if (logger1 != null)
                logger1.LogDebug("SendPackage start");
            bool flag = command != Command.ClearDisplay && command != Command.ShiftInfo && command != Command.DiagnosticInfo && command != Command.EveryDayReport && command != Command.LastDocumentsNumbers && command != Command.ObliterateFiscalReceipt && command != Command.PaperCut && command != Command.GetDateTime && command != Command.PaperPulling && command != Command.LastZReportInfo && command != Command.PrintDiagnosticInformation;
            if (!IsZReportAlreadyDone & flag)
                return false;
            if (_hasCriticalError & flag)
                throw new Exception(Environment.NewLine+"Проблема з фіскальним реєстратором FP700: " + Environment.NewLine + _currentPrinterStatus.TextError);
            if (!_serialDevice.IsOpen)
                return false;
            if (!_isReady)
            {
                ILogger<Fp700> logger2 = _logger;
                if (logger2 != null)
                    logger2.LogDebug("SendPackage printer not ready. Start waiting");
                _isReady = WaitForReady(waitingTimeout);
                ILogger<Fp700> logger3 = _logger;
                if (logger3 != null)
                    logger3.LogDebug("SendPackage printer not ready. Waiting complete");
            }
            if (!_isReady)
            {
                ILogger<Fp700> logger4 = _logger;
                if (logger4 != null)
                    logger4.LogDebug("SendPackage printer not ready. Exiting");
                return false;
            }
            ILogger<Fp700> logger5 = _logger;
            if (logger5 != null)
                logger5.LogDebug(string.Format("SendPackage executing command start : {0}", (object)command));
            ILogger<Fp700> logger6 = _logger;
            if (logger6 != null)
                logger6.LogDebug("SendPackage command  data: " + data);
            byte[] bytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding(1251), Encoding.UTF8.GetBytes(data));
            ILogger<Fp700> logger7 = _logger;
            if (logger7 != null)
                logger7.LogDebug("SendPackage command  data converted : " + Encoding.GetEncoding(1251).GetString(bytes));
            ILogger<Fp700> logger8 = _logger;
            if (logger8 != null)
                logger8.LogDebug("--------------------------------------------------------------------------------");
            byte[] buffer = new byte[218];
            int num1 = 0;
            int length = bytes.Length;
            int num2 = 0;
            if (length > 218)
                return false;
            byte[] numArray1 = buffer;
            int index1 = num1;
            int num3 = index1 + 1;
            numArray1[index1] = (byte)1;
            byte[] numArray2 = buffer;
            int index2 = num3;
            int num4 = index2 + 1;
            int num5 = (int)(byte)(36 + length);
            numArray2[index2] = (byte)num5;
            byte[] numArray3 = buffer;
            int index3 = num4;
            int num6 = index3 + 1;
            int num7 = (int)(byte)_sequenceNumber++;
            numArray3[index3] = (byte)num7;
            byte[] numArray4 = buffer;
            int index4 = num6;
            int num8 = index4 + 1;
            int num9 = (int)(byte)command;
            numArray4[index4] = (byte)num9;
            if (_sequenceNumber == 100)
                _sequenceNumber = 90;
            for (int index5 = 0; index5 < length; ++index5)
                buffer[num8++] = bytes[index5];
            byte[] numArray5 = buffer;
            int index6 = num8;
            int num10 = index6 + 1;
            numArray5[index6] = (byte)5;
            for (int index7 = 1; index7 < num10; ++index7)
                num2 += (int)buffer[index7] & (int)byte.MaxValue;
            byte[] numArray6 = buffer;
            int index8 = num10;
            int num11 = index8 + 1;
            int num12 = (int)(byte)((num2 >> 12 & 15) + 48);
            numArray6[index8] = (byte)num12;
            byte[] numArray7 = buffer;
            int index9 = num11;
            int num13 = index9 + 1;
            int num14 = (int)(byte)((num2 >> 8 & 15) + 48);
            numArray7[index9] = (byte)num14;
            byte[] numArray8 = buffer;
            int index10 = num13;
            int num15 = index10 + 1;
            int num16 = (int)(byte)((num2 >> 4 & 15) + 48);
            numArray8[index10] = (byte)num16;
            byte[] numArray9 = buffer;
            int index11 = num15;
            int num17 = index11 + 1;
            int num18 = (int)(byte)((num2 & 15) + 48);
            numArray9[index11] = (byte)num18;
            byte[] numArray10 = buffer;
            int index12 = num17;
            int count = index12 + 1;
            numArray10[index12] = (byte)3;
            ((Stream)_serialDevice).Write(buffer, 0, count);
            ((Stream)_serialDevice).Flush();
            _isReady = false;
            ILogger<Fp700> logger9 = _logger;
            if (logger9 != null)
                logger9.LogDebug("SendPackage executing command complete");
            return true;
        }

        private bool OnDataReceived(byte[] data)
        {
            _isError = false;
            if (data.Length == 1)
            {
                if (data[0] == (byte)21)
                {
                    ILogger<Fp700> logger = _logger;
                    if (logger != null)
                        logger.LogDebug("OnDataReceived: Printer error occured");
                    _isError = true;
                    return false;
                }
                if (data[0] == (byte)22)
                {
                    _isReady = false;
                    ILogger<Fp700> logger = _logger;
                    if (logger != null)
                        logger.LogDebug("OnDataReceived: Printer is waiting for a command");
                    return false;
                }
            }
            _isReady = true;
            int num1 = Array.IndexOf<byte>(data, (byte)1);
            int num2 = Array.IndexOf<byte>(data, (byte)4);
            int num3 = Array.IndexOf<byte>(data, (byte)5);
            int num4 = Array.IndexOf<byte>(data, (byte)3);
            if (num1 < 0 || num2 < 0 || num3 < 0 || num4 < 0)
            {
                _packageBuffer.AddRange((IEnumerable<byte>)data);
                if (!_packageBuffer.Contains((byte)3))
                {
                    ILogger<Fp700> logger = _logger;
                    if (logger != null)
                        logger.LogDebug("OnDataReceived: Printer received part of package. Waiting more...");
                    _packageBufferTimer.Start();
                    return false;
                }
                _packageBufferTimer.Stop();
                data = _packageBuffer.ToArray();
                _packageBuffer.Clear();
                num1 = Array.IndexOf<byte>(data, (byte)1);
                num2 = Array.IndexOf<byte>(data, (byte)4);
                num3 = Array.IndexOf<byte>(data, (byte)5);
                int num5 = Array.IndexOf<byte>(data, (byte)3);
                if (num1 < 0 || num2 < 0 || num3 < 0 || num5 < 0)
                {
                    _packageBufferTimer.Start();
                    ILogger<Fp700> logger = _logger;
                    if (logger != null)
                        logger.LogDebug("OnDataReceived: Printer received invalid package.");
                    return false;
                }
            }
            Command cmdNumber = (Command)int.Parse(BitConverter.ToString(data, num1 + 3, 1), NumberStyles.HexNumber);
            int count = num2 - num1 - 4;
            byte[] numArray1 = new byte[count];
            Buffer.BlockCopy((Array)data, num1 + 4, (Array)numArray1, 0, count);
            byte[] numArray2 = new byte[6];
            Buffer.BlockCopy((Array)data, num2 + 1, (Array)numArray2, 0, 6);
            byte[] dst = new byte[4];
            Buffer.BlockCopy((Array)data, num3 + 1, (Array)dst, 0, 4);
            _currentPrinterStatus = new();
            ShowStatus(cmdNumber, numArray1, (IReadOnlyList<byte>)numArray2);
            return true;
        }

        private void ShowStatus(Command cmdNumber, byte[] receivedData, IReadOnlyList<byte> status)
        {
            for (int index1 = 0; index1 < 6; ++index1)
            {
                string str = Convert.ToString(status[index1], 2);
                for (int index2 = str.Length - 1; index2 >= 0; --index2)
                {
                    if (int.Parse(str[index2].ToString()) == 1)
                        GetStatusBitDescriptionBg(index1, str.Length - 1 - index2);
                }
            }
            string str1 = Encoding.UTF8.GetString(Encoding.Convert(Encoding.GetEncoding(1251), Encoding.UTF8, receivedData));
            if (!_commandsCallbacks.ContainsKey(cmdNumber))
                return;
            Action<string> commandsCallback = _commandsCallbacks[cmdNumber];
            if (commandsCallback == null)
                return;
            commandsCallback(str1);
        }

        private string GetStatusBitDescriptionBg(int byteIndex, int bitIndex)
        {
            string bitDescriptionBg = "";
            try
            {
                switch (byteIndex)
                {
                    case 0:
                        switch (bitIndex)
                        {
                            case 0:
                                bitDescriptionBg = "# Синтаксическая ошибка.";
                                _currentPrinterStatus.IsSyntaxError = true;
                                break;
                            case 1:
                                bitDescriptionBg = "# Недопустимая команда";
                                _currentPrinterStatus.IsCommandNotPermited = true;
                                break;
                            case 2:
                                bitDescriptionBg = "Не установлены дата/время";
                                _currentPrinterStatus.IsDateAndTimeNotSet = true;
                                break;
                            case 3:
                                bitDescriptionBg = "Внешний дисплей не подключен.";
                                _currentPrinterStatus.IsDisplayDisconnected = true;
                                break;
                            case 4:
                                bitDescriptionBg = "# SAM не от этого устройства (регистратор не персонализирован).";
                                break;
                            case 5:
                                bitDescriptionBg = "Общая ошибка или все ошибки, обозначенные `#`";
                                _currentPrinterStatus.IsCommonError = true;
                                break;
                        }
                        break;
                    case 1:
                        switch (bitIndex)
                        {
                            case 0:
                                bitDescriptionBg = "При выполнении команды произошло переполнение поля суммы. Также вызывает появление статуса 1.1 и команда не приведет к изменению данных в регистраторе";
                                break;
                            case 1:
                                bitDescriptionBg = "# Выполнение команды не допускается в текущем фискальном режиме";
                                break;
                            case 2:
                                bitDescriptionBg = "# Аварийное обнуление RAM.";
                                break;
                            case 3:
                                bitDescriptionBg = "Открыт возвратный чек.";
                                break;
                            case 4:
                                bitDescriptionBg = "# Ошибка SAM";
                                break;
                            case 5:
                                bitDescriptionBg = "Крышка принтера открыта.";
                                _hasCriticalError = true;
                                _currentPrinterStatus.IsCoverOpen = true;
                                ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.Error, "[FP700] Cover is open")
                                { Status = eStatusRRO.Error, IsСritical = true });                                
                                break;
                            case 6:
                                bitDescriptionBg = "Регистратор персонализирован";
                                _currentPrinterStatus.IsPrinterFiscaled = true;
                                break;
                        }
                        break;
                    case 2:
                        switch (bitIndex)
                        {
                            case 0:
                                bitDescriptionBg = "# Бумага закончилась. Если этот статус возникнет при выполнении команды, связанной с печатью, то команда будет отклонена и состояние регистратора не изменится.";
                                _hasCriticalError = true;
                                _currentPrinterStatus.IsOutOffPaper = true;                                
                                ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.Error, "[FP700] Paper was ended")
                                { Status = eStatusRRO.Error, IsСritical = true });                                
                               break;
                            case 1:
                                bitDescriptionBg = "Заканчивается бумага";
                                _currentPrinterStatus.IsPaperNearEnd = true;
                                ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.On, "[FP700] Paper near end")
                                { Status = eStatusRRO.Warning, IsСritical = false });

                                break;
                            case 2:
                                bitDescriptionBg = "Носитель КЛЭФ заполнен (Осталось менее 1 МБ)";
                                _currentPrinterStatus.IsKSEFMemoryFull = true;
                                _hasCriticalError = true;
                                ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.Error, "[FP700] Fiscal memory is full")
                                { Status = eStatusRRO.Error, IsСritical = true });
                                 break;
                            case 3:
                                bitDescriptionBg = "Открыт фискальный чек";
                                _currentPrinterStatus.IsFiscalReceiptOpen = true;
                                break;
                            case 4:
                                bitDescriptionBg = "Носитель КЛЭФ приближается к заполнению (осталось не более 2 МБ)";
                                break;
                            case 5:
                                _currentPrinterStatus.IsNoFiscalReceiptOpen = true;
                                bitDescriptionBg = "Открыт нефискальный чек.";
                                break;
                            case 6:
                                bitDescriptionBg = "Носитель КЛЭФ почти заполнен (осталось примерно 1 МБ, возможна печать только отдельных чеков)";
                                break;
                        }
                        break;
                    case 3:
                        switch (bitIndex)
                        {
                            case 0:
                                bitDescriptionBg = "Состояние переключателя 1";
                                break;
                            case 1:
                                bitDescriptionBg = "Состояние переключателя 2";
                                break;
                            case 2:
                                bitDescriptionBg = "Состояние переключателя 3";
                                break;
                            case 3:
                                bitDescriptionBg = "Состояние переключателя 4";
                                break;
                            case 4:
                                bitDescriptionBg = "Состояние переключателя 5";
                                break;
                            case 5:
                                bitDescriptionBg = "Состояние переключателя 6";
                                break;
                            case 6:
                                bitDescriptionBg = "Состояние переключателя 7";
                                break;
                        }
                        break;
                    case 4:
                        switch (bitIndex)
                        {
                            case 0:
                                _hasCriticalError = true;
                                bitDescriptionBg = "*В фискальной памяти присутствуют ошибки";
                                _currentPrinterStatus.IsErrorOnWritingToFiscalMemory = true;
                                ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.Error, "[FP700] Fiscal Memory have error")
                                { Status = eStatusRRO.Error, IsСritical = true });
                                
                                break;
                            case 1:
                                _hasCriticalError = true;
                                bitDescriptionBg = "Фискальная память неработоспособна";
                                _currentPrinterStatus.IsCommonFiscalError = true;
                                ActionStatus?.Invoke(new RroStatus(eModelEquipment.FP700, eStateEquipment.Error, "[FP700] Fiscal Memory not working")
                                { Status = eStatusRRO.Error, IsСritical = true });

                                
                                break;
                            case 2:
                                bitDescriptionBg = "Заводской номер запрограммирован";
                                _currentPrinterStatus.IsSerialNumberSet = true;
                                break;
                            case 3:
                                bitDescriptionBg = "Осталось менее 50 записей в фискальную память";
                                _currentPrinterStatus.IsRecordsLowerThanFifty = true;
                                break;
                            case 4:
                                bitDescriptionBg = "* Заполнение фискальной памяти.";
                                _currentPrinterStatus.IsFiscalMemoryFull = true;
                                break;
                            case 5:
                                bitDescriptionBg = "Все ошибки с пометкой «*» для байтов 4 и 5.";
                                break;
                        }
                        break;
                    case 5:
                        switch (bitIndex)
                        {
                            case 0:
                                bitDescriptionBg = "* Фискальная память в режиме READONLY (`только чтение`)";
                                _currentPrinterStatus.IsFiscalMemoryReadOnly = true;
                                break;
                            case 1:
                                bitDescriptionBg = "Последняя запись в фискальную память была неудачной";
                                break;
                            case 2:
                                bitDescriptionBg = "Последняя запись в фискальную память была неудачной";
                                break;
                            case 3:
                                bitDescriptionBg = "Регистратор фискализирован";
                                _currentPrinterStatus.IsPrinterFiscaled = true;
                                break;
                            case 4:
                                bitDescriptionBg = "Налоговые ставки запрограммирован";
                                break;
                            case 5:
                                bitDescriptionBg = "Фискальный номер запрограммирован";
                                break;
                            case 6:
                                bitDescriptionBg = "Налоговый номер запрограммирован";
                                break;
                        }
                        break;
                    default:
                        bitDescriptionBg = "";
                        break;
                }
            }
            catch (Exception ex)
            {
                ILogger<Fp700> logger = _logger;
                if (logger != null)
                    logger.LogDebug(ex.Message, (object)ex.StackTrace);
            }
            return bitDescriptionBg;
        }

        private List<ReceiptItem> UngroupByExcise(List<ReceiptItem> receiptItems)
        {
            List<ReceiptItem> receiptItemList = new List<ReceiptItem>();
            foreach (ReceiptItem receiptItem1 in receiptItems)
            {
                receiptItem1.ProductQuantity = Math.Round(receiptItem1.ProductQuantity, 3, MidpointRounding.AwayFromZero);
                if (receiptItem1.Excises != null && receiptItem1.Excises.Count > 1)
                {
                    Decimal num1 = 0M;
                    Decimal num2 = 0M;
                    Decimal num3 = 0M;
                    for (int index = 0; index < receiptItem1.Excises.Count; ++index)
                    {
                        ReceiptItem receiptItem2 = new ReceiptItem(receiptItem1);
                        receiptItem2.ProductPrice = Math.Round(receiptItem2.ProductPrice, 2, MidpointRounding.AwayFromZero);
                        receiptItem2.ProductQuantity = 1M;
                        receiptItem2.Excises = new List<string>() {  receiptItem1.Excises[index] };

                        if (index != receiptItem1.Excises.Count - 1)
                        {
                            receiptItem2.Discount = Math.Round(receiptItem1.Discount / (Decimal)receiptItem1.Excises.Count, 2, MidpointRounding.AwayFromZero);
                            num1 += receiptItem2.Discount;
                            receiptItem2.FullPrice = Math.Round(receiptItem1.FullPrice / (Decimal)receiptItem1.Excises.Count, 2, MidpointRounding.AwayFromZero);
                            num2 += receiptItem2.FullPrice;
                            receiptItem2.TotalPrice = Math.Round(receiptItem1.TotalPrice / (Decimal)receiptItem1.Excises.Count, 2, MidpointRounding.AwayFromZero);
                            num3 += receiptItem2.TotalPrice;
                        }
                        else
                        {
                            receiptItem2.FullPrice = Math.Round(receiptItem1.FullPrice - num2, 2, MidpointRounding.AwayFromZero);
                            receiptItem2.TotalPrice = Math.Round(receiptItem1.TotalPrice - num3, 2, MidpointRounding.AwayFromZero);
                            receiptItem2.Discount = Math.Round(receiptItem1.Discount - num1, 2, MidpointRounding.AwayFromZero);
                        }
                        receiptItemList.Add(receiptItem2);
                    }
                }
                else
                {
                    receiptItem1.FullPrice = Math.Round(receiptItem1.FullPrice, 2, MidpointRounding.AwayFromZero);
                    receiptItem1.TotalPrice = Math.Round(receiptItem1.TotalPrice, 2, MidpointRounding.AwayFromZero);
                    receiptItem1.Discount = Math.Round(receiptItem1.Discount, 2, MidpointRounding.AwayFromZero);
                    receiptItem1.ProductPrice = Math.Round(receiptItem1.ProductPrice, 2, MidpointRounding.AwayFromZero);
                    receiptItemList.Add(receiptItem1);
                }
            }
            return receiptItemList;
        }

        private bool WaitForReady(int waitingTimeOut = 5)
        {
            StaticTimer.Wait((Func<bool>)(() => !_isReady), waitingTimeOut);
            return _isReady;
        }

        private void CloseIfOpened()
        {
            _serialDevice.Close();
            if (!_serialDevice.PortName.Equals(_port))
                _serialDevice.PortName = _port;
            _serialDevice.BaudRate = _baudRate;
        }

        public void Dispose()
        {
            CloseIfOpened();
            ((Stream)_serialDevice).Dispose();
        }

        bool IsFinish;
        StringBuilder bb;
        public string KSEFGetReceipt(string pCodeReceipt)
        {
            bb = new();
            string res = null;
            OnSynchronizeWaitCommandResult(Command.KSEF, $"R,{pCodeReceipt}", ResKSEF);
            while (!IsFinish)
            {
                OnSynchronizeWaitCommandResult(Command.KSEF, $"N", ResKSEF);
            }
            return bb.ToString();
        }

        void ResKSEF(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                IsFinish = true;
            }
            else
            {
                var b = response[0];
                IsFinish = (b == 'F');
                bb.Append(response.Substring(2));
                bb.Append(Environment.NewLine);
            }
        }
    }

    public class PrinterStatus
    {
        public bool IsCommonError { get; set; }

        public bool IsDateAndTimeNotSet { get; set; }

        public bool IsInvalidCommand { get; set; }

        public bool IsSyntaxError { get; set; }

        public bool IsRamReset { get; set; }

        public bool IsCommandNotPermited { get; set; }

        public bool IsAriphmeticOverflow { get; set; }

        public bool IsOutOffPaper { get; set; }

        public bool IsOutOffPaperJournal { get; set; }

        public bool IsCommonFiscalError { get; set; }

        public bool IsFiscalMemoryFull { get; set; }

        public bool IsKSEFMemoryFull { get; set; }

        public bool IsErrorOnWritingToFiscalMemory { get; set; }

        public bool IsFiscalMemoryReadOnly { get; set; }

        public bool IsFiscalReceiptNotOpened { get; set; }

        public bool IsFiscalReceiptOpen { get; set; }

        public bool IsNoFiscalReceiptOpen { get; set; }

        public bool IsPaperNearEnd { get; set; }

        public bool IsPaperNearEndJournal { get; set; }

        public bool IsDisplayDisconnected { get; set; }

        public bool IsCoverOpen { get; set; }

        public bool IsFiscalMemoryFormated { get; set; }

        public bool IsSerialNumberSet { get; set; }

        public bool IsTaxRatesSet { get; set; }

        public bool IsPrinterFiscaled { get; set; }

        public bool IsRecordsLowerThanFifty { get; set; }

        public bool IsPrinterResponse { get; set; }

        public bool IsProtocolError { get; set; }

        public string TextError
        {
            get
            {
                StringBuilder er = new();
                if (IsOutOffPaper)
                    er.Append("Папір закінчився!" + Environment.NewLine);
                if (IsCoverOpen)
                    er.Append("Кришка принтера відкрита." + Environment.NewLine);
                if (IsKSEFMemoryFull)
                    er.Append("КЛЕФ память Заповненна" + Environment.NewLine);
                if(IsErrorOnWritingToFiscalMemory)
                    er.Append("Фіскальна пам'ять має помилки" + Environment.NewLine);
                if (IsCommonFiscalError)
                    er.Append("Фіскальна пам'ять непрацездатна" + Environment.NewLine);
                if (IsCommonFiscalError)
                    er.Append("Фіскальна пам'ять непрацездатна" + Environment.NewLine);
                if (IsCommonFiscalError)
                    er.Append("Фіскальна пам'ять непрацездатна" + Environment.NewLine);


                return er.ToString();
            }
        }
    }

    public class DocumentNumbers
    {
        public string LastDocumentNumber { get; set; }

        public string LastFiscalDocumentNumber { get; set; }

        public string LastRefundFiscalDocumentNumber { get; set; }

        public string GlobalDocumentNumber { get; set; }
    }

    public class DiagnosticInfo
    {
        public string Model { get; set; }

        public string SoftVersion { get; set; }

        public DateTime SoftReleaseDate { get; set; }

        public string Check { get; set; }

        public string Switchers { get; set; }

        public string CountryCode { get; set; }

        public string SerialNumber { get; set; }

        public string FiscalNumber { get; set; }

        public string Id_Dev { get; set; }

        public string Id_Acq { get; set; }

        public string Id_Sam { get; set; }
    }

    public enum Command
    {
        GetLastError = 32, // 0x00000020
        ClearDisplay = 33, // 0x00000021
        OpenNonFiscalReceipt = 38, // 0x00000026
        CloseNonFiscalReceipt = 39, // 0x00000027
        PrintNonFiscalComment = 42, // 0x0000002A
        ReceiptDetailsPrintSetupAdditionalSettings = 43, // 0x0000002B
        PaperPulling = 44, // 0x0000002C
        PaperCut = 45, // 0x0000002D
        ShiftInfo = 46, // 0x0000002E
        OpenFiscalReceipt = 48, // 0x00000030
        RegisterProductInReceiptWithDisplay = 52, // 0x00000034
        PayInfoFiscalReceipt = 53, // 0x00000035
        PrintFiscalComment = 54, // 0x00000036
        PayAndCloseFiscalReceipt = 55, // 0x00000037
        CloseFiscalReceipt = 56, // 0x00000038
        ObliterateFiscalReceipt = 57, // 0x00000039
        RegisterProductInReceipt = 58, // 0x0000003A
        SetDateTime = 61, // 0x0000003D
        GetDateTime = 62, // 0x0000003E
        LastZReportInfo = 64, // 0x00000040
        GetInfoAboutSumCorrection = 67, // 0x00000043
        EveryDayReport = 69, // 0x00000045
        ServiceCashInOut = 70, // 0x00000046
        PrintDiagnosticInformation = 71, // 0x00000047
        FiscalTransactionStatus = 76, // 0x0000004C
        ShortReportByPeriod = 79, // 0x0000004F
        SendSound = 80, // 0x00000050
        ReturnReceipt = 85, // 0x00000055
        PrintCode = 88, // 0x00000058
        DiagnosticInfo = 90, // 0x0000005A
        PrintDividerLine = 93, // 0x0000005D
        FullReportByPeriod = 94, // 0x0000005E
       
        SetOperatorName = 102, // 0x00000066
        ArticleProgramming = 107, // 0x0000006B
        FiscalReceiptCopy = 109, // 0x0000006D
        AditionalInfo = 110, // 0x0000006E
        ArticleReport = 111, // 0x0000006F
        LastDocumentsNumbers = 113, // 0x00000071
        LogoProgramming = 115, // 0x00000073
        StateOfDataTransmission = 122, // 0x0000007A
        PrintServiceReceipts = 125, // 0x0000007D
        KSEF = 126  //   0x0000007Eh  РАБОТА С КЛЭФ
    }

    public enum ArticleReportType
    {
        S,
        P,
        G,
    }

    public abstract class SqLiteDataController
    {
        private readonly IConfiguration _configuration;
        private readonly string _tableName;
        private string _dbFile;

        protected SqLiteDataController(IConfiguration configuration, string tableName)
        {
            _configuration = configuration;
            _tableName = tableName;
            _dbFile = Path.Combine(configuration["MID:PathData"], "DB", _configuration.GetConnectionString("LocalDbConnection"));
        }

        protected SQLiteConnection GetConnection()
        {
            return new SQLiteConnection("Data Source=" + _dbFile);
        }

        protected async Task<bool> ExecuteNonQuery(string scriptBody, DynamicParameters obj = null, bool customName = false)
        {
            using SQLiteConnection connection = GetConnection();
            return await connection.ExecuteAsync(scriptBody, obj) > 0;
        }

        protected Task<TModel> GetByParams<TModel>(string scriptBody, DynamicParameters obj = null, bool customName = false)
        {
            using SQLiteConnection cnn = GetConnection();
            return cnn.QueryFirstOrDefaultAsync<TModel>(scriptBody, obj);
        }

      }

    public class Fp700DataController : SqLiteDataController
    {
        public Fp700DataController(IConfiguration configuration)
            : base(configuration, "FP700FiscalPrinterArticles")
        {
        }

        public Task<int> CreateOrUpdateArticle(ReceiptItem product, int plu)
        {
            string sql = @"INSERT INTO FP700FiscalPrinterArticles (Barcode, PLU, ProductName, Price) VALUES (@Barcode, @ProductPLU, @ProductName, @Price)";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@Barcode", product.ProductId.ToString());
            dynamicParameters.Add("@ProductPLU", plu);
            dynamicParameters.Add("@ProductName", product.ProductName);
            dynamicParameters.Add("@Price", product.ProductPrice);
            return GetByParams<int>(sql, dynamicParameters);
        }

        public Task<bool> DeleteArticle(int plu)
        {
            string sql = @"DELETE FROM FP700FiscalPrinterArticles WHERE PLU = @PLU";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@PLU", plu);
            return ExecuteNonQuery(sql, dynamicParameters);
        }

        public Task<bool> DeleteAllArticles()
        {
            string sql = @"DELETE FROM FP700FiscalPrinterArticles";
            return ExecuteNonQuery(sql);
        }       

        public Task<ProductArticle> GetArticleById(Guid id)
        {
            string sql = "SELECT * FROM FP700FiscalPrinterArticles WHERE Barcode = @Barcode";
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@Barcode", id.ToString());
            return GetByParams<ProductArticle>(sql, dynamicParameters);
        }
    }

}
