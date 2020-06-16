using System;
using System.Globalization;
using System.Windows.Data;

namespace YobotChart.Converters
{
    public class ComboBoxPopupMinWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (double)value;
            if (val == 0)
            {
                return 265;
            }

            return val + 65;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}