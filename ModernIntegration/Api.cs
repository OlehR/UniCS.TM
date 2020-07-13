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
        public virtual bool AddPayment( Guid parTerminalId, ReceiptPayment[] parPayment,Guid? parReceiptId= null) { throw new NotImplementedException(); }
        public virtual bool AddFiscalNumber(Guid parTerminalId, string parFiscalNumber, Guid? parReceiptId = null) { throw new NotImplementedException(); }
        public virtual bool ClearReceipt(Guid parTerminalId, Guid? parReceiptId = null) { throw new NotImplementedException(); }
        public virtual IEnumerable<ProductViewModel> GetBags() { throw new NotImplementedException(); }
        public virtual List<ProductCategory> GetAllCategories(Guid parTerminalId) { throw new NotImplementedException(); }
        public virtual List<ProductCategory> GetCategoriesByParentId(Guid parTerminalId, Guid categoryId) { throw new NotImplementedException(); }
        public virtual List<ProductViewModel> GetProductsByCategoryId(Guid parTerminalId, Guid categoryId) { throw new NotImplementedException(); }
        public virtual IEnumerable<ProductViewModel> GetProductsByName(Guid parTerminalId, string parName, int pageNumber = 0, bool excludeWeightProduct = false, Guid? categoryId = null, int parLimit = 10) { throw new NotImplementedException(); }

        public virtual bool UpdateReceipt(ReceiptViewModel parReceipt) { throw new NotImplementedException(); }
        public virtual bool RefundReceipt(Guid parTerminalId,RefundReceiptViewModel parReceipt) { throw new NotImplementedException(); }
        public virtual TypeSend SendReceipt(Guid parReceipt) { throw new NotImplementedException(); }
        public virtual TypeSend GetStatusReceipt(Guid parReceipt)  { throw new NotImplementedException(); } 
        public virtual CustomerViewModel GetCustomerByBarCode(Guid parTerminalId, string parS) { throw new NotImplementedException(); }
        public virtual CustomerViewModel GetCustomerByPhone(Guid parTerminalId,string parS) { throw new NotImplementedException(); }
        public virtual bool Terminals(List<Terminal> terminals) { throw new NotImplementedException(); }

        public virtual bool MoveSessionToAnotherTerminal(Guid firstTerminalId, Guid secondTerminalId) { throw new NotImplementedException(); }

        public virtual Task RequestSyncInfo(bool parIsFull=false){ throw new NotImplementedException(); }

        public virtual bool UpdateProductWeight(string parData, int parWeight, Guid parWares, TypeSaveWeight parTypeSaveWeight) { throw new NotImplementedException(); }

        public virtual Status GetCurentStatus() { throw new NotImplementedException(); }

        public virtual ReceiptViewModel GetNoFinishReceipt(Guid parTerminalId) { throw new NotImplementedException(); }
        public virtual IEnumerable<ReceiptViewModel> GetReceipts(DateTime parStartDate, DateTime parFinishDate, Guid? parTerminalId = null) 
            { throw new NotImplementedException(); }

        public virtual ReceiptViewModel GetReceiptByNumber(Guid pTerminalId, string pFiscalNumber)
        { throw new NotImplementedException(); }

        /// <summary>
        ///  Зберігає Фактичну вагу для останнього товару в чеку.
        /// </summary>
        /// <param name="pTerminalId"></param>
        /// <param name="pWeight"></param>
        public virtual void  SaveWeight(Guid pTerminalId, double pWeight)
        { throw new NotImplementedException(); }

        public virtual void CloseDb()
        { throw new NotImplementedException(); }

        //Блок роботи з контрольними вагами.

        /// <summary>
        /// Отримуємо "середню" вагу попередньої позиції
        /// Викликається після зчитаного штрихкода, чи добавленн товару іншим чином до добаввлення його в чек.(та перед оплатою) 
        /// Зразу необхідно викликати SaveWeight(); а вже після цього метод для добавлення нової позиції
        /// а коли прийде інформація про нову позицію - StartWeightNewGoogs
        /// </summary>
        public virtual double GetMidlWeight()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Початок провірки ваги нового товару має викликатись після GetMidlWeight().
        /// </summary>
        /// <param name="pWeight">Ваги 1 позиції</param>
        /// <param name="pCount">кількість позицій при видалені - </param>
        public virtual void StartWeightNewGoogs(IEnumerable<WeightInfo> pWeight, int pCount)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        ///  "Фіксуємо" невірну вагу можливо тільки в стані eStateScale.BadWeight || eStateScale.WaitGoods
        /// </summary>
        /// <returns>Якщо зуміли зафіксувати</returns>
        public virtual bool FixedWeight()
        {
            throw new NotImplementedException();        
        }

        /// <summary>
        ///  Переводимо вагу в стан очистіть вагу.
        /// </summary>
        /// <returns></returns>
        public virtual bool WaitClearScale()
        {
            throw new NotImplementedException();
        }


        public virtual void StartWork(Guid pTerminalId, int pCodeCashier)
        {
            throw new NotImplementedException();
        }
        public virtual void StopWork(Guid pTerminalId)
        {
            throw new NotImplementedException();
        }
        public Action<SyncInformation> OnSyncInfoCollected { get; set; }

        public Action<IEnumerable<ProductViewModel>, Guid> OnProductsChanged { get; set; }

        public Action<Status> OnStatusChanged { get; set; }

        /// <summary>
        /// TMP!!! Мають бути модерновські статуси
        /// </summary>
        public Action<eStateScale> OnChangedStatusScale { get; set; }
        public Action<CustomerViewModel, Guid> OnCustomerChanged { get; set; }


    }


   
}
