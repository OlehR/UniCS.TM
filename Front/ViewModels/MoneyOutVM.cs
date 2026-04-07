using Front.Models;
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

namespace Front.ViewModels
{
    public class MoneyOutVM : INotifyPropertyChanged
    {
        // ---- Стан ----
        private decimal _totalSum;
        private string _topAmount = string.Empty;

        public ObservableCollection<CashItem> Items { get; } = new();

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

        public MoneyOutVM()
        {
            ConfirmRowCommand = new RelayCommand(
                execute: param =>
                {
                    if (param is not CashItem item) return;

                    // TODO: виклик сервісу/репозиторію для підтвердження рядка
                    item.IsConfirmed = true;
                },
                canExecute: param => param is CashItem { IsConfirmed: false }
            );

            ConfirmTopCommand = new RelayCommand(
                execute: _ =>
                {
                    // TODO: виклик сервісу/репозиторію для підтвердження всієї операції
                },
                canExecute: _ => TopAmountIsValid
            );
        }

        // =========================================================
        // ПУБЛІЧНИЙ API для додавання рядків
        // (викликати з code-behind або сервісу коли прийшов новий запис)
        // =========================================================

        /// <summary>
        /// Додає один рядок до таблиці та перераховує загальну суму.
        /// Метод потокобезпечний — маршалізує виклик до UI-потоку.
        /// </summary>
        public void AddItem(CashItem item)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Items.Add(item);
                RecalcTotal();
            });
        }

        /// <summary>
        /// Додає кілька рядків одним викликом (кожен окремо — без batch-змін).
        /// </summary>
        public void AddItems(IEnumerable<CashItem> items)
        {
            foreach (var item in items)
                AddItem(item);
        }

        private void RecalcTotal() => TotalSum = Items.Sum(i => i.AvailableQty);

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
