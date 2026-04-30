using Front.Equipments;
using Front.Models;
using ModelMID;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Utils;

namespace Front.ViewModels
{
    public class MoneyOutVM : INotifyPropertyChanged
    {
        // ---- Стан ----
        private decimal _totalSum;
        private string _topAmount = string.Empty;
        BL Bl;
        public ObservableCollection<CashItem> Items { get; } = [];

        /// <summary>Сума AvailableQty по всіх рядках</summary>
        public decimal TotalSum
        {
            get => _totalSum;
            private set { _totalSum = value; OnPropertyChanged(); OnPropertyChanged(nameof(TopAmountIsValid)); }
        }

        /// <summary>Текст у верхньому полі вводу</summary>
        public string TopAmount
        {
            get => _topAmount;
            set
            {
                _topAmount = value;
                OnPropertyChanged();               
                OnPropertyChanged(nameof(TopAmountIsValid));
                CommandManager.InvalidateRequerySuggested();
                if (!IsRecalc) return;
                decimal Sum = _topAmount.ToDecimal();
                if(Sum>0)
                {
                    foreach (var el in Items)
                    {
                        var s=Math.Floor(el.AvailableQty / 100) * 100;
                        if (s < Sum)
                        {
                            el.InputQty = s.ToString();
                            Sum -= s;
                        }
                        else
                        {
                            el.InputQty = Sum.ToString();
                                s = 0;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>True якщо введене значення > 0 і менше TotalSum</summary>
        public bool TopAmountIsValid
        {
            get
            {
                if (!decimal.TryParse(TopAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                    return false;
                return v > 0m && v < TotalSum;
            }
        }

        // ---- Команди ----

        /// <summary>
        /// Підтвердити рядок таблиці.
        /// TODO: тут буде реальний виклик API/логіки.
        /// </summary>
        public ICommand ConfirmRowCommand { get; }

        /// <summary>
        /// Підтвердити верхню суму.
        /// TODO: тут буде реальний виклик API/логіки.
        /// </summary>
        public ICommand ConfirmTopCommand { get; }

        //public IEnumerable<Rro> RRO;
        public MoneyOutVM(IEnumerable<Equipment> pRRO)
        {
            Bl = BL.GetBL;
            foreach (var el in pRRO)
            {
                var RRO = el as Rro;
                if (RRO is not null)
                    Items.Add(new(RRO, RecalcTotal) { });
            }

            ConfirmRowCommand = new RelayCommand(
                execute: param =>
                {
                    if (param is not CashItem item) return;

                    MoneyOutCommand(item);
                },
                canExecute: param => param is CashItem { IsConfirmed: false }
            );

            ConfirmTopCommand = new RelayCommand(
                execute: _ =>
                {
                    bool allOk = true;
                    if (TopAmount.ToInt() > 0)
                        foreach (var el in Items)
                            allOk = allOk && MoneyOutCommand(el);

                    if (allOk)
                    {
                        bool Res = Send1CMonyeOut(TopAmount.ToInt());
                        if (Res)
                        {
                            Global.Message($"Успішно відправлено в 1С. Сума=>{TopAmount}", eTypeMessage.Information);
                            TopAmount = "";
                        }
                    }
                },
                canExecute: _ => TopAmountIsValid
            );
        }

        bool Send1CMonyeOut(int pSum)
        {
            return true;
        }
        private bool MoneyOutCommand(CashItem pCashItem)
        {
            int Sum = pCashItem.InputQty.ToInt();
            if (Sum <= 0) return true;

            InOutMoney IOM = new() { IdWorkplace = pCashItem.RRO.IdWorkplacePay, Sum = Sum };

            var r = pCashItem.RRO.MoveMoney(-Sum);

            if (r.CodeError == 0)
            {
                pCashItem.IsConfirmed = true;
                pCashItem.InputQty = "";

                Task.Run(async()=> { 
                    var r= await Bl.ds.InOutMoneyAsync(IOM);
                    if(!(r?.Success==true))
                        Global.Message.Invoke($"Помилка збереження винесення коштів по {IOM.IdWorkplace} в 1С=> " + r?.TextError, eTypeMessage.Error);
                });
                return true;
            }
            else
            {
                Global.Message.Invoke("Помилка " + r.Error, eTypeMessage.Error);
                return false;
            }
        }
        // =========================================================
        // ПУБЛІЧНИЙ API для додавання рядків
        // (викликати з code-behind або сервісу коли прийшов новий запис)
        // =========================================================


        bool IsRecalc = true;
        private void RecalcTotal()
            {
            TotalSum = Items.Sum(i => i.AvailableQty);
            IsRecalc = false;
            TopAmount = Items.Sum(i => i.InputQty.ToInt()).ToString();
            IsRecalc = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    // =========================================================
    // КОНВЕРТЕРИ
    // =========================================================

    /// <summary>bool → !bool (для IsEnabled підтверджених рядків)</summary>
    public sealed class InverseBoolConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c) => v is true ? false : (object)true;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => v is true ? false : (object)true;
    }

    /// <summary>IsConfirmed → фон рядка (зелений відтінок або прозорий)</summary>
    public sealed class ConfirmedRowBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush ConfirmedBrush =
            new(Color.FromRgb(240, 253, 244)); // #F0FDF4

        public object Convert(object v, Type t, object p, CultureInfo c)
            => v is true ? ConfirmedBrush : Brushes.Transparent;

        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => throw new NotSupportedException();
    }

    /// <summary>TopAmountIsValid → колір рамки TextBox (синій/червоний/сірий)</summary>
    public sealed class TopAmountBorderBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush Valid = new(Color.FromRgb(74, 108, 247));  // #4A6CF7
        private static readonly SolidColorBrush Invalid = new(Color.FromRgb(239, 68, 68));   // #EF4444
        private static readonly SolidColorBrush Empty = new(Color.FromRgb(209, 213, 219)); // #D1D5DB

        public object Convert(object v, Type t, object p, CultureInfo c)
            => v switch { true => Valid, false => Invalid, _ => Empty };

        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => throw new NotSupportedException();
    }
}
