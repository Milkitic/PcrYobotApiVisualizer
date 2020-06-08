using LiveCharts;
using LiveCharts.Wpf;

namespace PcrYobotExtension.ChartFramework
{
    public class CartesianChartConfigModel : ChartConfigModel<CartesianChart>
    {
        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();

        public string[] AxisXLabels { get; set; }

        public string AxisXTitle { get; set; }

        public string AxisYTitle { get; set; }

        public string[] AxisYLabels { get; set; }
    }
}