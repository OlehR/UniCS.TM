using PrintServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.AxHost;
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
            FN = System.Configuration.ConfigurationManager.AppSettings["FN"];
            OperatorName = System.Configuration.ConfigurationManager.AppSettings["OperatorID"];
            IsOpenWorkDay = false;         

            WCh = new WebCheck.ClassFiscal();
            Init();
            //y = 0;
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
            //Thread.Sleep(10000);
            if(!IsOpenWorkDay)
                OpenWorkDay();
            Console.WriteLine(pData.Xml);
            bool r = WCh.FiscalReceipt(pData.Xml);
            //var res = WCh.StatusBarXML();
            return GetResLogRRO(pData.Id, pData.TypeOperation, pData.Sum);
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
            string xml = $"<InputParameters> <Parameters FN = \"{FN}\"  OperatorID = \"{OperatorName}\" /> </InputParameters>";
            if (pTypeOperation == eTypeOperation.ZReport)
                WCh.ReportZ(xml);
            else
                WCh.ReportX(xml);

            return GetResLogRRO(pIdR, pTypeOperation,0, WCh.StatusBarXML());
        }

        LogRRO GetResLogRRO(IdReceipt pIdR, eTypeOperation pTypeOperation, decimal pSum = 0, string pResXML=null)
        {            
            string ReceiptXML=null, TextReceipt = null, ResXML = WCh.StatusBarXML();

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

        string GetCheckByFiscalNumber(string pTaxNum)
        {
            bool r = WCh.GetCheckByFiscalNumber($"<InputParameters> <Parameters FN=\"{FN}\" TaxNum=\"{pTaxNum}\" type=\"1\" /></InputParameters>");
            return WCh.StatusBarXML();
        }

        string Path { get {
                DateTime D = DateTime.Now; return $"C:/ProgramData/WebCheck/Archive/{FN}/{D.Year}/{D.Month}/{D.Day}/";  } } 
        string GetCheck(string pTaxNum)
        {
            DateTime D = DateTime.Now;
            string file = $"{Path}{pTaxNum}.txt";
            if(File.Exists(file))
             return  File.ReadAllText(file);            
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
            catch (Exception e) { }
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


