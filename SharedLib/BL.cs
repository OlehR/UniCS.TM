using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Utils;

namespace SharedLib
{
    public class BL
    {
        static BL sBL;
        public WDB_SQLite db;

        public Receipt curRecipt;
        //public bool IsRefund { get; set; }

        public DataSync ds;
        public ControlScale CS = new ControlScale();

        public static SortedList<int, long> UserIdbyWorkPlace = new SortedList<int, long>();
        //public Action<IEnumerable<ReceiptWares>, Guid> OnReceiptCalculationComplete { get; set; }
        
       // public string Ver = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        
        /// <summary>
        /// Для швидкого пошуку 
        /// </summary>
       // SortedList<Guid, int> WorkId;

        double LastWeight;
        int LastCodeWares;
        public BL(bool pIsUseOldDB = false)
        {
            db = new WDB_SQLite(default(DateTime), null, pIsUseOldDB);
            db.BildWorkplace();
            ds = new DataSync(this);
            //WorkId = new SortedList<Guid, int>();
            sBL = this;
        }
        
        public static BL GetBL { get { return sBL; } }

        public ReceiptWares AddReceiptWares(ReceiptWares pW, bool pRecalcPriceOnLine = true)
        {
            bool isZeroPrice=false;
            lock (db.GetObjectForLockByIdWorkplace(pW.IdWorkplace))
            {
                var Quantity = db.GetCountWares(pW);
                pW.QuantityOld = Quantity;
                pW.Quantity += Quantity;

                if (pW.AmountSalesBan>0 && pW.Quantity>pW.AmountSalesBan && (pW.CodeUnit != Global.WeightCodeUnit && pW.CodeUnit != Global.WeightCodeUnit))
                {
                    pW.Quantity = pW.AmountSalesBan;
                    if (Global.IsOldInterface)
                    {
                        isZeroPrice = true;
                        return null;
                    }
                    else
                        Global.OnClientWindows?.Invoke(pW.IdWorkplace, eTypeWindows.LimitSales, $"Даний товар {pW.NameWares} {Environment.NewLine} має обмеження в кількості {pW.AmountSalesBan} шт");
                
                }
                if (Quantity > 0)
                    db.UpdateQuantityWares(pW);
                else
                    db.AddWares(pW);
            }

            //Кешконтроль
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            // Get the elapsed time as a TimeSpan value.            

            _ = VR.SendMessageAsync(pW.IdWorkplace, pW.NameWares, pW.Articl, pW.Quantity, pW.Sum);
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine("\nVR=>" + ts.TotalMilliseconds + "\n");

            if (pRecalcPriceOnLine && Global.RecalcPriceOnLine)
                db.RecalcPriceAsync(pW);

            /*if (pW.PLU > 0)             
                 GenQRAsync(pW);*/
            if (isZeroPrice)
            {
                pW.Price = 0;
                pW.PriceDealer = 0;
            }
            return pW;
        }

        public Task GenQRAsync(IEnumerable<ReceiptWares> pRW)
        {
            return Task.Run(() => GenQRAsync1(pRW));
        }

        public async Task<bool> GenQRAsync1(IEnumerable<ReceiptWares> pW)
        {
            bool res = true;
            if (!Global.IsGenQrCoffe)
                return res;

            int Number = 0;
            foreach (var el in pW.Where(r => r.PLU > 0))
            {
                StringBuilder QRs = new StringBuilder();
                for (int i = 0; i < el.Quantity; i++)
                {
                    var QR = await ds.GetQrCoffe(el, Number++);
                    QRs.Append((QRs.Length > 0 ? "," : "") + QR);
                }
                el.QR = QRs.ToString();
                res &= db.UpdateQR(el);
            }
            return res;
        }

        public IEnumerable<QR> GetQR(IdReceipt pIdR)
        {
            return db.GetQR(pIdR);
        }

        public bool AddReceipt(IdReceipt parReceipt)
        {
            var receipt = new Receipt(parReceipt);
            return AddReceipt(receipt);
        }

        public bool AddReceipt(Receipt pReceipt)
        {
            return db.AddReceipt(pReceipt);
        }

        public int GetIdWorkplaceByTerminalId(Guid parTerminalId)
        {
            return Global.GetIdWorkplaceByTerminalId(parTerminalId);
        }

