using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Front.Equipments.pRRO_SG
{
    /// <summary>
    /// пРРО від SystemGroup
    /// </summary>
    public class pRRO_SG:Rro
    {
        string PathApi;
        int Wait;
        HttpClient client = new HttpClient();
       
        public pRRO_SG(IConfiguration pConfiguration, Action<string, string> pLogger = null, Action<eStatusRRO> pActionStatus = null) : 
                        base(pConfiguration, pLogger, pActionStatus) 
        {
            PathApi = pConfiguration["Devices:pRRO_SG:PathApi"];
            Wait = int.Parse(pConfiguration["Devices:pRRO_SG:Wait"] ?? "10000");
            client.Timeout = TimeSpan.FromMilliseconds(Wait);           
        }

        async Task<(string, HttpStatusCode)> HttpAsync(string pMetod, string pBody)
        {
            string res = null;
            HttpStatusCode Response = HttpStatusCode.Conflict;
            
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMilliseconds(15000);
            
                using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, PathApi + pMetod))
                {
                    try
                    {
                        Debug.WriteLine("HttpAsync/n" + pBody);
                        requestMessage.Content = new StringContent(pBody, Encoding.UTF8, "application/json");
                        SetStatus(eStatusRRO.WaitAnswer);
                        Debug.WriteLine("SendAsync=>");
                        var response = await client.SendAsync(requestMessage);
                        Debug.WriteLine("Response=>" + response.StatusCode);
                        SetStatus(eStatusRRO.ParseResult);
                        Response = response.StatusCode;
                        if (response.IsSuccessStatusCode)
                        {
                            res = await response.Content.ReadAsStringAsync();
                            Debug.WriteLine(res);
                        }
                        else
                        {
                            SetStatus(eStatusRRO.Error);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        SetStatus(eStatusRRO.Error);
                    }
                }
           
            return (res, Response);
            }

            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                SetStatus(eStatusRRO.Error);
            }
            return (res, Response);
        }

        override public async Task<LogRRO> PrintReceiptAsync(Receipt pR)
        {
            string res;
            var Res = new LogRRO(pR);
            HttpStatusCode Response;
            var r = new pRroRequestSG(pR);
            var Body = JsonConvert.SerializeObject(r);
            Debug.WriteLine("HttpAsync Start=>");
            (res, Response) = await HttpAsync("/innovate/printreceipt", Body);
            Debug.WriteLine("HttpAsync=>"+ res);
            //Thread.Sleep(2000);
            if (Response == HttpStatusCode.OK)
            {
                var xx = JsonConvert.DeserializeObject<pRroAnswerSG>(res);
                Res.TypeOperation = pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund;
                Res.NumberOperation = xx.receiptNumber;
                Res.TextReceipt = xx.text;
                Res.SUM = xx.sum;
                Res.FiscalNumber = xx.fiscalNumber;
                Res.JSON = res; //JsonConvert.SerializeObject(xx, Formatting.Indented);
                SetStatus(eStatusRRO.OK);
            }
            else
            {
                Res.Error = Response.ToString();
                Res.CodeError = -1;
            }
            return Res;
        }

        override public async Task<LogRRO> PrintZAsync(IdReceipt pIdR)
        {
            return await PrintXYAsync(false, pIdR);
        }

        override public async Task<LogRRO> PrintXAsync(IdReceipt pIdR)
        {
            return await PrintXYAsync(true, pIdR);
        }

        public async Task<LogRRO>  PrintXYAsync(bool pIsX, IdReceipt pIdR)
        {
            string res;
            var Res = new LogRRO(pIdR);
            Res.CodeReceipt = 0;
            HttpStatusCode Response;
            
            (res, Response) = await HttpAsync($"/innovate/{(pIsX?"x":"z")}report", "{\"cashierName\": \"" + OperatorName +"\"}");
            if (Response == HttpStatusCode.OK)
            {
                var xx = JsonConvert.DeserializeObject<pRroAnswerSG>(res);
                Res.TypeOperation = pIsX ? eTypeOperation.XReport : eTypeOperation.ZReport;
                Res.NumberOperation = xx.receiptNumber;
                Res.TextReceipt = xx.text;
                Res.SUM = Convert.ToDecimal(xx.sum) / 100m;
                Res.JSON = res; //JsonConvert.SerializeObject(xx, Formatting.Indented);
                SetStatus(eStatusRRO.OK);
            }
            else
            {
                Res.Error = Response.ToString();
                Res.CodeError = -1;
            }
            return Res;
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
            Res.CodeReceipt = 0;

            var Body = JsonConvert.SerializeObject(new pRroRequestBaseSG() 
                { docSubType= (pSum>0?eTypeDoc.MoneyIn : eTypeDoc.MoneyOut),sum= pSum,cashierName= OperatorName });
            string res;
            HttpStatusCode Response;
           
            (res, Response) = await HttpAsync("innovate/service"+(pSum >0? "in" :"out"), Body);
            if (Response == HttpStatusCode.OK)
            {
                var xx = JsonConvert.DeserializeObject<pRroAnswerSG>(res);
                Res.TypeOperation = pSum > 0 ? eTypeOperation.MoneyIn : eTypeOperation.MoneyOut;
                Res.TextReceipt = xx.text;
                Res.SUM = xx.sum;
                Res.NumberOperation = xx.receiptNumber;
                Res.JSON = res;
                SetStatus(eStatusRRO.OK);
            }
            else
                Res.Error = Response.ToString();
            return Res;
        }
    }
}
