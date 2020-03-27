using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SharedLib
{

    public class BL
    {
        public WDB_SQLite db;

        public DataSync ds;
        
        //public Action<IEnumerable<ReceiptWares>, Guid> OnReceiptCalculationComplete { get; set; }

        /// <summary>
        /// Для швидкого пошуку 
        /// </summary>
        SortedList<Guid, int> WorkId;


        public BL(bool  pIsUseOldDB=false)
        {
            db = new WDB_SQLite("",false,default(DateTime), pIsUseOldDB);
            ds = new DataSync(this);
            WorkId = new SortedList<Guid, int>();
         //   Global.OnReceiptCalculationComplete = (wareses, guid) => OnReceiptCalculationComplete?.Invoke(wareses, guid);
            

        }
        public ReceiptWares AddReceiptWares(ReceiptWares parW)
        {
            var Quantity = db.GetCountWares(parW);
            parW.QuantityOld = Quantity;
            parW.Quantity += Quantity;

            if (Quantity > 0)
                db.UpdateQuantityWares(parW);
            else
                db.AddWares(parW);

            if (ModelMID.Global.RecalcPriceOnLine)
                db.RecalcPriceAsync(parW);
            return parW;
        }

        public bool AddReceipt(IdReceipt parReceipt)
        {
            var receipt = new Receipt(parReceipt);
            return db.AddReceipt(receipt);
        }
        public bool AddReceipt(Receipt parReceipt)
        {
            return db.AddReceipt(parReceipt);
        }

        public int GetIdWorkplaceByTerminalId(Guid parTerminalId)
        {
            return Global.GetIdWorkplaceByTerminalId(parTerminalId);
        }
        public IdReceipt GetNewIdReceipt(Guid parTerminalId, int parCodePeriod = 0)
        {
            var idReceip = new IdReceipt() { IdWorkplace = GetIdWorkplaceByTerminalId(parTerminalId), CodePeriod = parCodePeriod };
            return db.GetNewReceipt(idReceip);
        }

        public Receipt GetLastReceipt(Guid parTerminalId, int parCodePeriod = 0)
        {
            var idReceip = new IdReceipt() { IdWorkplace = GetIdWorkplaceByTerminalId(parTerminalId), CodePeriod = (parCodePeriod == 0? Global.GetCodePeriod():parCodePeriod) };
            return db.GetLastReceipt(idReceip);
        }



        public bool UpdateReceiptFiscalNumber(IdReceipt receiptId, string parFiscalNumber)
        {
            var receipt = new Receipt(receiptId);
            receipt.NumberReceipt = parFiscalNumber;
            receipt.StateReceipt = eStateReceipt.Print;
            //db.RecalcPrice(receiptId);
            db.CloseReceipt(receipt);
            return true;
        }

        public ReceiptWares AddWaresBarCode(IdReceipt parReceipt, string parBarCode, decimal parQuantity = 0)
        {

            var w = parBarCode.Trim().Length >= 8 ? db.FindWares(parBarCode) : db.FindWares(null, null, 0, 0, 0, Convert.ToInt32(parBarCode));

            //ReceiptWares W = null;
            if (w == null || w.Count() == 0) // Якщо не знайшли спробуем по ваговим і штучним штрихкодам.          
            {
                foreach (var el in Global.CustomerBarCode.Where(el => el.KindBarCode == eKindBarCode.EAN13 /*&& (el.TypeBarCode == eTypeBarCode.WaresWeight || el.TypeBarCode == eTypeBarCode.WaresUnit )*/))
                {
                    w = null;
                    if (el.Prefix.Equals(parBarCode.Substring(0, el.Prefix.Length)))
                    {
                        if(el.KindBarCode==eKindBarCode.EAN13 && parBarCode.Length!=13)
                            break;

                        int varCode = Convert.ToInt32(parBarCode.Substring(el.Prefix.Length, el.LenghtCode));
                        int varValue = Convert.ToInt32(parBarCode.Substring(el.Prefix.Length + el.LenghtCode, el.LenghtQuantity));
                        switch (el.TypeCode)
                        {
                            case eTypeCode.Article:
                                w = db.FindWares(null, null, 0, 0, 0, varCode);
                                break;
                            case eTypeCode.Code:
                                w = db.FindWares(null, null, varCode);
                                break;
                            case eTypeCode.PercentDiscount:
                                ds.CheckDiscountBarCodeAsync(parReceipt, parBarCode, varCode);
                                return new ReceiptWares(parReceipt);                                
                            default:
                                break;
                        }
                        if (parQuantity > 0 && w != null && w.Count() == 1) //Знайшли що треба
                        {
                            //parQuantity = (w.First().CodeUnit == Global.WeightCodeUnit ? varValue / 1000m : varValue);
                            parQuantity = varValue;
                            break;
                        }
                    }
                }

            }

            if (w == null || w.Count() != 1)
                return null;
            var W = w.First();
            if (parQuantity == 0)
                return W;
            if (W.Price == 0)//Якщо немає ціни на товар !!!!TMP Краще обробляти на GUI буде пізніше
                return null;
            W.SetIdReceipt(parReceipt);
            W.Quantity = (W.CodeUnit == Global.WeightCodeUnit ? parQuantity/1000m : parQuantity);// Вага приходить в грамах
            return AddReceiptWares(W);
        }

        public ReceiptWares AddWaresCode(IdReceipt parReceipt, Guid parProductId, decimal parQuantity = 0)
        {
            int CodeWares = 0;
            if (int.TryParse(parProductId.ToString().Substring(24), out CodeWares))
            {
                var WId = new IdReceiptWares { WaresId = parProductId };

                var w = db.FindWares(null, null, WId.CodeWares, WId.CodeUnit);
                if (w.Count() == 1)
                {
                    var W = w.First();
                    if (parQuantity == 0)
                        return W;
                    W.SetIdReceipt(parReceipt);
                    W.Quantity = (W.CodeUnit==Global.WeightCodeUnit? parQuantity/1000m : parQuantity);//Хак для вагового товару Який приходить в грамах.
                    return AddReceiptWares(W);
                }
            }
            return null;
        }

        public IEnumerable<ReceiptWares> ViewReceiptWares(IdReceipt parIdReceipt)
        {

            var Res = db.ViewReceiptWares(parIdReceipt);
            //var El = Res.First();
            return Res;

        }
        public bool ChangeQuantity(IdReceiptWares parReceiptWaresId, decimal parQuantity)
        {
            var res = false;
            //var W = db.FindWares(null, null, parReceiptWaresId.CodeWares, parReceiptWaresId.CodeUnit);
           // if (W.Count() == 1)
            //{
                if (parQuantity == 0)
                    db.DeleteReceiptWares(parReceiptWaresId);
                else
                {
                var w = new ReceiptWares(parReceiptWaresId);
                    //w.SetIdReceiptWares();
                    w.Quantity = parQuantity;
                res=db.UpdateQuantityWares(w);
                }
                if (ModelMID.Global.RecalcPriceOnLine)
                    db.RecalcPriceAsync(parReceiptWaresId);

           // }
            return res;

        }
        public Receipt GetReceiptHead(IdReceipt idReceipt, bool parWithDetail = false)
        {
            DateTime Ldc = DateTime.ParseExact(idReceipt.CodePeriod.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
            if (Ldc == DateTime.Now.Date)
             return db.ViewReceipt(idReceipt, parWithDetail);

            var ldb = new WDB_SQLite(null, false, Ldc);
            return ldb.ViewReceipt(idReceipt, parWithDetail);



        }

        public Client GetClientByBarCode(IdReceipt idReceipt, string parBarCode)
        {
            var r = db.FindClient(parBarCode);
            if (r.Count() == 1)
            {
                var client = r.First();
                UpdateClientInReceipt(idReceipt, client);
               
                return client;
            }
            return null;
        }

        public Client GetClientByPhone(IdReceipt idReceipt, string parPhone)
        {
            var r = db.FindClient(null, parPhone);
            if (r.Count() == 1)
            {
                var client = r.First();
                UpdateClientInReceipt(idReceipt, client);
                return client;
            }
            return null;


        }

        private void UpdateClientInReceipt(IdReceipt idReceipt, Client parClient)
        {
            var RH = GetReceiptHead(idReceipt);
            RH.CodeClient = parClient.CodeClient;
            RH.PercentDiscount = parClient.PersentDiscount;
            db.ReplaceReceipt(RH);
            if (Global.RecalcPriceOnLine)
                db.RecalcPriceAsync(new IdReceiptWares(idReceipt));
            _ = ds.GetBonusAsync(parClient, Global.GetTerminalIdByIdWorkplace(idReceipt.IdWorkplace));
        }


        public IEnumerable<ReceiptWares> GetProductsByName(IdReceipt parReceipt, string parName, int parOffSet = -1, int parLimit = 10, int parCodeFastGroup = 0)
        {
            parName = parName.Trim();
            // Якщо пошук по штрихкоду і назва похожа на штрихкод або артикул
            if (!string.IsNullOrEmpty(parName))
            {
                var Reg = new Regex(@"^[0-9]{4,13}$");
                if (Reg.IsMatch(parName))
                {
                    if (parName.Length >= 8)
                    {
                        var w = AddWaresBarCode(parReceipt, parName);
                        if (w != null)
                            return new List<ReceiptWares> { w };
                    }
                    else
                    {
                        var ww = db.FindWares(null, null, 0, 0, 0, Convert.ToInt32(parName));
                        if (ww.Count() > 0)
                            return ww;
                    }
                }
            }


            var r = db.FindWares(null, parName, 0, 0, parCodeFastGroup, -1, parOffSet, parLimit);
            if (r.Count() > 0)
            {
                return r;
            }
            else
                return null;
        }

        public bool UpdateWorkPlace(IEnumerable<WorkPlace> parData)
        {
            db.ReplaceWorkPlace(parData);
            return true;
        }

        public bool MoveReceipt(IdReceipt parIdReceipt, IdReceipt parIdReceiptTo)
        {
            var param = new ParamMoveReceipt(parIdReceipt) { NewCodePeriod = parIdReceiptTo.CodePeriod, NewCodeReceipt = parIdReceiptTo.CodePeriod, NewIdWorkplace = parIdReceiptTo.IdWorkplace };
            return db.MoveReceipt(param);
        }

        public bool SetStateReceipt(IdReceipt receiptId, eStateReceipt parSrateReceipt)
        {
            var receipt = new Receipt(receiptId);
            receipt.StateReceipt = parSrateReceipt;
            db.CloseReceipt(receipt);
            return true;
        }

      
        public bool InsertWeight(string parBarCode, int parWeight, Guid? parWares = null)
        {
            if (string.IsNullOrEmpty(parBarCode)&& parWares==null)
                return false;

            if (parBarCode != null)
                return db.InsertWeight(new { BarCode = parBarCode, Weight = (decimal)parWeight / 1000m, Status = 0 });
            else
            {
                var Wares= new IdReceiptWares(new IdReceipt(), parWares.Value);
                return db.InsertWeight(new { BarCode = Wares.CodeWares.ToString(), Weight = (decimal)parWeight / 1000m, Status = -1 });
            }
            
        }

       
		public IEnumerable<ReceiptWares> GetWaresReceipt(IdReceipt parIdReceipt)
        {
            return db.ViewReceiptWares(parIdReceipt);
        }


        public IEnumerable<Receipt> GetReceipts(DateTime parStartDate, DateTime parFinishDate,int IdWorkPlace)
        {
            var res = db.GetReceipts(parStartDate.Date, parFinishDate.Date.AddDays(1), IdWorkPlace).ToList();
            if (parStartDate.Date != DateTime.Now.Date || parFinishDate.Date != DateTime.Now.Date)
            {
                var Ldc = parStartDate.Date;
                while (Ldc <= parFinishDate.Date)
                {
                    var ldb = new WDB_SQLite(null, false, Ldc);
                    var l= ldb.GetReceipts(Ldc.Date, Ldc.Date.AddDays(1), IdWorkPlace);
                    res.AddRange(l);
                    Ldc = Ldc.AddDays(1);
                }
            }
                //throw new NotImplementedException();
                return res;
        }

        public bool SaveRefundReceipt(Receipt parReceipt)       
        {
            db.ReplaceReceipt(parReceipt);
            db.ReplacePayment(parReceipt.Payment);
            var dbr = parReceipt.CodePeriod == parReceipt.CodePeriodRefund ? db : new WDB_SQLite("", true, parReceipt.RefundId.DTPeriod);

            foreach (var el in parReceipt.Wares)
            {
                db.AddWares(el);
                var w = new ReceiptWares(parReceipt.RefundId, el.WaresId);
                w.Quantity = el.Quantity;
                dbr.SetRefundedQuantity(w);
            }
            return true;
        }

     
        public bool SaveReceiptEvents(IEnumerable<ReceiptEvent> parRE)
        {
            if (parRE != null && parRE.Count() > 0)
            {
                db.DeleteReceiptEvent(parRE.First());
                db.InsertReceiptEvent(parRE);
            }
            return true;
        }

        public async Task<bool> SyncDataAsync(bool parIsFull)
        {
            var res=ds.SyncData(parIsFull);
            await ds.SendAllReceipt().ConfigureAwait(false);
            ds.LoadWeightKasa();
            return res;
        }

        public Task GetBonusAsync(Client parClient, Guid parTerminalId)
        {
            return ds.GetBonusAsync(parClient, parTerminalId);
        }

        public bool SendReceiptTo1C(IdReceipt parIdReceipt)
        {
            return ds.SendReceiptTo1C(parIdReceipt);
        }
    }
    


}




