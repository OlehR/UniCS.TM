using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Front.API
{
    public class KasaSocketClient
    {
        public  string SendMessage(string Message, eTypeMessage typeMessage, string address = "127.0.0.1", int port = 8068)
        {
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // подключаемся к удаленному хосту
                socket.Connect(ipPoint);
                string message = $"{Message};{typeMessage};";
                byte[] data = Encoding.Unicode.GetBytes(message);
                socket.Send(data);

                // получаем ответ
                data = new byte[256]; // буфер для ответа
                StringBuilder builder = new StringBuilder();
                int bytes = 0; // количество полученных байт

                do
                {
                    bytes = socket.Receive(data, data.Length, 0);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (socket.Available > 0);
                

                // закрываем сокет
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                return builder.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
