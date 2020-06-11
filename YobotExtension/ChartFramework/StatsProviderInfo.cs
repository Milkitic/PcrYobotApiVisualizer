using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YobotChart.ChartFramework
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