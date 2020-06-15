using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using YobotChart.Shared.Annotations;
using YobotChart.Shared.Win32.ChartFramework.StatsProviders;

namespace YobotChart.Shared.Win32.ChartFramework.SourceProviders
{
    public sealed class StatsProviderInfoSource
    {
        public static void LoadSource()
        {
            if (Default is null)
            {
                _ = new StatsProviderInfoSource();
            }
            else
            {
                Default._statsProvidersList = ChartProviderLoader.Load();
            }
        }

        private HashSet<StatsProviderInfo> _statsProvidersList;
        public HashSet<StatsProviderInfo> StatsProvidersList
        {
            get => _statsProvidersList;
            set
            {
                if (Equals(value, _statsProvidersList)) return;
                _statsProvidersList = value;
                OnPropertyChanged();
            }
        }

        private StatsProviderInfoSource()
        {
            _statsProvidersList = ChartProviderLoader.Load();
            Default = this;
        }

        public static StatsProviderInfoSource Default { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}