using LiveCharts.Wpf;

namespace PcrYobotExtension
{
    public interface IChartProvider
    {
        CartesianChart Graph { get; }
        CartesianChart RecreateGraph();
    }
}