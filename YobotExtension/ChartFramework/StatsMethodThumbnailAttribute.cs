using System;

namespace YobotExtension.ChartFramework
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