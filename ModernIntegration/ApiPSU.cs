using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ModernIntegration.Models;
using ModernIntegration.ViewModels;
using ModernIntegration.Enums;
using SharedLib;
using ModelMID;
using System.Linq;
using Receipt = ModernIntegration.Models.Receipt;
using ModelMID.DB;
using ModernIntegration.Model;
using System.Threading.Tasks;
using Utils;
using Newtonsoft.Json;

namespace ModernIntegration
{
    public partial class ApiPSU:Api 
    {
        public BL Bl;
        public Dictionary<Guid, IdReceipt> Receipts = new Dictionary<Guid, IdReceipt>();
        public ApiPSU()
        {
            Bl = new BL();
            Global.OnReceiptCalculationComplete += (wareses, pIdReceipt) =>
            {
                FileLogger.WriteLogMessage($"OnReceiptCalculationComplete =>Start", eTypeLog.Expanded);
                foreach (var el in wareses)
                {
                    FileLogger.WriteLogMessage($"OnReceiptCalculationComplete Promotion=>{el.GetStrWaresReceiptPromotion.Trim()} \n{el.NameWares} - {el.Price} Quantity=> {el.Quantity} SumDiscount=>{el.SumDiscount}", eTypeLog.Expanded);
                }
                
                OnProductsChanged?.Invoke(wareses.Select(s => GetProductViewModel(s)), Global.GetTerminalIdByIdWorkplace(pIdReceipt.IdWorkplace));
                FileLogger.WriteLogMessage($"OnReceiptCalculationComplete =>End", eTypeLog.Expanded);
            };

            Global.OnSyncInfoCollected += (SyncInfo) =>
            {
                OnSyncInfoCollected?.Invoke(SyncInfo);
                FileLogger.WriteLogMessage($"OnSyncInfoCollected Status=>{SyncInfo.Status} StatusDescription=>{SyncInfo.StatusDescription}",eTypeLog.Expanded);
            };

            Global.OnStatusChanged += (Status) => OnStatusChanged?.Invoke(Status);

            Global.OnChangedStatusScale += (Status) => OnChangedStatusScale?.Invoke(Status);

            Global.OnClientChanged += (client, guid) =>
            {               
                OnCustomerChanged?.Invoke(GetCustomerViewModelByClient(client), Global.GetTerminalIdByIdWorkplace(guid));
                FileLogger.WriteLogMessage($"Client.Wallet=> {client.Wallet} SumBonus=>{client.SumBonus} ", eTypeLog.Expanded);
            };

            Global.OnClientWindows += (pTerminalId,pTypeWindows, pMessage) =>
             {
                 TerminalCustomWindowModel TCV=null;
                 if (pTypeWindows == eTypeWindows.LimitSales)
                 {
                     TCV = new TerminalCustomWindowModel()
                     {
                         TerminalId = Global.GetTerminalIdByIdWorkplace(pTerminalId),
                         CustomWindow = new CustomWindowModel()
                         {
                             Caption = "",
                             Text = pMessage,
                             AnswerRequired = false,
                             Type = CustomWindowInputType.Buttons,
                             Buttons = new List<CustomWindowButton>() { new CustomWindowButton() { ActionData = "Ok", DisplayName = "Ok" } }
                         }
                     };
                 }
                 OnShowCustomWindow?.Invoke(TCV);
                 string sTCV= JsonConvert.SerializeObject(TCV);
                 FileLogger.WriteLogMessage($"OnClientWindows => {pTypeWindows} TerminalId=>{pTerminalId}{Environment.NewLine} Message=> {pMessage} {Environment.NewLine} TCV=>{sTCV}",eTypeLog.Expanded);
             };
        
        }

