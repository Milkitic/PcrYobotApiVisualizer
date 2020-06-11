using System;

namespace YobotChart.ChartFramework
{
    public class StatsProviderMetadataAttribute : Attribute
    {
        public StatsProviderMetadataAttribute(string guid)
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
            return obj is StatsProviderMetadataAttribute attr ? Equals(attr) : ReferenceEquals(this, obj);
        }

        protected bool Equals(StatsProviderMetadataAttribute other)
        {
            return Guid.Equals(other.Guid);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }
    }
}