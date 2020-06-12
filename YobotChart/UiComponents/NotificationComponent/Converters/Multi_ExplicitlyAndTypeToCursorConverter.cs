using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace YobotChart.UiComponents.NotificationComponent.Converters
{
    internal class Multi_ExplicitlyAndTypeToCursorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is NotificationType type && values[1] is bool closeExplicitly)
            {
                if (type != NotificationType.Alert || closeExplicitly) return Cursors.Arrow;
                return Cursors.Hand;
            }

            return Cursors.Arrow;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}