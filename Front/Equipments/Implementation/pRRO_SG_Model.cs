using ModelMID;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Equipments.pRRO_SG
{

    public enum eTypeDoc
    {
        Sale = 0,
        Refund = 1,
        MoneyIn = 2,
        MoneyOut = 4,
        NoFiscalReceipt = 100,
        CanceledCheck = 101
    }

    public enum eTypeRecord
    {
        SKU = 1,
        PAY = 2
    }

    public enum eTypePay
    {
        /// <summary>
        /// Готівка
        /// </summary>
        Cash = 0,
        /// <summary>
        /// Кредит
        /// </summary>
        Credit = 1,
        /// <summary>
        /// Чек????
        /// </summary>
        Recipt = 2,
        /// <summary>
        /// Картка
        /// </summary>
        Card = 3,
        /// <summary>
        /// Бонуси
        /// </summary>
        Bonus = 4
    }

    public class Record
    {

        public eTypeRecord type { get; set; }
        public int number { get; set; }
        public Record(eTypeRecord pType = eTypeRecord.SKU) { type = pType; }
    }

    public class RecordSKU : Record
    {
        public RecordSKU() : base(eTypeRecord.SKU) { }
        public RecordSKU(ReceiptWares pRW) : base(eTypeRecord.SKU)
        {
            SKU = pRW.CodeWares;
            UKTZED = pRW.CodeUKTZED;
            barcode = pRW.BarCode;
            if (!string.IsNullOrEmpty(pRW.ExciseStamp))
                excises = pRW.ExciseStamp.Split(',');
            name = pRW.NameWares;
            price = pRW.PriceEKKA;
            quantity = pRW.Quantity;
            discountSum = pRW.DiscountEKKA;
            amount = pRW.Sum ;
            //vatGroup = GetTaxGroup(pRW);
            codeUnit = pRW.CodeUnit;
            unitName = pRW.AbrUnit;
        }

        public ReceiptWares GetReceiptWares()
        {
            return new ReceiptWares()
            {
                CodeWares = SKU,
                CodeUKTZED = UKTZED,
                BarCode = barcode,
                //pRW.ExciseStamp = excises.
                NameWares = name,
                //PriceEKKA = Convert.ToHexString(price / 100m)
                Quantity = quantity,
                //discountSum = Convert.ToInt32(pRW.DiscountEKKA * 100);
                //amount = Convert.ToInt32(pRW.Sum * 100);
                //vatGroup = Global.GetTaxGroup(pRW.TypeVat, pRW.TypeWares);
                CodeUnit = codeUnit,
                AbrUnit = unitName
            };
        }
        public int SKU { get; set; }
        public string UKTZED { get; set; }
        public string barcode { get; set; }
        public IEnumerable<string> excises { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public decimal quantity { get; set; }
        public decimal discountSum { get; set; }
        public decimal amount { get; set; }
        public string vatGroup { get; set; }
        public string registeredArticle { get; set; }
        public int codeUnit { get; set; }
        public string unitName { get; set; }
    }

    public class Card
    {
        public Card() { }
        public Card(Payment pP, bool pIsSale)
        {
            sumPay = pP.SumPay;
            comission = 0;//???
            acquirer = pP.Bank;
            posId = pP.NumberTerminal;
            ePayType = pP.NumberCard;
            paySystem = pP.IssuerName;
            authCode = pP.CodeAuthorization;
            transactionId = pP.NumberSlip;
            payTypeName = pIsSale ? "Оплата" : "Повернення";
        }

        /// <summary>
        /// 3699 – Сума оплати в копійках
        /// </summary>
        public decimal sumPay { get; set; }
        /// <summary>
        /// 0???– Комісія банку
        /// </summary>
        public int comission { get; set; }
        /// <summary>
        /// ПриватБанк – Назва банку
        /// </summary>
        public string acquirer { get; set; }
        /// <summary>
        /// S1LF02AA – id банківського терміналу
        /// </summary>
        public string posId { get; set; }
        /// <summary>
        ///  "************243" – Номер картки
        /// </summary>
        public string ePayType { get; set; }
        /// <summary>
        /// "VISA" – Тип банківської картки
        /// </summary>
        public string paySystem { get; set; }
        /// <summary>
        /// 324242 – Код авторизації
        /// </summary>
        public string authCode { get; set; }
        /// <summary>
        /// 052472567833 - No транзакції
        /// </summary>
        public string transactionId { get; set; }
        /// <summary>
        /// ОПЛАТА – Ім’я операції
        /// </summary>
        public string payTypeName { get; set; }
        /// <summary>
        /// 1/0 – Вказівник на підпис
        /// </summary>
        public int signNeeded { get; set; } = 0;
    }

    public class RecordPay : Record
    {
        public RecordPay() : base(eTypeRecord.PAY) { }
        public RecordPay(Payment pP) : base(eTypeRecord.PAY)
        {
            sum = pP.SumPay;
            returnSum = 0;
            typePay = (pP.TypePay == ModelMID.eTypePay.Card ? eTypePay.Card : eTypePay.Cash);
            if (pP.TypePay == ModelMID.eTypePay.Card)
            {
                cards = new List<Card>() { new Card(pP, sum > 0) };
            }
        }
        public decimal sum { get; set; }
        public int returnSum { get; set; }
        public eTypePay typePay { get; set; }
        public IEnumerable<Card> cards { get; set; }
    }

    public class pRroRequestBaseSG
    {
        public eTypeDoc docSubType { get; set; }
//        [JsonIgnore]
        
        public decimal sum { get; set; }
        public Guid? id { get; set; }
        public string cashierName { get; set; }
    }
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class pRroRequestSG : pRroRequestBaseSG
    {
        public pRroRequestSG() { }
        public pRroRequestSG(Receipt pR)
        {
            if (pR == null) return;
            sum = pR.SumReceipt;
            docSubType = pR.TypeReceipt == eTypeReceipt.Refund ? eTypeDoc.Refund : eTypeDoc.Sale;
            id = pR._ReceiptId;
            var b = new List<Record>();
            foreach (var el in pR.Wares)
                b.Add(new RecordSKU(el));

            foreach (var el in pR.Payment)
                b.Add(new RecordPay(el));
            body = b;
        }

        public int roundDiscount { get; set; } = 0;

        public IEnumerable<Record> body { get; set; }
    }

    public class Tax
    {
        /// <summary>
        /// Код типу податку 
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// Найменування податку
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Літерне позначення податку 
        /// </summary>
        public string letters { get; set; }
        /// <summary>
        /// Розмір податку у відсотках
        /// </summary>
        public int percent { get; set; }
        /// <summary>
        ///  Ознака податку, який не включено у вартість
        /// </summary>
        public bool notIncluded { get; set; }
        /// <summary>
        /// База оподаткування без урахування знижки 
        /// </summary>
        public decimal baseFlow { get; set; }
        /// <summary>
        /// База оподаткування з урахуванням знижки
        /// </summary>
        public decimal discountedFlow { get; set; }
        /// <summary>
        /// Сума податку
        /// </summary>
        public decimal sum { get; set; }
    }

    public class pRroAnswerSG : pRroRequestSG
    {
        public pRroAnswerSG() { }
        /// <summary>
        /// QR Code для перевірки чека на сайті податкової
        /// </summary>
        public string QR { get; set; }
        /// <summary>
        /// Текстовий вигляд чека.
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// Податки
        /// </summary>
        public IEnumerable<Tax> taxes { get; set; }
        /// <summary>
        /// Локальний номер документа (тільки для фіскальних документів) 
        /// </summary>
        public int docNumber { get; set; }
        /// <summary>
        /// Фіскальний номер документа
        /// </summary>
        public string fiscalNumber { get; set; }
        /// <summary>
        /// Наскрізний номер документа (для фіскальних та нефіскальних документів) 
        /// </summary>
        public int receiptNumber { get; set; }
    }

    
}

