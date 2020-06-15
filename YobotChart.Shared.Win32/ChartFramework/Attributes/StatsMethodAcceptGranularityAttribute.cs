using System;
using YobotChart.Shared.Win32.ChartFramework.StatsProviders;

namespace YobotChart.Shared.Win32.ChartFramework.Attributes
{
    public class StatsMethodAcceptGranularityAttribute : Attribute
    {
        public GranularityType[] AcceptGranularities { get; }

        public StatsMethodAcceptGranularityAttribute(params GranularityType[] acceptGranularities)
        {
            AcceptGranularities = acceptGranularities;
        }
    }
}