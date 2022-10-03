using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.API
{
    public enum eTypeMessage
    {
        [Description("Відкрити/закрити зміну")]
        OnOffShift,

        [Description("Додати вагу")]
        AddWeight,

        [Description("Штрих код")]
        BarCode,

        [Description("Підтвердити вагу")]
        ConfirmWeight,

        [Description("Загальний стан")]
        GeneralCondition,
    }
}
