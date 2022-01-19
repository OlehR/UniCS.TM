using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Utils
{
    public enum eTypeLog
    {
        Full=0,
        Expanded=1,
        Error=2
    }
    public static class FileLogger
    {
        private static string PathLog= Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Log");
        private static eTypeLog TypeLog = eTypeLog.Full;

        private static Dictionary<int, Type> _types = new Dictionary<int, Type>();

        private static readonly object Locker = new object();
        private static readonly object DictionaryLocker = new object();
        static FileLogger()
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

        public static void WriteLogMessage(this string message, eTypeLog pTypeLog = eTypeLog.Full)
        {
#if DEBUG
            message.WriteConsoleDebug();
            if (TypeLog > pTypeLog)
                return;
#endif
            Task.Run(() =>
            {
                lock (Locker)
                {
                    var date = DateTime.Now;
                    File.AppendAllText(
                        $"{Path.Combine(PathLog, $"{date.Year}{date.Month}{date.Day}.log")}",
                        $@"[{date:dd-MM-yyyy HH:mm:ss}] {Enum.GetName(typeof(eTypeLog), pTypeLog)} {message}{Environment.NewLine}");
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