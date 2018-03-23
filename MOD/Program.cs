using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Media;
//using Microsoft.DirectX.AudioVideoPlayback;

class Program
{
    static void Send(List<string> paths)
    {
        Console.Title += "  ::Передача::";
        FileStream stream = null;
        BinaryReader f = null;
        byte[] fNumb = new byte[1] { 0 }; //Кол-во файлов
        byte[] bFName = new byte[512]; //Имя файла
        byte[] bFSize = new byte[512]; //Размер файла
        byte[] buffer = new byte[1024]; //Буфер для файла
        byte[] ping = new byte[1] { 0 }; //Синхронизация
        string fName = ""; //Имя ф-ла
        string host = ""; //Имя конечной точки
        ulong fSize = 0; //Размер ф-ла

        try
        {
            Console.WriteLine("Введите IP или сетевое имя принимаемого компьютера");
            Console.Write("\tIP or Host: ");
            Console.CursorVisible = true;
            host = Console.ReadLine();
            Console.CursorVisible = false;
            Console.WriteLine();
            fNumb[0] = (byte)paths.Count;
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = Dns.GetHostEntry(host).AddressList[0];
            IPEndPoint Addr = new IPEndPoint(ipAddress, 7070);
            s.Connect(Addr);
            SystemSounds.Asterisk.Play();
            s.Send(fNumb);
            s.Receive(ping); // 1
            for (byte i = 0; i < paths.Count; i++)
            {
                fName = Path.GetFileName(paths[i]);
                bFName = Encoding.UTF8.GetBytes(fName);
                //s.Send(bFName, fName.Length, SocketFlags.None); //Передаем имя
                s.Send(bFName); //Передаем имя
                s.Receive(ping); // 2
                stream = new FileStream(paths[i], FileMode.Open, FileAccess.Read);
                f = new BinaryReader(stream);
                fSize = (ulong)stream.Length;
                bFSize = Encoding.UTF8.GetBytes(Convert.ToString(stream.Length));
                s.Send(bFSize); //Передаем размер
                s.Receive(ping); // 3
                
                int bytes = 1024;
                Console.WriteLine("Передача файла " + fName + " розміром " + fSize.ToString() + ":");
                Console.BackgroundColor = ConsoleColor.Cyan;
                ulong processed = 0; //Байт передано
                while (processed < fSize) //Передаем файл
                {
                    if ((fSize - processed) < 1024)
                    {
                        bytes = (int)(fSize - processed);
                        byte[] buf = new byte[bytes];
                        f.Read(buf, 0, bytes);
                        int count_bytes = s.Send(buf);
                        processed = processed + (ulong)count_bytes;
                        //Console.WriteLine(processed.ToString() + "-" + count_bytes.ToString()+"-"+fSize);
                    }
                    else
                    {
                        f.Read(buffer, 0, bytes);
                        int count_bytes = s.Send(buffer);
                        processed = processed + (ulong)count_bytes;
                        //Console.WriteLine(processed.ToString() + "-" + count_bytes.ToString()+"-"+fSize);
                    }
                                         
                    Console.CursorLeft = 3; //Рисуем с татус бар
                    if ((processed * 100 / fSize) == 10)
                    { Console.Write("  "); Console.CursorLeft = 40; Console.Write("10%"); }
                    else if ((processed * 100 / fSize) == 20)
                    { Console.Write("    "); Console.CursorLeft = 40; Console.Write("20%"); }
                    else if ((processed * 100 / fSize) == 30)
                    { Console.Write("      "); Console.CursorLeft = 40; Console.Write("30%"); }
                    else if ((processed * 100 / fSize) == 40)
                    { Console.Write("        "); Console.CursorLeft = 40; Console.Write("40%"); }
                    else if ((processed * 100 / fSize) == 50)
                    { Console.Write("          "); Console.CursorLeft = 40; Console.Write("50%"); }
                    else if ((processed * 100 / fSize) == 60)
                    { Console.Write("            "); Console.CursorLeft = 40; Console.Write("60%"); }
                    else if ((processed * 100 / fSize) == 70)
                    { Console.Write("             "); Console.CursorLeft = 40; Console.Write("70%"); }
                    else if ((processed * 100 / fSize) == 80)
                    { Console.Write("               "); Console.CursorLeft = 40; Console.Write("80%"); }
                    else if ((processed * 100 / fSize) == 90)
                    { Console.Write("                  "); Console.CursorLeft = 40; Console.Write("90%"); }
                    

                }

                Console.CursorLeft = 3;
                Console.Write("                    "); Console.CursorLeft = 40; Console.WriteLine("100%");
                Console.BackgroundColor = ConsoleColor.DarkYellow;
                s.Receive(ping); // 5
                Console.WriteLine("Файл " + fName + " передан.\n");
                if (f != null)
                { f.Close(); }
            }
            s.Receive(ping);
            s.Close();
            SystemSounds.Beep.Play();
            Console.WriteLine("\n--------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Передано " + fNumb[0] + " файлов.");
            Console.WriteLine("Целевой компьютер: " + host);
        }
        catch (Exception e)
        {
            /*Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;*/
            Console.WriteLine("test"+e.Message);
        }
    }

