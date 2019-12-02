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

namespace SharedLib
{

    public class BL
    {
        public WDB_SQLite db;

        public Action<IEnumerable<ReceiptWares>, Guid> OnReceiptCalculationComplete { get; set; }

        /// <summary>
        /// Для швидкого пошуку 
        /// </summary>
        SortedList<Guid, int> WorkId;
        public BL()
        {
            db = new WDB_SQLite();
            WorkId = new SortedList<Guid, int>();
            WDB_SQLite.OnReceiptCalculationComplete = (wareses, guid) => OnReceiptCalculationComplete?.Invoke(wareses, guid);
        }
        public ReceiptWares AddReceiptWares(ReceiptWares parW)
        {
            var Quantity = db.GetCountWares(parW);

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
            return db.GetNewCodeReceipt(idReceip);
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
            var w = db.FindWares(parBarCode);
            //ReceiptWares W = null;
            if (w == null || w.Count() == 0) // Якщо не знайшли спробуем по ваговим і штучним штрихкодам.          
            {
                foreach (var el in Global.CustomerBarCode.Where(el => el.KindBarCode == eKindBarCode.EAN13 /*&& (el.TypeBarCode == eTypeBarCode.WaresWeight || el.TypeBarCode == eTypeBarCode.WaresUnit )*/))
                {
                    w = null;
                    if (el.Prefix.Equals(parBarCode.Substring(0, el.Prefix.Length)))
                    {
                        int varCode = Convert.ToInt32(parBarCode.Substring(el.Prefix.Length, el.LenghtCode));
                        int varValue= Convert.ToInt32(parBarCode.Substring(el.Prefix.Length+el.LenghtCode,el.LenghtQuantity));
                        switch(el.TypeCode)
                        {
                            case eTypeCode.Article:
                                w= db.FindWares(null, null, 0, 0, 0, varCode);
                                break;
                            case eTypeCode.Code:
                                w = db.FindWares(null, null, varCode);
                                break;
                            case eTypeCode.PercentDiscount:
                                CheckDiscountBarCodeAsync(parReceipt,parBarCode, varCode);
                                return null;
                            default:
                                break;
                        }
                        if (parQuantity>0 && w != null && w.Count() == 1) //Знайшли що треба
                        {
                            parQuantity = (w.First().CodeUnit == Global.WeightCodeUnit ? varValue / 1000m : varValue);
                            break;
                        }
                    }
                }
               
            }

            if(w == null || w.Count() != 1)
                return null;
            var W = w.First();
            if (parQuantity == 0)
                return W;
            W.SetIdReceipt(parReceipt);
            W.Quantity = parQuantity;
            return AddReceiptWares(W);
        }

        public async Task<bool> CheckDiscountBarCodeAsync(IdReceipt parIdReceipt, string parBarCode, int parPercent)
        {
            var Cat2 = db.CheckLastWares2Cat(parIdReceipt);
            if (Cat2 == null && Cat2.Count() != 0)
                return false;
            Cat2.First().BarCode2Category = parBarCode;
            Cat2.First().Price = Cat2.First().Price * (decimal)parPercent / 100m;

            bool isGood = true;
            try
            {
                string body = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                             "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd = \"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\n" +
                $"<soap:Body>\n<GetRestOfLabel xmlns=\"vopak\">\n<CodeOfLabel>{parBarCode}</CodeOfLabel> \n </GetRestOfLabel> \n </soap:Body>\n </soap:Envelope>";

                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMilliseconds(5000);
                // Add a new Request Message
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://1CSRV/utppsu/ws/ws1.1cws");
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
                }
            }
            catch (Exception ex)
            {
                var t = ex.Message;
            }

            if (isGood)
            {
                db.ReplaceWaresReceiptPromotion(Cat2); 
                db.InsertBarCode2Cat(Cat2.First());
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
                    W.Quantity = parQuantity;
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
            var W = db.FindWares(null, null, parReceiptWaresId.CodeWares, parReceiptWaresId.CodeUnit);
            if (W.Count() == 1)
            {
                if (parQuantity == 0)
                    db.DeleteReceiptWares(parReceiptWaresId);
                else
                {
                    var w = W.First();
                    w.SetIdReceiptWares(parReceiptWaresId);
                    w.Quantity = parQuantity;
                    return db.UpdateQuantityWares(w);
                }
                if (ModelMID.Global.RecalcPriceOnLine)
                    db.RecalcPriceAsync(parReceiptWaresId);

            }
            return false;

        }
        public Receipt GetReceiptHead(IdReceipt idReceipt)
        {
            return db.ViewReceipt(idReceipt);
        }

