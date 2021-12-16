using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Equipments.Implementation
{
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
