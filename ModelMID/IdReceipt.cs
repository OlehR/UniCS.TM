﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ModelMID
{
    public class IdReceipt
    {

        protected string PrefixWarehouse { get { switch (Global.CodeWarehouse) { case 9: return "K"; case 15: return "B"; case 148: return "E"; default: return "X"; } } }
        
        protected string Prefix
        {
            get
            {
                string res = Global.GetWorkPlaceByIdWorkplace(IdWorkplacePay > 0 ? IdWorkplacePay : IdWorkplace)?.Prefix;
                if (string.IsNullOrEmpty(res))
                    res = PrefixWarehouse + Global.GetNumberCashDeskByIdWorkplace(IdWorkplace);
                return res;                
            }
        }

        public int IdWorkplacePay { get; set; }
        public int IdWorkplace { get; set; }
        /// <summary>
        /// Код робочого місця (важливо коли кілька підприємців)
        /// </summary>
   
        public int CodePeriod { get; set; }
        public int CodeReceipt { get; set; }

        public DateTime DTPeriod { get 
            {
                DateTime res;
                if (DateTime.TryParseExact(CodePeriod.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out res))
                    return res;
                return DateTime.Now.Date;            
            } 
        }
        public IdReceipt()
        {
            IdWorkplace = 0;
            CodePeriod = 0;
            CodeReceipt = 0;
        }

        public IdReceipt(IdReceipt parIdReceipt)
        {
            SetIdReceipt(parIdReceipt);
        }

         public void SetIdReceipt(IdReceipt idReceipt)
        {
            if (idReceipt == null)
                return;
            IdWorkplace = idReceipt.IdWorkplace;
            CodePeriod = idReceipt.CodePeriod;
            CodeReceipt = idReceipt.CodeReceipt;
            IdWorkplacePay= idReceipt.IdWorkplacePay;
        }
        public override bool Equals(object obj)
        {
            if (obj is IdReceipt)
            {
                var o = (IdReceipt)obj;
                return IdWorkplace == o.IdWorkplace && CodePeriod == o.CodePeriod && CodeReceipt == o.CodeReceipt;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return IdWorkplace * 100000 + CodePeriod * 1000 + CodeReceipt;
        }

        public string NumberReceipt1C
        {
            get
            {
                var d = Convert.ToInt32(Math.Floor((DTPeriod - new DateTime(2019, 01, 01)).TotalDays)).ToString("D4");
                return Prefix + d + CodeReceipt.ToString("D4");//PrefixWarehouse + Global.GetNumberCashDeskByIdWorkplace(IdWorkplace) 
            }
        }
        /// <summary>
        /// Унікальний номер чека  коли 2 підприємця і 2 чека 
        /// </summary>
        public string NumberReceiptRRO
        {
            get { return NumberReceipt1C + (IdWorkplacePay>0?$"_{IdWorkplacePay}":"");}
        }
    }
}