        public IdReceipt GetNewIdReceipt(Guid parTerminalId, int parCodePeriod = 0)
        {
            return GetNewIdReceipt(GetIdWorkplaceByTerminalId(parTerminalId));            
        }

        public IdReceipt GetNewIdReceipt(int pIdWorkplace = 0, int pCodePeriod = 0)
        {
            var idReceip = new IdReceipt() { IdWorkplace = (pIdWorkplace == 0 ? Global.IdWorkPlace : pIdWorkplace), CodePeriod = (pCodePeriod == 0 ? Global.GetCodePeriod() : pCodePeriod) };
            curRecipt =  new Receipt( db.GetNewReceipt(idReceip));
            db.RecalcPriceAsync(new IdReceiptWares(curRecipt));
            return curRecipt;
        }

        public Receipt GetLastReceipt(Guid parTerminalId, int pCodePeriod = 0)
        {
            return GetLastReceipt(GetIdWorkplaceByTerminalId(parTerminalId), pCodePeriod);            
        }

        public Receipt GetLastReceipt(int pIdWorkplace = 0, int parCodePeriod = 0)
        {
            var idReceip = new IdReceipt() { IdWorkplace = (pIdWorkplace == 0 ? Global.IdWorkPlace : pIdWorkplace), CodePeriod = (parCodePeriod == 0 ? Global.GetCodePeriod() : parCodePeriod) };
            return db.GetLastReceipt(idReceip);
        }

        public bool UpdateReceiptFiscalNumber(IdReceipt receiptId, string pFiscalNumber, decimal pSumFiscal = 0, DateTime pDateFiscal = default(DateTime))
        {
            if (pDateFiscal == default(DateTime))
                pDateFiscal = DateTime.Now;
            var receipt = new Receipt(receiptId);
            receipt.NumberReceipt = pFiscalNumber;
            receipt.StateReceipt = eStateReceipt.Print;
            receipt.UserCreate = GetUserIdbyWorkPlace(receiptId.IdWorkplace);
            receipt.SumFiscal = pSumFiscal;
            receipt.DateReceipt = pDateFiscal;

            DateTime Ldc = receiptId.DTPeriod;

            WDB_SQLite ldb = (Ldc == DateTime.Now.Date ? db : new WDB_SQLite(Ldc));

            var Res=ldb.CloseReceipt(receipt);
            curRecipt = receipt;
            return Res;
        }

        [Obsolete("This metod  is deprecated")]
        public ReceiptWares AddWaresBarCode(string pBarCode, decimal pQuantity = 0)
        {
            return AddWaresBarCode(curRecipt, pBarCode, pQuantity);
        }

        public ReceiptWares AddWaresBarCode(IdReceipt pReceipt, string pBarCode, decimal pQuantity = 0)
        {
            if (pBarCode == null)
                return null;
            IEnumerable<ReceiptWares> w = null;
            pBarCode = pBarCode.Trim();
            if (pBarCode.Length >= 8)
                w = db.FindWares(pBarCode);//Пошук по штрихкоду
            else// Можливо артикул товару
            {
                int Article;
                if (int.TryParse(pBarCode, out Article))
                    w = db.FindWares(null, null, 0, 0, 0, Article);
                else
                    return null;
            }

            //ReceiptWares W = null;
            if (w == null || w.Count() == 0) // Якщо не знайшли спробуем по ваговим і штучним штрихкодам.          
            {
                foreach (var el in Global.CustomerBarCode.Where(el => el.KindBarCode == eKindBarCode.EAN13 /*&& (el.TypeBarCode == eTypeBarCode.WaresWeight || el.TypeBarCode == eTypeBarCode.WaresUnit )*/))
                {
                    w = null;
                    if (el.Prefix.Equals(pBarCode.Substring(0, el.Prefix.Length)))
                    {
                        if (el.KindBarCode == eKindBarCode.EAN13 && pBarCode.Length != 13)
                            break;

                        int varCode = Convert.ToInt32(pBarCode.Substring(el.Prefix.Length, el.LenghtCode));
                        int varValue = Convert.ToInt32(pBarCode.Substring(el.Prefix.Length + el.LenghtCode, el.LenghtQuantity));
                        switch (el.TypeCode)
                        {
                            case eTypeCode.Article:
                                w = db.FindWares(null, null, 0, 0, 0, varCode);
                                break;
                            case eTypeCode.Code:
                                w = db.FindWares(null, null, varCode);
                                break;
                            case eTypeCode.PercentDiscount:
                                _ = ds.CheckDiscountBarCodeAsync(pReceipt, pBarCode, varCode);
                                return new ReceiptWares(pReceipt);
                            default:
                                break;
                        }
                        if (w != null && w.Count() == 1) //Знайшли що треба
                        {
                            //parQuantity = (w.First().CodeUnit == Global.WeightCodeUnit ? varValue / 1000m : varValue);
                            if (pQuantity > 0)
                                pQuantity = varValue;
                            break;
                        }
                    }
                }

            }

            if (w == null || w.Count() != 1)
                return null;

            var W = w.First();
            W.RecalcTobacco();
            if (pBarCode.Length >= 8)
                W.BarCode = pBarCode;
            if (pQuantity == 0 || W.IsMultiplePrices) //Якщо сигарети не добававляємо товар.
                return W;

            if (W.Price == 0)//Якщо немає ціни на товар !!!!TMP Краще обробляти на GUI буде пізніше
                return W;
            W.SetIdReceipt(pReceipt);
            W.Quantity = (W.CodeUnit == Global.WeightCodeUnit ? pQuantity / 1000m : pQuantity);// Вага приходить в грамах
            return AddReceiptWares(W);
        }

