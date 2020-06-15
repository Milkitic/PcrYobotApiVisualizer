using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using YobotChart.Shared.Win32.ChartFramework;
using YobotChart.Shared.Win32.ChartFramework.ConfigModels;
using YobotChart.Shared.Win32.ChartFramework.SourceProviders;
using YobotChart.UiComponents.NotificationComponent;

namespace YobotChart.UserControls
{
    /// <summary>
    /// GraphControl.xaml 的交互逻辑
    /// </summary>
    public partial class DashboardTemplateControl : UserControl
    {
        private Timer _loadTimer;
        public StatsViewModel StatsViewModel => (StatsViewModel)DataContext;

        public Chart Chart { get; private set; }

        public DashboardTemplateControl()
        {
            InitializeComponent();
        }

        public void RecreateGraph()
        {
            ChartContainer.Child = null;

            Chart = new CartesianChart
            {
                LegendLocation = LegendLocation.Top,
                AxisX = new AxesCollection { new Axis() },
                AxisY = new AxesCollection { new Axis() }
            };

            var binding = new Binding("Text") { Path = new PropertyPath("ConfigModel.SeriesCollection") };
            Chart.SetBinding(Chart.SeriesProperty, binding);
            var bindAxisXTitle = new Binding("Title") { Path = new PropertyPath("ConfigModel.AxisXTitle") };
            Chart.AxisX[0].SetBinding(Axis.TitleProperty, bindAxisXTitle);
            var bindAxisXLabels = new Binding("Labels") { Path = new PropertyPath("ConfigModel.AxisXLabels") };
            Chart.AxisX[0].SetBinding(Axis.LabelsProperty, bindAxisXLabels);

            var bindAxisYTitle = new Binding("Title") { Path = new PropertyPath("ConfigModel.AxisYTitle") };
            Chart.AxisY[0].SetBinding(Axis.TitleProperty, bindAxisYTitle);
            var bindAxisYLabels = new Binding("Labels") { Path = new PropertyPath("ConfigModel.AxisYLabels") };
            Chart.AxisY[0].SetBinding(Axis.LabelsProperty, bindAxisYLabels);

            ChartContainer.Child = Chart;
        }

        public async Task UpdateGraph()
        {
            var apiSource = YobotApiSource.Default;

            _loadTimer?.Dispose();
            _loadTimer = new Timer((obj) => Dispatcher.Invoke(() => StatsViewModel.IsLoading = true), null, 1000,
                Timeout.Infinite);

            try
            {
                var functionInfo = StatsViewModel.SourceStatsFunction;
                var func = functionInfo.Function;

                if (func != null)
                {
                    IChartConfigModel result;
                    try
                    {
                        //GranularityModel granularityModel;
                        //if (functionInfo.AcceptGranularities == null)
                        //{
                        //    granularityModel = new GranularityModel(0,
                        //        apiSource.YobotApi.Challenges.Max(k => k.ChallengeTime).AddHours(-5).Date);
                        //}
                        //else if (functionInfo.AcceptGranularities.Contains(GranularityType.MultiRound))
                        //{
                        //    granularityModel = new GranularityModel(0, 1, 4);
                        //}
                        //else
                        //{
                        //    granularityModel = new GranularityModel(0,
                        //        apiSource.YobotApi.Challenges.Max(k => k.ChallengeTime).AddHours(-5).Date);
                        //}

                        //result = await func.Invoke(granularityModel);
                        result = await func.Invoke(StatsViewModel.GranularityModel);
                    }
                    catch (Exception ex)
                    {
                        Notification.Error(ex.Message);
                        return;
                    }

                    RecreateGraph();
                    result.ChartConfig?.Invoke(Chart);

                    switch (result.ChartType)
                    {
                        case ChartType.Cartesian:
                            var cartesianResult = (CartesianChartConfigModel)result;
                            StatsViewModel.ConfigModel = cartesianResult;
                            //StatsVm.StatsGraph.SeriesCollection = cartesianResult.SeriesCollection;
                            //StatsVm.StatsGraph.Title = cartesianResult.Title;
                            //StatsVm.StatsGraph.AxisXLabels = cartesianResult.AxisXLabels;
                            //StatsVm.StatsGraph.AxisYLabels = cartesianResult.AxisYLabels;
                            //StatsVm.StatsGraph.AxisXTitle = cartesianResult.AxisXTitle;
                            //StatsVm.StatsGraph.AxisYTitle = cartesianResult.AxisYTitle;
                            break;
                        case ChartType.Pie:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            finally
            {
                StatsViewModel.IsLoading = false;
                _loadTimer?.Dispose();
            }
        }
    }
}
