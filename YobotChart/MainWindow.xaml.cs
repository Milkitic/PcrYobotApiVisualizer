using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using YobotChart.Annotations;
using YobotChart.AutoUpdate;
using YobotChart.ChartFramework;
using YobotChart.Shared.Configuration;
using YobotChart.Shared.Win32;
using YobotChart.Shared.YobotService.V1;
using YobotChart.UiComponents.FrontDialogComponent;
using YobotChart.UserControls;
using YobotChart.ViewModels;
using YobotChart.YobotService;

namespace YobotChart
{
    public class MainWindowVm : INotifyPropertyChanged
    {
        private StatsVm _stats = new StatsVm();
        private HashSet<StatsProviderInfo> _statsProvidersList;

        public StatsVm Stats
        {
            get => _stats;
            set
            {
                if (Equals(value, _stats)) return;
                _stats = value;
                OnPropertyChanged();
            }
        }

        public HashSet<StatsProviderInfo> StatsProvidersList
        {
            get => _statsProvidersList;
            set
            {
                if (Equals(value, _statsProvidersList)) return;
                _statsProvidersList = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IChartProvider
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private MainWindowVm _viewModel;
        private IYobotServiceV1 _yobotService;
        private GiteeUpdater _updater;

        public Chart Chart { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            _viewModel = (MainWindowVm)DataContext;
            Execute.SetMainThreadContext();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new Task(async () =>
            {
                try
                {
                    _updater = new GiteeUpdater();
                    bool? hasUpdate = await _updater.CheckUpdateAsync();
                    if (hasUpdate == true && _updater.NewRelease.NewVerString != AppSettings.Default.General.IgnoredVer)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var newVersionWindow = new AutoUpdate.NewVersionWindow(_updater.NewRelease, this);
                            newVersionWindow.ShowDialog();
                        });
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "检测更新出现错误");
                }
            }).Start();

            _yobotService = new ServiceCore(Browser);
            _yobotService.InitRequested += YobotService_InitRequested;
            await Load();
        }

        private async Task<string> YobotService_InitRequested()
        {
            bool? result = null;
            var initUriControl = new InitUriControl();
            FrontDialogOverlay.ShowContent(initUriControl, new FrontDialogOverlay.ShowContentOptions
            {
                Height = 400,
                Width = 650,
                ShowDialogButtons = false,
                ShowTitleBar = false
            }, (sender, args) => result = true,
                (sender, args) => result = false);
            await Task.Run(() =>
            {
                while (result == null)
                {
                    Thread.Sleep(10);
                }
            });
            return initUriControl.Text;
        }

        private async Task Load()
        {
            await UpdateDataAsync();
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

            var binding = new Binding("Text") { Path = new PropertyPath("Stats.StatsGraph.SeriesCollection") };
            Chart.SetBinding(Chart.SeriesProperty, binding);
            var bindAxisXTitle = new Binding("Title") { Path = new PropertyPath("Stats.StatsGraph.AxisXTitle") };
            Chart.AxisX[0].SetBinding(Axis.TitleProperty, bindAxisXTitle);
            var bindAxisXLabels = new Binding("Labels") { Path = new PropertyPath("Stats.StatsGraph.AxisXLabels") };
            Chart.AxisX[0].SetBinding(Axis.LabelsProperty, bindAxisXLabels);

            var bindAxisYTitle = new Binding("Title") { Path = new PropertyPath("Stats.StatsGraph.AxisYTitle") };
            Chart.AxisY[0].SetBinding(Axis.TitleProperty, bindAxisYTitle);
            var bindAxisYLabels = new Binding("Labels") { Path = new PropertyPath("Stats.StatsGraph.AxisYLabels") };
            Chart.AxisY[0].SetBinding(Axis.LabelsProperty, bindAxisYLabels);

            ChartContainer.Child = Chart;
        }

        public void RecreateGraph(Chart chart)
        {
            ChartContainer.Child = null;

            Chart = chart;
            ChartContainer.Child = Chart;
            Grid.SetRow(Chart, 3);
        }

        private async void UpdateData_Click(object sender, RoutedEventArgs e)
        {
            await UpdateDataAsync();
        }

        private async Task UpdateDataAsync()
        {
            btnUpdateData.IsEnabled = false;

            try
            {
                bool updateInterface = _viewModel.Stats.ApiObj == null;
                var apiObj = await _yobotService.GetApiInfo();
                _viewModel.Stats.ApiObj = apiObj;
                _viewModel.Stats.ApiObj.Challenges = _viewModel.Stats.ApiObj.Challenges.OrderBy(k => k.ChallengeTime)
                    /*         .Where(k => k.ChallengeTime < new DateTime(2020, 5, 15))*/.ToArray();
                _viewModel.Stats.CycleCount = _viewModel.Stats.ApiObj.Challenges.GroupBy(k => k.Cycle).Count();

                if (updateInterface)
                {
                    UpdateInterface();
                }
            }
            catch (ArgumentNullException arg)
            {

            }
            finally
            {
                btnUpdateData.IsEnabled = true;
            }
        }

        private void UpdateInterface()
        {
            _viewModel.Stats.SelectedCycle = _viewModel.Stats.CycleCount;
            _viewModel.Stats.SelectedDate =
                _viewModel.Stats.ApiObj.Challenges.FirstOrDefault()?.ChallengeTime.AddHours(-5);

            _viewModel.StatsProvidersList = ChartProviderLoader.Load(_viewModel.Stats);
        }

        private Timer _loadTimer;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            _loadTimer?.Dispose();
            _loadTimer = new Timer((obj) => Dispatcher.Invoke(() => _viewModel.Stats.IsLoading = true), null, 1000,
                Timeout.Infinite);

            try
            {
                var functionInfo = (StatsFunctionInfo)((Button)sender).Tag;
                var func = functionInfo.Function;

                if (func != null)
                {
                    IChartConfigModel result;
                    try
                    {
                        GranularityModel granularityModel;
                        if (functionInfo.AcceptGranularities == null)
                        {
                            granularityModel = new GranularityModel(0,
                                _viewModel.Stats.ApiObj.Challenges.Max(k => k.ChallengeTime).Date);
                        }
                        else if (functionInfo.AcceptGranularities.Contains(GranularityType.MultiRound))
                        {
                            granularityModel = new GranularityModel(0, 1, 4);
                        }
                        else
                        {
                            granularityModel = new GranularityModel(0,
                                _viewModel.Stats.ApiObj.Challenges.Max(k => k.ChallengeTime).Date);
                        }

                        result = await func.Invoke(granularityModel);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }

                    RecreateGraph();
                    result.ChartConfig?.Invoke(Chart);

                    switch (result.ChartType)
                    {
                        case ChartType.Cartesian:
                            var cartesianResult = (CartesianChartConfigModel)result;
                            _viewModel.Stats.StatsGraph.SeriesCollection = cartesianResult.SeriesCollection;
                            _viewModel.Stats.StatsGraph.Title = cartesianResult.Title;
                            _viewModel.Stats.StatsGraph.AxisXLabels = cartesianResult.AxisXLabels;
                            _viewModel.Stats.StatsGraph.AxisYLabels = cartesianResult.AxisYLabels;
                            _viewModel.Stats.StatsGraph.AxisXTitle = cartesianResult.AxisXTitle;
                            _viewModel.Stats.StatsGraph.AxisYTitle = cartesianResult.AxisYTitle;
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
                _viewModel.Stats.IsLoading = false;
                _loadTimer?.Dispose();
            }
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            await _yobotService.LogoutAsync();
        }
    }
}