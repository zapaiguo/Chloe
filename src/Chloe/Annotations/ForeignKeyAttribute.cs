using System;

namespace Chloe.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ForeignKeyAttribute : Attribute
    {
        public ForeignKeyAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
    }
}
