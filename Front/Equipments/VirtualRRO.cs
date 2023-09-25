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
        string HeadReceipt = "ТОВ";
        string FiscalNumber = "657513548";
        int MaxCountCharacters = 34;
        List<string> TextReport = new();


        public VirtualRRO(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null, Printer pR = null) : this(pEquipment, pConfiguration, pLoggerFactory, pActionStatus)
        {
            Printer = pR;
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
            return new LogRRO(new IdReceipt() { IdWorkplace = Global.IdWorkPlace, CodePeriod = Global.GetCodePeriod() }) 
                { TypeOperation = eTypeOperation.CopyReceipt, TextReceipt = logRRO.LastOrDefault().TextReceipt, TypeRRO = "VirtualRRO", JSON = logRRO.LastOrDefault().ToJSON(), FiscalNumber = $"{FiscalNumber}_{eTypeOperation.CopyReceipt}",TypePay=TypePay };
        }

        override public LogRRO PrintZ(IdReceipt pIdR)
        {
            return PrintReport(pIdR, eTypeOperation.ZReport);
        }

        override public LogRRO PrintX(IdReceipt pIdR)
        {
            return PrintReport(pIdR, eTypeOperation.XReport);
        }

        private LogRRO PrintReport(IdReceipt pIdR, eTypeOperation typeOperation)
        {
            TextReport.Clear();

            var logRRO = Bl.GetLogRRO(pIdR);
            //кількість продажних чеків
            int CountSaleReceipt = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.Sale).Count();
            //Загальна сума продажів
            decimal TotalSaleSum = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.Sale).Select(x => x.SUM).Sum();
            //Кількість чеків на повернення
            int CountRefunddReceipt = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.Refund).Count();
            //Сума чеків на повернення
            decimal TotalRefundSum = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.Refund).Select(x => x.SUM).Sum();
            //кількість чеків внесення
            int CountMoneyInReceipt = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.MoneyIn).Count();
            //Сума внесень
            decimal TotalMoneyInSum = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.MoneyIn).Select(x => x.SUM).Sum();
            //Кількість чеків вилучення
            int CountMoneyOutReceipt = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.MoneyOut).Count();
            //сума вилучень
            decimal TotalMoneyOutSum = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation == eTypeOperation.MoneyOut).Select(x => x.SUM).Sum();
            //Готівки в касі
            decimal TotalSum = logRRO.Where(x => x.IdWorkplacePay == pIdR.IdWorkplacePay && x.TypeOperation != eTypeOperation.Refund).Select(x => x.SUM).Sum() - TotalRefundSum;


            TextReport.Add(PrintCenter(HeadReceipt));
            TextReport.Add(PrintCenter(typeOperation.GetDescription()));
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
            TextReport.Add(PrintTwoColums("Готівки в касі:", TotalSum.ToString("0.00")));
            TextReport.Add(PrintCenter("----------------------"));
            TextReport.Add(DateTime.Now.ToString());
            TextReport.Add($"ФН РРО {FiscalNumber}");
            if (Printer != null)
                Printer.Print(TextReport);
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
                    // Якщо поточний рядок вже перевищує максимальну довжину,
                    // то вирівнюємо його по центру і переносимо на новий рядок
                    formattedText += AlignCenter(currentLine) + Environment.NewLine;
                    currentLine = word;
                }
            }

            // Додаємо останній рядок до відформатованого тексту
            formattedText += AlignCenter(currentLine);

            return formattedText;
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
                        formattedText += currentLine + Environment.NewLine;
                        currentLine = word;
                    }
                }
                int remainingSpaces = MaxCountCharacters - (currentLine.Length + str2.Length);
                string alignedStr1 = currentLine.PadRight(currentLine.Length + remainingSpaces);
                return formattedText + alignedStr1 + str2;


                //return str1.PadRight(maxLength) + Environment.NewLine + str2.PadLeft(maxLength);
            }
            else
            {
                // Інакше, вирівнюємо str1 по лівому краю, а str2 по правому краю,
                // додаючи відповідну кількість пробілів, і об'єднуємо їх в одному рядку.
                int remainingSpaces = MaxCountCharacters - (str1.Length + str2.Length);
                string alignedStr1 = str1.PadRight(str1.Length + remainingSpaces);
                string alignedStr2 = str2.PadLeft(str2.Length + remainingSpaces);
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
            return new LogRRO(pIdR) { TypeOperation = typeOperation, FiscalNumber = typeOperation.GetDescription(), SUM = pSum, TypeRRO = "VirtualRRO", JSON = pIdR.ToJSON(), TypePay = TypePay };
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
            TextReport.Add(PrintCenter(HeadReceipt));
            TextReport.Add(PrintCenter(pR.TypeReceipt.GetDescription()));
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

            TextReport.Add(PrintTwoColums("Сума", pR.SumCreditCard.ToString("F2")));

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
            TextReport.Add($"ФН чеку {pR.Fiscal?.Number}");
            TextReport.Add($"ФН ПРРО {pR.Fiscal?.Id}");
            TextReport.Add($"{pR.Fiscal.DT.ToString("dd/MM/yyyy H:mm")}");

            if (pR.TypeReceipt == eTypeReceipt.Sale)
                TextReport.Add(PrintCenter("Фіскальний чек"));
            else
                TextReport.Add(PrintCenter("Видатковий чек"));

            //if (Printer != null)
            //    Printer.Print(TextReport);

            return new LogRRO(pR) { TypeOperation = pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund, SUM = pR.SumCreditCard, CodeError = 0, TypeRRO = "VirtualRRO", JSON = pR.ToJSON(), FiscalNumber = pR.Fiscal.Number, TextReceipt = string.Join(Environment.NewLine, TextReport), TypePay = TypePay };
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
            List<ReceiptText> d = pR.Select(el => new ReceiptText() { Text = el.StartsWith("QR=>") ? el.SubString(4) : el, RenderType = el.StartsWith("QR=>") ? eRenderAs.QR : eRenderAs.Text }).ToList();
            //PrintSeviceReceipt(d);
            return new LogRRO(new IdReceipt() { CodePeriod = Global.GetCodePeriod(), IdWorkplace = Global.IdWorkPlace }) { TypeOperation = eTypeOperation.NoFiscalReceipt, TypeRRO = Type.ToString(), JSON = pR.ToJSON(), TypePay = TypePay };

        }

        override public bool PutToDisplay(string ptext, int pLine = 1)
        {
            return true;
        }

        override public bool PeriodZReport(DateTime pBegin, DateTime pEnd, bool IsFull = true)
        {
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
