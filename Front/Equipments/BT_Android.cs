using Front.Equipments.Utils;
using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelMID;
using RJCP.IO.Ports;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using static Front.Models.KeyBoardUtilities;

namespace Front.Equipments.Implementation
{
    public class BT_Android : BankTerminal
    {
        private SerialPortStreamWrapper SerialDevice;
        private readonly object Lock = new object();
        int LastResult = 0;

        Payment Res = null;
        bool IsRes = false;
        bool? IsResLongOperation = null;
        string StrRes = null;
        Answer<dynamic> ResAnswer = null;       
        BatchTotals ResBatchTotals = null;

        public BT_Android(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) : base(pEquipment, pConfiguration, eModelEquipment.VirtualBankPOS, pLoggerFactory, pActionStatus)
        {
            Init();
            //ActionStatus = pActionStatus;
        }

        public override void Init()
        {
            lock (Lock)
            {
                TextError = string.Empty;
                try
                {
                    State = eStateEquipment.Init;
                    OpenPort();
                    SerialDevice.Open();
                    SerialDevice.DiscardInBuffer();
                    SerialDevice.DiscardOutBuffer();
                    SendCommand(eCommand.PingDevice,null,10*1000);
                    //State = eStateEquipment.On;
                }
                catch (Exception ex)
                {
                    TextError = ex.Message;
                    State = eStateEquipment.Error;
                }
                finally
                {
                    SerialDevice.OnReceivedData = new Func<byte[], bool>(OnDataReceived);
                }
            }
        }

        private void OpenPort()
        {
            if (SerialDevice != null)
            {
                if (IsReady)
                    SerialDevice.Close();
                SerialDevice.Dispose();
            }
            SerialPortStreamWrapper portStreamWrapper = new SerialPortStreamWrapper(SerialPort, BaudRate, Parity.None, StopBits.One, 8, new Func<byte[], bool>(OnDataReceived));
            portStreamWrapper.RtsEnable = true;
            SerialDevice = portStreamWrapper;
        }

        public override Payment Purchase(decimal pAmount, decimal pCash, int pIdWorkPlace = 0)
        {
            byte MerchantId = GetMechantIdByIdWorkPlace(pIdWorkPlace);
            SendCommand(pCash > 0 ? eCommand.Cashback : eCommand.Purchase, new PPurchase() { amount = pAmount.ToS(), discount = "0", merchantId = MerchantId.ToString() }.ToJSON(), 120 * 1000);
            return Res;
        }

        public override Payment Refund(decimal pAmount, string pRRN, int pIdWorkPlace = 0)
        {
            byte MerchantId = GetMechantIdByIdWorkPlace(pIdWorkPlace);
            SendCommand(eCommand.Refund, new { amount = pAmount.ToS(), rrn = pRRN, merchantId= MerchantId.ToString() }.ToJSON(), 120 * 1000);
            return Res;
        }

        public override BatchTotals PrintZ(int pIdWorkPlace = 0)
        {
            byte MerchantId = GetMechantIdByIdWorkPlace(pIdWorkPlace);
            SendCommand(eCommand.Verify, new { merchantId = MerchantId.ToString() }.ToJSON(), 20 * 1000);
            return ResBatchTotals;
        }

        public override BatchTotals PrintX(int pIdWorkPlace = 0)
        {
            byte MerchantId = GetMechantIdByIdWorkPlace(pIdWorkPlace);
            SendCommand(eCommand.Audit, new { merchantId = MerchantId.ToString() }.ToJSON(), 20 * 1000);
            return ResBatchTotals;
        }

        public override StatusEquipment TestDevice()
        {
            Init();
            SendCommand(eCommand.ServiceMessage, new { msgType = "identify" }.ToJSON(), 1000);
            string rr = $"{Environment.NewLine}{ResAnswer.@params}";
            SendCommand(eCommand.GetTerminalInfo, null, 1000);
            return new StatusEquipment(Model, State, $"{Environment.NewLine}{ResAnswer.@params}{Environment.NewLine}{rr}");
        }

        public override IEnumerable<string> GetLastReceipt()
        {
            SendCommand(eCommand.PrintReceiptNum, new { merchantId = "0" }.ToJSON(), 20 * 1000);
            return ResBatchTotals?.Receipt;
        }

