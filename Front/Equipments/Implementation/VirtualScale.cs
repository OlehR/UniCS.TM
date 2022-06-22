using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Front.Equipments.Virtual;

namespace Front.Equipments.Implementation
{
    public class VirtualScale:Scale
    {
        public VirtualScale(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<double, bool> pOnScalesData=null) : base(pEquipment, pConfiguration, eModelEquipment.VirtualScale, pLoggerFactory, pOnScalesData)
        {
            IpPort = Convert.ToInt32(Configuration["Devices:VirtualScaner:Port"]);
            IP = Configuration["Devices:VirtualScaner:IP"];
            _ = Work();
            State = eStateEquipment.On;
        }

        public async Task Work()
        {
            await Task.Run(() =>
            {
                // получаем адреса для запуска сокета
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(IP), IpPort);

                // создаем сокет
                Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    // связываем сокет с локальной точкой, по которой будем принимать данные
                    listenSocket.Bind(ipPoint);

                    // начинаем прослушивание
                    listenSocket.Listen(10);

                    Console.WriteLine("Сервер запущен. Ожидание подключений...");

                    while (true)
                    {
                        Socket handler = listenSocket.Accept();
                        // получаем сообщение
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0; // количество полученных байтов
                        byte[] data = new byte[256]; // буфер для получаемых данных

                        do
                        {
                            bytes = handler.Receive(data);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (handler.Available > 0);

                        //Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + builder.ToString());


                        OnScalesData?.Invoke(Convert.ToDouble(builder.ToString()), true);

                        // отправляем ответ
                        string message = "ваше сообщение доставлено";
                        data = Encoding.Unicode.GetBytes(message);

                        handler.Send(data);
                        // закрываем сокет
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }

        public override StatusEquipment TestDevice()
        {
            return new StatusEquipment(Model, State, "Ok");
        }
        public override string GetDeviceInfo()
        {
            return $"pModelEquipment={Model} State={State} Port={SerialPort} BaudRate={BaudRate}{Environment.NewLine}";
        }

    };

}

