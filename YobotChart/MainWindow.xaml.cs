using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using YobotChart.AutoUpdate;
using YobotChart.Pages;
using YobotChart.Shared.Annotations;
using YobotChart.Shared.Configuration;
using YobotChart.Shared.Win32;
using YobotChart.Shared.Win32.ChartFramework.SourceProviders;
using YobotChart.UiComponents.FrontDialogComponent;
using YobotChart.UiComponents.NotificationComponent;
using YobotChart.UserControls;
using YobotChart.YobotService;

namespace YobotChart
{
    public class MainWindowVm : INotifyPropertyChanged
    {
        private SharedVm _sharedVm = SharedVm.Default;

        public SharedVm SharedVm
        {
            get => _sharedVm;
            set
            {
                if (Equals(value, _sharedVm)) return;
                _sharedVm = value;
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
    public partial class MainWindow : WindowEx
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private GiteeUpdater _updater;
        private Task _task;
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Initialized(object sender, EventArgs e)
        {
            Execute.SetMainThreadContext();
            StatsProviderInfoSource.LoadSource();
            NotificationOverlay.ItemsSource = Notification.NotificationList;

            YobotApiSource.Default.YobotService = new ServiceCore(Browser);
            YobotApiSource.Default.YobotService.InitRequested += YobotService_InitRequested;

            SharedVm.Default.IsUpdatingData = true;

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
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            Notification.Show("暂不支持此功能。请手动配置\"appsettings.yml\"", 5);
        }

        private async void WindowEx_Shown(object sender, RoutedEventArgs e)
        {
            try
            {
                await YobotApiSource.Default.UpdateDataAsync();
            }
            finally
            {
                SharedVm.Default.IsUpdatingData = false;
            }

            MainFrame.AnimateNavigate(SingletonPageHelper.Get<DashBoardPage>());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            popUserPanel.IsOpen = false;
            await YobotApiSource.Default.YobotService.LogoutAsync();
            MainFrame.Content = null;
            await Relogin();
        }

        private void BtnUser_Click(object sender, RoutedEventArgs e)
        {
            if (SharedVm.Default.LoginUser != null)
            {
                popUserPanel.IsOpen = true;
            }
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (FrontDialogOverlay.MainCanvas.Visibility != Visibility.Hidden) return;
            await Relogin();
        }

        private async Task Relogin()
        {
            await YobotApiSource.Default.UpdateDataAsync();
            if (SharedVm.Default.LoginUser != null)
            {
                MainFrame.AnimateNavigate(SingletonPageHelper.Get<DashBoardPage>());
                Console.WriteLine("navigate");
            }
            else
            {
                Console.WriteLine("null");
            }
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

            if (_task != null && !_task.IsCanceled && !_task.IsFaulted && !_task.IsCompleted)
            {
                try
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    await Task.WhenAll(_task);
                }
                catch (Exception)
                {
                }
            }

            _cts = new CancellationTokenSource();
            _task = Task.Factory.StartNew(() =>
            {
                while (result == null && !_cts.IsCancellationRequested)
                {
                    Thread.Sleep(100);
                }
            });
            await _task;
            return initUriControl?.Text ?? "none";
        }
    }
}