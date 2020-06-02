using LiveCharts.Wpf;
using Newtonsoft.Json;
using PcrYobotExtension.Models;
using PcrYobotExtension.Services;
using PcrYobotExtension.UserControls.StatsGraphControls;
using PcrYobotExtension.ViewModels;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LiveCharts;
using LiveCharts.Wpf.Charts.Base;

namespace PcrYobotExtension
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IChartProvider
    {
        private StatsVm _viewModel;
        private YobotService _yobotService;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            _viewModel = (StatsVm)DataContext;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _yobotService = new YobotService(Browser);
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
                await _yobotService.InitAsync(text);
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

        public Chart Chart { get; private set; }
        public void RecreateGraph()
        {
            Haha.Child = null;

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

            Haha.Child = Chart;
        }

        public void RecreateGraph(Chart chart)
        {
            Haha.Child = null;

            Chart = chart;
            Haha.Child = Chart;
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
                var json = await _yobotService.GetApiInfo();
                _viewModel.ApiObj = JsonConvert.DeserializeObject<YobotApiModel>(json);
                _viewModel.ApiObj.Challenges = _viewModel.ApiObj.Challenges.OrderBy(k=>k.ChallengeTime)
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
            _viewModel.SelectedDay = _viewModel.ApiObj.Challenges.FirstOrDefault()?.ChallengeTime.AddHours(-5);

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
        }
    }
}
