
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModernExpo.SelfCheckout.Utils;
using RJCP.IO.Ports;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ModernExpo.SelfCheckout.Utils
{
    public class SerialPortStreamWrapper : SerialPortStream
    {
        public Func<byte[], bool> OnReceivedData { get; set; }

        public SerialPortStreamWrapper(
          string port,
          int baudRate,
          Parity parity = Parity.None,
          StopBits stopBits = StopBits.Two,
          int data = 8,
          Func<byte[], bool> onReceivedData = null)
          : base(port, baudRate, data, parity, stopBits)
        {
            this.OnReceivedData = onReceivedData;
            this.ReadTimeout = 1000;
            this.WriteTimeout = 1000;
        }

        public void Write(byte[] data) => this.Write(data, 0, data.Length);

        protected override void OnDataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            SerialPortStream serialPortStream = (SerialPortStream)sender;
            if (this.OnReceivedData == null)
            {
                base.OnDataReceived(sender, args);
            }
            else
            {
                int bytesToRead = serialPortStream.BytesToRead;
                byte[] buffer = new byte[bytesToRead];
                serialPortStream.Read(buffer, 0, bytesToRead);
                Func<byte[], bool> onReceivedData = this.OnReceivedData;
                if (onReceivedData != null)
                {
                    int num = onReceivedData(buffer) ? 1 : 0;
                }
                base.OnDataReceived(sender, args);
            }
        }

        public void ClearBuffer()
        {
            try
            {
                this.DiscardInBuffer();
                this.DiscardOutBuffer();
                this.Flush();
            }
            catch (Exception ex)
            {
            }
        }
    }
}


    

namespace Front.Equipments
{

    public static class StaticTimer
    {
        public static bool Wait(Func<bool> predicate, int timeOutInSeconds = 1)
        {
            if (predicate == null)
                return true;
            int num = 0;
            for (bool flag = predicate(); flag && num <= timeOutInSeconds * 1000; flag = predicate())
            {
                Thread.Sleep(1);
                ++num;
            }
            return num <= timeOutInSeconds * 1000;
        }
    }
    public class DelimiterIndex
    {
        public char Delimiter { get; set; }

        public List<int> Indexes { get; set; }
    }
    public static class StringExtensions
    {
        private static readonly char[] WordDelimiters = new char[13]
        {
      ' ',
      '.',
      ',',
      ':',
      ';',
      '\r',
      '!',
      '?',
      ')',
      '}',
      ']',
      '\n',
      '\t'
        };
        private static readonly char[] WordDelimitersWithoutNewLine = new char[12]
        {
      ' ',
      '.',
      ',',
      ':',
      ';',
      '\r',
      '!',
      '?',
      ')',
      '}',
      ']',
      '\t'
        };

        public static string SubString(this string str, int startIndex, int endIndex) => str.Substring(startIndex, str.Length - startIndex);

        public static string SubString(this string str, int startIndex) => str.Substring(startIndex, str.Length - startIndex);

        public static string StringToCharBytes(this string str)
        {
            string str1 = ((IEnumerable<char>)str.ToCharArray()).Aggregate<char, string>("", (Func<string, char, string>)((current, c) => current + string.Format("{0}, ", (object)Convert.ToByte(c))));
            return str1.Substring(0, str1.Length - 2);
        }

