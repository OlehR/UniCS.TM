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
        string ClientGuid = "1A3B944E-3632-467B-AFFA-";

        public Guid ClientId
        {
            get
            {
                var strGuid = new String('0', 12) + CodeClient.ToString();
                strGuid = ClientGuid + strGuid.Substring(strGuid.Length - 12);
                return Guid.Parse(strGuid);
            }
        }

        /// <summary>
        /// Код клієнта
        /// </summary>
        public int CodeClient { get; set; }
        /// <summary>
        ///  Назва клієнта
        /// </summary>
        public string NameClient { get; set; }
        /// <summary>
        /// Тип знижки
        /// </summary>
        public int TypeDiscount { get; set; }

        public string NameDiscount { get; set; }

        /// <summary>
        /// Штрихкод карточки
        /// </summary>
        public string BarCode { get; set; }
        public string  MainPhone { get; set; }

        /// <summary>
        /// Відсоток знижки / надбавки
        /// </summary>
        public decimal PersentDiscount { get; set; }
        /// <summary>
        /// Код дилерської категорії
        /// </summary>
        public int CodeDealer { get; set; }
        /// <summary>
        /// Сума накопичених бонусів
        /// </summary>
        public decimal SumBonus { get; set; }
        /// <summary>
        /// Відсоток перегахування.
        /// </summary>
        public decimal PercentBonus { get; set; }
        /// <summary>
        /// Сума накопичених бонусів в грошовому еквіваленті
        /// </summary>
        public decimal SumMoneyBonus { get; set; }
        /// <summary>
        /// Чи можна списувати бонуси за рахунок здачі
        /// </summary>
        public bool IsUseBonusToRest { get; set; }
        /// <summary>
        /// Здача Кошильок
        /// </summary>
        public decimal Wallet { get; set; }
        /// <summary>
        /// Чи можна нараховувати бонуси з здачі
        /// </summary>
        public bool IsUseBonusFromRest { get; set; }
        /// <summary>
        /// 0-Активна,1-Заблокована,2 - видалена.
        /// </summary>
        public int StatusCard { get; set; }
        /// <summary>
        /// Код карточки який видно на дисконтній карточці.
        /// </summary>
        public Int64 ViewCode { get; set; }

        public DateTime BirthDay { get; set; }
        public Client()
        {
        }
        public Client(int parCodeClient)
        {
            CodeClient=parCodeClient;
            //Clear();
        }
        public int GetClientByClientId(Guid parClientId)
        {            
            return Convert.ToInt32(parClientId.ToString().Substring(24, 12));
        }
        
    }
}
