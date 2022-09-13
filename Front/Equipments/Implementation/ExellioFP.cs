//using Exellio;
//using ExellioFP;
using Front.Equipments.Implementation;
using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelMID;
using ModelMID.DB;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace Front.Equipments
{
    public class ExellioFP : Rro
    {
        //FiscalPrinterClass 
           dynamic FP=null;
        
        public ExellioFP(Equipment pEquipment, IConfiguration pConfiguration, ILoggerFactory pLoggerFactory = null) : base(pEquipment, pConfiguration,eModelEquipment.ExellioFP, pLoggerFactory)
        {
            State = eStateEquipment.Init;
            try
            {
                SerialPort = Configuration["Devices:ExellioFP:SerialPort"];
                BaudRate = Convert.ToInt32(Configuration["Devices:ExellioFP:BaudRate"]);
                IP = Configuration["Devices:ExellioFP:IP"];
                IpPort = Convert.ToInt32(Configuration["Devices:ExellioFP:IpPort"]);
                //TMP!!! 
                //FP = new FiscalPrinterClass();                
                State = eStateEquipment.On;
            }
            catch(Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                State = eStateEquipment.Error;
            }
        }

        ///відкрити порт 
        private bool FpOpenPort(IdReceipt pReceipt = null)
        {
            if (FP == null)            
                return CheckResult();
            
            SetStatus(eStatusRRO.TryOpenPort);
            if (!string.IsNullOrEmpty(SerialPort) && BaudRate > 0)
                FP.OpenPort(SerialPort, BaudRate);
            else if (!string.IsNullOrEmpty(IP) && IpPort > 0)
                FP.ConnectLan(IP, IpPort.ToString()); //"10.1.5.221", "9100"
            else
            {
                CodeError = -99999;
                StrError = "Відсутні налаштування з'єднааня з Фіскальним апаратом";
            }
            if (!CheckResult())
            {
                SetStatus(eStatusRRO.Error, StrError, CodeError);
                FP.ClosePort();
                return false;
            }
            SetStatus(eStatusRRO.OpenPort);
            return true;
        }

        ///закрити порт 
        private bool FpClosePort()
        {
            SetStatus(eStatusRRO.ClosePort);
            FP?.ClosePort();           
            return CheckResult();
        }

        private bool CheckResult()
        {
            if (FP == null)
            {
                State = eStateEquipment.Error;
                StrError = "Відсутній екземпляр об'єкта Exellio.FiscalPrinterClass()";
                CodeError = -999999;
                return false;
            }

            CodeError = FP.LastError;
            StrError = FP.LastErrorText;
            if (CodeError != 0)
            {
                SetStatus(eStatusRRO.Error, StrError, CodeError);
                State = eStateEquipment.Error;
            }
            return CodeError == 0;
        }


        public override LogRRO PrintCopyReceipt(int pNCopy = 1)
        {
            if (FpOpenPort())
            {
                FP.MakeReceiptCopy(pNCopy);
                CheckResult();
                FpClosePort();
            }
            return new LogRRO(null) { CodeError = CodeError, Error = StrError, SUM = 0, TypeRRO = "ExellioFP", TypeOperation = eTypeOperation.CopyReceipt };
        }

        override public async Task<LogRRO> PrintZAsync(IdReceipt pIdR)
        {
            return await Task.Run(() =>
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
            });
        }

        override public async Task<LogRRO> PrintXAsync(IdReceipt pIdR)
        {
            return await Task.Run(() =>
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
            });
        }

        /// <summary>
        /// Внесення/Винесення коштів коштів.
        /// </summary>
        /// <param name="pSum"> pSum>0 - внесення</param>
        /// <returns></returns>
        override public async Task<LogRRO> MoveMoneyAsync(decimal pSum, IdReceipt pIdR = null)
        {
            return await Task.Run(() =>
            {
                if (FpOpenPort())
                {
                    FP.InOut(Convert.ToDouble(pSum));
                    if (!CheckResult())
                        FP.OpenDrawer();
                    FpClosePort();
                }
                return new LogRRO(pIdR) { CodeError = CodeError, Error = StrError, SUM = pSum, TypeRRO = "ExellioFP", TypeOperation = pSum > 0 ? eTypeOperation.MoneyIn : eTypeOperation.MoneyOut };
            });
        }
        private void PrintFiscalComents(Receipt  pR)
        {
            FP.PrintLine(1);
            FP.PrintFiscalText($"Номер документа: {pR.NumberReceipt1C}");
            if (pR.Client!=null) // якщо клієнт відсканував карточку
            {
                FP.PrintFiscalText($"Клієнт: ");
                FP.PrintFiscalText(pR.Client.NameClient); //з нової строчки, щоб все ім'я влізло
                FP.PrintFiscalText($"Бонуси:{pR.Client.SumBonus}");
                FP.PrintFiscalText($"Скарбничка:{pR.Client.SumMoneyBonus}");
            }
            FP.PrintLine(1);
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        override public async Task<LogRRO> PrintReceiptAsync(Receipt pR)
        {
            return await Task.Run(() =>
            {
                LogRRO Res = new LogRRO(pR)
                {
                    SUM = pR.SumFiscal,
                    TypeRRO = "ExellioFP",
                    TypeOperation = (pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund)
                };

                if (FpOpenPort())
                {
                    SetStatus(eStatusRRO.StartPrintReceipt);
                    if (FP.IsFiscalOpen)
                        FP.CancelReceipt();

                    //потрібна перевірка фіскальний чи нефіскальний чек відкривається
                    if (pR.TypeReceipt == eTypeReceipt.Sale)
                        FP.OpenFiscalReceipt(1, OperatorPass, 1);
                    else
                        FP.OpenReturnReceipt(1, OperatorPass, 1);
                    
                    //if (true) //якась перевірка чи друкувати коментарі чи ні)
                        PrintFiscalComents(pR);                    
                    
                    if (CheckResult())
                    {
                        //потрібно прочитати номер фіскального чека GetLastReceiptNum() властивість s1
                        foreach (var el in pR.Wares)
                        {
                            SetStatus(eStatusRRO.AddWares, el.NameWares);
                            var TaxGroup = Global.GetTaxGroup(el.TypeVat, (int)el.TypeWares);
                            int TG1 = 0, TG2 = 0;
                            int.TryParse(TaxGroup.Substring(0, 1), out TG1);
                            if (TaxGroup.Length > 1)
                                int.TryParse(TaxGroup.Substring(1, 1), out TG2);
                            var Name = (el.IsUseCodeUKTZED && !string.IsNullOrEmpty(el.CodeUKTZED) ? el.CodeUKTZED.Substring(0, 10) + "#" : "") + el.NameWares;
                            if (!String.IsNullOrEmpty(el.ExciseStamp))
                                FP.ExciseStamp = el.ExciseStamp;
                            var temp = Convert.ToInt32(Global.GetTaxGroup(el.TypeVat, (int)el.TypeWares));
                            //el.SumDiscount - завжди 0 тому не нараховує знижку
                            FP.Sale(el.CodeWares, Name,temp, 1, Convert.ToDouble(el.PriceEKKA), Convert.ToDouble(el.Quantity), 0, Convert.ToDouble(-el.DiscountEKKA), true, OperatorPass);
                            //Convert.ToInt32((el.CodeUnit == Global.WeightCodeUnit ? 1000 : 1) * el.Quantity), Convert.ToInt32(el.Price * 100), el.CodeUnit == Global.WeightCodeUnit ? 1 : 0, TG1, TG2, el.CodeWares, (el.DiscountEKKA > 0 ? 0 : -1), null, Convert.ToInt32(el.DiscountEKKA), null) != 1))

                            if (!CheckResult())
                                break;
                        }

                        if (CodeError == 0)
                        {
                            FP.SubTotal(0, 0); //як прочитати готівка / безготівка

                            pR.SumFiscal = Convert.ToDecimal(FP.s2);

                            if (pR.Payment != null && pR.Payment.Count() > 0)
                            {
                                foreach (var el in pR.Payment)
                                {//!!!TMP Спитати валіка.                                   
                                    FP.PayTerminalData = $"{el.NumberSlip},{0},{el.NumberTerminal},{0},{el.TypePay},{el.NumberCard},{el.CodeAuthorization},{0}";
                                    SetStatus(eStatusRRO.AddWares, FP.PayTerminalData);
                                    
                                    FP.Total("", el.TypePay == eTypePay.Cash ? 1 : 4, Convert.ToDouble(el.SumPay));
                                    
                                    //працює тільки якщо оплата по картці
                                    //FP.TotalPT("", el.TypePay == eTypePay.Cash ? 1 : 4, Convert.ToDouble(el.SumPay), $"{el.NumberSlip},{0},{el.NumberTerminal},{0},{el.TypePay},{el.NumberCard},{el.CodeAuthorization},{0}");

                                    if (!CheckResult())
                                        break;
                                }
                            }
                            if (CodeError == 0)
                            {
                                FP.CloseFiscalReceipt();
                                if (CheckResult())
                                    pR.NumberReceipt = pR.TypeReceipt == eTypeReceipt.Sale ? FP.s2 : FP.s3;
                            }
                        }
                        Res.CodeError = CodeError;
                        Res.Error = StrError;
                        if (CodeError != 0)
                        {
                            FP.CancelReceipt();
                        }                       
                    }
                    FpClosePort();
                }
                return Res;
            });
        }

        override public bool PutToDisplay(string pText)
        {
            FP.DisplayFreeText(pText);
            return true;
        }

        override public StatusEquipment TestDevice()
        {           
            if(FP!=null)
            //!!!TMP Треба розібратись про реальну поведінку з помилками.
            try
            {
                State = eStateEquipment.Init;
                if (FpOpenPort())
                {
                    FP.OpenNonfiscalReceipt();
                    FP.PrintNonfiscalText("Test of the Equipment: Ok");
                    FP.PrintNonfiscalText("Тест пристрою: Ok");
                    FP.CloseNonfiscalReceipt();

                    FP.GetErrorDetails("CLEAR");
                    FP.CheckConnect(SerialPort, BaudRate);
                    FP.GetErrorDetails("Cmd");

                    StrError = $"FPModel={FP.FPModel}{Environment.NewLine}Version{FP.Version}{Environment.NewLine}LibFileName={FP.LibFileName}{Environment.NewLine}" +
                        $"IsPaperOut={FP.IsPaperOut}{Environment.NewLine} IsFiscalised={FP.IsFiscalised}{Environment.NewLine} LogFileDir=>{FP.LogFileDir}{Environment.NewLine}" +
                        $"IsFiscalised={FP.IsFiscalised}{Environment.NewLine}IsFiscalOpen={FP.IsFiscalOpen}" +
                        $"ErrCode=>{FP.s2} LastErrorText=>{FP.LastErrorText}";

                    FpClosePort();
                    State = eStateEquipment.On;
                }
            }catch(Exception e)
            {
                State = eStateEquipment.Error;
                StrError = e.Message;
            }
            else
            {
                State = eStateEquipment.Error;
                StrError = "Відсутній екземпляр об'єкта Exellio";
            }
            return new StatusEquipment(Model,State, StrError + Environment.NewLine + InfoConnect);
        }
        public override string GetDeviceInfo()
        {            
            return $"pModelEquipment={Model} State={State} {InfoConnect}";
        }
    }
}
