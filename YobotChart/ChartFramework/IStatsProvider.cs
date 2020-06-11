using YobotChart.ViewModels;

namespace YobotChart.ChartFramework
{
    public interface IStatsProvider
    {
        /// <summary>
        /// 数据源
        /// </summary>
        StatsVm Stats { get; set; }
    }
}