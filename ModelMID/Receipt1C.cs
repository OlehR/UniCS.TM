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

        public UInt64 NumberReceipt { get; set; }
        public int CodeClientCard { get; set; }
        public int CodeWarehouse { get { return 9; } set { } }

        public string RefundNumber { get; set; }
        public string BarCodeCashier { get; set; }
        public IEnumerable<ReceiptWares1C> Wares;

        public int CodeBank { get; set; }
        public Receipt1C() { }
        public Receipt1C(Receipt parR)
        {
            Date = parR.DateReceipt;
            //Фінт з датою заради 4 знаків для дати. Вистачить на років 30.
            Number = parR.NumberReceipt1C;
            RefundNumber = parR.RefundNumberReceipt1C;
            TypeReceipt =  (parR.TypeReceipt== eTypeReceipt.Refund? eTypeReceipt.Refund:eTypeReceipt.Sale);
            NumberCashDesk = parR.IdWorkplace;            
            CodeClientCard = parR.CodeClient;
            BarCodeCashier = parR.UserCreate.ToString();

            UInt64 nr=0;
            if(UInt64.TryParse(parR.NumberReceipt,out nr))                
             NumberReceipt = nr;

            if (parR.Wares!=null && parR.StateReceipt>0) 
              Wares = parR.Wares.Select(r => new ReceiptWares1C(r));
            if (parR.Payment != null)
             Description = parR.Payment.Where(r => !string.IsNullOrEmpty(r.CodeAuthorization)).FirstOrDefault().CodeAuthorization;
            var wp = Global.GetWorkPlaceByIdWorkplace(parR.IdWorkplace);
            if (wp != null)
                CodeBank = (int)wp.TypePOS;
        }

        public string GetBase64()
        {
            var Receipt = JsonConvert.SerializeObject(this);
            var plainTextBytes = Encoding.UTF8.GetBytes(Receipt);
            var res= Convert.ToBase64String(plainTextBytes);
            
            return res; /// Convert.ToBase64String(plainTextBytes);
        }
        /*public string GetSOAP()
        {

            string SoapText = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + "\n" +
       "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" + "\n" +
       "<soap:Body><JSONCheck xmlns = \"vopak\" >" + "\n" +
       "<JSONSting>" + GetBase64() + " </JSONSting>" + "\n" +
       "</JSONCheck>" + "\n" +
       "</soap:Body>" + "\n" +
       "</soap:Envelope>";
            return SoapText;
        } */
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
        public bool IsPromotion { get {return  (CodePS > 1000000); } }
        private Int64 CodePS { get; set; }
            
        public decimal SumBonus { get; set; }
        public string BarCode2Category { get; set; }
        public int YearPS { get { return CodePS>20000000 ? Convert.ToInt32(CodePS.ToString().Substring((CodePS > 100000000 ? 1 : 0), 4)):0; } }
        public int NumberPS { get { return CodePS > 20000000 ? Convert.ToInt32(CodePS.ToString().Substring((CodePS > 100000000 ? 1 : 0)+4)):0; } }
        public ReceiptWares1C() { }
        public ReceiptWares1C(ReceiptWares parRW)
        {
            Order = parRW.Order;
            CodeWares = parRW.CodeWares;
            Quantity = parRW.Quantity;
            AbrUnit = parRW.AbrUnit;
            Price = parRW.Price;
            SumDiscount = parRW.SumDiscount;
            Sum = parRW.Sum- parRW.SumDiscount;            
            CodePS = ( parRW.TypePrice==eTypePrice.Promotion || parRW.TypePrice == eTypePrice.PromotionIndicative ? parRW.ParPrice1:0);            
            SumBonus = 0;//TMP!!! ще не реалізовано.
            BarCode2Category = parRW.BarCode2Category;
        }

    }
}
