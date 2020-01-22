using System;
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

        [OperationContract]
        [ServiceKnownType(typeof(string))]
        [WebGet]
        [WebMethod]
        //[HttpPost]
        string Print(string Wares);
    }
}

