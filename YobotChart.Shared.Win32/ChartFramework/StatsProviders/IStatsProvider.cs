using YobotChart.Shared.Win32.ChartFramework.SourceProviders;

namespace YobotChart.Shared.Win32.ChartFramework.StatsProviders
{
    public interface IStatsProvider
    {
        /// <summary>
        /// 数据源
        /// </summary>
        YobotApiSource YobotApiSource { get; set; }
    }
}