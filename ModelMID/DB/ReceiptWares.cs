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
        /// Код товару
        /// </summary>
        public int CodeGroup;

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
        /// Код одиниці виміру позамовчуванню
        /// </summary>
        public int TypeVat { get; set; }

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
        public decimal DiscountEKKA { get { return SumDiscount + (Priority == 1 ? 0 : (PriceDealer > Price ? (PriceDealer * Quantity - Sum) : 0)); } }

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

        public int UserCreate { get; set; }

        //  public int CodeWarehouse { get; set; }
        public string Description { get; set; }
        public decimal AdditionN1 { get; set; }
        public decimal AdditionN2 { get; set; }
        public decimal AdditionN3 { get; set; }
        public string AdditionC1 { get; set; }
        public DateTime AdditionD1 { get; set; }

        /// <summary>
        /// 0-звичайний,1-алкоголь,2-тютюн
        /// </summary>
        public int TypeWares { get; set; }

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

        /// <summary>
        /// Додаткові ваги
        /// </summary>
        public IEnumerable<decimal> AdditionalWeights;

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<WaresReceiptPromotion> ReceiptWaresPromotions;

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
                            var name = el.TypeDiscount == eTypeDiscount.Price ? (TypeWares == 2 ? $"Ціна =>{Math.Round(el.Price / 1.05m, 2)}*5%={el.Price}" : $"Ціна => {el.Price}") : (string.IsNullOrEmpty(el.NamePS) ? (string.IsNullOrEmpty(el.BarCode2Category) ? "" : el.BarCode2Category.Substring(3, 2) + "%") : el.NamePS);
                            Res += $"{name} - {el.Quantity} - {el.Sum}\n";
                        }
                }
                catch (Exception e) { }
                return Res;
            }
        }

        public decimal QuantityOld { get; set; }

        public int TotalRows { get; set; }

        public decimal RefundedQuantity { get; set; }

        /// <summary>
        /// Зафіксована похибка ваги відносно базової.
        /// </summary>
        public decimal FixWeight { get; set; }
        /// <summary>
        /// Код УКТЗЕТ
        /// </summary>
        public string CodeUKTZED { get; set; }
        public bool IsUseCodeUKTZED { get { return TypeWares == 1 || TypeWares == 2; } }

        public bool IsMultiplePrices { get { return Prices != null && Prices.Count() > 1 && TypeWares == 2; } }

        public IEnumerable<decimal> Prices;

        public string GetPrices { get { return Prices == null ? null : string.Join(";", Prices.Select(n => n.ToString(CultureInfo.InvariantCulture)).ToArray()); } }
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
            if (TypeWares == 2 && Prices != null && Prices.Count() == 1)
            {
                Price = 1.05M * Prices.First();
                PriceDealer = Price;
                Priority = 1;
                ParPrice1 = 999999;
            }
        }

        /*
             public virtual void SetWares(DataRow parRw, int parTypeFound = 0)
              {
                  Clear();
                  if (parRw != null)
                  {
                      CodeWares = Convert.ToInt32(parRw["code_wares"]);
                      NameWares = Convert.ToString(parRw["name_wares"]);
                      NameWaresReceipt = Convert.ToString(parRw["name_wares_receipt"]);
                      PercentVat = Convert.ToInt32(parRw["percent_vat"]);
                      CodeUnit = Convert.ToInt32(parRw["code_unit"]);
                      Price = Convert.ToDecimal(parRw["price_dealer"]);
                      Coefficient = Convert.ToInt32(parRw["coefficient"]);
                      TypeFound = Convert.ToInt32(parTypeFound);
                      TypePrice = Convert.ToInt32(parRw["Type_Price"]);
                      TypeVat = Convert.ToInt32(parRw["Type_Vat"]);
                  }

              }*/
    }
}