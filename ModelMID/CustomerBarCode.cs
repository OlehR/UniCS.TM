using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public enum eTypeCode
    {
        NotDefine=0,
        Article=1,
        Code=2
    }
    public enum eTypeBarCode
    {
        NotDefine = 0,
        WaresWeight =1,
        WaresUnit=2
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
        public int LenghtQuantity { get; set; }
    }
}
