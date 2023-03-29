using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

namespace ModelMID
{
    public class Receipt1C
    {
        /// <summary>
        /// Дата чека
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Номер чека 1С
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 1 - звичайний, -1 - повернення
        /// </summary>
        public eTypeReceipt TypeReceipt { get; set; }
        /// <summary>
        /// Номер каси
        /// </summary>
        public int NumberCashDesk { get; set; }
        public string Description { get; set; }
        public UInt64 NumberReceipt { get; set; }
        public int CodeClientCard { get; set; }
        public int CodeWarehouse { get; set; }
        public string RefundNumber { get; set; }
        /// <summary>
        /// Штрихкод касирів
        /// </summary>
        public string BarCodeCashier { get; set; }
        public IEnumerable<ReceiptWares1C> Wares;

        public int CodeBank { get; set; }
        public decimal SumWallet { get; set; }
        public Receipt1C() { }
        public Receipt1C(Receipt pR)
        {
            Date = pR.DateReceipt;
            //Фінт з датою заради 4 знаків для дати. Вистачить на років 30.
            Number = pR.NumberReceipt1C;
            RefundNumber = pR.RefundNumberReceipt1C;
            TypeReceipt =  (pR.TypeReceipt== eTypeReceipt.Refund? eTypeReceipt.Refund:eTypeReceipt.Sale);
            NumberCashDesk = pR.IdWorkplacePay > 0 ? pR.IdWorkplacePay : pR.IdWorkplace;
            var wp = Global.GetWorkPlaceByIdWorkplace(NumberCashDesk);
            CodeClientCard = pR.CodeClient;
            BarCodeCashier = pR.UserCreate.ToString();
            CodeWarehouse = wp.CodeWarehouse;

            if (ulong.TryParse(pR.NumberReceipt, out ulong nr))
                NumberReceipt = nr;

            if (pR.Wares!=null && pR.StateReceipt>0) 
              Wares = pR.Wares.Select(r => new ReceiptWares1C(r));

            var Card=pR.Payment.Where(r => r.TypePay == eTypePay.Card)?.FirstOrDefault();

            if (Card!=null)
                Description = Card.CodeAuthorization;
            else
                Description = "0000000";           

            SumWallet = pR.Payment.Where(r => r.TypePay == eTypePay.Wallet && r.SumPay>0)?.FirstOrDefault()?.SumPay ?? 0;

            var Cash = pR.Payment.Where(r => r.TypePay == eTypePay.Cash)?.FirstOrDefault();
            if (Cash != null) CodeBank = 1;
            else
                CodeBank = (int) (Card?.CodeBank ?? (wp?.TypePOS??eBank.NotDefine));           
        }

        public string GetBase64()
        {
            var Receipt = JsonConvert.SerializeObject(this);
            var plainTextBytes = Encoding.UTF8.GetBytes(Receipt);
            var res= Convert.ToBase64String(plainTextBytes);            
            return res; /// Convert.ToBase64String(plainTextBytes);
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
        public bool IsPromotion { get {return  (CodePS > 1000000); } }
        private Int64 CodePS { get; set; }
            
        public decimal SumBonus { get; set; }
        public string BarCode2Category { get; set; }
        public int YearPS { get { return CodePS>20000000 ? Convert.ToInt32(CodePS.ToString().Substring((CodePS > 100000000 ? 1 : 0), 4)):0; } }
        public int NumberPS { get { return CodePS > 20000000 ? Convert.ToInt32(CodePS.ToString().Substring((CodePS > 100000000 ? 1 : 0)+4)):0; } }
        public string ManualPercentDiscount { get { return string.IsNullOrEmpty(BarCode2Category) || BarCode2Category.Length != 13 ? null : BarCode2Category.Substring(3, 2); } }
        public int TypeDiscount { get { return string.IsNullOrEmpty(BarCode2Category) || BarCode2Category.Length != 13 ? 0:1; } }
        public ReceiptWares1C() { }
        public ReceiptWares1C(ReceiptWares parRW)
        {
            Order = parRW.Order;
            CodeWares = parRW.CodeWares;
            Quantity = parRW.Quantity;
            AbrUnit = parRW.AbrUnit;
            Price = parRW.Price;
            SumDiscount = parRW.SumDiscount+parRW.SumWallet;
            Sum = parRW.Sum - SumDiscount;            
            CodePS = ( parRW.TypePrice==eTypePrice.Promotion || parRW.TypePrice == eTypePrice.PromotionIndicative ? parRW.ParPrice1:0);            
            SumBonus = 0; //TMP!!! ще не реалізовано.
            BarCode2Category = parRW.BarCode2Category;
        }

    }

    public class ReceiptWaresDeleted1C : IdReceipt
    {
        /// <summary>
        /// Дата чека
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Номер часу 1С
        /// </summary>
        public string Number { get { return NumberReceipt1C; } }        
        /// <summary>
        /// Номер каси
        /// </summary>
        public int NumberCashDesk { get; set; }        
        /// <summary>
        /// Штрихкод касирів
        /// </summary>
        public string BarCodeCashier { get; set; }
        /// <summary>
        /// Порядок (Sort)
        /// </summary>
        public int Order { get; set; }
        public int CodeWares { get; set; }
        /// <summary>
        /// Час вставки рядка.
        /// </summary>
        public DateTime  DateCreate { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityOld { get; set; }
    }
}
