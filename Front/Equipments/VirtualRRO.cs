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

namespace Front.Equipments.Implementation
{
    public class VirtualRRO : Rro
    {
        Printer Printer;
        //public EquipmentFront EF = new EquipmentFront(null, null, null);
        BL Bl = BL.GetBL;
        string HeadReceipt= "ТОВ";
        string FiscalNumber = "657513548";
        
        public VirtualRRO(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null, Printer pR = null) : this(pEquipment, pConfiguration,  pLoggerFactory, pActionStatus)
       {
            Printer= pR;
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
            throw new NotImplementedException();
        }

        override public LogRRO PrintZ(IdReceipt pIdR)
        {
            return new LogRRO(pIdR) { TypeOperation = eTypeOperation.ZReport, FiscalNumber = "V0001111" };
        }

        override public LogRRO PrintX(IdReceipt pIdR)
        {
            List<string> str = new ();
            str.Add("Print X");
            if(Printer!=null)
                Printer.Print(str);
            
            
            var TMPvalue = Bl.GetLogRRO(pIdR);

            return new LogRRO(pIdR) { TypeOperation = eTypeOperation.XReport, FiscalNumber = "V0001111" };
        }


        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        override public LogRRO MoveMoney(decimal pSum, IdReceipt pIdR = null)
        {
            eTypeOperation typeOperation = pSum > 0 ? eTypeOperation.MoneyIn : eTypeOperation.MoneyOut;
            return new LogRRO(pIdR) { TypeOperation = typeOperation, FiscalNumber = typeOperation.GetDescription(), SUM = pSum, TypeRRO = "VirtualRRO", JSON = pIdR.ToJSON() };
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        override public LogRRO PrintReceipt(Receipt pR)
        {
            GetFiscalInfo(pR, null);
            if (Printer != null)
            {
                Printer.PrintReceipt(pR);
            }
            return new LogRRO(pR) { TypeOperation = pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund, FiscalNumber = pR.Fiscal.Number, SUM = pR.SumReceipt, CodeError = 0,  TypeRRO = "VirtualRRO", JSON = pR.ToJSON() };
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
            return new LogRRO(new IdReceipt() { CodePeriod = Global.GetCodePeriod(), IdWorkplace = Global.IdWorkPlace }) { TypeOperation = eTypeOperation.NoFiscalReceipt, TypeRRO = Type.ToString(), JSON = pR.ToJSON() };

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

    }
}
