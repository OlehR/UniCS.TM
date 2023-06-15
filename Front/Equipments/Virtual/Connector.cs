using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        public Connector( string pSerialPort,int pBaudRate, string pIP, int pIpPort)
        {
            SerialPort = pSerialPort;
            BaudRate = pBaudRate;
            IP = pIP;
            IpPort = pIpPort;
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
        public void Close() { }

        public string Send(string pCommand) 
        {
            return null;
        }


    }
}