        public ReceiptWares AddWaresCode(int pCodeWares, int pCodeUnit, decimal pQuantity = 0, decimal pPrice = 0)
        {
            return AddWaresCode(curRecipt, pCodeWares, pCodeUnit, pQuantity, pPrice);
        }

        public ReceiptWares AddWaresCode(IdReceipt pIdReceipt, int pCodeWares, int pCodeUnit, decimal pQuantity = 0, decimal pPrice = 0)
        {
            var w = db.FindWares(null, null, pCodeWares, pCodeUnit);
            if (w.Count() == 1)
            {
                var W = w.First();
                W.RecalcTobacco();
                if (pQuantity == 0)
                    return W;
                W.SetIdReceipt(pIdReceipt);
                if (pPrice > 0 || !W.IsMultiplePrices)
                {
                    W.Quantity = (W.CodeUnit == Global.WeightCodeUnit ? pQuantity / 1000m : pQuantity);//Хак для вагового товару Який приходить в грамах.
                    var res = AddReceiptWares(W, false);
                    if (pPrice > 0 && W.IsMultiplePrices)
                    {
                        WaresReceiptPromotion[] r = new WaresReceiptPromotion[1] { new WaresReceiptPromotion(W)
                            {CodeWares=W.CodeWares, Price=Math.Round(pPrice*(W.TypeWares==2?1.05M:1M),2), TypeDiscount=eTypeDiscount.Price,Quantity=pQuantity,CodePS=999999 }
                            };
                        db.ReplaceWaresReceiptPromotion(r);
                        db.RecalcHeadReceipt(pIdReceipt);
                    }
                    if (Global.RecalcPriceOnLine)
                        db.RecalcPriceAsync(W);
                    return res;

                }
                else
                    return W;
            }

            return null;
        }

        public IEnumerable<ReceiptWares> ViewReceiptWares(IdReceipt parIdReceipt, bool pIsReceiptWaresPromotion = false)
        {
            var Res = db.ViewReceiptWares(parIdReceipt, pIsReceiptWaresPromotion);
            //var El = Res.First();
            return Res;
        }
       
