using System;
using System.Collections.Generic;
using System.Net;
using System.Text;


namespace UtilNetwork
{
    public class Result
    {
        public int State { get; set; } = 0;
        public bool Success { get { return State == 0; } }
        [Obsolete("Proporty is deprecated, please use Success")]
        public bool status { get { return State == 0; } }
        public string TextError { get; set; }
        [Obsolete("Proporty is deprecated, please use TextError")]
        public string TextState { get { return TextError; } set { TextError = value; } }
 //      [Obsolete("Proporty is deprecated, please use Data")]        
 //       public string Info { get { return Data; } set { Data = value; } }
        //public eStateHTTP StateHTTP { get; set; }
//#if WEB
 //       [System.Text.Json.Serialization.JsonIgnore]
 //       [Newtonsoft.Json.JsonIgnore]
//#endif
        public string Data { get; set; }
        public eStateHTTP StateHTTP { get; set; } = eStateHTTP.HTTP_OK;

        public Result():this(0) { }        

        public Result(int pState = 0, string pTextError = "Ok", string pData = "")
        {
            State = pState;
            TextError = pTextError;
            Data = pData;
        }
        public Result(HttpStatusCode p)
        {
            State =  ((int)p >= 200) && ((int)p <= 299) ? 0: (int)p;
            TextError = p.ToString();
            Data = "";
        }
        public Result(HttpResult httpResult, string pData = null)
        {
            StateHTTP = httpResult.HttpState;
            if (httpResult.HttpState != eStateHTTP.HTTP_OK)                
            {
                State = -1;
                TextError = httpResult.HttpState.ToString();
            }
            
            Data = pData?? httpResult.Result;
        }
        public Result(Exception e)
        {
            State = -1;
            TextError = e.Message + "\n" + e.StackTrace;
        }        
    }

    public class Result<T>:Result
    {
        //public int State { get; set; } = 0;
        //public string TextError { get; set; }
        [Obsolete("Proporty is deprecated, please use Data")]        
//        public new T Info { get { return Data; } set { Data = value; } }
//        //public eStateHTTP StateHTTP { get; set; }
//#if WEB
//        [System.Text.Json.Serialization.JsonIgnore]
//        [Newtonsoft.Json.JsonIgnore]
//#endif
        public new T Data { get; set; }
        public Result() : base() { }

        public Result(Result pR):base(pR.State, pR.TextError){ }

        public Result(int pState = 0, string pTextError = "Ok"):base(pState, pTextError) { }        

        public Result(HttpResult httpResult, T pData =default ):base(httpResult)
        {            
            Data = pData;
        }
        public Result(Exception e) : base(e) { }
        
        public Result GetResult { get { return new Result { State=State, TextError = TextError }; } }
    }
}
