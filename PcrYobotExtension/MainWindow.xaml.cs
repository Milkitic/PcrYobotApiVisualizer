using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using YobotExtension.AutoUpdate;
using YobotExtension.ChartFramework;
using YobotExtension.Shared.Configuration;
using YobotExtension.Shared.Win32;
using YobotExtension.Shared.YobotService.V1;
using YobotExtension.UserControls.StatsGraphControls;
using YobotExtension.ViewModels;
using YobotExtension.YobotService;

namespace YobotExtension
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IChartProvider
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private StatsVm _viewModel;
        private IYobotServiceV1 _yobotService;
        private Updater _updater;

        public Chart Chart { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            _viewModel = (StatsVm)DataContext;
            Execute.SetMainThreadContext();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new Task(async () =>
            {
                try
                {
                    _updater = new Updater();
                    bool? hasUpdate = await _updater.CheckUpdateAsync();
                    if (hasUpdate == true && _updater.NewRelease.NewVerString != AppSettings.Default.General.IgnoredVer)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var newVersionWindow = new NewVersionWindow(_updater.NewRelease, this);
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
            _yobotService.InitRequested += _yobotService_InitRequested;
            await Load();
        }

        private async Task<bool> _yobotService_InitRequested()
        {
            var win = new PromptWindow();
            win.ShowDialog();
            var text = win.Text;
            try
            {
                await _yobotService.LoginAsync(text);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        private async Task Load()
        {
            await UpdateDataAsync();
            UpdateInterface();
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

            var binding = new Binding("Text") { Path = new PropertyPath("StatsGraph.SeriesCollection") };
            Chart.SetBinding(Chart.SeriesProperty, binding);
            var bindAxisXTitle = new Binding("Title") { Path = new PropertyPath("StatsGraph.AxisXTitle") };
            Chart.AxisX[0].SetBinding(Axis.TitleProperty, bindAxisXTitle);
            var bindAxisXLabels = new Binding("Labels") { Path = new PropertyPath("StatsGraph.AxisXLabels") };
            Chart.AxisX[0].SetBinding(Axis.LabelsProperty, bindAxisXLabels);

            var bindAxisYTitle = new Binding("Title") { Path = new PropertyPath("StatsGraph.AxisYTitle") };
            Chart.AxisY[0].SetBinding(Axis.TitleProperty, bindAxisYTitle);
            var bindAxisYLabels = new Binding("Labels") { Path = new PropertyPath("StatsGraph.AxisYLabels") };
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
                bool updateInterface = _viewModel.ApiObj == null;
                var apiObj = await _yobotService.GetApiInfo();
                _viewModel.ApiObj = apiObj;
                _viewModel.ApiObj.Challenges = _viewModel.ApiObj.Challenges.OrderBy(k => k.ChallengeTime)
                    /*         .Where(k => k.ChallengeTime < new DateTime(2020, 5, 15))*/.ToArray();
                _viewModel.CycleCount = _viewModel.ApiObj.Challenges.GroupBy(k => k.Cycle).Count();

                if (updateInterface)
                {
                    UpdateInterface();
                }
            }
            finally
            {
                btnUpdateData.IsEnabled = true;
            }
        }

        private void UpdateInterface()
        {
            _viewModel.SelectedCycle = _viewModel.CycleCount;
            _viewModel.SelectedDate = _viewModel.ApiObj.Challenges.FirstOrDefault()?.ChallengeTime.AddHours(-5);

            GraphControlPanel.Children.Clear();

            var asm = Assembly.GetExecutingAssembly();
            var switchControls = asm.GetExportedTypes().Where(k => k.GetInterfaces().Contains(typeof(IChartSwitchControl)));

            foreach (var switchControl in switchControls)
            {
                var obj = (UserControl)Activator.CreateInstance(switchControl);
                GraphControlPanel.Children.Add(obj);
                if (obj is IChartSwitchControl sw)
                {
                    sw.InitModels(_viewModel, this);
                }
            }

            var statsProviders = asm.GetExportedTypes()
                .Where(k => k.GetInterfaces().Contains(typeof(IStatsProvider)))
                .ToArray();
            HashSet<StatsProviderInfo> statsProviderInfos = new HashSet<StatsProviderInfo>();
            foreach (var statsProvider in statsProviders)
            {
                try
                {
                    var statisticsProviderInfo = new StatsProviderInfo();
                    var attr = statsProvider.GetCustomAttribute<StatsProviderMetadataAttribute>();

                    statsProviderInfos.Add(statisticsProviderInfo);
                    statisticsProviderInfo.Metadata = attr;

                    var instance = (IStatsProvider)Activator.CreateInstance(statsProvider);
                    //instance.ChartProvider = this;
                    instance.Challenges = _viewModel.ApiObj.Challenges.ToArray();

                    var methods = statsProvider.GetMethods();
                    foreach (var methodInfo in methods)
                    {
                        var o = methodInfo.GetCustomAttribute<StatsMethodAttribute>();
                        if (o == null) continue;

                        var statsFunctionInfo = new StatsFunctionInfo();
                        var granularityAttr = methodInfo.GetCustomAttribute<StatsMethodAcceptGranularityAttribute>();
                        if (granularityAttr != null)
                        {
                            statsFunctionInfo.AcceptGranularities = granularityAttr.AcceptGranularities;
                        }

                        Func<GranularityModel, Task<IChartConfigModel>> invokeFunc = null;
                        var retType = methodInfo.ReturnType;
                        if (retType.IsGenericType)
                        {
                            var genericType = retType.GetGenericTypeDefinition();
                            if (genericType.IsSubclassOf(typeof(Task)))
                            {
                                var genericArgs = retType.GetGenericArguments();
                                if (genericArgs.Length == 1 && genericArgs[0].GetInterfaces().Contains(typeof(IChartConfigModel)))
                                {
                                    var args = methodInfo.GetParameters();
                                    if (args.Length == 1 && args[0].ParameterType == typeof(GranularityModel))
                                    {
                                        invokeFunc = async (granularity) =>
                                        {
                                            var task = (Task)methodInfo.Invoke(instance, new object[] { granularity });
                                            await task.ConfigureAwait(false);
                                            var resultProperty = task.GetType().GetProperty("Result");
                                            return (IChartConfigModel)resultProperty?.GetValue(task);
                                        };
                                    }
                                    else
                                    {
                                        invokeFunc = async (granularity) =>
                                        {
                                            var task = (Task)methodInfo.Invoke(instance, null);
                                            await task.ConfigureAwait(false);
                                            var resultProperty = task.GetType().GetProperty("Result");
                                            return (IChartConfigModel)resultProperty?.GetValue(task);
                                        };
                                    }
                                }
                            }
                        }
                        else if (retType.GetInterfaces().Contains(typeof(IChartConfigModel)))
                        {
                            var args = methodInfo.GetParameters();
                            if (args.Length == 1 && args[0].ParameterType == typeof(GranularityModel))
                            {
                                invokeFunc = async (granularity) =>
                                {
                                    await Task.CompletedTask;
                                    var configModel = (IChartConfigModel)methodInfo.Invoke(instance, new object[] { granularity });
                                    return configModel;
                                };
                            }
                            else
                            {
                                invokeFunc = async (granularity) =>
                                {
                                    await Task.CompletedTask;
                                    var configModel = (IChartConfigModel)methodInfo.Invoke(instance, null);
                                    return configModel;
                                };
                            }
                        }

                        if (invokeFunc != null)
                        {
                            statsFunctionInfo.Function = invokeFunc;
                        }

                        statisticsProviderInfo.FunctionsMapping.Add(o, statsFunctionInfo);
                    }
                }
                catch (Exception e)
                {
                    continue;
                }
            }

            hhh.ItemsSource = statsProviderInfos;
            //foreach (var statsProviderInfo in statsProviderInfos)
            //{
            //    var obj = (UserControl)Activator.CreateInstance(statsProviderInfo);
            //    GraphControlPanel.Children.Add(obj);
            //    if (obj is IChartSwitchControl sw)
            //    {
            //        sw.InitModels(_viewModel, this);
            //    }
            //}
        }

        private Timer _loadTimer;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            _loadTimer?.Dispose();
            _loadTimer = new Timer((obj) => Dispatcher.Invoke(() => _viewModel.IsLoading = true), null, 1000,
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
                                _viewModel.ApiObj.Challenges.Max(k => k.ChallengeTime).Date);
                        }
                        else if (functionInfo.AcceptGranularities.Contains(GranularityType.MultiRound))
                        {
                            granularityModel = new GranularityModel(0, 1, 4);
                        }
                        else
                        {
                            granularityModel = new GranularityModel(0,
                                _viewModel.ApiObj.Challenges.Max(k => k.ChallengeTime).Date);
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
                            _viewModel.StatsGraph.SeriesCollection = cartesianResult.SeriesCollection;
                            _viewModel.StatsGraph.Title = cartesianResult.Title;
                            _viewModel.StatsGraph.AxisXLabels = cartesianResult.AxisXLabels;
                            _viewModel.StatsGraph.AxisYLabels = cartesianResult.AxisYLabels;
                            _viewModel.StatsGraph.AxisXTitle = cartesianResult.AxisXTitle;
                            _viewModel.StatsGraph.AxisYTitle = cartesianResult.AxisYTitle;
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
                _viewModel.IsLoading = false;
                _loadTimer?.Dispose();
            }
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            await _yobotService.LogoutAsync();
        }
    }
}
