using System;

namespace YobotChart.Shared.Win32.ChartFramework.Attributes
{
    public class StatsProviderAttribute : Attribute
    {
        public StatsProviderAttribute(string guid)
        {
            Guid = Guid.Parse(guid);
        }

        public string Name { get; set; }
        public string Author { get; set; }
        public Version Version { get; set; }
        public Guid Guid { get; }
        public string Description { get; set; }

        public override bool Equals(object obj)
        {
            return obj is StatsProviderAttribute attr ? Equals(attr) : ReferenceEquals(this, obj);
        }

        protected bool Equals(StatsProviderAttribute other)
        {
            return Guid.Equals(other.Guid);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }
    }
}