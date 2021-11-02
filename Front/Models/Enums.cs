using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Front.Models
{
    public enum StateMainWindows
    {
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
        /// Очікуємо ручного вводу пароля адміністратора
        /// </summary>
        WaitAdminPassWord
    }
}
