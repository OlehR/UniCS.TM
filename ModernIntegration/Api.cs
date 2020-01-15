using ModelMID;
using ModernIntegration.Model;
using ModernIntegration.Models;
using ModernIntegration.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

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
        //public bool CreateReceipt(Guid parTerminalId, Guid parReceipt) { throw new NotImplementedException(); }

        public virtual ProductViewModel AddProductByBarCode(Guid parTerminalId,string parS, decimal parQuantity = 0) { throw new NotImplementedException(); }
        public virtual ProductViewModel AddProductByProductId(Guid parTerminalId, Guid paparProductId,decimal parQuantity = 0) { throw new NotImplementedException(); }
        public virtual ReceiptViewModel ChangeQuantity(Guid parTerminalId, Guid parProductId, decimal parQuantity ) { throw new NotImplementedException(); }
        public virtual ReceiptViewModel GetReciept(Guid parReceipt) { throw new NotImplementedException(); }
        public virtual IEnumerable<ProductViewModel> GetProduct(Guid parTerminalId) { throw new NotImplementedException(); }
        public virtual ReceiptViewModel GetRecieptByTerminalId(Guid parTerminalId) { throw new NotImplementedException(); }
        public virtual bool AddPayment( Guid parTerminalId, ReceiptPayment[] parPayment) { throw new NotImplementedException(); }
        public virtual bool AddFiscalNumber(Guid parTerminalId, string parFiscalNumber) { throw new NotImplementedException(); }
        public virtual bool ClearReceipt(Guid parTerminalId) { throw new NotImplementedException(); }
        public virtual IEnumerable<ProductViewModel> GetBags() { throw new NotImplementedException(); }
        public virtual List<ProductCategory> GetAllCategories(Guid parTerminalId) { throw new NotImplementedException(); }
        public virtual List<ProductCategory> GetCategoriesByParentId(Guid parTerminalId, Guid categoryId) { throw new NotImplementedException(); }
        public virtual List<ProductViewModel> GetProductsByCategoryId(Guid parTerminalId, Guid categoryId) { throw new NotImplementedException(); }
        public virtual IEnumerable<ProductViewModel> GetProductsByName(Guid parTerminalId, string parName, int pageNumber = 0, bool excludeWeightProduct = false, Guid? categoryId = null, int parLimit = 10) { throw new NotImplementedException(); }

        public virtual bool UpdateReceipt(ReceiptViewModel parReceipt) { throw new NotImplementedException(); }
        public virtual bool RefundReceipt(RefundReceiptViewModel parReceipt) { throw new NotImplementedException(); }
        public virtual TypeSend SendReceipt(Guid parReceipt) { throw new NotImplementedException(); }
        public virtual TypeSend GetStatusReceipt(Guid parReceipt)  { throw new NotImplementedException(); } 
        public virtual CustomerViewModel GetCustomerByBarCode(Guid parTerminalId, string parS) { throw new NotImplementedException(); }
        public virtual CustomerViewModel GetCustomerByPhone(Guid parTerminalId,string parS) { throw new NotImplementedException(); }
        public virtual bool Terminals(List<Terminal> terminals) { throw new NotImplementedException(); }

        public virtual bool MoveSessionToAnotherTerminal(Guid firstTerminalId, Guid secondTerminalId) { throw new NotImplementedException(); }

        public virtual Task RequestSyncInfo(bool parIsFull=false){ throw new NotImplementedException(); }

        public virtual bool UpdateProductWeight(string parData, int parWeight, Guid? parWares = null) { throw new NotImplementedException(); }

        public virtual Status GetCurentStatus() { throw new NotImplementedException(); }

        public virtual ReceiptViewModel GetNoFinishReceipt(Guid parTerminalId) { throw new NotImplementedException(); }
        public virtual IEnumerable<ReceiptViewModel> GetReceipts(DateTime parStartDate, DateTime parFinishDate, Guid? parTerminalId = null) 
            { throw new NotImplementedException(); }

        public Action<SyncInformation> OnSyncInfoCollected { get; set; }



        public Action<IEnumerable<ProductViewModel>, Guid> OnProductsChanged { get; set; }

        public Action<Status> OnStatusChanged { get; set; }

       
    }
    
}
