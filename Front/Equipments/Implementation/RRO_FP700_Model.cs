using ModernExpo.SelfCheckout.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Equipments.Implementation.FP700_Model
{
    public class ProductArticle
    {
        public int Id { get; set; }

        public string ProductName { get; set; }

        public double Price { get; set; }

        public int PLU { get; set; }

        public string Barcode { get; set; }
    }

    public class MoneyMovingModel
    {
        public MoneyMovingDestination MoneyDestination { get; set; }

        public decimal Sum { get; set; }

        public string Description { get; set; }

        public int OperatorNumber { get; set; }
    }

    public interface IFiscalReceiptConfiguration
    {
    }

    public class Fp700ReceiptConfiguration : IFiscalReceiptConfiguration
    {
        public string HeaderLine1 { get; set; }

        public string HeaderLine2 { get; set; }

        public string HeaderLine3 { get; set; }

        public string HeaderLine4 { get; set; }

        public string HeaderLine5 { get; set; }

        public string HeaderLine6 { get; set; }

        public byte[] Logo { get; set; }

        public string FooterLine1 { get; set; }

        public string FooterLine2 { get; set; }

        public bool ShouldPrintLogo { get; set; }

        public bool ShouldAutoCutPaper { get; set; }
    }
}
