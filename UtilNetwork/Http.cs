using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace UtilNetwork
{    
    public class Http
    {
        public static async Task<HttpResult> HTTPRequestAsync(String pURL, String pData, String pContentType, String pLogin, String pPassWord, double pTimeOut = 15)
        {
            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                HttpClient client;
                client = new HttpClient(handler);

                if (string.IsNullOrEmpty(pLogin)) ;
                // client = new HttpClient();
                else
                {
                    //var credentials = new NetworkCredential(pLogin, pPassWord);
                    // handler.Credentials = credentials;
                    client = new HttpClient();
                    string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(pLogin + ":" + pPassWord));
                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + encoded);

                }

                client.Timeout = TimeSpan.FromSeconds(pTimeOut);

                //client.BaseAddress = new Uri(pURL);
                //var byteArray = Encoding.ASCII.GetBytes("admin:Xa38dF79");
                //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                StringContent content = null;
                if (pData != null) content = new StringContent(pData, Encoding.UTF8, pContentType);
                HttpResponseMessage response = await client.PostAsync(pURL, content).ConfigureAwait(continueOnCapturedContext: false);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return new HttpResult() { HttpState = eStateHTTP.HTTP_OK, Result = result };
                }
                else
                {
                    return new HttpResult() { HttpState = (eStateHTTP)(int)response.StatusCode, Result = response.StatusCode.ToString() };
                }
            }
            catch (Exception e)
            {
                return new HttpResult() { HttpState = eStateHTTP.Exeption, Result = e.Message };
            }
        }
        public static async Task<string> UploadFileAsync(string url, string filePath)
        {
            using (var httpClient = new HttpClient())
            {
                using (var form = new MultipartFormDataContent())
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        var streamContent = new StreamContent(fileStream);
                        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        form.Add(streamContent, "formFile", Path.GetFileName(filePath));

                        var response = await httpClient.PostAsync(url, form);
                        response.EnsureSuccessStatusCode();
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
        }

       
        public static HttpResult LoadFile(string pURL, string pFile)
        {
           try
            {
                WebClient webClient = new();
                webClient.DownloadFile(new Uri(pURL), pFile);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage("Http.LoadFile", e );
            }
            return new HttpResult();
        }
    }
    public enum eStateHTTP
    {
        Exeption = -1,
        HTTP_Not_Define_Error = 0,
        HTTP_OK = 200,

        /**
         * HTTP Status-Code 201: Created.
         */
        HTTP_CREATED = 201,

        /**
         * HTTP Status-Code 202: Accepted.
         */
        HTTP_ACCEPTED = 202,

        /**
         * HTTP Status-Code 203: Non-Authoritative Information.
         */
        HTTP_NOT_AUTHORITATIVE = 203,

        /**
         * HTTP Status-Code 204: No Content.
         */
        HTTP_NO_CONTENT = 204,

        /**
         * HTTP Status-Code 205: Reset Content.
         */
        HTTP_RESET = 205,

        /**
         * HTTP Status-Code 206: Partial Content.
         */
        HTTP_PARTIAL = 206,

        /* 3XX: relocation/redirect */

        /**
         * HTTP Status-Code 300: Multiple Choices.
         */
        HTTP_MULT_CHOICE = 300,

        /**
         * HTTP Status-Code 301: Moved Permanently.
         */
        HTTP_MOVED_PERM = 301,

        /**
         * HTTP Status-Code 302: Temporary Redirect.
         */
        HTTP_MOVED_TEMP = 302,

        /**
         * HTTP Status-Code 303: See Other.
         */
        HTTP_SEE_OTHER = 303,

        /**
         * HTTP Status-Code 304: Not Modified.
         */
        HTTP_NOT_MODIFIED = 304,

        /**
         * HTTP Status-Code 305: Use Proxy.
         */
        HTTP_USE_PROXY = 305,

        /* 4XX: client error */

        /**
         * HTTP Status-Code 400: Bad Request.
         */
        HTTP_BAD_REQUEST = 400,

        /**
         * HTTP Status-Code 401: Unauthorized.
         */
        HTTP_UNAUTHORIZED = 401,

        /**
         * HTTP Status-Code 402: Payment Required.
         */
        HTTP_PAYMENT_REQUIRED = 402,

        /**
         * HTTP Status-Code 403: Forbidden.
         */
        HTTP_FORBIDDEN = 403,

        /**
         * HTTP Status-Code 404: Not Found.
         */
        HTTP_NOT_FOUND = 404,

        /**
         * HTTP Status-Code 405: Method Not Allowed.
         */
        HTTP_BAD_METHOD = 405,

        /**
         * HTTP Status-Code 406: Not Acceptable.
         */
        HTTP_NOT_ACCEPTABLE = 406,

        /**
         * HTTP Status-Code 407: Proxy Authentication Required.
         */
        HTTP_PROXY_AUTH = 407,

        /**
         * HTTP Status-Code 408: Request Time-Out.
         */
        HTTP_CLIENT_TIMEOUT = 408,

        /**
         * HTTP Status-Code 409: Conflict.
         */
        HTTP_CONFLICT = 409,

        /**
         * HTTP Status-Code 410: Gone.
         */
        HTTP_GONE = 410,

        /**
         * HTTP Status-Code 411: Length Required.
         */
        HTTP_LENGTH_REQUIRED = 411,

        /**
         * HTTP Status-Code 412: Precondition Failed.
         */
        HTTP_PRECON_FAILED = 412,

        /**
         * HTTP Status-Code 413: Request Entity Too Large.
         */
        HTTP_ENTITY_TOO_LARGE = 413,

        /**
         * HTTP Status-Code 414: Request-URI Too Large.
         */
        HTTP_REQ_TOO_LONG = 414,

        /**
         * HTTP Status-Code 415: Unsupported Media Type.
         */
        HTTP_UNSUPPORTED_TYPE = 415,

        /* 5XX: server error */

        /**
         * HTTP Status-Code 500: Internal Server Error.
         * @deprecated   it is misplaced and shouldn't have existed.
         */

        HTTP_SERVER_ERROR = 500,

        /**
         * HTTP Status-Code 500: Internal Server Error.
         */
        HTTP_INTERNAL_ERROR = 500,

        /**
         * HTTP Status-Code 501: Not Implemented.
         */
        HTTP_NOT_IMPLEMENTED = 501,

        /**
         * HTTP Status-Code 502: Bad Gateway.
         */
        HTTP_BAD_GATEWAY = 502,

        /**
         * HTTP Status-Code 503: Service Unavailable.
         */
        HTTP_UNAVAILABLE = 503,

        /**
         * HTTP Status-Code 504: Gateway Timeout.
         */
        HTTP_GATEWAY_TIMEOUT = 504,

        /**
         * HTTP Status-Code 505: HTTP Version Not Supported.
         */
        HTTP_VERSION = 505
    }
    public class HttpResult
    {
        public eStateHTTP HttpState = eStateHTTP.HTTP_Not_Define_Error;
        public string Result = null;
    }
}
