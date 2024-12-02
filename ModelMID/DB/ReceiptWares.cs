using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Utils;

namespace ModelMID
{
    // - ділерська категорія - 2 ділерська категорія+знижка,3 -фіксація ціни,4-Обмеження по нижньому індикативу, 5-Обмеження по верхньому індикативу, 9 -акція

    public class ReceiptWares : IdReceiptWares, ICloneable
    {
        public int IdWorkplacePay { get { if (_IdWorkplacePay == 0) _IdWorkplacePay = Global.GetIdWorkPlacePay(CodeDirection, CodeTM, new int[]{ CodeGroup,CodeGroupUp,CodeDirection},CodeWares); return _IdWorkplacePay; } set { _IdWorkplacePay = value; } }
        int _IdWorkplacePay;

        /// <summary>
        /// Код групи товару
        /// </summary>
        public int CodeGroup { get; set; }
        /// <summary>
        /// Код групи товару 2 рівня(передостанній)
        /// </summary>
        public int CodeGroupUp { get; set; }

        /// <summary>
        /// Назва товару
        /// </summary>
        public string NameWares { get; set; }

        /// <summary>
        /// Назва для чека.
        /// </summary>
        public string NameWaresReceipt { get {return NameWares.Replace((char)9,' ').Replace((char)10,' ').Replace((char)13,' '); } }

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
        /// Ставка з РРО (Буква)
        /// </summary>         
        public string VatChar { get; set; }

        /// <summary>
        /// Код одиниці виміру позамовчуванню
        /// </summary>
        public int CodeDefaultUnit { get; set; }
        /// <summary>
        /// Коефіцієнт одиниці виіру по замовчуванню
        /// </summary>
        //public int CoefficientDefaultUnit { get; set; }

        decimal _Price;
        /// <summary>
        /// ціна за текучу одиницю.
        /// </summary>
        public decimal Price { get { return _Price == 0 && Prices?.Any() == true ? Prices.Max(el => el.Price) : _Price; } set { _Price = Math.Round(value, 2); } }
        // Дилерська категорія.                                       
        //public int CodeDealer { get; set; }

        /// <summary>
        /// Ціна по ДК
        /// </summary>
        public decimal PriceDealer { get; set; }

  
        /// <summary>
        /// Ціна для Касового апарата
        /// </summary>
        public decimal PriceEKKA { get { return Math.Round((long)eTypeDiscount.PercentDiscount != ParPrice2 && Price < PriceDealer ? Price :Math.Max(PriceDealer, Price),2, MidpointRounding.AwayFromZero); } }


        public decimal SumEKKA { get { return Math.Round(Quantity* PriceEKKA , 2, MidpointRounding.AwayFromZero); } }


        /// <summary>
        /// Знижка для Касового апарата та для Модерна
        /// </summary>
        public decimal SumDiscountEKKA { get {
                return SumTotalDiscount + SumEKKA - Sum ;
            }
        }
        //SumBonus + SumDiscount + SumWallet + 
        // (PriceEKKA < Math.Round(PriceDealer, 2, MidpointRounding.AwayFromZero) ? (Math.Round(PriceDealer, 2, MidpointRounding.AwayFromZero)-PriceEKKA) : 0); 

        /// <summary>
        /// Знижка для інтерфейсу
        /// </summary>
        public decimal Discount { get { return SumBonus + SumDiscount + SumWallet +  (PriceDealer > Price ? (PriceDealer * Quantity - Sum) : 0); } }

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

        public decimal SumTotal { get { return Sum - SumDiscount - SumWallet - SumBonus; } }
        /// <summary>
        /// Загальна знижка для РРО
        /// </summary>
        public decimal SumTotalDiscount { get { return decimal.Round(SumDiscount + SumWallet + SumBonus, 2); } }

        /// <summary>
        /// Назва акції для фіксованих цін. Тощо.
        /// </summary>
        public string NameDiscount { get; set; }

