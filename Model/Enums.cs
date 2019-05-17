using System;

namespace Model
{
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

    /// <summary>
    /// Будується запитом select listagg( c01||' = ' || t.code_data,','||chr(13)||chr(10))  within group(order by t.code_data ) from C.DATA_NAME t where t.data_level=204
    /// або C.UniCS.GenConstMID
    /// </summary>
    public enum CodeEvent
    {
        Main = 0,
        Login = 1,
        NewReceipt = 2,
        SetClient = 3,
        RecalcPrice = 4,
        ChangeReceipt = 5,
        CloseReceipt = 9,
        Find = 10,
        FindCodeWares = 11,
        FindNameWares = 12,
        FindList = 13,
        Wares = 20,
        ClearInfoWares = 21,
        AddWaresReceipt = 22,
        IncrementWares = 23,
        DecrementWares = 24,
        EditQuantityWares = 25,
        ChangeUnit = 26,
        DeleteWaresReceipt = 27,
        EKKA = 30,
        PrintReceipt = 31,
        PrintX = 32,
        PrintZ = 33,
        InputOutputMoney = 34,
        EditTimeEKKA = 35,
        Print0Receipt = 36,
        PrintCopy = 37,
        PrintZPer = 38,
        BankPos = 40,
        PrintBankPosX = 42,
        PrintBankPosZ = 43,
        WorkPrice = 50,
        ManualEditPrice = 51,
        ManualEditPercent = 52,
        ChangePriceDealer = 53,
        Return = 60,
        AllowReturn = 61,
        ReturnOtherWorkplace = 62,
        Client = 70,
        FindNameClient = 71,
        ChoiceFirms = 72
    }

    public enum TypeAccess
    {
        Question = -3, //Виводити вікно для введення логіна які розширять права на цю операцію. Якщо не введено не дозволяти цю операцію. 
        NoErrorAccess = -2, //не виконувати дію Видавати повідомленя про відсутність прав. Не виводити вікно для введення логіна які розширять права на цю операцію			
        No = -1, // не виконувати дію не видавати жодних повідомлень.
        Yes = 0 // Є права на зазначену операцію.		
    }

    /// <summary>
	/// Мови. (Тимчасово! Має ж бути системна така фігня.)
	/// </summary>
	public enum Language
    {
        def = 0,
        ar_XA,  //Арабська
        bg,//Болгарська
        hr,//Хорватська
        cs,//Чеська
        da,//Данська
        de,//Німецька
        el,//Грецька
        en = 1,//Англійська
        et,//Естонська
        es,//Іспанська
        fi,//Фінська
        fr,//Французька
        ga,//Ірландська
        hi,//Хінді
        hu,//Угорська
        he,//Іврит
        it,//Італійська
        ja,//Японська
        ko,//Корейська
        lv,//Латвійська
        lt,//Литовська
        nl,//Нідерландська
        no,//Норвезька
        pl,//Польська
        pt,//Портуґальська
        sv,//Шведська
        ro,//Румунська
        ru,//Російська
        sr_CS,//Сербська
        sk,//Словацька
        sl,//Словенська
        th,//Тайська
        tr,//Турецька
        uk_UA = 2,//Українська
        zh_chs,//Китайська (спрощене письмо)
        zh_cht//Китайська (традиційне письмо)
    };


}
