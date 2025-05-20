using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Utils;

namespace ModelMID
{
    public class Order
    {
        private int _OrderNumber;
        public int OrderNumber { get => Id < 100 ? Id : Id % 100; set => _OrderNumber = value; }
        public eStatus Status { get; set; }
        public string TranslatedStatus { get => Status.GetDescription(); }
        public string StatusIcon { get => $"Images/{Status}.png"; }
        public Order()
        {
            DateCreate = DateTime.Now;
        }
        public int Id { get; set; }
        public int IdWorkplace { get; set; }
        public int CodePeriod { get; set; }
        public int CodeReceipt { get; set; }
        public IEnumerable<OrderWares> Wares { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string Type { get; set; }
        public string JSON { get; set; }
    }
    public enum eStatus
    {
        [Description("Очікує")]
        Waiting,
        [Description("Готується")]
        Preparing,
        [Description("Готово!")]
        Ready,
    }

    public class OrderWares
    {
        public int IdWorkplace { get; set; }
        public int CodePeriod { get; set; }
        public int CodeReceipt { get; set; }
        public long CodeWares { get; set; }
        public string NameWares { get; set; }
        public decimal Quantity { get; set; }
        public int Sort { get; set; }
        public DateTime DateCreate { get; set; }
        public int UserCreate { get; set; }

        public IEnumerable<OrderReceiptLink> ReceiptLinks { get; set; }
        public OrderWares(ReceiptWares receiptWares)
        {
            this.CodeWares = receiptWares.CodeWares;
            this.NameWares = receiptWares.NameWares;
            this.UserCreate = receiptWares.UserCreate;
            this.CodePeriod = receiptWares.CodePeriod;
            this.CodeReceipt = receiptWares.CodeReceipt;
            this.IdWorkplace = receiptWares.IdWorkplace;
            this.Sort = receiptWares.Sort;
            this.Quantity = receiptWares.Quantity;
            this.DateCreate = DateTime.Now;
            this.UserCreate = receiptWares.UserCreate;
            this.ReceiptLinks = receiptLinks(receiptWares);

        }
        public OrderWares() { }
        private IEnumerable<OrderReceiptLink> receiptLinks(ReceiptWares receiptWares)
        {
            List<OrderReceiptLink> orderReceiptLinks = new List<OrderReceiptLink>();
            foreach (ReceiptWaresLink w in receiptWares.ReceiptWaresLink)
            {
                    orderReceiptLinks.Add(new OrderReceiptLink(w, receiptWares));
                
            }

            return orderReceiptLinks;
        }

    }

    public class OrderReceiptLink
    {
        public int IdWorkplace { get; set; }
        public string Name { get; set; }
        public int CodePeriod { get; set; }
        public int CodeReceipt { get; set; }
        public long CodeWares { get; set; }
        public decimal Quantity { get; set; }
        public long CodeWaresTo { get; set; }
        public int Sort { get; set; }
        public OrderReceiptLink(ReceiptWaresLink waresLink, ReceiptWares receiptWares)
        {
            IdWorkplace = waresLink.IdWorkplace;
            CodePeriod = waresLink.CodePeriod;
            CodeReceipt = waresLink.CodeReceipt;
            CodeWaresTo = receiptWares.CodeWares;
            CodeWares = waresLink.CodeWares;
            Name = waresLink.NameWares;
            Quantity = waresLink.Quantity;
            Sort = receiptWares.Sort;
        }
        public OrderReceiptLink() { }

    }
}
