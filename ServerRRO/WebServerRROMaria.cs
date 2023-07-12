using PrintServer;
using Resonance;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;

using File = System.IO.File;

namespace ServerRRO
{
    class WebServerRROMaria: IWebServerRRO
    {
        string FN, OperatorName, OperatorPass, StrError;       
        bool IsOpenWorkDay;
        eStateEquipment State;
        int CodeError;
        bool IsInit, IsError;

        M304ManagerApplication M304;

        public WebServerRROMaria()
        {
            try
            {
                M304 = new M304ManagerApplication();
              
                M304.Open();
                Init();
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
        }



        public void Init()
        {
            State = eStateEquipment.Init;
            if (IsInit)
            {
               
                    Done();
                  IsInit = false;
               
        }
         try
                {
            if (!SetError(M304.Init("10.1.5.188:13000", "Kacir", "123456" /*SerialPort, OperatorName, OperatorPass*/, false) != 1))
            {
                if (string.IsNullOrEmpty(M304.GetDocumentsInfoXML()))
                    Done();
                var dt = M304.GetPrinterTime();//!!! 20130606110200 треба звірити час.
                IsInit = true;
            }
            State = eStateEquipment.Error;
           
            }
            catch (Exception e)
            {
                State = eStateEquipment.Error;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            //return IsInit;

        }

        void Done()
        {
            if (M304 != null)
            {
                M304.Done();
            }
        }

        bool SetError(bool pIsError)
        {
            IsError = pIsError;
            if (IsError)
            {
                if (M304 != null)
                {
                    StrError = M304.LastErrorCode;
                    int.TryParse(M304.LastErrorCode, out CodeError);
                }
            }
            else
            {
                CodeError = 0;
                StrError = null;
            }
            return IsError;
        }

        public string GetFN() { return State==eStateEquipment.On?FN:null; }

        public bool OpenWorkDay()
        {            
            return IsOpenWorkDay;
        }

        public LogRRO PrintReceipt(PrintReceiptData pData)
        {
            return null;
        }

        public LogRRO PrintZ(IdReceipt pIdR)
        {
            return PrintXY(pIdR, eTypeOperation.ZReport);
        }

        public LogRRO PrintX(IdReceipt pIdR)
        {
            return PrintXY(pIdR, eTypeOperation.XReport);
        }

        public LogRRO MoveMoney(PrintReceiptData pData)
        {
            return null;
        }

        LogRRO PrintXY(IdReceipt pIdR, eTypeOperation pTypeOperation)
        {
            return null;
        }

        LogRRO GetResLogRRO(IdReceipt pIdR, eTypeOperation pTypeOperation, decimal pSum = 0, string pResXML = null)
        {
            return null;
        }

        string GetCheckByFiscalNumber(string pTaxNum)
        {
            return null;
        }

        string Path { get {
                DateTime D = DateTime.Now; return $"C:/ProgramData/WebCheck/Archive/{FN}/{D.Year}/{D.Month}/{D.Day}/";  } }

        string GetCheck(string pTaxNum)
        {
            try
            {
                DateTime D = DateTime.Now;
                string file = $"{Path}{pTaxNum}.txt";
                if (File.Exists(file))
                    return File.ReadAllText(file);
            }
            catch (Exception e)
            {
                State = eStateEquipment.Error;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return null;
        }
        
        string GetLastCheck()
        {
            try
            {
                var directory = new DirectoryInfo(Path);
                var myFile = (from f in directory.GetFiles()
                              orderby f.LastWriteTime descending
                              select f).First();
                return File.ReadAllText(myFile.FullName);
            }
            catch (Exception e)
            {
                State = eStateEquipment.Error;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return null;
        }

        string GetElement(string pStr, string pSeek, string pStart = null, string pStop = null)
        {
            int i = pStr.IndexOf(pSeek);
            if (i > 0)
                pStr = pStr.Substring(i + pSeek.Length);
            else return null;

            if (!string.IsNullOrEmpty(pStart))
            {
                i = pStr.IndexOf(pStart);
                if (i < 0)
                    return null;
                pStr = pStr.Substring(i + pStart.Length);
            }
            if (string.IsNullOrEmpty(pStop)) return pStr;

            i = pStr.IndexOf(pStop);
            if (i > 0)
                return pStr.Substring(0, i);

            return null;
        }

        public StatusEquipment TestDevice()
        {
            try
            {               
                return new StatusEquipment() { TextState = "",ModelEquipment=eModelEquipment.pRRo_WebCheck };
            }
            catch (Exception e)
            {
                State = eStateEquipment.Error;
                return new StatusEquipment() { State = -1, TextState = e.Message };
            }
        }

        public string GetDeviceInfo()
        {
            try
            {
                return GetResLogRRO(new IdReceipt(0,0),eTypeOperation.DeviceInfo).ToJSON() ;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}


