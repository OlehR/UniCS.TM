using System;
using System.Collections.Generic;
using System.Linq;
using ModelMID.DB;
using Utils;

namespace ModelMID
{
    /// <summary>
    /// Глобальні змінні.
    /// </summary>
    public class Global
    {
        public static Action<Receipt> OnReceiptCalculationComplete { get; set; }
        public static Action<SyncInformation> OnSyncInfoCollected { get; set; }
        public static Action<StatusBD> OnStatusChanged { get; set; }
        //public static Action<eStateScale> OnChangedStatusScale { get; set; }
        public static Action<Client> OnClientChanged { get; set; }
        public static Action<Receipt> OnReceiptChanged { get; set; }

        public static Action<int, eTypeWindows, string> OnClientWindows { get; set; }

        public static Action<string, eTypeMessage> Message { get; set; }

        public static SortedList<int, WorkPlace> WorkPlaceByWorkplaceId;

        public static SortedList<int, int> IdWorkPlacePayDirection = new SortedList<int, int>();
        public static SortedList<int, int> IdWorkPlacePayTM = new SortedList<int, int>();
        public static SortedList<int, int> IdWorkPlacePayGroup = new SortedList<int, int>();
        public static SortedList<int, int> IdWorkPlacePayWares = new SortedList<int, int>();

        public static Settings Settings { get; set; }

        //public static List<WorkPlace> AllWorkPlaces = new List<WorkPlace>();

        // Тип Робочого місця
        public static eTypeWorkplace TypeWorkplaceCurrent = eTypeWorkplace.SelfServicCheckout;
        //Тип Робочого місця з конфігураційного файлу
        public static eTypeWorkplace TypeWorkplace = eTypeWorkplace.SelfServicCheckout;
        /// <summary>
        /// Id робочого місця
        /// </summary>
        public static int IdWorkPlace = 62;

        public static int IdWorkPlaceIssuingCash;

        public static int CodeWarehouse { get { return Settings?.CodeWarehouse ?? 0; } }

        public static bool IsTest = false;
        /// <summary>
        /// !!!TMP Щоб розмежувати чеки на модерновських касах і наші. Стандарто має бути 0.
        /// </summary>
        public static int StartCodeReceipt = 0;

        public static int DefaultCodeDealer { get; set; }
        //{ get { switch (CodeWarehouse) { case 9: return 2; case 15: return 4; case 148: return 42; default: return 0; } } }//!!!!TMP      

        public static bool IsPrintOrderReceipt = false;
        public static string IPAddressOrderService = "127.0.0.1";

        public static bool IsGenQrCoffe = true;

        public static int CodeWaresWallet = 0;

        /// <summary>
        /// Чи можна розраховуватись готівкою.
        /// </summary>
        public static bool IsCash = false;

        /// <summary>
        /// Час між синхронізаціями в секундах. 0- відклчено.
        /// </summary>
        public static int DataSyncTime = 0;
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
        public static string PathPictures = @"D:\Pictures\";
        public static int PortAPI = 0;
        /// <summary>
        /// Шлях до MID.ini та Key.map
        /// </summary>
        public static string PathIni = @"D:\WORK\CS\UniCS.TM\SharedLib\";

        /// <summary>
        /// Максимальна кількість відкритих чеків касиром. Максимум 3.
        /// </summary>
        public static int QuantityOpenReceipt = 3;

        /// <summary>
        /// Перелік одиниць, по яким необхідно вводити кількість(при пошуку по назві чи коду)
        /// </summary>
        public static int WeightCodeUnit = 7;//кг
        public static int[] UnitMustInputQuantity = { 7 };

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

        /// <summary>
        /// Перераховувати ціни після кожної зміни в чеку
        /// </summary>
        public static bool RecalcPriceOnLine = true;

        public static int CodeFastGroupBag = 0;
        public static List<int> Bags;
        //public static SortedList<int, string> Tax = new SortedList<int, string>();
        public static DeltaWeight[] DeltaWeight;

        public static string Server1C = "";
        public static string _Api = null;
        public static string Api { get { return string.IsNullOrEmpty(_Api) ? Global.Settings?.Api : _Api; } }
        public static double MaxWeightBag = 100;

        public static List<CustomerBarCode> CustomerBarCode { get; set; }
        public static List<BlockSale> BlockSales { get; set; }

        public static eMethodExecutionLoggingType MethodExecutionLogging = eMethodExecutionLoggingType.Always;
        public static long LimitMethodExecutionTimeInMillis = 200;

        public static string AlcoholTimeStart = "07:00:00";
        public static string AlcoholTimeStop = "23:00:00";

        public static bool IsOldInterface = true;
       
