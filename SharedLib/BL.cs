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
using System.Timers;

namespace SharedLib
{
    public class BL
    {
        static BL sBL;
        public WDB_SQLite db;
        public DataSync ds;

        /// <summary>
        /// Подія виклику кастомного вікна
        /// </summary>
        public Action<CustomWindow> OnCustomWindow { get; set; }
        /// <summary>
        /// Виникає, коли зчитали штрихкод адміністратора в режимі КСО
        /// </summary>
        public Action<User> OnAdminBarCode { get; set; }

        public static SortedList<int, long> UserIdbyWorkPlace = new SortedList<int, long>();

        public BL(bool pIsUseOldDB = false)
        {
            db = new WDB_SQLite(default(DateTime), null, pIsUseOldDB);
            db.BildWorkplace();
            ds = new DataSync(this);
            sBL = this;

            /*Global.OnReceiptCalculationComplete += (pWares, pIdReceipt) =>
            {
                Global.OnReceiptChanged?.Invoke(GetReceiptHead(pIdReceipt));
            };*/
        }

        public static BL GetBL { get { return sBL ?? new BL(); } }

        public ReceiptWares AddReceiptWares(ReceiptWares pW, bool pRecalcPriceOnLine = true)
        {
            bool isZeroPrice = false;
            lock (db.GetObjectForLockByIdWorkplace(pW.IdWorkplace))
            {
                var Quantity = db.GetCountWares(pW);
                pW.QuantityOld = Quantity;
                pW.Quantity += Quantity;

                if (pW.AmountSalesBan > 0 && pW.Quantity > pW.AmountSalesBan && pW.CodeUnit != Global.WeightCodeUnit && pW.CodeUnit != Global.WeightCodeUnit)
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

        public IEnumerable<QR> GetQR(IdReceipt pIdR) { return db.GetQR(pIdR); }

        public bool AddReceipt(IdReceipt pReceipt) { return AddReceipt(new Receipt(pReceipt)); }

        public bool AddReceipt(Receipt pReceipt) { return db.AddReceipt(pReceipt); }

        public IdReceipt GetNewIdReceipt(int pIdWorkplace = 0, int pCodePeriod = 0)
        {
            var idReceip = new IdReceipt() { IdWorkplace = (pIdWorkplace == 0 ? Global.IdWorkPlace : pIdWorkplace), CodePeriod = (pCodePeriod == 0 ? Global.GetCodePeriod() : pCodePeriod) };
            var Recipt = new Receipt(db.GetNewReceipt(idReceip));
            db.RecalcPriceAsync(new IdReceiptWares(Recipt));
            Global.OnReceiptChanged?.Invoke(Recipt);
            return Recipt;
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

            var Res = ldb.CloseReceipt(receipt);
            Global.OnReceiptChanged?.Invoke(receipt);
            return Res;
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
            if (w == null || w.Count() == 0 && pBarCode.Length >= 8) // Якщо не знайшли спробуем по ваговим і штучним штрихкодам.          
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
                            {CodeWares=W.CodeWares, Price=Math.Round(pPrice*(W.TypeWares==eTypeWares.Tobacco?1.05M:1M),2), TypeDiscount=eTypeDiscount.Price,Quantity=pQuantity,CodePS=999999 }
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
            return Res;
        }

        public bool ChangeQuantity(IdReceiptWares pReceiptWaresId, decimal pQuantity)
        {
            bool isZeroPrice = false;
            var res = false;
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

        public Client GetClientByCode(IdReceipt pIdReceipt, int pCode)
        {
            return SetClient(pIdReceipt, db.FindClient(null, null, null, pCode));
        }

        public Client GetClientByBarCode(IdReceipt pIdReceipt, string pBarCode)
        {
            return SetClient(pIdReceipt, db.FindClient(pBarCode));
        }

        public Client GetClientByPhone(IdReceipt pIdReceipt, string pPhone)
        {
            return SetClient(pIdReceipt, db.FindClient(null, pPhone));
        }

        Client SetClient(IdReceipt pIdReceipt, IEnumerable<Client> r)
        {
            if (r.Count() == 1)
            {
                var client = r.First();
                UpdateClientInReceipt(pIdReceipt, client);
                return client;
            }
            if (Global.TypeWorkplace != eTypeWorkplace.NotDefine && r.Count() > 1)//Якщо не модерн і є кілька клієнтів.
            {
                // Створюємо  користувацьке вікно
                var CW = new CustomWindow()
                {
                    Text = "Зробіть вибір",
                    Caption = "Вибір карточки клієнта",
                    AnswerRequired = true,
                    Buttons = r.Select(el => new CustomButton() { Id = el.CodeClient, Text = el.NameClient })
                };
                //PathPicture = @"icons\Signal.png",
                //customWindow.ValidationMask = "Щось)";               
                OnCustomWindow?.Invoke(CW);
            }
            return null;
        }

        private void UpdateClientInReceipt(IdReceipt pIdReceipt, Client parClient)
        {
            var RH = GetReceiptHead(pIdReceipt);
            RH.CodeClient = parClient.CodeClient;
            RH.PercentDiscount = parClient.PersentDiscount;
            db.ReplaceReceipt(RH);
            if (Global.RecalcPriceOnLine)
                db.RecalcPriceAsync(new IdReceiptWares(pIdReceipt));
            _ = ds.GetBonusAsync(parClient, pIdReceipt.IdWorkplace);
        }

        public IEnumerable<ReceiptWares> GetProductsByName(IdReceipt pReceipt, string pName, int pOffSet = -1, int pLimit = 10, int pCodeFastGroup = 0)
        {
            pName = pName.Trim();
            string Article = null;
            decimal Quantity = 0m;
            // Якщо пошук по штрихкоду і назва похожа на штрихкод або артикул
            if (!string.IsNullOrEmpty(pName))
            {
                var Reg = new Regex(@"^[0-9]{4,13}$");
                if (Reg.IsMatch(pName))
                {
                    Article = pName;
                }
                else
                {
                    Reg = new Regex(@"^[0-9]{1,5}[*]{1}[0-9]{1,8}[*]{0,1}$");
                    if (Reg.IsMatch(pName))
                    {
                        var s = pName.Split('*');
                        Article = s[1];
                        if (pName.EndsWith("*"))
                            Quantity = Convert.ToDecimal(s[0]);
                        pName = null;
                    }
                }
            }
            else
                pName = null;

            if (!string.IsNullOrEmpty(Article))
            {
                var w = AddWaresBarCode(pReceipt, Article, Quantity);
                if (w != null)
                    return new List<ReceiptWares> { w };
            }

            var r = db.FindWares(null, pName, 0, 0, pCodeFastGroup, -1, pOffSet, pLimit);
            if (r.Count() > 0)
            {
                return r;
            }
            else
                return null;
        }

        public bool UpdateWorkPlace(IEnumerable<WorkPlace> parData) { return db.ReplaceWorkPlace(parData); }

        public bool MoveReceipt(IdReceipt pIdReceipt, IdReceipt pIdReceiptTo)
        {
            var param = new ParamMoveReceipt(pIdReceipt) { NewCodePeriod = pIdReceiptTo.CodePeriod, NewCodeReceipt = pIdReceiptTo.CodePeriod, NewIdWorkplace = pIdReceiptTo.IdWorkplace };
            return db.MoveReceipt(param);
        }

        public bool SetStateReceipt(IdReceipt pIdReceipt, eStateReceipt pStateReceipt)
        {
            var receipt = new Receipt(pIdReceipt) { StateReceipt = pStateReceipt, DateReceipt = DateTime.Now, UserCreate = GetUserIdbyWorkPlace(pIdReceipt.IdWorkplace) };
            return db.CloseReceipt(receipt);
        }

        public bool InsertWeight(string pBarCode, int parWeight, Guid parWares, TypeSaveWeight parTypeSaveWeight)
        {
            var CodeWares = 0;
            if (string.IsNullOrEmpty(pBarCode) && parWares == Guid.Empty)
                return false;
            if (parWares != Guid.Empty)
                CodeWares = new IdReceiptWares(new IdReceipt(), parWares).CodeWares;

            return db.InsertWeight(new { BarCode = (parTypeSaveWeight == TypeSaveWeight.Add || pBarCode == null ? CodeWares.ToString() : pBarCode), Weight = (decimal)parWeight / 1000m, Status = parTypeSaveWeight });
        }

        public IEnumerable<ReceiptWares> GetWaresReceipt(IdReceipt pIdReceipt = null)
        {
            return db.ViewReceiptWares(pIdReceipt);
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

        public bool SaveReceipt(Receipt pReceipt, bool isRefund = true)
        {
            var ReceiptId = isRefund ? pReceipt.RefundId : (IdReceipt)pReceipt;

            var dbR = pReceipt.CodePeriod == Global.GetCodePeriod() ? db : new WDB_SQLite(ReceiptId.DTPeriod);

            dbR.ReplaceReceipt(pReceipt);
            dbR.ReplacePayment(pReceipt.Payment);

            var dbr = pReceipt.CodePeriod == pReceipt.CodePeriodRefund ? db : new WDB_SQLite(ReceiptId.DTPeriod);
            foreach (var el in pReceipt.Wares)
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

        public bool SaveReceiptEvents(IEnumerable<ReceiptEvent> pRE,bool pIsReplace=true)
        {
            if (pRE != null && pRE.Count() > 0)
            {
                try
                {
                    if(pIsReplace)
                        db.DeleteReceiptEvent(pRE.First());
                    db.InsertReceiptEvent(pRE);
                }
                catch (Exception e)
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = e, Status = eSyncStatus.NoFatalError, StatusDescription = "SaveReceiptEvents N=>" + pRE?.Count().ToString() + '\n' + e.Message + '\n' + new System.Diagnostics.StackTrace().ToString() });
                }
            }
            return true;
        }

        public Task GetBonusAsync(Client pClient, int pIdWorkPlace) { return ds.GetBonusAsync(pClient, pIdWorkPlace); }

        public bool SendReceiptTo1C(IdReceipt parIdReceipt) { return ds.SendReceiptTo1C(parIdReceipt); }

        public void CloseDB() { db?.Close(); }

        public void StartWork(int pIdWorkplace, string pCodeCashier)
        {
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

        public bool FixWeight(ReceiptWares pRW) { 
            bool Res= db.FixWeight(pRW);
            var r = db.ViewReceipt(pRW, true);
            Global.OnReceiptCalculationComplete?.Invoke(r);
            return Res;
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
                        OnAdminBarCode?.Invoke(u);
                }
                else
                { _ = GetBonusAsync(c, Global.IdWorkPlace); }
            }
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
            var u = db.GetUser(new User() { Login = pLogin, PassWord = pPassWord });
            if (u.Count() > 0)
                return u.First();
            return null;
        }

        public bool InsertLogRRO(LogRRO pL) { return db.InsertLogRRO(pL); }

        public void AddEventAge(IdReceipt pRecipt)
        {
            List<ReceiptEvent> rr = new List<ReceiptEvent> { new ReceiptEvent(pRecipt) { EventType = eReceiptEventType.AgeRestrictedProduct, EventName = "Вік підтверджено", CreatedAt = DateTime.Now } }; 

            db.InsertReceiptEvent(rr);
        }
        /// <summary>
        /// Cстворення повернення на основі чека.
        /// </summary>
        /// <param name="IdR">Чек на який робиться повернення</param>
        /// <returns></returns>
        public Receipt CreateRefund(IdReceipt IdR)
        {
            try
            {
                var NewR = GetNewIdReceipt(IdR.IdWorkplace);
                var R = GetReceiptHead(IdR, true);
                R.Payment = null;
                R.ReceiptEvent = null;
                R.TypeReceipt = eTypeReceipt.Refund;
                R.StateReceipt = eStateReceipt.Prepare;
                R.RefundId = new IdReceipt(R);
                R.SetIdReceipt(NewR);
                db.ReplaceReceipt(R);

                foreach (var el in R.Wares)
                {
                    el.MaxRefundQuantity = el.Quantity - el.RefundedQuantity;
                    el.Quantity = 0;
                    db.AddWares(el);
                }
                Global.OnReceiptCalculationComplete?.Invoke(R);
                return R;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"BL.CreateRefund Exception =>( {IdR?.ToJSON() }) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
                return null;
            }
        }

        /// <summary>
        /// Викликається при початку списання бонусів.
        /// </summary>
        /// <param name="IdR"></param>
        /// <returns></returns>
        public Receipt CalcBonus(IdReceipt IdR)
        {
            var R = GetReceiptHead(IdR, true);
            return null;
        }

        public void SetCustomWindows(CustomWindowAnswer pCWA)
        {
            if (pCWA != null)
            {
                switch (pCWA.Id)
                {
                    case eWindows.ChoiceClient:
                        GetClientByCode(pCWA.idReceipt, pCWA.IdButton);
                        break;
                    case eWindows.PhoneClient:
                        GetClientByPhone(pCWA.idReceipt, pCWA.Text);
                        break;
                    case eWindows.ConfirmWeight:
                        if (pCWA.ExtData != null)
                        {
                            ReceiptWares r = pCWA.ExtData as ReceiptWares;
                            if (r != null)
                            {
                                AddEventAge(r);
                                //SaveReceiptEvents(new List<ReceiptEvent>() { new ReceiptEvent(r) { EventType = eReceiptEventType.IncorrectWeight, ProductConfirmedWeight = (int)r.WeightFact, ProductWeight = (int)r.WeightFact } });
                                FixWeight(r);
                            }
                        }
                        break;
                }
            }
        }

    }
}