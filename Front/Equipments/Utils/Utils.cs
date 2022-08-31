
using RJCP.IO.Ports;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;


namespace Front.Equipments.Utils
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


}
