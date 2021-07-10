/*
 * Created by SharpDevelop.
 * User: gelo
 * Date: 09.10.2013
 * Time: 9:51
 */
using System;
using System.IO;
using System.Data;
using System.Collections;
using ModelMID;
using System.Collections.Generic;
using ModelMID.DB;
using System.Linq;
using System.Text.RegularExpressions;
using Utils;

//using DatabaseLib; // тимчасово для ParametersCollection
namespace SharedLib
{

    /// <summary>
    /// Клас з віртуальними методоами для доступу до БД (Work Data Base)
    /// </summary>
    public class WDB : IDisposable
    {        
        public SQL db;
        protected Hashtable keySQL = new Hashtable();
        private bool isDisposed;

        /// <summary>
        /// Для блокування деяких асинхронних операцій по касі.
        /// </summary>
        private Dictionary<int, object> WorkplaceIdLockers = new Dictionary<int, object>();


        /*public delegate void CallWriteLogSQL(string parQvery, ParametersCollection parParameters = null);
        /// <summary>
        /// Функція для запису SQL запитів в локальну LOG базу 
        /// </summary>
		protected CallWriteLogSQL varCallWriteLogSQL; // это тот самый член-делегат :))
		*/
        public string varVersion = "0.0.1";
        protected string SqlCreateReceiptTable = "";

        protected string SqlConfig = "";
        protected string SqlReplaceConfig = "";

        /// <summary>
        /// Процедура пошуку.(для Баз з можливістю stored procedure) oracle,mssql,...
        /// </summary>
        protected string SqlFind = "";

        /// <summary>
        /// 
        /// </summary>
        protected string SqlGetPersentDiscountClientByReceipt = "";
        protected string SqlGetInfoClientByReceipt = "";
        /// <summary>
        /// Запит, який вертає знайдених клієнтів
        /// </summary>
        protected string SqlFoundClient = "";
        /// <summary>
        /// Запит, який вертає знайдені товари
        /// </summary>
        protected string SqlFoundWares = "";
        /// <summary>
        /// Повертає доступні одиниці виміру по товару
        /// </summary>
        protected string SqlAdditionUnit = "";
        /// <summary>
        /// Добавляє чек в базу.
        /// </summary>
        protected string SqlAddReceipt = "";
        protected string SqlReplaceReceipt = "";
        /// <summary>
        /// Запит який вертає інформацію про товари в чеку
        /// </summary>
        protected string SqlViewReceipt = "";
        /// <summary>
        /// Запит який вертає інформацію про товари в чеку
        /// </summary>
        protected string SqlViewReceiptWares = "";
        // <summary>
        /// Міняє клієнта в чеку
        /// </summary>
        protected string SqlUpdateClient = "";
        protected string SqlCloseReceipt = "";
        /// <summary>
        /// Добавляє товарну позицію в чек
        /// </summary>
        protected string SqlInsertWaresReceipt = "";
        protected string SqlReplaceWaresReceipt = "";
        protected string SqlGetCountWares = "";
        protected string SqlUpdateQuantityWares = "";
        protected string SqlDeleteReceiptWares = "";
        protected string SqlRecalcHeadReceipt = "";
        /// <summary>
        /// Внесення винесення грошей.
        /// </summary>
        protected string SqlInputOutputMoney = "";
        /// <summary>
        /// Добавляє інформацію про Z-звіт
        /// </summary>
        protected string SqlAddZ = "";
        /// <summary>
        /// Добавляє інформацію в log файл
        /// </summary>
        protected string SqlAddLog = "";
        /// <summary>
        /// Запит для генерації кодів по робочому місці.(наприклад номер чека)
        /// </summary>
        //protected string SqlGenWorkPlace = "";
        protected string SqlGetNewReceipt = "";

        protected string SqlLogin = "";
        protected string SqlGetPrice = "";

        //protected string SqlListPS = "";
        protected string SqlUpdatePrice = "";
        protected string SqlGetMinPriceIndicative = "";

        /*        protected string SqlGetLastUseCodeEkka = "";
                protected string SqlAddWaresEkka = "";
                protected string SqlDeleteWaresEkka = "";
                protected string SqlGetCodeEKKA = "";*/

        protected string SqlTranslation = "";
        protected string SqlFieldInfo = "";
        protected string SqlGetPermissions = "";
        protected string SqlGetAllPermissions = "";
        protected string SqlCopyWaresReturnReceipt = "";

        protected string SqlReplaceUnitDimension = "";
        protected string SqlReplaceGroupWares = "";
        protected string SqlReplaceWares = "";
        protected string SqlReplaceAdditionUnit = "";
        protected string SqlReplaceBarCode = "";
        protected string SqlReplacePrice = "";
        protected string SqlReplaceTypeDiscount = "";
        protected string SqlReplaceClient = "";


        protected string SqlGetFastGroup = "";



        protected string SqlReplaceFastGroup = "";
        protected string SqlReplaceFastWares = "";
        protected string SqlReplacePromotionSale = "";
        protected string SqlReplacePromotionSaleData = "";
        protected string SqlReplacePromotionSaleFilter = "";
        protected string SqlReplacePromotionSaleDealer = "";
        protected string SqlReplacePromotionSaleGroupWares = "";
        protected string SqlReplacePromotionSale2Category = "";
        protected string SqlReplacePromotionSaleGift = "";

        protected string SqlReplaceWaresReceiptPromotion = "";
        protected string SqlDeleteWaresReceiptPromotion = "";

        protected string SqlGetPricePromotionSale2Category = "";

