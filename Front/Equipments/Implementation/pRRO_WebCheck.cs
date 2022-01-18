using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Equipments.Implementation
{
    public class pRRO_WebCheck : Rro
    {
        string FN;
        WebCheck.ClassFiscal WCh = new WebCheck.ClassFiscal();
        public pRRO_WebCheck(IConfiguration pConfiguration, Action<string, string> pLogger = null, Action<eStatusRRO> pActionStatus = null) :
                       base(pConfiguration, pLogger, pActionStatus)
        {
            try
            {
                FN = pConfiguration["Devices:pRRO_WebCheck:FN"];
              OperatorName = pConfiguration["Devices:pRRO_WebCheck:OperatorID"];
                // !!!TMP Перенести асинхронно бо дуже довго
                bool r = WCh.Initialization($"<InputParameters> <Parameters FN = \"{FN}\"/> </InputParameters>");
                var rr = WCh.StatusBarXML();
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        bool OpenShift()
        {
            string xml = $"<InputParameters> <Parameters FN=\"{FN}\"  OperatorID=\"{OperatorName}\"/> </InputParameters>";
            return WCh.OpenShift(xml);
        }

        string ToXMLString(string pStr)
        {
            return pStr.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("'", "&apos;").Replace("<", "&lt;").Replace(">", "&gt;");//.Replace(".", "&period;");
        }
        string XmlWares(ReceiptWares pRW)
        {
            string Add = (pRW.IsUseCodeUKTZED ? $"UKTZED={pRW.CodeUKTZED}" : "") +
                         (!string.IsNullOrEmpty(pRW.ExciseStamp) ? " ExciseStamp=\"{pRW.ExciseStamp}\"" : "") +
                         (!string.IsNullOrEmpty(pRW.BarCode) ? $" Barcode=\"{pRW.BarCode}\"" : "");
            return $"<Good Code=\"{pRW.CodeWares}\" Name=\"{ToXMLString(pRW.NameWares) }\" Quantity=\"{pRW.Quantity}\" Price=\"{pRW.Price}\" Sum=\"{pRW.Sum}\" TaxRate=\"1\"  {Add} />";
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
                $"\n<Payments> <Payment ID=\"{1}\" Sum = \"{pR.SumCreditCard}\"/></Payments>\n</Check>";

            bool r = WCh.FiscalReceipt(xml);
            string ResXML = WCh.StatusBarXML();
            

            var FiscalNumber = GetElement(ResXML, "CheckID", "\"", "\"");
            string Error = GetElement(ResXML, "ErrHelp", "\"", "\"");
            string CodeError = GetElement(ResXML, "Err", "\"", "\"");

            var Res = new LogRRO(pR) {  FiscalNumber= FiscalNumber,SUM= pR.SumCreditCard, TypeRRO= "WebCheck", JSON = GetCheckByFiscalNumber(FiscalNumber), Error = Error,CodeError=int.Parse(CodeError) };

            
            return Res;
        }

        string GetCheckByFiscalNumber(string pTaxNum)
        {
            bool r = WCh.GetCheckByFiscalNumber($"<InputParameters> <Parameters FN=\"{FN}\" TaxNum=\"{pTaxNum}\" type=\"1\" /></InputParameters>");
            return WCh.StatusBarXML();
        }

        override public async Task<LogRRO> PrintZAsync(IdReceipt pIdR)
        {
            //innovate/zreport
            //throw new NotImplementedException();
            return null;
        }

        override public async Task<LogRRO> PrintXAsync(IdReceipt pIdR)
        {
            ///innovate/xreport 
            //throw new NotImplementedException();
            return null;//await PrintXYAsync(true, pIdR);
        }


        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        override public async Task<LogRRO> MoveMoneyAsync(decimal pSum, IdReceipt pIdR)
        {
            ///innovate/servicein внесення /innovate/serviceout  --винесення
            var Res = new LogRRO(pIdR);

            return Res;
        }
    }
}

