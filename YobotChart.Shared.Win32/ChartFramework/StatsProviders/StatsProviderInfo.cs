using System;
using System.Collections.Generic;

namespace YobotChart.Shared.Win32.ChartFramework.StatsProviders
{
    public class StatsProviderInfo
    {
        public string Name { get; internal set; }
        public string Author { get; internal set; }
        public Version Version { get; internal set; }
        public Guid Guid { get; internal set; }
        public string Description { get; internal set; }

        public List<StatsFunctionInfo> FunctionList { get; set; } = new List<StatsFunctionInfo>();
    }
}