using YobotExtension.Shared.YobotService;
using YobotExtension.ViewModels;

namespace YobotExtension.ChartFramework.StatsProviders
{
    [StatsProviderMetadata("872c4594-aaf1-4453-a652-fb304cb936f7",
        Author = "yf_extension",
        Name = "行会数据分析",
        Description = "包含行会的伤害趋势、周目花费时间、行会横向比较等。")]
    public class ClansStatsProvider : IStatsProvider
    {
        public StatsVm Stats { get; set; }
    }
}