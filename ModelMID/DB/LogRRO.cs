using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID.DB
{
    public enum eTypeOperation
    {
        Sale = 0,
        Refund = 1,
        MoneyIn = 2,
        MoneyOut = 4,
        NoFiscalReceipt = 100,
        CanceledCheck = 101
    }
    public class LogRRO: IdReceipt
    {
        public LogRRO(IdReceipt pIdReceipt) : base(pIdReceipt) { }
        public int NumberOperation { get; set; }
        public string FiscalNumber { get; set; }
        public eTypeOperation TypeOperation { get; set; }
        public decimal SUM { get; set; }
        public string TypeRRO { get; set; }
        public string JSON { get; set; }
        public string TextReceipt { get; set; }
        public string Error { get; set;}
        public int UserCreate { get; set;}
       
    }
}
