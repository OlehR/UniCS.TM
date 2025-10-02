using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Utils;

namespace Equipments.Equipments.Glory
{
    public static class GloryNetworkUtilities
    {
        public static async Task<string> HTTPRequestAsync(String pURL, string pMetod, String pData, double pTimeOut = 30)
        {
            try
            {
                var cookieContainer = new CookieContainer();

                var handler = new HttpClientHandler()
                {
                    CookieContainer = cookieContainer,
                    UseCookies = true // This is true by default, but explicitly setting it can be helpful for clarity.
                };
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                HttpClient client;
                client = new HttpClient(handler);


                client.DefaultRequestHeaders.Add("SOAPAction", pMetod);

                client.Timeout = TimeSpan.FromSeconds(pTimeOut);

                StringContent content = null;
                if (pData != null) content = new StringContent(pData, Encoding.UTF8, "text/xml");
                using HttpResponseMessage response = await client.PostAsync(pURL, content).ConfigureAwait(continueOnCapturedContext: false);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
                else
                {
                    return response.StatusCode.ToString();
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }

        public static void GloryStartListening(bool IsListening,string IP , int IpPort)
        {
            TcpListener tcpListener = (TcpListener)null;
            int num = 0;

            while (IsListening)
            {
                try
                {
                    tcpListener = new TcpListener(IPAddress.Parse(IP), IpPort);
                    tcpListener.Start();
                    FileLogger.WriteLogMessage( "Glory live listener: Waiting for a connection... ");
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    FileLogger.WriteLogMessage("Glory live listener: Connected!");
                    string str1 = (string)null;
                    NetworkStream stream = tcpClient.GetStream();
                    num = 0;
                    do
                    {
                        byte[] numArray = new byte[tcpClient.ReceiveBufferSize];
                        if (stream.CanRead)
                        {
                            int count = stream.Read(numArray, 0, tcpClient.ReceiveBufferSize);
                            if (count != 0)
                            {
                                string str2 = Encoding.ASCII.GetString(numArray, 0, count);
                                //LogEventStr = $"{LogEventStr} {Environment.NewLine} {str2} {Environment.NewLine} ";
                                FileLogger.WriteLogMessage(($"LiveListenerXML: {Environment.NewLine} {str2}"));

                                //Dispatcher.BeginInvoke(new ThreadStart(() =>
                                //{
                                //    LogEvent.Text = LogEventStr;
                                //    OnPropertyChanged(nameof(LogEvent));
                                //    LogEvent_ScrollViewer.ScrollToEnd();

                                //}));

                                var ser = new XmlSerializer(typeof(BbxEventRequest));
                                using var sr = new StringReader(str2);
                                var evt = (BbxEventRequest)ser.Deserialize(sr);
                                // str2 - строка з XML який прийшов з кеш-машини
                                // evt - розпаршений клас відповіді




                            }
                            else
                                break;
                        }
                        else
                        {
                            FileLogger.WriteLogMessage("Glory live listener: stream.CanRead false");
                            break;
                        }
                    }
                    while (IsListening);
                    FileLogger.WriteLogMessage("Glory live listener: Listening Stop");
                    tcpClient.Close();
                }
                catch (SocketException ex)
                {
                    ++num;
                    if (10 > num)
                        FileLogger.WriteLogMessage( $"Glory live listener: SocketException: {ex}");
                    else if (10 == num)
                        FileLogger.WriteLogMessage("Glory live listener: SocketException: LogWriteCounterOver");
                }
                catch (Exception ex)
                {
                    ++num;
                    if (10 > num)
                        FileLogger.WriteLogMessage( $"Glory live listener: Exception: {ex}");
                    else if (10 == num)
                        FileLogger.WriteLogMessage("Glory live listener: Exception: LogWriteCounterOver");
                }
                finally
                {
                    tcpListener.Stop();
                }
            }
        }
    }
}
