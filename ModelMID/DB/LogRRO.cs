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
        CanceledCheck = 101,
        ZReport =1000,
        XReport = 1001,
        PeriodZReport = 1002,
        CopyReceipt =1010,
        ClosePort,
        ZReportPOS = 2000,
        XReportPOS = 2001,
        SalePOS =   20100,
        RefundPOS = 20101
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
        public string Error { get; set; } = null;
        public int CodeError { get; set; } = 0;
        public int UserCreate { get; set;}       
    }
   
}
