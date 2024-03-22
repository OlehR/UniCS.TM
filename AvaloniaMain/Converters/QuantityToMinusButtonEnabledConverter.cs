using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaMain.Converters
{
    public class QuantityToMinusButtonEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal quantity)
            {
                return quantity > 1m; // Порівнюємо з 1m (1m - decimal), якщо quantity більше за 1, то повертаємо true
            }

            return false; // Якщо значення не є decimal або null, кнопка буде відключена
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
