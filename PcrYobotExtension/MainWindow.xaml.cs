using LiveCharts.Wpf;
using Newtonsoft.Json;
using PcrYobotExtension.Models;
using PcrYobotExtension.Services;
using PcrYobotExtension.UserControls.StatsGraphControls;
using PcrYobotExtension.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
            var json = await _yobotService.GetApiInfo();
            _viewModel.ApiObj = JsonConvert.DeserializeObject<YobotApiModel>(json);
            _viewModel.ApiObj.Challenges = _viewModel.ApiObj.Challenges
                .Where(k => k.ChallengeTime < new DateTime(2020, 5, 15)).ToArray();
            _viewModel.CycleCount = _viewModel.ApiObj.Challenges.GroupBy(k => k.Cycle).Count();
            _viewModel.SelectedCycle = _viewModel.CycleCount;

            foreach (UIElement child in GraphControlPanel.Children)
            {
                if (child is IChartSwitchControl c)
                {
                    c.InitModels(_viewModel, this);
                }
            }
        }

        public CartesianChart Graph => Chart;
        public CartesianChart RecreateGraph()
        {
            Chart = new CartesianChart();
            return Chart;
        }
    }
}
