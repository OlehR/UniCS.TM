using Front.Equipments.Utils;
using Microsoft.VisualBasic;
using RJCP.IO.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using System.Windows.Forms;
using Utils;
using UtilNetwork;
//using static System.Windows.Forms.AxHost;

namespace Front.Equipments.Virtual
{
    public class Connector
    {
        protected string SerialPort;
        protected int BaudRate;
        protected string IP;
        protected int IpPort;
        bool IsIp {get { return !string.IsNullOrEmpty(IP) && IpPort > 0; } }
        bool IsOpen = false;
        bool IsRead =false;
        byte[] Res = null;

        private SerialPortStreamWrapper SerialDevice;
        SocketClient ss;
        public bool IsReady { get { return SerialDevice != null; } }

        public Connector( string pSerialPort,int pBaudRate, string pIP, int pIpPort)
        {
            SerialPort = pSerialPort;
            BaudRate = pBaudRate;
            IP = pIP;
            IPAddress ip;
            if (IPAddress.TryParse(IP, out ip))
                IpPort = pIpPort;
            if(IsIp)
                ss= new SocketClient(ip, IpPort);
        }
        public bool Open()
        {
            if(IsOpen) { Close(); }

            if(IsIp)
            {

            }
            else
            {

            }
            return IsOpen;
        }

        public void Init()
        {
            lock (Lock)
            {
                //TextError = string.Empty;
                try
                {
                    //State = eStateEquipment.Init;
                    CloseIfOpen();
                    SerialDevice.Open();
                    SerialDevice.DiscardInBuffer();
                    SerialDevice.DiscardOutBuffer();
                    //State = eStateEquipment.On;
                }
                catch (Exception ex)
                {
                    //TextError = ex.Message;
                    //State = eStateEquipment.Error;
                }
                finally
                {
                    SerialDevice.OnReceivedData = new Func<byte[], bool>(OnDataReceived);
                }
                //StopWeight();
            }
        }

        public void Close() { }

        public string Send(string pCommand) 
        {
            var Data= Encoding.UTF8.GetBytes(pCommand);
            var Res = Send(Data);
            return Encoding.UTF8.GetString( Res);
        }

        public string SendWith0(string pCommand)
        {
            pCommand += ' ';
            var Data = Encoding.UTF8.GetBytes(pCommand);
            Data[Data.Length - 1] = (byte)0;
            var Res = Send(Data);
            return Encoding.UTF8.GetString(Res);
        }

        public byte[] Send(byte[] pCommand)
        {
            if (IsIp) 
            return null;//ss.StartAsync<>(pCommand);
            else
                return GetReadDataSync(pCommand);
        }

        private void CloseIfOpen()
        {
            if (SerialDevice != null)
            {
                SerialDevice.Close();
                SerialDevice.Dispose();
            }
            SerialPortStreamWrapper portStreamWrapper = new SerialPortStreamWrapper(SerialPort, BaudRate, Parity.Even, StopBits.One, 8, new Func<byte[], bool>(OnDataReceived));
            portStreamWrapper.RtsEnable = true;
            SerialDevice = portStreamWrapper;
        }
        
        object Lock = new();
        public byte[] GetReadDataSync(byte[] command)
        {
            lock (Lock)
            {
                if (!IsReady) return null;
                Res = null;
                SerialDevice.Write(command);
                IsRead = true;
                Thread.Sleep(30);
                int i = 1000;
                while (!IsRead && i-->0)  Thread.Sleep(30);               

                return Res;
            }
        }

        private bool OnDataReceived(byte[] data)
        {
            if (IsRead)
            {
                Res = data;
                IsRead = false;
            }
            //string Str = Encoding.ASCII.GetString(data);           
         
            return true;
        }

    }
}
