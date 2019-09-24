using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    class Payment:IdReceipt
    {
        eTypePay TypePay { get; set;}
        decimal Sum  { get; set;}
        decimal SumExt { get; set; }
        string NumberTerminal { get; set; }
        string NumberReceipt { get; set; }
        string CodeAuthorization { get; set; }
    }
}
