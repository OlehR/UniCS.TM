using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public enum Period
    {
        Year,
        Month,
        Day
    }
    public enum TypeAccess
    {
        Question = -3, //Виводити вікно для введення логіна які розширять права на цю операцію. Якщо не введено не дозволяти цю операцію. 
        NoErrorAccess = -2, //не виконувати дію Видавати повідомленя про відсутність прав. Не виводити вікно для введення логіна які розширять права на цю операцію			
        No = -1, // не виконувати дію не видавати жодних повідомлень.
        Yes = 0 // Є права на зазначену операцію.		
    }

    public enum TypePay
    {
        Partiall = 0,
        Cash = 1,
        Pos = 2,
        NonCash = 3
    }

    public enum TypeBonus
    {
        NonBonus = 0, // 
        Bonus = 1,
        BonusWithOutRest = 2, // Використовувати бонус з врахуванням здачі( якщо бонуса не вистачає - берем таким чином щоб здача була кругла)
        BonusToRest = 3,
        BonusFromRest = 4
    }
    
    /// <summary>
	/// Інформація про те що знайшли в універсальному вікні пошуку
	/// 0 - все,1 - товари,2-клієнти,3-купони та акціїї
	/// </summary>	
	public enum TypeFind
    {
        All = 0,
        Wares,
        Client,
        Action
    }
    public struct RezultFind
    {
        public TypeFind TypeFind;
        public int Count;
    }

    public enum TypePayment
    {
        Cash,
        Bonus,
        CreditCard,
        MoneyBox

    }
    public enum TypeCommit
    {
        Auto,
        Manual
    }
}
