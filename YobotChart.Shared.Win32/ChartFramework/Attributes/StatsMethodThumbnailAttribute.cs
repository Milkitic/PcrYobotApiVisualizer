using System;

namespace YobotChart.Shared.Win32.ChartFramework.Attributes
{
    public class StatsMethodThumbnailAttribute : Attribute
    {
        public string Path { get; }

        public StatsMethodThumbnailAttribute(string path)
        {
            Path = path;
        }
    }
}