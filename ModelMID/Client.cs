using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utils;

namespace ModelMID
{
    /// <summary>
    /// Зберігає інформацію про клієнта
    /// </summary>
    public class Client:Model.Client
    {  
        public IEnumerable<ReceiptGift> ReceiptGift { get; set; }
        public Client() { }
        public Client(int parCodeClient) => CodeClient=parCodeClient; 
    }
}
