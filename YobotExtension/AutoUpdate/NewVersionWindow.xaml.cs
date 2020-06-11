using System.Diagnostics;
using System.Windows;
using YobotExtension.Shared.Configuration;

namespace YobotExtension.AutoUpdate
{
    /// <summary>
    /// NewVersionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewVersionWindow : Window
    {
        private readonly GiteeRelease _release;
        private readonly MainWindow _mainWindow;

        public NewVersionWindow(GiteeRelease release, MainWindow mainWindow)
        {
            _release = release;
            _mainWindow = mainWindow;
            InitializeComponent();
            MainGrid.DataContext = _release;
        }

        //private void OpenHyperlink(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        //{
        //    var p = e.Parameter.ToString();
        //    if (p == "later")
        //    {
        //        Close();
        //    }
        //    else if (p == "ignore")
        //    {
        //        AppSettings.Default.IgnoredVer = _release.NewVerString;
        //        AppSettings.SaveDefault();
        //        Close();
        //    }
        //    else if (p == "update")
        //    {
        //        UpdateWindow updateWindow = new UpdateWindow(_release, _mainWindow);
        //        updateWindow.Show();
        //        Close();
        //    }
        //    else
        //        Process.Start(p);
        //}

        private void Update_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var updateWindow = new UpdateWindow(_release.GithubReleaseFile, _mainWindow);
            updateWindow.Show();
            Close();
        }

        private void HtmlUrl_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(_release.GithubReleasePage);
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.Default.General.IgnoredVer = _release.NewVerString;
            AppSettings.SaveDefault();
            Close();
        }

        private void Later_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
