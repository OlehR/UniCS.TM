﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ModelMID
{
    public class CustomWindow
    {        
        /// <summary>
        /// Id вікна. Буде в відповіді
        /// </summary>
        public int Id { get; set; }
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

        public IEnumerable<CustomButton> Buttons { get; set; }
    }

    public class CustomButton
    {
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
        /// <summary>
        /// Id вікна. Буде в відповіді
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Введений Текст
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Id натиснененої кнопки. (-1 -скасувати)
        /// </summary>
        public int IdButton { get; set; }
    }

}