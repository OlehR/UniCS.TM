﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using ModelMID;
using Utils;

namespace ModelMID.DB
{
    public class WorkPlace
    {
        public int IdWorkplace { get; set; }
        public string Name { get; set; }        
        public string VideoCameraIP { get; set; }
        public string VideoRecorderIP { get; set; }
        public eBank TypePOS { get; set; }
        public int CodeWarehouse { get; set; }
        public string StrCodeWarehouse { get { return $"{CodeWarehouse:D9}"; } }
        public int CodeDealer { get; set; }
        public string Prefix { get; set; }
        public bool IsChoice { get; set; }
        string _DNSName;
        public string DNSName
        {
            get { return _DNSName; }
            set
            {
                _DNSName = value;
                IPAddress ip;
                if (IPAddress.TryParse(_DNSName, out ip))
                {
                    IP = ip;
                    return;
                }
                if (!string.IsNullOrEmpty(value)) Task.Run(async () =>
            {
                var el = await Dns.GetHostEntryAsync(DNSName);
                if (el?.AddressList?.Length > 0) IP = el?.AddressList[0];
            });
            }
        }

        public eTypeWorkplace TypeWorkplace { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public IPAddress IP { get; set; }

        string _SettingsEx;
        public string SettingsEx
        {
            get { return _SettingsEx; }
            set
            {
                _SettingsEx = value;
                if (!string.IsNullOrEmpty(value))
                    try { _Settings = Newtonsoft.Json.JsonConvert.DeserializeObject
                            //JsonSerializer.Deserialize
                            <Settings>(value); } 
                    catch (Exception e) 
                    {
                        FileLogger.WriteLogMessage(this, "SettingsEx", e);
                    };
            }
        }
        Settings _Settings;
        [System.Text.Json.Serialization.JsonIgnore]
        public Settings Settings { get { return _Settings; } }
    }

}

