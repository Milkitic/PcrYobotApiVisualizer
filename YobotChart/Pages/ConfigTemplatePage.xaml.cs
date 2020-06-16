using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using YobotChart.Shared.Win32.ChartFramework;
using YobotChart.Shared.Win32.ChartFramework.SourceProviders;
using YobotChart.Shared.Win32.ChartFramework.StatsProviders;
using YobotChart.UiComponents;

namespace YobotChart.Pages
{
    /// <summary>
    /// ConfigTemplatePage.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigTemplatePage : Page
    {
        private readonly StatsViewModel _statsVm;
        private bool _canCbTrigger;

        public ConfigTemplatePage(StatsViewModel statsVm)
        {
            InitializeComponent();
            _statsVm = statsVm;
            DataContext = statsVm;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _canCbTrigger = false;
            _statsVm.InitProvider();
            var dateList = new DateTime?[] { null }.Concat(YobotApiSource.Default.DateList.Cast<DateTime?>()).ToList();
            CbStartDate.ItemsSource = dateList;
            CbEndDate.ItemsSource = dateList;
            CbStartDate.SelectedIndex = 1;
            CbEndDate.SelectedIndex = dateList.Count - 1;

            var roundList = new int?[] { null }.Concat(YobotApiSource.Default.RoundList.Cast<int?>()).ToList();
            CbStartRound.ItemsSource = roundList;
            CbEndRound.ItemsSource = roundList;
            CbStartRound.SelectedIndex = 1;
            CbEndRound.SelectedIndex = roundList.Count - 1;

            CbWidth.ItemsSource = Enumerable.Range(1, 4);
            CbHeight.ItemsSource = Enumerable.Range(1, 4);
            CbWidth.SelectedIndex = 0;
            CbHeight.SelectedIndex = 0;

            await UpdateGranularity();
            _canCbTrigger = true;
        }

        private async Task UpdateGranularity()
        {
            var phase = YobotApiSource.Default.PhaseList.Last();
            var acceptGranularities = _statsVm.SourceStatsFunction.AcceptGranularities;
            if (acceptGranularities.Contains(GranularityType.Total))
            {
                DateAdjust.Visibility = Visibility.Collapsed;
                RoundAdjust.Visibility = Visibility.Collapsed;
                _statsVm.GranularityModel = new GranularityModel(phase);
            }
            else
            {
                if (!acceptGranularities.Contains(GranularityType.SingleDate) &&
                    !acceptGranularities.Contains(GranularityType.MultiDate))
                {
                    DateAdjust.Visibility = Visibility.Collapsed;
                }
                else
                {
                    var startDate = (DateTime?)CbStartDate.SelectedItem;
                    var endDate = (DateTime?)CbEndDate.SelectedItem;
                    if (!acceptGranularities.Contains(GranularityType.SingleDate))
                    {
                        _statsVm.GranularityModel = new GranularityModel
                        {
                            GranularityType = GranularityType.MultiDate,
                            StartDate = startDate,
                            EndDate = endDate
                        };
                    }
                    else if (!acceptGranularities.Contains(GranularityType.MultiDate))
                    {
                        _statsVm.GranularityModel = new GranularityModel
                        {
                            GranularityType = GranularityType.SingleDate,
                            SelectedDate = startDate
                        };
                    }
                    else
                    {
                        if (startDate == endDate)
                        {
                            _statsVm.GranularityModel = new GranularityModel
                            {
                                GranularityType = GranularityType.SingleDate,
                                SelectedDate = startDate
                            };
                        }
                        else
                        {
                            _statsVm.GranularityModel = new GranularityModel
                            {
                                GranularityType = GranularityType.MultiDate,
                                StartDate = startDate,
                                EndDate = endDate
                            };
                        }
                    }
                }

                if (!acceptGranularities.Contains(GranularityType.SingleRound) &&
                    !acceptGranularities.Contains(GranularityType.MultiRound))
                {
                    RoundAdjust.Visibility = Visibility.Collapsed;
                }
                else
                {
                    var startRound = (int?)CbStartRound.SelectedItem;
                    var endRound = (int?)CbEndRound.SelectedItem;
                    if (!acceptGranularities.Contains(GranularityType.SingleRound))
                    {
                        _statsVm.GranularityModel = new GranularityModel
                        {
                            GranularityType = GranularityType.MultiRound,
                            StartRound = startRound,
                            EndRound = endRound
                        };
                    }
                    else if (!acceptGranularities.Contains(GranularityType.MultiRound))
                    {
                        _statsVm.GranularityModel = new GranularityModel
                        {
                            GranularityType = GranularityType.SingleRound,
                            SelectedRound = startRound
                        };
                    }
                    else
                    {
                        if (startRound == endRound)
                        {
                            _statsVm.GranularityModel = new GranularityModel
                            {
                                GranularityType = GranularityType.SingleRound,
                                SelectedRound = startRound
                            };
                        }
                        else
                        {
                            _statsVm.GranularityModel = new GranularityModel
                            {
                                GranularityType = GranularityType.MultiRound,
                                StartRound = startRound,
                                EndRound = endRound
                            };
                        }
                    }
                }
            }

            await TemplateControl.UpdateGraph();
        }

        private async void RoundAdjust_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_canCbTrigger) return;
            await UpdateGranularity();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var page = SingletonPageHelper.Get<DashBoardPage>();
            page.AddStatsViewModelAndSave(_statsVm);
            AnimatedFrame.Default?.AnimateNavigate(page);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            var animatedFrame = AnimatedFrame.Default;
            if (animatedFrame != null)
            {
                var stack = animatedFrame.BackStack;
                if (stack != null)
                {
                    AnimatedFrame.Default?.AnimateNavigateBack();
                }
            }
        }
    }
}
