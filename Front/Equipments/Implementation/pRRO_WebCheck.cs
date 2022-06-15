using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace Front.Equipments.Implementation
{
    public class pRRO_WebCheck : Rro
    {
        string FN;
        WebCheck.ClassFiscal WCh;
        public pRRO_WebCheck(Equipment pEquipment, IConfiguration pConfiguration, Action<string, string> pLogger = null, Action<StatusEquipment> pActionStatus = null) :
                       base(pEquipment, pConfiguration,eModelEquipment.pRRo_WebCheck, pLogger, pActionStatus)
        {
            try
            {
                State = eStateEquipment.Init;
                WCh = new WebCheck.ClassFiscal();
                FN = pConfiguration["Devices:pRRO_WebCheck:FN"];
              OperatorName = pConfiguration["Devices:pRRO_WebCheck:OperatorID"];
                IsOpenWorkDay = false;
                // !!!TMP Перенести асинхронно бо дуже довго
                if (WCh.Initialization($"<InputParameters> <Parameters FN = \"{FN}\" />  </InputParameters>"))
                {

                    string ResXML = WCh.StatusBarXML();
                    if (!string.IsNullOrEmpty(ResXML))
                    {
                       OpenWorkDay();                        
                    }
                }
                else
                    State = eStateEquipment.Error;
               
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public override bool OpenWorkDay()
        {
            string xml = $"<InputParameters> <Parameters FN=\"{FN}\" OperatorID=\"{OperatorName}\" /> </InputParameters>";

            if (WCh.GetCurrentStatus(xml))
            {
                string ResXML = WCh.StatusBarXML();
                if (!string.IsNullOrEmpty(ResXML))
                {
                    int iShiftNumber = -1;
                    var ShiftNumber = GetElement(ResXML, "ShiftNumber", "\"", "\"");
                    if (ShiftNumber != null)
                        iShiftNumber = ShiftNumber.ToInt(-1);

                    if (ShiftNumber.ToInt(-1) <= 0)
                    {
                        IsOpenWorkDay = WCh.OpenShift(xml);
                        ResXML = WCh.StatusBarXML();
                    }
                    else
                        IsOpenWorkDay = true;
                }
            }
            return IsOpenWorkDay;
        }
        string XmlWares(ReceiptWares pRW)
        {
            string Add = (pRW.IsUseCodeUKTZED ? $"UKTZED={pRW.CodeUKTZED}" : "") +
                         (!string.IsNullOrEmpty(pRW.ExciseStamp) ? " ExciseStamp=\"{pRW.ExciseStamp}\"" : "") +
                         (!string.IsNullOrEmpty(pRW.BarCode) ? $" Barcode=\"{pRW.BarCode}\"" : "");
            return $"<Good Code=\"{pRW.CodeWares}\" Name=\"{pRW.NameWares.ToXMLString() }\" Quantity=\"{pRW.Quantity}\" Price=\"{pRW.PriceDealer}\" Sum=\"{pRW.Sum}\" TaxRate=\"1\"  {Add} />";
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
        string GenGoods(IEnumerable<ReceiptWares> pRW)
        {
            StringBuilder xml = new StringBuilder("<Goods>");
            foreach (var el in pRW)
                xml.Append(XmlWares(el));
            xml.Append("</Goods>\n");
            return xml.ToString();
        }
        string GenL(Receipt pR)
        {
            string pay = "";
            if (pR.Payment != null && pR.Payment.Count() > 0)
            {
                var el = pR.Payment.First();
                pay = $"PA=\"{el.Bank}\" PB=\"{el.NumberTerminal}\" PC=\"{(pR.TypeReceipt == eTypeReceipt.Sale ? "СПЛАТА" : "Повернення")}\" PD=\"{el.NumberCard}\" PE=\"{el.NumberSlip}\" PSNM=\"{el.CardHolder}\" RRN=\"{el.CodeAuthorization}\"";
            }
            return $"<L {pay}/>";
        }

        override public async Task<LogRRO> PrintReceiptAsync(Receipt pR)
        {
            string xml = $"<Check Number=\"{pR.CodeReceipt}\" FN = \"{FN}\" OperationType=\"{(pR.TypeReceipt == eTypeReceipt.Sale ? 0 : 1)}\" uuid=\"{pR.ReceiptId}\">\n" +
                GenL(pR) + "\n" + GenGoods(pR.Wares) +
                $"\n<Payments> <Payment ID=\"{1}\" Sum = \"{pR.SumReceipt}\"/></Payments>\n</Check>";

            bool r = WCh.FiscalReceipt(xml);           
           
            return GetResLogRRO(pR, pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund, pR.SumReceipt);
        }

        LogRRO GetResLogRRO(IdReceipt pIdR, eTypeOperation pTypeOperation,decimal pSum=0)
        {
            string ResXML = WCh.StatusBarXML();
            string TextReceipt = null;
            var FiscalNumber = GetElement(ResXML, "CheckID", "\"", "\"");
            if (!string.IsNullOrEmpty(FiscalNumber))
                TextReceipt = GetCheckByFiscalNumber(FiscalNumber);
            string Error = GetElement(ResXML, "ErrHelp", "\"", "\"");

            CodeError = GetElement(ResXML, "Err", "\"", "\"").ToInt(); ;
            
            return new LogRRO(pIdR) { TypeOperation = pTypeOperation, TypeRRO = "WebCheck", FiscalNumber = FiscalNumber, SUM = pSum, JSON =ResXML, TextReceipt = TextReceipt, Error = Error, CodeError = CodeError };

        }

        string GetCheckByFiscalNumber(string pTaxNum)
        {
            bool r = WCh.GetCheckByFiscalNumber($"<InputParameters> <Parameters FN=\"{FN}\" TaxNum=\"{pTaxNum}\" type=\"1\" /></InputParameters>");
            return WCh.StatusBarXML();
        }

        LogRRO PrintXY(IdReceipt pIdR, eTypeOperation pTypeOperation)
        {
            string xml = $"<InputParameters> <Parameters FN = \"{FN}\"  OperatorID = \"{OperatorName}\" /> </InputParameters>";
            if (pTypeOperation == eTypeOperation.ZReport)
                WCh.ReportZ(xml);
            else
                WCh.ReportX(xml);

            var res = WCh.StatusBarXML();
            return GetResLogRRO(pIdR, pTypeOperation);
        }
        override public async Task<LogRRO> PrintZAsync(IdReceipt pIdR)
        {
            return PrintXY(pIdR, eTypeOperation.ZReport);
        }

        override public async Task<LogRRO> PrintXAsync(IdReceipt pIdR)
        {
            return PrintXY(pIdR, eTypeOperation.XReport);
        }


        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        override public async Task<LogRRO> MoveMoneyAsync(decimal pSum, IdReceipt pIdR)
        {
            string xml = $"<InputParameters> <Parameters FN = \"{FN}\" /> Sum{(pSum>0?"In":"Out")} = \"{Math.Abs(pSum)}\"";
            if(WCh.CashInOut(xml))   
             return GetResLogRRO(pIdR, pSum>0? eTypeOperation.MoneyIn: eTypeOperation.MoneyOut, pSum);
            return null;
        }
        public override string GetDeviceInfo() 
        {
            string res="";
            string xml = $"<InputParameters> <Parameters FN = \"{FN}\" />  </InputParameters>";
            if (WCh.GetCurrentStatus(xml))
            {
                res = WCh.StatusBarXML();              
            }
            else
                res = "GetCurrentStatus Не виконався";
            return res;
        }
    }
}

