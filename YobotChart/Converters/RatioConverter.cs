using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace YobotChart.Converters
{
    public class RatioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var d = System.Convert.ToDouble(parameter);
                return (double?)value * d;
            }
            catch (Exception ex)
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntPositiveTo26Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is int i && i > 0)
                    return 26;
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Multi_IntPositiveToVisibleConverter : IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var b = parameter == null;
                if (!(value is null))
                    return b ? Visibility.Visible : Visibility.Collapsed;
                return b ? Visibility.Collapsed : Visibility.Visible;
                //var b = parameter == null;
                //if (value is long i && i > 0)
                //    return b ? Visibility.Visible : Visibility.Collapsed;
                //return b ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2)
            {
                var b = parameter == null;
                if (values[0] is long i && i > 0)
                    return b ? Visibility.Visible : Visibility.Collapsed;
                return b ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}