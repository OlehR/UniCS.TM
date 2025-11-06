using ModelMID;
using ModelMID.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace SharedLib
{
    public class DataSync1C
    {
        public SoapTo1C soapTo1C = new SoapTo1C();
        
        public static async Task<bool> SendReceiptTo1CAsync(Receipt pR,  WDB_SQLite db=null) 
        {
           // if (!Global.Is1C) return null;
            //string Res = null;
            if (db != null && !Global.Settings.IsSend1C ) return false; //&& pIsChangeState

            //if (string.IsNullOrEmpty(pServer))
            var pServer = Global.Server1C;
            try
            {
                bool IsErrorSend = false;
               // if (pR.IdWorkplace != 36)
                {
                    List<int> IdWP = pR.IdWorkplacePays.Where(el => el == pR.IdWorkplace).ToList();
                    var l = pR.IdWorkplacePays.Where(el => el != pR.IdWorkplace);
                    if (l.Count() > 0)
                        IdWP.AddRange(l);

                    foreach (var el in IdWP)
                    {
                        pR.IdWorkplacePay = el;
                        var r = new Receipt1C(pR);
                        var body = SoapTo1C.GenBody("JSONCheck", new Parameters[] { new Parameters("JSONSting", r.GetBase64()) });
                        var res = Global.IsTest ? "0" : await SoapTo1C.RequestAsync(pServer, body, 60000, "application/json");
                        IsErrorSend |= !res.Equals("0");
                        FileLogger.WriteLogMessage($"DataSync1C\\SendReceiptTo1CAsync ({pR.NumberReceipt1C},{pR.CodePeriod},{pR.IdWorkplace},{pR.CodeReceipt})=>{res} Body=>{body}");
                        //if (!IsErrorSend)
                        //   Res += JsonConvert.SerializeObject(r)+Environment.NewLine;
                    }
                }
                if (IsErrorSend)
                    return false;
                pR.StateReceipt = eStateReceipt.Send;
                //if (pIsChangeState&& db!=null)
                    db?.SetStateReceipt(pR);//Змінюєм стан чека на відправлено.                
                return true;
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"DataSync1C\\SendReceiptTo1CAsync", ex);
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = $"SendReceiptTo1CAsync=> {pR.CodeReceipt}{Environment.NewLine}{ex.Message}{Environment.NewLine}{new System.Diagnostics.StackTrace()}" });
                return false;
            }
            finally
            {
                pR.IdWorkplacePay = 0;
            }
        }
        
        public static async Task<Client> GetBonusAsync(Client pClient, int pCodeWarehouse = 0)
        {
            try
            {
                var body = SoapTo1C.GenBody("GetBonusSum", new Parameters[] { new Parameters("CodeOfCard", pClient.BarCode) });
                var res = await SoapTo1C.RequestAsync(Global.Server1C, body,5000);
                
                if (!string.IsNullOrEmpty(res) )
                    pClient.SumBonus = res.ToDecimal(); //!!!TMP
                if (pClient.SumBonus > 0 && pCodeWarehouse > 0)
                {
                    body = SoapTo1C.GenBody("GetOtovProc", new Parameters[] {
                        new Parameters("CodeOfSklad",$"{pCodeWarehouse:D9}"),
                        new Parameters("CodeOfCard", pClient.BarCode),
                        new Parameters("Summ", pClient.SumBonus.ToS())
                    });
                    res = await SoapTo1C.RequestAsync(Global.Server1C, body,5000);                   
                    if (!string.IsNullOrEmpty(res) )
                    {
                        pClient.PercentBonus = res.ToDecimal() / 100m; //!!!TMP
                        pClient.SumMoneyBonus = Math.Round(pClient.SumBonus * pClient.PercentBonus, 2);
                    }
                }
                body = SoapTo1C.GenBody("GetMoneySum", new Parameters[] { new Parameters("CodeOfCard", pClient.BarCode) });
                res = await SoapTo1C.RequestAsync(Global.Server1C, body, 5000);                
                if (!string.IsNullOrEmpty(res) )
                    pClient.Wallet = res.ToDecimal();
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage("DataSync1C\\GetBonusAsync", ex);
                Global.OnSyncInfoCollected?.Invoke(new SyncInformation { Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = ex.Message });
            }
            Global.OnClientChanged?.Invoke(pClient);
            return pClient;
        }

        public static async Task<bool> Send1CReceiptWaresDeletedAsync(IEnumerable<ReceiptWaresDeleted1C> pRWD)
        {
            if (pRWD == null || pRWD.Count() == 0)
                return true;
            try
            {
                var d = new { data = pRWD };
                var r = JsonConvert.SerializeObject(d);
                var plainTextBytes = Encoding.UTF8.GetBytes(r);
                var resBase64 = Convert.ToBase64String(plainTextBytes);
                var body = SoapTo1C.GenBody("DeletedItemsInTheCheck", new Parameters[] { new Parameters("JSONSting", resBase64) });

                var res = await SoapTo1C.RequestAsync(Global.Server1C, body, 60000, "application/json");

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

        public static  async Task<eReturnClient> Send1CClientAsync(ClientNew pC)
        {
            eReturnClient Res = eReturnClient.ErrorConnect;
            if (pC == null)
                return eReturnClient.Error;
            try
            {
                var body = SoapTo1C.GenBody("IssuanceOfCards", new Parameters[]
                {
                    new Parameters("CardId", pC.BarcodeClient),
                    new Parameters("User",pC.BarcodeCashier),
                    new Parameters("ShopId",Global.CodeWarehouse.ToString()),
                    new Parameters("DateOper",pC.DateCreate.ToString("yyyy-MM-dd HH:mm:ss")),
                    new Parameters("NumTel",pC.Phone),
                    new Parameters("CheckoutId",Global.IdWorkPlace.ToString()),
                    new Parameters("TypeOfOperation","0")
                });

                var res = await SoapTo1C.RequestAsync(Global.Server1C, body, 5000, "application/json");

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
                 FileLogger.WriteLogMessage($"DataSync1C\\Send1CClientAsync{body}=>{res}");
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage("DataSync1C\\Send1CClientAsync", System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            return Res;
        }

        public static async Task<bool> IsUseDiscountBarCode(string pBarCode)
        {
            var body = SoapTo1C.GenBody("GetRestOfLabel", new Parameters[] { new Parameters("CodeOfLabel", pBarCode) });
            var res = await SoapTo1C.RequestAsync(Global.Server1C, body, 2000);
            return res.Equals("1");
        }
    }
}
