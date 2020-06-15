using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using YamlDotNet.Serialization;
using YobotChart.Shared.Annotations;
using YobotChart.Shared.Win32.ChartFramework.ConfigModels;
using YobotChart.Shared.Win32.ChartFramework.SourceProviders;
using YobotChart.Shared.Win32.ChartFramework.StatsProviders;

namespace YobotChart.Shared.Win32.ChartFramework
{
    public sealed class StatsViewModel : INotifyPropertyChanged
    {
        private StatsProviderInfo _sourceStatsProvider;
        private StatsFunctionInfo _sourceStatsFunction;

        private Guid _statsProviderGuid;
        private string _statsProviderMethodName;

        private IChartConfigModel _configModel;
        private GranularityModel _granularityModel;

        private bool _isLoading;

        [YamlIgnore]
        public StatsProviderInfo SourceStatsProvider
        {
            get
            {
                if (_sourceStatsProvider == null) InitProvider();
                return _sourceStatsProvider;
            }
            set => _sourceStatsProvider = value;
        }

        [YamlIgnore]
        public StatsFunctionInfo SourceStatsFunction
        {
            get
            {
                if (_sourceStatsFunction == null) InitProvider();
                return _sourceStatsFunction;
            }
            set => _sourceStatsFunction = value;
        }

        /// <summary>
        /// 源图表提供类Guid
        /// </summary>
        public Guid StatsProviderGuid
        {
            get => _statsProviderGuid;
            set
            {
                if (value.Equals(_statsProviderGuid)) return;
                _statsProviderGuid = value;
                SourceStatsProvider = null;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 源图表提供方法名称
        /// </summary>
        public string StatsProviderMethodName
        {
            get => _statsProviderMethodName;
            set
            {
                if (value == _statsProviderMethodName) return;
                _statsProviderMethodName = value;
                SourceStatsFunction = null;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 图表条件模型
        /// </summary>
        public GranularityModel GranularityModel
        {
            get => _granularityModel;
            set
            {
                if (Equals(value, _granularityModel)) return;
                _granularityModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 图表信息模型
        /// </summary>
        public IChartConfigModel ConfigModel
        {
            get => _configModel;
            set
            {
                if (Equals(value, _configModel)) return;
                _configModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 数据是否正在准备，即图表是否正在加载
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (value == _isLoading) return;
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public void InitProvider()
        {
            var provider =
                StatsProviderInfoSource.Default.StatsProvidersList.FirstOrDefault(k => k.Guid == StatsProviderGuid);
            if (provider == null)
            {
                throw new Exception("Not found provider: " + StatsProviderGuid);
            }

            var func = provider.FunctionList.FirstOrDefault(k => k.Name == StatsProviderMethodName);

            if (func == null)
            {
                throw new Exception("Not found function: " + StatsProviderMethodName);
            }

            _sourceStatsProvider = provider;
            _sourceStatsFunction = func;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}