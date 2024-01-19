using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class FindClient
    {
        /// <summary>
        /// Друзі
        /// </summary>
        public int PinCode { get; set; }
       
        public string BarCode { get; set; }
        public int CodeClient { get; set; }
        /// <summary>
        /// Код клієнта (Для СпарУкраїна
        /// </summary>
        public string GuidClient { get; set; }  
        public string Phone { get; set; }
        public int CodeWarehouse { get; set; }
        public Client Client { get; set; }
    }
}
