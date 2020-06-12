using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace YamlDotNet.Comment
{
    public class CommentGatheringTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector innerTypeDescriptor;

        public CommentGatheringTypeInspector(ITypeInspector innerTypeDescriptor)
        {
            if (innerTypeDescriptor == null)
            {
                throw new ArgumentNullException("innerTypeDescriptor");
            }

            this.innerTypeDescriptor = innerTypeDescriptor;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            return innerTypeDescriptor
                .GetProperties(type, container)
                .Select(d => new CommentsPropertyDescriptor(d));
        }

        private sealed class CommentsPropertyDescriptor : IPropertyDescriptor
        {
            private readonly IPropertyDescriptor baseDescriptor;

            public CommentsPropertyDescriptor(IPropertyDescriptor baseDescriptor)
            {
                this.baseDescriptor = baseDescriptor;
                Name = baseDescriptor.Name;
            }

            public string Name { get; set; }

            public Type Type { get { return baseDescriptor.Type; } }

            public Type TypeOverride
            {
                get { return baseDescriptor.TypeOverride; }
                set { baseDescriptor.TypeOverride = value; }
            }

            public int Order { get; set; }

            public ScalarStyle ScalarStyle
            {
                get { return baseDescriptor.ScalarStyle; }
                set { baseDescriptor.ScalarStyle = value; }
            }

            public bool CanWrite { get { return baseDescriptor.CanWrite; } }

            public void Write(object target, object value)
            {
                baseDescriptor.Write(target, value);
            }

            public T GetCustomAttribute<T>() where T : Attribute
            {
                return baseDescriptor.GetCustomAttribute<T>();
            }

            public IObjectDescriptor Read(object target)
            {
                var description = baseDescriptor.GetCustomAttribute<DescriptionAttribute>();
                return description != null
                    ? new CommentsObjectDescriptor(baseDescriptor.Read(target), description.Description)
                    : baseDescriptor.Read(target);
            }
        }
    }
}