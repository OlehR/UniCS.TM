using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    /// <summary>
	/// Глобальні змінні.
	/// </summary>
	public struct GlobalVar
    {
        /// <summary>
        /// Текучий чек (1-3) з масиву varReceipts
        /// </summary>
        public static int varCurrentReceipt = 0;
        /// <summary>
        /// Максимальна кількість відкритих чеків касиром. Максимум 3.
        /// </summary>
        public static int varQuantityOpenReceipt = 3;
        /// <summary>
        /// Список чеків (0-немає чека)
        /// </summary>
        public static int[] varReceipts = { 0, 0, 0 };
        /// <summary>
        /// Перелік одиниць, по яким необхідно вводити кількість(при пошуку по назві чи коду)
        /// </summary>
        public static int[] varUnitMustInputQuantity = { 7 };
        /// <summary>
        /// тип пошуку товара (0 - без обмежень, 1 - штрихкод , код, 2 - штрихкод)
        /// </summary>
        public static int varTypeFindWares = 0;
        /// <summary>
        /// тип пошуку клієнта (0 - без обмежень, 1 - штрихкод , код, 2 - штрихкод)
        /// </summary>
        public static int varTypeFindClient = 1;
        /// <summary>
        /// Id робочого місця
        /// </summary>
        public static int varIdWorkPlace = 140701;

        public static int varCodeWarehouse = 8151; // 1407 Сента;
                                                   /// <summary>
                                                   /// Тип періоду документів (0 - Глобальний, 1- рік, 2 -місяць, 3 - день.)
                                                   /// </summary>
        public static int varTypePeriod = 3;
        /// <summary>
        /// Мінімальна кількість симвлолів в штрихкоді товару
        /// </summary>
        public static int varMinLenghtBarCodeWares = 7;
        /// <summary>
        /// Мінімальна кількість симвлолів в штрихкоді клієнта
        /// </summary>
        public static int varMinLenghtBarCodeClient = 13;
        /// <summary>
        /// Шаблон чеків по замовчуванню
        /// </summary>
        public static int varDefaultCodePatternReceipt = -1;

        public static int varDefaultCodePatternReturnReceipt = -2;

        public static int[] varDefaultCodeDealer = { 0, 0, 0, 0, 0 };

        public static int varDefaultCodeClient = 0;

        public static string varBillCoins = "грн:1:500,200,100,50,20,10,5,2,1;коп:0.01:50,25,10,5,2,1";

        /// <summary>
        /// Перераховувати ціни після кожної зміни в чеку
        /// </summary>
        public static bool varRecalcPriceOnLine = true;
        /*		/// <summary>
		/// Кількість пос-терміналів на касі.
		/// </summary>
		public static int varQuantityPos = 2;*/
        /// <summary>
        /// Присутній пос-термінал на касі -1 - Ні, 0 - так без автоматизації, >0 код моделі(драйвера)
        /// </summary>
        public static int[] varModelPos = { 1, 0, -1 };

        /// <summary>
        /// Чи встановлено конект з POS.
        /// </summary>
        public static bool varIsPosConnect = false;

        /// <summary>
        /// Шлях до SqlLite бази
        /// </summary>
        public static string varPathDB = @"d:\mid\";

        /// <summary>
        /// Шлях до папки, в яку пиcати LOG
        /// </summary>
        public static string varPathLog = @"d:\temp\";

        /// <summary>
        /// Шлях до MID.ini та Key.map
        /// </summary>
        public static string varPathIni = @"";

        public static Language varLanguage = Language.uk_UA;
        //public static string var
        //public static DateTime varArxDate= new DateTime (1,1,1);\
    }
}
