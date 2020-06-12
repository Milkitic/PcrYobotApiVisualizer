using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace YobotChart.Converters
{

    public class WindowMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (WindowState)value;
            return state == WindowState.Maximized ? new Thickness(6, 7, 6, 6) : new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