        protected string SqlGetPriceDealer = "";

        protected string SqlCreateConfigTable = "";
        protected string SqlReplaceWorkPlace = "";
        protected string SqlGetWorkplace = "";

        protected string SqlMoveReceipt = "";
        protected string SqlReplacePayment = "";
        protected string SqlSetStateReceipt = "";
        protected string SqlInsertWeight = "";
        protected string SqlGetPayment = "";
        protected string SqlGetIdReceiptbyState = "";
        protected string SqlCheckLastWares2Cat = "";
        protected string SqlInsertBarCode2Cat = "";
        protected string SqlIsWaresInPromotionKit = "";
        protected string SqlGetDateFirstNotSendReceipt = "";
        protected string SqlGetLastReceipt = "";
        protected string SqlGetReceipts="";
        protected string SqlAdditionalWeightsWares="";
        protected string SqlInsertAddWeight = "";
        protected string SqlSetRefundedQuantity = "";
        
        protected string SqlInsertReceiptEvent = "";
        protected string SqlGetReceiptEvent = "";
        protected string SqlDeleteReceiptEvent = "";
        protected string SqlSetFixWeight = "";
        protected string SqlGetReceiptWaresPromotion = "";
        protected string SqlReceiptByFiscalNumbers = "";
        protected string SqlGetLastQuantity = "";
        protected string SqlReplaceMRC = "";
        protected string SqlGetPricesMRC = "";
        protected string SqlUpdateQR = "";
        protected string SqlGetQR = "";
        protected string SqlGetReceiptWaresDeleted = "";


        public WDB(string parFileSQL)
        {
            var start = DateTime.Now;
            this.ReadSQL(parFileSQL);
            InitSQL();
            FileLogger.ExtLogForClassConstruct(GetType(), GetHashCode(), $"{(DateTime.Now-start).TotalMilliseconds} ms readand init sql");
        }

        ~WDB()
        {
            FileLogger.ExtLogForClassDestruct(GetHashCode());
            Dispose(false);
        }
    
        public void SetLock(bool parIsLock)
        {
            db.SetLock(parIsLock);
        }
        public object GetObjectForLockByIdWorkplace(int parIdWorkplace)
        {
            if (!WorkplaceIdLockers.ContainsKey(parIdWorkplace) || WorkplaceIdLockers[parIdWorkplace] == null)
                WorkplaceIdLockers[parIdWorkplace] = new object();
            return WorkplaceIdLockers[parIdWorkplace];
        }
        public bool BildWorkplace()
        {
            Global.WorkPlaceByTerminalId = new SortedList<Guid, WorkPlace>();
            Global.WorkPlaceByWorkplaceId = new SortedList<int, WorkPlace>();
            foreach (var el in GetWorkPlace())
            {
                Global.WorkPlaceByTerminalId.Add(el.TerminalGUID, el);
                Global.WorkPlaceByWorkplaceId.Add(el.IdWorkplace, el);
            }
            return true;
        }

        public virtual bool SetConfig<T>(string parName, T parValue)
        {
            parValue.GetType().ToString();
            db.ExecuteNonQuery<object>(this.SqlReplaceConfig, new { NameVar = parName, DataVar = parValue, @TypeVar = parValue.GetType().ToString() });
            return true;
        }

        public virtual T GetConfig<T>(string parStr)
        {
            return this.db.ExecuteScalar<object, T>(this.SqlConfig, new { NameVar = parStr });
            //;
        }

        /// <summary>
        /// Повертає знайдений товар/товари
        /// </summary>
        public virtual IEnumerable<ReceiptWares> FindWares(string parBarCode = null, string parName = null, int parCodeWares = 0, int parCodeUnit = 0, int parCodeFastGroup = 0, int parArticl = -1, int parOffSet = -1, int parLimit = 10)
        {
            var Lim = parOffSet >= 0 ? $" limit {parLimit} offset {parOffSet}" : "";
            var Wares = this.db.Execute<object, ReceiptWares>(SqlFoundWares + Lim, new { CodeWares = parCodeWares, CodeUnit = parCodeUnit, BarCode = parBarCode, NameUpper = (parName == null ? null : "%" + parName.ToUpper() + "%"), CodeDealer = ModelMID.Global.DefaultCodeDealer, CodeFastGroup = parCodeFastGroup, Articl = parArticl });
            foreach (var el in Wares)
            {
                el.AdditionalWeights = db.Execute <object ,decimal> (SqlAdditionalWeightsWares,new { CodeWares=el.CodeWares});
                el.Prices = db.Execute<object, decimal>(SqlGetPricesMRC, new { CodeWares = el.CodeWares });
            }
            return Wares;
        }


        /// <summary>
        /// Повертає знайденого клієнта(клієнтів)
        /// </summary>
        /// <param name="parCodeWares">Код товару</param>
        /// <returns>
        ///Повертає  IEnumerable<Client> з клієнтами
        ///</returns>
        public virtual IEnumerable<Client> FindClient(string parBarCode = null, string parPhone = null, string parName = null, int parCodeClient = 0)
        {
            return this.db.Execute<object, Client>(SqlFoundClient, new { CodeClient = parCodeClient, Phone = parPhone, BarCode = parBarCode, Name = (parName == null ? null : "%" + parName + "%") });
        }

