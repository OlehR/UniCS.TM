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
        //public bool CreateReceipt(Guid parTerminalId, Guid parReceipt) { return false; }
     
        public virtual ProductViewModel AddProductByBarCode(Guid parTerminalId,string parS, decimal parQuantity = 0) { return null; }
        public virtual ProductViewModel AddProductByProductId(Guid parTerminalId, Guid paparProductId,decimal parQuantity = 0) { return null; }
        public virtual ReceiptViewModel ChangeQuantity(Guid parTerminalId, Guid parProductId, decimal parQuantity ) { return null; }
        public virtual ReceiptViewModel GetReciept(Guid parReceipt) { return null; }
        public virtual IEnumerable<ProductViewModel> GetProduct(Guid parTerminalId) { return null; }
        public virtual ReceiptViewModel GetRecieptByTerminalId(Guid parTerminalId) { return null; }
        public virtual bool AddPayment( Guid parTerminalId, ReceiptPayment[] parPayment) { return false; }
        public virtual bool AddFiscalNumber(Guid parTerminalId, string parFiscalNumber) { return false; }
        public virtual bool ClearReceipt(Guid parTerminalId) { return false; }

        public virtual IEnumerable<ProductViewModel> GetBags() { return null; }

        public virtual List<ProductCategory> GetAllCategories(Guid parTerminalId) { return null; }
        public virtual List<ProductCategory> GetCategoriesByParentId(Guid parTerminalId, Guid categoryId) { return null; }
        public virtual List<ProductViewModel> GetProductsByCategoryId(Guid parTerminalId, Guid categoryId) { return null; }
        public virtual IEnumerable<ProductViewModel> GetProductsByName(Guid parTerminalId, string parName, int pageNumber = 0, bool excludeWeightProduct = false, Guid? categoryId = null, int parLimit = 10) { return null; }

        public virtual bool UpdateReceipt(ReceiptViewModel parReceipt) { return false; }
        public virtual bool RefundReceipt(RefundReceiptViewModel parReceipt) { return false; }
        public virtual TypeSend SendReceipt(Guid parReceipt) { return TypeSend.NotReady; }
        public virtual TypeSend GetStatusReceipt(Guid parReceipt)  { return TypeSend.NotReady; } 

        public virtual CustomerViewModel GetCustomerByBarCode(Guid parTerminalId, string parS) { return null; }
        public virtual CustomerViewModel GetCustomerByPhone(Guid parTerminalId,string parS) { return null; }

        public virtual bool Terminals(List<Terminal> terminals) { return false; }

        public virtual bool MoveSessionToAnotherTerminal(Guid firstTerminalId, Guid secondTerminalId) { return false; }

        public virtual Task RequestSyncInfo(bool parIsFull=false){ return null; }

        public virtual bool UpdateProductWeight(string parS, int weight) { return false; }

        public virtual Status GetCurentStatus() { throw new NotImplementedException(); }

        public virtual ReceiptViewModel GetNoFinishReceipt(Guid parTerminalId) { throw new NotImplementedException(); }

        public Action<SyncInformation> OnSyncInfoCollected { get; set; }

        public Action<IEnumerable<ProductViewModel>, Guid> OnProductsChanged { get; set; }

        public Action<Status> OnStatusChanged { get; set; }
    }
    
}
