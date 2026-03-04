using Front.Equipments.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RJCP.IO.Ports;
using System.Text;
using System.Timers;

namespace Front.Equipments
{
    public class ScaleCom : Scale, IDisposable
    {
        private readonly ILogger<ScaleCom> _logger;
        private readonly System.Timers.Timer Timer;
        private readonly object Lock = new object();
        private SerialPortStreamWrapper SerialDevice;
        private eScaleCom ModelScale = eScaleCom.ICS15;
        public bool IsReady { get { return SerialDevice != null; } }

        public ScaleCom(Equipment pEquipment, IConfiguration pConfiguration, ILoggerFactory pLoggerFactory = null, Action<double, bool> pOnScalesData = null) : base(pEquipment, pConfiguration, eModelEquipment.ScaleCom, pLoggerFactory, pOnScalesData)
        {
            ModelScale = Configuration.GetValue<eScaleCom>($"{KeyPrefix}ModelScale", eScaleCom.ICS15);
            Init();
            Timer = new System.Timers.Timer(500.0);
            Timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            Timer.AutoReset = true;
        }

        public override void Init()
        {
            lock (Lock)
            {
                TextError = string.Empty;
                try
                {
                    State = eStateEquipment.Init;
                    CloseIfOpen();
                    SerialDevice.Open();
                    SerialDevice.DiscardInBuffer();
                    SerialDevice.DiscardOutBuffer();
                    State = eStateEquipment.On;
                }
                catch (Exception ex)
                {
                    TextError = ex.Message;
                    State = eStateEquipment.Error;
                    _logger?.LogError(ex, ex.Message);

                }
                finally
                {
                    SerialDevice.OnReceivedData = new Func<byte[], bool>(OnDataReceived);
                }
                StopWeight();
            }
        }

        public override StatusEquipment TestDevice() { Init(); return new StatusEquipment() { State = (int)State, TextState = State.ToString(), ModelEquipment = Model, StateEquipment = State }; }


        public override void StartWeight()
        {
            OnScalesData?.Invoke(0d, true);
            if (!SerialDevice.IsOpen)
                SerialDevice.Open();
            Timer.Start();
        }

        public override void StopWeight()
        {
            Timer?.Stop();
            SerialDevice?.Close();
        }
        bool IsRead = false;
        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            IsRead = true;

            byte[] SendCommand = ModelScale switch
            {
                eScaleCom.ICS15 => [0, 0, 0, 3],
                eScaleCom.CASPDC15 => [0],
                eScaleCom.LongG => [0x53, 0x49, 0x0D, 0x0A],
                _ => [0]
            };

        
            SerialDevice?.Write(SendCommand);
            if (CountZero++ >= 2)
            {
                OnScalesData?.Invoke(0d, true);
                CountZero = 0;
            }
        }
        //GetReadDataSync(new byte[4] {0,0,0,3},OnDataReceived2);

        private void CloseIfOpen()
        {
            if (SerialDevice != null)
            {
                if (IsReady)
                    SerialDevice.Close();
                SerialDevice.Dispose();
            }
            SerialPortStreamWrapper portStreamWrapper = new SerialPortStreamWrapper(SerialPort, BaudRate, ModelScale == eScaleCom.ICS15 ? Parity.Even : Parity.None, StopBits.One, 8, new Func<byte[], bool>(OnDataReceived));
            portStreamWrapper.RtsEnable = true;
            SerialDevice = portStreamWrapper;
        }

        int CountZero = 0;
        private bool OnDataReceived(byte[] data)
        {
            CountZero = 0;
            string  Str = Encoding.ASCII.GetString(data);
            if (ModelScale == eScaleCom.ICS15)
            {
                if (IsRead && Str.Length >= 6)
                {
                    IsRead = false;
                    Str = Str.Substring(0, 6);
                    char[] charArray = Str.ToCharArray();
                    Array.Reverse(charArray);
                    if (double.TryParse(charArray, out double Weight))
                    {
                        //if (Weight == 0d)
                        //{
                        // if (CountZero < 3)
                        //  {
                        //     CountZero++;
                        // return true;
                        // }
                        //}
                        //CountZero = 0;
                        //FileLogger.WriteLogMessage($"OnDataReceived Weight=>{Weight}");
                        OnScalesData?.Invoke(Weight, true);
                    }
                    return true;
                }
            }
            else if (ModelScale == eScaleCom.CASPDC15)
            {

                if (Str.Length >= 22)
                {
                    if (Str.IndexOf("ST") >= 0 && Str.IndexOf(".") > 0)
                    {
                        Str = Str.Substring(10, 8).Replace(".", "");
                        if (int.TryParse(Str, out int Weight))
                            OnScalesData?.Invoke(Weight, true);
                    }
                }
            }
            else if (ModelScale == eScaleCom.LongG)
            {
                if (Str.Length == 16 && Str[0] == ' ' && Str[1] == ' ' && Str[10] == ' ' && Str[13] == ' ' && Str[14] == 0x0D && Str[15] == 0x0A)
                {
                    Str = Str.Substring(2, 8);
                    if (decimal.TryParse(Str, out decimal Weight))
                        OnScalesData?.Invoke((int)(1000 * Weight), true);
                }
            }
            else if (ModelScale == eScaleCom.AXIS)
            {
                if (data[0] == 2 && data[1] == 45 && data[2] == 48)
                {
                    data = data[4..10];
                    Str = Encoding.ASCII.GetString(data);
                    // Str = Str.Substring(4, 10);
                    if (int.TryParse(Str, out int Weight))
                        OnScalesData?.Invoke(Weight, true);
                }
            }
                return true;
        }

        public void GetReadDataSync(byte[] command, Action<byte[]> onDatAction)
        {
            lock (Lock)
            {
                if (!IsReady || onDatAction == null) return;
                SerialDevice.Write(command);
                Thread.Sleep(30);
                do; while (SerialDevice.ReadBufferSize < 1);
                byte[] numArray = new byte[SerialDevice.ReadBufferSize];
                SerialDevice.Read(numArray, 0, numArray.Length);
                onDatAction?.Invoke(numArray);
            }
        }

        public void Dispose()
        {
            OnScalesData = null;
            SerialDevice?.Close();
            SerialDevice?.Dispose();
        }
    }
    public enum eScaleCom
    {
        ICS15,
        CASPDC15,
        LongG,
        AXIS
    }
}
