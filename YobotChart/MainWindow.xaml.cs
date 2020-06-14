using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using YobotChart.AutoUpdate;
using YobotChart.Pages;
using YobotChart.Shared.Configuration;
using YobotChart.Shared.Win32;
using YobotChart.Shared.Win32.ChartFramework;
using YobotChart.Shared.YobotService.V1;
using YobotChart.UiComponents.FrontDialogComponent;
using YobotChart.UiComponents.NotificationComponent;
using YobotChart.UserControls;
using YobotChart.YobotService;

namespace YobotChart
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private IYobotServiceV1 _yobotService;
        private GiteeUpdater _updater;

        private static Dictionary<Type, Page> _dic = new Dictionary<Type, Page>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            Execute.SetMainThreadContext();
            StatsProviderInfoSource.LoadSource();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NotificationOverlay.ItemsSource = Notification.NotificationList;

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
            _yobotService.InitRequested += YobotService_InitRequested;
            MainFrame.Navigate(GetInstance<DashBoardPage>());
            await Load();
        }

        private async Task<string> YobotService_InitRequested()
        {
            bool? result = null;
            InitUriControl initUriControl = null;
            Execute.OnUiThread(() =>
            {
                initUriControl = new InitUriControl();
                FrontDialogOverlay.ShowContent(initUriControl, new FrontDialogOverlay.ShowContentOptions
                {
                    Height = 400,
                    Width = 650,
                    ShowDialogButtons = false,
                    ShowTitleBar = false
                }, (sender, args) => result = true,
                    (sender, args) => result = false);
            });
            await Task.Factory.StartNew(() =>
            {
                while (result == null)
                {
                    Thread.Sleep(100);
                }
            });
            return initUriControl?.Text;
        }

        private async Task Load()
        {
            await UpdateDataAsync();
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
                var apiObj = await _yobotService.GetApiInfo().ConfigureAwait(false);
                var apiSource = YobotApiSource.Default;
                apiSource.YobotApi = apiObj;
                apiSource.YobotApi.Challenges = apiSource.YobotApi.Challenges.OrderBy(k => k.ChallengeTime)
                    /*         .Where(k => k.ChallengeTime < new DateTime(2020, 5, 15))*/.ToArray();
                //apiSource.CycleCount = apiSource.YobotApi.Challenges.GroupBy(k => k.Cycle).Count();

                //apiSource.SelectedCycle = apiSource.CycleCount;
                //apiSource.SelectedDate =
                apiSource.YobotApi.Challenges.FirstOrDefault()?.ChallengeTime.AddHours(-5);
            }
            catch (ArgumentNullException arg)
            {

            }
            finally
            {
                Execute.OnUiThread(() => btnUpdateData.IsEnabled = true);
            }
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            await _yobotService.LogoutAsync();
        }

        private void BtnAddTemplatePage_OnClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(GetInstance<SelectTemplatePage>());
        }

        private void BtnDashBoardPage_OnClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(GetInstance<DashBoardPage>());
        }

        private T GetInstance<T>() where T : Page, new()
        {
            var type = typeof(T);
            if (!_dic.ContainsKey(type))
            {
                _dic.Add(type, new T());
            }

            return (T)_dic[type];
        }
    }
}