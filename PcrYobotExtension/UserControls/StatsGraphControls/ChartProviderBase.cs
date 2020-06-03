using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using PcrYobotExtension.ViewModels;
using System;
using System.Threading.Tasks;
using LiveCharts;

namespace PcrYobotExtension.UserControls.StatsGraphControls
{
    internal class ChartProviderBase
    {
        /// <summary>
        /// 图表相关的上下文
        /// </summary>
        public StatsVm Stats { get; set; }

        /// <summary>
        /// 图表源
        /// </summary>
        public IChartProvider ChartProvider { get; set; }

        [Something("")]
        public async Task<CartesianChartConfigModel> Get()
        {
            var o = new CartesianChartConfigModel();
            o.ChartConfig = (e) => { };
        }
    }

    public static class ChartConfigModelHelper
    {
        public static readonly Type CartesianType = typeof(CartesianChart);
        public static readonly Type PieType = typeof(PieChart);
    }

    public class CartesianChartConfigModel : ChartConfigModel<CartesianChart>
    {
        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();

        public string[] AxisXLabels { get; set; }

        public string AxisXTitle { get; set; }

        public string AxisYTitle { get; set; }

        public string[] AxisYLabels { get; set; }
    }

    public abstract class ChartConfigModel<T> where T : Chart
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
    }

    public enum ChartType
    {
        Cartesian, Pie
    }
}
