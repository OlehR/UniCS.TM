using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ModelMID;
using System;
using System.Globalization;

namespace AvaloniaMain.Converters
{
    public class DatabaseUpdateStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is eSyncStatus status)
            {
                switch (status)
                {
                    case eSyncStatus.NotDefine:
                        return new SolidColorBrush(Colors.Orange);
                    case eSyncStatus.StartedFullSync:
                        return new SolidColorBrush(Colors.Yellow);
                    case eSyncStatus.ErrorDB:
                    case eSyncStatus.Error:
                        return new SolidColorBrush(Colors.Red);
                    case eSyncStatus.SyncFinishedError:
                        return new SolidColorBrush(Colors.Purple);
                    case eSyncStatus.SyncFinishedSuccess:
                        return new SolidColorBrush(new Color(0xFF, 0x41, 0x9E, 0x08)); // Using hexadecimal ARGB value
                    default:
                        return new SolidColorBrush(Colors.Transparent);
                }
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
