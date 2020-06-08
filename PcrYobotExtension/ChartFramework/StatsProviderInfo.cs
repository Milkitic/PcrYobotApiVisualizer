using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PcrYobotExtension.ChartFramework
{
    public class StatsProviderInfo
    {
        public StatsProviderMetadataAttribute Metadata { get; set; }

        public Dictionary<StatsMethodAttribute, Func<GranularityModel, Task<IChartConfigModel>>> FunctionsMapping
        {
            get;
            set;
        } = new Dictionary<StatsMethodAttribute, Func<GranularityModel, Task<IChartConfigModel>>>();
    }
}