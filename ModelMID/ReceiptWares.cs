using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class ReceiptWares:IdReceiptWares
    {
        /// <summary>
        /// Код товару
        /// </summary>
        //public int CodeWares;
        /// <summary>
        /// Назва товару
        /// </summary>
        public string NameWares;
        /// <summary>
        /// Назва для чека.
        /// </summary>
        public string NameWaresReceipt;
        //public int   TypeVat ;  			// Тип ставка ПДВ  ()
        /// <summary>
        /// % Ставки ПДВ (0 -0 ,20 -20%)
        /// </summary>
        public int PercentVat;
        /// <summary>
        /// Код одиниці виміру позамовчуванню
        /// </summary>
        /// 
        public int TypeVat;
        public int CodeDefaultUnit;
        /// <summary>
        /// Коефіцієнт одиниці виіру по замовчуванню
        /// </summary>
        public int CoefficientDefaultUnit;

        // Інформація про ціноутворення
        /// <summary>
        /// 
        /// </summary>
        public decimal Price; // ціна за базову одиницю.
                                 /// <summary>
                                 /// 
                                 /// </summary>
        public int CodeDealer; // Дилерська категорія.
                                  /// <summary>
                                  /// Тип ціноутворення ( 1 - ділерська категорія - 2 ділерська категорія+знижка,3 -фіксація ціни,4-Обмеження по нижньому індикативу, 5-Обмеження по верхньому індикативу, 9 -акція)
                                  /// </summary>
        public int TypePrice;

        /// <summary>
        /// 
        /// </summary>
        public int SumDiscount;
        //  Discount = // 

        // Інформація по знайденому товару
        /// <summary>
        /// Тип знайденої позиції 0-невідомо, 1 - по коду, 2 - По штрихкоду,  3- По штрихкоду - родичі. 4 - По назві
        /// </summary>
        public int TypeFound;
        /// <summary>
        /// Текуча одиниця виміру
        /// </summary>
        public int CodeUnit;
        /// <summary>
        /// Коефіцієнт текучої одиниці виміру
        /// </summary>
        public decimal Coefficient;
        /// <summary>
        /// Авривіатура текучої одиниці виміру
        /// </summary>
        public int AbrUnit;
        /// <summary>
        /// Кількість товару
        /// </summary>
        public decimal Quantity;
        /// <summary>
        /// код періорду прихідної
        /// </summary>
        public int CodePeriodIncome;
        /// <summary>
        /// Код прихідної
        /// </summary>
        public int CodeIncome;
        /// <summary>
        /// 
        /// </summary>
        public bool IsSave;
        public ReceiptWares()
        {
            Clear();
        }
        public void Clear()
        {
            CodeWares = 0;
            NameWares = "";
            NameWaresReceipt = "";
            PercentVat = 0;
            TypeVat = 0;
            CodeDefaultUnit = 0;
            CoefficientDefaultUnit = 0;
            Price = 0;
            CodeDealer = 0;
            TypePrice = 0;
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
