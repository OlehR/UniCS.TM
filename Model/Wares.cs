using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Model
{
    public class Wares
    {
        /// <summary>
        /// Код товару
        /// </summary>
        public int varCodeWares;
        /// <summary>
        /// Назва товару
        /// </summary>
        public string varNameWares;
        /// <summary>
        /// Назва для чека.
        /// </summary>
        public string varNameWaresReceipt;
        //public int   varTypeVat ;  			// Тип ставка ПДВ  ()
        /// <summary>
        /// % Ставки ПДВ (0 -0 ,20 -20%)
        /// </summary>
        public int varPercentVat;
        /// <summary>
        /// Код одиниці виміру позамовчуванню
        /// </summary>
        /// 
        public int varTypeVat;
        public int varCodeDefaultUnit;
        /// <summary>
        /// Коефіцієнт одиниці виіру по замовчуванню
        /// </summary>
        public int varCoefficientDefaultUnit;

        // Інформація про ціноутворення
        /// <summary>
        /// 
        /// </summary>
        public decimal varPrice; // ціна за базову одиницю.
                                 /// <summary>
                                 /// 
                                 /// </summary>
        public int varCodeDealer; // Дилерська категорія.
                                  /// <summary>
                                  /// Тип ціноутворення ( 1 - ділерська категорія - 2 ділерська категорія+знижка,3 -фіксація ціни,4-Обмеження по нижньому індикативу, 5-Обмеження по верхньому індикативу, 9 -акція)
                                  /// </summary>
        public int varTypePrice;

        /// <summary>
        /// 
        /// </summary>
        public int varSumDiscount;
        //  varDiscount = // 

        // Інформація по знайденому товару
        /// <summary>
        /// Тип знайденої позиції 0-невідомо, 1 - по коду, 2 - По штрихкоду,  3- По штрихкоду - родичі. 4 - По назві
        /// </summary>
        public int varTypeFound;
        /// <summary>
        /// Текуча одиниця виміру
        /// </summary>
        public int varCodeUnit;
        /// <summary>
        /// Коефіцієнт текучої одиниці виміру
        /// </summary>
        public decimal varCoefficient;
        /// <summary>
        /// Авривіатура текучої одиниці виміру
        /// </summary>
        public int varAbrUnit;
        /// <summary>
        /// Кількість товару
        /// </summary>
        public decimal varQuantity;
        /// <summary>
        /// код періорду прихідної
        /// </summary>
        public int varCodePeriodIncome;
        /// <summary>
        /// Код прихідної
        /// </summary>
        public int varCodeIncome;
        /// <summary>
        /// 
        /// </summary>
        public bool varIsSave;
        public Wares()
        {
            Clear();
        }
        public void Clear()
        {
            varCodeWares = 0;
            varNameWares = "";
            varNameWaresReceipt = "";
            varPercentVat = 0;
            varTypeVat = 0;
            varCodeDefaultUnit = 0;
            varCoefficientDefaultUnit = 0;
            varPrice = 0;
            varCodeDealer = 0;
            varTypePrice = 0;
            varSumDiscount = 0;
            varTypeFound = 0;
            varCodeUnit = 0;
            varCoefficient = 0;
            varCodePeriodIncome = 0;
            varCodeIncome = 0;
            varQuantity = 0;
            varIsSave = false;
        }
        public virtual void SetWares(DataRow parRw, int parTypeFound = 0)
        {
            Clear();
            if (parRw != null)
            {
                varCodeWares = Convert.ToInt32(parRw["code_wares"]);
                varNameWares = Convert.ToString(parRw["name_wares"]);
                varNameWaresReceipt = Convert.ToString(parRw["name_wares_receipt"]);
                varPercentVat = Convert.ToInt32(parRw["percent_vat"]);
                varCodeUnit = Convert.ToInt32(parRw["code_unit"]);
                varPrice = Convert.ToDecimal(parRw["price_dealer"]);
                varCoefficient = Convert.ToInt32(parRw["coefficient"]);
                varTypeFound = Convert.ToInt32(parTypeFound);
                varTypePrice = Convert.ToInt32(parRw["Type_Price"]);
                varTypeVat = Convert.ToInt32(parRw["Type_Vat"]);
            }

        }

    }

}
