using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Linq;
using YobotChart.Shared.Win32.Annotations;
using YobotChart.Shared.YobotService;

namespace YobotChart.Shared.Win32.ChartFramework.StatsProviders
{
    [StatsProviderMetadata("872c4594-aaf1-4453-a652-fb304cb936f7",
        Author = "yf_extension",
        Name = "行会数据分析",
        Description = "包含行会的伤害趋势、周目花费时间、行会横向比较等。")]
    public class ClansStatsProvider : IStatsProvider
    {
        public YobotApiSource YobotApiSource { get; set; }
        public IChallengeObject[] Challenges => YobotApiSource.YobotApi.Challenges;

        [StatsMethod("行会每天伤害趋势")]
        [StatsMethodAcceptGranularity(GranularityType.Total)]
        [StatsMethodThumbnail("行会每天伤害趋势.jpg")]
        [UsedImplicitly]
        public CartesianChartConfigModel DailyDamageTrend()
        {
            var totalDamageTrend = Challenges
                .GroupBy(k => k.ChallengeTime.Date.ToShortDateString()).ToList();
            var cartesianConf = new CartesianChartConfigModel
            {
                AxisXTitle = "日期",
                AxisYTitle = "伤害",
                Title = "行会每天伤害趋势",
                SeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Values = new ChartValues<int>(totalDamageTrend
                            .Select(k => k.Sum(o => o.Damage))
                        ),
                        Title = "伤害"
                    }
                },
                ChartConfig = chart => chart.AxisY[0].LabelFormatter = value => value.ToString("N0")
            };

            return cartesianConf;
        }

        [StatsMethod("行会周目花费时间趋势")]
        [StatsMethodAcceptGranularity(GranularityType.Total)]
        [StatsMethodThumbnail("行会周目花费时间趋势.jpg")]
        [UsedImplicitly]
        public CartesianChartConfigModel RoundCostTimeTrend()
        {
            var totalDamageTrend = Challenges
                .GroupBy(k => k.Cycle).ToList();

            var cartesianConf = new CartesianChartConfigModel
            {
                AxisXTitle = "周目",
                AxisYTitle = "周目花费时间",
                Title = "行会周目花费时间趋势",
                SeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Values = new ChartValues<double>(totalDamageTrend
                            .Select((k, i) =>
                            {
                                if (i == totalDamageTrend.Count - 1 && k.Count() < 90)
                                {
                                    return (
                                        DateTime.Now - k.Min(o => o.ChallengeTime)
                                    ).TotalSeconds;
                                }

                                return (
                                    k.Max(o => o.ChallengeTime) - k.Min(o => o.ChallengeTime)
                                ).TotalSeconds;
                            })
                        ),
                        Title = "花费时间"
                    }
                },
                ChartConfig = chart => chart.AxisY[0].LabelFormatter = value => TimeSpan.FromSeconds(value).ToString()
            };

            return cartesianConf;
        }
    }
}