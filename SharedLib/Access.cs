using ModelMID;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLib
{
    public class Access
    {
        public SortedList<eTypeAccess, eTypeUser> Right = new SortedList<eTypeAccess, eTypeUser>() { };
        static Access Instance = null;
        public static Access GetAccess() { return Instance == null ? new Access() : Instance; }
        public Access()
        {
            Right.Add(eTypeAccess.DelWares, eTypeUser.AdminSSC);
            Right.Add(eTypeAccess.DelReciept, eTypeUser.AdminSSC);
            Right.Add(eTypeAccess.ChoicePrice, eTypeUser.Сashier);
            Right.Add(eTypeAccess.ConfirmAge, eTypeUser.Сashier);
            Right.Add(eTypeAccess.ExciseStamp, eTypeUser.Сashier);
            Right.Add(eTypeAccess.FixWeight, eTypeUser.AdminSSC);
            Right.Add(eTypeAccess.AddNewWeight, eTypeUser.AdminSSC);
            Right.Add(eTypeAccess.ReturnReceipt, eTypeUser.Сashier);            
        }

        public bool GetRight(eTypeUser TypeUser, eTypeAccess TypeRight)
        {
            return (int)Right[TypeRight]>= (int)TypeUser;
        }

    }
}
