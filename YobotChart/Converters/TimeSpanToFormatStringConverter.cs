using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace YobotChart.Converters
{
    public class TimeSpanToFormatStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dt = TimeSpan.Zero;
            try
            {
                switch (value)
                {
                    case string str:
                        dt = TimeSpan.Parse(str);
                        break;
                    case TimeSpan span:
                        dt = span;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var sb = new StringBuilder();
                var d = dt.Days;
                var h = dt.Hours;
                var m = dt.Minutes;
                var s = dt.Seconds;
                if (d != 0) sb.Append(d + "天");
                if (h != 0) sb.Append(h + "小时");
                if (m != 0) sb.Append(m + "分");
                if (s != 0) sb.Append(s + "秒");

                return sb.ToString();
            }
            catch (Exception e)
            {
                return dt.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}