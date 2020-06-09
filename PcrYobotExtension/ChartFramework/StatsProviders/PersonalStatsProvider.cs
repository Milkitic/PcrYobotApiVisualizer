using LiveCharts;
using LiveCharts.Wpf;
using PcrYobotExtension.Annotations;
using PcrYobotExtension.Models;
using PcrYobotExtension.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PcrYobotExtension.ChartFramework.StatsProviders
{
    [StatsProviderMetadata("9b3a41ae-1ac3-4fad-84ec-e8b26164e58a", Author = "yf_extension", Name = "个人")]
    public class PersonalStatsProvider : IStatsProvider
    {
        public ChallengeModel[] Challenges { get; set; }

        [StatsMethod("个人每日刀伤横向比较")]
        [StatsMethodAcceptGranularity(GranularityType.SingleDate, GranularityType.MultiDate)]
        [UsedImplicitly]
        public async Task<CartesianChartConfigModel> PersonalDivideByChallengeTimesDayComparision(
            GranularityModel granularity)
        {
            if (granularity.SelectedDate is null &&
                granularity.StartDate is null &&
                granularity.EndDate is null)
                throw new Exception("未指定日期");

            var personsGrouping = GetPersonalDictionary(granularity, out var titlePrefix);

            var list = new List<PersonalDamageModel>();
            foreach (var kvp in personsGrouping)
            {
                var qqId = kvp.Key;

                var byDate = kvp.GroupBy(k => k.ChallengeTime.Date);
                // multi date support

                var damageModel = new PersonalDamageModel
                {
                    Name = $"{await QQService.GetQqNickNameAsync(qqId)} ({qqId})",
                    Damages = new List<int> { 0, 0, 0, 0, 0, 0 }
                };

                foreach (var grouping in byDate)
                {
                    int i = 0;
                    foreach (var challengeModel in grouping)
                    {
                        if (challengeModel.IsContinue)
                        {
                            if (i % 2 == 0) i++;
                            damageModel.Damages[i] += challengeModel.Damage;
                        }
                        else
                        {
                            if (i % 2 != 0) i++;
                            damageModel.Damages[i] += challengeModel.Damage;
                        }

                        i++;
                    }
                }

                list.Add(damageModel);
            }

            list = list.OrderBy(k => k.Damages.Sum()).ToList();

            var configModel = GetSharedConfigModel(titlePrefix, list);

            for (int i = 0; i < 6; i++)
            {
                var challengeIndex = i / 2 + 1;
                configModel.SeriesCollection.Add(new StackedRowSeries
                {
                    Title = i % 2 == 0 ? $"第{challengeIndex}刀" : $"第{challengeIndex}刀（补偿刀）",
                    Values = new ChartValues<int>(list.Select(k => k.Damages[i]).ToList()),
                    DataLabels = true
                });
            }

            ConfigStepOne(configModel);

            return configModel;
        }

        [StatsMethod("个人每日Boss伤害横向比较")]
        [StatsMethodAcceptGranularity(GranularityType.SingleDate, GranularityType.MultiDate)]
        [UsedImplicitly]
        public async Task<CartesianChartConfigModel> PersonalDivideByBossDayComparision(GranularityModel granularity)
        {
            if (granularity.SelectedDate is null &&
                granularity.StartDate is null &&
                granularity.EndDate is null)
                throw new Exception("未指定日期");

            var personsGrouping = GetPersonalDictionary(granularity, out var titlePrefix);

            var personBossDic = personsGrouping.ToDictionary(k => k.Key,
                k =>
                {
                    var dic = new Dictionary<int, int>();
                    foreach (var challengeModel in k)
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

            var list = new List<PersonalDamageModel>();
            foreach (var kvp in personBossDic)
            {
                var cycleModel = new PersonalDamageModel
                {
                    Name = $"{await QQService.GetQqNickNameAsync(kvp.Key)} ({kvp.Key})"
                };

                for (int i = 0; i < 5; i++)
                {
                    var bossNum = cycleModel.Damages.Count + 1;
                    var bossDamage = kvp.Value.ContainsKey(bossNum) ? kvp.Value[bossNum] : 0L;
                    cycleModel.Damages.Add((int)bossDamage);
                }

                list.Add(cycleModel);
            }

            list = list.OrderBy(k => k.Damages.Sum()).ToList();

            var configModel = GetSharedConfigModel(titlePrefix, list);

            for (int i = 0; i < 5; i++)
            {
                configModel.SeriesCollection.Add(new StackedRowSeries
                {
                    Title = $"BOSS {i + 1}",
                    Values = new ChartValues<int>(list.Select(k => k.Damages[i]).ToList()),
                    DataLabels = true
                });
            }

            ConfigStepOne(configModel);

            return configModel;
        }

        [StatsMethod("个人周目刀伤横向比较")]
        [StatsMethodAcceptGranularity(GranularityType.SingleRound, GranularityType.MultiRound)]
        [UsedImplicitly]
        public async Task<CartesianChartConfigModel> PersonalDivideByChallengeTimesRoundComparision(
            GranularityModel granularity)
        {
            if (granularity.SelectedRound is null &&
                granularity.StartRound is null &&
                granularity.EndRound is null)
                throw new Exception("未指定周目");

            var personsGrouping = GetPersonalDictionary(granularity, out var titlePrefix);

            var list = new List<PersonalDamageModel>();
            foreach (var kvp in personsGrouping)
            {
                var qqId = kvp.Key;

                var byDate = kvp.GroupBy(k => k.ChallengeTime.AddHours(-5).Date);
                // multi date support

                var damageModel = new PersonalDamageModel
                {
                    Name = $"{await QQService.GetQqNickNameAsync(qqId)} ({qqId})",
                    Damages = new List<int> { 0, 0, 0, 0, 0, 0 }
                };

                foreach (var grouping in byDate)
                {
                    int i = 0;
                    var all = grouping.ToList();
                    foreach (var challengeModel in all)
                    {
                        if (challengeModel.IsContinue)
                        {
                            if (i % 2 == 0) i++;
                            damageModel.Damages[i] += challengeModel.Damage;
                        }
                        else
                        {
                            if (i % 2 != 0) i++;
                            damageModel.Damages[i] += challengeModel.Damage;
                        }

                        i++;
                    }
                }

                list.Add(damageModel);
            }

            list = list.OrderBy(k => k.Damages.Sum()).ToList();

            var configModel = GetSharedConfigModel(titlePrefix, list);

            for (int i = 0; i < 6; i++)
            {
                var challengeIndex = i / 2 + 1;
                configModel.SeriesCollection.Add(new StackedRowSeries
                {
                    Title = i % 2 == 0 ? $"第{challengeIndex}刀" : $"第{challengeIndex}刀（补偿刀）",
                    Values = new ChartValues<int>(list.Select(k => k.Damages[i]).ToList()),
                    DataLabels = true
                });
            }

            ConfigStepOne(configModel);

            return configModel;
        }

        [StatsMethod("个人周目Boss伤害横向比较")]
        [StatsMethodAcceptGranularity(GranularityType.SingleRound, GranularityType.MultiRound)]
        [UsedImplicitly]
        public async Task<CartesianChartConfigModel> PersonalDivideByBossRoundComparision(GranularityModel granularity)
        {
            if (granularity.SelectedRound is null &&
                granularity.StartRound is null &&
                granularity.EndRound is null)
                throw new Exception("未指定周目");

            var personsGrouping = GetPersonalDictionary(granularity, out var titlePrefix);

            var personBossDic = personsGrouping.ToDictionary(k => k.Key,
                k =>
                {
                    var dic = new Dictionary<int, int>();
                    foreach (var challengeModel in k)
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

            var list = new List<PersonalDamageModel>();
            foreach (var kvp in personBossDic)
            {
                var cycleModel = new PersonalDamageModel
                {
                    Name = $"{await QQService.GetQqNickNameAsync(kvp.Key)} ({kvp.Key})"
                };

                for (int i = 0; i < 5; i++)
                {
                    var bossNum = cycleModel.Damages.Count + 1;
                    var bossDamage = kvp.Value.ContainsKey(bossNum) ? kvp.Value[bossNum] : 0L;
                    cycleModel.Damages.Add((int)bossDamage);
                }

                list.Add(cycleModel);
            }

            list = list.OrderBy(k => k.Damages.Sum()).ToList();

            var configModel = GetSharedConfigModel(titlePrefix, list);

            for (int i = 0; i < 5; i++)
            {
                configModel.SeriesCollection.Add(new StackedRowSeries
                {
                    Title = $"BOSS {i + 1}",
                    Values = new ChartValues<int>(list.Select(k => k.Damages[i]).ToList()),
                    DataLabels = true
                });
            }

            ConfigStepOne(configModel);

            return configModel;
        }

        private IEnumerable<IGrouping<long, ChallengeModel>> GetPersonalDictionary(GranularityModel granularity,
            out string titlePrefix)
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

                        var personsDic = challengeModels.GroupBy(k => k.QqId);
                        return personsDic;
                    }
                case GranularityType.Total:
                    break;
                case GranularityType.SingleRound:
                case GranularityType.MultiRound:
                    {
                        var grouped = Challenges
                            .Select(k => k.Clone())
                            .GroupBy(k => k.Cycle);
                        List<ChallengeModel> challengeModels;
                        if (granularity.GranularityType == GranularityType.SingleRound &&
                            granularity.SelectedRound != null)
                        {
                            var selectedRound = granularity.SelectedRound.Value;
                            var singleDateData = grouped.First(k => k.Key == selectedRound);
                            challengeModels = singleDateData.ToList();

                            titlePrefix = selectedRound + "周目";
                        }
                        else if (granularity.GranularityType == GranularityType.MultiRound &&
                                 granularity.StartRound != null &&
                                 granularity.EndRound != null)
                        {
                            var startRound = granularity.StartRound.Value;
                            var endRound = granularity.EndRound.Value;
                            var multiDateData = grouped
                                .Where(k => k.Key >= startRound && k.Key <= endRound);
                            challengeModels = multiDateData.SelectMany(k => k).ToList();
                            titlePrefix = $"{startRound}周目 - {endRound}周目";
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException();
                        }

                        var personsDic = challengeModels.GroupBy(k => k.QqId);
                        return personsDic;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new NotImplementedException();
        }

        private static CartesianChartConfigModel GetSharedConfigModel(string titlePrefix,
            IEnumerable<PersonalDamageModel> damages)
        {
            return new CartesianChartConfigModel
            {
                AxisYLabels = damages.Select(k => k.Name).ToArray(),
                AxisXTitle = "伤害",
                AxisYTitle = "成员",
                Title = titlePrefix + "个人伤害统计"
            };
        }

        private static void ConfigStepOne(CartesianChartConfigModel configModel)
        {
            configModel.ChartConfig = chart => { chart.AxisY[0].Separator = new Separator { Step = 1 }; };
        }

        internal class PersonalDamageModel
        {
            public string Name { get; set; }
            public List<int> Damages { get; set; } = new List<int>();
        }
    }
}