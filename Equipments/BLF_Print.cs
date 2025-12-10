using Front.Equipments;
using ModelMID.DB;
using ModelMID;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using UtilNetwork;
using System.Net;

namespace Front.Equipments
{
    public partial class BLF
    {
        public void PayAndPrint()
        {
            foreach (int el in MW.curReceipt.IdWorkplacePays)
            {
                MW.curReceipt.IdWorkplacePay = el;
                var WP = Global.GetWorkPlaceByIdWorkplace(el);
                if (!WP.Settings?.IsAlcoholLicense == true && MW.curReceipt.IsAlcohol)
                {
                    SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, new CustomWindow(eWindows.Info, $"По даному місцю=>{el} відсутня алкогольна ліцензія"));
                    return;
                }
                if (!WP.Settings?.IsTobaccoLicense == true && MW.curReceipt.IsTobacco)
                {
                    SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, new CustomWindow(eWindows.Info, $"По даному місцю=>{el} відсутня тютюнова ліцензія"));
                    return;
                }
            }

            //MW.curReceipt.Client.SumMoneyBonus = 100;
            MW.curReceipt.IdWorkplacePay = 0;
            var xx = Bl.db.GetNoPricePromorion(MW.curReceipt).Where(x => x.TypeDiscount == eTypeDiscount.TextСashier).FirstOrDefault();
            if (MW.curReceipt?.Client != null && xx != null)
            {
                SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, new CustomWindow(eWindows.NoPricePromotion, xx));
                return;
            }

            if (Global.Settings.MaxSumReceipt > 0 && MW.curReceipt.SumTotal > Global.Settings.MaxSumReceipt)
            {
                SetStateView(eStateMainWindows.WaitCustomWindows, eTypeAccess.NoDefine, null, new CustomWindow(eWindows.MaxSumReceipt, Global.Settings.MaxSumReceipt));
                return;
            }
            if (MW.curReceipt.StateReceipt < eStateReceipt.Pay && MW.curReceipt.CountWeightGoods > 0 && !MW.curReceipt.Wares.Any(x => x.CodeWares == Global.Settings.CodePackagesBag) && !MW.curReceipt.IsPakagesAded && MW.curReceipt.TypeReceipt == eTypeReceipt.Sale)
            {
                SetStateView(eStateMainWindows.AddMissingPackage);
                return;
            }

            if (MW.curReceipt.StateReceipt < eStateReceipt.Pay && MW.curReceipt.AgeRestrict > 0 && MW.curReceipt.IsConfirmAgeRestrict == false)
            {
                SetStateView(eStateMainWindows.WaitAdmin, eTypeAccess.ConfirmAge);
                return;
            }
            MW.EquipmentInfo = string.Empty;

            if ((Global.TypeWorkplaceCurrent == eTypeWorkplace.CashRegister &&
                (MW.curReceipt.StateReceipt == eStateReceipt.Prepare || MW.curReceipt.StateReceipt == eStateReceipt.StartPay)) || EF?.CashMachine != null)
            {
                SetStateView(eStateMainWindows.ChoicePaymentMethod);
            }
            else
            {
                var task = Task.Run(() => PrintAndCloseReceipt());
            }
        }
        object LockPayPrint = new object();
        /// <summary>
        /// Оплата і Друк чека.
        /// </summary>
        /// <returns></returns>
        public bool PrintAndCloseReceipt(Receipt pR = null, eTypePay pTP = eTypePay.Card, decimal pSumCash = 0m, decimal pIssuingCash = 0, decimal pSumWallet = 0, decimal pSumBonus = 0, bool pIsCashBack = false)
        {
            bool Res = false;
            string TextError = null;

            var R = MW.Bl.GetReceiptHead(pR ?? MW.curReceipt, true);
            SetCurReceipt(R, false);
            R.NameCashier = MW.AdminSSC?.NameUser;
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"pTP=>{pTP} pSumCash=>{pSumCash} pIssuingCash=>{pIssuingCash} pSumWallet=>{pSumWallet} pSumBonus=>{pSumBonus} curReceipt=> {MW.curReceipt.ToJson()}", eTypeLog.Expanded);

            int[] IdWorkplacePays = R.IdWorkplacePays;// Wares.Select(el => el.IdWorkplacePay).Distinct().OrderBy(el => el).ToArray();


            lock (LockPayPrint)
            {
                R.StateReceipt = Bl.GetStateReceipt(R);
                if (R.StateReceipt == eStateReceipt.Prepare || R.StateReceipt == eStateReceipt.PartialPay)
                {
                    try
                    {
                        if (R.TypeReceipt == eTypeReceipt.Sale)
                            Bl.GenQRAsync(R.Wares);
                        //var Pays = new List<Payment>();

                        IEnumerable<Payment> PayRefaund = (R.TypeReceipt == eTypeReceipt.Refund ? Bl.GetPayment(R.RefundId) : null);
                        string rrn = R.AdditionC1;
                        string rrnCashBack = R.AdditionCashBack;
                        if (pSumWallet != 0 || pSumBonus != 0)
                        {

                            Bl.db.DelPayWalletBonus(R);
                            if (pSumWallet != 0)
                            {
                                Bl.db.ReplacePayment(new Payment(R) { IdWorkplacePay = R.IdWorkplace, IsSuccess = true, TypePay = eTypePay.Wallet, SumPay = pSumWallet, SumExt = pSumWallet });
                                R.Payment = Bl.GetPayment(R);
                                R.ReCalcWallet();
                            }
                            if (pSumBonus != 0)
                            {
                                for (var i = 0; i < IdWorkplacePays.Length; i++)
                                {
                                    R.IdWorkplacePay = IdWorkplacePays[i];
                                    var Pay = new Payment(R) { IdWorkplacePay = R.IdWorkplacePay, IsSuccess = true, TypePay = eTypePay.Bonus, SumPay = R.SumTotal, SumExt = pSumBonus, PosAddAmount = R.Client?.PercentBonus ?? MW.Client?.PercentBonus ?? 0m };

                                    Bl.db.ReplacePayment(Pay);
                                    R.Payment = Bl.GetPayment(R);
                                    R.ReCalcBonus();
                                }
                                R.IdWorkplacePay = 0;
                            }
                            R.Payment = Bl.GetPayment(R);
                            if ((pSumWallet > 0 || pSumBonus > 0))
                            {
                                foreach (var el in R.Wares.Where(el => el.TypeWares == eTypeWares.Ordinary))
                                    Bl.db.ReplaceWaresReceipt(el);

                                if (pSumBonus > 0)
                                {
                                    FillPays(R);
                                    for (var i = 0; i < IdWorkplacePays.Length; i++)
                                    {
                                        R.IdWorkplacePay = IdWorkplacePays[i];
                                        Bl.db.ReplacePayment(new Payment(R) { IdWorkplacePay = R.IdWorkplacePay, IsSuccess = true, TypePay = eTypePay.Cash, SumPay = Rro.GetSumRoundCash(R.WorkplacePay?.Sum ?? 0.5m), SumExt = Rro.GetSumRoundCash(R.WorkplacePay?.Sum ?? 0.5m) });
                                    }
                                    R.IdWorkplacePay = 0;
                                    R.StateReceipt = eStateReceipt.Pay;
                                    Bl.SetStateReceipt(R, R.StateReceipt);
                                    R.Payment = Bl.GetPayment(R);
                                }
                            }
                        }
                        FillPays(R);
                        for (var i = 0; i < IdWorkplacePays.Length; i++)
                        {
                            var SumPay= R.Payment.Where(el => el.IdWorkplacePay == IdWorkplacePays[i] && el.TypePay != eTypePay.Wallet).Sum(el=>el.SumPay);
                            if (R.Payment != null && ((SumPay>0 && pTP == eTypePay.Cash) ||  (R.SumTotal<= SumPay && pTP == eTypePay.Card)))
                                continue;
                            R.StateReceipt = eStateReceipt.StartPay;
                            R.IdWorkplacePay = IdWorkplacePays[i];
                            Bl.SetStateReceipt(MW.curReceipt, eStateReceipt.StartPay);
                            decimal sum = R.WorkplacePays[i].Sum;
                            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Sum={sum}", eTypeLog.Expanded);
                            SetStateView(eStateMainWindows.ProcessPay);
                            Payment pay = null;
                            if (pTP == eTypePay.Cash)
                            {
                                var SumCash = R.WorkplacePays[i].SumCash;
                                pay = new Payment(R) { IsSuccess = true, TypePay = eTypePay.Cash, SumPay = SumCash, SumExt = (i == IdWorkplacePays.Length - 1 ? Rro.GetSumRoundCash(pSumCash) : Rro.GetSumRoundCash(SumCash)) };
                                pSumCash -= SumCash;
                                if (pSumCash < 0) pSumCash = 0;
                                Bl.db.ReplacePayment(pay, true);
                            }
                            else if (pTP == eTypePay.CashMachine)
                            {
                                pay = EF.CashMachinePay(R, Rro.GetSumRoundCash(pSumCash) * 100m, pay);
                                Bl.db.ReplacePayment(pay, true);
                            }
                            else
                            {
                                if (pIsCashBack) //2 оплати при використанні карточки нац кешбек
                                {
                                    bool IsPay = true;
                                    bool IsCashBackPay = R.Payment?.Any(el => el.IsCashBack) ?? false;
                                    if (R.SumCashBack > 0 && !IsCashBackPay)
                                    {
                                        var PayRef = PayRefaund?.Where(el => el.IdWorkplacePay == R.IdWorkplacePay && el.IsCashBack );
                                        if (PayRef != null && PayRef.Any())
                                            rrn = PayRef.First().CodeAuthorization;
                                        pay = EF.PosPay(R, R.TypeReceipt == eTypeReceipt.Sale ? R.SumCashBack : -R.SumCashBack, rrn, pay, Global.IdWorkPlaceIssuingCash == IdWorkplacePays[i] ? pIssuingCash : 0, true);
                                        IsPay = pay.IsSuccess;
                                    }
                                    bool IsNormalPay = R.Payment?.Any(el => !el.IsCashBack) ?? false;
                                    if (sum - R.SumCashBack>0 && IsPay && !IsNormalPay)
                                    {
                                        var PayRef = PayRefaund?.Where(el => el.IdWorkplacePay == R.IdWorkplacePay && !el.IsCashBack);
                                        if (PayRef != null && PayRef.Any())
                                            rrn = PayRef.First().CodeAuthorization;
                                        Thread.Sleep(2000);// таймаут для другої оплати оскільки термінал не відразу реагує на наступний запит
                                        pay = EF.PosPay(R, R.TypeReceipt == eTypeReceipt.Sale ? sum - R.SumCashBack : R.SumCashBack - sum, rrn, null, Global.IdWorkPlaceIssuingCash == IdWorkplacePays[i] ? pIssuingCash : 0, false);
                                    }
                                }
                                else
                                {
                                    if (R.TypeReceipt == eTypeReceipt.Refund && PayRefaund != null)
                                    {
                                        var PayRef = PayRefaund?.Where(el => el.IdWorkplacePay == R.IdWorkplacePay);
                                        if (PayRef != null && PayRef.Any())
                                            rrn = PayRef.First().CodeAuthorization;
                                    }
                                    pay = EF.PosPay(R, R.TypeReceipt == eTypeReceipt.Sale ? sum : -sum, rrn, pay, Global.IdWorkPlaceIssuingCash == IdWorkplacePays[i] ? pIssuingCash : 0);
                                }

                            }
                            if (pay != null && pay.IsSuccess)
                            {
                                R.StateReceipt = (i == IdWorkplacePays.Length - 1 ? eStateReceipt.Pay : eStateReceipt.PartialPay);
                                R.CodeCreditCard = pay.NumberCard;
                                R.NumberReceiptPOS = pay.NumberReceipt;
                                //R.Client = null;
                                R.SumCreditCard = pay.SumPay;
                                Bl.db.ReplaceReceipt(R);
                                R.Payment = Bl.GetPayment(R);
                            }
                            else
                            {
                                R.StateReceipt = R.Payment?.Any() == true ? eStateReceipt.PartialPay : eStateReceipt.Prepare;
                                Bl.SetStateReceipt(MW.curReceipt, R.StateReceipt);
                                TextError = $"Оплата не пройшла: {MW.EquipmentInfo}";
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        R.StateReceipt = eStateReceipt.Prepare;
                        Bl.SetStateReceipt(MW.curReceipt, eStateReceipt.Prepare);
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    }
                    finally { R.IdWorkplacePay = 0; }
                }

                R.StateReceipt = Bl.GetStateReceipt(R);
                if (R.StateReceipt == eStateReceipt.Pay || R.StateReceipt == eStateReceipt.PartialPrint || R.StateReceipt == eStateReceipt.StartPrint)
                {
                    //Друк номерка на піцу якщо він потрібний
                    PrintOrderReceipt(R);

                    LogRRO res = null;
                    //Відключаємо контроль контрольної ваги тимчасово до наступної зміни товарного складу.
                    MW.CS.IsControl = false;
                    R.Client = MW.Client;
                    R.StateReceipt = eStateReceipt.StartPrint;
                    Bl.SetStateReceipt(MW.curReceipt, R.StateReceipt);
                    try
                    {
                        SetStateView(eStateMainWindows.ProcessPrintReceipt);
                        R.LogRROs = Bl.GetLogRRO(R);

                        for (var i = 0; i < IdWorkplacePays.Length; i++)
                        {
                            R.IdWorkplacePay = IdWorkplacePays[i];
                            if (!R.LogRROs.Any(el => el.TypeOperation == (R.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund) && el.IdWorkplacePay == IdWorkplacePays[i] && el.CodeError == 0))
                            {
                                res = EF.PrintReceipt(R);
                                if (res.CodeError == 0)
                                {
                                    R.SumFiscal += res.SUM;
                                    R.StateReceipt = (i == IdWorkplacePays.Length - 1 ? eStateReceipt.Print : eStateReceipt.PartialPrint);
                                }
                            }
                            if (R.IsPrintIssueOfCash)
                            {
                                res = EF.IssueOfCash(R);
                            }
                        }
                        if (res == null)
                            return true;
                        if (res.CodeError == 0)
                        {
                            R.NumberReceipt = res.FiscalNumber;
                            R.StateReceipt = eStateReceipt.Print;
                            R.UserCreate = Bl.GetUserIdbyWorkPlace(R.IdWorkplace);
                            R.DateReceipt = DateTime.Now;
                            Bl.UpdateReceiptFiscalNumber(R);
                            MW.s.Play(eTypeSound.DoNotForgetProducts);
                            Bl.SendReceiptTo1C(MW.curReceipt);
                            SetCurReceipt(null);
                            Res = true;
                        }
                        else
                        {
                            Bl.SetStateReceipt(MW.curReceipt, R.StateReceipt == eStateReceipt.StartPrint ? eStateReceipt.Pay : eStateReceipt.StartPrint);
                            TextError = $"Помилка друку чеків:({res?.CodeError}){Environment.NewLine}{res.Error}";
                            //MessageBox.Show(res.Error, "Помилка друку чеків");
                            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Помилка друку чеків" + res.Error, eTypeLog.Error);
                        }
                    }
                    catch (Exception e)
                    {
                        Bl.SetStateReceipt(MW.curReceipt, R.StateReceipt);
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    }
                    finally
                    { R.IdWorkplacePay = 0; }
                }
                if (R.StateReceipt == eStateReceipt.Print || R.StateReceipt == eStateReceipt.Send)
                {
                    EF.Print(R);
                }
                SetStateView(eStateMainWindows.WaitInput);
                if (TextError != null)
                {
                    Thread.Sleep(100);
                    Global.Message?.Invoke(TextError, eTypeMessage.Error);
                }
                return Res;
            }
            return Res;
        }
        private void PrintOrderReceipt(Receipt R, bool IsTryAgain = true)
        {
            bool IsNeadOrderReceipt = R.Wares.Where(x => x.ProductionLocation != 0).Any();
            if (Global.IsPrintOrderReceipt && IsNeadOrderReceipt && R.TypeReceipt == eTypeReceipt.Sale)
            {


                Task.Run(async () =>
                {
                    List<OrderWares> wares = new();
                    foreach (var goods in R.Wares)
                    {
                        if (goods.ProductionLocation > 0) // Перевірка чи товар потрібно готувати на якісь із зон
                            wares.Add(new OrderWares(goods));
                    }
                    var order = (new Order { IdWorkplace = R.IdWorkplace, Status = eStatus.Waiting, CodePeriod = R.CodePeriod, CodeReceipt = R.CodeReceipt, DateCreate = DateTime.Now, Type = R.TranslationTypeReceipt, Wares = wares });

                    CommandAPI<Order> Command = new() { Command = eCommand.GetOrderNumber, Data = order };

                    try
                    {
                        var r = new SocketClient(IPAddress.Parse(Global.IPAddressOrderService), 3444);
                        var ComandStr = Command.ToJson();
                        var Ansver = await r.StartAsync(ComandStr);
                        //if (!Ansver.status && IsTryAgain)
                        //{
                        //    FileLogger.WriteLogMessage($"SocketAnsver: {Environment.NewLine}Command: {ComandStr} {Environment.NewLine}IdWorkPlace: {R.IdWorkplace}{Environment.NewLine}Ansver: {Ansver.TextState}{Environment.NewLine} Перша спроба", eTypeLog.Error);
                        //    PrintOrderReceipt(R, false);
                        //    return;
                        //}
                        List<string> list = new List<string>() { "Номер замовлення:", $"{Ansver.TextState}" };
                        var res = EF.PrintNoFiscalReceipt(R, list);
                        List<string> listWares = new List<string>();
                        listWares = R.Wares.Where(x => x.ProductionLocation > 0).Select(x => $"{x.NameWares} => {x.Quantity}").ToList();
                        listWares.Insert(0, $"Список замовлення №{Ansver.TextState}");
                        listWares.Add(DateTime.Now.ToString("g"));
                        var res2 = EF.PrintNoFiscalReceipt(R, listWares);
                        FileLogger.WriteLogMessage($"SocketAnsver: {Environment.NewLine}Command: {ComandStr} {Environment.NewLine}IdWorkPlace: {R.IdWorkplace}{Environment.NewLine}Ansver: {Ansver.TextState}", eTypeLog.Full);
                        //SocketAnsver?.Invoke(eCommand.GetOrderNumber, MainWorkplace, Ansver);
                    }
                    catch (Exception ex)
                    {
                        FileLogger.WriteLogMessage(this, $"GeneralCondition DNSName=>{IPAddress.Parse(Global.IPAddressOrderService)} {Command.ToJson()} ", ex);
                        //SocketAnsver?.Invoke(eCommand.GetOrderNumber, MainWorkplace, new Status(ex));
                    }
                });
            }
        }
        public void FillPays(Receipt pR)
        {
            int[] IdWorkplacePays = pR.IdWorkplacePays;//Wares.Select(el => el.IdWorkplacePay).Distinct().OrderBy(el => el).ToArray();
            pR.WorkplacePays = new WorkplacePay[IdWorkplacePays.Length];
            for (var i = 0; i < IdWorkplacePays.Length; i++)
            {
                pR.IdWorkplacePay = IdWorkplacePays[i];
                var r = new WorkplacePay() { IdWorkplacePay = IdWorkplacePays[i], Sum = EF.SumReceiptFiscal(pR), SumCash = EF.SumCashReceiptFiscal(pR) };
                pR.WorkplacePays[i] = r;
            }
            pR.IdWorkplacePay = 0;
        }
    }
}
