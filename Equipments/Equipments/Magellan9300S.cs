using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RJCP.IO.Ports;
using System.Globalization;
using System.Text;
using System.Timers;
using Front.Equipments.Utils;
using Utils;

namespace Front.Equipments
{
    public class Magellan9300S : IDisposable
    {
        private const string ResetDevice = "00";
        private const string EnableDevice = "01";
        private const string DisableDevice = "02";
        private const string ScannerStatus = "03";
        private readonly byte[] DisableQR = [81, 00, 00, 00, 06, 00, 10, 03, 0xd7, 00, 0x8f];
        private readonly byte[] EnableQR  = [81, 00, 00, 00, 06, 00, 10, 03, 0xd7, 01, 0x8e];
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
        private const double TimerInterval = 250;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Magellan9300S> _logger;
        private string _tmpStr = string.Empty;
        private readonly System.Timers.Timer _timer;
        private readonly object _locker = new object();
        private SerialPortStreamWrapper _serialDevice;
        private int _attempt;
        private double _currentWeight;
        private eDeviceConnectionStatus _currentStatus = eDeviceConnectionStatus.InitializationError;
        private int InitDeley = 500;
       
        private string _port => _configuration["Devices:Magellan9300S:Port"];

        private int _baudRate => int.Parse(_configuration["Devices:Magellan9300S:BaudRate"] ?? "9600");

        private bool IsUsePrefix = true;

        public Action<DeviceLog> OnDeviceWarning { get; set; }

        public Action<double> OnWeightChanged { get; set; }

        public Action<string> OnBarcodeScannerChange { get; set; }

        public bool IsReady
        {
            get
            {
                SerialPortStreamWrapper serialDevice = _serialDevice;
                return serialDevice != null;//&& __nonvirtual(serialDevice.IsOpen);
            }
        }

        public Magellan9300S(
          IConfiguration configuration,
          ILogger<Magellan9300S> logger)
        {
            if (logger != null)
                logger.LogDebug("Magellan9300S CTOR");
            _configuration = configuration;
            _logger = logger;
            _serialDevice = new(_port, _baudRate, Parity.Odd, StopBits.One, 7, new Func<byte[], bool>(OnDataReceived)) { RtsEnable = true }; 
            _timer = new System.Timers.Timer(TimerInterval);
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            _timer.AutoReset = true;

            string v = _configuration["Devices:Magellan9300S:IsUsePrefix"];
            if (!string.IsNullOrEmpty(v)) { IsUsePrefix = bool.Parse(v); }
        }

        public eDeviceConnectionStatus Init()
        {
            lock (_locker)
            {
                try
                {
                    _tmpStr = string.Empty;
                    if (_currentStatus == eDeviceConnectionStatus.Enabled)
                        return eDeviceConnectionStatus.Enabled;
                    Action<DeviceLog> onDeviceWarning1 = OnDeviceWarning;
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "[Magellan9300S] - Start Initialization");
                    
                    CloseIfOpen();
                    _serialDevice.OnReceivedData = (Func<byte[], bool>)null;
                    byte[] buffer = new byte[_serialDevice.ReadBufferSize];
                    _serialDevice.Open();
                    _serialDevice.Write(EnableQR);
                    _serialDevice.Read(buffer, 0, buffer.Length);

                    _serialDevice.Write(GetCommand(EnableDevice));
                    _serialDevice.Write(GetCommand(DisableToneScanner));
                    
                    _serialDevice.Read(buffer, 0, buffer.Length);
                    _serialDevice.DiscardInBuffer();
                    _serialDevice.DiscardOutBuffer();
                    _serialDevice.Write(GetCommand(HealthScanner));
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "[Magellan9300S] - Get info about scanner");
                    
                    for (int index = 0; _serialDevice.ReadBufferSize < 1 && index < 10; ++index)
                        Thread.Sleep(InitDeley);
                    byte[] numArray = new byte[_serialDevice.ReadBufferSize];
                    _serialDevice.Read(numArray, 0, numArray.Length);
                    string str = Encoding.ASCII.GetString(numArray);
                     
                    bool flag = str.ContainsIgnoreCase("OK");
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"[Magellan9300S] - Flag=>{flag}");
         
