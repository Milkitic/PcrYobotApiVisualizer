using System;
using System.Collections.Generic;
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
        private List<int> _roundList;
        private List<DateTime> _dateList;
        private List<int> _phaseList;

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

        /// <summary>
        /// 周目数
        /// </summary>
        public List<int> RoundList
        {
            get => _roundList;
            set
            {
                if (value == _roundList) return;
                _roundList = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 日期列表
        /// </summary>
        public List<DateTime> DateList
        {
            get => _dateList;
            set
            {
                if (Equals(value, _dateList)) return;
                _dateList = value;
                OnPropertyChanged();
            }
        }

        public List<int> PhaseList
        {
            get => _phaseList;
            set
            {
                if (Equals(value, _phaseList)) return;
                _phaseList = value;
                OnPropertyChanged();
            }
        }

        public async Task UpdateDataAsync()
        {
            YobotApi = await YobotService.GetApiInfo().ConfigureAwait(false);
            if (YobotApi == null) return;
            YobotApi.Challenges = YobotApi.Challenges.OrderBy(k => k.ChallengeTime).ToArray();
            RoundList = YobotApi.Challenges.GroupBy(k => k.Cycle).Select(k=>k.Key).ToList();
            DateList = YobotApi.Challenges.GroupBy(k => k.ChallengeTime.AddHours(-5).Date)
                .Select(k => k.Key).ToList();
            PhaseList = YobotApi.Challenges.GroupBy(k => k.BattleId).Select(k=>k.Key).ToList();
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