        public override ProductViewModel AddProductByBarCode(Guid pTerminalId, string pBarCode, decimal pQuantity = 0)
        {
            ProductViewModel res = null;
            try
            {
                var CurReceipt = GetCurrentReceiptByTerminalId(pTerminalId);
                var RW = Bl.AddWaresBarCode(CurReceipt, pBarCode, pQuantity);
                if (RW != null)
                    res = GetProductViewModel(RW);
                FileLogger.WriteLogMessage($"ApiPSU.AddProductByBarCode =>(pTerminalId=>{pTerminalId},pBarCode=>{pBarCode},pQuantity={pQuantity})=>({res?.ToJSON()})", eTypeLog.Full);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.AddProductByBarCode Exception =>(pTerminalId=>{pTerminalId},pBarCode=>{pBarCode},pQuantity={pQuantity}) => ({res?.ToJSON()}){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.AddProductByBarCode", e);
            }
            return res;
        }

        public override ProductViewModel AddProductByProductId(Guid pTerminalId, Guid pProductId, decimal pQuantity = 0, decimal pPrice = 0)
        {
            ProductViewModel res = null;
            try
            {
                var CurReceipt = GetCurrentReceiptByTerminalId(pTerminalId);
                //var g = CurReceipt.ReceiptId;
                var WId = new IdReceiptWares { WaresId = pProductId };
                var RW = Bl.AddWaresCode(CurReceipt, WId.CodeWares, WId.CodeUnit, pQuantity, pPrice);
                //TODO: OnReceiptChanged?.Invoke(receipt,terminalId);
                if (RW != null)
                    res = GetProductViewModel(RW);
                FileLogger.WriteLogMessage($"ApiPSU.AddProductByProductId =>(pTerminalId=>{pTerminalId},pProductId=>{pProductId},pQuantity={pQuantity})=>({res?.ToJSON()})", eTypeLog.Full);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.AddProductByProductId Exception =>(pTerminalId=>{pTerminalId},pProductId=>{pProductId},pQuantity={pQuantity}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.AddProductByProductId", e);
            }
            return res;
        }

        public override ReceiptViewModel ChangeQuantity(Guid pTerminalId, Guid pProductId, decimal pQuantity)
        {
            ReceiptViewModel Res = null;
            try
            {
                var CurReceipt = GetCurrentReceiptByTerminalId(pTerminalId);
                var CurReceiptWares = new IdReceiptWares(CurReceipt, pProductId);

                if (Bl.ChangeQuantity(CurReceiptWares, pQuantity))
                    Res = GetReceiptViewModel(CurReceipt);
                FileLogger.WriteLogMessage($"ApiPSU.AddProductByProductId =>(pTerminalId=>{pTerminalId},pProductId=>{pProductId},pQuantity={pQuantity})=>({Res?.ToJSON()})", eTypeLog.Full);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.AddProductByProductId Exception =>(pTerminalId=>{pTerminalId},pProductId=>{pProductId},pQuantity={pQuantity}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.ChangeQuantity", e);
            }
            return Res;
        }

        public override ReceiptViewModel GetReciept(Guid pReceipt)
        {
            ReceiptViewModel Res = null;
            try
            {
                Res = GetReceiptViewModel(new IdReceipt(pReceipt));
                FileLogger.WriteLogMessage($"ApiPSU.GetReciept =>(pReceipt=>{pReceipt})=>({Res?.ToJSON()})", eTypeLog.Full);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.GetReciept Exception =>(pReceipt=>{pReceipt}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.GetReciept", e);
            }
            return Res;
        }    

        public override ReceiptViewModel GetRecieptByTerminalId(Guid pTerminalId, bool pIsDetail = false)
        {

            ReceiptViewModel Res = null;
            try
            {
                var receiptId = GetCurrentReceiptByTerminalId(pTerminalId);
                Res= GetReceiptViewModel(receiptId, pIsDetail);
                FileLogger.WriteLogMessage($"ApiPSU.GetRecieptByTerminalId =>(pTerminalId=>{pTerminalId},pIsDetail={pIsDetail})=>({Res?.ToJSON()})", eTypeLog.Full);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.GetRecieptByTerminalId Exception =>(pTerminalId=>{pTerminalId},pIsDetail={pIsDetail}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.GetRecieptByTerminalId", e);
            }
            return Res;
        }

        public override bool AddPayment(Guid pTerminalId, ReceiptPayment[] pPayment, Guid? pReceiptId = null)
        {
            bool res = false;
            try{
                IdReceipt receiptId = (pReceiptId != null && pReceiptId != Guid.Empty) ? new IdReceipt(pReceiptId.Value) : GetCurrentReceiptByTerminalId(pTerminalId);
                Bl.db.ReplacePayment(pPayment.Select(r => ReceiptPaymentToPayment(receiptId, r)));
                res= Bl.SetStateReceipt(receiptId, eStateReceipt.Pay);
                FileLogger.WriteLogMessage($"ApiPSU.AddPayment Exception =>( pTerminalId=>{pTerminalId},pPayment=> {pPayment},pReceiptId={pReceiptId}) => ({res})", eTypeLog.Full);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.AddPayment Exception =>( pTerminalId=>{pTerminalId},pPayment=> {pPayment},pReceiptId={pReceiptId}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.AddPayment", e);
            }
            return res;
        }

        public override bool AddFiscalNumber(Guid pTerminalId, string pFiscalNumber, Guid? pReceiptId = null)
        {
            bool res = false;
            try
            {
                IdReceipt receiptId = (pReceiptId != null && pReceiptId != Guid.Empty) ? new IdReceipt(pReceiptId.Value) : GetCurrentReceiptByTerminalId(pTerminalId);
                Bl.UpdateReceiptFiscalNumber(receiptId, pFiscalNumber);
                ClearReceiptByReceiptId(receiptId);
                res = true;
                FileLogger.WriteLogMessage($"ApiPSU.AddFiscalNumber =>( pTerminalId=>{pTerminalId},pFiscalNumber=> {pFiscalNumber},pReceiptId={pReceiptId}) => ({res})", eTypeLog.Full);

            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.AddFiscalNumber Exception =>( pTerminalId=>{pTerminalId},pFiscalNumber=> {pFiscalNumber},pReceiptId={pReceiptId}) => ({res}){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.AddFiscalNumber", e);
            }
            return res;
        }

        public override bool ClearReceipt(Guid pTerminalId, Guid? pReceiptId = null)
        {
            bool res = false;
            try
            {
                IdReceipt receiptId = (pReceiptId != null && pReceiptId != Guid.Empty) ? new IdReceipt(pReceiptId.Value) : GetCurrentReceiptByTerminalId(pTerminalId);
                Bl.SetStateReceipt(receiptId, eStateReceipt.Canceled);
                Receipts[pTerminalId] = null;
                res = true;
                FileLogger.WriteLogMessage($"ApiPSU.ClearReceipt =>( pTerminalId=>{pTerminalId},pReceiptId={pReceiptId}) => ({res})", eTypeLog.Full);

            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.ClearReceipt Exception =>( pTerminalId=>{pTerminalId},pReceiptId={pReceiptId}) => ({res}){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.ClearReceipt", e);
            }
            return res;
        }

        public override IEnumerable<ProductViewModel> GetBags()
        {
            return Bl.db.GetBags().Select(r => GetProductViewModel(r));
        }

        public override List<ProductCategory> GetAllCategories(Guid pTerminalId)
        {
            var Res = new List<ProductCategory>();
            try
            {
                var wr = Bl.db.GetFastGroup(Global.CodeWarehouse);
                if (wr != null)
                    foreach (var el in wr)
                        Res.Add(GetProductCategory(el));
                FileLogger.WriteLogMessage($"ApiPSU.GetAllCategories =>( pTerminalId=>{pTerminalId}) => ({Res?.ToJSON()})", eTypeLog.Full);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.GetAllCategories Exception =>(pTerminalId=>{pTerminalId}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.GetAllCategories", e);
            }
            return Res;
        }

        public override List<ProductCategory> GetCategoriesByParentId(Guid pTerminalId, Guid categoryId) {throw new NotImplementedException();}

        public override List<ProductViewModel> GetProductsByCategoryId(Guid pTerminalId, Guid pCategoryId)
        {
            var Res = new List<ProductViewModel>();
            try
            {
                var ct = new FastGroup { FastGroupId = pCategoryId };
                var wr = Bl.db.GetWaresFromFastGroup(ct.CodeFastGroup);
                if (wr != null)
                    foreach (var el in wr)
                        Res.Add(GetProductViewModel(el));
                FileLogger.WriteLogMessage($"ApiPSU.GetProductsByCategoryId =>( pTerminalId=>{pTerminalId},pCategoryId=>{pCategoryId}) => ({Res?.ToJSON()})", eTypeLog.Full);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.GetProductsByCategoryId Exception =>(pTerminalId=>{pTerminalId},pCategoryId=>{pCategoryId}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.GetProductsByCategoryId", e);
            }
            return Res;
        }

        public override IEnumerable<ProductViewModel> GetProductsByName(Guid pTerminalId, string pName, int pPageNumber = 0, bool pExcludeWeightProduct = false, Guid? pCategoryId = null, int pLimit = 10)
        {
            IEnumerable<ProductViewModel> Res = null;
            try
            {
                FastGroup fastGroup = (pCategoryId == null ? new FastGroup() : new FastGroup(pCategoryId.Value));

                var receiptId = GetCurrentReceiptByTerminalId(pTerminalId);
                //int Limit = 10;
                var res = Bl.GetProductsByName(receiptId, pName.Replace(' ', '%').Trim(), pPageNumber * pLimit, pLimit, fastGroup.CodeFastGroup);
                if (res != null)
                    Res= res.Select(r => (GetProductViewModel(r)));
                FileLogger.WriteLogMessage($"ApiPSU.GetProductsByName =>( pTerminalId=>{pTerminalId},pName=>{pName},pPageNumber=>{pPageNumber},pExcludeWeightProduct=>{pExcludeWeightProduct},pCategoryId=>{pCategoryId},pLimit={pLimit}) => ({Res?.ToJSON()})", eTypeLog.Full);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.GetProductsByName Exception =>( pTerminalId=>{pTerminalId},pName=>{pName},pPageNumber=>{pPageNumber},pExcludeWeightProduct=>{pExcludeWeightProduct},pCategoryId=>{pCategoryId},pLimit={pLimit}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.GetProductsByName", e);
            }
            return Res;
        }

        //Зберігає
        public override bool UpdateReceipt(ReceiptViewModel pReceipt)
        {
            bool Res = false;
            try
            {
                if (pReceipt != null && pReceipt.ReceiptEvents != null)
                {
                    var RE = pReceipt.ReceiptEvents.Select(r => GetReceiptEvent(r));
                    if (RE != null)
                        Bl.SaveReceiptEvents(RE);
                    if (pReceipt.ReceiptItems != null)
                    {
                        var WR = pReceipt.ReceiptItems.Where(r => r.Excises != null && r.Excises.Count() > 0).Select(r => GetReceiptWaresFromReceiptItem(new IdReceipt(pReceipt.Id), r));
                        if (WR != null && WR.Count() > 0)
                            Res=Bl.UpdateExciseStamp(WR);
                    }
                }
                FileLogger.WriteLogMessage($"ApiPSU.UpdateReceipt =>(pTerminalId=>{pReceipt?.ToJSON()}) => ({Res})", eTypeLog.Full);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.UpdateReceipt Exception =>(pTerminalId=>{pReceipt?.ToJSON()}) => ({Res}){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.UpdateReceipt", e);
            }
            return Res;
        }

        public override TypeSend SendReceipt(Guid pReceipt)
        {
            Bl.SendReceiptTo1C(new IdReceipt(pReceipt));
            return TypeSend.NotReady;
        }

        public override TypeSend GetStatusReceipt(Guid pReceipt) { throw new NotImplementedException();}

        public override CustomerViewModel GetCustomerByBarCode(Guid pTerminalId, string pS)
        {
            CustomerViewModel Res = null;
            try
            {
                var curReceipt = GetCurrentReceiptByTerminalId(pTerminalId);
                var CM = Bl.GetClientByBarCode(curReceipt, pS);
                if (CM != null)
                {
                    _ = Bl.GetBonusAsync(CM, curReceipt.IdWorkplace);
                    Res = GetCustomerViewModelByClient(CM);
                }
                FileLogger.WriteLogMessage($"ApiPSU.GetCustomerByBarCode =>( pTerminalId=>{pTerminalId},pS=>{pS}) => ({Res?.ToJSON()})", eTypeLog.Full);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.GetCustomerByBarCode Exception =>( pTerminalId=>{pTerminalId},pS=>{pS}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.GetCustomerByBarCode", e);
            }
            return Res;
        }

        public override CustomerViewModel GetCustomerByPhone(Guid pTerminalId, string pPhone)
        {
            CustomerViewModel Res = null;
            try
            {
                var curReceipt = GetCurrentReceiptByTerminalId(pTerminalId);
                var CM = Bl.GetClientByPhone(GetCurrentReceiptByTerminalId(pTerminalId), pPhone);
                if (CM != null)
                {
                    _ = Bl.GetBonusAsync(CM, curReceipt.IdWorkplace);
                    Res = GetCustomerViewModelByClient(CM);
                }
                FileLogger.WriteLogMessage($"ApiPSU.GetCustomerByPhone =>( pTerminalId=>{pTerminalId},pPhone=>{pPhone}) => ({Res?.ToJSON()})", eTypeLog.Full);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.GetCustomerByPhone Exception =>( pTerminalId=>{pTerminalId},pPhone=>{pPhone}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.GetCustomerByPhone", e);
            }
            return Res;
        }
       
        /// <summary>
        /// Terminalses the specified terminals.
        /// </summary>
        /// <param name="terminals">The terminals.</param>
        /// <returns></returns>
        public override bool Terminals(List<Terminal> terminals)
        {
            return Bl.UpdateWorkPlace(terminals.Select(r => new WorkPlace() { IdWorkplace = r.CustomerId, Name = r.DisplayName, TerminalGUID = r.Id }));             
        }

        public override bool MoveSessionToAnotherTerminal(Guid firstTerminalId, Guid secondTerminalId)
        {
            //Якщо чек незакрито на терміналі куди переносити тоді помилка.
            if (Receipts.ContainsKey(secondTerminalId) && Receipts[secondTerminalId] != null)
                return false;
            var idReceipt = Bl.GetNewIdReceipt(Global.GetIdWorkplaceByTerminalId(secondTerminalId) );
            if (Bl.MoveReceipt(GetCurrentReceiptByTerminalId(firstTerminalId), idReceipt))
            {
                Receipts[secondTerminalId] = new ModelMID.Receipt(idReceipt);
                return true;
            }
            else
                return false;
        }
      
        public override bool UpdateProductWeight(string parData, int parWeight, Guid parWares, TypeSaveWeight parTypeSaveWeight)
        {
            return Bl.InsertWeight(parData, parWeight, parWares, parTypeSaveWeight);
        }

        public override async Task RequestSyncInfo(bool pIsFull = false)
        {
            // TODO: check status
            try
            {
                var res = await Bl.SyncDataAsync(pIsFull);                
            }
            catch (Exception ex)
            {
                var SyncInfo = new SyncInformation() { Status = eSyncStatus.SyncFinishedError, StatusDescription = ex.Message, SyncData = ex };
                FileLogger.WriteLogMessage($"OnSyncInfoCollected Status=>{SyncInfo.Status} StatusDescription=>{SyncInfo.StatusDescription}", eTypeLog.Expanded);
                OnSyncInfoCollected?.Invoke(SyncInfo);
            }
        }

        public override IEnumerable<ProductViewModel> GetProduct(Guid pTerminalId)
        {
            IEnumerable<ProductViewModel> Res = null;
            try
            {
                var receiptId = GetCurrentReceiptByTerminalId(pTerminalId);

                var res = Bl.GetWaresReceipt(new IdReceipt(receiptId));
                if (res != null)
                    Res = res.Select(r => (GetProductViewModel(r)));
                FileLogger.WriteLogMessage($"ApiPSU.GetProduct =>( pTerminalId=>{pTerminalId}) => ({Res?.ToJSON()})", eTypeLog.Full);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.GetProduct Exception =>( pTerminalId=>{pTerminalId}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.GetProduct", e);
            }
            return Res;
        }

        public override ReceiptViewModel GetNoFinishReceipt(Guid pTerminalId)
        {
            ReceiptViewModel Res = null;
            try
            {
                var receipt = Bl.GetLastReceipt(Global.GetIdWorkplaceByTerminalId(pTerminalId));
                if (receipt != null && receipt.StateReceipt == eStateReceipt.Prepare)
                {
                    Receipts[pTerminalId] = new ModelMID.Receipt(receipt);
                    Res = GetReceiptViewModel(receipt);
                }
                FileLogger.WriteLogMessage($"ApiPSU.GetNoFinishReceipt =>( pTerminalId=>{pTerminalId}) => ({Res?.ToJSON()})", eTypeLog.Full);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.GetNoFinishReceipt Exception =>( pTerminalId=>{pTerminalId}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.GetNoFinishReceipt", e);
            }
            return Res;
        }

        public override IEnumerable<ReceiptViewModel> GetReceipts(DateTime pStartDate, DateTime pFinishDate, Guid? pTerminalId = null)
        {
            IEnumerable<ReceiptViewModel> Res = null;
            try
            {
                int IdWorkplace = 0;
                if (pTerminalId != null)
                    IdWorkplace = Global.GetIdWorkplaceByTerminalId(pTerminalId.Value);

                var res = Bl.GetReceipts(pStartDate, pFinishDate, IdWorkplace);
                Res = res.Select(r => GetReceiptViewModel(r));
                FileLogger.WriteLogMessage($"ApiPSU.GetReceipts =>( pTerminalId=>{pTerminalId},pStartDate=>{pStartDate},pFinishDate=>{pFinishDate}) => ({Res?.Count()})", eTypeLog.Full);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.GetReceipts Exception =>( pTerminalId=>{pTerminalId}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.GetReceipts", e);
            }
            return Res;
        }

        public override ReceiptViewModel GetReceiptByNumber(Guid pTerminalId, string pFiscalNumber)
        {
             var IdWorkplace = Global.GetIdWorkplaceByTerminalId(pTerminalId);
             var res = Bl.GetReceiptByFiscalNumber(IdWorkplace, pFiscalNumber);             
             return GetReceiptViewModel(res);
        }

        public override bool RefundReceipt(Guid pTerminalId, RefundReceiptViewModel pReceipt)
        {
            var receipt = ReceiptViewModelToReceipt(pTerminalId,pReceipt);
            receipt.UserCreate = Bl.GetUserIdbyWorkPlace(receipt.IdWorkplace);
            return Bl.SaveReceipt(receipt);
        }

        public override void CloseDb()
        { 
            Bl.CloseDB();
        }

        public override void StartWork(Guid pTerminalId, string pBarCodeCashier)
        {
           var IdWorkplace = Global.GetIdWorkplaceByTerminalId(pTerminalId);
           Bl.StartWork(IdWorkplace, pBarCodeCashier);
        }

        public override void StopWork(Guid pTerminalId)
        {
            var IdWorkplace = Global.GetIdWorkplaceByTerminalId(pTerminalId);
            Bl.StoptWork(IdWorkplace);
        }

        public override bool SetWeight(Guid pTerminalId, Guid pProductId, decimal pWeight)
        {
            var CurReceipt = GetCurrentReceiptByTerminalId(pTerminalId);
            var RW = new ReceiptWares(CurReceipt, pProductId);
            RW.FixWeight = pWeight / 1000m;
            return Bl.FixWeight(RW);
        }

        public override IEnumerable<QRDefinitions> GetQR(Guid pTerminalId)
        {
            List<QRDefinitions> Res = new List<QRDefinitions>();
            try
            {
                var QR = Bl.GetQR(GetCurrentReceiptByTerminalId(pTerminalId));
                foreach (var el in QR)
                {
                    foreach (var qr in el.Qr.Split(','))
                    {
                        Res.Add(new QRDefinitions() { Caption = el.Name, Data = qr });
                    }
                }
                FileLogger.WriteLogMessage($"ApiPSU.GetQR =>( pTerminalId=>{pTerminalId}) => ({Res?.ToJSON()})", eTypeLog.Full);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"ApiPSU.GetQR Exception =>( pTerminalId=>{pTerminalId}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                throw new Exception("ApiPSU.GetQR", e);
            }
            return Res;
        }

        public override bool ProcessCustomWindowResult(Guid pTerminalId, string pCustomWindowResult)
        {
            FileLogger.WriteLogMessage($"ApiPSU.ProcessCustomWindowResult( pTerminalId=>{pTerminalId}, pCustomWindowResult=>{pCustomWindowResult})",eTypeLog.Expanded);
            return true;
        }
    }
}