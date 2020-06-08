using PcrYobotExtension.Models;

namespace PcrYobotExtension.ChartFramework
{
    public interface IStatsProvider
    {
        /// <summary>
        /// 数据源
        /// </summary>
        ChallengeModel[] Challenges { get; set; }
    }
}