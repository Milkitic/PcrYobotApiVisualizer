using System.ComponentModel;
using System.Runtime.CompilerServices;
using YobotChart.Shared.Win32.Properties;
using YobotChart.Shared.YobotService;

namespace YobotChart.Shared.Win32.ChartFramework.SourceProviders
{
    public sealed class YobotApiSource : INotifyPropertyChanged
    {

        private IYobotApiObject _yobotApi;
        /// <summary>
        /// 数据源
        /// </summary>
        public IYobotApiObject YobotApi
        {
            get => _yobotApi;
            set
            {
                if (Equals(value, _yobotApi)) return;
                _yobotApi = value;
                OnPropertyChanged();
            }
        }
        private static YobotApiSource _default;
        private static object _defaultLock = new object();

        public static YobotApiSource Default
        {
            get
            {
                lock (_defaultLock)
                {
                    return _default ?? (_default = new YobotApiSource());
                }
            }
        }

        private YobotApiSource()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}