        /// <summary>
        /// Вертає код Чека в зазначеному періоді
        /// </summary>
        /// <param name="parCodePeriod">Код періоду (201401 - січень 2014)</param>
        /// <returns>
        ///Повертає код чека
        ///</returns>
        public virtual IdReceipt GetNewReceipt(IdReceipt parIdReceipt)
        {

            lock (GetObjectForLockByIdWorkplace(parIdReceipt.IdWorkplace))
            {
              if (parIdReceipt.CodePeriod == 0)
                    parIdReceipt.CodePeriod = Global.GetCodePeriod();
                parIdReceipt.CodeReceipt = this.db.ExecuteScalar<IdReceipt, int>(SqlGetNewReceipt, parIdReceipt);
            }
            return parIdReceipt;
        }


        /// <summary>
        /// Добавляє  чека в базу
        /// </summary>
        /// <param name="parReceipt">Рядок з інформацією про чек</param>
        /// <returns>
        ///Успішно чи ні виконана операція
        ///</returns>
        public virtual bool AddReceipt(Receipt parReceipt)
        {
            return this.db.ExecuteNonQuery<Receipt>(SqlAddReceipt, parReceipt) == 0;
        }

        public virtual bool ReplaceReceipt(Receipt parReceipt)
        {
            return this.db.ExecuteNonQuery<Receipt>(SqlReplaceReceipt, parReceipt) == 0;
        }

        public virtual bool UpdateClient(IdReceipt parIdReceipt, int parCodeClient)
        {
            lock (GetObjectForLockByIdWorkplace(parIdReceipt.IdWorkplace))
            {
                return this.db.ExecuteNonQuery<IdReceipt>(SqlUpdateClient, parIdReceipt) == 0;
            }
        }

        public virtual bool CloseReceipt(Receipt parReceipt)
        {
            return this.db.ExecuteNonQuery<Receipt>(SqlCloseReceipt, parReceipt) == 0;
        }


        /// <summary>
        /// Повертає фактичну кількість після вставки(добавляє до текучої кількості - -1 якщо помилка;
        /// </summary>
        /// <param name="parParameters"></param>
        /// <returns></returns>
        public virtual bool AddWares(ReceiptWares parReceiptWares)
        {
            return this.db.ExecuteNonQuery<ReceiptWares>(SqlInsertWaresReceipt, parReceiptWares) == 0 /*&& RecalcHeadReceipt((IdReceipt)parReceiptWares)*/;
        }

        public virtual bool ReplaceWaresReceipt(ReceiptWares parReceiptWares)
        {

            return this.db.ExecuteNonQuery<ReceiptWares>(SqlReplaceWaresReceipt, parReceiptWares) == 0 /*&& RecalcHeadReceipt((IdReceipt)parReceiptWares)*/;
        }



        public virtual decimal GetCountWares(IdReceiptWares parIdReceiptWares)
        {
            return db.ExecuteScalar<IdReceiptWares, decimal>(SqlGetCountWares, parIdReceiptWares);
        }

        public virtual bool UpdateQuantityWares(ReceiptWares parIdReceiptWares)
        {
            lock (GetObjectForLockByIdWorkplace(parIdReceiptWares.IdWorkplace))
            {
                return this.db.ExecuteNonQuery(SqlUpdateQuantityWares, parIdReceiptWares) == 0 /*&& RecalcHeadReceipt(parParameters)*/;
            }
        }


        public virtual bool DeleteReceiptWares(ReceiptWares parIdReceiptWares)
        {
            return this.db.ExecuteNonQuery<IdReceiptWares>(SqlDeleteReceiptWares, parIdReceiptWares) == 0 /*&& RecalcHeadReceipt(parParameters)*/;
        }
        public virtual Receipt ViewReceipt(IdReceipt parIdReceipt, bool parWithDetail = false)
        {
            var res = this.db.Execute<IdReceipt, Receipt>(SqlViewReceipt, parIdReceipt);
            if (res.Count() == 1)
            {
                var r = res.FirstOrDefault();
                if (parWithDetail)
                {
                    r.Wares = ViewReceiptWares(parIdReceipt,true);
                    r.Payment = GetPayment(parIdReceipt);
                    r.ReceiptEvent = GetReceiptEvent(parIdReceipt);

                }
                return r;
            }
            return null;
        }

        /// <summary>
        /// Показує інформацю про товари в чеку 
        /// </summary>
        /// <param name="parParameters"> </param>
        /// <returns></returns>
     	public virtual IEnumerable<ReceiptWares> ViewReceiptWares(IdReceipt parIdReceipt, bool pIsReceiptWaresPromotion = false)
        {
            return ViewReceiptWares(new IdReceiptWares(parIdReceipt), pIsReceiptWaresPromotion); //this.db.Execute<IdReceipt, ReceiptWares>(SqlViewReceiptWares, parIdReceipt);
        }
        public virtual IEnumerable<ReceiptWares> ViewReceiptWares(IdReceiptWares parIdReceiptWares, bool pIsReceiptWaresPromotion = false)
        {
            var r = this.db.Execute<IdReceiptWares, ReceiptWares>(SqlViewReceiptWares, parIdReceiptWares);
            if (r != null && pIsReceiptWaresPromotion)
            {
                var wrp = GetReceiptWaresPromotion(parIdReceiptWares);
                if (wrp != null)
                    foreach (var el in r)
                    {
                        el.ReceiptWaresPromotions = wrp.Where(rr=> ((IdReceiptWares)rr).Equals( (IdReceiptWares)el));
                    }
            }
            return r;
        }


        /// <summary>
        /// Перераховує ціни з врахуванням акцій, кількостей і ТД
        /// </summary>
        /// <param name="parCodeReceipt">Код чека</param>
        /// <returns>
        ///Успішно чи ні виконана операція
        ///</returns>
        public virtual bool RecalcPrice(IdReceiptWares parIdReceipt)
        {
            return false;
        }