        public bool ChangeQuantity(IdReceiptWares pReceiptWaresId, decimal pQuantity)
        {
            bool isZeroPrice = false;
            var res = false;
            //var W = db.FindWares(null, null, parReceiptWaresId.CodeWares, parReceiptWaresId.CodeUnit);
            // if (W.Count() == 1)
            //{
            //var w= new ReceiptWares(parReceiptWaresId);
            var W = db.FindWares(null, null, pReceiptWaresId.CodeWares, pReceiptWaresId.CodeUnit);

            if (W.Count() == 1)
            {
                var w = W.First();
                w.SetIdReceipt(pReceiptWaresId);
                if (pQuantity == 0)
                {
                    db.DeleteReceiptWares(w);
                    _ = VR.SendMessageAsync(w.IdWorkplace, w.NameWares, w.Articl, w.Quantity, w.Sum, VR.eTypeVRMessage.DeleteWares);
                }
                else
                {
                    //w.SetIdReceiptWares();
                    w.Quantity = pQuantity;
                    w.Sort = -1;
                    if (w.AmountSalesBan > 0 && w.Quantity > w.AmountSalesBan && (w.CodeUnit != Global.WeightCodeUnit && w.CodeUnit != Global.WeightCodeUnit))
                    {
                        w.Quantity = w.AmountSalesBan;
                        if (Global.IsOldInterface)
                            isZeroPrice = true;
                        else
                            Global.OnClientWindows?.Invoke(w.IdWorkplace, eTypeWindows.LimitSales, $"Даний товар {w.NameWares} {Environment.NewLine} має обмеження в кількості {w.AmountSalesBan} шт");
                    }

                    res = db.UpdateQuantityWares(w);
                    _ = VR.SendMessageAsync(w.IdWorkplace, w.NameWares, w.Articl, w.Quantity, w.Sum, VR.eTypeVRMessage.UpdateWares);
                }
                if (Global.RecalcPriceOnLine)
                    db.RecalcPriceAsync(pReceiptWaresId);
            }
            return res;
        }

        public Receipt GetReceiptHead(IdReceipt idReceipt, bool parWithDetail = false)
        {
            DateTime Ldc = idReceipt.DTPeriod;
            if (Ldc == DateTime.Now.Date)
                return db.ViewReceipt(idReceipt, parWithDetail);

            var ldb = new WDB_SQLite(Ldc);
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
            _ = ds.GetBonusAsync(parClient, idReceipt.IdWorkplace);
        }

