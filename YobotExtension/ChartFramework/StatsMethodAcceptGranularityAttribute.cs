using System;

namespace YobotChart.ChartFramework
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