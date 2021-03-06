﻿using System;
using System.Data;
// using System.Data.SQLite;
//using DatabaseLib;
namespace Front.Equipments
{
    /// <summary>
    /// Клас для роботи з касовим апаратом
    /// </summary>
    public class EKKA: Equipment
    {
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

        public EKKA(string pSerialPortName, int pBaudRate, Action<string, string> pLogger) : base(pSerialPortName, pBaudRate) { }

        public virtual bool SetOperatorName(string parOperatorName)
        {
            varOperatorName = parOperatorName;
            return false;
        }
        
        public virtual bool PrintCopyReceipt(int parNCopy=1)
        {
            return false;
        }
        
       
        
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        virtual public  bool Open(int parPort,int parBaudRate)
        {
            varPort=parPort;
            varBaudRate=parBaudRate;
            return false;
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
   
        virtual public bool AddDiscountReceipt(decimal parDiscount)
        {
            throw new NotImplementedException();
        }
        
        virtual public bool PrintZ()
        {
            throw new NotImplementedException();
        }
        
        virtual public bool PrintX()
        {
            throw new NotImplementedException();
        }

        virtual public bool PrintMoveMoney(decimal parSum )
        {
            throw new NotImplementedException();
        }
        
        virtual public  bool CloseEKKA()
        {
            throw new NotImplementedException();
        }
        
  
    }
}
