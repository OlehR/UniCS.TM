using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ModelMID
{
    public enum eStateMainWindows
    {
        [Description("Не визначено")]
        NotDefine,
        /// <summary>
        /// Стартове вікно
        /// </summary>
        [Description("Початок покупки")]
        StartWindow,
        /// <summary>
        /// Основний стан - очікування події сканування та інших
        /// </summary>
        [Description("Головне вікно покупця")]
        WaitInput,
        /// <summary>
        /// Звичайне вікно проте можливо здійснювати не всі операції.
        /// Якщо очікуєм вагу - добавляти позиції.
        /// </summary>
        [Description("Очікування ваги")]
        BlockWeight,
        /// <summary>
        /// Основний стан - очікування події сканування та інших
        /// </summary>
        [Description("Очікування ваги власної сумки")]
        WaitOwnBag,
        /// <summary>
        /// Введення кількості в чеку повернення
        /// </summary>
        [Description("Чек повернення")]
        WaitInputRefund,
        /// <summary>
        /// Очікуємо вибір ціни Сигарет
        /// </summary>
        [Description("Вибір ціни на сигарети")]
        WaitInputPrice,
        /// <summary>
        /// Очукуємо Вагу з Магелана
        /// </summary>
        [Description("Зважування товару")]
        WaitWeight,
        /// <summary>
        /// Очікуєм введення акцизної мрки
        /// </summary>
        //WaitExciseStamp,
        /// <summary>
        /// В режимі пошуку товару
        /// </summary>
        [Description("Пошук товарів")]
        WaitFindWares,
        /// <summary>
        /// Проблема з контрольною вагою
        /// </summary>
        [Description("Проблема з контрольною вагою")]
        ProblemWeight,
        /// <summary>
        /// Очікуємо адміністратора
        /// </summary>
        [Description("Очікування адміністратора")]
        WaitAdmin,
        /// <summary>
        /// Відображаємо адмінпанель
        /// </summary>
        [Description("В адмін панелі")]
        AdminPanel,
        /// <summary>
        /// Очікуємо логін адміністратора
        /// </summary>
        [Description("Очікування пароль адміністратора")]
        WaitAdminLogin,
        /// <summary>
        /// Очікуєм підтвердження 18 років
        /// </summary>
        [Description("Підтвердження 18 років")]
        WaitConfirm18,
        /// <summary>
        /// Процес оплати
        /// </summary>
        [Description("Процес оплати")]
        ProcessPay,

        /// <summary>
        /// Процес оплати
        /// </summary>
        [Description("Друк чеку")]
        ProcessPrintReceipt,

        /// <summary>
        /// Вибір типу оплати
        /// </summary>
        [Description("Вибір типу оплати")]
        ChoicePaymentMethod,
        /// <summary>
        /// Користувацьке вікно.
        /// </summary>
        [Description("Користувацьке вікно")]
        WaitCustomWindows,

        /// <summary>
        /// Видача картки на касі
        /// </summary>
        [Description("Видача картки")]
        WaitInputIssueCard,
        /// <summary>
        /// Знайти клієнта за номером телефону
        /// </summary>
        [Description("Пошук за номером телефону")]
        FindClientByPhone,
        /// <summary>
        /// Зміна кількості товару
        /// </summary>
        [Description("Зміна кількості товару")]
        ChangeCountWares,
    }

    public enum eSender
    {
        NotDefine,
        /// <summary>
        /// Вага
        /// </summary>
        ControlScale,
        /// <summary>
        /// Обладнання
        /// </summary>
        Equipment,
        //Scaner
    }

    public enum eTypeMonitor
    {
        HorisontalMonitorKSO,
        VerticalMonitorKSO,
        HorisontalMonitorRegular,
        AnotherTypeMonitor
    }

    public enum eCommand
    {
        NotDefine,
        /// <summary>
        /// Підтвердження дії
        /// </summary>
        Confirm,
        /// <summary>
        /// Отримати штрихкод
        /// </summary>
        BarCode,
        /// <summary>
        /// Отримати вагу
        /// </summary>
        Weight,
        /// <summary>
        /// Отримати значення для контрольної ваги
        /// </summary>
        ControlWeight,
        /// <summary>
        /// Добавити вагу 
        /// </summary>
        AddWeight,
        /// <summary>
        /// Загальний стан програми.
        /// </summary>
        GeneralCondition,
        /// <summary>
        /// Відкрити зміну.
        /// </summary>
        OpenShift,
        /// <summary>
        /// Закритття зміни.
        /// </summary>
        CloseShift,
        /// <summary>
        /// Закрити програму
        /// </summary>
        Close,
        /// <summary>
        /// Перезапуск Компютера
        /// </summary>
        Restart,
        /// <summary>
        /// Виключенння компютера
        /// </summary>
        ShutDown,
        XReport,
        ZReport,
        XReportPOS,
        ZReportPOS,
        /// <summary>
        /// Звірка X
        /// </summary>
        VerifyX,
        /// <summary>
        /// Отримати чек 
        /// </summary>
        GetReceipt,
        /// <summary>
        /// Отримати текучий чек
        /// </summary>
        GetCurrentReceipt,
        /// <summary>
        /// Фіскалізувати чек
        /// </summary>
        FiscalReceipt,
        /// <summary>
        /// Видалити чек
        /// </summary>
        DeleteReceipt,
    }
}