using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

namespace ModelMID
{
    public class Receipt1C
    {
        public DateTime Date { get; set; }
        public string Number { get; set; }
        /// <summary>
        /// 1 - звичайний, -1 - повернення
        /// </summary>
        public eTypeReceipt TypeReceipt { get; set; }
        public int NumberCashDesk { get; set; }
        public string Description { get; set; }

        public int CodeClientCard { get; set; }
        public int CodeWarehouse { get { return 9; } set { } } 

        public IEnumerable<ReceiptWares1C> Wares;

        public Receipt1C() { }
        public Receipt1C(Receipt parR)
        {
            Date = parR.DateReceipt;
            //Фінт з датою заради 4 знаків для дати. Вистачить на років 30.
            Number = parR.NumberReceipt1C;
            TypeReceipt = parR.TypeReceipt;
            NumberCashDesk = parR.IdWorkplace;            
            CodeClientCard = parR.CodeClient;
            if(parR.Wares!=null) 
              Wares = parR.Wares.Select(r => new ReceiptWares1C(r));
            if (parR.Payment != null)
                Description = parR.Payment.Where(r => !string.IsNullOrEmpty(r.NumberSlip)).FirstOrDefault().NumberSlip;
        }

        public string GetSOAP()
        {
            var Receipt = JsonConvert.SerializeObject(this);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(Receipt);
            string SoapText = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + "\n" +
       "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" + "\n" +
       "<soap:Body><CreateReceipt xmlns = \"vopak\" >" + "\n" +
       "< xmlStr >" + System.Convert.ToBase64String(plainTextBytes) + " </ xmlStr >" + "\n" +
       "</ CreateOrderOfSuplier >" + "\n" +
       "</ soap:Body>" + "\n" +
       "</soap:Envelope>";
            return SoapText;
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
