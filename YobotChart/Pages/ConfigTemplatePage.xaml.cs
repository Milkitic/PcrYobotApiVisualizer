using System;
using System.Collections.Generic;
using System.Linq;
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
using YobotChart.Shared.Win32.ChartFramework;

namespace YobotChart.Pages
{
    /// <summary>
    /// ConfigTemplatePage.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigTemplatePage : Page
    {
        private readonly StatsViewModel _statsVm;

        public ConfigTemplatePage(StatsViewModel statsVm)
        {
            InitializeComponent();
            _statsVm = statsVm;
            DataContext = statsVm;
        }
    }
}
