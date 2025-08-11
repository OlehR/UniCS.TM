using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public enum eTypeLog
    {   
        Full = 0,
        Expanded = 1,
        Error = 2,
        None = 3,
        Memory = 4
    }
    public static class FileLogger
    {
        public static StringBuilder Str = new StringBuilder();
        private static string _PathLog = "Logs";
        public static string PathLog
        {
            set { _PathLog = value; CreateDir(); }
            get { return _PathLog ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Logs"); } //AppDomain.CurrentDomain.BaseDirectory
        }


        private static int IdWorkplace=0;
        public static eTypeLog TypeLog = eTypeLog.None;

        public static string GetFileName { get { return $"{Path.Combine(PathLog, $"Log_{IdWorkplace}_{DateTime.Now:yyyyMMdd}.log")}"; } }
        public static string GetFileNameDate(DateTime pD) { return $"{Path.Combine(PathLog, $"Log_{IdWorkplace}_{pD:yyyyMMdd}.log")}"; } 
        private static Dictionary<int, Type> _types = new Dictionary<int, Type>();

        private static readonly object Locker = new object();
        private static readonly object DictionaryLocker = new object();
        static FileLogger()
        {
            try
            {
                CreateDirectoryLog();
            }
            catch (Exception e)
            {
                
            }
            WriteLogMessage($"{Environment.NewLine}{Environment.NewLine}");
        }

        public static void Init(string pPathLog, int pIdWorkplace)
        {
            if (!string.IsNullOrEmpty(pPathLog))
                PathLog = pPathLog;
            CreateDirectoryLog();
            IdWorkplace = pIdWorkplace;
        }

        public static void CreateDirectoryLog()
        {
            if (!Directory.Exists(PathLog))
                Directory.CreateDirectory(PathLog);
        }
        public static void ExtLogForClass(Type type, int hashCode, string message, string parameters = null)
        {
            if (!string.IsNullOrWhiteSpace(parameters))
                message += $" {parameters}";

            WriteLogMessage($"[{type} - {hashCode}] {message}");
        }

        public static void ExtLogForClassConstruct(Type type, int hashCode, string parameters = null)
        {
            lock (DictionaryLocker)
            {
                if (!_types.ContainsKey(hashCode))
                    _types.Add(hashCode, type);
            }

            var message = "";
            if (!string.IsNullOrWhiteSpace(parameters))
                message += $" {parameters}";

            WriteLogMessage($"[{type} - {hashCode}] constructed {message}");
        }

        public static void ExtLogForClassDestruct(int hashCode, string parameters = null)
        {
            Type type;
            lock (DictionaryLocker)
            {
                if (_types.TryGetValue(hashCode, out type))
                    _types.Remove(hashCode);
            }

            var message = "";
            if (!string.IsNullOrWhiteSpace(parameters))
                message += $" {parameters}";

            WriteLogMessage($"[{type} - {hashCode}] destructed {message}");
        }

        public static void WriteLogMessage(object pO, string pMetodName, string pMessage, eTypeLog pTypeLog = eTypeLog.Full)
        {
            WriteLogMessage($"({pO?.GetHashCode()}){pO?.GetType().FullName}.{pMetodName} {pMessage}", pTypeLog);
        }


        public static void WriteLogMessage(object pO, string pMetodName, Exception pE)
        {
            WriteLogMessage($"({pO?.GetHashCode()}){pO?.GetType().FullName}.{pMetodName} {pE.Message}{Environment.NewLine}{pE.StackTrace}", eTypeLog.Error);
        }

        public static void WriteLogMessage( string pMetodName, Exception pE)
        {
            WriteLogMessage($"{pMetodName} {pE.Message}{Environment.NewLine}{pE.StackTrace}", eTypeLog.Error);
        }

        public static void WriteLogMessage( string message, eTypeLog pTypeLog = eTypeLog.Full)
        {  
#if DEBUG
            message.WriteConsoleDebug();
#endif
            if (TypeLog > pTypeLog && TypeLog!= eTypeLog.Memory)
                return;
            if(message.Length > 10000 )
                message = message.Substring(0, 10000); // Ограничение на размер сообщения
            string str = $@"[{DateTime.Now:dd-MM-yyyy HH:mm:ss:ffff}] {Enum.GetName(typeof(eTypeLog), pTypeLog)} {message}{Environment.NewLine}";
            if (TypeLog == eTypeLog.Memory)
            {
                if (Str.Length > 1000000) // Ограничение на размер памяти
                {
                    Str= Str.Remove(0, Str.Length - 100000);
                }
                Str.AppendLine(str);
                return;
            }

            Task.Run(() =>
            {
                lock (Locker)
                {
                    try
                    {
                        File.AppendAllText(GetFileName, str);                      
                    }
                    catch (Exception e)
                    {
                       var s = e.Message;
                    }                    
                }
            });
        }

        public static void WriteConsoleDebug(this string message)
        {
            Console.WriteLine($@"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] {message}");
            // Console.ReadKey();
        }

        public static void CreateDir()
        {
            if (!Directory.Exists(PathLog))
                Directory.CreateDirectory(PathLog);
        }
    }
}