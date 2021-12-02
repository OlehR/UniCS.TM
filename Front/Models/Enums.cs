using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Models
{
    public enum eStateMainWindows
    {
        NotDefine,
        /// <summary>
        /// Стартове вікно
        /// </summary>
        StartWindow,
        /// <summary>
        /// Основний стан - очікування події сканування та інших
        /// </summary>
        WaitInput,
        /// <summary>
        /// Очікуємо вибір ціни Сигарет
        /// </summary>
        WaitInputPrice,
        /// <summary>
        /// Очукуємо Вагу з Магелана
        /// </summary>
        WaitWeight,
        /// <summary>
        /// Очікуєм введення акцизної мрки
        /// </summary>
        WaitExciseStamp,
        /// <summary>
        /// В режимі пошуку товару
        /// </summary>
        WaitFindWares,
        /// <summary>
        /// Проблема з контрольною вагою
        /// </summary>
        ProblemWeight,
        /// <summary>
        /// Очікуємо адміністратора
        /// </summary>
        WaitAdmin,
        /// <summary>
        /// Очікуємо логін адміністратора
        /// </summary>
        WaitAdminLogin,
        /// <summary>
        /// Очікуємо пароль адміністратора
        /// </summary>
        WaitAdminPassword,
            /// <summary>
            /// Очікуєм підтвердження 18 років
            /// </summary>
        WaitConfirm18
    }
   
}
