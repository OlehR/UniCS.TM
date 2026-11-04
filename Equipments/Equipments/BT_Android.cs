using Front.Equipments.Utils;
using Microsoft.Extensions.Configuration;
using ModelMID;
using RJCP.IO.Ports;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Utils;

namespace Front.Equipments.Implementation
{
    public class BT_Android : BankTerminal
    {
        private SerialPortStreamWrapper SerialDevice;
        private readonly object Lock = new object();
        int LastResult = 0;

        Payment Res = null;
        volatile bool IsRes = false;
        bool? IsResLongOperation = null;
        string StrRes = null;
        Answer<dynamic> ResAnswer = null;
        BatchTotals ResBatchTotals = null;
        /// <summary>
        /// Команда (і msgType для ServiceMessage), відповідь на яку очікуємо
        /// </summary>
        eCommand WaitCommand = eCommand.NotDefine;
        string WaitMsgType = null;
        /// <summary>
        /// Буфер прийнятих байт до делімітера 0x00
        /// </summary>
        readonly List<byte> RcvBuffer = new List<byte>();
        const int MaxRcvBuffer = 1024 * 1024;

        /// <summary>
        /// Якщо в налаштуваннях вказано IP - працюємо через TCP (Ethernet/Wi-Fi), інакше через COM
        /// </summary>
        /// <summary>
        /// responseCode останньої фінансової операції
        /// </summary>
        string ResResponseCode = null;
        /// <summary>
        /// Останній отриманий LastStatMsgCode (11 - коригування транзакції)
        /// </summary>
        volatile int LastStatMsgCodeValue = 0;
        volatile bool IsDiscountName = false;
        string DiscountPan = null;
        /// <summary>
        /// Параметри поточної оплати (для коригування і перевірки картки нацкешбеку)
        /// </summary>
        decimal CurSum = 0;
        bool CurIsCashBack = false;
        /// <summary>
        /// Статус, з яким касою скасовано операцію (картка не відповідає нацкешбеку)
        /// </summary>
        eStatusPos? CancelStatus = null;

        bool IsTcp => !string.IsNullOrEmpty(IP);
        const int DefaultIpPort = 2000;
        TcpClient Tcp;
        NetworkStream TcpStream;
        /// <summary>
        /// Ширина рядка чека для екрана/принтера (чек з термінала приходить в HTML або тексті)
        /// </summary>
        readonly int ReceiptWidth;

