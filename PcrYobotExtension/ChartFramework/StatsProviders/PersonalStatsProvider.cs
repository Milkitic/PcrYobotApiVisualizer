using LiveCharts;
using LiveCharts.Wpf;
using PcrYobotExtension.Models;
using PcrYobotExtension.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PcrYobotExtension.ViewModels;

namespace PcrYobotExtension.ChartFramework.StatsProviders
{
    [StatsProviderMetadata("9b3a41ae-1ac3-4fad-84ec-e8b26164e58a", Author = "yf_extension", Name = "个人")]
    public class PersonalStatsProvider : IStatsProvider
    {
        public ChallengeModel[] Challenges { get; set; }

        [StatsMethod("个人每日刀伤横向比较")]
        [StatsMethodAcceptGranularity(GranularityType.SingleDate, GranularityType.SingleRound)]
        public async Task<CartesianChartConfigModel> PersonalDivideByChallengeTimesDayComparision(GranularityModel granularity)
        {
            if (granularity.SelectedDate is null)
                throw new Exception("请选择日期");
            var personsDic = GetPersonalDictionary(granularity, out var titlePrefix);

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

            var configModel = new CartesianChartConfigModel
            {
                AxisYLabels = list.Select(k => k.Name).ToArray(),
                AxisXTitle = "伤害",
                AxisYTitle = "成员",
                Title = titlePrefix + "个人伤害统计"
            };

            for (int i = 0; i < 6; i++)
            {
                var i1 = i;
                configModel.SeriesCollection.Add(new StackedRowSeries
                {
                    Title = i % 2 == 0 ? "第" + (i / 2 + 1) + "刀" : "第" + (i / 2 + 1) + "刀（补偿刀）",
                    Values = new ChartValues<int>(list.Select(k => k.TimeDamages[i1])),
                    DataLabels = true
                });
            }

            configModel.ChartConfig = chart => { chart.AxisY[0].Separator = new Separator { Step = 1 }; };

            return configModel;
        }

        [StatsMethod("个人每日Boss伤害横向比较")]
        public async Task<CartesianChartConfigModel> PersonalDivideByBossDayComparision(GranularityModel granularity)
        {
            if (granularity.SelectedDate is null)
                throw new Exception("请选择日期");

            var personsDic = GetPersonalDictionary(granularity, out var titlePrefix);

            var personBossDic = personsDic.ToDictionary(k => k.Key,
                k =>
                {
                    var dic = new Dictionary<int, int>();
                    foreach (var challengeModel in k.Value)
                    {
                        if (dic.ContainsKey(challengeModel.BossNum))
                        {
                            dic[challengeModel.BossNum] += challengeModel.Damage;
                        }
                        else
                        {
                            dic.Add(challengeModel.BossNum, challengeModel.Damage);
                        }
                    }

                    return dic;
                });

            var list = new List<CyclePersonalDamageModel>();
            foreach (var kvp in personBossDic)
            {
                var cycleModel = new CyclePersonalDamageModel
                {
                    Name = $"{await QQService.GetQqNickNameAsync(kvp.Key)} ({kvp.Key})"
                };

                for (int i = 0; i < 5; i++)
                {
                    cycleModel.BossDamages.Add((int)(kvp.Value.ContainsKey(cycleModel.BossDamages.Count + 1)
                        ? kvp.Value[cycleModel.BossDamages.Count + 1]
                        : 0L));
                }

                list.Add(cycleModel);
            }

            list = list.OrderBy(k => k.BossDamages.Sum()).ToList();


            var configModel = new CartesianChartConfigModel
            {
                AxisYLabels = list.Select(k => k.Name).ToArray(),
                AxisXTitle = "伤害",
                AxisYTitle = "成员",
                Title = titlePrefix + "个人伤害统计"
            };

            for (int i = 0; i < 5; i++)
            {
                var i1 = i;
                configModel.SeriesCollection.Add(new StackedRowSeries
                {
                    Title = "BOSS " + (i + 1),
                    Values = new ChartValues<int>(list.Select(k => k.BossDamages[i1])),
                    DataLabels = true
                });
            }

            configModel.ChartConfig = chart => { chart.AxisY[0].Separator = new Separator { Step = 1 }; };

            return configModel;
        }

        private Dictionary<long, List<ChallengeModel>> GetPersonalDictionary(GranularityModel granularity, out string titlePrefix)
        {
            switch (granularity.GranularityType)
            {
                case GranularityType.SingleDate:
                case GranularityType.MultiDate:
                    {
                        var grouped = Challenges
                            .Select(k => k.Clone())
                            .GroupBy(k => k.ChallengeTime.AddHours(-5).Date);
                        List<ChallengeModel> challengeModels;
                        if (granularity.GranularityType == GranularityType.SingleDate && granularity.SelectedDate != null)
                        {
                            var selectedDay = granularity.SelectedDate.Value;
                            var singleDateData = grouped.First(k => k.Key == selectedDay.Date);
                            challengeModels = singleDateData.ToList();

                            titlePrefix = selectedDay.ToString("M月d日");
                        }
                        else if (granularity.GranularityType == GranularityType.MultiDate &&
                                 granularity.StartDate != null &&
                                 granularity.EndDate != null)
                        {
                            var startDate = granularity.StartDate.Value;
                            var endDate = granularity.EndDate.Value;
                            var multiDateData = grouped
                                .Where(k => k.Key >= startDate && k.Key <= endDate);
                            challengeModels = multiDateData.SelectMany(k => k).ToList();
                            titlePrefix = $"{startDate:M月d日} - {endDate:M月d日}";
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException();
                        }

                        var personsDic = challengeModels.GroupBy(k => k.QqId)
                            .ToDictionary(k => k.Key,
                                k => k.ToList());
                        return personsDic;
                    }
                case GranularityType.Total:
                    break;
                case GranularityType.SingleRound:
                    break;
                case GranularityType.MultiRound:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new NotImplementedException();
        }

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
    }

    public class StatsMethodAcceptGranularityAttribute : Attribute
    {
        public GranularityType[] Types { get; }

        public StatsMethodAcceptGranularityAttribute(params GranularityType[] types)
        {
            Types = types;
        }
    }
}