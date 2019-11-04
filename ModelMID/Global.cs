using System;
using System.Collections.Generic;
using System.Text;
using ModelMID.DB;
namespace ModelMID
{

    /// <summary>
    /// Глобальні змінні.
    /// </summary>
    public class Global
    {

        public static SortedList<Guid, WorkPlace> WorkPlaceByTerminalId;
        public static SortedList<int, WorkPlace> WorkPlaceByWorkplaceId;
        /// <summary>
        /// Текучий чек (1-3) з масиву Receipts
        /// </summary>
        public static int CurrentReceipt = 0;
        /// <summary>
        /// Максимальна кількість відкритих чеків касиром. Максимум 3.
        /// </summary>
        public static int QuantityOpenReceipt = 3;
        /// <summary>
        /// Список чеків (0-немає чека)
        /// </summary>
        public static int[] Receipts = { 0, 0, 0 };
        /// <summary>
        /// Перелік одиниць, по яким необхідно вводити кількість(при пошуку по назві чи коду)
        /// </summary>
        public static int WeightCodeUnit = 7;//кг
        public static int[] UnitMustInputQuantity = { 7 };
        /// <summary>
        /// тип пошуку товара (0 - без обмежень, 1 - штрихкод , код, 2 - штрихкод)
        /// </summary>
        public static int TypeFindWares = 0;
        /// <summary>
        /// тип пошуку клієнта (0 - без обмежень, 1 - штрихкод , код, 2 - штрихкод)
        /// </summary>
        public static int TypeFindClient = 1;
        /// <summary>
        /// Id робочого місця
        /// </summary>
        public static int IdWorkPlace = 62;
        public static string PrefixWarehouse = "K";

        public static int CodeWarehouse = 9;
        /// <summary>
        /// Тип періоду документів (0 - Глобальний, 1- рік, 2 -місяць, 3 - день.)
        /// </summary>
        public static ePeriod TypePeriod = ePeriod.Day;
        /// <summary>
        /// Мінімальна кількість симвлолів в штрихкоді товару
        /// </summary>
        public static int MinLenghtBarCodeWares = 7;
        /// <summary>
        /// Мінімальна кількість симвлолів в штрихкоді клієнта
        /// </summary>
        public static int MinLenghtBarCodeClient = 13;
        /// <summary>
        /// Шаблон чеків по замовчуванню
        /// </summary>
        public static int DefaultCodePatternReceipt = -1;

        public static int DefaultCodePatternReturnReceipt = -2;

        public static int DefaultCodeDealer = 2;

        public static int DefaultCodeClient = 0;

        public static string BillCoins = "грн:1:500,200,100,50,20,10,5,2,1;коп:0.01:50,10";

        /// <summary>
        /// Перераховувати ціни після кожної зміни в чеку
        /// </summary>
        public static bool RecalcPriceOnLine = true;
        /*		/// <summary>
		/// Кількість пос-терміналів на касі.
		/// </summary>
		public static int QuantityPos = 2;*/
        /// <summary>
        /// Присутній пос-термінал на касі -1 - Ні, 0 - так без автоматизації, >0 код моделі(драйвера)
        /// </summary>
        public static int[] ModelPos = { 1, 0, -1 };

        /// <summary>
        /// Чи встановлено конект з POS.
        /// </summary>
        public static bool IsPosConnect = false;

        /// <summary>
        /// Шлях до SqlLite бази
        /// </summary>
        public static string PathDB = @"c:\mid\";

        /// <summary>
        /// Текуча директорія
        /// </summary>
        public static string PathCur = @"c:\mid\";
        /// <summary>
        /// Шлях до папки, в яку пиcати LOG
        /// </summary>
        public static string PathLog = @"c:\temp\";

        /// <summary>
        /// Шлях до MID.ini та Key.map
        /// </summary>
        public static string PathIni = @"D:\WORK\CS\UniCS.TM\SharedLib\";

        //public static Language Language = Language.uk_UA;
        //public static string 
        //public static DateTime ArxDate= new DateTime (1,1,1);\
        public static string WaresGuid = "1A3B944E-3632-467B-A53A-";
        public static string ClientGuid = "1A3B944E-3632-467B-AFFA-";


        public static SortedList<int, string> Tax = new SortedList<int, string>();

        public static string Server1C = "";

        public static List<CustomerBarCode> CustomerBarCode  { get;set;}
        public static int GetCodePeriod()
        {
            return GetCodePeriod(DateTime.Today);
        }
        /// <summary>
        /// Повертає  код періоду по даті
        /// </summary>
        /// <param name="varD">дата поякій вернути період</param>
        /// <returns>Код періоду</returns>
        public static int GetCodePeriod(DateTime varD)
        {
            if (varD == null)
                varD = DateTime.Today;
            switch (Global.TypePeriod)
            {
                case ePeriod.Year:
                    return Convert.ToInt32(varD.ToString("yyyy"));
                case ePeriod.Month:
                    return Convert.ToInt32(varD.ToString("yyyyMM"));
                case ePeriod.Day:
                    return Convert.ToInt32(varD.ToString("yyyyMMdd"));
            }
            return 0;
        }
        public static int GetIdWorkplaceByTerminalId(Guid parTerminalId)
        {
            if (WorkPlaceByTerminalId.ContainsKey(parTerminalId))
                return WorkPlaceByTerminalId[parTerminalId].IdWorkplace;
            return 0;
        }

        public static Guid GetTerminalIdByIdWorkplace(int parIdWorkPlace)
        {
            if (WorkPlaceByWorkplaceId.ContainsKey(parIdWorkPlace))
                return WorkPlaceByWorkplaceId[parIdWorkPlace].TerminalGUID;
            return Guid.Empty;
        }

        public static string GetNumberCashDeskByIdWorkplace(int parIdWorkPlace)
        {
            if (WorkPlaceByWorkplaceId.ContainsKey(parIdWorkPlace))
                return WorkPlaceByWorkplaceId[parIdWorkPlace].Name.Right(2).Replace(' ', '0').Replace('№', '0');
            ;
            return "00";
        }

        public static string GetTaxGroup(int parTypeVat)
        {
            return Tax[parTypeVat];
        }

        public static MethodExecutionLoggingType MethodExecutionLogging = MethodExecutionLoggingType.Always;
        public static long LimitMethodExecutionTimeInMillis = 200;
        
        public enum MethodExecutionLoggingType
        {
            MoreThenMillis = 0,
            Always,
            WhenErrored,
        }
    }
}