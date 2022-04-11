using Exellio;
using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Equipments
{
    public class ExellioFP : Rro
    {
        private FiscalPrinterClass FP = new FiscalPrinterClass();
        //public Exelio(string pSerialPortName, int pBaudRate, Action<string, string> pLogger) : base(pSerialPortName, pBaudRate,pLogger) { }
        public ExellioFP(IConfiguration pConfiguration, Action<string, string> pLogger = null) : base(pConfiguration) 
        {           
            Port = Configuration["Devices:Exellio:Port"];
            BaudRate = Convert.ToInt32( Configuration["Devices:Exellio:Port"]);            
            FP.OpenPort(Port, BaudRate);
        }

        ///відкрити порт 
        private bool FpOpenPort()
        {
            //FP.OpenPort(Port, BaudRate);
            FP.ConnectLan("10.1.5.221", "9100");
            CheckResult();
            if (CodeError != 0)
            {
                //потрібно зафіксувати FP.LastErrorText

                FP.ClosePort();
                return false;
            }
            return true;            
        }

        ///закрити порт 
        private LogRRO FpClosePort()
        {
            FP.ClosePort();
            //if (FP.LastError != 0)
            //{
                //потрібно зафіксувати FP.LastErrorText
                //CheckTheResult();
            //}

            return null;
        }

        private void CheckResult()
        {
            CodeError = FP.LastError;
            StrError = FP.LastErrorText;
        }


        public override LogRRO PrintCopyReceipt(int pNCopy = 1)
        {
            if (FpOpenPort()) 
            { 
                FP.MakeReceiptCopy(pNCopy);

                CheckResult();
                FpClosePort();
            }

            return null;
        }


        override public async Task<LogRRO> PrintZAsync(IdReceipt pIdR)
        {
            if (FpOpenPort())
            {
                if (FP.IsFiscalOpen)
                    FP.CancelReceipt();

                FP.ZReportWC(OperatorPass);

                CheckResult();
                FpClosePort();

            }

            return new LogRRO(pIdR) { CodeError = CodeError, Error = StrError, SUM = 0, TypeRRO = "ExellioFP", TypeOperation = eTypeOperation.ZReport };
        }

        override public async Task<LogRRO> PrintXAsync(IdReceipt pIdR)
        {
            if (FpOpenPort())
            {
                if (FP.IsFiscalOpen)                    
                    FP.CancelReceipt();

                FP.XReport(OperatorPass);

                CheckResult();
                FpClosePort();

            }

            return new LogRRO(pIdR) { CodeError = CodeError, Error = StrError, SUM = 0, TypeRRO = "ExellioFP", TypeOperation = eTypeOperation.XReport };
        }

        /// <summary>
        /// Внесення/Винесення коштів коштів.
        /// </summary>
        /// <param name="pSum"> pSum>0 - внесення</param>
        /// <returns></returns>
        override public async Task<LogRRO> MoveMoneyAsync(decimal pSum, IdReceipt pIdR = null)
        {
            if (FpOpenPort())
            { 
                FP.InOut(Convert.ToDouble( pSum));
                CheckResult();
                FP.OpenDrawer();
                FpClosePort();
            }


            return new LogRRO(pIdR) { CodeError = CodeError, Error = StrError, SUM = pSum, TypeRRO = "Maria304", TypeOperation = pSum > 0 ? eTypeOperation.MoneyIn : eTypeOperation.MoneyOut };
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        override public async Task<LogRRO> PrintReceiptAsync(Receipt pR)
        {

            if (FpOpenPort()) 
            {
                if (FP.IsFiscalOpen)
                    FP.CancelReceipt();

                //потрібна перевірка фіскальний чи нефіскальний чек відкривається

                if (pR.TypeReceipt == eTypeReceipt.Sale)
                    FP.OpenFiscalReceipt(1, OperatorPass, 1);
                else
                    FP.OpenReturnReceipt(1, OperatorPass, 1);

                CheckResult();

                //потрібно прочитати номер фіскального чека GetLastReceiptNum() властивість s1

                foreach (var el in pR.Wares)
                {
                    var TaxGroup = Global.GetTaxGroup(el.TypeVat, el.TypeWares);
                    int TG1 = 0, TG2 = 0;
                    int.TryParse(TaxGroup.Substring(0, 1), out TG1);
                    if (TaxGroup.Length > 1)
                        int.TryParse(TaxGroup.Substring(1, 1), out TG2);
                    var Name = (el.IsUseCodeUKTZED && !string.IsNullOrEmpty(el.CodeUKTZED) ? el.CodeUKTZED.Substring(0, 10) + "#" : "") + el.NameWares;
                    if (!String.IsNullOrEmpty(el.ExciseStamp))
                        FP.ExciseStamp = el.ExciseStamp;

                    FP.Sale(el.CodeWares, Name, Convert.ToInt32(Global.GetTaxGroup(el.TypeVat, el.TypeWares)), 1, Convert.ToDouble(el.Price), Convert.ToDouble(el.Quantity), 0, Convert.ToDouble(el.SumDiscount), true, OperatorPass);
                    //Convert.ToInt32((el.CodeUnit == Global.WeightCodeUnit ? 1000 : 1) * el.Quantity), Convert.ToInt32(el.Price * 100), el.CodeUnit == Global.WeightCodeUnit ? 1 : 0, TG1, TG2, el.CodeWares, (el.DiscountEKKA > 0 ? 0 : -1), null, Convert.ToInt32(el.DiscountEKKA), null) != 1))

                    CheckResult();
                    if (CodeError != 0)
                    {
                        return new LogRRO(pR)
                        {
                            CodeError = CodeError,
                            Error = StrError,
                            SUM = pR.SumFiscal,
                            TypeRRO = "ExellioFP",
                            TypeOperation = (pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund),
                            JSON = ""
                        };
                    }
                }

                FP.SubTotal(0, 0); //як прочитати готівка / безготівка

                pR.SumFiscal = Convert.ToDecimal(FP.s2);

                if (pR.Payment != null && pR.Payment.Count() > 0)
                {
                    foreach (var el in pR.Payment)
                    {//!!!TMP Спитати валіка.
                        FP.PayTerminalData = $"{el.NumberSlip},{0},{el.NumberTerminal},{el.TypePay},{el.NumberCard},{el.CodeAuthorization},{0}";
                        FP.TotalPT("", el.TypePay == eTypePay.Cash ? 1 : 4, Convert.ToDouble(el.SumPay), el.TransactionId);
                    }
                }
                CheckResult();
                if (CodeError != 0)
                    FP.CancelReceipt();

                FP.CloseFiscalReceipt();

                CheckResult();
                if (CodeError != 0)
                    FP.CancelReceipt();

                pR.NumberReceipt = pR.TypeReceipt == eTypeReceipt.Sale ? FP.s2 : FP.s3;

                return new LogRRO(pR)
                {
                    CodeError = CodeError,
                    Error = StrError,
                    SUM = pR.SumFiscal,
                    TypeRRO = "ExellioFP",
                    TypeOperation = (pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund),
                    JSON = ""
                };
            }
            return new LogRRO(pR)
            {
                CodeError = CodeError,
                Error = StrError,
                SUM = pR.SumFiscal,
                TypeRRO = "ExellioFP",
                TypeOperation = (pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund),
                JSON = ""
            };
        }

        override public bool PutToDisplay(string pText)
        {           
                FP.DisplayFreeText(pText);
            return true;
        }

        override public  eStateEquipment TestDevice() 
        {

            //!!!TMP Треба розібратись про реальну поведінку з помилками.
            eStateEquipment Res = eStateEquipment.Ok;
            if (FpOpenPort())
                { 

                FP.OpenNonfiscalReceipt();
                FP.PrintNonfiscalText("Test of the Equipment: Ok");
                FP.PrintNonfiscalText("Тест пристрою: Ok");
                FP.CloseNonfiscalReceipt();

                FP.GetErrorDetails("CLEAR");
                FP.CheckConnect(Port, BaudRate);
                FP.GetErrorDetails("Cmd");

                StrError = $"FPModel={FP.FPModel}{Environment.NewLine}Version{FP.Version}{Environment.NewLine}LibFileName={FP.LibFileName}{Environment.NewLine}" +
                    $"IsPaperOut={FP.IsPaperOut}{Environment.NewLine} IsFiscalised={FP.IsFiscalised}{Environment.NewLine} LogFileDir=>{FP.LogFileDir}{Environment.NewLine}" +
                    $"IsFiscalised={FP.IsFiscalised}{Environment.NewLine}IsFiscalOpen={FP.IsFiscalOpen}" +
                    $"ErrCode=>{FP.s2} LastErrorText=>{FP.LastErrorText}";

                FpClosePort();

            }

            return Res;
        }

    }
}
