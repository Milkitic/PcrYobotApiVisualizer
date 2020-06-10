using YobotExtension.Shared.YobotService;

namespace YobotExtension.ChartFramework
{
    public interface IStatsProvider
    {
        /// <summary>
        /// 数据源
        /// </summary>
        IChallengeObject[] Challenges { get; set; }
    }
}