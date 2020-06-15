using LiveCharts;
using LiveCharts.Wpf;

namespace YobotChart.Shared.Win32.ChartFramework.ConfigModels
{
    public class CartesianChartConfigModel : ChartConfigModel<CartesianChart>
    {
        private SeriesCollection _seriesCollection = new SeriesCollection();
        private string[] _axisXLabels;
        private string _axisXTitle;
        private string _axisYTitle;
        private string[] _axisYLabels;

        public SeriesCollection SeriesCollection
        {
            get => _seriesCollection;
            set
            {
                if (Equals(value, _seriesCollection)) return;
                _seriesCollection = value;
                OnPropertyChanged();
            }
        }

        public string[] AxisXLabels
        {
            get => _axisXLabels;
            set
            {
                if (Equals(value, _axisXLabels)) return;
                _axisXLabels = value;
                OnPropertyChanged();
            }
        }

        public string AxisXTitle
        {
            get => _axisXTitle;
            set
            {
                if (value == _axisXTitle) return;
                _axisXTitle = value;
                OnPropertyChanged();
            }
        }

        public string AxisYTitle
        {
            get => _axisYTitle;
            set
            {
                if (value == _axisYTitle) return;
                _axisYTitle = value;
                OnPropertyChanged();
            }
        }

        public string[] AxisYLabels
        {
            get => _axisYLabels;
            set
            {
                if (Equals(value, _axisYLabels)) return;
                _axisYLabels = value;
                OnPropertyChanged();
            }
        }
    }
}