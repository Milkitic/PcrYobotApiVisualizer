using PcrYobotExtension.ViewModels;

namespace PcrYobotExtension.UserControls.StatsGraphControls
{
    public interface IChartSwitchControl
    {
        /// <summary>
        /// 图表相关的上下文
        /// </summary>
        StatsVm Stats { get; }

        /// <summary>
        /// 图表源
        /// </summary>
        IChartProvider ChartProvider { get; }

        /// <summary>
        /// 注入相关模型，先写着，之后再动态注入
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="chartProvider"></param>
        void InitModels(StatsVm stats, IChartProvider chartProvider);
    }
}