        public IEnumerable<ReceiptWares> GetProductsByName(IdReceipt parReceipt, string parName, int parOffSet = -1, int parLimit = 10, int parCodeFastGroup = 0)
        {
            if (parReceipt == null)
                parReceipt = curRecipt;

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
            else
                parName = null;

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

        public bool MoveReceipt(IdReceipt pIdReceipt, IdReceipt pIdReceiptTo)
        {
            var param = new ParamMoveReceipt(pIdReceipt) { NewCodePeriod = pIdReceiptTo.CodePeriod, NewCodeReceipt = pIdReceiptTo.CodePeriod, NewIdWorkplace = pIdReceiptTo.IdWorkplace };
            return db.MoveReceipt(param);
        }

        public bool SetStateReceipt(IdReceipt pIdReceipt, eStateReceipt pStateReceipt)
        {
            if (pIdReceipt == null)
                pIdReceipt = curRecipt;
            var receipt = new Receipt(pIdReceipt) { StateReceipt = pStateReceipt, DateReceipt = DateTime.Now, UserCreate = GetUserIdbyWorkPlace(pIdReceipt.IdWorkplace) };
            var Res = db.CloseReceipt(receipt);

            return Res;
        }

        public bool InsertWeight(string parBarCode, int parWeight, Guid parWares, TypeSaveWeight parTypeSaveWeight)
        {
            var CodeWares = 0;

            if (string.IsNullOrEmpty(parBarCode) && parWares == Guid.Empty)
                return false;
            if (parWares != Guid.Empty)
                CodeWares = new IdReceiptWares(new IdReceipt(), parWares).CodeWares;

            return db.InsertWeight(new { BarCode = (parTypeSaveWeight == TypeSaveWeight.Add || parBarCode == null ? CodeWares.ToString() : parBarCode), Weight = (decimal)parWeight / 1000m, Status = parTypeSaveWeight });
        }

        public IEnumerable<ReceiptWares> GetWaresReceipt(IdReceipt pIdReceipt = null)
        {
            return db.ViewReceiptWares(pIdReceipt == null ? curRecipt : pIdReceipt);
        }

        public IEnumerable<Receipt> GetReceipts(DateTime parStartDate, DateTime parFinishDate, int IdWorkPlace)
        {
            var res = db.GetReceipts(parStartDate.Date, parFinishDate.Date.AddDays(1), IdWorkPlace).ToList();
            if (parStartDate.Date != DateTime.Now.Date || parFinishDate.Date != DateTime.Now.Date)
            {
                var Ldc = parStartDate.Date;
                while (Ldc <= parFinishDate.Date)
                {
                    using (var ldb = new WDB_SQLite(Ldc))
                    {
                        var l = ldb.GetReceipts(Ldc.Date, Ldc.Date.AddDays(1), IdWorkPlace);
                        res.AddRange(l);
                        Ldc = Ldc.AddDays(1);
                    }
                }
            }
            return res;
        }

        public Receipt GetReceiptByFiscalNumber(int IdWorkplace, string pFiscalNumber, DateTime pStartDate = default(DateTime), DateTime pFinishDate = default(DateTime))
        {
            if (pStartDate == default(DateTime))
                pStartDate = DateTime.Now.Date.AddDays(-14);
            if (pFinishDate == default(DateTime))
                pFinishDate = DateTime.Now;

            var Ldc = pStartDate.Date;
            while (Ldc <= pFinishDate.Date)
            {
                var ldb = new WDB_SQLite(Ldc);
                var l = ldb.GetReceiptByFiscalNumber(IdWorkplace, pFiscalNumber, pStartDate, pFinishDate);
                if (l != null && l.Count() >= 1)
                    return l.First();
                Ldc = Ldc.AddDays(1);
            }
            return null;
        }

        public bool SaveReceipt(Receipt parReceipt, bool isRefund = true)
        {
            var ReceiptId = isRefund ? parReceipt.RefundId : (IdReceipt)parReceipt;

            var dbR = parReceipt.CodePeriod == Global.GetCodePeriod() ? db : new WDB_SQLite(ReceiptId.DTPeriod);

            dbR.ReplaceReceipt(parReceipt);
            dbR.ReplacePayment(parReceipt.Payment);

            var dbr = parReceipt.CodePeriod == parReceipt.CodePeriodRefund ? db : new WDB_SQLite(ReceiptId.DTPeriod);
            foreach (var el in parReceipt.Wares)
            {
                dbR.AddWares(el);
                if (isRefund)
                {
                    var w = new ReceiptWares(ReceiptId, el.WaresId);
                    w.Quantity = el.Quantity;
                    dbr.SetRefundedQuantity(w);
                }
            }
            return true;
        }

        public bool SaveReceiptEvents(IEnumerable<ReceiptEvent> parRE)
        {
            if (parRE != null && parRE.Count() > 0)
            {
                try
                {
                    db.DeleteReceiptEvent(parRE.First());
                    db.InsertReceiptEvent(parRE);
                }
                catch (Exception e)
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = e, Status = eSyncStatus.NoFatalError, StatusDescription = "SaveReceiptEvents N=>" + parRE?.Count().ToString() + '\n' + e.Message + '\n' + new System.Diagnostics.StackTrace().ToString() });
                }
            }
            return true;
        }

        public async Task<bool> SyncDataAsync(bool parIsFull)
        {
            var res = ds.SyncData(ref parIsFull);
            var CurDate = DateTime.Now;
            // не обміннюємось чеками починаючи з 23:45 до 1:00
            if (!((CurDate.Hour == 23 && CurDate.Minute > 44) || CurDate.Hour == 0))
                await ds.SendAllReceipt().ConfigureAwait(false);

            if (CurDate.Hour < 7)
            {
                //  ds.LoadWeightKasa();
                ds.LoadWeightKasa2Period();
            }
            if (parIsFull)
                _ = ds.SendRWDeleteAsync();

            return res;
        }

        public Task GetBonusAsync(Client pClient, int pTerminalId)
        {
            return ds.GetBonusAsync(pClient, pTerminalId);
        }

        public bool SendReceiptTo1C(IdReceipt parIdReceipt)
        {
            return ds.SendReceiptTo1C(parIdReceipt);
        }

        public void CloseDB()
        {
            if (db != null)
                db.Close();
        }

        public void StartWork(int pIdWorkplace, string pCodeCashier)
        {
            //tmp 
            long CodeCashier = long.Parse(pCodeCashier);
            if (UserIdbyWorkPlace.ContainsKey(pIdWorkplace))
                UserIdbyWorkPlace[pIdWorkplace] = CodeCashier;
            else
                UserIdbyWorkPlace.Add(pIdWorkplace, CodeCashier);
        }

        public void StoptWork(int pIdWorkplace)
        {
            if (UserIdbyWorkPlace.ContainsKey(pIdWorkplace))
                UserIdbyWorkPlace.Remove(pIdWorkplace);
        }

        public long GetUserIdbyWorkPlace(int pIdWorkplace)
        {
            if (UserIdbyWorkPlace.ContainsKey(pIdWorkplace))
                return UserIdbyWorkPlace[pIdWorkplace];
            return 0;
        }

        public bool FixWeight(IdReceipt pIdReceipt, Guid pProductId, decimal pWeight)
        {
            var RW = new ReceiptWares(pIdReceipt, pProductId);
            RW.FixWeight = pWeight;
            return db.FixWeight(RW);
        }

        /// <summary>
        /// Отриманий штрихкод з Обладнання.
        /// </summary>
        /// <param name="pBarCode"></param>
        /// <param name="pTypeBarCode"></param>
        public void GetBarCode(string pBarCode, string pTypeBarCode)
        {

            var r = GetLastReceipt();
            var w = AddWaresBarCode(r, pBarCode, 1);
            if (w == null) //Можливо штрихкод не товар
            {
                var c = GetClientByBarCode(r, pBarCode);
                if (c == null)
                {
                    var u = GetUserByBarCode(pBarCode);
                    if (u != null)
                        Global.OnAdminBarCode?.Invoke(u);
                }
                else
                { _ = GetBonusAsync(c, Global.IdWorkPlace); }
            }


        }
        /// <summary>
        /// Отримана вага з Обладннааняю
        /// </summary>
        /// <param name="pWeight"></param>
        /// <param name="pIsStable"></param>
        public void GetScale(double pWeight, bool pIsStable)
        {
            if (pIsStable)
                LastWeight = pWeight;
            //MW.Weight = pWeight.ToString();
        }

        public void StartScale(int pCodeWares = 0)
        {
            LastCodeWares = pCodeWares;
            LastWeight = 0d;
        }

        /// <summary>
        /// Добавляє зважений товар в базу.
        /// </summary>
        public void AddWeightWares()
        {
            AddWaresCode(LastCodeWares, 0, Convert.ToDecimal(LastWeight));
        }

        public bool UpdateExciseStamp(IEnumerable<ReceiptWares> pRW)
        {
            var r = pRW.Where(e => e.ExciseStamp != null && e.ExciseStamp.Length > 0);
            try
            {
                return db.UpdateExciseStamp(r);
            }
            catch (Exception e)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = e, Status = eSyncStatus.NoFatalError, StatusDescription = "UpdateExciseStamp N=>" + r?.Count().ToString() + '\n' + e.Message + '\n' + new System.Diagnostics.StackTrace().ToString() });

            }
            return false;
        }

        public User GetUserByBarCode(string pBarCode)
        {
            var u = db.GetUser(new User() { BarCode = pBarCode });
            if (u.Count() > 0)
                return u.First();
            return null;
        }

        public User GetUserByLogin(string pLogin, string pPassWord)
        {
            var u = db.GetUser(new User() { Login = pLogin,PassWord = pPassWord });
            if (u.Count() > 0)
                return u.First();
            return null;
        }

        public bool InsertLogRRO(LogRRO pL)
        {
           return db.InsertLogRRO(pL);
        }

        public void AddEventAge()
        {
            List<ReceiptEvent> rr = new List<ReceiptEvent> { new ReceiptEvent(curRecipt) { EventType = ReceiptEventType.AgeRestrictedProduct, EventName = "Вага підтверджена", CreatedAt = DateTime.Now } };
            db.InsertReceiptEvent(rr);

        }

        public Receipt CreateRefund(IdReceipt IdR)
        {
            try
            {
                var NewR = GetNewIdReceipt(IdR.IdWorkplace);
                var R = GetReceiptHead(IdR, true);
                R.Payment = null;
                R.ReceiptEvent = null;
                R.TypeReceipt = eTypeReceipt.Refund;
                R.RefundId = new IdReceipt(R);
                R.SetIdReceipt(NewR);
                db.ReplaceReceipt(R);
                
                foreach (var el in R.Wares)
                {
                    el.MaxRefundQuantity = el.Quantity - el.RefundedQuantity;
                    el.Quantity = 0;
                    db.AddWares(el);
                }
                curRecipt = R;
                return R;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"BL.CreateRefund Exception =>( {IdR?.ToJSON() }) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                return null;
            }
        }
    }
}