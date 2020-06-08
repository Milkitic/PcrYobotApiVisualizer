using System;

namespace PcrYobotExtension.ChartFramework
{
    public class StatsMethodAttribute : Attribute
    {
        public StatsMethodAttribute(string name)
        {
            Name = name;
        }

        public Guid Guid { get; } = Guid.NewGuid();
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            return obj is StatsMethodAttribute attr ? Equals(attr) : base.Equals(obj);
        }

        protected bool Equals(StatsMethodAttribute other)
        {
            return Guid.Equals(other.Guid);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }
    }
}