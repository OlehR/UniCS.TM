using System;
using System.Net;

namespace Utils
{
    /// <summary>
    /// Клас з статусом виконання
    /// </summary>
    public class Status
    {
        /// <summary>
        /// 0 - Ok, інші стани код помилки.
        /// </summary>
        public int State { get; set; } = 0;
        /// <summary>
        /// Ok або текст помилки
        /// </summary>
        public string TextState { get; set; } = "Ok";
        public bool status { get { return State == 0; } }

        public Status(bool pState)
        {
            if (!pState)
            {
                State = -1;
                TextState = "Error";
            }
        }
        public Status(int pState = 0, string pTextState = "Ok")
        {
            State = pState;
            TextState = pTextState;
        }
        public Status(Exception e)
        {
            State = -1;
            TextState = e.Message + "\n" + e.StackTrace;
        }
        public Status(HttpStatusCode pSC)
        { if (pSC != HttpStatusCode.OK && pSC != HttpStatusCode.Created)
            {
                State = -(int)pSC;
                TextState = pSC.ToString();
            }
        }
        public Status()
        {
            State = 0;
            TextState = "Ok";
        }
    }
    public class Status<D> : Status
    {
        public D Data { get; set; }
        public Status() : base() { }
        public Status(D pD) : base() { Data = pD; }
        public Status(int pState = 0, string pTextState = "Ok") : base(pState, pTextState) { }
        public Status(Exception e) : base(e) { }
        public Status(HttpStatusCode pSC) : base(pSC) { }
    }
    public class StatusIsBonus : Status<string>
    {
        public bool is_bonus {get{ return Data?.IndexOf("нараховано бонусів ") > 0; } }
        public StatusIsBonus(int pState = 0, string pTextState = "Ok") : base(pState, pTextState) { }
        public StatusIsBonus(Status<string> pS)
        {
            new StatusIsBonus(pS.State, pS.TextState) { Data=pS.Data}; 
        }
    }

}
