﻿using ModernIntegration.Models;
using ModernIntegration.ViewModels;
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
        public virtual ProductViewModel AddProductByBarCode(Guid parTerminalId,string parS) { return null; }
        public virtual ProductViewModel AddProductByProductId(Guid parTerminalId, Guid paparProductId,decimal parQuantity = 0) { return null; }
        public virtual ReceiptViewModel ChangeQuantity(Guid parTerminalId, Guid parProductId, decimal parQuantity ) { return null; }
        public virtual ReceiptViewModel GetReciept(Guid parReceipt) { return null; }
        public virtual bool AddPayment( Guid parReceiptId, ReceiptPayment[] parPayment) { return false; }
        public virtual bool AddFiscalNumber(Guid parReceiptId,string parFiscalNumber) { return false; }
        public virtual bool ClearReceipt(Guid parTerminalId) { return false; }

        public virtual List<ProductViewModel> GetBags() { return null; }

        public virtual List<ProductCategory> GetAllCategories(Guid parTerminalId) { return null; }
        public virtual List<ProductCategory> GetCategoriesByParentId(Guid parTerminalId, Guid categoryId) { return null; }
        public virtual List<ProductViewModel> GetProductsByCategoryId(Guid parTerminalId, Guid categoryId) { return null; }
        public virtual List<ProductViewModel> GetProductsByName(string parName) { return null; }

        public virtual bool UpdateReceipt(ReceiptViewModel parReceipt) { return false; }
        public virtual TypeSend SendReceipt(Guid parReceipt) { return TypeSend.NotReady; }
        public virtual TypeSend GetStatusReceipt(Guid parReceipt)  { return TypeSend.NotReady; } 

        public virtual CustomerViewModel GetCustomerByBarCode(Guid parTerminalId, string parS) { return null; }
        public virtual CustomerViewModel GetCustomerByPhone(Guid parTerminalId,string parS) { return null; }

    }
}