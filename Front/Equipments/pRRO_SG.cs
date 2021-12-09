using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;
using Newtonsoft.Json;
using System;
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
           
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, PathApi + pMetod))
            {
                try
                {
                    requestMessage.Content = new StringContent(pBody, Encoding.UTF8, "application/json");
                    SetStatus(eStatusRRO.WaitAnswer);
                    var response = await client.SendAsync(requestMessage);
                    SetStatus(eStatusRRO.ParseResult);
                    Response = response.StatusCode;
                    if (response.IsSuccessStatusCode)
                    {
                        res = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        SetStatus(eStatusRRO.Error);
                    }
                }
                catch(Exception e)
                {
                    SetStatus(eStatusRRO.Error);
                }
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
            (res, Response) = await HttpAsync("/innovate/printreceipt", Body);
            if (Response == HttpStatusCode.OK)
            {
                var xx = JsonConvert.DeserializeObject<pRroAnswerSG>(res);
                Res.NumberOperation = xx.receiptNumber;
                Res.TextReceipt = xx.text;
                Res.SUM = Convert.ToDecimal(xx.sum) / 100m;
                Res.FiscalNumber = xx.fiscalNumber;
                Res.JSON = res; //JsonConvert.SerializeObject(xx, Formatting.Indented);
                SetStatus(eStatusRRO.OK);
            }
            else
                Res.Error = Response.ToString();
            return Res;
        }

        override public async Task<LogRRO> PrintZAsync(IdReceipt pIdR)
        {
            //innovate/zreport
            //throw new NotImplementedException();
            return await PrintXYAsync(false, pIdR);
        }

        override public async Task<LogRRO> PrintXAsync(IdReceipt pIdR)
        {
            ///innovate/xreport 
            //throw new NotImplementedException();
            return await PrintXYAsync(true, pIdR);
        }


        public async Task<LogRRO>  PrintXYAsync(bool pIsX, IdReceipt pIdR)
        {
            string res;
            var Res = new LogRRO(pIdR);
            Res.CodeReceipt = 0;
            HttpStatusCode Response;
            
            (res, Response) = await HttpAsync($"/innovate/{(pIsX?"x":"z")}report", "{}");
            if (Response == HttpStatusCode.OK)
            {
                var xx = JsonConvert.DeserializeObject<pRroAnswerSG>(res);
                Res.NumberOperation = xx.receiptNumber;
                Res.TextReceipt = xx.text;
                Res.SUM = Convert.ToDecimal(xx.sum) / 100m;
                Res.JSON = res; //JsonConvert.SerializeObject(xx, Formatting.Indented);
                SetStatus(eStatusRRO.OK);
            }
            else
                Res.Error = Response.ToString();
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
                { docSubType= (pSum>0?eTypeDoc.MoneyIn : eTypeDoc.MoneyOut),Sum= pSum,cashierName= OperatorName });
            string res;
            HttpStatusCode Response;
           
            (res, Response) = await HttpAsync("innovate/service"+(pSum >0? "in" :"out"), Body);
            if (Response == HttpStatusCode.OK)
            {
                var xx = JsonConvert.DeserializeObject<pRroAnswerSG>(res);
                Res.TextReceipt = xx.text;
                Res.SUM = Convert.ToDecimal(xx.sum) / 100m;
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
