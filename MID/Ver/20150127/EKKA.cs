using System;
using System.Data;
// using System.Data.SQLite;
using DatabaseLib;
namespace MID
{
    /// <summary>
    /// Клас для роботи з касовим апаратом
    /// </summary>
    public class EKKA
    {
        protected WDB db;
        protected bool varIsFiscal = true;
        protected int  varCodeEKKA = 0;
        protected int  varPort;
        protected int  varBaudRate;
        protected int  varCodeError = -1;
        protected string varStrError;
        protected int varOperatorNumber = 1;
        protected string varOperatorName;
        protected string varOperatorPass = "0000";
        protected int varCodeWorkPlace = 1;
        protected bool varIsAutoPrintOperator = false;
        
        public EKKA(WDB parDB = null )
        {
            OpenDB(parDB);
        }
        
        public virtual bool SetOperatorName(string parOperatorName)
        {
            varOperatorName = parOperatorName;
            return false;
        }
        
        public virtual bool PrintCopyReceipt(int parNCopy=1)
        {
            return false;
        }
        
        public bool OpenDB(WDB parDB = null )
        {
            if(parDB != null)
                db=parDB;
            else //Створюємо базу даних пізніше.
            {
                
            }
            varCodeEKKA=GetLastUseCodeEkkaDB();
            return true;
        }
        
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        virtual public  bool OpenEKKA(int parPort,int parBaudRate)
        {
            varPort=parPort;
            varBaudRate=parBaudRate;
            return false;
        }
        
        virtual public  int GetFirstFreeCodeEKKA()
        {
            return 0;
        }
        
        public  int GetLastUseCodeEkkaDB()
        {
        	return this.db.GetLastUseCodeEkka();
        }

        
        virtual public bool BeginReturnReceipt()
        {
            return false;
        }
        
        virtual public bool CloseReturnReceipt()
        {
            return false;
        }
        
        
        virtual public  bool BeginReceipt(bool parIsFiscal = true)
        {
            varIsFiscal=parIsFiscal;
            return false;
        }
        
        virtual public bool CloseReceipt(decimal parSumReceipt = 0, decimal parMoneyCash = 0, decimal parMoneyPos = 0, decimal parMoneyDiscount = 0)
        {
            return false;
        }
        
        /// <summary>
        /// Добавляє товар в чек
        /// </summary>
        /// <param name="parCodeEKKA">Код товару в касовому апараті</param>
        /// <param name="parQuantity">Кількість</param>
        /// <param name="parPrice">Ціна</param>
        /// <param name="parDiscount">Знижка</param>
        /// <returns>успішно чи ні добавлений товар</returns>
        /// 
        
        virtual public bool AddLine(int parCodeEKKA, decimal  parQuantity, decimal parDiscount = 0 )
        {
            return false;
        }
        
        /// <summary>
        /// Добавляє товар в чек
        /// </summary>
        /// <param name="parDataRow"> ROW з всією необхідною інформацією</param>
        /// <returns></returns>
        virtual public bool AddLine(DataRow parDataRow )
        {
            return false;
        }        
        /// <summary>
        /// Добавляє товар в ЕККА
        /// </summary>
        /// <param name="parCodeWares">Код товару</param>
        /// <param name="parNameWares">Назва товару</param>
        /// <param name="parPrice">Ціна</param>
        /// <returns>Артикл ЕККА 0 - помилка добавлення товару</returns>
        /// 
        
        virtual public int AddWares(int parCodeWares, int parGroupTax, string parNameWares, decimal parPrice)
        {
            return 0;
        }
        
        /// <summary>
        /// Добавляє товар в базу
        /// </summary>
        /// <param name="parCodeWares">Код товару</param>
        /// <param name="parNameWares">Назва товару</param>
        /// <param name="parPrice">Ціна</param>
        /// <returns>Артикл ЕККА</returns>
        public int AddWaresDB(int parCodeWares, string parNameWares, decimal parPrice)
        {
            ParametersCollection varParameters = new ParametersCollection();
            varParameters.Add("parCodeWares",parCodeWares ,DbType.Int32 );
            varParameters.Add("parPrice",parPrice,DbType.Decimal );
            varParameters.Add("parCodeEKKA",++this.varCodeEKKA,DbType.Int32);
            this.db.AddWaresEkka(varParameters);
            return this.varCodeEKKA;
        }

        public int GetCodeEKKA(int parCodeWares,  decimal parPrice)
        {
            ParametersCollection varParameters = new ParametersCollection();
            varParameters.Add("parCodeWares",parCodeWares ,DbType.Int32 );
            varParameters.Add("parPrice",(double) parPrice,DbType.Double );
            return this.db.GetCodeEKKA(varParameters);
        }

        public bool ClearWaresDB()
        {
        	return this.db.DeleteWaresEkka();
        }
        virtual public bool AddDiscountReceipt(decimal parDiscount)
        {
            return false;
        }
        
        virtual public bool PrintZ()
        {
            return false;
        }
        
        virtual public bool PrintX()
        {
            return false;
        }

        virtual public bool PrintMoveMoney(decimal parSum )
        {
            return false;
        }
        
        virtual public  bool CloseEKKA()
        {
            return false;
        }
        
        public string Error()
        {
            return varStrError;
        }
    }
}
