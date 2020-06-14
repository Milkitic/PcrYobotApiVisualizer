namespace YobotChart.Shared.Win32.ChartFramework
{
    public interface IStatsProvider
    {
        /// <summary>
        /// 数据源
        /// </summary>
        YobotApiSource YobotApiSource { get; set; }
    }
}