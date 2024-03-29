﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services;

namespace PrintServer
{

    [ServiceContract]
    public interface IWebPrintServer
    {

        // [OperationContract]
        // [ServiceKnownType(typeof(string))]
        //[WebGet]
        //[WebMethod]
        //[HttpPost]

        [OperationContract]
        [WebInvoke(
        Method = "POST",
        UriTemplate = "/Print",
        RequestFormat = WebMessageFormat.Json,
        ResponseFormat = WebMessageFormat.Json,
         BodyStyle = WebMessageBodyStyle.Bare)]
        string Print(Wares parWares);

        [WebInvoke(
        Method = "GET",
        UriTemplate = "/GetQueue",
        ResponseFormat = WebMessageFormat.Json,
         BodyStyle = WebMessageBodyStyle.Bare)]
        string GetQueue();
        [WebGet]
        [WebMethod]
        string ClearQueue();
    }

    //[DataContract]
    public class Wares
    {
        public string CodeWares { get; set; }
        public string Article { get; set; }
        public string NameDocument { get; set; }
        public int CodeWarehouse { get; set; }
        public DateTime Date { get; set; }
        public string SerialNumber { get; set; }
        public string NameDCT { get; set; }
        public string Login { get; set; }
        public eBrandName BrandName
        {
            get
            {
                if (CodeWarehouse < 30)
                    return eBrandName.Vopak;
                else if (CodeWarehouse == 163 || CodeWarehouse == 170)
                    return eBrandName.Lubo;
                else return eBrandName.Spar;

            }
        }


    }
    public enum eBrandName
    {
        Spar,
        Vopak,
        Lubo
    }
}

