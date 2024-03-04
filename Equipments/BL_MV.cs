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

namespace Front.Equipments
{
    public partial class BLF
    {
        static BLF sBLF;
        public static BLF GetBLF { get { return sBLF ?? new BLF(); } }
        
        IMW MW; 
        BL Bl { get { return MW?.Bl; } }
        EquipmentFront EF { get { return MW?.EF; } }
        public BLF()
        {
            sBLF = this;
        }
        public void Init(IMW pMW) => MW = pMW;
        

        public static Action<eStateMainWindows, eTypeAccess, ReceiptWares, CustomWindow, eSender> OnSetStateView { get; set; }
        public void SetStateView(eStateMainWindows pSMV = eStateMainWindows.NotDefine, eTypeAccess pTypeAccess = eTypeAccess.NoDefine, ReceiptWares pRW = null, CustomWindow pCW = null, eSender pS = eSender.NotDefine)=>        
            OnSetStateView?.Invoke(pSMV, pTypeAccess, pRW, pCW, pS);        
        
        public void GetBarCode(string pBarCode, string pTypeBarCode)
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"(pBarCode=>{pBarCode},  pTypeBarCode=>{pTypeBarCode})");
            if (MW.State == eStateMainWindows.StartWindow)
                SetStateView(eStateMainWindows.WaitInput);
            if (MW.State == eStateMainWindows.WaitInputIssueCard) return;
            

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
                            Bl.GetDiscount(new FindClient { BarCode = BarCode }, MW.curReceipt);
                    }
                }
            }

            var u = Bl.GetUserByBarCode(pBarCode);
            if (u != null)
            { Bl.OnAdminBarCode?.Invoke(u); return; }

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
                if (MW.IsAddNewWares && (MW.State == eStateMainWindows.WaitInput || MW.State == eStateMainWindows.StartWindow))
                {
                    if (MW.curReceipt == null || !MW.curReceipt.IsLockChange)
                    {
                        if (MW.curReceipt == null)
                            NewReceipt();
                        w = Bl.AddWaresBarCode(MW.curReceipt, pBarCode, 1);
                        if (w != null && w.CodeWares > 0)
                        {
                            MW.CurWares = w;
                            IsPrises(1, 0);
                        }
                    }
                }
                else
                {
                    w = Bl.AddWaresBarCode(MW.curReceipt, pBarCode, 1, true);
                }
                if (w != null)
                    return;

                if (MW.curReceipt != null)
                {
                    var c = Bl.GetClientByBarCode(MW.curReceipt, pBarCode.ToLower());
                    if (c != null) return;
                }
            }

            if ((MW.State != eStateMainWindows.WaitInput && MW.State != eStateMainWindows.StartWindow) || MW.curReceipt?.IsLockChange == true || !MW.IsAddNewWares)
                if (MW.State != eStateMainWindows.ProcessPay && MW.State != eStateMainWindows.ProcessPrintReceipt && MW.State != eStateMainWindows.WaitCustomWindows)
                    SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.AdminPanel);

        }        
       
        public string GetExciseStamp(string pBarCode)
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

        public void AddExciseStamp(string pES)
        {
            if (MW.CurWares == null)
                MW.CurWares = MW.curReceipt.GetLastWares;
            if (MW.CurWares != null)
            {
                if (!"None".Equals(pES))
                {
                    if (Global.Settings.IsCheckExciseStamp)
                    {
                        var res = Bl.ds.CheckExciseStamp(new ExciseStamp(MW.CurWares, pES));
                        if (res != null)
                        {
                            if (!res.Equals(MW.CurWares) && res.State >= 0)
                            {
                                Global.Message?.Invoke($"Дана акцизна марка вже використана {res.CodePeriod} {res.IdWorkplace} Чек=>{res.CodeReceipt} CodeWares=>{res.CodeWares}!",  eTypeMessage.Error);
                                return;
                            }
                        }
                    }
                }
                if (MW.CurWares.AddExciseStamp(pES))
                {                 //Додання акцизноії марки до алкоголю
                    Bl.UpdateExciseStamp(new List<ReceiptWares>() { MW.CurWares });
                    MW.TypeAccessWait = eTypeAccess.NoDefine;
                    SetStateView(eStateMainWindows.WaitInput);
                }
                else
                    Global.Message?.Invoke("Дана акцизна марка вже використана!", eTypeMessage.Error);
            }
        }

        public void IsPrises(decimal pQuantity = 0m, decimal pPrice = 0m)
        {
            if (MW.CurWares.TypeWares == eTypeWares.Alcohol && MW.CurWares?.Price > 0m)
            {
                SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ExciseStamp, MW.CurWares);
                return;
            }

            if (MW.CurWares?.Price == 0) //Повідомлення Про відсутність ціни
            {
                SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, new CustomWindow(eWindows.NoPrice, MW.CurWares.NameWares));
            }
            if (MW.CurWares?.Prices != null && pPrice == 0m) //Меню з вибором ціни. Сигарети.
            {
                if (MW.CurWares.IsMultiplePrices)
                {
                    SetStateView(eStateMainWindows.WaitInputPrice, eTypeAccess.NoDefine, MW.CurWares);
                }
            }
            if (MW.CurWares?.IsMultiplePrices==true && pPrice > 0m)
                MW.CurWares = null;
        }

        DateTime StartScan = DateTime.MinValue;

        public void TimeScan(bool? pIsSave = null)
        {
            if ((MW.State == eStateMainWindows.WaitAdmin && !MW.CS.IsProblem) || MW.State == eStateMainWindows.AdminPanel || MW.State == eStateMainWindows.WaitAdminLogin ||
                       MW.State == eStateMainWindows.ChoicePaymentMethod || MW.State == eStateMainWindows.ProcessPay || MW.State == eStateMainWindows.StartWindow || pIsSave == true)
            {
                if (StartScan != DateTime.MinValue)
                {
                    Bl.SaveReceiptEvents(new List<ReceiptEvent>() { new ReceiptEvent(MW.curReceipt) { ResolvedAt = StartScan, EventType = eReceiptEventType.TimeScanReceipt, EventName = "Час сканування чека" } }, false);
                    StartScan = DateTime.MinValue;
                }
            }
            else
            {
                if (pIsSave == false || (StartScan == DateTime.MinValue && (
                        MW.State == eStateMainWindows.WaitInput || MW.State == eStateMainWindows.WaitFindWares || MW.State == eStateMainWindows.WaitInputPrice || MW.State == eStateMainWindows.WaitInputIssueCard)))
                    StartScan = DateTime.Now;
            }
        }
        
        public void NewReceipt()
        {
            Bl.GetNewIdReceipt();
            if (MW.curReceipt != null)
                MW.s.NewReceipt(MW.curReceipt.CodeReceipt);
            if (StartScan != DateTime.MinValue) StartScan = DateTime.Now;
            //Dispatcher.BeginInvoke(new ThreadStart(() => { ShowClientBonus.Visibility = Visibility.Collapsed; }));
            EF.PutToDisplay(MW.curReceipt);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"CodeReceipt=>{MW.curReceipt?.CodeReceipt}");
        }

    }
}