        public virtual bool InputOutputMoney(decimal parMany)
        {
            return this.db.ExecuteNonQuery(SqlInputOutputMoney) == 0;
        }

        public virtual bool AddZ(System.Data.DataRow parRow)
        {
            return this.db.ExecuteNonQuery(SqlAddZ) == 0;
        }


        protected bool ReadSQL(String iniPath)
        {
            TextReader iniFile = null;
            String strLine = null;
            String currentRoot = "";
            String value = "";

            if (File.Exists(iniPath))
            {
                try
                {
                    iniFile = new StreamReader(iniPath);
                    strLine = iniFile.ReadLine();
                    while (strLine != null)
                    {
                        strLine = strLine.Trim().ToUpper();
                        if (strLine != "")
                        {
                            if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                            {
                                if (currentRoot.Length > 0 && value.Length > 0)
                                    keySQL.Add(currentRoot, value);
                                currentRoot = strLine.Substring(1, strLine.Length - 2).ToUpper();
                                value = "";
                            }
                            else
                            {
                                if (value.Length > 0)
                                    value += "\n" + strLine;
                                else
                                    value = strLine;
                            }
                        }
                        strLine = iniFile.ReadLine();
                    }
                    if (currentRoot.Length > 0 && value.Length > 0)
                        keySQL.Add(currentRoot, value);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (iniFile != null)
                        iniFile.Close();
                }
            }
            else
                throw new FileNotFoundException("Unable to locate " + iniPath);
            return true;

        }