        /// <summary>
        /// Корекція для вирівння в 1С
        /// </summary>
        public decimal Delta = 0;
        [JsonIgnore]
        public decimal Sum
        {
            get { return Math.Round(Quantity * Price, 2, MidpointRounding.AwayFromZero); }
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
        public string[] GetExciseStamp { get { return ExciseStamp?.Split(',') ?? new string[0]; } }

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
        /// <summary>
        /// Сума списаних грошей 
        /// </summary>
        public decimal SumWallet { get; set; }
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
        /// Штрихкод товару
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
            get
            {
                List<WaitWeight> res = AdditionalWeights != null && AdditionalWeights.Count() > 0 ?
                        AdditionalWeights.Select(r => new WaitWeight(r, WeightDelta > 0 ? WeightDelta : r * Global.GetCoefDeltaWeight(r))).ToList()
                        : new List<WaitWeight>();
                if (CodeUnit == Global.WeightCodeUnit)
                    res.Add(new WaitWeight(1, WeightDelta > 0 ? WeightDelta : Global.GetCoefDeltaWeight(Quantity)));
                else
                {
                    if (WeightBrutto > 0)
                        res.Add(new WaitWeight(WeightBrutto, WeightDelta > 0 ? WeightDelta : WeightBrutto * Global.GetCoefDeltaWeight(WeightBrutto)));
                    if (WeightFact > 0)
                        res.Add(new WaitWeight(WeightFact, WeightDelta > 0 ? WeightDelta : WeightFact * Global.GetCoefDeltaWeight(WeightFact)));

                }
                if (WeightFact == -1)
                    res.Add(new WaitWeight(0d, 10d));
                if (res.Count == 0)
                    res.Add(new WaitWeight(100000d, 0d));

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
        /// Сума списаних бонусів в грн
        /// </summary>
        public decimal SumBonus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public WaresReceiptPromotion[] ReceiptWaresPromotions { get; set; } = null;

        public bool IsReceiptPromotion { get { return !string.IsNullOrEmpty(GetStrWaresReceiptPromotion); } }

        public string GetStrWaresReceiptPromotion
        {
            get
            {
                string Res = (IdWorkplacePay > 0 && IdWorkplacePay != IdWorkplace ? "@" : "") + (string.IsNullOrEmpty(History) || CodeUnit != Global.WeightCodeUnit ? "" : $"{History} кг{Environment.NewLine}");
                
                try
                {
                    if (ReceiptWaresPromotions?.Any()==true)
                    {
                        foreach (var el in ReceiptWaresPromotions)
                        {
                            var name = el.TypeDiscount == eTypeDiscount.Price ? (TypeWares == eTypeWares.Tobacco ? $"Ціна =>{Math.Round(el.Price / 1.05m, 2):N2}*5%={el.Price:N2}" : $"Ціна => {el.Price}") : (string.IsNullOrEmpty(el.NamePS) ? (string.IsNullOrEmpty(el.BarCode2Category) ? "" : el.BarCode2Category.Substring(3, 2) + "%") : el.NamePS);
                            Res += $"{name} - {el.Quantity} - {el.Sum:N2}{Environment.NewLine}";
                        }
                        if (!string.IsNullOrEmpty(Res))
                            Res = Res?.Substring(0, Res.Length - 1);
                    }
                    else
                     Res+= (string.IsNullOrEmpty(NameDiscount) ? "" : NameDiscount + Environment.NewLine);

                    if (!string.IsNullOrEmpty(ExciseStamp))
                        Res += $"Акцизні марки:{ExciseStamp}";
                }
                catch (Exception e) { }
                //
                return Res;
                // return Res?.Substring(0, Res.Length - 1);
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
        public decimal FixWeight { get { return _FixWeight == null && WeightFact == -1 ? 0 : _FixWeight ?? 0; } set { _FixWeight = value; } }
        public decimal FixedWeightInKg { get { return FixWeight / 1000; } }

        decimal _FixWeightQuantity = 0;
        /// <summary>
        /// Для якої кількості зафіксована вага.
        /// </summary>
        public decimal FixWeightQuantity { get { return WeightFact == -1 ? Quantity : _FixWeightQuantity; } set { _FixWeightQuantity = value; } }

        /// <summary>
        /// Код УКТЗЕТ
        /// </summary>
        public string CodeUKTZED { get; set; }
        public bool IsUseCodeUKTZED { get { return TypeWares > 0; } }

        public bool IsMultiplePrices { get { return Prices != null && Prices.Count() > 1 && TypeWares == eTypeWares.Tobacco; } }

        decimal _LimitAge = 0;
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

        /// <summary>
        /// Торгова марка (в 1С - Бренд) 
        /// </summary>
        public int CodeTM { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string History { get; set; }
        public List<decimal> HistoryQuantity { get
            {
                List<decimal> Res = new List<decimal>();
                if (!string.IsNullOrEmpty(History))
                {
                    var res = History.Split(';');
                    foreach (var item in res)
                        Res.Add(item.ToDecimal());
                }
                return Res;
            } }

        public string Currency { get; set; }
        public int CodeOperator { get; set; }
        public string Operator { get; set; }
        public int[] Operators { get{               
                if (!string.IsNullOrEmpty(Operator))                
                    return Operator.Split(';').Select(el=>el.ToInt()).ToArray();
                return new int[0];
            } }
        public string GetPrices { get { return Prices == null ? null : string.Join(";", Prices.Select(n => n.Price.ToString(CultureInfo.InvariantCulture)).ToArray()); } }
        public ReceiptWares()
        {
            Clear();
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
            //NameWaresReceipt = "";
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

        public bool IsPlus { get { return (Parent?.IsLockChange != true && !IsWeight && TypeWares != eTypeWares.Tobacco && (MaxRefundQuantity == null || Quantity < MaxRefundQuantity) && (IsLast || !IsConfirmDel)); } } // { get; set; } = false;//

        public bool IsMinus { get { return Parent?.IsLockChange != true && !IsWeight && Quantity > 1 && TypeWares != eTypeWares.Tobacco && TypeWares != eTypeWares.Alcohol && (IsLast || !IsConfirmDel); } } //{ get; set; } = false;//

        public bool IsDel { get { return Parent?.IsLockChange != true; } }

        public bool IsConfirmDel { get { return WeightFact != -1; } }

        public bool IsNeedExciseStamp { get { return TypeWares == eTypeWares.Alcohol && GetExciseStamp.Length < Quantity && !(GetExciseStamp.LastOrDefault()?.Equals("None") == true); } }

        public bool IsBag { get { return Global.Bags==null?false: Global.Bags.Any(el => el == CodeWares); } }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public List<ReceiptWares> ParseByPrice()
        {
            var Res = new List<ReceiptWares>();
            ReceiptWares el = this.Clone() as ReceiptWares;
            decimal PromotionQuantity = 0;
            IEnumerable<WaresReceiptPromotion> PromotionPrice = null;

            if (el.ReceiptWaresPromotions != null)
            {
                PromotionPrice = el.ReceiptWaresPromotions.Where(r => r.TypeDiscount == eTypeDiscount.Price);
                PromotionQuantity = PromotionPrice.Sum(r => r.Quantity);
            }

            if (PromotionQuantity > 0 && el.ParPrice1 != 888888)
            {
                decimal AllQuantity = el.Quantity;
                var OtherPromotion = el.ReceiptWaresPromotions.Where(r => r.TypeDiscount != eTypeDiscount.Price);
                el.ReceiptWaresPromotions = null;

                if (PromotionQuantity < AllQuantity && OtherPromotion.Any())
                {
                    var SumDiscount = OtherPromotion.Sum(r => r.Sum);
                    var QuantityDiscount = OtherPromotion.Sum(r => r.Quantity);
                    el.SumDiscount = QuantityDiscount * (el.Price - SumDiscount / QuantityDiscount);
                    el.ReceiptWaresPromotions = OtherPromotion.ToArray();
                    el.Quantity = AllQuantity - PromotionQuantity;
                    Res.Add(el);
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
                            Res.Add(c);
                        }
                    }
                }
            }
            else
            {
                Res.Add(el);
            }
            return Res;
        }

        public List<ReceiptWares> ParseByExcise()
        {
            List<ReceiptWares> Res = new List<ReceiptWares>();
            ReceiptWares el = this.Clone() as ReceiptWares;

            el.Quantity = Math.Round(el.Quantity, 3, MidpointRounding.AwayFromZero);

            var ExciseStamp = el.GetExciseStamp.Where(e => !e.Equals("None")).ToArray();
            decimal Quantity = el.Quantity;

            if (ExciseStamp.Count() > 1)
            {
                for (int index = 0; index < Math.Min(ExciseStamp.Count(), Quantity); ++index)
                {
                    ReceiptWares NewEl = el.Clone() as ReceiptWares;
                    NewEl.Quantity = 1M;
                    NewEl.ExciseStamp = ExciseStamp[index];
                    el.Quantity--;
                    NewEl.SumDiscount = Math.Round(NewEl.Quantity * el.SumDiscount / Quantity, 2, MidpointRounding.AwayFromZero);
                    Res.Add(NewEl);
                }
            }

            if (el.Quantity > 0)
            {
                el.Price = Math.Round(el.Price, 2, MidpointRounding.AwayFromZero);
                el.SumDiscount = Math.Round(el.Quantity * el.SumDiscount / Quantity, 2, MidpointRounding.AwayFromZero);
                el.ExciseStamp = null;
                Res.Add(el);
            }
            return Res;
        }

        public List<ReceiptWares> ParseByWeight()
        {
            List<ReceiptWares> Res = new List<ReceiptWares>();
            if (SumBonus == 0 && IsWeight && !string.IsNullOrEmpty(History) && Math.Abs(HistoryQuantity.Sum() - Quantity) < 0.001m)
            {
                decimal Sum = 0m;
                int i = 0;
                var aa = Operators.Length;
                foreach (var el in HistoryQuantity)
                {
                    if (el > 0)
                    {
                        ReceiptWares NewEl = this.Clone() as ReceiptWares;
                        NewEl.Quantity = el;
                        NewEl.SumDiscount = Math.Round(NewEl.Quantity * SumDiscount / Quantity, 2, MidpointRounding.AwayFromZero);
                        NewEl.SumBonus = Math.Round(NewEl.Quantity * SumBonus / Quantity, 2, MidpointRounding.AwayFromZero);
                        NewEl.SumWallet = Math.Round(NewEl.Quantity * SumWallet / Quantity, 2, MidpointRounding.AwayFromZero);
                        NewEl.CodeOperator = (i < Operators.Length ? Operators[i] : 0);
                        Res.Add(NewEl);
                        Sum += NewEl.SumTotal;
                    }
                    i++;
                }
                // Коригування кінцевої суми.
                if (SumTotal != Sum)
                {
                    Res.First().Delta += -Sum + SumTotal;
                }
            }
            else
            {
                if (IsWeight && CodeOperator == 0 && Operators?.Any()==true)
                    CodeOperator = Operators?.FirstOrDefault()??0;
                Res.Add(this);
            }
            return Res;
        }

        public IEnumerable<GW> WaresLink { get; set; }
        public bool IsWaresLink { get { return WaresLink?.Any() == true; } }
        /// <summary>
        /// Місце виготовлення виробу.
        /// </summary>
        public int ProductionLocation { get; set; }
    }
}