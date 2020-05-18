using PcrYobotExtension.ViewModels;

namespace PcrYobotExtension.UserControls.StatsGraphControls
{
    public interface IChartSwitchControl
    {
        StatsVm Stats { get; }
        bool IsLoading { get; set; }
        IChartProvider ChartProvider { get; }
        void InitModels(StatsVm stats, IChartProvider chartProvider);
    }
}
