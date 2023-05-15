
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
//using ModernExpo.SelfCheckout.Utils;
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
using Front.Equipments.Utils;
//using SerialPortStreamWrapper = Front.Equipments.Utils.SerialPortStreamWrapper;
//using StaticTimer = Front.Equipments.Utils.StaticTimer;

namespace Front.Equipments
{
    public class Magellan9300S : IDisposable
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
        private eDeviceConnectionStatus _currentStatus = eDeviceConnectionStatus.InitializationError;

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

        public eDeviceConnectionStatus Init()
        {
            lock (this._locker)
            {
                try
                {
                    this._tmpStr = string.Empty;
                    if (this._currentStatus == eDeviceConnectionStatus.Enabled)
                        return eDeviceConnectionStatus.Enabled;
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
                        Thread.Sleep(200);
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
                    this._currentStatus = flag ? eDeviceConnectionStatus.Enabled : eDeviceConnectionStatus.InitializationError;
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
                        return eDeviceConnectionStatus.NotConnected;
                    }
                    Action<DeviceLog> onDeviceWarning1 = this.OnDeviceWarning;
                    if (onDeviceWarning1 != null)
                    {
                        BarcodeScannerLog barcodeScannerLog = new BarcodeScannerLog();
                        barcodeScannerLog.Category = TerminalLogCategory.Critical;
                        barcodeScannerLog.Message = "Initialization error";
                        onDeviceWarning1((DeviceLog)barcodeScannerLog);
                    }
                    return eDeviceConnectionStatus.InitializationError;
                }
                finally
                {
                    this._serialDevice.OnReceivedData = new Func<byte[], bool>(this.OnDataReceived);
                }
            }
        }

        public Task<eDeviceConnectionStatus> GetDeviceStatus() => this.TestDevice();

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

        public Task<eDeviceConnectionStatus> TestDevice()
        {
            lock (this._locker)
            {
                this.CloseIfOpen();
                try
                {
                    this._serialDevice.Open();
                    this._serialDevice.OnReceivedData = (Func<byte[], bool>)null;
                    eDeviceConnectionStatus result = eDeviceConnectionStatus.InitializationError;
                    this.GetReadDataSync("3p=", (Action<string>)(res =>
                    {
                        if (!res.ContainsIgnoreCase("OK"))
                        {
                            result = eDeviceConnectionStatus.InitializationError;
                        }
                        else
                        {
                            this.ForceGoodReadTone();
                            result = eDeviceConnectionStatus.Enabled;
                        }
                    }));
                    StaticTimer.Wait((Func<bool>)(() => result == eDeviceConnectionStatus.InitializationError), 2);
                    if (result == eDeviceConnectionStatus.InitializationError)
                        return Task.FromResult<eDeviceConnectionStatus>(result);
                    result = eDeviceConnectionStatus.InitializationError;
                    this.GetReadDataSync("14", (Action<string>)(res =>
                    {
                        if (!res.StartsWith("S14"))
                        {
                            result = eDeviceConnectionStatus.InitializationError;
                        }
                        else
                        {
                            res = res.Substring(3);
                            if (!res.StartsWith("3") && !res.StartsWith("5"))
                                result = eDeviceConnectionStatus.InitializationError;
                            else
                                result = eDeviceConnectionStatus.Enabled;
                        }
                    }));
                    StaticTimer.Wait((Func<bool>)(() => result == eDeviceConnectionStatus.InitializationError), 2);
                    return Task.FromResult<eDeviceConnectionStatus>(result);
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
                        return Task.FromResult<eDeviceConnectionStatus>(eDeviceConnectionStatus.NotConnected);
                    }
                    Action<DeviceLog> onDeviceWarning1 = this.OnDeviceWarning;
                    if (onDeviceWarning1 != null)
                    {
                        BarcodeScannerLog barcodeScannerLog = new BarcodeScannerLog();
                        barcodeScannerLog.Category = TerminalLogCategory.Critical;
                        barcodeScannerLog.Message = "Initialization error";
                        onDeviceWarning1((DeviceLog)barcodeScannerLog);
                    }
                    return Task.FromResult<eDeviceConnectionStatus>(eDeviceConnectionStatus.InitializationError);
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
                    Thread.Sleep(200);
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
                        Thread.Sleep(200);
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
                   _logger?.LogError(ex, ex.Message);
                    return null;
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
