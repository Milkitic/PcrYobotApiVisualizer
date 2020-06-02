using System;
using LiveCharts;
using LiveCharts.Wpf;
using PcrYobotExtension.Models;
using PcrYobotExtension.Services;
using PcrYobotExtension.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace PcrYobotExtension.UserControls.StatsGraphControls
{
    /// <summary>
    /// PersonalDamageControl.xaml 的交互逻辑
    /// </summary>
    public partial class PersonalDamageControl : UserControl, IChartSwitchControl
    {
        internal class DayPersonalDamageModel
        {
            public string Name { get; set; }
            public List<int> TimeDamages { get; set; } = new List<int>();
        }

        internal class CyclePersonalDamageModel
        {
            public string Name { get; set; }
            public List<int> BossDamages { get; set; } = new List<int>();
        }

        public PersonalDamageControl()
        {
            InitializeComponent();
        }

        public PersonalDamageControl(StatsVm stats, IChartProvider chartProvider) : this()
        {
            InitModels(stats, chartProvider);
        }

        public StatsVm Stats { get; private set; }
        public IChartProvider ChartProvider { get; private set; }
        public void InitModels(StatsVm stats, IChartProvider chartProvider)
        {
            Stats = stats;
            ChartProvider = chartProvider;
            CycleButtons.ItemsSource = Enumerable.Range(1, stats.CycleCount);
            var dates = Stats.ApiObj.Challenges
                .GroupBy(k => k.ChallengeTime.AddHours(-5).Date)
                .Select(k => k.Key)
                .ToList();
            DayButtons.ItemsSource = dates;
        }

        private async void BtnCyclePersonalDamage_Click(object sender, RoutedEventArgs e)
        {
            await CyclePersonalDamage();
        }

        private async Task CyclePersonalDamage()
        {
            Stats.IsLoading = true;
            ChartProvider.RecreateGraph();
            var totalDamageTrend = Stats.ApiObj.Challenges
                .Select(k => k.Clone())
                .GroupBy(k => k.Cycle).ToList()[Stats.SelectedCycle - 1];
            var challengeModels = totalDamageTrend.ToList();
            var personsDic = challengeModels.GroupBy(k => k.QqId)
                .ToDictionary(k => k.Key,
                    k =>
                    {
                        var dic = new Dictionary<int, ChallengeModel>();
                        foreach (var challengeModel in k)
                        {
                            if (dic.ContainsKey(challengeModel.BossNum))
                            {
                                dic[challengeModel.BossNum].Damage += challengeModel.Damage;
                            }
                            else
                            {
                                dic.Add(challengeModel.BossNum, challengeModel);
                            }
                        }

                        return dic;
                    });
            var list = new List<CyclePersonalDamageModel>();
            foreach (var kvp in personsDic)
            {
                var cycleModel = new CyclePersonalDamageModel
                {
                    Name = $"{await QQService.GetQqNickNameAsync(kvp.Key)} ({kvp.Key})"
                };
                for (int i = 0; i < 5; i++)
                {
                    cycleModel.BossDamages.Add((int)(kvp.Value.ContainsKey(cycleModel.BossDamages.Count + 1)
                        ? kvp.Value[cycleModel.BossDamages.Count + 1].Damage
                        : 0L));
                }

                list.Add(cycleModel);
            }

            list = list.OrderBy(k => k.BossDamages.Sum()).ToList();

            var vm = new StatsGraphVm
            {
                AxisYLabels = list.Select(k => k.Name).ToArray(),
                AxisXTitle = "伤害",
                AxisYTitle = "成员",
                Title = Stats.SelectedCycle + "周目个人伤害统计"
            };

            for (int i = 0; i < 5; i++)
            {
                var i1 = i;
                vm.SeriesCollection.Add(new StackedRowSeries
                {
                    Title = "BOSS " + (i + 1),
                    Values = new ChartValues<int>(list.Select(k => k.BossDamages[i1])),
                    DataLabels = true
                });
            }

            Stats.StatsGraph = vm;

            ChartProvider.Chart.AxisY[0].Separator = new LiveCharts.Wpf.Separator { Step = 1 };

            Stats.IsLoading = false;
        }

        private async void BtnCyclePersonalDamageChangeCycle_Click(object sender, RoutedEventArgs e)
        {
            Stats.SelectedCycle = (int)((Button)sender).Tag;
            await CyclePersonalDamage();
        }

        private async void BtnTotalPersonalDamage_Click(object sender, RoutedEventArgs e)
        {
            Stats.IsLoading = true;
            ChartProvider.RecreateGraph();

            var dic = Stats.ApiObj.Challenges
                .Select(k => k.Clone())
                .GroupBy(k => k.QqId)
                .ToDictionary(k => k.Key,
                    k => k.Sum(o => o.Damage))
                .OrderBy(k => k.Value)
                .ToDictionary(k => k.Key,
                    k => k.Value);

            var list = new List<string>();
            foreach (var dicKey in dic.Keys)
            {
                var nick = $"{await QQService.GetQqNickNameAsync(dicKey)} ({dicKey})";
                list.Add(nick);
            }

            var vm = new StatsGraphVm
            {
                AxisYLabels = list.ToArray(),
                AxisYTitle = "成员",
                AxisXTitle = "伤害",
                Title = "个人总伤害排名"
            };

            vm.SeriesCollection.Add(new RowSeries()
            {
                Title = "伤害",
                Values = new ChartValues<int>(dic.Values),
                DataLabels = true
            });

            Stats.StatsGraph = vm;

            ChartProvider.Chart.AxisY[0].Separator = new LiveCharts.Wpf.Separator { Step = 1 };
            ChartProvider.Chart.AxisX[0].LabelFormatter = value => value.ToString("N0");

            Stats.IsLoading = false;
        }

        private async void BtnTotalPersonalDamageMultiBoss_Click(object sender, RoutedEventArgs e)
        {
            Stats.IsLoading = true;
            ChartProvider.RecreateGraph();
            var challengeModels = Stats.ApiObj.Challenges.Select(k => k.Clone()).ToList();
            var personsDic = challengeModels.GroupBy(k => k.QqId)
                .ToDictionary(k => k.Key,
                    k =>
                    {
                        var dic = new Dictionary<int, ChallengeModel>();
                        foreach (var challengeModel in k)
                        {
                            if (dic.ContainsKey(challengeModel.BossNum))
                            {
                                dic[challengeModel.BossNum].Damage += challengeModel.Damage;
                            }
                            else
                            {
                                dic.Add(challengeModel.BossNum, challengeModel);
                            }
                        }

                        return dic;
                    });
            var list = new List<CyclePersonalDamageModel>();
            foreach (var kvp in personsDic)
            {
                var cycleModel = new CyclePersonalDamageModel
                {
                    Name = $"{await QQService.GetQqNickNameAsync(kvp.Key)} ({kvp.Key})"
                };
                for (int i = 0; i < 5; i++)
                {
                    cycleModel.BossDamages.Add((int)(kvp.Value.ContainsKey(cycleModel.BossDamages.Count + 1)
                        ? kvp.Value[cycleModel.BossDamages.Count + 1].Damage
                        : 0L));
                }

                list.Add(cycleModel);
            }

            list = list.OrderBy(k => k.BossDamages.Sum()).ToList();

            var vm = new StatsGraphVm
            {
                AxisYLabels = list.Select(k => k.Name).ToArray(),
                AxisXTitle = "伤害",
                AxisYTitle = "成员",
                Title = Stats.SelectedCycle + "周目个人伤害统计"
            };

            for (int i = 0; i < 5; i++)
            {
                var i1 = i;
                vm.SeriesCollection.Add(new StackedRowSeries
                {
                    Title = "BOSS " + (i + 1),
                    Values = new ChartValues<int>(list.Select(k => k.BossDamages[i1])),
                    DataLabels = true
                });
            }

            Stats.StatsGraph = vm;

            ChartProvider.Chart.AxisY[0].Separator = new LiveCharts.Wpf.Separator { Step = 1 };
            ChartProvider.Chart.AxisX[0].LabelFormatter = value => value.ToString("N0");

            Stats.IsLoading = false;
        }

        private async void BtnDayPersonalDamage_Click(object sender, RoutedEventArgs e)
        {
            await DayPersonalDamage();
        }

        private async void BtnDayPersonalDamageChangeDay_Click(object sender, RoutedEventArgs e)
        {
            Stats.SelectedDay = (DateTime?)((Button)sender).Tag;
            await DayPersonalDamage();
        }

        private async Task DayPersonalDamage()
        {
            if (Stats.SelectedDay is null) return;
            var selectedDay = Stats.SelectedDay.Value;
            Stats.IsLoading = true;
            ChartProvider.RecreateGraph();
            var totalDamageTrend = Stats.ApiObj.Challenges
                .Select(k => k.Clone())
                .GroupBy(k => k.ChallengeTime.AddHours(-5).Date)
                .ToDictionary(k => k.Key, k => k.ToList())
                [selectedDay.Date];

            var challengeModels = totalDamageTrend.ToList();
            var personsDic = challengeModels.GroupBy(k => k.QqId)
                .ToDictionary(k => k.Key,
                    k => k.ToList());
            var list = new List<DayPersonalDamageModel>();
            foreach (var kvp in personsDic)
            {
                var cycleModel = new DayPersonalDamageModel
                {
                    Name = $"{await QQService.GetQqNickNameAsync(kvp.Key)} ({kvp.Key})",
                    TimeDamages = new List<int> { 0, 0, 0, 0, 0, 0 }
                };

                int i = 0;
                foreach (var challengeModel in kvp.Value)
                {
                    if (challengeModel.IsContinue)
                    {
                        if (i % 2 == 0) i++;
                        cycleModel.TimeDamages[i] = challengeModel.Damage;
                    }
                    else
                    {
                        if (i % 2 != 0) i++;
                        cycleModel.TimeDamages[i] = challengeModel.Damage;
                    }

                    i++;
                }

                list.Add(cycleModel);
            }

            list = list.OrderBy(k => k.TimeDamages.Sum()).ToList();

            var vm = new StatsGraphVm
            {
                AxisYLabels = list.Select(k => k.Name).ToArray(),
                AxisXTitle = "伤害",
                AxisYTitle = "成员",
                Title = selectedDay.Date.ToShortDateString() + "个人伤害统计"
            };

            for (int i = 0; i < 6; i++)
            {
                var i1 = i;
                vm.SeriesCollection.Add(new StackedRowSeries
                {
                    Title = i % 2 == 0 ? "第" + (i / 2 + 1) + "刀" : "第" + (i / 2 + 1) + "刀（补偿刀）",
                    Values = new ChartValues<int>(list.Select(k => k.TimeDamages[i1])),
                    DataLabels = true
                });
            }

            Stats.StatsGraph = vm;

            ChartProvider.Chart.AxisY[0].Separator = new LiveCharts.Wpf.Separator { Step = 1 };

            Stats.IsLoading = false;
        }
    }
}
