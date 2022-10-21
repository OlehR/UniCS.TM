using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services;

namespace ServerRRO
{
    [ServiceContract]
    public interface IWebServerRRO
    {
        [OperationContract]
        [WebInvoke(
        Method = "POST",
        UriTemplate = "/OpenWorkDay",
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.Bare)]
        bool OpenWorkDay();

        [OperationContract]
        [WebInvoke(
          Method = "POST",
          UriTemplate = "/PrintZ",
          RequestFormat = WebMessageFormat.Json,
          ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.Bare)]
        LogRRO PrintZ(IdReceipt pIdR);

        [OperationContract]
        [WebInvoke(
          Method = "POST",
          UriTemplate = "/PrintX",
          RequestFormat = WebMessageFormat.Json,
          ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.Bare)]
        LogRRO PrintX(IdReceipt pIdR);

        [OperationContract]
        [WebInvoke(
          Method = "POST",
          UriTemplate = "/PrintReceipt",
          RequestFormat = WebMessageFormat.Json,
          ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.Bare)]
        LogRRO PrintReceipt(PrintReceiptData pData);

        [OperationContract]
        [WebInvoke(
          Method = "POST",
          UriTemplate = "/MoveMoney",
          RequestFormat = WebMessageFormat.Json,
          ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.Bare)]
        LogRRO MoveMoney(PrintReceiptData pData);

        [OperationContract]
        [WebInvoke(
          Method = "POST",
          UriTemplate = "/FN",
          RequestFormat = WebMessageFormat.Json,
          ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.Bare)]
        string GetFN();

        [OperationContract]
        [WebInvoke(
          Method = "POST",
          UriTemplate = "/TestDevice",
          RequestFormat = WebMessageFormat.Json,
          ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.Bare)]
        StatusEquipment TestDevice();


        [OperationContract]
        [WebInvoke(
          Method = "POST",
          UriTemplate = "/GetDeviceInfo",
          RequestFormat = WebMessageFormat.Json,
          ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.Bare)]
        string GetDeviceInfo();
    }
       
}

