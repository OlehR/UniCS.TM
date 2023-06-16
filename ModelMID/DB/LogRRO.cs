using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Utils;

namespace ModelMID.DB
{

    public enum eTypeOperation
    {
        [Description("NotDefine")]
        NotDefine = -1,
        [Description("Продаж")]
        Sale = 0,
        [Description("Повернення")]
        Refund = 1,
        [Description("MoneyIn")]
        MoneyIn = 2,
        [Description("MoneyOut")]
        MoneyOut = 4,
        [Description("Отримання суми готівки")]
        SumInCash = 5,
        [Description("Вивід на табло")]
        PutToDisplay =6,
        [Description("Не фіскальний")]
        NoFiscalReceipt = 100,
        [Description("Анульований чек")]
        CanceledCheck = 101,
        [Description("Тест RRO")]
        TestDevice = 150,
        [Description("Інформація про RRO")]
        DeviceInfo = 151,
        [Description("Програмування Артикулу РРО")]
        ProgramingArticle=152,
        [Description("Отримання тексту останнього чека")]
        LastReceipt =153,

        [Description("Z звіт")]
        ZReport = 1000,
        [Description("Х звіт")]
        XReport = 1001,
        [Description("Періодичний Z звіт")]
        PeriodZReport = 1002,
        [Description("Копія чеку")]
        CopyReceipt = 1010,
        [Description("Закритий порт")]
        ClosePort,
        [Description("Z звіт POS")]
        ZReportPOS = 2000,
        [Description("X звіт POS")]
        XReportPOS = 2001,
        [Description("Продаж POS")]
        SalePOS = 20100,
        [Description("Повернення POS")]
        RefundPOS = 20101
    }

    public class LogRRO : IdReceipt
    {
        public LogRRO() { }
        public LogRRO(IdReceipt pIdReceipt) : base(pIdReceipt) { }
        
        public int NumberOperation { get; set; }
        public string FiscalNumber { get; set; }
        public eTypeOperation TypeOperation { get; set; }
        public string TranslationTypeOperation { get { return TypeOperation.GetDescription(); } }
        public decimal SUM { get; set; }
        /// <summary>
        /// Для XZ сума повернення
        /// </summary>
        public decimal SumRefund { get; set; }
        public string TypeRRO { get; set; }        
        public string JSON { get; set; }
        public string TextReceipt { get; set; }
        public string Error { get; set; } = null;
        public int CodeError { get; set; } = 0;
        public int UserCreate { get; set;}       
    }
   
}
