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
            price = Convert.ToInt32(pRW.PriceEKKA * 100);
            quantity = pRW.Quantity;
            discountSum = Convert.ToInt32(pRW.DiscountEKKA * 100);
            amount = Convert.ToInt32(pRW.Sum * 100);
            vatGroup = Global.GetTaxGroup(pRW.TypeVat, pRW.TypeWares);
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
        public int price { get; set; }
        public decimal quantity { get; set; }
        public int discountSum { get; set; }
        public int amount { get; set; }
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
            sumPay = Convert.ToInt32(pP.SumPay * 100);
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
        public int sumPay { get; set; }
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
            sum = Convert.ToInt32(pP.SumPay * 100m);
            returnSum = 0;
            typePay = (pP.TypePay == ModelMID.eTypePay.Card ? eTypePay.Card : eTypePay.Cash);
            if (pP.TypePay == ModelMID.eTypePay.Card)
            {
                cards = new List<Card>() { new Card(pP, sum > 0) };
            }
        }
        public int sum { get; set; }
        public int returnSum { get; set; }
        public eTypePay typePay { get; set; }
        public IEnumerable<Card> cards { get; set; }
    }

    public class pRroRequestBaseSG
    {
        public eTypeDoc docSubType { get; set; }
        [JsonIgnore]
        public decimal? Sum;
        public int? sum { get { return Convert.ToInt32(Sum * 100); } set { Sum = Convert.ToDecimal(value) / 100m; } }
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
            Sum = pR.SumReceipt;
            docSubType = pR.TypeReceipt == eTypeReceipt.Refund ? eTypeDoc.Refund : eTypeDoc.Sale;
            id = pR.ReceiptId;
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

    // створення чеку
    public class CreateReceipt
    {
        public string departament { get; set; }
        public string cashier_name { get; set; }
        public string header { get; set; }
        public string footer { get; set; }
        public string barcode { get; set; }
        public IEnumerable<Discount> discounts { get; set; }
    }
    // кінець створення чека

    // додавання товару
    public class AddWares
    {
        public string code { get; set; }
        public string name { get; set; }
        public string price { get; set; }
        public string quantity { get; set; }
        public IEnumerable<TaxWares> taxes { get; set; }
        public IEnumerable<Discount> discounts { get; set; }
        public string uktzed { get; set; }
        public IEnumerable<Excise_barcod> excise_barcodes { get; set; }
        public string barcode { get; set; }
        public bool is_return { get; set; }
    }

    public class TaxWares
    {
        public int tax { get; set; }
    }
    public class Excise_barcod
    {
        public string excise_barcod { get; set; }
    }
    // кінець додавання товару

    public class Discount
    {
        public string type { get; set; } // "DISCOUNT", string??
        public string mode { get; set; } // "PERCENT", string??
        public int value { get; set; }
        public string name { get; set; }
        public IEnumerable<Tax_code> tax_codes { get; set; }
    }
    public class Tax_code
    {
        public int tax_code { get; set; }
    }

    // Get Shift
    public class GetShift
    {
        public int id { get; set; }
        public string external_id { get; set; } // (3fa85f64-5717-4562-b3fc-2c963f66afa6) - string?
        public int serial { get; set; }
        public DateTime opened_at { get; set; }
        public DateTime closed_at { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public Balance balance { get; set; }
        public IEnumerable<TaxGetShift> taxes { get; set; }
        public IEnumerable<PaymentGetShift> payments { get; set; }

    }
    public class Balance
    {
        public double initial { get; set; } // int?
        public double balance { get; set; }
        public double cash_sales { get; set; }
        public double card_sales { get; set; }
        public double cash_returns { get; set; }
        public double card_returns { get; set; }
        public double service_in { get; set; } // int?
        public double service_out { get; set; } // int?
        public DateTime updated_at { get; set; }
    }
    public class TaxGetShift
    {
        public int code { get; set; }
        public string label { get; set; }
        public string symbol { get; set; }
        public int rate { get; set; }
        public int extra_rate { get; set; }
        public bool included { get; set; }
        public DateTime created_at { get; set; }
        public int sales { get; set; }
        public int returns { get; set; }
        public double sales_turnover { get; set; }
        public double returns_turnover { get; set; }
    }
    public class PaymentGetShift
    {
        public string type { get; set; }
        public string label { get; set; }
        public int code { get; set; }
        public int sales { get; set; }
        public int returns { get; set; }
        public double service_in { get; set; } // int?
        public double service_out { get; set; } // int?
    }
    // Get Shift END



    // Close Receipt
    public class CloseReceipt
    {
        public IEnumerable<PaymentCloseReceipt> payments { get; set; }
        public Delivery delivery { get; set; }
        public bool print { get; set; }
        public bool remove_rest { get; set; }
        public bool technical_return { get; set; }
        public Context context { get; set; }
        public bool round { get; set; }
    }
    public class PaymentCloseReceipt
    {
        public string type;
        public int code;
        public double value;
        public string label;
        public string card_mask;
        public string bank_name;
        public string auth_code;
        public string rrn;
        public string payment_system;
        public string owner_name;
        public string terminal;
        public string acquirer_and_seller;
        public string receipt_no;
        public bool signature_required;

        public PaymentCloseReceipt() { }
        public PaymentCloseReceipt(string type, double value, string label)
        {
            this.type = type;
            this.value = value;
            this.label = label;
        }
        public PaymentCloseReceipt(string type, int code, double value, string label, string card_mask, string bank_name, string auth_code, string rrn,
            string payment_system, string owner_name, string terminal, string acquirer_and_seller, string receipt_no, bool signature_required)
        {
            this.type = type;
            this.code = code;
            this.value = value;
            this.label = label;
            this.card_mask = card_mask;
            this.bank_name = bank_name;
            this.auth_code = auth_code;
            this.rrn = rrn;
            this.payment_system = payment_system;
            this.owner_name = owner_name;
            this.terminal = terminal;
            this.acquirer_and_seller = acquirer_and_seller;
            this.receipt_no = receipt_no;
            this.signature_required = signature_required;

        }

    }
    public class Delivery
    {
        public string email { get; set; }
    }
    public class Context
    {
        public double additionalProp1 { get; set; }
        public double additionalProp2 { get; set; }
        public double additionalProp3 { get; set; }
    }
    // Close Receipt END
}

