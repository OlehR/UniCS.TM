using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utils;
using static System.Net.WebRequestMethods;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Threading;
using System.Globalization;

namespace Front.Equipments.Implementation
{
    public class pRRO_WebCheck : Rro
    {
        string Url, FN;
        public pRRO_WebCheck(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) :
                  base(pEquipment, pConfiguration, eModelEquipment.pRRo_WebCheck, pLoggerFactory, pActionStatus)
        {
            State = eStateEquipment.Init;
            //TMP!!!
            //WCh = new WebCheck.ClassFiscal();
            Url = pConfiguration["Devices:pRRO_WebCheck:Url"];
            FN =  RequestAsync($"{Url}/FN", HttpMethod.Post, null, 5000, "application/json").Replace("\"","");
            if(string.IsNullOrEmpty(FN))
                State = eStateEquipment.Init;
        }

        override public bool OpenWorkDay()
        {
             var r = RequestAsync($"{Url}/OpenWorkDay", HttpMethod.Post, null, 5000, "application/json");
                return true;            
        }

        override public LogRRO PrintReceipt(Receipt pR)
        {
            var culture = new CultureInfo("en-US");
            culture.NumberFormat.NumberDecimalSeparator = ".";
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            string xml = $"<Check Number=\"{pR.CodeReceipt}\" FN = \"{FN}\" OperationType=\"{(pR.TypeReceipt == eTypeReceipt.Sale ? 0 : 1)}\" uuid=\"{pR.ReceiptId}\">\n" +
                GenL(pR) + "\n" + GenGoods(pR.Wares) +
                $"\n<Payments> <Payment ID=\"{1}\" Sum = \"{pR.SumReceipt.ToS()}\"/></Payments>\n</Check>";

            PrintReceiptData Data = new() { Xml = xml, Id = pR, TypeOperation = pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund, Sum = pR.SumTotal };
            var r = RequestAsync($"{Url}/PrintReceipt", HttpMethod.Post, Data.ToJSON(), 5000, "application/json");
            if (!string.IsNullOrEmpty(r))
                return JsonConvert.DeserializeObject<LogRRO>(r);
            return null;
        }

        override public LogRRO PrintZ(IdReceipt pIdR)
        {
            var r = RequestAsync($"{Url}/PrintZ", HttpMethod.Post, null, 5000, "application/json");
            if (!string.IsNullOrEmpty(r))
                return JsonConvert.DeserializeObject<LogRRO>(r);
            return null;
        }

        override public LogRRO PrintX(IdReceipt pIdR)
        {
            var r = RequestAsync($"{Url}/PrintX", HttpMethod.Post, null, 5000, "application/json");
            if (!string.IsNullOrEmpty(r))
                return JsonConvert.DeserializeObject<LogRRO>(r);
            return null;
        }

        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        override public LogRRO MoveMoney(decimal pSum, IdReceipt pIdR)
        {
            PrintReceiptData Data = new() { Id = pIdR, TypeOperation = pSum >0? eTypeOperation.MoneyIn : eTypeOperation.MoneyOut, Sum = pSum};

            var r = RequestAsync($"{Url}/MoveMoney", HttpMethod.Post, Data.ToJSON(), 5000, "application/json");
            if (!string.IsNullOrEmpty(r))
                return JsonConvert.DeserializeObject<LogRRO>(r);
            return null;
        }

