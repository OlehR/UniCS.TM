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
        public SoapTo1C soapTo1C;
        //public Action<IEnumerable<ReceiptWares>, Guid> OnReceiptCalculationComplete { get; set; }

        /// <summary>
        /// Для швидкого пошуку 
        /// </summary>
        SortedList<Guid, int> WorkId;
        public BL()
        {
            db = new WDB_SQLite();
            WorkId = new SortedList<Guid, int>();
         //   Global.OnReceiptCalculationComplete = (wareses, guid) => OnReceiptCalculationComplete?.Invoke(wareses, guid);
            soapTo1C = new SoapTo1C();


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
                                CheckDiscountBarCodeAsync(parReceipt, parBarCode, varCode);
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

        public async Task<bool> CheckDiscountBarCodeAsync(IdReceipt parIdReceipt, string parBarCode, int parPercent)
        {
            try
            {
                var Cat2 = db.CheckLastWares2Cat(parIdReceipt);
                if (Cat2 == null || Cat2.Count() == 0)
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(parIdReceipt.IdWorkplace),  Status = eSyncStatus.IncorectProductForDiscount });
                    return false;
                }
                Cat2.First().BarCode2Category = parBarCode;
                Cat2.First().Price = Cat2.First().Price * (100m-(decimal)parPercent) / 100m;

                bool isGood = true;
                try
                {
                    var body=soapTo1C.GenBody("GetRestOfLabel", new Parameters[] { new Parameters("CodeOfLabel", parBarCode)  });
                    var  res = await soapTo1C.RequestAsync( Global.Server1C, body);
                    isGood = res.Equals("1");

                    /*     string body = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                      "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd = \"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\n" +
                         $"<soap:Body>\n<GetRestOfLabel xmlns=\"vopak\">\n<CodeOfLabel>{parBarCode}</CodeOfLabel> \n </GetRestOfLabel> \n </soap:Body>\n </soap:Envelope>";

                         HttpClient client = new HttpClient();
                         client.Timeout = TimeSpan.FromMilliseconds(5000);
                         // Add a new Request Message
                         HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, Global.Server1C);
                         //requestMessage.Headers.Add("Accept", "application/vnd.github.v3+json");
                         // Add our custom headers
                         requestMessage.Content = new StringContent(body, Encoding.UTF8, "text/xml");
                         var response = await client.SendAsync(requestMessage);

                         if (response.IsSuccessStatusCode)
                         {
                             var res = await response.Content.ReadAsStringAsync();
                             res = res.Substring(res.IndexOf(@"-instance"">") + 11);
                             res = res.Substring(0, res.IndexOf("</m:return>")).Trim();
                             isGood = res.Equals("1");
                         }*/
                    Global.ErrorDiscountOnLine=0;
                }
                catch (Exception ex)
                {
                    Global.ErrorDiscountOnLine++;
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId=Global.GetTerminalIdByIdWorkplace(parIdReceipt.IdWorkplace), Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = ex.Message });
                    Global.OnStatusChanged?.Invoke(db.GetStatus());

                }

                if (isGood)
                {

                    //Cat2.First()._Sum = Cat2.First().Sum; //Трохи костиль !!!!
                    //Cat2.First().Quantity = 0;
                    db.ReplaceWaresReceiptPromotion(Cat2);
                    db.InsertBarCode2Cat(Cat2.First());
                    db.RecalcHeadReceipt(parIdReceipt);
                    var r = ViewReceiptWares(new IdReceiptWares(parIdReceipt, 0));//вертаємо весь чек.
                    Global.OnReceiptCalculationComplete?.Invoke(r, Global.GetTerminalIdByIdWorkplace(parIdReceipt.IdWorkplace));
                }
                else 
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(parIdReceipt.IdWorkplace), Status = eSyncStatus.IncorectDiscountBarcode });
                    return false;
                }
            }

            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(parIdReceipt.IdWorkplace), Exception = ex, Status = eSyncStatus.Error, StatusDescription = ex.Message });
            }
            return true;
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
            _ = GetBonusAsync(parClient, Global.GetTerminalIdByIdWorkplace(idReceipt.IdWorkplace));
        }


        public IEnumerable<ReceiptWares> GetProductsByName(IdReceipt parReceipt, string parName, int parOffSet = -1, int parLimit = 10)
        {
            parName = parName.Trim();
            // Якщо пошук по штрихкоду і назва похожа на штрихкод або артикул
            if (!string.IsNullOrEmpty(parName))
            {
                var Reg = new Regex(@"^[0-9]{5,13}$");
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


            var r = db.FindWares(null, parName, 0, 0, 0, -1, parOffSet, parLimit);
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

        public bool SendReceiptTo1C(IdReceipt parIdReceipt)
        {
            var r = db.ViewReceipt(parIdReceipt, true);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            SendReceiptTo1CAsync(r,db);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return true;
        }
        public async Task<bool> SendReceiptTo1CAsync(Receipt parReceipt, WDB_SQLite parDB )
        {
            try
            {
                var r = new Receipt1C(parReceipt);

                var body = soapTo1C.GenBody("JSONCheck", new Parameters[] { new Parameters("JSONSting", r.GetBase64()) });
                var res = await soapTo1C.RequestAsync(Global.Server1C, body,10000, "application/json");
                
/*
                 HttpClient client = new HttpClient();

                 // Add a new Request Message
                 HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, Global.Server1C);
                 //requestMessage.Headers.Add("Accept", "application/vnd.github.v3+json");
                 // Add our custom headers
                 requestMessage.Content = new StringContent(//r.GetSOAP() 
                     body, Encoding.UTF8, "application/json");
                 var response = await client.SendAsync(requestMessage);

                 if (response.IsSuccessStatusCode)
                 {
                     var res = await response.Content.ReadAsStringAsync();
                     parReceipt.StateReceipt = eStateReceipt.Send;
                     db.SetStateReceipt(parReceipt);//Змінюєм стан чека на відправлено.
                     return true;
                 }
                 else
                 {
                     return false;
                 }
                
                */
                if (!string.IsNullOrEmpty(res) && res.Equals("0"))
                {
                    parReceipt.StateReceipt = eStateReceipt.Send;
                    parDB.SetStateReceipt(parReceipt);//Змінюєм стан чека на відправлено.
                }

                return res.Equals("0");
            }
            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = Global.GetTerminalIdByIdWorkplace(parReceipt.IdWorkplace), Exception =  ex, Status = eSyncStatus.NoFatalError, StatusDescription = parReceipt.ReceiptId.ToString() + " " + ex.Message });
                return false;
            }
        }
        public async Task<bool> SendAllReceipt(WDB_SQLite parDB = null)
        {
            var varDB = (parDB == null? db : parDB);
            var varReceipts = varDB.GetIdReceiptbyState(eStateReceipt.Print);
            foreach (var el in varReceipts)
                await SendReceiptTo1CAsync(varDB.ViewReceipt(el, true), varDB);
            if(parDB == null)
                SendOldReceipt();
            Global.OnStatusChanged?.Invoke(db.GetStatus());
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


        //async Task<bool>
        public bool SyncData(bool parIsFull)
        {
            try
            {
                WDB_SQLite SQLite;

                if (!parIsFull)                    
                {                    
                    var TD = db.GetConfig<DateTime>("Load_Full");
                    if (TD == default(DateTime) || DateTime.Now.Date != TD.Date)
                        parIsFull = true;                    
                }
                string varMidFile = db.GetCurrentMIDFile;

                if (parIsFull)
                {

                    DateTime varD = DateTime.Today;

                    if (File.Exists(varMidFile))
                    {
                        db.db.Close();
                        File.Delete(varMidFile);
                    }
                    SQLite = new WDB_SQLite(varMidFile, true);
                    //SQLite.CreateMIDTable();
                }
                else
                    SQLite = db;

                var MsSQL = new WDB_MsSql();
                var resS = MsSQL.LoadData(SQLite, parIsFull);

                if (parIsFull)
                {
                    Console.WriteLine("CreateMIDIndex Start");
                    SQLite.CreateMIDIndex();
                    Console.WriteLine("CreateMIDIndex Finish");
                    db = SQLite;
                    db.SetConfig<string>("Last_MID", varMidFile);
                    Console.WriteLine("Set config");
                }
                db.SetConfig<DateTime>("Load_" + (parIsFull ? "Full" : "Update"), DateTime.Now /*String.Format("{0:u}", DateTime.Now)*/);
            }
            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation {  Exception = ex, Status = eSyncStatus.Error, StatusDescription = ex.Message });
                Global.OnStatusChanged?.Invoke(db.GetStatus());

                return false;
            }
            return true;
        }
        public class TableStruc
        {
            public int Cid { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Dflt_value { get; set; }
            public int PK { get; set; }
        }
        public string BildSqlUpdate(string parTableName)
        {
            var r = db.db.Execute<TableStruc>($"PRAGMA table_info('{parTableName}');");
            var ListField = "";
            var Where = "";
            var On = "";

            foreach (var el in r)
            {
                ListField += (ListField.Length > 0 ? ", " : "") + el.Name;
                if (el.PK == 1)
                    On += (On.Length > 0 ? " and " : "") + $"main.{el.Name}=upd.{el.Name}";
                else
                    Where += (Where.Length > 0 ? " or " : "") + $"main.{el.Name}!=upd.{el.Name}";
            }

            var Res = $"replace parTableName ({ListField}) \n select {ListField} from main.{parTableName}\n join upd.{parTableName} on ( {On})\n where {Where}";
            return Res;
        }

        public void SendOldReceipt()
        {
            var Ldc = db.GetConfig<DateTime>("LastDaySend");
            var today = DateTime.Now.Date;

            if (Ldc == default(DateTime))
                Ldc = today.AddDays(-10);

            while (Ldc < today)
            {
                var ldb = new WDB_SQLite(null, false, Ldc);
                var t = SendAllReceipt(ldb);
                t.Wait();
                var  res = ldb.GetIdReceiptbyState(eStateReceipt.Print);
                if (res.Count() == 0)
                    db.SetConfig<DateTime>("LastDaySend", Ldc);
                else
                {
                    Global.OnSyncInfoCollected?.Invoke(new SyncInformation {  Status = eSyncStatus.NoFatalError, StatusDescription = $"SendOldReceipt => ErrorSend data {Ldc} Not Send => {res.Count()}" });
                    return;
                }

                Ldc = Ldc.AddDays(1);                
            }
            //Перекидаємо лічильник на сьогодня.
            db.SetConfig<DateTime>("LastDaySend", Ldc);

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

         public async Task GetBonusAsync(Client parClient,Guid parTerminalId)
        {
            try
            {
                decimal Sum;
                var body = soapTo1C.GenBody("GetBonusSum", new Parameters[] { new Parameters("CodeOfCard", parClient.BarCode) });
                var res = await soapTo1C.RequestAsync(Global.Server1C, body);
                if (!string.IsNullOrEmpty(res) && decimal.TryParse(res, out Sum))
                    parClient.SumBonus = Sum;
                body = soapTo1C.GenBody("GetMoneySum", new Parameters[] { new Parameters("CodeOfCard", parClient.BarCode) });
                res = await soapTo1C.RequestAsync(Global.Server1C, body);
                res = res.Replace('.', ',');
                if (!string.IsNullOrEmpty(res) && decimal.TryParse(res, out Sum))
                    parClient.Wallet = Sum;
                Global.OnClientChanged?.Invoke(parClient, parTerminalId);
            }
            catch (Exception ex)
            {
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = parTerminalId, Exception = ex, Status = eSyncStatus.NoFatalError , StatusDescription = ex.Message });
            }

            //return parClient;
        }

        public void LoadWeightKasa(DateTime parDT = default(DateTime))
        {
            if (parDT == default(DateTime))
            {
                parDT =  db.GetConfig<DateTime>("Load_Full");
                var parDTU = db.GetConfig<DateTime>("Load_Update");
                if (parDTU > parDT)
                    parDT = parDTU;
            }

            string SQLUpdate = @"
-- begin tran
   update barcode_out with (serializable) set weight=@Weight,Date=@Date
   where bar_code = @BarCode

   if @@rowcount = 0
   begin
      insert into barcode_out (bar_code, weight,Date) values ( @BarCode,@Weight,@Date)
   end
-- commit tran";

            var dbMs = new MSSQL();
            var path = Path.Combine(ModelMID.Global.PathDB, "config.db");
            var dbSqlite = new SQLite(path);
            var SqlSelect = @"select [barcode] as BarCode, weight as Weight, DATE_CREATE as Date,STATUS
from ( SELECT [barcode],DATE_CREATE,STATUS,weight,ROW_NUMBER() OVER (PARTITION BY barcode ORDER BY DATE_CREATE DESC) as [nn]  FROM WEIGHT where  DATE_CREATE>=:parDT ) dd
where nn=1 ";
            Console.WriteLine("Start LoadWeightKasa");
            var r = dbSqlite.Execute<object,BarCodeOut>(SqlSelect,new { parDT = parDT });
            Console.WriteLine(r.Count().ToString());
            dbMs.BulkExecuteNonQuery<BarCodeOut>(SQLUpdate, r);
            Console.WriteLine("Finish LoadWeightKasa");
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

    }
    


}