        bool SendCommand(eCommand pC, string pParam = "{}", int pWait = 0)
        {
            lock (Lock)
            {
                try
                {
                    LastResult = 2;
                    string Cmd = null;
                    byte delimiter = 0x00;
                    List<byte> data = new List<byte>();

                    switch (pC)
                    {
                        case eCommand.NotDefine:
                            break;
                        case eCommand.PingDevice:
                            data.Add(delimiter);
                            Cmd = "{\"method\":\"PingDevice\",\"step\":0}";
                            break;
                        case eCommand.GetTerminalInfo:
                            Cmd = "{\"method\":\"" + pC.ToString() + "\",\"step\":0}";
                            break;

                        case eCommand.Audit:
                        case eCommand.Verify:
                        case eCommand.PrintBatchJournal:
                        case eCommand.ServiceMessage:
                        case eCommand.Refund:
                        case eCommand.Purchase:
                        case eCommand.Cashback:
                        case eCommand.PrintReceiptNum:
                            Cmd = "{\"method\":\"" + pC.ToString() + "\",\"step\":0,\"params\":" + pParam + " }";
                            break;
                    }
                    List<byte> message = Encoding.UTF8.GetBytes(Cmd).ToList();
                    data.AddRange(message);
                    data.Add(delimiter);
                    var buffer = data.ToArray();
                    IsRes = false;
                    if (pC == eCommand.Purchase || pC == eCommand.Refund || pC == eCommand.Cashback)
                        IsResLongOperation = false;
                    SerialDevice.Write(buffer, 0, buffer.Length);
                    bool res;
                    if (pC == eCommand.Purchase || pC == eCommand.Refund || pC == eCommand.Cashback)
                        res = WaitWithStatus(pWait);
                    else
                        res = Wait(pWait);
                    return res;

                }
                catch (Exception ex)
                {
                    State = eStateEquipment.Error;
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                    return false;
                }
            }
        }

        public bool Wait(int pTime = 1000)
        {
            pTime = pTime / 10;
            while (pTime > 0 && !IsRes)
            {
                Thread.Sleep(10);
                pTime--;
            }
            return IsRes;
        }

        public bool WaitWithStatus(int pTime = 120 * 1000)
        {
            pTime = pTime / 200;
            while (pTime > 0 && IsResLongOperation == false)
            {
                Thread.Sleep(500);
                SendCommand(eCommand.ServiceMessage, new { msgType = "getLastStatMsgCode" }.ToJSON(), 0);
                pTime--;
            }
            return IsResLongOperation ?? true;
        }

        private bool OnDataReceived(byte[] data)
        {
            IdReceipt curIdReceipt = null;
            int len = data.Length - 1;
            if (len < 0) return false;
            while (data[len] == 0)
                len--;
            if (len < 2) return false;
            StrRes = Encoding.UTF8.GetString(data[0..(len + 1)]);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, StrRes);
            var strs = StrRes.Split((char)0);
            foreach (var el in strs)
                ParserRes(el);
            IsRes = true;
            return true;
        }

