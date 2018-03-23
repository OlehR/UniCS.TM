using System;
using System.Data;
//using DatabaseLib;
using ExellioFP_net;

namespace MID
{
    /// <summary>
    /// Клас для роботи з касовим апаратом
    /// </summary>
    public class NET_EXELLIO:EKKA
    {
        ExellioFP_net.FiscalPrinterClass ExelioFp =  new FiscalPrinterClass();
        
        public NET_EXELLIO(WDB parDB = null, int parPort=-1,int parBaudRate=0 ) :base (parDB)
        {
            if(parPort>0)
                OpenEKKA(parPort,parBaudRate);
        }
        
        override public  bool OpenEKKA(int parPort,int parBaudRate)
        {
            ExelioFp.OpenPort(string.Format("COM{0}",parPort),parBaudRate);
            return checkError();
        }

        override public  bool BeginReceipt(bool parIsFiscal = true)
        {
            base.BeginReceipt(parIsFiscal);
            
            if (parIsFiscal)
            {
                this.ExelioFp.OpenFiscalReceipt(varOperatorNumber, varOperatorPass, varCodeWorkPlace);
            }
            else
            {
                this.ExelioFp.OpenNonfiscalReceipt();
            }
            return checkError();
        }

        override public bool BeginReturnReceipt()
        {
            ExelioFp.OpenReturnReceipt(varOperatorNumber, varOperatorPass, varCodeWorkPlace);
            return checkError();
        }

        override public bool SetOperatorName(string parOperatorName)
        {
            ExelioFp.SetOperatorName(varOperatorNumber, varOperatorPass, parOperatorName);
            base.SetOperatorName(parOperatorName);
            return checkError();
        }

        override public bool PrintCopyReceipt(int parNCopy=1)
        {
            ExelioFp.MakeReceiptCopy(parNCopy);
            return checkError();
        }

        override public bool AddLine(int parCodeEKKA, decimal  parQuantity, decimal parDiscount = 0 )
        {
            ExelioFp.RegistrAndDisplayItem(parCodeEKKA, Convert.ToDouble(parQuantity), 0, Convert.ToDouble(parDiscount));
            return checkError();
        }
        
        //+++
        override public bool AddLine(DataRow parDataRow )
        {
            return false;
        }
        
        override public int AddWares(int parCodeWares, int parGroupTax, string parNameWares, decimal parPrice)
        {
            int varNumberArticle =  base.AddWares(parCodeWares, parGroupTax, parNameWares, parPrice);
            ExelioFp.SetArticle(varNumberArticle, parGroupTax, 1, Convert.ToDouble(parPrice), varOperatorPass, parNameWares);
            
            if (checkError())
                return varNumberArticle;
            else
                return -1;
        }

        // Total -- checkerror!!!!
        override public bool CloseReceipt(decimal parSumReceipt = 0, decimal parMoneyCash = 0, decimal parMoneyPos = 0, decimal parMoneyDiscount = 0)
        {
            if ((parMoneyCash == 0 && parMoneyPos == 0 && parMoneyDiscount == 0) || (parSumReceipt == parMoneyCash))
            {
                total(1, parSumReceipt);
                if (!checkError())
                    return checkError();
            }
            else
            {
                if (parMoneyDiscount != 0)
                {
                    subTotal(parMoneyDiscount);
                    if (!checkError())
                        return checkError();
                }
                if (parMoneyCash != 0)
                {
                    total(1, parMoneyCash);
                    if (!checkError())
                        return checkError();
                }
                if (parMoneyPos != 0 )
                {
                    total (4, parMoneyPos);
                    if (!checkError())
                        return checkError();
                }
            }
            
            if (varIsFiscal)
                this.ExelioFp.CloseFiscalReceipt();
            else
                this.ExelioFp.CloseNonfiscalReceipt();
            return checkError();
        }

        virtual public bool CloseReturnReceipt(decimal parSumReceipt = 0, decimal parMoneyCash = 0, decimal parMoneyPos = 0, decimal parMoneyDiscount = 0)
        {
            CloseReceipt(parSumReceipt, parMoneyCash, parMoneyPos, parMoneyDiscount);
            return CloseReceipt();
        }

        override public bool PrintZ()
        {
            ExelioFp.ZReport(varOperatorPass);
            if (checkError())
                return ClearWaresDB();
            else
                return checkError();
        }
        
        override public bool PrintX()
        {
            ExelioFp.XReport(varOperatorPass);
            return checkError();
        }

        override public bool PrintMoveMoney(decimal parSum)
        {
            ExelioFp.InOut(Convert.ToDouble(parSum));
            return checkError();
        }
        
        override public  bool CloseEKKA()
        {
            ExelioFp.ClosePort();
            return checkError();
        }
        //---------
        //---------
        public bool checkError(bool parIsLastError = true, int parCodeError = 0, string parStrError = "")
        {
            if (ExelioFp.LastError == 0)
            {
                varCodeError = ExelioFp.LastError;
                varStrError = string.Format("Код: {0}: {1}", ExelioFp.LastError, ExelioFp.LastErrorText);
                
                if (!parIsLastError)
                {
                    if (parCodeError != 0)
                    {
                        varCodeError = parCodeError;
                        varStrError = string.Format("Код: {0}: {1}", parCodeError, parStrError);
                    }
                }
            }
            else
            {
                varCodeError = ExelioFp.LastError;
                varStrError = string.Format("Код: {0}: {1}", ExelioFp.LastError, ExelioFp.LastErrorText);
            }
            
            if (varCodeError == 0)
                return true;
            else
                return false;
        }
        
        public void subTotal(decimal parSumDiscount)
        {
            ExelioFp.SubTotal(0, Convert.ToDouble(parSumDiscount));
        }
        
        public void total (int parMoneyType, decimal parMoney, string parDescription = "")
        {
            ExelioFp.Total( parDescription, parMoneyType, Convert.ToDouble(parMoney));
        }
    }
}