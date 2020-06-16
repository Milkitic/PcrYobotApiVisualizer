using System;
using System.Globalization;
using System.Windows.Data;

namespace YobotChart.Converters
{
    public class ComboBoxPopupVerticalOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int offset = 0;
            if (value is int index)
            {
                if (index >= 1)
                {
                    offset = -index * 34;
                }
            }

            int standard;
            if (parameter is string s)
            {
                standard = int.Parse(s);
            }
            else if (parameter is null)
            {
                standard = 0;
            }
            else
            {
                standard = (int)parameter;
            }

            return standard + offset;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}