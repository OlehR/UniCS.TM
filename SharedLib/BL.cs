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
            db.OnReceiptCalculationComplete = (wareses, guid) => OnReceiptCalculationComplete?.Invoke(wareses, guid);
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
            if (w.Count() == 1)
            {
                var W = w.First();
                if (parQuantity == 0)
                    return W;
                W.SetIdReceipt(parReceipt);
                W.Quantity = parQuantity;
                return AddReceiptWares(W);
            }
            else
                return null;

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


        public IEnumerable<ReceiptWares> GetProductsByName(string parName)
        {
            var r = db.FindWares(null, parName);
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
            SendReceiptTo1CAsync(db.ViewReceipt(parIdReceipt, true));
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
        public bool SendAllReceipt() 
        {
            var varReceipts=db.GetIdReceiptbyState( eStateReceipt.Print);
            foreach (var el in varReceipts)
                 SendReceiptTo1CAsync(db.ViewReceipt(el, true));
            return true;
        }
            
            

        public bool InsertWeight(string parBarCode,int parWeight)
        {
            return db.InsertWeight(new { BarCode = parBarCode, Weight = (decimal)parWeight / 1000m });
        }

        public async Task<bool> SyncData(bool parIsFull)
        {
            WDB_SQLite SQLite;
            if (parIsFull)
            {
                db.db.Close();
               // DateTime varD = DateTime.Today;
                string varMidFile = Path.Combine(ModelMID.Global.PathDB, @"MID.db"); /*_" + varD.ToString("yyyyMMdd") + "*/
                if (File.Exists(varMidFile))
                    File.Delete(varMidFile);
                SQLite = new WDB_SQLite(varMidFile);
                SQLite.CreateMIDTable();
            }
            else
                SQLite = db;

            var MsSQL = new WDB_MsSql();
            var resS = MsSQL.LoadData(SQLite, parIsFull);

            if (parIsFull)
            {
                SQLite.CreateMIDIndex();
                db=SQLite;                
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




