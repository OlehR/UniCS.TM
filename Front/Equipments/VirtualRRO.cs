using Front.Equipments.Implementation.ModelVchasno;
using Front.Equipments.Virtual;
using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Front.Equipments.Implementation.FP700_Model;
using Front.Equipments.Utils;
using SharedLib;
using Microsoft.Extensions.FileSystemGlobbing;
using static System.Net.Mime.MediaTypeNames;

namespace Front.Equipments.Implementation
{
    public class VirtualRRO : Rro
    {
        Printer Printer;
        BL Bl = BL.GetBL;
        public EquipmentFront EF;//= new EquipmentFront(null, null, null);
        string HeadReceipt = "ТОВ";
        string FiscalNumber = "657513548";
        int MaxCountCharacters = 34;//23;//34;
        List<string> TextReport = new();
        WDB_SQLite db = WDB_SQLite.GetInstance;


        public VirtualRRO(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null, Printer pR = null, EquipmentFront eF = null) : this(pEquipment, pConfiguration, pLoggerFactory, pActionStatus)
        {
            Printer = pR;
            EF = eF;
        }

        public VirtualRRO(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) :
                       base(pEquipment, pConfiguration, eModelEquipment.VirtualRRO, pLoggerFactory, pActionStatus)
        {
            HeadReceipt = Configuration[$"{KeyPrefix}Head"];
            FiscalNumber = Configuration[$"{KeyPrefix}FiscalNumber"];
            State = eStateEquipment.On;
        }

