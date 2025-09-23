using Model;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utils;

namespace ModelMID
{
    /// <summary>
    /// Зберігає інформацію про клієнта
    /// </summary>
    public class Client : Model.Client
    {
        public IEnumerable<ReceiptGift> ReceiptGift { get; set; }
        public Client() { }
        public Client(int parCodeClient) => CodeClient = parCodeClient;
        public Client(Model.Client pC)
        {
            CodeClient = pC.CodeClient;
            NameClient = pC.NameClient;
            TypeDiscount = pC.TypeDiscount;
            NameDiscount = pC.NameDiscount;
            BarCode = pC.BarCode;
            MainPhone = pC.MainPhone;
            PhoneAdd = pC.PhoneAdd;
            PersentDiscount = pC.PersentDiscount;
            CodeDealer = pC.CodeDealer;
            SumBonus = pC.SumBonus;
            PercentBonus = pC.PercentBonus;
            SumMoneyBonus = pC.SumMoneyBonus;
            IsUseBonusToRest = pC.IsUseBonusToRest;
            Wallet = pC.Wallet;
            IsUseBonusFromRest = pC.IsUseBonusFromRest;
            StatusCard = pC.StatusCard;
            ViewCode = pC.ViewCode;
            BirthDay = pC.BirthDay;
            OneTimePromotion = pC.OneTimePromotion;
            IsCheckOnline = pC.IsCheckOnline;
            IsMoneyCard = pC.IsMoneyCard;
            IsСertificate = pC.IsСertificate;
        }
    }
}
