using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Newtonsoft.Json;
using PcrYobotExtension.Annotations;
using PcrYobotExtension.Models;

namespace PcrYobotExtension
{
    public class MainWindowVm : INotifyPropertyChanged
    {
        private SeriesCollection _seriesCollection;
        public event PropertyChangedEventHandler PropertyChanged;

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
        private YobotApiModel _apiObj;
        private MainWindowVm _viewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            _viewModel = (MainWindowVm)DataContext;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var text = File.ReadAllText(@"C:\Users\Milky\Desktop\aa.json");
            _apiObj = JsonConvert.DeserializeObject<YobotApiModel>(text);

            //_viewModel.SeriesCollection = new SeriesCollection
            //{
            //    new LineSeries
            //    {
            //        Values = new ChartValues<double> { 3, 5, 7, 4 }
            //    },
            //    new ColumnSeries
            //    {
            //        Values = new ChartValues<decimal> { 5, 6, 2, 7 }
            //    }
            //};
        }
    }
}
