using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public enum eTypeCode
    {
        NotDefine = 0,
        Article = 1,
        Code = 2,
        PercentDiscount = 3,
        Coupon = 4,
        OneTimeCoupon = 5,
        Client = 6,
        BarCode2Category = 7
    }
    

    public enum eTypeBarCode
    {
        NotDefine = 0,
        WaresWeight = 1, //Ваговий
        WaresUnit = 2, //Штучний
        DiscountAfterWares = 3, //Знижка , яка надається після сканування товару.
        Coupon = 4        
    }

    public enum eKindBarCode
    {
        NotDefine = 0,
        EAN13 = 1,
        Code128=2,
        QR = 3
    }

    public class CustomerBarCode
    {
        public eKindBarCode KindBarCode { get; set; } // //1 - EAN-13
        public eTypeBarCode TypeBarCode { get; set; } //1 - Товарний Ваговий
        public string Prefix { get; set; }
        public eTypeCode TypeCode { get; set; }
        public int LenghtCode { get; set; }
        public int LenghtOperator { get; set; }
        public int LenghtQuantity { get; set; }
        public int TotalLenght { get { return KindBarCode == eKindBarCode.EAN13 ? 13 : (Prefix?.Length??0) + LenghtCode + LenghtOperator + LenghtQuantity; } }
    }
}
