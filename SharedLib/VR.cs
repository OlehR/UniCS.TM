using ModelMID;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib
{
    public class VR
    {
        public enum eTypeVRMessage
        {
            AddWares,
            UpdateWares,
            DeleteWares
        }
        public static async Task<string> SendMessageAsync(int IdWorkplace, string pStr, int pCode, decimal pQuantity, decimal pSum, eTypeVRMessage peType = eTypeVRMessage.AddWares)
        {
            string res = null;
            try
            {
                string parContex = "text/xml";
                string Body = GenBody(pStr, pCode, pQuantity, pSum);
                if (string.IsNullOrEmpty(Body))
                    return null;

                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMilliseconds(2000);
                var byteArray = Encoding.ASCII.GetBytes("admin:Xa38dF79");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var url = Global.GetVideoCameraIPByIdWorkplace(IdWorkplace);
                
                if (!string.IsNullOrEmpty(url))
                {
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, url);
                    requestMessage.Content = new StringContent(Body, Encoding.UTF8, parContex);
                    var response = await client.SendAsync(requestMessage);
                }
            }
            catch (Exception ex) 
            {
                var r = ex.Message;
            };

            return res;
        }

        static string GenBody(string pStr,int pCode ,decimal pQuantity, decimal pSum)
            {
            if (string.IsNullOrEmpty(pStr))
                return null;

            int Lenght = 22;
            string Str1= pStr.Substring(0, pStr.Length>= Lenght ? Lenght : pStr.Length), Str2= pStr.Length > Lenght ? pStr.Substring(Lenght+1) : "";
            var Body = "<?xml version=\"1.0\" encoding=\"UTF-8\"?> \n <TextOverlayList version=\"2.0\" xmlns=\"http://www.hikvision.com/ver20/XMLSchema\"> \n" +
    $"<TextOverlay> \n <id>1</id> \n <enabled>true</enabled> \n <positionX>0</positionX> \n <positionY>120</positionY> \n <displayText>{Str1}</displayText> \n </TextOverlay> \n"+
    $"<TextOverlay> \n <id>2</id> \n <enabled>true</enabled> \n <positionX>0</positionX> \n <positionY>90</positionY> \n <displayText>{Str2}</displayText> \n </TextOverlay> \n" +
    $"<TextOverlay> \n <id>3</id> \n <enabled>true</enabled> \n <positionX>0</positionX> \n <positionY>50</positionY> \n <displayText> {pCode} К-ть: {pQuantity} Сума: {pSum}</displayText> \n </TextOverlay> \n" +
    $"<TextOverlay> \n <id>4</id> \n <enabled>true</enabled> \n <positionX>0</positionX> \n <positionY>0</positionY> \n <displayText>  </displayText> \n </TextOverlay> \n" +
    "</TextOverlayList>";
            return Body;
            }
    }
}
