using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using YobotChart.Shared.Annotations;
using YobotChart.Shared.YobotService;
using YobotChart.Shared.YobotService.V1;

namespace YobotChart.Shared.Win32.ChartFramework.SourceProviders
{
    public sealed class YobotApiSource : INotifyPropertyChanged
    {
        public event Action SourceAutoUpdated;

        private object _updatingLock = new object();

        public IYobotServiceV1 YobotService { get; set; }
        private IYobotApiObject _yobotApi;
        private List<int> _roundList;
        private List<DateTime> _dateList;
        private List<int> _phaseList;
        private bool _isUpdating;

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

        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                if (value == _isUpdating) return;
                _isUpdating = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan AutoUpdateInterval { get; set; } = TimeSpan.FromSeconds(10);

        public async Task UpdateDataAsync()
        {
            lock (_updatingLock)
            {
                if (IsUpdating) return;
                IsUpdating = true;
            }

            try
            {
                YobotApi = await YobotService.GetApiInfo().ConfigureAwait(false);
                _sw.Restart();

                if (YobotApi == null) return;

                YobotApi.Challenges = YobotApi.Challenges
                    .OrderBy(k => k.ChallengeTime)
                    .ToArray();
                RoundList = YobotApi.Challenges
                    .GroupBy(k => k.Cycle)
                    .Select(k => k.Key)
                    .ToList();
                DateList = YobotApi.Challenges
                    .GroupBy(k => k.ChallengeTime.AddHours(-5).Date)
                    .Select(k => k.Key)
                    .ToList();
                PhaseList = YobotApi.Challenges
                    .GroupBy(k => k.BattleId)
                    .Select(k => k.Key)
                    .ToList();
            }
            finally
            {
                lock (_updatingLock) IsUpdating = false;
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
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            _sw.Start();
            Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    if (_sw.Elapsed >= AutoUpdateInterval)
                    {
                        bool isUpdating;
                        lock (_updatingLock) isUpdating = IsUpdating;

                        if (!isUpdating)
                        {
                            try
                            {
                                var yobotApi = YobotApi;
                                await UpdateDataAsync();
                                if (!ApiEquals(yobotApi, YobotApi)) SourceAutoUpdated?.Invoke();
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }

                        _sw.Restart();
                    }

                    Thread.Sleep(100);
                }
            });
        }

        private bool ApiEquals(IYobotApiObject api1, IYobotApiObject api2)
        {
            if (api1.Challenges.SequenceEqual(api2.Challenges, new ChallengeComparer()))
            {
                return true;
            }

            return false;
        }


        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
            _cts?.Cancel();
        }

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private Stopwatch _sw = new Stopwatch();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class ChallengeComparer : IEqualityComparer<IChallengeObject>
    {
        public bool Equals(IChallengeObject x, IChallengeObject y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return x.BattleId == y.BattleId &&
                   x.QQId == y.QQId &&
                   x.BehalfQQId == y.BehalfQQId &&
                   x.BossNum == y.BossNum &&
                   x.ChallengeTime == y.ChallengeTime &&
                   x.Cycle == y.Cycle &&
                   x.Damage == y.Damage &&
                   x.HealthRemain == y.HealthRemain &&
                   x.IsContinue == y.IsContinue &&
                   x.Message == y.Message;
        }

        public int GetHashCode(IChallengeObject obj)
        {
            unchecked
            {
                var hashCode = obj.BattleId;
                hashCode = (hashCode * 397) ^ obj.QQId.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.BehalfQQId?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ obj.BossNum;
                hashCode = (hashCode * 397) ^ obj.ChallengeTime.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Cycle;
                hashCode = (hashCode * 397) ^ obj.Damage;
                hashCode = (hashCode * 397) ^ obj.HealthRemain.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.IsContinue.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.Message?.GetHashCode() ?? 0;
                return hashCode;
            }
        }
    }
}