using System;
using System.Collections.Generic;
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
        /// Тип ціноутворення ( 1 - ділерська категорія - 2 ділерська категорія+знижка,3 -фіксація ціни,4-Обмеження по нижньому індикативу, 5-Обмеження по верхньому індикативу, 9 -акція)
        /// </summary>
        public eTypePrice TypePrice { get; set; }
        /// <summary>
        ///  ДК,Код акції
        /// </summary>
        public long ParPrice1 { get; set; }
        //Номер набору, підставлена ДК тощо
        public long ParPrice2 { get; set; }

        public long ParPrice3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal SumDiscount { get; set; }
        //  Discount = // 
        public decimal Sum
        {
            get { return Quantity * Price; }
            set { Price = (Quantity > 0?value / Quantity:0); }
        }
        private decimal? _vat=null;
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
            { get { return Global.WeightCodeUnit == CodeUnit; } }
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
    public ReceiptWares()
        {
            Clear();
        }
        public ReceiptWares(IdReceipt idReceipt, Guid parWaresId) : base(idReceipt, parWaresId)
        { }

        
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
/*        public virtual void SetWares(DataRow parRw, int parTypeFound = 0)
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