        public static string GetUntilOrEmpty(this string text, string stopAt)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                int length = text.IndexOf(stopAt, StringComparison.Ordinal);
                if (length > 0)
                    return text.Substring(0, length);
            }
            return string.Empty;
        }

        public static bool ContainsIgnoreCase(this string str, string containsStr) => str.ToLower().Contains(containsStr.ToLower());

        public static bool EqualsNotFullWord(this string str, string containsStr)
        {
            string lower = str.ToLower();
            string tempStrContains = containsStr.ToLower();
            Func<char, int, int> selector = (Func<char, int, int>)((t1, i) => tempStrContains.Where<char>((Func<char, int, bool>)((t, j) => i == j)).Count<char>((Func<char, bool>)(t => (int)t1 == (int)t)));
            return (Decimal)lower.Select<char, int>(selector).Sum() >= (Decimal)str.Length - Math.Round((Decimal)((double)str.Length * 0.1));
        }

        public static string LimitCharacters(this string text, int length)
        {
            if (text.Length <= length)
                return text;
            int length1 = text.LastIndexOfAny(StringExtensions.WordDelimiters, length - 3);
            return length1 > length / 2 ? text.Substring(0, length1) + "..." : text.Substring(0, length - 3) + "...";
        }

        public static string LimitCharactersForLine(
          this string text,
          int maxLineLength,
          char lineSpliter = '\n',
          int maxLineCount = 2)
        {
            if (text.Length <= maxLineLength)
                return text;
            string str1 = text.Substring(0, maxLineLength);
            int startIndex = 0;
            string str2 = "";
            int num1 = text.Length - startIndex;
            int num2 = 0;
            for (; num1 > 0; num1 = text.Length - startIndex)
            {
                ++num2;
                if (num2 == maxLineCount)
                {
                    str2 += str1;
                    if (str1.Length == maxLineLength)
                    {
                        str2 = str2.Remove(str2.Length - 3, 3) + "...";
                        break;
                    }
                    break;
                }
                int length1 = str1.LastIndexOfAny(StringExtensions.WordDelimiters, str1.Length - 1);
                if (length1 > str1.Length / 2)
                {
                    string str3 = str1.Substring(0, length1);
                    str2 = str2 + str3 + lineSpliter.ToString();
                    startIndex += str3.Length;
                }
                else
                {
                    str2 += str1;
                    if (text.Length - startIndex < 0 || str1.Length > str1.Length / 2)
                        str2 += lineSpliter.ToString();
                    startIndex += str1.Length;
                }
                int length2 = text.Length - startIndex <= maxLineLength ? text.Length - startIndex : maxLineLength;
                if (length2 >= 0)
                    str1 = text.Substring(startIndex, length2);
                else
                    break;
            }
            int length = str2.LastIndexOfAny(new char[1]
            {
        lineSpliter
            }, str2.Length - 1);
            if (length == str2.Length - 1)
                str2 = str2.Substring(0, length);
            return str2;
        }

        public static string LimitCharactersForTwoLines(
          this string text,
          int maxLineLength,
          char lineSpliter = '\n')
        {
            if (text.Length <= maxLineLength)
                return text;
            text = text.Insert(maxLineLength, lineSpliter.ToString());
            if (text.Length - maxLineLength - 1 <= maxLineLength)
                return text;
            text = text.Substring(0, maxLineLength * 2 - 3);
            text += "...";
            return text;
        }

        public static bool EqualsIgnoreCase(this string str, string containsStr) => str.ToLowerInvariant().Equals(containsStr.ToLowerInvariant());

        public static string DicitionaryToString<TK, TV>(
          this Dictionary<TK, TV> dictionary,
          string keySeparator,
          string pairSeparator = "\n")
        {
            if (dictionary == null || dictionary.Count == 0)
                return "";
            StringBuilder stringBuilder = new StringBuilder();
            foreach (KeyValuePair<TK, TV> keyValuePair in dictionary)
            {
                stringBuilder.Append((object)keyValuePair.Key);
                stringBuilder.Append(keySeparator);
                stringBuilder.Append((object)keyValuePair.Value);
                stringBuilder.Append(pairSeparator);
            }
            return stringBuilder.ToString(0, stringBuilder.Length - 1);
        }

        public static string WordToFirstCapitalSymbol(this string str) => str[0].ToString().ToUpperInvariant() + str.Substring(1);

        public static string WordToFirstLowerSymbol(this string str) => str[0].ToString().ToLowerInvariant() + str.Substring(1);

        public static string SplitWholeWordByCapitalChars(this string str, bool shouldUseFirstCapital = false)
        {
            string str1 = string.Empty;
            List<int> intList = new List<int>();
            for (int index = 0; index < str.Length; ++index)
            {
                if (char.IsUpper(str[index]))
                    intList.Add(index);
            }
            int startIndex = 0;
            for (int index = 1; index <= intList.Count; ++index)
            {
                int length = index == intList.Count ? str.Length - startIndex : intList[index] - startIndex;
                string str2 = str.Substring(startIndex, length);
                startIndex = index == intList.Count ? 0 : intList[index];
                if (shouldUseFirstCapital)
                    str2 = str2.WordToFirstCapitalSymbol();
                str1 = str1 + str2 + " ";
            }
            return str1.TrimEnd(' ');
        }

        public static string WordToWholeByCapitalChars(this string str, bool shouldUseFirstCapital = false)
        {
            string str1 = ((IEnumerable<string>)str.Split(' ')).Aggregate<string, string>(string.Empty, (Func<string, string, string>)((s, s1) => s + s1));
            if (shouldUseFirstCapital)
                str1.WordToFirstCapitalSymbol();
            return str1;
        }

        public static List<string> SplitStringList(
          this IEnumerable<string> list,
          int maxLengthOfLine)
        {
            List<string> stringList = new List<string>();
            foreach (string str1 in list)
            {
                if (str1.Length <= maxLengthOfLine)
                {
                    stringList.Add(str1);
                }
                else
                {
                    string[] strArray = str1.Split(StringExtensions.WordDelimiters);
                    List<char> list1 = ((IEnumerable<char>)str1.ToCharArray()).ToList<char>();
                    List<DelimiterIndex> source = new List<DelimiterIndex>();
                    foreach (char wordDelimiter1 in StringExtensions.WordDelimiters)
                    {
                        char wordDelimiter = wordDelimiter1;
                        for (int index1 = list1.FindIndex((Predicate<char>)(c => (int)c == (int)wordDelimiter)); index1 != -1; index1 = list1.FindIndex((Predicate<char>)(c => (int)c == (int)wordDelimiter)))
                        {
                            list1[index1] = ' ';
                            int index2 = source.FindIndex((Predicate<DelimiterIndex>)(x => (int)x.Delimiter == (int)wordDelimiter));
                            if (index2 == -1)
                                source.Add(new DelimiterIndex()
                                {
                                    Delimiter = wordDelimiter,
                                    Indexes = new List<int>() { index1 }
                                });
                            else
                                source[index2].Indexes.Add(index1);
                        }
                    }
                    int num = 0;
                    string str2 = "";
                    source.Select<DelimiterIndex, List<int>>((Func<DelimiterIndex, List<int>>)(x => x.Indexes)).Count<List<int>>((Func<List<int>, bool>)(x => x.Count<int>((Func<int, bool>)(x1 => x1 < maxLengthOfLine)) > 0));
                    foreach (string str3 in strArray)
                    {
                        num += str3.Length;
                        if (num > maxLengthOfLine)
                            num -= str3.Length;
                        else
                            str2 += str3;
                    }
                }
            }
            return stringList;
        }

        public static IEnumerable<string> SplitStringToList(
          this string text,
          int maxLengthOfLine)
        {
            if (text.Length <= maxLengthOfLine)
                return (IEnumerable<string>)new string[1]
                {
          text
                };
            List<char> list = ((IEnumerable<char>)text.ToCharArray()).ToList<char>();
            string str = "";
            List<DelimiterIndex> delimiterIndexList = new List<DelimiterIndex>();
            int num1 = 0;
            foreach (char ch in StringExtensions.WordDelimitersWithoutNewLine)
            {
                char wordDelimiter = ch;
                for (int index1 = list.FindIndex((Predicate<char>)(c => (int)c == (int)wordDelimiter)); index1 != -1; index1 = list.FindIndex((Predicate<char>)(c => (int)c == (int)wordDelimiter)))
                {
                    list[index1] = '~';
                    int num2 = index1 + 1;
                    num1 += num2;
                    bool flag = false;
                    if (num1 >= maxLengthOfLine)
                    {
                        int count = list.Count;
                        list.RemoveRange(num1 - num2, list.Count);
                        str = str + new string(list.ToArray()) + "\n";
                        flag = true;
                    }
                    int index2 = delimiterIndexList.FindIndex((Predicate<DelimiterIndex>)(x => (int)x.Delimiter == (int)wordDelimiter));
                    if (index2 == -1)
                        delimiterIndexList.Add(new DelimiterIndex()
                        {
                            Delimiter = wordDelimiter,
                            Indexes = new List<int>()
              {
                flag ? index1 + 1 : index1
              }
                        });
                    else
                        delimiterIndexList[index2].Indexes.Add(flag ? index1 + 1 : index1);
                }
            }
            return (IEnumerable<string>)str.Split('\n');
        }

        public static string RemoveAllWhiteSpaces(this string str) => string.Concat<char>(str.Where<char>((Func<char, bool>)(c => !char.IsWhiteSpace(c))));
    }

    public sealed class PrefixOfCodes
    {
        private readonly string _name;
        private readonly int _value;
        private static readonly Dictionary<int, PrefixOfCodes> Instance = new Dictionary<int, PrefixOfCodes>();
        public static readonly PrefixOfCodes Code39 = new PrefixOfCodes(0, "*");
        public static readonly PrefixOfCodes ITF = new PrefixOfCodes(1, "I");
        public static readonly PrefixOfCodes ChinisePostCode = new PrefixOfCodes(2, "H");
        public static readonly PrefixOfCodes UPCA = new PrefixOfCodes(3, "A");
        public static readonly PrefixOfCodes UPCE = new PrefixOfCodes(4, "E");
        public static readonly PrefixOfCodes EAN13 = new PrefixOfCodes(5, "F");
        public static readonly PrefixOfCodes EAN8 = new PrefixOfCodes(6, "FF");
        public static readonly PrefixOfCodes CodeBar = new PrefixOfCodes(7, "%");
        public static readonly PrefixOfCodes Code128 = new PrefixOfCodes(8, "#");
        public static readonly PrefixOfCodes Code93 = new PrefixOfCodes(9, "&");
        public static readonly PrefixOfCodes MSI = new PrefixOfCodes(10, "@");
        public static readonly PrefixOfCodes GS1Omni = new PrefixOfCodes(11, "R4");
        public static readonly PrefixOfCodes GS1Limited = new PrefixOfCodes(12, "RL");
        public static readonly PrefixOfCodes GS1Expanded = new PrefixOfCodes(13, "R4");
        public static readonly PrefixOfCodes Industrial = new PrefixOfCodes(14, "D");
        public static readonly PrefixOfCodes Code11 = new PrefixOfCodes(15, "O");
        public static readonly PrefixOfCodes Standart = new PrefixOfCodes(16, "s");
        public static readonly PrefixOfCodes Matrix = new PrefixOfCodes(17, "G");
        public static readonly PrefixOfCodes QR = new PrefixOfCodes(18, nameof(QR));
        public static readonly PrefixOfCodes Code32 = new PrefixOfCodes(19, "p");
        public static readonly PrefixOfCodes Interleaved = new PrefixOfCodes(20, "i");
        public static readonly PrefixOfCodes PDF417 = new PrefixOfCodes(21, "P");
        public static readonly PrefixOfCodes Default = new PrefixOfCodes(22, "B3");
        public static readonly PrefixOfCodes SapB1 = new PrefixOfCodes(23, "B1");

        private PrefixOfCodes(int value, string name)
        {
            this._value = value;
            this._name = name;
            PrefixOfCodes.Instance.Add(value, this);
        }

        public static PrefixOfCodes GetPrefixByString(string str)
        {
            foreach (KeyValuePair<int, PrefixOfCodes> keyValuePair in PrefixOfCodes.Instance)
            {
                if (str.StartsWith((string)keyValuePair.Value))
                    return keyValuePair.Value;
            }
            return (PrefixOfCodes)null;
        }

        public static explicit operator string(PrefixOfCodes str) => str._name;
    }

    public enum DeviceConnectionStatus
    {
        NotConnected = 1,
        InitializationError = 2,
        Enabled = 3,
        Disabled = 4,
    }

    public enum TerminalLogCategory
    {
        All = 0,
        TerminalAuthorized = 10, // 0x0000000A
        TerminalNotAuthorized = 11, // 0x0000000B
        Warning = 12, // 0x0000000C
        Critical = 13, // 0x0000000D
        ProductAcceptIncorectWeight = 15, // 0x0000000F
        UiException = 16, // 0x00000010
        OK = 17, // 0x00000011
        Error = 18, // 0x00000012
        ProductWeightAcceptedByTag = 19, // 0x00000013
    }

    public enum DeviceType
    {
        None = 0,
        ControlScales = 10, // 0x0000000A
        UserScales = 11, // 0x0000000B
        BuildInBarcodeScanner = 20, // 0x00000014
        HandBarcodeScanner = 21, // 0x00000015
        CashRecyclers = 30, // 0x0000001E
        Flag = 40, // 0x00000028
        PosTerminal = 50, // 0x00000032
        ReceiptPrinter = 60, // 0x0000003C
        FiscalPrinter = 70, // 0x00000046
        MagneticCardReader = 80, // 0x00000050
    }

    public class DeviceLog
    {
        public TerminalLogCategory Category { get; set; }

        public DeviceType DeviceType { get; set; }

        public string Message { get; set; }

        public System.DateTime? DateTime { get; set; }
    }
    public interface IBaseDevice : IDisposable
    {
        Action<DeviceLog> OnDeviceWarning { get; set; }

        bool IsReady { get; }

        DeviceConnectionStatus Init();

        Task<DeviceConnectionStatus> GetDeviceStatus();

        Task<DeviceConnectionStatus> TestDevice();

        Task<string> GetInfo();
    }

    public interface IBarcodeScanner : IBaseDevice, IDisposable
    {
        Action<string> OnBarcodeScannerChange { get; set; }

        void ForceGoodReadTone();
    }

    public interface IUserScales : IBaseDevice, IDisposable
    {
        void StartGetWeight();

        void StopGetWeight();

        Action<double> OnWeightChanged { get; set; }
    }


    public class BarcodeScannerLog : DeviceLog
    {
        public BarcodeScannerLog() => this.DeviceType = DeviceType.BuildInBarcodeScanner;
    }


    public class Magellan9300S : IBarcodeScanner, IBaseDevice, IDisposable, IUserScales
    {
        private const string ResetDevice = "00";
        private const string EnableDevice = "01";
        private const string DisableDevice = "02";
        private const string ScannerStatus = "03";
        private const string SoftResetScanner = "320";
        private const string EnableToneScanner = "32F";
        private const string DisableToneScanner = "339";
        private const string IdentificationStatus = "3p<";
        private const string BeepScanner = "334";
        private const string HealthScanner = "3p=";
        private const string GetRealWeight = "11";
        private const string ScaleCancel = "12";
        private const string ScaleStatus = "13";
        private const string GetWeightWithStatus = "14";
        private const string ScaleDisplayStatus = "23";
        private const int TimerInterval = 250;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Magellan9300S> _logger;
        private string _tmpStr = string.Empty;
        private readonly System.Timers.Timer _timer;
        private readonly object _locker = new object();
        private SerialPortStreamWrapper _serialDevice;
        private int _attempt;
        private double _currentWeight;
        private DeviceConnectionStatus _currentStatus = DeviceConnectionStatus.InitializationError;

        private string _port => this._configuration["Devices:Magellan9300S:Port"];

        private int _baudRate => int.Parse(this._configuration["Devices:Magellan9300S:BaudRate"] ?? "9600");

        public Action<DeviceLog> OnDeviceWarning { get; set; }

        public Action<double> OnWeightChanged { get; set; }

        public Action<string> OnBarcodeScannerChange { get; set; }

        public bool IsReady
        {
            get
            {
                SerialPortStreamWrapper serialDevice = this._serialDevice;
                return serialDevice != null;//&& __nonvirtual(serialDevice.IsOpen);
            }
        }

        public Magellan9300S(
          IConfiguration configuration,
          ILogger<Magellan9300S> logger)
        {
            if (logger != null)
                logger.LogDebug("Magellan9300S CTOR");
            this._configuration = configuration;
            this._logger = logger;
            SerialPortStreamWrapper portStreamWrapper = new SerialPortStreamWrapper(this._port, this._baudRate, Parity.Odd, StopBits.One, 7, new Func<byte[], bool>(this.OnDataReceived));
            portStreamWrapper.RtsEnable = true;
            this._serialDevice = portStreamWrapper;
            this._timer = new System.Timers.Timer(250.0);
            this._timer.Elapsed += new ElapsedEventHandler(this.OnTimedEvent);
            this._timer.AutoReset = true;
        }

        public DeviceConnectionStatus Init()
        {
            lock (this._locker)
            {
                try
                {
                    this._tmpStr = string.Empty;
                    if (this._currentStatus == DeviceConnectionStatus.Enabled)
                        return DeviceConnectionStatus.Enabled;
                    Action<DeviceLog> onDeviceWarning1 = this.OnDeviceWarning;
                    if (onDeviceWarning1 != null)
                    {
                        BarcodeScannerLog barcodeScannerLog = new BarcodeScannerLog();
                        barcodeScannerLog.Category = TerminalLogCategory.All;
                        barcodeScannerLog.Message = "[Magellan9300S] - Start Initialization";
                        onDeviceWarning1((DeviceLog)barcodeScannerLog);
                    }
                    this.CloseIfOpen();
                    this._serialDevice.OnReceivedData = (Func<byte[], bool>)null;
                    this._serialDevice.Open();
                    this._serialDevice.Write(this.GetCommand("01"));
                    this._serialDevice.Write(this.GetCommand("339"));
                    byte[] buffer = new byte[this._serialDevice.ReadBufferSize];
                    this._serialDevice.Read(buffer, 0, buffer.Length);
                    this._serialDevice.DiscardInBuffer();
                    this._serialDevice.DiscardOutBuffer();
                    this._serialDevice.Write(this.GetCommand("3p="));
                    Action<DeviceLog> onDeviceWarning2 = this.OnDeviceWarning;
                    if (onDeviceWarning2 != null)
                    {
                        BarcodeScannerLog barcodeScannerLog = new BarcodeScannerLog();
                        barcodeScannerLog.Category = TerminalLogCategory.All;
                        barcodeScannerLog.Message = "[Magellan9300S] - Get info about scanner";
                        onDeviceWarning2((DeviceLog)barcodeScannerLog);
                    }
                    for (int index = 0; this._serialDevice.ReadBufferSize < 1 && index < 10; ++index)
                        Thread.Sleep(100);
                    byte[] numArray = new byte[this._serialDevice.ReadBufferSize];
                    this._serialDevice.Read(numArray, 0, numArray.Length);
                    string str = Encoding.ASCII.GetString(numArray);
                    if(_logger!=null)
                        _logger.LogDebug("[Magellan9300S] - Initialization message - " + str);
                    bool flag = str.ContainsIgnoreCase("OK");
                    Action<DeviceLog> onDeviceWarning3 = this.OnDeviceWarning;
                    if (onDeviceWarning3 != null)
                    {
                        BarcodeScannerLog barcodeScannerLog = new BarcodeScannerLog();
                        barcodeScannerLog.Category = TerminalLogCategory.All;
                        barcodeScannerLog.Message = string.Format("[Magellan9300S] - Initialization result {0}", (object)flag);
                        onDeviceWarning3((DeviceLog)barcodeScannerLog);
                    }
                    this._currentStatus = flag ? DeviceConnectionStatus.Enabled : DeviceConnectionStatus.InitializationError;
                    return this._currentStatus;
                }
                catch (Exception ex)
                {
                    ILogger<Magellan9300S> logger = this._logger;
                    if (logger != null)
                        logger.LogError(ex, ex.Message);
                    if (ex.Message.ContainsIgnoreCase("port"))
                    {
                        Action<DeviceLog> onDeviceWarning = this.OnDeviceWarning;
                        if (onDeviceWarning != null)
                        {
                            BarcodeScannerLog barcodeScannerLog = new BarcodeScannerLog();
                            barcodeScannerLog.Category = TerminalLogCategory.Critical;
                            barcodeScannerLog.Message = "Device not connected";
                            onDeviceWarning((DeviceLog)barcodeScannerLog);
                        }
                        return DeviceConnectionStatus.NotConnected;
                    }
                    Action<DeviceLog> onDeviceWarning1 = this.OnDeviceWarning;
                    if (onDeviceWarning1 != null)
                    {
                        BarcodeScannerLog barcodeScannerLog = new BarcodeScannerLog();
                        barcodeScannerLog.Category = TerminalLogCategory.Critical;
                        barcodeScannerLog.Message = "Initialization error";
                        onDeviceWarning1((DeviceLog)barcodeScannerLog);
                    }
                    return DeviceConnectionStatus.InitializationError;
                }
                finally
                {
                    this._serialDevice.OnReceivedData = new Func<byte[], bool>(this.OnDataReceived);
                }
            }
        }

        public Task<DeviceConnectionStatus> GetDeviceStatus() => this.TestDevice();

        public void GetReadDataSync(string command, Action<string> onDatAction)
        {
            if (!this.IsReady || onDatAction == null)
                return;
            this._serialDevice.Write(this.GetCommand(command));
            do
                ;
            while (this._serialDevice.ReadBufferSize < 1);
            byte[] numArray = new byte[this._serialDevice.ReadBufferSize];
            this._serialDevice.Read(numArray, 0, numArray.Length);
            onDatAction(Encoding.ASCII.GetString(numArray));
        }

        public Task<DeviceConnectionStatus> TestDevice()
        {
            lock (this._locker)
            {
                this.CloseIfOpen();
                try
                {
                    this._serialDevice.Open();
                    this._serialDevice.OnReceivedData = (Func<byte[], bool>)null;
                    DeviceConnectionStatus result = DeviceConnectionStatus.InitializationError;
                    this.GetReadDataSync("3p=", (Action<string>)(res =>
                    {
                        if (!res.ContainsIgnoreCase("OK"))
                        {
                            result = DeviceConnectionStatus.InitializationError;
                        }
                        else
                        {
                            this.ForceGoodReadTone();
                            result = DeviceConnectionStatus.Enabled;
                        }
                    }));
                    StaticTimer.Wait((Func<bool>)(() => result == DeviceConnectionStatus.InitializationError), 2);
                    if (result == DeviceConnectionStatus.InitializationError)
                        return Task.FromResult<DeviceConnectionStatus>(result);
                    result = DeviceConnectionStatus.InitializationError;
                    this.GetReadDataSync("14", (Action<string>)(res =>
                    {
                        if (!res.StartsWith("S14"))
                        {
                            result = DeviceConnectionStatus.InitializationError;
                        }
                        else
                        {
                            res = res.Substring(3);
                            if (!res.StartsWith("3") && !res.StartsWith("5"))
                                result = DeviceConnectionStatus.InitializationError;
                            else
                                result = DeviceConnectionStatus.Enabled;
                        }
                    }));
                    StaticTimer.Wait((Func<bool>)(() => result == DeviceConnectionStatus.InitializationError), 2);
                    return Task.FromResult<DeviceConnectionStatus>(result);
                }
                catch (Exception ex)
                {
                    ILogger<Magellan9300S> logger = this._logger;
                    if (logger != null)
                        logger.LogError(ex, ex.Message);
                    if (ex.Message.ContainsIgnoreCase("port"))
                    {
                        Action<DeviceLog> onDeviceWarning = this.OnDeviceWarning;
                        if (onDeviceWarning != null)
                        {
                            BarcodeScannerLog barcodeScannerLog = new BarcodeScannerLog();
                            barcodeScannerLog.Category = TerminalLogCategory.Critical;
                            barcodeScannerLog.Message = "Device not connected";
                            onDeviceWarning((DeviceLog)barcodeScannerLog);
                        }
                        return Task.FromResult<DeviceConnectionStatus>(DeviceConnectionStatus.NotConnected);
                    }
                    Action<DeviceLog> onDeviceWarning1 = this.OnDeviceWarning;
                    if (onDeviceWarning1 != null)
                    {
                        BarcodeScannerLog barcodeScannerLog = new BarcodeScannerLog();
                        barcodeScannerLog.Category = TerminalLogCategory.Critical;
                        barcodeScannerLog.Message = "Initialization error";
                        onDeviceWarning1((DeviceLog)barcodeScannerLog);
                    }
                    return Task.FromResult<DeviceConnectionStatus>(DeviceConnectionStatus.InitializationError);
                }
                finally
                {
                    this._serialDevice.OnReceivedData = new Func<byte[], bool>(this.OnDataReceived);
                }
            }
        }

        public Task<string> GetInfo()
        {
            lock (this._locker)
            {
                this.CloseIfOpen();
                try
                {
                    this._serialDevice.Open();
                    this._serialDevice.OnReceivedData = (Func<byte[], bool>)null;
                    this._serialDevice.Write(this.GetCommand("3p<"));
                    Thread.Sleep(100);
                    int num1 = 0;
                    string result = "";
                    byte[] numArray;
                    for (numArray = new byte[1]; num1 < 10 && numArray[numArray.Length - 1] != (byte)13; ++num1)
                    {
                        int bytesToRead = this._serialDevice.BytesToRead;
                        if (bytesToRead > 0)
                        {
                            byte[] buffer = new byte[bytesToRead];
                            this._serialDevice.Read(buffer, 0, buffer.Length);
                            if (numArray[0] == (byte)0)
                                numArray = buffer;
                            else
                                Array.Copy((Array)buffer, 0, (Array)numArray, 0, buffer.Length);
                        }
                        Thread.Sleep(100);
                    }
                    List<byte> byteList = new List<byte>();
                    for (int index = 3; index < numArray.Length - 1; ++index)
                    {
                        byte num2 = numArray[index];
                        if (num2 != (byte)1)
                            byteList.Add(num2);
                    }
                    string str1 = Encoding.ASCII.GetString(byteList.ToArray());
                    char[] chArray = new char[1] { Convert.ToChar(4) };
                    foreach (string str2 in str1.Split(chArray))
                    {
                        if (!string.IsNullOrEmpty(str2))
                        {
                            switch (str2[0])
                            {
                                case 'A':
                                    result = result + "Primary Scanner application ROM ID: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'B':
                                    result = result + "Primary Scanner bootloader ROM ID: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'C':
                                    result = result + "Primary Scanner configuration file ID: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'D':
                                    result = result + "Remote display version: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'E':
                                    result = result + "Smart EAS version: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'H':
                                    result = result + "Primary Scanner hardware ID: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'I':
                                    result = result + "Primary Scanner interface: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'L':
                                    result = result + "Secondary Scanner application in system: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'M':
                                    result = result + "Primary Scanner top model number: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'R':
                                    result = result + "Primary Scanner application revision level: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'S':
                                    result = result + "Primary Scanner serial number: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'U':
                                    result = result + "Universal interface application ROM ID: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'W':
                                    result = result + "Internal scale information: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'X':
                                    result = result + "Secondary handheld scanner model name/number: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'b':
                                    result = result + "Secondary scanner bootloader in system: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'm':
                                    result = result + "Primary Scanner main board serial number: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'r':
                                    result = result + "RF Scanner radio version: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                case 'u':
                                    result = result + "Universal interface bootloader ROM ID: " + str2.Remove(0, 1) + "\n";
                                    continue;
                                default:
                                    continue;
                            }
                        }
                    }
                    return Task.FromResult<string>(result);
                }
                catch (Exception ex)
                {
                    ILogger<Magellan9300S> logger = this._logger;
                    if (logger != null)
                        logger.LogError(ex, ex.Message);
                    return (Task<string>)null;
                }
                finally
                {
                    this._serialDevice.OnReceivedData = new Func<byte[], bool>(this.OnDataReceived);
                }
            }
        }

        public void StartGetWeight()
        {
            if (!this._serialDevice.IsOpen)
                this._serialDevice.Open();
            this._attempt = 0;
            this._timer.Start();
        }

        public void StopGetWeight()
        {
            this._serialDevice.Write(this.GetCommand("12"));
            this._timer.Stop();
        }

        public void ForceGoodReadTone()
        {
            try
            {
                if (!this.IsReady)
                    return;
                this._serialDevice?.Write(this.GetCommand("334"));
            }
            catch (Exception ex)
            {
                ILogger<Magellan9300S> logger = this._logger;
                if (logger != null)
                    logger.LogError(ex, ex.Message);
                Action<DeviceLog> onDeviceWarning = this.OnDeviceWarning;
                if (onDeviceWarning == null)
                    return;
                BarcodeScannerLog barcodeScannerLog = new BarcodeScannerLog();
                barcodeScannerLog.Category = TerminalLogCategory.Warning;
                barcodeScannerLog.Message = "Cannot make beep";
                onDeviceWarning((DeviceLog)barcodeScannerLog);
            }
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e) => this._serialDevice?.Write(this.GetCommand("11"));

        private void CloseIfOpen()
        {
            if (this.IsReady)
                this._serialDevice.Close();
            this._serialDevice.Dispose();
            SerialPortStreamWrapper portStreamWrapper = new SerialPortStreamWrapper(this._port, this._baudRate, Parity.Odd, StopBits.One, 7, new Func<byte[], bool>(this.OnDataReceived));
            portStreamWrapper.RtsEnable = true;
            this._serialDevice = portStreamWrapper;
        }

        private bool OnDataReceived(byte[] data)
        {
            if (this.IsSystemResponse(data))
                return false;
            this._tmpStr += Encoding.ASCII.GetString(data);
            if (!this._tmpStr.Contains("\r"))
                return false;
            this._tmpStr = this._tmpStr.Replace("\r", string.Empty);
            if (this._tmpStr.StartsWith("S08"))
                this.BarcodeProcessing(this._tmpStr.Substring(3));
            else if (this._tmpStr.StartsWith("S11"))
                this.WeightProcessing(this._tmpStr.Substring(3));
            this._tmpStr = string.Empty;
            return true;
        }

        private void WeightProcessing(string weightStr)
        {
            double result;
            if (!double.TryParse(weightStr, NumberStyles.Any, (IFormatProvider)CultureInfo.InvariantCulture, out result))
                result = -1.0;
            if (result == this._currentWeight && this._attempt == 3)
            {
                Action<double> onWeightChanged = this.OnWeightChanged;
                if (onWeightChanged != null)
                    onWeightChanged(result);
                this._attempt = 0;
            }
            else if (result == this._currentWeight)
                ++this._attempt;
            else
                this._currentWeight = result;
        }

        private void BarcodeProcessing(string barcode)
        {
            ILogger<Magellan9300S> logger = this._logger;
            if (logger != null)
                logger.LogDebug("[M9300S] Scanned code - " + barcode);
            PrefixOfCodes prefixByString = PrefixOfCodes.GetPrefixByString(barcode);
            if (prefixByString != null)
                barcode = barcode.TrimStart(((string)prefixByString).ToCharArray());
            Action<string> barcodeScannerChange = this.OnBarcodeScannerChange;
            if (barcodeScannerChange == null)
                return;
            barcodeScannerChange(barcode.Trim());
        }

        private bool IsSystemResponse(byte[] data) => data.Length <= 2;

        private byte[] GetCommand(string command) => Encoding.ASCII.GetBytes("S" + command + "\r");

        public void Dispose()
        {
            this.OnBarcodeScannerChange = (Action<string>)null;
            this.OnWeightChanged = (Action<double>)null;
            this._serialDevice?.Close();
            this._serialDevice?.Dispose();
        }
    }

}
