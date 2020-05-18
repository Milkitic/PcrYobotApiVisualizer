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

namespace PcrYobotExtension.UserControls.StatsGraphControls
{
    /// <summary>
    /// PersonalDamageControl.xaml 的交互逻辑
    /// </summary>
    public partial class PersonalDamageControl : UserControl, IChartSwitchControl
    {
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
    }
}
