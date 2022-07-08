using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ModelMID
{
    public enum eWindows
    {
        ChoiceClient,
        PhoneClient,
        ConfirmWeight,
        ExciseStamp,
        NoPrice,
        RestoreLastRecipt
    }

    public class CustomWindow
    {        
        /// <summary>
        /// Id вікна. Буде в відповіді
        /// </summary>
        public eWindows Id { get; set; }
        /// <summary>
        /// Назва вікна
        /// </summary>
        public string Caption { get; set; }
        /// <summary>
        /// Текст в вікні
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Шлях до картинки
        /// </summary>
        public string PathPicture { get; set; }

        /// <summary>
        /// На майбутне замість тексту HTML в вікні.
        /// </summary>
        public string HTML { get; set; }

        /// <summary>
        /// Чи можна натиснути кнопку скасувати.
        /// </summary>
        public bool AnswerRequired { get; set; } = false;
        /// <summary>
        /// Якщо очікуємо Текстова поле то маска в вигляді регулярки
        /// </summary>
        public string ValidationMask { get; set; }
        /// <summary>
        /// В скільки рядків розміщати кнопки.
        /// </summary>
        public int ColumnButtons { get; set; } = 1;
        public ObservableCollection<CustomButton> Buttons { get; set; }

        public bool IsCancelButton { get; set; } = true;
    }

    public class CustomButton
    {
        /// <summary>
        /// Доступна лише в режимі адміністратора.
        /// </summary>
        public bool IsAdmin { get; set; } = false;

        /// <summary>
        /// Id >0 кнопки. Буде в відповіді 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Текст вікні
        /// </summary>
        public string Text { get; set; }
    }

    public class CustomWindowAnswer
    {
        public IdReceipt idReceipt { get; set; }
        /// <summary>
        /// Id вікна. Буде в відповіді
        /// </summary>
        public eWindows Id { get; set; }

        /// <summary>
        /// Введений Текст
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Id натиснененої кнопки. (-1 - Cancel )
        /// </summary>
        public int IdButton { get; set; }

        /// <summary>
        /// Додаткові дані для обробки.
        /// </summary>
        public object ExtData { get; set; } = null;
    }
}