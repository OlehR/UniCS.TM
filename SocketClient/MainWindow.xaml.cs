using System;
using System.Text;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Collections.ObjectModel;

namespace SocketClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        // адрес и порт сервера, к которому будем подключаться
        static int port = 8005; // порт сервера
        static string address = "127.0.0.1"; // адрес сервера
        public int id = 1;
        public ObservableCollection<ListHistoris> Histori { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Histori = new ObservableCollection<ListHistoris> { };
            ListHistori.ItemsSource = Histori;
        }

        private void SentMessage(object sender, RoutedEventArgs e)
        {
            string Response = SocketClient(WriteTextMesage.Text);
            Histori.Add(new ListHistoris { Id = id, TextMessage = WriteTextMesage.Text, ServerResponse = Response });
            id++;
        }

        public static string SocketClient(string Message)
        {
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // подключаемся к удаленному хосту
                socket.Connect(ipPoint);
                string message = Message;
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
                //MessageBox.Show("Відповідь сервера: " + builder.ToString());

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

        private void SentCashier(object sender, RoutedEventArgs e)
        {
            string Response= SocketClient("111 222");
            Histori.Add(new ListHistoris { Id = id, TextMessage = "111 222", ServerResponse = Response });
            id++;
        }

        private void SentWeight(object sender, RoutedEventArgs e)
        {
            var rand = new Random();
            var local = rand.Next(40, 2000).ToString();
            string Response = SocketClient(local);
            Histori.Add(new ListHistoris { Id = id, TextMessage = local, ServerResponse = Response });
            id++;
            // MessageBox.Show(rand.ToString());
        }

        private void historiList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ListHistoris p = (ListHistoris)ListHistori.SelectedItem;
            WriteTextMesage.Text = p.TextMessage;
        }
        public class ListHistoris
        {
            public int Id { get; set; }
            public string TextMessage { get; set; }
            public string ServerResponse { get; set; }
        }
    }
}

