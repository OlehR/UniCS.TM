using PrintServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using static System.Windows.Forms.AxHost;

namespace ServerRRO
{
    class WebServerRRO: IWebServerRRO
    {
        string FN, OperatorName;
        WebCheck.ClassFiscal WCh;
        bool IsOpenWorkDay;
        eStateEquipment State;
        int CodeError;
        string fileName;
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
            bool r = WCh.FiscalReceipt(pData.Xml);
            var res = WCh.StatusBarXML();
            return GetResLogRRO(pData.Id, pData.TypeOperation, pData.Sum, res);
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

            var res = WCh.StatusBarXML();
            return GetResLogRRO(pIdR, pTypeOperation,0,res);
        }

        LogRRO GetResLogRRO(IdReceipt pIdR, eTypeOperation pTypeOperation, decimal pSum = 0, string pResXML=null)
        {            
            string ResXML = WCh.StatusBarXML();
            if (string.IsNullOrEmpty(pResXML))
                pResXML = ResXML;
            string TextReceipt = null;
            var FiscalNumber = GetElement(ResXML, "CheckID", "\"", "\"");
            if (!string.IsNullOrEmpty(FiscalNumber))
                TextReceipt = GetCheckByFiscalNumber(FiscalNumber);
            string Error = GetElement(ResXML, "ErrHelp", "\"", "\"");

            CodeError = GetElement(ResXML, "Err", "\"", "\"").ToInt(); ;

            return new LogRRO(pIdR) { TypeOperation = pTypeOperation, TypeRRO = "WebCheck", FiscalNumber = FiscalNumber, SUM = pSum, JSON = pResXML, TextReceipt = TextReceipt, Error = Error, CodeError = CodeError };
        }

        string GetCheckByFiscalNumber(string pTaxNum)
        {
            bool r = WCh.GetCheckByFiscalNumber($"<InputParameters> <Parameters FN=\"{FN}\" TaxNum=\"{pTaxNum}\" type=\"1\" /></InputParameters>");
            return WCh.StatusBarXML();
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

    }
}


