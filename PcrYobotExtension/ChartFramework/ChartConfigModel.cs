using System;

namespace PcrYobotExtension.ChartFramework
{
    public abstract class ChartConfigModel<T> : IChartConfigModel where T : LiveCharts.Wpf.Charts.Base.Chart
    {
        private static readonly Type GenericType = typeof(T);
        public ChartType ChartType { get; }

        public ChartConfigModel()
        {
            if (GenericType == ChartConfigModelHelper.CartesianType ||
                GenericType.IsSubclassOf(ChartConfigModelHelper.CartesianType))
            {
                ChartType = ChartType.Cartesian;
            }
            else if (GenericType == ChartConfigModelHelper.PieType ||
                     GenericType.IsSubclassOf(ChartConfigModelHelper.PieType))
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
    }
}