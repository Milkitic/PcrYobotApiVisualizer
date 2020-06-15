using System;
using System.ComponentModel;

namespace YobotChart.Shared.Win32.ChartFramework.ConfigModels
{
    public interface IChartConfigModel : INotifyPropertyChanged
    {
        ChartType ChartType { get; }
        string Title { get; set; }
        Action<LiveCharts.Wpf.Charts.Base.Chart> ChartConfig { get; set; }
    }
}