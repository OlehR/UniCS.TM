using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Utils
{
    public enum eTypeLog
    {
        Full = 0,
        Expanded = 1,
        Error = 2
    }
    public static class FileLogger
    {
        private static string PathLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static int IdWorkplace;
        public static eTypeLog TypeLog = eTypeLog.Full;

        public static string GetFileName { get { return $"{Path.Combine(PathLog, $"Log_{IdWorkplace}_{DateTime.Now:yyyyMMdd}.log")}"; } }
        private static Dictionary<int, Type> _types = new Dictionary<int, Type>();

        private static readonly object Locker = new object();
        private static readonly object DictionaryLocker = new object();
        static FileLogger()
        {
            CreateDirectoryLog();
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
            WriteLogMessage($"{pO?.GetType().FullName}.{pMetodName} {pMessage}", pTypeLog);
        }

        public static void WriteLogMessage(object pO, string pMetodName, Exception pE)
        {
            WriteLogMessage($"{pO?.GetType().FullName}.{pMetodName} {pE.Message}{Environment.NewLine}{pE.StackTrace}", eTypeLog.Error);
        }

        public static void WriteLogMessage( string message, eTypeLog pTypeLog = eTypeLog.Full)
        {
#if DEBUG
            message.WriteConsoleDebug();
#endif
            if (TypeLog > pTypeLog)
                return;

            Task.Run(() =>
            {
                lock (Locker)
                {
                    try
                    {
                        File.AppendAllText(GetFileName,
                        $@"[{DateTime.Now:dd-MM-yyyy HH:mm:ss:ffff}] {Enum.GetName(typeof(eTypeLog), pTypeLog)} {message}{Environment.NewLine}");
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
    }
}