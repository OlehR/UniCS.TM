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
using System.IO;
using Model;

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

        public BL()
        {
            try
            {
                db = WDB_SQLite.GetInstance;
                ds = new DataSync(this);
                Global.BildWorkplace(db.GetWorkPlace());
                if (!File.Exists(db.LastMidFile))
                {
                    bool res = true;
                    _ = ds.SyncData(ref res);
                }
                db.BildWaresWarehouse();

                sBL = this;
                var WP = Global.GetWorkPlaceByIdWorkplace(Global.IdWorkPlace);
                if (WP != null)
                {
                    Global.DefaultCodeDealer = WP.CodeDealer;
                }
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage("BL", ex);
            }
        }

        public static BL GetBL { get { return sBL ?? new BL(); } }

        public ReceiptWares AddReceiptWares(ReceiptWares pW, bool pRecalcPriceOnLine = true)
        {
            var State = GetStateReceipt(pW);
            if (State != eStateReceipt.Prepare)
            {
                OnCustomWindow?.Invoke(new CustomWindow(eWindows.NoPrice, $"Чеку в стані {State} заборонено {Environment.NewLine} добавляти товар"));
                return null;
            }
            //Провірка на час продажу алкоголю

            if (Global.BlockSales != null && Global.BlockSales.Any() && Global.BlockSales.Max(el => el.IsBlock(pW.TypeWares)))
            {
                OnCustomWindow?.Invoke(new CustomWindow(eWindows.BlockSale, $"Товари {pW.NameWares} з групи {pW.TypeWares}{Environment.NewLine} продаж тимчасово заборонено на даній КСО"));
                return null;
            }

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
                    }
                    else
                    {
                        Global.OnClientWindows?.Invoke(pW.IdWorkplace, eTypeWindows.LimitSales, $"Даний товар {pW.NameWares} {Environment.NewLine} має обмеження в кількості {pW.AmountSalesBan} шт");

                        OnCustomWindow?.Invoke(new CustomWindow(eWindows.LimitSales, $"Даний товар {pW.NameWares} {Environment.NewLine} має обмеження в кількості {pW.AmountSalesBan} шт"));
                    }
                    return null;
                }
                if (pW.PriceDealer == 0)
                {
                    OnCustomWindow?.Invoke(new CustomWindow(eWindows.NoPrice, pW.NameWares));
                    return null;
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

            VR.SendMessage(pW.IdWorkplace, pW.NameWares, pW.Articl, pW.Quantity, pW.Sum);
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine("\nVR=>" + ts.TotalMilliseconds + "\n");

            if (pRecalcPriceOnLine && Global.RecalcPriceOnLine)
                db.RecalcPriceAsync(pW);

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

        public Receipt GetNewIdReceipt(int pIdWorkplace = 0, int pCodePeriod = 0)
        {
            var idReceip = new IdReceipt() { IdWorkplace = (pIdWorkplace == 0 ? Global.IdWorkPlace : pIdWorkplace), CodePeriod = (pCodePeriod == 0 ? Global.GetCodePeriod() : pCodePeriod), CodeReceipt = Global.StartCodeReceipt };
            var Recipt = new Receipt(db.GetNewReceipt(idReceip));
            Global.OnReceiptCalculationComplete?.Invoke(Recipt);
            //db.RecalcPriceAsync(new IdReceiptWares(Recipt));
            //Global.OnReceiptChanged?.Invoke(Recipt);
            return Recipt;
        }

        public Receipt GetLastReceipt(int pIdWorkplace = 0, int parCodePeriod = 0)
        {
            var idReceip = new IdReceipt() { IdWorkplace = (pIdWorkplace == 0 ? Global.IdWorkPlace : pIdWorkplace), CodePeriod = (parCodePeriod == 0 ? Global.GetCodePeriod() : parCodePeriod) };
            return db.GetLastReceipt(idReceip);
        }

        public bool UpdateReceiptFiscalNumber(Receipt pR)
        {
            var Res = db.CloseReceipt(pR);
            // var r = db.ViewReceipt(pR, true);
            Global.OnReceiptCalculationComplete?.Invoke(pR);
            return Res;
        }

        public ReceiptWares AddWaresBarCode(IdReceipt pReceipt, string pBarCode, decimal pQuantity = 0, bool IsOnlyDiscount = false)
        {
            if (pBarCode == null)
                return null;
            IEnumerable<ReceiptWares> w = null;
            pBarCode = (string)pBarCode.Trim().Clone();
            if (!IsOnlyDiscount)
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
            int CodeOperator=0;
            //ReceiptWares W = null;
            if (w == null || w.Count() == 0 && pBarCode.Length >= 8) // Якщо не знайшли спробуем по ваговим і штучним штрихкодам.          
            {
                foreach (var el in Global.Settings.CustomerBarCode.Where(el => el.KindBarCode == eKindBarCode.EAN13 || el.KindBarCode == eKindBarCode.Code128 /*&& (el.TypeBarCode == eTypeBarCode.WaresWeight || el.TypeBarCode == eTypeBarCode.WaresUnit )*/))
                {
                    w = null;
                    if (el.TotalLenght != pBarCode.Length)
                        continue;
                    if (el.Prefix.Equals(pBarCode[..el.Prefix.Length]))
                    {
                        if (el.KindBarCode == eKindBarCode.EAN13 && pBarCode.Length != 13)
                            continue;

                        int Code = pBarCode.Substring(el.Prefix.Length, el.LenghtCode).ToInt();
                        CodeOperator = el.LenghtOperator > 0 ? Convert.ToInt32(pBarCode.Substring(el.Prefix.Length+el.LenghtCode, el.LenghtOperator)) : 0;
                        int varValue = el.LenghtQuantity>0?Convert.ToInt32(pBarCode.Substring(el.Prefix.Length + el.LenghtCode + el.LenghtOperator, el.LenghtQuantity)):0;
                        if (!IsOnlyDiscount || el.TypeCode == eTypeCode.PercentDiscount)
                        {
                            switch (el.TypeCode)
                            {
                                case eTypeCode.Article:
                                    w = db.FindWares(null, null, 0, 0, 0, Code);
                                    break;
                                case eTypeCode.Code:
                                    w = db.FindWares(null, null, Code);
                                    break;
                                case eTypeCode.PercentDiscount:
                                    _ = ds.CheckDiscountBarCodeAsync(pReceipt, pBarCode, Code);
                                    return new ReceiptWares(pReceipt);
                                case eTypeCode.Coupon:                                    
                                case eTypeCode.OneTimeCoupon:
                                case eTypeCode.OneTimeCouponGift:
                                    //if (
                                    _ = CheckOneTimeAsync(pReceipt, pBarCode.ToLong(), el.TypeCode); //)
                                     return new ReceiptWares(pReceipt);
                                //else return null;
                                case eTypeCode.GiftCard:
                                    w= CheckGiftCardAsync(pReceipt,pBarCode);
                                    if(w==null) return new ReceiptWares(pReceipt);
                                    break;                                   
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
            }

            if (w == null || w.Count() != 1)
                return null;

            var W = w.First();
            W.CodeOperator = CodeOperator;
            W.RecalcTobacco();
            if (pBarCode.Length >= 8)
                W.BarCode = pBarCode;
            if ((pQuantity == 0 && W.Quantity==0)|| W.IsMultiplePrices ) //Якщо сигарети не добававляємо товар.
                return W;

            if (W.Price == 0)//Якщо немає ціни на товар !!!!TMP Краще обробляти на GUI буде пізніше
                return W;
            W.SetIdReceipt(pReceipt);
            if(W.CodeDefaultUnit!=W.CodeUnit && W.Coefficient!=1 && W.Coefficient!=0)
            {
                W.CodeUnit = W.CodeDefaultUnit;
                W.Quantity = W.Coefficient;
            }
            if(W.Quantity==0)
             W.Quantity = (W.CodeUnit == Global.WeightCodeUnit ? pQuantity / 1000m : pQuantity);// Вага приходить в грамах
            return AddReceiptWares(W);
        }

        public async Task<bool> CheckOneTimeAsync(IdReceipt pReceipt, long pCodeData, eTypeCode pTypeCode)
        {
            var RC = new OneTime(pReceipt) { CodeData = pCodeData, TypeData = pTypeCode, CodePS = db.GetCodePS(pCodeData) };
            Status<OneTime> R = null;
            if (pTypeCode == eTypeCode.OneTimeCoupon || pTypeCode == eTypeCode.OneTimeCouponGift || pTypeCode == eTypeCode.GiftCard )
            {
                R = await ds.CheckOneTime(RC);
                if (R == null || !R.status || R.Data == null || !pReceipt.Equals(R.Data))
                {
                    Global.Message?.Invoke(R == null || R.status == false || R.Data == null ? $"Проблема з перевіркою купона {pCodeData} => {R?.TextState}" :
                                    (R.Data.State>0? $"Даний купон=>{pCodeData} вже використано в чеку {R.Data.IdWorkplace}/{R.Data.NumberReceipt1C}" : 
                                    $"Купон=>{pCodeData} тимчасово недоступний. Оскільки була спроба використати в чеку => {R.Data.NumberReceipt1C} Повторно можна використати {R.Data.DateCreate.AddMinutes(5)}")                                    
                                    , eTypeMessage.Information);
                    return false;
                }
                else
                {
                    if (pTypeCode == eTypeCode.GiftCard) return true;
                }
            }
            if (pTypeCode == eTypeCode.OneTimeCouponGift)
            {
                var r = pReceipt as Receipt;
                if (r != null && (r.CodeClient == 0 || r.CodeClient != R?.Data.CodeClient))
                {
                    Global.Message?.Invoke(r.CodeClient==0? $"Відскануйте свою картку лояльності" :
                         $"Цей купон належить іншому клієнту ({r.CodeClient}/{R?.Data.CodeClient})"//$"Код клієнта  купона=>{R?.Data.CodeClient} і в чеку=>{r.CodeClient} різні"
                        , eTypeMessage.Information);
                    return false;
                }
            }

            if (RC.CodePS > 0)
            {
                if (pTypeCode == eTypeCode.OneTimeCouponGift)
                    db.ReplaceReceiptGift(new ReceiptGift(pReceipt) { CodePS = RC.CodePS, NumberGroup = 0, CodeCoupon = pCodeData, Quantity = -1 });
                db.ReplaceOneTime(RC);
                _ = db.RecalcPriceAsync(new(pReceipt));
                return true;
            }
            Global.Message?.Invoke($"Даний купон=>{pCodeData} не можна застосувати!!!", eTypeMessage.Information);
            return true;
        }

        public ReceiptWares AddWaresCode(IdReceipt pIdReceipt, long pCodeWares, int pCodeUnit, decimal pQuantity = 0, decimal pPrice = 0, bool IsFixPrice = false)
        {
            var State = GetStateReceipt(pIdReceipt);
            if (State != eStateReceipt.Prepare)
            {
                OnCustomWindow?.Invoke(new CustomWindow(eWindows.NoPrice, $"Чеку в стані {State} заборонено {Environment.NewLine} добавляти товар"));
                return null;
            }
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
                    if (W.PriceDealer == 0 && IsFixPrice)
                        W.PriceDealer = pPrice;
                    var res = AddReceiptWares(W, false);
                    if (pPrice > 0 && (W.IsMultiplePrices || IsFixPrice))
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

        public bool ChangeQuantity(IdReceiptWares pReceiptWaresId, decimal pQuantity,User pUser=null)
        {
            var State = GetStateReceipt(pReceiptWaresId);
            if (State != eStateReceipt.Prepare)
            {
                OnCustomWindow?.Invoke(new CustomWindow(eWindows.NoPrice, $"Чеку в стані {State} заборонено {Environment.NewLine} змінювати кількість"));
                return false;
            }

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
                    VR.SendMessage(w.IdWorkplace, $"{pUser?.NameUser} =>{w.NameWares}", w.Articl, w.Quantity, w.Sum, VR.eTypeVRMessage.DeleteWares);
                }
                else
                {
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
                    VR.SendMessage(w.IdWorkplace, $"{pUser?.NameUser} => {w.NameWares}", w.Articl, w.Quantity, w.Sum, VR.eTypeVRMessage.UpdateWares);
                }
                if (Global.RecalcPriceOnLine)
                    db.RecalcPriceAsync(pReceiptWaresId, pQuantity==0?pUser:null);
            }
            return res;
        }

        public Receipt GetReceiptHead(IdReceipt pIdR, bool parWithDetail = false) => db.GetReceiptHead(pIdR, parWithDetail);        

        public ModelMID.Client GetClientByCode(IdReceipt pIdReceipt, long pCode) => SetClient(pIdReceipt, db.FindClient(null, null, null, pCode));

        public ModelMID.Client GetClientByBarCode(IdReceipt pIdReceipt, string pBarCode) => SetClient(pIdReceipt, db.FindClient(pBarCode));
        public ModelMID.Client GetClientByPhone(IdReceipt pIdReceipt, string pPhone) => SetClient(pIdReceipt, db.FindClient(null, pPhone), pPhone);
        ModelMID.Client SetClient(IdReceipt pIdReceipt, IEnumerable<ModelMID.Client> pClients, string pPhone = null)
        {
            var r = pClients.Where(el => el.StatusCard == eStatusCard.Active);
            if (!r.Any())
                r=pClients;

            if (r.Count() == 0 && pPhone != null)
            {
                if (Global.Settings.IsUseCardSparUkraine)
                    GetDiscount(new FindClient() { Phone = pPhone }, pIdReceipt);
                else
                    OnCustomWindow?.Invoke(new CustomWindow(eWindows.Info, $"Клієнта з номером {pPhone} не знайдено в базі!"));
            }
            if (r.Count() == 1)
            {
                var client = r.First();
                UpdateClientInReceipt(pIdReceipt, client);
                return client;
            }
            if (Global.TypeWorkplaceCurrent != eTypeWorkplace.NotDefine && r.Count() > 1)//Якщо не модерн і є кілька клієнтів.
            {
                // Створюємо  користувацьке вікно
                var CW = new CustomWindow(eWindows.ChoiceClient, r);
                OnCustomWindow?.Invoke(CW);
            }
            return null;
        }

        private void UpdateClientInReceipt(IdReceipt pIdReceipt, ModelMID.Client pClient, bool pIsGetDiscount = true)
        {
            var State = GetStateReceipt(pIdReceipt);
            if (State != eStateReceipt.Prepare)
            {
                OnCustomWindow?.Invoke(new CustomWindow(eWindows.NoPrice, $"Чеку в стані {State} заборонено {Environment.NewLine} змінювати клієнта"));
                return;
            }

            var RH = GetReceiptHead(pIdReceipt);
            RH.CodeClient = pClient.CodeClient;
            RH.PercentDiscount = pClient.PersentDiscount;
            db.ReplaceReceipt(RH);
                       
            Global.OnClientChanged?.Invoke(pClient);
            if (Global.RecalcPriceOnLine)
                db.RecalcPriceAsync(new IdReceiptWares(pIdReceipt));
            if (pIsGetDiscount)
                GetDiscount(new FindClient() { Client = pClient, CodeWarehouse = Global.CodeWarehouse }, pIdReceipt);
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
                    //Reg = new Regex(@"^[0-9]{1,5}[,.]{0-1}[0-9]{1,5}[*]{1}[0-9]{1,8}[*]{0,1}$");
                    if (pName?.IndexOf('*') > 0 || pName?.IndexOf('=') > 0)//Reg.IsMatch(pName))
                    {
                        var s = pName.Split(pName?.IndexOf('*') > 0 ? '*' : '=');
                        int.TryParse(s[1], out int res);
                        if (res != 0)
                        {
                            Article = s[1];
                            pName = null;
                        }

                        //if (pName.EndsWith("*"))  Quantity = Convert.ToDecimal(s[0]);
                        else pName = s[1];
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

        public bool SetStateReceipt(IdReceipt pIdReceipt, eStateReceipt pStateReceipt)
        {
            if (pIdReceipt == null)
                return false;
            var receipt = new Receipt(pIdReceipt) { StateReceipt = pStateReceipt, DateReceipt = DateTime.Now, UserCreate = GetUserIdbyWorkPlace(pIdReceipt.IdWorkplace),TypeWorkplace=Global.TypeWorkplaceCurrent };
            return db.CloseReceipt(receipt);
        }

        public IEnumerable<ReceiptWares> GetWaresReceipt(IdReceipt pIdR)
        {
            if (pIdR.CodePeriod == Global.GetCodePeriod()) return db.ViewReceiptWares(pIdR);
            using var dbT = new WDB_SQLite(pIdR.DTPeriod);
            return dbT.ViewReceiptWares(pIdR);
        }


        public IEnumerable<Receipt> GetReceipts(DateTime parStartDate, DateTime parFinishDate, int IdWorkPlace)
        {
            var res = db.GetReceipts(parStartDate.Date, parFinishDate.Date.AddDays(1), IdWorkPlace).ToList();
            if (parStartDate.Date != DateTime.Now.Date || parFinishDate.Date != DateTime.Now.Date)
            {
                var Ldc = parStartDate.Date;
                while (Ldc <= parFinishDate.Date)
                {
                    using var ldb = new WDB_SQLite(Ldc);
                    var l = ldb.GetReceipts(Ldc.Date, DateTime.Now.Date.AddDays(1)/*Ldc.Date.AddDays(1)*/, IdWorkPlace);
                    res.AddRange(l);
                    Ldc = Ldc.AddDays(1);
                }
            }
            return res;
        }

        public Receipt GetReceiptByFiscalNumber(int IdWorkplace, string pFiscalNumber, DateTime pStartDate = default(DateTime), DateTime pFinishDate = default(DateTime))
        {
            if (pStartDate == default)
                pStartDate = DateTime.Now.Date.AddDays(-14);
            if (pFinishDate == default)
                pFinishDate = DateTime.Now;

            var Ldc = pStartDate.Date;
            while (Ldc <= pFinishDate.Date)
            {
                using var ldb = new WDB_SQLite(Ldc);
                var l = ldb.GetReceiptByFiscalNumber(IdWorkplace, pFiscalNumber, pStartDate, pFinishDate);
                if (l != null && l.Count() >= 1)
                    return l.First();
                Ldc = Ldc.AddDays(1);
            }
            return null;
        }

        /*public bool SaveReceiptEvents(IEnumerable<ReceiptEvent> pRE, bool pIsReplace = true)
        {
            if (pRE != null && pRE.Count() > 0)
            {
                try
                {
                    if (pIsReplace)
                        db.DeleteReceiptEvent(pRE.First());
                    db.InsertReceiptEvent(pRE);
                }
                catch (Exception e)
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = e, Status = eSyncStatus.NoFatalError, StatusDescription = "SaveReceiptEvents N=>" + pRE?.Count().ToString() + '\n' + e.Message + '\n' + new System.Diagnostics.StackTrace().ToString() });
                }
            }
            return true;
        }*/

        //public Task GetBonusAsync(Client pClient) { return ds.Ds1C.GetBonusAsync(pClient, Global.CodeWarehouse); }

        public void SendReceiptTo1C(IdReceipt parIdReceipt)
        {
            Task.Run(() => { ds.SendReceiptTo1C(parIdReceipt); });
        }

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

        public bool FixWeight(ReceiptWares pRW)
        {
            bool Res = db.FixWeight(pRW);
            var r = GetReceiptHead(pRW, true);
            Global.OnReceiptCalculationComplete?.Invoke(r);
            return Res;
        }

        public bool UpdateExciseStamp(IEnumerable<ReceiptWares> pRW)
        {
            var r = pRW.Where(e => e.ExciseStamp != null && e.ExciseStamp.Length > 0);
            try
            {
                bool Res = db.UpdateExciseStamp(r);
                var res = GetReceiptHead(pRW.First(), true);
                Global.OnReceiptCalculationComplete?.Invoke(res);
                return Res;
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
            try
            {
                var u = db?.GetUser(new User() { Login = pLogin, PassWord = pPassWord });
                if (u?.Count() > 0)
                    return u.First();
            }
            catch (Exception) { }
            return null;
        }

        public bool InsertLogRRO(LogRRO pL) { return db.InsertLogRRO(new List<LogRRO>() { pL }); }
        public bool InsertLogRRO(IEnumerable<LogRRO> pL) { return db.InsertLogRRO(pL); }

        public void AddEventAge(Receipt pRecipt)
        {
            ReceiptEvent el = new ReceiptEvent(pRecipt) { EventType = eReceiptEventType.AgeRestrictedProduct, EventName = "Вік підтверджено", CreatedAt = DateTime.Now };

            db.InsertReceiptEvent(el);
            pRecipt.ReceiptEvent = pRecipt.ReceiptEvent == null ? new List<ReceiptEvent> { el } : pRecipt.ReceiptEvent.Append<ReceiptEvent>(el);
        }
        public void AddEvent(Receipt pRecipt, eReceiptEventType pE, string text = null)
        {
            ReceiptEvent el = new ReceiptEvent(pRecipt) { EventType = pE, EventName = text ?? pE.ToString(), CreatedAt = DateTime.Now };
            db.InsertReceiptEvent(el);
            pRecipt.ReceiptEvent = pRecipt.ReceiptEvent == null ? new List<ReceiptEvent> { el } : pRecipt.ReceiptEvent.Append<ReceiptEvent>(el);
        }

        /// <summary>
        /// Cстворення повернення на основі чека.
        /// </summary>
        /// <param name="IdR">Чек на який робиться повернення</param>
        /// <returns></returns>
        public Receipt CreateRefund(IdReceipt IdR, bool pIsFull = false, bool IsRefund = true)
        {
            try
            {
                var NewR = GetNewIdReceipt(IdR.IdWorkplace);
                var R = GetReceiptHead(IdR, true);
                R.AdditionC1 = R.Payment?.Where(x=>x.IsCashBack != true)?.First()?.CodeAuthorization;
                R.AdditionCashBack = R.Payment?.Where(x => x.IsCashBack == true)?.FirstOrDefault()?.CodeAuthorization;
                R.Payment = null;
                R.ReceiptEvent = null;
                R.DateReceipt = DateTime.Now;
                if (IsRefund)
                    R.TypeReceipt = eTypeReceipt.Refund;
                R.StateReceipt = eStateReceipt.Prepare;
                R.RefundId = new IdReceipt(R);
                R.SetIdReceipt(NewR);
                db.ReplaceReceipt(R);

                foreach (var el in R.Wares)
                {
                    el.MaxRefundQuantity = el.Quantity - el.RefundedQuantity;
                    if (IsRefund && el.SumDiscount > 0)
                    {
                        el.Sum -= el.SumDiscount;
                        el.Price = Math.Round(el.Sum / el.Quantity,2,MidpointRounding.ToZero);
                        el.SumDiscount = 0;
                    }
                    if (!pIsFull)
                        el.Quantity = 0;

                    db.AddWares(el);
                }

                if (!IsRefund)
                {
                    var pr = db.GetReceiptWaresPromotion(IdR);
                    if (pr != null && pr.Any())
                    {
                        foreach (var el in pr)
                            el.SetIdReceipt(NewR);
                        db.ReplaceWaresReceiptPromotion(pr);
                    }
                }
                db.RecalcHeadReceipt(NewR);
                Global.OnReceiptCalculationComplete?.Invoke(R);
                return R;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"BL.CreateRefund Exception =>( {IdR?.ToJSON()}) => (){Environment.NewLine}Message=>{e.Message}{Environment.NewLine}StackTrace=>{e.StackTrace}", eTypeLog.Error);
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
                            switch (pCWA.IdButton)
                            {
                                case 1: //Фіксування ваги
                                    if (r != null)
                                    {
                                        var rr = new ReceiptEvent(r) { EventType = eReceiptEventType.IncorrectWeight, EventName = "Ручне підтвердження ваги", CreatedAt = DateTime.Now };
                                        db.InsertReceiptEvent(rr);
                                        FixWeight(r);
                                    }
                                    break;
                                case 2: //Добавити вагу
                                    if (r != null && r.FixWeight > 0 && r.FixWeightQuantity > 0)
                                    {
                                        FixWeight(r);
                                        // db.InsertWeight(new  { BarCode = r.CodeWares, Weight = (decimal) r.FixWeight /(r.FixWeightQuantity*1000m), Status = -1 });
                                        db.InsertAddWeight(new AddWeight { CodeWares = r.CodeWares, CodeUnit = r.CodeUnit, Weight = (decimal)r.FixWeight / (r.FixWeightQuantity * 1000m), IsManual = true });
                                    }
                                    break;
                                case 3: //видалення поточної позиції
                                    if (r != null) ChangeQuantity(r, 0);
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        public void AddOwnBag(IdReceipt pR, decimal pWeight)
        {
            var rr = new ReceiptEvent(pR) { EventType = eReceiptEventType.OwnBag, EventName = "Власна думка", ProductConfirmedWeight = Convert.ToInt32(pWeight), CreatedAt = DateTime.Now };
            db.InsertReceiptEvent(rr);

            var r = GetReceiptHead(pR, true);
            Global.OnReceiptCalculationComplete?.Invoke(r);
        }

        public eStateReceipt GetStateReceipt(IdReceipt pR) => db.GetStateReceipt(pR);

        public IEnumerable<Payment> GetPayment(IdReceipt idReceipt) => db.GetPayment(idReceipt);

        public IEnumerable<LogRRO> GetLogRRO(IdReceipt pR) => db.GetLogRRO(pR);

        public void GetDiscount(FindClient pFC, IdReceipt pIdReceipt)
        {
            Task.Run(async () =>
            {
                ModelMID.Client cl = await ds.GetDiscount(pFC);
                if (pFC.Client != null)
                {
                    //cl = await ds.Ds1C.GetBonusAsync(pFC.Client, pFC.CodeWarehouse);
                }
                else
                {
                    //cl = await ds.GetDiscount(pFC);
                    if (cl != null)
                    {
                        UpdateClientInReceipt(pIdReceipt, cl, false);
                    }
                }
                if (cl?.OneTimePromotion?.Any() == true)
                {
                    foreach (var el in cl.OneTimePromotion.Select(el => new OneTime(pIdReceipt) { CodePS = el, TypeData = eTypeCode.Client, CodeData = cl.CodeClient }))
                        db.ReplaceOneTime(el);
                    if (Global.RecalcPriceOnLine)
                        db.RecalcPriceAsync(new IdReceiptWares(pIdReceipt));
                }

                if(cl?.ReceiptGift?.Any()== true)
                {
                    foreach (var el in cl.ReceiptGift.Select(el => new ReceiptGift(pIdReceipt) { CodePS = el.CodePS, NumberGroup=el.NumberGroup,Quantity=el.Quantity}))
                        db.ReplaceReceiptGift(el);
                }

                if (cl != null)
                    Global.OnClientChanged?.Invoke(cl);
            });
        }

        IEnumerable<ReceiptWares> CheckGiftCardAsync(IdReceipt pReceipt,string pBarCode)
        {
            var c = GetClientByBarCode(pReceipt, pBarCode.ToLower());
            if (c != null) return null;
            Receipt R = pReceipt as Receipt;
            if (R != null && !R.IsAddGiftCard(pBarCode))
            {
                Global.Message?.Invoke($"Даний сертифікат =>{pBarCode} вже використаний в текучому чеку!", eTypeMessage.Information);
                return null;
            }
            if (!StaticModel.CheckGiftCard(pBarCode))
                return null;
            bool r=AsyncHelper.RunSync(()=> CheckOneTimeAsync(pReceipt, pBarCode.ToLong(), eTypeCode.GiftCard));
            if (!r) return null;
            var Type = pBarCode[1..2];
            var Ind = Type.ToInt(-1);
            
            if (Ind >= 0 && Global.Settings.CodeWaresGiftCart.Length > Ind)
            {
                var w = db.FindWares(null, null, Global.Settings.CodeWaresGiftCart[Ind]);
                if (w.Count() == 1)
                {
                    var W = w.FirstOrDefault();
                    W.Quantity = 1;
                    W.AdditionC1 = pBarCode;
                }                
                return w;
            }
            return null;
        }
    }
}