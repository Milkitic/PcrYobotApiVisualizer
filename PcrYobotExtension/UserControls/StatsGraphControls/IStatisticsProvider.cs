using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using PcrYobotExtension.Models;
using PcrYobotExtension.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PcrYobotExtension.UserControls.StatsGraphControls
{
    [StatisticsProviderMetadata("9b3a41ae-1ac3-4fad-84ec-e8b26164e58a", Author = "yf_extension", Name = "个人")]
    public class PersonalStatisticsProvider : IStatisticsProvider
    {
        internal class DayPersonalDamageModel
        {
            public string Name { get; set; }
            public List<int> TimeDamages { get; set; } = new List<int>();
        }

        public ChallengeModel[] Challenges { get; set; }

        [StatisticsMethod("个人每日刀伤横向比较")]
        public async Task<CartesianChartConfigModel> Get(GranularityModel granularity)
        {
            if (granularity.SelectedDate is null)
                throw new Exception("请选择日期");
            var selectedDay = granularity.SelectedDate.Value;
            ;
            var totalDamageTrend = Challenges
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

            var configModel = new CartesianChartConfigModel
            {
                AxisYLabels = list.Select(k => k.Name).ToArray(),
                AxisXTitle = "伤害",
                AxisYTitle = "成员",
                Title = selectedDay.Date.ToShortDateString() + "个人伤害统计"
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

        [StatisticsMethod("个人每日Boss伤害横向比较")]
        public CartesianChartConfigModel Get2(GranularityModel granularity)
        {
            var o = new CartesianChartConfigModel();
            o.ChartConfig = (e) => { };
            o.Title = "ss";
            return o;
        }
    }

    public class StatisticsMethodAttribute : Attribute
    {
        public StatisticsMethodAttribute(string name)
        {
            Name = name;
        }

        public Guid Guid { get; } = Guid.NewGuid();
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            return obj is StatisticsMethodAttribute attr ? Equals(attr) : base.Equals(obj);
        }

        protected bool Equals(StatisticsMethodAttribute other)
        {
            return Guid.Equals(other.Guid);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }
    }


    public interface IStatisticsProvider
    {
        /// <summary>
        /// 数据源
        /// </summary>
        ChallengeModel[] Challenges { get; set; }
    }

    public static class ChartConfigModelHelper
    {
        public static readonly Type CartesianType = typeof(CartesianChart);
        public static readonly Type PieType = typeof(PieChart);
    }

    public class CartesianChartConfigModel : ChartConfigModel<CartesianChart>
    {
        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();

        public string[] AxisXLabels { get; set; }

        public string AxisXTitle { get; set; }

        public string AxisYTitle { get; set; }

        public string[] AxisYLabels { get; set; }
    }

    public interface IChartConfigModel
    {
        ChartType ChartType { get; }
        string Title { get; set; }
        Action<Chart> ChartConfig { get; set; }
    }

    public abstract class ChartConfigModel<T> : IChartConfigModel where T : Chart
    {
        private static readonly Type GenericType = typeof(T);
        public ChartType ChartType { get; }

        public ChartConfigModel()
        {
            if (GenericType == ChartConfigModelHelper.CartesianType ||
                GenericType.IsSubclassOf(ChartConfigModelHelper.CartesianType))
            {
                ChartType = ChartType.Cartesian;
            }
            else if (GenericType == ChartConfigModelHelper.PieType ||
                     GenericType.IsSubclassOf(ChartConfigModelHelper.PieType))
            {
                ChartType = ChartType.Pie;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(ChartType), @"unsupported chart type");
            }
        }

        public string Title { get; set; }

        public Action<T> ChartConfig { get; set; }

        Action<Chart> IChartConfigModel.ChartConfig
        {
            get => o => ChartConfig?.Invoke((T)o);
            set => ChartConfig = value;
        }
    }

    public class GranularityModel
    {
        public GranularityModel(int phase, IList<string> userIds = null)
        {
            SelectedPhase = phase;
            GranularityType = GranularityType.Total;
            SelectedUserIds = new ReadOnlyCollection<string>(userIds ?? Array.Empty<string>());
        }

        public GranularityModel(int phase, int round, IList<string> userIds = null)
        {
            SelectedPhase = phase;
            SelectedRound = round;
            GranularityType = GranularityType.SingleRound;
            SelectedUserIds = new ReadOnlyCollection<string>(userIds ?? Array.Empty<string>());
        }

        public GranularityModel(int phase, DateTime date, IList<string> userIds = null)
        {
            SelectedPhase = phase;
            SelectedDate = date;
            GranularityType = GranularityType.SingleDate;
            SelectedUserIds = new ReadOnlyCollection<string>(userIds ?? Array.Empty<string>());
        }

        public GranularityModel(int phase, int startRound, int endRound, IList<string> userIds = null)
        {
            SelectedPhase = phase;
            StartRound = startRound;
            EndRound = endRound;
            GranularityType = GranularityType.MultiRound;
            SelectedUserIds = new ReadOnlyCollection<string>(userIds ?? Array.Empty<string>());
        }

        public GranularityModel(int phase, DateTime startDate, DateTime endDate, IList<string> userIds = null)
        {
            SelectedPhase = phase;
            StartDate = startDate;
            EndDate = endDate;
            GranularityType = GranularityType.MultiDate;
            SelectedUserIds = new ReadOnlyCollection<string>(userIds ?? Array.Empty<string>());
        }

        public GranularityType GranularityType { get; }

        public int SelectedPhase { get; }

        public int? SelectedRound { get; }
        public DateTime? SelectedDate { get; }

        public int? StartRound { get; }
        public int? EndRound { get; }

        public DateTime? StartDate { get; }
        public DateTime? EndDate { get; }

        public ReadOnlyCollection<string> SelectedUserIds { get; }
    }

    public enum GranularityType
    {
        Total,
        SingleRound,
        MultiRound,
        SingleDate,
        MultiDate
    }

    public enum ChartType
    {
        Cartesian,
        Pie
    }

    public class StatisticsProviderMetadataAttribute : Attribute
    {
        public StatisticsProviderMetadataAttribute(string guid)
        {
            Guid = Guid.Parse(guid);
        }

        public string Name { get; set; }
        public string Author { get; set; }
        public Version Version { get; set; }
        public Guid Guid { get; }

        public override bool Equals(object obj)
        {
            return obj is StatisticsProviderMetadataAttribute attr ? Equals(attr) : ReferenceEquals(this, obj);
        }

        protected bool Equals(StatisticsProviderMetadataAttribute other)
        {
            return Guid.Equals(other.Guid);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }
    }

    public class StatisticsProviderInfo
    {
        public StatisticsProviderMetadataAttribute Metadata { get; set; }

        public Dictionary<StatisticsMethodAttribute, Func<GranularityModel, Task<IChartConfigModel>>> FunctionsMapping
        {
            get;
            set;
        } = new Dictionary<StatisticsMethodAttribute, Func<GranularityModel, Task<IChartConfigModel>>>();
    }
}