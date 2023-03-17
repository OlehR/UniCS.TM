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
    public class ScaleCom :Scale, IDisposable
    {
        private readonly ILogger<ScaleCom> _logger;       
        private readonly System.Timers.Timer Timer;
        private readonly object Lock = new object();
        private SerialPortStreamWrapper SerialDevice;        

        public bool IsReady { get { return SerialDevice != null; } }

        public ScaleCom(Equipment pEquipment, IConfiguration pConfiguration,ILoggerFactory pLoggerFactory = null, Action<double, bool> pOnScalesData = null) : base(pEquipment, pConfiguration, eModelEquipment.ScaleCom, pLoggerFactory, pOnScalesData)
        {            
            SerialPortStreamWrapper portStreamWrapper = new SerialPortStreamWrapper(SerialPort, BaudRate, Parity.Odd, StopBits.One, 7, new Func<byte[], bool>(OnDataReceived));
            portStreamWrapper.RtsEnable = true;
            SerialDevice = portStreamWrapper;
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
                    SerialDevice.OnReceivedData = new Func<byte[], bool>(OnDataReceived);
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
            if (!SerialDevice.IsOpen)
                SerialDevice.Open();            
            Timer.Start();
        }

        public void StopGetWeight()
        {
            //_serialDevice.Write(GetCommand("12"));
            Timer.Stop();
        }       

        private void OnTimedEvent(object sender, ElapsedEventArgs e) => SerialDevice?.Write(new byte[4] {0,0,0,4});
        //GetReadDataSync(new byte[4] {0,0,0,4},OnDataReceived2);

        private void CloseIfOpen()
        {
            if (IsReady)
                SerialDevice.Close();
            SerialDevice.Dispose();
            SerialPortStreamWrapper portStreamWrapper = new SerialPortStreamWrapper(SerialPort, BaudRate, Parity.Odd, StopBits.One, 7, new Func<byte[], bool>(OnDataReceived));
            portStreamWrapper.RtsEnable = true;
            SerialDevice = portStreamWrapper;
        }

        private bool OnDataReceived(byte[] data)
        {            
            string Str = Encoding.ASCII.GetString(data);
            if (Str.Length >= 6)
            {
                char[] charArray = Str.ToCharArray();
                Array.Reverse(charArray);
                if (double.TryParse(charArray, out double Weight))
                    OnScalesData.Invoke(Weight/1000d, true); 
                return true;
            }
            return true;
        }

        public void GetReadDataSync(byte[] command, Action<byte[]> onDatAction)
        {
            if (!IsReady || onDatAction == null) return;
            SerialDevice.Write(command);
            do; while (SerialDevice.ReadBufferSize < 1);
            byte[] numArray = new byte[SerialDevice.ReadBufferSize];
            SerialDevice.Read(numArray, 0, numArray.Length);
            onDatAction?.Invoke(numArray);
        }

        private void OnDataReceived2(byte[] data)  => OnDataReceived(data);
        


        public void Dispose()
        {
            OnScalesData = null;
            SerialDevice?.Close();
            SerialDevice?.Dispose();
        }
    }
}
