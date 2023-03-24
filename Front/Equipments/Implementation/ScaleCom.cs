﻿using Microsoft.Extensions.Configuration;
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
    public class ScaleCom : Scale, IDisposable
    {
        private readonly ILogger<ScaleCom> _logger;
        private readonly System.Timers.Timer Timer;
        private readonly object Lock = new object();
        private SerialPortStreamWrapper SerialDevice;

        public bool IsReady { get { return SerialDevice != null; } }

        public ScaleCom(Equipment pEquipment, IConfiguration pConfiguration, ILoggerFactory pLoggerFactory = null, Action<double, bool> pOnScalesData = null) : base(pEquipment, pConfiguration, eModelEquipment.ScaleCom, pLoggerFactory, pOnScalesData)
        {
            
            Init();
            Timer = new System.Timers.Timer(200.0);
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
            }
        }

        public override StatusEquipment TestDevice() { Init(); return new StatusEquipment() { State = (int)State, TextState = State.ToString(), ModelEquipment = Model, StateEquipment = State }; }

        public override string GetDeviceInfo()
        {
            return $"pModelEquipment={Model} State={State} Port={SerialPort} BaudRate={BaudRate}{Environment.NewLine}";
        }
        public override void StartWeight()
        {
            if (!SerialDevice.IsOpen)
                SerialDevice.Open();
            Timer.Start();
        }

        public override void StopWeight()
        {
            Timer.Stop();
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e) => SerialDevice?.Write(new byte[4] { 0, 0, 0, 3 });
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

        private bool OnDataReceived(byte[] data)
        {
            string Str = Encoding.ASCII.GetString(data);
            if (Str.Length >= 6)
            {
                Str=Str.Substring(0, 6);
                char[] charArray = Str.ToCharArray();
                Array.Reverse(charArray);
                if (double.TryParse(charArray, out double Weight))
                    OnScalesData?.Invoke(Weight , true);
                return true;
            }
            return true;
        }

        public void GetReadDataSync(byte[] command, Action<byte[]> onDatAction)
        {
            lock (Lock)
            {
                if (!IsReady || onDatAction == null) return;
                SerialDevice.Write(command);
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
}