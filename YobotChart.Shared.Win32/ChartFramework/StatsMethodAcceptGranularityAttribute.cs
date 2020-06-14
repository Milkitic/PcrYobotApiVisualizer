using System;

namespace YobotChart.Shared.Win32.ChartFramework
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