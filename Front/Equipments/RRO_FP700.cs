﻿using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using Front.Equipments.Virtual;
using Microsoft.Extensions.Logging;
using ModelMID.DB;
using ModelMID;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Timers;
using Utils;
using Front.Equipments.Utils;

using Front.Equipments.Implementation.FP700_Model;
using Timer = System.Timers.Timer;
using SharedLib;
using System.Data;
using System.Threading;
using System.Diagnostics;
using Front.Equipments.Implementation.ModelVchasno;
using System.Windows.Forms;
using System.Windows.Markup;

namespace Front.Equipments
{
    public class RRO_FP700 : Rro
    {
        private readonly IConfiguration _configuration;
        private readonly SerialPortStreamWrapper SerialDevice;
        private PrinterStatus _currentPrinterStatus;
       //private volatile bool _isReady = true;
        private volatile bool _isWaiting = true;
        private volatile bool _isError;
        private volatile bool _hasCriticalError;
        private int _sequenceNumber = 90;
        private readonly ILogger<RRO_FP700> _logger;
        private string DateFormat = "dd-MM-yy HH:mm:ss";
        //private Dictionary<eCommand, Action<string>> _commandsCallbacks;
        Action<string> CommandsCallbacks;
        private readonly List<byte> _packageBuffer = new List<byte>();
        private readonly Timer _packageBufferTimer;
        private const string ReportDateFormat = "ddMMyy";

        WDB_SQLite db = new WDB_SQLite();

        private int _tillNumber => Configuration.GetValue<int>($"{KeyPrefix}TillNumber");

        private int _operatorCode => 1;

        private string _operatorPassword => Configuration[$"{KeyPrefix}OperatorPassword"];

        private string _adminPassword => Configuration[$"{KeyPrefix}AdminPassword"];

        private int _maxItemLength => Configuration.GetValue<int>($"{KeyPrefix}MaxItemLength");

        public bool IsZReportAlreadyDone { get; private set; }

        public RRO_FP700(Equipment pEquipment, IConfiguration pConfiguration, ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) : base(pEquipment, pConfiguration, eModelEquipment.RRO_FP700, pLoggerFactory, pActionStatus)
        {
            try
            {
                ILogger<RRO_FP700> logger = pLoggerFactory?.CreateLogger<RRO_FP700>();

                _logger = logger;
                ActionStatus = pActionStatus;

                //_commandsCallbacks = new Dictionary<eCommand, Action<string>>();
                _currentPrinterStatus = new PrinterStatus();
                SerialPortStreamWrapper portStreamWrapper = new SerialPortStreamWrapper(SerialPort, BaudRate, onReceivedData: new Func<byte[], bool>(OnDataReceived));
                portStreamWrapper.Encoding = Encoding.GetEncoding(1251);
                SerialDevice = portStreamWrapper;
                _packageBufferTimer = new Timer();
                _packageBufferTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                _packageBufferTimer.Interval = 5000.0;
                _packageBufferTimer.Enabled = true;

                var res = Init();
                State = IsReady && res == eDeviceConnectionStatus.Enabled ? eStateEquipment.On : eStateEquipment.Error;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                State = eStateEquipment.Error;
            }
        }

        public override bool OpenWorkDay() { return true; }

        public override LogRRO PrintCopyReceipt(int parNCopy = 1)
        {
            var res = CopyReceipt();
            return new LogRRO(new IdReceipt() { CodePeriod = Global.GetCodePeriod(), IdWorkplace = Global.IdWorkPlace }) { TypeOperation = eTypeOperation.CopyReceipt, TypeRRO = Type.ToString(), FiscalNumber = GetLastZReportNumber() };
        }

        public override LogRRO PrintZ(IdReceipt pIdR)
        {
            string res="";
            res = ZReport();
            return new LogRRO(pIdR) { TypeOperation = eTypeOperation.ZReport, TypeRRO = Type.ToString(), FiscalNumber = GetLastZReportNumber(), TextReceipt = res };
        }

        public override LogRRO PrintX(IdReceipt pIdR)
        {
            string res = null;
            res = XReport();
            return new LogRRO(pIdR) { TypeOperation = eTypeOperation.XReport, TypeRRO = Type.ToString(), FiscalNumber = GetLastZReportNumber(), TextReceipt = res };
        }

        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        public override LogRRO MoveMoney(decimal pSum, IdReceipt pIdR = null)
        {
            MoneyMoving(pSum);
            return new LogRRO(pIdR) { TypeOperation = pSum > 0 ? eTypeOperation.MoneyIn : eTypeOperation.MoneyIn, SUM = pSum, TypeRRO = Type.ToString(), FiscalNumber = GetLastZReportNumber() };
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        public override LogRRO PrintReceipt(Receipt pR)
        {
            string FiscalNumber = null;
            decimal SumFiscal = 0;
            List<ReceiptText> Comments = null;
            if (pR?.ReceiptComments?.Count() > 0)
                Comments = pR.ReceiptComments.Select(r => new ReceiptText() { Text = r, RenderType = eRenderAs.Text }).ToList();
            /*if (pR.TypeReceipt == eTypeReceipt.Sale) */
            (FiscalNumber, SumFiscal) = PrintReceipt(pR, Comments);
            /* else
                 (FiscalNumber,SumFiscal) = ReturnReceipt(pR);*/
            pR.NumberReceipt = FiscalNumber;         
            return new LogRRO(pR) { TypeOperation = pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund, TypeRRO = Type.ToString(), FiscalNumber = FiscalNumber, SUM = SumFiscal /*pR.SumReceipt - pR.SumBonus,*/ };
        }       

        public override bool PeriodZReport(DateTime pBegin, DateTime pEnd, bool IsFull = true)
        {
            bool res = false;
            res = FullReportByDate(pBegin, pEnd, IsFull);
            return res;
        }

        public override LogRRO PrintNoFiscalReceipt(IEnumerable<string> pR)
        {
            List<ReceiptText> d = pR.Select(el => new ReceiptText() { Text = el.StartsWith("QR=>") ? el.SubString(4) : el, RenderType = el.StartsWith("QR=>") ? eRenderAs.QR : eRenderAs.Text }).ToList();
            PrintSeviceReceipt(d);
            return new LogRRO(new IdReceipt() { CodePeriod = Global.GetCodePeriod(), IdWorkplace = Global.IdWorkPlace }) { TypeOperation = eTypeOperation.NoFiscalReceipt, TypeRRO = Type.ToString(), JSON = pR.ToJSON() };
        }

        public override StatusEquipment TestDevice()
        {
            eDeviceConnectionStatus res;
            try
            {
                var s = SetRoundCash();
                res = TestDeviceSync();
                if (res == eDeviceConnectionStatus.Enabled)
                {
                    State = eStateEquipment.On;
                    DeleteAllArticles();
                }
                else
                    State = eStateEquipment.Error;
            }
            catch (Exception e)
            {
                State = eStateEquipment.Error;
                return new StatusEquipment() { State = -1, TextState = e.Message };
            }
            return new StatusEquipment() { TextState = res.ToString(), State = (State == eStateEquipment.On ? 0 : -1) };
        }

        public override string GetDeviceInfo()
        {
            string res = null;
            {
                try
                {
                    res = GetInfoSync();
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    res = e.Message;
                }
            }
            return res;
        }

        override public bool ProgramingArticle(ReceiptWares pRW)
        {
            FiscalArticle Res = null;
            if (pRW != null)
            {
                try
                {
                    Res = SetupArticleTable(pRW);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                }
            }
            return Res != null;
        }

        override public string GetTextLastReceipt(bool IsZReport = false)
        {
            string res = null;
            var r = GetLastReceiptNumber();
            if(IsZReport)
            {
                int rr;
                if (int.TryParse(r, out rr)) //!!!TMP Костиль, бо автоматично формує пакет
                    r = (rr - 1).ToString();  
            }
            res = KSEFGetReceipt(r);
            return res;
        }

        public override void Stop() { IsStop = true; }

        public virtual decimal GetSumFromTextReceipt(string pTextReceipt)
        {
            decimal Res = 0;
            try
            {
                var Start = pTextReceipt.IndexOf("С У М А       ");
                var End = pTextReceipt.IndexOf("Г Р Н");
                string Sum = pTextReceipt.Substring(Start + 10, End - Start - 10).Replace(" ", "");
                Res = decimal.Parse(Sum);
            }
            catch { }
            return Res;
        }

        public bool IsReady
        {
            get
            {
                SerialPortStreamWrapper serialDevice = SerialDevice;
                return serialDevice != null && serialDevice.IsOpen;
            }
        }

        Action<StatusEquipment> ActionStatus { get; set; }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (_packageBuffer.Count > 0)
                _packageBuffer.Clear();
            _packageBufferTimer.Stop();
        }

