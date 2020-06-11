using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Path = System.IO.Path;

namespace YobotExtension.AutoUpdate
{
    /// <summary>
    /// UpdateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateWindow : Window
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _downloadUrl;
        private readonly MainWindow _mainWindow;
        private Downloader _downloader;
        private readonly string _savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.zip");

        public UpdateWindow(string downloadUrl, MainWindow mainWindow)
        {
            _downloadUrl = downloadUrl;
            _mainWindow = mainWindow;
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _mainWindow.Close();
            _downloader = new Downloader(_downloadUrl);
            _downloader.OnStartDownloading += Downloader_OnStartDownloading;
            _downloader.OnDownloading += Downloader_OnDownloading;
            _downloader.OnFinishDownloading += Downloader_OnFinishDownloading;
            try
            {
                await _downloader.DownloadAsync(_savePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while updating.");
                MessageBox.Show(this, "更新出错，请重启软件重试：" + ex.Message, Title,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Downloader_OnStartDownloading(long size)
        {
            Dispatcher.BeginInvoke(new Action(() => DlProgress.Maximum = size));
        }

        private void Downloader_OnDownloading(long size, long downloadedSize, long speed)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DlProgress.Value = downloadedSize;
                LblSpeed.Content = CountSize(speed) + "/s";
                LblProgress.Content = $"{Math.Round(downloadedSize / (float)size * 100)} %";
            }));
        }

        private void Downloader_OnFinishDownloading()
        {
            Process.Start(new FileInfo(_savePath).DirectoryName);
            Process.Start(_savePath);
            Dispatcher.BeginInvoke(new Action(Close));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _downloader.Interrupt();
        }

        private static string CountSize(long size)
        {
            string strSize = "";
            long factSize = size;
            if (factSize < 1024)
                strSize = $"{factSize:F2} B";
            else if (factSize >= 1024 && factSize < 1048576)
                strSize = (factSize / 1024f).ToString("F2") + " KB";
            else if (factSize >= 1048576 && factSize < 1073741824)
                strSize = (factSize / 1024f / 1024f).ToString("F2") + " MB";
            else if (factSize >= 1073741824)
                strSize = (factSize / 1024f / 1024f / 1024f).ToString("F2") + " GB";
            return strSize;
        }
    }
}
