﻿using Front.Equipments.Virtual;
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
        bool IsError = false;

        M304ManagerApplication M304_;
        dynamic M304;
        public RRO_Maria(Equipment pEquipment, IConfiguration pConfiguration, Microsoft.Extensions.Logging.ILoggerFactory pLoggerFactory = null, Action<StatusEquipment> pActionStatus = null) : base(pEquipment, pConfiguration,eModelEquipment.RRO_Maria, pLoggerFactory, pActionStatus)
        {
            try
            {
                OperatorName = Configuration?.GetValue<string>($"{KeyPrefix}OperatorName");
                OperatorPass = Configuration?.GetValue<string>($"{KeyPrefix}OperatorPass");

                Type t = System.Type.GetTypeFromProgID("M304Manager.Application");
                M304 = Activator.CreateInstance(t);
                Init();
                
                //CST.XReport();
                //M304.OpenTextDocument();
                //M304.PrintQR("12346");
                //M304.FreeTextLine(0,0,3,"hello");//, doubleWidth: true, doubleHeight: true);
                //M304.CloseTextDocument();
                
               // M304_ = new M304ManagerApplication();                
                
            }
            catch(Exception e) 
            { var m=e.Message; }
        }

        bool Init()
        {
            if (IsInit)
            {
                Done();
                IsInit = false;
            }
            
            if (!SetError(M304.Init(SerialPort, OperatorName, OperatorPass, false) != 1))
            {
                if( string.IsNullOrEmpty (M304.GetDocumentsInfoXML()))
                    Done();
                var dt = M304.GetPrinterTime();//!!! 20130606110200 треба звірити час.
                IsInit = true;
            }
              
            return IsInit;
        }

        void Done() 
        {
            if(M304!=null)
            {
                M304.Done();
            }
        }
        
        bool SetError(bool pIsError)
        {
            IsError = pIsError;
            if (IsError)
            {
                if (M304 != null)
                {
                    StrError = M304.LastErrorCode;
                    int.TryParse(M304.LastErrorCode, out CodeError);
                }
            }
            else
            {
                CodeError = 0;
                StrError = null;
            }
            return IsError;
        }

        public override LogRRO PrintCopyReceipt(int parNCopy = 1)
        {
            Init();
            M304.CheckCopy();
            Done();
            return null;
        }


        override public LogRRO PrintZ(IdReceipt pIdR)
        {
            if (Init())
            {
                SetError(M304.ZReportAsync() != 1);
                Done();
            }
            return new LogRRO(pIdR) { CodeError = CodeError, Error = StrError, SUM = 0, TypeRRO = "Maria304", TypeOperation = eTypeOperation.ZReport };
        }

       override public LogRRO PrintX(IdReceipt pIdR)
        {
            if (Init())
            {
                SetError(M304.XReportAsync() != 1);
                Done();
            }
           return new LogRRO(pIdR) { CodeError = CodeError, Error = StrError, SUM = 0, TypeRRO = "Maria304", TypeOperation = eTypeOperation.XReport };
       }

       /// <summary>
       /// Внесення/Винесення коштів коштів.
       /// </summary>
       /// <param name="pSum"> pSum>0 - внесення</param>
       /// <returns></returns>
      override public LogRRO MoveMoney(decimal pSum, IdReceipt pIdR = null)
        {
           SetError(M304.MoveCash((pSum > 0 ? 1 : 0), Convert.ToInt32(Math.Abs(pSum) * 100m)) != 1);
           return new LogRRO(pIdR) {CodeError=CodeError,Error=StrError,SUM= pSum,TypeRRO= "Maria304" , TypeOperation = pSum > 0?eTypeOperation.MoneyIn:eTypeOperation.MoneyOut};
       }

       /// <summary>
       /// Друк чека
       /// </summary>
       /// <param name="pR"></param>
       /// <returns></returns>
       override public LogRRO PrintReceipt(Receipt pR)
        {
           if (Init())
           {

               if (!SetError((pR.TypeReceipt == eTypeReceipt.Sale ? M304.OpenCheck() : M304.OpenReturnCheck()) != 1))
               {
                   //M304.LastCheckNumber;
                   // M304.NextZNumber;
                   foreach (var el in pR.Wares)
                   {
                       var taxGroup = TaxGroup(el);
                       int TG1 = 0, TG2 = 0;
                       int.TryParse(taxGroup[0..0], out TG1);
                       if (taxGroup.Length > 1)
                           int.TryParse(taxGroup[1..1], out TG2);
                       var Name = (el.IsUseCodeUKTZED && !string.IsNullOrEmpty(el.CodeUKTZED) ? el.CodeUKTZED.Substring(0, 10) + "#" : "") + el.NameWares;
                       if (!String.IsNullOrEmpty(el.ExciseStamp))
                           if (SetError((M304.AddExciseStamps(el.ExciseStamp?.Split(',')) != OperationResult.Success)))
                               break;

                       if (SetError(M304.FiscalLineEx(Name, Convert.ToInt32((el.CodeUnit == Global.WeightCodeUnit ? 1000 : 1) * el.Quantity), Convert.ToInt32(el.Price * 100), el.CodeUnit == Global.WeightCodeUnit ? 1 : 0, TG1, TG2, el.CodeWares, (el.DiscountEKKA > 0 ? 0 : -1), null, Convert.ToInt32(el.DiscountEKKA), null) != 1))
                           break;
                   }

                   pR.SumFiscal = M304.CheckSum / 100M;

                   if (pR.Payment?.Any(el=>el.TypePay==eTypePay.Card)==true)
                   {
                       foreach (var el in pR.Payment)
                       {
                           if (SetError(M304.AddSlip(2, "0", el.NumberTerminal, el.Bank, el.NumberSlip, el.CodeAuthorization, "paymentSystem", el.CodeAuthorization) != OperationResult.Success))
                               break;
                       }
                   }

                   /*
                    Параметры
   paymentFormIndex: номер безналичной формы оплаты (1..19)
   merchantID: задаёт параметр "ID еквайра,торгівця" (1..32 символов)
   terminalID: задаёт параметр "ID пристрою" (1..8 символов)
   operationType: задаёт параметр "Вид операції" (1..16 символов)
   PAN: задаёт параметр "ЕПЗ" (0..32 символов)
   approvalCode: задаёт параметр "Код авторизації" (0..6 символов)
   paymentSystem: задаёт параметр "Платіжна система" (1..16 символов)
   transactionCode: задаёт параметр "Код транзакції" (0..12 символов)
   fee: задаёт параметр "Сума комісії" (необязательный параметр)
   printCashierSignaturePlaceholder: печатать место для подписи кассира (необязательный
   параметр)
   printCardholderSignaturePlaceholder: печатать место для подписи держателя карты


                  AddSlip(paymentFormIndex,
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
               if (!IsError)
               {
                   SetError(M304.CloseCheckEx(/*СуммаНал * 100*/0, Convert.ToInt32(pR.SumFiscal * 100M)) != 1);
                   M304.PutToDisplay(pR.SumFiscal.ToString());
               }             
           }

           if (!IsError)
           {
               decimal Sum = 0m;
               string sum = M304.GetCheckResultXML();
            /*   if (!string.IsNullOrEmpty(sum))
                   decimal.TryParse(sum, out Sum);
               pR.SumFiscal = Sum / 100m;*/

               M304.GetDocumentsInfoXML(); // Отримати фіскальні та інші номера чека.
           }
           pR.NumberReceipt = M304.LastCheckNumber.ToString();

           Done();

           return new LogRRO(pR) { CodeError = CodeError, Error = StrError, SUM = pR.SumFiscal, TypeRRO = "Maria304", 
               TypeOperation = (pR.TypeReceipt == eTypeReceipt.Sale ? eTypeOperation.Sale : eTypeOperation.Refund),
               JSON= M304.GetCheckResultXML()
       };

       }

       override public bool PutToDisplay(string pText, int pLine = 1)
        {
           if (!IsInit)
               Init();
            pText = pText.Replace(Environment.NewLine, " ");
           if (IsInit)
             return M304.PutToExternalDisplay(pText) == 1;
           return false;
       }     

    }
}