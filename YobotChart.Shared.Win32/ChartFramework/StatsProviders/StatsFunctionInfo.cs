using System;
using System.Threading.Tasks;
using YobotChart.Shared.Win32.ChartFramework.ConfigModels;

namespace YobotChart.Shared.Win32.ChartFramework.StatsProviders
{
    public class StatsFunctionInfo
    {
        public StatsFunctionInfo()
        {
            
        }

        public StatsFunctionInfo(Guid providerGuid)
        {
            ProviderGuid = providerGuid;
        }

        public Guid ProviderGuid { get; }
        public Guid Guid { get; internal set; }
        public string Name { get; internal set; }

        public Func<GranularityModel, Task<IChartConfigModel>> Function { get; internal set; }
        public GranularityType[] AcceptGranularities { get; internal set; }
        public string ThumbnailPath { get; internal set; }
    }
}