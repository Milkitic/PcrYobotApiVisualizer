using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YobotChart.Shared.Win32.ChartFramework.Attributes;
using YobotChart.Shared.Win32.ChartFramework.ConfigModels;

namespace YobotChart.Shared.Win32.ChartFramework.StatsProviders
{
    public class StatsProviderInfo
    {
        public StatsProviderMetadataAttribute Metadata { get; set; }

        public List<StatsFunctionInfo> FunctionList { get; set; } = new List<StatsFunctionInfo>();
    }

    public class StatsFunctionInfo
    {
        public StatsMethodAttribute Attribute { get; set; }
        public Func<GranularityModel, Task<IChartConfigModel>> Function { get; set; }
        public GranularityType[] AcceptGranularities { get; set; }
        public string ThumbnailPath { get; set; }
    }
}