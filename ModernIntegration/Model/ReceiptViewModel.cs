using System;
using System.Collections.Generic;
using System.Linq;
using ModernExpo.SelfCheckout.Entities.Enums;
using ModernExpo.SelfCheckout.Entities.Models;
using ModernExpo.SelfCheckout.Entities.Session;

namespace ModernExpo.SelfCheckout.Entities.ViewModels
{
    public class ReceiptViewModel
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the terminal identifier.
        /// </summary>
        /// <value>
        /// The terminal identifier.
        /// </value>
        public Guid TerminalId { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public ReceiptStatusType Status { get; set; }

        /// <summary>
        /// Gets or sets the type of the payment.
        /// </summary>
        /// <value>
        /// The type of the payment.
        /// </value>
        public PaymentType PaymentType { get; set; }

        /// <summary>
        /// Gets or sets the fiscal number.
        /// </summary>
        /// <value>
        /// The fiscal number.
        /// </value>
        public string FiscalNumber { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the paid amount.
        /// </summary>
        /// <value>
        /// The paid amount.
        /// </value>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Gets or sets the total discount.
        /// </summary>
        /// <value>
        /// The total discount.
        /// </value>
        public decimal Discount { get; set; }

        /// <summary>
        /// Gets or sets the total amount.
        /// </summary>
        /// <value>
        /// The total amount.
        /// </value>
        public decimal TotalAmount { get; set; }


        private List<ReceiptItem> _receiptItems;
        /// <summary>
        /// Gets or sets the receipt items.
        /// </summary>
        /// <value>
        /// The receipt items.
        /// </value>
        public List<ReceiptItem> ReceiptItems
        {
            get => _receiptItems;
            set
            {

                _receiptItems = value;
                if (_receiptItems == null)
                {
                    return;
                }
                foreach (var item in _receiptItems)
                {
                    item.ReceiptId = Id;
                }
            }
        }

        /// <summary>
        /// Gets or sets the customer.
        /// </summary>
        /// <value>
        /// The customer.
        /// </value>
        public CustomerViewModel Customer { get; set; }

        /// <summary>
        /// The payment information
        /// </summary>
        private ReceiptPayment _paymentInfo;

        /// <summary>
        /// Gets or sets the payment information.
        /// </summary>
        /// <value>
        /// The payment information.
        /// </value>
        public ReceiptPayment PaymentInfo
        {
            get => _paymentInfo;
            set
            {
                if (value == null)
                {
                    _paymentInfo = null;
                    return;
                }
                value.ReceiptId = Id;
                _paymentInfo = value;
            }
        }

        /// <summary>
        /// Gets or sets the receipt events.
        /// </summary>
        /// <value>
        /// The receipt events.
        /// </value>
        public List<ReceiptEvent> ReceiptEvents { get; set; }

        /// <summary>
        /// Gets or sets the good luck.
        /// </summary>
        /// <value>
        /// The good luck.
        /// </value>
        public string GoodLuck { get; set; } = " ";

        /// <summary>
        /// Gets or sets the ad.
        /// </summary>
        /// <value>
        /// The ad.
        /// </value>
        public string Ad { get; set; } = " ";

        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        /// <value>
        /// The created at.
        /// </value>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the updated at.
        /// </summary>
        /// <value>
        /// The updated at.
        /// </value>
        public DateTime UpdatedAt { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiptViewModel"/> class.
        /// </summary>
        public ReceiptViewModel()
        {
            Status = ReceiptStatusType.Created;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceiptViewModel" /> class.
        /// </summary>
        /// <param name="receipt">The receipt.</param>
        /// <param name="receiptItems">The receipt items.</param>
        /// <param name="customer">The customer.</param>
        /// <param name="sessionEvents">The session events.</param>
        public ReceiptViewModel(Receipt receipt, List<ReceiptItem> receiptItems, CustomerViewModel customer, List<SessionProductEvent> sessionEvents)
        {
            Id = receipt.Id;
            FiscalNumber = receipt.FiscalNumber;
            Status = receipt.Status;
            TerminalId = receipt.TerminalId;
            TotalAmount = receipt.TotalAmount;
            Customer = customer;
            CreatedAt = receipt.CreatedAt;
            UpdatedAt = receipt.UpdatedAt;
            ReceiptItems = receiptItems;

            if (sessionEvents != null)
            {
                ReceiptEvents = sessionEvents.Select(s => new ReceiptEvent(s, Id, ReceiptItems.FirstOrDefault(f=>f.ProductId == s.ProductId)?.Id)).ToList();
            }
        }

        public ReceiptViewModel(ReceiptViewModel model)
        {
            Id = model.Id;
            TerminalId = model.TerminalId;
            Status = model.Status;
            PaymentType = model.PaymentType;
            FiscalNumber = model.FiscalNumber;
            Amount = model.Amount;
            Discount = model.Discount;
            TotalAmount = model.TotalAmount;
            ReceiptItems = new List<ReceiptItem>();
            if (model.ReceiptItems != null)
            {
                ReceiptItems.AddRange(model.ReceiptItems);
            }
            Customer = model.Customer;
            PaymentInfo = model.PaymentInfo;
            CreatedAt = model.CreatedAt;
            UpdatedAt = model.UpdatedAt;
        }

        /// <summary>
        /// To the receipt.
        /// </summary>
        /// <returns></returns>
        public Receipt ToReceipt()
        {
            CreatedAt = CreatedAt == default(DateTime) ? DateTime.Now : CreatedAt;
            UpdatedAt = UpdatedAt == default(DateTime) ? DateTime.Now : UpdatedAt;
            Id = Id == default(Guid) ? Guid.NewGuid() : Id;
            return new Receipt()
            {
                Id = Id,
                FiscalNumber = FiscalNumber,
                Status = Status,
                Amount = Amount,
                Discount = Discount,
                TerminalId = TerminalId,
                TotalAmount = TotalAmount,
                CustomerId = Customer?.Id,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt
            };
        }
    }

    public class UpdateReceiptViewModel
    {
        public List<ProductViewModel> ProductsWithWarning { get; set; }
        public ReceiptViewModel Receipt { get; set; }
        public ReceiptPayment PaymentInfo { get; set; }
    }
}