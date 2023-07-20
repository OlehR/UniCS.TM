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
using Utils;

namespace Front.Equipments
{
    public class ScanerCom : Scaner, IDisposable
    {
        private readonly ILogger<ScaleCom> _logger;
        private readonly System.Timers.Timer Timer;
        private readonly object Lock = new object();
        private SerialPortStreamWrapper SerialDevice;

        public bool IsReady { get { return SerialDevice != null; } }
        public ScanerCom(Equipment pEquipment, IConfiguration pConfiguration, ILoggerFactory pLoggerFactory, Action<string, string> pOnBarCode) : base(pEquipment, pConfiguration, eModelEquipment.VirtualScaner, pLoggerFactory, pOnBarCode)
        {

            Init();
            // Timer = new System.Timers.Timer(500.0);
            // Timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            //Timer.AutoReset = true;
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

            }
        }

        public override StatusEquipment TestDevice() { Init(); return new StatusEquipment() { State = (int)State, TextState = State.ToString(), ModelEquipment = Model, StateEquipment = State }; }

        
        bool IsRead = false;
        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            IsRead = true;
            SerialDevice?.Write(new byte[4] { 0, 0, 0, 3 });
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
            SerialPortStreamWrapper portStreamWrapper = new SerialPortStreamWrapper(SerialPort, BaudRate, Parity.Even, StopBits.One, 8, new Func<byte[], bool>(OnDataReceived));
            portStreamWrapper.RtsEnable = true;
            SerialDevice = portStreamWrapper;
        }

        //int CountZero = 0;
        private bool OnDataReceived(byte[] data)
        {
            string Str = Encoding.ASCII.GetString(data);

            FileLogger.WriteLogMessage("OnDataReceived=>" + Str);
            Str = Str.Replace("\r", "");
            Str = Str.Replace("\n", "");

            OnBarCode?.Invoke(Str, null);

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

        public override void ForceGoodReadTone() { }
        public override void StartMultipleTone() { }
        public override void StopMultipleTone() { }


        public void Dispose()
        {
            OnBarCode = null;
            SerialDevice?.Close();
            SerialDevice?.Dispose();
        }
    }
}
