using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using YobotChart.Shared.Annotations;
using YobotChart.Shared.YobotService;
using YobotChart.Shared.YobotService.V1;

namespace YobotChart.Shared.Win32.ChartFramework.SourceProviders
{
    public sealed class YobotApiSource : INotifyPropertyChanged
    {
        public IYobotServiceV1 YobotService { get; set; }
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

        public async Task UpdateDataAsync()
        {
            var apiObj = await YobotService.GetApiInfo().ConfigureAwait(false);
            YobotApi = apiObj;
            YobotApi.Challenges = YobotApi.Challenges.OrderBy(k => k.ChallengeTime).ToArray();
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