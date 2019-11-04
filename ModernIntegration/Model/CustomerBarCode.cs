using System;
using System.Collections.Generic;
using System.Text;

namespace ModernIntegration.Model
{
    public class CustomerBarCode
    {
        int KindBarCode { get; set; } // //1 - EAN-13
        int TypeBarCode { get; set; } //1 - Товарний Ваговий
        string Prefix { get; set; }
        string TypeCode { get; set; } //: "A", //A-Артикул,C-Код
        int LenghtCode { get; set; }
        int LenghtQuantity { get; set; }
    }
}
