using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using YobotChart.AutoUpdate;
using YobotChart.Pages;
using YobotChart.Shared.Configuration;
using YobotChart.Shared.Win32;
using YobotChart.Shared.Win32.ChartFramework.SourceProviders;
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

        private GiteeUpdater _updater;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Initialized(object sender, EventArgs e)
        {
            Execute.SetMainThreadContext();
            NotificationOverlay.ItemsSource = Notification.NotificationList;

            YobotApiSource.Default.YobotService = new ServiceCore(Browser);
            YobotApiSource.Default.YobotService.InitRequested += YobotService_InitRequested;

            await UpdateApiData();

            StatsProviderInfoSource.LoadSource();

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
            
            MainFrame.AnimateNavigate(SingletonPageHelper.Get<DashBoardPage>());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            await YobotApiSource.Default.YobotService.LogoutAsync();
        }

        private async void UpdateData_Click(object sender, RoutedEventArgs e)
        {
            await UpdateApiData();
        }

        private void BtnAddTemplatePage_OnClick(object sender, RoutedEventArgs e)
        {
            MainFrame.AnimateNavigate(SingletonPageHelper.Get<SelectTemplatePage>());
        }

        private void BtnDashBoardPage_OnClick(object sender, RoutedEventArgs e)
        {
            MainFrame.AnimateNavigate(SingletonPageHelper.Get<DashBoardPage>());
        }

        private async Task UpdateApiData()
        {

            btnUpdateData.IsEnabled = false;

            try
            {
                await YobotApiSource.Default.UpdateDataAsync();
            }
            catch (ArgumentNullException arg)
            {

            }
            finally
            {
                btnUpdateData.IsEnabled = true;
            }
        }
    }
}