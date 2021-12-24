using Front.Equipments.Ingenico;
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
        Action<IPosStatus> ActionStatus;

        public VirtualBankPOS(IConfiguration pConfiguration, Action<string, string> pLogger = null, Action<IPosStatus> pActionStatus = null) : base(pConfiguration, pLogger,eModelEquipment.VirtualBankPOS)
        {
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
                NumberSlip= $"{TransactionCode++}"
            };
        }

        public override Payment Purchase(decimal pAmount)
        {
            ActionStatus?.Invoke(new PosStatus() {Status=ePosStatus.WaitingForCard,MsgCode=(byte)(ePosStatus.WaitingForCard), MsgDescription= ePosStatus.WaitingForCard.ToString() });
            Thread.Sleep(2000);
            ActionStatus?.Invoke(new PosStatus() { Status = ePosStatus.PinInputWaitKey, MsgCode = (byte)(ePosStatus.PinInputWaitKey), MsgDescription = ePosStatus.PinInputWaitKey.ToString() });
            Thread.Sleep(3000);
            ActionStatus?.Invoke(new PosStatus() { Status = ePosStatus.TransactionIsAlreadyComplete, MsgCode = (byte)(ePosStatus.TransactionIsAlreadyComplete), MsgDescription = ePosStatus.TransactionIsAlreadyComplete.ToString() });
            Thread.Sleep(1000);
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

        public override eStateEquipment TestDevice()
        {
            return eStateEquipment.Ok;
        }
    }
}
