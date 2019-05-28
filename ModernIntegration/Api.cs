using ModernExpo.SelfCheckout.Entities.Models;
using ModernExpo.SelfCheckout.Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernIntegration
{

    public enum TypeSend
        {
        NotReady,
        WaitSend,
        Send
    }
    public class Api
    {
        //public bool CreateReceipt(Guid parTerminalId, Guid parReceipt) { return false; }
        public ProductViewModel AddProductByBarCode(Guid parTerminalId,string parS) { return null; }
        public ProductViewModel AddProductByProductId(Guid parTerminalId, Guid paparProductId,decimal parQuantity = 0) { return null; }
        public ReceiptViewModel ChangeQuanity(Guid parTerminalId, Guid parProductId, decimal parQuantity ) { return null; }
        public ReceiptViewModel GetReciept(Guid parReceipt) { return null; }
        public bool AddPayment(Guid parTerminalId, Guid parReceiptId, ReceiptPayment[] parPayment) { return false; }
        public bool AddFiscalNumber(Guid parReceiptId,string parFiscalNumber) { return false; }
        public bool ClearReceipt(Guid parTerminalId) { return false; }

        public List<ProductViewModel> GetBags() { return null; }

        public List<ProductCategory> GetAllCategories() { return null; }
        public List<ProductCategory> GetCategoriesByParentId(Guid categoryId) { return null; }
        public List<ProductViewModel> GetProductsByCategoryId(Guid categoryId) { return null; }

        public bool UpdateReceipt(ReceiptViewModel parReceipt) { return false; }
        public TypeSend SendReceipt(Guid parReceipt) { return TypeSend.NotReady; }
        public TypeSend GetStatusReceipt(Guid parReceipt)  { return TypeSend.NotReady; } 

        public CustomerViewModel GetCustomerByBarCode(string parS) { return null; }
        public CustomerViewModel GetCustomerByPhone(string parS) { return null; }


    }
}
