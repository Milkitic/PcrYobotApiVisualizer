using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using PcrYobotExtension.Annotations;
using PcrYobotExtension.Models;
using Path = System.IO.Path;

namespace PcrYobotExtension
{
    public class MainWindowVm : INotifyPropertyChanged
    {
        private SeriesCollection _seriesCollection;
        private YobotApiModel _apiObj;
        private string[] _axisXLabels;
        //private Func<long, string> _axisYFormatter = value => value.ToString("N");
        private string _axisYTitle;
        private string _axisXTitle;
        private string[] _axisYLabels;
        private int _cycleCount;
        private int _selectedCycle = -1;

        public SeriesCollection SeriesCollection
        {
            get => _seriesCollection;
            set
            {
                if (Equals(value, _seriesCollection)) return;
                _seriesCollection = value;
                OnPropertyChanged();
            }
        }

        public YobotApiModel ApiObj
        {
            get => _apiObj;
            set
            {
                if (Equals(value, _apiObj)) return;
                _apiObj = value;
                OnPropertyChanged();
            }
        }

        //public Func<long, string> AxisYFormatter
        //{
        //    get => _axisYFormatter;
        //    set
        //    {
        //        if (Equals(value, _axisYFormatter)) return;
        //        _axisYFormatter = value;
        //        OnPropertyChanged();
        //    }
        //}

        public string[] AxisXLabels
        {
            get => _axisXLabels;
            set
            {
                if (Equals(value, _axisXLabels)) return;
                _axisXLabels = value;
                OnPropertyChanged();
            }
        }

        public string AxisXTitle
        {
            get => _axisXTitle;
            set
            {
                if (value == _axisXTitle) return;
                _axisXTitle = value;
                OnPropertyChanged();
            }
        }

        public string AxisYTitle
        {
            get => _axisYTitle;
            set
            {
                if (value == _axisYTitle) return;
                _axisYTitle = value;
                OnPropertyChanged();
            }
        }

        public string[] AxisYLabels
        {
            get => _axisYLabels;
            set
            {
                if (Equals(value, _axisYLabels)) return;
                _axisYLabels = value;
                OnPropertyChanged();
            }
        }

        public int CycleCount
        {
            get => _cycleCount;
            set
            {
                if (value == _cycleCount) return;
                _cycleCount = value;
                OnPropertyChanged();
            }
        }

        public int SelectedCycle
        {
            get => _selectedCycle;
            set
            {
                if (value == _selectedCycle) return;
                _selectedCycle = value;
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
        private MainWindowVm _viewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            _viewModel = (MainWindowVm)DataContext;
            WebBrowserVersionEmulation();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //var o = await Utils.GetCookie("http://101.132.134.254:9222/yobot/login/#qqid=2241521134&key=JwGveb");
            WebBrowser.Navigated += wbMain_Navigated;
            WebBrowser.Navigate("http://101.132.134.254:9222/yobot/login/c/#qqid=2241521134&key=VFEMZN");
            
            var text = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"template.json"));
            _viewModel.ApiObj = JsonConvert.DeserializeObject<YobotApiModel>(text);
            _viewModel.CycleCount = _viewModel.ApiObj.Challenges.GroupBy(k => k.Cycle).Count();
            _viewModel.SelectedCycle = _viewModel.CycleCount;
            CycleButtons.ItemsSource = Enumerable.Range(1, _viewModel.CycleCount);
        }

        private void wbMain_Navigated(object sender, NavigationEventArgs e)
        {
            SetSilent(WebBrowser, true); // make it silent
        }

