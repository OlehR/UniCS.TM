using Front.Equipments.Implementation.FP700_Model;
using Front.Equipments.Utils;
using Front.Equipments.Virtual;
using Front.Models;
using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;
using ModernExpo.SelfCheckout.Entities.Models.Terminal;
//using Resonance;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Linq;
using Utils;

namespace Front.Equipments.Implementation
{
    public class RRO_Maria : Rro
    {
        bool IsInit = false;
        bool IsError = false;
        WDB_SQLite db = WDB_SQLite.GetInstance;

        //M304ManagerApplication M304_;
        dynamic M304;
        public RRO_Maria(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) : base(pEquipment, pConfiguration, eModelEquipment.RRO_Maria, pLoggerFactory, pActionStatus)
        {
            try
            {
                OperatorName = Configuration?.GetValue<string>($"{KeyPrefix}OperatorName");
                OperatorPass = Configuration?.GetValue<string>($"{KeyPrefix}OperatorPass");

                Type t = System.Type.GetTypeFromProgID("M304Manager.Application");
                M304 = Activator.CreateInstance(t);
                Init();
                try
                {
                    string dt = M304.GetPrinterTime();
                    DateTime? FiscalDateTime = dt.ToDateTime("yyyyMMddHHmmss");
                    if (Math.Abs(((FiscalDateTime ?? DateTime.Now) - DateTime.Now).TotalSeconds) > 30) //Якщо час фіскалки відрізняється більше ніж на 30 секунд.
                        if (M304.GetBusinessDayState() == 1) // і немає відкритої зміни
                        {
                            M304.SetInternalTime(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);//Змінюємо час на фіскалці
                        }
                }
                catch (Exception e)
                {
                    ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_Maria, eStateEquipment.Init, e.Message)
                    { Status = eStatusRRO.Init, IsСritical = false });
                }
                //CST.XReport();
                //M304.OpenTextDocument();
                //M304.PrintQR("12346");
                //M304.FreeTextLine(0,0,3,"hello");//, doubleWidth: true, doubleHeight: true);
                //M304.CloseTextDocument();

