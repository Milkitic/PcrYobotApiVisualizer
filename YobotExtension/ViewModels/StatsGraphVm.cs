using LiveCharts;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using YobotChart.Annotations;

namespace YobotChart.ViewModels
{
    public class StatsGraphVm : INotifyPropertyChanged
    {
        private SeriesCollection _seriesCollection = new SeriesCollection();
        private string[] _axisXLabels;
        private string[] _axisYLabels;
        private string _axisYTitle;
        private string _axisXTitle;
        private string _title;

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

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}