        protected String GetSQL(String parNameVar)
        {
            try
            {
                return keySQL[parNameVar.ToUpper()].ToString();
            }
            catch
            {
                return null;
            }

        }
        protected bool InitSQL()
        {
            SqlCreateReceiptTable = GetSQL("SqlCreateReceiptTable");

            SqlConfig = GetSQL("SqlConfig");
            SqlReplaceConfig = GetSQL("SqlReplaceConfig");


            SqlGetPersentDiscountClientByReceipt = GetSQL("SqlGetPersentDiscountClientByReceipt");
            SqlGetInfoClientByReceipt = GetSQL("SqlGetInfoClientByReceipt");

            SqlFoundClient = GetSQL("SqlFoundClient");
            SqlFoundWares = GetSQL("SqlFoundWares");
            SqlAdditionUnit = GetSQL("SqlAdditionUnit");
            SqlViewReceipt = GetSQL("SqlViewReceipt");
            SqlViewReceiptWares = GetSQL("SqlViewReceiptWares");
            SqlAddReceipt = GetSQL("SqlAddReceipt");
            if (SqlAddReceipt != null)
                SqlReplaceReceipt = SqlAddReceipt.Replace("INSERT ", "replace ");
            SqlUpdateClient = GetSQL("SqlUpdateClient");
            SqlCloseReceipt = GetSQL("SqlCloseReceipt");

            SqlInsertWaresReceipt = GetSQL("SqlInsertWaresReceipt");
            SqlReplaceWaresReceipt = GetSQL("SqlReplaceWaresReceipt");

            SqlRecalcHeadReceipt = GetSQL("SqlRecalcHeadReceipt");
            SqlGetCountWares = GetSQL("SqlGetCountWares");
            SqlUpdateQuantityWares = GetSQL("SqlUpdateQuantityWares");
            SqlDeleteReceiptWares = GetSQL("SqlDeleteReceiptWares");
            //SqlInputOutputMoney = GetSQL("SqlInputOutputMoney");
            //SqlAddZ = GetSQL("SqlAddZ");
            //SqlAddLog = GetSQL("SqlAddLog");
            //SqlGenWorkPlace=GetSQL("SqlGenWorkPlace");
            SqlGetNewReceipt = GetSQL("SqlGetNewReceipt");

            /*SqlInsertGenWorkPlace = GetSQL("SqlInsertGenWorkPlace");
			SqlSelectGenWorkPlace = GetSQL("SqlSelectGenWorkPlace");
			SqlUpdateGenWorkPlace = GetSQL("SqlUpdateGenWorkPlace");*/
            SqlGetPrice = GetSQL("SqlGetPrice");
            SqlLogin = GetSQL("SqlLogin");
            /* SqlPrepareLockFilterT1 = GetSQL("SqlPrepareLockFilterT1");
             SqlPrepareLockFilterT2 = GetSQL("SqlPrepareLockFilterT2");
             SqlPrepareLockFilterT3 = GetSQL("SqlPrepareLockFilterT3");
             SqlPrepareLockFilterT4 = GetSQL("SqlPrepareLockFilterT4");
             SqlPrepareLockFilterT5 = GetSQL("SqlPrepareLockFilterT5");
             SqlListPS = GetSQL("SqlListPS");
                         SqlGetLastUseCodeEkka = GetSQL("SqlGetLastUseCodeEkka");
                         SqlAddWaresEkka = GetSQL("SqlAddWaresEkka");
                         SqlDeleteWaresEkka = GetSQL("SqlDeleteWaresEkka");
                         SqlGetCodeEKKA = GetSQL("SqlGetCodeEKKA");
             */
            SqlGetMinPriceIndicative = GetSQL("SqlGetMinPriceIndicative");
            SqlTranslation = GetSQL("SqlTranslation");
            SqlFieldInfo = GetSQL("SqlFieldInfo");
            SqlGetAllPermissions = GetSQL("SqlGetAllPermissions");
            SqlGetPermissions = GetSQL("SqlGetPermissions");
            SqlCopyWaresReturnReceipt = GetSQL("SqlCopyWaresReturnReceipt");

            SqlReplaceUnitDimension = GetSQL("SqlReplaceUnitDimension");
            SqlReplaceGroupWares = GetSQL("SqlReplaceGroupWares");
            SqlReplaceWares = GetSQL("SqlReplaceWares");
            SqlReplaceAdditionUnit = GetSQL("SqlReplaceAdditionUnit");
            SqlReplaceBarCode = GetSQL("SqlReplaceBarCode");
            SqlReplacePrice = GetSQL("SqlReplacePrice");
            SqlReplaceTypeDiscount = GetSQL("SqlReplaceTypeDiscount");
            SqlReplaceClient = GetSQL("SqlReplaceClient");

            SqlGetFastGroup = GetSQL("SqlGetFastGroup");

            SqlReplaceFastGroup = GetSQL("SqlReplaceFastGroup");
            SqlReplaceFastWares = GetSQL("SqlReplaceFastWares");
            SqlReplacePromotionSale = GetSQL("SqlReplacePromotionSale");
            SqlReplacePromotionSaleData = GetSQL("SqlReplacePromotionSaleData");
            SqlReplacePromotionSaleFilter = GetSQL("SqlReplacePromotionSaleFilter");
            SqlReplacePromotionSaleDealer = GetSQL("SqlReplacePromotionSaleDealer");
            SqlReplacePromotionSaleGroupWares = GetSQL("SqlReplacePromotionSaleGroupWares");
            SqlReplacePromotionSale2Category = GetSQL("SqlReplacePromotionSale2Category");
            SqlReplacePromotionSaleGift = GetSQL("SqlReplacePromotionSaleGift");

            SqlGetPricePromotionSale2Category = GetSQL("SqlGetPricePromotionSale2Category");


            SqlReplaceWaresReceiptPromotion = GetSQL("SqlReplaceWaresReceiptPromotion");
            SqlDeleteWaresReceiptPromotion = GetSQL("SqlDeleteWaresReceiptPromotion");
            SqlGetPriceDealer = GetSQL("SqlGetPriceDealer");

            SqlCreateConfigTable = GetSQL("SqlCreateConfigTable");
            SqlReplaceWorkPlace = GetSQL("SqlReplaceWorkplace");
            SqlGetWorkplace = GetSQL("SqlGetWorkplace");
            SqlReplacePayment = GetSQL("SqlReplacePayment");
            SqlSetStateReceipt = GetSQL("SqlSetStateReceipt");
            SqlInsertWeight = GetSQL("SqlInsertWeight");
            SqlGetPayment = GetSQL("SqlGetPayment");
            SqlGetIdReceiptbyState = GetSQL("SqlGetIdReceiptbyState");

            SqlCheckLastWares2Cat = GetSQL("SqlCheckLastWares2Cat");

            SqlInsertBarCode2Cat = GetSQL("SqlInsertBarCode2Cat");

            SqlIsWaresInPromotionKit = GetSQL("SqlIsWaresInPromotionKit");
            SqlGetDateFirstNotSendReceipt = GetSQL("SqlGetDateFirstNotSendReceipt");
            SqlGetLastReceipt = GetSQL("SqlGetLastReceipt");
            SqlGetReceipts = GetSQL("SqlGetReceipts");
            SqlAdditionalWeightsWares = GetSQL("SqlAdditionalWeightsWares");
            SqlInsertAddWeight = GetSQL("SqlInsertAddWeight");
            SqlSetRefundedQuantity = GetSQL("SqlSetRefundedQuantity");

            SqlInsertReceiptEvent = GetSQL("SqlInsertReceiptEvent");
            SqlGetReceiptEvent = GetSQL("SqlGetReceiptEvent");
            SqlDeleteReceiptEvent = GetSQL("SqlDeleteReceiptEvent");

            SqlSetFixWeight = GetSQL("SqlSetFixWeight");

            SqlGetReceiptWaresPromotion = GetSQL("SqlGetReceiptWaresPromotion");
            SqlReceiptByFiscalNumbers = GetSQL("SqlReceiptByFiscalNumbers");
            SqlGetLastQuantity = GetSQL("SqlGetLastQuantity");

            SqlReplaceMRC = GetSQL("SqlReplaceMRC");
            SqlGetPricesMRC = GetSQL("SqlGetPricesMRC");

            SqlUpdateQR = GetSQL("SqlUpdateQR");
            SqlGetQR = GetSQL("SqlGetQR");
            SqlGetReceiptWaresDeleted = GetSQL("SqlGetReceiptWaresDeleted");


            return true;
        }

        public virtual bool LoadDataFromFile(string parFile)
        {
            using (StreamReader streamReader = new StreamReader(parFile)) //Открываем файл для чтения)
            {
                string varHead = "insert into " + Path.GetFileNameWithoutExtension(parFile) + "(" + streamReader.ReadLine() + ") values ";
                db.BeginTransaction();
                while (!streamReader.EndOfStream)
                {
                    db.ExecuteNonQuery(varHead + "(" + streamReader.ReadLine() + ")");
                }
                db.CommitTransaction();
                return true;
            }
        }
   
        public virtual bool RecalcHeadReceipt(IdReceipt parReceipt)
        {
            return this.db.ExecuteNonQuery<IdReceipt>(this.SqlRecalcHeadReceipt, parReceipt) == 0;
        }

