using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Utils.Status;

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
        public Status()
        {
            State = 1;
            TextState ="Ok";
        }
    }
    public class StatusD<D> : Status
    {
        public D Data { get; set; }
        public StatusD() : base() { }
        public StatusD(int pState = 0, string pTextState = "Ok"):base(pState ,  pTextState) { }
        public StatusD(Exception e) : base(e) { }
    }

}
