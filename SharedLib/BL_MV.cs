using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelMID;
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

        /*
        public void GetBarCode(string pBarCode, string pTypeBarCode)
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"(pBarCode=>{pBarCode},  pTypeBarCode=>{pTypeBarCode})");
            if (State == eStateMainWindows.StartWindow)
                SetStateView(eStateMainWindows.WaitInput);
            if (State == eStateMainWindows.WaitInputIssueCard)
            {
                IssueCardUC.SetBarCode(pBarCode);
                return;
            }

            //Точно треба зробити через стан eStateMainWindows
            if (Global.Settings.IsUseCardSparUkraine && NumericPad.Visibility == Visibility.Visible && "Введіть номер телефону".Equals(InputNumberPhone.Desciption))
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
                if (IsAddNewWares && (State == eStateMainWindows.WaitInput || State == eStateMainWindows.StartWindow))
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
                    w = AddWaresBarCode(curReceipt, pBarCode, 1, true);
                }
                if (w != null)
                    return;

                if (MW.curReceipt != null)
                {
                    var c = GetClientByBarCode(MW.curReceipt, pBarCode.ToLower());
                    if (c != null) return;
                }
            }

            if ((State != eStateMainWindows.WaitInput && State != eStateMainWindows.StartWindow) || MW.curReceipt?.IsLockChange == true || !IsAddNewWares)
                if (State != eStateMainWindows.ProcessPay && State != eStateMainWindows.ProcessPrintReceipt && State != eStateMainWindows.WaitCustomWindows)
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.AdminPanel);

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