        /*   string FN;
           //TMP!!! //WebCheck.ClassFiscal
           dynamic WCh;
           public pRRO_WebCheck(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) :
                          base(pEquipment, pConfiguration, eModelEquipment.pRRo_WebCheck, pLoggerFactory, pActionStatus)
           {
               State = eStateEquipment.Init;
               //TMP!!!
               //WCh = new WebCheck.ClassFiscal();
               FN = pConfiguration["Devices:pRRO_WebCheck:FN"];
               OperatorName = pConfiguration["Devices:pRRO_WebCheck:OperatorID"];
               IsOpenWorkDay = false;
           }

           override public void Init()
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

           override public bool OpenWorkDay()
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

           override public LogRRO PrintReceipt(Receipt pR)
           {
               string xml = $"<Check Number=\"{pR.CodeReceipt}\" FN = \"{FN}\" OperationType=\"{(pR.TypeReceipt == eTypeReceipt.Sale ? 0 : 1)}\" uuid=\"{pR.ReceiptId}\">\n" +
                   GenL(pR) + "\n" + GenGoods(pR.Wares) +
                   $"\n<Payments> <Payment ID=\"{1}\" Sum = \"{pR.SumReceipt}\"/></Payments>\n</Check>";

               bool r = WCh.FiscalReceipt(xml);

               return GetResLogRRO(pR, pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund, pR.SumReceipt);
           }

           override public LogRRO PrintZ(IdReceipt pIdR)
           {
               return PrintXY(pIdR, eTypeOperation.ZReport);
           }

           override public LogRRO PrintX(IdReceipt pIdR)
           {
               return PrintXY(pIdR, eTypeOperation.XReport);
           }

           /// <summary>
           /// Внесення/Винесення коштів коштів. pSum>0 - внесення
           /// </summary>
           /// <param name="pSum"></param>
           /// <returns></returns>
           override public LogRRO MoveMoney(decimal pSum, IdReceipt pIdR)
           {
               string xml = $"<InputParameters> <Parameters FN = \"{FN}\" /> Sum{(pSum > 0 ? "In" : "Out")} = \"{Math.Abs(pSum)}\"";
               if (WCh.CashInOut(xml))
                   return GetResLogRRO(pIdR, pSum > 0 ? eTypeOperation.MoneyIn : eTypeOperation.MoneyOut, pSum);
               return null;
           }

           override public StatusEquipment TestDevice()
           {
               string Error = null;
               string Res = null;
               try
               {
                   string xml = $"<InputParameters> <Parameters FN = \"{FN}\" />  </InputParameters>";
                   if (WCh.GetCurrentStatus(xml))
                   {
                       Res = WCh.StatusBarXML();
                   }
                   else
                       Res = "GetCurrentStatus Не виконався";
               }
               catch (Exception e)
               {
                   Error = e.Message;
                   State = eStateEquipment.Error;
               }
               return new StatusEquipment(Model, State, $"{Error} {Environment.NewLine} {Res}");
           }

           override public string GetDeviceInfo()
           {
               string res = "";
               string xml = $"<InputParameters> <Parameters FN = \"{FN}\" />  </InputParameters>";
               if (WCh.GetCurrentStatus(xml))
               {
                   res = WCh.StatusBarXML();
               }
               else
                   res = "GetCurrentStatus Не виконався";
               return $"State={State} {res}";
           }


           LogRRO GetResLogRRO(IdReceipt pIdR, eTypeOperation pTypeOperation, decimal pSum = 0)
           {
               string ResXML = WCh.StatusBarXML();
               string TextReceipt = null;
               var FiscalNumber = GetElement(ResXML, "CheckID", "\"", "\"");
               if (!string.IsNullOrEmpty(FiscalNumber))
                   TextReceipt = GetCheckByFiscalNumber(FiscalNumber);
               string Error = GetElement(ResXML, "ErrHelp", "\"", "\"");

               CodeError = GetElement(ResXML, "Err", "\"", "\"").ToInt(); ;

               return new LogRRO(pIdR) { TypeOperation = pTypeOperation, TypeRRO = "WebCheck", FiscalNumber = FiscalNumber, SUM = pSum, JSON = ResXML, TextReceipt = TextReceipt, Error = Error, CodeError = CodeError };

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
        */

        string XmlWares(ReceiptWares pRW)
        {
            string Add = (pRW.IsUseCodeUKTZED ? $"UKTZED=\"{pRW.CodeUKTZED}\"" : "") +
                         (!string.IsNullOrEmpty(pRW.ExciseStamp) ? $" ExciseStamp=\"{pRW.ExciseStamp}\"" : "") +
                         (!string.IsNullOrEmpty(pRW.BarCode) ? $" Barcode=\"{pRW.BarCode}\"" : "");
            return $"<Good Code=\"{pRW.CodeWares}\" Name=\"{pRW.NameWares.ToXMLString()}\" Quantity=\"{pRW.Quantity.ToS()}\" Price=\"{pRW.PriceEKKA.ToS()}\" Sum=\"{pRW.Sum.ToS()}\" TaxRate=\"{pRW.TaxGroup}\"  {Add} />";
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

        static public string RequestAsync(string parUrl, HttpMethod pMethod, string pBody = null, int pWait = 5000, string pContex = "application/json;charset=UTF-8", AuthenticationHeaderValue pAuthentication = null)
        {
            string res = null;
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(pWait);

            if (pAuthentication != null)
                client.DefaultRequestHeaders.Authorization = pAuthentication;
            HttpRequestMessage requestMessage = new HttpRequestMessage(pMethod, parUrl);
            if (!string.IsNullOrEmpty(pBody))
                requestMessage.Content = new StringContent(pBody, Encoding.UTF8, pContex);

            var response = client.SendAsync(requestMessage).Result;

            if (response.IsSuccessStatusCode)
            {
                res = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                return null;
            }
            return res;
        }

    }
    
    public class PrintReceiptData
    {        
        public IdReceipt Id { get; set; }       
        public string Xml { get; set; }       
        public eTypeOperation TypeOperation { get; set; }      
        public decimal Sum { get; set; }
    }

}

