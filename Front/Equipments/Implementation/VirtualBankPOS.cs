using Front.Equipments.Ingenico;
using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using ModelMID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Front.Equipments.Implementation
{
    public class VirtualBankPOS:BankTerminal
    {
        int AuthCode = 123456000;
        long NumberReceipt = 0;
        int TransactionCode = 7700000;
        decimal Sum =0m, SumRefund = 0m;
        uint Count=0, CountRefund = 0;       
      
        public VirtualBankPOS(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) : base(pEquipment, pConfiguration, eModelEquipment.VirtualBankPOS, pLoggerFactory)
        {
            State = eStateEquipment.On;
            Random rnd = new Random();
            NumberReceipt = rnd.Next(1, 100000);
            AuthCode+= rnd.Next(1, 1000000);
            TransactionCode += rnd.Next(1, 1000000);
            ActionStatus = pActionStatus;
        }
        Payment GetPaymentResultModel(decimal pAmount)
        {
            return new Payment()
            {
                TypePay= eTypePay.Card,
                Bank = "Приват",
                CardHolder = "VISA",
                NumberReceipt = NumberReceipt++,
                IssuerName = "NoName",
                DateCreate = DateTime.Now,
                PosAddAmount = 0,
                PosPaid = pAmount,
                SumPay = pAmount,
                SumExt =0,
                NumberCard ="******0123",                
                CodeAuthorization = $"{AuthCode++}",
                NumberTerminal = "SML_Local",                
                NumberSlip= $"{TransactionCode++}",
                IsSuccess=true,
                Receipt = new List<string>() { "Тестовий Чек", $"Сума: {pAmount}",$"CodeAuthorization{AuthCode}","Тестова Оплата" }
                
            };
        }

        public override Payment Purchase(decimal pAmount,decimal pCash)
        {
            int Interval = 1500;
            SetStatus(eStatusPos.WaitingForCard);
            Thread.Sleep(Interval);
            SetStatus(eStatusPos.PinInputWaitKey);
            Thread.Sleep(Interval);
            SetStatus(eStatusPos.TransactionIsAlreadyComplete);
            Thread.Sleep(Interval);
            return GetPaymentResultModel(pAmount);
        }

        public override Payment Refund(decimal pAmount, string pRRN)
        {
            return GetPaymentResultModel(pAmount);
        }

        BatchTotals GetBatchTotals()
        {
            return new BatchTotals() { CencelledCount = 0, CencelledSum = 0, CreditCount = Count, CreditSum = (uint)Sum, DebitCount = CountRefund, DebitSum = (uint)SumRefund };
        }

        public override BatchTotals PrintZ()
        {
            return GetBatchTotals();
        }

        public override BatchTotals PrintX()
        {
            return GetBatchTotals();
        }

        public override StatusEquipment TestDevice()
        {
            return new StatusEquipment(Model,State,"Ok");
        }
        public override string GetDeviceInfo()
        {
            return $"pModelEquipment={Model} State={State} Port={SerialPort} BaudRate={BaudRate}{Environment.NewLine}";
        }
    }
}
