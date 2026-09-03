using Front.Equipments.Implementation.ModelVchasno;
using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;
using SharedLib;
using System.Linq;
using Utils;

namespace Front.Equipments.Implementation
{
    public class RRO_DoubleReceipt : Rro
    {
        Rro Fiscal, Virtual;
        long CodeWares;
      string NameWares;
      decimal Price = 10.00m;
        int[] TypeWaresReplace = [];

        WDB_SQLite db;
        ReceiptWares Wares2Cat;

        public RRO_DoubleReceipt(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null,  IEnumerable<Rro> Rros = null) : base(pEquipment, pConfiguration, eModelEquipment.RRO_DoubleReceipt, pLoggerFactory, pActionStatus)
        {
            db = WDB_SQLite.GetInstance;
            var RealRRO = Configuration?.GetValue<string>($"{KeyPrefix}RealRRO");
            var VirtualRRO = Configuration?.GetValue<string>($"{KeyPrefix}VirtualRRO");
            CodeWares= Configuration?.GetValue<long>($"{KeyPrefix}CodeWares") ?? 0;
            NameWares = Configuration?.GetValue<string>($"{KeyPrefix}NameWares")??"";
            Price = Configuration?.GetValue<decimal>($"{KeyPrefix}Price") ?? 10.00m;
            TypeWaresReplace = Configuration?.GetSection($"{KeyPrefix}TypeWaresReplace").Get<int[]>(); //Get<int[]>($"{KeyPrefix}TypeVatReplace");
            //Configuration.GetSection($"{KeyPrefix}TypeVatReplace").Bind();
            Fiscal=Rros?.Where(r=>r.DeviceConfigName.Equals(RealRRO) )?.FirstOrDefault();
            Virtual=Rros?.Where(r=>r.DeviceConfigName.Equals(VirtualRRO) )?.FirstOrDefault();
            
            Wares2Cat = db.FindWares(null, null, CodeWares)?.FirstOrDefault();
            if (Fiscal == null || Virtual == null || Wares2Cat == null || Price == 0)
                State = eStateEquipment.Error;
            else
            {
                if(!string.IsNullOrEmpty(NameWares))
                  Wares2Cat.NameWares = NameWares;
                Wares2Cat.Price = Price;
                State = eStateEquipment.On;
            }
           
        }
        

        public override LogRRO PrintCopyReceipt(int parNCopy = 1)=>Virtual.PrintCopyReceipt(parNCopy);
        override public LogRRO PrintZ(IdReceipt pIdR)=> Virtual.PrintZ(pIdR);

        override public LogRRO PrintX(IdReceipt pIdR)=> Virtual.PrintX(pIdR);

        override public LogRRO MoveMoney(decimal pSum, IdReceipt pIdR = null)=> Virtual.MoveMoney(pSum, pIdR);


        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        override public LogRRO PrintReceipt(Receipt pR)
        {
            try
            {
                IEnumerable<ReceiptWares> WaresNew = [], Wares = pR.Wares;
                foreach (var el in Wares)
                {
                    if (((int)el.TypeWares).In(TypeWaresReplace))
                    {
                        ReceiptWares W = (ReceiptWares)Wares2Cat.Clone();
                        W.IdWorkplacePay = pR.IdWorkplacePay;
                        W.Quantity = el.Sum / Price;
                        WaresNew = WaresNew.Append(W);
                    }
                    else
                    {
                        WaresNew = WaresNew.Append(el);
                    }
                }
                pR.Wares = WaresNew;
                var z = Fiscal.PrintReceipt(pR);
                if (z.CodeError != 0) return z;

                pR.Wares = Wares;
                var v = Virtual.PrintReceipt(pR);
                return v;
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this,"PrintReceipt",ex);
                State = eStateEquipment.Error;
                return new LogRRO() { Error = ex.Message, CodeError = -1 };
            }
            finally
             {
                pR.Wares = pR.Wares;
            }
        }
        public override void GetFiscalInfo(Receipt pR, object pRes)=> Virtual.GetFiscalInfo(pR, pRes);

        override public LogRRO PrintNoFiscalReceipt(IEnumerable<string> pR)=> Virtual.PrintNoFiscalReceipt(pR);

        override public bool PutToDisplay(string ptext, int pLine = 1) => true;

        override public bool PeriodZReport(IdReceipt pIdR, DateTime pBegin, DateTime pEnd, bool IsFull = true) => Virtual.PeriodZReport(pIdR, pBegin, pEnd, IsFull);

        public override StatusEquipment TestDevice()=> Virtual.TestDevice();
        public override decimal GetSumInCash(IdReceipt pIdR)=> Virtual.GetSumInCash(pIdR);
    }
}