        eCommand ParserRes(string pStr)
        {
            eCommand Command = eCommand.NotDefine;
            try
            {
                Answer<object> res = JsonSerializer.Deserialize<Answer<object>>(pStr);                
                Enum.TryParse<eCommand>(res.method, out Command);
                switch (Command)
                {
                    case eCommand.NotDefine:
                        break;
                    case eCommand.PingDevice:
                        ResAnswer = JsonSerializer.Deserialize<Answer<dynamic>>(pStr);
                        if (!ResAnswer.error)
                            State = eStateEquipment.On;
                        break;
                    case eCommand.GetTerminalInfo:
                    case eCommand.ServiceMessage:
                        var ResServiceMessage = JsonSerializer.Deserialize<Answer<PAServiceMessage>>(pStr);
                        byte LastStatMsgCode;
                        if ("getLastStatMsgCode".Equals(ResServiceMessage.@params.msgType) && !string.IsNullOrEmpty(ResServiceMessage.@params.LastStatMsgCode) && byte.TryParse(ResServiceMessage.@params.LastStatMsgCode, out LastStatMsgCode))
                        {
                            InvokeLastStatusMsg(LastStatMsgCode);
                        }
                        ResAnswer = JsonSerializer.Deserialize<Answer<dynamic>>(pStr);
                        break;
                    case eCommand.Refund:
                    case eCommand.Purchase:
                    case eCommand.Cashback:
                        var ResJson = JsonSerializer.Deserialize<Answer<PAPurchase>>(pStr);
                        var par = ResJson.@params;
                        Res = new Payment()
                        {
                            Bank = par.bankAcquirer,
                            CardHolder = par.cardHolderName,
                            CodeAuthorization = par.rrn,
                            CodeBank = CodeBank,
                            IssuerName = par.issuerName,
                            IsSuccess = !ResJson.error,
                            NumberCard = par.pan,
                            SumPay = par.amount?.ToDecimal() ?? 0,
                            NumberReceipt = par.invoiceNumber?.ToInt() ?? 0,
                            NumberSlip = par?.approvalCode,
                            NumberTerminal = par.terminalId,
                            Receipt = par.receipt?.Split('\n'),
                            SumExt = par.amountCash?.ToDecimal() ?? 0,
                            TransactionId = par.processingCode
                        };
                        IsResLongOperation = true;
                        break;
                    case eCommand.Audit:
                    case eCommand.Verify:
                    case eCommand.PrintBatchJournal:
                    case eCommand.PrintReceiptNum:
                        var ResJsonR = JsonSerializer.Deserialize<Answer<PAReceipt>>(pStr);
                        ResBatchTotals = new BatchTotals() { Receipt = ResJsonR.@params.receipt?.Split('\n') };
                        break;
                }
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);                
            }
            return Command;
        }
    }

    #region Model
    enum eCommand
    {
        NotDefine = -1,
        PingDevice,
        ServiceMessage,
        /// <summary>
        /// Оплата
        /// </summary>
        Purchase,
        Refund,
        /// <summary>
        /// Видача готівки
        /// </summary>
        Cashback,
        GetTerminalInfo,
        /// <summary>
        /// X-звіт
        /// </summary>
        Audit,
        //Z-звіт
        Verify,
        //Друк чека(Останнього)
        PrintReceiptNum,
        /// <summary>
        /// X-звіт??
        /// </summary>
        PrintBatchJournal

    }

    enum eServiceMessage
    {
        NotDefine = -1,
        deviceBusy,
        interrupt,
        interruptTransmitted,
        methodNotImplemented,
        getMerchantList,
        getMaskList,
        debug,
        getLastResult,
        getLastStatMsgCode,
        getLastStatMsgDescription,
        identify
    }

    class Params
    {
        public string answer { get; set; }
    }

    class PAPingDevice
    {
        public string code { get; set; }
        public string responseCode { get; set; }
    }

    class PAPurchase
    {
        public string amount { get; set; }
        public string amountCash { get; set; }
        public string approvalCode { get; set; }
        public string invoiceNumber { get; set; }
        public string merchant { get; set; }
        public string receipt { get; set; }
        public string result { get; set; }
        public string resultCashback { get; set; }
        public string rrn { get; set; }
        public string rrnExt { get; set; }
        public string captureReference { get; set; }
        public string cardExpiryDate { get; set; }
        public string cardHolderName { get; set; }
        public string date { get; set; }
        public string discount { get; set; }
        public string hstFld63Sf89 { get; set; }
        public string issuerName { get; set; }
        //public string merchant { get; set; }
        public string pan { get; set; }
        public string posConditionCode { get; set; }
        public string posEntryMode { get; set; }
        public string processingCode { get; set; }
        public string responseCode { get; set; }
        public string terminalId { get; set; }
        public string time { get; set; }
        public string track1 { get; set; }
        public string adv { get; set; }
        public string adv2p { get; set; }
        public string bankAcquirer { get; set; }
        public string paymentSystem { get; set; }
        public string subMerchant { get; set; }
        public string signVerif { get; set; }
        public string txnType { get; set; }
        public string trnStatus { get; set; }       
    }

    class PPurchase
    {
        public string amount { get; set; }
        public string amountCash { get; set; }
        public string discount { get; set; }
        public string merchantId { get; set; }
        public string facepay { get; set; } = "false";
        public string subMerchant { get; set; }
    }
    
    class PAReceipt 
    {
        public string receipt { get; set; }
        public string responseCode { get; set; }
    }
    class PAServiceMessage
    {
        public string msgType { get; set; }
        public string LastStatMsgCode { get; set; }
        public string LastStatMsgDescription { get; set; }
        //public string result { get; set; }
        //public string vendor { get; set; }
        //public string model { get; set; }

    }
    
    class Request<par>
    {
        public Request() { }
        public Request(eCommand pC,par pP) { method = pC.ToString();@params = pP; }
        public string method { get; set; }
        public int step { get; set; } = 0;
        public par @params { get; set; }        
    }

    class Answer<par>: Request<par>
    {
        public Answer() { }       
        public bool error { get; set; }
        public string errorDescription { get; set; }
    }
    #endregion
}