        /// <summary>
        /// </summary>
        /// <param name="parParameters"></param>
        /// <param name="parIsCurrentDay"> Якщо повернення в день продажу. Важливо тільки для SQLite </param>
        /// <returns></returns>
        public virtual bool CopyWaresReturnReceipt(IdReceipt parIdReceipt, bool parIsCurrentDay = true)
        {
            throw new NotImplementedException();
        }

        public virtual bool ReplaceUnitDimension(IEnumerable<UnitDimension> parData)
        {
            db.BulkExecuteNonQuery<UnitDimension>(SqlReplaceUnitDimension, parData);
            return true;
        }

        public virtual bool ReplaceGroupWares(IEnumerable<GroupWares> parData)
        {
            db.BulkExecuteNonQuery<GroupWares>(SqlReplaceGroupWares, parData);
            return true;
        }

        public virtual bool ReplaceWares(IEnumerable<Wares> parData)
        {
            db.BulkExecuteNonQuery<Wares>(SqlReplaceWares, parData);
            return true;
        }
        public virtual bool ReplaceAdditionUnit(IEnumerable<AdditionUnit> parData)
        {
            db.BulkExecuteNonQuery<AdditionUnit>(SqlReplaceAdditionUnit, parData);
            return true;
        }
        public virtual bool ReplaceBarCode(IEnumerable<Barcode> parData)
        {
            db.BulkExecuteNonQuery<Barcode>(SqlReplaceBarCode, parData);
            return true;
        }
        public virtual bool ReplacePrice(IEnumerable<Price> parData)
        {
            db.BulkExecuteNonQuery<Price>(SqlReplacePrice, parData);
            return true;
        }
        public virtual bool ReplaceTypeDiscount(IEnumerable<TypeDiscount> parData)
        {
            db.BulkExecuteNonQuery<TypeDiscount>(SqlReplaceTypeDiscount, parData);
            return true;
        }
        public virtual bool ReplaceClient(IEnumerable<Client> parData)
        {
            db.BulkExecuteNonQuery<Client>(SqlReplaceClient, parData);
            return true;
        }

        public virtual bool ReplaceFastGroup(IEnumerable<FastGroup> parData)
        {
            db.BulkExecuteNonQuery<FastGroup>(SqlReplaceFastGroup, parData);
            return true;
        }
        public virtual bool ReplaceFastWares(IEnumerable<FastWares> parData)
        {
            db.BulkExecuteNonQuery<FastWares>(SqlReplaceFastWares, parData);
            return true;
        }

        public virtual bool ReplacePromotionSale(IEnumerable<PromotionSale> parData)
        {
            db.BulkExecuteNonQuery<PromotionSale>(SqlReplacePromotionSale, parData);
            return true;
        }

        public virtual bool ReplacePromotionSaleData(IEnumerable<PromotionSaleData> parData)
        {
            db.BulkExecuteNonQuery<PromotionSaleData>(SqlReplacePromotionSaleData, parData);
            return true;
        }

        public virtual bool ReplacePromotionSaleFilter(IEnumerable<PromotionSaleFilter> parData)
        {
            db.BulkExecuteNonQuery<PromotionSaleFilter>(SqlReplacePromotionSaleFilter, parData);
            return true;
        }

        public virtual bool ReplacePromotionSaleDealer(IEnumerable<PromotionSaleDealer> parData)
        {
            db.BulkExecuteNonQuery<PromotionSaleDealer>(SqlReplacePromotionSaleDealer, parData);
            return true;
        }

        public virtual bool ReplacePromotionSaleGroupWares(IEnumerable<PromotionSaleGroupWares> parData)
        {
            db.BulkExecuteNonQuery<PromotionSaleGroupWares>(SqlReplacePromotionSaleGroupWares, parData);
            return true;
        }

        public virtual bool ReplacePromotionSaleGift(IEnumerable<PromotionSaleGift> parData)
        {
            db.BulkExecuteNonQuery<PromotionSaleGift>(SqlReplacePromotionSaleGift, parData);
            return true;
        }


        public virtual bool ReplacePromotionSale2Category(IEnumerable<PromotionSale2Category> parData)
        {
            db.BulkExecuteNonQuery<PromotionSale2Category>(SqlReplacePromotionSale2Category, parData);
            return true;
        }

        public virtual bool ReplaceMRC(IEnumerable<MRC> parData)
        {
            db.BulkExecuteNonQuery<MRC>(SqlReplaceMRC, parData);
            return true;
        }

        public virtual IEnumerable<ReceiptWares> GetWaresFromFastGroup(int parCodeFastGroup) { return null; }
        public virtual IEnumerable<FastGroup> GetFastGroup(int parCodeUpFastGroup)
        {
            var FG = new FastGroup { CodeUp = parCodeUpFastGroup };
            return db.Execute<FastGroup, FastGroup>(SqlGetFastGroup, FG);
        }


        public virtual PricePromotion GetPrice(ParameterPromotion parPromotion)
        {
            var PriceDealer = db.ExecuteScalar<ParameterPromotion, decimal>(SqlGetPriceDealer, parPromotion);
            var Res = new PricePromotion() { Price = PriceDealer };
            foreach (var el in db.Execute<ParameterPromotion, PricePromotion>(SqlGetPrice, parPromotion))
            {
                if ((el.CalcPrice(PriceDealer) < Res.Price && Res.Priority<=el.Priority) || Res.Priority < el.Priority)
                {

                    var IsUsePrice = (Res.Priority == el.Priority);
                    Res = el;                

                    Res.Price = el.CalcPrice(PriceDealer, IsUsePrice);
                }
            }
            return Res;
        }

