using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ModelMID;
using ModelMID.DB;
using SharedLib;
using Utils;

namespace SharedLib
{
    public partial class BL
    {
        IMW MW; 
        public static Action<eStateMainWindows, eTypeAccess, ReceiptWares, CustomWindow, eSender> OnSetStateView { get; set; }
        public void SetStateView(eStateMainWindows pSMV = eStateMainWindows.NotDefine, eTypeAccess pTypeAccess = eTypeAccess.NoDefine, ReceiptWares pRW = null, CustomWindow pCW = null, eSender pS = eSender.NotDefine)
        {
            OnSetStateView?.Invoke(pSMV, pTypeAccess, pRW, pCW, pS);
        }

        /*public void GetBarCode(string pBarCode, string pTypeBarCode)
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"(pBarCode=>{pBarCode},  pTypeBarCode=>{pTypeBarCode})");
            if (MW.State == eStateMainWindows.StartWindow)
                SetStateView(eStateMainWindows.WaitInput);
            if (MW.State == eStateMainWindows.WaitInputIssueCard)
            {
                IssueCardUC.SetBarCode(pBarCode);
                return;
            }

            if (Global.Settings.IsUseCardSparUkraine && MW.State == eStateMainWindows.FindClientByPhone)
            {
                // pBarCode = "MTE2MmZlMGNjLTNlZmQtNDYxZC05NThiLTFjYmI3NjQ4YjM1NDIzLjAxLjIwMjQgMTM6MDE6Mjg=";
                if (pBarCode.Length > 56)
                {
                    var QR = pBarCode.FromBase64();
                    if (!string.IsNullOrEmpty(QR) && "1".Equals(QR[..1]) && QR.Length >= 56)
                    {
                        string BarCode = QR[1..37];
                        string Time = QR[37..56];
                        DateTime dt = Time.ToDateTime("dd.MM.yyyy HH:mm:ss");
                        if ((DateTime.Now - dt).TotalSeconds < 120)
                            GetDiscount(new FindClient { BarCode = BarCode }, MW.curReceipt);
                    }
                }
            }

            var u = GetUserByBarCode(pBarCode);
            if (u != null)
            { OnAdminBarCode?.Invoke(u); return; }

            if (MW.TypeAccessWait == eTypeAccess.ExciseStamp)
            {
                string ExciseStamp = GetExciseStamp(pBarCode);
                if (!string.IsNullOrEmpty(ExciseStamp))
                {
                    AddExciseStamp(ExciseStamp);
                    return;
                }
            }
            else
            {
                ReceiptWares w = null;
                if (IsAddNewWares && (MW.State == eStateMainWindows.WaitInput || MW.State == eStateMainWindows.StartWindow))
                {
                    if (MW.curReceipt == null || !MW.curReceipt.IsLockChange)
                    {
                        if (MW.curReceipt == null)
                            NewReceipt();
                        w = AddWaresBarCode(MW.curReceipt, pBarCode, 1);
                        if (w != null && w.CodeWares > 0)
                        {
                            MW.CurWares = w;
                            IsPrises(1, 0);
                        }
                    }
                }
                else
                {
                    w = AddWaresBarCode(MW.curReceipt, pBarCode, 1, true);
                }
                if (w != null)
                    return;

                if (MW.curReceipt != null)
                {
                    var c = GetClientByBarCode(MW.curReceipt, pBarCode.ToLower());
                    if (c != null) return;
                }
            }

            if ((MW.State != eStateMainWindows.WaitInput && MW.State != eStateMainWindows.StartWindow) || MW.curReceipt?.IsLockChange == true || !IsAddNewWares)
                if (MW.State != eStateMainWindows.ProcessPay && MW.State != eStateMainWindows.ProcessPrintReceipt && MW.State != eStateMainWindows.WaitCustomWindows)
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.AdminPanel);

        }
*/
        private string GetExciseStamp(string pBarCode)
        {
            if (pBarCode.Contains("t.gov.ua"))
            {
                string Res = pBarCode.Substring(pBarCode.IndexOf("t.gov.ua") + 9);
                pBarCode = Res.Substring(0, Res.Length - 11);
            }

            Regex regex = new Regex(@"^\w{4}[0-9]{6}?$");
            if (regex.IsMatch(pBarCode))
                return pBarCode;
            return null;
        }

        /*void AddExciseStamp(string pES)
        {
            if (MW.CurWares == null)
                MW.CurWares = MW.curReceipt.GetLastWares;
            if (MW.CurWares != null)
            {
                if (!"None".Equals(pES))
                {
                    if (Global.Settings.IsCheckExciseStamp)
                    {
                        var res = ds.CheckExciseStamp(new ExciseStamp(MW.CurWares, pES));
                        if (res != null)
                        {
                            if (!res.Equals(MW.CurWares) && res.State >= 0)
                            {
                                CustomMessage.Show($"Дана акцизна марка вже використана {res.CodePeriod} {res.IdWorkplace} Чек=>{res.CodeReceipt} CodeWares=>{res.CodeWares}!", "Увага", eTypeMessage.Error);
                                return;
                            }
                        }
                    }
                }
                if (MW.CurWares.AddExciseStamp(pES))
                {                 //Додання акцизноії марки до алкоголю
                    UpdateExciseStamp(new List<ReceiptWares>() { MW.CurWares });
                    MW.TypeAccessWait = eTypeAccess.NoDefine;
                    SetStateView(eStateMainWindows.WaitInput);
                }
                else
                    CustomMessage.Show("Дана акцизна марка вже використана!", "Увага", eTypeMessage.Error);
            }
        }*/

        void IsPrises(decimal pQuantity = 0m, decimal pPrice = 0m)
        {
            if (MW.CurWares.TypeWares == eTypeWares.Alcohol && MW.CurWares?.Price > 0m)
            {
                SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ExciseStamp, MW.CurWares);
                return;
            }

            if (MW.CurWares.Price == 0) //Повідомлення Про відсутність ціни
            {
                SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, new CustomWindow(eWindows.NoPrice, MW.CurWares.NameWares));
            }
            if (MW.CurWares.Prices != null && pPrice == 0m) //Меню з вибором ціни. Сигарети.
            {
                if (MW.CurWares.IsMultiplePrices)
                {
                    SetStateView(eStateMainWindows.WaitInputPrice, eTypeAccess.NoDefine, MW.CurWares);
                }
            }
            if (MW.CurWares.IsMultiplePrices && pPrice > 0m)
                MW.CurWares = null;
        }


    }
}
