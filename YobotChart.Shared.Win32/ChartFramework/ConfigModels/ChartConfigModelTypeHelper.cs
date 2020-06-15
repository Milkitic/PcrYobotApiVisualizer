using System;
using LiveCharts.Wpf;

namespace YobotChart.Shared.Win32.ChartFramework.ConfigModels
{
    public static class ChartConfigModelTypeHelper
    {
        public static readonly Type CartesianType = typeof(CartesianChart);
        public static readonly Type PieType = typeof(PieChart);
    }
}