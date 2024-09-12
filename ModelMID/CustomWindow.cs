using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        RestoreLastRecipt,
        LimitSales,
        ConfirmAge,
        Info,
        BlockSale,
        UseBonus,
        MaxSumReceipt

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
        /// Поле вводу
        /// </summary>
        public string InputText { get; set; }
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
            Id = pW;
            switch (pW)
            {
                case eWindows.RestoreLastRecipt:
                    Caption = $"Відновлення останнього чека на суму {pObject}";
                    IsCancelButton = false;
                    Buttons = new ObservableCollection<CustomButton>() {
                        new CustomButton() {CustomWindow = this, Id=1,  Text="Відновити"},
                        new CustomButton() {CustomWindow = this, Id=2,  Text="Скасувати"} };
                    break;
                case eWindows.UseBonus:
                    Text = "Зробіть вибір";
                    Caption = "Зазначені номери клієнта";
                    AnswerRequired = true;
                    List<string> phones = pObject as List<string>;
                    Buttons = new ObservableCollection<CustomButton>();
                    for (int i = 0; i < phones.Count(); i++)
                    {
                        Buttons.Add(new CustomButton() { CustomWindow = this, Id = i, Text = phones[i] });
                    }
                    break;
                case eWindows.NoPrice:
                    Text = pObject as string;
                    Caption = "Відсутня ціна на товар!";
                    AnswerRequired = true;
                    break;
                case eWindows.LimitSales:
                    Text = pObject as string;
                    Caption = "Обмеження кількості!";
                    AnswerRequired = true;
                    break;
                case eWindows.ExciseStamp:
                    Text = "Ввід акцизної марки";
                    Caption = "Назва товару";
                    AnswerRequired = true;
                    ValidationMask = @"^\w{4}[0-9]{6}?$";
                    if (pObject is bool IsCashRegister == true)
                        Buttons = new ObservableCollection<CustomButton>()
                        {
                            new CustomButton() {CustomWindow = this,  Id = 33, Text = "Підтвердження акцизу", IsNeedAdmin = true }
                        };
                    else
                        Buttons = new ObservableCollection<CustomButton>()
                        {
                            new CustomButton() {CustomWindow = this,  Id = 32, Text = "Акцизний код відсутній", IsNeedAdmin = false },
                            new CustomButton() {CustomWindow = this,  Id = 33, Text = "Підтвердження акцизу", IsNeedAdmin = true }
                        };

                    break;
                case eWindows.PhoneClient:
                    Text = "Введіть ваш номер!";
                    Caption = "Пошук за номером телефону";
                    AnswerRequired = true;
                    ValidationMask = @"^[+]{0,1}[0-9]{10,13}$";
                    // Buttons = new List<CustomButton>() {new CustomButton() { Id = 666, Text = "Пошук картки" } }                    
                    break;

                case eWindows.ChoiceClient:
                    Text = "Зробіть вибір";
                    Caption = "Вибір карточки клієнта";
                    AnswerRequired = true;
                    IEnumerable<Client> d = pObject as IEnumerable<Client>;
                    Buttons = new ObservableCollection<CustomButton>(d.Select(el => new CustomButton() { CustomWindow = this, Id = el.CodeClient, Text = el.NameDiscount + ' ' + el.NameClient }));
                    break;

                case eWindows.ConfirmAge:
                    Text = "Зробіть вибір";
                    Caption = "Мені є 18";
                    AnswerRequired = true;
                    string textButton1, textButton2;
                    if (pObject is bool IsCashRegister2 == true)
                    {
                        textButton1 = "Так, клієнту є 18 років";
                        textButton2 = "Ні, клієнту менше 18 років";

                    }
                    else
                    {
                        textButton1 = "Так, мені є 18 років";
                        textButton2 = "Ні, мені менше 18 років";
                    }
                    Buttons = new ObservableCollection<CustomButton>() {
                        new CustomButton() { CustomWindow = this, Id = 1, Text = textButton1, IsNeedAdmin = false },
                        new CustomButton() { CustomWindow = this, Id = 0, Text = textButton2, IsNeedAdmin = false }
                         };
                    break;
                case eWindows.Info:
                    Caption = "Увага";
                    Text = pObject as string;
                    AnswerRequired = true;
                    break;
                case eWindows.BlockSale:
                    Caption = "Увага";
                    Text = pObject as string;
                    AnswerRequired = true;
                    break;
                case eWindows.MaxSumReceipt:
                    Caption = "Увага!";
                    decimal v = Convert.ToDecimal(pObject);
                    Text = $"Сума чека не може перевищувати {v} грн" ;
                    AnswerRequired = true;
                    break;
                default:
                    Buttons = new ObservableCollection<CustomButton>();
                    break;

            }

        }

        public CustomWindow(eStateScale pST, bool IsViewAddWeight = false, bool IsDelReceipt = false)
        {
            Id = eWindows.ConfirmWeight;
            IsCancelButton = false;
            switch (pST)
            {
                case eStateScale.WaitClear:
                    Buttons = new ObservableCollection<CustomButton>()
                    { new CustomButton() { CustomWindow=this, Id = 4, Text = "Тарувати", IsNeedAdmin = true } ,
                      new CustomButton() {CustomWindow=this, Id = 5, Text = "Вхід в адмінку", IsNeedAdmin = true } };
                    if (IsDelReceipt)
                        Buttons.Add(new CustomButton() { CustomWindow = this, Id = 6, Text = "Видалити чек", IsNeedAdmin = true });
                    break;
                case eStateScale.BadWeight:
                    Buttons = new ObservableCollection<CustomButton>();
                    Buttons.Add(new CustomButton() { CustomWindow = this, Id = 1, Text = "Підтвердити вагу", IsNeedAdmin = true });
                    if (IsViewAddWeight)
                        Buttons.Add(new CustomButton() { CustomWindow = this, Id = 2, Text = "Добавити вагу", IsNeedAdmin = true });
                    Buttons.Add(new CustomButton() { CustomWindow = this, Id = -1, Text = "Закрити", IsNeedAdmin = false });
                    break;

                case eStateScale.WaitGoods:
                    Buttons = new ObservableCollection<CustomButton>()
                    { new CustomButton() {CustomWindow = this,  Id = 1, Text = "Підтвердити вагу", IsNeedAdmin = true },
                      new CustomButton() {CustomWindow = this,  Id = 3, Text = "Видалити товар", IsNeedAdmin = true },
                      new CustomButton() {CustomWindow = this,  Id = -1, Text = "Закрити", IsNeedAdmin = false }};

                    break;
                case eStateScale.NotStabilized:
                    Buttons = new ObservableCollection<CustomButton>()
                    { new CustomButton() {CustomWindow = this,  Id = 1, Text = "Підтвердити вагу", IsNeedAdmin = true },
                      new CustomButton() {CustomWindow = this,  Id = -1, Text = "Закрити", IsNeedAdmin = false }};
                    break;
            }
        }

    }

    public class CustomButton : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        bool _IsAdmin = false;
        /// <summary>
        /// Доступна лише в режимі адміністратора.
        /// </summary>
        public bool IsNeedAdmin { get { return _IsAdmin; } set { _IsAdmin = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GetBackgroundColor")); } }

        /// <summary>
        /// Id >0 кнопки. Буде в відповіді 
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Текст вікні
        /// </summary>
        public string Text { get; set; }

        public CustomWindow CustomWindow { get; set; }
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
        public long IdButton { get; set; }

        /// <summary>
        /// Додаткові дані для обробки.
        /// </summary>
        public object ExtData { get; set; } = null;
    }
}