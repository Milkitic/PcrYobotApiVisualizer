using System;
using System.Globalization;
using System.Windows.Data;

namespace YobotChart.Converters
{
    public class DateTimeToFormatStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
            {
                try
                {
                    if (dt == default)
                        return "不限";
                    string format = parameter is string s ? s : null;
                    return string.IsNullOrEmpty(format)
                        ? dt.ToString(CultureInfo.CurrentCulture)
                        : dt.ToString(format);
                }
                catch (Exception e)
                {
                    return dt.ToString(CultureInfo.CurrentCulture);
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class Int32ToFormatStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
            {
                try
                {
                    if (i == -1)
                        return "不限";
                    return i;
                }
                catch (Exception e)
                {
                    return i.ToString();
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}