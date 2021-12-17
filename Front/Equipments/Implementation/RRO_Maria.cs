using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;
using Resonance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Equipments.Implementation
{
    public class RRO_Maria:Rro
    {
       
        bool IsInit=false;

        M304ManagerApplication M304 = new M304ManagerApplication() { ThrowExceptionsOnError = true };
        public RRO_Maria(IConfiguration pConfiguration, Action<string, string> pLogger = null, Action<eStatusRRO> pActionStatus = null) : base(pConfiguration,pLogger, pActionStatus)
        {
            M304.Open();
        }

        bool Init()
        {
            if (IsInit)
            {
                M304.Done();
                IsInit = false;
            }

            if (M304.Init(Port, OperatorName, OperatorPass, false) == 1)
            {
                if( string.IsNullOrEmpty (M304.GetDocumentsInfoXML()))
                    M304.Done();
                var dt = M304.GetPrinterTime();//20130606110200 треба звірити час.
                IsInit = true;
            }
            else
            {
                StrError= M304.LastErrorMessage;
                int.TryParse(M304.LastErrorCode, out CodeError);
             }            
            return IsInit;
        }

        public override LogRRO PrintCopyReceipt(int parNCopy = 1)
        {
            M304.CheckCopy();
            return null;
        }


        
        override public async Task<LogRRO> PrintZAsync(IdReceipt pIdR)
        {
            M304.ZReportAsync();
            return null;
        }

        override public async Task<LogRRO> PrintXAsync(IdReceipt pIdR)
        {
            M304.XReportAsync();
            return null;
        }

        /// <summary>
        /// Внесення/Винесення коштів коштів. pSum>0 - внесення
        /// </summary>
        /// <param name="pSum"></param>
        /// <returns></returns>
        override public LogRRO MoveMoney(decimal pSum)
        {
            return null;//throw new NotImplementedException();
        }

        override public async Task<LogRRO> MoveMoneyAsync(decimal pSum, IdReceipt pIdR)
        {
            //M305.
            return null;//throw new NotImplementedException();
        }

        /// <summary>
        /// Друк чека
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        override public async Task<LogRRO> PrintReceiptAsync(Receipt pR)
        {
            if ((pR.TypeReceipt == eTypeReceipt.Sale ? M304.OpenCheck() : M304.OpenReturnCheck()) == 1)
            {
                //M304.LastCheckNumber;
               // M304.NextZNumber;
               foreach(var el in pR.Wares)
                {
                    var TaxGroup = Global.GetTaxGroup(el.TypeVat, el.TypeWares);
                    int TG1 = 0, TG2 = 0;
                    int.TryParse(TaxGroup.Substring(0, 1), out TG1);
                    if(TaxGroup.Length>1)
                        int.TryParse(TaxGroup.Substring(1, 1), out TG2);
                    var Name = (el.IsUseCodeUKTZED && !String.IsNullOrEmpty(el.CodeUKTZED) ? el.CodeUKTZED.Substring(0,10) + "#" : "") + el.NameWares;
                    if (!String.IsNullOrEmpty(el.ExciseStamp))
                        M304.AddExciseStamps(el.ExciseStamp?.Split(','));
                    M304.FiscalLineEx(Name,Convert.ToInt32( (el.CodeUnit == Global.WeightCodeUnit ? 1000 : 1) * el.Quantity), Convert.ToInt32(el.Price * 100), el.CodeUnit == Global.WeightCodeUnit ? 1 : 0, TG1, TG2, el.CodeWares, (el.DiscountEKKA > 0 ? 0 : -1), null, Convert.ToInt32(el.DiscountEKKA), null);

                }
               if(pR.Payment!=null && pR.Payment.Count()>0)
                {
                    //foreach(var el in pR.Payment)
                   // M304.AddSlip(2,1,el.NumberTerminal,el.Bank,)
                }

                /*AddSlip(paymentFormIndex,
                                     merchantID,
                                     terminalID,
                                     payTypeName,
                                     ePayType,
                                     approvalCode,
                                     paymentSystem,
                                     transactionCode,
                                     fee,
                                     printCashierSignaturePlaceholder,
                                     printCardholderSignaturePlaceholder);

             Если Рез = 0 Тогда   
                 Объект.ОписаниеОшибки = Объект.Драйвер.LastErrorMessage;
                 Сообщить(Объект.Драйвер.LastErrorCode);
                 Результат = мОшибкаНеизвестно;
                 АннулироватьЧек(Объект);
             КонецЕсли;

         КонецЕсли;	
     КонецЕсли;	

     //Объект.Драйвер.PutToExternalDisplay("");

     //Рез = Объект.Драйвер.SetMaxRounding(10);
     //Если Рез = 0 Тогда   
     //	Объект.ОписаниеОшибки = Объект.Драйвер.LastErrorMessage;
     //	Результат = мОшибкаНеизвестно;
     //	АннулироватьЧек(Объект);
     //КонецЕсли;
     //
     //Рез = Объект.Драйвер.GetMaxRounding();
     //Если Рез = 0 Тогда   
     //	Объект.ОписаниеОшибки = Объект.Драйвер.LastErrorMessage;
     //	Результат = мОшибкаНеизвестно;
     //	АннулироватьЧек(Объект);
     //КонецЕсли;
     //
     //Рез = Объект.Драйвер.EnableRounding();
     //Если Рез = 0 Тогда   
     //	Объект.ОписаниеОшибки = Объект.Драйвер.LastErrorMessage;
     //	Результат = мОшибкаНеизвестно;
     //	АннулироватьЧек(Объект);
     //КонецЕсли;

     //СуммаНал = Окр(СуммаНал,1);

     //Сообщить(Рез);
     Рез = Объект.Драйвер.CloseCheckEx(СуммаНал*100, СуммаБезнал*100, 0, 0,);	

     Объект.Драйвер.PutToDisplay(СуммаНал + СуммаБезнал); 

     Если Рез/100 > 0 и СуммаНал > 0 Тогда //если оплата наличкой
         СуммаНал = Окр(Рез/100,1);
     КонецЕсли;	

     Если Рез = 0 Тогда   
         Объект.ОписаниеОшибки = Объект.Драйвер.LastErrorMessage;
         Результат = мОшибкаНеизвестно;
         АннулироватьЧек(Объект);
     Иначе
         Объект.Драйвер.Done();
     КонецЕсли;*/


            }
            else
            {
                StrError = M304.LastErrorCode;
            }
            

            return null;
            
        }

        override public bool PutToDisplay(string pText)
        {
            if (!IsInit)
                Init();
            if (IsInit)
              return M304.PutToExternalDisplay(pText, true) == OperationResult.Success;
            return false;

        }


    }
}
