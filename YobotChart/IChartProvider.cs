using LiveCharts.Wpf.Charts.Base;

namespace YobotChart
{
    public interface IChartProvider
    {
        /// <summary>
        /// 图表控件
        /// </summary>
        Chart Chart { get; }

        /// <summary>
        /// 重建图表
        /// </summary>
        /// <returns></returns>
        void RecreateGraph();

        /// <summary>
        /// 重建自定义图表
        /// </summary>
        void RecreateGraph(Chart chart);
    }
}