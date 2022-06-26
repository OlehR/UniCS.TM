﻿using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ModelMID
{
    // - ділерська категорія - 2 ділерська категорія+знижка,3 -фіксація ціни,4-Обмеження по нижньому індикативу, 5-Обмеження по верхньому індикативу, 9 -акція

    public class ReceiptWares : IdReceiptWares
    {
        /// <summary>
        /// Код компанії (важливо коли кілька підприємців)
        /// </summary>
        public int CodeCompany { get; set; }
        /// <summary>
        /// Код групи товару
        /// </summary>
        public int CodeGroup { get; set; }

        /// <summary>
        /// Назва товару
        /// </summary>
        public string NameWares { get; set; }

        /// <summary>
        /// Назва для чека.
        /// </summary>
        public string NameWaresReceipt { get; set; }

        public int Articl { get; set; }
        public int CodeBrand { get; set; }

        /// <summary>
        /// % Ставки ПДВ (0 -0 ,20 -20%)
        /// </summary>
        public decimal PercentVat { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int TypeVat { get; set; }

        /// <summary>
        /// Код одиниці виміру позамовчуванню
        /// </summary>
        public int CodeDefaultUnit { get; set; }
        /// <summary>
        /// Коефіцієнт одиниці виіру по замовчуванню
        /// </summary>
        //public int CoefficientDefaultUnit { get; set; }

        /// <summary>
        /// ціна за текучу одиницю.
        /// </summary>
        public decimal Price { get; set; }
        // Дилерська категорія.                                       
        //public int CodeDealer { get; set; }

        /// <summary>
        /// Ціна по ДК
        /// </summary>
        public decimal PriceDealer { get; set; }

        /// <summary>
        /// Ціна для Касового апарата та для Модерна
        /// </summary>
        public decimal PriceEKKA { get { return (Priority == 1 ? Price : (Price > PriceDealer ? Price : PriceDealer)); } }

        /// <summary>
        /// Знижка для Касового апарата та для Модерна
        /// </summary>
        public decimal DiscountEKKA { get { return SumBonus + SumDiscount + (Priority == 1 ? 0 : (PriceDealer > Price ? (PriceDealer * Quantity - Sum) : 0)); } }

        /// <summary>
        /// Приоритет спрацьованої акції
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// Тип ціноутворення ( 1 - ділерська категорія - 2 ділерська категорія+знижка,3 -фіксація ціни,4-Обмеження по нижньому індикативу, 5-Обмеження по верхньому індикативу, 9 -акція)
        /// </summary>
        public eTypePrice TypePrice { get; set; }

        /// <summary>
        ///  ДК,Код акції
        /// </summary>
        public long ParPrice1 { get; set; }
        /// <summary>
        ///Номер набору, підставлена ДК тощо
        /// </summary>
        public long ParPrice2 { get; set; }
        /// <summary>
        /// Відсотки по акції тощо.
        /// </summary>
        public decimal ParPrice3 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal SumDiscount { get; set; }

        public decimal SumTotal { get { return Sum - SumDiscount; } }

        /// <summary>
        /// Назва акції для фіксованих цін. Тощо.
        /// </summary>
        public string NameDiscount { get; set; }

        public decimal Sum
        {
            get { return Math.Round(Quantity * Price, 2); }
            set { Price = (Quantity > 0 ? value / Quantity : 0); }
        }

        private decimal? _vat = null;

        public decimal SumVat
        {
            get { return _vat == null ? (Sum * PercentVat) / 100m : (decimal)_vat; }
            set { _vat = value; }
        }

        // Інформація по знайденому товару
        /// <summary>
        /// Тип знайденої позиції 0-невідомо, 1 - по коду, 2 - По штрихкоду,  3- По штрихкоду - родичі. 4 - По назві
        /// </summary>
        public int TypeFound { get; set; }

        /// <summary>
        /// Текуча одиниця виміру
        /// </summary>
        //public int CodeUnit { get; set; }
        /// <summary>
        /// Коефіцієнт текучої одиниці виміру
        /// </summary>
        public decimal Coefficient { get; set; }

        /// <summary>
        /// Авривіатура текучої одиниці виміру
        /// </summary>
        public string AbrUnit { get; set; }

        /// <summary>
        /// Кількість товару
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// код періорду прихідної
        /// </summary>
        public int CodePeriodIncome { get; set; }

        /// <summary>
        /// Код прихідної
        /// </summary>
        public int CodeIncome { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSave { get; set; }

        public bool IsWeight
        {
            get { return Global.WeightCodeUnit == CodeUnit; }
        }

        public int Sort { get; set; }

        public string ExciseStamp { get; set; }
        public string[] GetExciseStamp { get { return ExciseStamp?.Split(','); } }
        
        public bool AddExciseStamp(string pES)
        {
            if (string.IsNullOrEmpty(ExciseStamp))
                ExciseStamp = pES;
            else
            {
                if (!pES.Equals("None"))
                {
                    if (ExciseStamp.IndexOf(pES) >= 0)
                        return false;
                }
                ExciseStamp = $"{ExciseStamp},{pES}";
            }
            return true;
        }

        public int UserCreate { get; set; }

        //  public int CodeWarehouse { get; set; }
        public string Description { get; set; }
        public decimal AdditionN1 { get; set; }
        public decimal AdditionN2 { get; set; }
        public decimal AdditionN3 { get; set; }
        public string AdditionC1 { get; set; }
        public DateTime AdditionD1 { get; set; }

        /// <summary>
        /// 0-звичайний,1-алкоголь,2-тютюн,3-легкий алкоголь 
        /// </summary>
        public eTypeWares TypeWares { get; set; }


        /// <summary>
        /// Штрихкод 2 категорії
        /// </summary>
        public string BarCode2Category { get; set; }

        /// <summary>
        /// Штрихкод 2 категорії
        /// </summary>
        public string BarCode { get; set; }

        public decimal WeightBrutto { get; set; }

        public decimal WeightFact { get; set; }
        public decimal WeightDelta { get; set; }

        /// <summary>
        /// Додаткові ваги
        /// </summary>
        public IEnumerable<decimal> AdditionalWeights { get; set; }

        public WaitWeight[] AllWeights
        {   
            //Global.GetCoefDeltaWeight
            get {

                List<WaitWeight> res = AdditionalWeights != null && AdditionalWeights.Count() > 0 ?
                        AdditionalWeights.Select(r => new WaitWeight(r, WeightDelta > 0 ? WeightDelta : WeightFact* Global.GetCoefDeltaWeight(r))).ToList()
                        : new List<WaitWeight>();
                //res.Ins
                if (WeightBrutto > 0)
                    res.Add(new WaitWeight(WeightBrutto, WeightDelta > 0 ? WeightDelta : WeightBrutto*Global.GetCoefDeltaWeight(WeightBrutto)));
                if (WeightFact > 0)
                    res.Add(new WaitWeight(WeightFact, WeightDelta > 0 ? WeightDelta : WeightFact*Global.GetCoefDeltaWeight(WeightFact)));

                return res.ToArray(); 
            }
        }
        /// <summary>
        /// Максимальна кількість, яку можна продавати
        /// </summary>
        public int AmountSalesBan { get; set; }

        /// <summary>
        /// Кількість Яку ще можна повернути.
        /// </summary>
        public decimal? MaxRefundQuantity { get; set; } = null;
        /// <summary>
        /// Сума списаних бонусів
        /// </summary>
        public decimal SumBonus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<WaresReceiptPromotion> ReceiptWaresPromotions;

        public bool IsReceiptPromotion { get { return !string.IsNullOrEmpty(GetStrWaresReceiptPromotion); } }

        public string GetStrWaresReceiptPromotion
        {
            get
            {
                string Res = (string.IsNullOrEmpty(NameDiscount) ? "" : NameDiscount + "\n");
                try
                {
                    if (ReceiptWaresPromotions != null)
                        foreach (var el in ReceiptWaresPromotions)
                        {
                            var name = el.TypeDiscount == eTypeDiscount.Price ? (TypeWares == eTypeWares.Tobacco ? $"Ціна =>{Math.Round(el.Price / 1.05m, 2)}*5%={el.Price}" : $"Ціна => {el.Price}") : (string.IsNullOrEmpty(el.NamePS) ? (string.IsNullOrEmpty(el.BarCode2Category) ? "" : el.BarCode2Category.Substring(3, 2) + "%") : el.NamePS);
                            Res += $"{name} - {el.Quantity} - {el.Sum}\n";
                        }
                }
                catch (Exception e) { }
                if (string.IsNullOrEmpty(Res))
                    return Res;
                return Res?.Substring(0, Res.Length - 1);
            }
        }

        public decimal QuantityOld { get; set; }

        public int TotalRows { get; set; }

        /// <summary>
        /// Кількість Повернутого.
        /// </summary>
        public decimal RefundedQuantity { get; set; }

        decimal? _FixWeight = null;
        /// <summary>
        /// Зафіксована вага контрольною вагою.
        /// </summary>
        public decimal FixWeight { get { return _FixWeight == null && WeightFact == -1 ? 0 : _FixWeight??0; } set { _FixWeight = value; } }

        decimal _FixWeightQuantity=0;
        /// <summary>
        /// Для якої кількості зафіксована вага.
        /// </summary>
        public decimal FixWeightQuantity { get { return WeightFact == -1 ? Quantity : _FixWeightQuantity; } set { _FixWeightQuantity = value; } }

        /// <summary>
        /// Код УКТЗЕТ
        /// </summary>
        public string CodeUKTZED { get; set; }
        public bool IsUseCodeUKTZED { get { return TypeWares >0; } }

        public bool IsMultiplePrices { get { return Prices != null && Prices.Count() > 1 && TypeWares == eTypeWares.Tobacco; } }

        decimal _LimitAge =0;
        /// <summary>
        /// Вікові обмеження (Піротехніка)
        /// </summary>
        public decimal LimitAge { get { return TypeWares > 0 && _LimitAge < 18 ? 18 : _LimitAge; } set { _LimitAge = value; } }


        public IEnumerable<MRC> Prices;

        /// <summary>
        /// PLU - кавомашини
        /// </summary>
        public int PLU { get; set; }

        /// <summary>
        /// QR кавомашин
        /// </summary>
        public string QR { get; set; }
        /// <summary>
        /// Напрямок
        /// </summary>        

        public bool IsLast { get; set; }
        public int CodeDirection { get; set; }

        public string GetPrices { get { return Prices == null ? null : string.Join(";", Prices.Select(n => n.Price.ToString(CultureInfo.InvariantCulture)).ToArray()); } }
        public ReceiptWares()
        {
            Clear();
        }

        public ReceiptWares(IdReceipt idReceipt, Guid parWaresId) : base(idReceipt, parWaresId)
        {
        }

        public ReceiptWares(IdReceipt idReceipt) : base(idReceipt)
        {
        }

        public ReceiptWares(IdReceiptWares idReceiptWares) : base(idReceiptWares, idReceiptWares.CodeWares,
            idReceiptWares.CodeUnit)
        {
        }

        public void Clear()
        {
            CodeWares = 0;
            NameWares = "";
            NameWaresReceipt = "";
            PercentVat = 0;
            TypeVat = 0;
            CodeDefaultUnit = 0;
            //CoefficientDefaultUnit = 0;
            Price = 0;
            //CodeDealer = 0;
            TypePrice = eTypePrice.NotDefine;
            SumDiscount = 0;
            TypeFound = 0;
            CodeUnit = 0;
            Coefficient = 0;
            CodePeriodIncome = 0;
            CodeIncome = 0;
            Quantity = 0;
            IsSave = false;
        }
        public void RecalcTobacco()
        {
            if (TypeWares == eTypeWares.Tobacco && Prices != null && Prices.Count() == 1)
            {
                Price = 1.05M * Prices.First().Price;
                PriceDealer = Price;
                Priority = 1;
                ParPrice1 = 999999;
            }
        }

        public bool IsPlus { get { return Parent?.IsLockChange != true && !IsWeight && (MaxRefundQuantity == null || Quantity < MaxRefundQuantity) && IsLast; } } // { get; set; } = false;//

        public bool IsMinus { get { return Parent?.IsLockChange != true && !IsWeight && Quantity > 1 && IsLast; } } //{ get; set; } = false;//

        public bool IsDel { get { return  Parent?.IsLockChange != true; } }

        public bool IsConfirmDel { get { return WeightFact != -1; } }

        public bool IsNeedExciseStamp { get { return TypeWares == eTypeWares.Alcohol && GetExciseStamp.Length<Quantity; } }

    }
}