using System;

namespace PcrYobotExtension.ChartFramework
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