        public BT_Android(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) : base(pEquipment, pConfiguration, eModelEquipment.VirtualBankPOS, pLoggerFactory, pActionStatus)
        {
            if (BaudRate <= 0)
                BaudRate = 115200; //За протоколом 115200 8N1
            ReceiptWidth = Configuration?.GetValue<int>($"{KeyPrefix}ReceiptWidth") ?? 0;
            if (ReceiptWidth <= 0)
                ReceiptWidth = PosReceiptText.DefaultWidth;
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
                    lock (RcvBuffer)
                        RcvBuffer.Clear();
                    if (IsTcp)
                        OpenTcp();
                    else
                    {
                        OpenPort();
                        SerialDevice.Open();
                        SerialDevice.DiscardInBuffer();
                        SerialDevice.DiscardOutBuffer();
                    }
                    if (SendCommand(eCommand.PingDevice, null, 10 * 1000))
                        SetCodeBank();
                }
                catch (Exception ex)
                {
                    TextError = ex.Message;
                    State = eStateEquipment.Error;
                }
                finally
                {
                    if (SerialDevice != null)
                        SerialDevice.OnReceivedData = new Func<byte[], bool>(OnDataReceived);
                }
            }
        }

        /// <summary>
        /// Постійне TCP-з'єднання з терміналом (порт за замовчуванням 2000) + фоновий потік читання
        /// </summary>
        private void OpenTcp()
        {
            CloseTcp();
            int Port = IpPort > 0 ? IpPort : DefaultIpPort;
            var Client = new TcpClient();
            if (!Client.ConnectAsync(IP, Port).Wait(5000))
            {
                Client.Dispose();
                throw new TimeoutException($"Немає з'єднання з терміналом {IP}:{Port}");
            }
            Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            Tcp = Client;
            TcpStream = Client.GetStream();
            var Stream = TcpStream;
            new Thread(() => ReadTcp(Client, Stream)) { IsBackground = true, Name = "BT_Android_TCP" }.Start();
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Connected {IP}:{Port}");
        }

        private void CloseTcp()
        {
            try { Tcp?.Close(); } catch { }
            Tcp = null;
            TcpStream = null;
            lock (RcvBuffer)
                RcvBuffer.Clear();
        }

        private void ReadTcp(TcpClient pClient, NetworkStream pStream)
        {
            var Buf = new byte[4096];
            try
            {
                int n;
                while ((n = pStream.Read(Buf, 0, Buf.Length)) > 0)
                    OnDataReceived(Buf[0..n]);
            }
            catch (Exception ex)
            {
                if (ReferenceEquals(pClient, Tcp))
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            if (ReferenceEquals(pClient, Tcp))
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Disconnected {IP}");
        }

        /// <summary>
        /// Запис у COM або TCP. Для TCP при обриві - перепідключення
        /// </summary>
        private void WriteData(byte[] pBuffer)
        {
            if (IsTcp)
            {
                if (Tcp == null || !Tcp.Connected)
                    OpenTcp();
                TcpStream.Write(pBuffer, 0, pBuffer.Length);
            }
            else
                SerialDevice.Write(pBuffer, 0, pBuffer.Length);
        }

        void SetCodeBank()
        {
            CodeBank = eBank.PrivatBank;//!!!TMP В майбутньому треба доробити
            if (CodeBank == eBank.NotDefine)
                State = eStateEquipment.Error;
            else
                State = eStateEquipment.On;
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

        public override Payment Purchase(decimal pAmount, decimal pCash, int pIdWorkPlace = 0, bool pIsCashBack = false)
        {
            byte MerchantId = GetMechantIdByIdWorkPlace(pIdWorkPlace, pIsCashBack);
            PPurchase Par = pCash > 0 ?
                new PPurchase() { amount = pAmount.ToS(), amountCash = pCash.ToS(), merchantId = MerchantId.ToString(), facepay = null } :
                new PPurchase() { amount = pAmount.ToS(), discount = "0", merchantId = MerchantId.ToString() };
            CurSum = pAmount;
            CurIsCashBack = pIsCashBack;
            CancelStatus = null;
            OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.WaitingForCard });
            SendCommand(pCash > 0 ? eCommand.Cashback : eCommand.Purchase, Par.ToJSON(), 120 * 1000);
            return GetPaymentResult(pIsCashBack);
        }

        public override Payment Refund(decimal pAmount, string pRRN, int pIdWorkPlace = 0, bool pIsCashBack = false)
        {
            byte MerchantId = GetMechantIdByIdWorkPlace(pIdWorkPlace, pIsCashBack);
            CurSum = pAmount;
            CurIsCashBack = pIsCashBack;
            CancelStatus = null;
            OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.WaitingForCard });
            SendCommand(eCommand.Refund, new { amount = pAmount.ToS(), rrn = pRRN, merchantId= MerchantId.ToString() }.ToJSON(), 120 * 1000);
            return GetPaymentResult(pIsCashBack);
        }

        /// <summary>
        /// Результат фінансової операції + статус для екрана покупця (успіх або причина відмови)
        /// </summary>
        Payment GetPaymentResult(bool pIsCashBack)
        {
            Payment r = Res;
            if (CancelStatus != null)
            {
                if (r?.IsSuccess == true) // Термінал провів оплату, незважаючи на interrupt - гроші списані, повертаємо як є
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Operation approved after interrupt ({CancelStatus}) RRN=>{r.CodeAuthorization}", eTypeLog.Error);
                else
                {
                    OnStatus?.Invoke(new PosStatus() { Status = CancelStatus.Value });
                    return new Payment() { IsSuccess = false, CodeBank = CodeBank, IsCashBack = pIsCashBack };
                }
            }
            if (r == null)
                return new Payment() { IsSuccess = false, CodeBank = CodeBank, IsCashBack = pIsCashBack };
            r.IsCashBack = pIsCashBack;
            if (r.IsSuccess)
                OnStatus?.Invoke(new PosStatus() { Status = eStatusPos.SuccessfullyFulfilled });
            else if (uint.TryParse(ResResponseCode, out uint ResponseCode))
            {
                var Status = GetStatusByResponseCode(ResponseCode);
                if (Status != null)
                    OnStatus?.Invoke(new PosStatus() { Status = Status.Value });
            }
            return r;
        }

        /// <summary>
        /// LastStatMsgCode = 11: перевірка картки нацкешбеку (як у BT_Ingenico) і коригування транзакції (correctTransaction)
        /// </summary>
        void CorrectTransaction()
        {
            var Prefixes = Global.Settings?.CashBackCard?.Where(el => el?.Length > 0 && !string.IsNullOrEmpty(el[0])).Select(el => el[0]).ToList();
            if (Prefixes?.Count > 0)
            {
                string Pan = GetDiscountPan();
                // PAN може бути замаскований - перевіряти можна тільки якщо відкриті цифри покривають префікси
                bool IsKnownPan = !string.IsNullOrEmpty(Pan) && Prefixes.All(el => Pan.Length >= el.Length && Pan.Take(el.Length).All(char.IsDigit));
                bool IsCashBackCard = IsKnownPan && Prefixes.Any(el => Pan.StartsWith(el));
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"pIsCashBack=>{CurIsCashBack} IsCashBackCard=>{IsCashBackCard} IsKnownPan=>{IsKnownPan} {Pan}");
                if (CurIsCashBack && !IsKnownPan)
                    CancelStatus = eStatusPos.TerminalNotReadyCashBack;
                else if (CurIsCashBack != IsCashBackCard)
                    CancelStatus = CurIsCashBack ? eStatusPos.NoCashBackCard : eStatusPos.CashBackCardUseProhibited;
                if (CancelStatus != null)
                {
                    SendServiceMessage("interrupt");
                    return;
                }
            }
            SendServiceMessage(new { msgType = "correctTransaction", amount = CurSum.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), discount = "0.00" });
        }

        /// <summary>
        /// PAN картки поточної транзакції (ServiceMessage getDiscountName)
        /// </summary>
        string GetDiscountPan()
        {
            IsDiscountName = false;
            DiscountPan = null;
            SendServiceMessage("getDiscountName");
            for (int i = 0; i < 300 && !IsDiscountName; i++)
                Thread.Sleep(10);
            return "PAN_UNDEF".Equals(DiscountPan) ? null : DiscountPan;
        }

        public override BatchTotals PrintZ(int pIdWorkPlace = 0)
        {
            byte MerchantId = GetMechantIdByIdWorkPlace(pIdWorkPlace);
            SendCommand(eCommand.Verify, new { merchantId = MerchantId.ToString() }.ToJSON(), 90 * 1000);
            return ResBatchTotals;
        }

        public override BatchTotals PrintX(int pIdWorkPlace = 0)
        {
            byte MerchantId = GetMechantIdByIdWorkPlace(pIdWorkPlace);
            SendCommand(eCommand.Audit, new { merchantId = MerchantId.ToString() }.ToJSON(), 90 * 1000);
            return ResBatchTotals;
        }

        public override StatusEquipment TestDevice()
        {
            Init();
            SendCommand(eCommand.ServiceMessage, new { msgType = "identify" }.ToJSON(), 1000, "identify");
            string rr = $"{Environment.NewLine}{ResAnswer?.@params}";
            SendCommand(eCommand.GetTerminalInfo, null, 1000);
            return new StatusEquipment(Model, State, $"{Environment.NewLine}{ResAnswer?.@params}{Environment.NewLine}{rr}");
        }

        public override IEnumerable<string> GetLastReceipt()
        {
            // invoiceNumber "0" - останній чек у пакеті, prnFlag "0" - не друкувати повторно на терміналі
            SendCommand(eCommand.PrintReceiptNum, new { invoiceNumber = "0", prnFlag = "0" }.ToJSON(), 20 * 1000);
            return ResBatchTotals?.Receipt;
        }

        public override void Cancel() {
            // Асинхронне сервісне повідомлення - не змінює стан очікування поточної операції
            SendServiceMessage("interrupt");
        }

        /// <summary>
        /// Відправка сервісного повідомлення без очікування відповіді (можна під час виконання іншої операції)
        /// </summary>
        void SendServiceMessage(string pMsgType) => SendServiceMessage(new { msgType = pMsgType });

        void SendServiceMessage(object pParams)
        {
            try
            {
                string Cmd = "{\"method\":\"" + eCommand.ServiceMessage.ToString() + "\",\"step\":0,\"params\":" + pParams.ToJSON() + " }";
                List<byte> data = Encoding.UTF8.GetBytes(Cmd).ToList();
                data.Add(0x00);
                var buffer = data.ToArray();
                lock (Lock)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, Cmd);
                    WriteData(buffer);
                }
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        bool SendCommand(eCommand pC, string pParam = "{}", int pWait = 0, string pMsgType = null)
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
                lock (Lock)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, Cmd);
                    List<byte> message = Encoding.UTF8.GetBytes(Cmd).ToList();
                    data.AddRange(message);
                    data.Add(delimiter);
                    var buffer = data.ToArray();
                    // Скидаємо результати попередньої операції, щоб при тайм-ауті не повернути старі дані
                    Res = null;
                    ResBatchTotals = null;
                    ResAnswer = null;
                    ResResponseCode = null;
                    LastStatMsgCodeValue = 0;
                    WaitCommand = pC;
                    WaitMsgType = pMsgType;
                    IsRes = false;
                    if (pC == eCommand.Purchase || pC == eCommand.Refund || pC == eCommand.Cashback)
                        IsResLongOperation = false;

                    WriteData(buffer);
                }
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
            bool Is11 = false;
            pTime = pTime / 200;
            while (pTime > 0 && IsResLongOperation == false)
            {
                Thread.Sleep(500);
                SendServiceMessage("getLastStatMsgCode");
                pTime--;
                // Коригування можливе тільки в межах продажу (Purchase, Cashback)
                if (!Is11 && LastStatMsgCodeValue == 11 && (WaitCommand == eCommand.Purchase || WaitCommand == eCommand.Cashback))
                {
                    Is11 = true;
                    CorrectTransaction();
                }
            }
            return IsResLongOperation ?? true;
        }

        /// <summary>
        /// Дані з порту можуть приходити частинами - накопичуємо в буфері і розбираємо тільки повні повідомлення (до 0x00)
        /// </summary>
        private bool OnDataReceived(byte[] data)
        {
            if (data == null || data.Length == 0) return false;
            List<string> Messages = new List<string>();
            lock (RcvBuffer)
            {
                RcvBuffer.AddRange(data);
                int Pos;
                while ((Pos = RcvBuffer.IndexOf(0x00)) >= 0)
                {
                    if (Pos > 0)
                        Messages.Add(Encoding.UTF8.GetString(RcvBuffer.GetRange(0, Pos).ToArray()));
                    RcvBuffer.RemoveRange(0, Pos + 1);
                }
                if (RcvBuffer.Count > MaxRcvBuffer)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Receive buffer overflow ({RcvBuffer.Count} bytes) without delimiter. Clear.");
                    RcvBuffer.Clear();
                }
            }
            foreach (var el in Messages)
            {
                StrRes = el;
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, StrRes);
                ParserRes(el);
            }
            return Messages.Count > 0;
        }

        eCommand ParserRes(string pStr)
        {
            eCommand Command = eCommand.NotDefine;
            try
            {
                Answer<object> res = JsonSerializer.Deserialize<Answer<object>>(pStr);
                Enum.TryParse<eCommand>(res.method?.Trim(), out Command);
                // Відповідь вважаємо отриманою тільки якщо вона на очікувану команду
                bool IsWaited = Command != eCommand.NotDefine && Command == WaitCommand;
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
                        string MsgType = ResServiceMessage.@params?.msgType?.Trim();
                        if ("getLastStatMsgCode".Equals(MsgType) && !string.IsNullOrEmpty(ResServiceMessage.@params.LastStatMsgCode) && byte.TryParse(ResServiceMessage.@params.LastStatMsgCode, out LastStatMsgCode))
                        {
                            LastStatMsgCodeValue = LastStatMsgCode;
                            InvokeLastStatusMsg(LastStatMsgCode);
                        }
                        if ("getDiscountName".Equals(MsgType))
                        {
                            DiscountPan = ResServiceMessage.@params.pan;
                            IsDiscountName = true;
                        }
                        if (Command == eCommand.ServiceMessage)
                            IsWaited &= string.Equals(MsgType, WaitMsgType, StringComparison.OrdinalIgnoreCase);
                        if (IsWaited)
                            ResAnswer = JsonSerializer.Deserialize<Answer<dynamic>>(pStr);
                        break;
                    case eCommand.Refund:
                    case eCommand.Purchase:
                    case eCommand.Cashback:
                        if (!IsWaited) break; //Запізніла відповідь на попередню операцію
                        var ResJson = JsonSerializer.Deserialize<Answer<PAPurchase>>(pStr);
                        var par = ResJson.@params ?? new PAPurchase();
                        ResResponseCode = par.responseCode;
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
                            NumberReceipt = par.invoiceNumber?.ToLong() ?? 0, //Може перевищувати int32
                            NumberSlip = par?.approvalCode,
                            NumberTerminal = par.terminalId,
                            Receipt = PosReceiptText.ToLines(par.receipt, ReceiptWidth),
                            SumExt = par.amountCash?.ToDecimal() ?? 0,
                            PosAddAmount = par.discount?.ToDecimal() ?? 0,
                            //Номера транзакції в пакеті (TrnBatchNum як у BPOS) протокол не передає - беремо номер чека
                            TransactionId = par.invoiceNumber,
                            MerchantID = par.merchant,
                            TransactionStatus = ResJson.errorDescription,
                            DateCreate = DateTime.TryParseExact($"{par.date} {par.time}", "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var Dt) ? Dt : DateTime.Now
                        };
                        IsResLongOperation = true;
                        break;
                    case eCommand.Audit:
                    case eCommand.Verify:
                    case eCommand.PrintBatchJournal:
                    case eCommand.PrintReceiptNum:
                        if (!IsWaited) break;
                        var ResJsonR = JsonSerializer.Deserialize<Answer<PAReceipt>>(pStr);
                        ResBatchTotals = new BatchTotals() { Receipt = PosReceiptText.ToLines(ResJsonR.@params?.receipt, ReceiptWidth) };
                        break;
                }
                if (IsWaited)
                    IsRes = true;
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
        public string discountName { get; set; }
        public string pan { get; set; }
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