        /// <summary>
        ///  час Першого помилкового запита до сервера 
        /// </summary>
        public static DateTime FirstErrorDiscountOnLine = default(DateTime);
        /// <summary>
        /// Кількість помилкових запитів до сервера
        /// </summary>
        private static int _ErrorDiscountOnLine;
        public static int ErrorDiscountOnLine
        {
            get { return _ErrorDiscountOnLine; }
            set
            {
                if (value == 0) FirstErrorDiscountOnLine = default(DateTime);
                _ErrorDiscountOnLine = value;
            }
        }
        
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
        /// <summary>
        /// Вертаємо компанію по коду товара (Коли кілька підприємців)
        /// </summary>
        /// <param name="CodeWares"></param>
        /// <returns></returns>
        public static int GetCodeCompany(int CodeWares) { return 1; }

        public static WorkPlace GetWorkPlaceByIdWorkplace(int parIdWorkPlace)
        {
            if (WorkPlaceByWorkplaceId!=null && WorkPlaceByWorkplaceId.ContainsKey(parIdWorkPlace))
                return WorkPlaceByWorkplaceId[parIdWorkPlace];
            return null;
        }

        public static string GetNumberCashDeskByIdWorkplace(int parIdWorkPlace)
        {
            if (WorkPlaceByWorkplaceId!=null && WorkPlaceByWorkplaceId.ContainsKey(parIdWorkPlace))
                return WorkPlaceByWorkplaceId[parIdWorkPlace].Name.Right(2).Replace(' ', '0').Replace('№', '0');            
            return "00";
        }

        public static string GetVideoCameraIPByIdWorkplace(int parIdWorkPlace)
        {
            if (WorkPlaceByWorkplaceId.ContainsKey(parIdWorkPlace))
                return WorkPlaceByWorkplaceId[parIdWorkPlace].VideoCameraIP;
            return null;
        }

        public static int GetIdWorkPlacePay(int pCodeDirection, int pCodeTM, int[] pCodeGroup = null,int pCodeWares=0)
        {
            if (IdWorkPlacePayWares.ContainsKey(pCodeWares))
                return IdWorkPlacePayWares[pCodeWares];
            if (IdWorkPlacePayTM.ContainsKey(pCodeTM))
                return IdWorkPlacePayTM[pCodeTM];
            if (pCodeGroup?.Any() == true)
                foreach (int el in pCodeGroup)
                    if (el != 0 && IdWorkPlacePayGroup.ContainsKey(el))
                        return IdWorkPlacePayGroup[el];
            if (IdWorkPlacePayDirection.ContainsKey(pCodeDirection))
                return IdWorkPlacePayDirection[pCodeDirection];            
            return IdWorkPlace;
        }
        public static IEnumerable<int> IdWorkPlaces;

        public static IEnumerable<WorkPlace> GetIdWorkPlaces
        {
            get {
                bool isFirst = true;
                List < WorkPlace > res= new List<WorkPlace >();
                foreach (var el in IdWorkPlaces.Distinct())
                {
                    var xx = GetWorkPlaceByIdWorkplace(el);
                    if (xx != null)
                    {
                        xx.IsChoice = isFirst;
                        res.Add(xx);
                        isFirst = false;
                    }
                }
                return res; 
            }
        }

        public static eExchangeStatus GetExchangeStatus(DateTime parDT)
        {
            var Diff = DateTime.Now - parDT;
            if (parDT == default(DateTime) || Diff.Minutes < 15) return eExchangeStatus.Green;

            if (Diff.Minutes < 30) return eExchangeStatus.LightGreen;

            if (Diff.Hours < 1) return eExchangeStatus.Yellow;

            if (Diff.Hours < 3) return eExchangeStatus.Orange;

            return eExchangeStatus.Red;
        }

        public static decimal GetCoefDeltaWeight(decimal parWeight)
        {
            var res = 0.5m;
            if (DeltaWeight != null)
                for (int i = 0; i < DeltaWeight.Length; i++)
                {
                    if (DeltaWeight[i].Weight > parWeight)
                        break;
                    res = DeltaWeight[i].Coef;
                }
            return res;
        }

        public static decimal RoundDown(decimal pNumber)
        {
            decimal res = Math.Round(pNumber, 2);
            if (res - pNumber == 0.005m)
                res -= 0.01m;
            return res;
        }

        public static bool BildWorkplace(IEnumerable<WorkPlace> pWP)
        {
            var WorkPlaceByWorkplaceId = new SortedList<int, WorkPlace>();
            if (pWP?.Any() == true)
            {
                foreach (var el in pWP)
                {
                    WorkPlaceByWorkplaceId.Add(el.IdWorkplace, el);
                    if (el.IdWorkplace == Global.IdWorkPlace && el.Settings != null)
                        Global.Settings = el.Settings;
                }
                Global.WorkPlaceByWorkplaceId = WorkPlaceByWorkplaceId;
            }
            FileLogger.WriteLogMessage("Global", System.Reflection.MethodBase.GetCurrentMethod().Name, $"Записів=>{pWP?.Count()}");

            return true;
        }
    }
}