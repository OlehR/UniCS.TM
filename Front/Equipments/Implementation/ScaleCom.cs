using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RJCP.IO.Ports;
using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Front.Equipments.Utils;
using Front.Equipments.Virtual;

namespace Front.Equipments
{
    public class ScaleCom :Scale, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ScaleCom> _logger;
        private string _tmpStr = string.Empty;
        private readonly System.Timers.Timer _timer;
        private readonly object _locker = new object();
        private SerialPortStreamWrapper _serialDevice;
        

        public bool IsReady { get { return _serialDevice != null; } }

        public ScaleCom(Equipment pEquipment, IConfiguration pConfiguration,ILoggerFactory pLoggerFactory = null, Action<double, bool> pOnScalesData = null) : base(pEquipment, pConfiguration, eModelEquipment.ScaleModern, pLoggerFactory, pOnScalesData)
        {            
            SerialPortStreamWrapper portStreamWrapper = new SerialPortStreamWrapper(SerialPort, BaudRate, Parity.Odd, StopBits.One, 7, new Func<byte[], bool>(OnDataReceived));
            portStreamWrapper.RtsEnable = true;
            _serialDevice = portStreamWrapper;
            _timer = new System.Timers.Timer(200.0);
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            _timer.AutoReset = true;
        }
        
        public override void Init()
        {
            lock (_locker)
            {
                TextError = string.Empty;
                try
                {
                    State = eStateEquipment.Init;
                    _tmpStr = string.Empty;
                    CloseIfOpen();
                    State = eStateEquipment.On;                    
                }
                catch (Exception ex)
                {
                    State = eStateEquipment.Error;
                    TextError = ex.Message;
                    _logger?.LogError(ex, ex.Message);
                    /*if (logger != null)                       
                    if (ex.Message.ContainsIgnoreCase("port"))
                    { 
                            barcodeScannerLog.Message = "Device not connected";                     
                        barcodeScannerLog.Message = "Initialization error";                       
                    }*/

                }
                finally
                {
                    _serialDevice.OnReceivedData = new Func<byte[], bool>(OnDataReceived);
                }
            }
        }

        public override StatusEquipment TestDevice() { Init(); return new StatusEquipment() { State = (int)State, TextState = State.ToString(), ModelEquipment = Model, StateEquipment = State }; }

        public override string GetDeviceInfo()
        {
            return $"pModelEquipment={Model} State={State} Port={SerialPort} BaudRate={BaudRate}{Environment.NewLine}";
        }

        public void StartGetWeight()
        {
            if (!_serialDevice.IsOpen)
                _serialDevice.Open();            
            _timer.Start();
        }

        public void StopGetWeight()
        {
            //_serialDevice.Write(GetCommand("12"));
            _timer.Stop();
        }       

        private void OnTimedEvent(object sender, ElapsedEventArgs e) => _serialDevice?.Write(new byte[4] {0,0,0,4});

        private void CloseIfOpen()
        {
            if (IsReady)
                _serialDevice.Close();
            _serialDevice.Dispose();
            SerialPortStreamWrapper portStreamWrapper = new SerialPortStreamWrapper(SerialPort, BaudRate, Parity.Odd, StopBits.One, 7, new Func<byte[], bool>(OnDataReceived));
            portStreamWrapper.RtsEnable = true;
            _serialDevice = portStreamWrapper;
        }

        private bool OnDataReceived(byte[] data)
        {            
            string Str = Encoding.ASCII.GetString(data);
            if (Str.Length >= 6)
            {
                char[] charArray = Str.ToCharArray();
                Array.Reverse(charArray);
                if (double.TryParse(charArray, out double Weight))
                    OnScalesData.Invoke(Weight, true); return true;
            }
            return true;
        }
    
        public void Dispose()
        {
            OnScalesData = null;
            _serialDevice?.Close();
            _serialDevice?.Dispose();
        }
    }
}