        public virtual Int64 GetPricePromotionSale2Category(IdReceiptWares parIdReceiptWares)
        {
            return db.ExecuteScalar<IdReceiptWares, Int64>(SqlGetPricePromotionSale2Category, parIdReceiptWares);
        }


        [Obsolete("Устарівша функція використовувувати акційний механізм через GetTypeDiscountClientByReceipt")]
        public decimal GetPersentDiscountClientByReceipt(IdReceipt parIdReceipt)
        {
            return db.ExecuteScalar<IdReceipt, decimal>(SqlGetPersentDiscountClientByReceipt, parIdReceipt);
        }

        public IEnumerable<ParameterPromotion> GetInfoClientByReceipt(IdReceipt parIdReceipt)
        {
            return db.Execute<IdReceipt, ParameterPromotion>(SqlGetInfoClientByReceipt, parIdReceipt);
        }

        public virtual MinPriceIndicative GetMinPriceIndicative(IdReceiptWares parIdReceiptWares)
        {
            var res = db.Execute<IdReceiptWares, MinPriceIndicative>(SqlGetMinPriceIndicative, parIdReceiptWares);
            if (res != null)
                return res.FirstOrDefault();
            return null;
        }


        public virtual bool DeleteWaresReceiptPromotion(IdReceipt parIdReceipt)
        {
            db.ExecuteNonQuery<IdReceipt>(SqlDeleteWaresReceiptPromotion, parIdReceipt);
            return true;
        }

        public virtual bool ReplaceWaresReceiptPromotion(IEnumerable<WaresReceiptPromotion> parData)
        {
            db.BulkExecuteNonQuery<WaresReceiptPromotion>(SqlReplaceWaresReceiptPromotion, parData);
            return true;
        }

        public virtual bool ReplaceWorkPlace(IEnumerable<WorkPlace> parData)
        {
            db.BulkExecuteNonQuery<WorkPlace>(SqlReplaceWorkPlace, parData);
            return true;
        }

        public virtual IEnumerable<WorkPlace> GetWorkPlace()
        {
            return db.Execute<WorkPlace>(SqlGetWorkplace);
        }
        public virtual bool MoveReceipt(ParamMoveReceipt parMoveReceipt)
        {
            db.ExecuteNonQuery<ParamMoveReceipt>(SqlMoveReceipt, parMoveReceipt);
            return true;
        }
        public virtual bool ReplacePayment(IEnumerable<Payment> parData)
        {
            if (parData != null && parData.Count() > 0)
            {
                if (parData.Count() == 1)//Костиль через проблеми з мультипоточністю BD
                    db.ExecuteNonQuery<Payment>(SqlReplacePayment, parData.First());
                else
                    db.BulkExecuteNonQuery<Payment>(SqlReplacePayment, parData);
            }
            return true;
        }

        public virtual bool SetStateReceipt(Receipt parReceipt)
        {
            db.ExecuteNonQuery<Receipt>(SqlSetStateReceipt, parReceipt);
            return true;
        }

        public virtual bool InsertWeight(Object parWeight)
        {
            db.ExecuteNonQuery<Object>(SqlInsertWeight, parWeight);
            return true;
        }

        public virtual IEnumerable<Payment> GetPayment(IdReceipt parIdReceipt)
        {
            return db.Execute<IdReceipt, Payment>(SqlGetPayment, parIdReceipt);
        }
        public virtual IEnumerable<IdReceipt> GetIdReceiptbyState(eStateReceipt parState = eStateReceipt.Print)
        {
            return db.Execute<object, IdReceipt>(SqlGetIdReceiptbyState, new { StateReceipt = parState });
        }
        public virtual IEnumerable<ReceiptWares> GetBags()
        {
            throw new NotImplementedException();
        }

        public virtual bool InsertBarCode2Cat(WaresReceiptPromotion parWRP)
        {
            //var o = new { CodePeriod = parIdReceipt.CodePeriod, CodeReceipt = parIdReceipt.CodeReceipt, IdWorkplace = parIdReceipt.IdWorkplace, CodeWares= parIdReceipt.CodeWares, BarCode = parBarCode };
            db.ExecuteNonQuery<WaresReceiptPromotion>(SqlInsertBarCode2Cat, parWRP);
            return true;
        }


        public virtual IEnumerable<WaresReceiptPromotion> CheckLastWares2Cat(IdReceipt parIdReceipt)
        {
            return db.Execute<IdReceipt, WaresReceiptPromotion>(SqlCheckLastWares2Cat, parIdReceipt);
        }

        public virtual decimal GetLastQuantity(IdReceiptWares parIdReceiptWares)
        {
            return db.ExecuteScalar<IdReceiptWares, decimal>(SqlGetLastQuantity, parIdReceiptWares);
        }


        public virtual bool IsWaresInPromotionKit(int parCodeWares)
        {
            return this.db.ExecuteScalar<object, int>(SqlIsWaresInPromotionKit, new { CodeWares = parCodeWares }) > 0;
        }
        public virtual DateTime GetLastNotSendReceipt()
        {
            var Ldc = GetConfig<DateTime>("LastDaySend");
            if (Ldc != DateTime.Now.Date)
                return Ldc;

            return this.db.ExecuteScalar<DateTime>(SqlGetDateFirstNotSendReceipt);

        }
        public virtual DateTime GetLastUpdateDirectory()
        {
            var strTDF = GetConfig<DateTime>("Load_Full");
            var strTDU = GetConfig<DateTime>("Load_Update");
            if (strTDU < strTDF)
                return strTDF;
            else
                return strTDU;
        }

