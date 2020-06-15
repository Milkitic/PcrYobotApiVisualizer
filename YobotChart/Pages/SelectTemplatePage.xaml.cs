using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using YobotChart.Annotations;
using YobotChart.Shared.Win32.ChartFramework;
using YobotChart.Shared.Win32.ChartFramework.SourceProviders;
using YobotChart.Shared.Win32.ChartFramework.StatsProviders;
using YobotChart.UiComponents;

namespace YobotChart.Pages
{
    public class SelectTemplatePageVm : INotifyPropertyChanged
    {
        private StatsProviderInfoSource _statsProviderInfoSource = StatsProviderInfoSource.Default;

        public StatsProviderInfoSource StatsProviderInfoSource
        {
            get => _statsProviderInfoSource;
            set
            {
                if (Equals(value, _statsProviderInfoSource)) return;
                _statsProviderInfoSource = value;
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
    /// SelectTemplatePage.xaml 的交互逻辑
    /// </summary>
    public partial class SelectTemplatePage : Page
    {
        private SelectTemplatePageVm _viewModel;

        public SelectTemplatePage()
        {
            InitializeComponent();
            _viewModel = (SelectTemplatePageVm)DataContext;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var functionInfo = (StatsFunctionInfo)button.Tag;

            var func = functionInfo.Function;
            var statsVm = new StatsViewModel
            {
                StatsProviderGuid = functionInfo.ProviderGuid,
                StatsProviderMethodName = functionInfo.Name
            };

            AnimatedFrame.Default?.AnimateNavigate(new ConfigTemplatePage(statsVm));
        }
    }
}
