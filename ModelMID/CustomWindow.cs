using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ModelMID
{
    public enum eWindows
    {
        NoDefinition,
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
        public eWindows Id { get; set; } = eWindows.NoDefinition;
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

        /// <summary>
        /// Чи відображати кнопку Cancel
        /// </summary>
        public bool IsCancelButton { get; set; } = true;

        public CustomWindow() { }

        public CustomWindow(eWindows pW, object pObject = null)
        {
            switch (pW)
            {
                case eWindows.RestoreLastRecipt:
                    Id = eWindows.RestoreLastRecipt;
                    Caption = $"Відновлення останнього чека на суму {pObject}";
                    IsCancelButton = false;
                    Buttons = new ObservableCollection<CustomButton>() {
                        new CustomButton() { Id=1,  Text="Відновити"},
                        new CustomButton() { Id=2,  Text="Скасувати"} };
                    break;
                case eWindows.NoPrice:
                    Id = eWindows.NoPrice;
                    Text = pObject as string;
                    Caption = "Відсутня ціна на товар!";
                    AnswerRequired = true;
                    break;
                case eWindows.ExciseStamp:
                    Id = eWindows.ExciseStamp;
                    Text = "Ввід акцизної марки";
                    Caption = "Назва товару";
                    AnswerRequired = true;
                    ValidationMask = @"^\w{4}[0-9]{6}?$";
                    Buttons = new ObservableCollection<CustomButton>() {new CustomButton() { Id = 31, Text = "Ok", IsAdmin = false},
                                                                    new CustomButton() { Id = 32, Text = "Акцизний код відсутній", IsAdmin = true } };
                    break;
                case eWindows.PhoneClient:
                    Id = eWindows.PhoneClient;
                    Text = "Введіть ваш номер!";
                    Caption = "Пошук за номером телефону";
                    AnswerRequired = true;
                    ValidationMask = @"^[+]{0,1}[0-9]{10,13}$";
                    // Buttons = new List<CustomButton>() {new CustomButton() { Id = 666, Text = "Пошук картки" } }                    
                    break;
                    default:
                    Buttons = new ObservableCollection<CustomButton>();
                    break;
                case eWindows.ChoiceClient:
                    Id = eWindows.ChoiceClient;
                    Text = "Зробіть вибір";
                    Caption = "Вибір карточки клієнта";
                    AnswerRequired = true;
                    IEnumerable<Client> d = pObject as IEnumerable<Client>;
                    Buttons = (System.Collections.ObjectModel.ObservableCollection<CustomButton>) d.Select(el => new CustomButton() { Id = el.CodeClient, Text = el.NameClient });
                    break;
            }

        }

        public CustomWindow(eStateScale pST)
        {
            Id = eWindows.ConfirmWeight;
            
                switch (pST)
                {
                    case eStateScale.WaitClear:
                        Buttons = new ObservableCollection<CustomButton>()
                    { new CustomButton() { Id = 4, Text = "Тарувати", IsAdmin = true } ,
                      new CustomButton() { Id = 5, Text = "Вхід в адмінку", IsAdmin = true } };
                        break;
                    case eStateScale.BadWeight:
                        Buttons = new ObservableCollection<CustomButton>()
                    { new CustomButton() { Id = 1, Text = "Підтвердити вагу", IsAdmin = true },
                      new CustomButton() { Id = 2, Text = "Добавити вагу", IsAdmin = true } ,
                      new CustomButton() { Id = -1, Text = "Закрити", IsAdmin = false }};
                        break;

                    case eStateScale.WaitGoods:
                        Buttons = new ObservableCollection<CustomButton>()
                    { new CustomButton() { Id = 1, Text = "Підтвердити вагу", IsAdmin = true },
                      new CustomButton() { Id = 3, Text = "Видалити товар", IsAdmin = true } };
                        break;
                    case eStateScale.NotStabilized:
                        Buttons = new ObservableCollection<CustomButton>()
                    { new CustomButton() { Id = 1, Text = "Підтвердити вагу", IsAdmin = true },
                      new CustomButton() { Id = -1, Text = "Закрити", IsAdmin = false }};
                        break;
                }            
        }
    
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