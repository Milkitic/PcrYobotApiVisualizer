using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using YobotChart.Annotations;
using YobotChart.Shared.Win32.ChartFramework;

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
            var o = JsonConvert.SerializeObject(_viewModel, Formatting.Indented);
            _viewModel = JsonConvert.DeserializeObject<SelectTemplatePageVm>(o);
        }
    }
}
