using System.Linq;
using System.Windows;
using System.Windows.Controls;
using YobotChart.Shared.Win32.ChartFramework;
using YobotChart.Shared.Win32.ChartFramework.SourceProviders;
using YobotChart.Shared.Win32.ChartFramework.StatsProviders;

namespace YobotChart.Pages
{
    /// <summary>
    /// ConfigTemplatePage.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigTemplatePage : Page
    {
        private readonly StatsViewModel _statsVm;

        public ConfigTemplatePage(StatsViewModel statsVm)
        {
            InitializeComponent();
            _statsVm = statsVm;
            DataContext = statsVm;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _statsVm.InitProvider();
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
                    if (!acceptGranularities.Contains(GranularityType.SingleDate))
                    {
                        _statsVm.GranularityModel = new GranularityModel()
                        {
                            GranularityType = GranularityType.MultiDate
                        };
                    }
                    else if (!acceptGranularities.Contains(GranularityType.MultiDate))
                    {
                        _statsVm.GranularityModel = new GranularityModel()
                        {
                            GranularityType = GranularityType.SingleDate
                        };
                    }
                    else
                    {
                        // todo: depend on what user selected
                        _statsVm.GranularityModel = new GranularityModel()
                        {
                            GranularityType = GranularityType.SingleDate
                        };
                    }
                }

                if (!acceptGranularities.Contains(GranularityType.SingleRound) &&
                        !acceptGranularities.Contains(GranularityType.MultiRound))
                {
                    RoundAdjust.Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (!acceptGranularities.Contains(GranularityType.SingleRound))
                    {
                        _statsVm.GranularityModel = new GranularityModel()
                        {
                            GranularityType = GranularityType.MultiRound
                        };
                    }
                    else if (!acceptGranularities.Contains(GranularityType.MultiRound))
                    {
                        _statsVm.GranularityModel = new GranularityModel()
                        {
                            GranularityType = GranularityType.SingleRound
                        };
                    }
                    else
                    {
                        // todo: depend on what user selected
                        _statsVm.GranularityModel = new GranularityModel()
                        {
                            GranularityType = GranularityType.SingleRound
                        };
                    }
                }
            }

            await TemplateControl.UpdateGraph();
        }
    }
}
