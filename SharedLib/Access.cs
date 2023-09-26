using ModelMID;
using ModelMID.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLib
{
    public class Access
    {
        /// <summary>
        /// Користувач системи.
        /// </summary>
        public User СurUser { get; set; }

        /// <summary>
        /// Користувач для підтвердження операції
        /// </summary>
        public User UserOperation { get; set; }

        public SortedList<eTypeAccess, eTypeUser> Right = new SortedList<eTypeAccess, eTypeUser>() { };
        static Access Instance = null;
        public static Access GetAccess() { return Instance == null ? new Access() : Instance; }
        
        public Access()
        {
            Init();            
        }

        public void Init() 
        {
            Right.Clear();
            eTypeUser R = (Global.TypeWorkplaceCurrent == eTypeWorkplace.SelfServicCheckout ? eTypeUser.AdminSSC : eTypeUser.Guardian);//eTypeUser.AdminSSC;//
            Right.Add(eTypeAccess.DelWares, R);
            Right.Add(eTypeAccess.DelReciept, R);
            Right.Add(eTypeAccess.ReturnReceipt, R);
            Right.Add(eTypeAccess.ChoicePrice, eTypeUser.Сashier);
            Right.Add(eTypeAccess.ConfirmAge, eTypeUser.Сashier);
            Right.Add(eTypeAccess.ExciseStamp, eTypeUser.Сashier);
            Right.Add(eTypeAccess.FixWeight, eTypeUser.AdminSSC);
            Right.Add(eTypeAccess.AddNewWeight, eTypeUser.AdminSSC);
            Right.Add(eTypeAccess.AdminPanel, Global.TypeWorkplaceCurrent == eTypeWorkplace.SelfServicCheckout ? eTypeUser.AdminSSC : eTypeUser.Сashier);
            Right.Add(eTypeAccess.UseBonus, eTypeUser.Guardian);
        }

        public bool GetRight(eTypeUser pTypeUser, eTypeAccess pTypeRight)
        {
            if (pTypeRight < 0)
                return true;
            return (int)Right[pTypeRight]<= (int)pTypeUser;
        }
        public bool GetRight(eTypeAccess pTypeRight)
        {
            return GetRight(СurUser.TypeUser, pTypeRight);  //(int)Right[pTypeRight] <= (int)СurUser.TypeUser;
        }
        public bool GetRight(User pUser, eTypeAccess pTypeRight)
        {
            return GetRight(pUser.TypeUser, pTypeRight); //(int)Right[pTypeRight] <= (int)pUser.TypeUser;
        }
    }
}
