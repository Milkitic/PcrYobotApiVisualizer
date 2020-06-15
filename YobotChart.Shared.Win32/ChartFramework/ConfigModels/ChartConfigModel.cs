using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using YobotChart.Shared.Win32.Properties;

namespace YobotChart.Shared.Win32.ChartFramework.ConfigModels
{
    public abstract class ChartConfigModel<T> : IChartConfigModel where T : LiveCharts.Wpf.Charts.Base.Chart
    {
        private static readonly Type GenericType = typeof(T);
        public ChartType ChartType { get; }

        public ChartConfigModel()
        {
            if (GenericType == ChartConfigModelTypeHelper.CartesianType ||
                GenericType.IsSubclassOf(ChartConfigModelTypeHelper.CartesianType))
            {
                ChartType = ChartType.Cartesian;
            }
            else if (GenericType == ChartConfigModelTypeHelper.PieType ||
                     GenericType.IsSubclassOf(ChartConfigModelTypeHelper.PieType))
            {
                ChartType = ChartType.Pie;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(ChartType), @"unsupported chart type");
            }
        }

        public string Title { get; set; }

        public Action<T> ChartConfig { get; set; }

        Action<LiveCharts.Wpf.Charts.Base.Chart> IChartConfigModel.ChartConfig
        {
            get => o => ChartConfig?.Invoke((T)o);
            set => ChartConfig = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}