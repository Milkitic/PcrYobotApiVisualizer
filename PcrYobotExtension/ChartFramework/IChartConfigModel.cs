using System;

namespace YobotExtension.ChartFramework
{
    public interface IChartConfigModel
    {
        ChartType ChartType { get; }
        string Title { get; set; }
        Action<LiveCharts.Wpf.Charts.Base.Chart> ChartConfig { get; set; }
    }
}