                // M304_ = new M304ManagerApplication();                

            }
            catch (Exception e)
            { var m = e.Message; }
        }

        bool Init()
        {
            if (IsInit)
            {
                Done();
                IsInit = false;
            }

            if (!SetError(M304.Init(SerialPort, OperatorName, OperatorPass, false) != 1))
            {
                if (string.IsNullOrEmpty(M304.GetDocumentsInfoXML()))
                    Done();
                IsInit = true;
            }
            State = IsInit ? eStateEquipment.On : eStateEquipment.Error;
            return IsInit;
        }

        void Done()
        {
            if (M304 != null)
            {
                M304.Done();
                IsInit = false;
            }
        }

        bool SetError(bool pIsError)
        {
            IsError = pIsError;
            if (IsError)
            {
                if (M304 != null)
                {
                    StrError = M304.LastErrorCode;
                    int.TryParse(M304.LastErrorCode, out CodeError);
                }
            }
            else
            {
                CodeError = 0;
                StrError = null;
            }
            return IsError;
        }

        public override LogRRO PrintCopyReceipt(int parNCopy = 1)
        {
            Init();
            M304.CheckCopy();
            Done();
            return null;
        }


        override public LogRRO PrintZ(IdReceipt pIdR)
        {
            if (Init())
            {
                db.DelAllFiscalArticle();
                SetError(M304.ZReport() != 1);
                Done();
            }
            return new LogRRO(pIdR) { CodeError = CodeError, Error = StrError, SUM = 0, TypeRRO = "Maria304", TypeOperation = eTypeOperation.ZReport };
        }

        override public LogRRO PrintX(IdReceipt pIdR)
        {
            if (Init())
            {
                SetError(M304.XReport() != 1);
                Done();
            }
            return new LogRRO(pIdR) { CodeError = CodeError, Error = StrError, SUM = 0, TypeRRO = "Maria304", TypeOperation = eTypeOperation.XReport };
        }
        public override bool PeriodZReport(IdReceipt pIdR, DateTime pBegin, DateTime pEnd, bool IsFull = true)
        {
            bool res = false;
            if (Init())
            {
                res = SetError(M304.PeriodicalFiscalReportDateEx(pBegin, pEnd, IsFull) == 1) ? true : false;
                Done();
            }
            return res;
        }

        /// <summary>
        /// Внесення/Винесення коштів коштів.
        /// </summary>
        /// <param name="pSum"> pSum>0 - внесення</param>
        /// <returns></returns>
        override public LogRRO MoveMoney(decimal pSum, IdReceipt pIdR = null)
        {
            Init();
            SetError(M304.MoveCash((pSum > 0 ? 1 : 0), Convert.ToInt32(Math.Abs(pSum) * 100m)) != 1);
            Done();
            return new LogRRO(pIdR) { CodeError = CodeError, Error = StrError, SUM = pSum, TypeRRO = "Maria304", TypeOperation = pSum > 0 ? eTypeOperation.MoneyIn : eTypeOperation.MoneyOut };
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        override public LogRRO PrintReceipt(Receipt pR)
        {
            string XMLCheckResult = "";
            if (Init())
            {
                int SumCashPay = 0;
                int SumCardPay = 0;
                int SumFiscal = 0;
                int TypeDiscount = -1; // -1/0/1 – нет_скидки/скидка/надбавка
                string RRN = "";
                List<ReceiptText> Comments = null;
                if (!SetError((pR.TypeReceipt == eTypeReceipt.Sale ? M304.OpenCheck() : M304.OpenReturnCheck()) != 1))
                {
                    if (pR?.ReceiptComments?.Any() == true)
                    {
                        Comments = pR.ReceiptComments.Select(r => new ReceiptText() { Text = r, RenderType = eRenderAs.Text }).ToList();
                        foreach (ReceiptText comment in Comments)
                        {
                            if (!string.IsNullOrWhiteSpace(comment.Text))
                                M304.FreeTextLine(1, 1, 0, comment.GetText(43));
                        }
                    }
                    foreach (var el in pR.GetParserWaresReceipt(true, false))
                    {
                        var taxGroup = TaxGroup(el);
                        int TG1 = 0, TG2 = 0;
                        int.TryParse(taxGroup[..1], out TG1);
                        if (taxGroup.Length > 1)
                            int.TryParse(taxGroup[1..2], out TG2);
                        var Name = (el.IsUseCodeUKTZED && !string.IsNullOrEmpty(el.CodeUKTZED) ? el.CodeUKTZED.Substring(0, 10) + "#" : "") + el.NameWares;
                        if (!String.IsNullOrEmpty(el.ExciseStamp))
                        {
                            string[] ExciseStamps = el.ExciseStamp.Split(",");
                            foreach (var item in ExciseStamps)
                            {
                                if (SetError((M304.AddExciseStamps(item) != 1)))
                                    break;
                            }
                            
                        }
                            
                        if (el.SumDiscountEKKA == 0)
                            TypeDiscount = -1;//без знижок
                        if (el.SumDiscountEKKA > 0)
                            TypeDiscount = 0;//знижка
                        else TypeDiscount = 1; // надбавка

                        FiscalArticle article = db.GetFiscalArticle(el);
                        if (article == null)
                        {
                            article = new() { IdWorkplacePay = el.IdWorkplacePay, CodeWares = el.CodeWares, NameWares = Name, PLU = el.CodeWares, Price = el.PriceDealer };
                            db.AddFiscalArticle(article);
                        }

                        if (SetError(M304.FiscalLineEx(article.NameWares, Convert.ToInt32((el.CodeUnit == Global.WeightCodeUnit ? 1000 : 1) * el.Quantity), Convert.ToInt32(el.PriceEKKA * 100), el.CodeUnit == Global.WeightCodeUnit ? 1 : 0, TG1, TG2, el.CodeWares, TypeDiscount, null, Math.Abs(Convert.ToInt32(el.SumDiscountEKKA * 100m)), null) == 0))
                        {                            
                            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Помилка програмування товару:{article.CodeWares} PLU=>{article.PLU} {article.NameWares} {StrError}");
                            throw new Exception($"Помилка програмування товару!{Environment.NewLine}{article.CodeWares} PLU=>{article.PLU}{article.NameWares}{Environment.NewLine}{StrError}");
                        }
                    }

                    if (pR.Payment?.Any() == true)
                    {
                        foreach (var el in pR.Payment)
                        {
                            if (el.TypePay == eTypePay.Card)
                            {
                                SumCardPay = Decimal.ToInt32(el.SumPay * 100m);
                                RRN = el.CodeAuthorization;
                                if (el.NumberTerminal.Length > 8)
                                    el.NumberTerminal = el.NumberTerminal[..8];
                                if (string.IsNullOrEmpty(el.CardHolder))
                                    el.CardHolder = " ";

                                if (SetError(M304.AddSlip(2, "0", el.NumberTerminal, pR.TypeReceipt.ToString(), el.NumberCard, el.CodeAuthorization, el.CardHolder, el.CodeReceipt.ToString()) != 1))
                                {
                                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Помилка AddSlip:  {StrError}");
                                    throw new Exception(Environment.NewLine + "Помилка AddSlip:" + Environment.NewLine + StrError);
                                }
                            }
                            if (el.TypePay == eTypePay.Cash)
                            {
                                SumCashPay = Decimal.ToInt32(el.SumExt * 100m); // сума яку дає покупець
                            }
                        }
                    }
                }
                else
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Помилка відкриття чеку:  {StrError}");
                    throw new Exception(Environment.NewLine + "Помилка відкриття чеку!" + Environment.NewLine + StrError);
                }
                SumFiscal = M304.CheckSum; // сума чеку яка розрахувала фіскалка
                decimal roundFiscal = 0;
                if (SumFiscal > 0)
                    roundFiscal = (Math.Round(SumFiscal / 1000m, 2, MidpointRounding.AwayFromZero) * 10m) - SumFiscal / 100m; //  сума заокруглення для 1С

                pR.SumFiscal = SumFiscal / 100M;
                if (SumCashPay == 0 && Math.Abs(SumCardPay - SumFiscal) < 10)
                    SumCardPay = SumFiscal;
                if (SumCardPay == 0 && SumCashPay - SumFiscal < 0)
                    SumCashPay = SumFiscal + Convert.ToInt32(roundFiscal * 100);

                if (!IsError && roundFiscal != 0)
                {
                    try
                    {
                        var pay = new Payment(pR) { IsSuccess = true, TypePay = eTypePay.FiscalInfo, SumPay = SumFiscal / 100m, SumExt = roundFiscal };
                        pR.Payment = pR.Payment == null ? new List<Payment>() { pay } : pR.Payment.Append<Payment>(pay);
                        db.ReplacePayment(pay, true);
                    }
                    catch (Exception e)
                    {
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    }
                }
                if (!IsError)
                {
                    if (pR?.Footer?.Any() == true)
                    {
                        Comments = pR.Footer.Select(r => new ReceiptText() { Text = r, RenderType = eRenderAs.Text }).ToList();
                        foreach (ReceiptText comment in Comments)
                        {
                            if (!string.IsNullOrWhiteSpace(comment.Text))
                                M304.FreeTextLine(0, 1, 0, comment.GetText(43));
                        }
                    }

                    if (Global.IsTest)
                    {
                        M304.AbortCheck();
                    }
                    else
                    {
                        if (SetError(M304.CloseCheckEx(SumCashPay, SumCardPay, 0, 0, RRN) == 0))
                        {
                            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Помилка закриття чеку: {StrError}");
                            throw new Exception(Environment.NewLine + "Помилка закриття чеку!" + Environment.NewLine + StrError);
                        }
                    }

                    M304.PutToDisplay(pR.SumFiscal.ToString());
                }
                pR.NumberReceipt = M304.LastCheckNumber.ToString();
                XMLCheckResult = M304.GetCheckResultXML();

            }


            Done();

            return new LogRRO(pR)
            {
                CodeError = !string.IsNullOrEmpty(StrError) && CodeError == 0 ? -1 : CodeError, ///Бог зна чи поможе. Повноцінно змоделювати ситуацію не вийшло
                Error = StrError,
                SUM = pR.SumFiscal,
                TypeRRO = "Maria304",
                TypeOperation = (pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund),
                JSON = XMLCheckResult
            };

        }

        override public bool PutToDisplay(string pText, int pLine = 1)
        {
            if (string.IsNullOrEmpty(pText))
                pText = "Вітаємо Вас в нашому магазині!";

            if (!IsInit)
                Init();
            pText = pText.Replace(Environment.NewLine, " ");
            if (IsInit)
                return M304.PutToExternalDisplay(pText, true) == 1;

            return false;
        }

        public override StatusEquipment TestDevice()
        {
            Init();
            eDeviceConnectionStatus res;
            int timeToLock = -1;
            try
            {
                timeToLock = M304.GetTimeToPendingLock();
                res = timeToLock > 0 ? eDeviceConnectionStatus.Enabled : eDeviceConnectionStatus.Disabled;
                if (res == eDeviceConnectionStatus.Enabled)
                {
                    State = eStateEquipment.On;
                }
                else
                    State = eStateEquipment.Error;
            }
            catch (Exception e)
            {
                State = eStateEquipment.Error;
                return new StatusEquipment() { State = -1, TextState = e.Message };
            }
            var ts = TimeSpan.FromSeconds(timeToLock);
            Done();
            return new StatusEquipment() { TextState = $"{res}. Час до блокування: {ts.Hours + ts.Days * 24}год {ts.Minutes}хв ", State = (State == eStateEquipment.On ? 0 : -1) };
        }
        public override string GetDeviceInfo()
        {
            Init();
            string res = null;
            {
                try
                {
                    string Shift = M304.GetBusinessDayState == 2 ? "Відкрита." : "Закрита";
                    int timeToLock = M304.GetTimeToPendingLock();
                    var ts = TimeSpan.FromSeconds(timeToLock);
                    res = $"Фіскальна зміна: {Shift}{Environment.NewLine}" +
                        $"Серійний номер: {M304.GetPrinterSerialNumber}{Environment.NewLine}" +
                        $"Час до блокування: {ts.Hours + ts.Days * 24}год {ts.Minutes}хв{Environment.NewLine}" +
                        $"Version Resonance.OLEManager.dll=>{M304.Version}{Environment.NewLine}" +
                        $"{M304.GetPrinterConfigXML()}";
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    res = e.Message;
                }
            }
            Done();
            return res;
        }
        public override bool OpenMoneyBox(int pTime = 15)
        {
            if (!IsInit)
                Init();
            if (IsInit)
                return M304.OpenCashBox() == 1;

            return false;
        }
        override public LogRRO PrintNoFiscalReceipt(IEnumerable<string> pR)
        {
            List<ReceiptText> d = pR.Select(el => new ReceiptText() { Text = el.StartsWith("QR=>") ? el.SubString(4) : el, RenderType = el.StartsWith("QR=>") ? eRenderAs.QR : eRenderAs.Text }).ToList();
            PrintSeviceReceipt(d);
            return new LogRRO(new IdReceipt() { CodePeriod = Global.GetCodePeriod(), IdWorkplace = Global.IdWorkPlace }) { TypeOperation = eTypeOperation.NoFiscalReceipt, TypeRRO = Type.ToString(), JSON = pR.ToJSON() };
        }
        public bool PrintSeviceReceipt(List<ReceiptText> texts)
        {
            //M304.OpenTextDocument();
            //M304.PrintQR("12346");
            //M304.FreeTextLine(0,0,3,"hello");//, doubleWidth: true, doubleHeight: true);
            //M304.CloseTextDocument();
            Init();
            M304.PutToExternalDisplay("", true);
            M304.OpenTextDocument();
            foreach (ReceiptText text in texts)
            {
                switch (text.RenderType)
                {
                    case eRenderAs.Text:
                        M304.FreeTextLine(0, 0, 0, text.Text);
                        continue;
                    case eRenderAs.QR:
                        M304.PrintQR(text.Text, 40);
                        continue;
                    default:
                        continue;
                }
            }
            M304.CloseTextDocument();
            Done();
            return true;
        }
        public override decimal GetSumInCash(IdReceipt pIdR)
        {
            string res = "";
            CashInfo MoneyInfo = new();
            if (!IsInit)
                Init();
            if (IsInit)
                res = M304.GetCashInfoXML();

            //<?xml version="1.0"?><m301_cash_info><cash_info rest="187910" income="0" outcome="0" sales="20" return="20" total="187910" check_income="12" check_outcome="12" /></m301_cash_info>

            MoneyInfo = ParseXml(res);

            return MoneyInfo.Total;
        }
        public decimal GetSumInCash_Maria(string xmlRes, string strPars = "total")
        {
            int pos = xmlRes.IndexOf(strPars);

            return 50;
        }
        public CashInfo ParseXml(string xmlString)
        {


            string rest = "";
            string income = "";
            string outcome = "";
            string sales = "";
            string returnVal = "";
            string total = "";
            string checkIncome = "";
            string checkOutcome = "";

            try
            {
                // Створення об'єкту XmlReader на основі XML-строки
                using (XmlReader reader = XmlReader.Create(new System.IO.StringReader(xmlString)))
                {
                    while (reader.Read())
                    {
                        // Перевірка, чи поточна позиція є елементом
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            // Перевірка, чи поточний елемент є <cash_info>
                            if (reader.Name == "cash_info")
                            {
                                // Отримання значення атрибутів
                                rest = reader.GetAttribute("rest");
                                income = reader.GetAttribute("income");
                                outcome = reader.GetAttribute("outcome");
                                sales = reader.GetAttribute("sales");
                                returnVal = reader.GetAttribute("return");
                                total = reader.GetAttribute("total");
                                checkIncome = reader.GetAttribute("check_income");
                                checkOutcome = reader.GetAttribute("check_outcome");
                            }
                        }
                    }

                }
                return new CashInfo
                {
                    Rest = Convert.ToDecimal(rest) / 100,
                    Income = Convert.ToDecimal(income) / 100,
                    Outcome = Convert.ToDecimal(outcome) / 100,
                    Sales = Convert.ToDecimal(sales) / 100,
                    ReturnVal = Convert.ToDecimal(returnVal) / 100,
                    Total = Convert.ToDecimal(total) / 100,
                    CheckIncome = Convert.ToDecimal(checkIncome) / 100,
                    CheckOutcome = Convert.ToDecimal(checkOutcome) / 100
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Помилка при парсингу XML: " + ex.Message);
                return new CashInfo { Rest = -1m, Income = -1m, Outcome = -1m, CheckOutcome = -1m, CheckIncome = -1m, ReturnVal = -1m, Sales = -1m, Total = -1m };
            }

        }

        public override LogRRO IssueOfCash(Receipt pR)
        {
            Thread.Sleep(500);
            Payment Pay = pR?.IssueOfCash;
            if (Init())
            {
                //Чек видачі готівки.

                if (Pay != null)
                {
                    if (Pay.NumberTerminal.Length > 8)
                        Pay.NumberTerminal = Pay.NumberTerminal[..8];
                    if (string.IsNullOrEmpty(Pay.CardHolder))
                        Pay.CardHolder = " ";
                    if (SetError(M304.CashWithdrawal(Decimal.ToInt32(Pay.SumPay * 100m), 0, "0", Pay.NumberTerminal, Pay.NumberCard, Pay.CardHolder, Pay.CodeAuthorization, Pay.CodeReceipt.ToString()) != 1))
                    {
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Помилка видачі готівки: {StrError}");
                        //throw new Exception(Environment.NewLine + "Помилка видачі готівки" + Environment.NewLine + StrError);
                        return new LogRRO(pR) { TypeOperation = eTypeOperation.IssueOfCash, SUM = Pay.SumPay, CodeError = -1, Error = StrError };
                    }
                }
                Done();
            }
            return new LogRRO(pR) { TypeOperation = eTypeOperation.IssueOfCash, SUM = Pay?.SumPay ?? 0, CodeError = this.CodeError, Error = this.StrError };
        }

    }
    public class CashInfo
    {
        public decimal Rest { get; set; }
        public decimal Income { get; set; }
        public decimal Outcome { get; set; }
        public decimal Sales { get; set; }
        public decimal ReturnVal { get; set; }
        public decimal Total { get; set; }
        public decimal CheckIncome { get; set; }
        public decimal CheckOutcome { get; set; }
    }

}