    static void Receive(string path)
    {
        Console.Title += "  ::Прием::";
        FileStream stream = null;
        BinaryWriter f = null;
        byte[] fNumb = new byte[1] { 0 }; //Кол-во файлов
        byte[] bFName = new byte[512]; //Имя файла
        byte[] bFSize = new byte[512]; //Размер файла
        byte[] buffer = new byte[1024]; //Буфер для файла
        byte[] ping = new byte[1] { 0 }; //Синхронизация
        string fName = ""; //Имя ф-ла
        string fullPath = ""; //Полный путь
        ulong fSize = 0; //Размер ф-ла

        try
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
            // IPAddress ipAddress = new IPAddress(0xFFFFFFFF);
            IPEndPoint Addr = new IPEndPoint(IPAddress.Any, 7070);
            s.Bind(Addr);
            Console.WriteLine("Ждем подключения...");
            s.Listen(1);

            Socket cl = s.Accept(); //Коннектнутый сокет
            Console.WriteLine("Подключение выполнено...");
            SystemSounds.Asterisk.Play();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path); //Создаем каталог
            }
            cl.Receive(fNumb); //в [0] лежит кол-во файлов
            cl.Send(ping); // 1
            Console.WriteLine();


            for (byte i = 0; i < fNumb[0]; i++)
            {
                cl.Receive(bFName); //Принимаем имя
                cl.Send(ping); // 2
                fName = Encoding.UTF8.GetString(bFName);
                for (int k = 0; k < bFName.Length; k++)
                {
                    bFName[k] = 0;
                }
                fName = fName.TrimEnd('\0');
                if (fName == "")
                { fName = " "; }
                fullPath = path + fName;
                while (File.Exists(fullPath))
                {
                    int dotPos = fullPath.LastIndexOf('.');
                    if (dotPos == -1)
                    {
                        fullPath += "[1]";
                    }
                    else
                    {
                        fullPath = fullPath.Insert(dotPos, "[1]");
                    }
                }
                cl.Receive(bFSize); //Принимаем размер
                cl.Send(ping); // 3
                fSize = Convert.ToUInt64(Encoding.UTF8.GetString(bFSize));
                stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);
                f = new BinaryWriter(stream);
                Console.WriteLine("Передача файла " + fName + ":");
                Console.WriteLine("Розмір файла " + fSize + ":");
                Console.BackgroundColor = ConsoleColor.Cyan;
                ulong processed = 0; //Байт принято
                while (processed < fSize) //Принимаем файл
                {
                    if ((fSize - processed) < 1024)
                    {
                        int bytes = (int)(fSize - processed);
                        processed = (ulong)bytes + processed;
                        //Console.WriteLine(bytes + "-" + processed);
                        byte[] buf = new byte[bytes];
                        bytes = cl.Receive(buf);
                        f.Write(buf, 0, bytes);
                    }
                    else
                    {
                        int bytes = cl.Receive(buffer);
                        processed = (ulong)bytes + processed;
                        //Console.WriteLine(bytes+"-"+processed);
                        f.Write(buffer, 0, bytes);
                    }
                    //cl.Send(ping); // 4
                    //processed = processed + 1024;
                    Console.CursorLeft = 3; //Рисуем с татус бар
                    if ((processed * 100 / fSize) == 10)
                    { Console.Write("  "); Console.CursorLeft = 40; Console.Write("10%"); }
                    else if ((processed * 100 / fSize) == 20)
                    { Console.Write("    "); Console.CursorLeft = 40; Console.Write("20%"); }
                    else if ((processed * 100 / fSize) == 30)
                    { Console.Write("      "); Console.CursorLeft = 40; Console.Write("30%"); }
                    else if ((processed * 100 / fSize) == 40)
                    { Console.Write("        "); Console.CursorLeft = 40; Console.Write("40%"); }
                    else if ((processed * 100 / fSize) == 50)
                    { Console.Write("          "); Console.CursorLeft = 40; Console.Write("50%"); }
                    else if ((processed * 100 / fSize) == 60)
                    { Console.Write("            "); Console.CursorLeft = 40; Console.Write("60%"); }
                    else if ((processed * 100 / fSize) == 70)
                    { Console.Write("             "); Console.CursorLeft = 40; Console.Write("70%"); }
                    else if ((processed * 100 / fSize) == 80)
                    { Console.Write("               "); Console.CursorLeft = 40; Console.Write("80%"); }
                    else if ((processed * 100 / fSize) == 90)
                    { Console.Write("                  "); Console.CursorLeft = 40; Console.Write("90%"); }
                }
                Console.CursorLeft = 3;
                Console.Write("                    "); Console.CursorLeft = 40; Console.WriteLine("100%");
                Console.BackgroundColor = ConsoleColor.DarkYellow;
                cl.Send(ping); // 5
                Console.WriteLine("Розмір прийнято файла" + processed);
                Console.WriteLine("Файл " + fName + " принят.\n");
                if (f != null)
                { f.Close(); }
            }
            cl.Send(ping);
            s.Close();
            cl.Close();
            SystemSounds.Beep.Play();
            Console.WriteLine("\n--------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Принято " + fNumb[0] + " файлов.");
            Console.WriteLine("Целевая папка: " + path);
        }
        catch (Exception e)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
        }        
    }

    static void ReceiveCallback(IAsyncResult AsyncCall)
    {
        System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
        Byte[] message = encoding.GetBytes("Я занят");

        Socket listener = (Socket)AsyncCall.AsyncState;
        Socket client = listener.EndAccept(AsyncCall);

        Console.WriteLine("Принято соединение от: {0}", client.RemoteEndPoint);
        client.Send(message);

        Console.WriteLine("Закрытие соединения");
        client.Close();

        // После того как завершили соединение, говорим ОС что мы готовы принять новое
        listener.BeginAccept(new AsyncCallback(ReceiveCallback), listener);
    }

    static void Main(string[] args)
    {
        List<string> paths = new List<string>();
        Console.Title = "FileSender  (body90)";
        Console.CursorVisible = false;
        Console.BackgroundColor = ConsoleColor.DarkYellow;
        Console.Clear();
        int status = 0;
        if (args.Length == 0)
        {
            Console.WriteLine("Нажмите:");
            Console.WriteLine("\tПРОБЕЛ - Для приема файла.");
            Console.WriteLine("\tДРУГУЮ КЛАВИШУ - Для передачи.");
            ulong a = 1554545466, b=45455556;
            decimal f = b*100/a;
            Console.WriteLine(f+"-"+a/b);
            Console.WriteLine();
            ConsoleKeyInfo key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.Spacebar:
                    Console.WriteLine("Введите путь к целевому каталогу (оканчивающийся на \"\\\")\n" + "Или введите пустую строку для сохранения в C:\\ :");
                    Console.Write("\t");
                    Console.CursorVisible = true;
                    string path = Console.ReadLine();
                    Console.CursorVisible = false;
                    if (path == "")
                    { path = @"C:\"; }
                    while (status == 0)
                    {
                        Receive(path);
                    }
                    break;
                default:
                    Console.WriteLine("Введите путь к файлу:");
                    Console.Write("\t");
                    Console.CursorVisible = true;
                    paths.Add(Console.ReadLine());
                    Console.WriteLine();
                    Console.CursorVisible = false;
                    Send(paths);
                    break;
            }
        }
        else
        {
            paths.AddRange(args);
            Send(paths);
        }
        Console.ReadKey();
    }
}
