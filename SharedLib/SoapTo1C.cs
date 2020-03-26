using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using ModelMID;

namespace SharedLib
{
    public class Parameters
    {
        public Parameters() { }
        public Parameters(string parName, string parValue)
        {
            Name = parName;
            Value = parValue;
        }
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class SoapTo1C
    {
        public string GenBody(string parFunction, IEnumerable<Parameters> parPar)
        {
            string parameters = "";
            if (parPar != null)
                foreach (var el in parPar)
                    parameters += $"\n<{el.Name}>{el.Value}</{el.Name}>";

            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                 "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd = \"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\n" +
                    $"<soap:Body>\n<{parFunction} xmlns=\"vopak\">{parameters}</{parFunction}>\n</soap:Body>\n</soap:Envelope>";
        }

        public async System.Threading.Tasks.Task<string> RequestAsync(string parUrl,string parBody,int parWait=1000,string parContex= "text/xml")
        {
            string res = null;
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(parWait);

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, parUrl);

            requestMessage.Content = new StringContent(parBody, Encoding.UTF8, parContex);
            var response = await client.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                res = await response.Content.ReadAsStringAsync();
                res = res.Substring(res.IndexOf(@"-instance"">") + 11);
                res = res.Substring(0, res.IndexOf("</m:return>")).Trim();
            }
            else
            {  
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation {  Exception = null, Status = eSyncStatus.NoFatalError, StatusDescription = "RequestAsync=>" + response.RequestMessage });
            }
            return res;
        }


    }
}
