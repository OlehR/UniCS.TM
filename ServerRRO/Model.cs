using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServerRRO
{
    public enum eStateEquipment { On = 0, Init = 1, Off = 2, Process = 3, Error = 4 }

    public enum eTypeOperation
    {
        [Description("Продаж")]
        Sale = 0,
        [Description("Повернення")]
        Refund = 1,
        [Description("MoneyIn")]
        MoneyIn = 2,
        [Description("MoneyOut")]
        MoneyOut = 4,
        [Description("Не фіскальний")]
        NoFiscalReceipt = 100,
        [Description("Анульований чек")]
        CanceledCheck = 101,
        [Description("Тест RRO")]
        TestDevice = 150,
        [Description("Інформація про RRO")]
        DeviceInfo = 151,
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

    [DataContract]
    public class IdReceipt
    {
        [DataMember]
        public int IdWorkplace { get; set; }

        [DataMember]
        public int CodePeriod { get; set; }

        [DataMember]
        public int CodeReceipt { get; set; }
        public IdReceipt(IdReceipt idReceipt)
        {
            if (idReceipt == null)
                return;
            IdWorkplace = idReceipt.IdWorkplace;
            CodePeriod = idReceipt.CodePeriod;
            CodeReceipt = idReceipt.CodeReceipt;
        }
    }
 
    [DataContract]
    public class LogRRO : IdReceipt
    {
        //public LogRRO() { }
        public LogRRO(IdReceipt pIdReceipt) : base(pIdReceipt) { }
        [DataMember]
        public int NumberOperation { get; set; }
        [DataMember]
        public string FiscalNumber { get; set; }
        [DataMember]
        public eTypeOperation TypeOperation { get; set; }
        //public string TranslationTypeOperation { get { return TypeOperation.GetDescription(); } }
        [DataMember]
        public decimal SUM { get; set; }
        [DataMember]
        public string TypeRRO { get; set; }
        [DataMember]
        public string JSON { get; set; }
        [DataMember]
        public string TextReceipt { get; set; }
        [DataMember]
        public string Error { get; set; } = null;
        [DataMember]
        public int CodeError { get; set; } = 0;
        [DataMember]
        public int UserCreate { get; set; }
    }

    [DataContract]
    public class PrintReceiptData
    {
        [DataMember]
        public IdReceipt Id { get; set; }
        [DataMember]
        public string Xml { get; set; }
        [DataMember]
        public eTypeOperation TypeOperation { get; set; }
        [DataMember]
        public decimal Sum { get; set; }
    }
}