        public Client GetClientByBarCode(IdReceipt idReceipt, string parBarCode)
        {
            var r = db.FindClient(parBarCode);
            if (r.Count() == 1)
            {
                var client = r.First();
                UpdateClientInReceipt(idReceipt, client);
                if (ModelMID.Global.RecalcPriceOnLine)
                    db.RecalcPriceAsync(new IdReceiptWares(idReceipt));
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
        }


        public IEnumerable<ReceiptWares> GetProductsByName(IdReceipt parReceipt, string parName, int parOffSet = -1, int parLimit = 10)
        {
            parName = parName.Trim();
            // Якщо пошук по штрихкоду і назва похожа на штрихкод або артикул
            if (!string.IsNullOrEmpty(parName) )
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


            var r = db.FindWares(null, parName,0,0,0,-1, parOffSet, parLimit);
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
            SendReceiptTo1CAsync(r);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return true;            
        }
        public async Task<bool> SendReceiptTo1CAsync(Receipt parReceipt)
        {
            var r = new Receipt1C(parReceipt);
            HttpClient client = new HttpClient();

            // Add a new Request Message
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, Global.Server1C);
            //requestMessage.Headers.Add("Accept", "application/vnd.github.v3+json");
            // Add our custom headers
            requestMessage.Content = new StringContent(r.GetSOAP(), Encoding.UTF8, "application/json");
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
        }
        public async Task<bool> SendAllReceipt() 
        {
            var varReceipts=db.GetIdReceiptbyState( eStateReceipt.Print);
            foreach (var el in varReceipts)
                  await SendReceiptTo1CAsync(db.ViewReceipt(el, true));
            return true;
        }
            
            

        public bool InsertWeight(string parBarCode,int parWeight)
        {
            return db.InsertWeight(new { BarCode = parBarCode, Weight = (decimal)parWeight / 1000m });
        }


        //async Task<bool>
        public bool SyncData(bool parIsFull)
        {
            WDB_SQLite SQLite;
            
            if (!parIsFull)
            {
                var strTD = db.GetConfig<string>("Load_Full");
                if (strTD == null || strTD.Length < 10)
                    parIsFull = true;
                else
                {
                    var dt = DateTime.Parse(strTD.Substring(0, 10));
                    if (DateTime.Now.Date != dt.Date)
                        parIsFull = true;
                }
            }
            string varMidFile = db.GetCurrentMIDFile;
         
            if (parIsFull)
            {
                db.db.Close();
                DateTime varD = DateTime.Today;
                
                if (File.Exists(varMidFile))
                    File.Delete(varMidFile);
                SQLite = new WDB_SQLite(varMidFile,true);
                //SQLite.CreateMIDTable();
            }
            else
                SQLite = db;

            var MsSQL = new WDB_MsSql();
            var resS = MsSQL.LoadData(SQLite, parIsFull);

            if (parIsFull)
            {
                try
                {
                    SQLite.CreateMIDIndex();
                    db = SQLite;
                    db.SetConfig<string>("Last_MID", varMidFile);
                }
                catch (Exception ex)
                {
                    var er=ex.Message;
                }
            }
            db.SetConfig<string>("Load_" + (parIsFull ? "Full" : "Update"), String.Format("{0:u}", DateTime.Now));

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
           var r= db.db.Execute<TableStruc>($"PRAGMA table_info('{parTableName}');");
            var ListField="";
            var Where = "";
            var On = "";

            foreach(var el in r)
            {
                ListField+= (ListField.Length > 0 ? ", " : "") +el.Name ;
                if (el.PK==1)
                    On += (On.Length>0? " and ":"")+  $"main.{el.Name}=upd.{el.Name}";
                else
                    Where += (Where.Length > 0 ? " or " : "")+ $"main.{el.Name}!=upd.{el.Name}";
            }
 
            var Res= $"replace parTableName ({ListField}) \n select {ListField} from main.{parTableName}\n join upd.{parTableName} on ( {On})\n where {Where}";
            return Res;
        }
    }
}




