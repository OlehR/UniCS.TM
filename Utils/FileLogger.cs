using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Utils
{
    public static class FileLogger
    {
        private static Dictionary<int, Type> _types = new Dictionary<int, Type>();

        private static readonly object Locker = new object();
        private static readonly object DictionaryLocker = new object();

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

        public static void WriteLogMessage(this string message, string subLog = null)
        {
#if DEBUG
            message.WriteConsoleDebug();
#endif
            Task.Run(() =>
            {
                lock (Locker)
                {
                    var date = DateTime.Now;
                    File.AppendAllText(
                        $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{date.Year}_{date.Month}_{date.Day}.log")}",
                        $@"[{date:dd-MM-yyyy HH:mm:ss}] {message}{Environment.NewLine}");
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