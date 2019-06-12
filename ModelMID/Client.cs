using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    /// <summary>
    /// Зберігає інформацію про клієнта
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Код клієнта
        /// </summary>
        public int Code_Client;
        /// <summary>
        ///  Назва клієнта
        /// </summary>
        public string Name_Client;
        /// <summary>
        /// Тип знижки
        /// </summary>
        public int Type_Discount;
        /// <summary>
        /// Відсоток знижки / надбавки
        /// </summary>
        public double Discount;
        /// <summary>
        /// Код дилерської категорії
        /// </summary>
        public int Code_Dealer;
        /// <summary>
        /// Сума накопичених бонусів
        /// </summary>
        public decimal SumBonus;
        /// <summary>
        /// Сума накопичених бонусів в грошовому еквіваленті
        /// </summary>
        public decimal SumMoneyBonus;
        /// <summary>
        /// Чи можна списувати бонуси за рахунок здачі
        /// </summary>
        public bool IsUseBonusToRest;
        /// <summary>
        /// Чи можна нараховувати бонуси з здачі
        /// </summary>
        public bool IsUseBonusFromRest;

        /*
        public Client()
        {
            Clear();
        }
        public void Clear()
        {
            Code_Client = 0;
            Name_Client = "";
            Type_Discount = 0;
            Discount = 0;
            Code_Dealer = 0;
            SumBonus = 0;
            SumMoneyBonus = 0;
            IsUseBonusFromRest = false;
            IsUseBonusToRest = false;

        }*/

       /* public virtual void SetClient(DataRow parRw)
        {
            Clear();
            CodeClient = Convert.ToInt32(parRw["Code_Client"]);
            NameClient = Convert.ToString(parRw["Name_Client"]);
            TypeDiscount = Convert.ToInt32(parRw["Type_Discount"]);
            Discount = Convert.ToInt32(parRw["Discount"]);
            CodeDealer = Convert.ToInt32(parRw["Code_Dealer"]);
            SumBonus = Convert.ToDecimal(parRw["Sum_Bonus"]);
            SumMoneyBonus = Convert.ToDecimal(parRw["Sum_Money_Bonus"]);
            IsUseBonusFromRest = (Convert.ToInt32(parRw["Is_Use_Bonus_From_Rest"]) == 1);
            IsUseBonusToRest = (Convert.ToInt32(parRw["Is_Use_Bonus_To_Rest"]) == 1);

            if (CodeDealer <= 0)
                CodeDealer = Global.DefaultCodeDealer[-CodeDealer];
        }*/


    }
}
