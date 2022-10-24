using PrintServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;

using File = System.IO.File;

namespace ServerRRO
{
    class WebServerRRO: IWebServerRRO
    {
        string FN, OperatorName;
        WebCheck.ClassFiscal WCh;
        bool IsOpenWorkDay;
        eStateEquipment State;
        int CodeError;
        //string fileName;
        //int y = 1;
        public WebServerRRO()
        {
            try
            {
                FN = System.Configuration.ConfigurationManager.AppSettings["FN"];
                OperatorName = System.Configuration.ConfigurationManager.AppSettings["OperatorID"];
                string PathLog = System.Configuration.ConfigurationManager.AppSettings["PathLog"];
                string IdWorkplace = System.Configuration.ConfigurationManager.AppSettings["IdWorkplace"];
                IsOpenWorkDay = false;
                FileLogger.Init(PathLog, int.Parse(IdWorkplace));
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"FN={FN} OperatorID={OperatorName} IdWorkplace={IdWorkplace} PathLog={PathLog}");
                WCh = new WebCheck.ClassFiscal();
                Init();
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
        }
    
        public void Init()
        {
            try
            {
                State = eStateEquipment.Init;
                // !!!TMP Перенести асинхронно бо дуже довго
                if (WCh.Initialization($"<InputParameters> <Parameters FN = \"{FN}\" />  </InputParameters>"))
                {
                    string ResXML = WCh.StatusBarXML();
                    if (!string.IsNullOrEmpty(ResXML))
                    {
                        OpenWorkDay();
                    }
                    State = eStateEquipment.On;
                }
                else
                    State = eStateEquipment.Error;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "End");
            }
            catch (Exception e)
            {
                State = eStateEquipment.Error;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
        }

        public string GetFN() { return State==eStateEquipment.On?FN:null; }

        public bool OpenWorkDay()
        {
            string xml = $"<InputParameters> <Parameters FN=\"{FN}\" /> </InputParameters>";
            try
            {
                if (WCh.GetCurrentStatus(xml))
                {
                    string ResXML = WCh.StatusBarXML();
                    if (!string.IsNullOrEmpty(ResXML))
                    {
                        int iShiftNumber = -1;
                        var ShiftNumber = GetElement(ResXML, "ShiftNumber", "\"", "\"");
                        if (ShiftNumber != null)
                            iShiftNumber = ShiftNumber.ToInt(-1);

                        if (iShiftNumber <= 0)
                        {
                            xml = $"<InputParameters> <Parameters FN=\"{FN}\" OperatorID=\"{OperatorName}\" /> </InputParameters>";
                            IsOpenWorkDay = WCh.OpenShift(xml);
                            ResXML = WCh.StatusBarXML();
                        }
                        else
                            IsOpenWorkDay = true;
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"IsOpenWorkDay={IsOpenWorkDay}");
                    }
                }
            }
            catch (Exception e)
            {
                State = eStateEquipment.Error;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return IsOpenWorkDay;
        }

        public LogRRO PrintReceipt(PrintReceiptData pData)
        {
            try
            {
                if (!IsOpenWorkDay)
                    OpenWorkDay();
                Console.WriteLine(pData.Xml);
                bool r = WCh.FiscalReceipt(pData.Xml);
                return GetResLogRRO(pData.Id, pData.TypeOperation, pData.Sum);
            }
            catch (Exception e)
            {
                State = eStateEquipment.Error;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new LogRRO(pData.Id) { CodeError=-1,Error=e.Message, TypeOperation = pData.TypeOperation, TypeRRO = "WebCheck" };
            }           
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
            string xml = $"<InputParameters> <Parameters FN = \"{FN}\" /> Sum{(pData.Sum > 0 ? "In" : "Out")} = \"{Math.Abs(pData.Sum)}\"";
            if (WCh.CashInOut(xml))
                return GetResLogRRO(pData.Id, pData.Sum > 0 ? eTypeOperation.MoneyIn : eTypeOperation.MoneyOut, pData.Sum);
            return null;
        }

        LogRRO PrintXY(IdReceipt pIdR, eTypeOperation pTypeOperation)
        {
            try
            {
                string xml = $"<InputParameters> <Parameters FN = \"{FN}\"  OperatorID = \"{OperatorName}\" /> </InputParameters>";
                if (pTypeOperation == eTypeOperation.ZReport)
                    WCh.ReportZ(xml);
                else
                    WCh.ReportX(xml);
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"pTypeOperation=>{pTypeOperation}");
                return GetResLogRRO(pIdR, pTypeOperation, 0, WCh.StatusBarXML());
            }
            catch (Exception e)
            {
                State = eStateEquipment.Error;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new LogRRO(pIdR) { CodeError = -1, Error = e.Message, TypeOperation = pTypeOperation, TypeRRO = "WebCheck" };
            }            
        }

        LogRRO GetResLogRRO(IdReceipt pIdR, eTypeOperation pTypeOperation, decimal pSum = 0, string pResXML = null)
        {
            try
            {
                string ReceiptXML = null, TextReceipt = null, ResXML = WCh.StatusBarXML();

                var FiscalNumber = GetElement(ResXML, "CheckID", "\"", "\"");

                string Error = GetElement(ResXML, "ErrHelp", "\"", "\"");

                CodeError = GetElement(ResXML, "Err", "\"", "\"").ToInt();

                if (!string.IsNullOrEmpty(FiscalNumber))
                {
                    ReceiptXML = GetCheckByFiscalNumber(FiscalNumber);
                    TextReceipt = GetCheck(FiscalNumber);
                }
                if (pTypeOperation == eTypeOperation.XReport)
                    TextReceipt = GetCheck("LastX");
                if (pTypeOperation == eTypeOperation.ZReport)
                    TextReceipt = GetLastCheck();
                if (!string.IsNullOrEmpty(pResXML))
                    ReceiptXML = pResXML;

                return new LogRRO(pIdR) { TypeOperation = pTypeOperation, TypeRRO = "WebCheck", FiscalNumber = FiscalNumber, SUM = pSum, JSON = ReceiptXML, TextReceipt = TextReceipt, Error = Error, CodeError = CodeError };
            }
            catch (Exception e)
            {
                State = eStateEquipment.Error;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new LogRRO(pIdR) { CodeError = -1, Error = e.Message, TypeOperation = pTypeOperation, TypeRRO = "WebCheck" };
            }
        }

        string GetCheckByFiscalNumber(string pTaxNum)
        {
            bool r = WCh.GetCheckByFiscalNumber($"<InputParameters> <Parameters FN=\"{FN}\" TaxNum=\"{pTaxNum}\" type=\"1\" /></InputParameters>");
            return WCh.StatusBarXML();
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

                IsOpenWorkDay = false;
                WCh = null;
                WCh = new WebCheck.ClassFiscal();
                Init();
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


