using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using YobotExtension.ViewModels;

namespace YobotExtension.UserControls.StatsGraphControls
{
    /// <summary>
    /// TotalDamageTrendControl.xaml 的交互逻辑
    /// </summary>
    public partial class TotalDamageTrendControl : UserControl, IChartSwitchControl
    {
        public TotalDamageTrendControl()
        {
            InitializeComponent();
        }

        public TotalDamageTrendControl(StatsVm stats, IChartProvider chartProvider) : this()
        {
            InitModels(stats, chartProvider);
        }

        public StatsVm Stats { get; private set; }
        public IChartProvider ChartProvider { get; private set; }

        public void InitModels(StatsVm stats, IChartProvider chartProvider)
        {
            ChartProvider = chartProvider;
            Stats = stats;
        }

        private void BtnTotalDamageTrend_Click(object sender, RoutedEventArgs e)
        {
            Stats.IsLoading = true;
            ChartProvider.RecreateGraph();

            ChartProvider.Chart.AxisY[0].Separator.ClearValue(LiveCharts.Wpf.Separator.StepProperty);

            var totalDamageTrend = Stats.ApiObj.Challenges
                .GroupBy(k => k.Cycle).ToList();
            var vm = new StatsGraphVm
            {
                SeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Values = new ChartValues<double>(totalDamageTrend
                            .Select(k =>
                            {
                                return (
                                    k.Max(o => o.ChallengeTime) - k.Min(o => o.ChallengeTime)
                                ).TotalSeconds;
                            })
                        ),
                        Title = "花费时间"
                    }
                },
                AxisXLabels = totalDamageTrend.Select(k => k.Key.ToString()).ToArray(),
                AxisXTitle = "周目",
                AxisYTitle = "周目花费时间"
            };

            Stats.StatsGraph = vm;
            ChartProvider.Chart.AxisY[0].LabelFormatter = value => TimeSpan.FromSeconds(value).ToString();

            Stats.IsLoading = false;
        }

        private void BtnTotalDamageDailyTrend_Click(object sender, RoutedEventArgs e)
        {
            Stats.IsLoading = true;
            ChartProvider.RecreateGraph();

            //ChartProvider.Chart.AxisY[0].Separator.ClearValue(LiveCharts.Wpf.Separator.StepProperty);

            var totalDamageTrend = Stats.ApiObj.Challenges
                .GroupBy(k => k.ChallengeTime.Date.ToShortDateString()).ToList();
            var vm = new StatsGraphVm
            {
                SeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Values = new ChartValues<int>(totalDamageTrend
                            .Select(k =>
                            {
                                return k.Sum(o=>o.Damage);
                            })
                        ),
                        Title = "伤害"
                    }
                },
                AxisXLabels = totalDamageTrend.Select(k => k.Key.ToString()).ToArray(),
                AxisXTitle = "日期",
                AxisYTitle = "伤害"
            };

            Stats.StatsGraph = vm;
            ChartProvider.Chart.AxisY[0].LabelFormatter = value => value.ToString("N0");
            Stats.StatsGraph.Title = "行会每天伤害趋势";
            //ChartProvider.Chart.AxisX[0].LabelFormatter = value => value.ToString("N0");

            Stats.IsLoading = false;
        }
    }
}