        public eDeviceConnectionStatus Init()
        {
            try
            {
                if(!ReopenPort())                
                    return eDeviceConnectionStatus.InitializationError;                

                ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.Init, "[FP700] - Get info about printer")
                { Status = eStatusRRO.Init });

                string infoSync = GetInfoSync();
                ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.Init, $"[FP700] - Initialization result {infoSync}")
                { Status = eStatusRRO.Init });

                if (string.IsNullOrEmpty(infoSync))
                {
                    ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.Error, "Cannot read data from printer")
                    { Status = eStatusRRO.Error, IsСritical = true });
                    return eDeviceConnectionStatus.InitializationError;
                }
                try
                {
                    SetRoundCash();
                    if (Math.Abs(((GetCurrentFiscalPrinterDate() ?? DateTime.Now) - DateTime.Now).Seconds) > 30) //Якщо час фіскалки відрізняється більше ніж на 30 секунд.
                        if (MinuteLastZReport().TotalMinutes == 0) // і немає відкритої зміни
                        {
                            SetupTime(DateTime.Now);//Змінюємо час на фіскалці
                        }
                }
                catch (Exception e)
                {
                    ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.Init, e.Message)
                    { Status = eStatusRRO.Init, IsСritical = false });
                }

                int num = IsZReportDone() ? 1 : 0;
                if (num == 0)
                {
                    ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.Error, "[FP700] - Should end previous day and create Z-Report")
                    { Status = eStatusRRO.Error, IsСritical = true });
                }
                ClearDisplay();
                //DeleteAllArticles();
                return (num & (OnSynchronizeWaitCommandResult(eCommand.PaperCut) ? 1 : 0)) != 0 ? eDeviceConnectionStatus.Enabled : eDeviceConnectionStatus.InitializationError;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
                ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.Error, "Device not connected")
                { Status = eStatusRRO.Error, IsСritical = true });
                return eDeviceConnectionStatus.NotConnected;
            }
        }

        public string GetInfoSync()
        {
            try
            {
                DiagnosticInfo diagnosticInfo = GetDiagnosticInfo();
                DocumentNumbers lastNumbers = null;
                var RoundCash = GetRoundCash();
                if (diagnosticInfo != null)
                    lastNumbers = GetLastNumbers();
                if (diagnosticInfo == null || lastNumbers == null)
                    throw new Exception("Fp700 getInfo error");
                return _currentPrinterStatus.TextError + $"Model:{diagnosticInfo.Model}\nSoftVersion: {diagnosticInfo.SoftVersion}\nSoftReleaseDate: {diagnosticInfo.SoftReleaseDate}\n SerialNumber: {diagnosticInfo.SerialNumber}\n RegistrationNumber: {diagnosticInfo.FiscalNumber}\n" +
                    $"BaudRate: {SerialDevice.BaudRate}\nComPort:{SerialDevice.PortName}\n" +
                    $"LastDocumentNumber: {lastNumbers.LastDocumentNumber}\nLastReceiptNumber: {lastNumbers.LastFiscalDocumentNumber}\nLastZReportNumber: {GetLastZReportNumber()}\nIsZReportDone: {IsZReportDone()}\nCurentTime: {GetCurrentFiscalPrinterDate() ?? DateTime.MinValue}\n" +
                    $"RoundCash: {RoundCash}"
                    ;
            }
            catch (Exception ex)
            {
                _logger?.LogDebug("Fp700 getInfo error");
                _logger?.LogError(ex, ex.Message);
                ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.Error, "Get info error" + _currentPrinterStatus.TextError)
                { Status = eStatusRRO.Error, IsСritical = true });
                return (string)null;
            }
        }

        public Task<string> GetInfo() => Task.Run<string>((Func<string>)(() => GetInfoSync()));

        public eDeviceConnectionStatus GetDeviceStatusSync()
        {
            try
            {
                if(!ReopenPort())
                    return eDeviceConnectionStatus.InitializationError;                
                return GetDiagnosticInfo() == null ? eDeviceConnectionStatus.NotConnected : eDeviceConnectionStatus.Enabled;
            }
            catch (Exception ex)
            {                
                ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.Error, _currentPrinterStatus.TextError + ex.Message)
                { Status = eStatusRRO.Error, IsСritical = true });
                return eDeviceConnectionStatus.NotConnected;
            }
        }

        public Task<eDeviceConnectionStatus> GetDeviceStatus() => Task.Run<eDeviceConnectionStatus>((Func<eDeviceConnectionStatus>)(() => GetDeviceStatusSync()));

        public eDeviceConnectionStatus TestDeviceSync()
        {
            try
            {
                _hasCriticalError = false;
                ObliterateFiscalReceipt();
                if (!ReopenPort())
                    return eDeviceConnectionStatus.InitializationError;
                _logger?.LogDebug("Fp700 after open");
                ClearDisplay();
                IsZReportDone();
                if (SendPackage(eCommand.PrintDiagnosticInformation))
                    return eDeviceConnectionStatus.Enabled;
                return SerialDevice.IsOpen ? eDeviceConnectionStatus.InitializationError : eDeviceConnectionStatus.NotConnected;
            }
            catch (Exception ex)
            {
                _logger?.LogDebug("Fp700 open error");
                _logger?.LogError(ex, ex.Message);
                ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.Error, ex.Message)
                { Status = eStatusRRO.Error, IsСritical = true });
                return eDeviceConnectionStatus.NotConnected;
            }
        }

        private bool ReopenPort()
        {            
            CloseIfOpened();            
            if (SerialDevice.PortName == null || SerialDevice.BaudRate == 0)
                return false;
            SerialDevice.Open();
            //_isReady = true;
            _isError = false;
            _isWaiting = false;
            return true;
        }

        public Task<eDeviceConnectionStatus> TestDevice2() => Task.Run<eDeviceConnectionStatus>(new Func<eDeviceConnectionStatus>(TestDeviceSync));

        public (string, decimal) PrintReceipt(Receipt pR, List<ReceiptText> Comments = null)
        {
            ObliterateFiscalReceipt();
            string s = GetLastReceiptNumber();
            
            if (string.IsNullOrEmpty(s))
                s = "0";
            int.TryParse(s, out int result1);
            if (pR.TypeReceipt == eTypeReceipt.Sale) OpenReceipt(pR?.NameCashier);
            else OpenReturnReceipt();

            if (Comments != null && Comments.Count() > 0)
                PrintFiscalComments(Comments);
            FillUpReceiptItems(pR.GetParserWaresReceipt());
            decimal Total = 0, TotalRnd = 0;
            (Total, TotalRnd) = SubTotal();
            if (Total > 0 || TotalRnd > 0)
                try
                {
                    var pay = new Payment(pR) { IsSuccess = true, TypePay = eTypePay.FiscalInfo, SumPay = Total, SumExt = TotalRnd - Total };
                    pR.Payment = pR.Payment == null ? new List<Payment>() { pay } : pR.Payment.Append<Payment>(pay);
                    db.ReplacePayment(pay, true);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                }

            //pR.SumFiscal = Sum;
            if (!PayReceipt(pR, TotalRnd))
            {
                ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.Error, "Check was not printed")
                { Status = eStatusRRO.Error, IsСritical = true });
            }
            CloseReceipt();
            ClearDisplay();

            if (!int.TryParse(GetLastReceiptNumber(), out int result2))
                return (null, TotalRnd);

            //Чек видачі готівки.
            Payment Pay = pR?.Payment?.Where(el => el.TypePay == eTypePay.IssueOfCash)?.FirstOrDefault();
            if (Pay != null)
                IssueOfCash(Pay);

            _logger?.LogDebug($"[ FP700 ] newLastReceipt = {result2} / lastReceipt = {result1}");
            string str = result2 > result1 ? result2.ToString() : null;
            if (str == null)
                ObliterateFiscalReceipt();
            return (str, TotalRnd);
        }        

        public bool OpenReceipt(string pCashier = null)
        {
            if (!string.IsNullOrEmpty(pCashier))
            {
                string str = pCashier;
                if (str.Length >= 24)
                    str = str.Remove(23);
                OnSynchronizeWaitCommandResult(eCommand.SetOperatorName, string.Format("{0},{1},{2}", (object)_operatorCode, (object)_operatorPassword, (object)str));
            }
            ClearDisplay();
            OnSynchronizeWaitCommandResult(eCommand.OpenFiscalReceipt, string.Format("{0},{1},{2}", (object)_operatorCode, (object)_operatorPassword, (object)_tillNumber), (Action<string>)(res => Console.WriteLine(res)));
            return true;
        }

        public bool FillUpReceiptItems(IEnumerable<ReceiptWares> pRW)
        {
            //var RW = UngroupByExcise(pRW);

            //List<FiscalArticle> FiscalArticleList = receiptItems != null && receiptItems.Count != 0 ? SetupArticleTable(receiptItems) : throw new Exception("Cannot register clear receipt items");
            foreach (ReceiptWares el in pRW) //(int index = 0; index < FiscalArticleList.Count; ++index)
            {
                FiscalArticle FiscalArticle = SetupArticleTable(el);// FiscalArticleList[index];
                //ReceiptItem receiptItem = receiptItems[index];
                string Price = string.Empty;
                decimal BasePrice = el.Price < el.PriceDealer ? el.PriceDealer : el.Price;
                decimal discont = el.Price < BasePrice ? Math.Round(BasePrice * el.Quantity, 2, MidpointRounding.AwayFromZero) - Math.Round(el.Sum, 2, MidpointRounding.AwayFromZero) : 0;

                //if (FiscalArticle.Price != BasePrice)
                    Price = "#" + BasePrice.ToS();

                // tring((IFormatProvider)CultureInfo.InvariantCulture);

                string data = $"{FiscalArticle.PLU}*{el.Quantity.ToS()}{Price}";
                if (el.SumTotalDiscount + discont != 0M)
                    data += ";" + (el.SumTotalDiscount + discont > 0M ? "-" : "+") + Math.Abs(el.SumTotalDiscount + discont).ToS();

                if (!string.IsNullOrWhiteSpace(el.BarCode))
                    data = data + "&" + el.BarCode;
                if (!string.IsNullOrEmpty(el.ExciseStamp))
                    data += "!" + el.ExciseStamp;

                int num2 = OnSynchronizeWaitCommandResult(eCommand.RegisterProductInReceiptWithDisplay, data, (Action<string>)(res => { })) ? 1 : 0;
                //string errCode = string.Empty;
                //OnSynchronizeWaitCommandResult(eCommand.GetLastError, onResponseCallback: ((Action<string>)(res => errCode = res)));
                //_logger?.LogDebug("[ FP700 ] FillUpItemLastCode: " + errCode);
                if (num2 == 0)
                    throw new Exception("Registed product in receipt");
            }
            return true;
        }

        public bool PrintFiscalComments(List<ReceiptText> comments)
        {
            foreach (ReceiptText comment in comments)
            {
                if (!string.IsNullOrWhiteSpace(comment.Text))
                    OnSynchronizeWaitCommandResult(eCommand.PrintFiscalComment, comment.Text);
            }
            return true;
        }

        public bool PayReceipt(Receipt pR, decimal pRealSum)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Payment Pay = pR?.Payment?.Where(el => el.TypePay == eTypePay.Card)?.FirstOrDefault();
            _logger?.LogDebug($"[FP700] PayReceipt {Pay?.TypePay}");
            if (Pay == null || Pay.TypePay == eTypePay.Cash)
            {
                Pay = pR?.Payment?.Where(el => el.TypePay == eTypePay.Cash)?.FirstOrDefault();

                decimal Sum = pRealSum;

                if (Pay != null && Pay.SumExt > Sum)
                    Sum = Pay.SumExt;

                stringBuilder.Append("P+" + Sum.ToString("F2", CultureInfo.InvariantCulture));
            }
            else

            if (Pay.TypePay == eTypePay.Card)
            {
                Decimal totalAmount = 0M;
                OnSynchronizeWaitCommandResult(eCommand.FiscalTransactionStatus, onResponseCallback: (Action<string>)(res =>
                {
                    string[] strArray = res.Split(',');
                    if (strArray.Length == 1)
                        totalAmount = Decimal.Parse(res.Substring(4, 12)) / 100M;
                    else
                        totalAmount = Decimal.Parse(strArray[2]) / 100M;
                }));
                if (Pay == null) stringBuilder.Append("D");
                else
                {
                    stringBuilder.Append(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "D+{0:0.00}", (object)totalAmount));
                    stringBuilder.Append("," + GetPayStr(Pay, pR.TypeReceipt));
                }

            }
            else
                throw new Exception("Cannot pay receipt with incorrect payment type");

            string data = $"\n\t{stringBuilder}";
            bool paySuccess = false;
            char paidCode = char.MinValue;
            int amount = 0;
            OnSynchronizeWaitCommandResult(eCommand.PayInfoFiscalReceipt, data, (Action<string>)(res =>
            {

                _logger?.LogDebug("[ FP700 ] PayInfoFiscalReceipt res = " + res);
                paidCode = res[0];
                int.TryParse(res.Substring(1), out amount);
                paySuccess = paidCode == 'D' && amount == 0 || paidCode == 'R';
            }));
            if (!paySuccess)
            {
                if (amount == 0)
                {
                    string errCode = string.Empty;
                    OnSynchronizeWaitCommandResult(eCommand.GetLastError, onResponseCallback: ((Action<string>)(res => errCode = res)));
                    _logger?.LogDebug("[ FP700 ] GetLastError: " + errCode);
                }
                throw new Exception("NotPaid");
            }
            return paySuccess;
        }
        string GetPayStr(Payment pPay, eTypeReceipt pTR = eTypeReceipt.Sale)
        {
            return $"{pPay.CodeAuthorization},Магазин,{pPay.NumberTerminal}," +
            (string.IsNullOrWhiteSpace(pPay.IssuerName) ? "картка" : pPay.IssuerName) +
             (pPay.TypePay == eTypePay.IssueOfCash ? ",Видача" : (pTR == eTypeReceipt.Sale ? ",оплата" : ",повернення")) +
        $",{pPay.NumberCard},{pPay.NumberSlip},0.00";
        }


        /// <summary>
        /// Видача готівки
        /// </summary>
        /// <param name="pPay"></param>
        /// <returns></returns>
        public bool IssueOfCash(Payment pPay)
        {
            bool res = false;
            string Command = $"-{pPay.SumPay.ToS()}&{GetPayStr(pPay)}";
            OnSynchronizeWaitCommandResult(eCommand.ServiceCashInOut, Command, (Action<string>)(response =>
            {
                string[] strArray = response.Split(',');
                if (strArray.Length < 4)
                    return;
                if (strArray[0].Equals("P"))
                {
                    res = true;
                }
                else
                {
                    if (!strArray[0].Equals("F"))
                        return;
                    res = false;
                }
            }));
            return res;
        }

        public string CloseReceipt()//ReceiptViewModel receipt
        {
            string res = (string)null;
            OnSynchronizeWaitCommandResult(eCommand.CloseFiscalReceipt, onResponseCallback: ((Action<string>)(response =>
            {
                if (string.IsNullOrEmpty(response))
                    return;
                string[] strArray = response.Split(',');
                if (strArray.Length < 3)
                    return;
                res = strArray[3];
            })));
            //ClearDisplay();
            return res;
        }

        public void PrintDividerLine(bool shouldPrintBeforeFiscalInfo) => OnSynchronizeWaitCommandResult(eCommand.PrintDividerLine);

        public bool CopyReceipt() => OnSynchronizeWaitCommandResult(eCommand.FiscalReceiptCopy, "1");

        override public bool OpenMoneyBox(int pTime = 15) => OnSynchronizeWaitCommandResult(eCommand.OpenMoneyBox);

        public bool MoneyMoving(decimal pSum)
        {
            bool res = false;
            OnSynchronizeWaitCommandResult(eCommand.ServiceCashInOut, pSum == 0 ? "" : (pSum > 0 ? "+" : "-") + Math.Abs(pSum).ToS(), (Action<string>)(response =>
            {
                string[] strArray = response.Split(',');
                if (strArray.Length < 4)
                    return;
                if (strArray[0].Equals("P"))
                {
                    res = true;
                }
                else
                {
                    if (!strArray[0].Equals("F"))
                        return;
                    res = false;
                }
            }));
            return res;
        }

        public bool FullReportByDate(DateTime startDate, DateTime? endDate, bool IsFull)
        {
            string str = "";
            if (endDate.HasValue)
                str = "," + endDate.Value.ToString("ddMMyy");
            return OnSynchronizeWaitCommandResult(IsFull ? eCommand.FullReportByPeriod : eCommand.ShortReportByPeriod, _operatorPassword + "," + startDate.ToString("ddMMyy") + str);
        }

        public void OpenReturnReceipt() => OnSynchronizeWaitCommandResult(eCommand.ReturnReceipt, string.Format("{0},{1},{2}", (object)_operatorCode, (object)_operatorPassword, (object)_tillNumber), (Action<string>)(res => _logger?.LogDebug("[ FP700 ] ReturnReceipt res = " + res)));

        public bool PrintSeviceReceipt(List<ReceiptText> texts)
        {
            ObliterateFiscalReceipt();
            ClearDisplay();
            OnSynchronizeWaitCommandResult(eCommand.OpenNonFiscalReceipt);
            foreach (ReceiptText text in texts)
            {
                switch (text.RenderType)
                {
                    case eRenderAs.Text:
                        OnSynchronizeWaitCommandResult(eCommand.PrintNonFiscalComment, text.Text);
                        continue;
                    case eRenderAs.QR:
                        OnSynchronizeWaitCommandResult(eCommand.PrintCode, "Q," + text.Text);
                        continue;
                    default:
                        continue;
                }
            }
            OnSynchronizeWaitCommandResult(eCommand.CloseNonFiscalReceipt);
            return true;
        }

        public bool OpenServiceReceipt()
        {
            ObliterateFiscalReceipt();
            ClearDisplay();
            OnSynchronizeWaitCommandResult(eCommand.OpenNonFiscalReceipt);
            return true;
        }

        public bool CloseServiceReceipt()
        {
            OnSynchronizeWaitCommandResult(eCommand.CloseNonFiscalReceipt);
            return true;
        }

        public bool PrintServiceLine(ReceiptText text)
        {
            if (string.IsNullOrEmpty(text?.Text))
                return true;
            OnSynchronizeWaitCommandResult(eCommand.PrintNonFiscalComment, text.Text);
            return true;
        }

        public bool PrintServiceLines(List<ReceiptText> texts)
        {
            foreach (ReceiptText text in texts)
                PrintServiceLine(text);
            return true;
        }

        public bool SetupReceipt(Fp700ReceiptConfiguration configuration)
        {
            if (!(configuration is Fp700ReceiptConfiguration receiptConfiguration))
                return false;
            SendPackage(eCommand.ReceiptDetailsPrintSetupAdditionalSettings, "0" + receiptConfiguration.HeaderLine1);
            SendPackage(eCommand.ReceiptDetailsPrintSetupAdditionalSettings, "1" + receiptConfiguration.HeaderLine2);
            SendPackage(eCommand.ReceiptDetailsPrintSetupAdditionalSettings, "2" + receiptConfiguration.HeaderLine3);
            SendPackage(eCommand.ReceiptDetailsPrintSetupAdditionalSettings, "3" + receiptConfiguration.HeaderLine4);
            SendPackage(eCommand.ReceiptDetailsPrintSetupAdditionalSettings, "4" + receiptConfiguration.HeaderLine5);
            SendPackage(eCommand.ReceiptDetailsPrintSetupAdditionalSettings, "5" + receiptConfiguration.HeaderLine6);
            SendPackage(eCommand.ReceiptDetailsPrintSetupAdditionalSettings, "6" + receiptConfiguration.FooterLine1);
            SendPackage(eCommand.ReceiptDetailsPrintSetupAdditionalSettings, "7" + receiptConfiguration.FooterLine2);
            return SendPackage(eCommand.ReceiptDetailsPrintSetupAdditionalSettings, string.Format("L{0}", (object)(receiptConfiguration.ShouldPrintLogo ? 1 : 0)));
        }

        public bool SetupPrinter() => true;

        public bool SetupPaperWidth(eFiscalPrinterPaperWidthEnum width) => OnSynchronizeWaitCommandResult(eCommand.ReceiptDetailsPrintSetupAdditionalSettings, "N" + (width == eFiscalPrinterPaperWidthEnum.Width80mm ? "0" : "1"));

        public bool SetupTime(DateTime time) => OnSynchronizeWaitCommandResult(eCommand.SetDateTime, time.ToString(DateFormat));

        public string XReport()
        {
            string Res = null;
            ObliterateFiscalReceipt();
            OnSynchronizeWaitCommandResult(eCommand.EveryDayReport, _operatorPassword + ",2", ((Action<string>)(response => Res = response)));

            return Res;
        }

        public string ZReport()
        {
            string Res = null;
            ObliterateFiscalReceipt();
            OnSynchronizeWaitCommandResult(eCommand.EveryDayReport, _operatorPassword + ",0", ((Action<string>)(response => Res = response)));
            IsZReportAlreadyDone = true;
            DeleteAllArticles();            
            return Res;
        }

        public (decimal, decimal) SubTotal()
        {
            decimal Total = 0, TotalRnd = 0;
            string res = null;
            OnSynchronizeWaitCommandResult(eCommand.SubTotal, onResponseCallback: ((Action<string>)(response => res = response)));
            var r = res?.Split(',');
            if (r.Length > 1)
            {
                decimal.TryParse(r[0], out Total);
                decimal.TryParse(r[1], out TotalRnd);
            }
            return (Total / 100M, TotalRnd / 100M);
        }

        public bool ArticleReport() => OnSynchronizeWaitCommandResult(eCommand.ArticleReport, string.Format("{0},{1}", (object)_operatorPassword, (object)eArticleReportType.S));

        public void DeleteAllProgrammingArticles() => db.DelAllFiscalArticle();

        public bool ObliterateFiscalReceipt()
        {
            OnSynchronizeWaitCommandResult(eCommand.CloseNonFiscalReceipt);
            OnSynchronizeWaitCommandResult(eCommand.ObliterateFiscalReceipt, onResponseCallback: ((Action<string>)(res => _logger?.LogDebug("[ FP700 ] ObliterateFiscalReceipt res = " + res))));
            return true;
        }

        public string GetLastReceiptNumber() => GetLastNumbers().LastDocumentNumber;

        public string GetLastRefundReceiptNumber() => GetLastNumbers().LastDocumentNumber;

        public string GetLastZReportNumber()
        {
            string result = "";
            OnSynchronizeWaitCommandResult(eCommand.LastZReportInfo, "0", (Action<string>)(res =>
            {
                if (string.IsNullOrEmpty(res))
                    return;
                string[] strArray = res.Split(',');
                if (strArray.Length < 7)
                    return;
                result = strArray[0];
            }));
            return result;
        }

        private bool IsZReportDone()
        {
            bool result = false;
            OnSynchronizeWaitCommandResult(eCommand.ShiftInfo, onResponseCallback: ((Action<string>)(res =>
            {
                if (string.IsNullOrWhiteSpace(res))
                    return;
                switch (res[0])
                {
                    case 'F':
                        result = false;
                        break;
                    case 'P':
                        result = true;
                        string[] strArray = res.Split(',');
                        if (strArray.Length < 2)
                            break;
                        int.TryParse(strArray[1], out int _);
                        break;
                    case 'Z':
                        result = true;
                        break;
                }
            })));
            IsZReportAlreadyDone = result;
            return result;
        }

        /// <summary>
        /// -1 хв - Помилка, 0 Зміна не розпочата, >0 Час відкритої зміни. 
        /// </summary>
        /// <returns></returns>
        public TimeSpan MinuteLastZReport()
        {
            int Time = -1;
            OnSynchronizeWaitCommandResult(eCommand.ShiftInfo, onResponseCallback: ((Action<string>)(res =>
            {
                if (string.IsNullOrWhiteSpace(res))
                    return;

                if (res[0] == 'F' || res[0] == 'P')
                {
                    string[] strArray = res.Split(',');
                    if (strArray.Length < 2)
                        return;
                    if (int.TryParse(strArray[1], out Time))
                        return;
                }
                else
                if (res[0] == 'Z')
                    Time = 0;
            })));
            return TimeSpan.FromMinutes(Time);
        }

        private DocumentNumbers GetLastNumbers()
        {
            DocumentNumbers documentNumbers = new DocumentNumbers();
            OnSynchronizeWaitCommandResult(eCommand.LastDocumentsNumbers, onResponseCallback: ((Action<string>)(res =>
            {
                _logger?.LogDebug("FP700 [GetLastNumbers] " + res);
                if (string.IsNullOrEmpty(res))
                    return;
                string[] strArray = res.Split(',');
                if (strArray.Length < 3)
                    return;
                documentNumbers.LastDocumentNumber = strArray[0];
                documentNumbers.LastFiscalDocumentNumber = strArray[1];
                documentNumbers.LastRefundFiscalDocumentNumber = strArray[2];
                if (strArray.Length > 3)
                    documentNumbers.GlobalDocumentNumber = strArray[3];
            })));
            return documentNumbers;
        }

        public DateTime? GetCurrentFiscalPrinterDate()
        {
            DateTime? currentDate = new DateTime?();
            OnSynchronizeWaitCommandResult(eCommand.GetDateTime, onResponseCallback: ((Action<string>)(response => currentDate = new DateTime?(DateTime.ParseExact(response, DateFormat, (IFormatProvider)CultureInfo.InvariantCulture)))));
            return currentDate;
        }

        private void DeleteAllArticles() => OnSynchronizeWaitCommandResult(eCommand.ArticleProgramming, "DA," + _operatorPassword, (Action<string>)(res =>
        {
            if (res.Trim().ToUpper().StartsWith("F"))
            {
                _logger?.LogDebug("[Fp700] Error during articles deleting");
            }
            else
            {
                if (!res.Trim().ToUpper().StartsWith("P"))
                    return;
                
                db.DelAllFiscalArticle();               
            }
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"End {res}");
        }));

        public FiscalArticle SetupArticleTable(ReceiptWares pRW)
        {
            FiscalArticle article = db.GetFiscalArticle(pRW);

            if (article == null || article.CodeWares != pRW.CodeWares)
            {
                if (!(pRW.Price == 0M))
                {
                    Thread.Sleep(5);
                    int FirstFreeArticle = FindFirstFreeArticle();
                    if (FirstFreeArticle == -1)
                        throw new Exception("Fp700 FindFirstFreeArticle return -1");
                    bool isSuccess = false;
                    string str = string.Empty;
                    if (!string.IsNullOrWhiteSpace(pRW.CodeUKTZED))
                        str = "^" + pRW.CodeUKTZED + ",";
                    string data = string.Format("P{0}{1},1,{2}{3},{4},", (object)TaxGroup(pRW), (object)FirstFreeArticle, (object)str, (object)pRW.Price.ToString("F2", (IFormatProvider)CultureInfo.InvariantCulture), (object)_operatorPassword) + pRW.NameWaresReceipt.LimitCharactersForTwoLines(_maxItemLength, '\t');
                    OnSynchronizeWaitCommandResult(eCommand.ArticleProgramming, data, (Action<string>)(res =>
                   {
                       if (res.Trim().ToUpper().Equals("P"))
                       { 
                           article = new FiscalArticle() { CodeWares = pRW.CodeWares, Price = pRW.Price, NameWares = pRW.NameWaresReceipt, PLU = FirstFreeArticle, IdWorkplacePay = pRW.IdWorkplacePay };
                           db.AddFiscalArticle(article);
                           isSuccess = true;
                       }
                       else
                       {
                           ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.Error, "Fp700 writing article FALSE: " + res)
                           { Status = eStatusRRO.Error, IsСritical = true });                          
                       }
                   }));
                    if (!isSuccess)
                        throw new Exception("Fp700 writing article FALSE");
                }
            }

            return article;
        }

        private void ClearDisplay()
        {
            OnSynchronizeWaitCommandResult(eCommand.ClearDisplay);                
        }

        private int FindFirstFreeArticle()
        {
            int firstPluNumber = -1;
            OnSynchronizeWaitCommandResult(eCommand.ArticleProgramming, "X", (Action<string>)(response =>
            {
                int startIndex = -1;
                for (int index = 0; index < response.Length; ++index)
                {
                    int result;
                    if (int.TryParse(response[index].ToString(), out result) && result != 0)
                    {
                        startIndex = index;
                        break;
                    }
                }
                if (startIndex == -1)
                    return;
                int.TryParse(response.Substring(startIndex), out firstPluNumber);
            }));
            return firstPluNumber;
        }

        private DiagnosticInfo GetDiagnosticInfo()
        {
            DiagnosticInfo diagnosticInfo = (DiagnosticInfo)null;
            OnSynchronizeWaitCommandResult(eCommand.DiagnosticInfo, onResponseCallback: ((Action<string>)(res =>
            {
                try
                {
                    if (string.IsNullOrEmpty(res))
                        return;
                    string[] strArray1 = res.Split(',');
                    if (strArray1.Length < 10)
                        return;
                    string[] strArray2 = strArray1[1].Split(' ');
                    diagnosticInfo = new DiagnosticInfo()
                    {
                        Model = strArray1[0],
                        SoftVersion = strArray2[0],
                        SoftReleaseDate = DateTime.ParseExact(strArray2[1], "ddMMMyy", CultureInfo.InvariantCulture),
                        Check = strArray1[2],
                        Switchers = strArray1[3],
                        CountryCode = strArray1[4],
                        SerialNumber = strArray1[5],
                        FiscalNumber = strArray1[6],
                        Id_Dev = strArray1[7],
                        Id_Acq = strArray1[8],
                        Id_Sam = strArray1[9]
                    };
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, ex.Message);
                }
            })));
            return diagnosticInfo;
        }

        private string GetRoundCash()
        {
            string Res = null;
            OnSynchronizeWaitCommandResult(eCommand.ReceiptDetailsPrintSetupAdditionalSettings, "IR", onResponseCallback: (Action<string>)(res =>
            {
                Res = res;
            }));
            return Res;
        }

        public string SetRoundCash(string pMoney = "10")
        {
            var s = GetRoundCash();
            if (!s.Equals(pMoney))
            {
                string Res = null;
                OnSynchronizeWaitCommandResult(eCommand.ReceiptDetailsPrintSetupAdditionalSettings, $"R{pMoney}");
                Thread.Sleep(200);
                OnSynchronizeWaitCommandResult(eCommand.SaveSettingInMemory);
                Thread.Sleep(200);
            }
            return s = GetRoundCash();
        }

        public bool CanOpenReceipt() => CheckModemStatus() && IsZReportDone();

        private bool CheckModemStatus(bool tryToReopen = false)
        {
            bool result = false;
            bool shouldThrow = false;
            OnSynchronizeWaitCommandResult(eCommand.StateOfDataTransmission, onResponseCallback: ((Action<string>)(res =>
            {
                if (string.IsNullOrWhiteSpace(res))
                {
                    if (!tryToReopen)
                    {
                        try
                        {
                            if (ReopenPort())
                            {
                                result = CheckModemStatus(true);
                                return;
                            }
                            shouldThrow = true;
                            return;
                        }
                        catch
                        {
                            shouldThrow = true;
                            return;
                        }
                    }
                }
                string[] strArray = res.Split(',');
                if (strArray.Length < 6)
                    return;
                result = strArray[5].EqualsIgnoreCase("ok");
            })));
            if (shouldThrow)
                throw new Exception("[FP700] Printer not ready");
            return result;
        }

       bool isResultGot = false;
       object Lock = new();
        Stopwatch Timer = new Stopwatch();
       private bool OnSynchronizeWaitCommandResult( eCommand pCommand, string pData = "",  Action<string> onResponseCallback = null, Action<Exception> onExceptionCallback = null)
       {
            if (!SerialDevice.IsOpen)
                if(!ReopenPort())
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Not OpenPort");
                    return false;
                }

            Timer.Restart(); //= Stopwatch.StartNew();
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Send {pCommand} Data=>{pData}");
            lock (Lock)
            {
                if(Timer.ElapsedMilliseconds>20)
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Long Lock Time => { Timer.Elapsed}");
                isResultGot = false;
                CommandsCallbacks = onResponseCallback;

                try
                {
                    if (!SendPackage(pCommand, pData))
                    {
                        FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"SendPackage=>False Time => {Timer.Elapsed}",eTypeLog.Error);
                        return false;
                    }
                    StaticTimer.Wait((Func<bool>)(() => !isResultGot), 2);
                }
                catch (Exception ex)
                {
                    if (ex.Message.StartsWith("FP 700 has critical error"))
                        throw;
                }
            }           
            return isResultGot;
        }
        public bool SendPackage(eCommand command, string data = "", int waitingTimeout = 10)
        {
            // FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"{command} {data}");

           bool flag = command != eCommand.ClearDisplay && command != eCommand.ShiftInfo && command != eCommand.DiagnosticInfo && command != eCommand.EveryDayReport && command != eCommand.LastDocumentsNumbers && command != eCommand.ObliterateFiscalReceipt && command != eCommand.PaperCut && command != eCommand.GetDateTime && command != eCommand.PaperPulling && command != eCommand.LastZReportInfo && command != eCommand.PrintDiagnosticInformation;
           if (!IsZReportAlreadyDone & flag)
               return false;
           if (_hasCriticalError & flag)
               throw new Exception(Environment.NewLine + "Проблема з фіскальним реєстратором FP700: " + Environment.NewLine + _currentPrinterStatus.TextError);
           if (!SerialDevice.IsOpen)
               return false;
           
           byte[] bytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding(1251), Encoding.UTF8.GetBytes(data));

           byte[] buffer = new byte[218];
           int num1 = 0;
           int length = bytes.Length;
           int num2 = 0;
           if (length > 218)
               return false;
           byte[] numArray1 = buffer;
           int index1 = num1;
           int num3 = index1 + 1;
           numArray1[index1] = (byte)1;
           byte[] numArray2 = buffer;
           int index2 = num3;
           int num4 = index2 + 1;
           int num5 = (int)(byte)(36 + length);
           numArray2[index2] = (byte)num5;
           byte[] numArray3 = buffer;
           int index3 = num4;
           int num6 = index3 + 1;
           int num7 = (int)(byte)_sequenceNumber++;
           numArray3[index3] = (byte)num7;
           byte[] numArray4 = buffer;
           int index4 = num6;
           int num8 = index4 + 1;
           int num9 = (int)(byte)command;
           numArray4[index4] = (byte)num9;
           if (_sequenceNumber == 100)
               _sequenceNumber = 90;
           for (int index5 = 0; index5 < length; ++index5)
               buffer[num8++] = bytes[index5];
           byte[] numArray5 = buffer;
           int index6 = num8;
           int num10 = index6 + 1;
           numArray5[index6] = (byte)5;
           for (int index7 = 1; index7 < num10; ++index7)
               num2 += (int)buffer[index7] & (int)byte.MaxValue;
           byte[] numArray6 = buffer;
           int index8 = num10;
           int num11 = index8 + 1;
           int num12 = (int)(byte)((num2 >> 12 & 15) + 48);
           numArray6[index8] = (byte)num12;
           byte[] numArray7 = buffer;
           int index9 = num11;
           int num13 = index9 + 1;
           int num14 = (int)(byte)((num2 >> 8 & 15) + 48);
           numArray7[index9] = (byte)num14;
           byte[] numArray8 = buffer;
           int index10 = num13;
           int num15 = index10 + 1;
           int num16 = (int)(byte)((num2 >> 4 & 15) + 48);
           numArray8[index10] = (byte)num16;
           byte[] numArray9 = buffer;
           int index11 = num15;
           int num17 = index11 + 1;
           int num18 = (int)(byte)((num2 & 15) + 48);
           numArray9[index11] = (byte)num18;
           byte[] numArray10 = buffer;
           int index12 = num17;
           int count = index12 + 1;
           numArray10[index12] = (byte)3;
           //FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Sended {command} Buffer=>{Encoding.GetEncoding(1251).GetString(buffer)}");

           ((Stream)SerialDevice).Write(buffer, 0, count);
           ((Stream)SerialDevice).Flush();
           
         return true;
       }

       private bool OnDataReceived(byte[] data)
        {
            _isError = false;
            if (data.Length == 1)
            {
                if (data[0] == 21)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Printer error occured");
                    _isError = true;
                    return false;
                }
                if (data[0] == 22)
                {               
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Printer is waiting for a command");
                    return false;
                }
            }
           
            int num1 = Array.IndexOf<byte>(data, (byte)1);
            int num2 = Array.IndexOf<byte>(data, (byte)4);
            int num3 = Array.IndexOf<byte>(data, (byte)5);
            int num4 = Array.IndexOf<byte>(data, (byte)3);
            if (num1 < 0 || num2 < 0 || num3 < 0 || num4 < 0)
            {
                _packageBuffer.AddRange((IEnumerable<byte>)data);
                if (!_packageBuffer.Contains((byte)3))
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Printer received part of package. Waiting more...");
                    _packageBufferTimer.Start();
                    return false;
                }
                _packageBufferTimer.Stop();
                data = _packageBuffer.ToArray();
                _packageBuffer.Clear();
                num1 = Array.IndexOf<byte>(data, (byte)1);
                num2 = Array.IndexOf<byte>(data, (byte)4);
                num3 = Array.IndexOf<byte>(data, (byte)5);
                num4 = Array.IndexOf<byte>(data, (byte)3);
                if (num1 < 0 || num2 < 0 || num3 < 0 || num4 < 0)
                {
                    _packageBufferTimer.Start();
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Printer received invalid package.");
                    return false;
                }
            }
            eCommand Command = (eCommand)int.Parse(BitConverter.ToString(data, num1 + 3, 1), NumberStyles.HexNumber);
            int count = num2 - num1 - 4;
            byte[] ReceivedData = new byte[count];
            Buffer.BlockCopy((Array)data, num1 + 4, (Array)ReceivedData, 0, count);
            byte[] numArray2 = new byte[6];
            Buffer.BlockCopy((Array)data, num2 + 1, (Array)numArray2, 0, 6);
            //byte[] dst = new byte[4];
            //Buffer.BlockCopy((Array)data, num3 + 1, (Array)dst, 0, 4);
            _currentPrinterStatus = new();
            ShowStatus( (IReadOnlyList<byte>)numArray2);

            string Res = Encoding.UTF8.GetString(Encoding.Convert(Encoding.GetEncoding(1251), Encoding.UTF8, ReceivedData));
            CommandsCallbacks?.Invoke(Res);
            isResultGot=true;
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"CallBackResult {Command} Time=>{Timer.Elapsed} Res=>{Res}");
            return true;
        }

       private void ShowStatus( IReadOnlyList<byte> status)
       {
           for (int index1 = 0; index1 < 6; ++index1)
           {
               string str = Convert.ToString(status[index1], 2);
               for (int index2 = str.Length - 1; index2 >= 0; --index2)
               {
                   if (int.Parse(str[index2].ToString()) == 1)
                       GetStatusBitDescriptionBg(index1, str.Length - 1 - index2);
               }
           }           
       }       

        /*
        private bool OnSynchronizeWaitCommandResult( eCommand command, string data = "",  Action<string> onResponseCallback = null, Action<Exception> onExceptionCallback = null)
        {
            bool isResultGot = false;
            Stopwatch timer =  Stopwatch.StartNew();
           
            if (!StaticTimer.Wait((Func<bool>)(() => _commandsCallbacks.ContainsKey(command)), 2))
                _commandsCallbacks.Remove(command);
            _commandsCallbacks.Add(command, (Action<string>)(response =>
            {
                try
                {
                   onResponseCallback?.Invoke(response);
                   FileLogger.WriteLogMessage(this, "OnSynchronizeWaitCommandResult", $"CallBackResult {command} Time=>{timer.Elapsed} Data=>{data} Res=>{response}");
                   isResultGot = true;
                }
                catch (Exception ex)
                {                    
                    onExceptionCallback?.Invoke(ex);
                }
                finally
                {
                    _commandsCallbacks.Remove(command);
                }
            }));
            try
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Send {command} Data=>{data} _commandsCallbacks.Count=>{_commandsCallbacks.Count()}");
                if (!SendPackage(command, data))
                {                   
                    return false;
                }
                StaticTimer.Wait((Func<bool>)(() => !isResultGot),3);
                return isResultGot;
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("FP 700 has critical error"))
                    _commandsCallbacks.Remove(command);
                throw;
            }
        }

        public bool SendPackage(eCommand command, string data = "", int waitingTimeout = 10)
        {
           // FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"{command} {data}");
            
            bool flag = command != eCommand.ClearDisplay && command != eCommand.ShiftInfo && command != eCommand.DiagnosticInfo && command != eCommand.EveryDayReport && command != eCommand.LastDocumentsNumbers && command != eCommand.ObliterateFiscalReceipt && command != eCommand.PaperCut && command != eCommand.GetDateTime && command != eCommand.PaperPulling && command != eCommand.LastZReportInfo && command != eCommand.PrintDiagnosticInformation;
            if (!IsZReportAlreadyDone & flag)
                return false;
            if (_hasCriticalError & flag)
                throw new Exception(Environment.NewLine + "Проблема з фіскальним реєстратором FP700: " + Environment.NewLine + _currentPrinterStatus.TextError);
            if (!SerialDevice.IsOpen)
                return false;
            if (!_isReady)
            {
                _isReady = WaitForReady(waitingTimeout);
            }
            if (!_isReady)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Not Ready {command}",eTypeLog.Error);
                return false;
            }
            byte[] bytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding(1251), Encoding.UTF8.GetBytes(data));
            
            byte[] buffer = new byte[218];
            int num1 = 0;
            int length = bytes.Length;
            int num2 = 0;
            if (length > 218)
                return false;
            byte[] numArray1 = buffer;
            int index1 = num1;
            int num3 = index1 + 1;
            numArray1[index1] = (byte)1;
            byte[] numArray2 = buffer;
            int index2 = num3;
            int num4 = index2 + 1;
            int num5 = (int)(byte)(36 + length);
            numArray2[index2] = (byte)num5;
            byte[] numArray3 = buffer;
            int index3 = num4;
            int num6 = index3 + 1;
            int num7 = (int)(byte)_sequenceNumber++;
            numArray3[index3] = (byte)num7;
            byte[] numArray4 = buffer;
            int index4 = num6;
            int num8 = index4 + 1;
            int num9 = (int)(byte)command;
            numArray4[index4] = (byte)num9;
            if (_sequenceNumber == 100)
                _sequenceNumber = 90;
            for (int index5 = 0; index5 < length; ++index5)
                buffer[num8++] = bytes[index5];
            byte[] numArray5 = buffer;
            int index6 = num8;
            int num10 = index6 + 1;
            numArray5[index6] = (byte)5;
            for (int index7 = 1; index7 < num10; ++index7)
                num2 += (int)buffer[index7] & (int)byte.MaxValue;
            byte[] numArray6 = buffer;
            int index8 = num10;
            int num11 = index8 + 1;
            int num12 = (int)(byte)((num2 >> 12 & 15) + 48);
            numArray6[index8] = (byte)num12;
            byte[] numArray7 = buffer;
            int index9 = num11;
            int num13 = index9 + 1;
            int num14 = (int)(byte)((num2 >> 8 & 15) + 48);
            numArray7[index9] = (byte)num14;
            byte[] numArray8 = buffer;
            int index10 = num13;
            int num15 = index10 + 1;
            int num16 = (int)(byte)((num2 >> 4 & 15) + 48);
            numArray8[index10] = (byte)num16;
            byte[] numArray9 = buffer;
            int index11 = num15;
            int num17 = index11 + 1;
            int num18 = (int)(byte)((num2 & 15) + 48);
            numArray9[index11] = (byte)num18;
            byte[] numArray10 = buffer;
            int index12 = num17;
            int count = index12 + 1;
            numArray10[index12] = (byte)3;
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Sended {command} Buffer=>{Encoding.GetEncoding(1251).GetString(buffer)}");

            ((Stream)SerialDevice).Write(buffer, 0, count);
            ((Stream)SerialDevice).Flush();
            _isReady = false;
          return true;
        }

        private bool OnDataReceived(byte[] data)
        {
            _isError = false;
            if (data.Length == 1)
            {
                if (data[0] == 21)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Printer error occured");
                    _isError = true;
                    return false;
                }
                if (data[0] == 22)
                {
                    _isReady = false;
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Printer is waiting for a command");
                    return false;
                }
            }
            _isReady = true;
            int num1 = Array.IndexOf<byte>(data, (byte)1);
            int num2 = Array.IndexOf<byte>(data, (byte)4);
            int num3 = Array.IndexOf<byte>(data, (byte)5);
            int num4 = Array.IndexOf<byte>(data, (byte)3);
            if (num1 < 0 || num2 < 0 || num3 < 0 || num4 < 0)
            {
                _packageBuffer.AddRange((IEnumerable<byte>)data);
                if (!_packageBuffer.Contains((byte)3))
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Printer received part of package. Waiting more...");
                    _packageBufferTimer.Start();
                    return false;
                }
                _packageBufferTimer.Stop();
                data = _packageBuffer.ToArray();
                _packageBuffer.Clear();
                num1 = Array.IndexOf<byte>(data, (byte)1);
                num2 = Array.IndexOf<byte>(data, (byte)4);
                num3 = Array.IndexOf<byte>(data, (byte)5);
                num4 = Array.IndexOf<byte>(data, (byte)3);
                if (num1 < 0 || num2 < 0 || num3 < 0 || num4 < 0)
                {
                    _packageBufferTimer.Start();
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Printer received invalid package.");
                    return false;
                }
            }
            eCommand cmdNumber = (eCommand)int.Parse(BitConverter.ToString(data, num1 + 3, 1), NumberStyles.HexNumber);
            int count = num2 - num1 - 4;
            byte[] numArray1 = new byte[count];
            Buffer.BlockCopy((Array)data, num1 + 4, (Array)numArray1, 0, count);
            byte[] numArray2 = new byte[6];
            Buffer.BlockCopy((Array)data, num2 + 1, (Array)numArray2, 0, 6);
           //byte[] dst = new byte[4];
            //Buffer.BlockCopy((Array)data, num3 + 1, (Array)dst, 0, 4);
            _currentPrinterStatus = new();
            ShowStatus(cmdNumber, numArray1, (IReadOnlyList<byte>)numArray2);
            return true;
        }

        private void ShowStatus(eCommand cmdNumber, byte[] receivedData, IReadOnlyList<byte> status)
        {
            for (int index1 = 0; index1 < 6; ++index1)
            {
                string str = Convert.ToString(status[index1], 2);
                for (int index2 = str.Length - 1; index2 >= 0; --index2)
                {
                    if (int.Parse(str[index2].ToString()) == 1)
                        GetStatusBitDescriptionBg(index1, str.Length - 1 - index2);
                }
            }
            string str1 = Encoding.UTF8.GetString(Encoding.Convert(Encoding.GetEncoding(1251), Encoding.UTF8, receivedData));
            //FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, str1);

            if (!_commandsCallbacks.ContainsKey(cmdNumber))
                return;
           _commandsCallbacks[cmdNumber]?.Invoke(str1);           
        }
        */

        private string GetStatusBitDescriptionBg(int byteIndex, int bitIndex)
        {
            string bitDescriptionBg = "";
            try
            {
                switch (byteIndex)
                {
                    case 0:
                        switch (bitIndex)
                        {
                            case 0:
                                bitDescriptionBg = "# Синтаксическая ошибка.";
                                _currentPrinterStatus.IsSyntaxError = true;
                                break;
                            case 1:
                                bitDescriptionBg = "# Недопустимая команда";
                                _currentPrinterStatus.IsCommandNotPermited = true;
                                break;
                            case 2:
                                bitDescriptionBg = "Не установлены дата/время";
                                _currentPrinterStatus.IsDateAndTimeNotSet = true;
                                break;
                            case 3:
                                bitDescriptionBg = "Внешний дисплей не подключен.";
                                _currentPrinterStatus.IsDisplayDisconnected = true;
                                break;
                            case 4:
                                bitDescriptionBg = "# SAM не от этого устройства (регистратор не персонализирован).";
                                break;
                            case 5:
                                bitDescriptionBg = "Общая ошибка или все ошибки, обозначенные `#`";
                                _currentPrinterStatus.IsCommonError = true;
                                break;
                        }
                        break;
                    case 1:
                        switch (bitIndex)
                        {
                            case 0:
                                bitDescriptionBg = "При выполнении команды произошло переполнение поля суммы. Также вызывает появление статуса 1.1 и команда не приведет к изменению данных в регистраторе";
                                break;
                            case 1:
                                bitDescriptionBg = "# Выполнение команды не допускается в текущем фискальном режиме";
                                break;
                            case 2:
                                bitDescriptionBg = "# Аварийное обнуление RAM.";
                                break;
                            case 3:
                                bitDescriptionBg = "Открыт возвратный чек.";
                                break;
                            case 4:
                                bitDescriptionBg = "# Ошибка SAM";
                                break;
                            case 5:
                                bitDescriptionBg = "Крышка принтера открыта.";
                                _hasCriticalError = true;
                                _currentPrinterStatus.IsCoverOpen = true;
                                ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.Error, "[FP700] Cover is open")
                                { Status = eStatusRRO.Error, IsСritical = true });
                                break;
                            case 6:
                                bitDescriptionBg = "Регистратор персонализирован";
                                _currentPrinterStatus.IsPrinterFiscaled = true;
                                break;
                        }
                        break;
                    case 2:
                        switch (bitIndex)
                        {
                            case 0:
                                bitDescriptionBg = "# Бумага закончилась. Если этот статус возникнет при выполнении команды, связанной с печатью, то команда будет отклонена и состояние регистратора не изменится.";
                                _hasCriticalError = true;
                                _currentPrinterStatus.IsOutOffPaper = true;
                                ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.Error, "[FP700] Paper was ended")
                                { Status = eStatusRRO.Error, IsСritical = true });
                                break;
                            case 1:
                                bitDescriptionBg = "Заканчивается бумага";
                                _currentPrinterStatus.IsPaperNearEnd = true;
                                ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.On, "[FP700] Paper near end")
                                { Status = eStatusRRO.Warning, IsСritical = false });

                                break;
                            case 2:
                                bitDescriptionBg = "Носитель КЛЭФ заполнен (Осталось менее 1 МБ)";
                                _currentPrinterStatus.IsKSEFMemoryFull = true;
                                _hasCriticalError = true;
                                ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.Error, "[FP700] Fiscal memory is full")
                                { Status = eStatusRRO.Error, IsСritical = true });
                                break;
                            case 3:
                                bitDescriptionBg = "Открыт фискальный чек";
                                _currentPrinterStatus.IsFiscalReceiptOpen = true;
                                break;
                            case 4:
                                bitDescriptionBg = "Носитель КЛЭФ приближается к заполнению (осталось не более 2 МБ)";
                                break;
                            case 5:
                                _currentPrinterStatus.IsNoFiscalReceiptOpen = true;
                                bitDescriptionBg = "Открыт нефискальный чек.";
                                break;
                            case 6:
                                bitDescriptionBg = "Носитель КЛЭФ почти заполнен (осталось примерно 1 МБ, возможна печать только отдельных чеков)";
                                break;
                        }
                        break;
                    case 3:
                        switch (bitIndex)
                        {
                            case 0:
                                bitDescriptionBg = "Состояние переключателя 1";
                                break;
                            case 1:
                                bitDescriptionBg = "Состояние переключателя 2";
                                break;
                            case 2:
                                bitDescriptionBg = "Состояние переключателя 3";
                                break;
                            case 3:
                                bitDescriptionBg = "Состояние переключателя 4";
                                break;
                            case 4:
                                bitDescriptionBg = "Состояние переключателя 5";
                                break;
                            case 5:
                                bitDescriptionBg = "Состояние переключателя 6";
                                break;
                            case 6:
                                bitDescriptionBg = "Состояние переключателя 7";
                                break;
                        }
                        break;
                    case 4:
                        switch (bitIndex)
                        {
                            case 0:
                                _hasCriticalError = true;
                                bitDescriptionBg = "*В фискальной памяти присутствуют ошибки";
                                _currentPrinterStatus.IsErrorOnWritingToFiscalMemory = true;
                                ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.Error, "[FP700] Fiscal Memory have error")
                                { Status = eStatusRRO.Error, IsСritical = true });

                                break;
                            case 1:
                                _hasCriticalError = true;
                                bitDescriptionBg = "Фискальная память неработоспособна";
                                _currentPrinterStatus.IsCommonFiscalError = true;
                                ActionStatus?.Invoke(new RroStatus(eModelEquipment.RRO_FP700, eStateEquipment.Error, "[FP700] Fiscal Memory not working")
                                { Status = eStatusRRO.Error, IsСritical = true });


                                break;
                            case 2:
                                bitDescriptionBg = "Заводской номер запрограммирован";
                                _currentPrinterStatus.IsSerialNumberSet = true;
                                break;
                            case 3:
                                bitDescriptionBg = "Осталось менее 50 записей в фискальную память";
                                _currentPrinterStatus.IsRecordsLowerThanFifty = true;
                                break;
                            case 4:
                                bitDescriptionBg = "* Заполнение фискальной памяти.";
                                _currentPrinterStatus.IsFiscalMemoryFull = true;
                                break;
                            case 5:
                                bitDescriptionBg = "Все ошибки с пометкой «*» для байтов 4 и 5.";
                                break;
                        }
                        break;
                    case 5:
                        switch (bitIndex)
                        {
                            case 0:
                                bitDescriptionBg = "* Фискальная память в режиме READONLY (`только чтение`)";
                                _currentPrinterStatus.IsFiscalMemoryReadOnly = true;
                                break;
                            case 1:
                                bitDescriptionBg = "Последняя запись в фискальную память была неудачной";
                                break;
                            case 2:
                                bitDescriptionBg = "Последняя запись в фискальную память была неудачной";
                                break;
                            case 3:
                                bitDescriptionBg = "Регистратор фискализирован";
                                _currentPrinterStatus.IsPrinterFiscaled = true;
                                break;
                            case 4:
                                bitDescriptionBg = "Налоговые ставки запрограммирован";
                                break;
                            case 5:
                                bitDescriptionBg = "Фискальный номер запрограммирован";
                                break;
                            case 6:
                                bitDescriptionBg = "Налоговый номер запрограммирован";
                                break;
                        }
                        break;
                    default:
                        bitDescriptionBg = "";
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex.Message, (object)ex.StackTrace);
            }
            return bitDescriptionBg;
        }

        /*private bool WaitForReady(int waitingTimeOut = 5)
        {
            StaticTimer.Wait((Func<bool>)(() => !_isReady), waitingTimeOut);
            return _isReady;
        }*/

        private void CloseIfOpened()
        {
            SerialDevice.Close();
            if (!SerialDevice.PortName.Equals(SerialPort))
                SerialDevice.PortName = SerialPort;
            if(SerialDevice.BaudRate!=BaudRate)
                SerialDevice.BaudRate = BaudRate;
        }

        public void Dispose()
        {
            CloseIfOpened();
            ((Stream)SerialDevice).Dispose();
        }

        volatile bool IsStop = false;
        bool IsFinish;
        StringBuilder bb;
        public string KSEFGetReceipt(string pCodeReceipt)
        {
            IsStop = false;
            bb = new();
            string res = null;
            OnSynchronizeWaitCommandResult(eCommand.KSEF, $"R,{pCodeReceipt}", ResKSEF);
            while (!IsFinish && !IsStop)
            {
                OnSynchronizeWaitCommandResult(eCommand.KSEF, $"N", ResKSEF);
            }
            if(!IsStop)
                CloseIfOpened();
            IsStop = false;
            return bb.ToString();
        }

        void ResKSEF(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                IsFinish = true;
            }
            else
            {                
                IsFinish = (response[0] == 'F');
                if (response.Length > 1)
                {
                    bb.Append(response.Substring(2));
                    bb.Append(Environment.NewLine);
                }
            }
        }

        public override decimal GetSumInCash()
        {
            decimal Sum = -1;
            OnSynchronizeWaitCommandResult(eCommand.ServiceCashInOut, "", (Action<string>)(response =>
            {
                string[] strArray = response.Split(',');
                if (strArray.Length < 3)
                    return;
                if (strArray[0].Equals("P") && strArray.Length >= 2)
                {
                    Sum = strArray[1].ToDecimal() / 100m;
                }
                else
                {
                    if (!strArray[0].Equals("F"))
                        return;
                }
            }));            
            return Sum;
        }
        
        override public bool PutToDisplay(string pText=null, int pLine = 1)
        {
            if (string.IsNullOrEmpty(pText))
                return false;
            if(pLine==0)
            {
                var R=pText.Split(Environment.NewLine);
                if (R.Length >= 1) PutToDisplay(R[0], 1);
                if (R.Length >= 2) PutToDisplay(R[1], 2);
                return true;
            }
            
            if(pLine== 1) ClearDisplay();
            if (!string.IsNullOrEmpty(pText))
            {
                if (pText?.Length > 20)
                    pText = pText[..20];
                if (!string.IsNullOrWhiteSpace(pText))
                    OnSynchronizeWaitCommandResult(pLine==1? eCommand.FirstLineDisplay: eCommand.LastLineDisplay, pText);
            }
            return true;
        }
    }
}