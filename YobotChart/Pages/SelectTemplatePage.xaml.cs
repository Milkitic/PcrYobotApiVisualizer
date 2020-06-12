using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using YobotChart.Annotations;
using YobotChart.ChartFramework;
using YobotChart.ViewModels;

namespace YobotChart.Pages
{
    public class SelectTemplatePageVm : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private HashSet<StatsProviderInfo> _statsProvidersList = ChartProviderLoader.Load(StatsVm.Default);

        public HashSet<StatsProviderInfo> StatsProvidersList
        {
            get => _statsProvidersList;
            set
            {
                if (Equals(value, _statsProvidersList)) return;
                _statsProvidersList = value;
                OnPropertyChanged();
            }
        }

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
        public SelectTemplatePage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