        public virtual Status GetStatus()
        {
            var DTFirstErrorDiscountOnLine = Global.FirstErrorDiscountOnLine;
            var DTLastNotSendReceipt = GetLastNotSendReceipt();
            var DTGetLastUpdateDirectory = GetLastUpdateDirectory();
            var ExchangeStatus = Global.GetExchangeStatus(DTFirstErrorDiscountOnLine);
            var ExchangeStatus1 = Global.GetExchangeStatus(DTGetLastUpdateDirectory);
            if (ExchangeStatus1 > ExchangeStatus)
                ExchangeStatus = ExchangeStatus1;
            ExchangeStatus1 = Global.GetExchangeStatus(DTLastNotSendReceipt);
            if (ExchangeStatus1 > ExchangeStatus)
                ExchangeStatus = ExchangeStatus1;

            var res = new Status { Descriprion = $"DTFirstErrorDiscountOnLine=>{DTFirstErrorDiscountOnLine}, DTLastNotSendReceipt=>{DTLastNotSendReceipt},DTGetLastUpdateDirectory=>{DTGetLastUpdateDirectory}" };
            res.SetColor(ExchangeStatus);
            return res;
        }

        public virtual Receipt GetLastReceipt(IdReceipt parIdReceipt)
        {
            var r = this.db.Execute<IdReceipt, Receipt>(SqlGetLastReceipt, parIdReceipt);
            if (r != null && r.Count() == 1)
                return r.First();
            return null;
        }
        public virtual IEnumerable<Receipt> GetReceipts(DateTime parStartDate, DateTime parFinishDate,int parIdWorkPlace)
        {
            return db.Execute< object, Receipt>(SqlGetReceipts, new { StartDate = parStartDate, FinishDate = parFinishDate, IdWorkPlace = parIdWorkPlace });
        }


        public virtual IEnumerable<Receipt> GetReceiptByFiscalNumber(int pIdWorkPlace,string pFiscalNumber, DateTime pStartDate = default(DateTime), DateTime pFinishDate = default(DateTime))
        {
            return db.Execute<object, Receipt>(SqlReceiptByFiscalNumbers, new { StartDate = pStartDate, FinishDate = pFinishDate, IdWorkPlace = pIdWorkPlace, NumberReceipt = pFiscalNumber });
        }
        

        public virtual bool InsertAddWeight (AddWeight parAddWeight)
        {
            return db.ExecuteNonQuery<AddWeight>(SqlInsertAddWeight, parAddWeight)>0;
        }
        public virtual bool SetRefundedQuantity(ReceiptWares parReceiptWares)
        {
            return db.ExecuteNonQuery<ReceiptWares>(SqlSetRefundedQuantity, parReceiptWares) > 0;
        }

        public virtual bool  InsertReceiptEvent(IEnumerable<ReceiptEvent> parRE)
        {
            return db.BulkExecuteNonQuery<ReceiptEvent>(SqlInsertReceiptEvent, parRE) > 0;
        }
            
        public virtual IEnumerable<ReceiptEvent> GetReceiptEvent(IdReceipt parIdReceipt)
        {
            return db.Execute<IdReceipt, ReceiptEvent>(SqlGetReceiptEvent, parIdReceipt);
        }

        public virtual bool DeleteReceiptEvent(IdReceipt parIdReceipt)
        {
            return db.ExecuteNonQuery<IdReceipt>(SqlDeleteReceiptEvent, parIdReceipt) > 0;
        }

        public virtual bool FixWeight(ReceiptWares parIdReceipt)
        {
            return db.ExecuteNonQuery<ReceiptWares>(SqlSetFixWeight, parIdReceipt) > 0;
        }

        public virtual bool UpdateQR(ReceiptWares pRW)
        {
            return db.ExecuteNonQuery<ReceiptWares>(SqlUpdateQR, pRW) > 0;
        }
        public virtual IEnumerable<QR> GetQR(IdReceipt pR)
        {
            return db.Execute<IdReceipt, QR>(SqlGetQR, pR);
        }
        public virtual int GetVersion(string varDB)
        {
            try
            {
                return db.ExecuteScalar<int>($"Select max(ver) from VER_{varDB}");
            }
            catch 
            {
                return 0;
            }
        }
        public virtual bool SetVersion(string varDB,int parVer)
        {
            try
            {
                return db.ExecuteNonQuery($"update VER_{varDB} set ver={parVer} ") > 0;
            }
            catch
            {
                return false;
            }
        }

        public virtual IEnumerable<WaresReceiptPromotion> GetReceiptWaresPromotion(IdReceiptWares parIdReceiptWares)
        {
            return this.db.Execute<IdReceipt, WaresReceiptPromotion>(SqlGetReceiptWaresPromotion, parIdReceiptWares);
        }
        public virtual IEnumerable<WaresReceiptPromotion> GetReceiptWaresPromotion(IdReceipt parIdReceipt)
        {
            
            return GetReceiptWaresPromotion(new IdReceiptWares(parIdReceipt));
        }

        public virtual IEnumerable<ReceiptWaresDeleted1C> GetReceiptWaresDeleted()
        {
            return this.db.Execute<ReceiptWaresDeleted1C>(SqlGetReceiptWaresDeleted);            
        }

        public virtual void Close(bool isWait = false)
        {
            if (db != null)
                db.Close(isWait);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;
            if (disposing)
            {
                Close();
            }
            // free native resources if there are any.

            isDisposed = true;
        }




    }
}
