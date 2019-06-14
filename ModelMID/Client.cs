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
        public Guid ClientId
        {
            get
            {
                var strGuid = new String('0', 12) + CodeClient.ToString();
                strGuid = GlobalVar.ClientGuid + strGuid.Substring(strGuid.Length - 12);
                return Guid.Parse(strGuid);
            }
        }

        /// <summary>
        /// Код клієнта
        /// </summary>
        public int CodeClient;
        /// <summary>
        ///  Назва клієнта
        /// </summary>
        public string NameClient;
        /// <summary>
        /// Тип знижки
        /// </summary>
        public int TypeDiscount;
        /// <summary>
        /// Відсоток знижки / надбавки
        /// </summary>
        public double Discount;
        /// <summary>
        /// Код дилерської категорії
        /// </summary>
        public int CodeDealer;
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

        public Client(int parCodeClient)
        {
            CodeClient=parCodeClient;
            //Clear();
        }
        /*
        public Client()
        {
            Clear();
        }
        public void Clear()
        {
            CodeClient = 0;
            NameClient = "";
            TypeDiscount = 0;
            Discount = 0;
            CodeDealer = 0;
            SumBonus = 0;
            SumMoneyBonus = 0;
            IsUseBonusFromRest = false;
            IsUseBonusToRest = false;

        }*/

        /* public virtual void SetClient(DataRow parRw)
         {
             Clear();
             CodeClient = Convert.ToInt32(parRw["CodeClient"]);
             NameClient = Convert.ToString(parRw["NameClient"]);
             TypeDiscount = Convert.ToInt32(parRw["TypeDiscount"]);
             Discount = Convert.ToInt32(parRw["Discount"]);
             CodeDealer = Convert.ToInt32(parRw["CodeDealer"]);
             SumBonus = Convert.ToDecimal(parRw["SumBonus"]);
             SumMoneyBonus = Convert.ToDecimal(parRw["SumMoneyBonus"]);
             IsUseBonusFromRest = (Convert.ToInt32(parRw["IsUseBonusFromRest"]) == 1);
             IsUseBonusToRest = (Convert.ToInt32(parRw["IsUseBonusToRest"]) == 1);

             if (CodeDealer <= 0)
                 CodeDealer = Global.DefaultCodeDealer[-CodeDealer];
         }*/


    }
}
