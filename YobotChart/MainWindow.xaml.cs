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
            StatsProviderInfoSource.LoadSource();
            NotificationOverlay.ItemsSource = Notification.NotificationList;

            YobotApiSource.Default.YobotService = new ServiceCore(Browser);
            YobotApiSource.Default.YobotService.InitRequested += YobotService_InitRequested;

            SharedVm.Default.IsUpdatingData = true;
            try
            {
                await YobotApiSource.Default.UpdateDataAsync();
            }
            finally
            {
                SharedVm.Default.IsUpdatingData = false;
            }

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

        private Task task;
        private CancellationTokenSource cts;
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

            if (task != null && !task.IsCanceled && !task.IsFaulted && !task.IsCompleted)
            {
                try
                {
                    cts.Cancel();
                    cts.Dispose();
                    await Task.WhenAll(task);
                }
                catch (Exception)
                {
                }
            }

            cts = new CancellationTokenSource();
            task = Task.Factory.StartNew(() =>
            {
                while (result == null && !cts.IsCancellationRequested)
                {
                    Thread.Sleep(100);
                }
            });
            await task;
            return initUriControl?.Text;
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
    }
}