        private void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }

        private static void WebBrowserVersionEmulation()
        {
            const string browserEmulationKey =
                @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";

            String appname = Process.GetCurrentProcess().ProcessName + ".exe";

            // Webpages are displayed in IE9 Standards mode, regardless of the !DOCTYPE directive.
            const int browserEmulationMode = 11001;

            RegistryKey registryKeyObj =
                Registry.CurrentUser.OpenSubKey(browserEmulationKey, RegistryKeyPermissionCheck.ReadWriteSubTree) ??
                Registry.CurrentUser.CreateSubKey(browserEmulationKey);

            if (registryKeyObj == null) return;

            registryKeyObj.SetValue(appname, browserEmulationMode, RegistryValueKind.DWord);
            registryKeyObj.Close();
        }

        private void BtnTotalDamageTrend_Click(object sender, RoutedEventArgs e)
        {
            Chart.AxisY[0].Separator.Step = double.NaN;
            //DataContext = null;

            var totalDamageTrend = _viewModel.ApiObj.Challenges
                      .GroupBy(k => k.Cycle).ToList();
            _viewModel.SeriesCollection = new SeriesCollection()
                    {
                        new LineSeries
                        {
                            Values = new ChartValues<long>(totalDamageTrend
                                .Select(k =>
                                {
                                    return k.Max(o => o.ChallengeTime) - k.Min(o => o.ChallengeTime);
                                })
                            ),
                            Title = "花费时间"
                        }
                    };
            _viewModel.AxisXLabels = totalDamageTrend.Select(k => k.Key.ToString()).ToArray();
            Chart.AxisY[0].LabelFormatter = value => TimeSpan.FromSeconds(value).ToString();
            _viewModel.AxisXTitle = "周目";
            _viewModel.AxisYTitle = "周目花费时间";
            //DataContext = _viewModel;
        }

        private async void BtnCyclePersonalDamage_Click(object sender, RoutedEventArgs e)
        {
            //DataContext = null;

            await CyclePersonalDamage();
        }

        private async Task CyclePersonalDamage()
        {
            var totalDamageTrend = _viewModel.ApiObj.Challenges
                .GroupBy(k => k.Cycle).ToList()[_viewModel.SelectedCycle - 1];
            var challengeModels = totalDamageTrend.ToList();
            var personsDic = challengeModels.GroupBy(k => k.QqId)
                .ToDictionary(k => k.Key,
                    k =>
                    {
                        var dic = new Dictionary<int, ChallengeModel>();
                        foreach (var challengeModel in k)
                        {
                            if (dic.ContainsKey(challengeModel.BossNum))
                            {
                                dic[challengeModel.BossNum].Damage += challengeModel.Damage;
                            }
                            else
                            {
                                dic.Add(challengeModel.BossNum, challengeModel);
                            }
                        }

                        return dic;
                    });
            var list = new List<CyclePersonalDamageModel>();
            foreach (var kvp in personsDic)
            {
                var cycleModel = new CyclePersonalDamageModel
                {
                    Name = $"{await Utils.GetQqNickNameAsync(kvp.Key)} ({kvp.Key})"
                };
                for (int i = 0; i < 5; i++)
                {
                    cycleModel.BossDamages.Add((int)(kvp.Value.ContainsKey(cycleModel.BossDamages.Count + 1)
                        ? kvp.Value[cycleModel.BossDamages.Count + 1].Damage
                        : 0L));
                }

                list.Add(cycleModel);
            }

            list = list.OrderBy(k => k.BossDamages.Sum()).ToList();

            _viewModel.SeriesCollection = new SeriesCollection();
            for (int i = 0; i < 5; i++)
            {
                var i1 = i;
                _viewModel.SeriesCollection.Add(new StackedRowSeries
                {
                    Title = "BOSS " + (i + 1),
                    Values = new ChartValues<int>(list.Select(k => k.BossDamages[i1])),
                    DataLabels = true
                });
            }

            _viewModel.AxisYLabels = list.Select(k => k.Name).ToArray();
            _viewModel.AxisXLabels = null;
            Chart.AxisY[0].Separator = new LiveCharts.Wpf.Separator
            {
                Step = 1,
            };
            LblTitle.Content = _viewModel.SelectedCycle + "周目个人伤害统计";
            _viewModel.AxisXTitle = "伤害";
            _viewModel.AxisYTitle = "成员";
            //DataContext = _viewModel;
        }

        private async void BtnCyclePersonalDamageChangeCycle_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectedCycle = (int)((Button)sender).Tag;
            await CyclePersonalDamage();
        }
    }

    internal class CyclePersonalDamageModel
    {
        public string Name { get; set; }
        public List<int> BossDamages { get; set; } = new List<int>();
    }
}