        public override LogRRO PrintCopyReceipt(int parNCopy = 1)
        {
            var logRRO = Bl.GetLogRRO(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod() });
            if (Printer != null)
                Printer.Print(logRRO.LastOrDefault().TextReceipt.Split(Environment.NewLine).ToList());
            else
                EF.PrintNoFiscalReceipt(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = Global.IdWorkPlace }, logRRO.LastOrDefault().TextReceipt.Split(Environment.NewLine).ToList());
            return new LogRRO(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod() })
            { TypeOperation = eTypeOperation.CopyReceipt, TextReceipt = logRRO.LastOrDefault().TextReceipt, TypeRRO = "VirtualRRO", JSON = logRRO.LastOrDefault().ToJSON(), FiscalNumber = $"{FiscalNumber}_{eTypeOperation.CopyReceipt}", TypePay = TypePay };
        }

        override public LogRRO PrintZ(IdReceipt pIdR)
        {
            var logRRO = Bl.GetLogRRO(pIdR);
            return PrintReport(pIdR, eTypeOperation.ZReport, logRRO);
        }

        override public LogRRO PrintX(IdReceipt pIdR)
        {
            var logRRO = Bl.GetLogRRO(pIdR);
            return PrintReport(pIdR, eTypeOperation.XReport, logRRO);
        }

        private LogRRO PrintReport(IdReceipt pIdR, eTypeOperation typeOperation, IEnumerable<LogRRO> logRRO, DateTime pBegin = new DateTime(), DateTime pEnd = new DateTime())
        {
            TextReport.Clear();


            //кількість продажних чеків
            int CountSaleReceipt = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.Sale && x.TypePay == eTypePay.Cash).Count();
            //Загальна сума продажів
            decimal TotalSaleSum = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.Sale && x.TypePay == eTypePay.Cash).Select(x => x.SUM).Sum();
            //Кількість чеків на повернення
            int CountRefunddReceipt = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.Refund && x.TypePay == eTypePay.Cash).Count();
            //Сума чеків на повернення
            decimal TotalRefundSum = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.Refund && x.TypePay == eTypePay.Cash).Select(x => x.SUM).Sum();
            //кількість чеків внесення
            int CountMoneyInReceipt = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.MoneyIn && x.TypePay == eTypePay.Cash).Count();
            //Сума внесень
            decimal TotalMoneyInSum = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.MoneyIn && x.TypePay == eTypePay.Cash).Select(x => x.SUM).Sum();
            //Кількість чеків вилучення
            int CountMoneyOutReceipt = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.MoneyOut && x.TypePay == eTypePay.Cash).Count();
            //сума вилучень
            decimal TotalMoneyOutSum = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.MoneyOut && x.TypePay == eTypePay.Cash).Select(x => x.SUM).Sum();
            //Готівки в касі
            decimal TotalSum = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation != eTypeOperation.Refund && x.TypePay == eTypePay.Cash).Select(x => x.SUM).Sum() - TotalRefundSum;


            TextReport.Add(PrintCenter(HeadReceipt));
            TextReport.Add(PrintCenter(typeOperation.GetDescription()));
            if (typeOperation == eTypeOperation.PeriodZReport)
                TextReport.Add(PrintCenter($"{pBegin.ToString("dd/MM/yyyy")}-{pEnd.ToString("dd/MM/yyyy")}"));
            TextReport.Add(PrintCenter("Продажі"));
            TextReport.Add(PrintTwoColums("Чеків", CountSaleReceipt.ToString()));
            TextReport.Add(PrintTwoColums("Загальна сума:", TotalSaleSum.ToString("0.00")));
            TextReport.Add(PrintCenter("----------------------"));
            TextReport.Add(PrintCenter("Повернення"));
            TextReport.Add(PrintTwoColums("Чеків", CountRefunddReceipt.ToString()));
            TextReport.Add(PrintTwoColums("Загальна сума:", TotalRefundSum.ToString("0.00")));
            TextReport.Add(PrintCenter("----------------------"));
            TextReport.Add(PrintCenter("Службові внесення/вилучення"));
            if (CountMoneyInReceipt > 0)
            {
                TextReport.Add(PrintTwoColums("Кількість внесень:", CountMoneyInReceipt.ToString()));
                TextReport.Add(PrintTwoColums("Сума внесень:", TotalMoneyInSum.ToString("0.00")));
            }
            if (CountMoneyOutReceipt > 0)
            {
                TextReport.Add(PrintTwoColums("Кількість вилучень:", CountMoneyOutReceipt.ToString()));
                TextReport.Add(PrintTwoColums("Сума вилучень:", TotalMoneyOutSum.ToString("0.00")));
            }
            if (typeOperation != eTypeOperation.PeriodZReport)
                TextReport.Add(PrintTwoColums("Готівки в касі:", TotalSum.ToString("0.00")));
            TextReport.Add(PrintCenter("----------------------"));
            TextReport.Add(DateTime.Now.ToString());
            TextReport.Add($"ФН РРО {FiscalNumber}");
            if (Global.GetCodePeriod() != pIdR.CodePeriod)
                TextReport.Add(PrintCenter($"Копія звіту за {pIdR.CodePeriod}")); ;

            if (Printer != null)
                Printer.Print(TextReport);
            else
                EF.PrintNoFiscalReceipt(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = Global.IdWorkPlace }, TextReport);


            return new LogRRO(pIdR) { TypeOperation = eTypeOperation.XReport, FiscalNumber = $"{FiscalNumber}_{typeOperation}", JSON = pIdR.ToJSON(), TextReceipt = string.Join(Environment.NewLine, TextReport), TypeRRO = "VirtualRRO", TypePay = TypePay };
        }
        private string PrintCenter(string text)
        {
            if (text.Length <= MaxCountCharacters)
            {
                // Якщо текст поміщається в один рядок, вирівнюємо його по центру і повертаємо.
                return AlignCenter(text);
            }

            // Розділити текст на слова
            string[] words = text.Split(' ');

            string formattedText = "";
            string currentLine = "";

            foreach (string word in words)
            {
                if ((currentLine + word).Length <= MaxCountCharacters)
                {
                    // Додаємо слово до поточного рядка
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        currentLine += " ";
                    }
                    currentLine += word;
                }
                else
                {
                    TextReport.Add(AlignCenter(currentLine));
                    currentLine = word;
                }
            }
            return AlignCenter(currentLine);
        }

        private string AlignCenter(string text)
        {
            if (text.Length >= MaxCountCharacters)
            {
                // Якщо текст вже більше або дорівнює максимальній довжині,
                // то повертаємо його без змін.
                return text;
            }

            int totalSpaces = MaxCountCharacters - text.Length;
            int leftSpaces = totalSpaces / 2;
            int rightSpaces = totalSpaces - leftSpaces;

            // Форматуємо текст з додаванням пробілів зліва і справа для вирівнювання по центру.
            string formattedText = new string(' ', leftSpaces) + text;

            return formattedText;
        }

        private string PrintTwoColums(string str1, string str2)
        {
            if (str1.Length + str2.Length >= MaxCountCharacters)
            {
                // Розділити текст на слова
                string[] words = str1.Split(' ');

                string formattedText = "";
                string currentLine = "";

                foreach (string word in words)
                {
                    if ((currentLine + word).Length <= this.MaxCountCharacters)
                    {
                        // Додаємо слово до поточного рядка
                        if (!string.IsNullOrEmpty(currentLine))
                        {
                            currentLine += " ";
                        }
                        currentLine += word;
                    }
                    else
                    {
                        //formattedText += currentLine + Environment.NewLine;
                        TextReport.Add(currentLine);
                        currentLine = word;
                    }
                }
                int remainingSpaces = MaxCountCharacters - (currentLine.Length + str2.Length);
                string alignedStr1 = currentLine.PadRight(currentLine.Length + remainingSpaces);
                return alignedStr1 + str2;


                //return str1.PadRight(maxLength) + Environment.NewLine + str2.PadLeft(maxLength);
            }
            else
            {
                // Інакше, вирівнюємо str1 по лівому краю, а str2 по правому краю,
                // додаючи відповідну кількість пробілів, і об'єднуємо їх в одному рядку.
                int remainingSpaces = MaxCountCharacters - (str1.Length + str2.Length);
                string alignedStr1 = str1.PadRight(str1.Length + remainingSpaces / 2);
                string alignedStr2 = str2.PadLeft(str2.Length + remainingSpaces / 2);
                return alignedStr1 + alignedStr2;
            }
        }

        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        override public LogRRO MoveMoney(decimal pSum, IdReceipt pIdR = null)
        {
            eTypeOperation typeOperation = pSum > 0 ? eTypeOperation.MoneyIn : eTypeOperation.MoneyOut;
            if (pIdR != null)
            {
                var logRRO = Bl.GetLogRRO(pIdR);
                //Сума чеків на повернення
                decimal TotalRefundSum = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.Refund && x.TypePay == eTypePay.Cash).Select(x => x.SUM).Sum();
                //Готівки в касі
                decimal TotalSum = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation != eTypeOperation.Refund && x.TypePay == eTypePay.Cash).Select(x => x.SUM).Sum() - TotalRefundSum;
                if (typeOperation == eTypeOperation.MoneyOut && TotalSum < Math.Abs(pSum))
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Сума видачі меньша за суму готівки в касі!");
                    throw new Exception(Environment.NewLine + "Сума видачі меньша за суму готівки в касі!" + Environment.NewLine + StrError);
                }
            }



            string strTypeOperation = typeOperation == eTypeOperation.MoneyIn ? "СЛУЖБОВЕ ВНЕСЕННЯ" : "СЛУЖБОВА ВИДАЧА";
            TextReport.Clear();
            TextReport.Add(PrintCenter(HeadReceipt));
            TextReport.Add(PrintCenter("----------------------"));
            TextReport.Add(PrintCenter(strTypeOperation));
            TextReport.Add(PrintTwoColums("ГОТІВКА", pSum.ToString("F2")));
            TextReport.Add(PrintCenter("----------------------"));
            TextReport.Add(DateTime.Now.ToString());
            TextReport.Add($"ФН РРО {FiscalNumber}");
            if (Printer != null)
                Printer.Print(TextReport);
            else
                EF.PrintNoFiscalReceipt(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = Global.IdWorkPlace }, TextReport);

            return new LogRRO(pIdR) { TypeOperation = typeOperation, FiscalNumber = typeOperation.GetDescription(), SUM = pSum, TypeRRO = "VirtualRRO", JSON = pIdR.ToJSON(), TypePay = TypePay, TextReceipt = string.Join(Environment.NewLine, TextReport) };
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        override public LogRRO PrintReceipt(Receipt pR)
        {
            TextReport.Clear();
            GetFiscalInfo(pR, null);

            if (Printer != null)   // якщо є принтер тоді потрібний друк шапки інакше шапку друкує фіскальний апарат
            {
                TextReport.Add(PrintCenter(HeadReceipt));
                TextReport.Add(PrintCenter(pR.TypeReceipt.GetDescription()));
            }

            TextReport.Add(($"Касир {pR.NameCashier}"));
            TextReport.Add(($"Касир {pR.NumberReceipt1C}"));
            if (!string.IsNullOrEmpty(pR.Client?.NameClient))
            {
                TextReport.Add(($"Касир {pR.Client?.NameClient}"));
                TextReport.Add(($"Бонуси: {pR.Client?.SumBonus}"));
                TextReport.Add(($"Скарбничка: {pR.Client?.Wallet}"));
            }
            TextReport.Add(PrintCenter($"------------------------"));
            foreach (var item in pR.Wares)
            {
                string CodeUKTZED = string.Empty;
                if (item.IsUseCodeUKTZED)
                {
                    TextReport.Add($"УКТЗЕД:{item.CodeUKTZED} ШК:{item.BarCode}");
                }
                if (item.ExciseStamp != null)
                {
                    var ExciseStamp = item.ExciseStamp.Split(',');
                    for (int j = 0; j < ExciseStamp.Length; j++)
                    {
                        TextReport.Add($"Акцизна марка: {ExciseStamp[j]}");
                    }
                }

                string Quantity_x_Price = $"{item.Quantity} x {item.PriceEKKA}";
                string Price_VatChar = (item.PriceEKKA * item.Quantity).ToString("F2") + item.VatChar;
                if (item.Quantity == 1)
                {
                    TextReport.Add(PrintTwoColums(item.NameWares, Price_VatChar));
                }
                else
                {
                    TextReport.Add(item.NameWares);
                    TextReport.Add(PrintTwoColums(Quantity_x_Price, Price_VatChar));
                }
                if (item.SumDiscountEKKA > 0)
                {
                    TextReport.Add(PrintTwoColums("Знижка", item.SumDiscountEKKA.ToString("f2")));
                }
            }
            TextReport.Add(PrintCenter($"------------------------"));


            TextReport.Add(PrintTwoColums("Сума", pR.SumCash.ToString("F2")));
            decimal roundFiscal = pR.SumCash - SumReceiptFiscal(pR);
            
            if (roundFiscal != 0)
                TextReport.Add(PrintTwoColums("Заокруглення", roundFiscal.ToString("F2")));
            if (pR.Fiscal?.Taxes?.Count() > 0)
            {
                foreach (var item in (pR.Fiscal.Taxes))
                {
                    TextReport.Add(PrintTwoColums(item.Name, item.Sum.ToString("f2")));
                }
            }
            TextReport.Add(PrintCenter($"------------------------"));


            //інформація про банк
            if (pR.Payment.Count() > 0)
            {
                foreach (var item in pR.Payment)
                {
                    string IDTerminal = "Ідент. еквайра";
                    string EPZ = "ЕПЗ";
                    string CardHolder = "Платіжна система";
                    string RRN = "RRN";
                    string CodeAuthorization = "Код авт.";
                    if (item.TypePay == eTypePay.Card)
                    {
                        TextReport.Add(PrintTwoColums(IDTerminal, item.NumberTerminal));
                        TextReport.Add(PrintTwoColums(EPZ, item.NumberCard));
                        TextReport.Add(PrintTwoColums(CardHolder, item.CardHolder));
                        TextReport.Add(PrintTwoColums(RRN, item.CodeAuthorization));
                        TextReport.Add(PrintTwoColums(CodeAuthorization, item.NumberSlip));
                        //розділювач
                        TextReport.Add(PrintCenter($"------------------------"));
                    }
                }
            }
            if (pR.Footer.Any())
            {
                TextReport.AddRange(pR.Footer);
                TextReport.Add(PrintCenter($"------------------------"));
            }

            if (Printer != null) // якщо друк на фіскалку тоді не друкувати
            {
                TextReport.Add($"ФН чеку {pR.Fiscal?.Number}");
                TextReport.Add($"ФН ПРРО {pR.Fiscal?.Id}");
                TextReport.Add($"{pR.Fiscal.DT.ToString("dd/MM/yyyy H:mm")}");

                if (pR.TypeReceipt == eTypeReceipt.Sale)
                    TextReport.Add(PrintCenter("Фіскальний чек"));
            }
            if (pR.TypeReceipt == eTypeReceipt.Refund)
                TextReport.Add(PrintCenter("Видатковий чек"));
            else
            {
                TextReport.Add($"QR=>{pR.Fiscal.QR}");
            }
            if (Printer == null)
            {
                EF.PrintNoFiscalReceipt(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = Global.IdWorkPlace }, TextReport);
            }
            else
                Printer.Print(TextReport);

            if (roundFiscal != 0)
            {
                try
                {
                    var pay = new Payment(pR) { IsSuccess = true, TypePay = eTypePay.FiscalInfo, SumPay = pR.SumReceipt, SumExt = roundFiscal };
                    pR.Payment = pR.Payment == null ? new List<Payment>() { pay } : pR.Payment.Append<Payment>(pay);
                    db.ReplacePayment(pay, true);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                }
            }

            return new LogRRO(pR) { TypeOperation = pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund, SUM = pR.SumCash, CodeError = 0, TypeRRO = "VirtualRRO", JSON = pR.ToJSON(), FiscalNumber = pR.Fiscal.Number, TextReceipt = string.Join(Environment.NewLine, TextReport), TypePay = TypePay };
        }
        public override void GetFiscalInfo(Receipt pR, object pRes)
        {
            var DT = DateTime.Now;
            Random rnd = new Random();
            string NumberReceipt = rnd.Next(100000000, 999999999).ToString();
            string QR = $"{Guid.NewGuid()}{Environment.NewLine}{pR.SumReceipt}{Environment.NewLine}{DT.ToString("dd/MM/yyyy H:mm")}";


            //ToDateTime("d-M-yyyy HH:mm:ss");
            pR.Fiscals.Add(pR.IdWorkplacePay, new Fiscal()
            {
                IdWorkplacePay = pR.IdWorkplacePay,
                QR = QR,
                Sum = pR.SumFiscal,
                SumRest = pR.SumRest,
                Id = FiscalNumber,
                Number = NumberReceipt,
                Head = HeadReceipt,
                //Taxes = Res.info.printinfo.taxes?.Select(el => new TaxResult() { Name = el.tax_fname, Sum = el.tax_sum, IdWorkplacePay = pR.IdWorkplacePay }),
                DT = DT
            });

        }
        override public LogRRO PrintNoFiscalReceipt(IEnumerable<string> pR)
        {
            if (Printer != null)
                Printer.Print(pR);
            else
                EF.PrintNoFiscalReceipt(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod(), IdWorkplacePay = Global.IdWorkPlace }, pR);
            return new LogRRO(new IdReceipt() { CodePeriod = Global.GetCodePeriod(), IdWorkplace = Global.IdWorkPlace }) { TypeOperation = eTypeOperation.NoFiscalReceipt, TypeRRO = Type.ToString(), JSON = pR.ToJSON(), TypePay = TypePay };

        }

        override public bool PutToDisplay(string ptext, int pLine = 1)
        {
            return true;
        }

        override public bool PeriodZReport(IdReceipt pIdR, DateTime pBegin, DateTime pEnd, bool IsFull = true)
        {
            var serchDate = pBegin;
            pIdR.CodePeriod = Global.GetCodePeriod(pBegin);
            List<LogRRO> logRRO = Bl.GetLogRRO(pIdR).ToList();
            while (serchDate < pEnd)
            {
                serchDate = serchDate.AddDays(1);
                pIdR.CodePeriod = Global.GetCodePeriod(serchDate);
                logRRO.AddRange(Bl.GetLogRRO(pIdR).ToList());
            }
            PrintReport(pIdR, eTypeOperation.PeriodZReport, logRRO, pBegin, pEnd);
            return true;
        }
        public override StatusEquipment TestDevice()
        {
            return new StatusEquipment(Model, State, "Ok");
        }
        public override decimal GetSumInCash(IdReceipt pIdR)
        {
            pIdR.IdWorkplace = Global.IdWorkPlace; //ПЕРЕРОБИТИ

            var logRRO = Bl.GetLogRRO(pIdR);
            decimal TotalRefundSum = logRRO.Where(x => x.TypeOperation == eTypeOperation.Refund).Select(x => x.SUM).Sum();
            decimal TotalSum = logRRO.Where(x => x.TypeOperation != eTypeOperation.Refund).Select(x => x.SUM).Sum() - TotalRefundSum;
            return TotalSum; // загальна готівка по всім IdWorkplace
        }

    }
}
