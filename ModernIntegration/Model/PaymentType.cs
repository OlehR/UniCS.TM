using System;
using System.Collections.Generic;
using System.Text;

namespace ModernIntegration.Enums
{
    [Flags]
    public enum PaymentType
    {
        Card = 1,
        Cash = 2,
        None = 4,
        Face = 8,
    }
}