                    _currentStatus = flag ? eDeviceConnectionStatus.Enabled : eDeviceConnectionStatus.InitializationError;
                    return _currentStatus;
                }
                catch (Exception ex)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                    if (ex.Message.ContainsIgnoreCase("port"))
                    {                        
                        return eDeviceConnectionStatus.NotConnected;
                    }
                    
                    return eDeviceConnectionStatus.InitializationError;
                }
                finally
                {
                    _serialDevice.OnReceivedData = new Func<byte[], bool>(OnDataReceived);
                }
            }
        }

        public void GetReadDataSync(string command, Action<string> onDatAction)
        {
            if (!IsReady || onDatAction == null)
                return;
            _serialDevice.Write(GetCommand(command));
            do
                ;
            while (_serialDevice.ReadBufferSize < 1);
            byte[] numArray = new byte[_serialDevice.ReadBufferSize];
            _serialDevice.Read(numArray, 0, numArray.Length);
            onDatAction(Encoding.ASCII.GetString(numArray));
        }

        public eDeviceConnectionStatus TestDevice()
        {
            lock (_locker)
            {
                CloseIfOpen();
                try
                {
                    _serialDevice.Open();
                    _serialDevice.OnReceivedData = (Func<byte[], bool>)null;
                    eDeviceConnectionStatus result = eDeviceConnectionStatus.InitializationError;
                    GetReadDataSync(HealthScanner, (Action<string>)(res =>
                    {
                        if (!res.ContainsIgnoreCase("OK"))
                        {
                            result = eDeviceConnectionStatus.InitializationError;
                        }
                        else
                        {
                            ForceGoodReadTone();
                            result = eDeviceConnectionStatus.Enabled;
                        }
                    }));
                    StaticTimer.Wait((Func<bool>)(() => result == eDeviceConnectionStatus.InitializationError), 2);
                    if (result == eDeviceConnectionStatus.InitializationError)
                        return result;
                    result = eDeviceConnectionStatus.InitializationError;
                    GetReadDataSync(GetWeightWithStatus, (Action<string>)(res =>
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
                    return result;
                }
                catch (Exception ex)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                    
                    if (ex.Message.ContainsIgnoreCase("port"))
                    {                        
                        return eDeviceConnectionStatus.NotConnected;
                    }
                    
                    return eDeviceConnectionStatus.InitializationError;
                }
                finally
                {
                    _serialDevice.OnReceivedData = new Func<byte[], bool>(OnDataReceived);
                }
            }
        }

        public string GetInfo()
        {
            lock (_locker)
            {
                CloseIfOpen();
                try
                {
                    _serialDevice.Open();
                    _serialDevice.OnReceivedData = (Func<byte[], bool>)null;
                    _serialDevice.Write(GetCommand(IdentificationStatus));
                    Thread.Sleep(InitDeley);
                    int num1 = 0;
                    string result = "";
                    byte[] numArray;
                    for (numArray = new byte[1]; num1 < 10 && numArray[numArray.Length - 1] != (byte)13; ++num1)
                    {
                        int bytesToRead = _serialDevice.BytesToRead;
                        if (bytesToRead > 0)
                        {
                            byte[] buffer = new byte[bytesToRead];
                            _serialDevice.Read(buffer, 0, buffer.Length);
                            if (numArray[0] == (byte)0)
                                numArray = buffer;
                            else
                                Array.Copy((Array)buffer, 0, (Array)numArray, 0, buffer.Length);
                        }
                        Thread.Sleep(InitDeley);
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
                    return result;
                }
                catch (Exception ex)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                    return null;
                }
                finally
                {
                    _serialDevice.OnReceivedData = new Func<byte[], bool>(OnDataReceived);
                }
            }
        }

        public void StartGetWeight()
        {
            if (!_serialDevice.IsOpen)
                _serialDevice.Open();
            _attempt = 0;
            _timer.Start();
        }

        public void StopGetWeight()
        {
            _serialDevice.Write(GetCommand(ScaleCancel));
            _timer.Stop();
        }

        public void ForceGoodReadTone()
        {
            try
            {
                if (!IsReady)
                    return;
                _serialDevice?.Write(GetCommand(BeepScanner));
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                Action<DeviceLog> onDeviceWarning = OnDeviceWarning;
                if (onDeviceWarning == null)
                    return;
                BarcodeScannerLog barcodeScannerLog = new BarcodeScannerLog();
                barcodeScannerLog.Category = TerminalLogCategory.Warning;
                barcodeScannerLog.Message = "Cannot make beep";
                onDeviceWarning((DeviceLog)barcodeScannerLog);
            }
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e) => _serialDevice?.Write(GetCommand(GetWeightWithStatus)); //11

        private void CloseIfOpen()
        {
            if (IsReady)
                _serialDevice.Close();
            _serialDevice.Dispose();
            SerialPortStreamWrapper portStreamWrapper = new SerialPortStreamWrapper(_port, _baudRate, Parity.Odd, StopBits.One, 7, new Func<byte[], bool>(OnDataReceived));
            portStreamWrapper.RtsEnable = true;
            _serialDevice = portStreamWrapper;
        }

        private bool OnDataReceived(byte[] data)
        {
            if (IsSystemResponse(data))
                return false;
            _tmpStr += Encoding.ASCII.GetString(data);
            if (!_tmpStr.Contains("\r"))
                return false;
            _tmpStr = _tmpStr.Replace("\r", string.Empty);
            if (_tmpStr.StartsWith("S08"))
            {
                int ind = _tmpStr.IndexOf("S14");
                if (ind > 0)
                    BarcodeProcessing(_tmpStr.Substring(3, ind - 3));
                else
                    BarcodeProcessing(_tmpStr.Substring(3));
            }
                //BarcodeProcessing(_tmpStr.Substring(3));
            else if (_tmpStr.StartsWith("S11")) //11
                WeightProcessing(_tmpStr.Substring(3));

            else if (_tmpStr.StartsWith("S144")) //14
                WeightProcessing14(_tmpStr.Substring(4));
            else if (_tmpStr.StartsWith("S143") || _tmpStr.StartsWith("S141")) //14
                WeightProcessing14("0");
            _tmpStr = string.Empty;
            return true;
        }

        private void WeightProcessing14(string weightStr)
        {
            double result;
            if (double.TryParse(weightStr, NumberStyles.Any, (IFormatProvider)CultureInfo.InvariantCulture, out result))
            { 
                Action<double> onWeightChanged = OnWeightChanged;
                if (onWeightChanged != null)
                    onWeightChanged(result);
                _attempt = 0;
            }
            
        }

        private void WeightProcessing(string weightStr)
        {
            double result;
            if (!double.TryParse(weightStr, NumberStyles.Any, (IFormatProvider)CultureInfo.InvariantCulture, out result))
                result = -1.0;
            if (result == _currentWeight && _attempt == 3)
            {
                Action<double> onWeightChanged = OnWeightChanged;
                if (onWeightChanged != null)
                    onWeightChanged(result);
                _attempt = 0;
            }
            else if (result == _currentWeight)
                ++_attempt;
            else
                _currentWeight = result;
        }

        private void BarcodeProcessing(string barcode)
        {
            _logger?.LogDebug("[M9300S] Scanned code - " + barcode);
            if (IsUsePrefix)
            {
                PrefixOfCodes prefixByString = PrefixOfCodes.GetPrefixByString(barcode);
                if (prefixByString != null)
                    barcode = barcode.TrimStart(((string)prefixByString).ToCharArray());
            }
            OnBarcodeScannerChange?.Invoke(barcode.Trim());
        }

        private bool IsSystemResponse(byte[] data) => data.Length <= 2;

        private byte[] GetCommand(string command) => Encoding.ASCII.GetBytes("S" + command + "\r");

        public void Dispose()
        {
            OnBarcodeScannerChange = (Action<string>)null;
            OnWeightChanged = (Action<double>)null;
            _serialDevice?.Close();
            _serialDevice?.Dispose();
        }
    }

}
