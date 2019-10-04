using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ModelMID
{
    public class Receipt1C
    {
        public DateTime Date { get; set; }
        public string Number { get; set; }
        /// <summary>
        /// 1 - звичайний, -1 - повернення
        /// </summary>
        public int TypeReceipt { get; set; }
        public int NumberCashDesk { get; set; }
        public string Description { get; set; }

        public int CodeClientCard { get; set; }

        public IEnumerable<ReceiptWares1C> Wares;

        public Receipt1C() { }
        public Receipt1C(Receipt parR)
        {
            Date = parR.DateReceipt;
        
            Number=GlobalVar.PrefixWarehouse + "14"+ (Date-new DateTime(2019,01,01)).TotalDays.ToString("D4") +parR.CodeReceipt.ToString("D4");///TMP!!! Придуати номер каси.
            TypeReceipt = 1;///TMP!!!
            NumberCashDesk = parR.IdWorkplace;
            Description = null;///TMP!!! Має бути сліп.
            CodeClientCard = parR.CodeClient;
            if(parR.Wares!=null) 
              Wares = parR.Wares.Select(r => new ReceiptWares1C(r));

        }
        
                
    }
    
    public class ReceiptWares1C
    {
        public int Order { get; set; }
        public int CodeWares { get; set; }
        public decimal Quantity { get; set; }
        public string AbrUnit { get; set; }
        public decimal Price { get; set; }
        public decimal SumDiscount { get; set; }
        public decimal Sum { get; set; }
        public bool IsPromotion { get; set; }
        public Int64 CodePS { get; set; }
        public decimal SumBonus { get; set; }
        public string BarCode2Category { get; set; }
        public ReceiptWares1C() { }
        public ReceiptWares1C(ReceiptWares parRW)
        {
            Order = parRW.Order;
            CodeWares = parRW.CodeWares;
            Quantity = parRW.Quantity;
            AbrUnit = parRW.AbrUnit;
            Price = parRW.Price;
            SumDiscount = parRW.SumDiscount;
            Sum = parRW.Sum;
            IsPromotion = (CodePS >0);
            CodePS = ( parRW.TypePrice==eTypePrice.Promotion? parRW.ParPrice1:0);
            SumBonus = 0;//TMP!!! ще не реалізовано.
            BarCode2Category = parRW.BarCode2Category;

        }

    }
}
