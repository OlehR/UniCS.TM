using ModelMID;
using ModelMID.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Utils;

namespace SharedLib
{
    public class DataSync1C
    {
        public SoapTo1C soapTo1C = new SoapTo1C();
        WDB_SQLite db { get { return bl?.db; } }
        BL bl;
        public DataSync1C(BL pBL) {
           // db = WDB_SQLite.GetInstance;
            bl = pBL;
        }

        public async Task<bool> SendReceiptTo1CAsync(Receipt pR, string pServer = null, bool pIsChangeState = true)
        {
           // if (!Global.Is1C) return null;
            //string Res = null;
            if (!Global.Settings.IsSend1C && pIsChangeState) return false;

            if (string.IsNullOrEmpty(pServer))
                pServer = Global.Server1C;
            try
            {
                bool IsErrorSend = false;
                if (pR.IdWorkplace != 36)
                {
                    List<int> IdWP = pR.IdWorkplacePays.Where(el => el == pR.IdWorkplace).ToList();
                    var l = pR.IdWorkplacePays.Where(el => el != pR.IdWorkplace);
                    if (l.Count() > 0)
                        IdWP.AddRange(l);

                    foreach (var el in IdWP)
                    {
                        pR.IdWorkplacePay = el;
                        var r = new Receipt1C(pR);
                        var body = soapTo1C.GenBody("JSONCheck", new Parameters[] { new Parameters("JSONSting", r.GetBase64()) });
                        var res = Global.IsTest ? "0" : await soapTo1C.RequestAsync(pServer, body, 60000, "application/json");
                        IsErrorSend |= !res.Equals("0");
                        FileLogger.WriteLogMessage(this, "SendReceiptTo1CAsync", $"({pR.NumberReceipt1C},{pR.CodePeriod},{pR.IdWorkplace},{pR.CodeReceipt})=>{res}");
                        //if (!IsErrorSend)
                        //   Res += JsonConvert.SerializeObject(r)+Environment.NewLine;
                    }
                }
                if (IsErrorSend)
                    return false;
                pR.StateReceipt = eStateReceipt.Send;
                if (pIsChangeState&& db!=null)
                    db.SetStateReceipt(pR);//Змінюєм стан чека на відправлено.                
                return true;
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = $"SendReceiptTo1CAsync=> {pR.CodeReceipt}{Environment.NewLine}{ex.Message}{Environment.NewLine}{new System.Diagnostics.StackTrace()}" });
                return false;
            }
            finally
            {
                pR.IdWorkplacePay = 0;
            }
        }
        
        public async Task<Client> GetBonusAsync(Client pClient, int pCodeWarehouse = 0)
        {
            try
            {
                var body = soapTo1C.GenBody("GetBonusSum", new Parameters[] { new Parameters("CodeOfCard", pClient.BarCode) });
                var res = await soapTo1C.RequestAsync(Global.Server1C, body,5000);
                
                if (!string.IsNullOrEmpty(res) )
                    pClient.SumBonus = res.ToDecimal(); //!!!TMP
                if (pClient.SumBonus > 0 && pCodeWarehouse > 0)
                {
                    body = soapTo1C.GenBody("GetOtovProc", new Parameters[] {
                        new Parameters("CodeOfSklad",$"{pCodeWarehouse:D9}"),
                        new Parameters("CodeOfCard", pClient.BarCode),
                        new Parameters("Summ", pClient.SumBonus.ToS())
                    });
                    res = await soapTo1C.RequestAsync(Global.Server1C, body,5000);                   
                    if (!string.IsNullOrEmpty(res) )
                    {
                        pClient.PercentBonus = res.ToDecimal() / 100m; //!!!TMP
                        pClient.SumMoneyBonus = Math.Round(pClient.SumBonus * pClient.PercentBonus, 2);
                    }
                }
                body = soapTo1C.GenBody("GetMoneySum", new Parameters[] { new Parameters("CodeOfCard", pClient.BarCode) });
                res = await soapTo1C.RequestAsync(Global.Server1C, body, 5000);                
                if (!string.IsNullOrEmpty(res) )
                    pClient.Wallet = res.ToDecimal();
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, "GetBonusAsync", ex);
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = ex.Message });
            }
            Global.OnClientChanged?.Invoke(pClient);
            return pClient;
        }

        public async Task<bool> Send1CReceiptWaresDeletedAsync(IEnumerable<ReceiptWaresDeleted1C> pRWD)
        {
            if (pRWD == null || pRWD.Count() == 0)
                return true;
            try
            {
                var d = new { data = pRWD };
                var r = JsonConvert.SerializeObject(d);
                var plainTextBytes = Encoding.UTF8.GetBytes(r);
                var resBase64 = Convert.ToBase64String(plainTextBytes);
                var body = soapTo1C.GenBody("DeletedItemsInTheCheck", new Parameters[] { new Parameters("JSONSting", resBase64) });

                var res = await soapTo1C.RequestAsync(Global.Server1C, body, 60000, "application/json");

                if (!string.IsNullOrEmpty(res) && res.Equals("0"))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                var el = pRWD.First();
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = "Send1CReceiptWaresDeletedAsync=>" + el.CodePeriod.ToString() + " " + ex.Message + '\n' + new System.Diagnostics.StackTrace().ToString() });
                return false;
            }
        }

        public async Task<eReturnClient> Send1CClientAsync(ClientNew pC)
        {
            eReturnClient Res = eReturnClient.ErrorConnect;
            if (pC == null)
                return eReturnClient.Error;
            try
            {
                var body = soapTo1C.GenBody("IssuanceOfCards", new Parameters[]
                {
                    new Parameters("CardId", pC.BarcodeClient),
                    new Parameters("User",pC.BarcodeCashier),
                    new Parameters("ShopId",Global.CodeWarehouse.ToString()),
                    new Parameters("DateOper",pC.DateCreate.ToString("yyyy-MM-dd HH:mm:ss")),
                    new Parameters("NumTel",pC.Phone),
                    new Parameters("CheckoutId",Global.IdWorkPlace.ToString()),
                    new Parameters("TypeOfOperation","0")
                });

                var res = await soapTo1C.RequestAsync(Global.Server1C, body, 5000, "application/json");

                if (!string.IsNullOrEmpty(res))
                {
                    int r = 0;
                    if (int.TryParse(res, out r))
                    {
                        Res = (eReturnClient)r;
                    }
                    else
                        Res = eReturnClient.Error;
                }
                 FileLogger.WriteLogMessage(this, "Send1CClientAsync", $"{body}=>{res}");
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            return Res;
        }

        public async Task<bool> IsUseDiscountBarCode(string pBarCode)
        {
            var body = soapTo1C.GenBody("GetRestOfLabel", new Parameters[] { new Parameters("CodeOfLabel", pBarCode) });
            var res = await soapTo1C.RequestAsync(Global.Server1C, body, 2000);
            return res.Equals("1");
        }
    }
}
