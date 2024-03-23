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

namespace Front.Equipments
{
    public partial class BLF
    {
        object LockPayPrint = new object();
        /// <summary>
        /// Оплата і Друк чека.
        /// </summary>
        /// <returns></returns>
        public bool PrintAndCloseReceipt(Receipt pR = null, eTypePay pTP = eTypePay.Card, decimal pSumCash = 0m, decimal pIssuingCash = 0, decimal pSumWallet = 0, decimal pSumBonus = 0)
        {
            bool Res = false;
            /*string TextError = null;

            var R = MW.Bl.GetReceiptHead(pR ?? MW.curReceipt, true);
            SetCurReceipt(R, false);
            R.NameCashier = AdminSSC?.NameUser;
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"pTP=>{pTP} pSumCash=>{pSumCash} pIssuingCash=>{pIssuingCash} pSumWallet=>{pSumWallet} pSumBonus=>{pSumBonus} curReceipt=> {MW.curReceipt.ToJson()}", eTypeLog.Expanded);

            int[] IdWorkplacePays = R.IdWorkplacePays;// Wares.Select(el => el.IdWorkplacePay).Distinct().OrderBy(el => el).ToArray();
            IsManyPayments = IdWorkplacePays.Length > 1;
            OnPropertyChanged(nameof(IsManyPayments));
            FillPays(R);
            AmountManyPayments = "";
            foreach (var item in R.WorkplacePays)
                AmountManyPayments += $"{item.Sum} | ";
            AmountManyPayments = AmountManyPayments.Substring(0, AmountManyPayments.Length - 2);
            OnPropertyChanged(nameof(AmountManyPayments));
            SumTotalManyPayments = $"Загальна сума: {R.SumTotal}₴";
            OnPropertyChanged(nameof(SumTotalManyPayments));

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
                                    var Pay = new Payment(R) { IdWorkplacePay = R.IdWorkplacePay, IsSuccess = true, TypePay = eTypePay.Bonus, SumPay = R.SumTotal, SumExt = pSumBonus, PosAddAmount = R.Client?.PercentBonus ?? Client?.PercentBonus ?? 0m };

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
                                        Bl.db.ReplacePayment(new Payment(R) { IdWorkplacePay = R.IdWorkplacePay, IsSuccess = true, TypePay = eTypePay.Cash, SumPay = Math.Round(R.WorkplacePay?.Sum ?? 0, 1), SumExt = Math.Round(R.WorkplacePay?.Sum ?? 0, 1) });
                                    }
                                    R.IdWorkplacePay = 0;
                                    R.StateReceipt = eStateReceipt.Pay;
                                    Bl.SetStateReceipt(R, R.StateReceipt);
                                    R.Payment = Bl.GetPayment(R);
                                }
                            }
                            FillPays(R);
                        }

                        for (var i = 0; i < IdWorkplacePays.Length; i++)
                        {
                            if (R.Payment != null && R.Payment.Any(el => el.IdWorkplacePay == IdWorkplacePays[i] && el.TypePay != eTypePay.Wallet))
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
                                pay = new Payment(R) { IsSuccess = true, TypePay = eTypePay.Cash, SumPay = SumCash, SumExt = (i == IdWorkplacePays.Length - 1 ? Math.Round(pSumCash, 1) : Math.Round(SumCash, 1)) };
                                pSumCash -= SumCash;
                                if (pSumCash < 0) pSumCash = 0;
                                Bl.db.ReplacePayment(pay, true);
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
                                TextError = $"Оплата не пройшла: {EquipmentInfo}";
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
            }*/
            return Res;
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
