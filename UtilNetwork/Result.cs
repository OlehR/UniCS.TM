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
        public string TextError { get; set; }
        public string Info { get; set; }
        public eStateHTTP StateHTTP { get; set; }

        public Result():this(0) { }        

        public Result(int pState = 0, string pTextError = "Ok", string pInfo = "")
        {
            State = pState;
            TextError = pTextError;
            Info = pInfo;
        }
        public Result(HttpStatusCode p)
        {
            State =  ((int)p >= 200) && ((int)p <= 299) ? 0: (int)p;
            TextError = p.ToString();
            Info = "";
        }
        public Result(HttpResult httpResult, string pInfo = null)
        {
            StateHTTP = httpResult.HttpState;
            if (httpResult.HttpState != eStateHTTP.HTTP_OK)                
            {
                State = -1;
                TextError = httpResult.HttpState.ToString();
            }
            
            Info = pInfo?? httpResult.Result;
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
        public new T Info { get; set; }
        //public eStateHTTP StateHTTP { get; set; }

        public Result() : base() { }

        public Result(Result pR):base(pR.State, pR.TextError){ }

        public Result(int pState = 0, string pTextError = "Ok"):base(pState, pTextError) { }        

        public Result(HttpResult httpResult, T pInfo =default ):base(httpResult)
        {            
            Info = pInfo;
        }
        public Result(Exception e) : base(e) { }
        
        public Result GetResult { get { return new Result { State=State, TextError = TextError }; } }
    }

}
